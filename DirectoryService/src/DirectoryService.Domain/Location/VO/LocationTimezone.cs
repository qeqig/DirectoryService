using CSharpFunctionalExtensions;

namespace DirectoryService.Domain.Location.VO;

public record LocationTimezone
{
    private LocationTimezone(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<LocationTimezone> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<LocationTimezone>($"Time zone is empty");

        try
        {
            TimeZoneInfo.FindSystemTimeZoneById(value);
        }
        catch (TimeZoneNotFoundException)
        {
            return Result.Failure<LocationTimezone>($"Invalid IANA time zone code");
        }

        return new LocationTimezone(value);
    }
}