namespace BaseDock.Application.Features.Projects.Commands.CreateProject;

using BaseDock.Application.Abstractions.Data;
using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Application.Features.Projects.DTOs;
using BaseDock.Application.Features.Projects.Mappers;
using BaseDock.Domain.Entities;
using BaseDock.Domain.Enums;
using BaseDock.Domain.Primitives;
using Microsoft.EntityFrameworkCore;

public sealed class CreateProjectCommandHandler(IApplicationDbContext db)
    : ICommandHandler<CreateProjectCommand, Result<ProjectDto>>
{
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

        // Check slug uniqueness
        var slugExists = await db.Projects
            .AnyAsync(p => p.Slug == command.Slug, cancellationToken);

        if (slugExists)
        {
            return Result.Failure<ProjectDto>(
                Error.Conflict("Project.SlugExists", $"A project with slug '{command.Slug}' already exists."));
        }

        var project = Project.Create(
            command.Name,
            command.Slug,
            command.Description,
            command.ProjectType,
            command.CreatedByUserId);

        // Set type-specific configuration
        if (command.ProjectType == ProjectType.ComposeFile && !string.IsNullOrWhiteSpace(command.ComposeFileContent))
        {
            project.SetComposeFile(command.ComposeFileContent);
        }
        else if (command.ProjectType == ProjectType.DockerImage && !string.IsNullOrWhiteSpace(command.DockerImageConfig))
        {
            project.SetDockerImageConfig(command.DockerImageConfig);
        }

        // Add the creator as a member
        project.AddMember(command.CreatedByUserId);

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
                project.AddMember(userId);
            }
        }

        db.Projects.Add(project);
        await db.SaveChangesAsync(cancellationToken);

        // Reload with members and user info
        var createdProject = await db.Projects
            .Include(p => p.Members)
            .ThenInclude(m => m.User)
            .FirstAsync(p => p.Id == project.Id, cancellationToken);

        return Result.Success(createdProject.ToDto());
    }
}
