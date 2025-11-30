using CSharpFunctionalExtensions;
using Shared;

namespace DirectoryService.Domain.Location.VO;

public sealed record LocationName
{
    public const int MAX_LENGTH = 120;

    public const int MIN_LENGTH = 3;

    private LocationName(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<LocationName, Error> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return GeneralErrors.ValueIsRequired("location name");

        if (value.Length > MAX_LENGTH || value.Length < MIN_LENGTH)
            return GeneralErrors.ValueIsInvalid("location name");

        return new LocationName(value);
    }
}