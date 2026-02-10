using DirectoryService.Application.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.Application.Configurations;

public static class ApplicationDependencyInjection
{
    public static IServiceCollection AddApplicationConfiguration(this IServiceCollection services)
    {
        // services.AddValidatorsFromAssembly(typeof(ApplicationDependencyInjection).Assembly);

        var assembly = typeof(ApplicationDependencyInjection).Assembly;

        services.Scan(scan => scan.FromAssemblies(assembly)
            .AddClasses(classes => classes
                .AssignableToAny(typeof(ICommandHandler<,>), typeof(ICommandHandler<>)))
            .AsSelfWithInterfaces()
            .WithScopedLifetime());

        return services;
    }
}