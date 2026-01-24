using CSharpFunctionalExtensions;
using Shared.SharedKernel;

namespace FileService.Domain;

public sealed record StorageKey
{
    public string Bucket { get; }

    public string Key { get; }

    public string Prefix { get; }

    public string Value { get; }

    public string FullPath { get; }

    private StorageKey() { }

    private StorageKey(string bucket, string prefix, string key)
    {
        Bucket = bucket;
        Prefix = prefix;
        Key = key;
        Value = string.IsNullOrEmpty(Prefix) ? key : $"{Prefix}/{key}";
        FullPath = $"{Bucket}/{Value}";
    }

    public static Result<StorageKey, Error> Create(string location, string? prefix, string key)
    {
        if (string.IsNullOrWhiteSpace(location))
            return GeneralErrors.ValueIsInvalid("location");

        Result<string, Error> normalizedKeyResult = NormalizeSegment(key);
        if (normalizedKeyResult.IsFailure)
            return normalizedKeyResult.Error;

        Result<string, Error> normalizedPrefixKey = NormalizePrefix(prefix);
        if (normalizedPrefixKey.IsFailure)
            return normalizedPrefixKey.Error;

        return new StorageKey(location.Trim(), normalizedPrefixKey.Value, normalizedKeyResult.Value);
    }

    public Result<StorageKey, Error> AppendSegment(string value)
    {
        Result<string, Error> normalizedValueResult = NormalizeSegment(value);
        if (normalizedValueResult.IsFailure)
            return normalizedValueResult.Error;

        var appendedStorageKey = Create(Bucket, Prefix, $"{Key}/{normalizedValueResult.Value}");
        if (appendedStorageKey.IsFailure)
            return appendedStorageKey.Error;

        return appendedStorageKey.Value;
    }

    private static Result<string, Error> NormalizePrefix(string? prefix)
    {
        if (string.IsNullOrWhiteSpace(prefix))
            return string.Empty;

        string[] parts = prefix.Trim().Replace('\\', '/').Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        List<string> normalizedParts = [];
        foreach (string part in parts)
        {
            Result<string, Error> normalizedPart = NormalizeSegment(part);
            if (normalizedPart.IsFailure)
                return normalizedPart;

            if(string.IsNullOrEmpty(normalizedPart.Value))
                normalizedParts.Add(normalizedPart.Value);
        }

        return string.Join('/', normalizedParts);
    }

    private static Result<string, Error> NormalizeSegment(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return GeneralErrors.ValueIsInvalid("key");

        string trimmed = value.Trim();

        if(trimmed.Contains('/', StringComparison.Ordinal) || trimmed.Contains('\\', StringComparison.Ordinal))
            return GeneralErrors.ValueIsInvalid("key");

        return trimmed;
    }
}