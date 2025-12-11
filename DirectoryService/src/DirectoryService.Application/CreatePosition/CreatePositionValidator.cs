using DirectoryService.Application.Validation;
using DirectoryService.Contracts.Position;
using DirectoryService.Domain.Position.VO;
using FluentValidation;
using Shared;

namespace DirectoryService.Application.CreatePosition;

public class CreatePositionValidator : AbstractValidator<CreatePositionDto>
{
    public CreatePositionValidator()
    {
        RuleFor(p => p.Name)
            .MustBeValueObject(PositionName.Create);

        RuleFor(p => p.Description)
            .MustBeValueObject(Description.Create);

        RuleFor(p => p.DepartmentIds)
            .Must(departmentsIds => departmentsIds.Distinct().Count() == departmentsIds.Length)
            .WithError(GeneralErrors.ValueIsInvalid("departmentsIds"))
            .NotEmpty()
            .WithError(GeneralErrors.ValueIsRequired("departmentsIds"));
    }
}