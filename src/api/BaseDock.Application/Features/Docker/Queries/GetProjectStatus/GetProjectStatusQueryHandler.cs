namespace BaseDock.Application.Features.Docker.Queries.GetProjectStatus;

using BaseDock.Application.Abstractions.Data;
using BaseDock.Application.Abstractions.Docker;
using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Application.Features.Docker.DTOs;
using BaseDock.Domain.Enums;
using BaseDock.Domain.Primitives;
using Microsoft.EntityFrameworkCore;

public sealed class GetProjectStatusQueryHandler(
    IApplicationDbContext db,
    IDockerComposeService dockerService)
    : IQueryHandler<GetProjectStatusQuery, Result<DeploymentStatusDto>>
{
    public async Task<Result<DeploymentStatusDto>> HandleAsync(
        GetProjectStatusQuery query,
        CancellationToken cancellationToken = default)
    {
        var project = await db.Projects
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == query.ProjectId, cancellationToken);

        if (project is null)
        {
            return Result.Failure<DeploymentStatusDto>(Error.NotFound("Project", query.ProjectId));
        }

        // Get live container status from Docker
        var statusResult = await dockerService.GetStatusAsync(project.Slug, cancellationToken);
        var containers = statusResult.IsSuccess ? statusResult.Value : [];

        // Determine live status from containers
        var liveStatus = DetermineOverallStatus(containers);

        // If live status differs from stored status and project was deployed,
        // return the live status (containers might have been stopped externally)
        var effectiveStatus = project.DeploymentStatus == DeploymentStatus.NotDeployed
            ? project.DeploymentStatus
            : liveStatus;

        return Result.Success(new DeploymentStatusDto(
            effectiveStatus,
            project.LastDeployedAt,
            project.LastDeploymentError,
            containers));
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
