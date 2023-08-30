using System.Text;
using System.Text.Json;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Grpc.Core;
using Linker.Domains;
using Microsoft.AspNetCore.Mvc;
using Linker.Exceptions;
using Linker.Infrastructure;
using Pb;

namespace Linker.Controllers;

public class WebSocketController: ControllerBase
{
    private readonly ILogger<WebSocketController> _logger;

    private readonly InfrastructureService _infrastructureService;

    private const int BufferSize = 512;
    public WebSocketController(ILogger<WebSocketController> logger,IConfiguration configuration, InfrastructureService infrastructureService)
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

        var token = HttpContext.Request.Headers.Authorization;


        AuthenticateReply authenticateReply;
        try
        {
            authenticateReply = await _infrastructureService.ImLogicClient.AuthenticateAsync(new AuthenticateRequest
            {
                Token = token
            });
        }
        catch (RpcException ex) when(ex.StatusCode == Grpc.Core.StatusCode.Unauthenticated)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }
      
        
        var appId = authenticateReply.AppId;
        var userId = authenticateReply.UserId;
        var deviceType = authenticateReply.DeviceType;
        
        using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();

        var connection = new ConnectionBuilder().AddLogger(_logger).BufferSize(BufferSize).Build(webSocket);
        connection.UserId = userId;
        connection.DeviceType = deviceType;
        connection.AppId = appId;
        
        
        connection.OnDataEvent += async (sender, args) =>
        {
            if (sender is Connection c)
               await _onData(c,args);
        };
        connection.OnCloseEvent += async (sender, _) =>
        {
            if (sender is Connection c)
            {
                await _onClose(c);
            }
        };
        connection.OnConnectEvent += async (sender, _) =>
        {
            if (sender is Connection c)
            {
               await _onConnect(c);
            }
        };
        await connection.Start();
    }

    private async Task _onData(Connection connection, Connection.OnDataEventArgs args)
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
                    await _heartbeat(connection);
                    break;
                default:
                    throw new ConnectException(ConnectErrorCode.NotSupportedMethod, "not supported method");
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
    
    private async Task _onConnect(Connection connection)
    {
        _logger.LogDebug("connection connect:{}", connection.ConnectionId);
        var linkConnectEvent = new LinkStateEvent
        {
            Metadata = 
            {
                {LinkMetadataType.AppId.ToString(),connection.AppId},
                {LinkMetadataType.UserId.ToString(),connection.UserId},
                {LinkMetadataType.ConnectionId.ToString(),connection.ConnectionId},
                {LinkMetadataType.DeviceType.ToString(),connection.DeviceType.ToString()},
            },
            State = LinkState.Connect,
        };
        await _infrastructureService.PublishLinkStateEventAsync(linkConnectEvent);
    }


    private async Task _onClose(Connection connection)
    {
        _logger.LogDebug("connection closed:{ConnectionId}", connection.ConnectionId);
        var linkConnectEvent = new LinkStateEvent
        {
            Metadata = 
            {
                {LinkMetadataType.AppId.ToString(),connection.AppId},
                {LinkMetadataType.UserId.ToString(),connection.UserId},
                {LinkMetadataType.ConnectionId.ToString(),connection.ConnectionId},
                {LinkMetadataType.DeviceType.ToString(),connection.DeviceType.ToString()},
            },
            State = LinkState.Disconnect,
        };
        await _infrastructureService.PublishLinkStateEventAsync(linkConnectEvent);
        
    }

    private async Task _heartbeat(Connection connection)
    {
        _logger.LogDebug("connection closed:{ConnectionId}", connection.ConnectionId);
        var linkConnectEvent = new LinkStateEvent
        {
            Metadata = 
            {
                {LinkMetadataType.AppId.ToString(),connection.AppId},
                {LinkMetadataType.UserId.ToString(),connection.UserId},
                {LinkMetadataType.ConnectionId.ToString(),connection.ConnectionId},
                {LinkMetadataType.DeviceType.ToString(),connection.DeviceType.ToString()},
            },
            State = LinkState.Heartbeat,
        };
        await _infrastructureService.PublishLinkStateEventAsync(linkConnectEvent);
    }
}