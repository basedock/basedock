namespace BaseDock.Application.Features.Environments.Commands.CreateEnvironment;

using System.Text.RegularExpressions;
using BaseDock.Application.Abstractions.Data;
using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Application.Features.Environments.DTOs;
using BaseDock.Application.Features.Environments.Mappers;
using BaseDock.Domain.Entities;
using BaseDock.Domain.Primitives;
using Microsoft.EntityFrameworkCore;

public sealed partial class CreateEnvironmentCommandHandler(IApplicationDbContext db, TimeProvider dateTime)
    : ICommandHandler<CreateEnvironmentCommand, Result<EnvironmentDto>>
{
    private readonly TimeProvider _dateTime = dateTime;

    public async Task<Result<EnvironmentDto>> HandleAsync(
        CreateEnvironmentCommand command,
        CancellationToken cancellationToken = default)
    {
        // Find project and verify membership
        var project = await db.Projects
            .Include(p => p.Members)
            .FirstOrDefaultAsync(p => p.Slug == command.ProjectSlug, cancellationToken);

        if (project is null)
        {
            return Result.Failure<EnvironmentDto>(
                Error.NotFound("Project.NotFound", $"Project with slug '{command.ProjectSlug}' not found."));
        }

        if (!project.Members.Any(m => m.UserId == command.UserId))
        {
            return Result.Failure<EnvironmentDto>(
                Error.Forbidden("You are not a member of this project."));
        }

        // Check name uniqueness within project
        var nameExists = await db.Environments
            .AnyAsync(e => e.ProjectId == project.Id && e.Name == command.Name, cancellationToken);

        if (nameExists)
        {
            return Result.Failure<EnvironmentDto>(
                Error.Conflict("Environment.NameExists", $"An environment with name '{command.Name}' already exists in this project."));
        }

        // Generate unique slug within project
        var slug = await GenerateUniqueSlugAsync(project.Id, command.Name, cancellationToken);

        var environment = Environment.Create(
            command.Name,
            slug,
            command.Description,
            project.Id,
            _dateTime.GetUtcNow(),
            isDefault: false);

        db.Environments.Add(environment);
        await db.SaveChangesAsync(cancellationToken);

        // Reload with collections for proper DTO mapping
        var createdEnvironment = await db.Environments
            .Include(e => e.Variables)
            .Include(e => e.DockerImageResources)
            .Include(e => e.DockerfileResources)
            .Include(e => e.DockerComposeResources)
            .Include(e => e.PostgreSQLResources)
            .Include(e => e.RedisResources)
            .FirstAsync(e => e.Id == environment.Id, cancellationToken);

        return Result.Success(createdEnvironment.ToDto());
    }

    private async Task<string> GenerateUniqueSlugAsync(Guid projectId, string name, CancellationToken cancellationToken)
    {
        var baseSlug = GenerateSlug(name);
        var slug = baseSlug;
        var counter = 1;

        while (await db.Environments.AnyAsync(e => e.ProjectId == projectId && e.Slug == slug, cancellationToken))
        {
            slug = $"{baseSlug}-{counter}";
            counter++;

            // Safety limit - use random suffix
            if (counter > 100)
            {
                slug = $"{baseSlug}-{Guid.NewGuid().ToString()[..8]}";
                break;
            }
        }

        return slug;
    }

    private static string GenerateSlug(string text)
    {
        // Convert to lowercase
        var slug = text.ToLowerInvariant();

        // Replace spaces with hyphens
        slug = slug.Replace(' ', '-');

        // Remove invalid characters (keep only letters, numbers, and hyphens)
        slug = SlugRegex().Replace(slug, "");

        // Remove multiple consecutive hyphens
        slug = MultipleHyphensRegex().Replace(slug, "-");

        // Trim hyphens from start and end
        slug = slug.Trim('-');

        return slug;
    }

    [GeneratedRegex("[^a-z0-9-]")]
    private static partial Regex SlugRegex();

    [GeneratedRegex("-+")]
    private static partial Regex MultipleHyphensRegex();
}
