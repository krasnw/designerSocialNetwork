using Back.Models;
using Back.Services.Interfaces;
using Npgsql;
using Microsoft.AspNetCore.SignalR;

namespace Back.Services;

public class ChatService : IChatService
{
    private readonly IDatabaseService _databaseService;
    private readonly IHubContext<ChatHub> _hubContext;

    public ChatService(IDatabaseService databaseService, IHubContext<ChatHub> hubContext)
    {
        _databaseService = databaseService;
        _hubContext = hubContext;
    }

    public bool SendRequest(string username, Chat.Request request)
    {
        if (username == request.Receiver)
        {
            throw new InvalidOperationException("Cannot send request to yourself");
        }

        var getUserIdQuery = @"
    SELECT id FROM api_schema.user WHERE username = @Username";

        var checkExistingRequestQuery = @"
    SELECT COUNT(*) FROM api_schema.request r
    JOIN api_schema.user buyer ON r.buyer_id = buyer.id
    JOIN api_schema.user seller ON r.seller_id = seller.id
    WHERE buyer.username = @SenderUsername 
    AND seller.username = @ReceiverUsername 
    AND r.request_status = 'pending'";

        NpgsqlConnection connection = null;
        NpgsqlCommand command = null;

        try
        {
            connection = _databaseService.GetConnection();

            // Check for existing pending request
            command = new NpgsqlCommand(checkExistingRequestQuery, connection);
            command.Parameters.AddWithValue("@SenderUsername", username);
            command.Parameters.AddWithValue("@ReceiverUsername", request.Receiver);
            var existingRequests = (long)command.ExecuteScalar();
            
            if (existingRequests > 0)
            {
                throw new InvalidOperationException("A pending request already exists");
            }

            //sender ID
            command = new NpgsqlCommand(getUserIdQuery, connection);
            command.Parameters.AddWithValue("@Username", username);
            var senderId = (int?)command.ExecuteScalar();
            if (senderId == null) throw new Exception("Sender not found");

            //receiver ID
            command.Parameters.Clear();
            command.Parameters.AddWithValue("@Username", request.Receiver);
            var receiverId = (int?)command.ExecuteScalar();
            if (receiverId == null) throw new Exception("Receiver not found");

            //main request
            var insertQuery = @"
        INSERT INTO api_schema.request (buyer_id, seller_id, request_description, request_status)
        VALUES (@SenderId, @ReceiverId, @Description, @Status::api_schema.request_status)";

            command = new NpgsqlCommand(insertQuery, connection);
            command.Parameters.AddWithValue("@SenderId", senderId);
            command.Parameters.AddWithValue("@ReceiverId", receiverId);
            command.Parameters.AddWithValue("@Description", request.Description);
            command.Parameters.AddWithValue("@Status", "pending");

            command.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while sending request: {ex.Message}");
            return false;
        }
        finally
        {
            command?.Dispose();
            connection?.Close();
        }

        return true;
    }

    public async Task<List<Chat.RequestResponse>> GetUserRequests(string username)
    {
        var query = @"
            SELECT r.id, 
                   buyer.username as sender_username,
                   seller.username as receiver_username,
                   r.request_description,
                   r.request_status
            FROM api_schema.request r
            JOIN api_schema.""user"" buyer ON r.buyer_id = buyer.id
            JOIN api_schema.""user"" seller ON r.seller_id = seller.id
            WHERE seller.username = @Username
            AND r.request_status = 'pending'
            ORDER BY r.id DESC";

        var requests = new List<Chat.RequestResponse>();
        using var connection = _databaseService.GetConnection();
        using var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue("@Username", username);

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            requests.Add(new Chat.RequestResponse
            {
                Id = reader.GetInt32(0),
                Sender = reader.GetString(1),
                Receiver = reader.GetString(2),
                Description = reader.GetString(3),
                Status = reader.GetString(4)
            });
        }

        return requests;
    }

    public async Task<List<string>> GetChatUsers(string username)
    {
        var query = @"
            SELECT DISTINCT u.username
            FROM api_schema.chat c
            JOIN api_schema.""user"" u ON 
                (u.id = c.buyer_id OR u.id = c.seller_id)
            JOIN api_schema.""user"" currentUser ON 
                (currentUser.id = c.buyer_id OR currentUser.id = c.seller_id)
            WHERE currentUser.username = @Username AND u.username != @Username
            ORDER BY u.username";

        var users = new List<string>();
        using var connection = _databaseService.GetConnection();
        using var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue("@Username", username);

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            users.Add(reader.GetString(0));
        }

        return users;
    }

    public async Task<bool> AcceptRequest(int requestId)
    {
        // First check if request exists and is in pending state
        var checkQuery = @"
            SELECT COUNT(*) 
            FROM api_schema.request 
            WHERE id = @RequestId 
            AND request_status = 'pending'";

        var updateQuery = @"
            UPDATE api_schema.request 
            SET request_status = 'accepted'::api_schema.request_status
            WHERE id = @RequestId
            AND request_status = 'pending';

            INSERT INTO api_schema.chat (buyer_id, seller_id, history_file_path, start_date, chat_status)
            SELECT r.buyer_id, r.seller_id, 
                   '/chats/chat_' || r.buyer_id || r.seller_id || '_' || EXTRACT(EPOCH FROM now())::integer || '.txt',
                   CURRENT_DATE,
                   'active'::api_schema.chat_status
            FROM api_schema.request r
            WHERE r.id = @RequestId;";

        using var connection = _databaseService.GetConnection();
        
        // Check if request exists
        using var checkCommand = new NpgsqlCommand(checkQuery, connection);
        checkCommand.Parameters.AddWithValue("@RequestId", requestId);
        var requestExists = (long)await checkCommand.ExecuteScalarAsync() > 0;

        if (!requestExists)
        {
            return false;
        }

        // If request exists, proceed with update
        using var updateCommand = new NpgsqlCommand(updateQuery, connection);
        updateCommand.Parameters.AddWithValue("@RequestId", requestId);

        try
        {
            await updateCommand.ExecuteNonQueryAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error accepting request: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> DeleteRequest(int requestId)
    {
        var query = @"DELETE FROM api_schema.request WHERE id = @RequestId";

        using var connection = _databaseService.GetConnection();
        using var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue("@RequestId", requestId);

        try
        {
            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting request: {ex.Message}");
            return false;
        }
    }

    public async Task<Chat.Message> SendMessage(string senderUsername, Chat.MessageDto message)
    {
        // Get user IDs from usernames
        var getUserIdsQuery = @"
            SELECT id FROM api_schema.user WHERE username = @Username";

        using var connection = _databaseService.GetConnection();
        
        // Get sender ID from JWT token username
        using var senderCommand = new NpgsqlCommand(getUserIdsQuery, connection);
        senderCommand.Parameters.AddWithValue("@Username", senderUsername);
        var senderId = (int?)await senderCommand.ExecuteScalarAsync();
        if (senderId == null) throw new Exception("Sender not found");

        // Get receiver ID
        using var receiverCommand = new NpgsqlCommand(getUserIdsQuery, connection);
        receiverCommand.Parameters.AddWithValue("@Username", message.ReceiverUsername);
        var receiverId = (int?)await receiverCommand.ExecuteScalarAsync();
        if (receiverId == null) throw new Exception("Receiver not found");

        var newMessage = new Chat.Message
        {
            SenderId = senderId.Value,
            ReceiverId = receiverId.Value,
            Content = message.Content,
            Type = message.Type,
            CreatedAt = DateTime.UtcNow
        };

        // Save message to database
        var insertQuery = @"
            INSERT INTO api_schema.message (sender_id, receiver_id, content, type, created_at)
            VALUES (@SenderId, @ReceiverId, @Content, @Type, @CreatedAt)
            RETURNING id";

        using var command = new NpgsqlCommand(insertQuery, connection);
        command.Parameters.AddWithValue("@SenderId", newMessage.SenderId);
        command.Parameters.AddWithValue("@ReceiverId", newMessage.ReceiverId);
        command.Parameters.AddWithValue("@Content", newMessage.Content);
        command.Parameters.AddWithValue("@Type", newMessage.Type.ToString());
        command.Parameters.AddWithValue("@CreatedAt", newMessage.CreatedAt);

        newMessage.Id = (int)await command.ExecuteScalarAsync();

        // Notify receiver via SignalR
        await _hubContext.Clients.User(message.ReceiverUsername)
            .SendAsync("ReceiveMessage", newMessage);

        return newMessage;
    }

    public async Task<List<Chat.Message>> GetConversation(string user1Username, string user2Username)
    {
        var query = @"
            SELECT m.id, m.sender_id, m.receiver_id, m.content, m.type, m.created_at, m.is_read
            FROM api_schema.message m
            JOIN api_schema.user u1 ON m.sender_id = u1.id
            JOIN api_schema.user u2 ON m.receiver_id = u2.id
            WHERE (u1.username = @User1Username AND u2.username = @User2Username)
               OR (u1.username = @User2Username AND u2.username = @User1Username)
            ORDER BY m.created_at";

        var messages = new List<Chat.Message>();
        using var connection = _databaseService.GetConnection();
        using var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue("@User1Username", user1Username);
        command.Parameters.AddWithValue("@User2Username", user2Username);

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            messages.Add(new Chat.Message
            {
                Id = reader.GetInt32(0),
                SenderId = reader.GetInt32(1),
                ReceiverId = reader.GetInt32(2),
                Content = reader.GetString(3),
                Type = Enum.Parse<Chat.MessageType>(reader.GetString(4)),  // Updated
                CreatedAt = reader.GetDateTime(5),
                IsRead = reader.GetBoolean(6)
            });
        }

        return messages;
    }

    public async Task<Chat.PaymentRequest> CreatePaymentRequest(Chat.PaymentRequestDto request)
    {
        var newRequest = new Chat.PaymentRequest
        {
            RequesterId = request.RequesterId,
            ReceiverId = request.ReceiverId,
            Amount = request.Amount,
            IsPaid = false
        };

        // Save payment request to database
        var insertQuery = @"
            INSERT INTO api_schema.payment_request (requester_id, receiver_id, amount, is_paid)
            VALUES ((SELECT id FROM api_schema.user WHERE username = @RequesterUsername), 
                    (SELECT id FROM api_schema.user WHERE username = @ReceiverUsername), 
                    @Amount, @IsPaid)
            RETURNING id";

        using var connection = _databaseService.GetConnection();
        using var command = new NpgsqlCommand(insertQuery, connection);
        command.Parameters.AddWithValue("@RequesterUsername", request.RequesterUsername);
        command.Parameters.AddWithValue("@ReceiverUsername", request.ReceiverUsername);
        command.Parameters.AddWithValue("@Amount", newRequest.Amount);
        command.Parameters.AddWithValue("@IsPaid", newRequest.IsPaid);

        newRequest.Id = (int)await command.ExecuteScalarAsync();

        // Notify receiver via SignalR
        await _hubContext.Clients.User(request.ReceiverUsername)
            .SendAsync("ReceivePaymentRequest", request);

        return newRequest;
    }
}