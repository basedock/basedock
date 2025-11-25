using Microsoft.AspNetCore.SignalR;

namespace BaseDock.Api.Hubs;

public class DockerHub : Hub
{
    public async Task SendUpdate(string message)
    {
        await Clients.All.SendAsync("ReceiveUpdate", message);
    }
}
