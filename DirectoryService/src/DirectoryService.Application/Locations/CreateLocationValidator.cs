using DirectoryService.Contracts.Location;
using DirectoryService.Domain;
using FluentValidation;

namespace DirectoryService.Application.Locations;

public class CreateLocationValidator : AbstractValidator<CreateLocationDTO>
{
    public CreateLocationValidator()
    {
        RuleFor(l => l.Name)
            .NotEmpty()
            .NotNull()
            .MaximumLength(LengthConstant.LENGTH_500);

        RuleFor(l => l.Address.Country)
            .NotEmpty()
            .NotNull()
            .MaximumLength(LengthConstant.LENGTH_500);

        RuleFor(l => l.Address.City)
            .NotEmpty()
            .NotNull()
            .MaximumLength(LengthConstant.LENGTH_500);

        RuleFor(l => l.Address.Street)
            .NotEmpty()
            .NotNull()
            .MaximumLength(LengthConstant.LENGTH_500);

        RuleFor(l => l.Address.HouseNumber)
            .NotEmpty()
            .NotNull()
            .MaximumLength(LengthConstant.LENGTH_500);

        RuleFor(l => l.TimeZone)
            .NotEmpty()
            .NotNull()
            .Must(tz => TimeZoneInfo.TryFindSystemTimeZoneById(tz, out _))
            .MaximumLength(LengthConstant.LENGTH_500);
    }
}