using CSharpFunctionalExtensions;
using DirectoryService.Application.IRepositories;
using DirectoryService.Domain.Department;
using DirectoryService.Domain.Department.VO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared;

namespace DirectoryService.Infrastructure.Departments;

public class DepartmentRepository : IDepartmentsRepository
{
    private readonly DirectoryServiceDbContext _dbContext;
    private readonly ILogger<DepartmentRepository> _logger;

    public DepartmentRepository(DirectoryServiceDbContext dbContext, ILogger<DepartmentRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Result<Guid, Error>> AddAsync(Department department, CancellationToken cancellationToken = default)
    {
        var name = department.DepartmentName.Value;
        try
        {
            await _dbContext.Departments.AddAsync(department, cancellationToken);

            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Department {Department.Id} added", department.Id);

            return department.Id.Value;
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogError(ex, "Cancel the operation of adding a position with the name {name}", name);
            return GeneralErrors.DataBase();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Operation failed with name {name}", name);
            return GeneralErrors.DataBase();
        }

    }

    public async Task<Result<Department, Error>> GetById(DepartmentId departmentId, CancellationToken cancellationToken = default)
    {
        var department = await _dbContext.Departments
            .Where(d => d.IsActive == true)
            .FirstOrDefaultAsync(d => departmentId == d.Id, cancellationToken);

        if (department == null)
            return GeneralErrors.NotFound(departmentId.Value, "department");

        return department;
    }

    public async Task<UnitResult<Errors>> ChekExisting(Guid[] ids, CancellationToken cancellationToken = default)
    {
        var departmentsIds = DepartmentId.Current(ids);

        var existingIds = await _dbContext.Departments
            .Where(d => departmentsIds.Contains(d.Id) && d.IsActive)
            .Select(d => d.Id.Value)
            .ToListAsync(cancellationToken);

        var missingIds = ids.Except(existingIds).ToList();

        var errors = missingIds.Select(x => GeneralErrors.NotFound(x, "department")).ToList();

        return errors.Count != 0
            ? UnitResult.Failure(new Errors(errors))
            : UnitResult.Success<Errors>();
    }

    public async Task<UnitResult<Error>> DeleteLocations(DepartmentId departmentId, CancellationToken cancellationToken = default)
    {
        await _dbContext.DepartmentLocations
            .Where(l => l.DepartmentId == departmentId)
            .ExecuteDeleteAsync(cancellationToken);

        return UnitResult.Success<Error>();
    }
}