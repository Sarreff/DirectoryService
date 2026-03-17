using CSharpFunctionalExtensions;
using DirectoryService.Application.Departments.MoveDepartment;
using DirectoryService.Contracts.Departments;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Departments.ValueObjects;
using DirectoryService.Domain.Locations.ValueObjects;
using DirectoryService.IntegrationTests.Infrastructure;
using DirectoryService.Shared;
using Microsoft.EntityFrameworkCore;
using Path = DirectoryService.Domain.Departments.ValueObjects.Path;

namespace DirectoryService.IntegrationTests.Departments.Move;

public class MoveDepartmentTests : DirectoryBaseTests
{
    public MoveDepartmentTests(DirectoryTestWebFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task MoveDepartment_without_children_to_parent_should_succeed()
    {
        // arrange
        LocationId locationId = await CreateValidLocation();
        Department departmentToMove = await CreateValidParentDepartment(
            "MovedDepartment",
            "movedIdentifier",
            [locationId]);

        Department parentDept = await CreateValidParentDepartment(
            "ParentDepartment",
            "parentIdentifier",
            [locationId]);

        const int movedDepth = 1;
        const int parentDepth = 0;

        DateTime originalDepartmentToMoveUpdatedAt = await GetDepartmentUpdatedAt(departmentToMove.Id);

        CancellationToken cancellationToken = CancellationToken.None;

        // act
        Result<Guid, Errors> result =
            await ExecuteHandler<MoveDepartmentHandler, Result<Guid, Errors>>((sut) =>
            {
                var command =
                    new MoveDepartmentCommand(
                        departmentToMove.Id.Value,
                        new MoveDepartmentRequest(parentDept.Id.Value));

                return sut.Handle(command, cancellationToken);
            });

        // assert
        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);

        await ExecuteInDb(async dbContext =>
        {
            var movedDepartment = await dbContext.Departments
                .FirstAsync(d => d.Id == new DepartmentId(result.Value), cancellationToken);

            var parentDepartment = await dbContext.Departments
                .FirstAsync(d => d.Id == parentDept.Id, cancellationToken);

            Assert.Equal(result.Value, movedDepartment.Id.Value);
            Assert.NotEqual(movedDepartment.Id.Value, parentDepartment.Id.Value);

            Assert.NotNull(movedDepartment.ParentId);
            Assert.Equal(parentDepartment.Id.Value, movedDepartment.ParentId.Value);

            Assert.Equal(movedDepth, movedDepartment.Depth);
            Assert.Equal(parentDepth, parentDepartment.Depth);
            Assert.Equal(parentDepartment.Depth + 1, movedDepartment.Depth);

            Assert.True(movedDepartment.IsActive);
            Assert.True(parentDepartment.IsActive);

            Assert.True(movedDepartment.UpdatedAt > originalDepartmentToMoveUpdatedAt);

            var expectedPath = parentDepartment.Path.CreateChild(movedDepartment.Identifier);
            var expectedParentPath = Path.CreateParent(parentDepartment.Identifier);
            Assert.Equal(expectedPath.Value, movedDepartment.Path.Value);
            Assert.Equal(expectedParentPath.Value, parentDepartment.Path.Value);

            int departmentsCount = await dbContext.Departments.CountAsync(cancellationToken);
            Assert.Equal(2, departmentsCount);
        });
    }

    [Fact]
    public async Task MoveDepartment_without_children_to_root_should_succeed()
    {
        // arrange
        LocationId locationId = await CreateValidLocation();

        Department parentDept = await CreateValidParentDepartment(
            "ParentDepartment",
            "parentIdentifier",
            [locationId]);

        Department departmentToMove = await CreateValidChildDepartment(
            "MovedDepartment",
            "movedIdentifier",
            parentDept,
            [locationId]);

        const int movedDepth = 0;
        const int parentDepth = 0;

        DateTime originalDepartmentToMoveUpdatedAt = await GetDepartmentUpdatedAt(departmentToMove.Id);

        CancellationToken cancellationToken = CancellationToken.None;

        // act
        Result<Guid, Errors> result =
            await ExecuteHandler<MoveDepartmentHandler, Result<Guid, Errors>>((sut) =>
            {
                var command =
                    new MoveDepartmentCommand(
                        departmentToMove.Id.Value,
                        new MoveDepartmentRequest(null));

                return sut.Handle(command, cancellationToken);
            });

        // assert
        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);

        await ExecuteInDb(async dbContext =>
        {
            var movedDepartment = await dbContext.Departments
                .FirstAsync(d => d.Id == new DepartmentId(result.Value), cancellationToken);

            var parentDepartment = await dbContext.Departments
                .FirstAsync(d => d.Id == parentDept.Id, cancellationToken);

            Assert.Equal(result.Value, movedDepartment.Id.Value);
            Assert.NotEqual(movedDepartment.Id.Value, parentDepartment.Id.Value);

            Assert.Null(movedDepartment.ParentId);

            Assert.Equal(movedDepth, movedDepartment.Depth);
            Assert.Equal(parentDepth, parentDepartment.Depth);

            Assert.True(movedDepartment.IsActive);
            Assert.True(parentDepartment.IsActive);

            Assert.True(movedDepartment.UpdatedAt > originalDepartmentToMoveUpdatedAt);

            var expectedPath = Path.CreateParent(movedDepartment.Identifier);
            var expectedParentPath = Path.CreateParent(parentDepartment.Identifier);
            Assert.Equal(expectedPath.Value, movedDepartment.Path.Value);
            Assert.Equal(expectedParentPath.Value, parentDepartment.Path.Value);

            int departmentsCount = await dbContext.Departments.CountAsync(cancellationToken);
            Assert.Equal(2, departmentsCount);
        });
    }

    [Fact]
    public async Task MoveDepartment_with_children_to_parent_should_succeed()
    {
        // arrange
        LocationId locationId = await CreateValidLocation();

        Department departmentToMove = await CreateValidParentDepartment(
            "MovedDepartment",
            "movedIdentifier",
            [locationId]);

        Department child = await CreateValidChildDepartment(
            "MovedChildDepartment",
            "movedChildIdentifier",
            departmentToMove,
            [locationId]);

        Department subChild = await CreateValidChildDepartment(
            "MovedSubChildDepartment",
            "movedSubChildIdentifier",
            child,
            [locationId]);

        Department parentDept = await CreateValidParentDepartment(
            "ParentDepartment",
            "parentIdentifier",
            [locationId]);

        const int movedDepth = 1;
        const int parentDepth = 0;
        const int childDepth = 2;
        const int subChildDepth = 3;

        DateTime originalDepartmentToMoveUpdatedAt = await GetDepartmentUpdatedAt(departmentToMove.Id);
        DateTime originalChildUpdatedAt = await GetDepartmentUpdatedAt(child.Id);
        DateTime originalSubChildUpdatedAt = await GetDepartmentUpdatedAt(subChild.Id);

        CancellationToken cancellationToken = CancellationToken.None;

        // act
        Result<Guid, Errors> result =
            await ExecuteHandler<MoveDepartmentHandler, Result<Guid, Errors>>((sut) =>
            {
                var command =
                    new MoveDepartmentCommand(
                        departmentToMove.Id.Value,
                        new MoveDepartmentRequest(parentDept.Id.Value));

                return sut.Handle(command, cancellationToken);
            });

        // assert
        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);

        await ExecuteInDb(async dbContext =>
        {
            var movedDepartment = await dbContext.Departments
                .FirstAsync(d => d.Id == new DepartmentId(result.Value), cancellationToken);

            var childDepartment = await dbContext.Departments
                .FirstAsync(cd => cd.Id == child.Id, cancellationToken);

            var subChildDepartment = await dbContext.Departments
                .FirstAsync(scd => scd.Id == subChild.Id, cancellationToken);

            var parentDepartment = await dbContext.Departments
                .FirstAsync(d => d.Id == parentDept.Id, cancellationToken);

            Assert.Equal(result.Value, movedDepartment.Id.Value);

            Assert.NotNull(movedDepartment.ParentId);
            Assert.Equal(parentDepartment.Id.Value, movedDepartment.ParentId!.Value);
            Assert.Equal(movedDepartment.Id.Value, childDepartment.ParentId!.Value);
            Assert.Equal(childDepartment.Id.Value, subChildDepartment.ParentId!.Value);

            Assert.Equal(movedDepth, movedDepartment.Depth);
            Assert.Equal(childDepth, childDepartment.Depth);
            Assert.Equal(subChildDepth, subChildDepartment.Depth);
            Assert.Equal(parentDepth, parentDepartment.Depth);
            Assert.Equal(parentDepartment.Depth + 1, movedDepartment.Depth);
            Assert.Equal(movedDepartment.Depth + 1, childDepartment.Depth);
            Assert.Equal(childDepartment.Depth + 1, subChildDepartment.Depth);

            Assert.True(movedDepartment.IsActive);
            Assert.True(childDepartment.IsActive);
            Assert.True(subChildDepartment.IsActive);
            Assert.True(parentDepartment.IsActive);

            Assert.True(movedDepartment.UpdatedAt > originalDepartmentToMoveUpdatedAt);
            Assert.True(childDepartment.UpdatedAt > originalChildUpdatedAt);
            Assert.True(subChildDepartment.UpdatedAt > originalSubChildUpdatedAt);

            var movedExpectedPath =
                parentDepartment.Path.CreateChild(movedDepartment.Identifier);
            var childExpectedPath =
                movedExpectedPath.CreateChild(childDepartment.Identifier);
            var subChildExpectedPath =
                childExpectedPath.CreateChild(subChildDepartment.Identifier);
            var expectedParentPath = Path.CreateParent(parentDepartment.Identifier);
            Assert.Equal(movedExpectedPath.Value, movedDepartment.Path.Value);
            Assert.Equal(childExpectedPath.Value, childDepartment.Path.Value);
            Assert.Equal(subChildExpectedPath.Value, subChildDepartment.Path.Value);
            Assert.Equal(expectedParentPath.Value, parentDepartment.Path.Value);

            int departmentsCount = await dbContext.Departments.CountAsync(cancellationToken);
            Assert.Equal(4, departmentsCount);
        });
    }

    [Fact]
    public async Task MoveDepartment_with_children_to_root_should_succeed()
    {
        // arrange
        LocationId locationId = await CreateValidLocation();

        Department parentDept = await CreateValidParentDepartment(
            "ParentDepartment",
            "parentIdentifier",
            [locationId]);

        Department departmentToMove = await CreateValidChildDepartment(
            "MovedDepartment",
            "movedIdentifier",
            parentDept,
            [locationId]);

        Department child = await CreateValidChildDepartment(
            "MovedChildDepartment",
            "movedChildIdentifier",
            departmentToMove,
            [locationId]);

        Department subChild = await CreateValidChildDepartment(
            "MovedSubChildDepartment",
            "movedSubChildIdentifier",
            child,
            [locationId]);

        const int movedDepth = 0;
        const int parentDepth = 0;
        const int childDepth = 1;
        const int subChildDepth = 2;

        DateTime originalDepartmentToMoveUpdatedAt = await GetDepartmentUpdatedAt(departmentToMove.Id);
        DateTime originalChildUpdatedAt = await GetDepartmentUpdatedAt(child.Id);
        DateTime originalSubChildUpdatedAt = await GetDepartmentUpdatedAt(subChild.Id);

        CancellationToken cancellationToken = CancellationToken.None;

        // act
        Result<Guid, Errors> result =
            await ExecuteHandler<MoveDepartmentHandler, Result<Guid, Errors>>((sut) =>
            {
                var command =
                    new MoveDepartmentCommand(
                        departmentToMove.Id.Value,
                        new MoveDepartmentRequest(null));

                return sut.Handle(command, cancellationToken);
            });

        // assert
        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);

        await ExecuteInDb(async dbContext =>
        {
            var movedDepartment = await dbContext.Departments
                .FirstAsync(d => d.Id == new DepartmentId(result.Value), cancellationToken);

            var childDepartment = await dbContext.Departments
                .FirstAsync(cd => cd.Id == child.Id, cancellationToken);

            var subChildDepartment = await dbContext.Departments
                .FirstAsync(scd => scd.Id == subChild.Id, cancellationToken);

            var parentDepartment = await dbContext.Departments
                .FirstAsync(d => d.Id == parentDept.Id, cancellationToken);

            Assert.Equal(result.Value, movedDepartment.Id.Value);

            Assert.Null(movedDepartment.ParentId);
            Assert.Equal(movedDepartment.Id.Value, childDepartment.ParentId!.Value);
            Assert.Equal(childDepartment.Id.Value, subChildDepartment.ParentId!.Value);

            Assert.Equal(movedDepth, movedDepartment.Depth);
            Assert.Equal(childDepth, childDepartment.Depth);
            Assert.Equal(subChildDepth, subChildDepartment.Depth);
            Assert.Equal(parentDepth, parentDepartment.Depth);
            Assert.Equal(movedDepartment.Depth + 1, childDepartment.Depth);
            Assert.Equal(childDepartment.Depth + 1, subChildDepartment.Depth);

            Assert.True(movedDepartment.IsActive);
            Assert.True(childDepartment.IsActive);
            Assert.True(subChildDepartment.IsActive);
            Assert.True(parentDepartment.IsActive);

            var movedExpectedPath =
                Path.CreateParent(movedDepartment.Identifier);
            var childExpectedPath =
                movedExpectedPath.CreateChild(childDepartment.Identifier);
            var subChildExpectedPath =
                childExpectedPath.CreateChild(subChildDepartment.Identifier);
            var expectedParentPath = Path.CreateParent(parentDepartment.Identifier);
            Assert.Equal(movedExpectedPath.Value, movedDepartment.Path.Value);
            Assert.Equal(childExpectedPath.Value, childDepartment.Path.Value);
            Assert.Equal(subChildExpectedPath.Value, subChildDepartment.Path.Value);
            Assert.Equal(expectedParentPath.Value, parentDepartment.Path.Value);

            Assert.True(movedDepartment.UpdatedAt > originalDepartmentToMoveUpdatedAt);
            Assert.True(childDepartment.UpdatedAt > originalChildUpdatedAt);
            Assert.True(subChildDepartment.UpdatedAt > originalSubChildUpdatedAt);

            int departmentsCount = await dbContext.Departments.CountAsync(cancellationToken);
            Assert.Equal(4, departmentsCount);
        });
    }

    [Fact]
    public async Task MoveDepartment_with_cyclical_dependency_should_return_conflict_error()
    {
        // arrange
        LocationId locationId = await CreateValidLocation();

        Department departmentToMove = await CreateValidParentDepartment(
            "MovedDepartment",
            "movedIdentifier",
            [locationId]);

        Department child = await CreateValidChildDepartment(
            "MovedChildDepartment",
            "movedChildIdentifier",
            departmentToMove,
            [locationId]);

        Department subChild = await CreateValidChildDepartment(
            "MovedSubChildDepartment",
            "movedSubChildIdentifier",
            child,
            [locationId]);

        const short movedDepth = 0;
        const short childDepth = 1;
        const short subChildDepth = 2;

        DateTime originalDepartmentToMoveUpdatedAt = await GetDepartmentUpdatedAt(departmentToMove.Id);
        DateTime originalChildUpdatedAt = await GetDepartmentUpdatedAt(child.Id);
        DateTime originalSubChildUpdatedAt = await GetDepartmentUpdatedAt(subChild.Id);

        CancellationToken cancellationToken = CancellationToken.None;

        // act
        Result<Guid, Errors> result =
            await ExecuteHandler<MoveDepartmentHandler, Result<Guid, Errors>>((sut) =>
            {
                var command =
                    new MoveDepartmentCommand(
                        departmentToMove.Id.Value,
                        new MoveDepartmentRequest(subChild.Id.Value));

                return sut.Handle(command, cancellationToken);
            });

        // assert
        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Error);

        Assert.Contains(result.Error, e => e.Type == ErrorType.CONFLICT);

        await ExecuteInDb(async dbContext =>
        {
            var movedDepartment = await dbContext.Departments
                .FirstAsync(d => d.Id == departmentToMove.Id, cancellationToken);

            var childDepartment = await dbContext.Departments
                .FirstAsync(cd => cd.Id == child.Id, cancellationToken);

            var subChildDepartment = await dbContext.Departments
                .FirstAsync(scd => scd.Id == subChild.Id, cancellationToken);

            Assert.Null(movedDepartment.ParentId);
            Assert.Equal(movedDepartment.Id.Value, childDepartment.ParentId!.Value);
            Assert.Equal(childDepartment.Id.Value, subChildDepartment.ParentId!.Value);

            Assert.Equal(movedDepth, movedDepartment.Depth);
            Assert.Equal(childDepth, childDepartment.Depth);
            Assert.Equal(subChildDepth, subChildDepartment.Depth);
            Assert.Equal(movedDepartment.Depth + 1, childDepartment.Depth);
            Assert.Equal(childDepartment.Depth + 1, subChildDepartment.Depth);

            Assert.True(movedDepartment.IsActive);
            Assert.True(childDepartment.IsActive);
            Assert.True(subChildDepartment.IsActive);

            var movedExpectedPath =
                Path.CreateParent(movedDepartment.Identifier);
            var childExpectedPath =
                movedExpectedPath.CreateChild(childDepartment.Identifier);
            var subChildExpectedPath =
                childExpectedPath.CreateChild(subChildDepartment.Identifier);
            Assert.Equal(movedExpectedPath.Value, movedDepartment.Path.Value);
            Assert.Equal(childExpectedPath.Value, childDepartment.Path.Value);
            Assert.Equal(subChildExpectedPath.Value, subChildDepartment.Path.Value);

            Assert.Equal(originalDepartmentToMoveUpdatedAt, movedDepartment.UpdatedAt);
            Assert.Equal(originalChildUpdatedAt, childDepartment.UpdatedAt);
            Assert.Equal(originalSubChildUpdatedAt, subChildDepartment.UpdatedAt);

            int departmentsCount = await dbContext.Departments.CountAsync(cancellationToken);
            Assert.Equal(3, departmentsCount);
        });
    }

    [Fact]
    public async Task MoveDepartment_to_same_parent_should_change_nothing()
    {
        // arrange
        LocationId locationId = await CreateValidLocation();

        Department parentDept = await CreateValidParentDepartment(
            "ParentDepartment",
            "parentIdentifier",
            [locationId]);

        Department departmentToMove = await CreateValidChildDepartment(
            "MovedDepartment",
            "movedIdentifier",
            parentDept,
            [locationId]);

        Department child = await CreateValidChildDepartment(
            "MovedChildDepartment",
            "movedChildIdentifier",
            departmentToMove,
            [locationId]);

        Department subChild = await CreateValidChildDepartment(
            "MovedSubChildDepartment",
            "movedSubChildIdentifier",
            child,
            [locationId]);

        const int parentDepth = 0;
        const int movedDepth = 1;
        const int childDepth = 2;
        const int subChildDepth = 3;

        DateTime originalParentUpdatedAt = await GetDepartmentUpdatedAt(parentDept.Id);
        DateTime originalDepartmentToMoveUpdatedAt = await GetDepartmentUpdatedAt(departmentToMove.Id);
        DateTime originalChildUpdatedAt = await GetDepartmentUpdatedAt(child.Id);
        DateTime originalSubChildUpdatedAt = await GetDepartmentUpdatedAt(subChild.Id);

        CancellationToken cancellationToken = CancellationToken.None;

        // act
        Result<Guid, Errors> result =
            await ExecuteHandler<MoveDepartmentHandler, Result<Guid, Errors>>((sut) =>
            {
                var command =
                    new MoveDepartmentCommand(
                        departmentToMove.Id.Value,
                        new MoveDepartmentRequest(parentDept.Id.Value));

                return sut.Handle(command, cancellationToken);
            });

        // assert
        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);

        await ExecuteInDb(async dbContext =>
        {
            var movedDepartment = await dbContext.Departments
                .FirstAsync(d => d.Id == new DepartmentId(result.Value), cancellationToken);

            var childDepartment = await dbContext.Departments
                .FirstAsync(cd => cd.Id == child.Id, cancellationToken);

            var subChildDepartment = await dbContext.Departments
                .FirstAsync(scd => scd.Id == subChild.Id, cancellationToken);

            var parentDepartment = await dbContext.Departments
                .FirstAsync(d => d.Id == parentDept.Id, cancellationToken);

            Assert.Equal(result.Value, movedDepartment.Id.Value);

            Assert.Null(parentDepartment.ParentId);
            Assert.Equal(parentDepartment.Id.Value, movedDepartment.ParentId!.Value);
            Assert.Equal(movedDepartment.Id.Value, childDepartment.ParentId!.Value);
            Assert.Equal(childDepartment.Id.Value, subChildDepartment.ParentId!.Value);

            Assert.Equal(movedDepth, movedDepartment.Depth);
            Assert.Equal(childDepth, childDepartment.Depth);
            Assert.Equal(subChildDepth, subChildDepartment.Depth);
            Assert.Equal(parentDepth, parentDepartment.Depth);
            Assert.Equal(movedDepartment.Depth + 1, childDepartment.Depth);
            Assert.Equal(childDepartment.Depth + 1, subChildDepartment.Depth);

            Assert.True(movedDepartment.IsActive);
            Assert.True(childDepartment.IsActive);
            Assert.True(subChildDepartment.IsActive);
            Assert.True(parentDepartment.IsActive);

            var expectedParentPath = Path.CreateParent(parentDepartment.Identifier);
            var movedExpectedPath =
                expectedParentPath.CreateChild(movedDepartment.Identifier);
            var childExpectedPath =
                movedExpectedPath.CreateChild(childDepartment.Identifier);
            var subChildExpectedPath =
                childExpectedPath.CreateChild(subChildDepartment.Identifier);
            Assert.Equal(movedExpectedPath.Value, movedDepartment.Path.Value);
            Assert.Equal(childExpectedPath.Value, childDepartment.Path.Value);
            Assert.Equal(subChildExpectedPath.Value, subChildDepartment.Path.Value);
            Assert.Equal(expectedParentPath.Value, parentDepartment.Path.Value);

            Assert.Equal(originalParentUpdatedAt, parentDepartment.UpdatedAt);
            Assert.Equal(originalDepartmentToMoveUpdatedAt, movedDepartment.UpdatedAt);
            Assert.Equal(originalChildUpdatedAt, childDepartment.UpdatedAt);
            Assert.Equal(originalSubChildUpdatedAt, subChildDepartment.UpdatedAt);

            int departmentsCount = await dbContext.Departments.CountAsync(cancellationToken);
            Assert.Equal(4, departmentsCount);
        });
    }

    [Fact]
    public async Task MoveDepartment_to_itself_should_fail()
    {
        // arrange
        LocationId locationId = await CreateValidLocation();

        Department departmentToMove = await CreateValidParentDepartment(
            "MovedDepartment",
            "movedIdentifier",
            [locationId]);

        Department child = await CreateValidChildDepartment(
            "MovedChildDepartment",
            "movedChildIdentifier",
            departmentToMove,
            [locationId]);

        Department subChild = await CreateValidChildDepartment(
            "MovedSubChildDepartment",
            "movedSubChildIdentifier",
            child,
            [locationId]);

        const int movedDepth = 0;
        const int childDepth = 1;
        const int subChildDepth = 2;

        DateTime originalDepartmentToMoveUpdatedAt = await GetDepartmentUpdatedAt(departmentToMove.Id);
        DateTime originalChildUpdatedAt = await GetDepartmentUpdatedAt(child.Id);
        DateTime originalSubChildUpdatedAt = await GetDepartmentUpdatedAt(subChild.Id);

        CancellationToken cancellationToken = CancellationToken.None;

        // act
        Result<Guid, Errors> result =
            await ExecuteHandler<MoveDepartmentHandler, Result<Guid, Errors>>((sut) =>
            {
                var command =
                    new MoveDepartmentCommand(
                        departmentToMove.Id.Value,
                        new MoveDepartmentRequest(departmentToMove.Id.Value));

                return sut.Handle(command, cancellationToken);
            });

        // assert
        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Error);

        Assert.Contains(result.Error, e => e.Type == ErrorType.VALIDATION);

        await ExecuteInDb(async dbContext =>
        {
            var movedDepartment = await dbContext.Departments
                .FirstAsync(d => d.Id == departmentToMove.Id, cancellationToken);

            var childDepartment = await dbContext.Departments
                .FirstAsync(cd => cd.Id == child.Id, cancellationToken);

            var subChildDepartment = await dbContext.Departments
                .FirstAsync(scd => scd.Id == subChild.Id, cancellationToken);

            Assert.Null(movedDepartment.ParentId);
            Assert.Equal(movedDepartment.Id.Value, childDepartment.ParentId!.Value);
            Assert.Equal(childDepartment.Id.Value, subChildDepartment.ParentId!.Value);

            Assert.Equal(movedDepth, movedDepartment.Depth);
            Assert.Equal(childDepth, childDepartment.Depth);
            Assert.Equal(subChildDepth, subChildDepartment.Depth);
            Assert.Equal(movedDepartment.Depth + 1, childDepartment.Depth);
            Assert.Equal(childDepartment.Depth + 1, subChildDepartment.Depth);

            Assert.True(movedDepartment.IsActive);
            Assert.True(childDepartment.IsActive);
            Assert.True(subChildDepartment.IsActive);

            var movedExpectedPath =
                Path.CreateParent(movedDepartment.Identifier);
            var childExpectedPath =
                movedExpectedPath.CreateChild(childDepartment.Identifier);
            var subChildExpectedPath =
                childExpectedPath.CreateChild(subChildDepartment.Identifier);
            Assert.Equal(movedExpectedPath.Value, movedDepartment.Path.Value);
            Assert.Equal(childExpectedPath.Value, childDepartment.Path.Value);
            Assert.Equal(subChildExpectedPath.Value, subChildDepartment.Path.Value);

            Assert.Equal(originalDepartmentToMoveUpdatedAt, movedDepartment.UpdatedAt);
            Assert.Equal(originalChildUpdatedAt, childDepartment.UpdatedAt);
            Assert.Equal(originalSubChildUpdatedAt, subChildDepartment.UpdatedAt);

            int departmentsCount = await dbContext.Departments.CountAsync(cancellationToken);
            Assert.Equal(3, departmentsCount);
        });
    }

    [Fact]
    public async Task MoveDepartment_to_not_exist_parent_should_fail()
    {
        // arrange
        LocationId locationId = await CreateValidLocation();

        DepartmentId parentId = CreateNotExistDepartment();

        Department departmentToMove = await CreateValidParentDepartment(
            "MovedDepartment",
            "movedIdentifier",
            [locationId]);

        Department child = await CreateValidChildDepartment(
            "MovedChildDepartment",
            "movedChildIdentifier",
            departmentToMove,
            [locationId]);

        Department subChild = await CreateValidChildDepartment(
            "MovedSubChildDepartment",
            "movedSubChildIdentifier",
            child,
            [locationId]);

        const short movedDepth = 0;
        const short childDepth = 1;
        const short subChildDepth = 2;

        DateTime originalDepartmentToMoveUpdatedAt = await GetDepartmentUpdatedAt(departmentToMove.Id);
        DateTime originalChildUpdatedAt = await GetDepartmentUpdatedAt(child.Id);
        DateTime originalSubChildUpdatedAt = await GetDepartmentUpdatedAt(subChild.Id);

        CancellationToken cancellationToken = CancellationToken.None;

        // act
        Result<Guid, Errors> result =
            await ExecuteHandler<MoveDepartmentHandler, Result<Guid, Errors>>((sut) =>
            {
                var command =
                    new MoveDepartmentCommand(
                        departmentToMove.Id.Value,
                        new MoveDepartmentRequest(parentId.Value));

                return sut.Handle(command, cancellationToken);
            });

        // assert
        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Error);

        Assert.Contains(result.Error, e => e.Type == ErrorType.NOT_FOUND);

        await ExecuteInDb(async dbContext =>
        {
            var movedDepartment = await dbContext.Departments
                .FirstAsync(d => d.Id == departmentToMove.Id, cancellationToken);

            var childDepartment = await dbContext.Departments
                .FirstAsync(cd => cd.Id == child.Id, cancellationToken);

            var subChildDepartment = await dbContext.Departments
                .FirstAsync(scd => scd.Id == subChild.Id, cancellationToken);

            Assert.Null(movedDepartment.ParentId);
            Assert.Equal(movedDepartment.Id.Value, childDepartment.ParentId!.Value);
            Assert.Equal(childDepartment.Id.Value, subChildDepartment.ParentId!.Value);

            Assert.Equal(movedDepth, movedDepartment.Depth);
            Assert.Equal(childDepth, childDepartment.Depth);
            Assert.Equal(subChildDepth, subChildDepartment.Depth);
            Assert.Equal(movedDepartment.Depth + 1, childDepartment.Depth);
            Assert.Equal(childDepartment.Depth + 1, subChildDepartment.Depth);

            Assert.True(movedDepartment.IsActive);
            Assert.True(childDepartment.IsActive);
            Assert.True(subChildDepartment.IsActive);

            var movedExpectedPath =
                Path.CreateParent(movedDepartment.Identifier);
            var childExpectedPath =
                movedExpectedPath.CreateChild(childDepartment.Identifier);
            var subChildExpectedPath =
                childExpectedPath.CreateChild(subChildDepartment.Identifier);
            Assert.Equal(movedExpectedPath.Value, movedDepartment.Path.Value);
            Assert.Equal(childExpectedPath.Value, childDepartment.Path.Value);
            Assert.Equal(subChildExpectedPath.Value, subChildDepartment.Path.Value);

            Assert.Equal(originalDepartmentToMoveUpdatedAt, movedDepartment.UpdatedAt);
            Assert.Equal(originalChildUpdatedAt, childDepartment.UpdatedAt);
            Assert.Equal(originalSubChildUpdatedAt, subChildDepartment.UpdatedAt);

            int departmentsCount = await dbContext.Departments.CountAsync(cancellationToken);
            Assert.Equal(3, departmentsCount);
        });
    }
}