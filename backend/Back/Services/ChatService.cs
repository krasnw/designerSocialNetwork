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
        var checkExistingQuery = @"
        SELECT COUNT(*) 
        FROM api_schema.request r
        JOIN api_schema.user u1 ON r.buyer_id = u1.id
        JOIN api_schema.user u2 ON r.seller_id = u2.id
        WHERE r.request_status = 'accepted'
        AND ((u1.username = @Username1 AND u2.username = @Username2)
         OR (u1.username = @Username2 AND u2.username = @Username1))";

        var getUserIdQuery = @"SELECT id FROM api_schema.user WHERE username = @Username";

        NpgsqlConnection connection = null;
        NpgsqlCommand command = null;

        try
        {
            connection = _databaseService.GetConnection();

            // Check for existing accepted request
            command = new NpgsqlCommand(checkExistingQuery, connection);
            command.Parameters.AddWithValue("@Username1", username);
            command.Parameters.AddWithValue("@Username2", request.Receiver);
            var existingAcceptedRequests = (long)command.ExecuteScalar();

            if (existingAcceptedRequests > 0)
            {
                return ChatRequestResult.Failed;
            }

            //sender ID
            command = new NpgsqlCommand(getUserIdQuery, connection);
            command.Parameters.AddWithValue("@Username", username);
            var senderId = (int?)command.ExecuteScalar();
            if (senderId == null) return ChatRequestResult.SenderNotFound;

            //receiver ID
            command.Parameters.Clear();
            command.Parameters.AddWithValue("@Username", request.Receiver);
            var receiverId = (int?)command.ExecuteScalar();
            if (receiverId == null) return ChatRequestResult.ReceiverNotFound;

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
            other_user.profile_picture
        FROM api_schema.request r
        JOIN api_schema.""user"" current_u 
            ON (current_u.id = r.buyer_id OR current_u.id = r.seller_id)
        JOIN api_schema.""user"" other_user 
            ON (other_user.id = r.buyer_id OR other_user.id = r.seller_id)
            AND other_user.id != current_u.id
        WHERE current_u.username = @Username
        AND r.request_status = 'accepted'
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
                ProfileImage = reader.IsDBNull(3) ? "" : reader.GetString(3)
            });
        }

        return users;
    }

    public async Task<(RequestActionResult Result, string Message)> AcceptRequest(int requestId, string acceptingUsername)
    {
        if (string.IsNullOrEmpty(acceptingUsername))
            return (RequestActionResult.Error, "Invalid username");

        using var connection = _databaseService.GetConnection();
        
        try 
        {
            // First check if request exists
            var exists = await RequestExists(requestId, connection);
            if (!exists)
                return (RequestActionResult.NotFound, "Request not found");

            var checkQuery = @"
                WITH request_details AS (
                    SELECT 
                        r.id,
                        r.request_status,
                        seller.username as seller_username,
                        EXISTS (
                            SELECT 1
                            FROM api_schema.request r2
                            WHERE r2.request_status = 'accepted'
                            AND (
                                (r2.buyer_id = r.buyer_id AND r2.seller_id = r.seller_id)
                                OR (r2.buyer_id = r.seller_id AND r2.seller_id = r.buyer_id)
                            )
                        ) as has_accepted_request
                    FROM api_schema.request r
                    JOIN api_schema.user seller ON r.seller_id = seller.id
                    WHERE r.id = @RequestId
                )
                SELECT 
                    CASE 
                        WHEN (SELECT seller_username FROM request_details) != @AcceptingUsername THEN 'NotSeller'
                        WHEN (SELECT has_accepted_request FROM request_details) THEN 'AlreadyAccepted'
                        WHEN (SELECT request_status FROM request_details) != 'pending' THEN 'AlreadyAccepted'
                        ELSE 'Success'
                    END as status";

            // Check request status
            using (var checkCommand = new NpgsqlCommand(checkQuery, connection))
            {
                checkCommand.Parameters.AddWithValue("@RequestId", requestId);
                checkCommand.Parameters.AddWithValue("@AcceptingUsername", acceptingUsername);
                var status = await checkCommand.ExecuteScalarAsync() as string;

                switch (status)
                {
                    case "NotSeller":
                        return (RequestActionResult.NotSeller, "Only the seller can accept requests");
                    case "AlreadyAccepted":
                        return (RequestActionResult.AlreadyAccepted, "Request cannot be accepted - there is already an active chat between these users");
                    case "Success":
                        var updateQuery = @"
                            UPDATE api_schema.request 
                            SET request_status = 'accepted'::api_schema.request_status
                            WHERE id = @RequestId;

                            INSERT INTO api_schema.chat (buyer_id, seller_id, history_file_path, start_date, chat_status)
                            SELECT r.buyer_id, r.seller_id, 
                                   '/chats/chat_' || r.buyer_id || r.seller_id || '_' || EXTRACT(EPOCH FROM now())::integer || '.txt',
                                   CURRENT_DATE,
                                   'active'::api_schema.chat_status
                            FROM api_schema.request r
                            WHERE r.id = @RequestId;";

                        using (var updateCommand = new NpgsqlCommand(updateQuery, connection))
                        {
                            updateCommand.Parameters.AddWithValue("@RequestId", requestId);
                            await updateCommand.ExecuteNonQueryAsync();
                            return (RequestActionResult.Success, "Request accepted successfully");
                        }
                    default:
                        return (RequestActionResult.Error, "Unknown error occurred");
                }
            }
        }
        catch (Exception ex)
        {
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

    public async Task<Chat.Message> SendMessage(string senderUsername, Chat.MessageDto message)
    {
        using var connection = _databaseService.GetConnection();
        using var transaction = connection.BeginTransaction();

        try
        {
            // Get user IDs
            var (senderId, receiverId) = await GetUserIds(senderUsername, message.ReceiverUsername);

            // Handle image uploads first if any
            var uploadedImagePaths = new List<string>();
            if (message.Images != null && message.Images.Any())
            {
                foreach (var image in message.Images)
                {
                    if (!_imageService.IsImageValid(image))
                    {
                        throw new ArgumentException($"Invalid image: {image.FileName}");
                    }
                    var path = await _imageService.UploadImageAsync(image, senderUsername);
                    uploadedImagePaths.Add(path);
                }
            }

            // Create message
            var messageType = uploadedImagePaths.Any() ? Chat.MessageType.Complex : Chat.MessageType.Text;
            var insertMessageQuery = @"
                INSERT INTO api_schema.message (sender_id, receiver_id, text_content, type, created_at)
                VALUES (@SenderId, @ReceiverId, @TextContent, @Type::api_schema.message_type, @CreatedAt)
                RETURNING id";

            using var cmd = new NpgsqlCommand(insertMessageQuery, connection);
            cmd.Parameters.AddWithValue("@SenderId", senderId);
            cmd.Parameters.AddWithValue("@ReceiverId", receiverId);
            cmd.Parameters.AddWithValue("@TextContent", (object?)message.TextContent ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Type", messageType.ToString("G")); // "G" format ensures exact enum name
            cmd.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow);

            var messageId = (int)await cmd.ExecuteScalarAsync();

            // Store image paths if any
            if (uploadedImagePaths.Any())
            {
                var insertImageQuery = @"
                    INSERT INTO api_schema.message_image (message_id, image_path)
                    VALUES (@MessageId, @ImagePath)";

                foreach (var path in uploadedImagePaths)
                {
                    using var imgCmd = new NpgsqlCommand(insertImageQuery, connection);
                    imgCmd.Parameters.AddWithValue("@MessageId", messageId);
                    imgCmd.Parameters.AddWithValue("@ImagePath", path);
                    await imgCmd.ExecuteNonQueryAsync();
                }
            }

            await transaction.CommitAsync();

            // Create response object
            var newMessage = new Chat.Message
            {
                Id = messageId,
                SenderId = senderId,
                SenderUsername = senderUsername,
                ReceiverId = receiverId,
                ReceiverUsername = message.ReceiverUsername,
                TextContent = message.TextContent,
                ImagePaths = uploadedImagePaths,
                Type = messageType,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };

            // Notify through SignalR
            await _hubContext.Clients.User(message.ReceiverUsername)
                .SendAsync("ReceiveMessage", newMessage);

            return newMessage;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
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
                AND c.chat_status = 'active'
            )";

        using var cmd = new NpgsqlCommand(query, connection);
        cmd.Parameters.AddWithValue("@Username1", username1);
        cmd.Parameters.AddWithValue("@Username2", username2);
        return (bool)await cmd.ExecuteScalarAsync();
    }

    public async Task<Chat.TransactionMessageResponse> SendTransactionMessage(string senderUsername, Chat.TransactionMessage message)
    {
        using var connection = _databaseService.GetConnection();
        using var transaction = connection.BeginTransaction();

        try
        {
            // Check for active chat first
            var hasActiveChat = await VerifyActiveChatExists(senderUsername, message.ReceiverUsername, connection);
            if (!hasActiveChat)
            {
                throw new InvalidOperationException("Cannot send transaction: no active chat found between users");
            }

            // First, get the chat ID
            var chatQuery = @"
                SELECT c.id 
                FROM api_schema.chat c
                JOIN api_schema.user u1 ON c.buyer_id = u1.id OR c.seller_id = u1.id
                JOIN api_schema.user u2 ON (c.buyer_id = u2.id OR c.seller_id = u2.id) AND u2.id != u1.id
                WHERE u1.username = @Username1 
                AND u2.username = @Username2
                AND c.chat_status = 'active'";

            using var chatCmd = new NpgsqlCommand(chatQuery, connection);
            chatCmd.Parameters.AddWithValue("@Username1", senderUsername);
            chatCmd.Parameters.AddWithValue("@Username2", message.ReceiverUsername);
            var chatId = await chatCmd.ExecuteScalarAsync() as int?;

            if (!chatId.HasValue)
                throw new InvalidOperationException("No active chat found between users");

            var (senderId, receiverId) = await GetUserIds(senderUsername, message.ReceiverUsername);
            var timestamp = DateTime.UtcNow;
            var transactionNumber = $"TR-{timestamp.Ticks}-{chatId}";
            var transactionHash = GenerateTransactionHash(senderUsername, message.ReceiverUsername, message.Amount, timestamp);

            // Insert message
            var insertMessageQuery = @"
                INSERT INTO api_schema.message (sender_id, receiver_id, text_content, type, created_at)
                VALUES (@SenderId, @ReceiverId, @Description, 'Transaction'::api_schema.message_type, @CreatedAt)
                RETURNING id";

            using var cmd = new NpgsqlCommand(insertMessageQuery, connection, transaction);
            cmd.Parameters.AddWithValue("@SenderId", senderId);
            cmd.Parameters.AddWithValue("@ReceiverId", receiverId);
            cmd.Parameters.AddWithValue("@Description", message.Description);
            cmd.Parameters.AddWithValue("@Type", Chat.MessageType.Transaction.ToString("G"));
            cmd.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow);

            var messageId = (int)await cmd.ExecuteScalarAsync();

            // Insert transaction details with chat_id and transaction_number
            var insertTransactionQuery = @"
                INSERT INTO api_schema.transaction_message 
                    (message_id, chat_id, transaction_number, transaction_hash, amount, is_approved)
                VALUES (@MessageId, @ChatId, @TransactionNumber, @TransactionHash, @Amount, false)";

            using var transCmd = new NpgsqlCommand(insertTransactionQuery, connection, transaction);
            transCmd.Parameters.AddWithValue("@MessageId", messageId);
            transCmd.Parameters.AddWithValue("@ChatId", chatId.Value);
            transCmd.Parameters.AddWithValue("@TransactionNumber", transactionNumber);
            transCmd.Parameters.AddWithValue("@TransactionHash", transactionHash);
            transCmd.Parameters.AddWithValue("@Amount", message.Amount);
            await transCmd.ExecuteNonQueryAsync();

            await transaction.CommitAsync();

            var response = new Chat.TransactionMessageResponse
            {
                MessageId = messageId,
                TransactionNumber = transactionNumber,
                TransactionHash = transactionHash,
                ChatId = chatId.Value,
                SenderUsername = senderUsername,
                ReceiverUsername = message.ReceiverUsername,
                Amount = message.Amount,
                Description = message.Description,
                IsApproved = false,
                CreatedAt = DateTime.UtcNow
            };

            // Notify through SignalR
            await _hubContext.Clients.User(message.ReceiverUsername)
                .SendAsync("ReceiveTransactionMessage", response);

            return response;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> ApproveTransaction(string transactionHash, string approverUsername)
    {
        using var connection = _databaseService.GetConnection();
        using var transaction = connection.BeginTransaction();

        try
        {
            // Verify transaction exists and belongs to approver
            var verifyQuery = @"
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
                    tm.is_approved,
                    tm.id as transaction_message_id
                FROM api_schema.message m
                JOIN api_schema.transaction_message tm ON m.id = tm.message_id
                JOIN api_schema.user sender ON m.sender_id = sender.id
                JOIN api_schema.user receiver ON m.receiver_id = receiver.id
                JOIN api_schema.wallet w ON receiver.id = w.user_id
                WHERE tm.transaction_hash = @TransactionHash 
                AND receiver.username = @ApproverUsername";

            using var cmd = new NpgsqlCommand(verifyQuery, connection, transaction);
            cmd.Parameters.AddWithValue("@TransactionHash", transactionHash);
            cmd.Parameters.AddWithValue("@ApproverUsername", approverUsername);

            using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
            {
                throw new InvalidOperationException("Transaction not found or you are not authorized to approve it");
            }

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
            var transactionMessageId = reader.GetInt32(10);
            reader.Close();

            // Check if already approved
            if (isApproved)
            {
                throw new InvalidOperationException("Transaction has already been approved");
            }

            // Check if approver has enough balance
            if (approverBalance < amount)
            {
                throw new InvalidOperationException($"Insufficient funds. Required: {amount}, Available: {approverBalance}");
            }

            // Add automatic approval message
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
                    @TransactionNumber,
                    'TransactionApproval'::api_schema.message_type,
                    @CreatedAt
                )
                RETURNING id;";

            using var approvalCmd = new NpgsqlCommand(approvalMessageQuery, connection, transaction);
            approvalCmd.Parameters.AddWithValue("@SenderId", receiverId);
            approvalCmd.Parameters.AddWithValue("@ReceiverId", senderId);
            approvalCmd.Parameters.AddWithValue("@TransactionNumber", transactionNumber);
            approvalCmd.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow);
            
            var approvalMessageId = (int)await approvalCmd.ExecuteScalarAsync();

            // Update transaction with approval details
            var updateQuery = @"
                UPDATE api_schema.transaction_message 
                SET is_approved = true,
                    approved_by_message_id = @ApprovalMessageId,
                    approved_at = @ApprovedAt
                WHERE transaction_hash = @TransactionHash";

            using var updateCmd = new NpgsqlCommand(updateQuery, connection, transaction);
            updateCmd.Parameters.AddWithValue("@TransactionHash", transactionHash);
            updateCmd.Parameters.AddWithValue("@ApprovalMessageId", approvalMessageId);
            updateCmd.Parameters.AddWithValue("@ApprovedAt", DateTime.UtcNow);
            await updateCmd.ExecuteNonQueryAsync();

            // Process the money transfer
            var transferQuery = @"
                -- Deduct from approver's wallet
                UPDATE api_schema.wallet 
                SET amount = amount - @Amount 
                WHERE id = @ApproverWalletId;

                -- Add to sender's wallet
                UPDATE api_schema.wallet 
                SET amount = amount + @Amount 
                WHERE user_id = @SenderId;

                -- Create inner transaction record
                INSERT INTO api_schema.inner_transaction 
                (amount, transaction_date, user_id, wallet_id)
                VALUES (@Amount, CURRENT_DATE, @SenderId, @ApproverWalletId)";

            using var transferCmd = new NpgsqlCommand(transferQuery, connection, transaction);
            transferCmd.Parameters.AddWithValue("@Amount", amount);
            transferCmd.Parameters.AddWithValue("@ApproverWalletId", approverWalletId);
            transferCmd.Parameters.AddWithValue("@SenderId", senderId);
            await transferCmd.ExecuteNonQueryAsync();

            // Create and send approval notification
            var approvalMessage = new Chat.TransactionApprovalMessage
            {
                Id = approvalMessageId,
                OriginalTransactionMessageId = messageId,
                TransactionHash = transactionHash,
                Amount = amount,
                ApprovedBy = approverUsername,
                ApprovedAt = DateTime.UtcNow,
                TransactionNumber = transactionNumber,
                ChatId = chatId,
                Type = Chat.MessageType.TransactionApproval
            };

            await _hubContext.Clients.User(senderUsername)
                .SendAsync("ReceiveTransactionApproval", approvalMessage);

            await transaction.CommitAsync();
            return true;
        }
        catch (InvalidOperationException)
        {
            await transaction.RollbackAsync();
            throw;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            Console.WriteLine($"Error processing transaction: {ex.Message}");
            throw new InvalidOperationException($"Failed to process transaction: {ex.Message}");
        }
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

    public async Task<List<Chat.Message>> GetConversation(string user1Username, string user2Username)
    {
        return await _databaseService.ExecuteWithConnectionAsync(async connection =>
        {
            var query = @"
                WITH MessageData AS (
                    SELECT 
                        m.id, 
                        m.sender_id,
                        sender.username as sender_username,
                        m.receiver_id,
                        receiver.username as receiver_username,
                        COALESCE(m.text_content, '') as text_content,
                        m.type,
                        m.created_at,
                        m.is_read,
                        ARRAY_AGG(DISTINCT mi.image_path) FILTER (WHERE mi.image_path IS NOT NULL) as image_paths,
                        m.id as approval_message_id,
                        CASE 
                            WHEN m.type = 'TransactionApproval' THEN m.text_content
                            ELSE COALESCE(tm.transaction_number, '')
                        END as transaction_number,
                        CASE 
                            WHEN m.type = 'TransactionApproval' THEN 
                                (SELECT tm2.transaction_hash 
                                 FROM api_schema.transaction_message tm2 
                                 WHERE tm2.transaction_number = m.text_content)
                            ELSE COALESCE(tm.transaction_hash, '')
                        END as transaction_hash
                    FROM api_schema.message m
                    JOIN api_schema.user sender ON m.sender_id = sender.id
                    JOIN api_schema.user receiver ON m.receiver_id = receiver.id
                    LEFT JOIN api_schema.message_image mi ON m.id = mi.message_id
                    LEFT JOIN api_schema.transaction_message tm ON m.id = tm.message_id
                    WHERE (sender.username = @User1Username AND receiver.username = @User2Username)
                       OR (sender.username = @User2Username AND receiver.username = @User1Username)
                    GROUP BY m.id, m.sender_id, sender.username, m.receiver_id, receiver.username, 
                             m.text_content, m.type, m.created_at, m.is_read, tm.transaction_number, tm.transaction_hash
                )
                SELECT * FROM MessageData
                ORDER BY created_at ASC";

            await using var cmd = new NpgsqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@User1Username", user1Username);
            cmd.Parameters.AddWithValue("@User2Username", user2Username);

            var messages = new List<Chat.Message>();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                messages.Add(new Chat.Message
                {
                    Id = reader.GetInt32(0),
                    SenderId = reader.GetInt32(1),
                    SenderUsername = reader.GetString(2),
                    ReceiverId = reader.GetInt32(3),
                    ReceiverUsername = reader.GetString(4),
                    TextContent = reader.GetString(5),
                    Type = Enum.Parse<Chat.MessageType>(reader.GetString(6)),
                    CreatedAt = reader.GetDateTime(7),
                    IsRead = reader.GetBoolean(8),
                    ImagePaths = !reader.IsDBNull(9) 
                        ? ((string[])reader.GetValue(9)).Where(p => p != null).ToList() 
                        : new List<string>(),
                    ApprovedByMessageId = reader.IsDBNull(10) ? null : reader.GetInt32(10),
                    TransactionNumber = reader.GetString(11),
                    TransactionHash = reader.GetString(12)
                });
            }

            return messages;
        });
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
}