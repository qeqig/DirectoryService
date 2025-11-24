using CSharpFunctionalExtensions;

namespace DirectoryService.Domain;

public class DepartmentPosition
{
    // EF core
    private DepartmentPosition() { }

    private DepartmentPosition(Guid departmentId, Guid positionId)
    {
        Id = Guid.NewGuid();
        DepartmentId = departmentId;
        PositionId = positionId;
    }

    public Guid Id { get; private set; }

    public Guid DepartmentId { get; private set; }

    public Guid PositionId { get; private set; }

    public static Result<DepartmentPosition> Create(Guid departmentId, Guid positionId)
    {
        if (departmentId == Guid.Empty || positionId == Guid.Empty)
            return Result.Failure<DepartmentPosition>("Department Id and Position Id cannot be empty.");

        return new DepartmentPosition(departmentId, positionId);
    }
}