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

public sealed class StartMultipartUploadEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("files/multipart/start", async (
            IFormFile file,
            [FromQuery] Guid contextId,
            [FromQuery] string context,
            [FromServices] StartMultipartUploadHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(
                new StartMultipartUploadRequest(
                file.FileName,
                file.ContentType,
                file.ContentType,
                file.Length,
                context,
                contextId), cancellationToken);

            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        }).DisableAntiforgery();
    }
}



public sealed class StartMultipartUploadValidator : AbstractValidator<StartMultipartUploadRequest>
{
    public StartMultipartUploadValidator()
    {
        RuleFor(x => x.FileName)
            .NotEmpty()
            .WithError(GeneralErrors.ValueIsInvalid("file name"));

        RuleFor(x => x.AssetType)
            .NotEmpty()
            .WithError(GeneralErrors.ValueIsInvalid("asset type"));

        RuleFor(x => x.ContentType)
            .NotEmpty()
            .WithError(GeneralErrors.ValueIsInvalid("content type"));

        RuleFor(x => x.Size)
            .Must(size => size > 0)
            .WithError(GeneralErrors.ValueIsInvalid("file size"));

        RuleFor(x => x.Context)
            .NotEmpty()
            .WithError(GeneralErrors.ValueIsInvalid("context"));

        RuleFor(x => x.ContextId)
            .NotEmpty()
            .WithError(GeneralErrors.ValueIsInvalid("context id"));
    }
}


public sealed class StartMultipartUploadHandler : ICommandHandler<StartMultipartUploadResponse, StartMultipartUploadRequest>
{
    private readonly IS3Provider _s3Provider;
    private readonly IMediaRepository _mediaRepository;
    private readonly IChunkSizeCalculator _chunkSizeCalculator;
    private readonly IValidator<StartMultipartUploadRequest> _validator;
    private readonly ILogger<StartMultipartUploadHandler> _logger;

    public StartMultipartUploadHandler(
        IS3Provider s3Provider,
        IMediaRepository mediaRepository,
        IChunkSizeCalculator chunkSizeCalculator,
        IValidator<StartMultipartUploadRequest> validator,
        ILogger<StartMultipartUploadHandler> logger)
    {
        _s3Provider = s3Provider;
        _mediaRepository = mediaRepository;
        _chunkSizeCalculator = chunkSizeCalculator;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<StartMultipartUploadResponse, Errors>> Handle(StartMultipartUploadRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request);

        if (!validationResult.IsValid)
            return validationResult.ToList();

        var fileNameResult = FileName.Create(request.FileName);

        if (fileNameResult.IsFailure)
            return fileNameResult.Error.ToErrors();

        var contentTypeResult = ContentType.Create(request.ContentType);

        if (contentTypeResult.IsFailure)
            return contentTypeResult.Error.ToErrors();

        var chunkCalculationResult = _chunkSizeCalculator.CalculateChunkSize(request.Size);

        var mediaDataResult = MediaData.Create(
            fileNameResult.Value,
            contentTypeResult.Value,
            request.Size,
            chunkCalculationResult.Value.TotalChunks);

        var mediaAssetResult = MediaAsset.CreateForUpload(mediaDataResult.Value, request.AssetType.ToAssetType());

        var storageKey = mediaAssetResult.Value.RawKey ?? mediaAssetResult.Value.FinalKey;

        await _mediaRepository.AddAsync(mediaAssetResult.Value,  cancellationToken);

        var startUploadResult = await _s3Provider.StartMultipartUploadAsync(
            storageKey,
            mediaAssetResult.Value.MediaData,
            cancellationToken);

        if (startUploadResult.IsFailure)
            return startUploadResult.Error.ToErrors();

        var chunkUploadUrlsResult = await _s3Provider.GenerateAllChunksUploadUrlsAsync(
            storageKey,
            startUploadResult.Value,
            chunkCalculationResult.Value.TotalChunks,
            cancellationToken);

        if (chunkUploadUrlsResult.IsFailure)
            return chunkUploadUrlsResult.Error.ToErrors();

        _logger.LogInformation("Started MultipartUpload");

        return new StartMultipartUploadResponse(
            mediaAssetResult.Value.Id,
            startUploadResult.Value,
            chunkUploadUrlsResult.Value,
            chunkCalculationResult.Value.ChunkSize);
    }
}