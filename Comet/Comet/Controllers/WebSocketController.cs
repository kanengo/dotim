using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Comet.Domains;
using Microsoft.AspNetCore.Mvc;
using Comet;
using Comet.Exceptions;

namespace Comet.Controllers;

public class WebSocketController: ControllerBase
{
    private readonly ILogger<WebSocketController> _logger;

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
        try
        {
            await _handleWebsocket(webSocket);
        }
        catch (WebSocketException ex)
        {
            _logger.LogDebug("WebSocketException:{Code}-{Message}", ex.ErrorCode, ex.Message);
            if (webSocket.State is WebSocketState.Open or WebSocketState.CloseReceived)
                await webSocket.CloseAsync(WebSocketCloseStatus.InternalServerError, $"Internal server error:{ex.ErrorCode}-{ex.Message}",
                    CancellationToken.None);
        }
        catch (ConnectException ex)
        {   
            _logger.LogWarning("ConnectException:{Code}-{Message}", ex.ErrorCode, ex.Message);
            if (webSocket.State is WebSocketState.Open or WebSocketState.CloseReceived)
                await webSocket.CloseAsync(WebSocketCloseStatus.ProtocolError, $"{ex.ErrorCode}-{ex.Message}",
                    CancellationToken.None);
        }
        catch (Exception ex)
        {
            _logger.LogError("Exception:{Message}", ex.Message);
            throw;
        }
    }

    private async Task _handleWebsocket(WebSocket webSocket)
    {
        var connection = new Connection(webSocket, 512, _logger, new CancellationTokenSource());

        
        await foreach (var data in connection.ReceiveAsync().ConfigureAwait(false))
        {
            _logger.LogDebug("Receive packet:{Data}",Encoding.UTF8.GetString(data));
            try
            {
                var packet = JsonSerializer.Deserialize<RequestPacket>(data);
                if (packet is null)
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("receive packet deserialize exception:{Message}",ex.Message);
                throw new ConnectException(ConnectErrorCode.ReceivedPacketInvalid, "receive packet json deserialize failed");
            }
        

            // await webSocket.SendAsync(new ArraySegment<byte>(data), WebSocketMessageType.Binary,true, CancellationToken.None);
        }
        _logger.LogInformation("connection close");
    }

    // private async Task _heartbeat(string message)
    // {
    //     
    // }
}