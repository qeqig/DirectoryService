using DirectoryService.Application.IRepositories;
using DirectoryService.Infrastructure.Departments;
using DirectoryService.Infrastructure.Locations;
using DirectoryService.Infrastructure.Positions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.Infrastructure;

public static class DependencyInjecttion
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped(s => new DirectoryServiceDbContext(configuration));

        services.AddScoped<ILocationsRepository, LocationRepository>();

        services.AddScoped<IDepartmentsRepository, DepartmentRepository>();

        services.AddScoped<IPositionsRepository,  PositionRepository>();

        return services;
    }
}