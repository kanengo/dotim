using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Comet.Domains;
using Microsoft.AspNetCore.Mvc;
using Comet.Exceptions;
using Comet.Infrastructure;
using Microsoft.AspNetCore.Authentication;
using NanoidDotNet;
using Pb;

namespace Comet.Controllers;

public class WebSocketController: ControllerBase
{
    private readonly ILogger<WebSocketController> _logger;

    private readonly InfrastructureService _infrastructureService;

    private const int BufferSize = 512; 

    public WebSocketController(ILogger<WebSocketController> logger, InfrastructureService infrastructureService)
    {
        _logger = logger;

        _infrastructureService = infrastructureService;
    }
    [Route("/ws_sub")]
    public async Task Connect()
    {
        if (!HttpContext.WebSockets.IsWebSocketRequest)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        var appId = "1000001";
        var userId = "6544";
        
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
        connection.OnConnectEvent += async (sender, args) =>
        {
            if (sender is Connection c)
            {
               await _onConnect(appId,userId,c);
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
    
    private async Task _onConnect(string appId, string userId, Connection connection)
    {
        _logger.LogDebug("connection connect:{}", connection.ConnectionId);

        await _infrastructureService.ImLogic.ConnectLoginAsync(new ConnectLoginRequest
        {
            ConnectionId = connection.ConnectionId,
            UserId = userId,
            AppId = appId,
        });
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