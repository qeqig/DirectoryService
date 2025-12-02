using DirectoryService.Application;
using Serilog;
using Serilog.Exceptions;

namespace DirectoryService.Presentation;

public static class DependencyInjection
{
    public static IServiceCollection AddProgramDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddSerilogLogging(configuration)
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

    private static IServiceCollection AddSerilogLogging(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSerilog((sp, lc) => lc
            .ReadFrom.Configuration(configuration)
            .ReadFrom.Services(sp)
            .Enrich.FromLogContext()
            .Enrich.WithExceptionDetails()
            .Enrich.WithProperty("ServiceName", "DirectoryService"));

        return services;
    }
}
