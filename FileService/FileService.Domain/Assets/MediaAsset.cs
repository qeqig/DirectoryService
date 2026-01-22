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

    public StorageKey RawKey { get; protected set; } = null!;

    public StorageKey FinalKey { get; protected set; } = null!;

    public MediaOwner Owner { get; protected set; } = null!;


    protected MediaAsset() { }

    protected MediaAsset(
        Guid id,
        MediaData mediaData,
        MediaStatus status,
        AssetType assetType,
        MediaOwner owner,
        StorageKey rawKey)
    {
        Id = id;
        MediaData = mediaData;
        Status = status;
        AssetType = assetType;
        Owner = owner;
        RawKey = rawKey;
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
}

public enum MediaStatus
{
    UPLOADING,
    UPLOADED,
    READY,
    FAILED,
    DELETED,
}