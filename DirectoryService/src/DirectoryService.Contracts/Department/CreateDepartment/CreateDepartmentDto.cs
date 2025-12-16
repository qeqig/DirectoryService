namespace DirectoryService.Contracts.Department.CreateDepartment;

public record CreateDepartmentDto(string Name, string Identifier, Guid? ParentId, Guid[] LocationsId);
