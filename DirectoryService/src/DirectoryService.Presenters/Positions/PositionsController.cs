using DirectoryService.Application.Abstractions;
using DirectoryService.Application.CreatePosition;
using DirectoryService.Contracts.Position;
using DirectoryService.Presenters.EndpointResults;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.Presenters.Positions;

[ApiController]
[Route("/api/positions")]
public class PositionsController : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<Envelope<string>>(200)]
    [ProducesResponseType<Envelope>(404)]
    [ProducesResponseType<Envelope>(500)]
    [ProducesResponseType<Envelope>(401)]
    public async Task<EndpointResult<Guid>> Create(
        [FromServices] ICommandHandler<Guid, CreatePositionCommand> handler,
        [FromBody] CreatePositionDto dto,
        CancellationToken cancellationToken)
    {
        var command = new CreatePositionCommand(dto);

        return await handler.Handle(command, cancellationToken);
    }
}