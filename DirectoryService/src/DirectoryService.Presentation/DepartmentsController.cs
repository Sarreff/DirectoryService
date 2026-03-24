using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Departments.CreateDepartment;
using DirectoryService.Application.Departments.GetDepartment;
using DirectoryService.Application.Departments.MoveDepartment;
using DirectoryService.Application.Departments.UpdateDepartment;
using DirectoryService.Contracts.Departments;
using DirectoryService.Presentation.Results;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.Presentation;

[ApiController]
[Route("api/departments")]
public class DepartmentsController : ControllerBase
{
    [HttpPost]
    public async Task<EndpointResult<Guid>> Create(
        [FromServices] ICommandHandler<Guid, CreateDepartmentCommand> handler,
        [FromBody] CreateDepartmentCommand command,
        CancellationToken cancellationToken)
    {
        return await handler.Handle(command, cancellationToken);
    }

    [HttpPatch("{departmentId:guid}/locations")]
    public async Task<EndpointResult<Guid>> UpdateDepartmentLocations(
        [FromRoute] Guid departmentId,
        [FromServices] ICommandHandler<Guid, UpdateDepartmentLocationsCommand> handler,
        [FromBody] UpdateDepartmentLocationsRequest request,
        CancellationToken cancellationToken)
    {
        return await handler.Handle(new UpdateDepartmentLocationsCommand(departmentId, request), cancellationToken);
    }

    [HttpPut("{departmentId:guid}/parent")]
    public async Task<EndpointResult<Guid>> MoveDepartment(
        [FromRoute] Guid departmentId,
        [FromServices] ICommandHandler<Guid, MoveDepartmentCommand> handler,
        [FromBody] MoveDepartmentRequest request,
        CancellationToken cancellationToken)
    {
        return await handler.Handle(new MoveDepartmentCommand(departmentId, request), cancellationToken);
    }

    [HttpGet("top-positions")]
    public async Task<EndpointResult<GetTopDepartmentsByPositionCountDto>> GetTopPositions(
        [FromServices] IQueryHandler<GetTopDepartmentsByPositionCountDto, GetTopPositionsQuery> handler,
        CancellationToken cancellationToken)
    {
        return await handler.Handle(new GetTopPositionsQuery(), cancellationToken);
    }
}