using FileService.Core;
using FileService.Infrastructure.Postgres.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FileService.Infrastructure.Postgres;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructurePostgres(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IMediaRepository,  MediaRepository>();

        return services;
    }
}