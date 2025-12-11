namespace BaseDock.Application.Features.Docker.Commands.StopProject;

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

public sealed class StopProjectCommandHandler(
    IApplicationDbContext db,
    IDockerComposeService dockerComposeService,
    IDockerContainerService dockerContainerService,
    IProjectFileService fileService,
    IDeploymentNotificationService notificationService)
    : ICommandHandler<StopProjectCommand, Result<DeploymentStatusDto>>
{
    public async Task<Result<DeploymentStatusDto>> HandleAsync(
        StopProjectCommand command,
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
            ProjectType.ComposeFile => await StopComposeProjectAsync(project, cancellationToken),
            ProjectType.DockerImage => await StopDockerImageProjectAsync(project, cancellationToken),
            _ => Result.Failure<DeploymentStatusDto>(
                Error.Validation("Project.InvalidType", "Unknown project type."))
        };
    }

    private async Task<Result<DeploymentStatusDto>> StopComposeProjectAsync(
        Project project,
        CancellationToken cancellationToken)
    {
        var composeFilePath = fileService.GetComposeFilePath(project.Slug);

        if (!File.Exists(composeFilePath))
        {
            return Result.Failure<DeploymentStatusDto>(
                Error.Validation("Project.NotDeployed", "Project has not been deployed yet."));
        }

        var stopResult = await dockerComposeService.StopAsync(project.Slug, composeFilePath, cancellationToken);

        if (stopResult.IsFailure)
        {
            return Result.Failure<DeploymentStatusDto>(stopResult.Error);
        }

        project.SetDeploymentStatus(DeploymentStatus.Stopped);
        await db.SaveChangesAsync(cancellationToken);

        // Get container status
        var statusResult = await dockerComposeService.GetStatusAsync(project.Slug, cancellationToken);
        var containers = statusResult.IsSuccess ? statusResult.Value : [];

        var result = new DeploymentStatusDto(
            DeploymentStatus.Stopped,
            project.LastDeployedAt,
            null,
            containers);

        await notificationService.NotifyStatusChangedAsync(project.Id, result, cancellationToken);

        return Result.Success(result);
    }

    private async Task<Result<DeploymentStatusDto>> StopDockerImageProjectAsync(
        Project project,
        CancellationToken cancellationToken)
    {
        var containerName = GetContainerName(project.Slug);

        var stopResult = await dockerContainerService.StopAsync(containerName, cancellationToken);

        if (stopResult.IsFailure)
        {
            return Result.Failure<DeploymentStatusDto>(stopResult.Error);
        }

        project.SetDeploymentStatus(DeploymentStatus.Stopped);
        await db.SaveChangesAsync(cancellationToken);

        // Get container status
        var statusResult = await dockerContainerService.GetStatusAsync(containerName, cancellationToken);
        var containers = statusResult.IsSuccess ? new[] { statusResult.Value } : Array.Empty<ContainerInfo>();

        var result = new DeploymentStatusDto(
            DeploymentStatus.Stopped,
            project.LastDeployedAt,
            null,
            containers);

        await notificationService.NotifyStatusChangedAsync(project.Id, result, cancellationToken);

        return Result.Success(result);
    }

    private static string GetContainerName(string projectSlug) => $"basedock-{projectSlug}";
}
