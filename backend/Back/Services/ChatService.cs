using Back.Models;
using Back.Services.Interfaces;
using Npgsql;
using Microsoft.AspNetCore.SignalR;

namespace Back.Services;

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

<<<<<<< Updated upstream
                foreach (var path in uploadedImagePaths)
=======
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

            return message; // Just return the message, don't send it
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error sending message: {ex.Message}");
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
>>>>>>> Stashed changes
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

<<<<<<< Updated upstream
            // Notify through SignalR
            await _hubContext.Clients.User(message.ReceiverUsername)
                .SendAsync("ReceiveMessage", newMessage);

            return newMessage;
=======
            return message; // Just return the message, don't send it
>>>>>>> Stashed changes
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

<<<<<<< Updated upstream
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
=======
            var signalRMessageId = GenerateMessageId();
            await _hubContext.Clients.User(senderUsername)
                .ReceiveTransactionApproval(approvalMessage, signalRMessageId);
>>>>>>> Stashed changes

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

<<<<<<< Updated upstream
        newRequest.Id = (int)await command.ExecuteScalarAsync();
=======
        try
        {
            var (senderId, receiverId) = await GetUserIds(senderUsername, receiverUsername);
            if (senderId == 0 || receiverId == 0)
            {
                throw new InvalidOperationException("Invalid user IDs");
            }
>>>>>>> Stashed changes

        // Notify receiver via SignalR
        await _hubContext.Clients.User(request.ReceiverUsername)
            .SendAsync("ReceivePaymentRequest", request);

<<<<<<< Updated upstream
        return newRequest;
=======
            // Delete any existing end request messages
            var deleteOldRequestsQuery = @"
                DELETE FROM api_schema.message 
                WHERE sender_id = @SenderId 
                AND receiver_id = @ReceiverId
                AND type = 'EndRequest'::api_schema.message_type";

            using var deleteCmd = new NpgsqlCommand(deleteOldRequestsQuery, connection, transaction);
            deleteCmd.Parameters.AddWithValue("@SenderId", senderId);
            deleteCmd.Parameters.AddWithValue("@ReceiverId", receiverId);
            await deleteCmd.ExecuteNonQueryAsync();

            // Generate hash and create message
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

            // Validate message creation
            if (messageId == 0)
            {
                throw new InvalidOperationException("Failed to create message");
            }

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

            // Log the created message
            _logger.LogInformation($"Created end request message: {messageId} from {senderUsername} to {receiverUsername}");

            return message;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error creating end request message: {ex.Message}");
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
                Type = Chat.MessageType.EndRequestApproval,
                ReceiverUsername = senderUsername // Add this line
            };

            var signalRMessageId = GenerateMessageId();
            await _hubContext.Clients.User(senderUsername)
                .ReceiveEndRequestApproval(approvalMessage, signalRMessageId);

            await _hubContext.Clients.Users(new[] { senderUsername, receiverUsername })
                .ChatStatusChanged(new { ChatId = chatId, Status = "disabled" });

            return approvalMessage;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
>>>>>>> Stashed changes
    }

    private string GenerateMessageId() => Guid.NewGuid().ToString();
}