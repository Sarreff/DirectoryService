using CSharpFunctionalExtensions;
using DirectoryService.Domain.Departments;
using DirectoryService.Shared;

namespace DirectoryService.Application.Departments;

public interface IDepartmentRepository
{
    Task<Result<Guid, Error>> AddAsync(Department department, CancellationToken cancellationToken);

    Task<Result<Guid, Error>> UpdateAsync(Department department, CancellationToken cancellationToken);

    Task<Result<Department, Error>> GetByIdAsync(Guid departmentId, CancellationToken cancellationToken);

    Task<Result<Guid, Error>> DeleteAsync(Guid departmentId, CancellationToken cancellationToken);
}