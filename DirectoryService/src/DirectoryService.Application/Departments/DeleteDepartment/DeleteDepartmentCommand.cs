using Core.Abstractions;

namespace DirectoryService.Application.Departments.DeleteDepartment;

public record DeleteDepartmentCommand(Guid Id) : ICommand;