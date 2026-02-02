namespace FileService.Contracts;

public sealed record GetChunkUploadUrlResponse(string UploadUrl, int PartNumber);