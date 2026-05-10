using Neostore.Api.Middlewares;
using Neostore.Api.Services;
using Serilog;

namespace Neostore.Api;
public static class Startup
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
        {
            builder.Host.UseSerilog((context, config) =>
                config.ReadFrom.Configuration(context.Configuration));

            builder.Services.AddServices(builder.Configuration);
        }

        WebApplication app = builder.Build();
        {
            app.UseSerilogRequestLogging();
            app.ConfigureMiddlewares();
            app.MapControllers();
            app.Run();
        }
    }
}