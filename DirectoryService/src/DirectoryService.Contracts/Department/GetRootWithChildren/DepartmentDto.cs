namespace DirectoryService.Contracts.Department.GetRootWithChildren;

public record DepartmentDto
{
    public Guid Id { get; init; }

    public string Name { get; init; }

    public string Identifier { get; init; }

    public Guid? ParentId { get; init; }

    public string Path { get; init; }

    public int Depth { get; init; }

    public bool IsActive { get; init; }

    public DateTime CreatedAt { get; init; }

    public DateTime UpdatedAt { get; init; }

    public bool HasMoreChildren { get; init; }
}