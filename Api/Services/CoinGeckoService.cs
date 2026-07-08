using System.Text.Json;
using CryptoPulse.Api.Dtos;
using Microsoft.Extensions.Caching.Memory;

namespace CryptoPulse.Api.Services;

public class CoinGeckoService
{
    private readonly HttpClient _http;
    private readonly IMemoryCache _cache;
    private const string Base = "https://api.coingecko.com/api/v3";

    public CoinGeckoService(HttpClient http, IMemoryCache cache)
    {
        _http = http;
        _cache = cache;
        // CoinGecko requires a User-Agent or it may 403
        if (!_http.DefaultRequestHeaders.Contains("User-Agent"))
            _http.DefaultRequestHeaders.Add("User-Agent", "CryptoPulse/1.0");
    }

    // GET /search?query=eth
    public async Task<List<CoinSearchResult>> SearchAsync(string query)
    {
        var url = $"{Base}/search?query={Uri.EscapeDataString(query)}";
        using var doc = JsonDocument.Parse(await _http.GetStringAsync(url));

        var results = new List<CoinSearchResult>();
        if (doc.RootElement.TryGetProperty("coins", out var coins))
        {
            foreach (var c in coins.EnumerateArray())
            {
                results.Add(new CoinSearchResult(
                    c.GetProperty("id").GetString() ?? "",
                    (c.GetProperty("symbol").GetString() ?? "").ToUpper(),
                    c.GetProperty("name").GetString() ?? "",
                    c.TryGetProperty("thumb", out var t) ? t.GetString() : null
                ));
            }
        }
        return results.Take(10).ToList();
    }

    // GET /simple/price?ids=bitcoin,ethereum&vs_currencies=usd  (batched + cached)
    public async Task<Dictionary<string, decimal>> GetPricesAsync(IEnumerable<string> coinIds)
    {
        var ids = coinIds.Distinct().OrderBy(x => x).ToList();
        if (ids.Count == 0) return new();

        var cacheKey = "prices:" + string.Join(",", ids);
        if (_cache.TryGetValue(cacheKey, out Dictionary<string, decimal>? cached) && cached is not null)
            return cached;

        var url = $"{Base}/simple/price?ids={string.Join(",", ids)}&vs_currencies=usd";
        using var doc = JsonDocument.Parse(await _http.GetStringAsync(url));

        var prices = new Dictionary<string, decimal>();
        foreach (var coin in doc.RootElement.EnumerateObject())
        {
            if (coin.Value.TryGetProperty("usd", out var usd))
                prices[coin.Name] = usd.GetDecimal();
        }

        _cache.Set(cacheKey, prices, TimeSpan.FromSeconds(60));
        return prices;
    }
}