using CSharpFunctionalExtensions;
using DirectoryService.Application.Locations;
using DirectoryService.Domain.Locations;
using DirectoryService.Shared;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Infrastructure.Postgres.Repositories;

public class LocationsEfCoreRepository : ILocationsRepository
{
    private readonly DirectoryServiceDbContext _context;
    private readonly ILogger<LocationsEfCoreRepository> _logger;

    public LocationsEfCoreRepository(DirectoryServiceDbContext context, ILogger<LocationsEfCoreRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<Guid, Error>> AddAsync(Location location, CancellationToken cancellationToken)
    {
        try
        {
            await _context.Locations.AddAsync(location, cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError("The operation to add the location with id: {LocationId} has failed.", location.Id.Value);
        }

        return location.Id.Value;
    }

    public Task<Result<Guid, Error>> UpdateAsync(Location location, CancellationToken cancellationToken)
        => throw new NotImplementedException();

    public Task<Result<Location, Error>> GetByIdAsync(Guid locationId, CancellationToken cancellationToken)
        => throw new NotImplementedException();

    public Task<Result<Guid, Error>> DeleteAsync(Guid locationId, CancellationToken cancellationToken)
        => throw new NotImplementedException();
}