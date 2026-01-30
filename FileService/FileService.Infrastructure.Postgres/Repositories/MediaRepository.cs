using System.Linq.Expressions;
using CSharpFunctionalExtensions;
using FileService.Core;
using FileService.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.SharedKernel;

namespace FileService.Infrastructure.Postgres.Repositories;

public class MediaRepository : IMediaRepository
{
    private readonly FileServiceDbContext _dbContext;
    private readonly ILogger<MediaRepository> _logger;

    public MediaRepository(FileServiceDbContext context, ILogger<MediaRepository> logger)
    {
        _dbContext = context;
        _logger = logger;
    }

    public async Task<Result<Guid, Error>> AddAsync(MediaAsset mediaAsset, CancellationToken cancellationToken = default)
    {
        try
        {
            await _dbContext.MediaAssets.AddAsync(mediaAsset, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return mediaAsset.Id;

        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            return GeneralErrors.ValueIsInvalid();
        }
    }

    public async Task<Result<MediaAsset, Error>> GetBy(
        Expression<Func<MediaAsset, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        var record = await _dbContext.MediaAssets.FirstOrDefaultAsync(predicate, cancellationToken);

        if (record is null)
            return GeneralErrors.NotFound(null, "media");

        return record;
    }

    public async Task<Result<IReadOnlyList<MediaAsset>, Error>> GetManyBy(
        Expression<Func<MediaAsset, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        var records = await _dbContext.MediaAssets.Where(predicate).ToListAsync(cancellationToken);

        if (records.Count == 0)
            return GeneralErrors.NotFound(null, "media");

        return records;
    }

    public async Task<UnitResult<Error>> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
            return UnitResult.Success<Error>();

        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            return UnitResult.Failure(Error.Failure("save.change.error", "Error saving media asset."));
        }
    }
}