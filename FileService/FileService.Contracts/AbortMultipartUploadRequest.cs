using Core.Abstractions;

namespace FileService.Contracts;

public sealed record AbortMultipartUploadRequest(Guid MediaAssetId, string UploadId) : ICommand;