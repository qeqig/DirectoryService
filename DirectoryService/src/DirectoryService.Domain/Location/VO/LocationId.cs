namespace DirectoryService.Domain.Location.VO;

public record LocationId : IComparable<LocationId>
{
    private LocationId(Guid value)
    {
        Value = value;
    }

    public Guid Value { get; }

    public static LocationId Create() => new(Guid.NewGuid());

    public static LocationId Current(Guid id) => new(id);

    public int CompareTo(LocationId? other)
    {
        if (other is null) return 1;
        return Value.CompareTo(other.Value);
    }
}
