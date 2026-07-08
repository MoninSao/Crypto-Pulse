using CryptoPulse.Api.Data;
using CryptoPulse.Api.Dtos;
using CryptoPulse.Api.Models;
using CryptoPulse.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// --- Services ---
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddMemoryCache();
builder.Services.AddHttpClient<CoinGeckoService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Allow the Vite dev server to call us
builder.Services.AddCors(o => o.AddPolicy("frontend", p => p
    .WithOrigins("http://localhost:5173")
    .AllowAnyHeader()
    .AllowAnyMethod()));

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("frontend");

// --- Endpoints ---

// 1. Search coins (proxies CoinGecko)
app.MapGet("/api/coins/search", async (string q, CoinGeckoService gecko) =>
{
    if (string.IsNullOrWhiteSpace(q)) return Results.Ok(new List<CoinSearchResult>());
    return Results.Ok(await gecko.SearchAsync(q));
});

// 2. List holdings enriched with live price + value + total
app.MapGet("/api/holdings", async (AppDbContext db, CoinGeckoService gecko) =>
{
    var holdings = await db.Holdings.OrderByDescending(h => h.CreatedAt).ToListAsync();
    var prices = await gecko.GetPricesAsync(holdings.Select(h => h.CoinId));

    var views = holdings.Select(h =>
    {
        var price = prices.TryGetValue(h.CoinId, out var p) ? p : 0m;
        return new HoldingView(h.Id, h.CoinId, h.Symbol, h.Quantity, price, price * h.Quantity);
    }).ToList();

    var total = views.Sum(v => v.CurrentValue);
    return Results.Ok(new PortfolioView(views, total));
});

// 3. Add a holding
app.MapPost("/api/holdings", async (HoldingInput input, AppDbContext db) =>
{
    if (input.Quantity <= 0) return Results.BadRequest("Quantity must be positive.");

    var holding = new Holding
    {
        CoinId = input.CoinId,
        Symbol = input.Symbol,
        Quantity = input.Quantity,
        CreatedAt = DateTime.UtcNow
    };
    db.Holdings.Add(holding);
    await db.SaveChangesAsync();
    return Results.Created($"/api/holdings/{holding.Id}", holding);
});

// 4. Delete a holding
app.MapDelete("/api/holdings/{id:long}", async (long id, AppDbContext db) =>
{
    var holding = await db.Holdings.FindAsync(id);
    if (holding is null) return Results.NotFound();
    db.Holdings.Remove(holding);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.Run();