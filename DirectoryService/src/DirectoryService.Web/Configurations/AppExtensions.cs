namespace DirectoryService.Web.Configurations;

public static class AppExtensions
{
    public static IApplicationBuilder Configure(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "DirectoryService"));
        }

        app.MapControllers();

        return app;
    }
}