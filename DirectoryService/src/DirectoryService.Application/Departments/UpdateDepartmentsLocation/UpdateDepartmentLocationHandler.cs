using Core.Abstractions;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using DirectoryService.Application.IRepositories;
using DirectoryService.Contracts.Department.UpdateDepartments;
using DirectoryService.Domain;
using DirectoryService.Domain.Department.VO;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Location.VO;
using FluentValidation;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using Shared.SharedKernel;

namespace DirectoryService.Application.Departments.UpdateDepartmentsLocation;

public class UpdateDepartmentLocationHandler : ICommandHandler<Guid, UpdateDepartmentLocationCommand>
{
    private readonly IDepartmentsRepository _departmentsRepository;
    private readonly ILocationsRepository _locationsRepository;
    private readonly IValidator<UpdateDepartmentLocationDto> _validator;
    private readonly ITransactionManager _transactionManager;
    private readonly ILogger<UpdateDepartmentLocationHandler> _logger;
    private readonly HybridCache _cache;

    public UpdateDepartmentLocationHandler(
        IDepartmentsRepository departmentsRepository,
        ILocationsRepository locationsRepository,
        IValidator<UpdateDepartmentLocationDto> validator,
        ITransactionManager transactionManager,
        ILogger<UpdateDepartmentLocationHandler> logger,
        HybridCache cache)
    {
        _departmentsRepository = departmentsRepository;
        _locationsRepository = locationsRepository;
        _validator = validator;
        _transactionManager = transactionManager;
        _logger = logger;
        _cache = cache;
    }

    public async Task<Result<Guid, Errors>> Handle(UpdateDepartmentLocationCommand command, CancellationToken cancellationToken = default)
    {
         var validationResult = await _validator.ValidateAsync(command.Dto, cancellationToken);

         if (!validationResult.IsValid)
             return GeneralErrors.ValueIsInvalid("UpdateDepartmentLocation").ToErrors();

         var transactionResult = await _transactionManager.BeginTransactionAsync(cancellationToken: cancellationToken);

         if (transactionResult.IsFailure)
             return transactionResult.Error.ToErrors();

         using var transaction = transactionResult.Value;

         var departmentId = DepartmentId.Current(command.DepartmentId);

         var department = await _departmentsRepository.GetBy(d => d.Id == departmentId && d.IsActive, cancellationToken);

         if (department == null)
         {
             transaction.Rollback();
             return GeneralErrors.NotFound(departmentId.Value, "department").ToErrors();
         }

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

         await _cache.RemoveByTagAsync(Constants.DEPARTMENT_CACHE_KEY, cancellationToken);

         _logger.LogInformation("Department {departmentId} location updated successfully", department.Id);

         return department.Id.Value;
    }
}