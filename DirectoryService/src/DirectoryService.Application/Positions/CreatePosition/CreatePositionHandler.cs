using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Departments;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.DepartmentPositions;
using DirectoryService.Domain.DepartmentPositions.ValueObjects;
using DirectoryService.Domain.Departments.ValueObjects;
using DirectoryService.Domain.Positions;
using DirectoryService.Domain.Positions.ValueObjects;
using DirectoryService.Shared;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Name = DirectoryService.Domain.Positions.ValueObjects.Name;

namespace DirectoryService.Application.Positions.CreatePosition;

public class CreatePositionHandler : ICommandHandler<Guid, CreatePositionCommand>
{
    private readonly IPositionsRepository _positionsRepository;
    private readonly IDepartmentsRepository _departmentsRepository;
    private readonly IValidator<CreatePositionCommand> _validator;
    private readonly ILogger<CreatePositionHandler> _logger;

    public CreatePositionHandler(
        IPositionsRepository positionsRepository,
        IDepartmentsRepository departmentsRepository,
        IValidator<CreatePositionCommand> validator,
        ILogger<CreatePositionHandler> logger)
    {
        _positionsRepository = positionsRepository;
        _departmentsRepository = departmentsRepository;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<Guid, Errors>> Handle(CreatePositionCommand command, CancellationToken cancellationToken)
    {
        ValidationResult validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToError();
        }

        PositionId positionId = new(Guid.NewGuid());

        var nameResult = Name.Create(command.CreatePositionRequest.Name);
        var isNameUniqueResult = await _positionsRepository
            .IsNameUniqueAndActiveAsync(nameResult.Value, cancellationToken);
        if (isNameUniqueResult.IsFailure)
            return isNameUniqueResult.Error.ToErrors();

        var descriptionResult = Description.Create(command.CreatePositionRequest.Description);

        var departmentIds = command.CreatePositionRequest.DepartmentsId
            .Select(d => new DepartmentId(d)).ToList();
        var departmentsExistAndActiveResult = await _departmentsRepository
            .AllDepartmentsExistAndActiveAsync(departmentIds, cancellationToken);
        if (departmentsExistAndActiveResult.IsFailure)
            return departmentsExistAndActiveResult.Error.ToErrors();

        List<DepartmentPosition> departmentPositions = [];
        foreach (var departmentId in departmentIds)
        {
            var newDepartmentPosition = new DepartmentPosition(
                new DepartmentPositionId(Guid.NewGuid()),
                departmentId,
                positionId);

            departmentPositions.Add(newDepartmentPosition);
        }

        Position newPosition = new(
            positionId,
            nameResult.Value,
            descriptionResult.Value,
            departmentPositions);

        var addResult = await _positionsRepository.AddAsync(newPosition, cancellationToken);
        if (addResult.IsFailure)
            return addResult.Error.ToErrors();

        _logger.LogInformation("Position created with id {PositionId}", positionId);

        return positionId.Value;
    }
}