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

public sealed class GetChunkUploadUrlEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("files/multipart/url", async (
            [FromBody] GetChunkUploadUrlRequest request,
            [FromServices] GetChunkUploadUrlHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(request, cancellationToken);

            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });
    }
}

public sealed class GetChunkUploadUrlValidator : AbstractValidator<GetChunkUploadUrlRequest>
{
    public GetChunkUploadUrlValidator()
    {
        RuleFor(x => x.MediaAssetId)
            .NotEmpty()
            .WithError(GeneralErrors.ValueIsInvalid("media asset id"));

        RuleFor(x => x.UploadId)
            .NotEmpty()
            .WithError(GeneralErrors.ValueIsInvalid("upload id"));

        RuleFor(x => x.PartNumber)
            .Must(n => n > 0)
            .WithError(GeneralErrors.ValueIsInvalid("part number"));
    }
}

public sealed class GetChunkUploadUrlHandler
{
    private readonly IMediaRepository _mediaRepository;
    private readonly IS3Provider _s3Provider;
    private readonly IValidator<GetChunkUploadUrlRequest> _validator;
    private readonly ILogger<GetChunkUploadUrlHandler> _logger;

    public GetChunkUploadUrlHandler(
        IMediaRepository mediaRepository,
        IS3Provider s3Provider,
        IValidator<GetChunkUploadUrlRequest> validator,
        ILogger<GetChunkUploadUrlHandler> logger)
    {
        _mediaRepository = mediaRepository;
        _s3Provider = s3Provider;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<GetChunkUploadUrlResponse, Errors>> Handle(GetChunkUploadUrlRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request,  cancellationToken);

        if (!validationResult.IsValid)
            return validationResult.ToList();

        var mediaAssetResult = await _mediaRepository.GetBy(x => x.Id == request.MediaAssetId, cancellationToken);

        if (mediaAssetResult.IsFailure)
            return mediaAssetResult.Error.ToErrors();

        var storageKey = mediaAssetResult.Value.RawKey ?? mediaAssetResult.Value.FinalKey;

        var chunkUploadResult = await _s3Provider.GenerateChunkUploadUrl(
            storageKey,
            request.UploadId,
            request.PartNumber,
            cancellationToken);

        if (chunkUploadResult.IsFailure)
            return chunkUploadResult.Error.ToErrors();

        _logger.LogInformation("Chunk upload url generated");

        return new GetChunkUploadUrlResponse(chunkUploadResult.Value, request.PartNumber);
    }
}