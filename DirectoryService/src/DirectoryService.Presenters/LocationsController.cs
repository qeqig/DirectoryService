using DirectoryService.Application.Locations;
using DirectoryService.Contracts.Location;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.Presenters;

[ApiController]
[Route("/api/locations")]
public class LocationsController : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<Guid>> Create(
        [FromServices] CreateLocationHandler handler,
        [FromBody] CreateLocationDTO dto,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(dto, cancellationToken);

        if (result.IsSuccess)
            return Ok(result.Value);
        else
            return BadRequest(result.Error);
    }
}