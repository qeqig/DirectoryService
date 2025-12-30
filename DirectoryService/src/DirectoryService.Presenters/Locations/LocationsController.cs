using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Locations.CreateLocation;
using DirectoryService.Application.Locations.GetLocation;
using DirectoryService.Contracts.Location;
using DirectoryService.Domain.Location;
using DirectoryService.Presenters.EndpointResults;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.Presenters.Locations;

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
        [FromBody] CreateLocationDto dto,
        CancellationToken cancellationToken)
    {
        var command = new CreateLocationCommand(dto);

        return await handler.Handle(command, cancellationToken);
    }

    [HttpGet]
    public async Task<ActionResult<GetLocationResponse>> GetLocation(
        [FromQuery] GetLocationDto dto,
        [FromServices] GetLocationHandler handler,
        CancellationToken cancellationToken)
    {
        var query = new GetLocationQuery(dto);

        var result = await handler.Handle(query, cancellationToken);

        return Ok(result.Value);
    }
}