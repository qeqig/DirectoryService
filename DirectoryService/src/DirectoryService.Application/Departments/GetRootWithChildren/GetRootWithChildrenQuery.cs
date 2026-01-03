using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Department.GetRootWithChildren;

namespace DirectoryService.Application.Departments.GetRootWithChildren;

public record GetRootWithChildrenQuery(GetRootWithChildrenDto Dto) : IQuery;