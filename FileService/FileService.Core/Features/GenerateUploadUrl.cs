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

public sealed class GenerateUploadUrlEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("files/{fileId:guid}/upload-url", async (
            Guid fileId,
            [FromServices] GenerateUploadUrlHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(new GenerateUploadUrlRequest(fileId), cancellationToken);

            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });
    }
}

public sealed record GenerateUploadUrlRequest(Guid FileId) : ICommand;

public sealed class GenerateUploadUrlValidator : AbstractValidator<GenerateUploadUrlRequest>
{
    public GenerateUploadUrlValidator()
    {
        RuleFor(x => x.FileId)
            .NotEmpty()
            .WithError(GeneralErrors.ValueIsInvalid("FileId"));
    }
}

public sealed class GenerateUploadUrlHandler : ICommandHandler<string, GenerateUploadUrlRequest>
{
    private readonly IS3Provider _s3Provider;
    private readonly IMediaRepository _mediaRepository;
    private readonly IValidator<GenerateUploadUrlRequest> _validator;
    private readonly ILogger<GenerateUploadUrlHandler> _logger;

    public GenerateUploadUrlHandler(
        IS3Provider s3Provider,
        IMediaRepository mediaRepository,
        IValidator<GenerateUploadUrlRequest> validator,
        ILogger<GenerateUploadUrlHandler> logger)
    {
        _s3Provider = s3Provider;
        _mediaRepository = mediaRepository;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<string, Errors>> Handle(GenerateUploadUrlRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
            return validationResult.ToList();

        var mediaAssetResult = await _mediaRepository.GetBy(x => x.Id == request.FileId,  cancellationToken);

        if (mediaAssetResult.IsFailure)
            return mediaAssetResult.Error.ToErrors();

        var key = mediaAssetResult.Value.RawKey == null || mediaAssetResult.Value.RawKey.IsEmpty()
            ? mediaAssetResult.Value.FinalKey
            : mediaAssetResult.Value.RawKey;

        var uploadUrlResult = await _s3Provider.GenerateUploadUrlAsync(key, mediaAssetResult.Value.MediaData, cancellationToken);

        if (uploadUrlResult.IsFailure)
            return uploadUrlResult.Error.ToErrors();

        _logger.LogInformation("Generated upload url: {UploadUrl}", request.FileId);

        return uploadUrlResult.Value;
    }
}