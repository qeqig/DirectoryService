using CSharpFunctionalExtensions;
using DirectoryService.Domain.DepartmentPositions;
using DirectoryService.Domain.Position.VO;
using Shared.SharedKernel;

namespace DirectoryService.Domain.Position;

public sealed class Position : Entity<PositionId>
{
    private readonly List<DepartmentPosition> _departments;

    // EF core
    private Position(PositionId id)
        : base(id) { }

    private Position(
        PositionId id,
        PositionName name,
        Description? description,
        IEnumerable<DepartmentPosition> departments)
        : base(id)
    {
        Name = name;
        Description = description;
        CreatedAt = DateTime.UtcNow;
        IsActive = true;
        _departments = departments.ToList();
    }

    public PositionName Name { get; private set; } = null!;

    public Description? Description { get; private set; }

    public bool IsActive { get; private set; } = true;

    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

    public IReadOnlyList<DepartmentPosition> Departments => _departments.AsReadOnly();

    public static Result<Position, Error> Create(PositionName name, Description? description,  IEnumerable<DepartmentPosition> departments)
    {
        var newPositionId = PositionId.Create();

        return new Position(newPositionId, name, description, departments);
    }
}