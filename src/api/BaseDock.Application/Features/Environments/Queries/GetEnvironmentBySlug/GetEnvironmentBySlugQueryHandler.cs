namespace BaseDock.Application.Features.Environments.Queries.GetEnvironmentBySlug;

using BaseDock.Application.Abstractions.Data;
using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Application.Features.Environments.DTOs;
using BaseDock.Application.Features.Environments.Mappers;
using BaseDock.Domain.Primitives;
using Microsoft.EntityFrameworkCore;

public sealed class GetEnvironmentBySlugQueryHandler(IApplicationDbContext db)
    : IQueryHandler<GetEnvironmentBySlugQuery, Result<EnvironmentDetailDto>>
{
    public async Task<Result<EnvironmentDetailDto>> HandleAsync(
        GetEnvironmentBySlugQuery query,
        CancellationToken cancellationToken = default)
    {
        // Find project by slug and verify user membership
        var project = await db.Projects
            .AsNoTracking()
            .Include(p => p.Members)
            .FirstOrDefaultAsync(p => p.Slug == query.ProjectSlug, cancellationToken);

        if (project is null)
        {
            return Result.Failure<EnvironmentDetailDto>(
                Error.NotFound("Project.NotFound", $"Project with slug '{query.ProjectSlug}' not found."));
        }

        if (!project.Members.Any(m => m.UserId == query.UserId))
        {
            return Result.Failure<EnvironmentDetailDto>(
                Error.Forbidden("You are not a member of this project."));
        }

        var environment = await db.Environments
            .AsNoTracking()
            .Include(e => e.Variables)
            .Include(e => e.DockerImageResources)
            .Include(e => e.DockerfileResources)
            .Include(e => e.DockerComposeResources)
            .Include(e => e.PostgreSQLResources)
            .Include(e => e.RedisResources)
            .FirstOrDefaultAsync(
                e => e.ProjectId == project.Id && e.Slug == query.EnvironmentSlug,
                cancellationToken);

        if (environment is null)
        {
            return Result.Failure<EnvironmentDetailDto>(
                Error.NotFound("Environment.NotFound", $"Environment with slug '{query.EnvironmentSlug}' not found."));
        }

        return Result.Success(environment.ToDetailDto());
    }
}
