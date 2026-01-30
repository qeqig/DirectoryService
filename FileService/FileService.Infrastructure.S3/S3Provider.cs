using Amazon.S3;
using Amazon.S3.Model;
using CSharpFunctionalExtensions;
using FileService.Contracts;
using FileService.Core;
using FileService.Domain;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.SharedKernel;

namespace FileService.Infrastructure.S3;

public class S3Provider : IS3Provider
{
    private readonly ILogger<S3Provider> _logger;
    private readonly IAmazonS3 _s3Client;
    private readonly S3Options _s3Options;

    private readonly SemaphoreSlim _requestsSemaphore;

    public S3Provider(IAmazonS3 s3Client, IOptions<S3Options> s3Options, ILogger<S3Provider> logger)
    {
        _s3Client = s3Client;
        _logger = logger;
        _s3Options = s3Options.Value;
        _requestsSemaphore = new SemaphoreSlim(_s3Options.MaxConcurrentRequests);
    }

    public async Task<Result<string, Error>> StartMultipartUploadAsync(
        string bucketName,
        string key,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new InitiateMultipartUploadRequest
            {
                BucketName = bucketName,
                Key = key,
                ContentType = contentType,
            };

            var result = await _s3Client.InitiateMultipartUploadAsync(request, cancellationToken);

            return result.UploadId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting multipart upload");
            return S3ErrorMapper.ToError(ex);
        }
    }

    public async Task<Result<IReadOnlyList<string>, Error>> GenerateAllChunksUploadUrlsAsync(
        string bucketName,
        string key,
        string uploadId,
        int totalChunks,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var tasks = Enumerable.Range(1, totalChunks)
                .Select(async partNumber =>
                {
                    await _requestsSemaphore.WaitAsync(cancellationToken);

                    try
                    {
                        var request = new GetPreSignedUrlRequest
                        {
                            BucketName = bucketName,
                            Key = key,
                            Verb = HttpVerb.PUT,
                            UploadId = uploadId,
                            PartNumber = partNumber,
                            Expires = DateTime.UtcNow.AddHours(_s3Options.UploadUrlExpirationHours),
                            Protocol = _s3Options.WithSsl ? Protocol.HTTP : Protocol.HTTPS,
                        };

                        string? url = await _s3Client.GetPreSignedURLAsync(request);

                        return url;
                    }
                    finally
                    {
                        _requestsSemaphore.Release();
                    }
                });

            string[] results = await Task.WhenAll(tasks);

            return results;

        }
        catch (Exception ex)
        {
            return S3ErrorMapper.ToError(ex);
        }

    }

    public async Task<Result<string, Error>> GenerateDownloadUrlAsync(StorageKey storageKey)
    {
        try
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = storageKey.Bucket,
                Key = storageKey.Key,
                Verb = HttpVerb.GET,
                Expires = DateTime.UtcNow.AddHours(_s3Options.DownloadUrlExpirationHours),
            };

            var response = await _s3Client.GetPreSignedURLAsync(request);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating download url");
            return S3ErrorMapper.ToError(ex);
        }
    }

    public async Task<Result<string, Error>> CompleteMultipartUploadAsync(
        string bucketName,
        string key,
        string uploadId,
        IReadOnlyList<PartETagDto> partETags,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new CompleteMultipartUploadRequest
            {
                BucketName = bucketName,
                Key = key,
                UploadId = uploadId,
                PartETags = partETags
                    .Select(p => new PartETag
                    {
                        ETag = p.ETag, PartNumber = p.PartNumber,
                    }).ToList(),
            };

            CompleteMultipartUploadResponse response = await _s3Client.CompleteMultipartUploadAsync(request, cancellationToken);

            return response.Key;

        }
        catch (Exception ex)
        {
            return S3ErrorMapper.ToError(ex);
        }
    }

    public async Task<UnitResult<Error>> UploadFileAsync(
        StorageKey storageKey, Stream stream, MediaData mediaData, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new PutObjectRequest
            {
                BucketName = storageKey.Bucket,
                Key = storageKey.Key,
                ContentType = mediaData.ContentType.Value,
                InputStream = stream,
            };

            await _s3Client.PutObjectAsync(request, cancellationToken);

            return UnitResult.Success<Error>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file");
            return S3ErrorMapper.ToError(ex);
        }
    }

    public async Task<Result<string, Error>> DownloadFileAsync(
        StorageKey storageKey, string tempPath, CancellationToken cancellationToken = default)
    {
        try
        {
            var getObjectRequest = new GetObjectRequest
            {
                BucketName = storageKey.Bucket,
                Key = storageKey.Key,
            };

            var getObjectResponse = await _s3Client.GetObjectAsync(getObjectRequest, cancellationToken);

            var pathToFile = tempPath + "\\" + storageKey.Key;

            await getObjectResponse.WriteResponseStreamToFileAsync(pathToFile, true, cancellationToken);

            return getObjectResponse.Key;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading file");
            return S3ErrorMapper.ToError(ex);
        }
    }

    public async Task<Result<string, Error>> DeleteFileAsync(StorageKey storageKey, CancellationToken cancellationToken = default)
    {
        try
        {
            var deleteObjectRequest = new DeleteObjectRequest
            {
                BucketName = storageKey.Bucket,
                Key = storageKey.Key,
            };

            var deleteObjectResponse = await _s3Client.DeleteObjectAsync(deleteObjectRequest, cancellationToken);
            return deleteObjectResponse.DeleteMarker;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file");
            return S3ErrorMapper.ToError(ex);
        }
    }

    public async Task<Result<string, Error>> GenerateUploadUrlAsync(
        StorageKey storageKey, MediaData mediaData, CancellationToken cancellationToken = default)
    {
        try
        {
            var createPresignedPostRequest = new CreatePresignedPostRequest
            {
                BucketName = storageKey.Bucket,
                Key = storageKey.Key,
                Expires = DateTime.UtcNow.AddHours(_s3Options.UploadUrlExpirationHours),
            };

            var presignedPostResponse = await _s3Client.CreatePresignedPostAsync(createPresignedPostRequest);

            return presignedPostResponse.Url;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating upload url");
            return S3ErrorMapper.ToError(ex);
        }
    }

    public async Task<Result<IReadOnlyList<string>, Error>> GenerateDownloadUrlsAsync(IEnumerable<StorageKey> storageKeys)
    {
        try
        {
            var tasks = storageKeys.Select(async key =>
            {
                await _requestsSemaphore.WaitAsync();

                try
                {
                    return await GenerateDownloadUrlAsync(key);
                }
                finally
                {
                    _requestsSemaphore.Release();
                }
            });
            var downloadUrlsResult = await Task.WhenAll(tasks);

            return downloadUrlsResult.Select(d => d.Value).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating download urls");
            return S3ErrorMapper.ToError(ex);
        }
    }

}