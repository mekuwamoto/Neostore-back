
namespace Neostore.Api.Middlewares;

internal static class MiddlewareConfiguration
{
    internal static IApplicationBuilder ConfigureMiddlewares(this IApplicationBuilder app)
    {
        app.UseHttpsRedirection();
        app.UseAuthorization();
        return app;
    }
}
