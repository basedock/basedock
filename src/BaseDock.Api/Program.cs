using BaseDock.Domain.Entities;
using BaseDock.Infrastructure;
using BaseDock.Infrastructure.Persistence;
using BaseDock.Api.Hubs;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddNpgsqlDbContext<ApplicationDbContext>("basedock");

builder.Services.AddAuthorization();
builder.Services.AddIdentityApiEndpoints<ApplicationUser>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddInfrastructureServices();

builder.Services.AddSignalR()
    .AddStackExchangeRedis(builder.Configuration.GetConnectionString("redis") ?? throw new InvalidOperationException("Redis connection string not found."));

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate();
        await DbInitializer.InitializeAsync(services, app.Configuration);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

app.UseHttpsRedirection();

// Only allow login endpoint, disable all other Identity API endpoints
app.Use(async (context, next) =>
{
    var path = context.Request.Path;
    string[] disabledPaths = ["/api/register", "/api/refresh", "/api/confirmEmail",
        "/api/resendConfirmationEmail", "/api/forgotPassword", "/api/resetPassword"];

    if (disabledPaths.Any(p => path.StartsWithSegments(p, StringComparison.OrdinalIgnoreCase)))
    {
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        return;
    }
    await next();
});

app.UseAuthorization();

app.MapGroup("/api").MapIdentityApi<ApplicationUser>();

app.MapPost("/api/logout", async (SignInManager<ApplicationUser> signInManager) =>
{
    await signInManager.SignOutAsync();
    return TypedResults.Ok();
}).RequireAuthorization();

app.MapHub<DockerHub>("/hubs/docker");

app.Run();
