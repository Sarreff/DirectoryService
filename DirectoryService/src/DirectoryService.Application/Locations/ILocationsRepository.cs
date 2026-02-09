using CSharpFunctionalExtensions;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Shared;

namespace DirectoryService.Application.Locations;

public interface ILocationsRepository
{
    Task<Result<Guid, Error>> AddAsync(Location location, CancellationToken cancellationToken);

    Task<Result<Guid, Error>> UpdateAsync(Location location, CancellationToken cancellationToken);

    Task<Result<Location, Error>> GetByIdAsync(Guid locationId, CancellationToken cancellationToken);

    Task<Result<Guid, Error>> DeleteAsync(Guid locationId, CancellationToken cancellationToken);
}