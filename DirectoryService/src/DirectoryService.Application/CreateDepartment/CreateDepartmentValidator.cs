using DirectoryService.Application.Validation;
using DirectoryService.Contracts.Department;
using DirectoryService.Domain.Department.VO;
using FluentValidation;
using Shared;

namespace DirectoryService.Application.CreateDepartment;

public class CreateDepartmentValidator : AbstractValidator<CreateDepartmentDto>
{
    public CreateDepartmentValidator()
    {
        RuleFor(d => d.Name)
            .MustBeValueObject(DepartmentName.Create);

        RuleFor(d => d.Identifier)
            .MustBeValueObject(Identifier.Create);

        RuleFor(d => d.ParentId)
            .Must(parentId => parentId == null || parentId != Guid.Empty)
            .WithError(GeneralErrors.ValueIsRequired("parent id"));

        RuleFor(d => d.LocationsId)
            .Must(locationIds => locationIds.Distinct().Count() == locationIds.Length)
            .WithError(GeneralErrors.ValueIsInvalid("location ids"))
            .NotEmpty()
            .WithError(GeneralErrors.ValueIsRequired("location ids"));
    }
}