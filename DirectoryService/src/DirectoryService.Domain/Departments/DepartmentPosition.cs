using DirectoryService.Domain.Departments.ValueObjects;
using DirectoryService.Domain.Positions;
using DirectoryService.Domain.Positions.ValueObjects;

namespace DirectoryService.Domain.Departments;

public sealed class DepartmentPosition
{
    public DepartmentPosition(DepartmentPositionId id, DepartmentId departmentId, PositionId positionId)
    {
        Id = id;
        DepartmentId = departmentId;
        PositionId = positionId;
    }

    // EF Core
    private DepartmentPosition() { }

    public DepartmentPositionId Id { get; }

    public DepartmentId DepartmentId { get; }

    public PositionId PositionId { get; }
}