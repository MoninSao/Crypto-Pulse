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

// Add SignalR for real-time price updates
builder.Services.AddSignalR();
builder.Services.AddScoped<CryptoPulse.Api.Services.PortfolioService>();
builder.Services.AddHostedService<CryptoPulse.Api.Services.PriceBroadcastService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Allow the Vite dev server to call us
builder.Services.AddCors(o => o.AddPolicy("frontend", p => p
    .WithOrigins("http://localhost:5173")
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials()));  // Required for SignalR handshake

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("frontend");

// Map SignalR hub for price updates
app.MapHub<CryptoPulse.Api.Hubs.PriceHub>("/hubs/prices");

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
app.MapGet("/api/holdings", async (CryptoPulse.Api.Services.PortfolioService portfolio) =>
{
    Console.WriteLine("[API] GET /api/holdings");
    var view = await portfolio.BuildAsync();
    return Results.Ok(view);
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

// 5. Debug endpoint for measuring API call reduction
app.MapGet("/api/debug/stats", () =>
    Results.Ok(new { CoinGeckoApiCallCount = CoinGeckoService.ApiCallCount })
);

app.Run();