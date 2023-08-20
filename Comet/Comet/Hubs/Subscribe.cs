using Microsoft.AspNetCore.SignalR;

namespace Comet.Hubs;

public class SubscribeHub: Hub
{
    private readonly ILogger<SubscribeHub> _logger;
    public SubscribeHub(ILogger<SubscribeHub> logger)
    {
        _logger = logger;
    }
    public override Task OnConnectedAsync()
    {
        _logger.Log(LogLevel.Information, "user:{@ContextUser}", Context.UserIdentifier);
        return base.OnConnectedAsync();
    }

    public Task Heartbeat()
    {
        _logger.Log(LogLevel.Information, "heartbeat");
        return Task.CompletedTask;
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        return base.OnDisconnectedAsync(exception);
    }
}