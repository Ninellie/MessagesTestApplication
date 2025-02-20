using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using ClientApplication.Models;

namespace ClientApplication.Controllers;

public class HomeController(ILogger<HomeController> logger, HttpClient httpClient, IConfiguration configuration)
    : Controller
{
    private string ServerUrl { get; } = configuration["ServerUrl"]!;
    private string WebSocketUrl { get; } = configuration["WebSocketUrl"]!;

    
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
    
    public IActionResult Client1()
    {
        logger.LogInformation("Client1 is called");
        return View();
    }
    
    public IActionResult Client2()
    {
        logger.LogInformation("Client2 is called");
        var messages = new List<Message>();
        ViewBag.WebSocketUrl = WebSocketUrl;
        return View(messages);
    }
    
    public async Task<IActionResult> Client3()
    {
        logger.LogInformation("Client3 is called");
        var messages = await GetMessagesFromServer();
        return View(messages);
    }
    
    [HttpPost]
    public async Task<IActionResult> ProcessInputClient1(string userInput)
    {
        logger.LogInformation("HTTP POST: Process Input Client1 {Input text}", userInput);
        await PostMessageToServer(userInput);
        return View("Client1");
    }

    [HttpGet]
    public async Task<IActionResult> GetMessages()
    {
        logger.LogInformation("HTTP GET: Get Messages");
        var messages = await GetMessagesFromServer();
        return Json(messages);
    }

    private async Task<List<Message>> GetMessagesFromServer()
    {
        try
        {
            var response = await httpClient.GetStringAsync(ServerUrl);
            return JsonSerializer.Deserialize<List<Message>>(response, new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<Message>();
        }
        catch
        {
            return
            [
                new Message()
                {
                    Id = 1,
                    ReceivedAt = DateTime.Now,
                    Text = "Get Messages From Server Error!"
                }
            ]; // В случае ошибки возвращаем пустой список
        }
    }
    
    private async Task PostMessageToServer(string text)
    {
        logger.LogInformation("HTTP POST: sending message to server {Message text}", text);
        var response = await httpClient.PutAsJsonAsync(ServerUrl, text);
        response.EnsureSuccessStatusCode();
        if (response.IsSuccessStatusCode)
        {
            logger.LogInformation("HTTP POST: successfully sended message to server {Message text}", text);
        }
        else
        {
            logger.LogInformation("HTTP POST: send error message to server {Message text}", text);
        }
    }
}