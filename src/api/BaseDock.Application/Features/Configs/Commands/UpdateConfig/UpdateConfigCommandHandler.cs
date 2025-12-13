namespace BaseDock.Application.Features.Configs.Commands.UpdateConfig;

using BaseDock.Application.Abstractions.Data;
using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Application.Features.Configs.DTOs;
using BaseDock.Application.Features.Configs.Mappers;
using BaseDock.Domain.Primitives;
using Microsoft.EntityFrameworkCore;

public sealed class UpdateConfigCommandHandler(IApplicationDbContext db)
    : ICommandHandler<UpdateConfigCommand, Result<ConfigDto>>
{
    public async Task<Result<ConfigDto>> HandleAsync(
        UpdateConfigCommand command,
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

        var config = await db.Configs
            .FirstOrDefaultAsync(c => c.Id == command.ConfigId && c.EnvironmentId == environment.Id, cancellationToken);

        if (config is null)
        {
            return Result.Failure<ConfigDto>(
                Error.NotFound("Config.NotFound", "Config not found."));
        }

        config.Update(
            command.Content,
            command.FilePath,
            command.External,
            command.ExternalName);

        await db.SaveChangesAsync(cancellationToken);

        return Result.Success(config.ToDto());
    }
}
