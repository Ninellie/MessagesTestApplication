using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace ServerApplication;

public class WebSocketHandler(ILogger<WebSocketHandler> logger)
{
    private readonly List<WebSocket> _sockets = new();

    public async Task HandleConnection(WebSocket webSocket)
    {
        _sockets.Add(webSocket);
        logger.LogInformation("Added WebSocket connection");
        while (webSocket.State == WebSocketState.Open)
        {
            await Task.Delay(100);
        }
        
        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Закрыто клиентом", CancellationToken.None);
        _sockets.Remove(webSocket);
        logger.LogInformation("Removed WebSocket connection");
    }

    public async Task BroadcastMessage(Message message)
    {
        var json = JsonSerializer.Serialize(message, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower });
        var bytes = Encoding.UTF8.GetBytes(json);

        logger.LogInformation("Message to broadcast: {Message}", json);
        
        foreach (var socket in _sockets.Where(s => s.State == WebSocketState.Open))
        {
            await socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }
}