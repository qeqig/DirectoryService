using Core.Abstractions;
using Core.Validation;
using CSharpFunctionalExtensions;
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

public sealed class UploadFileEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("files/upload", async (
            IFormFile file,
            [FromServices] UploadFileHandler handler,
            CancellationToken cancellationToken) =>
        {
            var fileName = file.FileName;
            var contentType = file.ContentType;
            await using var stream = file.OpenReadStream();

            var request = new UploadFileRequest(fileName, stream, contentType, file.Length);

            var result = await handler.Handle(request, cancellationToken);

            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        }).DisableAntiforgery();
    }
}

public record UploadFileRequest(string FileName, Stream Stream, string ContentType, long Size) : ICommand;

public class UploadFileValidator : AbstractValidator<UploadFileRequest>
{
    public UploadFileValidator()
    {
        RuleFor(x => x.FileName)
            .NotEmpty()
            .WithError(GeneralErrors.ValueIsInvalid("FileName"));

        RuleFor(x => x.ContentType)
            .NotEmpty()
            .WithError(GeneralErrors.ValueIsInvalid("ContentType"));

        RuleFor(x => x.Size)
            .NotEmpty()
            .WithError(GeneralErrors.ValueIsInvalid("Size"));
    }
}

public sealed class UploadFileHandler : ICommandHandler<Guid, UploadFileRequest>
    {
        private readonly IMediaRepository _mediaRepository;
        private readonly IS3Provider _s3Provider;
        private readonly IValidator<UploadFileRequest> _validator;
        private readonly ILogger<UploadFileHandler> _logger;

        public UploadFileHandler(
            IMediaRepository mediaRepository,
            IS3Provider s3Provider,
            IValidator<UploadFileRequest> validator,
            ILogger<UploadFileHandler> logger)
        {
            _mediaRepository = mediaRepository;
            _s3Provider = s3Provider;
            _validator = validator;
            _logger = logger;
        }

        public async Task<Result<Guid, Errors>> Handle(UploadFileRequest command, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(command,  cancellationToken);

            if (!validationResult.IsValid)
                return validationResult.ToList();

            var fileNameResult = FileName.Create(command.FileName);

            if (!fileNameResult.IsSuccess)
                return fileNameResult.Error.ToErrors();

            var contentTypeResult = ContentType.Create(command.ContentType);

            if (!contentTypeResult.IsSuccess)
                return contentTypeResult.Error.ToErrors();

            var mediaDataResult = MediaData.Create(fileNameResult.Value, contentTypeResult.Value, command.Size, 1);

            if (!mediaDataResult.IsSuccess)
                return mediaDataResult.Error.ToErrors();

            var mediaAssetResult = MediaAsset.CreateForUpload(mediaDataResult.Value, command.ContentType.ToAssetType());

            if (!mediaAssetResult.IsSuccess)
                return mediaAssetResult.Error.ToErrors();

            var saveResult = await _mediaRepository.AddAsync(mediaAssetResult.Value, cancellationToken);

            if (!saveResult.IsSuccess)
                return saveResult.Error.ToErrors();

            var key = mediaAssetResult.Value.RawKey == null || mediaAssetResult.Value.RawKey.IsEmpty()
                ? mediaAssetResult.Value.FinalKey
                : mediaAssetResult.Value.RawKey;

            var uploadResult = await _s3Provider.UploadFileAsync(key, command.Stream, mediaDataResult.Value, cancellationToken);

            if (!uploadResult.IsSuccess)
                return uploadResult.Error.ToErrors();

            _logger.LogInformation("Uploaded file {FileName}", mediaAssetResult.Value.Id);

            return mediaAssetResult.Value.Id;
        }
    }