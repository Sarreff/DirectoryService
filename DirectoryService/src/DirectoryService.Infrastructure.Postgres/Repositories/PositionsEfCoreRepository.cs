using CSharpFunctionalExtensions;
using DirectoryService.Application.Positions;
using DirectoryService.Domain.Positions;
using DirectoryService.Domain.Positions.ValueObjects;
using DirectoryService.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Infrastructure.Postgres.Repositories;

public class PositionsEfCoreRepository : IPositionsRepository
{
    private readonly DirectoryServiceDbContext _context;
    private readonly ILogger<PositionsEfCoreRepository> _logger;

    public PositionsEfCoreRepository(DirectoryServiceDbContext context, ILogger<PositionsEfCoreRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<Guid, Error>> AddAsync(Position position, CancellationToken cancellationToken)
    {
        try
        {
            await _context.Positions.AddAsync(position, cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogError(
                ex,
                "Operation was cancelled while creating position with name {Name}", position.Name.Value);

            return PositionErrors.OperationCancelled();
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unexpected error while creating position with name {Name}", position.Name.Value);

            return PositionErrors.DatabaseError();
        }

        return position.Id.Value;
    }

    public Task<Result<Guid, Error>> UpdateAsync(Position position, CancellationToken cancellationToken)
        => throw new NotImplementedException();

    public Task<Result<Position, Error>> GetByIdAsync(Guid positionId, CancellationToken cancellationToken)
        => throw new NotImplementedException();

    public Task<Result<Guid, Error>> DeleteAsync(Guid positionId, CancellationToken cancellationToken)
        => throw new NotImplementedException();

    public async Task<Result<bool, Error>> IsNameUniqueAndActiveAsync(Name name, CancellationToken cancellationToken)
    {
        try
        {
            bool exists = await _context.Positions
                .Where(p => p.IsActive)
                .Select(p => p.Name.Value)
                .ContainsAsync(name.Value, cancellationToken);

            return exists ? PositionErrors.DuplicateActiveName() : true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while checking active position name uniqueness");
            return PositionErrors.DatabaseError();
        }
    }
}