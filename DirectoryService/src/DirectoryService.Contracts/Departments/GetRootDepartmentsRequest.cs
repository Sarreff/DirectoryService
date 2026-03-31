namespace DirectoryService.Contracts.Departments;

public record GetRootDepartmentsRequest
{
    public int? Page { get; init; }

    public int? Size { get; init; }

    public int? Prefetch { get; init; }
}