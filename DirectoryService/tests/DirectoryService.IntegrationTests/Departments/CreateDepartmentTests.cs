using CSharpFunctionalExtensions;
using DirectoryService.Application.Departments.CreateDepartment;
using DirectoryService.Contracts.Departments;
using DirectoryService.Domain.Departments.ValueObjects;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Locations.ValueObjects;
using DirectoryService.IntegrationTests.Infrastructure;
using DirectoryService.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Name = DirectoryService.Domain.Locations.ValueObjects.Name;

namespace DirectoryService.IntegrationTests.Departments;

public class CreateDepartmentTests : DirectoryBaseTests
{
    public CreateDepartmentTests(DirectoryTestWebFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task CreateDepartment_with_valid_location_should_succeed()
    {
        // arrange
        LocationId locationId = await CreateValidLocation();
        var cancellationToken = CancellationToken.None;

        // act
        Result<Guid, Errors> result = await ExecuteHandler((sut) =>
        {
            var command =
                new CreateDepartmentCommand(new CreateDepartmentRequest(
                    "Department",
                    "department",
                    null,
                    [locationId.Value]));

            return sut.Handle(command, cancellationToken);
        });

        // assert
        await AssertDepartmentCreated(result, 1, cancellationToken);
    }

    [Fact]
    public async Task CreateDepartment_with_several_valid_locations_should_succeed()
    {
        // arrange
        LocationId locationId1 = await CreateValidLocation();
        LocationId locationId2 = await CreateValidLocation();
        LocationId locationId3 = await CreateValidLocation();
        var cancellationToken = CancellationToken.None;

        // act
        var result = await ExecuteHandler((sut) =>
        {
            var command =
                new CreateDepartmentCommand(new CreateDepartmentRequest(
                    "Department",
                    "department",
                    null,
                    [locationId1.Value, locationId2.Value, locationId3.Value]));

            return sut.Handle(command, cancellationToken);
        });

        // assert
        await AssertDepartmentCreated(result, 3, cancellationToken);
    }

    [Fact]
    public async Task CreateDepartment_invalid_name_should_fail()
    {
        // arrange
        LocationId locationId = await CreateValidLocation();
        var cancellationToken = CancellationToken.None;

        // act
        Result<Guid, Errors> result = await ExecuteHandler((sut) =>
        {
            var command =
                new CreateDepartmentCommand(new CreateDepartmentRequest(
                    " ",
                    "department",
                    null,
                    [locationId.Value]));

            return sut.Handle(command, cancellationToken);
        });

        // assert
        await AssertDepartmentDidNotCreated(result, ErrorType.VALIDATION, cancellationToken);
    }

    [Fact]
    public async Task CreateDepartment_with_inactive_location_should_fail()
    {
        // arrange
        LocationId locationId = await CreateInactiveLocation();
        var cancellationToken = CancellationToken.None;

        // act
        var result = await ExecuteHandler((sut) =>
        {
            var command =
                new CreateDepartmentCommand(new CreateDepartmentRequest(
                    "Department",
                    "department",
                    null,
                    [locationId.Value]));

            return sut.Handle(command, cancellationToken);
        });

        // assert
        await AssertDepartmentDidNotCreated(result, ErrorType.NOT_FOUND, cancellationToken);
    }

    [Fact]
    public async Task CreateDepartment_location_not_found_should_fail()
    {
        // arrange
        LocationId locationId = CreateNotExistLocation();
        var cancellationToken = CancellationToken.None;

        // act
        var result = await ExecuteHandler((sut) =>
        {
            var command =
                new CreateDepartmentCommand(new CreateDepartmentRequest(
                    "Department",
                    "department",
                    null,
                    [locationId.Value]));

            return sut.Handle(command, cancellationToken);
        });

        // assert
        await AssertDepartmentDidNotCreated(result, ErrorType.NOT_FOUND, cancellationToken);
    }

    [Fact]
    public async Task CreateDepartment_parent_not_found_should_fail()
    {
        // arrange
        DepartmentId parentId = new(Guid.NewGuid());
        LocationId locationId = await CreateValidLocation();
        var cancellationToken = CancellationToken.None;

        // act
        var result = await ExecuteHandler((sut) =>
        {
            var command =
                new CreateDepartmentCommand(new CreateDepartmentRequest(
                    "Department",
                    "department",
                    parentId.Value,
                    [locationId.Value]));

            return sut.Handle(command, cancellationToken);
        });

        // assert
        await AssertDepartmentDidNotCreated(result, ErrorType.NOT_FOUND, cancellationToken);
    }

    private async Task AssertDepartmentCreated(
        Result<Guid, Errors> result,
        int expectedLocationCount,
        CancellationToken cancellationToken)
    {
        await ExecuteInDb(async dbContext =>
        {
            var department = await dbContext.Departments
                .Include(d => d.DepartmentLocations)
                .FirstAsync(d => d.Id == new DepartmentId(result.Value), cancellationToken);

            Assert.True(result.IsSuccess);
            Assert.NotEqual(Guid.Empty, result.Value);

            Assert.NotNull(department);
            Assert.Equal(result.Value, department.Id.Value);

            Assert.Equal("Department", department.Name.Value);
            Assert.Equal("department", department.Identifier.Value);

            Assert.Equal(expectedLocationCount, department.DepartmentLocations.Count);
        });
    }

    private async Task AssertDepartmentDidNotCreated(
        Result<Guid, Errors> result,
        ErrorType type,
        CancellationToken cancellationToken)
    {
        await ExecuteInDb(async dbContext =>
        {
            int departmentsCount = await dbContext.Departments.CountAsync(cancellationToken);

            Assert.True(result.IsFailure);
            Assert.NotEmpty(result.Error);
            Assert.Equal(0, departmentsCount);
            Assert.Contains(result.Error, e => e.Type == type);
        });
    }

    private async Task<LocationId> CreateValidLocation()
    {
        string randomSuffix = Guid.NewGuid().ToString()[..8];

        return await ExecuteInDb(async dbContext =>
        {
            LocationId locationId = new(Guid.NewGuid());
            var location = new Location(
                locationId,
                Name.Create($"Location {randomSuffix}").Value,
                Address.Create(
                    "UK",
                    "London",
                    $"Baker Street {randomSuffix}",
                    221,
                    1).Value,
                Timezone.Create("Europe/London").Value,
                true);

            dbContext.Locations.Add(location);
            await dbContext.SaveChangesAsync();

            return locationId;
        });
    }

    private async Task<LocationId> CreateInactiveLocation()
    {
        string randomSuffix = Guid.NewGuid().ToString()[..8];

        return await ExecuteInDb(async dbContext =>
        {
            LocationId locationId = new(Guid.NewGuid());
            var location = new Location(
                locationId,
                Name.Create($"Location {randomSuffix}").Value,
                Address.Create(
                    "UK",
                    "London",
                    $"Baker Street {randomSuffix}",
                    221,
                    1).Value,
                Timezone.Create("Europe/London").Value,
                false);

            dbContext.Locations.Add(location);
            await dbContext.SaveChangesAsync();

            return locationId;
        });
    }

    private LocationId CreateNotExistLocation()
    {
        return new LocationId(Guid.NewGuid());
    }

    private async Task<T> ExecuteHandler<T>(Func<CreateDepartmentHandler, Task<T>> action)
    {
        await using var scope = Services.CreateAsyncScope();

        var sut = scope.ServiceProvider.GetRequiredService<CreateDepartmentHandler>();

        return await action(sut);
    }
}