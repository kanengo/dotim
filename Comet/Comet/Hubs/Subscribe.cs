using Microsoft.AspNetCore.SignalR;

namespace Comet.Hubs;

public class SubscribeHub: Hub
{
    public override Task OnConnectedAsync()
    {
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        return base.OnDisconnectedAsync(exception);
    }
}