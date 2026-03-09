using CSharpFunctionalExtensions;
using DirectoryService.Application.Departments;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Departments.ValueObjects;
using DirectoryService.Infrastructure.Postgres.Configurations;
using DirectoryService.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using Path = DirectoryService.Domain.Departments.ValueObjects.Path;

namespace DirectoryService.Infrastructure.Postgres.Repositories;

public class DepartmentsEfCoreRepository : IDepartmentsRepository
{
    private readonly DirectoryServiceDbContext _context;
    private readonly ILogger<DepartmentsEfCoreRepository> _logger;

    public DepartmentsEfCoreRepository(DirectoryServiceDbContext context, ILogger<DepartmentsEfCoreRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<Guid, Error>> AddAsync(Department department, CancellationToken cancellationToken)
    {
        try
        {
            await _context.Departments.AddAsync(department, cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            PostgresException? pgEx = FindPostgresException(ex);

            if (pgEx?.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                if (string.Equals(
                        pgEx.ConstraintName,
                        DepartmentIndex.NAME,
                        StringComparison.InvariantCultureIgnoreCase))
                {
                    _logger.LogInformation(
                        "Unique name violation for department with name: '{Name}'",
                        department.Name.Value);

                    return DepartmentErrors.NameConflict(department.Name.Value);
                }

                if (string.Equals(
                        pgEx.ConstraintName,
                        DepartmentIndex.IDENTIFIER,
                        StringComparison.InvariantCultureIgnoreCase))
                {
                    _logger.LogInformation(
                        "Unique identifier violation for department with identifier: '{Identifier}'",
                        department.Identifier.Value);

                    return DepartmentErrors.IdentifierConflict(department.Identifier.Value);
                }
            }

            _logger.LogError(
                ex,
                "Database update error while creating department with name {Name}",
                department.Name.Value);

            return DepartmentErrors.DatabaseError();
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogError(
                ex,
                "Operation was cancelled while creating department with name {Name}", department.Name.Value);

            return DepartmentErrors.OperationCancelled();
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unexpected error while creating department with name {Name}", department.Name.Value);

            return DepartmentErrors.DatabaseError();
        }

        return department.Id.Value;
    }

    public Task<Result<Guid, Error>> UpdateAsync(Department department, CancellationToken cancellationToken)
        => throw new NotImplementedException();

    public async Task<Result<Department, Error>> GetByIdAsync(Guid departmentId, CancellationToken cancellationToken)
    {
        try
        {
            var id = new DepartmentId(departmentId);
            Department? department = await _context.Departments
                .Where(d => d.Id == id && d.IsActive)
                .FirstOrDefaultAsync(cancellationToken);

            if (department is not null)
            {
                return department;
            }

            _logger.LogError("Department with id {Id} not found or inactive", departmentId);
            return DepartmentErrors.DepartmentsNotFoundOrInactive();
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogError(
                ex,
                "Operation was cancelled while getting department with id {Id}", departmentId);

            return DepartmentErrors.OperationCancelled();
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unexpected error while getting department with id {Id}", departmentId);

            return DepartmentErrors.DatabaseError();
        }
    }

    public async Task<Result<Department, Error>> GetByIdWithDepartmentLocationsAsync(
        Guid departmentId,
        CancellationToken cancellationToken)
    {
        try
        {
            var id = new DepartmentId(departmentId);
            Department? department = await _context.Departments
                .Where(d => d.Id == id && d.IsActive)
                .Include(d => d.DepartmentLocations)
                .FirstOrDefaultAsync(cancellationToken);

            if (department is not null)
                return department;

            _logger.LogError("Department with id {Id} not found or inactive", departmentId);
            return DepartmentErrors.DepartmentsNotFoundOrInactive();
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogError(
                ex,
                "Operation was cancelled while getting department with id {Id}", departmentId);

            return DepartmentErrors.OperationCancelled();
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unexpected error while getting department with id {Id}", departmentId);

            return DepartmentErrors.DatabaseError();
        }
    }

    public async Task<Result<Department, Error>> GetByIdWithLockAsync(
        DepartmentId departmentId,
        CancellationToken cancellationToken)
    {
        var department = await _context.Departments
            .FromSqlInterpolated($"""
                                  SELECT *
                                  FROM departments
                                  WHERE id = {departmentId.Value}
                                    AND is_active = TRUE
                                  FOR UPDATE
                                  """)
            .FirstOrDefaultAsync(cancellationToken);

        if (department is not null)
        {
            return department;
        }

        _logger.LogError("Department with id {Id} not found or inactive", departmentId);
        return DepartmentErrors.DepartmentsNotFoundOrInactive();

    }

    public async Task MoveDepartmentSubtreeAsync(
        Path oldPath,
        Path newPath,
        Guid? newParentId,
        CancellationToken cancellationToken)
    {
        string oldPathStr = oldPath.Value;
        string newPathStr = newPath.Value;

        await _context.Database
            .ExecuteSqlInterpolatedAsync(
                $"""
                 UPDATE departments
                     SET
                         parent_id = {newParentId},
                         path = {newPathStr}::ltree,
                         depth = nlevel({newPathStr}::ltree) - 1,
                         updated_at = NOW()
                     WHERE path = {oldPathStr}::ltree;
                     
                 UPDATE departments
                     SET
                         path = {newPathStr}::ltree || subpath(path, nlevel({oldPathStr}::ltree)),
                         depth = nlevel({newPathStr}::ltree || subpath(path, nlevel({oldPathStr}::ltree))) - 1,
                         updated_at = NOW()
                     WHERE path <@ {oldPathStr}::ltree
                       AND path != {oldPathStr}::ltree;
                 """,
                cancellationToken);
    }

    public async Task LockDescendantsAsync(string oldPath, CancellationToken cancellationToken)
    {
        await _context.Database.ExecuteSqlInterpolatedAsync(
            $"""
             SELECT *
             FROM departments
             WHERE path <@ {oldPath}::ltree 
                AND path != {oldPath}::ltree
             ORDER BY path
             FOR UPDATE
             """,
            cancellationToken);
    }

    public Task<Result<Guid, Error>> DeleteAsync(Guid departmentId, CancellationToken cancellationToken)
        => throw new NotImplementedException();

    public async Task<UnitResult<Error>> DeleteDepartmentLocationsByDepartmentId(
        DepartmentId departmentId,
        CancellationToken cancellationToken)
    {
        try
        {
            await _context.DepartmentLocations
                .Where(ld => ld.DepartmentId == departmentId)
                .ExecuteDeleteAsync(cancellationToken);

            return UnitResult.Success<Error>();
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogError(
                ex,
                "Operation was cancelled while deleting DepartmentLocations by department with id {Id}",
                departmentId);

            return DepartmentErrors.OperationCancelled();
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unexpected error while deleting DepartmentLocations by department with id {Id}",
                departmentId);

            return DepartmentErrors.DatabaseError();
        }
    }

    public async Task<Result<bool, Error>> AllDepartmentsExistAndActiveAsync(
        IEnumerable<DepartmentId> departmentIds,
        CancellationToken cancellationToken)
    {
        try
        {
            int existingCount = await _context.Departments
                .CountAsync(
                    d =>
                        departmentIds.Contains(d.Id) &&
                        d.IsActive,
                    cancellationToken);

            if (existingCount == departmentIds.Count())
            {
                return true;
            }

            _logger.LogError("Some departments were not found or inactive in the database");
            return DepartmentErrors.DepartmentsNotFoundOrInactive();
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unexpected error while checking active departments existing in database");

            return DepartmentErrors.DatabaseError();
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