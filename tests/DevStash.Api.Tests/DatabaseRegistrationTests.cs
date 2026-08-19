using DevStash.Api.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace DevStash.Api.Tests;

public sealed class DatabaseRegistrationTests : IClassFixture<WebApplicationFactory<global::Program>>
{
    private readonly WebApplicationFactory<global::Program> _factory;

    public DatabaseRegistrationTests(WebApplicationFactory<global::Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public void Services_ResolveANpgsqlContextWithinARequestScope()
    {
        using var scope = _factory.Services.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<DevStashDbContext>();

        Assert.Equal("Npgsql.EntityFrameworkCore.PostgreSQL", context.Database.ProviderName);
    }

    [Fact]
    public void Services_CreateOneContextPerScope()
    {
        using var firstScope = _factory.Services.CreateScope();
        using var secondScope = _factory.Services.CreateScope();

        var first = firstScope.ServiceProvider.GetRequiredService<DevStashDbContext>();
        var firstAgain = firstScope.ServiceProvider.GetRequiredService<DevStashDbContext>();
        var second = secondScope.ServiceProvider.GetRequiredService<DevStashDbContext>();

        Assert.Same(first, firstAgain);
        Assert.NotSame(first, second);
    }
}
