using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.DepartmentLocations.ValueObjects;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Departments.ValueObjects;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Locations.ValueObjects;
using DirectoryService.Domain.Positions;
using DirectoryService.Domain.Positions.ValueObjects;
using DirectoryService.Infrastructure.Postgres;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using DepartmentName = DirectoryService.Domain.Departments.ValueObjects.Name;
using LocationName = DirectoryService.Domain.Locations.ValueObjects.Name;
using PositionName = DirectoryService.Domain.Positions.ValueObjects.Name;

namespace DirectoryService.IntegrationTests.Infrastructure;

public class DirectoryBaseTests : IClassFixture<DirectoryTestWebFactory>, IAsyncLifetime
{
    private readonly DirectoryTestWebFactory _factory;
    private readonly Func<Task> _resetDatabase;

    protected DirectoryBaseTests(DirectoryTestWebFactory factory)
    {
        _factory = factory;
        _resetDatabase = factory.ResetDatabaseAsync;
    }

    public static string GenerateRandomLetters(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        return new string(
            Enumerable.Range(0, length)
                .Select(_ => chars[Random.Shared.Next(chars.Length)])
                .ToArray());
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await _resetDatabase();
    }

    public async Task<T> ExecuteInDb<T>(Func<DirectoryServiceDbContext, Task<T>> action)
    {
        await using var scope = _factory.Services.CreateAsyncScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<DirectoryServiceDbContext>();

        return await action(dbContext);
    }

    public async Task ExecuteInDb(Func<DirectoryServiceDbContext, Task> action)
    {
        await using var scope = _factory.Services.CreateAsyncScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<DirectoryServiceDbContext>();

        await action(dbContext);
    }

    protected async Task<LocationId> CreateValidLocation()
    {
        string randomSuffix = Guid.NewGuid().ToString()[..8];

        return await ExecuteInDb(async dbContext =>
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

    protected async Task<LocationId> CreateInactiveLocation()
    {
        string randomSuffix = Guid.NewGuid().ToString()[..8];

        return await ExecuteInDb(async dbContext =>
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

    protected LocationId CreateNotExistLocation()
    {
        return new LocationId(Guid.NewGuid());
    }

    protected async Task<PositionId> CreateValidPosition(IEnumerable<DepartmentId> departmentIds)
    {
        string randomSuffix = Guid.NewGuid().ToString()[..8];

        return await ExecuteInDb(async dbContext =>
        {
            var name = PositionName.Create($"Position {randomSuffix}").Value;
            var description = Description.Create($"Description for {name}").Value;

            var position = Position.Create(name, description, departmentIds);

            dbContext.Positions.Add(position.Value);
            await dbContext.SaveChangesAsync();

            return position.Value.Id;
        });
    }

    protected async Task<Department> CreateValidParentDepartment(
        string name,
        string identifier,
        IEnumerable<LocationId> locationIds,
        bool isActive = true)
    {
        List<LocationId> locationIdsList = locationIds.ToList();

        return await ExecuteInDb(async dbContext =>
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

            if (!isActive)
                department.Value.Deactivate();

            dbContext.Departments.Add(department.Value);

            await dbContext.SaveChangesAsync();

            return department.Value;
        });
    }

    protected async Task<Department> CreateValidChildDepartment(
        string name,
        string identifier,
        Department parentDepartment,
        IEnumerable<LocationId> locationIds,
        bool isActive = true)
    {
        List<LocationId> locationIdsList = locationIds.ToList();

        return await ExecuteInDb(async dbContext =>
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

            if (!isActive)
                department.Value.Deactivate();

            dbContext.Departments.Add(department.Value);

            await dbContext.SaveChangesAsync();

            return department.Value;
        });
    }

    protected DepartmentId CreateNotExistDepartment()
    {
        return new DepartmentId(Guid.NewGuid());
    }

    protected async Task<DateTime> GetDepartmentUpdatedAt(DepartmentId departmentId)
    {
        return await ExecuteInDb(async dbContext =>
        {
            var dep = await dbContext.Departments
                .FirstAsync(d => d.Id == departmentId);

            return dep.UpdatedAt;
        });
    }

    protected async Task<T> ExecuteHandler<TService, T>(Func<TService, Task<T>> action)
        where TService : notnull
    {
        await using var scope = _factory.Services.CreateAsyncScope();

        var sut = scope.ServiceProvider.GetRequiredService<TService>();

        return await action(sut);
    }
}