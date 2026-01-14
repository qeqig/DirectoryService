using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using Shared.SharedKernel;

namespace DirectoryService.Domain.Department.VO;

public sealed record Identifier
{
    public const int MAX_LENGTH = 150;

    public const int MIN_LENGTH = 3;

    private static readonly Regex _latin = new Regex("^[a-zA-Z0-9]+$",  RegexOptions.Compiled);

    private Identifier(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<Identifier, Error> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return GeneralErrors.ValueIsRequired("department identifier");

        if (value.Length is > MAX_LENGTH or < MIN_LENGTH)
            return GeneralErrors.ValueIsInvalid("department identifier");

        if (!_latin.IsMatch(value))
            return GeneralErrors.ValueIsInvalid("department identifier");

        return new Identifier(value);
    }
}