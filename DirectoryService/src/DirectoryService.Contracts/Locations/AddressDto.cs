namespace DirectoryService.Contracts.Locations;

public record AddressDto(
    string AddressCountry,
    string AddressCity,
    string AddressStreet,
    int AddressBuilding,
    int AddressOfficeNumber
);