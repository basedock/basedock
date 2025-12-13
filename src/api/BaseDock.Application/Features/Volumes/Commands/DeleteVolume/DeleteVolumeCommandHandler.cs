namespace BaseDock.Application.Features.Volumes.Commands.DeleteVolume;

using BaseDock.Application.Abstractions.Data;
using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Domain.Primitives;
using Microsoft.EntityFrameworkCore;

public sealed class DeleteVolumeCommandHandler(IApplicationDbContext db)
    : ICommandHandler<DeleteVolumeCommand, Result>
{
    public async Task<Result> HandleAsync(
        DeleteVolumeCommand command,
        CancellationToken cancellationToken = default)
    {
        var project = await db.Projects
            .Include(p => p.Members)
            .FirstOrDefaultAsync(p => p.Slug == command.ProjectSlug, cancellationToken);

        if (project is null)
        {
            return Result.Failure(
                Error.NotFound("Project.NotFound", $"Project with slug '{command.ProjectSlug}' not found."));
        }

        if (!project.Members.Any(m => m.UserId == command.UserId))
        {
            return Result.Failure(
                Error.Forbidden("You are not a member of this project."));
        }

        var environment = await db.Environments
            .FirstOrDefaultAsync(e => e.ProjectId == project.Id && e.Slug == command.EnvironmentSlug, cancellationToken);

        if (environment is null)
        {
            return Result.Failure(
                Error.NotFound("Environment.NotFound", $"Environment with slug '{command.EnvironmentSlug}' not found."));
        }

        var volume = await db.Volumes
            .FirstOrDefaultAsync(v => v.Id == command.VolumeId && v.EnvironmentId == environment.Id, cancellationToken);

        if (volume is null)
        {
            return Result.Failure(
                Error.NotFound("Volume.NotFound", "Volume not found."));
        }

        db.Volumes.Remove(volume);
        await db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
