namespace BaseDock.Application.Features.Secrets.Commands.CreateSecret;

using BaseDock.Application.Abstractions.Data;
using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Application.Features.Secrets.DTOs;
using BaseDock.Application.Features.Secrets.Mappers;
using BaseDock.Domain.Entities;
using BaseDock.Domain.Primitives;
using Microsoft.EntityFrameworkCore;

public sealed class CreateSecretCommandHandler(IApplicationDbContext db, TimeProvider dateTime)
    : ICommandHandler<CreateSecretCommand, Result<SecretDto>>
{
    private readonly TimeProvider _dateTime = dateTime;

    public async Task<Result<SecretDto>> HandleAsync(
        CreateSecretCommand command,
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

        var nameExists = await db.Secrets
            .AnyAsync(s => s.EnvironmentId == environment.Id && s.Name == command.Name, cancellationToken);

        if (nameExists)
        {
            return Result.Failure<SecretDto>(
                Error.Conflict("Secret.NameExists", $"A secret with name '{command.Name}' already exists in this environment."));
        }

        var secret = Secret.Create(
            environment.Id,
            command.Name,
            command.Content,
            command.FilePath,
            command.External,
            command.ExternalName,
            _dateTime.GetUtcNow());

        db.Secrets.Add(secret);
        await db.SaveChangesAsync(cancellationToken);

        return Result.Success(secret.ToDto());
    }
}
