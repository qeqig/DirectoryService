using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Department.MoveDepartments;

namespace DirectoryService.Application.Departments.MoveDepartment;

public record MoveDepartmentCommand(Guid DepartmentId, MoveDepartmentsDto Dto) : ICommand;