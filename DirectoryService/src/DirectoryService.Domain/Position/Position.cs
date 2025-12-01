using CSharpFunctionalExtensions;
using DirectoryService.Domain.Position.VO;
using Shared;

namespace DirectoryService.Domain.Position;

public sealed class Position : Entity<PositionId>
{
    // EF core
    private Position(PositionId id)
        : base(id) { }

    private Position(
        PositionId id,
        PositionName name,
        string? description)
        : base(id)
    {
        Name = name;
        Description = description;
        CreatedAt = DateTime.UtcNow;
        IsActive = true;
    }

    public PositionName Name { get; private set; }

    public string? Description { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    static Result<Position, Error> Create(PositionName name, string? description)
    {
        if (description != null && description.Length > 1000)
            return GeneralErrors.ValueIsInvalid("position");

        var newPositionId = PositionId.Create();

        return new Position(newPositionId, name, description);
    }
}