using CryptoPulse.Api.Services;
using Microsoft.AspNetCore.SignalR;

namespace CryptoPulse.Api.Services;

public class PriceBroadcastService : BackgroundService
{
    private readonly IHubContext<CryptoPulse.Api.Hubs.PriceHub> _hub;
    private readonly IServiceScopeFactory _scopeFactory;
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(10);

    public PriceBroadcastService(IHubContext<CryptoPulse.Api.Hubs.PriceHub> hub, IServiceScopeFactory scopeFactory)
    {
        _hub = hub;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // BackgroundService is a singleton; DbContext is scoped — resolve per tick.
                using var scope = _scopeFactory.CreateScope();
                var portfolio = scope.ServiceProvider.GetRequiredService<PortfolioService>();
                var view = await portfolio.BuildAsync();

                await _hub.Clients.All.SendAsync("portfolioUpdate", view, stoppingToken);
                Console.WriteLine($"[Broadcast] Pushed update — {view.Holdings.Count} holdings, total ${view.PortfolioTotal}");
            }
            catch (Exception ex)
            {
                // A CoinGecko 429 or transient error must not kill the loop.
                Console.WriteLine($"[Broadcast] Tick failed: {ex.Message}");
            }

            await Task.Delay(Interval, stoppingToken);
        }
    }
}
