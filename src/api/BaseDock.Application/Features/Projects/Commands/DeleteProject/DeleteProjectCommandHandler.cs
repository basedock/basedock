namespace BaseDock.Application.Features.Projects.Commands.DeleteProject;

using BaseDock.Application.Abstractions.Data;
using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Domain.Primitives;
using Microsoft.EntityFrameworkCore;

public sealed class DeleteProjectCommandHandler(IApplicationDbContext db)
    : ICommandHandler<DeleteProjectCommand, Result>
{
    public async Task<Result> HandleAsync(
        DeleteProjectCommand command,
        CancellationToken cancellationToken = default)
    {
        var project = await db.Projects
            .FirstOrDefaultAsync(p => p.Id == command.ProjectId, cancellationToken);

        if (project is null)
        {
            return Result.Failure(Error.NotFound("Project", command.ProjectId));
        }

        db.Projects.Remove(project);
        await db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
