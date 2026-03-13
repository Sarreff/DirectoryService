using DirectoryService.Infrastructure.Postgres;
using DirectoryService.IntegrationTests.Helpers;
using Microsoft.Extensions.DependencyInjection;
using DepartmentAssertions = DirectoryService.IntegrationTests.Departments.DepartmentAssertions;

namespace DirectoryService.IntegrationTests.Infrastructure;

public class DirectoryBaseTests : IClassFixture<DirectoryTestWebFactory>, IAsyncLifetime, IDbExecutor
{
    protected readonly DepartmentAssertions AssertDb;
    protected readonly TestDataBuilder Data;
    private readonly DirectoryTestWebFactory _factory;
    private readonly Func<Task> _resetDatabase;

    protected DirectoryBaseTests(DirectoryTestWebFactory factory)
    {
        _factory = factory;
        _resetDatabase = factory.ResetDatabaseAsync;
        Data = new TestDataBuilder(this);
        AssertDb = new DepartmentAssertions(this);
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await _resetDatabase();
    }

    public async Task<T> ExecuteInDb<T>(Func<DirectoryServiceDbContext, Task<T>> action)
    {
        await using var scope = _factory.Services.CreateAsyncScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<DirectoryServiceDbContext>();

        return await action(dbContext);
    }

    public async Task ExecuteInDb(Func<DirectoryServiceDbContext, Task> action)
    {
        await using var scope = _factory.Services.CreateAsyncScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<DirectoryServiceDbContext>();

        await action(dbContext);
    }

    protected async Task<T> ExecuteHandler<TService, T>(Func<TService, Task<T>> action)
        where TService : notnull
    {
        await using var scope = _factory.Services.CreateAsyncScope();

        var sut = scope.ServiceProvider.GetRequiredService<TService>();

        return await action(sut);
    }
}