using CSharpFunctionalExtensions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Departments.ValueObjects;
using DirectoryService.Shared;
using Microsoft.EntityFrameworkCore;
using Path = DirectoryService.Domain.Departments.ValueObjects.Path;

namespace DirectoryService.IntegrationTests.Departments;

public partial class DepartmentAssertions
{
    // Перемещение департамента (без дочерних) к родителю
    public async Task DepartmentMovedToParent(
        Result<Guid, Errors> result,
        DepartmentId parentId,
        short expectedMovedDeptDepth,
        short expectedParentDeptDepth,
        DateTime originalDeptMovedUpdatedAt,
        CancellationToken cancellationToken)
    {
        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);

        await _db.ExecuteInDb(async dbContext =>
        {
            var movedDepartment = await dbContext.Departments
                .FirstAsync(d => d.Id == new DepartmentId(result.Value), cancellationToken);

            var parentDepartment = await dbContext.Departments
                .FirstAsync(d => d.Id == parentId, cancellationToken);

            Assert.Equal(result.Value, movedDepartment.Id.Value);
            Assert.NotEqual(movedDepartment.Id.Value, parentDepartment.Id.Value);

            Assert.NotNull(movedDepartment.ParentId);
            Assert.Equal(parentDepartment.Id.Value, movedDepartment.ParentId.Value);

            Assert.Equal(expectedMovedDeptDepth, movedDepartment.Depth);
            Assert.Equal(expectedParentDeptDepth, parentDepartment.Depth);
            Assert.Equal(parentDepartment.Depth + 1, movedDepartment.Depth);

            Assert.True(movedDepartment.IsActive);
            Assert.True(parentDepartment.IsActive);

            Assert.True(movedDepartment.UpdatedAt > originalDeptMovedUpdatedAt);

            var expectedPath = parentDepartment.Path.CreateChild(movedDepartment.Identifier);
            var expectedParentPath = Path.CreateParent(parentDepartment.Identifier);
            Assert.Equal(expectedPath.Value, movedDepartment.Path.Value);
            Assert.Equal(expectedParentPath.Value, parentDepartment.Path.Value);

            int departmentsCount = await dbContext.Departments.CountAsync(cancellationToken);
            Assert.Equal(2, departmentsCount);
        });
    }

    // Перемещение департамента (без дочерних) в корень
    public async Task DepartmentMovedToRoot(
        Result<Guid, Errors> result,
        DepartmentId parentId,
        short expectedMovedDeptDepth,
        short expectedParentDeptDepth,
        DateTime originalDeptMovedUpdatedAt,
        CancellationToken cancellationToken)
    {
        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);

        await _db.ExecuteInDb(async dbContext =>
        {
            var movedDepartment = await dbContext.Departments
                .FirstAsync(d => d.Id == new DepartmentId(result.Value), cancellationToken);

            var parentDepartment = await dbContext.Departments
                .FirstAsync(d => d.Id == parentId, cancellationToken);

            Assert.Equal(result.Value, movedDepartment.Id.Value);
            Assert.NotEqual(movedDepartment.Id.Value, parentDepartment.Id.Value);

            Assert.Null(movedDepartment.ParentId);

            Assert.Equal(expectedMovedDeptDepth, movedDepartment.Depth);
            Assert.Equal(expectedParentDeptDepth, parentDepartment.Depth);

            Assert.True(movedDepartment.IsActive);
            Assert.True(parentDepartment.IsActive);

            Assert.True(movedDepartment.UpdatedAt > originalDeptMovedUpdatedAt);

            var expectedPath = Path.CreateParent(movedDepartment.Identifier);
            var expectedParentPath = Path.CreateParent(parentDepartment.Identifier);
            Assert.Equal(expectedPath.Value, movedDepartment.Path.Value);
            Assert.Equal(expectedParentPath.Value, parentDepartment.Path.Value);

            int departmentsCount = await dbContext.Departments.CountAsync(cancellationToken);
            Assert.Equal(2, departmentsCount);
        });
    }

    // Перемещение департамента (с дочерними) к родителю
    public async Task DepartmentWithChildrenMovedToParent(
        Result<Guid, Errors> result,
        DepartmentId parentId,
        DepartmentId childId,
        DepartmentId subChildId,
        short expectedMovedDeptDepth,
        short expectedParentDepth,
        short expectedChildDepth,
        short expectedSubChildDepth,
        DateTime originalDeptMovedUpdatedAt,
        DateTime originalChildUpdatedAt,
        DateTime originalSubChildUpdatedAt,
        CancellationToken cancellationToken)
    {
        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);

        await _db.ExecuteInDb(async dbContext =>
        {
            var movedDepartment = await dbContext.Departments
                .FirstAsync(d => d.Id == new DepartmentId(result.Value), cancellationToken);

            var childDepartment = await dbContext.Departments
                .FirstAsync(cd => cd.Id == childId, cancellationToken);

            var subChildDepartment = await dbContext.Departments
                .FirstAsync(scd => scd.Id == subChildId, cancellationToken);

            var parentDepartment = await dbContext.Departments
                .FirstAsync(d => d.Id == parentId, cancellationToken);

            Assert.Equal(result.Value, movedDepartment.Id.Value);

            Assert.NotNull(movedDepartment.ParentId);
            Assert.Equal(parentDepartment.Id.Value, movedDepartment.ParentId!.Value);
            Assert.Equal(movedDepartment.Id.Value, childDepartment.ParentId!.Value);
            Assert.Equal(childDepartment.Id.Value, subChildDepartment.ParentId!.Value);

            Assert.Equal(expectedMovedDeptDepth, movedDepartment.Depth);
            Assert.Equal(expectedChildDepth, childDepartment.Depth);
            Assert.Equal(expectedSubChildDepth, subChildDepartment.Depth);
            Assert.Equal(expectedParentDepth, parentDepartment.Depth);
            Assert.Equal(parentDepartment.Depth + 1, movedDepartment.Depth);
            Assert.Equal(movedDepartment.Depth + 1, childDepartment.Depth);
            Assert.Equal(childDepartment.Depth + 1, subChildDepartment.Depth);

            Assert.True(movedDepartment.IsActive);
            Assert.True(childDepartment.IsActive);
            Assert.True(subChildDepartment.IsActive);
            Assert.True(parentDepartment.IsActive);

            Assert.True(movedDepartment.UpdatedAt > originalDeptMovedUpdatedAt);
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

    // Перемещение департамента (с дочерними) в корень
    public async Task DepartmentWithChildrenMovedToRoot(
        Result<Guid, Errors> result,
        DepartmentId parentId,
        DepartmentId childId,
        DepartmentId subChildId,
        short expectedMovedDeptDepth,
        short expectedParentDepth,
        short expectedChildDepth,
        short expectedSubChildDepth,
        DateTime originalDeptMovedUpdatedAt,
        DateTime originalChildUpdatedAt,
        DateTime originalSubChildUpdatedAt,
        CancellationToken cancellationToken)
    {
        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);

        await _db.ExecuteInDb(async dbContext =>
        {
            var movedDepartment = await dbContext.Departments
                .FirstAsync(d => d.Id == new DepartmentId(result.Value), cancellationToken);

            var childDepartment = await dbContext.Departments
                .FirstAsync(cd => cd.Id == childId, cancellationToken);

            var subChildDepartment = await dbContext.Departments
                .FirstAsync(scd => scd.Id == subChildId, cancellationToken);

            var parentDepartment = await dbContext.Departments
                .FirstAsync(d => d.Id == parentId, cancellationToken);

            Assert.Equal(result.Value, movedDepartment.Id.Value);

            Assert.Null(movedDepartment.ParentId);
            Assert.Equal(movedDepartment.Id.Value, childDepartment.ParentId!.Value);
            Assert.Equal(childDepartment.Id.Value, subChildDepartment.ParentId!.Value);

            Assert.Equal(expectedMovedDeptDepth, movedDepartment.Depth);
            Assert.Equal(expectedChildDepth, childDepartment.Depth);
            Assert.Equal(expectedSubChildDepth, subChildDepartment.Depth);
            Assert.Equal(expectedParentDepth, parentDepartment.Depth);
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

            Assert.True(movedDepartment.UpdatedAt > originalDeptMovedUpdatedAt);
            Assert.True(childDepartment.UpdatedAt > originalChildUpdatedAt);
            Assert.True(subChildDepartment.UpdatedAt > originalSubChildUpdatedAt);

            int departmentsCount = await dbContext.Departments.CountAsync(cancellationToken);
            Assert.Equal(4, departmentsCount);
        });
    }

    // Перемещение департамента отменено
    public async Task DepartmentMoveRejected(
        Result<Guid, Errors> result,
        DepartmentId departmentId,
        DepartmentId childId,
        DepartmentId subChildId,
        short expectedMovedDeptDepth,
        short expectedChildDepth,
        short expectedSubChildDepth,
        DateTime originalDeptMovedUpdatedAt,
        DateTime originalChildUpdatedAt,
        DateTime originalSubChildUpdatedAt,
        ErrorType type,
        CancellationToken cancellationToken,
        DepartmentId? parentId = null)
    {
        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Error);

        Assert.Contains(result.Error, e => e.Type == type);

        await _db.ExecuteInDb(async dbContext =>
        {
            var movedDepartment = await dbContext.Departments
                .FirstAsync(d => d.Id == departmentId, cancellationToken);

            var childDepartment = await dbContext.Departments
                .FirstAsync(cd => cd.Id == childId, cancellationToken);

            var subChildDepartment = await dbContext.Departments
                .FirstAsync(scd => scd.Id == subChildId, cancellationToken);

            Assert.Null(movedDepartment.ParentId);
            Assert.Equal(movedDepartment.Id.Value, childDepartment.ParentId!.Value);
            Assert.Equal(childDepartment.Id.Value, subChildDepartment.ParentId!.Value);

            Assert.Equal(expectedMovedDeptDepth, movedDepartment.Depth);
            Assert.Equal(expectedChildDepth, childDepartment.Depth);
            Assert.Equal(expectedSubChildDepth, subChildDepartment.Depth);
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

            Assert.Equal(originalDeptMovedUpdatedAt, movedDepartment.UpdatedAt);
            Assert.Equal(originalChildUpdatedAt, childDepartment.UpdatedAt);
            Assert.Equal(originalSubChildUpdatedAt, subChildDepartment.UpdatedAt);

            int departmentsCount = await dbContext.Departments.CountAsync(cancellationToken);
            Assert.Equal(3, departmentsCount);
        });
    }

    // Ошибочное перемещение департамента завершилось без изменений
    public async Task DepartmentMoveNothingChanged(
        Result<Guid, Errors> result,
        DepartmentId parentId,
        DepartmentId childId,
        DepartmentId subChildId,
        short expectedMovedDeptDepth,
        short expectedParentDepth,
        short expectedChildDepth,
        short expectedSubChildDepth,
        DateTime originalParentUpdatedAt,
        DateTime originalDeptMovedUpdatedAt,
        DateTime originalChildUpdatedAt,
        DateTime originalSubChildUpdatedAt,
        CancellationToken cancellationToken)
    {
        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);

        await _db.ExecuteInDb(async dbContext =>
        {
            var movedDepartment = await dbContext.Departments
                .FirstAsync(d => d.Id == new DepartmentId(result.Value), cancellationToken);

            var childDepartment = await dbContext.Departments
                .FirstAsync(cd => cd.Id == childId, cancellationToken);

            var subChildDepartment = await dbContext.Departments
                .FirstAsync(scd => scd.Id == subChildId, cancellationToken);

            var parentDepartment = await dbContext.Departments
                .FirstAsync(d => d.Id == parentId, cancellationToken);

            Assert.Equal(result.Value, movedDepartment.Id.Value);

            Assert.Null(parentDepartment.ParentId);
            Assert.Equal(parentDepartment.Id.Value, movedDepartment.ParentId!.Value);
            Assert.Equal(movedDepartment.Id.Value, childDepartment.ParentId!.Value);
            Assert.Equal(childDepartment.Id.Value, subChildDepartment.ParentId!.Value);

            Assert.Equal(expectedMovedDeptDepth, movedDepartment.Depth);
            Assert.Equal(expectedChildDepth, childDepartment.Depth);
            Assert.Equal(expectedSubChildDepth, subChildDepartment.Depth);
            Assert.Equal(expectedParentDepth, parentDepartment.Depth);
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
            Assert.Equal(originalDeptMovedUpdatedAt, movedDepartment.UpdatedAt);
            Assert.Equal(originalChildUpdatedAt, childDepartment.UpdatedAt);
            Assert.Equal(originalSubChildUpdatedAt, subChildDepartment.UpdatedAt);

            int departmentsCount = await dbContext.Departments.CountAsync(cancellationToken);
            Assert.Equal(4, departmentsCount);
        });
    }
}