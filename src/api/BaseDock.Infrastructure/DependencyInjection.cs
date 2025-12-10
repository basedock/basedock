namespace BaseDock.Infrastructure;

using BaseDock.Application.Abstractions.Data;
using BaseDock.Application.Abstractions.Docker;
using BaseDock.Application.Abstractions.FileSystem;
using BaseDock.Application.Abstractions.Security;
using BaseDock.Infrastructure.Docker;
using BaseDock.Infrastructure.FileSystem;
using BaseDock.Infrastructure.Persistence;
using BaseDock.Infrastructure.Persistence.Seeding;
using BaseDock.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IApplicationDbContext>(sp =>
            sp.GetRequiredService<ApplicationDbContext>());

        services.AddScoped<DatabaseSeeder>();

        // Redis
        var redisConnectionString = configuration.GetConnectionString("Redis") ?? "localhost:6379";
        services.AddSingleton(new RedisConnectionFactory(redisConnectionString));

        // File System Settings
        services.Configure<FileSystem.FileSystemSettings>(
            configuration.GetSection(FileSystem.FileSystemSettings.SectionName));

        // JWT Settings
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        // Security Services
        services.AddSingleton<IJwtService, JwtService>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        services.AddScoped<ITokenBlacklistService, TokenBlacklistService>();

        // Docker Services
        services.AddSingleton<IProjectFileService, ProjectFileService>();
        services.AddSingleton<IDockerComposeService, DockerComposeService>();

        return services;
    }
}
