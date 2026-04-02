using CSharpFunctionalExtensions;
using DirectoryService.Domain.Departments.ValueObjects;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Locations.ValueObjects;
using DirectoryService.Shared;

namespace DirectoryService.Application.Locations;

public interface ILocationsRepository
{
    Task<Result<Guid, Error>> AddAsync(Location location, CancellationToken cancellationToken);

    Task<Result<Guid, Error>> UpdateAsync(Location location, CancellationToken cancellationToken);

    Task<Location?> GetByIdAsync(LocationId locationId, CancellationToken cancellationToken);

    Task<Result<Guid, Error>> DeleteAsync(Guid locationId, CancellationToken cancellationToken);

    Task<UnitResult<Error>> DeactivateLocationsAsync(DepartmentId departmentId, CancellationToken cancellationToken);

    Task<Result<bool, Error>> AllLocationsExistAndActiveAsync(
        IEnumerable<LocationId> locations,
        CancellationToken cancellationToken);
}