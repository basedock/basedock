namespace BaseDock.Application.Features.Projects.Commands.RemoveProjectMembers;

using BaseDock.Application.Abstractions.Data;
using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Application.Features.Projects.DTOs;
using BaseDock.Application.Features.Projects.Mappers;
using BaseDock.Domain.Primitives;
using Microsoft.EntityFrameworkCore;

public sealed class RemoveProjectMembersCommandHandler(IApplicationDbContext db)
    : ICommandHandler<RemoveProjectMembersCommand, Result<ProjectDto>>
{
    public async Task<Result<ProjectDto>> HandleAsync(
        RemoveProjectMembersCommand command,
        CancellationToken cancellationToken = default)
    {
        var project = await db.Projects
            .Include(p => p.Members)
            .ThenInclude(m => m.User)
            .FirstOrDefaultAsync(p => p.Id == command.ProjectId, cancellationToken);

        if (project is null)
        {
            return Result.Failure<ProjectDto>(
                Error.NotFound("Project", command.ProjectId));
        }

        var userIdsToRemove = command.UserIds.ToHashSet();
        var membersToRemove = project.Members
            .Where(m => userIdsToRemove.Contains(m.UserId))
            .ToList();

        foreach (var member in membersToRemove)
        {
            project.RemoveMember(member);
        }

        await db.SaveChangesAsync(cancellationToken);

        return Result.Success(project.ToDto());
    }
}
