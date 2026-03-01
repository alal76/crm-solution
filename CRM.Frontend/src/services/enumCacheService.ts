/**
 * ENUM-FE-003: enumCacheService.ts
 * Client-side localStorage cache for enum values to reduce redundant API calls.
 * TTL is 1 hour. Invalidation can be triggered manually (e.g. after CRUD ops).
 * Also registers its invalidate() function with enumService (ENUM-FE-018).
 */
import enumService, { registerCacheInvalidator } from './enumService';
import { EnumValue } from '../types/enums';

const CACHE_PREFIX = 'crm_enum_cache_';
const CACHE_TTL_MS = 60 * 60 * 1000; // 1 hour

interface CacheEntry {
  values: EnumValue[];
  timestamp: number;
}

const enumCacheService = {
  /**
   * Return cached EnumValue[] for the given category name.
   * Fetches fresh data and repopulates the cache on a miss or expired TTL.
   */
  async getValues(categoryName: string): Promise<EnumValue[]> {
    const key = CACHE_PREFIX + categoryName.toLowerCase();
    try {
      const raw = localStorage.getItem(key);
      if (raw) {
        const entry: CacheEntry = JSON.parse(raw);
        if (Date.now() - entry.timestamp < CACHE_TTL_MS) {
          return entry.values;
        }
      }
    } catch {
      /* ignore JSON parse errors */
    }

    // Cache miss or expired — fetch from API
    const values = await enumService.getValuesByCategoryName(categoryName);
    try {
      const entry: CacheEntry = { values, timestamp: Date.now() };
      localStorage.setItem(key, JSON.stringify(entry));
    } catch {
      /* ignore storage quota errors */
    }
    return values;
  },

  /**
   * Invalidate the local cache.
   * Pass a categoryName to invalidate only that category; omit to clear all.
   */
  invalidate(categoryName?: string): void {
    if (categoryName) {
      localStorage.removeItem(CACHE_PREFIX + categoryName.toLowerCase());
    } else {
      Object.keys(localStorage)
        .filter(k => k.startsWith(CACHE_PREFIX))
        .forEach(k => localStorage.removeItem(k));
    }
  },
};

// ENUM-FE-018: Register the cache invalidator so enumService mutations auto-invalidate
registerCacheInvalidator((name?: string) => enumCacheService.invalidate(name));

export default enumCacheService;
