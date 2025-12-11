namespace BaseDock.Application.Features.Environments.Queries.GetEnvironmentsByProject;

using BaseDock.Application.Abstractions.Data;
using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Application.Features.Environments.DTOs;
using BaseDock.Application.Features.Environments.Mappers;
using BaseDock.Domain.Primitives;
using Microsoft.EntityFrameworkCore;

public sealed class GetEnvironmentsByProjectQueryHandler(IApplicationDbContext db)
    : IQueryHandler<GetEnvironmentsByProjectQuery, Result<IEnumerable<EnvironmentDto>>>
{
    public async Task<Result<IEnumerable<EnvironmentDto>>> HandleAsync(
        GetEnvironmentsByProjectQuery query,
        CancellationToken cancellationToken = default)
    {
        // Find project by slug and verify user membership
        var project = await db.Projects
            .AsNoTracking()
            .Include(p => p.Members)
            .FirstOrDefaultAsync(p => p.Slug == query.ProjectSlug, cancellationToken);

        if (project is null)
        {
            return Result.Failure<IEnumerable<EnvironmentDto>>(
                Error.NotFound("Project.NotFound", $"Project with slug '{query.ProjectSlug}' not found."));
        }

        if (!project.Members.Any(m => m.UserId == query.UserId))
        {
            return Result.Failure<IEnumerable<EnvironmentDto>>(
                Error.Forbidden("You are not a member of this project."));
        }

        var environments = await db.Environments
            .AsNoTracking()
            .Include(e => e.Variables)
            .Include(e => e.DockerImageResources)
            .Include(e => e.DockerfileResources)
            .Include(e => e.DockerComposeResources)
            .Include(e => e.PostgreSQLResources)
            .Include(e => e.RedisResources)
            .Where(e => e.ProjectId == project.Id)
            .OrderBy(e => e.Name)
            .ToListAsync(cancellationToken);

        return Result.Success(environments.ToDtos());
    }
}
