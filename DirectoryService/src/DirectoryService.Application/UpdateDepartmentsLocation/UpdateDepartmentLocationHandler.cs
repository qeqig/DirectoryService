using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Application.IRepositories;
using DirectoryService.Contracts.Department.UpdateDepartments;
using DirectoryService.Domain.Department.VO;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Location.VO;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Shared;

namespace DirectoryService.Application.UpdateDepartmentsLocation;

public class UpdateDepartmentLocationHandler : ICommandHandler<Guid, UpdateDepartmentLocationCommand>
{
    private readonly IDepartmentsRepository _departmentsRepository;
    private readonly ILocationsRepository _locationsRepository;
    private readonly IValidator<UpdateDepartmentLocationDto> _validator;
    private readonly ITransactionManager _transactionManager;
    private readonly ILogger<UpdateDepartmentLocationHandler> _logger;

    public UpdateDepartmentLocationHandler(
        IDepartmentsRepository departmentsRepository,
        ILocationsRepository locationsRepository,
        IValidator<UpdateDepartmentLocationDto> validator,
        ITransactionManager transactionManager,
        ILogger<UpdateDepartmentLocationHandler> logger)
    {
        _departmentsRepository = departmentsRepository;
        _locationsRepository = locationsRepository;
        _validator = validator;
        _transactionManager = transactionManager;
        _logger = logger;
    }

    public async Task<Result<Guid, Errors>> Handle(UpdateDepartmentLocationCommand command, CancellationToken cancellationToken = default)
    {
         var validationResult = await _validator.ValidateAsync(command.Dto, cancellationToken);

         if (!validationResult.IsValid)
             return GeneralErrors.ValueIsInvalid("UpdateDepartmentLocation").ToErrors();

         var transactionResult = await _transactionManager.BeginTransactionAsync(cancellationToken);

         if (transactionResult.IsFailure)
             return transactionResult.Error.ToErrors();

         using var transaction = transactionResult.Value;

         var departmentId = DepartmentId.Current(command.DepartmentId);

         var getDepartmentResult = await _departmentsRepository.GetById(departmentId, cancellationToken);

         if (getDepartmentResult.IsFailure)
         {
             transaction.Rollback();
             return getDepartmentResult.Error.ToErrors();
         }

         var department = getDepartmentResult.Value;

         var checkExistingResult = await _locationsRepository.CheckExisting(command.Dto.LocationIds, cancellationToken);

         if (checkExistingResult.IsFailure)
         {
             transaction.Rollback();
             return checkExistingResult.Error;
         }

         var locationIds = command.Dto.LocationIds.Select(locationId =>
             DepartmentLocation.Create(DepartmentLocationId.Create(), departmentId, LocationId.Current(locationId)));

         department.UpdateLocationId(locationIds);

         await _departmentsRepository.DeleteLocations(departmentId, cancellationToken);

         await _transactionManager.SaveChangesAsync(cancellationToken);

         var commitedResult = transaction.Commit();

         if (commitedResult.IsFailure)
             return commitedResult.Error.ToErrors();

         _logger.LogInformation("Department {departmentId} location updated successfully", department.Id);

         return department.Id.Value;
    }
}