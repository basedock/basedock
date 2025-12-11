namespace BaseDock.Application.Features.Docker.Queries.GetProjectLogs;

using BaseDock.Application.Abstractions.Data;
using BaseDock.Application.Abstractions.Docker;
using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Domain.Enums;
using BaseDock.Domain.Primitives;
using Microsoft.EntityFrameworkCore;

public sealed class GetProjectLogsQueryHandler(
    IApplicationDbContext db,
    IDockerComposeService dockerComposeService,
    IDockerContainerService dockerContainerService)
    : IQueryHandler<GetProjectLogsQuery, Result<string>>
{
    public async Task<Result<string>> HandleAsync(
        GetProjectLogsQuery query,
        CancellationToken cancellationToken = default)
    {
        var project = await db.Projects
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == query.ProjectId, cancellationToken);

        if (project is null)
        {
            return Result.Failure<string>(Error.NotFound("Project", query.ProjectId));
        }

        return project.ProjectType switch
        {
            ProjectType.ComposeFile => await dockerComposeService.GetLogsAsync(
                project.Slug,
                query.ServiceName,
                query.TailLines,
                cancellationToken),
            ProjectType.DockerImage => await dockerContainerService.GetLogsAsync(
                GetContainerName(project.Slug),
                query.TailLines,
                cancellationToken),
            _ => Result.Failure<string>(
                Error.Validation("Project.InvalidType", "Unknown project type."))
        };
    }

    private static string GetContainerName(string projectSlug) => $"basedock-{projectSlug}";
}
