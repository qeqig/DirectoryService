using System.Linq.Expressions;
using CSharpFunctionalExtensions;
using DirectoryService.Domain.Department;
using DirectoryService.Domain.Department.VO;
using Shared.SharedKernel;
using Path = DirectoryService.Domain.Department.VO.Path;

namespace DirectoryService.Application.IRepositories;

public interface IDepartmentsRepository
{
    Task<Result<Guid, Error>> AddAsync(Department department, CancellationToken cancellationToken = default);

    Task<UnitResult<Errors>> ChekExisting(Guid[] ids, CancellationToken cancellationToken = default);

    Task<UnitResult<Error>> DeleteLocations(DepartmentId departmentId, CancellationToken cancellationToken = default);

    Task<bool> CheckExistChildForParent(DepartmentId departmentId, DepartmentId parentId, CancellationToken cancellationToken = default);

    Task<Department?> GetBy(Expression<Func<Department, bool>> predicate, CancellationToken cancellationToken = default);

    Task<Result<Department, Error>> GetByIdWithLock(DepartmentId departmentId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Department>> GetChildWithLock(Path path, CancellationToken cancellationToken = default);

    Task<UnitResult<Error>> UpdateChildren(Path path, Department department, CancellationToken cancellationToken = default);

    Task<UnitResult<Error>> UpdateChildAndPath(Path oldDepartmentPath, Path departmentPath, DepartmentId departmentId, CancellationToken cancellationToken = default);
}