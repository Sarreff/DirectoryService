namespace DirectoryService.Contracts.Departments;

public record GetChildDepartmentsRequest
{
    public int? Page { get; init; }

    public int? Size { get; init; }
}