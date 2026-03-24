namespace DirectoryService.Contracts.Departments;

public record DepartmentWithPositionCountDto
{
    public Guid Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string Identifier { get; init; } = string.Empty;

    public Guid? ParentId { get; init; }

    public bool IsActive { get; init; }

    public int PositionCount { get; init; }
}