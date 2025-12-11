namespace BaseDock.Application.Features.Projects.Commands.UpdateProject;

using BaseDock.Application.Abstractions.Data;
using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Application.Features.Projects.DTOs;
using BaseDock.Application.Features.Projects.Mappers;
using BaseDock.Domain.Primitives;
using Microsoft.EntityFrameworkCore;

public sealed class UpdateProjectCommandHandler(IApplicationDbContext db, TimeProvider dateTime)
    : ICommandHandler<UpdateProjectCommand, Result<ProjectDto>>
{
    private readonly TimeProvider _dateTime = dateTime;

    public async Task<Result<ProjectDto>> HandleAsync(
        UpdateProjectCommand command,
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

        // Check if name is being changed and if it conflicts
        if (project.Name != command.Name)
        {
            var nameExists = await db.Projects
                .AnyAsync(p => p.Name == command.Name && p.Id != command.ProjectId, cancellationToken);

            if (nameExists)
            {
                return Result.Failure<ProjectDto>(
                    Error.Conflict("Project.NameExists", $"A project with name '{command.Name}' already exists."));
            }
        }

        project.Update(command.Name, command.Description, _dateTime.GetUtcNow());
        await db.SaveChangesAsync(cancellationToken);

        return Result.Success(project.ToDto());
    }
}
