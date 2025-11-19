using CSharpFunctionalExtensions;

namespace DirectoryService.Domain.Department.VO;

public record Path
{
    private Path(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<Path> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<Path>("Path cannot be empty");

        return new Path(value);
    }
}