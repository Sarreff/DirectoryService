using CSharpFunctionalExtensions;
using DirectoryService.Domain.DepartmentPositions;
using DirectoryService.Domain.DepartmentPositions.ValueObjects;
using DirectoryService.Domain.Departments.ValueObjects;
using DirectoryService.Domain.Positions.ValueObjects;
using DirectoryService.Shared;
using Name = DirectoryService.Domain.Positions.ValueObjects.Name;

namespace DirectoryService.Domain.Positions;

public sealed class Position
{
    private readonly List<DepartmentPosition> _departmentPositions = [];

    public Position(
        PositionId id,
        Name name,
        Description description,
        IEnumerable<DepartmentPosition> departmentPositions)
    {
        Id = id;
        Name = name;
        Description = description;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        _departmentPositions = departmentPositions.ToList();
    }

    // EF Core
    private Position() { }

    public PositionId Id { get; }

    public Name Name { get; private set; }

    public Description Description { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public DateTime? DeletedAt { get; private set; }

    public IReadOnlyList<DepartmentPosition> DepartmentPositions => _departmentPositions;

    public static Result<Position, Error> Create(
        Name name,
        Description description,
        IEnumerable<DepartmentId> departmentIds)
    {
        PositionId positionId = new PositionId(Guid.NewGuid());

        var departmentList = departmentIds.ToList();

        if (departmentList.Count == 0)
        {
            return Error.Validation(
                "departmentId.list.length",
                "Department ids list must contain at least one department id");
        }

        List<DepartmentPosition> newDepartmentPositions = [];
        foreach (var departmentId in departmentList)
        {
            var newDP = new DepartmentPosition(
                new DepartmentPositionId(Guid.NewGuid()),
                departmentId,
                positionId);

            newDepartmentPositions.Add(newDP);
        }

        return new Position(
            positionId,
            name,
            description,
            newDepartmentPositions);
    }

    public void Deactivate()
    {
        IsActive = false;
        DeletedAt = DateTime.UtcNow;
    }
}