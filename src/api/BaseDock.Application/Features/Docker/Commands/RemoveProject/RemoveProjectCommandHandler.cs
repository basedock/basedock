namespace BaseDock.Application.Features.Docker.Commands.RemoveProject;

using BaseDock.Application.Abstractions.Data;
using BaseDock.Application.Abstractions.Docker;
using BaseDock.Application.Abstractions.FileSystem;
using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Application.Abstractions.Notifications;
using BaseDock.Application.Features.Docker.DTOs;
using BaseDock.Domain.Entities;
using BaseDock.Domain.Enums;
using BaseDock.Domain.Primitives;
using Microsoft.EntityFrameworkCore;

public sealed class RemoveProjectCommandHandler(
    IApplicationDbContext db,
    IDockerComposeService dockerComposeService,
    IDockerContainerService dockerContainerService,
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

        return project.ProjectType switch
        {
            ProjectType.ComposeFile => await RemoveComposeProjectAsync(project, cancellationToken),
            ProjectType.DockerImage => await RemoveDockerImageProjectAsync(project, cancellationToken),
            _ => Result.Failure<DeploymentStatusDto>(
                Error.Validation("Project.InvalidType", "Unknown project type."))
        };
    }

    private async Task<Result<DeploymentStatusDto>> RemoveComposeProjectAsync(
        Project project,
        CancellationToken cancellationToken)
    {
        var composeFilePath = fileService.GetComposeFilePath(project.Slug);

        // Remove containers if compose file exists
        if (File.Exists(composeFilePath))
        {
            var removeResult = await dockerComposeService.RemoveAsync(project.Slug, composeFilePath, cancellationToken);

            if (removeResult.IsFailure)
            {
                // Log but continue - containers might not exist
            }
        }

        // Delete project directory
        await fileService.DeleteProjectDirectoryAsync(project.Slug, cancellationToken);

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

    private async Task<Result<DeploymentStatusDto>> RemoveDockerImageProjectAsync(
        Project project,
        CancellationToken cancellationToken)
    {
        var containerName = GetContainerName(project.Slug);

        // Remove container (force to handle running containers)
        var removeResult = await dockerContainerService.RemoveAsync(containerName, force: true, cancellationToken);

        if (removeResult.IsFailure)
        {
            // Log but continue - container might not exist
        }

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

    private static string GetContainerName(string projectSlug) => $"basedock-{projectSlug}";
}
