/**
 * RecentItemsContext - Track recently viewed records
 */

import React, { createContext, useContext, useState, useEffect, useMemo, useCallback, ReactNode } from 'react';
import { useLocation } from 'react-router-dom';

// Recent item types
export type RecentItemType = 
  | 'account' 
  | 'contact' 
  | 'lead' 
  | 'opportunity' 
  | 'product' 
  | 'campaign'
  | 'quote'
  | 'order'
  | 'invoice'
  | 'ticket'
  | 'incident'
  | 'knowledge'
  | 'user'
  | 'other';

// Recent item structure
export interface RecentItem {
  id: string | number;
  type: RecentItemType;
  title: string;
  subtitle?: string;
  path: string;
  icon?: string;
  timestamp: number;
  metadata?: Record<string, unknown>;
}

// Context type
interface RecentItemsContextType {
  recentItems: RecentItem[];
  addRecentItem: (item: Omit<RecentItem, 'timestamp'>) => void;
  removeRecentItem: (id: string | number, type: RecentItemType) => void;
  clearRecentItems: () => void;
  clearRecentItemsByType: (type: RecentItemType) => void;
  getRecentItemsByType: (type: RecentItemType) => RecentItem[];
  maxItems: number;
  setMaxItems: (max: number) => void;
}

const RecentItemsContext = createContext<RecentItemsContextType | undefined>(undefined);

const STORAGE_KEY = 'crm_recent_items';
const DEFAULT_MAX_ITEMS = 20;

interface RecentItemsProviderProps {
  children: ReactNode;
  maxItems?: number;
  trackNavigation?: boolean;
}

// Entity path patterns for automatic tracking
const entityPatterns: { pattern: RegExp; type: RecentItemType }[] = [
  { pattern: /^\/accounts\/(\d+)/, type: 'account' },
  { pattern: /^\/contacts\/(\d+)/, type: 'contact' },
  { pattern: /^\/leads\/(\d+)/, type: 'lead' },
  { pattern: /^\/opportunities\/(\d+)/, type: 'opportunity' },
  { pattern: /^\/products\/(\d+)/, type: 'product' },
  { pattern: /^\/campaigns\/(\d+)/, type: 'campaign' },
  { pattern: /^\/quotes\/(\d+)/, type: 'quote' },
  { pattern: /^\/orders\/(\d+)/, type: 'order' },
  { pattern: /^\/invoices\/(\d+)/, type: 'invoice' },
  { pattern: /^\/services\/(\d+)/, type: 'ticket' },
  { pattern: /^\/incidents\/(\d+)/, type: 'incident' },
  { pattern: /^\/knowledge\/(\d+)/, type: 'knowledge' },
  { pattern: /^\/admin\/users\/(\d+)/, type: 'user' },
];

export const RecentItemsProvider: React.FC<RecentItemsProviderProps> = ({
  children,
  maxItems: initialMaxItems = DEFAULT_MAX_ITEMS,
  trackNavigation = true,
}) => {
  // Load from localStorage
  const [recentItems, setRecentItems] = useState<RecentItem[]>(() => {
    if (typeof window !== 'undefined') {
      try {
        const stored = localStorage.getItem(STORAGE_KEY);
        if (stored) {
          return JSON.parse(stored);
        }
      } catch {
        // Invalid stored data, start fresh
      }
    }
    return [];
  });

  const [maxItems, setMaxItems] = useState(initialMaxItems);
  const location = useLocation();

  // Persist to localStorage
  useEffect(() => {
    if (typeof window !== 'undefined') {
      localStorage.setItem(STORAGE_KEY, JSON.stringify(recentItems));
    }
  }, [recentItems]);

  // Add recent item
  const addRecentItem = useCallback((item: Omit<RecentItem, 'timestamp'>) => {
    setRecentItems((prev) => {
      // Remove existing item with same id and type
      const filtered = prev.filter(
        (existing) => !(existing.id === item.id && existing.type === item.type)
      );

      // Add new item at the beginning
      const newItem: RecentItem = {
        ...item,
        timestamp: Date.now(),
      };

      // Trim to max items
      const updated = [newItem, ...filtered].slice(0, maxItems);
      return updated;
    });
  }, [maxItems]);

  // Remove recent item
  const removeRecentItem = useCallback((id: string | number, type: RecentItemType) => {
    setRecentItems((prev) =>
      prev.filter((item) => !(item.id === id && item.type === type))
    );
  }, []);

  // Clear all recent items
  const clearRecentItems = useCallback(() => {
    setRecentItems([]);
  }, []);

  // Clear recent items by type
  const clearRecentItemsByType = useCallback((type: RecentItemType) => {
    setRecentItems((prev) => prev.filter((item) => item.type !== type));
  }, []);

  // Get recent items by type
  const getRecentItemsByType = useCallback(
    (type: RecentItemType) => recentItems.filter((item) => item.type === type),
    [recentItems]
  );

  // Auto-track navigation (for detail pages)
  useEffect(() => {
    if (!trackNavigation) return;

    const path = location.pathname;
    
    // Check if current path matches any entity pattern
    for (const { pattern, type } of entityPatterns) {
      const match = path.match(pattern);
      if (match) {
        const id = match[1];
        // We can't get the title from the URL, so this is a placeholder
        // The actual page component should call addRecentItem with full details
        break;
      }
    }
  }, [location.pathname, trackNavigation]);

  const value = useMemo(
    () => ({
      recentItems,
      addRecentItem,
      removeRecentItem,
      clearRecentItems,
      clearRecentItemsByType,
      getRecentItemsByType,
      maxItems,
      setMaxItems,
    }),
    [
      recentItems,
      addRecentItem,
      removeRecentItem,
      clearRecentItems,
      clearRecentItemsByType,
      getRecentItemsByType,
      maxItems,
    ]
  );

  return (
    <RecentItemsContext.Provider value={value}>
      {children}
    </RecentItemsContext.Provider>
  );
};

export const useRecentItems = (): RecentItemsContextType => {
  const context = useContext(RecentItemsContext);
  if (!context) {
    throw new Error('useRecentItems must be used within a RecentItemsProvider');
  }
  return context;
};

// Hook for tracking page views with full details
export const useTrackRecentItem = () => {
  const { addRecentItem } = useRecentItems();

  return useCallback(
    (item: Omit<RecentItem, 'timestamp'>) => {
      addRecentItem(item);
    },
    [addRecentItem]
  );
};

export default RecentItemsContext;
