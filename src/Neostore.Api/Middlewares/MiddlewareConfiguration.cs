
namespace Neostore.Api.Middlewares;

internal static class MiddlewareConfiguration
{
    internal static WebApplication ConfigureMiddlewares(this WebApplication app)
    {
        app.UseMiddleware<ExceptionMiddleware>();
        app.UseCors("AllowFrontend");
        if (!app.Environment.IsDevelopment())
        {
            app.UseHttpsRedirection();
        }
        app.UseAuthorization();
        return app;
    }
}
