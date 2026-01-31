namespace FileService.Domain;

public enum AssetType
{
    VIDEO,
    PREVIEW,
    AVATAR,
}

public static class AssetTypeExtensions
{
    public static AssetType ToAssetType(this string value)
    {
        return value switch
        {
            _ when value.Contains("video", StringComparison.InvariantCultureIgnoreCase) => AssetType.VIDEO,
            _ when value.Contains("image", StringComparison.InvariantCultureIgnoreCase) => AssetType.PREVIEW,
            _ => throw new ArgumentException("not found AssetType!"),
        };
    }
}