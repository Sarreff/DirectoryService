using System.Data;
using DirectoryService.Application.Database;
using Npgsql;

namespace DirectoryService.IntegrationTests.Infrastructure;

public class TestDbConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public TestDbConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    public Task<IDbConnection> CreateConnectionAsync(CancellationToken cancellationToken = default)
    {
        var conn = new NpgsqlConnection(_connectionString);
        return Task.FromResult<IDbConnection>(conn);
    }
}