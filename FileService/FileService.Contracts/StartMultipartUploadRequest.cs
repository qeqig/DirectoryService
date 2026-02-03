using Core.Abstractions;

namespace FileService.Contracts;

public record StartMultipartUploadRequest(
    string FileName,
    string AssetType,
    string ContentType,
    long Size,
    string Context,
    Guid ContextId) : ICommand;