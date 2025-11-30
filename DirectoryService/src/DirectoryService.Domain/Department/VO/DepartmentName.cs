using CSharpFunctionalExtensions;
using Shared;

namespace DirectoryService.Domain.Department.VO;

public sealed record DepartmentName
{
    public const int MAX_LENGTH = 150;
    public const int MIN_LENGTH = 3;
    private DepartmentName(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<DepartmentName, Error> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return GeneralErrors.ValueIsRequired("department name");

        if (value.Length is > MAX_LENGTH or < MIN_LENGTH)
            return GeneralErrors.ValueIsInvalid("department name");

        return new DepartmentName(value);
    }
}