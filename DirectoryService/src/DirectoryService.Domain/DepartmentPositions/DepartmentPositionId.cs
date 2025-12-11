using DirectoryService.Domain.Department.VO;

namespace DirectoryService.Domain.DepartmentPositions;

public sealed record DepartmentPositionId
{
    public DepartmentPositionId(Guid value)
    {
        Value = value;
    }

    public Guid Value { get; }

    public static DepartmentPositionId Create() => new (Guid.NewGuid());

    public static DepartmentPositionId Current(Guid id) => new(id);
}
