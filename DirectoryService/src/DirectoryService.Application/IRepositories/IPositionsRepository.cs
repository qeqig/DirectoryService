using CSharpFunctionalExtensions;
using DirectoryService.Domain.Position;
using Shared;

namespace DirectoryService.Application.IRepositories;

public interface IPositionsRepository
{
    Task<Result<Guid, Error>> AddAsync(Position position, CancellationToken cancellationToken = default);
}