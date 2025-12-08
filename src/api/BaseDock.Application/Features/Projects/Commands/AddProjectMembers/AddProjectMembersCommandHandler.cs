namespace BaseDock.Application.Features.Projects.Commands.AddProjectMembers;

using BaseDock.Application.Abstractions.Data;
using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Application.Features.Projects.DTOs;
using BaseDock.Application.Features.Projects.Mappers;
using BaseDock.Domain.Primitives;
using Microsoft.EntityFrameworkCore;

public sealed class AddProjectMembersCommandHandler(IApplicationDbContext db)
    : ICommandHandler<AddProjectMembersCommand, Result<ProjectDto>>
{
    public async Task<Result<ProjectDto>> HandleAsync(
        AddProjectMembersCommand command,
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

        var existingMemberIds = project.Members.Select(m => m.UserId).ToHashSet();
        var newUserIds = command.UserIds.Distinct().Where(id => !existingMemberIds.Contains(id)).ToList();

        if (newUserIds.Count == 0)
        {
            return Result.Success(project.ToDto());
        }

        // Verify users exist
        var validUserIds = await db.Users
            .Where(u => newUserIds.Contains(u.Id))
            .Select(u => u.Id)
            .ToListAsync(cancellationToken);

        foreach (var userId in validUserIds)
        {
            project.AddMember(userId);
        }

        await db.SaveChangesAsync(cancellationToken);

        // Reload to get new member details
        var updatedProject = await db.Projects
            .Include(p => p.Members)
            .ThenInclude(m => m.User)
            .FirstAsync(p => p.Id == project.Id, cancellationToken);

        return Result.Success(updatedProject.ToDto());
    }
}
