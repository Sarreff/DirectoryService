using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Application.Locations;
using DirectoryService.Application.Positions;
using DirectoryService.Application.Validation;
using DirectoryService.Contracts.Departments;
using DirectoryService.Domain.Departments.ValueObjects;
using DirectoryService.Shared;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Departments.SoftDeleteDepartment;

public class SoftDeleteDepartmentHandler : ICommandHandler<SoftDeletedDepartmentDto, SoftDeleteDepartmentCommand>
{
    private readonly IDepartmentsRepository _departmentsRepository;
    private readonly ILocationsRepository _locationsRepository;
    private readonly IPositionsRepository _positionsRepository;
    private readonly IValidator<SoftDeleteDepartmentCommand> _validator;
    private readonly ITransactionManager _transactionManager;
    private readonly ILogger<SoftDeleteDepartmentHandler> _logger;

    public SoftDeleteDepartmentHandler(
        IDepartmentsRepository departmentsRepository,
        ILocationsRepository locationsRepository,
        IPositionsRepository positionsRepository,
        IValidator<SoftDeleteDepartmentCommand> validator,
        ITransactionManager transactionManager,
        ILogger<SoftDeleteDepartmentHandler> logger)
    {
        _departmentsRepository = departmentsRepository;
        _locationsRepository = locationsRepository;
        _positionsRepository = positionsRepository;
        _validator = validator;
        _transactionManager = transactionManager;
        _logger = logger;
    }

    public async Task<Result<SoftDeletedDepartmentDto, Errors>> Handle(
        SoftDeleteDepartmentCommand deleteCommand,
        CancellationToken cancellationToken)
    {
        ValidationResult validationResult = await _validator.ValidateAsync(deleteCommand, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToError();
        }

        var transactionScopeResult = await _transactionManager.BeginTransactionAsync(cancellationToken);
        if (transactionScopeResult.IsFailure)
        {
            return transactionScopeResult.Error.ToErrors();
        }

        using var transactionScope = transactionScopeResult.Value;

        try
        {
            var departmentResult = await _departmentsRepository
                .GetByIdWithLockAsync(new DepartmentId(deleteCommand.DepartmentId), cancellationToken);
            if (departmentResult.IsFailure)
            {
                transactionScope.Rollback();
                return departmentResult.Error.ToErrors();
            }

            var departmentToDelete = departmentResult.Value;
            string oldPath = departmentToDelete.Path.Value;
            string newPath = departmentToDelete.Path.AddDeletedPrefix().Value;
            await _departmentsRepository.LockDescendantsAsync(oldPath, cancellationToken);

            if (!departmentToDelete.IsActive)
            {
                transactionScope.Rollback();
                return DepartmentErrors.OperationCancelled().ToErrors();
            }

            departmentToDelete.Deactivate();

            var locationsResult = await _locationsRepository
                .DeactivateLocationsAsync(departmentToDelete.Id, cancellationToken);
            if (locationsResult.IsFailure)
            {
                transactionScope.Rollback();
                return locationsResult.Error.ToErrors();
            }

            var positionsResult = await _positionsRepository
                .DeactivatePositionsAsync(departmentToDelete.Id, cancellationToken);
            if (positionsResult.IsFailure)
            {
                transactionScope.Rollback();
                return positionsResult.Error.ToErrors();
            }

            await _departmentsRepository.SoftDeleteDepartmentSubtreeAsync(departmentToDelete.Path, cancellationToken);

            var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);
            if (saveResult.IsFailure)
            {
                transactionScope.Rollback();
                return saveResult.Error.ToErrors();
            }

            var commitedResult = transactionScope.Commit();
            if (commitedResult.IsFailure)
                return commitedResult.Error.ToErrors();

            _logger.LogInformation(
                "Department with id {DepartmentId} has been soft-deleted",
                departmentToDelete.Id.Value);

            return new SoftDeletedDepartmentDto
            {
                Id = departmentToDelete.Id.Value,
                Name = departmentToDelete.Name.Value,
                Identifier = departmentToDelete.Identifier.Value,
                Path = newPath,
                IsActive = departmentToDelete.IsActive,
                CreatedAt = departmentToDelete.CreatedAt,
                DeletedAt = departmentToDelete.DeletedAt,
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unexpected error while soft-deleting department with id {Id}", deleteCommand.DepartmentId);
            transactionScope.Rollback();
            return GeneralErrors
                .Failure($"Unexpected error while soft-deleting department with id {deleteCommand.DepartmentId}")
                .ToErrors();
        }
    }
}