using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Departments.ValueObjects;
using DirectoryService.Shared;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Path = DirectoryService.Domain.Departments.ValueObjects.Path;

namespace DirectoryService.Application.Departments.MoveDepartment;

public class MoveDepartmentHandler : ICommandHandler<Guid, MoveDepartmentCommand>
{
    private readonly IDepartmentsRepository _departmentsRepository;
    private readonly IValidator<MoveDepartmentCommand> _validator;
    private readonly ITransactionManager _transactionManager;
    private readonly ILogger<MoveDepartmentHandler> _logger;

    public MoveDepartmentHandler(
        IDepartmentsRepository departmentsRepository,
        IValidator<MoveDepartmentCommand> validator,
        ITransactionManager transactionManager,
        ILogger<MoveDepartmentHandler> logger)
    {
        _departmentsRepository = departmentsRepository;
        _validator = validator;
        _transactionManager = transactionManager;
        _logger = logger;
    }

    public async Task<Result<Guid, Errors>> Handle(
        MoveDepartmentCommand moveCommand,
        CancellationToken cancellationToken)
    {
        var transactionScopeResult = await _transactionManager.BeginTransactionAsync(cancellationToken);
        if (transactionScopeResult.IsFailure)
        {
            return transactionScopeResult.Error.ToErrors();
        }

        using var transactionScope = transactionScopeResult.Value;

        ValidationResult validationResult = await _validator.ValidateAsync(moveCommand, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToError();
        }

        // Получаем перемещаемый департамент
        var departmentResult = await _departmentsRepository
            .GetByIdWithLockAsync(new DepartmentId(moveCommand.DepartmentId), cancellationToken);
        if (departmentResult.IsFailure)
        {
            transactionScope.Rollback();
            return departmentResult.Error.ToErrors();
        }

        Department department = departmentResult.Value;

        // Получаем родительский департамент (если есть)
        Department? parentDepartment = null;
        if (moveCommand.Request.ParentId is not null)
        {
            var parentResult = await _departmentsRepository
                .GetByIdWithLockAsync(new DepartmentId(moveCommand.Request.ParentId.Value), cancellationToken);
            if (parentResult.IsFailure)
            {
                transactionScope.Rollback();
                return parentResult.Error.ToErrors();
            }

            parentDepartment = parentResult.Value;
        }

        // Блокируем дочерние подразделения
        await _departmentsRepository.LockDescendantsAsync(department.Path.Value, cancellationToken);

        // Департамент не должен ссылаться на дочернее подразделение
        if (parentDepartment is not null && parentDepartment.Path.IsDescendantOf(department.Path))
        {
            transactionScope.Rollback();
            return DepartmentErrors.DepartmentCycleError().ToErrors();
        }

        // Формируем новый путь
        Path newPath = parentDepartment is null ?
            Path.CreateParent(department.Identifier)
            : parentDepartment.Path.CreateChild(department.Identifier);
        Path oldPath = department.Path;

        // Родитель уже установлен (одинаковый), просто выходим
        if (department.Path == newPath)
        {
            return department.Id.Value;
        }

        // Переносим департамент и все дочерние подразделения
        await _departmentsRepository.MoveDepartmentSubtreeAsync(
            oldPath,
            newPath,
            parentDepartment?.Id.Value,
            cancellationToken);

        await _transactionManager.SaveChangesAsync(cancellationToken);

        var commitedResult = transactionScope.Commit();
        if (commitedResult.IsFailure)
            return commitedResult.Error.ToErrors();

        _logger.LogInformation(
            "Department {DepartmentId} moved from {OldPath} to {NewPath}",
            department.Id.Value,
            oldPath.Value,
            newPath.Value);

        return department.Id.Value;
    }
}