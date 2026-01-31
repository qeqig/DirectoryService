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

public record DeleteFileRequest(Guid FileId) : ICommand;

public class DeleteFileEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("files/{fileId:guid}", async (
            Guid fileId,
            [FromServices] DeleteFileHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(new DeleteFileRequest(fileId), cancellationToken);

            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });
    }
}

public class DeleteFileValidator : AbstractValidator<DeleteFileRequest>
{
    public DeleteFileValidator()
    {
        RuleFor(x => x.FileId)
            .NotEmpty()
            .WithError(GeneralErrors.ValueIsRequired());
    }
}

public sealed class DeleteFileHandler : ICommandHandler<Guid, DeleteFileRequest>
{
    private readonly IMediaRepository _mediaRepository;
    private readonly IValidator<DeleteFileRequest> _validator;
    private readonly IS3Provider _s3Provider;
    private readonly ILogger<DeleteFileHandler> _logger;

    public DeleteFileHandler(
        IMediaRepository mediaRepository,
        IValidator<DeleteFileRequest> validator,
        IS3Provider s3Provider,
        ILogger<DeleteFileHandler> logger)
    {
        _mediaRepository = mediaRepository;
        _validator = validator;
        _s3Provider = s3Provider;
        _logger = logger;
    }

    public async Task<Result<Guid, Errors>> Handle(DeleteFileRequest command, CancellationToken cancellationToken)
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

        var deleteFileResult = await _s3Provider.DeleteFileAsync(key, cancellationToken);

        if (deleteFileResult.IsFailure)
            return deleteFileResult.Error.ToErrors();

        var markDeleteResult = mediaAssetResult.Value.MarkDeleted(DateTime.UtcNow);

        if (markDeleteResult.IsFailure)
            return markDeleteResult.Error.ToErrors();

        var saveChangesResult = await _mediaRepository.SaveChangesAsync(cancellationToken);

        if (saveChangesResult.IsFailure)
            return saveChangesResult.Error.ToErrors();

        _logger.LogInformation("Deleted file {fileId}", command.FileId);

        return mediaAssetResult.Value.Id;
    }
}