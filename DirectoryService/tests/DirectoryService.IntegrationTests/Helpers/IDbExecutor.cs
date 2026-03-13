using DirectoryService.Infrastructure.Postgres;

namespace DirectoryService.IntegrationTests.Helpers;

public interface IDbExecutor
{
    Task<T> ExecuteInDb<T>(Func<DirectoryServiceDbContext, Task<T>> action);

    Task ExecuteInDb(Func<DirectoryServiceDbContext, Task> action);
}