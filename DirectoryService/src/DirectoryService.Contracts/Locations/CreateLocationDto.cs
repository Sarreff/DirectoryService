namespace DirectoryService.Contracts.Locations;

public record CreateLocationDto(
    string Name,
    string AddressCountry,
    string AddressCity,
    string AddressStreet,
    int AddressBuilding,
    int AddressOfficeNumber,
    string Timezone,
    bool IsActive);