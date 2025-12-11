namespace DirectoryService.Domain.Location.VO;

public sealed record LocationId : IComparable<LocationId>
{
    private LocationId(Guid value)
    {
        Value = value;
    }

    public Guid Value { get; }

    public static LocationId Create() => new(Guid.NewGuid());

    public static LocationId Current(Guid id) => new(id);

    public static LocationId[] Current(Guid[] id) => id.Select(Current).ToArray();

    public int CompareTo(LocationId? other)
    {
        if (other is null) return 1;
        return Value.CompareTo(other.Value);
    }
}
