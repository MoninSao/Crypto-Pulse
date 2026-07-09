import { useCallback, useEffect, useState } from "react";
import * as signalR from "@microsoft/signalr";
import { getPortfolio } from "./api";
import type { PortfolioView } from "./types";
import AddHoldingForm from "./AddHoldingForm";
import HoldingTable from "./HoldingTable";
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

  // SignalR: real-time price updates
  useEffect(() => {
    const conn = new signalR.HubConnectionBuilder()
      .withUrl(`${import.meta.env.VITE_API_BASE}/hubs/prices`)
      .withAutomaticReconnect()
      .build();

    conn.on("portfolioUpdate", (view: PortfolioView) => {
      console.log("[SignalR] Portfolio update received", view);
      setData(view);
    });

    conn
      .start()
      .then(() => console.log("[SignalR] Connected"))
      .catch((e) => console.error("[SignalR] connection failed", e));

    return () => {
      conn.stop();
    };
  }, []);

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
          <HoldingTable holdings={data?.holdings ?? []} onChanged={load} />
        )}
      </section>
    </div>
  );
}