using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Locations;

namespace DirectoryService.Application.Locations.GetLocation;

public record GetLocationsQuery(GetLocationsRequest GetLocationsRequest) : IQuery;