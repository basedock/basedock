namespace BaseDock.Infrastructure.Notifications;

using BaseDock.Application.Abstractions.Notifications;
using BaseDock.Application.Features.Docker.DTOs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

public class DeploymentNotificationService<THub>(
    IHubContext<THub> hubContext,
    ILogger<DeploymentNotificationService<THub>> logger) : IDeploymentNotificationService
    where THub : Hub
{
    public async Task NotifyStatusChangedAsync(Guid projectId, DeploymentStatusDto status, CancellationToken ct = default)
    {
        try
        {
            await hubContext.Clients
                .Group($"project-{projectId}")
                .SendAsync("DeploymentStatusChanged", status, ct);

            logger.LogDebug(
                "Notified project {ProjectId} clients of status change to {Status}",
                projectId,
                status.Status);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to notify clients of status change for project {ProjectId}", projectId);
        }
    }

    public async Task NotifyLogUpdateAsync(Guid projectId, string logLine, CancellationToken ct = default)
    {
        try
        {
            await hubContext.Clients
                .Group($"project-{projectId}")
                .SendAsync("LogUpdate", logLine, ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to notify clients of log update for project {ProjectId}", projectId);
        }
    }
}
