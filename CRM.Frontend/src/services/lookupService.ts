import apiClient from './apiClient';

export interface LookupItem {
  id: number;
  key: string;
  value: string;
  meta?: string | null;
}

const cache: Record<string, Promise<LookupItem[]>> = {};

export const getLookupItems = (categoryName: string): Promise<LookupItem[]> => {
  const key = categoryName;
  if (!cache[key]) {
    cache[key] = apiClient.get(`/lookups/items/${encodeURIComponent(categoryName)}`).then(r => r.data as LookupItem[]).catch(err => {
      delete cache[key];
      throw err;
    });
  }
  return cache[key];
};

const lookupService = { getLookupItems };
export default lookupService;
