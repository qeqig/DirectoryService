using CSharpFunctionalExtensions;
using Shared;

namespace DirectoryService.Domain.Department.VO;

public record Path
{
    private Path(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<Path, Error> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return GeneralErrors.ValueIsRequired("department name");

        return new Path(value);
    }
}