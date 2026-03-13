using CSharpFunctionalExtensions;
using DirectoryService.Application.Departments.UpdateDepartment;
using DirectoryService.Contracts.Departments;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations.ValueObjects;
using DirectoryService.IntegrationTests.Infrastructure;
using DirectoryService.Shared;

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
        LocationId locationId = await Data.CreateValidLocation();

        Department department = await Data
            .CreateValidParentDepartment(
                "Department",
                "department",
                [locationId]);

        LocationId locationIdForUpdate = await Data.CreateValidLocation();

        const int expectedLocationCount = 1;

        CancellationToken cancellationToken = CancellationToken.None;

        // act
        Result<Guid, Errors> result =
            await ExecuteHandler<UpdateDepartmentLocationsHandler, Result<Guid, Errors>>(
                (sut) =>
            {
                var command =
                    new UpdateDepartmentLocationsCommand(
                        department.Id.Value,
                        new UpdateDepartmentLocationsRequest([locationIdForUpdate.Value]));

                return sut.Handle(command, cancellationToken);
            });

        // assert
        await AssertDb.DepartmentUpdated(
            result,
            expectedLocationCount,
            [locationId],
            [locationIdForUpdate],
            cancellationToken);
    }

    [Fact]
    public async Task UpdateDepartment_with_several_valid_locations_should_succeed()
    {
        // arrange
        LocationId locationId1 = await Data.CreateValidLocation();
        LocationId locationId2 = await Data.CreateValidLocation();

        Department department = await Data
            .CreateValidParentDepartment(
                "Department",
                "department",
                [locationId1, locationId2]);

        LocationId locationIdForUpdate1 = await Data.CreateValidLocation();
        LocationId locationIdForUpdate2 = await Data.CreateValidLocation();
        LocationId locationIdForUpdate3 = await Data.CreateValidLocation();
        LocationId locationIdForUpdate4 = await Data.CreateValidLocation();

        const int expectedLocationCount = 4;

        CancellationToken cancellationToken = CancellationToken.None;

        // act
        Result<Guid, Errors> result =
            await ExecuteHandler<UpdateDepartmentLocationsHandler, Result<Guid, Errors>>(
                (sut) =>
            {
                var command =
                    new UpdateDepartmentLocationsCommand(
                        department.Id.Value,
                        new UpdateDepartmentLocationsRequest([
                            locationIdForUpdate1.Value,
                            locationIdForUpdate2.Value,
                            locationIdForUpdate3.Value,
                            locationIdForUpdate4.Value
                        ]));

                return sut.Handle(command, cancellationToken);
            });

        // assert
        await AssertDb.DepartmentUpdated(
            result,
            expectedLocationCount,
            [locationId1, locationId2],
            [locationIdForUpdate1, locationIdForUpdate2, locationIdForUpdate3, locationIdForUpdate4],
            cancellationToken);
    }

    [Fact]
    public async Task UpdateDepartment_with_inactive_location_should_return_not_found()
    {
        // arrange
        LocationId locationId = await Data.CreateValidLocation();

        Department department = await Data
            .CreateValidParentDepartment(
                "Department",
                "department",
                [locationId]);

        DateTime originalDepartmentUpdatedAt = await Data.GetDepartmentUpdatedAt(department.Id);

        LocationId locationIdForUpdate = await Data.CreateInactiveLocation();

        const int expectedLocationCount = 1;

        CancellationToken cancellationToken = CancellationToken.None;

        // act
        Result<Guid, Errors> result =
            await ExecuteHandler<UpdateDepartmentLocationsHandler, Result<Guid, Errors>>(
                (sut) =>
            {
                var command =
                    new UpdateDepartmentLocationsCommand(
                        department.Id.Value,
                        new UpdateDepartmentLocationsRequest([locationIdForUpdate.Value]));

                return sut.Handle(command, cancellationToken);
            });

        // assert
        await AssertDb.DepartmentDidNotUpdated(
            result,
            department.Id,
            expectedLocationCount,
            [locationId],
            ErrorType.NOT_FOUND,
            originalDepartmentUpdatedAt,
            cancellationToken);
    }

    [Fact]
    public async Task UpdateDepartment_location_not_found_should_return_not_found()
    {
        // arrange
        LocationId locationId = await Data.CreateValidLocation();

        Department department = await Data
            .CreateValidParentDepartment(
                "Department",
                "department",
                [locationId]);

        DateTime originalDepartmentUpdatedAt = await Data.GetDepartmentUpdatedAt(department.Id);

        LocationId locationIdForUpdate = Data.CreateNotExistLocation();

        const int expectedLocationCount = 1;

        CancellationToken cancellationToken = CancellationToken.None;

        // act
        Result<Guid, Errors> result =
            await ExecuteHandler<UpdateDepartmentLocationsHandler, Result<Guid, Errors>>(
                (sut) =>
            {
                var command =
                    new UpdateDepartmentLocationsCommand(
                        department.Id.Value,
                        new UpdateDepartmentLocationsRequest([locationIdForUpdate.Value]));

                return sut.Handle(command, cancellationToken);
            });

        // assert
        await AssertDb.DepartmentDidNotUpdated(
            result,
            department.Id,
            expectedLocationCount,
            [locationId],
            ErrorType.NOT_FOUND,
            originalDepartmentUpdatedAt,
            cancellationToken);
    }
}