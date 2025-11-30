using CSharpFunctionalExtensions;
using Shared;

namespace DirectoryService.Domain.Location.VO;

public sealed record LocationTimezone
{
    private LocationTimezone(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<LocationTimezone, Error> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return GeneralErrors.ValueIsRequired("timezone");

        try
        {
            TimeZoneInfo.FindSystemTimeZoneById(value);
        }
        catch (TimeZoneNotFoundException)
        {
            return GeneralErrors.ValueIsInvalid("timezone");
        }

        return new LocationTimezone(value);
    }
}