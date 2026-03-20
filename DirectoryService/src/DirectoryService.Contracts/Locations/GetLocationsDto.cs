namespace DirectoryService.Contracts.Locations;

public record GetLocationsDto(List<LocationDto> Locations, long TotalCount);