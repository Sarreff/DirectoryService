using CSharpFunctionalExtensions;
using DirectoryService.Domain.Departments.ValueObjects;
using DirectoryService.Domain.Positions;
using DirectoryService.Shared;
using Name = DirectoryService.Domain.Positions.ValueObjects.Name;

namespace DirectoryService.Application.Positions;

public interface IPositionsRepository
{
    Task<Result<Guid, Error>> AddAsync(Position position, CancellationToken cancellationToken);

    Task<Result<Guid, Error>> UpdateAsync(Position position, CancellationToken cancellationToken);

    Task<Result<Position, Error>> GetByIdAsync(Guid positionId, CancellationToken cancellationToken);

    Task<Result<Guid, Error>> DeleteAsync(Guid positionId, CancellationToken cancellationToken);

    Task<UnitResult<Error>> DeactivatePositionsAsync(DepartmentId departmentId, CancellationToken cancellationToken);

    Task<Result<bool, Error>> IsNameUniqueAndActiveAsync(Name name, CancellationToken cancellationToken);
}