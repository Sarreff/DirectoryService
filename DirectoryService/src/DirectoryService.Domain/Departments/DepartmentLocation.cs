using DirectoryService.Domain.Locations;

namespace DirectoryService.Domain.Departments;

public sealed class DepartmentLocation
{
    public DepartmentLocation(Guid id, Department department, Location location)
    {
        Id = id;
        DepartmentId = department.Id;
        Department = department;
        LocationId = location.Id;
        Location = location;
    }

    // EF Core
    private DepartmentLocation() { }

    public Guid Id { get; }

    public Guid DepartmentId { get; }

    public Department Department { get; private set; }

    public Guid LocationId { get; set; }

    public Location Location { get; private set; }
}