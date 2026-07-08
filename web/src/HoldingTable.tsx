import type { HoldingView } from "./types";
import { deleteHolding } from "./api";

const getDecimalPlaces = (n: number): number => {
  const str = n.toString();
  if (!str.includes(".")) return 0;
  return str.split(".")[1].length;
};

const usd = (n: number, minDecimalPlaces?: number) =>
  n.toLocaleString("en-US", {
    style: "currency",
    currency: "USD",
    minimumFractionDigits: minDecimalPlaces ?? 2,
    maximumFractionDigits: minDecimalPlaces ?? 2,
  });

export default function HoldingsTable({
  holdings,
  onChanged,
}: {
  holdings: HoldingView[];
  onChanged: () => void;
}) {
  if (holdings.length === 0) {
    return <p className="muted">No holdings yet. Add one above to get started.</p>;
  }

  async function remove(id: number) {
    await deleteHolding(id);
    onChanged();
  }

  return (
    <table className="holdings">
      <thead>
        <tr>
          <th>Coin</th>
          <th>Quantity</th>
          <th>Price</th>
          <th>Value</th>
          <th></th>
        </tr>
      </thead>
      <tbody>
        {holdings.map((h) => {
          const decimalPlaces = getDecimalPlaces(h.currentPrice);
          return (
            <tr key={h.id}>
              <td>{h.symbol}</td>
              <td>{h.quantity}</td>
              <td>{usd(h.currentPrice, decimalPlaces)}</td>
              <td>{usd(h.currentValue, decimalPlaces)}</td>
              <td>
                <button className="link danger" onClick={() => remove(h.id)}>
                  remove
                </button>
              </td>
            </tr>
          );
        })}
      </tbody>
    </table>
  );
}