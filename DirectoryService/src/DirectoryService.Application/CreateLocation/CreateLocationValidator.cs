using DirectoryService.Application.Validation;
using DirectoryService.Contracts.Location;
using DirectoryService.Domain.Location.VO;
using FluentValidation;

namespace DirectoryService.Application.CreateLocation;

public class CreateLocationValidator : AbstractValidator<CreateLocationDTO>
{
    public CreateLocationValidator()
    {
        RuleFor(l => l.Name)
            .MustBeValueObject(LocationName.Create);

        RuleFor(l => l.Address)
            .MustBeValueObject(x =>
                LocationAddress.Create(x.Country, x.City, x.Street, x.HouseNumber));

        RuleFor(l => l.TimeZone)
            .MustBeValueObject(LocationTimezone.Create);
    }
}