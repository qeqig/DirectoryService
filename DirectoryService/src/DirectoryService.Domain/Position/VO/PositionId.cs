namespace DirectoryService.Domain.Position.VO;

public record PositionId : IComparable<PositionId>
{

    private PositionId(Guid value)
    {
        Value = value;
    }

    public Guid Value { get; }

    public static PositionId Create() => new(Guid.NewGuid());

    public static PositionId Current(Guid id) => new(id);

    public int CompareTo(PositionId? other)
    {
        if (other is null) return 1;
        return Value.CompareTo(other.Value);
    }
}