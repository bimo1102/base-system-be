using System.Reflection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ProtoBuf.Grpc.Configuration;
using ProtoBuf.Grpc.Server;

namespace BaseApplication.Extensions;

public static class GrpcServiceExtensions
{
    /// <summary>
    /// Đăng ký code-first gRPC (protobuf-net.Grpc) — pipeline S2S giữa các microservice.
    /// </summary>
    public static IServiceCollection ConfigCodeFirstGrpc(this IServiceCollection services)
    {
        services.AddCodeFirstGrpc(config =>
        {
            config.ResponseCompressionLevel = System.IO.Compression.CompressionLevel.NoCompression;
            config.MaxReceiveMessageSize = int.MaxValue;
            config.MaxSendMessageSize = int.MaxValue;
        });

        services.TryAddSingleton(
            BinderConfiguration.Create(
                binder: new ServiceBinderWithServiceResolutionFromServiceCollection(services)));

        services.AddCodeFirstGrpcReflection();
        return services;
    }
}

/// <summary>
/// Resolve gRPC service implementation từ DI container khi contract là interface.
/// </summary>
public class ServiceBinderWithServiceResolutionFromServiceCollection(IServiceCollection services)
    : ServiceBinder
{
    public override IList<object> GetMetadata(MethodInfo method, Type contractType, Type serviceType)
    {
        var resolvedServiceType = serviceType;
        if (serviceType.IsInterface)
        {
            resolvedServiceType = services
                                      .SingleOrDefault(x => x.ServiceType == serviceType)
                                      ?.ImplementationType
                                  ?? serviceType;
        }

        return base.GetMetadata(method, contractType, resolvedServiceType);
    }
}
