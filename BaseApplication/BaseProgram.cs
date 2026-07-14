using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json.Serialization;
using BaseApplication.Extensions;
using BaseApplication.HostedServices;
using BaseApplication.Services;
using BasePostgreSQLRepositories;
using BaseSQLServerRepository;
using Common;
using Configs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ProtoBuf.Grpc.Server;
using Serilog;
using Serilog.Events;

namespace BaseApplication;

/// <summary>
/// Common bootstrap cho mọi microservice (theo script TYT.BaseProgram).
/// Mỗi service chỉ cần:
/// <code>
/// BaseProgram.Run(args,
///     services => { services.AddTransient&lt;IMyService, MyService&gt;(); return services; },
///     app => { app.MapGrpcService&lt;MyService&gt;(); });
/// </code>
/// </summary>
public abstract class BaseProgram
{
    public static string? HostName;
    public static string? HostIp;

    public static void Run(
        string[] args,
        Func<IServiceCollection, IServiceCollection>? registerServiceFunc,
        Action<WebApplication>? registerRoutingUrl)
    {
        ResolveHostInfo();

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = args
        });

        ConfigSetting.Init(builder.Configuration);

        var appName = ConfigSettingEnum.AppName.GetConfig();
        var appVersion = ConfigSettingEnum.AppVersion.GetConfig();
        if (string.IsNullOrWhiteSpace(appName))
        {
            appName = "App";
        }

        if (string.IsNullOrWhiteSpace(appVersion))
        {
            appVersion = "0.1";
        }

        ConfigureSerilog(appName, appVersion);

        builder.Host.UseContentRoot(Directory.GetCurrentDirectory());
        builder.Host.UseSerilog();

        ConfigureKestrel(builder);
        builder.Services.Configure<HostOptions>(options =>
        {
            options.ShutdownTimeout = TimeSpan.FromMinutes(10);
        });

        builder.Services.AddLogging(p => p.AddSerilog(Log.Logger));

        // --- Core DI ---
        builder.Services.ConfigCodeFirstGrpc();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddTransient<ContextService>();
        builder.Services.AddSingleton<ILogAction, LogAction>();

        RegisterWriteDb(builder.Services);   // SQL Server (Command)
        RegisterReadDb(builder.Services);    // PostgreSQL (Query)
        RegisterAuthentication(builder.Services);
        RegisterCors(builder.Services);
        RegisterMvcAndSwagger(builder.Services, appName, appVersion);

        builder.Services.AddResponseCompression();
        builder.Services.AddHostedService<AppInitHostedService>();

        // Service-specific DI (repositories, gRPC services...)
        registerServiceFunc?.Invoke(builder.Services);

        var app = builder.Build();
        ConfigurePipeline(app, appName, appVersion, registerRoutingUrl);
        app.Run();
    }

    #region Host / logging / kestrel

    private static void ResolveHostInfo()
    {
        HostName = Dns.GetHostName();
        Console.WriteLine($"HostName: {HostName}");

        try
        {
            var entry = Dns.GetHostEntry(HostName);
            foreach (var address in entry.AddressList)
            {
                Console.WriteLine($"IP Address is : {address}");
                if (address.AddressFamily == AddressFamily.InterNetwork)
                {
                    HostIp = address.ToString();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Resolve host IP failed: {ex.Message}");
        }

        Console.WriteLine($"HostIp: {HostIp}");
        ThreadPool.GetMinThreads(out var minWorker, out var minIoc);
        ThreadPool.GetMaxThreads(out var maxWorker, out var maxIoc);
        Console.WriteLine($"minWorker: {minWorker}, minIOC: {minIoc}");
        Console.WriteLine($"maxWorker: {maxWorker}, maxIOC: {maxIoc}");
    }

    private static void ConfigureSerilog(string appName, string appVersion)
    {
        var logEventLevel = (LogEventLevel)ConfigSettingEnum.LogEventLevel.GetConfig().AsInt((int)LogEventLevel.Information);
        var logPath = Path.Combine(Environment.CurrentDirectory, "log");

        var loggerConfiguration = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", logEventLevel)
            .Enrich.WithProperty("HostName", HostName)
            .Enrich.WithProperty("AppName", appName)
            .Enrich.WithProperty("AppVersion", appVersion)
            .Enrich.FromLogContext()
            .WriteTo.Console(
                outputTemplate:
                "[{Level} {Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] {Message} {Properties}{NewLine}{Exception}",
                restrictedToMinimumLevel: logEventLevel)
            .WriteTo.File(
                $"{logPath}/log-.txt",
                fileSizeLimitBytes: 1_000_000,
                rollOnFileSizeLimit: true,
                shared: false,
                flushToDiskInterval: TimeSpan.FromSeconds(5),
                rollingInterval: RollingInterval.Day,
                restrictedToMinimumLevel: logEventLevel,
                buffered: true);

        Log.Logger = loggerConfiguration.CreateLogger();
    }

    private static void ConfigureKestrel(WebApplicationBuilder builder)
    {
        var httpPort = ConfigSettingEnum.HttpPort.GetConfig().AsInt();
        if (httpPort <= 0)
        {
            httpPort = 30000;
        }

        // 1 = HTTP/1.1 (REST), 2 = HTTP/2 (gRPC), 3 = cả hai (2 port)
        var httpType = ConfigSettingEnum.HttpType.GetConfig().AsInt();
        if (httpType is not (1 or 2 or 3))
        {
            httpType = 1;
        }

        builder.WebHost.UseKestrel(options =>
        {
            options.AllowSynchronousIO = true;
            options.Limits.MinRequestBodyDataRate = null;
            options.Limits.MaxRequestBodySize = 50_971_520_000;
            options.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(60);
            options.Limits.MaxConcurrentConnections = int.MaxValue;
            options.Limits.MaxConcurrentUpgradedConnections = int.MaxValue;
            options.Limits.MaxRequestBufferSize = null;
            options.Limits.MaxResponseBufferSize = null;

            var http2 = options.Limits.Http2;
            http2.InitialConnectionWindowSize = 2 * 1024 * 1024 * 2;
            http2.InitialStreamWindowSize = 1024 * 1024;
            http2.MaxStreamsPerConnection = int.MaxValue;

            switch (httpType)
            {
                case 1:
                    options.ListenAnyIP(httpPort,
                        listenOptions => listenOptions.Protocols = HttpProtocols.Http1);
                    break;
                case 2:
                    options.ListenAnyIP(httpPort,
                        listenOptions => listenOptions.Protocols = HttpProtocols.Http2);
                    break;
                case 3:
                    options.ListenAnyIP(httpPort,
                        listenOptions => listenOptions.Protocols = HttpProtocols.Http1);
                    var httpPort2 = ConfigSettingEnum.HttpPort2.GetConfig().AsInt();
                    if (httpPort2 <= 0)
                    {
                        httpPort2 = httpPort + 1;
                    }

                    options.ListenAnyIP(httpPort2,
                        listenOptions => listenOptions.Protocols = HttpProtocols.Http2);
                    break;
            }
        });
    }

    #endregion

    #region DI registration

    /// <summary>Write side: SQL Server (Command / Domain).</summary>
    private static void RegisterWriteDb(IServiceCollection services)
    {
        var dbConnectionString = ConfigSettingEnum.DbConnectionString.GetConfig();
        if (dbConnectionString.Length == 0)
        {
            return;
        }

        services.AddTransient<IDbConnectionFactory>(sp =>
        {
            var log = sp.GetRequiredService<ILogger<DbConnectionFactory>>();
            return new DbConnectionFactory(dbConnectionString, log);
        });
    }

    /// <summary>Read side: PostgreSQL (Query / ReadModel).</summary>
    private static void RegisterReadDb(IServiceCollection services)
    {
        var postgresConnectionString = ConfigSettingEnum.PostgresConnectionString.GetConfig();
        if (postgresConnectionString.Length == 0)
        {
            return;
        }

        services.AddTransient<IPostgresConnectionFactory>(sp =>
        {
            var log = sp.GetRequiredService<ILogger<PostgresConnectionFactory>>();
            return new PostgresConnectionFactory(postgresConnectionString, log);
        });
    }

    private static void RegisterAuthentication(IServiceCollection services)
    {
        var authenticationType = ConfigSettingEnum.AuthenticationType.GetConfig().AsInt();
        if (authenticationType <= 0)
        {
            return;
        }

        IdentityModelEventSource.ShowPII = true;
        var xApiKeyEnable = ConfigSettingEnum.XApiKeyEnable.GetConfig().AsInt();
        var jwtKey = ConfigSettingEnum.JwtTokensKey.GetConfig();

        // Type 1: JWT Bearer (phù hợp gRPC/API service)
        if (authenticationType == 1)
        {
            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, cfg =>
                {
                    cfg.RequireHttpsMetadata = false;
                    cfg.SaveToken = true;
                    cfg.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.ASCII.GetBytes(
                                string.IsNullOrEmpty(jwtKey)
                                    ? "CHANGE_ME_DEV_JWT_KEY_AT_LEAST_32_CHARS!"
                                    : jwtKey)),
                        ValidateLifetime = true
                    };

                    if (xApiKeyEnable == 1)
                    {
                        cfg.Events = new JwtBearerEvents
                        {
                            OnMessageReceived = context =>
                            {
                                if (context.Request.Headers.ContainsKey("x-api-key"))
                                {
                                    context.Token = context.Request.Headers["x-api-key"];
                                }

                                return Task.CompletedTask;
                            }
                        };
                    }
                });
            services.AddAuthorization();
        }
    }

    private static void RegisterCors(IServiceCollection services)
    {
        var corsConfig = ConfigSettingEnum.Cors.GetConfig().AsEmpty();
        if (corsConfig.Length == 0)
        {
            return;
        }

        services.AddCors(options =>
        {
            var origins = corsConfig.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            options.AddPolicy(Constant.CorsPolicy, policyBuilder =>
            {
                if (origins is ["*"])
                {
                    policyBuilder.SetIsOriginAllowed(_ => true).AllowAnyHeader().AllowCredentials();
                }
                else
                {
                    policyBuilder.WithOrigins(origins).AllowAnyHeader().AllowCredentials();
                }
            });
        });
    }

    private static void RegisterMvcAndSwagger(IServiceCollection services, string appName, string appVersion)
    {
        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            });

        var useSwagger = ConfigSettingEnum.UseSwagger.GetConfig().AsInt() == 1;
        if (!useSwagger)
        {
            return;
        }

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.CustomSchemaIds(type => type.FullName);
            c.SwaggerDoc("v1", new OpenApiInfo { Title = appName, Version = appVersion });
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "JWT: Bearer {token}",
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });
    }

    #endregion

    #region Pipeline

    private static void ConfigurePipeline(
        WebApplication app,
        string appName,
        string appVersion,
        Action<WebApplication>? registerRoutingUrl)
    {
        var useSwagger = ConfigSettingEnum.UseSwagger.GetConfig().AsInt() == 1;
        if (useSwagger)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", $"{appName} {appVersion}");
            });
        }

        if (ConfigSettingEnum.Https.GetConfig().AsInt() == 1)
        {
            app.Use(async (ctx, next) =>
            {
                ctx.Request.Scheme = "https";
                await next();
            });
        }

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
        }

        app.UseSerilogRequestLogging();
        app.UseRouting();

        var corsConfig = ConfigSettingEnum.Cors.GetConfig().AsEmpty();
        if (corsConfig.Length > 0)
        {
            app.UseCors(Constant.CorsPolicy);
        }

        var forwardedHeaderOptions = new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedHost |
                               ForwardedHeaders.XForwardedFor |
                               ForwardedHeaders.XForwardedProto
        };
        forwardedHeaderOptions.KnownProxies.Clear();
#pragma warning disable ASPDEPR005 // KnownNetworks obsolete in ASP.NET Core 10 — use KnownIPNetworks
        forwardedHeaderOptions.KnownNetworks.Clear();
#pragma warning restore ASPDEPR005
        app.UseForwardedHeaders(forwardedHeaderOptions);
        app.UseResponseCompression();

        if (ConfigSettingEnum.AuthenticationType.GetConfig().AsInt() > 0)
        {
            app.UseAuthentication();
            app.UseAuthorization();
        }

        // Map gRPC / custom endpoints của từng microservice
        registerRoutingUrl?.Invoke(app);

        app.MapControllers();
        app.MapGet("/", () => Results.Ok(new { app = appName, version = appVersion }));
        app.MapCodeFirstGrpcReflectionService();
    }

    #endregion
}
