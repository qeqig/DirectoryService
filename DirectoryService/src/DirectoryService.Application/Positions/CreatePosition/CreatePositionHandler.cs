using Core.Abstractions;
using CSharpFunctionalExtensions;
using DirectoryService.Application.IRepositories;
using DirectoryService.Contracts.Position;
using DirectoryService.Domain.Department.VO;
using DirectoryService.Domain.DepartmentPositions;
using DirectoryService.Domain.Position;
using DirectoryService.Domain.Position.VO;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Shared.SharedKernel;

namespace DirectoryService.Application.Positions.CreatePosition;

public class CreatePositionHandler : ICommandHandler<Guid, CreatePositionCommand>
{
    private readonly IPositionsRepository _repository;

    private readonly IDepartmentsRepository _departmentRepository;

    private readonly IValidator<CreatePositionDto> _validator;

    private readonly ILogger<CreatePositionHandler> _logger;

    public CreatePositionHandler(
        IPositionsRepository repository,
        IValidator<CreatePositionDto> validator,
        ILogger<CreatePositionHandler> logger,
        IDepartmentsRepository departmentRepository)
    {
        _repository = repository;
        _validator = validator;
        _logger = logger;
        _departmentRepository = departmentRepository;
    }

    public async Task<Result<Guid, Errors>> Handle(CreatePositionCommand command, CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(command.CreatePositionDto, cancellationToken);

        if (!validationResult.IsValid)
        {
            return GeneralErrors.ValueIsInvalid("position").ToErrors();
        }

        var positionId = PositionId.Create();

        var name = PositionName.Create(command.CreatePositionDto.Name).Value;

        var description = Description.Create(command.CreatePositionDto.Description).Value;

        var chekExistingResult = await _departmentRepository.ChekExisting(command.CreatePositionDto.DepartmentIds, cancellationToken);

        if (chekExistingResult.IsFailure)
            return chekExistingResult.Error;

        var departmentsIds = command.CreatePositionDto.DepartmentIds.Select(dp =>
            DepartmentPosition.Create(DepartmentPositionId.Create(), DepartmentId.Current(dp), positionId));

        var position = Position.Create(name, description, departmentsIds);

        var repositoryResult = await _repository.AddAsync(position.Value, cancellationToken);

        if (repositoryResult.IsFailure)
            return Error.Failure(null, repositoryResult.Error.Message).ToErrors();

        _logger.LogInformation("Created position {PositionId}", position.Value.Id);

        return positionId.Value;
    }
}