using DirectoryService.Application.Locations;
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
        [FromServices] CreateLocationHandler handler,
        [FromBody] CreateLocationDTO dto,
        CancellationToken cancellationToken)
    {
       return await handler.Handle(dto, cancellationToken);
    }
}