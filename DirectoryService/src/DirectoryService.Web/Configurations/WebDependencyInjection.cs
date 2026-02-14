using DirectoryService.Presentation.Envelopes;
using DirectoryService.Shared;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Exceptions;

namespace DirectoryService.Web.Configurations;

public static class WebDependencyInjection
{
    public static IServiceCollection AddConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        return services
            .AddSerilogLogging(configuration)
            .AddOpenApiSpec();
    }

    private static IServiceCollection AddOpenApiSpec(this IServiceCollection services)
    {
        services.AddOpenApi(options =>
        {
            options.AddSchemaTransformer((schema, context, _) =>
            {
                if (context.JsonTypeInfo.Type == typeof(Envelope<Errors>))
                {
                    if (schema.Properties.TryGetValue("errors", out var errorsProp))
                    {
                        errorsProp.Items.Reference = new OpenApiReference
                        {
                            Type = ReferenceType.Schema,
                            Id = "Error",
                        };
                    }
                }

                return Task.CompletedTask;
            });
        });
        services.AddControllers();

        return services;
    }

    private static IServiceCollection AddSerilogLogging(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSerilog((sp, lc) => lc
            .ReadFrom.Configuration(configuration)
            .ReadFrom.Services(sp)
            .Enrich.FromLogContext()
            .Enrich.WithExceptionDetails()
            .Enrich.WithProperty("ServiceName", "DirectoryService"));

        return services;
    }
}