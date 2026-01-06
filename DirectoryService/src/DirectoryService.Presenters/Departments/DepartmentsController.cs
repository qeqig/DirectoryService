using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Departments.CreateDepartment;
using DirectoryService.Application.Departments.DeleteDepartment;
using DirectoryService.Application.Departments.GetChildDepartmentsById;
using DirectoryService.Application.Departments.GetRootWithChildren;
using DirectoryService.Application.Departments.GetTopDepartmentsByPosition;
using DirectoryService.Application.Departments.MoveDepartment;
using DirectoryService.Application.Departments.UpdateDepartmentsLocation;
using DirectoryService.Contracts.Department.CreateDepartment;
using DirectoryService.Contracts.Department.GetChildDepartmentsById;
using DirectoryService.Contracts.Department.GetRootWithChildren;
using DirectoryService.Contracts.Department.GetTopDepartmentsByPosition;
using DirectoryService.Contracts.Department.MoveDepartments;
using DirectoryService.Contracts.Department.UpdateDepartments;
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

    [HttpPatch("{departmentId:guid}/locations")]
    [ProducesResponseType<Envelope<Guid>>(200)]
    [ProducesResponseType<Envelope>(404)]
    [ProducesResponseType<Envelope>(500)]
    [ProducesResponseType<Envelope>(401)]
    public async Task<EndpointResult<Guid>> UpdateDepartmentLocationId(
        Guid departmentId,
        [FromServices] ICommandHandler<Guid, UpdateDepartmentLocationCommand> handler,
        [FromBody] UpdateDepartmentLocationDto dto,
        CancellationToken cancellationToken)
    {
        var command = new UpdateDepartmentLocationCommand(departmentId, dto);

        return await handler.Handle(command, cancellationToken);
    }

    [HttpPut("{departmentId:guid}/parent")]
    [ProducesResponseType<Envelope<Guid>>(200)]
    [ProducesResponseType<Envelope>(404)]
    [ProducesResponseType<Envelope>(500)]
    [ProducesResponseType<Envelope>(401)]
    public async Task<EndpointResult<Guid>> MoveDepartment(
        Guid departmentId,
        [FromServices] ICommandHandler<Guid, MoveDepartmentCommand> handler,
        [FromBody] MoveDepartmentsDto dto,
        CancellationToken cancellationToken)
    {
        var command = new MoveDepartmentCommand(departmentId, dto);

        return await handler.Handle(command, cancellationToken);
    }

    [HttpGet("top-positions")]
    public async Task<ActionResult<GetTopDepartmentResponse>> GetTopDepartments(
        [FromServices] GetTopDepartmentsByPositionHandler handler,
        CancellationToken cancellationToken)
    {
        var departments = await handler.Handle(new GetTopDepartmentsByPositionQuery(),  cancellationToken);

        return Ok(departments.Value);
    }

    [HttpGet("root-with-children")]
    public async Task<ActionResult<GetRootWithChildrenResponse>> GetRootWithChildren(
        [FromQuery] GetRootWithChildrenDto dto,
        [FromServices] GetRootWithChildrenHandler handler,
        CancellationToken cancellationToken)
    {
        var departments = await handler.Handle(new GetRootWithChildrenQuery(dto), cancellationToken);

        return Ok(departments.Value);
    }

    [HttpGet("{parentId:Guid}/children")]
    public async Task<ActionResult> GetChildDepartmentsById(
        [FromRoute] Guid parentId,
        [FromQuery] GetChildDepartmentsByIdDto dto,
        [FromServices] GetChildDepartmentsByIdHandler handler,
        CancellationToken cancellationToken)
    {
        var departments = await handler.Handle(new GetChildDepartmentsByIdQuery(parentId, dto), cancellationToken);

        return Ok(departments.Value);
    }

    [HttpDelete("{departmentId:guid}")]
    public async Task<EndpointResult<Guid>> SoftDeleteDepartment(
        [FromRoute] Guid departmentId,
        [FromServices] ICommandHandler<Guid, DeleteDepartmentCommand> handler,
        CancellationToken cancellationToken)
    {
        var command = new DeleteDepartmentCommand(departmentId);

        return await handler.Handle(command, cancellationToken);
    }
}