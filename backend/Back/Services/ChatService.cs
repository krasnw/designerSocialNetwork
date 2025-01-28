﻿using Back.Models;
using Back.Services.Interfaces;
using Npgsql;
using Microsoft.AspNetCore.SignalR;

namespace Back.Services;

public enum ChatRequestResult
{
    Success,
    ReceiverNotFound,
    SenderNotFound,
    Failed
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
        var getUserIdQuery = @"
    SELECT id FROM api_schema.user WHERE username = @Username";

        NpgsqlConnection connection = null;
        NpgsqlCommand command = null;

        try
        {
            connection = _databaseService.GetConnection();

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

    public async Task<List<string>> GetChatUsers(string username)
    {
        var query = @"
        SELECT DISTINCT other_user.username
        FROM api_schema.request r
        JOIN api_schema.""user"" current_u 
            ON (current_u.id = r.buyer_id OR current_u.id = r.seller_id)
        JOIN api_schema.""user"" other_user 
            ON (other_user.id = r.buyer_id OR other_user.id = r.seller_id)
            AND other_user.id != current_u.id
        WHERE current_u.username = @Username
        AND r.request_status = 'accepted'
        ORDER BY other_user.username";

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

    public async Task<Chat.TransactionMessageResponse> SendTransactionMessage(string senderUsername, Chat.TransactionMessage message)
    {
        using var connection = _databaseService.GetConnection();
        using var transaction = connection.BeginTransaction();

        try
        {
            var (senderId, receiverId) = await GetUserIds(senderUsername, message.ReceiverUsername);

            // Insert message - ensure exact enum case match
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

            // Insert transaction details
            var insertTransactionQuery = @"
                INSERT INTO api_schema.transaction_message (message_id, amount, is_approved)
                VALUES (@MessageId, @Amount, false)";

            using var transCmd = new NpgsqlCommand(insertTransactionQuery, connection, transaction);
            transCmd.Parameters.AddWithValue("@MessageId", messageId);
            transCmd.Parameters.AddWithValue("@Amount", message.Amount);
            await transCmd.ExecuteNonQueryAsync();

            await transaction.CommitAsync();

            var response = new Chat.TransactionMessageResponse
            {
                MessageId = messageId,
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

    public async Task<bool> ApproveTransaction(int messageId, string approverUsername)
    {
        using var connection = _databaseService.GetConnection();
        using var transaction = connection.BeginTransaction();

        try
        {
            // Verify transaction exists and belongs to approver
            var verifyQuery = @"
                SELECT 
                    m.sender_id,
                    sender.username as sender_username,
                    tm.amount,
                    w.id as wallet_id,
                    w.amount as wallet_balance
                FROM api_schema.message m
                JOIN api_schema.transaction_message tm ON m.id = tm.message_id
                JOIN api_schema.user sender ON m.sender_id = sender.id
                JOIN api_schema.user receiver ON m.receiver_id = receiver.id
                JOIN api_schema.wallet w ON receiver.id = w.user_id
                WHERE m.id = @MessageId 
                AND receiver.username = @ApproverUsername 
                AND tm.is_approved = false";

            using var cmd = new NpgsqlCommand(verifyQuery, connection, transaction);
            cmd.Parameters.AddWithValue("@MessageId", messageId);
            cmd.Parameters.AddWithValue("@ApproverUsername", approverUsername);

            using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
            {
                return false;
            }

            var senderId = reader.GetInt32(0);
            var senderUsername = reader.GetString(1);
            var amount = reader.GetDecimal(2);
            var approverWalletId = reader.GetInt32(3);
            var approverBalance = reader.GetDecimal(4);
            reader.Close();

            // Check if approver has enough balance
            if (approverBalance < amount)
            {
                throw new InvalidOperationException("Insufficient funds to complete the transaction");
            }

            // Update transaction status
            var updateQuery = @"
                UPDATE api_schema.transaction_message 
                SET is_approved = true 
                WHERE message_id = @MessageId";

            using var updateCmd = new NpgsqlCommand(updateQuery, connection, transaction);
            updateCmd.Parameters.AddWithValue("@MessageId", messageId);
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

            await transaction.CommitAsync();

            // Notify sender through SignalR
            await _hubContext.Clients.User(senderUsername)
                .SendAsync("TransactionApproved", new { 
                    MessageId = messageId,
                    Amount = amount,
                    NewBalance = approverBalance - amount
                });

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
            throw new Exception("Failed to process transaction");
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
        var query = @"
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
                ARRAY_AGG(mi.image_path) as image_paths
            FROM api_schema.message m
            JOIN api_schema.user sender ON m.sender_id = sender.id
            JOIN api_schema.user receiver ON m.receiver_id = receiver.id
            LEFT JOIN api_schema.message_image mi ON m.id = mi.message_id
            WHERE (sender.username = @User1Username AND receiver.username = @User2Username)
               OR (sender.username = @User2Username AND receiver.username = @User1Username)
            GROUP BY m.id, m.sender_id, sender.username, m.receiver_id, receiver.username, m.text_content, m.type, m.created_at, m.is_read
            ORDER BY m.created_at";

        var messages = new List<Chat.Message>();
        using var connection = _databaseService.GetConnection();
        using var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue("@User1Username", user1Username);
        command.Parameters.AddWithValue("@User2Username", user2Username);

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var message = new Chat.Message
            {
                Id = reader.GetInt32(0),
                SenderId = reader.GetInt32(1),
                SenderUsername = reader.GetString(2),
                ReceiverId = reader.GetInt32(3),
                ReceiverUsername = reader.GetString(4),
                TextContent = reader.IsDBNull(5) ? null : reader.GetString(5),
                Type = Enum.Parse<Chat.MessageType>(reader.GetString(6)),
                CreatedAt = reader.GetDateTime(7),
                IsRead = reader.GetBoolean(8)
            };

            // Handle image paths array
            if (!reader.IsDBNull(9))
            {
                var imagePaths = (string[])reader.GetValue(9);
                message.ImagePaths = imagePaths.Where(p => p != null).ToList();
            }

            messages.Add(message);
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