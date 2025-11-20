using CSharpFunctionalExtensions;

namespace DirectoryService.Domain.Location.VO;

public record LocationName
{
    public const int MAX_LENGTH = 120;

    public const int MIN_LENGTH = 3;

    private LocationName(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<LocationName> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<LocationName>("LocationName cannot be empty.");

        if (value.Length > MAX_LENGTH || value.Length < MIN_LENGTH)
            return Result.Failure<LocationName>($"Location name must be between {MIN_LENGTH} and {MAX_LENGTH} characters long.");

        return new LocationName(value);
    }
}