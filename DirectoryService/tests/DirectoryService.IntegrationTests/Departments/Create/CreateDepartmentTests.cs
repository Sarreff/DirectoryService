using CSharpFunctionalExtensions;
using DirectoryService.Application.Departments.CreateDepartment;
using DirectoryService.Contracts.Departments;
using DirectoryService.Domain.Departments.ValueObjects;
using DirectoryService.Domain.Locations.ValueObjects;
using DirectoryService.IntegrationTests.Infrastructure;
using DirectoryService.Shared;
using Microsoft.EntityFrameworkCore;

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
        LocationId locationId = await CreateValidLocation();

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
        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);

        await ExecuteInDb(async dbContext =>
        {
            var department = await dbContext.Departments
                .Include(d => d.DepartmentLocations)
                .FirstAsync(d => d.Id == new DepartmentId(result.Value), cancellationToken);

            Assert.Equal(result.Value, department.Id.Value);

            Assert.Equal("Department", department.Name.Value);
            Assert.Equal("department", department.Identifier.Value);

            Assert.Single(department.DepartmentLocations);
        });
    }

    [Fact]
    public async Task CreateDepartment_with_several_valid_locations_should_succeed()
    {
        // arrange
        LocationId locationId1 = await CreateValidLocation();
        LocationId locationId2 = await CreateValidLocation();
        LocationId locationId3 = await CreateValidLocation();
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
        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);

        await ExecuteInDb(async dbContext =>
        {
            var department = await dbContext.Departments
                .Include(d => d.DepartmentLocations)
                .FirstAsync(d => d.Id == new DepartmentId(result.Value), cancellationToken);

            Assert.Equal(result.Value, department.Id.Value);

            Assert.Equal("Department", department.Name.Value);
            Assert.Equal("department", department.Identifier.Value);

            Assert.Equal(expectedLocationCount, department.DepartmentLocations.Count);
        });
    }

    [Fact]
    public async Task CreateDepartment_invalid_name_should_fail()
    {
        // arrange
        LocationId locationId = await CreateValidLocation();

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
        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Error);

        await ExecuteInDb(async dbContext =>
        {
            int departmentsCount = await dbContext.Departments.CountAsync(cancellationToken);

            Assert.Equal(0, departmentsCount);
            Assert.Contains(result.Error, e => e.Type == ErrorType.VALIDATION);
        });
    }

    [Fact]
    public async Task CreateDepartment_with_inactive_location_should_fail()
    {
        // arrange
        LocationId locationId = await CreateInactiveLocation();

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
        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Error);

        await ExecuteInDb(async dbContext =>
        {
            int departmentsCount = await dbContext.Departments.CountAsync(cancellationToken);

            Assert.Equal(0, departmentsCount);
            Assert.Contains(result.Error, e => e.Type == ErrorType.NOT_FOUND);
        });
    }

    [Fact]
    public async Task CreateDepartment_location_not_found_should_fail()
    {
        // arrange
        LocationId locationId = CreateNotExistLocation();

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
        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Error);

        await ExecuteInDb(async dbContext =>
        {
            int departmentsCount = await dbContext.Departments.CountAsync(cancellationToken);

            Assert.Equal(0, departmentsCount);
            Assert.Contains(result.Error, e => e.Type == ErrorType.NOT_FOUND);
        });
    }

    [Fact]
    public async Task CreateDepartment_parent_not_found_should_fail()
    {
        // arrange
        DepartmentId parentId = new(Guid.NewGuid());
        LocationId locationId = await CreateValidLocation();

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
        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Error);

        await ExecuteInDb(async dbContext =>
        {
            int departmentsCount = await dbContext.Departments.CountAsync(cancellationToken);

            Assert.Equal(0, departmentsCount);
            Assert.Contains(result.Error, e => e.Type == ErrorType.NOT_FOUND);
        });
    }
}