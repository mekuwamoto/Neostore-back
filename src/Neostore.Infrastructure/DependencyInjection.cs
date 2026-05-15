using Amazon.Runtime;
using Amazon.S3;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Neostore.Application.Interfaces;
using Neostore.Infrastructure.Options;
using Neostore.Infrastructure.Services;

namespace Neostore.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        S3Options s3Options = configuration.GetSection(S3Options.SectionName).Get<S3Options>()!;
        services.Configure<S3Options>(configuration.GetSection(S3Options.SectionName));

        AmazonS3Config s3Config = new()
        {
            ServiceURL = s3Options.ServiceUrl,
            ForcePathStyle = true
        };

        BasicAWSCredentials credentials = new(s3Options.AccessKey, s3Options.SecretKey);
        AmazonS3Client s3Client = new(credentials, s3Config);

        services.AddSingleton<IAmazonS3>(s3Client);
        services.AddScoped<IS3Service, S3Service>();

        return services;
    }
}
