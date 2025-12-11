namespace DirectoryService.Contracts.Department;

public record CreateDepartmentDto(string Name, string Identifier, Guid? ParentId, Guid[] LocationsId);
