namespace BaseDock.Application.Features.Projects.Queries.GetProjectBySlug;

using BaseDock.Application.Abstractions.Data;
using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Application.Features.Projects.DTOs;
using BaseDock.Application.Features.Projects.Mappers;
using BaseDock.Domain.Primitives;
using Microsoft.EntityFrameworkCore;

public sealed class GetProjectBySlugQueryHandler(IApplicationDbContext db)
    : IQueryHandler<GetProjectBySlugQuery, Result<ProjectDto>>
{
    public async Task<Result<ProjectDto>> HandleAsync(
        GetProjectBySlugQuery query,
        CancellationToken cancellationToken = default)
    {
        var project = await db.Projects
            .AsNoTracking()
            .Include(p => p.Members)
            .ThenInclude(m => m.User)
            .FirstOrDefaultAsync(p => p.Slug == query.Slug, cancellationToken);

        if (project is null)
        {
            return Result.Failure<ProjectDto>(
                Error.NotFound("Project.NotFound", $"Project with slug '{query.Slug}' not found."));
        }

        // Check if user is a member of this project
        var isMember = project.Members.Any(m => m.UserId == query.UserId);
        if (!isMember)
        {
            return Result.Failure<ProjectDto>(
                Error.Forbidden("You are not a member of this project."));
        }

        return Result.Success(project.ToDto());
    }
}
