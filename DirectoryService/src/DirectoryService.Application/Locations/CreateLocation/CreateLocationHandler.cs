using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Locations.ValueObjects;
using DirectoryService.Shared;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Locations.CreateLocation;

public class CreateLocationHandler : ICommandHandler<Guid, CreateLocationCommand>
{
    private readonly ILocationsRepository _locationsRepository;
    private readonly IValidator<CreateLocationCommand> _validator;
    private readonly ILogger<CreateLocationHandler> _logger;

    public CreateLocationHandler(
        ILocationsRepository locationsRepository,
        IValidator<CreateLocationCommand> validator,
        ILogger<CreateLocationHandler> logger)
    {
        _locationsRepository = locationsRepository;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<Guid, Errors>> Handle(CreateLocationCommand command, CancellationToken cancellationToken)
    {
        ValidationResult validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToError();
        }

        LocationId locationId = new(Guid.NewGuid());

        var nameResult = Name.Create(command.CreateLocationRequest.Name);

        var addressResult = Address.Create(
            command.CreateLocationRequest.AddressDto.AddressCountry,
            command.CreateLocationRequest.AddressDto.AddressCity,
            command.CreateLocationRequest.AddressDto.AddressStreet,
            command.CreateLocationRequest.AddressDto.AddressBuilding,
            command.CreateLocationRequest.AddressDto.AddressOfficeNumber);

        var timezoneResult = Timezone.Create(command.CreateLocationRequest.Timezone);

        Location location = new Location(
            locationId,
            nameResult.Value,
            addressResult.Value,
            timezoneResult.Value,
            command.CreateLocationRequest.IsActive);

        var addResult = await _locationsRepository.AddAsync(location, cancellationToken);
        if (addResult.IsFailure)
            return addResult.Error.ToErrors();

        _logger.LogInformation("Location created with id {LocationId}", locationId.Value);

        return locationId.Value;
    }
}