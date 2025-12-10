using CSharpFunctionalExtensions;
using DirectoryService.Application.IRepositories;
using DirectoryService.Domain.Location;
using DirectoryService.Domain.Location.VO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using Shared;

namespace DirectoryService.Infrastructure.Locations;

public class LocationRepository : ILocationsRepository
{
    private readonly DirectoryServiceDbContext _dbContext;
    private readonly ILogger<LocationRepository> _logger;

    public LocationRepository(DirectoryServiceDbContext dbContext,  ILogger<LocationRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Result<Guid, Error>> AddAsync(Location location, CancellationToken cancellationToken = default)
    {
        string name = location.LocationName.Value;
        string address = location.Address.ToString();

        try
        {
            await _dbContext.Locations.AddAsync(location, cancellationToken);

            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Location {Location.Id} created", location.Id);

            return location.Id.Value;
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx)
        {
            if (pgEx is { SqlState: PostgresErrorCodes.UniqueViolation, ConstraintName: not null })
            {
                if (pgEx.ConstraintName.Contains("name"))
                    return GeneralErrors.AlreadyExist(name);
            }

            if (pgEx.ConstraintName.Contains("address"))
                return GeneralErrors.AlreadyExist(address);

            _logger.LogError(ex, "Error adding location with name {name}", name);
            return GeneralErrors.DataBase();
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogError(ex, "Cancel the operation of adding a location with the name {name}", name);
            return GeneralErrors.DataBase();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failure adding location");
            return GeneralErrors.DataBase();
        }

    }

    public async Task<UnitResult<Errors>> CheckExisting(Guid[] ids, CancellationToken cancellationToken = default)
    {
        var locationId = LocationId.Current(ids);

        var existingIds = await _dbContext.Locations
            .Where(l => locationId.Contains(l.Id) && l.IsActive == true)
            .Select(l => l.Id.Value)
            .ToListAsync(cancellationToken);

        var missingIds = ids.Except(existingIds).ToList();

        var errors = missingIds.Select(id => GeneralErrors.NotFound(id, "location")).ToList();

        return errors.Count != 0
            ? UnitResult.Failure(new Errors(errors))
            : UnitResult.Success<Errors>();
    }

    public async Task<Result<IReadOnlyCollection<Location>, Errors>> GetByIds(List<LocationId> locationIds, CancellationToken cancellationToken = default)
    {
        var locations = await _dbContext.Locations.Where(l => locationIds.Contains(l.Id)).ToListAsync(cancellationToken);
        
        var notFoundIds = locationIds.Except(locations.Select(l => l.Id));

        if (notFoundIds.Any())
        {
            var errors = notFoundIds.Select(l => GeneralErrors.NotFound(l.Value));

            return new Errors(errors);
        }

        return locations;
    }
}