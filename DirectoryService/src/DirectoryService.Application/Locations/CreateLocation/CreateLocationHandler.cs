using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Locations.ValueObjects;
using DirectoryService.Shared;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Locations.CreateLocation;

public class CreateLocationHandler : ICommandHandler<Guid, CreateLocationCommand>
{
    private readonly ILocationsRepository _locationsRepository;
    private readonly ILogger<CreateLocationHandler> _logger;

    public CreateLocationHandler(ILocationsRepository locationsRepository, ILogger<CreateLocationHandler> logger)
    {
        _locationsRepository = locationsRepository;
        _logger = logger;
    }

    public async Task<Result<Guid, Errors>> Handle(CreateLocationCommand command, CancellationToken cancellationToken)
    {
        LocationId locationId = new(Guid.NewGuid());
        var nameResult = Name.Create(command.CreateLocationRequest.Name);
        if (nameResult.IsFailure)
            return nameResult.Error.ToErrors();

        var addressResult = Address.Create(
            command.CreateLocationRequest.AddressDto.AddressCountry,
            command.CreateLocationRequest.AddressDto.AddressCity,
            command.CreateLocationRequest.AddressDto.AddressStreet,
            command.CreateLocationRequest.AddressDto.AddressBuilding,
            command.CreateLocationRequest.AddressDto.AddressOfficeNumber);
        if (addressResult.IsFailure)
            return addressResult.Error.ToErrors();

        var timezoneResult = Timezone.Create(command.CreateLocationRequest.Timezone);
        if (timezoneResult.IsFailure)
            return timezoneResult.Error.ToErrors();

        Location location = new Location(
            locationId,
            nameResult.Value,
            addressResult.Value,
            timezoneResult.Value,
            command.CreateLocationRequest.IsActive);

        await _locationsRepository.AddAsync(location, cancellationToken);

        _logger.LogInformation("Location created with id {LocationId}", locationId.Value);

        return locationId.Value;
    }
}