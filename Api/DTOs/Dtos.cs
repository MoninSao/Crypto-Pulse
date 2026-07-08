namespace CryptoPulse.Api.Dtos;

// Incoming body for POST /api/holdings
public record HoldingInput(string CoinId, string Symbol, decimal Quantity);

// A single row in the enriched holdings response
public record HoldingView(
    long Id,
    string CoinId,
    string Symbol,
    decimal Quantity,
    decimal CurrentPrice,
    decimal CurrentValue
);

// The full GET /api/holdings response
public record PortfolioView(List<HoldingView> Holdings, decimal PortfolioTotal);

// A coin search result row
public record CoinSearchResult(string CoinId, string Symbol, string Name, string? Thumb);