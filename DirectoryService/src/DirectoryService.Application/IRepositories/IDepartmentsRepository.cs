using CSharpFunctionalExtensions;
using DirectoryService.Domain.Department;
using DirectoryService.Domain.Department.VO;
using Shared;

namespace DirectoryService.Application.IRepositories;

public interface IDepartmentsRepository
{
    Task<Result<Guid, Error>> AddAsync(Department department, CancellationToken cancellationToken = default);

    Task<Result<Department, Error>> GetById(DepartmentId departmentId, CancellationToken cancellationToken = default);

    Task<UnitResult<Errors>> ChekExisting(Guid[] ids, CancellationToken cancellationToken = default);

    Task<UnitResult<Error>> DeleteLocations(DepartmentId departmentId, CancellationToken cancellationToken = default);
}