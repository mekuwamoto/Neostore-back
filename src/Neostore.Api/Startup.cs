using Neostore.Api.Middlewares;
using Neostore.Api.Services;

namespace Neostore.Api;
public static class Startup
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
        {
            builder.Services.AddServices(builder.Configuration);
        }

        WebApplication app = builder.Build();
        {
            app.ConfigureMiddlewares();
            app.MapControllers();
            app.Run();
        }
    }
}