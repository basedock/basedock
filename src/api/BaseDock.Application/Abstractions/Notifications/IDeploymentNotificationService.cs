namespace BaseDock.Application.Abstractions.Notifications;

using BaseDock.Application.Features.Docker.DTOs;

public interface IDeploymentNotificationService
{
    Task NotifyStatusChangedAsync(Guid projectId, DeploymentStatusDto status, CancellationToken ct = default);

    Task NotifyLogUpdateAsync(Guid projectId, string logLine, CancellationToken ct = default);
}
