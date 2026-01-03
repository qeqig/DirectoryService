namespace DirectoryService.Contracts.Department.GetRootWithChildren;

public record GetRootWithChildrenResponse(List<DepartmentDto> Roots, int RootCount);