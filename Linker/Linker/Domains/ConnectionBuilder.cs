using System.Net.WebSockets;
using NanoidDotNet;

namespace Linker.Domains;

public struct ConnectionBuilder
{
    private int _bufferSize = 512;

    private ILogger? _logger = default;

    private string _connectionId  = Nanoid.Generate();

    private CancellationTokenSource? _cancellationTokenSource;

    public ConnectionBuilder()
    {
        _cancellationTokenSource = null;
    }

    public ConnectionBuilder BufferSize(int size)
    {
        _bufferSize = size;

        return this;
    }

    public ConnectionBuilder AddLogger(ILogger logger)
    {
        _logger = logger;
        return this;
    }
    
    public ConnectionBuilder ConnectionId(string id)
    {
        _connectionId = id;

        return this;
    }

    public ConnectionBuilder CancellationTokenSource(CancellationTokenSource cts)
    {
        _cancellationTokenSource = cts;
        return this;
    }

    public Connection Build(WebSocket webSocket)
    {
        return new Connection(_connectionId, webSocket, _logger, _bufferSize, _cancellationTokenSource);
    }
}