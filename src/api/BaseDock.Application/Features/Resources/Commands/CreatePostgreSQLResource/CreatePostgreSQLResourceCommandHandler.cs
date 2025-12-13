namespace BaseDock.Application.Features.Resources.Commands.CreatePostgreSQLResource;

using System.Text.RegularExpressions;
using BaseDock.Application.Abstractions.Data;
using BaseDock.Application.Abstractions.Docker;
using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Application.Features.Resources.DTOs;
using BaseDock.Domain.Entities.Resources;
using BaseDock.Domain.Primitives;
using Microsoft.EntityFrameworkCore;

public sealed partial class CreatePostgreSQLResourceCommandHandler(
    IApplicationDbContext db,
    IComposeGeneratorService composeGenerator,
    TimeProvider dateTime)
    : ICommandHandler<CreatePostgreSQLResourceCommand, Result<PostgreSQLResourceDto>>
{
    public async Task<Result<PostgreSQLResourceDto>> HandleAsync(
        CreatePostgreSQLResourceCommand command,
        CancellationToken cancellationToken = default)
    {
        // Find project and verify membership
        var project = await db.Projects
            .Include(p => p.Members)
            .FirstOrDefaultAsync(p => p.Slug == command.ProjectSlug, cancellationToken);

        if (project is null)
        {
            return Result.Failure<PostgreSQLResourceDto>(
                Error.NotFound("Project.NotFound", $"Project with slug '{command.ProjectSlug}' not found."));
        }

        if (!project.Members.Any(m => m.UserId == command.UserId))
        {
            return Result.Failure<PostgreSQLResourceDto>(
                Error.Forbidden("You are not a member of this project."));
        }

        // Find environment
        var environment = await db.Environments
            .FirstOrDefaultAsync(e =>
                e.ProjectId == project.Id &&
                e.Slug == command.EnvSlug,
                cancellationToken);

        if (environment is null)
        {
            return Result.Failure<PostgreSQLResourceDto>(
                Error.NotFound("Environment.NotFound", $"Environment with slug '{command.EnvSlug}' not found."));
        }

        // Check name uniqueness within environment
        var nameExists = await db.PostgreSQLResources
            .AnyAsync(r => r.EnvironmentId == environment.Id && r.Name == command.Name, cancellationToken);

        if (nameExists)
        {
            return Result.Failure<PostgreSQLResourceDto>(
                Error.Conflict("Resource.NameExists", $"A PostgreSQL resource with name '{command.Name}' already exists in this environment."));
        }

        // Generate unique slug
        var slug = await GenerateUniqueSlugAsync(environment.Id, command.Name, cancellationToken);

        var resource = PostgreSQLResource.Create(
            command.Name,
            slug,
            command.Description,
            command.DatabaseName,
            command.Password,
            environment.Id,
            dateTime.GetUtcNow(),
            command.Version,
            command.Port,
            command.Username);

        db.PostgreSQLResources.Add(resource);
        await db.SaveChangesAsync(cancellationToken);

        // Regenerate compose file
        var composeResult = await composeGenerator.GenerateComposeFileAsync(
            environment.Id,
            command.ProjectSlug,
            cancellationToken);

        if (composeResult.IsSuccess)
        {
            environment.SetComposeFilePath(composeResult.Value);
            await db.SaveChangesAsync(cancellationToken);
        }

        var serviceName = composeGenerator.GetServiceName(
            command.ProjectSlug,
            command.EnvSlug,
            slug);

        return Result.Success(new PostgreSQLResourceDto(
            resource.Id,
            resource.Name,
            resource.Slug,
            resource.Description,
            resource.Version,
            resource.Port,
            resource.DatabaseName,
            resource.Username,
            resource.DeploymentStatus.ToString(),
            serviceName,
            resource.CreatedAt,
            resource.LastDeployedAt));
    }

    private async Task<string> GenerateUniqueSlugAsync(Guid environmentId, string name, CancellationToken cancellationToken)
    {
        var baseSlug = GenerateSlug(name);
        var slug = baseSlug;
        var counter = 1;

        while (await db.PostgreSQLResources.AnyAsync(r => r.EnvironmentId == environmentId && r.Slug == slug, cancellationToken))
        {
            slug = $"{baseSlug}-{counter}";
            counter++;

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
        var slug = text.ToLowerInvariant();
        slug = slug.Replace(' ', '-');
        slug = SlugRegex().Replace(slug, "");
        slug = MultipleHyphensRegex().Replace(slug, "-");
        slug = slug.Trim('-');
        return slug;
    }

    [GeneratedRegex("[^a-z0-9-]")]
    private static partial Regex SlugRegex();

    [GeneratedRegex("-+")]
    private static partial Regex MultipleHyphensRegex();
}
