using Back.Models;

namespace Back.Services.Interfaces
{
    public interface ISubscriptionService
    {
        Task<bool> BuyAccess(string buyerId, string sellerId);
        Task<bool> Cancel(string buyerId, string sellerId);
        Task<bool> IsSubscribed(string buyerId, string sellerId);
        Task<List<User>> GetAllSubscriptions(string buyerId);
        Task<List<User>> GetAllSubscribers(string sellerId);
        Task<float> GetAccessFee(string sellerId);
        Task<bool> EnableAutoRenewal(string buyerId, string sellerId);
        Task<bool> DisableAutoRenewal(string buyerId, string sellerId);
        Task<List<ExpiringSubscription>> GetExpiringSubscriptions();
    }

    public class ExpiringSubscription
    {
        public int Id { get; set; }
        public string BuyerUsername { get; set; }
        public string SellerUsername { get; set; }
        public decimal WalletBalance { get; set; }
        public decimal PriceAtTime { get; set; }
    }
}