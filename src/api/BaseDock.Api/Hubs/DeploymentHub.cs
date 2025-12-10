namespace BaseDock.Api.Hubs;

using Microsoft.AspNetCore.SignalR;

public class DeploymentHub : Hub
{
    /// <summary>
    /// Join a project group to receive real-time deployment status updates for a specific project.
    /// </summary>
    /// <param name="projectId">The project ID to subscribe to</param>
    public async Task JoinProjectGroup(Guid projectId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"project-{projectId}");
    }

    /// <summary>
    /// Leave a project group to stop receiving updates for a specific project.
    /// </summary>
    /// <param name="projectId">The project ID to unsubscribe from</param>
    public async Task LeaveProjectGroup(Guid projectId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"project-{projectId}");
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        // Groups are automatically cleaned up when a connection is closed
        await base.OnDisconnectedAsync(exception);
    }
}
