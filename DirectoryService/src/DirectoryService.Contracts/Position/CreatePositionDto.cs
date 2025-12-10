namespace DirectoryService.Contracts.Position;

public record CreatePositionDto(string Name, string? Description, Guid[] DepartmentIds);