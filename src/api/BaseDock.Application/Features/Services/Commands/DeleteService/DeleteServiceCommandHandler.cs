namespace BaseDock.Application.Features.Services.Commands.DeleteService;

using BaseDock.Application.Abstractions.Data;
using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Domain.Primitives;
using Microsoft.EntityFrameworkCore;

public sealed class DeleteServiceCommandHandler(IApplicationDbContext db)
    : ICommandHandler<DeleteServiceCommand, Result>
{
    public async Task<Result> HandleAsync(
        DeleteServiceCommand command,
        CancellationToken cancellationToken = default)
    {
        // Find project and verify membership
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

        // Find environment
        var environment = await db.Environments
            .FirstOrDefaultAsync(e => e.ProjectId == project.Id && e.Slug == command.EnvironmentSlug, cancellationToken);

        if (environment is null)
        {
            return Result.Failure(
                Error.NotFound("Environment.NotFound", $"Environment with slug '{command.EnvironmentSlug}' not found."));
        }

        // Find service
        var service = await db.Services
            .FirstOrDefaultAsync(s => s.Id == command.ServiceId && s.EnvironmentId == environment.Id, cancellationToken);

        if (service is null)
        {
            return Result.Failure(
                Error.NotFound("Service.NotFound", "Service not found."));
        }

        db.Services.Remove(service);
        await db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
