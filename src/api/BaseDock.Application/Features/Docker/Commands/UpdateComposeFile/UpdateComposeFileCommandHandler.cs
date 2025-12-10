namespace BaseDock.Application.Features.Docker.Commands.UpdateComposeFile;

using BaseDock.Application.Abstractions.Data;
using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Application.Features.Projects.DTOs;
using BaseDock.Application.Features.Projects.Mappers;
using BaseDock.Application.Validators;
using BaseDock.Domain.Primitives;
using Microsoft.EntityFrameworkCore;

public sealed class UpdateComposeFileCommandHandler(IApplicationDbContext db)
    : ICommandHandler<UpdateComposeFileCommand, Result<ProjectDto>>
{
    public async Task<Result<ProjectDto>> HandleAsync(
        UpdateComposeFileCommand command,
        CancellationToken cancellationToken = default)
    {
        var project = await db.Projects
            .Include(p => p.Members)
            .ThenInclude(m => m.User)
            .FirstOrDefaultAsync(p => p.Id == command.ProjectId, cancellationToken);

        if (project is null)
        {
            return Result.Failure<ProjectDto>(Error.NotFound("Project", command.ProjectId));
        }

        // Validate compose file content
        var validationResult = ComposeFileValidator.Validate(command.ComposeFileContent);
        if (validationResult.IsFailure)
        {
            return Result.Failure<ProjectDto>(validationResult.Error);
        }

        project.SetComposeFile(command.ComposeFileContent);
        await db.SaveChangesAsync(cancellationToken);

        return Result.Success(project.ToDto());
    }
}
