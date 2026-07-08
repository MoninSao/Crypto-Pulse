import { useEffect, useState } from "react";
import { searchCoins, addHolding } from "./api";
import type { CoinSearchResult } from "./types";

export default function AddHoldingForm({ onAdded }: { onAdded: () => void }) {
  const [query, setQuery] = useState("");
  const [results, setResults] = useState<CoinSearchResult[]>([]);
  const [selected, setSelected] = useState<CoinSearchResult | null>(null);
  const [quantity, setQuantity] = useState("");
  const [busy, setBusy] = useState(false);

  // Debounced search
  useEffect(() => {
    if (selected || query.trim().length < 2) {
      setResults([]);
      return;
    }
    const t = setTimeout(async () => {
      try {
        setResults(await searchCoins(query.trim()));
      } catch {
        setResults([]);
      }
    }, 300);
    return () => clearTimeout(t);
  }, [query, selected]);

  async function handleSubmit() {
    if (!selected || !quantity || Number(quantity) <= 0) return;
    setBusy(true);
    try {
      await addHolding(selected.coinId, selected.symbol, Number(quantity));
      setQuery("");
      setSelected(null);
      setQuantity("");
      setResults([]);
      onAdded();
    } catch (e) {
      alert("Could not add holding.");
    } finally {
      setBusy(false);
    }
  }

  return (
    <div className="card">
      <h2>Add a holding</h2>

      {!selected ? (
        <div className="search-wrap">
          <input
            placeholder="Search a coin (e.g. bitcoin)"
            value={query}
            onChange={(e) => setQuery(e.target.value)}
          />
          {results.length > 0 && (
            <ul className="results">
              {results.map((c) => (
                <li key={c.coinId} onClick={() => setSelected(c)}>
                  {c.thumb && <img src={c.thumb} alt="" width={18} height={18} />}
                  <span>{c.name}</span>
                  <span className="muted">{c.symbol}</span>
                </li>
              ))}
            </ul>
          )}
        </div>
      ) : (
        <div className="selected-row">
          <div>
            Selected: <strong>{selected.name}</strong> ({selected.symbol})
          </div>
          <button className="link" onClick={() => setSelected(null)}>change</button>
        </div>
      )}

      {selected && (
        <div className="qty-row">
          <input
            type="number"
            min="0"
            step="any"
            placeholder="Amount you own"
            value={quantity}
            onChange={(e) => setQuantity(e.target.value)}
          />
          <button disabled={busy} onClick={handleSubmit}>
            {busy ? "Adding…" : "Add"}
          </button>
        </div>
      )}
    </div>
  );
}