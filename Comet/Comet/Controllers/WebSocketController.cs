using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Comet.Models;
using Microsoft.AspNetCore.Mvc;

namespace Comet.Controllers;

public class WebSocketController: ControllerBase
{
    private readonly ILogger<WebSocketController> _logger;

    public WebSocketController(ILogger<WebSocketController> logger)
    {
        _logger = logger;
    }
    [Route("/ws")]
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
            _logger.LogError("WebSocketException:{Code}-{Message}", ex.ErrorCode, ex.Message);
            if (webSocket.State.Equals(WebSocketState.Open) || webSocket.State.Equals(WebSocketState.CloseReceived))
                await webSocket.CloseAsync(WebSocketCloseStatus.InternalServerError, "Internal server error",
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
        var buffer = new byte[512];
        var receiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        while (!receiveResult.CloseStatus.HasValue)
        {
            var segmentBuffer = new ArraySegment<byte>(buffer, 0, receiveResult.Count);
            _logger.LogDebug("receive:{Buffer}",Encoding.UTF8.GetString(segmentBuffer));
            var request = JsonSerializer.Deserialize<WebSocketRequest>(segmentBuffer);
            
            
            receiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        }

        await webSocket.CloseAsync(receiveResult.CloseStatus.Value, receiveResult.CloseStatusDescription,
            CancellationToken.None);
    }

    private async Task _heartbeat(string message)
    {
        
    }
}