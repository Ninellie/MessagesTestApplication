using Microsoft.AspNetCore.Mvc;
using ServerApplication.DAL;

namespace ServerApplication.Controllers;

[ApiController]
[Route("[controller]")]
public class MessagesController(ILogger<MessagesController> logger,
    MessagesRepository repository,
    WebSocketHandler webSocketHandler) : ControllerBase
{
    // Получить все сообщения за последние 10 минут
    [HttpGet]
    public async Task<ActionResult<List<Message>>> GetRecentMessages()
    {
        logger.LogInformation("HTTP GET: messages recent");
        return await repository.GetRecentMessagesAsync();
    }
    
    // Добавить новое сообщение
    [HttpPut]
    public async Task<IActionResult> AddMessage([FromBody] string text)
    {
        if (string.IsNullOrWhiteSpace(text) || text.Length > 128)
            return BadRequest("Invalid message");
        
        logger.LogInformation("HTTP PUT: Incoming message {MessageText}", text);

        var message = await repository.AddMessageAsync(text);
        await webSocketHandler.BroadcastMessage(message);
        return Ok();
    }
    
    // Установить соединение по веб сокету
    [Route("ws")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task WsGet()
    {
        logger.LogInformation("WebSocket: get connection");
        if (HttpContext.WebSockets.IsWebSocketRequest)
        { 
            using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            await webSocketHandler.HandleConnection(webSocket);
        }
        else
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
    }
}