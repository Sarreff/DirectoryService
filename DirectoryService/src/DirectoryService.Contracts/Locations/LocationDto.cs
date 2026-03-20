namespace DirectoryService.Contracts.Locations;

public record LocationDto
{
    public Guid Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public AddressDto Address { get; init; } = null!;

    public string Timezone { get; init; } = string.Empty;

    public bool IsActive { get; init; }

    public DateTime CreatedAt { get; init; }

    public DateTime UpdatedAt { get; init; }
}