var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();

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
