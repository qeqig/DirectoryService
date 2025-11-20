using CSharpFunctionalExtensions;

namespace DirectoryService.Domain.Department.VO;

public record DepartmentName
{
    public const int MAX_LENGTH = 150;
    public const int MIN_LENGTH = 3;
    private DepartmentName(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<DepartmentName> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            Result.Failure<DepartmentName>($"Name cannot be empty");

        if (value.Length > MAX_LENGTH || value.Length < MIN_LENGTH)
            Result.Failure<DepartmentName>($"The name must be between {MIN_LENGTH} and {MAX_LENGTH} characters long.");

        return new DepartmentName(value);
    }
}