using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Locations.CreateLocation;
using DirectoryService.Domain.Exceptions;
using DirectoryService.Presentation.Results;
using DirectoryService.Shared;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.Presentation;

[ApiController]
[Route("api/locations")]
public class LocationsController : ControllerBase
{
    [HttpPost]
    public async Task<EndpointResult<Guid>> Create(
        [FromServices] ICommandHandler<Guid, CreateLocationCommand> handler,
        [FromBody] CreateLocationCommand command,
        CancellationToken cancellationToken)
    {
        return await handler.Handle(command, cancellationToken);
    }

    // Для проверки ExceptionMiddleware
    [HttpGet]
    public async Task<EndpointResult<string>> Test()
    {
        throw new NotFoundException(GeneralErrors.Failure("Тестовая ошибка"));
    }
}