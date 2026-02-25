using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Departments.CreateDepartment;
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
}