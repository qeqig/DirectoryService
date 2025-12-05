using CSharpFunctionalExtensions;
using DirectoryService.Domain.Location;
using Shared;

namespace DirectoryService.Application.IRepositories;

public interface ILocationsRepository
{
    Task<Result<Guid, Error>> AddAsync(Location location,  CancellationToken cancellationToken);
}