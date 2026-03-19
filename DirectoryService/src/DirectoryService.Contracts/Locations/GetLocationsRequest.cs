namespace DirectoryService.Contracts.Locations;

public record GetLocationsRequest
{
    public IReadOnlyList<Guid>? DepartmentIds { get; init; }

    public string? SearchName { get; init; }

    public bool? IsActive { get; init; }

    public int? Page { get; init; } = 1;

    public int? PageSize { get; init; } = 20;

    public string? SortBy { get; init; } = "name";

    public string? SortByOrder { get; init; } = "asc";
}