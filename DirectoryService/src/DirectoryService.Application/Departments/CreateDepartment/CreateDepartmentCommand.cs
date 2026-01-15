using Core.Abstractions;
using DirectoryService.Contracts.Department.CreateDepartment;

namespace DirectoryService.Application.Departments.CreateDepartment;

public record CreateDepartmentCommand(CreateDepartmentDto CreateDepartmentDto) : ICommand;