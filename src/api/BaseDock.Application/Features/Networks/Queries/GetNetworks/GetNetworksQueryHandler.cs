namespace BaseDock.Application.Features.Networks.Queries.GetNetworks;

using BaseDock.Application.Abstractions.Data;
using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Application.Features.Networks.DTOs;
using BaseDock.Application.Features.Networks.Mappers;
using BaseDock.Domain.Primitives;
using Microsoft.EntityFrameworkCore;

public sealed class GetNetworksQueryHandler(IApplicationDbContext db)
    : IQueryHandler<GetNetworksQuery, Result<IEnumerable<NetworkDto>>>
{
    public async Task<Result<IEnumerable<NetworkDto>>> HandleAsync(
        GetNetworksQuery query,
        CancellationToken cancellationToken = default)
    {
        var project = await db.Projects
            .AsNoTracking()
            .Include(p => p.Members)
            .FirstOrDefaultAsync(p => p.Slug == query.ProjectSlug, cancellationToken);

        if (project is null)
        {
            return Result.Failure<IEnumerable<NetworkDto>>(
                Error.NotFound("Project.NotFound", $"Project with slug '{query.ProjectSlug}' not found."));
        }

        if (!project.Members.Any(m => m.UserId == query.UserId))
        {
            return Result.Failure<IEnumerable<NetworkDto>>(
                Error.Forbidden("You are not a member of this project."));
        }

        var environment = await db.Environments
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.ProjectId == project.Id && e.Slug == query.EnvironmentSlug, cancellationToken);

        if (environment is null)
        {
            return Result.Failure<IEnumerable<NetworkDto>>(
                Error.NotFound("Environment.NotFound", $"Environment with slug '{query.EnvironmentSlug}' not found."));
        }

        var networks = await db.Networks
            .AsNoTracking()
            .Where(n => n.EnvironmentId == environment.Id)
            .OrderBy(n => n.Name)
            .ToListAsync(cancellationToken);

        return Result.Success(networks.ToDtos());
    }
}
