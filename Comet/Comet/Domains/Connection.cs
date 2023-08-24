using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Comet.Domains.Models;
using Comet.Exceptions;

namespace Comet.Domains;

public class Connection
{
    private readonly WebSocket _webSocket;

    private readonly Protocol.Protocol _protocol;

    private readonly ArraySegment<byte> _buffer;

    private readonly ILogger _logger;

    private readonly CancellationTokenSource _cts;

    public Connection(WebSocket webSocket, int bufferSize, ILogger logger, CancellationTokenSource cts)
    {
        _webSocket = webSocket;

        _protocol = new Protocol.Protocol(bufferSize);

        _buffer = new ArraySegment<byte>(new byte[bufferSize]);

        _logger = logger;

        _cts = cts;
    }

    public async IAsyncEnumerable<byte[]> ReceiveAsync()
    {
        while (true)
        {
            var receiveResult = await _webSocket.ReceiveAsync(_buffer, _cts.Token);
            if (receiveResult.CloseStatus.HasValue)
            {
                _logger.LogDebug("websocket receive close: {Status}-{Desc}", receiveResult.CloseStatus, receiveResult.CloseStatusDescription);
                if (_webSocket.State is WebSocketState.Open or WebSocketState.CloseReceived or WebSocketState.CloseSent)
                {
                    await _webSocket.CloseAsync(receiveResult.CloseStatus.Value, receiveResult.CloseStatusDescription,
                        CancellationToken.None);
                }
                yield break;
            }

            var n = receiveResult.Count;
            var available = _protocol.BufferAvailable;
            while (n > 0)
            {
                var count = n >= available ? available : n;
                n -= _protocol.Write(_buffer[..count]);
                var completeData = _protocol.CheckCompleteData();
                if (completeData is not null)
                    yield return completeData;

                available = _protocol.BufferAvailable;
                //缓存不够
                if (available == 0 && n > 0)
                {
                      throw new ConnectException(ConnectErrorCode.ReceivedBufferTooLarge, "no more cache to receive data");
                }
            }
            
            switch (_webSocket.State)
            {
                case WebSocketState.Open:
                    continue;
                case WebSocketState.CloseReceived or WebSocketState.CloseSent:
                    await _webSocket.CloseAsync(WebSocketCloseStatus.Empty, "websocket closed",
                        CancellationToken.None);
                    break;
            }

            yield break;
        }
    }
}