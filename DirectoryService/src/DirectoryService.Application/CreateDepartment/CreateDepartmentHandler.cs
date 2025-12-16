using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.IRepositories;
using DirectoryService.Contracts.Department;
using DirectoryService.Contracts.Department.CreateDepartment;
using DirectoryService.Domain.Department;
using DirectoryService.Domain.Department.VO;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Location.VO;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Shared;

namespace DirectoryService.Application.CreateDepartment;

public class CreateDepartmentHandler : ICommandHandler<Guid, CreateDepartmentCommand>
{
    private readonly ILocationsRepository _locationsRepository;

    private readonly IDepartmentsRepository _departmentsRepository;

    private readonly IValidator<CreateDepartmentDto> _validator;

    private readonly ILogger<CreateDepartmentHandler> _logger;

    public CreateDepartmentHandler(
        ILocationsRepository locationsRepository,
        IDepartmentsRepository departmentsRepository,
        IValidator<CreateDepartmentDto> validator,
        ILogger<CreateDepartmentHandler> logger)
    {
        _locationsRepository = locationsRepository;
        _departmentsRepository = departmentsRepository;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<Guid, Errors>> Handle(CreateDepartmentCommand command, CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(command.CreateDepartmentDto, cancellationToken);

        if (!validationResult.IsValid)
        {
            return GeneralErrors.ValueIsInvalid("department").ToErrors();
        }

        var departmentId = DepartmentId.Create();

        var name = DepartmentName.Create(command.CreateDepartmentDto.Name).Value;

        var identifier = Identifier.Create(command.CreateDepartmentDto.Identifier).Value;

        var parentId = command.CreateDepartmentDto.ParentId;

        var chekExistingResult = await _locationsRepository.CheckExisting(command.CreateDepartmentDto.LocationsId, cancellationToken);

        if (chekExistingResult.IsFailure)
            return chekExistingResult.Error;

        var locationIds = command.CreateDepartmentDto.LocationsId.Select(l => DepartmentLocation.Create(DepartmentLocationId.Create(), departmentId, LocationId.Current(l)));

        Department department;

        if (parentId == null)
        {
            var createParentResult = Department.CreateParent(name, identifier, locationIds);

            if (createParentResult.IsFailure)
                return createParentResult.Error.ToErrors();

            department = createParentResult.Value;
        }
        else
        {
            var parentDepartmentId = DepartmentId.Current(parentId.Value);
            var getParentResult = await _departmentsRepository.GetById(parentDepartmentId, cancellationToken);

            if (getParentResult.IsFailure)
                return getParentResult.Error.ToErrors();

            var childDepartmentResult = Department.CreateChild(name, identifier, getParentResult.Value, locationIds, departmentId);

            if (childDepartmentResult.IsFailure)
                return childDepartmentResult.Error.ToErrors();

            department = childDepartmentResult.Value;
        }

        var repositoryResult = await _departmentsRepository.AddAsync(department, cancellationToken);

        if (repositoryResult.IsFailure)
            return Error.Failure(null, repositoryResult.Error.Message).ToErrors();

        _logger.LogInformation("Created department {departmentId}", departmentId.Value);

        return departmentId.Value;
    }
}