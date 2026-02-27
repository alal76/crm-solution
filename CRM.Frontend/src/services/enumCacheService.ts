/**
 * CRM Solution - Enum Cache Service
 * Phase 3: Frontend Implementation (SPEC-GEN-002)
 * Provides client-side caching with memory + localStorage fallback
 */

import { EnumValue } from '../types/enums';
import enumService from './enumService';

const CACHE_KEY_PREFIX = 'enum_cache_';
const CACHE_TTL_MS = 3600000; // 1 hour

interface CacheEntry {
  values: EnumValue[];
  timestamp: number;
}

class EnumCacheService {
  private memoryCache: Map<string, CacheEntry> = new Map();

  private getCacheKey(categoryName: string): string {
    return `${CACHE_KEY_PREFIX}${categoryName}`;
  }

  private isValid(entry: CacheEntry): boolean {
    return (Date.now() - entry.timestamp) < CACHE_TTL_MS;
  }

  async getValues(categoryName: string, includeInactive = false): Promise<EnumValue[]> {
    const cacheKey = this.getCacheKey(categoryName);
    
    // Check memory cache first
    const memEntry = this.memoryCache.get(cacheKey);
    if (memEntry && this.isValid(memEntry)) {
      return includeInactive ? memEntry.values : memEntry.values.filter(v => v.isActive);
    }

    // Check localStorage
    try {
      const stored = localStorage.getItem(cacheKey);
      if (stored) {
        const entry: CacheEntry = JSON.parse(stored);
        if (this.isValid(entry)) {
          this.memoryCache.set(cacheKey, entry);
          return includeInactive ? entry.values : entry.values.filter(v => v.isActive);
        }
      }
    } catch (error) {
      console.warn('Error reading from localStorage cache:', error);
    }

    // Fetch from API
    const response = await enumService.getValuesByCategoryName(categoryName, true); // always fetch all
    const entry: CacheEntry = {
      values: response.data,
      timestamp: Date.now()
    };

    this.memoryCache.set(cacheKey, entry);
    try {
      localStorage.setItem(cacheKey, JSON.stringify(entry));
    } catch (error) {
      console.warn('Error writing to localStorage cache:', error);
    }

    return includeInactive ? entry.values : entry.values.filter(v => v.isActive);
  }

  invalidate(categoryName?: string): void {
    if (categoryName) {
      const cacheKey = this.getCacheKey(categoryName);
      this.memoryCache.delete(cacheKey);
      try {
        localStorage.removeItem(cacheKey);
      } catch (error) {
        console.warn('Error removing from localStorage cache:', error);
      }
    } else {
      // Clear all enum caches
      this.memoryCache.clear();
      try {
        const keysToRemove = Object.keys(localStorage).filter(key => key.startsWith(CACHE_KEY_PREFIX));
        keysToRemove.forEach(key => localStorage.removeItem(key));
      } catch (error) {
        console.warn('Error clearing localStorage cache:', error);
      }
    }
  }
}

export const enumCacheService = new EnumCacheService();
export default enumCacheService;
