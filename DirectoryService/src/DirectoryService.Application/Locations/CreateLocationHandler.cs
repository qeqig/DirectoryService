using CSharpFunctionalExtensions;
using DirectoryService.Contracts.Location;
using DirectoryService.Domain.Location;
using DirectoryService.Domain.Location.VO;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Shared;

namespace DirectoryService.Application.Locations;

public class CreateLocationHandler
{
    private readonly ILocationsRepository _locationsRepository;

    private readonly IValidator<CreateLocationDTO> _validator;

    private readonly ILogger<CreateLocationHandler> _logger;

    public CreateLocationHandler(
        ILocationsRepository locationsRepository,
        ILogger<CreateLocationHandler> logger,
        IValidator<CreateLocationDTO> validator)
    {
        _locationsRepository = locationsRepository;
        _logger = logger;
        _validator = validator;
    }

    public async Task<Result<Guid, Errors>> Handle(CreateLocationDTO dto, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(dto,  cancellationToken);

        if (!validationResult.IsValid)
        {
            return GeneralErrors.ValueIsInvalid("loccation").ToErrors();
        }

        var locName = LocationName.Create(dto.Name).Value;

        var address = LocationAddress.Create(
            dto.Address.Country,
            dto.Address.City,
            dto.Address.Street,
            dto.Address.HouseNumber).Value;

        var timeZone = LocationTimezone.Create(dto.TimeZone).Value;

        var location = Location.Create(locName, timeZone, address);

        await _locationsRepository.AddAsync(location.Value, cancellationToken);

        _logger.LogInformation("Created location with id {location.Value.Id}", location.Value.Id);

        return location.Value.Id.Value;
    }
}