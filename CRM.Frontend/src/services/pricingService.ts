import api from './apiClient';

type Params = Record<string, string | number | boolean | undefined>;

export interface PricingCalculationRequest {
  productId?: number;
  priceBookId?: number;
  quantity?: number;
  currency?: string;
  discountPercent?: number;
  discountAmount?: number;
  overrides?: Record<string, unknown>;
  [key: string]: unknown;
}

export const getPriceBooks = (params?: Params) => api.get('/pricebooks', { params }).then((res) => res.data);

export const getPriceBook = (id: number) => api.get(`/api/pricebooks/${id}`).then((res) => res.data);

export const getPriceBookEntries = (priceBookId: number, params?: Params) =>
  api.get(`/api/pricebooks/${priceBookId}/entries`, { params }).then((res) => res.data);

export const getPriceForProduct = (productId: number, priceBookId?: number) =>
  api.get('/pricing/product', { params: { productId, priceBookId } }).then((res) => res.data);

export const calculatePrice = (payload: PricingCalculationRequest) => api.post('/pricing/calculate', payload).then((res) => res.data);

export const applyDiscount = (orderId: number, discountAmount: number, reason?: string) =>
  api.post(`/api/pricing/${orderId}/apply-discount`, { discountAmount, reason }).then((res) => res.data);

export const getPricingRules = (params?: Params) => api.get('/pricing/rules', { params }).then((res) => res.data);

export default {
  getPriceBooks,
  getPriceBook,
  getPriceBookEntries,
  getPriceForProduct,
  calculatePrice,
  applyDiscount,
  getPricingRules,
};
