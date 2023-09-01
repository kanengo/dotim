using System.Net.WebSockets;
using Linker.Exceptions;
using Pb;

namespace Linker.Domains;

public class Connection
{
    private readonly WebSocket _webSocket;

    private readonly Protocol.Protocol _protocol;

    private readonly ArraySegment<byte> _buffer;

    private readonly ILogger? _logger;

    private readonly CancellationTokenSource _cts;

    private bool stopped = false;

    public event EventHandler<OnDataEventArgs>? OnDataEvent;
    public event EventHandler? OnCloseEvent;

    public event EventHandler? OnConnectEvent;
    
    public string ConnectionId {get;}

    public string UserId { get; set; } = "";

    public DeviceType DeviceType { get; set; } = DeviceType.None;

    public string AppId { get; set; } = "";

    public class OnDataEventArgs : EventArgs
    {
        public byte[] Data { get; }

        public OnDataEventArgs(byte[] data)
        {
            Data = data;
        }
    }

    public Connection(string connectionId, WebSocket webSocket, ILogger? logger,int bufferSize, CancellationTokenSource? cts)
    {
        ConnectionId = connectionId;
        
        _webSocket = webSocket;

        _protocol = new Protocol.Protocol(bufferSize);

        _buffer = new ArraySegment<byte>(new byte[bufferSize]);

        _logger = logger;

        _cts = cts??new CancellationTokenSource();
    }

    public async Task Start()
    {
        if (stopped)
        {
            return;
        }
        try
        {   
            OnConnectEvent?.Invoke(this, EventArgs.Empty);
            await foreach (var data in _receiveAsync().ConfigureAwait(false))
            {
                // _logger.LogDebug("Receive packet:{Data}",Encoding.UTF8.GetString(data));
                try
                {
                    OnDataEvent?.Invoke(this, new OnDataEventArgs(data));
                }
                catch (ConnectException ex)
                {
                    _logger?.LogWarning("receive data event invoke connect exception:{Code}-{Message}",ex.ErrorCode, ex.Message);
                    throw;
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning("receive data event invoke exception:{Message}", ex.Message);
                    throw;
                }
            }
        }
        catch (WebSocketException ex)
        {
            _logger?.LogDebug("WebSocketException:{Code}-{Message}", ex.ErrorCode, ex.Message);
            await CloseAsync(WebSocketCloseStatus.InternalServerError,
                $"Internal server error:{ex.ErrorCode}-{ex.Message}");
        }
        catch (ConnectException ex)
        {   
            _logger?.LogWarning("ConnectException:{Code}-{Message}", ex.ErrorCode, ex.Message);
            await CloseAsync(WebSocketCloseStatus.ProtocolError, $"{ex.ErrorCode}-{ex.Message}");
        }
        catch (Exception ex)
        {
            _logger?.LogError("Exception:{Message}", ex.Message);
            await CloseAsync(WebSocketCloseStatus.InternalServerError, $"{ex.Message}");
        }
        

    }

    private async Task CloseAsync(WebSocketCloseStatus webSocketCloseStatus, string? statusDescription)
    {
        if (stopped)
        {
            return;
        }
        if (_webSocket.State is WebSocketState.Open or WebSocketState.CloseReceived or WebSocketState.CloseSent)
        {
            await _webSocket.CloseAsync(webSocketCloseStatus, statusDescription,
                CancellationToken.None);
        }

        stopped = true;
        OnCloseEvent?.Invoke(this, EventArgs.Empty);
    }

    public async Task SendMessage(ArraySegment<byte> data)
    {
        if (stopped)
        {
            return;
        }
        await _webSocket.SendAsync(data, WebSocketMessageType.Binary, true, _cts.Token);
    }

    private async IAsyncEnumerable<byte[]> _receiveAsync()
    {
        while (true)
        {
            var receiveResult = await _webSocket.ReceiveAsync(_buffer, _cts.Token);
            if (receiveResult.CloseStatus.HasValue)
            {
                _logger?.LogDebug("websocket receive close: {Status}-{Desc}", receiveResult.CloseStatus, receiveResult.CloseStatusDescription);
                await CloseAsync(receiveResult.CloseStatus.Value, receiveResult.CloseStatusDescription);
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
                    await CloseAsync(WebSocketCloseStatus.Empty, "websocket closed");
                    break;
                case WebSocketState.Aborted or WebSocketState.Closed:
                    break;
            }

            yield break;
        }
    }
}