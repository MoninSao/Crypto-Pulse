using CryptoPulse.Api.Data;
using CryptoPulse.Api.Dtos;
using Microsoft.EntityFrameworkCore;

namespace CryptoPulse.Api.Services;

public class PortfolioService
{
    private readonly AppDbContext _db;
    private readonly CoinGeckoService _gecko;

    public PortfolioService(AppDbContext db, CoinGeckoService gecko)
    {
        _db = db;
        _gecko = gecko;
    }

    public async Task<PortfolioView> BuildAsync()
    {
        var holdings = await _db.Holdings.OrderByDescending(h => h.CreatedAt).ToListAsync();
        var prices = await _gecko.GetPricesAsync(holdings.Select(h => h.CoinId));

        var views = holdings.Select(h =>
        {
            var price = prices.TryGetValue(h.CoinId, out var p) ? p : 0m;
            return new HoldingView(h.Id, h.CoinId, h.Symbol, h.Quantity, price, price * h.Quantity);
        }).ToList();

        return new PortfolioView(views, views.Sum(v => v.CurrentValue));
    }
}
