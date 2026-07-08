import type { CoinSearchResult, PortfolioView } from "./types";

const BASE = import.meta.env.VITE_API_BASE;

export async function searchCoins(q: string): Promise<CoinSearchResult[]> {
  console.log(`[API] GET /api/coins/search?q=${q}`);
  const startTime = performance.now();
  try {
    const res = await fetch(`${BASE}/api/coins/search?q=${encodeURIComponent(q)}`);
    if (!res.ok) throw new Error("Search failed");
    const data = await res.json();
    const duration = (performance.now() - startTime).toFixed(2);
    console.log(`[API] Search completed in ${duration}ms - ${data.length} results`);
    return data;
  } catch (error) {
    const duration = (performance.now() - startTime).toFixed(2);
    console.error(`[API] Search failed after ${duration}ms:`, error);
    throw error;
  }
}

export async function getPortfolio(): Promise<PortfolioView> {
  console.log(`[API] GET /api/holdings`);
  const startTime = performance.now();
  try {
    const res = await fetch(`${BASE}/api/holdings`);
    if (!res.ok) throw new Error("Failed to load holdings");
    const data = await res.json();
    const duration = (performance.now() - startTime).toFixed(2);
    console.log(`[API] Loaded ${data.holdings.length} holdings in ${duration}ms - Total: $${data.totalValue}`);
    return data;
  } catch (error) {
    const duration = (performance.now() - startTime).toFixed(2);
    console.error(`[API] Failed to load holdings after ${duration}ms:`, error);
    throw error;
  }
}

export async function addHolding(coinId: string, symbol: string, quantity: number) {
  console.log(`[API] POST /api/holdings - ${symbol}: ${quantity}`);
  const startTime = performance.now();
  try {
    const res = await fetch(`${BASE}/api/holdings`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ coinId, symbol, quantity }),
    });
    if (!res.ok) throw new Error("Failed to add holding");
    const data = await res.json();
    const duration = (performance.now() - startTime).toFixed(2);
    console.log(`[API] Holding added in ${duration}ms - ID: ${data.id}`);
    return data;
  } catch (error) {
    const duration = (performance.now() - startTime).toFixed(2);
    console.error(`[API] Failed to add holding after ${duration}ms:`, error);
    throw error;
  }
}

export async function deleteHolding(id: number) {
  console.log(`[API] DELETE /api/holdings/${id}`);
  const startTime = performance.now();
  try {
    const res = await fetch(`${BASE}/api/holdings/${id}`, { method: "DELETE" });
    if (!res.ok) throw new Error("Failed to delete holding");
    const duration = (performance.now() - startTime).toFixed(2);
    console.log(`[API] Holding deleted in ${duration}ms - ID: ${id}`);
  } catch (error) {
    const duration = (performance.now() - startTime).toFixed(2);
    console.error(`[API] Failed to delete holding after ${duration}ms:`, error);
    throw error;
  }
}