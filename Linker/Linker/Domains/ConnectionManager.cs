using System.Collections.Concurrent;

namespace Linker.Domains;

public class ConnectionManager
{
    public static ConnectionManager Instance { get; } = new();
    
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, ConcurrentDictionary<string, Connection>>> _connections = new ();

    public bool AddConnection(Connection connection)
    {
        var appClients = _connections.GetOrAdd(connection.AppId, _ => 
            new ConcurrentDictionary<string, ConcurrentDictionary<string, Connection>>());
        
        var userConnections = appClients.GetOrAdd(connection.UserId, _ => 
            new ConcurrentDictionary<string, Connection>());
        
        return userConnections.TryAdd(connection.ConnectionId, connection);
    }

    public bool RemoveConnection(string appId, string userId, string connectionId)
    {
        if (!_connections.TryGetValue(appId, out var appConnections))
            return false;

        return appConnections.TryGetValue(userId, out var userConnections) && 
               userConnections.TryRemove(connectionId, out _);
    }

    public async Task SendMessage(string appId, string userId, ArraySegment<byte> data)
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
    
    public async Task SendMessageByUserIds(string appId, IEnumerable<string> userIds, ArraySegment<byte> data)
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
}