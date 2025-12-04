using DirectoryService.Application.Abstractions;
using DirectoryService.Application.CreateLocation;
using DirectoryService.Contracts.Location;
using DirectoryService.Presenters.EndpointResults;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.Presenters;

[ApiController]
[Route("/api/locations")]
public class LocationsController : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<Envelope<string>>(200)]
    [ProducesResponseType<Envelope>(404)]
    [ProducesResponseType<Envelope>(500)]
    [ProducesResponseType<Envelope>(401)]
    public async Task<EndpointResult<Guid>> Create(
        [FromServices] ICommandHandler<Guid, CreateLocationCommand> handler,
        [FromBody] CreateLocationDTO dto,
        CancellationToken cancellationToken)
    {
        var command = new CreateLocationCommand(dto);

        return await handler.Handle(command, cancellationToken);
    }
}