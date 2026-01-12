using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Application.IRepositories;
using DirectoryService.Domain;
using DirectoryService.Domain.Department.VO;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using Shared;

namespace DirectoryService.Application.Departments.DeleteDepartment;

public class DeleteDepartmentHandler : ICommandHandler<Guid, DeleteDepartmentCommand>
{
    private readonly IDepartmentsRepository _departmentsRepository;

    private readonly ILocationsRepository _locationsRepository;

    private readonly IPositionsRepository _positionsRepository;

    private readonly ITransactionManager _transactionManager;

    private readonly ILogger<DeleteDepartmentHandler> _logger;

    private readonly HybridCache _cache;

    public DeleteDepartmentHandler(
        IDepartmentsRepository departmentsRepository,
        ILocationsRepository locationsRepository,
        IPositionsRepository positionsRepository,
        ITransactionManager transactionManager,
        ILogger<DeleteDepartmentHandler> logger,
        HybridCache cache)
    {
        _departmentsRepository = departmentsRepository;
        _locationsRepository = locationsRepository;
        _positionsRepository = positionsRepository;
        _transactionManager = transactionManager;
        _logger = logger;
        _cache = cache;
    }

    public async Task<Result<Guid, Errors>> Handle(DeleteDepartmentCommand command, CancellationToken cancellationToken = default)
    {
        var departmentId = DepartmentId.Current(command.Id);

        var transactionScopeResult = await _transactionManager.BeginTransactionAsync();

        if (transactionScopeResult.IsFailure)
            return transactionScopeResult.Error.ToErrors();

        using var transactionScope = transactionScopeResult.Value;

        var departmentResult = await _departmentsRepository.GetByIdWithLock(departmentId, cancellationToken);

        if (departmentResult.IsFailure)
        {
            transactionScope.Rollback();
            return departmentResult.Error.ToErrors();
        }

        var department = departmentResult.Value;

        var updateLocationResult = await _locationsRepository.DeactivateLocations(departmentId, cancellationToken);

        if (updateLocationResult.IsFailure)
        {
            transactionScope.Rollback();
            return updateLocationResult.Error;
        }

        var updatePositionResult = await _positionsRepository.DeactivatePosition(departmentId, cancellationToken);

        if (updatePositionResult.IsFailure)
        {
            transactionScope.Rollback();
            return updatePositionResult.Error;
        }

        var oldPath = department.Path;

        department.SoftDelete();

        var newPath = department.Path;

        await _departmentsRepository.GetChildWithLock(department.Path, cancellationToken);

        var updateChildResult = await _departmentsRepository.UpdateChildAndPath(oldPath, newPath, departmentId, cancellationToken);

        if (updateChildResult.IsFailure)
        {
            transactionScope.Rollback();
            return updateChildResult.Error.ToErrors();
        }

        try
        {
            await _transactionManager.SaveChangesAsync(cancellationToken);

        }
        catch (Exception ex)
        {
            transactionScope.Rollback();
            _logger.LogError(ex, "Error updating department");
            return GeneralErrors.DataBase().ToErrors();
        }

        var commitResult = transactionScope.Commit();
        if (commitResult.IsFailure)
            return commitResult.Error.ToErrors();

        await _cache.RemoveByTagAsync(Constants.DEPARTMENT_CACHE_KEY, cancellationToken);

        _logger.LogInformation("Department deleted");

        return department.Id.Value;
    }
}