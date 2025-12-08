namespace BaseDock.Application.Features.Projects.Queries.GetProjectById;

using BaseDock.Application.Abstractions.Data;
using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Application.Features.Projects.DTOs;
using BaseDock.Application.Features.Projects.Mappers;
using BaseDock.Domain.Primitives;
using Microsoft.EntityFrameworkCore;

public sealed class GetProjectByIdQueryHandler(IApplicationDbContext db)
    : IQueryHandler<GetProjectByIdQuery, Result<ProjectDto>>
{
    public async Task<Result<ProjectDto>> HandleAsync(
        GetProjectByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var project = await db.Projects
            .AsNoTracking()
            .Include(p => p.Members)
            .ThenInclude(m => m.User)
            .FirstOrDefaultAsync(p => p.Id == query.ProjectId, cancellationToken);

        if (project is null)
        {
            return Result.Failure<ProjectDto>(
                Error.NotFound("Project", query.ProjectId));
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
