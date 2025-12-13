namespace BaseDock.Application.Features.Configs.Commands.CreateConfig;

using BaseDock.Application.Abstractions.Data;
using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Application.Features.Configs.DTOs;
using BaseDock.Application.Features.Configs.Mappers;
using BaseDock.Domain.Entities;
using BaseDock.Domain.Primitives;
using Microsoft.EntityFrameworkCore;

public sealed class CreateConfigCommandHandler(IApplicationDbContext db, TimeProvider dateTime)
    : ICommandHandler<CreateConfigCommand, Result<ConfigDto>>
{
    private readonly TimeProvider _dateTime = dateTime;

    public async Task<Result<ConfigDto>> HandleAsync(
        CreateConfigCommand command,
        CancellationToken cancellationToken = default)
    {
        var project = await db.Projects
            .Include(p => p.Members)
            .FirstOrDefaultAsync(p => p.Slug == command.ProjectSlug, cancellationToken);

        if (project is null)
        {
            return Result.Failure<ConfigDto>(
                Error.NotFound("Project.NotFound", $"Project with slug '{command.ProjectSlug}' not found."));
        }

        if (!project.Members.Any(m => m.UserId == command.UserId))
        {
            return Result.Failure<ConfigDto>(
                Error.Forbidden("You are not a member of this project."));
        }

        var environment = await db.Environments
            .FirstOrDefaultAsync(e => e.ProjectId == project.Id && e.Slug == command.EnvironmentSlug, cancellationToken);

        if (environment is null)
        {
            return Result.Failure<ConfigDto>(
                Error.NotFound("Environment.NotFound", $"Environment with slug '{command.EnvironmentSlug}' not found."));
        }

        var nameExists = await db.Configs
            .AnyAsync(c => c.EnvironmentId == environment.Id && c.Name == command.Name, cancellationToken);

        if (nameExists)
        {
            return Result.Failure<ConfigDto>(
                Error.Conflict("Config.NameExists", $"A config with name '{command.Name}' already exists in this environment."));
        }

        var config = Config.Create(
            environment.Id,
            command.Name,
            command.Content,
            command.FilePath,
            command.External,
            command.ExternalName,
            _dateTime.GetUtcNow());

        db.Configs.Add(config);
        await db.SaveChangesAsync(cancellationToken);

        return Result.Success(config.ToDto());
    }
}
