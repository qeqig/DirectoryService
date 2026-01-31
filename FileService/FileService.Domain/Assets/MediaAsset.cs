using CSharpFunctionalExtensions;
using Shared.SharedKernel;

namespace FileService.Domain;

public abstract class MediaAsset
{
    public Guid Id { get; protected set; }

    public MediaData MediaData { get; protected set; } = null!;

    public AssetType AssetType { get; protected set; }

    public MediaStatus Status { get; protected set; }

    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; protected set; } = DateTime.UtcNow;

    public StorageKey? RawKey { get; protected set; }

    public StorageKey? FinalKey { get; protected set; }

    public MediaOwner Owner { get; protected set; } = null!;

    protected MediaAsset() { }

    protected MediaAsset(
        Guid id,
        MediaData mediaData,
        MediaStatus status,
        AssetType assetType,
        MediaOwner owner,
        StorageKey? rawKey,
        StorageKey? finalKey)
    {
        Id = id;
        MediaData = mediaData;
        Status = status;
        AssetType = assetType;
        Owner = owner;
        RawKey = rawKey;
        FinalKey = finalKey;
    }

    public UnitResult<Error> MarkUploaded(DateTime time)
    {
        if (Status != MediaStatus.UPLOADING)
            return GeneralErrors.ValueIsInvalid(nameof(Status));

        Status = MediaStatus.UPLOADED;
        UpdatedAt = DateTime.UtcNow;

        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> MarkReady(StorageKey finalKey, DateTime time)
    {
        if (Status != MediaStatus.UPLOADING)
            return GeneralErrors.ValueIsInvalid(nameof(Status));

        Status = MediaStatus.READY;
        UpdatedAt = DateTime.UtcNow;
        FinalKey = finalKey;

        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> MarkFailed(DateTime time)
    {
        if (Status != MediaStatus.UPLOADED)
            return GeneralErrors.ValueIsInvalid(nameof(Status));

        Status = MediaStatus.FAILED;
        UpdatedAt = DateTime.UtcNow;

        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> MarkDeleted(DateTime time)
    {
        Status = MediaStatus.DELETED;
        UpdatedAt = DateTime.UtcNow;

        return UnitResult.Success<Error>();
    }

    public static Result<MediaAsset, Error> CreateForUpload(MediaData mediaData, AssetType assetType)
    {
        var assetId = Guid.NewGuid();
        var mediaOwnerResult = MediaOwner.ForDepartment(Guid.NewGuid());
        if (mediaOwnerResult.IsFailure) return mediaOwnerResult.Error;
        var mediaOwner = mediaOwnerResult.Value;

        switch (assetType)
        {
            case AssetType.VIDEO:
                var videoResult = VideoAsset.CreateForUpload(assetId, mediaData, mediaOwner);
                return videoResult.IsFailure ? Result.Failure<MediaAsset, Error>(videoResult.Error) : Result.Success<MediaAsset, Error>(videoResult.Value);
            case AssetType.PREVIEW:
                var previewResult = PreviewAsset.CreateForUpload(assetId, mediaData, mediaOwner);
                return previewResult.IsFailure ? Result.Failure<MediaAsset, Error>(previewResult.Error) : Result.Success<MediaAsset, Error>(previewResult.Value);

            default: throw new ArgumentOutOfRangeException(nameof(assetType), assetType, null);
        }
    }
}

public enum MediaStatus
{
    UPLOADING,
    UPLOADED,
    READY,
    FAILED,
    DELETED,
}