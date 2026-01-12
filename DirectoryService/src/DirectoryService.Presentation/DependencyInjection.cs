using DirectoryService.Application;

namespace DirectoryService.Presentation;

public static class DependencyInjection
{
    public static IServiceCollection AddProgramDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddWebDependencies()
            .AddApplication();

        return services;
    }

    private static IServiceCollection AddWebDependencies(this IServiceCollection services)
    {
        services.AddControllers();
        services.AddOpenApi();

        return services;
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
