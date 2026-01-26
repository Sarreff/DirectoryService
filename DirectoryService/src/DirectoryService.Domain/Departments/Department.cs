using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Positions;

namespace DirectoryService.Domain.Departments;

public sealed class Department
{
    private readonly List<DepartmentLocation> _departmentLocations = [];
    private readonly List<DepartmentPosition> _departmentPositions = [];

    public Department(
        Guid id,
        Name name,
        Identifier identifier,
        Guid? parentId,
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

    public Guid Id { get; }

    public Name Name { get; private set; }

    public Identifier Identifier { get; private set; }

    public Guid? ParentId { get; private set; }

    public Path Path { get; private set; }

    public short Depth { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public IReadOnlyList<DepartmentLocation> DepartmentLocations => _departmentLocations;

    public IReadOnlyList<DepartmentPosition> DepartmentPositions => _departmentPositions;

    public void AddLocation(Location location)
    {
        var id = Guid.NewGuid();
        var newDepartmentLocation = new DepartmentLocation(id, this, location);
        _departmentLocations.Add(newDepartmentLocation);
    }

    public void AddPosition(Position position)
    {
        var id = Guid.NewGuid();
        var newDepartmentPosition = new DepartmentPosition(id, this, position);
        _departmentPositions.Add(newDepartmentPosition);
    }
}