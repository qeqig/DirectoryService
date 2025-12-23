using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.IRepositories;
using DirectoryService.Contracts.Location;
using DirectoryService.Domain.Location;
using DirectoryService.Domain.Location.VO;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Shared;

namespace DirectoryService.Application.Locations.CreateLocation;

public class CreateLocationHandler : ICommandHandler<Guid, CreateLocationCommand>
{
    private readonly ILocationsRepository _locationsRepository;

    private readonly IValidator<CreateLocationDto> _validator;

    private readonly ILogger<CreateLocationHandler> _logger;

    public CreateLocationHandler(
        ILocationsRepository locationsRepository,
        ILogger<CreateLocationHandler> logger,
        IValidator<CreateLocationDto> validator)
    {
        _locationsRepository = locationsRepository;
        _logger = logger;
        _validator = validator;
    }

    public async Task<Result<Guid, Errors>> Handle(CreateLocationCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command.CreateLocationDto, cancellationToken);

        if (!validationResult.IsValid)
        {
            return GeneralErrors.ValueIsInvalid("location").ToErrors();
        }

        var locName = LocationName.Create(command.CreateLocationDto.Name).Value;

        var address = LocationAddress.Create(
            command.CreateLocationDto.Address.Country,
            command.CreateLocationDto.Address.City,
            command.CreateLocationDto.Address.Street,
            command.CreateLocationDto.Address.HouseNumber).Value;

        var timeZone = LocationTimezone.Create(command.CreateLocationDto.TimeZone).Value;

        var location = Location.Create(locName, timeZone, address);

        var repositoryResult = await _locationsRepository.AddAsync(location.Value, cancellationToken);

        if (repositoryResult.IsFailure)
            return Error.Failure(null, repositoryResult.Error.Message).ToErrors();

        _logger.LogInformation("Created location with id {location.Value.Id}", location.Value.Id);

        return location.Value.Id.Value;
    }
}