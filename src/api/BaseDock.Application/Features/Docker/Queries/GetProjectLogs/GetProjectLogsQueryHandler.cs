namespace BaseDock.Application.Features.Docker.Queries.GetProjectLogs;

using BaseDock.Application.Abstractions.Data;
using BaseDock.Application.Abstractions.Docker;
using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Domain.Primitives;
using Microsoft.EntityFrameworkCore;

public sealed class GetProjectLogsQueryHandler(
    IApplicationDbContext db,
    IDockerComposeService dockerService)
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

        return await dockerService.GetLogsAsync(
            project.Name,
            query.ServiceName,
            query.TailLines,
            cancellationToken);
    }
}
