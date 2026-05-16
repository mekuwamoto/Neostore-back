using Neostore.Api.Options;
using Neostore.Application;
using Neostore.Infrastructure;
using Neostore.Persistence;

namespace Neostore.Api.Services;

internal static class DependencyInjection
{
    internal static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AppApiServices(configuration)
            .AddApplication(configuration)
            .AddInfrastructure(configuration)
            .AddPersistence(configuration);
        return services;
    }

    internal static IServiceCollection AppApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<CorsOptions>(configuration.GetSection(CorsOptions.SectionName));

        CorsOptions corsOptions = configuration
            .GetSection(CorsOptions.SectionName)
            .Get<CorsOptions>() ?? new CorsOptions();

        services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", policy =>
            {
                policy
                    .WithOrigins(corsOptions.AllowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        services.AddControllers();
        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer((document, context, ct) =>
            {
                document.Info = new()
                {
                    Title = "Neostore API",
                    Version = "v1",
                    Description = "API administrativa do Neostore — gestão de produtos, categorias e usuários."
                };
                return Task.CompletedTask;
            });
        });
        return services;
    }
}
