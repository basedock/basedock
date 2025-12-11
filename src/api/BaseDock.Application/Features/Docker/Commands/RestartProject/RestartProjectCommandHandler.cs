namespace BaseDock.Application.Features.Docker.Commands.RestartProject;

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

public sealed class RestartProjectCommandHandler(
    IApplicationDbContext db,
    IDockerComposeService dockerComposeService,
    IDockerContainerService dockerContainerService,
    IProjectFileService fileService,
    IDeploymentNotificationService notificationService)
    : ICommandHandler<RestartProjectCommand, Result<DeploymentStatusDto>>
{
    public async Task<Result<DeploymentStatusDto>> HandleAsync(
        RestartProjectCommand command,
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
            ProjectType.ComposeFile => await RestartComposeProjectAsync(project, cancellationToken),
            ProjectType.DockerImage => await RestartDockerImageProjectAsync(project, cancellationToken),
            _ => Result.Failure<DeploymentStatusDto>(
                Error.Validation("Project.InvalidType", "Unknown project type."))
        };
    }

    private async Task<Result<DeploymentStatusDto>> RestartComposeProjectAsync(
        Project project,
        CancellationToken cancellationToken)
    {
        var composeFilePath = fileService.GetComposeFilePath(project.Slug);

        if (!File.Exists(composeFilePath))
        {
            return Result.Failure<DeploymentStatusDto>(
                Error.Validation("Project.NotDeployed", "Project has not been deployed yet."));
        }

        // Set status to deploying while restarting
        project.SetDeploymentStatus(DeploymentStatus.Deploying);
        await db.SaveChangesAsync(cancellationToken);

        var deployingStatus = new DeploymentStatusDto(
            DeploymentStatus.Deploying,
            project.LastDeployedAt,
            null,
            []);
        await notificationService.NotifyStatusChangedAsync(project.Id, deployingStatus, cancellationToken);

        var restartResult = await dockerComposeService.RestartAsync(project.Slug, composeFilePath, cancellationToken);

        if (restartResult.IsFailure)
        {
            project.SetDeploymentError(restartResult.Error.Message);
            await db.SaveChangesAsync(cancellationToken);
            return Result.Failure<DeploymentStatusDto>(restartResult.Error);
        }

        // Get container status
        var statusResult = await dockerComposeService.GetStatusAsync(project.Slug, cancellationToken);
        var containers = statusResult.IsSuccess ? statusResult.Value : [];
        var finalStatus = DetermineOverallStatus(containers);

        project.SetDeploymentStatus(finalStatus);
        await db.SaveChangesAsync(cancellationToken);

        var result = new DeploymentStatusDto(
            finalStatus,
            project.LastDeployedAt,
            null,
            containers);

        await notificationService.NotifyStatusChangedAsync(project.Id, result, cancellationToken);

        return Result.Success(result);
    }

    private async Task<Result<DeploymentStatusDto>> RestartDockerImageProjectAsync(
        Project project,
        CancellationToken cancellationToken)
    {
        var containerName = GetContainerName(project.Slug);

        // Set status to deploying while restarting
        project.SetDeploymentStatus(DeploymentStatus.Deploying);
        await db.SaveChangesAsync(cancellationToken);

        var deployingStatus = new DeploymentStatusDto(
            DeploymentStatus.Deploying,
            project.LastDeployedAt,
            null,
            []);
        await notificationService.NotifyStatusChangedAsync(project.Id, deployingStatus, cancellationToken);

        var restartResult = await dockerContainerService.RestartAsync(containerName, cancellationToken);

        if (restartResult.IsFailure)
        {
            project.SetDeploymentError(restartResult.Error.Message);
            await db.SaveChangesAsync(cancellationToken);
            return Result.Failure<DeploymentStatusDto>(restartResult.Error);
        }

        // Get container status
        var statusResult = await dockerContainerService.GetStatusAsync(containerName, cancellationToken);
        var containers = statusResult.IsSuccess ? new[] { statusResult.Value } : Array.Empty<ContainerInfo>();
        var finalStatus = DetermineOverallStatus(containers);

        project.SetDeploymentStatus(finalStatus);
        await db.SaveChangesAsync(cancellationToken);

        var result = new DeploymentStatusDto(
            finalStatus,
            project.LastDeployedAt,
            null,
            containers);

        await notificationService.NotifyStatusChangedAsync(project.Id, result, cancellationToken);

        return Result.Success(result);
    }

    private static string GetContainerName(string projectSlug) => $"basedock-{projectSlug}";

    private static DeploymentStatus DetermineOverallStatus(IEnumerable<ContainerInfo> containers)
    {
        var containerList = containers.ToList();

        if (containerList.Count == 0)
        {
            return DeploymentStatus.NotDeployed;
        }

        var runningCount = containerList.Count(c =>
            c.State.Equals("running", StringComparison.OrdinalIgnoreCase));

        if (runningCount == containerList.Count)
        {
            return DeploymentStatus.Running;
        }

        if (runningCount == 0)
        {
            return DeploymentStatus.Stopped;
        }

        return DeploymentStatus.PartiallyRunning;
    }
}
