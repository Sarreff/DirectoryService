using CSharpFunctionalExtensions;
using DirectoryService.Application.Departments.GetDepartment;
using DirectoryService.Contracts.Departments;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations.ValueObjects;
using DirectoryService.IntegrationTests.Infrastructure;
using DirectoryService.Shared;

namespace DirectoryService.IntegrationTests.Departments.Get;

public class GetRootDepartmentsTests : DirectoryBaseTests
{
    public GetRootDepartmentsTests(DirectoryTestWebFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task GetRootDepartments_with_children_should_return_correct_total_count()
    {
        // arrange
        LocationId locationId = await CreateValidLocation();

        Department rootDepartment1 = await CreateValidParentDepartment(
            "RootDepartment1",
            "rootOne",
            [locationId]);

        Department childDepartment1 = await CreateValidChildDepartment(
            "ChildDepartment1",
            "childOne",
            rootDepartment1,
            [locationId]);

        Department childDepartment2 = await CreateValidChildDepartment(
            "ChildDepartment2",
            "childTwo",
            rootDepartment1,
            [locationId]);

        Department rootDepartment2 = await CreateValidParentDepartment(
            "RootDepartment2",
            "rootTwo",
            [locationId]);

        Department childDepartment3 = await CreateValidChildDepartment(
            "ChildDepartment3",
            "childThree",
            rootDepartment2,
            [locationId]);

        const int page = 1;
        const int size = 20;
        const int prefetch = 3;
        const int expectedTotalCount = 2;

        CancellationToken cancellationToken = CancellationToken.None;

        // act
        Result<GetRootDepartmentsDto, Errors> result =
            await ExecuteHandler<GetRootDepartmentsHandler, Result<GetRootDepartmentsDto, Errors>>(
                (sut) =>
            {
                var query = new GetRootDepartmentsQuery(page, size, prefetch);
                return sut.Handle(query, cancellationToken);
            });

        // assert
        var roots = result.Value.RootDepartments;
        Assert.Equal(expectedTotalCount, result.Value.TotalCount);

        Assert.All(roots, root => Assert.NotEmpty(root.Children));
        foreach (var root in roots)
        {
            Assert.All(root.Children, child => Assert.Equal(root.Id, child.ParentId));
        }
    }

    [Fact]
    public async Task GetRootDepartments_with_prefetch_limit_should_return_correct_children_count()
    {
        // arrange
        LocationId locationId = await CreateValidLocation();

        Department rootDepartment = await CreateValidParentDepartment(
            "RootDepartment",
            "root",
            [locationId]);

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

        Department childDepartment4 = await CreateValidChildDepartment(
            "ChildDepartment4",
            "childFour",
            rootDepartment,
            [locationId]);

        Department childDepartment5 = await CreateValidChildDepartment(
            "ChildDepartment5",
            "childFive",
            rootDepartment,
            [locationId]);

        const int page = 1;
        const int size = 20;
        const int prefetch = 2;
        const int expectedTotalCount = 1;
        const int expectedChildrenCount = 2;

        CancellationToken cancellationToken = CancellationToken.None;

        // act
        Result<GetRootDepartmentsDto, Errors> result =
            await ExecuteHandler<GetRootDepartmentsHandler, Result<GetRootDepartmentsDto, Errors>>(
                (sut) =>
                {
                    var query = new GetRootDepartmentsQuery(page, size, prefetch);
                    return sut.Handle(query, cancellationToken);
                });

        // assert
        Assert.True(result.IsSuccess);

        Assert.Single(result.Value.RootDepartments);
        Assert.Equal(expectedTotalCount, result.Value.TotalCount);

        var root = result.Value.RootDepartments.First();
        Assert.True(root.HasMoreChildren);
        Assert.Equal(expectedChildrenCount, root.Children.Count);

        foreach (var child in root.Children)
        {
            Assert.Equal(child.ParentId, root.Id);
        }
    }

    [Fact]
    public async Task GetRootDepartments_when_prefetch_equals_children_should_not_have_more_children()
    {
        // arrange
        LocationId locationId = await CreateValidLocation();

        Department rootDepartment = await CreateValidParentDepartment(
            "RootDepartment",
            "root",
            [locationId]);

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

        Department childDepartment4 = await CreateValidChildDepartment(
            "ChildDepartment4",
            "childFour",
            rootDepartment,
            [locationId]);

        Department childDepartment5 = await CreateValidChildDepartment(
            "ChildDepartment5",
            "childFive",
            rootDepartment,
            [locationId]);

        const int page = 1;
        const int size = 20;
        const int prefetch = 5;
        const int expectedTotalCount = 1;
        const int expectedChildrenCount = 5;

        CancellationToken cancellationToken = CancellationToken.None;

        // act
        Result<GetRootDepartmentsDto, Errors> result =
            await ExecuteHandler<GetRootDepartmentsHandler, Result<GetRootDepartmentsDto, Errors>>(
                (sut) =>
                {
                    var query = new GetRootDepartmentsQuery(page, size, prefetch);
                    return sut.Handle(query, cancellationToken);
                });

        // assert
        Assert.True(result.IsSuccess);

        var root = Assert.Single(result.Value.RootDepartments);
        Assert.Equal(expectedTotalCount, result.Value.TotalCount);

        Assert.Equal(expectedChildrenCount, root.Children.Count);
        Assert.False(root.HasMoreChildren);

        foreach (var child in root.Children)
        {
            Assert.Equal(child.ParentId, root.Id);
        }
    }

    [Fact]
    public async Task GetRootDepartments_when_children_less_than_prefetch_should_not_have_more_children()
    {
        // arrange
        LocationId locationId = await CreateValidLocation();

        Department rootDepartment = await CreateValidParentDepartment(
            "RootDepartment",
            "root",
            [locationId]);

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

        const int page = 1;
        const int size = 20;
        const int prefetch = 5;
        const int expectedTotalCount = 1;
        const int expectedChildrenCount = 2;

        CancellationToken cancellationToken = CancellationToken.None;

        // act
        Result<GetRootDepartmentsDto, Errors> result =
            await ExecuteHandler<GetRootDepartmentsHandler, Result<GetRootDepartmentsDto, Errors>>(
                (sut) =>
                {
                    var query = new GetRootDepartmentsQuery(page, size, prefetch);
                    return sut.Handle(query, cancellationToken);
                });

        // assert
        Assert.True(result.IsSuccess);

        var root = Assert.Single(result.Value.RootDepartments);
        Assert.Equal(expectedTotalCount, result.Value.TotalCount);

        Assert.Equal(expectedChildrenCount, root.Children.Count);
        Assert.False(root.HasMoreChildren);

        foreach (var child in root.Children)
        {
            Assert.Equal(child.ParentId, root.Id);
        }
    }

    [Fact]
    public async Task GetRootDepartments_with_zero_prefetch_should_return_no_children_and_has_more_true()
    {
        // arrange
        LocationId locationId = await CreateValidLocation();

        Department rootDepartment = await CreateValidParentDepartment(
            "RootDepartment",
            "root",
            [locationId]);

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

        const int page = 1;
        const int size = 20;
        const int prefetch = 0;
        const int expectedTotalCount = 1;

        CancellationToken cancellationToken = CancellationToken.None;

        // act
        Result<GetRootDepartmentsDto, Errors> result =
            await ExecuteHandler<GetRootDepartmentsHandler, Result<GetRootDepartmentsDto, Errors>>(
                (sut) =>
                {
                    var query = new GetRootDepartmentsQuery(page, size, prefetch);
                    return sut.Handle(query, cancellationToken);
                });

        // assert
        Assert.True(result.IsSuccess);

        var root = Assert.Single(result.Value.RootDepartments);
        Assert.Equal(expectedTotalCount, result.Value.TotalCount);

        Assert.Empty(root.Children);
        Assert.True(root.HasMoreChildren);
    }

    [Fact]
    public async Task GetRootDepartments_should_ignore_inactive_children_in_has_more_logic()
    {
        // arrange
        LocationId locationId = await CreateValidLocation();

        Department rootDepartment = await CreateValidParentDepartment(
            "RootDepartment",
            "root",
            [locationId]);

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

        Department inactiveChildDepartment3 = await CreateValidChildDepartment(
            "ChildDepartment3",
            "childThree",
            rootDepartment,
            [locationId],
            false);

        const int page = 1;
        const int size = 20;
        const int prefetch = 2;
        const int expectedTotalCount = 1;
        const int expectedChildrenCount = 2;

        CancellationToken cancellationToken = CancellationToken.None;

        // act
        Result<GetRootDepartmentsDto, Errors> result =
            await ExecuteHandler<GetRootDepartmentsHandler, Result<GetRootDepartmentsDto, Errors>>(
                (sut) =>
                {
                    var query = new GetRootDepartmentsQuery(page, size, prefetch);
                    return sut.Handle(query, cancellationToken);
                });

        // assert
        Assert.True(result.IsSuccess);

        var root = Assert.Single(result.Value.RootDepartments);
        Assert.Equal(expectedTotalCount, result.Value.TotalCount);

        Assert.Equal(expectedChildrenCount, root.Children.Count);
        Assert.False(root.HasMoreChildren);

        foreach (var child in root.Children)
        {
            Assert.Equal(child.ParentId, root.Id);
        }
    }

    [Fact]
    public async Task GetRootDepartments_prefetch_should_not_load_grandchildren()
    {
        // arrange
        LocationId locationId = await CreateValidLocation();

        Department rootDepartment = await CreateValidParentDepartment(
            "RootDepartment",
            "root",
            [locationId]);

        Department childDepartment = await CreateValidChildDepartment(
            "ChildDepartment",
            "child",
            rootDepartment,
            [locationId]);

        Department grandChildDepartment = await CreateValidChildDepartment(
            "GrandChildDepartment",
            "GrandChild",
            childDepartment,
            [locationId]);

        const int page = 1;
        const int size = 20;
        const int prefetch = 1;
        const int expectedTotalCount = 1;

        CancellationToken cancellationToken = CancellationToken.None;

        // act
        Result<GetRootDepartmentsDto, Errors> result =
            await ExecuteHandler<GetRootDepartmentsHandler, Result<GetRootDepartmentsDto, Errors>>(
                (sut) =>
                {
                    var query = new GetRootDepartmentsQuery(page, size, prefetch);
                    return sut.Handle(query, cancellationToken);
                });

        // assert
        Assert.True(result.IsSuccess);

        var root = Assert.Single(result.Value.RootDepartments);
        Assert.Equal(expectedTotalCount, result.Value.TotalCount);
        Assert.Equal(rootDepartment.Id.Value, root.Id);

        var children = root.Children;
        var child = Assert.Single(children);
        Assert.True(child.HasMoreChildren);
        Assert.Equal(child.Id, childDepartment.Id.Value);
        Assert.Empty(child.Children);
        Assert.Equal(child.ParentId, root.Id);
    }
}