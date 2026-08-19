using DevStash.Api.Data;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DevStash.Api.Tests.Infrastructure;

public sealed class AuthWebApplicationFactory : WebApplicationFactory<global::Program>
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");
    private bool _databaseCreated;

    public AuthWebApplicationFactory()
    {
        _connection.Open();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting(
            "ConnectionStrings:DevStashDatabase",
            "Host=localhost;Database=unused_auth_tests;Username=devstash");

        builder.ConfigureServices(services =>
        {
            services.AddSingleton<IDataProtectionProvider>(new EphemeralDataProtectionProvider());
            services.RemoveAll<IDbContextOptionsConfiguration<DevStashDbContext>>();
            services.RemoveAll<DbContextOptions<DevStashDbContext>>();
            services.AddDbContext<DevStashDbContext>(options => options.UseSqlite(_connection));
        });
    }

    public HttpClient CreateHttpsClient()
    {
        var client = CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost"),
            HandleCookies = true
        });

        if (!_databaseCreated)
        {
            using var scope = Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<DevStashDbContext>();
            context.Database.EnsureCreated();
            _databaseCreated = true;
        }

        return client;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            _connection.Dispose();
        }
    }
}
