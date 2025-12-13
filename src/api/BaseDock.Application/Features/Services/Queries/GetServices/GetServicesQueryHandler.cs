namespace BaseDock.Application.Features.Services.Queries.GetServices;

using BaseDock.Application.Abstractions.Data;
using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Application.Features.Services.DTOs;
using BaseDock.Application.Features.Services.Mappers;
using BaseDock.Domain.Primitives;
using Microsoft.EntityFrameworkCore;

public sealed class GetServicesQueryHandler(IApplicationDbContext db)
    : IQueryHandler<GetServicesQuery, Result<IEnumerable<ServiceDto>>>
{
    public async Task<Result<IEnumerable<ServiceDto>>> HandleAsync(
        GetServicesQuery query,
        CancellationToken cancellationToken = default)
    {
        // Find project and verify membership
        var project = await db.Projects
            .AsNoTracking()
            .Include(p => p.Members)
            .FirstOrDefaultAsync(p => p.Slug == query.ProjectSlug, cancellationToken);

        if (project is null)
        {
            return Result.Failure<IEnumerable<ServiceDto>>(
                Error.NotFound("Project.NotFound", $"Project with slug '{query.ProjectSlug}' not found."));
        }

        if (!project.Members.Any(m => m.UserId == query.UserId))
        {
            return Result.Failure<IEnumerable<ServiceDto>>(
                Error.Forbidden("You are not a member of this project."));
        }

        // Find environment
        var environment = await db.Environments
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.ProjectId == project.Id && e.Slug == query.EnvironmentSlug, cancellationToken);

        if (environment is null)
        {
            return Result.Failure<IEnumerable<ServiceDto>>(
                Error.NotFound("Environment.NotFound", $"Environment with slug '{query.EnvironmentSlug}' not found."));
        }

        var services = await db.Services
            .AsNoTracking()
            .Where(s => s.EnvironmentId == environment.Id)
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);

        return Result.Success(services.ToDtos());
    }
}
