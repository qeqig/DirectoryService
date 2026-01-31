using Core.Abstractions;
using Core.Validation;
using CSharpFunctionalExtensions;
using FluentValidation;
using Framework.EndpointResults;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Shared.SharedKernel;

namespace FileService.Core.Features;

public sealed class DownloadFileEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("files/{fileId:guid}", async (
            Guid fileId,
            [FromQuery] string path,
            [FromServices] DownloadFileHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(new DownloadFileRequest(fileId, path), cancellationToken);

            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });
    }
}

public sealed record DownloadFileRequest(Guid FileId, string Path) : ICommand;

public sealed class DowloadFileValidator : AbstractValidator<DownloadFileRequest>
{
    public DowloadFileValidator()
    {
        RuleFor(x => x.FileId)
            .NotEmpty()
            .WithError(GeneralErrors.ValueIsInvalid("FileId"));

        RuleFor(x => x.Path)
            .NotEmpty()
            .WithError(GeneralErrors.ValueIsInvalid("Path"));
    }
}

public sealed class DownloadFileHandler : ICommandHandler<string, DownloadFileRequest>
{
    private readonly IS3Provider _s3Provider;
    private readonly IMediaRepository _mediaRepository;
    private readonly IValidator<DownloadFileRequest> _validator;
    private readonly ILogger<DownloadFileHandler> _logger;

    public DownloadFileHandler(
        IS3Provider s3Provider,
        IMediaRepository mediaRepository,
        IValidator<DownloadFileRequest> validator,
        ILogger<DownloadFileHandler> logger)
    {
        _s3Provider = s3Provider;
        _mediaRepository = mediaRepository;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<string, Errors>> Handle(DownloadFileRequest command, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command);

        if (!validationResult.IsValid)
            return validationResult.ToList();

        var mediaAssetResult = await _mediaRepository.GetBy(x => x.Id == command.FileId, cancellationToken);

        if (mediaAssetResult.IsFailure)
            return mediaAssetResult.Error.ToErrors();

        var key = mediaAssetResult.Value.RawKey == null || mediaAssetResult.Value.RawKey.IsEmpty()
            ? mediaAssetResult.Value.FinalKey
            : mediaAssetResult.Value.RawKey;

        var downloadFileResult = await _s3Provider.DownloadFileAsync(key, command.Path, cancellationToken);

        if (downloadFileResult.IsFailure)
            return downloadFileResult.Error.ToErrors();

        _logger.LogInformation("Downloaded file {fileId}.", command.FileId);

        return downloadFileResult.Value;
    }
}