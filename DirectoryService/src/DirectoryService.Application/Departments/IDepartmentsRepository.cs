using CSharpFunctionalExtensions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Departments.ValueObjects;
using DirectoryService.Shared;
using Path = DirectoryService.Domain.Departments.ValueObjects.Path;

namespace DirectoryService.Application.Departments;

public interface IDepartmentsRepository
{
    Task<Result<Guid, Error>> AddAsync(Department department, CancellationToken cancellationToken);

    Task<Result<Guid, Error>> UpdateAsync(Department department, CancellationToken cancellationToken);

    Task<Result<Department, Error>> GetByIdAsync(Guid departmentId, CancellationToken cancellationToken);

    Task<Result<Department, Error>> GetByIdWithDepartmentLocationsAsync(
        Guid departmentId,
        CancellationToken cancellationToken);

    Task<Result<Department, Error>> GetByIdWithLockAsync(
        DepartmentId departmentId,
        CancellationToken cancellationToken,
        bool isActive = true);

    Task MoveDepartmentSubtreeAsync(
        Path oldPath,
        Path newPath,
        Guid? newParentId,
        CancellationToken cancellationToken);

    Task LockDescendantsAsync(string oldPath, CancellationToken cancellationToken);

    Task<Result<Guid, Error>> DeleteAsync(Guid departmentId, CancellationToken cancellationToken);

    Task<UnitResult<Error>> DeleteDepartmentLocationsByDepartmentId(
        DepartmentId departmentId,
        CancellationToken cancellationToken);

    Task<Result<bool, Error>> AllDepartmentsExistAndActiveAsync(
        IEnumerable<DepartmentId> departmentIds,
        CancellationToken cancellationToken);

    Task SoftDeleteDepartmentSubtreeAsync(Path oldPath, CancellationToken cancellationToken);
}