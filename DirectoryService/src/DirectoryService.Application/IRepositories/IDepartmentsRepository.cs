using CSharpFunctionalExtensions;
using DirectoryService.Domain.Department;
using Shared;

namespace DirectoryService.Application.IRepositories;

public interface IDepartmentsRepository
{
    Task<Result<Guid, Error>> AddAsync(Department department, CancellationToken cancellationToken = default);

    Task<Result<Department, Error>> GetById(Guid departmentId, CancellationToken cancellationToken = default);

    Task<UnitResult<Errors>> ChekExisting(Guid[] ids, CancellationToken cancellationToken = default);
}