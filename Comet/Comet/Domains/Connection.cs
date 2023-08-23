using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Comet.Domains.Models;

namespace Comet.Domains;

public class Connection
{
    private readonly WebSocket _webSocket;

    private readonly Protocol.Protocol _protocol;

    private readonly ArraySegment<byte> _buffer;

    private readonly ILogger _logger;

    public Connection(WebSocket webSocket, int bufferSize, ILogger logger)
    {
        _webSocket = webSocket;

        _protocol = new Protocol.Protocol(bufferSize);

        _buffer = new ArraySegment<byte>(new byte[bufferSize]);

        _logger = logger;
    }

    public async Task<byte[]?> Read()
    {
        var receiveResult = await _webSocket.ReceiveAsync(_buffer, CancellationToken.None);
        if (receiveResult.CloseStatus.HasValue)
        {
            await _webSocket.CloseAsync(receiveResult.CloseStatus.Value, receiveResult.CloseStatusDescription,
                CancellationToken.None);
            return default;
        }

        _protocol.Write(_buffer[0..receiveResult.Count]);

        var data = _protocol.CheckCompleteData();

        return null;
    }
}