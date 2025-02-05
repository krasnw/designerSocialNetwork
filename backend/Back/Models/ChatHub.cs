using Microsoft.AspNetCore.SignalR;
using Back.Models;
using Back.Services.Interfaces;
using System.Collections.Concurrent;

namespace Back.Models
{
    public class ChatHub : Hub
    {
        private readonly IChatService _chatService;
<<<<<<< Updated upstream
=======
        private readonly ILogger<ChatHub> _logger;
        private static readonly ConcurrentDictionary<string, HashSet<string>> _processedMessageIds = new();
        private static readonly ConcurrentDictionary<string, (DateTime timestamp, object message)> _pendingMessages = new();
        private const int MESSAGE_TIMEOUT_SECONDS = 30;
        private CancellationTokenSource _heartbeatCts;
        private static readonly ConcurrentDictionary<string, HashSet<int>> _processedDbMessageIds = new();
>>>>>>> Stashed changes

        public ChatHub(IChatService chatService)
        {
            _chatService = chatService;
<<<<<<< Updated upstream
=======
            _logger = logger;
            _heartbeatCts = new CancellationTokenSource();
>>>>>>> Stashed changes
        }

        public override async Task OnConnectedAsync()
        {
            var username = Context.User?.Identity?.Name;
            if (!string.IsNullOrEmpty(username))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, username);
<<<<<<< Updated upstream
=======
                _logger.LogInformation($"User {username} connected with connection ID {Context.ConnectionId}");
                _ = StartHeartbeatAsync(Context.ConnectionId, _heartbeatCts.Token);
            }
            else
            {
                _logger.LogWarning($"Connection attempted without username. ConnectionId: {Context.ConnectionId}");
>>>>>>> Stashed changes
            }
            await base.OnConnectedAsync();
        }

<<<<<<< Updated upstream
        public async Task SendMessage(Chat.MessageDto message)
=======
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            try
            {
                _heartbeatCts.Cancel();
                var username = Context.User?.Identity?.Name;
                if (!string.IsNullOrEmpty(username))
                {
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, username);
                    _logger.LogInformation($"User {username} disconnected");
                    _processedDbMessageIds.TryRemove(username, out _);
                    _processedMessageIds.TryRemove(username, out _);
                }
            }
            finally
            {
                _heartbeatCts.Dispose();
                _heartbeatCts = new CancellationTokenSource();
            }
            await base.OnDisconnectedAsync(exception);
        }

        private async Task HandleMessageDelivery<T>(T message, string receiverUsername, Func<T, string, Task> deliveryMethod) 
            where T : class
        {
            var messageId = Guid.NewGuid().ToString();
            try
            {
                // Log the message before sending
                _logger.LogInformation($"Preparing to deliver message type {typeof(T).Name} to {receiverUsername}");
                
                if (message == null)
                {
                    _logger.LogError("Message is null");
                    throw new ArgumentNullException(nameof(message));
                }

                _pendingMessages.TryAdd(messageId, (DateTime.UtcNow, message));
                await deliveryMethod(message, messageId);
                _ = CheckMessageDelivery(messageId, receiverUsername, message, deliveryMethod);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending {typeof(T).Name}: {ex.Message}");
                throw;
            }
        }

        private async Task CheckMessageDelivery<T>(string messageId, string receiverUsername, T message, Func<T, string, Task> deliveryMethod) 
            where T : class
        {
            await Task.Delay(TimeSpan.FromSeconds(5));

            for (int i = 0; i < 3; i++)
            {
                if (!_pendingMessages.ContainsKey(messageId))
                    return;

                if (DateTime.UtcNow - _pendingMessages[messageId].timestamp > TimeSpan.FromSeconds(MESSAGE_TIMEOUT_SECONDS))
                {
                    _pendingMessages.TryRemove(messageId, out _);
                    return;
                }

                try
                {
                    var userProcessedMessages = _processedDbMessageIds.GetOrAdd(receiverUsername, _ => new HashSet<int>());
                    var messageDbId = GetMessageId(message);
                    
                    if (!userProcessedMessages.Contains(messageDbId))
                    {
                        await deliveryMethod(message, messageId);
                        userProcessedMessages.Add(messageDbId);
                        await Task.Delay(TimeSpan.FromSeconds(5 * (i + 1)));
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Retry {i + 1} failed for message {messageId}: {ex.Message}");
                }
            }
        }

        private int GetMessageId(object message)
        {
            return message switch
            {
                Chat.MessageComplex m => m.Id,
                Chat.MessageTransaction m => m.Id,
                Chat.MessageTransactionApproval m => m.Id,
                Chat.MessageEndRequest m => m.Id,
                Chat.MessageEndRequestApproval m => m.Id,
                _ => throw new ArgumentException($"Unknown message type: {message.GetType()}")
            };
        }

        public async Task SendMessage(Chat.MessageRequest message)
>>>>>>> Stashed changes
        {
            var senderUsername = Context.User?.Identity?.Name;
            if (string.IsNullOrEmpty(senderUsername))
                throw new HubException("Unauthorized");

<<<<<<< Updated upstream
            var result = await _chatService.SendMessage(senderUsername, message);
            await Clients.User(message.ReceiverUsername)
                .SendAsync("ReceiveMessage", result);
=======
            var result = await _chatService.SendComplexMessage(senderUsername, message);
            await HandleMessageDelivery(result, message.ReceiverUsername, 
                (msg, id) => Clients.User(message.ReceiverUsername).ReceiveMessage(msg, id));
        }

        public async Task AcknowledgeMessage(string messageId)
        {
            var username = Context.User?.Identity?.Name;
            if (string.IsNullOrEmpty(username))
                throw new HubException("Unauthorized");

            _processedMessageIds.AddOrUpdate(
                username,
                new HashSet<string> { messageId },
                (key, oldSet) => { oldSet.Add(messageId); return oldSet; }
            );

            if (_pendingMessages.TryRemove(messageId, out _))
            {
                await Clients.Caller.MessageStatus(new { messageId, status = "delivered" });
            }
>>>>>>> Stashed changes
        }
        
        public async Task SendPaymentRequest(Chat.PaymentRequestDto request)
        {
            var senderUsername = Context.User?.Identity?.Name;
            if (string.IsNullOrEmpty(senderUsername))
                throw new HubException("Unauthorized");

<<<<<<< Updated upstream
            var result = await _chatService.CreatePaymentRequest(request);
            await Clients.User(request.ReceiverUsername)
                .SendAsync("ReceivePaymentRequest", result);
=======
            var result = await _chatService.SendTransactionMessage(senderUsername, request);
            await HandleMessageDelivery(result, request.ReceiverUsername, 
                (msg, id) => Clients.User(request.ReceiverUsername).ReceiveTransactionMessage(msg, id));
>>>>>>> Stashed changes
        }

        public async Task SendTransactionMessage(Chat.TransactionMessage message)
        {
            var senderUsername = Context.User?.Identity?.Name;
            if (string.IsNullOrEmpty(senderUsername))
                throw new HubException("Unauthorized");

<<<<<<< Updated upstream
            var result = await _chatService.SendTransactionMessage(senderUsername, message);
            await Clients.User(message.ReceiverUsername)
                .SendAsync("ReceiveTransactionMessage", result);
=======
            await HandleMessageDelivery(approval, approval.ReceiverUsername, 
                (msg, id) => Clients.User(approval.ReceiverUsername).ReceiveTransactionApproval(msg, id));
        }

        public async Task SendEndRequest(Chat.MessageEndRequest request)
        {
            var senderUsername = Context.User?.Identity?.Name;
            if (string.IsNullOrEmpty(senderUsername))
                throw new HubException("Unauthorized");

            try
            {
                // First process the request and get the complete message
                var result = await _chatService.SendEndRequestMessage(senderUsername, request.ReceiverUsername);
                
                // Validate the result
                if (result == null || string.IsNullOrEmpty(result.EndRequestHash))
                {
                    throw new HubException("Failed to create end request message");
                }

                // Then handle delivery
                await HandleMessageDelivery(result, request.ReceiverUsername, 
                    async (msg, id) => 
                    {
                        _logger.LogInformation($"Sending end request message: {msg.Id} to {msg.ReceiverUsername}");
                        await Clients.User(request.ReceiverUsername).ReceiveEndRequestMessage(msg, id);
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing end request: {ex.Message}");
                throw;
            }
        }

        public async Task SendEndRequestApproval(Chat.MessageEndRequestApproval approval, string[] usernames)
        {
            var senderUsername = Context.User?.Identity?.Name;
            if (string.IsNullOrEmpty(senderUsername))
                throw new HubException("Unauthorized");

            foreach (var username in usernames)
            {
                await HandleMessageDelivery(approval, username, 
                    (msg, id) => Clients.User(username).ReceiveEndRequestApproval(msg, id));
            }
        }

        public async Task NotifyChatStatusChanged(string[] usernames, object statusUpdate)
        {
            var senderUsername = Context.User?.Identity?.Name;
            if (string.IsNullOrEmpty(senderUsername))
                throw new HubException("Unauthorized");

            await Clients.Users(usernames).ChatStatusChanged(statusUpdate);
        }

        public async Task SendMessageStatus(string username, object status)
        {
            await Clients.User(username).MessageStatus(status);
        }

        public async Task TestConnection()
        {
            try
            {
                await Clients.Caller.TestResponse("Test message from server");
                _logger.LogInformation("Test message sent successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in TestConnection: {ex.Message}");
                throw;
            }
>>>>>>> Stashed changes
        }

        private async Task StartHeartbeatAsync(string connectionId, CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        if (Context == null || Context.ConnectionId != connectionId)
                        {
                            _logger.LogWarning($"Connection context changed or lost for {connectionId}");
                            break;
                        }

                        await Clients.Caller.Heartbeat();
                        _logger.LogDebug($"Heartbeat sent to {connectionId}");
                        await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);
                    }
                    catch (OperationCanceledException)
                    {
                        // Normal cancellation, just exit
                        break;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error in heartbeat for {connectionId}: {ex.Message}");
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Heartbeat task failed for {connectionId}: {ex.Message}");
            }
            finally
            {
                _logger.LogInformation($"Heartbeat stopped for {connectionId}");
            }
        }

        private async Task CleanupProcessedMessages()
        {
            while (!_heartbeatCts.Token.IsCancellationRequested)
            {
                try
                {
                    // Clean up every hour
                    await Task.Delay(TimeSpan.FromHours(1), _heartbeatCts.Token);

                    foreach (var userMessages in _processedDbMessageIds)
                    {
                        if (userMessages.Value.Count > 1000) // Arbitrary limit
                        {
                            _processedDbMessageIds.TryRemove(userMessages.Key, out _);
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }
    }
}