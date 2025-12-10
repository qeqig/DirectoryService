using DirectoryService.Application.Abstractions;
using DirectoryService.Application.CreateDepartment;
using DirectoryService.Contracts.Department;
using DirectoryService.Presenters.EndpointResults;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.Presenters.Departments;

[ApiController]
[Route("/api/departments")]
public class DepartmentsController : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<Envelope<string>>(200)]
    [ProducesResponseType<Envelope>(404)]
    [ProducesResponseType<Envelope>(500)]
    [ProducesResponseType<Envelope>(401)]
    public async Task<EndpointResult<Guid>> Create(
        [FromServices] ICommandHandler<Guid, CreateDepartmentCommand> handler,
        [FromBody] CreateDepartmentDto dto,
        CancellationToken cancellationToken)
    {
        var command = new CreateDepartmentCommand(dto);

        return await handler.Handle(command, cancellationToken);
    }
}