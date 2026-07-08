using CryptoPulse.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CryptoTracker.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Holding> Holdings => Set<Holding>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Holding>(e =>
        {
            e.ToTable("holdings");
            e.Property(h => h.Id).HasColumnName("id");
            e.Property(h => h.CoinId).HasColumnName("coin_id");
            e.Property(h => h.Symbol).HasColumnName("symbol");
            e.Property(h => h.Quantity).HasColumnName("quantity");
            e.Property(h => h.CreatedAt).HasColumnName("created_at");
        });
    }
}