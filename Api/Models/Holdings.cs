namespace CryptoPulse.Api.Models;

public class Holding
{
    public long Id { get; set; }
    public string CoinId { get; set; } = "";   // CoinGecko id, e.g. "bitcoin"
    public string Symbol { get; set; } = "";   // display, e.g. "BTC"
    public decimal Quantity { get; set; }
    public DateTime CreatedAt { get; set; }
}