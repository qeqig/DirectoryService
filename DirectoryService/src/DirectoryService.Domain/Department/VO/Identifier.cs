using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;

namespace DirectoryService.Domain.Department.VO;

public record Identifier
{
    public const int MAX_LENGTH = 150;

    public const int MIN_LENGTH = 3;

    private static readonly Regex _latin = new Regex("^[a-zA-Z0-9]+$",  RegexOptions.Compiled);

    private Identifier(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<Identifier> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<Identifier>("Identifier cannot be empty");

        if (value.Length > MAX_LENGTH || value.Length < MIN_LENGTH)
            return Result.Failure<Identifier>($"Identifier length must be between {MIN_LENGTH} and {MAX_LENGTH}.");

        if (!_latin.IsMatch(value))
            return Result.Failure<Identifier>("The identifier must contain only Latin letters");

        return new Identifier(value);
    }
}