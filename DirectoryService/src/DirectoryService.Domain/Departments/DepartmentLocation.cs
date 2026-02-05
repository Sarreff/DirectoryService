using DirectoryService.Domain.Departments.ValueObjects;
using DirectoryService.Domain.Locations.ValueObjects;

namespace DirectoryService.Domain.Departments;

public sealed class DepartmentLocation
{
    public DepartmentLocation(DepartmentLocationId id, DepartmentId departmentId, LocationId locationId)
    {
        Id = id;
        DepartmentId = departmentId;
        LocationId = locationId;
    }

    // EF Core
    private DepartmentLocation() { }

    public DepartmentLocationId Id { get; }

    public DepartmentId DepartmentId { get; }

    public LocationId LocationId { get; }
}