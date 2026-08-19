using DevStash.Api.Data;
using DevStash.Api.Data.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();

var connectionString = builder.Configuration.GetConnectionString("DevStashDatabase")
    ?? throw new InvalidOperationException(
        "Connection string 'DevStashDatabase' is not configured. " +
        "Set it with user secrets or ConnectionStrings__DevStashDatabase.");

builder.Services.AddDbContext<DevStashDbContext>(options =>
    DevStashDbContextOptions.Configure(options, connectionString));

builder.Services
    .AddIdentityCore<ApplicationUser>()
    .AddRoles<IdentityRole<Guid>>()
    .AddEntityFrameworkStores<DevStashDbContext>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapHealthChecks("/health");

app.MapGet("/api", () => TypedResults.Ok(new ApiInfo("DevStash API", "v1")))
    .WithName("GetApiInfo");

app.Run();

public sealed record ApiInfo(string Name, string Version);

public partial class Program { }
