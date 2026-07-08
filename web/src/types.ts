export interface CoinSearchResult {
  coinId: string;
  symbol: string;
  name: string;
  thumb?: string | null;
}

export interface HoldingView {
  id: number;
  coinId: string;
  symbol: string;
  quantity: number;
  currentPrice: number;
  currentValue: number;
}

export interface PortfolioView {
  holdings: HoldingView[];
  portfolioTotal: number;
}