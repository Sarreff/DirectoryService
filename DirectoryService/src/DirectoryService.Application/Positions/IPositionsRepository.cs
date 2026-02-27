using CSharpFunctionalExtensions;
using DirectoryService.Domain.Positions;
using DirectoryService.Domain.Positions.ValueObjects;
using DirectoryService.Shared;

namespace DirectoryService.Application.Positions;

public interface IPositionsRepository
{
    Task<Result<Guid, Error>> AddAsync(Position position, CancellationToken cancellationToken);

    Task<Result<Guid, Error>> UpdateAsync(Position position, CancellationToken cancellationToken);

    Task<Result<Position, Error>> GetByIdAsync(Guid positionId, CancellationToken cancellationToken);

    Task<Result<Guid, Error>> DeleteAsync(Guid positionId, CancellationToken cancellationToken);

    Task<Result<bool, Error>> IsNameUniqueAndActiveAsync(Name name, CancellationToken cancellationToken);
}