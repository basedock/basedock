namespace BaseDock.Infrastructure.Persistence.Seeding;

using BaseDock.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

public class DatabaseSeeder(
    ApplicationDbContext db,
    IConfiguration configuration,
    ILogger<DatabaseSeeder> logger)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await SeedAdminUserAsync(cancellationToken);
    }

    private async Task SeedAdminUserAsync(CancellationToken cancellationToken)
    {
        var adminEmail = configuration["ADMIN_EMAIL"];
        var adminPassword = configuration["ADMIN_PASSWORD"];

        if (string.IsNullOrEmpty(adminEmail) || string.IsNullOrEmpty(adminPassword))
        {
            logger.LogWarning("ADMIN_EMAIL or ADMIN_PASSWORD not configured. Skipping admin user seeding.");
            return;
        }

        var adminExists = await db.Users.AnyAsync(u => u.Email == adminEmail.ToLowerInvariant(), cancellationToken);

        if (adminExists)
        {
            logger.LogInformation("Admin user already exists. Skipping seeding.");
            return;
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword);
        var admin = User.Create(adminEmail, "Admin", passwordHash, isAdmin: true);

        db.Users.Add(admin);
        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Admin user created successfully with email: {Email}", adminEmail);
    }
}
