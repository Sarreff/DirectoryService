using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.Infrastructure.Postgres.Configurations;

public static class InfrastructureDependencyInjection
{
    public static IServiceCollection AddInfrastructureConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<DirectoryServiceDbContext>(_ =>
            new DirectoryServiceDbContext(configuration.GetConnectionString("DirectoryServiceDb")!));

        return services;
    }
}