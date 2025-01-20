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
            VALUES (
                (SELECT id FROM api_schema.user WHERE username = @BuyerUsername),
                (SELECT id FROM api_schema.user WHERE username = @SellerUsername),
                @TransactionId,
                @PriceAtTime,
                @AccessDate
            )";

        public const string CancelSubscription = @"
            DELETE FROM api_schema.subscription
            WHERE buyer_id = (SELECT id FROM api_schema.user WHERE username = @BuyerId)
            AND seller_id = (SELECT id FROM api_schema.user WHERE username = @SellerId)";

        public const string CheckSubscription = @"
            SELECT COUNT(*) 
            FROM api_schema.private_access pa
            JOIN api_schema.user buyer ON pa.buyer_id = buyer.id
            JOIN api_schema.user seller ON pa.seller_id = seller.id
            WHERE buyer.username = @BuyerUsername 
            AND seller.username = @SellerUsername
            AND pa.access_date > CURRENT_DATE";

        public const string GetSubscriptions = @"
            SELECT DISTINCT u.* 
            FROM api_schema.user u
            JOIN api_schema.private_access pa ON u.id = pa.seller_id
            JOIN api_schema.user buyer ON pa.buyer_id = buyer.id
            WHERE buyer.username = @BuyerUsername
            AND pa.access_date > CURRENT_DATE";

        public const string GetSubscribers = @"
            SELECT DISTINCT u.* FROM api_schema.user u
            JOIN api_schema.private_access pa ON u.id = pa.buyer_id
            WHERE pa.seller_id = (SELECT id FROM api_schema.user WHERE username = @SellerId)
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
    }

    private readonly IDatabaseService _databaseService;
    private readonly IUserService _userService;

    public SubscriptionService(IDatabaseService databaseService, IUserService userService)
    {
        _databaseService = databaseService;
        _userService = userService;
    }

    public async Task<bool> BuyAccess(string buyerId, string sellerId)
    {
        using var connection = _databaseService.GetConnection();
        using var transaction = await connection.BeginTransactionAsync();

        try
        {
            var accessFee = await GetAccessFee(sellerId);
            
            // Check buyer's balance
            var buyerBalanceParams = new Dictionary<string, object> { { "@Username", buyerId } };
            using var balanceReader = await _databaseService.ExecuteQueryAsync(SqlQueries.GetWalletBalance, buyerBalanceParams);
            if (!balanceReader.Read())
                throw new Exception("Buyer wallet not found");

            var buyerBalance = balanceReader.GetInt32(0);
            if (buyerBalance < accessFee)
                throw new Exception("Insufficient funds");

            // Process wallet transactions
            await ProcessWalletTransactions(buyerId, sellerId, accessFee);

            // Create buyer's transaction record and get its ID
            var buyerTransactionId = await CreateTransactionRecord(buyerId, -accessFee);

            // Create subscription with transaction reference
            var subscriptionParams = new Dictionary<string, object>
            {
                { "@BuyerId", buyerId },
                { "@SellerId", sellerId },
                { "@TransactionId", buyerTransactionId },
                { "@PriceAtTime", accessFee },
                { "@AccessDate", DateTime.UtcNow.AddDays(30) } // Set access for 30 days
            };

            await _databaseService.ExecuteNonQueryAsync(SqlQueries.BuyAccess, subscriptionParams);

            await transaction.CommitAsync();
            return true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            Console.WriteLine($"Error in subscription transaction: {ex.Message}");
            return false;
        }
    }

    private async Task ProcessWalletTransactions(string buyerUsername, string sellerUsername, float amount)
    {
        // Deduct from buyer's wallet
        var deductParams = new Dictionary<string, object>
        {
            { "@Username", buyerUsername },
            { "@Amount", -amount }
        };
        await _databaseService.ExecuteNonQueryAsync(SqlQueries.UpdateWalletBalance, deductParams);

        // Add to seller's wallet
        var addParams = new Dictionary<string, object>
        {
            { "@Username", sellerUsername },
            { "@Amount", amount }
        };
        await _databaseService.ExecuteNonQueryAsync(SqlQueries.UpdateWalletBalance, addParams);
    }

    private async Task<int> CreateTransactionRecord(string username, float amount)
    {
        var transactionParams = new Dictionary<string, object>
        {
            { "@Amount", amount },
            { "@TransactionDate", DateTime.UtcNow },
            { "@Username", username }
        };

        using var reader = await _databaseService.ExecuteQueryAsync(SqlQueries.CreateInnerTransaction, transactionParams);
        if (reader.Read())
        {
            return reader.GetInt32(0);
        }
        throw new Exception("Failed to create transaction record");
    }

    public async Task<bool> Cancel(string buyerId, string sellerId)
    {
        try
        {
            var parameters = new Dictionary<string, object>
            {
                { "@BuyerId", buyerId },
                { "@SellerId", sellerId }
            };

            await _databaseService.ExecuteNonQueryAsync(SqlQueries.CancelSubscription, parameters);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error canceling subscription: {ex.Message}");
            return false;
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

    public async Task<List<User>> GetAllSubscriptions(string buyerId)
    {
        var subscriptions = new List<User>();
        try
        {
            var parameters = new Dictionary<string, object>
            {
                { "@BuyerId", buyerId }
            };

            using var reader = await _databaseService.ExecuteQueryAsync(SqlQueries.GetSubscriptions, parameters);
            while (reader.Read())
            {
                subscriptions.Add(new User(
                    reader.GetString(reader.GetOrdinal("username")),
                    reader.GetString(reader.GetOrdinal("email")),
                    reader.GetString(reader.GetOrdinal("user_password")),
                    reader.GetString(reader.GetOrdinal("first_name")),
                    reader.GetString(reader.GetOrdinal("last_name")),
                    reader.GetString(reader.GetOrdinal("phone_number")),
                    reader.GetDecimal(reader.GetOrdinal("access_fee")),
                    reader.GetString(reader.GetOrdinal("account_status")),
                    reader.GetString(reader.GetOrdinal("account_level")),
                    reader.GetStringOrDefault(reader.GetOrdinal("profile_description")),
                    reader.GetStringOrDefault(reader.GetOrdinal("profile_picture"))
                ));
            }
            return subscriptions;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting subscriptions: {ex.Message}");
            return new List<User>();
        }
    }

    public async Task<List<User>> GetAllSubscribers(string sellerId)
    {
        var subscribers = new List<User>();
        try
        {
            var parameters = new Dictionary<string, object>
            {
                { "@SellerId", sellerId }
            };

            using var reader = await _databaseService.ExecuteQueryAsync(SqlQueries.GetSubscribers, parameters);
            while (reader.Read())
            {
                subscribers.Add(new User(
                    reader.GetString(reader.GetOrdinal("username")),
                    reader.GetString(reader.GetOrdinal("email")),
                    reader.GetString(reader.GetOrdinal("user_password")),
                    reader.GetString(reader.GetOrdinal("first_name")),
                    reader.GetString(reader.GetOrdinal("last_name")),
                    reader.GetString(reader.GetOrdinal("phone_number")),
                    reader.GetDecimal(reader.GetOrdinal("access_fee")),
                    reader.GetString(reader.GetOrdinal("account_status")),
                    reader.GetString(reader.GetOrdinal("account_level")),
                    reader.GetStringOrDefault(reader.GetOrdinal("profile_description")),
                    reader.GetStringOrDefault(reader.GetOrdinal("profile_picture"))
                ));
            }
            return subscribers;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting subscribers: {ex.Message}");
            return new List<User>();
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
