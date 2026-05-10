using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Neostore.Persistence.Options;

namespace Neostore.Persistence.Context;

public class NeostoreDbContextFactory : IDesignTimeDbContextFactory<NeostoreDbContext>
{
    public NeostoreDbContext CreateDbContext(string[] args)
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "..", "Neostore.Api"))
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        DatabaseOptions dbOptions = configuration
            .GetSection(DatabaseOptions.SectionName)
            .Get<DatabaseOptions>() ?? new DatabaseOptions();

        string connectionString = dbOptions.ToConnectionString();

        DbContextOptionsBuilder<NeostoreDbContext> optionsBuilder = new();

        optionsBuilder.UseMySql(
            connectionString,
            new MySqlServerVersion(new Version(8, 0, 0))
        );

        return new NeostoreDbContext(optionsBuilder.Options);
    }
}
