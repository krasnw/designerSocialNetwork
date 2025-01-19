using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Back.Services.Interfaces;

namespace Back.Services;

public class SubscriptionRenewalService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(1); // Check every hour

    public SubscriptionRenewalService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var subscriptionService = scope.ServiceProvider.GetRequiredService<ISubscriptionService>();
                await ProcessRenewals(subscriptionService);
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }
    }

    private async Task ProcessRenewals(ISubscriptionService subscriptionService)
    {
        try
        {
            var expiringSubscriptions = await subscriptionService.GetExpiringSubscriptions();
            
            foreach (var subscription in expiringSubscriptions)
            {
                if (subscription.WalletBalance >= subscription.PriceAtTime)
                {
                    await subscriptionService.BuyAccess(
                        subscription.BuyerUsername,
                        subscription.SellerUsername
                    );
                }
                else
                {
                    await subscriptionService.DisableAutoRenewal(
                        subscription.BuyerUsername,
                        subscription.SellerUsername
                    );
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing subscription renewals: {ex.Message}");
        }
    }
}
