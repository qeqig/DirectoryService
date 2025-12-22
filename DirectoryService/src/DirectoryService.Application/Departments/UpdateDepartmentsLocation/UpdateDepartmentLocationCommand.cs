using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Department.UpdateDepartments;

namespace DirectoryService.Application.Departments.UpdateDepartmentsLocation;

public record UpdateDepartmentLocationCommand(Guid DepartmentId, UpdateDepartmentLocationDto Dto) : ICommand;
