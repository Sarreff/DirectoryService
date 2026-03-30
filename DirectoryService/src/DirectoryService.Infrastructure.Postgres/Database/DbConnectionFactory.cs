using System.Data;
using DirectoryService.Application.Database;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace DirectoryService.Infrastructure.Postgres.Database;

public class DbConnectionFactory : IDisposable, IAsyncDisposable, IDbConnectionFactory
{
    private readonly NpgsqlDataSource _dataSource;

    public DbConnectionFactory(IConfiguration configuration, ILoggerFactory loggerFactory)
    {
        string connectionString = configuration.GetConnectionString(Constants.DATABASE)
                                  ?? throw new InvalidOperationException("Connection string not found");

        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString)
            .UseLoggerFactory(loggerFactory);

        _dataSource = dataSourceBuilder.Build();
    }

    public async Task<IDbConnection> CreateConnectionAsync(CancellationToken cancellationToken = default)
    {
        return await _dataSource.OpenConnectionAsync(cancellationToken);
    }

    public void Dispose()
    {
        _dataSource.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await _dataSource.DisposeAsync();
    }
}