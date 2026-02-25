using CSharpFunctionalExtensions;
using DirectoryService.Application.Departments;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Departments.ValueObjects;
using DirectoryService.Infrastructure.Postgres.Configurations;
using DirectoryService.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace DirectoryService.Infrastructure.Postgres.Repositories;

public class DepartmentsEfCoreRepository : IDepartmentRepository
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
                        "Unique name violation for department '{Name}'",
                        department.Name.Value);
                }

                return DepartmentErrors.NameConflict(department.Name.Value);
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
            Department? department = await _context.Departments.
                Where(d => d.Id == id)
                .FirstOrDefaultAsync(cancellationToken);

            if (department is not null)
            {
                return department;
            }

            _logger.LogError("Department with id {Id} not found", departmentId);
            return GeneralErrors.NotFound(departmentId);
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

    public Task<Result<Guid, Error>> DeleteAsync(Guid departmentId, CancellationToken cancellationToken)
        => throw new NotImplementedException();

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