using System.Linq.Expressions;
using CSharpFunctionalExtensions;
using FileService.Domain;
using Shared.SharedKernel;

namespace FileService.Core;

public interface IMediaRepository
{
    Task<Result<Guid, Error>> AddAsync(MediaAsset mediaAsset, CancellationToken cancellationToken = default);

    Task<Result<MediaAsset, Error>> GetBy(Expression<Func<MediaAsset, bool>> predicate, CancellationToken cancellationToken = default);

    Task<UnitResult<Error>> SaveChangesAsync(CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<MediaAsset>, Error>> GetManyBy(
        Expression<Func<MediaAsset, bool>> predicate,
        CancellationToken cancellationToken = default);
}