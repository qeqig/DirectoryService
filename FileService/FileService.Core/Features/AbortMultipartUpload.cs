using Core.Abstractions;
using Core.Validation;
using CSharpFunctionalExtensions;
using FileService.Contracts;
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

public sealed class AbortMultipartUploadEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("files/multipart/abort", async (
            [FromBody] AbortMultipartUploadRequest request,
            [FromServices] AbortMultipartUploadHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(request, cancellationToken);

            return result.IsSuccess ? Results.Ok() : Results.BadRequest(result.Error);
        });
    }
}

public sealed class AbortMultipartUploadRequestValidator : AbstractValidator<AbortMultipartUploadRequest>
{
    public AbortMultipartUploadRequestValidator()
    {
        RuleFor(x => x.MediaAssetId)
            .NotEmpty()
            .WithError(GeneralErrors.ValueIsInvalid());

        RuleFor(x => x.UploadId)
            .NotEmpty()
            .WithError(GeneralErrors.ValueIsInvalid());
    }
}

public sealed class AbortMultipartUploadHandler : ICommandHandler<AbortMultipartUploadRequest>
{
    private readonly IMediaRepository _mediaRepository;
    private readonly IS3Provider _s3Provider;
    private readonly IValidator<AbortMultipartUploadRequest> _validator;
    private readonly ILogger<AbortMultipartUploadHandler> _logger;

    public AbortMultipartUploadHandler(
        IMediaRepository mediaRepository,
        IS3Provider s3Provider,
        IValidator<AbortMultipartUploadRequest> validator,
        ILogger<AbortMultipartUploadHandler> logger)
    {
        _mediaRepository = mediaRepository;
        _s3Provider = s3Provider;
        _validator = validator;
        _logger = logger;
    }

    public async Task<UnitResult<Errors>> Handle(AbortMultipartUploadRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
            return validationResult.ToList();

        var mediaAssetResult = await _mediaRepository.GetBy(x => x.Id == request.MediaAssetId, cancellationToken);

        if (mediaAssetResult.IsFailure)
            return mediaAssetResult.Error.ToErrors();

        var storageKey = mediaAssetResult.Value.RawKey == null || mediaAssetResult.Value.RawKey.IsEmpty()
            ? mediaAssetResult.Value.FinalKey
            : mediaAssetResult.Value.RawKey;

        var abortMultipartResult = await _s3Provider.AbortMultipartUploadAsync(
            storageKey,
            request.UploadId,
            cancellationToken);

        if (abortMultipartResult.IsFailure)
            return abortMultipartResult.Error.ToErrors();

        var markResult = mediaAssetResult.Value.MarkFailed(DateTime.UtcNow);

        if (markResult.IsFailure)
            return markResult.Error.ToErrors();

        var saveResult = await _mediaRepository.SaveChangesAsync(cancellationToken);

        if (saveResult.IsFailure)
            return saveResult.Error.ToErrors();

        _logger.LogInformation("Successfully aborted upload");

        return UnitResult.Success<Errors>();
    }
}