using CSharpFunctionalExtensions;
using DirectoryService.Application.Departments.CreateDepartment;
using DirectoryService.Contracts.Departments;
using DirectoryService.Domain.Departments.ValueObjects;
using DirectoryService.Domain.Locations.ValueObjects;
using DirectoryService.IntegrationTests.Infrastructure;
using DirectoryService.Shared;

namespace DirectoryService.IntegrationTests.Departments.Create;

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
        LocationId locationId = await Data.CreateValidLocation();
        const int expectedLocationCount = 1;

        var cancellationToken = CancellationToken.None;

        // act
        Result<Guid, Errors> result = await ExecuteHandler<CreateDepartmentHandler, Result<Guid, Errors>>(
            (sut) =>
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
        await AssertDb.DepartmentCreated(result, expectedLocationCount, cancellationToken);
    }

    [Fact]
    public async Task CreateDepartment_with_several_valid_locations_should_succeed()
    {
        // arrange
        LocationId locationId1 = await Data.CreateValidLocation();
        LocationId locationId2 = await Data.CreateValidLocation();
        LocationId locationId3 = await Data.CreateValidLocation();
        const int expectedLocationCount = 3;

        var cancellationToken = CancellationToken.None;

        // act
        var result = await ExecuteHandler<CreateDepartmentHandler, Result<Guid, Errors>>(
            (sut) =>
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
        await AssertDb.DepartmentCreated(result, expectedLocationCount, cancellationToken);
    }

    [Fact]
    public async Task CreateDepartment_invalid_name_should_fail()
    {
        // arrange
        LocationId locationId = await Data.CreateValidLocation();

        var cancellationToken = CancellationToken.None;

        // act
        Result<Guid, Errors> result = await ExecuteHandler<CreateDepartmentHandler, Result<Guid, Errors>>(
            (sut) =>
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
        await AssertDb.DepartmentDidNotCreated(result, ErrorType.VALIDATION, cancellationToken);
    }

    [Fact]
    public async Task CreateDepartment_with_inactive_location_should_fail()
    {
        // arrange
        LocationId locationId = await Data.CreateInactiveLocation();

        var cancellationToken = CancellationToken.None;

        // act
        var result = await ExecuteHandler<CreateDepartmentHandler, Result<Guid, Errors>>(
            (sut) =>
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
        await AssertDb.DepartmentDidNotCreated(result, ErrorType.NOT_FOUND, cancellationToken);
    }

    [Fact]
    public async Task CreateDepartment_location_not_found_should_fail()
    {
        // arrange
        LocationId locationId = Data.CreateNotExistLocation();

        var cancellationToken = CancellationToken.None;

        // act
        var result = await ExecuteHandler<CreateDepartmentHandler, Result<Guid, Errors>>(
            (sut) =>
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
        await AssertDb.DepartmentDidNotCreated(result, ErrorType.NOT_FOUND, cancellationToken);
    }

    [Fact]
    public async Task CreateDepartment_parent_not_found_should_fail()
    {
        // arrange
        DepartmentId parentId = new(Guid.NewGuid());
        LocationId locationId = await Data.CreateValidLocation();

        var cancellationToken = CancellationToken.None;

        // act
        var result = await ExecuteHandler<CreateDepartmentHandler, Result<Guid, Errors>>(
            (sut) =>
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
        await AssertDb.DepartmentDidNotCreated(result, ErrorType.NOT_FOUND, cancellationToken);
    }
}