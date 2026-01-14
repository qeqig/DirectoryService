using CSharpFunctionalExtensions;
using DirectoryService.Domain.Department;
using DirectoryService.Domain.Location;
using DirectoryService.Infrastructure;
using Shared.SharedKernel;

namespace DirectoryService.IntegrationTests;

public class TestDepartmentData
{
    public async Task<UnitResult<Error>> AddLocationToBase(
        DirectoryServiceDbContext dbContext,
        Location[] locations,
        CancellationToken cancellationToken)
    {
        await dbContext.Locations.AddRangeAsync(locations, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return UnitResult.Success<Error>();
    }

    public async Task<UnitResult<Error>> AddDepartamentToBase(
        DirectoryServiceDbContext dbContext,
        Department[] departaments,
        CancellationToken cancellationToken)
    {
        await dbContext.Departments.AddRangeAsync(departaments, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return UnitResult.Success<Error>();
    }
}
