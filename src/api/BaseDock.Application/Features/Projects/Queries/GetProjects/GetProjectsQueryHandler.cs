namespace BaseDock.Application.Features.Projects.Queries.GetProjects;

using BaseDock.Application.Abstractions.Data;
using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Application.Features.Projects.DTOs;
using BaseDock.Application.Features.Projects.Mappers;
using BaseDock.Domain.Primitives;
using Microsoft.EntityFrameworkCore;

public sealed class GetProjectsQueryHandler(IApplicationDbContext db)
    : IQueryHandler<GetProjectsQuery, Result<IEnumerable<ProjectDto>>>
{
    public async Task<Result<IEnumerable<ProjectDto>>> HandleAsync(
        GetProjectsQuery query,
        CancellationToken cancellationToken = default)
    {
        // Get only projects where the user is a member
        var projectsQuery = db.Projects
            .AsNoTracking()
            .Include(p => p.Members)
            .ThenInclude(m => m.User)
            .Include(p => p.Environments)
            .Where(p => p.Members.Any(m => m.UserId == query.UserId))
            .OrderByDescending(p => p.CreatedAt);

        var projects = query.Limit.HasValue
            ? await projectsQuery.Take(query.Limit.Value).ToListAsync(cancellationToken)
            : await projectsQuery.ToListAsync(cancellationToken);

        return Result.Success(projects.ToDtos());
    }
}
