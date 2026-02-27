using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Locations;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.DepartmentLocations.ValueObjects;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Departments.ValueObjects;
using DirectoryService.Domain.Locations.ValueObjects;
using DirectoryService.Shared;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Name = DirectoryService.Domain.Departments.ValueObjects.Name;

namespace DirectoryService.Application.Departments.CreateDepartment;

public class CreateDepartmentHandler : ICommandHandler<Guid, CreateDepartmentCommand>
{
    private readonly IDepartmentsRepository _departmentsRepository;
    private readonly ILocationsRepository _locationsRepository;
    private readonly IValidator<CreateDepartmentCommand> _validator;
    private readonly ILogger<CreateDepartmentHandler> _logger;

    public CreateDepartmentHandler(
        IDepartmentsRepository departmentsRepository,
        ILocationsRepository locationsRepository,
        IValidator<CreateDepartmentCommand> validator,
        ILogger<CreateDepartmentHandler> logger)
    {
        _departmentsRepository = departmentsRepository;
        _locationsRepository = locationsRepository;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<Guid, Errors>> Handle(CreateDepartmentCommand command, CancellationToken cancellationToken)
    {
        ValidationResult validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToError();
        }

        DepartmentId departmentId = new(Guid.NewGuid());

        var nameResult = Name.Create(command.CreateDepartmentRequest.Name);

        var identifierResult = Identifier.Create(command.CreateDepartmentRequest.Identifier);

        var locationIds = command.CreateDepartmentRequest.LocationsId
            .Select(l => new LocationId(l)).ToList();
        var locationsExistResult = await _locationsRepository.AllLocationsExistAsync(locationIds, cancellationToken);
        if (locationsExistResult.IsFailure)
            return locationsExistResult.Error.ToErrors();

        List<DepartmentLocation> departmentLocations = [];
        foreach (var locationId in locationIds)
        {
            var newDepartmentLocation = new DepartmentLocation(
                new DepartmentLocationId(Guid.NewGuid()),
                departmentId,
                locationId);

            departmentLocations.Add(newDepartmentLocation);
        }

        var parentId = command.CreateDepartmentRequest.ParentId;

        Result<Department, Error> departmentResult = default;
        if (parentId is null)
        {
            departmentResult = Department.CreateParent(
                nameResult.Value,
                identifierResult.Value,
                departmentLocations,
                departmentId);
        }
        else
        {
            var departmentParent = await _departmentsRepository
                .GetByIdAsync(parentId.Value, cancellationToken);
            if (departmentParent.IsFailure)
            {
                _logger.LogError("Failed to get department {DepartmentId}", departmentId.Value);
                return departmentParent.Error.ToErrors();
            }

            departmentResult = Department.CreateChild(
                nameResult.Value,
                identifierResult.Value,
                departmentParent.Value,
                departmentLocations,
                departmentId);
        }

        if (departmentResult.IsFailure)
            return departmentResult.Error.ToErrors();

        var addResult = await _departmentsRepository.AddAsync(departmentResult.Value, cancellationToken);
        if (addResult.IsFailure)
            return addResult.Error.ToErrors();

        _logger.LogInformation("Department created with id {DepartmentId}", departmentId);

        return departmentId.Value;
    }
}