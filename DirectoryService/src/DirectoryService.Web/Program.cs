using System.Globalization;
using DirectoryService.Application.Configurations;
using DirectoryService.Infrastructure.Postgres.Configurations;
using DirectoryService.Web.Configurations;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting web application...");

    var builder = WebApplication.CreateBuilder(args);

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