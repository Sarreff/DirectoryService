using CSharpFunctionalExtensions;
using DirectoryService.Application.Departments.MoveDepartment;
using DirectoryService.Contracts.Departments;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Departments.ValueObjects;
using DirectoryService.Domain.Locations.ValueObjects;
using DirectoryService.IntegrationTests.Infrastructure;
using DirectoryService.Shared;

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
        LocationId locationId = await Data.CreateValidLocation();
        Department departmentToMove = await Data.
            CreateValidParentDepartment(
                "MovedDepartment",
                "movedIdentifier",
                [locationId]);

        Department parentDepartment = await Data
            .CreateValidParentDepartment(
                "ParentDepartment",
                "parentIdentifier",
                [locationId]);

        const int movedDepth = 1;
        const int parentDepth = 0;

        DateTime originalDepartmentToMoveUpdatedAt = await Data.GetDepartmentUpdatedAt(departmentToMove.Id);

        CancellationToken cancellationToken = CancellationToken.None;

        // act
        Result<Guid, Errors> result =
            await ExecuteHandler<MoveDepartmentHandler, Result<Guid, Errors>>((sut) =>
            {
                var command =
                    new MoveDepartmentCommand(
                        departmentToMove.Id.Value,
                        new MoveDepartmentRequest(parentDepartment.Id.Value));

                return sut.Handle(command, cancellationToken);
            });

        // assert
        await AssertDb.DepartmentMovedToParent(
            result,
            parentDepartment.Id,
            movedDepth,
            parentDepth,
            originalDepartmentToMoveUpdatedAt,
            cancellationToken);
    }

    [Fact]
    public async Task MoveDepartment_without_children_to_root_should_succeed()
    {
        // arrange
        LocationId locationId = await Data.CreateValidLocation();

        Department parentDepartment = await Data
            .CreateValidParentDepartment(
                "ParentDepartment",
                "parentIdentifier",
                [locationId]);

        Department departmentToMove = await Data
            .CreateValidChildDepartment(
                "MovedDepartment",
                "movedIdentifier",
                parentDepartment,
                [locationId]);

        const int movedDepth = 0;
        const int parentDepth = 0;

        DateTime originalDepartmentToMoveUpdatedAt = await Data.GetDepartmentUpdatedAt(departmentToMove.Id);

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
        await AssertDb.DepartmentMovedToRoot(
            result,
            parentDepartment.Id,
            movedDepth,
            parentDepth,
            originalDepartmentToMoveUpdatedAt,
            cancellationToken);
    }

    [Fact]
    public async Task MoveDepartment_with_children_to_parent_should_succeed()
    {
        // arrange
        LocationId locationId = await Data.CreateValidLocation();

        Department departmentToMove = await Data
            .CreateValidParentDepartment(
                "MovedDepartment",
                "movedIdentifier",
                [locationId]);

        Department child = await Data
            .CreateValidChildDepartment(
                "MovedChildDepartment",
                "movedChildIdentifier",
                departmentToMove,
                [locationId]);

        Department subChild = await Data
            .CreateValidChildDepartment(
                "MovedSubChildDepartment",
                "movedSubChildIdentifier",
                child,
                [locationId]);

        Department parentDepartment = await Data
            .CreateValidParentDepartment(
                "ParentDepartment",
                "parentIdentifier",
                [locationId]);

        const int movedDepth = 1;
        const int parentDepth = 0;
        const int childDepth = 2;
        const int subChildDepth = 3;

        DateTime originalDepartmentToMoveUpdatedAt = await Data.GetDepartmentUpdatedAt(departmentToMove.Id);
        DateTime originalChildUpdatedAt = await Data.GetDepartmentUpdatedAt(child.Id);
        DateTime originalSubChildUpdatedAt = await Data.GetDepartmentUpdatedAt(subChild.Id);

        CancellationToken cancellationToken = CancellationToken.None;

        // act
        Result<Guid, Errors> result =
            await ExecuteHandler<MoveDepartmentHandler, Result<Guid, Errors>>((sut) =>
            {
                var command =
                    new MoveDepartmentCommand(
                        departmentToMove.Id.Value,
                        new MoveDepartmentRequest(parentDepartment.Id.Value));

                return sut.Handle(command, cancellationToken);
            });

        // assert
        await AssertDb.DepartmentWithChildrenMovedToParent(
            result,
            parentDepartment.Id,
            child.Id,
            subChild.Id,
            movedDepth,
            parentDepth,
            childDepth,
            subChildDepth,
            originalDepartmentToMoveUpdatedAt,
            originalChildUpdatedAt,
            originalSubChildUpdatedAt,
            cancellationToken);
    }

    [Fact]
    public async Task MoveDepartment_with_children_to_root_should_succeed()
    {
        // arrange
        LocationId locationId = await Data.CreateValidLocation();

        Department parentDepartment = await Data
            .CreateValidParentDepartment(
                "ParentDepartment",
                "parentIdentifier",
                [locationId]);

        Department departmentToMove = await Data
            .CreateValidChildDepartment(
                "MovedDepartment",
                "movedIdentifier",
                parentDepartment,
                [locationId]);

        Department child = await Data
            .CreateValidChildDepartment(
                "MovedChildDepartment",
                "movedChildIdentifier",
                departmentToMove,
                [locationId]);

        Department subChild = await Data
            .CreateValidChildDepartment(
                "MovedSubChildDepartment",
                "movedSubChildIdentifier",
                child,
                [locationId]);

        const int movedDepth = 0;
        const int parentDepth = 0;
        const int childDepth = 1;
        const int subChildDepth = 2;

        DateTime originalDepartmentToMoveUpdatedAt = await Data.GetDepartmentUpdatedAt(departmentToMove.Id);
        DateTime originalChildUpdatedAt = await Data.GetDepartmentUpdatedAt(child.Id);
        DateTime originalSubChildUpdatedAt = await Data.GetDepartmentUpdatedAt(subChild.Id);

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
        await AssertDb.DepartmentWithChildrenMovedToRoot(
            result,
            parentDepartment.Id,
            child.Id,
            subChild.Id,
            movedDepth,
            parentDepth,
            childDepth,
            subChildDepth,
            originalDepartmentToMoveUpdatedAt,
            originalChildUpdatedAt,
            originalSubChildUpdatedAt,
            cancellationToken);
    }

    [Fact]
    public async Task MoveDepartment_with_cyclical_dependency_should_return_conflict_error()
    {
        // arrange
        LocationId locationId = await Data.CreateValidLocation();

        Department departmentToMove = await Data
            .CreateValidParentDepartment(
                "MovedDepartment",
                "movedIdentifier",
                [locationId]);

        Department child = await Data
            .CreateValidChildDepartment(
                "MovedChildDepartment",
                "movedChildIdentifier",
                departmentToMove,
                [locationId]);

        Department subChild = await Data
            .CreateValidChildDepartment(
                "MovedSubChildDepartment",
                "movedSubChildIdentifier",
                child,
                [locationId]);

        const short movedDepth = 0;
        const short childDepth = 1;
        const short subChildDepth = 2;

        DateTime originalDepartmentToMoveUpdatedAt = await Data.GetDepartmentUpdatedAt(departmentToMove.Id);
        DateTime originalChildUpdatedAt = await Data.GetDepartmentUpdatedAt(child.Id);
        DateTime originalSubChildUpdatedAt = await Data.GetDepartmentUpdatedAt(subChild.Id);

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
        await AssertDb.DepartmentMoveRejected(
            result,
            departmentToMove.Id,
            child.Id,
            subChild.Id,
            movedDepth,
            childDepth,
            subChildDepth,
            originalDepartmentToMoveUpdatedAt,
            originalChildUpdatedAt,
            originalSubChildUpdatedAt,
            ErrorType.CONFLICT,
            cancellationToken);
    }

    [Fact]
    public async Task MoveDepartment_to_same_parent_should_change_nothing()
    {
        // arrange
        LocationId locationId = await Data.CreateValidLocation();

        Department parentDepartment = await Data
            .CreateValidParentDepartment(
                "ParentDepartment",
                "parentIdentifier",
                [locationId]);

        Department departmentToMove = await Data
            .CreateValidChildDepartment(
                "MovedDepartment",
                "movedIdentifier",
                parentDepartment,
                [locationId]);

        Department child = await Data
            .CreateValidChildDepartment(
                "MovedChildDepartment",
                "movedChildIdentifier",
                departmentToMove,
                [locationId]);

        Department subChild = await Data
            .CreateValidChildDepartment(
                "MovedSubChildDepartment",
                "movedSubChildIdentifier",
                child,
                [locationId]);

        const int parentDepth = 0;
        const int movedDepth = 1;
        const int childDepth = 2;
        const int subChildDepth = 3;

        DateTime originalParentUpdatedAt = await Data.GetDepartmentUpdatedAt(parentDepartment.Id);
        DateTime originalDepartmentToMoveUpdatedAt = await Data.GetDepartmentUpdatedAt(departmentToMove.Id);
        DateTime originalChildUpdatedAt = await Data.GetDepartmentUpdatedAt(child.Id);
        DateTime originalSubChildUpdatedAt = await Data.GetDepartmentUpdatedAt(subChild.Id);

        CancellationToken cancellationToken = CancellationToken.None;

        // act
        Result<Guid, Errors> result =
            await ExecuteHandler<MoveDepartmentHandler, Result<Guid, Errors>>((sut) =>
            {
                var command =
                    new MoveDepartmentCommand(
                        departmentToMove.Id.Value,
                        new MoveDepartmentRequest(parentDepartment.Id.Value));

                return sut.Handle(command, cancellationToken);
            });

        // assert
        await AssertDb.DepartmentMoveNothingChanged(
            result,
            parentDepartment.Id,
            child.Id,
            subChild.Id,
            movedDepth,
            parentDepth,
            childDepth,
            subChildDepth,
            originalParentUpdatedAt,
            originalDepartmentToMoveUpdatedAt,
            originalChildUpdatedAt,
            originalSubChildUpdatedAt,
            cancellationToken);
    }

    [Fact]
    public async Task MoveDepartment_to_itself_should_fail()
    {
        // arrange
        LocationId locationId = await Data.CreateValidLocation();

        Department departmentToMove = await Data
            .CreateValidParentDepartment(
                "MovedDepartment",
                "movedIdentifier",
                [locationId]);

        Department child = await Data
            .CreateValidChildDepartment(
                "MovedChildDepartment",
                "movedChildIdentifier",
                departmentToMove,
                [locationId]);

        Department subChild = await Data
            .CreateValidChildDepartment(
                "MovedSubChildDepartment",
                "movedSubChildIdentifier",
                child,
                [locationId]);

        const int movedDepth = 0;
        const int childDepth = 1;
        const int subChildDepth = 2;

        DateTime originalDepartmentToMoveUpdatedAt = await Data.GetDepartmentUpdatedAt(departmentToMove.Id);
        DateTime originalChildUpdatedAt = await Data.GetDepartmentUpdatedAt(child.Id);
        DateTime originalSubChildUpdatedAt = await Data.GetDepartmentUpdatedAt(subChild.Id);

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
        await AssertDb.DepartmentMoveRejected(
            result,
            departmentToMove.Id,
            child.Id,
            subChild.Id,
            movedDepth,
            childDepth,
            subChildDepth,
            originalDepartmentToMoveUpdatedAt,
            originalChildUpdatedAt,
            originalSubChildUpdatedAt,
            ErrorType.VALIDATION,
            cancellationToken);
    }

    [Fact]
    public async Task MoveDepartment_to_not_exist_parent_should_fail()
    {
        // arrange
        LocationId locationId = await Data.CreateValidLocation();

        DepartmentId parentId = Data.CreateNotExistDepartment();

        Department departmentToMove = await Data
            .CreateValidParentDepartment(
                "MovedDepartment",
                "movedIdentifier",
                [locationId]);

        Department departmentChild = await Data
            .CreateValidChildDepartment(
                "MovedChildDepartment",
                "movedChildIdentifier",
                departmentToMove,
                [locationId]);

        Department departmentSubChild = await Data
            .CreateValidChildDepartment(
                "MovedSubChildDepartment",
                "movedSubChildIdentifier",
                departmentChild,
                [locationId]);

        const short movedDepth = 0;
        const short childDepth = 1;
        const short subChildDepth = 2;

        DateTime originalDepartmentToMoveUpdatedAt = await Data.GetDepartmentUpdatedAt(departmentToMove.Id);
        DateTime originalChildUpdatedAt = await Data.GetDepartmentUpdatedAt(departmentChild.Id);
        DateTime originalSubChildUpdatedAt = await Data.GetDepartmentUpdatedAt(departmentSubChild.Id);

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
        await AssertDb.DepartmentMoveRejected(
            result,
            departmentToMove.Id,
            departmentChild.Id,
            departmentSubChild.Id,
            movedDepth,
            childDepth,
            subChildDepth,
            originalDepartmentToMoveUpdatedAt,
            originalChildUpdatedAt,
            originalSubChildUpdatedAt,
            ErrorType.NOT_FOUND,
            cancellationToken,
            parentId);
    }
}