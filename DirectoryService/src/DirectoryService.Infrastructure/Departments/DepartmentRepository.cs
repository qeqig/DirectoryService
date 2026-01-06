using System.Linq.Expressions;
using CSharpFunctionalExtensions;
using DirectoryService.Application.IRepositories;
using DirectoryService.Domain.Department;
using DirectoryService.Domain.Department.VO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared;
using Path = DirectoryService.Domain.Department.VO.Path;

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

    public async Task<bool> CheckExistChildForParent(DepartmentId departmentId, DepartmentId parentId, CancellationToken cancellationToken = default) =>
        await _dbContext.Departments.AnyAsync(
            d => d.Id == departmentId
                 && d.ChildDepartments.Any(c => c.Id == parentId), cancellationToken);

    public async Task<Department?> GetBy(Expression<Func<Department, bool>> predicate, CancellationToken cancellationToken = default) =>
        await _dbContext.Departments.FirstOrDefaultAsync(predicate, cancellationToken);

    public async Task<Result<Department, Error>> GetByIdWithLock(DepartmentId departmentId, CancellationToken cancellationToken = default)
    {
        var id = departmentId.Value;

        var department = await _dbContext.Departments.FromSql(
            $"""
             SELECT 
                id,
                parent_id,
                depth,
                is_active,
                created_at,
                updated_at,
                deleted_at,
                name,
                identifier,
                path
             FROM departments WHERE id = {id} AND is_active FOR UPDATE
             """)
            .FirstOrDefaultAsync(cancellationToken);

        if (department == null)
            return GeneralErrors.NotFound(departmentId.Value, "department");

        return department;
    }

    public async Task<IReadOnlyList<Department>> GetChildWithLock(Path path, CancellationToken cancellationToken = default)
    {
        var parentPath = path.Value;

        var child = await _dbContext.Departments.FromSql(
            $"""
             SELECT 
                id,
                parent_id,
                depth,
                is_active,
                created_at,
                updated_at,
                deleted_at,
                name,
                identifier,
                path
              FROM departments WHERE path <@ {parentPath}::ltree FOR UPDATE
             """).ToListAsync(cancellationToken);

        return child;
    }

    public async Task<UnitResult<Error>> UpdateChildren(Path path, Department department,
        CancellationToken cancellationToken = default)
    {
        var oldPath = path.Value;

        var newPath = department.Path.Value;

        var newDepth = department.Depth;

        try
        {
            await _dbContext.Database.ExecuteSqlAsync(
                $"""
                 UPDATE departments 
                 SET depth = {newDepth} + nlevel(subpath(path, nlevel({oldPath}::ltree), nlevel(path::ltree))),
                 path = ({newPath}::ltree || subpath(path, nlevel({oldPath}::ltree), nlevel(path::ltree)))
                 WHERE path <@ {oldPath}::ltree
                 AND path != {oldPath}::ltree
                 """, cancellationToken);

            return UnitResult.Success<Error>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating department");
            return GeneralErrors.DataBase();
        }
    }

    public async Task<UnitResult<Error>> UpdateChildAndPath(Path oldDepartmentPath, Path departmentPath, DepartmentId departmentId, CancellationToken cancellationToken = default)
    {
        var oldPath = oldDepartmentPath.Value;

        var newPath = departmentPath.Value;

        var depId = departmentId.Value;

        try
        {
            await _dbContext.Database.ExecuteSqlAsync($"""
                                                       UPDATE departments
                                                       SET path = ({newPath}::ltree || subpath(path, nlevel({oldPath}::ltree), nlevel(path::ltree))),
                                                           updated_at = now()
                                                       WHERE path <@ {oldPath}::ltree
                                                       AND path != {oldPath}::ltree
                                                       """);
            return UnitResult.Success<Error>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating department");
            return GeneralErrors.DataBase();
        }
    }
}