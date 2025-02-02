using Back.Models;
using Back.Services.Interfaces;
using Npgsql;
using Microsoft.AspNetCore.SignalR;
using System.Security.Cryptography;
using System.Text;

namespace Back.Services;

public enum ChatRequestResult
{
    Success,
    ReceiverNotFound,
    SenderNotFound,
    Failed
}

// Add new enum for detailed error status
public enum RequestActionResult
{
    Success,
    NotFound,
    AlreadyAccepted,
    NotSeller,
    Error
}

public enum ChatStatusResult
{
    Active,
    Disabled,
    NonExistent
}

public class ChatService : IChatService
{
    private readonly IDatabaseService _databaseService;
    private readonly IHubContext<ChatHub> _hubContext;
    private readonly IImageService _imageService;

    public ChatService(IDatabaseService databaseService, IHubContext<ChatHub> hubContext, IImageService imageService)
    {
        _databaseService = databaseService;
        _hubContext = hubContext;
        _imageService = imageService;
    }

    public ChatRequestResult SendRequest(string username, Chat.Request request)
    {
        var getUserIdQuery = @"SELECT id FROM api_schema.user WHERE username = @Username";
        
        NpgsqlConnection connection = null;
        NpgsqlCommand command = null;

        try
        {
            connection = _databaseService.GetConnection();

            // Get buyer (sender) ID
            command = new NpgsqlCommand(getUserIdQuery, connection);
            command.Parameters.AddWithValue("@Username", username);
            var buyerId = (int?)command.ExecuteScalar();
            if (buyerId == null) return ChatRequestResult.SenderNotFound;

            // Get seller (receiver) ID
            command.Parameters.Clear();
            command.Parameters.AddWithValue("@Username", request.Receiver);
            var sellerId = (int?)command.ExecuteScalar();
            if (sellerId == null) return ChatRequestResult.ReceiverNotFound;

            // Insert request
            var insertQuery = @"
                INSERT INTO api_schema.request 
                    (buyer_id, seller_id, request_description, request_status)
                VALUES 
                    (@BuyerId, @SellerId, @Description, 'pending'::api_schema.request_status)";

            command = new NpgsqlCommand(insertQuery, connection);
            command.Parameters.AddWithValue("@BuyerId", buyerId);
            command.Parameters.AddWithValue("@SellerId", sellerId);
            command.Parameters.AddWithValue("@Description", request.Description);

            command.ExecuteNonQuery();
            return ChatRequestResult.Success;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while sending request: {ex.Message}");
            return ChatRequestResult.Failed;
        }
        finally
        {
            command?.Dispose();
            connection?.Close();
        }
    }

    public async Task<List<Chat.RequestResponse>> GetUserRequests(string username)
    {
        var query = @"
        SELECT r.id, 
               r.request_description,
               buyer.username as sender_username,
               buyer.first_name as sender_first_name,
               buyer.last_name as sender_last_name,
               buyer.profile_picture as sender_profile_image
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
                Description = reader.GetString(1),
                SenderProfile = new Chat.UserMiniProfile
                {
                    Username = reader.GetString(2),
                    FirstName = reader.GetString(3),
                    LastName = reader.GetString(4),
                    ProfileImage = reader.IsDBNull(5) ? "" : reader.GetString(5)  // Changed from null to empty string
                }
            });
        }

        return requests;
    }

    public async Task<List<Chat.UserMiniProfile>> GetChatUsers(string username)
    {
        var query = @"
        SELECT DISTINCT 
            other_user.username,
            other_user.first_name,
            other_user.last_name,
            other_user.profile_picture,
            c.chat_status::text as chat_status
        FROM api_schema.chat c
        JOIN api_schema.""user"" current_u 
            ON (current_u.id = c.buyer_id OR current_u.id = c.seller_id)
        JOIN api_schema.""user"" other_user 
            ON (other_user.id = c.buyer_id OR other_user.id = c.seller_id)
            AND other_user.id != current_u.id
        WHERE current_u.username = @Username
        ORDER BY other_user.username";

        var users = new List<Chat.UserMiniProfile>();
        using var connection = _databaseService.GetConnection();
        using var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue("@Username", username);

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            users.Add(new Chat.UserMiniProfile
            {
                Username = reader.GetString(0),
                FirstName = reader.GetString(1),
                LastName = reader.GetString(2),
                ProfileImage = reader.IsDBNull(3) ? "" : reader.GetString(3),
                ChatStatus = reader.GetString(4)
            });
        }

        return users;
    }

    private async Task<bool> ExistingChatBetweenUsers(int buyerId, int sellerId, NpgsqlConnection connection)
    {
        var query = @"
            SELECT EXISTS (
                SELECT 1 
                FROM api_schema.chat 
                WHERE (buyer_id = @BuyerId AND seller_id = @SellerId)
                OR (buyer_id = @SellerId AND seller_id = @BuyerId)
            )";

        using var cmd = new NpgsqlCommand(query, connection);
        cmd.Parameters.AddWithValue("@BuyerId", buyerId);
        cmd.Parameters.AddWithValue("@SellerId", sellerId);
        return (bool)await cmd.ExecuteScalarAsync();
    }

    public async Task<(RequestActionResult Result, string Message)> AcceptRequest(int requestId, string acceptingUsername)
    {
        if (string.IsNullOrEmpty(acceptingUsername))
            return (RequestActionResult.Error, "Invalid username");

        using var connection = _databaseService.GetConnection();
        using var transaction = connection.BeginTransaction();
        
        try 
        {
            // First check if request exists and get buyer/seller IDs
            var checkQuery = @"
                SELECT 
                    r.id,
                    r.buyer_id,
                    r.seller_id,
                    r.request_status::text,
                    seller.username as seller_username,
                    buyer.username as buyer_username
                FROM api_schema.request r
                JOIN api_schema.user seller ON r.seller_id = seller.id
                JOIN api_schema.user buyer ON r.buyer_id = buyer.id
                WHERE r.id = @RequestId";

            using var checkCommand = new NpgsqlCommand(checkQuery, connection);
            checkCommand.Parameters.AddWithValue("@RequestId", requestId);
            
            using var reader = await checkCommand.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
                return (RequestActionResult.NotFound, "Request not found");

            var buyerId = reader.GetInt32(1);
            var sellerId = reader.GetInt32(2);
            var status = reader.GetString(3);
            var sellerUsername = reader.GetString(4);
            var buyerUsername = reader.GetString(5);
            reader.Close();

            // Validate request
            if (sellerUsername != acceptingUsername)
                return (RequestActionResult.NotSeller, "Only the seller can accept requests");
            
            if (status != "pending")
                return (RequestActionResult.AlreadyAccepted, "Request is not pending");

            // Check if there's already an active request
            var checkActiveRequestQuery = @"
                SELECT EXISTS (
                    SELECT 1 
                    FROM api_schema.request r
                    WHERE ((r.buyer_id = @BuyerId AND r.seller_id = @SellerId)
                        OR (r.buyer_id = @SellerId AND r.seller_id = @BuyerId))
                    AND r.request_status = 'accepted'::api_schema.request_status
                )";

            using var activeRequestCmd = new NpgsqlCommand(checkActiveRequestQuery, connection);
            activeRequestCmd.Parameters.AddWithValue("@BuyerId", buyerId);
            activeRequestCmd.Parameters.AddWithValue("@SellerId", sellerId);
            
            var hasActiveRequest = (bool)await activeRequestCmd.ExecuteScalarAsync();
            if (hasActiveRequest)
                return (RequestActionResult.AlreadyAccepted, "There is already an active request between these users");

            // Check for existing chat and reactivate if disabled
            var chatQuery = @"
                WITH UpdatedChat AS (
                    UPDATE api_schema.chat
                    SET chat_status = 'active'
                    WHERE ((buyer_id = @BuyerId AND seller_id = @SellerId)
                        OR (buyer_id = @SellerId AND seller_id = @BuyerId))
                    AND chat_status = 'disabled'
                    RETURNING id
                )
                SELECT id FROM UpdatedChat";

            using var chatCmd = new NpgsqlCommand(chatQuery, connection, transaction);
            chatCmd.Parameters.AddWithValue("@BuyerId", buyerId);
            chatCmd.Parameters.AddWithValue("@SellerId", sellerId);
            
            var existingChatId = await chatCmd.ExecuteScalarAsync();
            
            if (existingChatId == null)
            {
                // Create new chat
                var createChatQuery = @"
                    INSERT INTO api_schema.chat (
                        buyer_id, 
                        seller_id, 
                        history_file_path, 
                        start_date, 
                        chat_status
                    )
                    VALUES (
                        @BuyerId,
                        @SellerId,
                        @HistoryPath,
                        CURRENT_DATE,
                        'active'
                    )
                    RETURNING id";

                using var createCmd = new NpgsqlCommand(createChatQuery, connection, transaction);
                createCmd.Parameters.AddWithValue("@BuyerId", buyerId);
                createCmd.Parameters.AddWithValue("@SellerId", sellerId);
                createCmd.Parameters.AddWithValue("@HistoryPath", 
                    $"/chats/chat_{buyerId}{sellerId}_{DateTime.UtcNow.Ticks}.txt");
                existingChatId = await createCmd.ExecuteScalarAsync();
            }

            // Update request status
            var updateQuery = @"
                UPDATE api_schema.request 
                SET request_status = 'accepted'::api_schema.request_status
                WHERE id = @RequestId";

            using var updateCmd = new NpgsqlCommand(updateQuery, connection, transaction);
            updateCmd.Parameters.AddWithValue("@RequestId", requestId);
            await updateCmd.ExecuteNonQueryAsync();

            await transaction.CommitAsync();
            return (RequestActionResult.Success, "Request accepted successfully");
        }
        catch (PostgresException pgEx)
        {
            await transaction.RollbackAsync();
            Console.WriteLine($"PostgreSQL error in AcceptRequest: {pgEx.MessageText}");
            return (RequestActionResult.Error, $"Database error: {pgEx.MessageText}");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            Console.WriteLine($"Error in AcceptRequest: {ex.Message}");
            return (RequestActionResult.Error, "Database error occurred");
        }
    }

    private async Task<bool> RequestExists(int requestId, NpgsqlConnection connection)
    {
        var query = "SELECT EXISTS(SELECT 1 FROM api_schema.request WHERE id = @RequestId)";
        using var cmd = new NpgsqlCommand(query, connection);
        cmd.Parameters.AddWithValue("@RequestId", requestId);
        return (bool)await cmd.ExecuteScalarAsync();
    }

    public async Task<bool> DeleteRequest(int requestId, string username)
    {
        var query = @"
            WITH request_details AS (
                SELECT seller.username as seller_username
                FROM api_schema.request r
                JOIN api_schema.user seller ON r.seller_id = seller.id
                WHERE r.id = @RequestId
            )
            DELETE FROM api_schema.request 
            WHERE id = @RequestId
            AND EXISTS (
                SELECT 1 
                FROM request_details 
                WHERE seller_username = @Username
            )
            RETURNING id";

        using var connection = _databaseService.GetConnection();
        using var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue("@RequestId", requestId);
        command.Parameters.AddWithValue("@Username", username);

        try
        {
            var result = await command.ExecuteScalarAsync();
            return result != null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting request: {ex.Message}");
            return false;
        }
    }

    public async Task<Chat.MessageText> SendTextMessage(string senderUsername, string receiverUsername, string content)
    {
        using var connection = _databaseService.GetConnection();
        using var transaction = connection.BeginTransaction();

        try
        {
            var (senderId, receiverId) = await GetUserIds(senderUsername, receiverUsername);

            // Verify active chat exists
            var hasActiveChat = await VerifyActiveChatExists(senderUsername, receiverUsername, connection);
            if (!hasActiveChat)
            {
                throw new InvalidOperationException("No active chat found between users");
            }

            var insertQuery = @"
                INSERT INTO api_schema.message (
                    sender_id, 
                    receiver_id, 
                    text_content, 
                    type, 
                    created_at
                )
                VALUES (
                    @SenderId, 
                    @ReceiverId, 
                    @TextContent, 
                    'Text'::api_schema.message_type, 
                    @CreatedAt
                )
                RETURNING id";

            using var cmd = new NpgsqlCommand(insertQuery, connection, transaction);
            cmd.Parameters.AddWithValue("@SenderId", senderId);
            cmd.Parameters.AddWithValue("@ReceiverId", receiverId);
            cmd.Parameters.AddWithValue("@TextContent", content);
            cmd.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow);

            var messageId = (int)await cmd.ExecuteScalarAsync();
            await transaction.CommitAsync();

            var message = new Chat.MessageText
            {
                Id = messageId,
                SenderId = senderId,
                SenderUsername = senderUsername,
                ReceiverId = receiverId,
                ReceiverUsername = receiverUsername,
                TextContent = content,
                Type = Chat.MessageType.Text
            };

            await _hubContext.Clients.User(receiverUsername)
                .SendAsync("ReceiveMessage", message);

            return message;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<Chat.MessageComplex> SendComplexMessage(string senderUsername, Chat.MessageRequest request)
    {
        using var connection = _databaseService.GetConnection();
        using var transaction = connection.BeginTransaction();

        try
        {
            var (senderId, receiverId) = await GetUserIds(senderUsername, request.ReceiverUsername);

            // Verify active chat exists
            var hasActiveChat = await VerifyActiveChatExists(senderUsername, request.ReceiverUsername, connection);
            if (!hasActiveChat)
            {
                throw new InvalidOperationException("No active chat found between users");
            }

            var uploadedImagePaths = new List<string>();
            if (request.Images != null)
            {
                if (request.Images.Length > 10)
                    throw new ArgumentException("Cannot upload more than 10 images");

                foreach (var image in request.Images)
                {
                    if (!_imageService.IsImageValid(image))
                        throw new ArgumentException($"Invalid image: {image.FileName}");
                    
                    var path = await _imageService.UploadImageAsync(image, senderUsername);
                    uploadedImagePaths.Add(path);
                }
            }

            var insertMessageQuery = @"
                INSERT INTO api_schema.message (
                    sender_id, 
                    receiver_id, 
                    text_content, 
                    type, 
                    created_at
                )
                VALUES (
                    @SenderId, 
                    @ReceiverId, 
                    @TextContent, 
                    'Complex'::api_schema.message_type, 
                    @CreatedAt
                )
                RETURNING id";

            using var cmd = new NpgsqlCommand(insertMessageQuery, connection, transaction);
            cmd.Parameters.AddWithValue("@SenderId", senderId);
            cmd.Parameters.AddWithValue("@ReceiverId", receiverId);
            cmd.Parameters.AddWithValue("@TextContent", (object?)request.TextContent ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow);

            var messageId = (int)await cmd.ExecuteScalarAsync();

            foreach (var path in uploadedImagePaths)
            {
                var insertImageQuery = @"
                    INSERT INTO api_schema.message_image (message_id, image_path)
                    VALUES (@MessageId, @ImagePath)";

                using var imgCmd = new NpgsqlCommand(insertImageQuery, connection, transaction);
                imgCmd.Parameters.AddWithValue("@MessageId", messageId);
                imgCmd.Parameters.AddWithValue("@ImagePath", path);
                await imgCmd.ExecuteNonQueryAsync();
            }

            await transaction.CommitAsync();

            var message = new Chat.MessageComplex
            {
                Id = messageId,
                SenderId = senderId,
                SenderUsername = senderUsername,
                ReceiverId = receiverId,
                ReceiverUsername = request.ReceiverUsername,
                TextContent = request.TextContent,
                ImagePaths = uploadedImagePaths,
                Type = Chat.MessageType.Complex
            };

            await _hubContext.Clients.User(request.ReceiverUsername)
                .SendAsync("ReceiveMessage", message);

            return message;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<List<object>> GetConversation(string user1Username, string user2Username)
    {
        return await _databaseService.ExecuteWithConnectionAsync(async connection =>
        {
            var query = @"
                WITH TransactionData AS (
                    SELECT 
                        tm.message_id,
                        tm.transaction_number,
                        tm.transaction_hash,
                        tm.amount,
                        tm.is_approved,
                        tm.approved_at,
                        m2.id as approval_message_id
                    FROM api_schema.transaction_message tm
                    LEFT JOIN api_schema.message m2 ON m2.text_content = tm.transaction_number 
                        AND m2.type = 'TransactionApproval'
                )
                SELECT 
                    m.id, 
                    m.sender_id,
                    sender.username as sender_username,
                    m.receiver_id,
                    receiver.username as receiver_username,
                    m.text_content,
                    m.type,
                    m.created_at,
                    m.is_read,
                    ARRAY_AGG(DISTINCT mi.image_path) FILTER (WHERE mi.image_path IS NOT NULL) as image_paths,
                    td.transaction_number,
                    td.transaction_hash,
                    td.amount,
                    td.is_approved,
                    td.approval_message_id,
                    CASE 
                        WHEN m.type = 'EndRequest' THEN m.text_content
                        WHEN m.type = 'EndRequestApproval' THEN m.text_content
                        ELSE NULL 
                    END as end_request_hash
                FROM api_schema.message m
                JOIN api_schema.user sender ON m.sender_id = sender.id
                JOIN api_schema.user receiver ON m.receiver_id = receiver.id
                LEFT JOIN api_schema.message_image mi ON m.id = mi.message_id
                LEFT JOIN TransactionData td ON m.id = td.message_id
                WHERE (sender.username = @User1Username AND receiver.username = @User2Username)
                   OR (sender.username = @User2Username AND receiver.username = @User1Username)
                GROUP BY 
                    m.id, m.sender_id, sender.username, m.receiver_id, receiver.username, 
                    m.text_content, m.type, m.created_at, m.is_read,
                    td.transaction_number, td.transaction_hash, td.amount, td.is_approved, 
                    td.approval_message_id
                ORDER BY m.created_at ASC";

            var messages = new List<object>();
            await using var cmd = new NpgsqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@User1Username", user1Username);
            cmd.Parameters.AddWithValue("@User2Username", user2Username);

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var type = Enum.Parse<Chat.MessageType>(reader.GetString(6));
                object message = type switch
                {
                    Chat.MessageType.Text => new Chat.MessageText
                    {
                        Id = reader.GetInt32(0),
                        SenderId = reader.GetInt32(1),
                        SenderUsername = reader.GetString(2),
                        ReceiverId = reader.GetInt32(3),
                        ReceiverUsername = reader.GetString(4),
                        TextContent = reader.GetString(5),
                        Type = type
                    },

                    Chat.MessageType.Complex => new Chat.MessageComplex
                    {
                        Id = reader.GetInt32(0),
                        SenderId = reader.GetInt32(1),
                        SenderUsername = reader.GetString(2),
                        ReceiverId = reader.GetInt32(3),
                        ReceiverUsername = reader.GetString(4),
                        TextContent = !reader.IsDBNull(5) ? reader.GetString(5) : null,
                        ImagePaths = !reader.IsDBNull(9) ? 
                            ((string[])reader.GetValue(9)).Where(p => p != null).ToList() : 
                            new List<string>(),
                        Type = type
                    },

                    Chat.MessageType.Transaction => new Chat.MessageTransaction
                    {
                        Id = reader.GetInt32(0),
                        SenderId = reader.GetInt32(1),
                        SenderUsername = reader.GetString(2),
                        ReceiverId = reader.GetInt32(3),
                        ReceiverUsername = reader.GetString(4),
                        Description = reader.GetString(5),
                        Amount = !reader.IsDBNull(12) ? reader.GetDecimal(12) : 0m,
                        TransactionNumber = !reader.IsDBNull(10) ? reader.GetString(10) : "",
                        TransactionHash = !reader.IsDBNull(11) ? reader.GetString(11) : "",
                        IsApproved = !reader.IsDBNull(13) && reader.GetBoolean(13),
                        Type = type,
                        CreatedAt = reader.GetDateTime(7)
                    },

                    Chat.MessageType.TransactionApproval => new Chat.MessageTransactionApproval
                    {
                        Id = reader.GetInt32(0),
                        OriginalTransactionMessageId = !reader.IsDBNull(14) ? reader.GetInt32(14) : 0,
                        Amount = !reader.IsDBNull(12) ? reader.GetDecimal(12) : 0m,
                        ApprovedBy = reader.GetString(2),
                        ApprovedAt = reader.GetDateTime(7),
                        TransactionNumber = reader.GetString(5), // approval messages store transaction number in text_content
                        TransactionHash = !reader.IsDBNull(11) ? reader.GetString(11) : "",
                        Type = type
                    },

                    Chat.MessageType.EndRequest => new Chat.MessageEndRequest
                    {
                        Id = reader.GetInt32(0),
                        SenderId = reader.GetInt32(1),
                        SenderUsername = reader.GetString(2),
                        ReceiverId = reader.GetInt32(3),
                        ReceiverUsername = reader.GetString(4),
                        EndRequestHash = reader.GetString(5), // Get hash from text_content
                        Type = type
                    },

                    Chat.MessageType.EndRequestApproval => new Chat.MessageEndRequestApproval
                    {
                        Id = reader.GetInt32(0),
                        OriginalEndRequestMessageId = !reader.IsDBNull(14) ? reader.GetInt32(14) : 0,
                        ApprovedBy = reader.GetString(2),
                        ApprovedAt = reader.GetDateTime(7),
                        EndRequestHash = reader.GetString(5), // Get hash from text_content
                        Type = type
                    },

                    _ => throw new ArgumentException($"Unknown message type: {type}")
                };

                messages.Add(message);
            }

            return messages;
        });
    }

    private string GenerateTransactionHash(string senderUsername, string receiverUsername, decimal amount, DateTime timestamp)
    {
        var data = $"{senderUsername}:{receiverUsername}:{amount}:{timestamp.Ticks}";
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(data));
        return BitConverter.ToString(bytes).Replace("-", "").ToLower();
    }

    private async Task<bool> VerifyActiveChatExists(string username1, string username2, NpgsqlConnection connection)
    {
        var query = @"
            SELECT EXISTS (
                SELECT 1 
                FROM api_schema.chat c
                JOIN api_schema.user u1 ON c.buyer_id = u1.id OR c.seller_id = u1.id
                JOIN api_schema.user u2 ON (c.buyer_id = u2.id OR c.seller_id = u2.id) AND u2.id != u1.id
                WHERE ((u1.username = @Username1 AND u2.username = @Username2)
                   OR (u1.username = @Username2 AND u2.username = @Username1))
                AND c.chat_status = 'active'::api_schema.chat_status
            )";

        using var cmd = new NpgsqlCommand(query, connection);
        cmd.Parameters.AddWithValue("@Username1", username1);
        cmd.Parameters.AddWithValue("@Username2", username2);
        return (bool)await cmd.ExecuteScalarAsync();
    }

    public async Task<Chat.MessageTransaction> SendTransactionMessage(string senderUsername, Chat.TransactionRequest request)
    {
        using var connection = _databaseService.GetConnection();
        using var transaction = connection.BeginTransaction();

        try
        {
            var (senderId, receiverId) = await GetUserIds(senderUsername, request.ReceiverUsername);

            // Get chat ID
            var chatQuery = @"
                SELECT c.id 
                FROM api_schema.chat c
                WHERE ((c.buyer_id = @SenderId AND c.seller_id = @ReceiverId)
                    OR (c.buyer_id = @ReceiverId AND c.seller_id = @SenderId))
                AND c.chat_status = 'active'";

            using var chatCmd = new NpgsqlCommand(chatQuery, connection, transaction);
            chatCmd.Parameters.AddWithValue("@SenderId", senderId);
            chatCmd.Parameters.AddWithValue("@ReceiverId", receiverId);
            var chatId = (int)await chatCmd.ExecuteScalarAsync();

            var timestamp = DateTime.UtcNow;
            var transactionNumber = $"TR-{timestamp.Ticks}-{chatId}";
            var transactionHash = GenerateTransactionHash(senderUsername, request.ReceiverUsername, request.Amount, timestamp);

            // Insert message
            var insertMessageQuery = @"
                INSERT INTO api_schema.message (
                    sender_id, receiver_id, text_content, type, created_at
                )
                VALUES (
                    @SenderId, @ReceiverId, @Description, 
                    'Transaction'::api_schema.message_type, @CreatedAt
                )
                RETURNING id";

            using var msgCmd = new NpgsqlCommand(insertMessageQuery, connection, transaction);
            msgCmd.Parameters.AddWithValue("@SenderId", senderId);
            msgCmd.Parameters.AddWithValue("@ReceiverId", receiverId);
            msgCmd.Parameters.AddWithValue("@Description", request.Description);
            msgCmd.Parameters.AddWithValue("@CreatedAt", timestamp);
            var messageId = (int)await msgCmd.ExecuteScalarAsync();

            // Insert transaction details
            var insertTransactionQuery = @"
                INSERT INTO api_schema.transaction_message (
                    message_id, chat_id, transaction_number, 
                    transaction_hash, amount, is_approved
                )
                VALUES (
                    @MessageId, @ChatId, @TransactionNumber,
                    @TransactionHash, @Amount, false
                )";

            using var transCmd = new NpgsqlCommand(insertTransactionQuery, connection, transaction);
            transCmd.Parameters.AddWithValue("@MessageId", messageId);
            transCmd.Parameters.AddWithValue("@ChatId", chatId);
            transCmd.Parameters.AddWithValue("@TransactionNumber", transactionNumber);
            transCmd.Parameters.AddWithValue("@TransactionHash", transactionHash);
            transCmd.Parameters.AddWithValue("@Amount", request.Amount);
            await transCmd.ExecuteNonQueryAsync();

            await transaction.CommitAsync();

            var message = new Chat.MessageTransaction
            {
                Id = messageId,
                SenderId = senderId,
                SenderUsername = senderUsername,
                ReceiverId = receiverId,
                ReceiverUsername = request.ReceiverUsername,
                Description = request.Description,
                Amount = request.Amount,
                TransactionNumber = transactionNumber,
                TransactionHash = transactionHash,
                IsApproved = false,
                Type = Chat.MessageType.Transaction,
                CreatedAt = timestamp
            };

            await _hubContext.Clients.User(request.ReceiverUsername)
                .SendAsync("ReceiveTransactionMessage", message);

            return message;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<Chat.MessageTransactionApproval> ApproveTransaction(string transactionHash, string approverUsername)
    {
        using var connection = _databaseService.GetConnection();
        using var transaction = connection.BeginTransaction();

        try
        {
            // Get transaction details
            var getTransactionQuery = @"
                SELECT 
                    m.id as message_id,
                    m.sender_id,
                    sender.username as sender_username,
                    m.receiver_id,
                    tm.amount,
                    w.id as wallet_id,
                    w.amount as wallet_balance,
                    tm.transaction_number,
                    tm.chat_id,
                    tm.is_approved
                FROM api_schema.message m
                JOIN api_schema.transaction_message tm ON m.id = tm.message_id
                JOIN api_schema.user sender ON m.sender_id = sender.id
                JOIN api_schema.user receiver ON m.receiver_id = receiver.id
                JOIN api_schema.wallet w ON receiver.id = w.user_id
                WHERE tm.transaction_hash = @TransactionHash
                AND receiver.username = @ApproverUsername";

            using var cmd = new NpgsqlCommand(getTransactionQuery, connection, transaction);
            cmd.Parameters.AddWithValue("@TransactionHash", transactionHash);
            cmd.Parameters.AddWithValue("@ApproverUsername", approverUsername);

            using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
                throw new InvalidOperationException("Transaction not found or you are not authorized to approve it");

            var messageId = reader.GetInt32(0);
            var senderId = reader.GetInt32(1);
            var senderUsername = reader.GetString(2);
            var receiverId = reader.GetInt32(3);
            var amount = reader.GetDecimal(4);
            var approverWalletId = reader.GetInt32(5);
            var approverBalance = reader.GetDecimal(6);
            var transactionNumber = reader.GetString(7);
            var chatId = reader.GetInt32(8);
            var isApproved = reader.GetBoolean(9);
            reader.Close();

            if (isApproved)
                throw new InvalidOperationException("Transaction has already been approved");

            if (approverBalance < amount)
                throw new InvalidOperationException($"Insufficient funds. Required: {amount}, Available: {approverBalance}");

            // Create approval message
            var approvalMessageQuery = @"
                INSERT INTO api_schema.message (
                    sender_id, receiver_id, text_content, type, created_at
                )
                VALUES (
                    @SenderId, @ReceiverId, @TransactionNumber,
                    'TransactionApproval'::api_schema.message_type, @CreatedAt
                )
                RETURNING id";

            using var approvalCmd = new NpgsqlCommand(approvalMessageQuery, connection, transaction);
            approvalCmd.Parameters.AddWithValue("@SenderId", receiverId);
            approvalCmd.Parameters.AddWithValue("@ReceiverId", senderId);
            approvalCmd.Parameters.AddWithValue("@TransactionNumber", transactionNumber);
            approvalCmd.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow);

            var approvalMessageId = (int)await approvalCmd.ExecuteScalarAsync();

            // Process transaction approval and funds transfer
            await ProcessTransactionApproval(connection, transaction, transactionHash, approvalMessageId, 
                amount, approverWalletId, senderId);

            var approvalMessage = new Chat.MessageTransactionApproval
            {
                Id = approvalMessageId,
                OriginalTransactionMessageId = messageId,
                Amount = amount,
                ApprovedBy = approverUsername,
                ApprovedAt = DateTime.UtcNow,
                TransactionNumber = transactionNumber,
                Type = Chat.MessageType.TransactionApproval
            };

            await _hubContext.Clients.User(senderUsername)
                .SendAsync("ReceiveTransactionApproval", approvalMessage);

            await transaction.CommitAsync();
            return approvalMessage;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private async Task ProcessTransactionApproval(NpgsqlConnection connection, NpgsqlTransaction transaction, 
        string transactionHash, int approvalMessageId, decimal amount, int approverWalletId, int senderId)
    {
        var updateQuery = @"
            UPDATE api_schema.transaction_message 
            SET is_approved = true,
                approved_by_message_id = @ApprovalMessageId,
                approved_at = @ApprovedAt
            WHERE transaction_hash = @TransactionHash;

            UPDATE api_schema.wallet 
            SET amount = amount - @Amount 
            WHERE id = @ApproverWalletId;

            UPDATE api_schema.wallet 
            SET amount = amount + @Amount 
            WHERE user_id = @SenderId;

            INSERT INTO api_schema.inner_transaction 
                (amount, transaction_date, user_id, wallet_id)
            VALUES 
                (@Amount, CURRENT_DATE, @SenderId, @ApproverWalletId)";

        using var cmd = new NpgsqlCommand(updateQuery, connection, transaction);
        cmd.Parameters.AddWithValue("@TransactionHash", transactionHash);
        cmd.Parameters.AddWithValue("@ApprovalMessageId", approvalMessageId);
        cmd.Parameters.AddWithValue("@ApprovedAt", DateTime.UtcNow);
        cmd.Parameters.AddWithValue("@Amount", amount);
        cmd.Parameters.AddWithValue("@ApproverWalletId", approverWalletId);
        cmd.Parameters.AddWithValue("@SenderId", senderId);
        await cmd.ExecuteNonQueryAsync();
    }

    private async Task<(int senderId, int receiverId)> GetUserIds(string senderUsername, string receiverUsername)
    {
        var query = @"
            SELECT id, username 
            FROM api_schema.user 
            WHERE username IN (@Sender, @Receiver)";

        using var cmd = new NpgsqlCommand(query, _databaseService.GetConnection());
        cmd.Parameters.AddWithValue("@Sender", senderUsername);
        cmd.Parameters.AddWithValue("@Receiver", receiverUsername);

        var userIds = new Dictionary<string, int>();
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            userIds[reader.GetString(1)] = reader.GetInt32(0);
        }

        if (!userIds.TryGetValue(senderUsername, out var senderId) || 
            !userIds.TryGetValue(receiverUsername, out var receiverId))
        {
            throw new ArgumentException("Invalid sender or receiver username");
        }

        return (senderId, receiverId);
    }

    public async Task<bool> HasOpenRequest(string username1, string username2)
    {
        if (string.IsNullOrEmpty(username1) || string.IsNullOrEmpty(username2))
            return false;

        var query = @"
        SELECT EXISTS (
            SELECT 1
            FROM api_schema.request r
            JOIN api_schema.user u1 ON r.buyer_id = u1.id
            JOIN api_schema.user u2 ON r.seller_id = u2.id
            WHERE ((u1.username = @Username1 AND u2.username = @Username2)
                OR (u1.username = @Username2 AND u2.username = @Username1))
            AND r.request_status = 'accepted'
        )";

        using var connection = _databaseService.GetConnection();
        using var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue("@Username1", username1);
        command.Parameters.AddWithValue("@Username2", username2);

        try
        {
            return (bool)await command.ExecuteScalarAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error checking open request: {ex.Message}");
            return false;
        }
    }

    private async Task<bool> HasActiveChatBetweenUsers(string username1, string username2, NpgsqlConnection connection)
    {
        var query = @"
            SELECT EXISTS (
                SELECT 1 
                FROM api_schema.chat c
                JOIN api_schema.user u1 ON c.buyer_id = u1.id OR c.seller_id = u1.id
                JOIN api_schema.user u2 ON (c.buyer_id = u2.id OR c.seller_id = u2.id) AND u2.id != u1.id
                WHERE u1.username = @Username1 
                AND u2.username = @Username2
                AND c.chat_status = 'active'
            )";

        using var cmd = new NpgsqlCommand(query, connection);
        cmd.Parameters.AddWithValue("@Username1", username1);
        cmd.Parameters.AddWithValue("@Username2", username2);
        return (bool)await cmd.ExecuteScalarAsync();
    }

    public async Task<ChatStatusResult> GetChatStatus(string username1, string username2)
    {
        var query = @"
            SELECT chat_status::text
            FROM api_schema.chat c
            JOIN api_schema.user u1 ON c.buyer_id = u1.id OR c.seller_id = u1.id
            JOIN api_schema.user u2 ON (c.buyer_id = u2.id OR c.seller_id = u2.id) AND u2.id != u1.id
            WHERE (u1.username = @Username1 AND u2.username = @Username2)
               OR (u1.username = @Username2 AND u2.username = @Username1)
            ORDER BY c.id DESC
            LIMIT 1";

        using var connection = _databaseService.GetConnection();
        using var cmd = new NpgsqlCommand(query, connection);
        cmd.Parameters.AddWithValue("@Username1", username1);
        cmd.Parameters.AddWithValue("@Username2", username2);

        var status = await cmd.ExecuteScalarAsync() as string;
        
        return status switch
        {
            "active" => ChatStatusResult.Active,
            "disabled" => ChatStatusResult.Disabled,
            null => ChatStatusResult.NonExistent,
            _ => ChatStatusResult.NonExistent
        };
    }

    private string GenerateEndRequestHash(string senderUsername, string receiverUsername, DateTime timestamp)
    {
        var data = $"{senderUsername}:{receiverUsername}:{timestamp.Ticks}";
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(data));
        return BitConverter.ToString(bytes).Replace("-", "").ToLower();
    }

    public async Task<Chat.MessageEndRequest> SendEndRequestMessage(string senderUsername, string receiverUsername)
    {
        using var connection = _databaseService.GetConnection();
        using var transaction = connection.BeginTransaction();

        try
        {
            var (senderId, receiverId) = await GetUserIds(senderUsername, receiverUsername);

            // Verify active chat exists
            var hasActiveChat = await VerifyActiveChatExists(senderUsername, receiverUsername, connection);
            if (!hasActiveChat)
            {
                throw new InvalidOperationException("No active chat found between users");
            }

            // Delete any existing end request messages from this sender
            var deleteOldRequestsQuery = @"
                DELETE FROM api_schema.message 
                WHERE sender_id = @SenderId 
                AND receiver_id = @ReceiverId
                AND type = 'EndRequest'::api_schema.message_type";

            using var deleteCmd = new NpgsqlCommand(deleteOldRequestsQuery, connection, transaction);
            deleteCmd.Parameters.AddWithValue("@SenderId", senderId);
            deleteCmd.Parameters.AddWithValue("@ReceiverId", receiverId);
            await deleteCmd.ExecuteNonQueryAsync();

            // Rest of the existing code...
            var timestamp = DateTime.UtcNow;
            var endRequestHash = GenerateEndRequestHash(senderUsername, receiverUsername, timestamp);

            var insertQuery = @"
                INSERT INTO api_schema.message (
                    sender_id, 
                    receiver_id, 
                    text_content, 
                    type, 
                    created_at
                )
                VALUES (
                    @SenderId, 
                    @ReceiverId, 
                    @EndRequestHash, 
                    'EndRequest'::api_schema.message_type, 
                    @CreatedAt
                )
                RETURNING id";

            using var cmd = new NpgsqlCommand(insertQuery, connection, transaction);
            cmd.Parameters.AddWithValue("@SenderId", senderId);
            cmd.Parameters.AddWithValue("@ReceiverId", receiverId);
            cmd.Parameters.AddWithValue("@EndRequestHash", endRequestHash);
            cmd.Parameters.AddWithValue("@CreatedAt", timestamp);

            var messageId = (int)await cmd.ExecuteScalarAsync();
            await transaction.CommitAsync();

            var message = new Chat.MessageEndRequest
            {
                Id = messageId,
                SenderId = senderId,
                SenderUsername = senderUsername,
                ReceiverId = receiverId,
                ReceiverUsername = receiverUsername,
                EndRequestHash = endRequestHash,
                Type = Chat.MessageType.EndRequest
            };

            await _hubContext.Clients.User(receiverUsername)
                .SendAsync("ReceiveEndRequestMessage", message);

            return message;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<Chat.MessageEndRequestApproval> ApproveEndRequest(string endRequestHash, string approverUsername)
    {
        using var connection = _databaseService.GetConnection();
        using var transaction = connection.BeginTransaction();

        try
        {
            // First get the request and chat details
            var getDetailsQuery = @"
                WITH EndRequestApproval AS (
                    SELECT m.id 
                    FROM api_schema.message m
                    WHERE m.text_content = @EndRequestHash 
                    AND m.type = 'EndRequestApproval'
                )
                SELECT 
                    m.id as message_id,
                    m.sender_id,
                    sender.username as sender_username,
                    m.receiver_id,
                    receiver.username as receiver_username,
                    m.created_at,
                    c.id as chat_id,
                    r.id as request_id
                FROM api_schema.message m
                JOIN api_schema.user sender ON m.sender_id = sender.id
                JOIN api_schema.user receiver ON m.receiver_id = receiver.id
                JOIN api_schema.chat c ON (c.buyer_id = m.sender_id AND c.seller_id = m.receiver_id)
                    OR (c.buyer_id = m.receiver_id AND c.seller_id = m.sender_id)
                JOIN api_schema.request r ON (r.buyer_id = m.sender_id AND r.seller_id = m.receiver_id)
                    OR (r.buyer_id = m.receiver_id AND r.seller_id = m.sender_id)
                LEFT JOIN EndRequestApproval era ON m.id = era.id
                WHERE m.text_content = @EndRequestHash
                AND m.type = 'EndRequest'
                AND receiver.username = @ApproverUsername
                AND c.chat_status = 'active'
                AND r.request_status = 'accepted'
                AND era.id IS NULL
                FOR UPDATE";  // Add locking to prevent race conditions

            using var cmd = new NpgsqlCommand(getDetailsQuery, connection, transaction);
            cmd.Parameters.AddWithValue("@EndRequestHash", endRequestHash);
            cmd.Parameters.AddWithValue("@ApproverUsername", approverUsername);

            using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
                throw new InvalidOperationException("End request not found or you are not authorized to approve it");

            var messageId = reader.GetInt32(0);
            var senderId = reader.GetInt32(1);
            var senderUsername = reader.GetString(2);
            var receiverId = reader.GetInt32(3);
            var receiverUsername = reader.GetString(4);
            var chatId = reader.GetInt32(6);
            var requestId = reader.GetInt32(7);
            reader.Close();

            // Update both chat and request status in a single atomic operation
            var updateStatusQuery = @"
                WITH chat_update AS (
                    UPDATE api_schema.chat
                    SET 
                        chat_status = 'disabled'::api_schema.chat_status,
                        message_id = @MessageId
                    WHERE id = @ChatId 
                    AND chat_status = 'active'
                    RETURNING id
                ),
                request_update AS (
                    UPDATE api_schema.request
                    SET request_status = 'completed'::api_schema.request_status
                    WHERE id = @RequestId
                    AND request_status = 'accepted'
                    AND EXISTS (SELECT 1 FROM chat_update)
                    RETURNING seller_id
                )
                UPDATE api_schema.""user""
                SET completed_requests = completed_requests + 1
                WHERE id = (SELECT seller_id FROM request_update)
                RETURNING id;";

            using var updateCmd = new NpgsqlCommand(updateStatusQuery, connection, transaction);
            updateCmd.Parameters.AddWithValue("@ChatId", chatId);
            updateCmd.Parameters.AddWithValue("@MessageId", messageId);
            updateCmd.Parameters.AddWithValue("@RequestId", requestId);

            var result = await updateCmd.ExecuteScalarAsync();
            if (result == null)
            {
                throw new InvalidOperationException("Failed to update chat and request status");
            }

            // Create approval message
            var approvalMessageQuery = @"
                INSERT INTO api_schema.message (
                    sender_id,
                    receiver_id,
                    text_content,
                    type,
                    created_at
                )
                VALUES (
                    @SenderId,
                    @ReceiverId,
                    @EndRequestHash,
                    'EndRequestApproval'::api_schema.message_type,
                    @CreatedAt
                )
                RETURNING id";

            using var approvalCmd = new NpgsqlCommand(approvalMessageQuery, connection, transaction);
            approvalCmd.Parameters.AddWithValue("@SenderId", receiverId);
            approvalCmd.Parameters.AddWithValue("@ReceiverId", senderId);
            approvalCmd.Parameters.AddWithValue("@EndRequestHash", endRequestHash);
            approvalCmd.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow);

            var approvalMessageId = (int)await approvalCmd.ExecuteScalarAsync();

            await transaction.CommitAsync();

            var approvalMessage = new Chat.MessageEndRequestApproval
            {
                Id = approvalMessageId,
                OriginalEndRequestMessageId = messageId,
                ApprovedBy = approverUsername,
                ApprovedAt = DateTime.UtcNow,
                EndRequestHash = endRequestHash,
                Type = Chat.MessageType.EndRequestApproval
            };

            // Notify both users about the changes
            await _hubContext.Clients.User(senderUsername)
                .SendAsync("ReceiveEndRequestApproval", approvalMessage);

            await _hubContext.Clients.Users(new[] { senderUsername, receiverUsername })
                .SendAsync("ChatStatusChanged", new { ChatId = chatId, Status = "disabled" });

            return approvalMessage;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}