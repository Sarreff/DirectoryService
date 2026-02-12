using CSharpFunctionalExtensions;
using DirectoryService.Application.Locations;
using DirectoryService.Domain.Locations;
using DirectoryService.Shared;

namespace DirectoryService.Infrastructure.Postgres.Repositories;

public class LocationsEfCoreRepository : ILocationsRepository
{
    private readonly DirectoryServiceDbContext _context;

    public LocationsEfCoreRepository(DirectoryServiceDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid, Error>> AddAsync(Location location, CancellationToken cancellationToken)
    {
        await _context.Locations.AddAsync(location, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        return location.Id.Value;
    }

    public Task<Result<Guid, Error>> UpdateAsync(Location location, CancellationToken cancellationToken)
        => throw new NotImplementedException();

    public Task<Result<Location, Error>> GetByIdAsync(Guid locationId, CancellationToken cancellationToken)
        => throw new NotImplementedException();

    public Task<Result<Guid, Error>> DeleteAsync(Guid locationId, CancellationToken cancellationToken)
        => throw new NotImplementedException();
}