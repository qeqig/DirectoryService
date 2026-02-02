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

public class GenerateDownloadUrlsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("files/download-urls", async (
            Guid[] fileIds,
            [FromServices] GenerateDownloadUrlsHandler hanler,
            CancellationToken cancellationToken) =>
        {
            var result = await hanler.Handle(new GenerateDownloadUrlsRequest(fileIds), cancellationToken);

            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });
    }
}

public sealed record GenerateDownloadUrlsRequest(IEnumerable<Guid> FileIds) : ICommand;

public sealed class GenerateDownloadUrlsValidator : AbstractValidator<GenerateDownloadUrlsRequest>
{
    public GenerateDownloadUrlsValidator()
    {
        RuleForEach(x => x.FileIds)
            .NotNull()
            .WithError(GeneralErrors.ValueIsInvalid());
    }
}

public sealed class GenerateDownloadUrlsHandler : ICommandHandler<IReadOnlyList<string>, GenerateDownloadUrlsRequest>
{
    private readonly IS3Provider _s3Provider;
    private readonly IMediaRepository _mediaRepository;
    private readonly IValidator<GenerateDownloadUrlsRequest> _validator;
    private readonly ILogger<GenerateDownloadUrlsHandler> _logger;

    public GenerateDownloadUrlsHandler(
        IS3Provider s3Provider,
        IMediaRepository mediaRepository,
        IValidator<GenerateDownloadUrlsRequest> validator,
        ILogger<GenerateDownloadUrlsHandler> logger)
    {
        _s3Provider = s3Provider;
        _mediaRepository = mediaRepository;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<IReadOnlyList<string>, Errors>> Handle(GenerateDownloadUrlsRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
            return validationResult.ToList();

        var mediaAssetResult = await _mediaRepository.GetManyBy(x => request.FileIds.Contains(x.Id), cancellationToken);

        if (mediaAssetResult.IsFailure)
            return mediaAssetResult.Error.ToErrors();

        var keys = mediaAssetResult.Value.Select(x => x.RawKey == null || x.RawKey.IsEmpty()
            ? x.FinalKey
            : x.RawKey ).ToList();

        var downloadUrls = await _s3Provider.GenerateDownloadUrlsAsync(keys);

        if (downloadUrls.IsFailure)
            return downloadUrls.Error.ToErrors();

        _logger.LogInformation("Download urls generated");

        return Result.Success<IReadOnlyList<string>, Errors>(downloadUrls.Value);
    }
}