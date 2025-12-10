namespace DirectoryService.Domain.DepartmentLocations;

public sealed record DepartmentLocationId
{
    private DepartmentLocationId(Guid value)
    {
        Value = value;
    }

    public Guid Value { get; }

    public static DepartmentLocationId Create() => new(Guid.NewGuid());

    public static DepartmentLocationId Current(Guid id) => new(id);
}