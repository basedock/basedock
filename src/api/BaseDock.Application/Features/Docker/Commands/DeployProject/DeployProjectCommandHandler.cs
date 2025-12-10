namespace BaseDock.Application.Features.Docker.Commands.DeployProject;

using BaseDock.Application.Abstractions.Data;
using BaseDock.Application.Abstractions.Docker;
using BaseDock.Application.Abstractions.FileSystem;
using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Application.Abstractions.Notifications;
using BaseDock.Application.Features.Docker.DTOs;
using BaseDock.Domain.Enums;
using BaseDock.Domain.Primitives;
using Microsoft.EntityFrameworkCore;

public sealed class DeployProjectCommandHandler(
    IApplicationDbContext db,
    IDockerComposeService dockerService,
    IProjectFileService fileService,
    IDeploymentNotificationService notificationService)
    : ICommandHandler<DeployProjectCommand, Result<DeploymentStatusDto>>
{
    public async Task<Result<DeploymentStatusDto>> HandleAsync(
        DeployProjectCommand command,
        CancellationToken cancellationToken = default)
    {
        var project = await db.Projects
            .FirstOrDefaultAsync(p => p.Id == command.ProjectId, cancellationToken);

        if (project is null)
        {
            return Result.Failure<DeploymentStatusDto>(Error.NotFound("Project", command.ProjectId));
        }

        if (string.IsNullOrEmpty(project.ComposeFileContent))
        {
            return Result.Failure<DeploymentStatusDto>(
                Error.Validation("Project.NoComposeFile", "No compose file configured for this project."));
        }

        // Update status to deploying
        project.SetDeploymentStatus(DeploymentStatus.Deploying);
        await db.SaveChangesAsync(cancellationToken);

        // Notify clients about status change
        var deployingStatus = new DeploymentStatusDto(
            DeploymentStatus.Deploying,
            project.LastDeployedAt,
            null,
            []);
        await notificationService.NotifyStatusChangedAsync(project.Id, deployingStatus, cancellationToken);

        // Write compose file to disk
        var writeResult = await fileService.WriteComposeFileAsync(
            project.Name,
            project.ComposeFileContent,
            cancellationToken);

        if (writeResult.IsFailure)
        {
            project.SetDeploymentError(writeResult.Error.Message);
            await db.SaveChangesAsync(cancellationToken);
            return Result.Failure<DeploymentStatusDto>(writeResult.Error);
        }

        // Deploy using docker compose
        var composeFilePath = fileService.GetComposeFilePath(project.Name);
        var deployResult = await dockerService.DeployAsync(project.Name, composeFilePath, cancellationToken);

        if (deployResult.IsFailure)
        {
            project.SetDeploymentError(deployResult.Error.Message);
            await db.SaveChangesAsync(cancellationToken);

            var errorStatus = new DeploymentStatusDto(
                DeploymentStatus.Error,
                project.LastDeployedAt,
                deployResult.Error.Message,
                []);
            await notificationService.NotifyStatusChangedAsync(project.Id, errorStatus, cancellationToken);

            return Result.Failure<DeploymentStatusDto>(deployResult.Error);
        }

        // Get container status
        var statusResult = await dockerService.GetStatusAsync(project.Name, cancellationToken);
        var containers = statusResult.IsSuccess ? statusResult.Value : [];
        var finalStatus = DetermineOverallStatus(containers);

        project.SetDeploymentStatus(finalStatus, DateTime.UtcNow);
        await db.SaveChangesAsync(cancellationToken);

        var result = new DeploymentStatusDto(
            finalStatus,
            project.LastDeployedAt,
            project.LastDeploymentError,
            containers);

        await notificationService.NotifyStatusChangedAsync(project.Id, result, cancellationToken);

        return Result.Success(result);
    }

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
