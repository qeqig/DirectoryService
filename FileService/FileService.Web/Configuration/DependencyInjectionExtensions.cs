using FileService.Core;
using FileService.Infrastructure.S3;
using Framework.EndpointResults;
using Framework.Logging;
using Framework.Swagger;

namespace FileService.Web.Configuration;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddLogging(configuration, "FileService")
            .AddOpenApiSpec("FileService", "v1")
            .AddEndpoints(typeof(DependencyInjectionCoreExtensions).Assembly)
            .AddS3(configuration);

        services.AddCore(configuration);

        return services;
    }
}