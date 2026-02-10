using DirectoryService.Application;
using Framework.Middlewares;

namespace DirectoryService.Presentation;

public static class DependencyInjection
{
    public static IServiceCollection AddProgramDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddWebDependencies()
            .AddApplication()
            .AddCors();

        return services;
    }

    private static IServiceCollection AddWebDependencies(this IServiceCollection services)
    {
        services.AddControllers();
        services.AddOpenApi();

        return services;
    }

    public static IApplicationBuilder AddConfigure(this WebApplication app)
    {
        app.UseExceptionMiddleware();

        app.UseCors(builder => builder.WithOrigins("http://localhost:3000")
            .AllowCredentials()
            .AllowAnyHeader()
            .AllowAnyMethod());

        return app;
    }

    public static IServiceCollection AddDistributedCache(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddStackExchangeRedisCache(options =>
        {
            string connectionString = configuration.GetConnectionString("Redis") ??
                                      throw new ArgumentNullException(nameof(connectionString));

            options.Configuration = connectionString;
        });

        services.AddHybridCache();

        return services;
    }
}
