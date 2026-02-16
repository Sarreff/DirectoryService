using CSharpFunctionalExtensions;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.DepartmentLocations.ValueObjects;
using DirectoryService.Domain.DepartmentPositions;
using DirectoryService.Domain.DepartmentPositions.ValueObjects;
using DirectoryService.Domain.Departments.ValueObjects;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Positions;
using DirectoryService.Shared;
using Name = DirectoryService.Domain.Departments.ValueObjects.Name;
using Path = DirectoryService.Domain.Departments.ValueObjects.Path;

namespace DirectoryService.Domain.Departments;

public sealed class Department
{
    private readonly List<Department> _childrenDepartments = [];
    private readonly List<DepartmentLocation> _departmentLocations = [];
    private readonly List<DepartmentPosition> _departmentPositions = [];

    private Department(
        DepartmentId id,
        Name name,
        Identifier identifier,
        DepartmentId? parentId,
        Path path,
        short depth,
        IEnumerable<DepartmentLocation> departmentLocations)
    {
        Id = id;
        Name = name;
        Identifier = identifier;
        ParentId = parentId;
        Path = path;
        Depth = depth;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        _departmentLocations = departmentLocations.ToList();
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

    public IReadOnlyList<Department> ChildrenDepartments => _childrenDepartments;

    public IReadOnlyList<DepartmentLocation> DepartmentLocations => _departmentLocations;

    public IReadOnlyList<DepartmentPosition> DepartmentPositions => _departmentPositions;

    public void AddLocation(Location location)
    {
        // добавить проверку на дубли
        var newDepartmentLocation = new DepartmentLocation(new DepartmentLocationId(Guid.NewGuid()), Id, location.Id);
        _departmentLocations.Add(newDepartmentLocation);
    }

    public void AddPosition(Position position)
    {
        // добавить проверку на дубли
        var newDepartmentPosition = new DepartmentPosition(new DepartmentPositionId(Guid.NewGuid()), Id, position.Id);
        _departmentPositions.Add(newDepartmentPosition);
    }

    public static Result<Department, Error> CreateParent(
        Name name,
        Identifier identifier,
        IEnumerable<DepartmentLocation> departmentLocations,
        DepartmentId? departmentId = null)
    {
        var departmentLocationsList = departmentLocations.ToList();

        if (departmentLocationsList.Count == 0)
        {
            return Error.Validation(
                "department.location.length",
                "Department locations must contain at least one location",
                "department.location");
        }

        var path = Path.CreateParent(identifier);

        return new Department(
            departmentId ?? new DepartmentId(Guid.NewGuid()),
            name,
            identifier,
            null,
            path,
            0,
            departmentLocationsList);
    }

    public static Result<Department, Error> CreateChild(
        Name name,
        Identifier identifier,
        Department parent,
        IEnumerable<DepartmentLocation> departmentLocations,
        DepartmentId? departmentId = null)
    {
        var departmentLocationsList = departmentLocations.ToList();

        if (departmentLocationsList.Count == 0)
        {
            return Error.Validation(
                "department.location.length",
                "Department locations must contain at least one location",
                "department.location");
        }

        var path = parent.Path.CreateChild(identifier);

        return new Department(
            departmentId ?? new DepartmentId(Guid.NewGuid()),
            name,
            identifier,
            parent.ParentId,
            path,
            (short)(parent.Depth + 1),
            departmentLocationsList);
    }
}