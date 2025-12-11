using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Department;

namespace DirectoryService.Application.CreateDepartment;

public record CreateDepartmentCommand(CreateDepartmentDto CreateDepartmentDto) : ICommand;