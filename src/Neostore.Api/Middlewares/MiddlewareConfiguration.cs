
namespace Neostore.Api.Middlewares;

internal static class MiddlewareConfiguration
{
    internal static IApplicationBuilder ConfigureMiddlewares(this IApplicationBuilder app)
    {
        app.UseMiddleware<ExceptionMiddleware>();
        app.UseCors("AllowFrontend");
        app.UseHttpsRedirection();
        app.UseAuthorization();
        return app;
    }
}
