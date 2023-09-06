using System.Collections.Concurrent;
using System.Net.WebSockets;
using Shared.Timer;

namespace Linker.Domains;

public class ConnectionManager : IDisposable
{
    public static ConnectionManager Instance { get; } = new();
    
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, ConcurrentDictionary<string, Connection>>> _connections = new ();

    private readonly TimeWheel<Connection> _connectionTimer = new(TimeSpan.FromSeconds(1), ServiceIdentity.Instance.CancellationTokenSource.Token);

    private const double HeartbeatTimeoutMs = 60 * 1000;

    private ConnectionManager()
    {
        _connectionTimer.OnTick += async (sender, args) =>
        {
            var connection = args.Data;
            if (connection is null || connection.Closed)
            {
                return;
            }

            var now = DateTime.Now;
            if (now.Subtract(connection.HeartbeatTime).TotalMilliseconds > HeartbeatTimeoutMs) //心跳超时了
            { 
                await connection.CloseAsync(WebSocketCloseStatus.NormalClosure, "connection heartbeat timeout");
                return;
            }

            if (sender is TimeWheel<Connection> tw)
            {
                tw.Timeout(TimeSpan.FromMilliseconds(HeartbeatTimeoutMs), connection);
            }
        };
        _connectionTimer.Start();
    }

    public static bool AddConnection(Connection connection) => Instance._addConnection(connection);
    public static bool RemoveConnection(string appId, string userId, string connectionId) => Instance._removeConnection(appId, userId, connectionId);
    public static Task SendMessage(string appId, string userId, ArraySegment<byte> data) => Instance._sendMessage(appId, userId, data);
    public static Task SendMessage(string appId, IEnumerable<string> userIds, ArraySegment<byte> data) => Instance._sendMessageByUserIds(appId, userIds, data);

    private bool _addConnection(Connection connection)
    {
        var appClients = _connections.GetOrAdd(connection.AppId, _ => 
            new ConcurrentDictionary<string, ConcurrentDictionary<string, Connection>>());
        
        var userConnections = appClients.GetOrAdd(connection.UserId, _ => 
            new ConcurrentDictionary<string, Connection>());
        
        return userConnections.TryAdd(connection.ConnectionId, connection);
    }

    private bool _removeConnection(string appId, string userId, string connectionId)
    {
        if (!_connections.TryGetValue(appId, out var appConnections))
            return false;

        return appConnections.TryGetValue(userId, out var userConnections) && 
               userConnections.TryRemove(connectionId, out _);
    }

    private async Task _sendMessage(string appId, string userId, ArraySegment<byte> data)
    {
        if (!_connections.TryGetValue(appId, out var appConnections))
            return;
        
        if (!appConnections.TryGetValue(userId, out var userConnections))
            return;
        
        foreach (var (_, connection) in userConnections)
        {
            await connection.SendMessage(data);
        }
    }

    private async Task _sendMessageByUserIds(string appId, IEnumerable<string> userIds, ArraySegment<byte> data)
    {
        if (!_connections.TryGetValue(appId, out var appConnections))
            return;
        
        foreach (var userId in userIds)
        {
            if (!appConnections.TryGetValue(userId, out var userConnections))
                return;
        
            foreach (var (_, connection) in userConnections)
            {
                await connection.SendMessage(data);
            }
        }
       
    }

    public void Dispose()
    {
        
        GC.SuppressFinalize(this);
    }
}