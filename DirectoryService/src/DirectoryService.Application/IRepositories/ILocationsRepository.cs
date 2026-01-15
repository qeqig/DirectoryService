using CSharpFunctionalExtensions;
using DirectoryService.Domain.Department.VO;
using DirectoryService.Domain.Location;
using DirectoryService.Domain.Location.VO;
using Shared.SharedKernel;

namespace DirectoryService.Application.IRepositories;

public interface ILocationsRepository
{
    Task<Result<Guid, Error>> AddAsync(Location location,  CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyCollection<Location>, Errors>> GetByIds(List<LocationId> locationIds, CancellationToken cancellationToken = default);

    Task<UnitResult<Errors>> CheckExisting(Guid[] ids, CancellationToken cancellationToken = default);

    Task<UnitResult<Errors>> DeactivateLocations(DepartmentId departmentId, CancellationToken cancellationToken = default);
}