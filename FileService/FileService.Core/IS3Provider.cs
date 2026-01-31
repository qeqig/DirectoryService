using CSharpFunctionalExtensions;
using FileService.Contracts;
using FileService.Domain;
using Shared.SharedKernel;

namespace FileService.Core;

public interface IS3Provider
{
    Task<Result<string, Error>> StartMultipartUploadAsync(
        StorageKey storageKey,
        string contentType,
        CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<string>, Error>> GenerateAllChunksUploadUrlsAsync(
        StorageKey storageKey,
        string uploadId,
        int totalChunks,
        CancellationToken cancellationToken = default);

    Task<Result<string, Error>> GenerateDownloadUrlAsync(StorageKey storageKey);

    Task<Result<string, Error>> CompleteMultipartUploadAsync(
        StorageKey storageKey,
        string uploadId,
        IReadOnlyList<PartETagDto> partETags,
        CancellationToken cancellationToken = default);

    Task<UnitResult<Error>> UploadFileAsync(
        StorageKey storageKey, Stream stream, MediaData mediaData, CancellationToken cancellationToken = default);

    Task<Result<string, Error>> DownloadFileAsync(
        StorageKey storageKey, string tempPath, CancellationToken cancellationToken = default);

    Task<Result<string, Error>> DeleteFileAsync(StorageKey storageKey, CancellationToken cancellationToken = default);

    Task<Result<string, Error>> GenerateUploadUrlAsync(
        StorageKey storageKey, MediaData mediaData, CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<string>, Error>> GenerateDownloadUrlsAsync(IEnumerable<StorageKey> storageKeys);
}