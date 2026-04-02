using CSharpFunctionalExtensions;
using DirectoryService.Application.Departments.SoftDeleteDepartment;
using DirectoryService.Contracts.Departments;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Departments.ValueObjects;
using DirectoryService.Domain.Locations.ValueObjects;
using DirectoryService.Domain.Positions.ValueObjects;
using DirectoryService.IntegrationTests.Infrastructure;
using DirectoryService.Shared;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.IntegrationTests.Departments.SoftDelete;

public class SoftDeleteDepartmentTests : DirectoryBaseTests
{
    public SoftDeleteDepartmentTests(DirectoryTestWebFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task SoftDeleteDepartment_should_update_subtree_paths_and_deactivate_only_root()
    {
        // arrange
        LocationId locationId = await CreateValidLocation();

        Department departmentToDelete = await CreateValidParentDepartment(
            "Root",
            "root",
            [locationId]);

        Department childDepartment = await CreateValidChildDepartment(
            "Child",
            "child",
            departmentToDelete,
            [locationId]);

        Department grandChildDepartment = await CreateValidChildDepartment(
            "GrandChild",
            "grandchild",
            childDepartment,
            [locationId]);

        PositionId positionId = await CreateValidPosition([
            departmentToDelete.Id,
            childDepartment.Id,
            grandChildDepartment.Id]);

        const string expectedRootPath = "deleted-root";
        const string expectedChildPath = "deleted-root.child";
        const string expectedGrandChildPath = "deleted-root.child.grandchild";

        CancellationToken cancellationToken = CancellationToken.None;

        // act
        Result<SoftDeletedDepartmentDto, Errors> result =
            await ExecuteHandler<SoftDeleteDepartmentHandler, Result<SoftDeletedDepartmentDto, Errors>>(
                (sut) =>
        {
            var command = new SoftDeleteDepartmentCommand(departmentToDelete.Id.Value);
            return sut.Handle(command, cancellationToken);
        });

        // assert
        Assert.True(result.IsSuccess);

        await ExecuteInDb(async dbContext =>
        {
            var root = await dbContext.Departments
                .SingleAsync(d => d.Id == departmentToDelete.Id, cancellationToken);
            Assert.Equal(expectedRootPath, root.Path.Value);
            Assert.False(root.IsActive);
            Assert.NotNull(root.DeletedAt);

            Assert.Equal(result.Value.Id, root.Id.Value);
            Assert.Equal(expectedRootPath, result.Value.Path);
            Assert.False(result.Value.IsActive);
            Assert.NotNull(result.Value.DeletedAt);

            var child = await dbContext.Departments
                .SingleAsync(c => c.Id == childDepartment.Id, cancellationToken);
            Assert.Equal(expectedChildPath, child.Path.Value);
            Assert.StartsWith(expectedRootPath, child.Path.Value);
            Assert.True(child.IsActive);

            var grandChild = await dbContext.Departments
                .SingleAsync(g => g.Id == grandChildDepartment.Id, cancellationToken);
            Assert.Equal(expectedGrandChildPath, grandChild.Path.Value);
            Assert.StartsWith(expectedRootPath, grandChild.Path.Value);
            Assert.True(grandChild.IsActive);

            var location = await dbContext.Locations
                .SingleAsync(l => l.Id == locationId, cancellationToken);
            Assert.True(location.IsActive);

            var position = await dbContext.Positions
                .SingleAsync(p => p.Id == positionId, cancellationToken);
            Assert.True(position.IsActive);
        });
    }

    [Fact]
    public async Task SoftDeleteDepartment_should_update_subtree_and_deactivate_only_orphan_dependencies()
    {
        // arrange
        LocationId locationId1 = await CreateValidLocation();
        LocationId locationId2 = await CreateValidLocation();

        Department departmentToDelete = await CreateValidParentDepartment(
            "Root",
            "root",
            [locationId1]);

        Department childDepartment = await CreateValidChildDepartment(
            "Child",
            "child",
            departmentToDelete,
            [locationId2]);

        Department grandChildDepartment = await CreateValidChildDepartment(
            "GrandChild",
            "grandchild",
            childDepartment,
            [locationId2]);

        PositionId positionId1 = await CreateValidPosition([departmentToDelete.Id]);
        PositionId positionId2 = await CreateValidPosition([
            departmentToDelete.Id,
            childDepartment.Id,
            grandChildDepartment.Id]);

        const string expectedRootPath = "deleted-root";
        const string expectedChildPath = "deleted-root.child";
        const string expectedGrandChildPath = "deleted-root.child.grandchild";

        CancellationToken cancellationToken = CancellationToken.None;

        // act
        Result<SoftDeletedDepartmentDto, Errors> result =
            await ExecuteHandler<SoftDeleteDepartmentHandler, Result<SoftDeletedDepartmentDto, Errors>>(
                (sut) =>
        {
            var command = new SoftDeleteDepartmentCommand(departmentToDelete.Id.Value);
            return sut.Handle(command, cancellationToken);
        });

        // assert
        Assert.True(result.IsSuccess);

        await ExecuteInDb(async dbContext =>
        {
            var root = await dbContext.Departments
                .SingleAsync(d => d.Id == departmentToDelete.Id, cancellationToken);
            Assert.Equal(expectedRootPath, root.Path.Value);
            Assert.False(root.IsActive);
            Assert.NotNull(root.DeletedAt);

            Assert.Equal(result.Value.Id, root.Id.Value);
            Assert.Equal(expectedRootPath, result.Value.Path);
            Assert.False(result.Value.IsActive);
            Assert.NotNull(result.Value.DeletedAt);

            var child = await dbContext.Departments
                .SingleAsync(c => c.Id == childDepartment.Id, cancellationToken);
            Assert.Equal(expectedChildPath, child.Path.Value);
            Assert.StartsWith(expectedRootPath, child.Path.Value);
            Assert.True(child.IsActive);

            var grandChild = await dbContext.Departments
                .SingleAsync(g => g.Id == grandChildDepartment.Id, cancellationToken);
            Assert.Equal(expectedGrandChildPath, grandChild.Path.Value);
            Assert.StartsWith(expectedRootPath, grandChild.Path.Value);
            Assert.True(grandChild.IsActive);

            var location1 = await dbContext.Locations
                .SingleAsync(l => l.Id == locationId1, cancellationToken);
            Assert.False(location1.IsActive);

            var location2 = await dbContext.Locations
                .SingleAsync(l => l.Id == locationId2, cancellationToken);
            Assert.True(location2.IsActive);

            var position1 = await dbContext.Positions
                .SingleAsync(p => p.Id == positionId1, cancellationToken);
            Assert.False(position1.IsActive);

            var position2 = await dbContext.Positions
                .SingleAsync(p => p.Id == positionId2, cancellationToken);
            Assert.True(position2.IsActive);
        });
    }

    [Fact]
    public async Task SoftDeleteDepartment_should_fail_when_department_is_inactive()
    {
        // arrange
        LocationId locationId = await CreateValidLocation();

        Department departmentToDelete = await CreateValidParentDepartment(
            "Root",
            "deleted-root",
            [locationId],
            false);

        Department childDepartment = await CreateValidChildDepartment(
            "Child",
            "child",
            departmentToDelete,
            [locationId]);

        Department grandChildDepartment = await CreateValidChildDepartment(
            "GrandChild",
            "grandchild",
            childDepartment,
            [locationId]);

        PositionId positionId = await CreateValidPosition([
            departmentToDelete.Id,
            childDepartment.Id,
            grandChildDepartment.Id]);

        const string expectedRootPath = "deleted-root";
        const string expectedChildPath = "deleted-root.child";
        const string expectedGrandChildPath = "deleted-root.child.grandchild";

        CancellationToken cancellationToken = CancellationToken.None;

        // act
        Result<SoftDeletedDepartmentDto, Errors> result =
            await ExecuteHandler<SoftDeleteDepartmentHandler, Result<SoftDeletedDepartmentDto, Errors>>(
                (sut) =>
                {
                    var command = new SoftDeleteDepartmentCommand(departmentToDelete.Id.Value);
                    return sut.Handle(command, cancellationToken);
                });

        // assert
        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Error);

        Assert.Contains(result.Error, e => e.Type == ErrorType.NOT_FOUND);

        await ExecuteInDb(async dbContext =>
        {
            var root = await dbContext.Departments
                .SingleAsync(d => d.Id == departmentToDelete.Id, cancellationToken);
            Assert.Equal(expectedRootPath, root.Path.Value);
            Assert.False(root.IsActive);

            var child = await dbContext.Departments
                .SingleAsync(c => c.Id == childDepartment.Id, cancellationToken);
            Assert.Equal(expectedChildPath, child.Path.Value);
            Assert.StartsWith(expectedRootPath, child.Path.Value);
            Assert.True(child.IsActive);

            var grandChild = await dbContext.Departments
                .SingleAsync(g => g.Id == grandChildDepartment.Id, cancellationToken);
            Assert.Equal(expectedGrandChildPath, grandChild.Path.Value);
            Assert.StartsWith(expectedRootPath, grandChild.Path.Value);
            Assert.True(grandChild.IsActive);

            var location = await dbContext.Locations
                .SingleAsync(l => l.Id == locationId, cancellationToken);
            Assert.True(location.IsActive);

            var position = await dbContext.Positions
                .SingleAsync(p => p.Id == positionId, cancellationToken);
            Assert.True(position.IsActive);
        });
    }

    [Fact]
    public async Task SoftDeleteDepartment_should_fail_when_department_not_found()
    {
        // arrange
        DepartmentId notExistId = new DepartmentId(Guid.NewGuid());

        CancellationToken cancellationToken = CancellationToken.None;

        // act
        Result<SoftDeletedDepartmentDto, Errors> result =
            await ExecuteHandler<SoftDeleteDepartmentHandler, Result<SoftDeletedDepartmentDto, Errors>>(
                (sut) =>
                {
                    var command = new SoftDeleteDepartmentCommand(notExistId.Value);
                    return sut.Handle(command, cancellationToken);
                });

        // assert
        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Error);

        Assert.Contains(result.Error, e => e.Type == ErrorType.NOT_FOUND);
    }

    [Fact]
    public async Task SoftDeleteDepartment_should_handle_partially_inactive_subtree_correctly()
    {
        // arrange
        LocationId locationId = await CreateValidLocation();

        Department departmentToDelete = await CreateValidParentDepartment(
            "Root",
            "root",
            [locationId]);

        Department childDepartment = await CreateValidChildDepartment(
            "Child",
            "deleted-child",
            departmentToDelete,
            [locationId],
            false);

        Department grandChildDepartment = await CreateValidChildDepartment(
            "GrandChild",
            "grandchild",
            childDepartment,
            [locationId]);

        PositionId positionId = await CreateValidPosition([
            departmentToDelete.Id,
            childDepartment.Id,
            grandChildDepartment.Id]);

        const string expectedRootPath = "deleted-root";
        const string expectedChildPath = "deleted-root.deleted-child";
        const string expectedGrandChildPath = "deleted-root.deleted-child.grandchild";

        string originalChildPath = childDepartment.Path.Value;

        CancellationToken cancellationToken = CancellationToken.None;

        // act
        Result<SoftDeletedDepartmentDto, Errors> result =
            await ExecuteHandler<SoftDeleteDepartmentHandler, Result<SoftDeletedDepartmentDto, Errors>>(
                (sut) =>
                {
                    var command = new SoftDeleteDepartmentCommand(departmentToDelete.Id.Value);
                    return sut.Handle(command, cancellationToken);
                });

        // assert
        Assert.True(result.IsSuccess);

        await ExecuteInDb(async dbContext =>
        {
            var root = await dbContext.Departments
                .SingleAsync(d => d.Id == departmentToDelete.Id, cancellationToken);
            Assert.Equal(expectedRootPath, root.Path.Value);
            Assert.False(root.IsActive);
            Assert.NotNull(root.DeletedAt);

            Assert.Equal(result.Value.Id, root.Id.Value);
            Assert.Equal(expectedRootPath, result.Value.Path);
            Assert.False(result.Value.IsActive);
            Assert.NotNull(result.Value.DeletedAt);

            var child = await dbContext.Departments
                .SingleAsync(c => c.Id == childDepartment.Id, cancellationToken);
            Assert.Equal(expectedChildPath, child.Path.Value);
            Assert.StartsWith(expectedRootPath, child.Path.Value);
            Assert.False(child.IsActive);
            Assert.Equal(originalChildPath.Replace("root.deleted-child", "deleted-root.deleted-child"), child.Path.Value);

            var grandChild = await dbContext.Departments
                .SingleAsync(g => g.Id == grandChildDepartment.Id, cancellationToken);
            Assert.Equal(expectedGrandChildPath, grandChild.Path.Value);
            Assert.StartsWith(expectedRootPath, grandChild.Path.Value);
            Assert.True(grandChild.IsActive);

            var location = await dbContext.Locations
                .SingleAsync(l => l.Id == locationId, cancellationToken);
            Assert.True(location.IsActive);

            var position = await dbContext.Positions
                .SingleAsync(p => p.Id == positionId, cancellationToken);
            Assert.True(position.IsActive);
        });
    }

    [Fact]
    public async Task SoftDeleteDepartment_should_handle_concurrent_requests_correctly()
    {
        // arrange
        LocationId locationId = await CreateValidLocation();

        Department department = await CreateValidParentDepartment(
            "Root",
            "root",
            [locationId]);

        CancellationToken cancellationToken = CancellationToken.None;

        var command = new SoftDeleteDepartmentCommand(department.Id.Value);

        // act
        var task1 = Task.Run(
            () => ExecuteHandler<SoftDeleteDepartmentHandler, Result<SoftDeletedDepartmentDto, Errors>>(
                sut => sut.Handle(command, cancellationToken)), cancellationToken);

        var task2 = Task.Run(
            () => ExecuteHandler<SoftDeleteDepartmentHandler, Result<SoftDeletedDepartmentDto, Errors>>(
                sut => sut.Handle(command, cancellationToken)), cancellationToken);

        var task3 = Task.Run(
            () => ExecuteHandler<SoftDeleteDepartmentHandler, Result<SoftDeletedDepartmentDto, Errors>>(
                sut => sut.Handle(command, cancellationToken)), cancellationToken);

        var results = await Task.WhenAll(task1, task2, task3);

        // assert
        int successCount = results.Count(r => r.IsSuccess);
        int failureCount = results.Count(r => r.IsFailure);

        Assert.Equal(1, successCount);
        Assert.Equal(2, failureCount);

        var failures = results.Where(r => r.IsFailure).ToList();
        Assert.All(failures, r =>
            Assert.Contains(r.Error, e => e.Type == ErrorType.NOT_FOUND));

        await ExecuteInDb(async dbContext =>
        {
            var root = await dbContext.Departments
                .SingleAsync(d => d.Id == department.Id, cancellationToken);

            Assert.False(root.IsActive);
            Assert.NotNull(root.DeletedAt);
            Assert.StartsWith("deleted-", root.Path.Value);
        });
    }
}