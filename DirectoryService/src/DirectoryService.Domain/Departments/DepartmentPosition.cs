using DirectoryService.Domain.Positions;

namespace DirectoryService.Domain.Departments;

public sealed class DepartmentPosition
{
    public DepartmentPosition(Guid id, Department department, Position position)
    {
        Id = id;
        DepartmentId = department.Id;
        Department = department;
        PositionId = position.Id;
        Position = position;
    }

    // EF Core
    private DepartmentPosition() { }

    public Guid Id { get; }

    public Guid DepartmentId { get; }

    public Department Department { get; private set; }

    public Guid PositionId { get; }

    public Position Position { get; private set; }
}