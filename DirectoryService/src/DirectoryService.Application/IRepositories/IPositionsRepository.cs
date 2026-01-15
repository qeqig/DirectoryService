using CSharpFunctionalExtensions;
using DirectoryService.Domain.Department.VO;
using DirectoryService.Domain.Position;
using Shared.SharedKernel;

namespace DirectoryService.Application.IRepositories;

public interface IPositionsRepository
{
    Task<Result<Guid, Error>> AddAsync(Position position, CancellationToken cancellationToken = default);

    Task<UnitResult<Errors>> DeactivatePosition(DepartmentId departmentId, CancellationToken cancellationToken = default);
}