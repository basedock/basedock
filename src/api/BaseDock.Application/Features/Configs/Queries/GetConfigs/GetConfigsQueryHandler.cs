namespace BaseDock.Application.Features.Configs.Queries.GetConfigs;

using BaseDock.Application.Abstractions.Data;
using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Application.Features.Configs.DTOs;
using BaseDock.Application.Features.Configs.Mappers;
using BaseDock.Domain.Primitives;
using Microsoft.EntityFrameworkCore;

public sealed class GetConfigsQueryHandler(IApplicationDbContext db)
    : IQueryHandler<GetConfigsQuery, Result<IEnumerable<ConfigDto>>>
{
    public async Task<Result<IEnumerable<ConfigDto>>> HandleAsync(
        GetConfigsQuery query,
        CancellationToken cancellationToken = default)
    {
        var project = await db.Projects
            .AsNoTracking()
            .Include(p => p.Members)
            .FirstOrDefaultAsync(p => p.Slug == query.ProjectSlug, cancellationToken);

        if (project is null)
        {
            return Result.Failure<IEnumerable<ConfigDto>>(
                Error.NotFound("Project.NotFound", $"Project with slug '{query.ProjectSlug}' not found."));
        }

        if (!project.Members.Any(m => m.UserId == query.UserId))
        {
            return Result.Failure<IEnumerable<ConfigDto>>(
                Error.Forbidden("You are not a member of this project."));
        }

        var environment = await db.Environments
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.ProjectId == project.Id && e.Slug == query.EnvironmentSlug, cancellationToken);

        if (environment is null)
        {
            return Result.Failure<IEnumerable<ConfigDto>>(
                Error.NotFound("Environment.NotFound", $"Environment with slug '{query.EnvironmentSlug}' not found."));
        }

        var configs = await db.Configs
            .AsNoTracking()
            .Where(c => c.EnvironmentId == environment.Id)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);

        return Result.Success(configs.ToDtos());
    }
}
