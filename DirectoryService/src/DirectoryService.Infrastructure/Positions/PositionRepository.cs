using CSharpFunctionalExtensions;
using DirectoryService.Application.IRepositories;
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
}