using CSharpFunctionalExtensions;
using Shared.SharedKernel;

namespace DirectoryService.Domain.Position.VO;

public record Description
{
    public const int MAX_LENGTH_DESCRIPTION = 1000;

    private Description(string? value)
    {
        Value = value;
    }

    public string? Value;

    public static Result<Description, Error> Create(string? value)
    {
        if (value is { Length: > MAX_LENGTH_DESCRIPTION })
            return GeneralErrors.ValueIsInvalid("position description");

        return new Description(value);
    }
}