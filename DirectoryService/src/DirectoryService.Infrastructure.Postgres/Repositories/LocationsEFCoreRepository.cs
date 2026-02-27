using CSharpFunctionalExtensions;
using DirectoryService.Application.Locations;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Locations.ValueObjects;
using DirectoryService.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using Index = DirectoryService.Infrastructure.Postgres.Configurations.Index;

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
        catch (DbUpdateException ex)
        {
            PostgresException? pgEx = FindPostgresException(ex);

            if (pgEx?.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                if (string.Equals(
                        pgEx.ConstraintName,
                        Index.NAME,
                        StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogInformation(
                        "Unique name violation for location '{Name}'",
                        location.Name.Value);

                    return LocationErrors.NameConflict(location.Name.Value);
                }

                if (string.Equals(
                        pgEx.ConstraintName,
                        "ix_location_address_full_path",
                        StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogInformation(
                        "Unique address violation for location '{Address}'",
                        location.Address.ToString());

                    return LocationErrors.AddressConflict();
                }
            }

            _logger.LogError(
                ex,
                "Database update error while creating location with name {Name}",
                location.Name.Value);

            return LocationErrors.DatabaseError();
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogError(
                ex,
                "Operation was cancelled while creating location with name {Name}", location.Name.Value);

            return LocationErrors.OperationCancelled();
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unexpected error while creating location with name {Name}", location.Name.Value);

            return LocationErrors.DatabaseError();
        }

        return location.Id.Value;
    }

    public Task<Result<Guid, Error>> UpdateAsync(Location location, CancellationToken cancellationToken)
        => throw new NotImplementedException();

    public Task<Result<Location, Error>> GetByIdAsync(Guid locationId, CancellationToken cancellationToken)
        => throw new NotImplementedException();

    public Task<Result<Guid, Error>> DeleteAsync(Guid locationId, CancellationToken cancellationToken)
        => throw new NotImplementedException();

    public async Task<Result<bool, Error>> AllLocationsExistAndActiveAsync(
        IEnumerable<LocationId> locationIds,
        CancellationToken cancellationToken)
    {
        try
        {
            int existingCount = await _context.Locations
                .CountAsync(
                    l =>
                        locationIds.Contains(l.Id) &&
                        l.IsActive,
                    cancellationToken);

            if (existingCount == locationIds.Count())
            {
                return true;
            }

            _logger.LogError("Some locations were not found in the database or they are inactive");
            return LocationErrors.LocationsNotFoundOrInactive();
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unexpected error while checking locations existing in database");

            return LocationErrors.DatabaseError();
        }
    }

    private static PostgresException? FindPostgresException(Exception ex)
    {
        Exception? current = ex;
        while (current != null)
        {
            if (current is PostgresException pgEx)
                return pgEx;

            current = current.InnerException;
        }

        return null;
    }
}