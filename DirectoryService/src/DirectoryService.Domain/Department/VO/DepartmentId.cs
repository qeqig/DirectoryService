namespace DirectoryService.Domain.Department.VO;

public sealed record DepartmentId : IComparable<DepartmentId>
{
    private DepartmentId(Guid value)
    {
        Value = value;
    }

    public Guid Value { get; } = Guid.Empty;

    public static DepartmentId Create() => new(Guid.NewGuid());

    public static DepartmentId Current(Guid id) => new(id);

    public int CompareTo(DepartmentId? other)
    {
        if (other is null) return 1;
        return Value.CompareTo(other.Value);
    }
}