using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace DevStash.Api.Data;

public sealed class DevStashDbContextFactory : IDesignTimeDbContextFactory<DevStashDbContext>
{
    public DevStashDbContext CreateDbContext(string[] args)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .AddUserSecrets<Program>(optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DevStashDatabase")
            ?? throw new InvalidOperationException(
                "Connection string 'DevStashDatabase' is not configured. " +
                "Set it with user secrets or ConnectionStrings__DevStashDatabase.");

        var optionsBuilder = new DbContextOptionsBuilder<DevStashDbContext>();
        DevStashDbContextOptions.Configure(optionsBuilder, connectionString);

        return new DevStashDbContext(optionsBuilder.Options);
    }
}
