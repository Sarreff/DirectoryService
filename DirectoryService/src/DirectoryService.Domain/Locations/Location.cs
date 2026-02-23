using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations.ValueObjects;

namespace DirectoryService.Domain.Locations;

public sealed class Location
{
    private readonly List<DepartmentLocation> _departmentLocations = [];

    public Location(LocationId id, Name name, Address address, Timezone timezone, bool isActive)
    {
        Id = id;
        Name = name;
        Address = address;
        Timezone = timezone;
        IsActive = isActive;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        FullPath = CreateFullPath(address);
    }

    private static string CreateFullPath(Address address) =>
        $"{Normalize(address.Country)}, {Normalize(address.City)}, {Normalize(address.Street)}, {address.Building}, {address.OfficeNumber}";

    private static string Normalize(string value) =>
        value?.Trim().ToLowerInvariant() ?? string.Empty;

    // EF Core
    private Location() { }

    public LocationId Id { get; private set; }

    public Name Name { get; private set; }

    public Address Address { get; private set; }

    public string FullPath { get; private set; }

    public Timezone Timezone { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public IReadOnlyList<DepartmentLocation> DepartmentLocations => _departmentLocations;
}