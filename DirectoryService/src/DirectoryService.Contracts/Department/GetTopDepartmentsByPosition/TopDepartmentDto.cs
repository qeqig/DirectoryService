namespace DirectoryService.Contracts.Department.GetTopDepartmentsByPosition;

public class TopDepartmentDto
{
    public Guid Id { get; init; }

    public string Name { get; init; } = null!;

    public string Path { get; init; } = null!;

    public int Depth { get; init; }

    public DateTime CreatedAt { get; init; }

    public int PositionCount { get; init; }

    public bool IsActive { get; init; }
}