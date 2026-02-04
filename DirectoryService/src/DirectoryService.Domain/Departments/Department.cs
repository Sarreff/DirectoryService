using DirectoryService.Domain.Departments.ValueObjects;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Positions;
using Name = DirectoryService.Domain.Departments.ValueObjects.Name;
using Path = DirectoryService.Domain.Departments.ValueObjects.Path;

namespace DirectoryService.Domain.Departments;

public sealed class Department
{
    private readonly List<DepartmentLocation> _departmentLocations = [];
    private readonly List<DepartmentPosition> _departmentPositions = [];

    public Department(
        DepartmentId id,
        Name name,
        Identifier identifier,
        DepartmentId? parentId,
        Path path,
        short depth,
        bool isActive)
    {
        Id = id;
        Name = name;
        Identifier = identifier;
        ParentId = parentId;
        Path = path;
        Depth = depth;
        IsActive = isActive;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    // EF Core
    private Department() { }

    public DepartmentId Id { get; }

    public Name Name { get; private set; }

    public Identifier Identifier { get; private set; }

    public DepartmentId? ParentId { get; private set; }

    public Path Path { get; private set; }

    public short Depth { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public IReadOnlyList<DepartmentLocation> DepartmentLocations => _departmentLocations;

    public IReadOnlyList<DepartmentPosition> DepartmentPositions => _departmentPositions;

    public void AddLocation(Location location)
    {
        var newDepartmentLocation = new DepartmentLocation(new DepartmentLocationId(Guid.NewGuid()), Id, location.Id);
        _departmentLocations.Add(newDepartmentLocation);
    }

    public void AddPosition(Position position)
    {
        var newDepartmentPosition = new DepartmentPosition(new DepartmentPositionId(Guid.NewGuid()), Id, position.Id);
        _departmentPositions.Add(newDepartmentPosition);
    }
}