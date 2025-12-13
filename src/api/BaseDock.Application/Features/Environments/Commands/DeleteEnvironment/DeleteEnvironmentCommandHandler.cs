namespace BaseDock.Application.Features.Environments.Commands.DeleteEnvironment;

using BaseDock.Application.Abstractions.Data;
using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Domain.Primitives;
using Microsoft.EntityFrameworkCore;

public sealed class DeleteEnvironmentCommandHandler(IApplicationDbContext db)
    : ICommandHandler<DeleteEnvironmentCommand, Result>
{
    public async Task<Result> HandleAsync(
        DeleteEnvironmentCommand command,
        CancellationToken cancellationToken = default)
    {
        // Find project and verify membership
        var project = await db.Projects
            .Include(p => p.Members)
            .Include(p => p.Environments)
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

        // Find the environment
        var environment = project.Environments
            .FirstOrDefault(e => e.Slug == command.EnvironmentSlug);

        if (environment is null)
        {
            return Result.Failure(
                Error.NotFound("Environment.NotFound", $"Environment with slug '{command.EnvironmentSlug}' not found."));
        }

        // Prevent deletion of default environment
        if (environment.IsDefault)
        {
            return Result.Failure(
                Error.Validation("Environment.CannotDeleteDefault", "Cannot delete the default environment."));
        }

        db.Environments.Remove(environment);
        await db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
