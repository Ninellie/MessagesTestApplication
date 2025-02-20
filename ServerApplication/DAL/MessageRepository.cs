using Npgsql;

namespace ServerApplication.DAL;

public class MessagesRepository(IConfiguration configuration)
{
    private readonly string _connectionString = configuration.GetConnectionString("DefaultConnection")!;
    
    public async Task<List<Message>> GetRecentMessagesAsync()
    {
        var messages = new List<Message>();

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(
            "SELECT id, received_at, text FROM messages WHERE received_at >= NOW() - INTERVAL '10 minutes' ORDER BY received_at DESC",
            connection);
        await using var reader = await command.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            messages.Add(new Message
            {
                Id = reader.GetInt32(0),
                ReceivedAt = reader.GetDateTime(1),
                Text = reader.GetString(2)
            });
        }

        return messages;
    }
    
    public async Task<Message> AddMessageAsync(string text)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(
            "INSERT INTO messages (text) VALUES (@text) RETURNING id, received_at, text", connection);
        command.Parameters.AddWithValue("@text", text);

        await using var reader = await command.ExecuteReaderAsync();
        
        if (await reader.ReadAsync())
        {
            return new Message
            {
                Id = reader.GetInt32(0),
                ReceivedAt = reader.GetDateTime(1),
                Text = reader.GetString(2)
            };
        }

        throw new Exception("Message was not added");
    }

    public async Task CreateMessagesTable()
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(
            "CREATE TABLE messages (\n    id SERIAL PRIMARY KEY,\n    received_at TIMESTAMP DEFAULT NOW(),\n    text VARCHAR(128) NOT NULL\n);", connection);
        
        await command.ExecuteNonQueryAsync();
    }
}
