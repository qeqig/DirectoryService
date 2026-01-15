using Core.Validation;
using DirectoryService.Contracts.Department.UpdateDepartments;
using FluentValidation;
using Shared.SharedKernel;

namespace DirectoryService.Application.Departments.UpdateDepartmentsLocation;

public class UpdateDepartmentLocationValidator : AbstractValidator<UpdateDepartmentLocationDto>
{
    public UpdateDepartmentLocationValidator()
    {
        RuleFor(d => d.LocationIds)
            .NotEmpty()
            .WithError(GeneralErrors.ValueIsRequired("LocationIds"))
            .Must(locationIds => locationIds.Distinct().Count() == locationIds.Length)
            .WithError(GeneralErrors.ValueIsInvalid("LocationIds"));
    }
}