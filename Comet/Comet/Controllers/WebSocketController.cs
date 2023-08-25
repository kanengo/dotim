using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Comet.Domains;
using Microsoft.AspNetCore.Mvc;
using Comet.Exceptions;
using NanoidDotNet;

namespace Comet.Controllers;

public class WebSocketController: ControllerBase
{
    private readonly ILogger<WebSocketController> _logger;

    private const int BufferSize = 512; 

    public WebSocketController(ILogger<WebSocketController> logger)
    {
        _logger = logger;
    }
    [Route("/ws_sub")]
    public async Task Connect()
    {
        if (!HttpContext.WebSockets.IsWebSocketRequest)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();

        var connection = new ConnectionBuilder().AddLogger(_logger).BufferSize(BufferSize).Build(webSocket);
        connection.OnDataEvent += (sender, args) =>
        {
            if (sender is Connection c)
                _onData(c, args);
        };
        connection.OnCloseEvent += (sender, args) =>
        {
            if (sender is Connection c)
            {
                _onClose(c.ConnectionId);
            }
        };
        connection.OnConnectEvent += (sender, args) =>
        {
            if (sender is Connection c)
            {
                _onConnect(c);
            }
        };
        await connection.Start();
    }

    private void _onData(Connection connection, Connection.OnDataEventArgs args)
    {
        var data = args.Data;
        try
        {   
            var packet = JsonSerializer.Deserialize<RequestPacket>(data);
            if (packet is null)
            {
                _logger.LogDebug("Receive packet is null: RawData:{Data}", Encoding.UTF8.GetString(data));
                return;
            }

            _logger.LogDebug("Receive packet:{Data}", packet);

            switch (packet.Method)
            {
                case "heartbeat":
                    // await _heartbeat();
                    break;
            }
        }
        catch (Exception ex) when(ex is JsonException or NotSupportedException)
        {
            _logger.LogWarning("receive packet deserialize exception:{Message}", ex.Message);
            throw new ConnectException(ConnectErrorCode.ReceivedPacketInvalid,
                "receive packet json deserialize failed");
        }
        catch (Exception ex)
        {
            _logger.LogWarning("receive packet exception:{Message}", ex.Message);
            throw;
        }
    }
    
    private void _onConnect(Connection connection)
    {
        _logger.LogDebug("connection connect:{}", connection.ConnectionId);
    }


    private void _onClose(string connectionId)
    {
        _logger.LogDebug("connection closed:{}", connectionId);
    }

    private async Task _heartbeat()
    {
        await Task.CompletedTask;
    }
}