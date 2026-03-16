using CSharpFunctionalExtensions;
using DirectoryService.Application.Departments.UpdateDepartment;
using DirectoryService.Contracts.Departments;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Departments.ValueObjects;
using DirectoryService.Domain.Locations.ValueObjects;
using DirectoryService.IntegrationTests.Infrastructure;
using DirectoryService.Shared;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.IntegrationTests.Departments.UpdateLocations;

public class UpdateDepartmentLocationsTests : DirectoryBaseTests
{
    public UpdateDepartmentLocationsTests(DirectoryTestWebFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task UpdateDepartment_with_valid_location_should_succeed()
    {
        // arrange
        LocationId locationId = await CreateValidLocation();

        Department departmentToUpdate = await CreateValidParentDepartment(
            "Department",
            "department",
            [locationId]);

        LocationId locationIdForUpdate = await CreateValidLocation();

        CancellationToken cancellationToken = CancellationToken.None;

        // act
        Result<Guid, Errors> result =
            await ExecuteHandler<UpdateDepartmentLocationsHandler, Result<Guid, Errors>>(
                (sut) =>
            {
                var command =
                    new UpdateDepartmentLocationsCommand(
                        departmentToUpdate.Id.Value,
                        new UpdateDepartmentLocationsRequest([locationIdForUpdate.Value]));

                return sut.Handle(command, cancellationToken);
            });

        // assert
        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);

        await ExecuteInDb(async dbContext =>
        {
            var department = await dbContext.Departments
                .Include(d => d.DepartmentLocations)
                .FirstAsync(d => d.Id == new DepartmentId(result.Value), cancellationToken);

            Assert.NotNull(department);
            Assert.Equal(result.Value, department.Id.Value);

            var oldLocationIds = new HashSet<Guid> { locationId.Value };

            var actualLocationIds = department.DepartmentLocations
                .Select(dl => dl.LocationId.Value)
                .ToHashSet();

            var expectedLocationIds = new HashSet<Guid> { locationIdForUpdate.Value };

            Assert.Single(actualLocationIds);
            Assert.Equal(expectedLocationIds, actualLocationIds);
            Assert.NotEqual(oldLocationIds, actualLocationIds);
        });
    }

    [Fact]
    public async Task UpdateDepartment_with_several_valid_locations_should_succeed()
    {
        // arrange
        LocationId locationId1 = await CreateValidLocation();
        LocationId locationId2 = await CreateValidLocation();

        Department departmentToUpdate = await CreateValidParentDepartment(
            "Department",
            "department",
            [locationId1, locationId2]);

        LocationId locationIdForUpdate1 = await CreateValidLocation();
        LocationId locationIdForUpdate2 = await CreateValidLocation();
        LocationId locationIdForUpdate3 = await CreateValidLocation();
        LocationId locationIdForUpdate4 = await CreateValidLocation();

        const int expectedLocationCount = 4;

        CancellationToken cancellationToken = CancellationToken.None;

        // act
        Result<Guid, Errors> result =
            await ExecuteHandler<UpdateDepartmentLocationsHandler, Result<Guid, Errors>>(
                (sut) =>
            {
                var command =
                    new UpdateDepartmentLocationsCommand(
                        departmentToUpdate.Id.Value,
                        new UpdateDepartmentLocationsRequest([
                            locationIdForUpdate1.Value,
                            locationIdForUpdate2.Value,
                            locationIdForUpdate3.Value,
                            locationIdForUpdate4.Value
                        ]));

                return sut.Handle(command, cancellationToken);
            });

        // assert
        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);

        await ExecuteInDb(async dbContext =>
        {
            var department = await dbContext.Departments
                .Include(d => d.DepartmentLocations)
                .FirstAsync(d => d.Id == new DepartmentId(result.Value), cancellationToken);

            Assert.NotNull(department);
            Assert.Equal(result.Value, department.Id.Value);

            var oldLocationIds = new HashSet<Guid>
            {
                locationId1.Value,
                locationId2.Value,
            };

            var actualLocationIds = department.DepartmentLocations
                .Select(dl => dl.LocationId.Value)
                .ToHashSet();

            var expectedLocationIds = new HashSet<Guid>
            {
                locationIdForUpdate1.Value,
                locationIdForUpdate2.Value,
                locationIdForUpdate3.Value,
                locationIdForUpdate4.Value,
            };

            Assert.Equal(expectedLocationCount, actualLocationIds.Count);
            Assert.Equal(expectedLocationIds, actualLocationIds);
            Assert.NotEqual(oldLocationIds, actualLocationIds);
        });
    }

    [Fact]
    public async Task UpdateDepartment_with_inactive_location_should_return_not_found()
    {
        // arrange
        LocationId locationId = await CreateValidLocation();

        Department departmentToUpdate = await CreateValidParentDepartment(
            "Department",
            "department",
            [locationId]);

        DateTime originalDepartmentUpdatedAt = await GetDepartmentUpdatedAt(departmentToUpdate.Id);

        LocationId locationIdForUpdate = await CreateInactiveLocation();

        CancellationToken cancellationToken = CancellationToken.None;

        // act
        Result<Guid, Errors> result =
            await ExecuteHandler<UpdateDepartmentLocationsHandler, Result<Guid, Errors>>(
                (sut) =>
            {
                var command =
                    new UpdateDepartmentLocationsCommand(
                        departmentToUpdate.Id.Value,
                        new UpdateDepartmentLocationsRequest([locationIdForUpdate.Value]));

                return sut.Handle(command, cancellationToken);
            });

        // assert
        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Error);

        Assert.Contains(result.Error, e => e.Type == ErrorType.NOT_FOUND);

        await ExecuteInDb(async dbContext =>
        {
            var department = await dbContext.Departments
                .Include(d => d.DepartmentLocations)
                .FirstAsync(d => d.Id == departmentToUpdate.Id, cancellationToken);

            var expectedLocationIds = new HashSet<Guid> { locationId.Value };

            var actualLocationIds = department.DepartmentLocations
                .Select(dl => dl.LocationId.Value)
                .ToHashSet();

            Assert.Single(actualLocationIds);
            Assert.Equal(expectedLocationIds, actualLocationIds);

            Assert.Equal(originalDepartmentUpdatedAt, department.UpdatedAt);
        });
    }

    [Fact]
    public async Task UpdateDepartment_location_not_found_should_return_not_found()
    {
        // arrange
        LocationId locationId = await CreateValidLocation();

        Department departmentToUpdate = await CreateValidParentDepartment(
            "Department",
            "department",
            [locationId]);

        DateTime originalDepartmentUpdatedAt = await GetDepartmentUpdatedAt(departmentToUpdate.Id);

        LocationId locationIdForUpdate = CreateNotExistLocation();

        CancellationToken cancellationToken = CancellationToken.None;

        // act
        Result<Guid, Errors> result =
            await ExecuteHandler<UpdateDepartmentLocationsHandler, Result<Guid, Errors>>(
                (sut) =>
            {
                var command =
                    new UpdateDepartmentLocationsCommand(
                        departmentToUpdate.Id.Value,
                        new UpdateDepartmentLocationsRequest([locationIdForUpdate.Value]));

                return sut.Handle(command, cancellationToken);
            });

        // assert
        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Error);

        Assert.Contains(result.Error, e => e.Type == ErrorType.NOT_FOUND);

        await ExecuteInDb(async dbContext =>
        {
            var department = await dbContext.Departments
                .Include(d => d.DepartmentLocations)
                .FirstAsync(d => d.Id == departmentToUpdate.Id, cancellationToken);

            var expectedLocationIds = new HashSet<Guid> { locationId.Value };

            var actualLocationIds = department.DepartmentLocations
                .Select(dl => dl.LocationId.Value)
                .ToHashSet();

            Assert.Single(actualLocationIds);
            Assert.Equal(expectedLocationIds, actualLocationIds);

            Assert.Equal(originalDepartmentUpdatedAt, department.UpdatedAt);
        });
    }
}