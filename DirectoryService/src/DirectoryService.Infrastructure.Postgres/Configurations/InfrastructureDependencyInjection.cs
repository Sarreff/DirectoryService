using DirectoryService.Application.Database;
using DirectoryService.Application.Departments;
using DirectoryService.Application.Locations;
using DirectoryService.Application.Positions;
using DirectoryService.Infrastructure.Postgres.Database;
using DirectoryService.Infrastructure.Postgres.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.Infrastructure.Postgres.Configurations;

public static class InfrastructureDependencyInjection
{
    public static IServiceCollection AddInfrastructureConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContextPool<DirectoryServiceDbContext>((sp, options) =>
        {
            string connectionString = configuration.GetConnectionString(Constants.DATABASE)
                                      ?? throw new InvalidOperationException("Connection string not found");

            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.SetPostgresVersion(14, 0);

                // Конфликт из-за TransactionManager, пока убрал
                // npgsqlOptions.EnableRetryOnFailure(null);
            });

            options.ConfigureWarnings(warnings =>
                warnings.Ignore(RelationalEventId.CommandError));

            string? environment = configuration["ASPNETCORE_ENVIRONMENT"];
            if (environment != "Development")
            {
                return;
            }

            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
        });

        services.AddScoped<IDepartmentsRepository, DepartmentsEfCoreRepository>();
        services.AddScoped<ILocationsRepository, LocationsEfCoreRepository>();
        services.AddScoped<IPositionsRepository, PositionsEfCoreRepository>();
        services.AddScoped<ITransactionManager, TransactionManager>();

        return services;
    }
}