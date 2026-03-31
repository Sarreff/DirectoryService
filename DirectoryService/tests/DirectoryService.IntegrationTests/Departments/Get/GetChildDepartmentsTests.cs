using CSharpFunctionalExtensions;
using DirectoryService.Application.Departments.GetDepartment;
using DirectoryService.Contracts.Departments;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations.ValueObjects;
using DirectoryService.IntegrationTests.Infrastructure;
using DirectoryService.Shared;

namespace DirectoryService.IntegrationTests.Departments.Get;

public class GetChildDepartmentsTests : DirectoryBaseTests
{
    public GetChildDepartmentsTests(DirectoryTestWebFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task GetChildDepartments_should_return_all_children_for_given_parent()
    {
        // arrange
        LocationId locationId = await CreateValidLocation();

        Department rootDepartment = await CreateValidParentDepartment(
            "RootDepartment",
            "root",
            [locationId]);
        Guid rootId = rootDepartment.Id.Value;

        Department childDepartment1 = await CreateValidChildDepartment(
            "ChildDepartment1",
            "childOne",
            rootDepartment,
            [locationId]);

        Department childDepartment2 = await CreateValidChildDepartment(
            "ChildDepartment2",
            "childTwo",
            rootDepartment,
            [locationId]);

        Department childDepartment3 = await CreateValidChildDepartment(
            "ChildDepartment3",
            "childThree",
            rootDepartment,
            [locationId]);

        const int page = 1;
        const int size = 20;
        const int expectedChildrenCount = 3;

        CancellationToken cancellationToken = CancellationToken.None;

        // act
        Result<GetChildDepartmentsDto, Errors> result =
            await ExecuteHandler<GetChildDepartmentsHandler, Result<GetChildDepartmentsDto, Errors>>(
                (sut) =>
                {
                    var query = new GetChildDepartmentsQuery(rootId, page, size);
                    return sut.Handle(query, cancellationToken);
                });

        // assert
        var children = result.Value.ChildDepartments;
        Assert.Equal(expectedChildrenCount, result.Value.TotalCount);

        foreach (var child in children)
        {
            Assert.Equal(child.ParentId, rootId);
        }
    }

    [Fact]
    public async Task GetChildDepartments_with_page_size_should_return_correct_children_and_total_count()
    {
        // arrange
        LocationId locationId = await CreateValidLocation();

        Department rootDepartment = await CreateValidParentDepartment(
            "RootDepartment",
            "root",
            [locationId]);
        Guid rootId = rootDepartment.Id.Value;

        Department childDepartment1 = await CreateValidChildDepartment(
            "ChildDepartment1",
            "childOne",
            rootDepartment,
            [locationId]);

        Department childDepartment2 = await CreateValidChildDepartment(
            "ChildDepartment2",
            "childTwo",
            rootDepartment,
            [locationId]);

        Department childDepartment3 = await CreateValidChildDepartment(
            "ChildDepartment3",
            "childThree",
            rootDepartment,
            [locationId]);

        const int page = 1;
        const int size = 2;
        const int expectedChildrenCount = 2;
        const int expectedTotalChildrenCount = 3;

        CancellationToken cancellationToken = CancellationToken.None;

        // act
        Result<GetChildDepartmentsDto, Errors> result =
            await ExecuteHandler<GetChildDepartmentsHandler, Result<GetChildDepartmentsDto, Errors>>(
                (sut) =>
                {
                    var query = new GetChildDepartmentsQuery(rootId, page, size);
                    return sut.Handle(query, cancellationToken);
                });

        // assert
        var children = result.Value.ChildDepartments;
        Assert.Equal(expectedChildrenCount, result.Value.ChildDepartments.Count);
        Assert.Equal(expectedTotalChildrenCount, result.Value.TotalCount);

        foreach (var child in children)
        {
            Assert.Equal(child.ParentId, rootId);
        }
    }

    [Fact]
    public async Task GetChildDepartments_should_set_HasMoreChildren_when_children_have_grandchildren()
    {
        // arrange
        LocationId locationId = await CreateValidLocation();

        Department rootDepartment = await CreateValidParentDepartment(
            "RootDepartment",
            "root",
            [locationId]);
        Guid rootId = rootDepartment.Id.Value;

        Department childDepartment = await CreateValidChildDepartment(
            "ChildDepartment",
            "child",
            rootDepartment,
            [locationId]);

        Department grandChild = await CreateValidChildDepartment(
            "GrandChild",
            "grandChild",
            childDepartment,
            [locationId]);

        const int page = 1;
        const int size = 20;
        const int expectedTotalChildrenCount = 1;

        CancellationToken cancellationToken = CancellationToken.None;

        // act
        Result<GetChildDepartmentsDto, Errors> result =
            await ExecuteHandler<GetChildDepartmentsHandler, Result<GetChildDepartmentsDto, Errors>>(
                (sut) =>
                {
                    var query = new GetChildDepartmentsQuery(rootId, page, size);
                    return sut.Handle(query, cancellationToken);
                });

        // assert
        var child = Assert.Single(result.Value.ChildDepartments);
        Assert.Equal(expectedTotalChildrenCount, result.Value.TotalCount);
        Assert.Equal(child.ParentId, rootId);
        Assert.True(child.HasMoreChildren);
    }

    [Fact]
    public async Task GetChildDepartments_should_return_empty_when_parent_has_no_children()
    {
        // arrange
        LocationId locationId = await CreateValidLocation();

        Department rootDepartment = await CreateValidParentDepartment(
            "RootDepartment",
            "root",
            [locationId]);
        Guid rootId = rootDepartment.Id.Value;

        const int page = 1;
        const int size = 20;
        const int expectedTotalChildrenCount = 0;

        CancellationToken cancellationToken = CancellationToken.None;

        // act
        Result<GetChildDepartmentsDto, Errors> result =
            await ExecuteHandler<GetChildDepartmentsHandler, Result<GetChildDepartmentsDto, Errors>>(
                (sut) =>
                {
                    var query = new GetChildDepartmentsQuery(rootId, page, size);
                    return sut.Handle(query, cancellationToken);
                });

        // assert
        Assert.Empty(result.Value.ChildDepartments);
        Assert.Equal(expectedTotalChildrenCount, result.Value.TotalCount);
    }

    [Fact]
    public async Task GetChildDepartments_should_exclude_inactive_children()
    {
        // arrange
        LocationId locationId = await CreateValidLocation();

        Department rootDepartment = await CreateValidParentDepartment(
            "RootDepartment",
            "root",
            [locationId]);
        Guid rootId = rootDepartment.Id.Value;

        Department childDepartment1 = await CreateValidChildDepartment(
            "ChildDepartment1",
            "childOne",
            rootDepartment,
            [locationId]);

        Department childDepartment2 = await CreateValidChildDepartment(
            "ChildDepartment2",
            "childTwo",
            rootDepartment,
            [locationId]);

        Department childDepartment3 = await CreateValidChildDepartment(
            "ChildDepartment3",
            "childThree",
            rootDepartment,
            [locationId],
            false);

        const int page = 1;
        const int size = 20;
        const int expectedChildrenCount = 2;
        const int expectedTotalChildrenCount = 2;

        CancellationToken cancellationToken = CancellationToken.None;

        // act
        Result<GetChildDepartmentsDto, Errors> result =
            await ExecuteHandler<GetChildDepartmentsHandler, Result<GetChildDepartmentsDto, Errors>>(
                (sut) =>
                {
                    var query = new GetChildDepartmentsQuery(rootId, page, size);
                    return sut.Handle(query, cancellationToken);
                });

        // assert
        var children = result.Value.ChildDepartments;
        Assert.Equal(expectedChildrenCount, result.Value.ChildDepartments.Count);
        Assert.Equal(expectedTotalChildrenCount, result.Value.TotalCount);

        foreach (var child in children)
        {
            Assert.Equal(child.ParentId, rootId);
        }
    }
}