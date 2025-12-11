namespace BaseDock.Application.Features.Docker.Commands.StopProject;

using BaseDock.Application.Abstractions.Data;
using BaseDock.Application.Abstractions.Docker;
using BaseDock.Application.Abstractions.FileSystem;
using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Application.Abstractions.Notifications;
using BaseDock.Application.Features.Docker.DTOs;
using BaseDock.Domain.Enums;
using BaseDock.Domain.Primitives;
using Microsoft.EntityFrameworkCore;

public sealed class StopProjectCommandHandler(
    IApplicationDbContext db,
    IDockerComposeService dockerService,
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

        var composeFilePath = fileService.GetComposeFilePath(project.Slug);

        if (!File.Exists(composeFilePath))
        {
            return Result.Failure<DeploymentStatusDto>(
                Error.Validation("Project.NotDeployed", "Project has not been deployed yet."));
        }

        var stopResult = await dockerService.StopAsync(project.Slug, composeFilePath, cancellationToken);

        if (stopResult.IsFailure)
        {
            return Result.Failure<DeploymentStatusDto>(stopResult.Error);
        }

        project.SetDeploymentStatus(DeploymentStatus.Stopped);
        await db.SaveChangesAsync(cancellationToken);

        // Get container status
        var statusResult = await dockerService.GetStatusAsync(project.Slug, cancellationToken);
        var containers = statusResult.IsSuccess ? statusResult.Value : [];

        var result = new DeploymentStatusDto(
            DeploymentStatus.Stopped,
            project.LastDeployedAt,
            null,
            containers);

        await notificationService.NotifyStatusChangedAsync(project.Id, result, cancellationToken);

        return Result.Success(result);
    }
}
