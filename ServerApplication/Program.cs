using ServerApplication;
using ServerApplication.DAL;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<MessagesRepository>();
builder.Services.AddSingleton<WebSocketHandler>();

var app = builder.Build();

app.UseRouting();

app.MapControllers();

var webSocketOptions = new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromMinutes(10)
};

webSocketOptions.AllowedOrigins.Add("http://localhost:5062");
app.UseWebSockets(webSocketOptions);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var messagesRepository = app.Services.GetRequiredService<MessagesRepository>();
await messagesRepository.CreateMessagesTable();

app.Run();