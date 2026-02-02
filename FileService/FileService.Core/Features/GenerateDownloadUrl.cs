using Core.Abstractions;
using Core.Validation;
using CSharpFunctionalExtensions;
using FileService.Core.FilesStorage;
using FluentValidation;
using Framework.EndpointResults;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Shared.SharedKernel;

namespace FileService.Core.Features;

public class GenerateDownloadUrlEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("files/{fileId:guid}/download-url", async (
            Guid fileId,
            [FromServices] DownloadUrlHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(new DownloadUrlRequest(fileId), cancellationToken);

            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });
    }
}

public sealed record DownloadUrlRequest(Guid FileId) : ICommand;

public sealed class DownloadUrlValidator : AbstractValidator<DownloadUrlRequest>
{
    public DownloadUrlValidator()
    {
        RuleFor(x => x.FileId)
            .NotEmpty()
            .WithError(GeneralErrors.ValueIsInvalid("FileId"));
    }
}

public sealed class DownloadUrlHandler : ICommandHandler<string, DownloadUrlRequest>
{
    private readonly IS3Provider _s3Provider;
    private readonly IMediaRepository _mediaRepository;
    private readonly IValidator<DownloadUrlRequest> _validator;
    private readonly ILogger<DownloadUrlHandler> _logger;

    public DownloadUrlHandler(
        IS3Provider s3Provider,
        IMediaRepository mediaRepository,
        IValidator<DownloadUrlRequest> validator,
        ILogger<DownloadUrlHandler> logger)
    {
        _s3Provider = s3Provider;
        _mediaRepository = mediaRepository;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<string, Errors>> Handle(DownloadUrlRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
            return validationResult.ToList();

        var mediaAssetResult = await _mediaRepository.GetBy(x => x.Id == request.FileId, cancellationToken);

        if (mediaAssetResult.IsFailure)
            return mediaAssetResult.Error.ToErrors();

        var key = mediaAssetResult.Value.RawKey == null || mediaAssetResult.Value.RawKey.IsEmpty()
            ? mediaAssetResult.Value.FinalKey
            : mediaAssetResult.Value.RawKey;

        var downloadUrlResult = await _s3Provider.GenerateDownloadUrlAsync(key);

        if (downloadUrlResult.IsFailure)
            return downloadUrlResult.Error.ToErrors();

        _logger.LogInformation("Download url generated");

        return downloadUrlResult.Value;
    }
}