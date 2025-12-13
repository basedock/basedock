namespace BaseDock.Application.Features.Volumes.Commands.CreateVolume;

using BaseDock.Application.Abstractions.Data;
using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Application.Features.Volumes.DTOs;
using BaseDock.Application.Features.Volumes.Mappers;
using BaseDock.Domain.Entities;
using BaseDock.Domain.Primitives;
using Microsoft.EntityFrameworkCore;

public sealed class CreateVolumeCommandHandler(IApplicationDbContext db, TimeProvider dateTime)
    : ICommandHandler<CreateVolumeCommand, Result<VolumeDto>>
{
    private readonly TimeProvider _dateTime = dateTime;

    public async Task<Result<VolumeDto>> HandleAsync(
        CreateVolumeCommand command,
        CancellationToken cancellationToken = default)
    {
        var project = await db.Projects
            .Include(p => p.Members)
            .FirstOrDefaultAsync(p => p.Slug == command.ProjectSlug, cancellationToken);

        if (project is null)
        {
            return Result.Failure<VolumeDto>(
                Error.NotFound("Project.NotFound", $"Project with slug '{command.ProjectSlug}' not found."));
        }

        if (!project.Members.Any(m => m.UserId == command.UserId))
        {
            return Result.Failure<VolumeDto>(
                Error.Forbidden("You are not a member of this project."));
        }

        var environment = await db.Environments
            .FirstOrDefaultAsync(e => e.ProjectId == project.Id && e.Slug == command.EnvironmentSlug, cancellationToken);

        if (environment is null)
        {
            return Result.Failure<VolumeDto>(
                Error.NotFound("Environment.NotFound", $"Environment with slug '{command.EnvironmentSlug}' not found."));
        }

        var nameExists = await db.Volumes
            .AnyAsync(v => v.EnvironmentId == environment.Id && v.Name == command.Name, cancellationToken);

        if (nameExists)
        {
            return Result.Failure<VolumeDto>(
                Error.Conflict("Volume.NameExists", $"A volume with name '{command.Name}' already exists in this environment."));
        }

        var volume = Volume.Create(
            environment.Id,
            command.Name,
            command.Driver,
            command.DriverOpts,
            command.Labels,
            command.External,
            command.ExternalName,
            _dateTime.GetUtcNow());

        db.Volumes.Add(volume);
        await db.SaveChangesAsync(cancellationToken);

        return Result.Success(volume.ToDto());
    }
}
