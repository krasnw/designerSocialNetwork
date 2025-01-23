using Back.Models;
using Back.Services.Interfaces;
using Npgsql;

namespace Back.Services;

public class SubscriptionService : ISubscriptionService
{
    private static class SqlQueries
    {
        public const string BuyAccess = @"
            INSERT INTO api_schema.private_access (
                buyer_id, 
                seller_id, 
                transaction_id, 
                price_at_time, 
                access_date
            )
            SELECT 
                buyer.id,
                seller.id,
                @TransactionId,
                @PriceAtTime,
                @AccessDate
            FROM api_schema.user buyer
            CROSS JOIN api_schema.user seller
            WHERE buyer.username = @BuyerUsername
            AND seller.username = @SellerUsername";

        // Fix table name in CancelSubscription query
        public const string CancelSubscription = @"
            DELETE FROM api_schema.private_access
            WHERE buyer_id = (SELECT id FROM api_schema.user WHERE username = @BuyerId)
            AND seller_id = (SELECT id FROM api_schema.user WHERE username = @SellerId)";

        // Fix parameter names in CheckSubscription query
        public const string CheckSubscription = @"
            SELECT COUNT(*) 
            FROM api_schema.private_access pa
            JOIN api_schema.user buyer ON pa.buyer_id = buyer.id
            JOIN api_schema.user seller ON pa.seller_id = seller.id
            WHERE buyer.username = @BuyerId 
            AND seller.username = @SellerId
            AND pa.access_date > CURRENT_DATE";

        // Update GetSubscriptions query to select only needed fields
        public const string GetSubscriptions = @"
            SELECT DISTINCT 
                u.username,
                u.first_name,
                u.last_name,
                COALESCE(u.profile_picture, '') as profile_picture
            FROM api_schema.user u
            JOIN api_schema.private_access pa ON u.id = pa.seller_id
            JOIN api_schema.user buyer ON pa.buyer_id = buyer.id
            WHERE buyer.username = @BuyerId
            AND pa.access_date > CURRENT_DATE";

        // Update GetSubscribers query to match GetSubscriptions format
        public const string GetSubscribers = @"
            SELECT DISTINCT 
                u.username,
                u.first_name,
                u.last_name,
                COALESCE(u.profile_picture, '') as profile_picture
            FROM api_schema.user u
            JOIN api_schema.private_access pa ON u.id = pa.buyer_id
            JOIN api_schema.user seller ON pa.seller_id = seller.id
            WHERE seller.username = @SellerId
            AND pa.access_date > CURRENT_DATE";

        public const string GetAccessFee = @"
            SELECT access_fee FROM api_schema.user
            WHERE username = @SellerId";

        public const string GetWalletBalance = @"
            SELECT w.amount 
            FROM api_schema.wallet w
            JOIN api_schema.user u ON w.user_id = u.id
            WHERE u.username = @Username";

        public const string UpdateWalletBalance = @"
            UPDATE api_schema.wallet w
            SET amount = amount + @Amount
            FROM api_schema.user u
            WHERE w.user_id = u.id AND u.username = @Username";

        public const string CreateInnerTransaction = @"
            INSERT INTO api_schema.inner_transaction (
                amount, 
                transaction_date, 
                user_id, 
                wallet_id
            )
            SELECT 
                @Amount,
                @TransactionDate,
                u.id,
                w.id
            FROM api_schema.user u
            JOIN api_schema.wallet w ON w.user_id = u.id
            WHERE u.username = @Username
            RETURNING id";

        public const string GetExpiringSubscriptions = @"
            SELECT 
                pa.id, pa.buyer_id, pa.seller_id, pa.price_at_time,
                u_buyer.username as buyer_username,
                u_seller.username as seller_username,
                w.amount as wallet_balance
            FROM api_schema.private_access pa
            JOIN api_schema.user u_buyer ON pa.buyer_id = u_buyer.id
            JOIN api_schema.user u_seller ON pa.seller_id = u_seller.id
            JOIN api_schema.wallet w ON u_buyer.id = w.user_id
            WHERE pa.access_date <= CURRENT_DATE + INTERVAL '1 day'
            AND pa.auto_renewal = true
            AND NOT EXISTS (
                SELECT 1 FROM api_schema.private_access newer
                WHERE newer.buyer_id = pa.buyer_id
                AND newer.seller_id = pa.seller_id
                AND newer.access_date > pa.access_date
            )";

        public const string EnableAutoRenewal = @"
            UPDATE api_schema.private_access
            SET auto_renewal = true
            WHERE buyer_id = (SELECT id FROM api_schema.user WHERE username = @BuyerId)
            AND seller_id = (SELECT id FROM api_schema.user WHERE username = @SellerId)
            AND access_date > CURRENT_DATE";

        public const string DisableAutoRenewal = @"
            UPDATE api_schema.private_access
            SET auto_renewal = false
            WHERE buyer_id = (SELECT id FROM api_schema.user WHERE username = @BuyerId)
            AND seller_id = (SELECT id FROM api_schema.user WHERE username = @SellerId)
            AND access_date > CURRENT_DATE";

        // Add new query to check for existing active subscription
        public const string CheckExistingSubscription = @"
            SELECT COUNT(*) 
            FROM api_schema.private_access pa
            JOIN api_schema.user buyer ON pa.buyer_id = buyer.id
            JOIN api_schema.user seller ON pa.seller_id = seller.id
            WHERE buyer.username = @BuyerUsername 
            AND seller.username = @SellerUsername
            AND pa.access_date > CURRENT_DATE";

        // Add new query to check if subscription exists before canceling
        public const string CheckCancelableSubscription = @"
            SELECT COUNT(*) 
            FROM api_schema.private_access pa
            JOIN api_schema.user buyer ON pa.buyer_id = buyer.id
            JOIN api_schema.user seller ON pa.seller_id = seller.id
            WHERE buyer.username = @BuyerId 
            AND seller.username = @SellerId
            AND pa.access_date > CURRENT_DATE";
    }

    private readonly IDatabaseService _databaseService;
    private readonly IUserService _userService;

    public SubscriptionService(IDatabaseService databaseService, IUserService userService)
    {
        _databaseService = databaseService;
        _userService = userService;
    }

    // Fix BuyAccess method to properly handle the transaction
    public async Task<bool> BuyAccess(string buyerId, string sellerId)
    {
        using var connection = _databaseService.GetConnection();
        using var transaction = await connection.BeginTransactionAsync();

        try
        {
            // Check if subscription already exists
            var checkParams = new Dictionary<string, object>
            {
                { "@BuyerUsername", buyerId },
                { "@SellerUsername", sellerId }
            };

            using (var checkReader = await _databaseService.ExecuteQueryAsync(
                SqlQueries.CheckExistingSubscription,
                checkParams,
                connection,
                transaction))
            {
                if (checkReader.Read() && checkReader.GetInt32(0) > 0)
                {
                    throw new Exception("You already have an active subscription to this user");
                }
            }

            var accessFee = await GetAccessFee(sellerId, connection, transaction);
            if (accessFee <= 0)
                throw new Exception("Invalid access fee");
            
            // Check buyer's balance
            var buyerBalanceParams = new Dictionary<string, object> { { "@Username", buyerId } };
            int buyerBalance;
            using (var balanceReader = await _databaseService.ExecuteQueryAsync(
                SqlQueries.GetWalletBalance, 
                buyerBalanceParams, 
                connection, 
                transaction))
            {
                if (!balanceReader.Read())
                    throw new Exception("Buyer wallet not found");
                buyerBalance = balanceReader.GetInt32(0);
            } // Reader is disposed here

            if (buyerBalance < accessFee)
                throw new Exception("Insufficient funds");

            // Process wallet transactions
            await ProcessWalletTransactions(buyerId, sellerId, accessFee, connection, transaction);

            // Create buyer's transaction record and get its ID
            var buyerTransactionId = await CreateTransactionRecord(buyerId, -accessFee, connection, transaction);

            // Create subscription with transaction reference
            var subscriptionParams = new Dictionary<string, object>
            {
                { "@BuyerUsername", buyerId },
                { "@SellerUsername", sellerId },
                { "@TransactionId", buyerTransactionId },
                { "@PriceAtTime", accessFee },
                { "@AccessDate", DateTime.UtcNow.AddDays(30) } // Set access for 30 days
            };

            await _databaseService.ExecuteNonQueryAsync(SqlQueries.BuyAccess, subscriptionParams, connection, transaction);

            await transaction.CommitAsync();
            return true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            Console.WriteLine($"Error in subscription transaction: {ex.Message}");
            throw; // Rethrow to let controller handle the error
        }
    }

    // Update ProcessWalletTransactions to use the same transaction
    private async Task ProcessWalletTransactions(string buyerUsername, string sellerUsername, float amount, 
        NpgsqlConnection connection, NpgsqlTransaction transaction)
    {
        // Deduct from buyer's wallet
        var deductParams = new Dictionary<string, object>
        {
            { "@Username", buyerUsername },
            { "@Amount", -amount }
        };
        await _databaseService.ExecuteNonQueryAsync(SqlQueries.UpdateWalletBalance, deductParams, connection, transaction);

        // Add to seller's wallet
        var addParams = new Dictionary<string, object>
        {
            { "@Username", sellerUsername },
            { "@Amount", amount }
        };
        await _databaseService.ExecuteNonQueryAsync(SqlQueries.UpdateWalletBalance, addParams, connection, transaction);
    }

    // Update CreateTransactionRecord to use the same transaction
    private async Task<int> CreateTransactionRecord(string username, float amount, 
        NpgsqlConnection connection, NpgsqlTransaction transaction)
    {
        var transactionParams = new Dictionary<string, object>
        {
            { "@Amount", amount },
            { "@TransactionDate", DateTime.UtcNow },
            { "@Username", username }
        };

        using var reader = await _databaseService.ExecuteQueryAsync(
            SqlQueries.CreateInnerTransaction, 
            transactionParams, 
            connection, 
            transaction);

        if (reader.Read())
        {
            return reader.GetInt32(0);
        }
        throw new Exception("Failed to create transaction record");
    }

    // Update GetAccessFee to use provided connection and transaction when available
    private async Task<float> GetAccessFee(string sellerId, NpgsqlConnection? connection = null, NpgsqlTransaction? transaction = null)
    {
        try
        {
            var parameters = new Dictionary<string, object> { { "@SellerId", sellerId } };

            if (connection != null && transaction != null)
            {
                using var reader = await _databaseService.ExecuteQueryAsync(
                    SqlQueries.GetAccessFee, 
                    parameters, 
                    connection, 
                    transaction);
                return reader.Read() ? reader.GetFloat(0) : 0;
            }
            else
            {
                using var reader = await _databaseService.ExecuteQueryAsync(SqlQueries.GetAccessFee, parameters);
                return reader.Read() ? reader.GetFloat(0) : 0;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting access fee: {ex.Message}");
            return 0;
        }
    }

    public async Task<bool> Cancel(string buyerId, string sellerId)
    {
        using var connection = _databaseService.GetConnection();
        using var transaction = await connection.BeginTransactionAsync();

        try
        {
            // Check if subscription exists and is active
            var checkParams = new Dictionary<string, object>
            {
                { "@BuyerId", buyerId },
                { "@SellerId", sellerId }
            };

            using (var checkReader = await _databaseService.ExecuteQueryAsync(
                SqlQueries.CheckCancelableSubscription,
                checkParams,
                connection,
                transaction))
            {
                if (!checkReader.Read() || checkReader.GetInt32(0) == 0)
                {
                    throw new Exception("No active subscription found to cancel");
                }
            }

            // Proceed with cancellation
            await _databaseService.ExecuteNonQueryAsync(
                SqlQueries.CancelSubscription, 
                checkParams, 
                connection, 
                transaction);

            await transaction.CommitAsync();
            return true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            Console.WriteLine($"Error canceling subscription: {ex.Message}");
            throw; // Rethrow to let controller handle the error
        }
    }

    public async Task<bool> IsSubscribed(string buyerId, string sellerId)
    {
        try
        {
            var parameters = new Dictionary<string, object>
            {
                { "@BuyerId", buyerId },
                { "@SellerId", sellerId }
            };

            using var reader = await _databaseService.ExecuteQueryAsync(SqlQueries.CheckSubscription, parameters);
            if (reader.Read())
            {
                return reader.GetInt32(0) > 0;
            }
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error checking subscription: {ex.Message}");
            return false;
        }
    }

    // Create a small record for subscription response
    public record SubscriptionInfo(
        string Username,
        string FirstName,
        string LastName,
        string ProfileImage
    );

    // Update the GetAllSubscriptions method to return SubscriptionInfo
    public async Task<List<SubscriptionInfo>> GetAllSubscriptions(string buyerId)
    {
        var subscriptions = new List<SubscriptionInfo>();
        try
        {
            var parameters = new Dictionary<string, object>
            {
                { "@BuyerId", buyerId }
            };

            using var reader = await _databaseService.ExecuteQueryAsync(SqlQueries.GetSubscriptions, parameters);
            while (reader.Read())
            {
                subscriptions.Add(new SubscriptionInfo(
                    reader.GetString(reader.GetOrdinal("username")),
                    reader.GetString(reader.GetOrdinal("first_name")),
                    reader.GetString(reader.GetOrdinal("last_name")),
                    reader.GetString(reader.GetOrdinal("profile_picture"))
                ));
            }
            return subscriptions;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting subscriptions: {ex.Message}");
            throw;
        }
    }

    // Remove the unused GetValueOrDefault helper method since we're handling nulls directly
    
    // Update GetAllSubscribers to return SubscriptionInfo
    public async Task<List<SubscriptionInfo>> GetAllSubscribers(string sellerId)
    {
        var subscribers = new List<SubscriptionInfo>();
        try
        {
            var parameters = new Dictionary<string, object>
            {
                { "@SellerId", sellerId }
            };

            using var reader = await _databaseService.ExecuteQueryAsync(SqlQueries.GetSubscribers, parameters);
            while (reader.Read())
            {
                subscribers.Add(new SubscriptionInfo(
                    reader.GetString(reader.GetOrdinal("username")),
                    reader.GetString(reader.GetOrdinal("first_name")),
                    reader.GetString(reader.GetOrdinal("last_name")),
                    reader.GetString(reader.GetOrdinal("profile_picture"))
                ));
            }
            return subscribers;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting subscribers: {ex.Message}");
            return new List<SubscriptionInfo>();
        }
    }

    public async Task<float> GetAccessFee(string sellerId)
    {
        try
        {
            var parameters = new Dictionary<string, object>
            {
                { "@SellerId", sellerId }
            };

            using var reader = await _databaseService.ExecuteQueryAsync(SqlQueries.GetAccessFee, parameters);
            if (reader.Read())
            {
                return reader.GetFloat(0);
            }
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting access fee: {ex.Message}");
            return 0;
        }
    }

    public async Task<bool> EnableAutoRenewal(string buyerId, string sellerId)
    {
        try
        {
            var parameters = new Dictionary<string, object>
            {
                { "@BuyerId", buyerId },
                { "@SellerId", sellerId }
            };

            await _databaseService.ExecuteNonQueryAsync(SqlQueries.EnableAutoRenewal, parameters);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error enabling auto-renewal: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> DisableAutoRenewal(string buyerId, string sellerId)
    {
        try
        {
            var parameters = new Dictionary<string, object>
            {
                { "@BuyerId", buyerId },
                { "@SellerId", sellerId }
            };

            await _databaseService.ExecuteNonQueryAsync(SqlQueries.DisableAutoRenewal, parameters);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error disabling auto-renewal: {ex.Message}");
            return false;
        }
    }

    public async Task<List<ExpiringSubscription>> GetExpiringSubscriptions()
    {
        var subscriptions = new List<ExpiringSubscription>();
        try
        {
            using var reader = await _databaseService.ExecuteQueryAsync(SqlQueries.GetExpiringSubscriptions, new Dictionary<string, object>());
            while (reader.Read())
            {
                subscriptions.Add(new ExpiringSubscription
                {
                    Id = reader.GetInt32(reader.GetOrdinal("id")),
                    BuyerUsername = reader.GetString(reader.GetOrdinal("buyer_username")),
                    SellerUsername = reader.GetString(reader.GetOrdinal("seller_username")),
                    WalletBalance = reader.GetDecimal(reader.GetOrdinal("wallet_balance")),
                    PriceAtTime = reader.GetDecimal(reader.GetOrdinal("price_at_time"))
                });
            }
            return subscriptions;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting expiring subscriptions: {ex.Message}");
            return new List<ExpiringSubscription>();
        }
    }
}
