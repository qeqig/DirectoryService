namespace FileService.Contracts;

public sealed record StartMultipartUploadResponse(
    Guid MediaAssetId,
    string UploadId,
    IReadOnlyList<ChunkUploadUrl> ChunkUploadUrls,
    long ChunkSize);