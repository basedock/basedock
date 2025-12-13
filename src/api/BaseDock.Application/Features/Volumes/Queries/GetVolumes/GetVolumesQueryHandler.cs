namespace BaseDock.Application.Features.Volumes.Queries.GetVolumes;

using BaseDock.Application.Abstractions.Data;
using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Application.Features.Volumes.DTOs;
using BaseDock.Application.Features.Volumes.Mappers;
using BaseDock.Domain.Primitives;
using Microsoft.EntityFrameworkCore;

public sealed class GetVolumesQueryHandler(IApplicationDbContext db)
    : IQueryHandler<GetVolumesQuery, Result<IEnumerable<VolumeDto>>>
{
    public async Task<Result<IEnumerable<VolumeDto>>> HandleAsync(
        GetVolumesQuery query,
        CancellationToken cancellationToken = default)
    {
        var project = await db.Projects
            .AsNoTracking()
            .Include(p => p.Members)
            .FirstOrDefaultAsync(p => p.Slug == query.ProjectSlug, cancellationToken);

        if (project is null)
        {
            return Result.Failure<IEnumerable<VolumeDto>>(
                Error.NotFound("Project.NotFound", $"Project with slug '{query.ProjectSlug}' not found."));
        }

        if (!project.Members.Any(m => m.UserId == query.UserId))
        {
            return Result.Failure<IEnumerable<VolumeDto>>(
                Error.Forbidden("You are not a member of this project."));
        }

        var environment = await db.Environments
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.ProjectId == project.Id && e.Slug == query.EnvironmentSlug, cancellationToken);

        if (environment is null)
        {
            return Result.Failure<IEnumerable<VolumeDto>>(
                Error.NotFound("Environment.NotFound", $"Environment with slug '{query.EnvironmentSlug}' not found."));
        }

        var volumes = await db.Volumes
            .AsNoTracking()
            .Where(v => v.EnvironmentId == environment.Id)
            .OrderBy(v => v.Name)
            .ToListAsync(cancellationToken);

        return Result.Success(volumes.ToDtos());
    }
}
