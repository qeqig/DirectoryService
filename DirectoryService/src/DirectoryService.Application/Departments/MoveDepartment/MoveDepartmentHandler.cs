using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Application.IRepositories;
using DirectoryService.Domain;
using DirectoryService.Domain.Department;
using DirectoryService.Domain.Department.VO;
using FluentValidation;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using Shared;

namespace DirectoryService.Application.Departments.MoveDepartment;

public class MoveDepartmentHandler : ICommandHandler<Guid, MoveDepartmentCommand>
{
    private readonly ITransactionManager _transactionManager;

    private readonly IDepartmentsRepository _departmentsRepository;

    private readonly IValidator<MoveDepartmentCommand> _validator;

    private readonly ILogger<MoveDepartmentHandler> _logger;

    private readonly HybridCache _cache;

    public MoveDepartmentHandler(
        ITransactionManager transactionManager,
        IDepartmentsRepository departmentsRepository,
        IValidator<MoveDepartmentCommand> validator,
        ILogger<MoveDepartmentHandler> logger,
        HybridCache cache)
    {
        _transactionManager = transactionManager;
        _departmentsRepository = departmentsRepository;
        _validator = validator;
        _logger = logger;
        _cache = cache;
    }

    public async Task<Result<Guid, Errors>> Handle(MoveDepartmentCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
        {
            return GeneralErrors.ValueIsInvalid("department").ToErrors();
        }

        var depId = command.DepartmentId;

        var departmentId = DepartmentId.Current(depId);

        var newParentId = command.Dto.ParentId;

        Department? newParentDepartment = null;

        if (newParentId.HasValue && newParentId.Value == depId)
        {
            return GeneralErrors.ValueIsInvalid("department").ToErrors();
        }

        var transactionScopeResult = await _transactionManager.BeginTransactionAsync(cancellationToken: cancellationToken);

        if (transactionScopeResult.IsFailure)
        {
            return transactionScopeResult.Error.ToErrors();
        }

        using var transactionScope = transactionScopeResult.Value;

        if (newParentId.HasValue)
        {
            var newParentDepId = DepartmentId.Current(newParentId.Value);

            var hasChildDepartments =
                await _departmentsRepository.CheckExistChildForParent(departmentId, newParentDepId, cancellationToken);

            if (hasChildDepartments)
            {
                transactionScope.Rollback();
                return GeneralErrors.ValueIsInvalid("department").ToErrors();
            }

            newParentDepartment =
                await _departmentsRepository.GetBy(d => d.IsActive && d.Id == newParentDepId, cancellationToken);

            if (newParentDepartment == null)
            {
                transactionScope.Rollback();
                return GeneralErrors.NotFound(newParentId).ToErrors();
            }
        }

        var departmentResult = await _departmentsRepository.GetByIdWithLock(departmentId, cancellationToken);

        if (departmentResult.IsFailure)
        {
            transactionScope.Rollback();
            return departmentResult.Error.ToErrors();
        }

        var department = departmentResult.Value;

        var oldDepartmentPath = department.Path;

        var childDepartments = await _departmentsRepository.GetChildWithLock(department.Path, cancellationToken);

        department.MoveDepartment(newParentDepartment);

        var updateChildResult = await _departmentsRepository.UpdateChildren(oldDepartmentPath, department, cancellationToken);

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
            _logger.LogError(ex, ex.Message);
            return GeneralErrors.ValueIsInvalid("department").ToErrors();
        }

        var commitedResult = transactionScope.Commit();

        if (commitedResult.IsFailure)
                return commitedResult.Error.ToErrors();

        await _cache.RemoveByTagAsync(Constants.DEPARTMENT_CACHE_KEY, cancellationToken);

        _logger.LogInformation("department moved");

        return depId;
        }
    }
