using CSharpFunctionalExtensions;
using FileService.Core;
using FileService.Domain;
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
}