using BaseApplication;

// SystemManager — microservice bootstrap theo script BaseProgram.
// Khi có service/repository, đăng ký tại đây giống TYT.SystemManager.
BaseProgram.Run(args,
    services =>
    {
        // services.AddTransient<IMenuService, MenuService>();
        // services.AddTransient<IMenuRepository, MenuRepository>();
        return services;
    },
    endpoints =>
    {
        // endpoints.MapGrpcService<MenuService>();
    });
