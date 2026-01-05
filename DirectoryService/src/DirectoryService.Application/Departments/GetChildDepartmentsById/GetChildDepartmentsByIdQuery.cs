using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Department.GetChildDepartmentsById;

namespace DirectoryService.Application.Departments.GetChildDepartmentsById;

public record GetChildDepartmentsByIdQuery(Guid ParentId, GetChildDepartmentsByIdDto Dto) : IQuery;