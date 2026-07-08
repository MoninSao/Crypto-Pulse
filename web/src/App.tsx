import { useCallback, useEffect, useState } from "react";
import { getPortfolio } from "./api";
import type { PortfolioView } from "./types";
import AddHoldingForm from "./AddHoldingForm";
import HoldingsTable from "./HoldingsTable";
import "./App.css";

const usd = (n: number) =>
  n.toLocaleString("en-US", { style: "currency", currency: "USD" });

export default function App() {
  const [data, setData] = useState<PortfolioView | null>(null);
  const [loading, setLoading] = useState(true);

  const load = useCallback(async () => {
    setLoading(true);
    try {
      setData(await getPortfolio());
    } catch {
      setData(null);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    load();
  }, [load]);

  return (
    <div className="container">
      <header>
        <h1>Crypto Portfolio Tracker</h1>
        <div className="total">
          {data ? usd(data.portfolioTotal) : "—"}
          <span className="total-label">total value</span>
        </div>
      </header>

      <AddHoldingForm onAdded={load} />

      <section className="card">
        <div className="section-head">
          <h2>Your holdings</h2>
          <button className="link" onClick={load}>refresh</button>
        </div>
        {loading ? (
          <p className="muted">Loading…</p>
        ) : (
          <HoldingsTable holdings={data?.holdings ?? []} onChanged={load} />
        )}
      </section>
    </div>
  );
}