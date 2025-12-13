namespace BaseDock.Application.Features.Networks.Commands.DeleteNetwork;

using BaseDock.Application.Abstractions.Data;
using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Domain.Primitives;
using Microsoft.EntityFrameworkCore;

public sealed class DeleteNetworkCommandHandler(IApplicationDbContext db)
    : ICommandHandler<DeleteNetworkCommand, Result>
{
    public async Task<Result> HandleAsync(
        DeleteNetworkCommand command,
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

        var network = await db.Networks
            .FirstOrDefaultAsync(n => n.Id == command.NetworkId && n.EnvironmentId == environment.Id, cancellationToken);

        if (network is null)
        {
            return Result.Failure(
                Error.NotFound("Network.NotFound", "Network not found."));
        }

        db.Networks.Remove(network);
        await db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
