namespace DirectoryService.Domain.Department.VO;

public sealed record Path
{
    private const char Separator = '.';
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
}