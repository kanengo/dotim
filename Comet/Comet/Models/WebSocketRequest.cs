using System.Text.Json.Serialization;

namespace Comet.Models;

public class WebSocketRequest
{
    
    [JsonPropertyName("method")]
    public string Method { get; }
    
    [JsonPropertyName("data")]
    public byte[]? Data { get; }

    public WebSocketRequest(string method, byte[]? data)
    {
        Method = method;
        Data = data;
    }
}