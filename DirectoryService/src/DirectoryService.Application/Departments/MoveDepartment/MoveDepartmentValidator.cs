using DirectoryService.Application.Validation;
using FluentValidation;
using Shared;

namespace DirectoryService.Application.Departments.MoveDepartment;

public class MoveDepartmentValidator : AbstractValidator<MoveDepartmentCommand>
{
    public MoveDepartmentValidator()
    {
        RuleFor(d => d.DepartmentId)
            .NotNull()
            .NotEmpty()
            .WithError(GeneralErrors.ValueIsRequired("departmentId"));

        RuleFor(d => d.Dto)
            .NotNull()
            .WithError(GeneralErrors.ValueIsRequired("departmentDto"));
    }
}