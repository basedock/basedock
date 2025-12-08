using BaseDock.Api.Middleware;
using BaseDock.Application;
using BaseDock.Infrastructure;
using BaseDock.Infrastructure.Persistence;
using BaseDock.Infrastructure.Persistence.Seeding;
using Carter;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add layers
builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

// Add Carter for modular endpoints
builder.Services.AddCarter();

// OpenAPI
builder.Services.AddOpenApi();

var app = builder.Build();

// Handle --export-openapi CLI argument
if (args.Contains("--export-openapi"))
{
    app.MapOpenApi();
    app.MapCarter();
    await app.StartAsync();

    var addressFeature = app.Services.GetRequiredService<Microsoft.AspNetCore.Hosting.Server.IServer>()
        .Features.Get<Microsoft.AspNetCore.Hosting.Server.Features.IServerAddressesFeature>();
    var serverAddress = addressFeature?.Addresses.FirstOrDefault(a => a.StartsWith("https://"))
        ?? addressFeature?.Addresses.FirstOrDefault()
        ?? "https://localhost:7073";

    var handler = new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    };
    using var httpClient = new HttpClient(handler) { BaseAddress = new Uri(serverAddress) };
    var json = await httpClient.GetStringAsync("/openapi/v1.json");

    var outputPath = args.SkipWhile(a => a != "--export-openapi").Skip(1).FirstOrDefault() ?? "openapi.json";
    var directory = Path.GetDirectoryName(outputPath);
    if (!string.IsNullOrEmpty(directory))
    {
        Directory.CreateDirectory(directory);
    }
    await File.WriteAllTextAsync(outputPath, json);

    Console.WriteLine($"OpenAPI specification exported to: {Path.GetFullPath(outputPath)}");
    await app.StopAsync();
    return;
}

// Apply migrations and seed database
await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();

    var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
    await seeder.SeedAsync();
}

// Middleware pipeline
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

// Map Carter modules
app.MapCarter();

app.Run();
