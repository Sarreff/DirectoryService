using System.Globalization;
using DirectoryService.Application.Configurations;
using DirectoryService.Infrastructure.Postgres.Configurations;
using DirectoryService.Web.Configurations;
using Serilog;
using Serilog.Exceptions;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting web application...");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, config) =>
    {
        config
            .ReadFrom.Configuration(context.Configuration)
            .Enrich.FromLogContext()
            .Enrich.WithExceptionDetails()
            .Enrich.WithProperty("ServiceName", "DirectoryService")
            .Filter.ByExcluding(logEvent => logEvent.Properties.TryGetValue("EventId", out var eventId) &&
                                            eventId.ToString().Contains("20102") &&
                                            logEvent.MessageTemplate.Text.Contains("23505"));
    });

    builder.Services
        .AddConfiguration(builder.Configuration)
        .AddInfrastructureConfiguration(builder.Configuration)
        .AddApplicationConfiguration();

    var app = builder.Build();

    app.Configure();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

namespace DirectoryService.Web
{
    public partial class Program { }
}