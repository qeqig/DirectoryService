using CSharpFunctionalExtensions;
using DirectoryService.Domain.Department.VO;
using DirectoryService.Domain.Position.VO;

namespace DirectoryService.Domain;

public class DepartmentPosition
{
    // EF core
    private DepartmentPosition() { }

    private DepartmentPosition(DepartmentId departmentId, PositionId positionId)
    {
        Id = Guid.NewGuid();
        DepartmentId = departmentId;
        PositionId = positionId;
    }

    public Guid Id { get; private set; }

    public DepartmentId DepartmentId { get; private set; }

    public PositionId PositionId { get; private set; }
}