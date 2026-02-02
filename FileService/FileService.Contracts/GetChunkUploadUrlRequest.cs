namespace FileService.Contracts;

public sealed record GetChunkUploadUrlRequest(Guid MediaAssetId, string UploadId, int PartNumber);