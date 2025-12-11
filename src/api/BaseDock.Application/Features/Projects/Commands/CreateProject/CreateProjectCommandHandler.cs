namespace BaseDock.Application.Features.Projects.Commands.CreateProject;

using System.Text.RegularExpressions;
using BaseDock.Application.Abstractions.Data;
using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Application.Features.Projects.DTOs;
using BaseDock.Application.Features.Projects.Mappers;
using BaseDock.Domain.Entities;
using BaseDock.Domain.Primitives;
using Microsoft.EntityFrameworkCore;

public sealed partial class CreateProjectCommandHandler(IApplicationDbContext db, TimeProvider dateTime)
    : ICommandHandler<CreateProjectCommand, Result<ProjectDto>>
{
    private readonly TimeProvider _dateTime = dateTime;

    public async Task<Result<ProjectDto>> HandleAsync(
        CreateProjectCommand command,
        CancellationToken cancellationToken = default)
    {
        // Check name uniqueness
        var nameExists = await db.Projects
            .AnyAsync(p => p.Name == command.Name, cancellationToken);

        if (nameExists)
        {
            return Result.Failure<ProjectDto>(
                Error.Conflict("Project.NameExists", $"A project with name '{command.Name}' already exists."));
        }

        // Generate unique slug from name
        var slug = await GenerateUniqueSlugAsync(command.Name, cancellationToken);
        var now = _dateTime.GetUtcNow();

        var project = Project.Create(
            command.Name,
            slug,
            command.Description,
            command.CreatedByUserId,
            now);

        // Add the creator as a member
        project.AddMember(command.CreatedByUserId, now);

        // Add additional members if provided
        if (command.MemberIds != null)
        {
            var memberIds = command.MemberIds.Distinct().Where(id => id != command.CreatedByUserId);
            var existingUserIds = await db.Users
                .Where(u => memberIds.Contains(u.Id))
                .Select(u => u.Id)
                .ToListAsync(cancellationToken);

            foreach (var userId in existingUserIds)
            {
                project.AddMember(userId, now);
            }
        }

        db.Projects.Add(project);

        // Create default environments
        var productionEnv = Environment.Create(
            "Production",
            "production",
            "Production environment",
            project.Id,
            now,
            isDefault: true);

        var developmentEnv = Environment.Create(
            "Development",
            "development",
            "Development environment",
            project.Id,
            now,
            isDefault: false);

        db.Environments.Add(productionEnv);
        db.Environments.Add(developmentEnv);

        await db.SaveChangesAsync(cancellationToken);

        // Reload with members, user info, and environments
        var createdProject = await db.Projects
            .Include(p => p.Members)
            .ThenInclude(m => m.User)
            .Include(p => p.Environments)
            .FirstAsync(p => p.Id == project.Id, cancellationToken);

        return Result.Success(createdProject.ToDto());
    }

    private async Task<string> GenerateUniqueSlugAsync(string name, CancellationToken cancellationToken)
    {
        var baseSlug = GenerateSlug(name);
        var slug = baseSlug;
        var counter = 1;

        while (await db.Projects.AnyAsync(p => p.Slug == slug, cancellationToken))
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
