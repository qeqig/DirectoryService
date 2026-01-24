using CSharpFunctionalExtensions;
using Shared.SharedKernel;

namespace FileService.Domain;

public class VideoAsset : MediaAsset
{
    public const long MAX_SIZE = 5_368_709_120;
    public const string LOCATION = "videos";
    public const string ALLOWED_CONTENT_TYPE = "video";
    public const string RAW_PREFIX = "raw";
    public const string HLS_PREFIX = "hls";
    public const string MASTER_PLAYLIST_NAME = "master.m3u8";

    public static readonly string[] AllowedExtensions = ["mp4", "mkv", "avi", "mov"];

    public StorageKey HlsRootKey { get; private set; }

    protected VideoAsset() { }

    private VideoAsset(
        Guid id,
        MediaData mediaData,
        MediaStatus status,
        MediaOwner mediaOwner,
        StorageKey key)
        : base(id, mediaData, status, AssetType.VIDEO, mediaOwner, key)
    {
    }

    public static UnitResult<Error> Validate(MediaData mediaData)
    {
        if (!AllowedExtensions.Contains(mediaData.FileName.Extension))
            return Error.Validation("video.invalid.extension", $"File extension must be one of: {string.Join(",", AllowedExtensions)}");

        if (mediaData.ContentType.Category != MediaType.VIDEO)
            return Error.Validation("video.invalid.content-type", $"File content type must be {ALLOWED_CONTENT_TYPE}");

        if (mediaData.Size > MAX_SIZE)
            return Error.Validation("video.invalid.size", $"File size must be less than {MAX_SIZE} bytes");

        return UnitResult.Success<Error>();
    }

    public static Result<VideoAsset, Error> CreateForUpload(Guid id, MediaData mediaData, MediaOwner owner)
    {
        UnitResult<Error> validationResult = Validate(mediaData);
        if (validationResult.IsFailure)
            return validationResult.Error;

        var key = StorageKey.Create(LOCATION, null, id.ToString());

        if (key.IsFailure)
            return key.Error;

        return new VideoAsset(id, mediaData, MediaStatus.UPLOADING, owner, key.Value);
    }

    public UnitResult<Error> CompleteProcessing(DateTime time)
    {
        Result<StorageKey, Error> newHlsRootKeyResult = HlsRootKey.AppendSegment(MASTER_PLAYLIST_NAME);

        if (newHlsRootKeyResult.IsFailure)
            return newHlsRootKeyResult.Error;

        HlsRootKey = newHlsRootKeyResult.Value;

        MarkReady(HlsRootKey, time);

        return UnitResult.Success<Error>();
    }
}