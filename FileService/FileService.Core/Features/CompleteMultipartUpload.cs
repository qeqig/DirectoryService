using Core.Abstractions;
using Core.Validation;
using CSharpFunctionalExtensions;
using FileService.Contracts;
using FileService.Core.FilesStorage;
using FileService.Domain;
using FluentValidation;
using Framework.EndpointResults;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Shared.SharedKernel;

namespace FileService.Core.Features;

public sealed class CompleteMultipartUploadEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("files/multipart/complete", async (
            [FromBody] CompleteMultipartUploadRequest request,
            [FromServices] CompleteMultipartUploadHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(request, cancellationToken);

            return result.IsSuccess ? Results.Ok() : Results.BadRequest(result.Error);
        });
    }
}



public sealed class CompleteMultipartUploadValidator : AbstractValidator<CompleteMultipartUploadRequest>
{
    public CompleteMultipartUploadValidator()
    {
        RuleFor(x => x.MediaAssetId)
            .NotEmpty()
            .WithError(GeneralErrors.ValueIsInvalid("mediaAssetId"));

        RuleFor(x => x.UploadId)
            .NotEmpty()
            .WithError(GeneralErrors.ValueIsInvalid("uploadId"));

        RuleFor(x => x.PartETags)
            .NotEmpty()
            .WithError(GeneralErrors.ValueIsInvalid("partETags"));
    }
}


public sealed class CompleteMultipartUploadHandler : ICommandHandler<CompleteMultipartUploadRequest>
{
    private readonly IS3Provider _s3Provider;
    private readonly IMediaRepository _mediaRepository;
    private readonly IValidator<CompleteMultipartUploadRequest> _validator;
    private readonly ILogger<CompleteMultipartUploadHandler> _logger;

    public CompleteMultipartUploadHandler(
        IS3Provider s3Provider,
        IMediaRepository mediaRepository,
        IValidator<CompleteMultipartUploadRequest> validator,
        ILogger<CompleteMultipartUploadHandler> logger)
    {
        _s3Provider = s3Provider;
        _mediaRepository = mediaRepository;
        _validator = validator;
        _logger = logger;
    }

    public async Task<UnitResult<Errors>> Handle(CompleteMultipartUploadRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
            return validationResult.ToList();

        (_, bool isFailure, MediaAsset? mediaAsset, Error? error) = await _mediaRepository.GetBy(m => m.Id == request.MediaAssetId,  cancellationToken);

        if (isFailure)
            return error.ToErrors();

        if (mediaAsset.MediaData.ExpectedChunksCount != request.PartETags.Count)
            return GeneralErrors.Failure().ToErrors();

        var storageKey = mediaAsset.RawKey ?? mediaAsset.FinalKey;

        var completeUploadResult = await _s3Provider.CompleteMultipartUploadAsync(
            storageKey,
            request.UploadId,
            request.PartETags,
            cancellationToken);

        if (completeUploadResult.IsFailure)
        {
            mediaAsset.MarkFailed(DateTime.UtcNow);
            await _mediaRepository.SaveChangesAsync(cancellationToken);
            return completeUploadResult.Error.ToErrors();
        }

        var markResult = mediaAsset.MarkUploaded(DateTime.UtcNow);

        if (markResult.IsFailure)
            return markResult.Error.ToErrors();

        var saveResult = await _mediaRepository.SaveChangesAsync(cancellationToken);

        if (saveResult.IsFailure)
            return saveResult.Error.ToErrors();

        _logger.LogInformation("CompleteMultipartUpload completed");

        return UnitResult.Success<Errors>();
    }
}