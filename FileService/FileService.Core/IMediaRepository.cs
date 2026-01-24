using CSharpFunctionalExtensions;
using FileService.Domain;
using Shared.SharedKernel;

namespace FileService.Core;

public interface IMediaRepository
{
    Task<Result<Guid, Error>> AddAsync(MediaAsset mediaAsset, CancellationToken cancellationToken = default);
}