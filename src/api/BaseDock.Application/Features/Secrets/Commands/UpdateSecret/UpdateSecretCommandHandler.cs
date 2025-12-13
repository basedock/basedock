namespace BaseDock.Application.Features.Secrets.Commands.UpdateSecret;

using BaseDock.Application.Abstractions.Data;
using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Application.Features.Secrets.DTOs;
using BaseDock.Application.Features.Secrets.Mappers;
using BaseDock.Domain.Primitives;
using Microsoft.EntityFrameworkCore;

public sealed class UpdateSecretCommandHandler(IApplicationDbContext db)
    : ICommandHandler<UpdateSecretCommand, Result<SecretDto>>
{
    public async Task<Result<SecretDto>> HandleAsync(
        UpdateSecretCommand command,
        CancellationToken cancellationToken = default)
    {
        var project = await db.Projects
            .Include(p => p.Members)
            .FirstOrDefaultAsync(p => p.Slug == command.ProjectSlug, cancellationToken);

        if (project is null)
        {
            return Result.Failure<SecretDto>(
                Error.NotFound("Project.NotFound", $"Project with slug '{command.ProjectSlug}' not found."));
        }

        if (!project.Members.Any(m => m.UserId == command.UserId))
        {
            return Result.Failure<SecretDto>(
                Error.Forbidden("You are not a member of this project."));
        }

        var environment = await db.Environments
            .FirstOrDefaultAsync(e => e.ProjectId == project.Id && e.Slug == command.EnvironmentSlug, cancellationToken);

        if (environment is null)
        {
            return Result.Failure<SecretDto>(
                Error.NotFound("Environment.NotFound", $"Environment with slug '{command.EnvironmentSlug}' not found."));
        }

        var secret = await db.Secrets
            .FirstOrDefaultAsync(s => s.Id == command.SecretId && s.EnvironmentId == environment.Id, cancellationToken);

        if (secret is null)
        {
            return Result.Failure<SecretDto>(
                Error.NotFound("Secret.NotFound", "Secret not found."));
        }

        secret.Update(
            command.Content,
            command.FilePath,
            command.External,
            command.ExternalName);

        await db.SaveChangesAsync(cancellationToken);

        return Result.Success(secret.ToDto());
    }
}
