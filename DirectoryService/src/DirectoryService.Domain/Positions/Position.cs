using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Positions.ValueObjects;

namespace DirectoryService.Domain.Positions;

public sealed class Position
{
    private readonly List<DepartmentPosition> _departmentPositions = [];

    public Position(PositionId id, Name name, Description description, bool isActive)
    {
        Id = id;
        Name = name;
        Description = description;
        IsActive = isActive;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    // EF Core
    private Position() { }

    public PositionId Id { get; }

    public Name Name { get; private set; }

    public Description Description { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public IReadOnlyList<DepartmentPosition> DepartmentPositions => _departmentPositions;
}