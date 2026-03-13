using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.DepartmentLocations.ValueObjects;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Departments.ValueObjects;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Locations.ValueObjects;
using Microsoft.EntityFrameworkCore;
using DepartmentName = DirectoryService.Domain.Departments.ValueObjects.Name;
using LocationName = DirectoryService.Domain.Locations.ValueObjects.Name;

namespace DirectoryService.IntegrationTests.Helpers;

public class TestDataBuilder
{
    private readonly IDbExecutor _db;

    public TestDataBuilder(IDbExecutor db)
    {
        _db = db;
    }

    public static string GenerateRandomLetters(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        return new string(
            Enumerable.Range(0, length)
                .Select(_ => chars[Random.Shared.Next(chars.Length)])
                .ToArray());
    }

    public async Task<LocationId> CreateValidLocation()
    {
        string randomSuffix = Guid.NewGuid().ToString()[..8];

        return await _db.ExecuteInDb(async dbContext =>
        {
            var locationId = new LocationId(Guid.NewGuid());
            var name = LocationName.Create($"Location {randomSuffix}").Value;
            var address = Address.Create(
                "UK",
                "London",
                $"Baker Street {randomSuffix}",
                221,
                1).Value;
            var timezone = Timezone.Create("Europe/London").Value;

            var location = new Location(locationId, name, address, timezone, true);

            dbContext.Locations.Add(location);
            await dbContext.SaveChangesAsync();

            return locationId;
        });
    }

    public async Task<LocationId> CreateInactiveLocation()
    {
        string randomSuffix = Guid.NewGuid().ToString()[..8];

        return await _db.ExecuteInDb(async dbContext =>
        {
            var locationId = new LocationId(Guid.NewGuid());
            var name = LocationName.Create($"Location {randomSuffix}").Value;
            var address = Address.Create(
                "UK",
                "London",
                $"Baker Street {randomSuffix}",
                221,
                1).Value;
            var timezone = Timezone.Create("Europe/London").Value;

            var location = new Location(locationId, name, address, timezone, false);

            dbContext.Locations.Add(location);
            await dbContext.SaveChangesAsync();

            return locationId;
        });
    }

    public LocationId CreateNotExistLocation()
    {
        return new LocationId(Guid.NewGuid());
    }

    public async Task<Department> CreateValidParentDepartment(
        string name,
        string identifier,
        IEnumerable<LocationId> locationIds)
    {
        List<LocationId> locationIdsList = locationIds.ToList();

        return await _db.ExecuteInDb(async dbContext =>
        {
            DepartmentId departmentId = new(Guid.NewGuid());
            var nameResult = DepartmentName.Create(name);
            var identifierResult = Identifier.Create(identifier);

            List<DepartmentLocation> departmentLocations = [];
            foreach (var locationId in locationIdsList)
            {
                var newDepartmentLocation = new DepartmentLocation(
                    new DepartmentLocationId(Guid.NewGuid()),
                    departmentId,
                    locationId);

                departmentLocations.Add(newDepartmentLocation);
            }

            var department = Department.CreateParent(
                nameResult.Value,
                identifierResult.Value,
                departmentLocations,
                departmentId);

            dbContext.Departments.Add(department.Value);

            await dbContext.SaveChangesAsync();

            return department.Value;
        });
    }

    public async Task<Department> CreateValidChildDepartment(
        string name,
        string identifier,
        Department parentDepartment,
        IEnumerable<LocationId> locationIds)
    {
        List<LocationId> locationIdsList = locationIds.ToList();

        return await _db.ExecuteInDb(async dbContext =>
        {
            DepartmentId departmentId = new(Guid.NewGuid());
            var nameResult = DepartmentName.Create(name);
            var identifierResult = Identifier.Create(identifier);

            List<DepartmentLocation> departmentLocations = [];
            foreach (var locationId in locationIdsList)
            {
                var newDepartmentLocation = new DepartmentLocation(
                    new DepartmentLocationId(Guid.NewGuid()),
                    departmentId,
                    locationId);

                departmentLocations.Add(newDepartmentLocation);
            }

            var department = Department.CreateChild(
                nameResult.Value,
                identifierResult.Value,
                parentDepartment,
                departmentLocations,
                departmentId);

            dbContext.Departments.Add(department.Value);

            await dbContext.SaveChangesAsync();

            return department.Value;
        });
    }

    public DepartmentId CreateNotExistDepartment()
    {
        return new DepartmentId(Guid.NewGuid());
    }

    public async Task<DateTime> GetDepartmentUpdatedAt(DepartmentId departmentId)
    {
        return await _db.ExecuteInDb(async dbContext =>
        {
            var dep = await dbContext.Departments
                .FirstAsync(d => d.Id == departmentId);

            return dep.UpdatedAt;
        });
    }
}