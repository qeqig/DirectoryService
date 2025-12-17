using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Department;
using DirectoryService.Contracts.Department.CreateDepartment;

namespace DirectoryService.Application.CreateDepartment;

public record CreateDepartmentCommand(CreateDepartmentDto CreateDepartmentDto) : ICommand;