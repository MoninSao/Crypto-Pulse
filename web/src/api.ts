import type { CoinSearchResult, PortfolioView } from "./types";

const BASE = import.meta.env.VITE_API_BASE;

export async function searchCoins(q: string): Promise<CoinSearchResult[]> {
  const res = await fetch(`${BASE}/api/coins/search?q=${encodeURIComponent(q)}`);
  if (!res.ok) throw new Error("Search failed");
  return res.json();
}

export async function getPortfolio(): Promise<PortfolioView> {
  const res = await fetch(`${BASE}/api/holdings`);
  if (!res.ok) throw new Error("Failed to load holdings");
  return res.json();
}

export async function addHolding(coinId: string, symbol: string, quantity: number) {
  const res = await fetch(`${BASE}/api/holdings`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ coinId, symbol, quantity }),
  });
  if (!res.ok) throw new Error("Failed to add holding");
  return res.json();
}

export async function deleteHolding(id: number) {
  const res = await fetch(`${BASE}/api/holdings/${id}`, { method: "DELETE" });
  if (!res.ok) throw new Error("Failed to delete holding");
}