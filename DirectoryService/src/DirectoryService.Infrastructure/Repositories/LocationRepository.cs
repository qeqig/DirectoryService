using DirectoryService.Application.Locations;
using DirectoryService.Domain.Location;

namespace DirectoryService.Infrastructure.Repositories;

public class LocationRepository : ILocationsRepository
{
    private readonly DirectoryServiceDbContext _dbContext;

    public LocationRepository(DirectoryServiceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> AddAsync(Location location, CancellationToken cancellationToken)
    {
        await _dbContext.Locations.AddAsync(location, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return location.Id.Value;
    }
}