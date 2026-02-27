using CSharpFunctionalExtensions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Departments.ValueObjects;
using DirectoryService.Shared;

namespace DirectoryService.Application.Departments;

public interface IDepartmentsRepository
{
    Task<Result<Guid, Error>> AddAsync(Department department, CancellationToken cancellationToken);

    Task<Result<Guid, Error>> UpdateAsync(Department department, CancellationToken cancellationToken);

    Task<Result<Department, Error>> GetByIdAsync(Guid departmentId, CancellationToken cancellationToken);

    Task<Result<Guid, Error>> DeleteAsync(Guid departmentId, CancellationToken cancellationToken);

    Task<Result<bool, Error>> AllDepartmentsExistAndActiveAsync(
        IEnumerable<DepartmentId> departmentIds,
        CancellationToken cancellationToken);
}