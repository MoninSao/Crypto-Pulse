using CryptoPulse.Api.Data;
using CryptoPulse.Api.Dtos;
using CryptoPulse.Api.Models;
using CryptoPulse.Api.Services;
using DotNetEnv;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

Env.Load();  // reads api/.env into environment variables

// --- Services ---
var conn = builder.Configuration.GetConnectionString("Default")!
    .Replace("__DB_PASSWORD__", Environment.GetEnvironmentVariable("DB_PASSWORD"));

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseNpgsql(conn));

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
    Console.WriteLine($"[API] GET /api/coins/search - query: {q}");
    if (string.IsNullOrWhiteSpace(q)) return Results.Ok(new List<CoinSearchResult>());
    var result = await gecko.SearchAsync(q);
    Console.WriteLine($"[API] Search returned {result.Count} results");
    return Results.Ok(result);
});

// 2. List holdings enriched with live price + value + total
app.MapGet("/api/holdings", async (AppDbContext db, CoinGeckoService gecko) =>
{
    Console.WriteLine("[API] GET /api/holdings");
    var holdings = await db.Holdings.OrderByDescending(h => h.CreatedAt).ToListAsync();
    Console.WriteLine($"[API] Loaded {holdings.Count} holdings from database");
    var prices = await gecko.GetPricesAsync(holdings.Select(h => h.CoinId));
    Console.WriteLine($"[API] Fetched prices for {prices.Count} coins");

    var views = holdings.Select(h =>
    {
        var price = prices.TryGetValue(h.CoinId, out var p) ? p : 0m;
        return new HoldingView(h.Id, h.CoinId, h.Symbol, h.Quantity, price, price * h.Quantity);
    }).ToList();

    var total = views.Sum(v => v.CurrentValue);
    Console.WriteLine($"[API] Portfolio total value: ${total}");
    return Results.Ok(new PortfolioView(views, total));
});

// 3. Add a holding
app.MapPost("/api/holdings", async (HoldingInput input, AppDbContext db) =>
{
    Console.WriteLine($"[API] POST /api/holdings - coinId: {input.CoinId}, symbol: {input.Symbol}, quantity: {input.Quantity}");
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
    Console.WriteLine($"[API] Holding created with ID: {holding.Id}");
    return Results.Created($"/api/holdings/{holding.Id}", holding);
});

// 4. Delete a holding
app.MapDelete("/api/holdings/{id:long}", async (long id, AppDbContext db) =>
{
    Console.WriteLine($"[API] DELETE /api/holdings/{id}");
    var holding = await db.Holdings.FindAsync(id);
    if (holding is null)
    {
        Console.WriteLine($"[API] Holding {id} not found");
        return Results.NotFound();
    }
    db.Holdings.Remove(holding);
    await db.SaveChangesAsync();
    Console.WriteLine($"[API] Holding {id} deleted");
    return Results.NoContent();
});

app.Run();