using CSharpFunctionalExtensions;

namespace DirectoryService.Domain.Department.VO;

public sealed record Path
{
    private const char Separator = '.';
    
    private const string Delete = "deleted_";
    private Path(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Path CreateParent(Identifier identifier)
    {
        return new Path(identifier.Value);
    }

    public Path CreateChild(Department parent, Identifier childIdentifier)
    {
        return new Path(parent.Path.Value + Separator + childIdentifier.Value);
    }

    public static Result<Path> CreateForSoftDelete(Path path)
    {
        var value = path.Value;
        value = Delete + value;
        return new Path(value);
    }
}