using CSharpFunctionalExtensions;
using DirectoryService.Application.IRepositories;
using DirectoryService.Domain.Department.VO;
using DirectoryService.Domain.Position;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using Shared;

namespace DirectoryService.Infrastructure.Positions;

public class PositionRepository : IPositionsRepository
{
    private readonly DirectoryServiceDbContext _dbContext;
    private ILogger<PositionRepository> _logger;

    public PositionRepository(DirectoryServiceDbContext dbContext, ILogger<PositionRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Result<Guid, Error>> AddAsync(Position position, CancellationToken cancellationToken = default)
    {
        var name = position.Name.Value;
        try
        {
            await _dbContext.AddAsync(position, cancellationToken);

            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Position {PositionId} added", position.Id);

            return position.Id.Value;
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx)
        {
            if (pgEx.SqlState == PostgresErrorCodes.UniqueViolation && pgEx.ConstraintName is not null)
            {
                if (pgEx.ConstraintName.Contains("name"))
                {
                    return GeneralErrors.AlreadyExist(name);
                }
            }

            _logger.LogError(pgEx, "Position {PositionId} could not be added", position.Id);

            return GeneralErrors.DataBase();
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogError(ex, "Cancel the operation of adding a position with the name {name}", name);
            return GeneralErrors.DataBase();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while adding a position with the name {name}", name);
            return GeneralErrors.DataBase();
        }
    }

    public async Task<UnitResult<Errors>> DeactivatePosition(DepartmentId departmentId, CancellationToken cancellationToken = default)
    {
        var depId = departmentId.Value;

        try
        {
            await _dbContext.Database.ExecuteSqlAsync(
                $"""
                                                  WITH lock_positions AS (SELECT d.position_id
                                                                          FROM department_positions d
                                                                          JOIN positions p ON d.position_id = p.id
                                                                          WHERE d.department_id = {depId}
                                                                          AND p.is_active = true
                                                                          FOR UPDATE)
                                                  UPDATE positions
                                                  SET is_active = false,
                                                  updated_at = now()
                                                  WHERE id in (SELECT position_id FROM lock_positions)
                                                  """, cancellationToken);
            return Result.Success<Errors>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while deactivating a position with the name {name}", depId);
            return GeneralErrors.DataBase().ToErrors();
        }
    }
}