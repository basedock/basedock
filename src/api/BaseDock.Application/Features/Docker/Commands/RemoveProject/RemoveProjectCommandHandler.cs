namespace BaseDock.Application.Features.Docker.Commands.RemoveProject;

using BaseDock.Application.Abstractions.Data;
using BaseDock.Application.Abstractions.Docker;
using BaseDock.Application.Abstractions.FileSystem;
using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Application.Abstractions.Notifications;
using BaseDock.Application.Features.Docker.DTOs;
using BaseDock.Domain.Enums;
using BaseDock.Domain.Primitives;
using Microsoft.EntityFrameworkCore;

public sealed class RemoveProjectCommandHandler(
    IApplicationDbContext db,
    IDockerComposeService dockerService,
    IProjectFileService fileService,
    IDeploymentNotificationService notificationService)
    : ICommandHandler<RemoveProjectCommand, Result<DeploymentStatusDto>>
{
    public async Task<Result<DeploymentStatusDto>> HandleAsync(
        RemoveProjectCommand command,
        CancellationToken cancellationToken = default)
    {
        var project = await db.Projects
            .FirstOrDefaultAsync(p => p.Id == command.ProjectId, cancellationToken);

        if (project is null)
        {
            return Result.Failure<DeploymentStatusDto>(Error.NotFound("Project", command.ProjectId));
        }

        var composeFilePath = fileService.GetComposeFilePath(project.Name);

        // Remove containers if compose file exists
        if (File.Exists(composeFilePath))
        {
            var removeResult = await dockerService.RemoveAsync(project.Name, composeFilePath, cancellationToken);

            if (removeResult.IsFailure)
            {
                // Log but continue - containers might not exist
            }
        }

        // Delete project directory
        await fileService.DeleteProjectDirectoryAsync(project.Name, cancellationToken);

        project.SetDeploymentStatus(DeploymentStatus.NotDeployed);
        await db.SaveChangesAsync(cancellationToken);

        var result = new DeploymentStatusDto(
            DeploymentStatus.NotDeployed,
            null,
            null,
            []);

        await notificationService.NotifyStatusChangedAsync(project.Id, result, cancellationToken);

        return Result.Success(result);
    }
}
