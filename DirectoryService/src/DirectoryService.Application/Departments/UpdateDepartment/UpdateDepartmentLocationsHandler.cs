using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Application.Locations;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.Locations.ValueObjects;
using DirectoryService.Shared;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Departments.UpdateDepartment;

public class UpdateDepartmentLocationsHandler : ICommandHandler<Guid, UpdateDepartmentLocationsCommand>
{
    private readonly IDepartmentsRepository _departmentsRepository;
    private readonly ILocationsRepository _locationsRepository;
    private readonly IValidator<UpdateDepartmentLocationsCommand> _validator;
    private readonly ITransactionManager _transactionManager;
    private readonly ILogger<UpdateDepartmentLocationsHandler> _logger;

    public UpdateDepartmentLocationsHandler(
        IDepartmentsRepository departmentsRepository,
        ILocationsRepository locationsRepository,
        IValidator<UpdateDepartmentLocationsCommand> validator,
        ITransactionManager transactionManager,
        ILogger<UpdateDepartmentLocationsHandler> logger)
    {
        _departmentsRepository = departmentsRepository;
        _locationsRepository = locationsRepository;
        _validator = validator;
        _transactionManager = transactionManager;
        _logger = logger;
    }

    public async Task<Result<Guid, Errors>> Handle(
        UpdateDepartmentLocationsCommand updateCommand,
        CancellationToken cancellationToken)
    {
        var transactionScopeResult = await _transactionManager.BeginTransactionAsync(cancellationToken);
        if (transactionScopeResult.IsFailure)
        {
            return transactionScopeResult.Error.ToErrors();
        }

        using var transactionScope = transactionScopeResult.Value;

        ValidationResult validationResult = await _validator.ValidateAsync(updateCommand, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToError();
        }

        var departmentResult = await _departmentsRepository
            .GetByIdAsync(updateCommand.DepartmentId, cancellationToken);
        if (departmentResult.IsFailure)
        {
            transactionScope.Rollback();
            return departmentResult.Error.ToErrors();
        }

        var department = departmentResult.Value;

        var locationIds = updateCommand.UpdateDepartmentLocationsRequest.LocationIds
            .Select(l => new LocationId(l)).ToList();
        var locationIdsResult = await _locationsRepository
            .AllLocationsExistAndActiveAsync(locationIds, cancellationToken);
        if (locationIdsResult.IsFailure)
        {
            transactionScope.Rollback();
            return locationIdsResult.Error.ToErrors();
        }

        var updateResult = department.UpdateDepartmentLocations(locationIds);
        if (updateResult.IsFailure)
            return updateResult.Error.ToErrors();

        var deleteResult = await _departmentsRepository
            .DeleteDepartmentLocationsByDepartmentId(department.Id, cancellationToken);
        if (deleteResult.IsFailure)
        {
            transactionScope.Rollback();
            return deleteResult.Error.ToErrors();
        }

        await _transactionManager.SaveChangesAsync(cancellationToken);

        var commitedResult = transactionScope.Commit();
        if (commitedResult.IsFailure)
            return commitedResult.Error.ToErrors();

        _logger.LogInformation("Department with id {DepartmentId} has been updated", department.Id.Value);

        return department.Id.Value;
    }
}