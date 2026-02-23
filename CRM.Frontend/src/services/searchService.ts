// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
import apiClient from './apiClient';

// ============================================================================
// Search Service - Global search typeahead across all entity types
// ============================================================================

/** A single search result returned from the global search endpoint. */
export interface SearchResult {
  id: number;
  type: 'account' | 'contact' | 'opportunity' | 'ticket' | 'lead';
  title: string;
  subtitle?: string;
  icon?: string;
}

/** Service for global search operations. */
const searchService = {
  /**
   * Perform a global search across all entity types.
   * Falls back to parallel per-entity queries if the /search/global endpoint is unavailable.
   */
  globalSearch: async (query: string, limit: number = 10): Promise<SearchResult[]> => {
    if (!query || query.trim().length < 2) return [];

    try {
      // Try the unified global search endpoint first
      const response = await apiClient.get<SearchResult[]>('/search/global', {
        params: { query: query.trim(), limit },
      });
      return response.data;
    } catch {
      // Fallback: search individual entity endpoints in parallel
      return searchService.fallbackSearch(query, limit);
    }
  },

  /**
   * Fallback search that queries multiple entity endpoints in parallel.
   * Used when the global search endpoint is not available.
   */
  fallbackSearch: async (query: string, limit: number = 10): Promise<SearchResult[]> => {
    const perEntityLimit = Math.max(3, Math.ceil(limit / 5));
    const trimmed = query.trim();

    const [accounts, contacts, opportunities, leads, tickets] = await Promise.allSettled([
      apiClient.get('/accounts', { params: { search: trimmed, pageSize: perEntityLimit } }).then(r => r.data),
      apiClient.get('/contacts', { params: { search: trimmed, pageSize: perEntityLimit } }).then(r => r.data),
      apiClient.get('/opportunities', { params: { search: trimmed, pageSize: perEntityLimit } }).then(r => r.data),
      apiClient.get('/leads', { params: { search: trimmed, pageSize: perEntityLimit } }).then(r => r.data),
      apiClient.get('/servicerequests', { params: { search: trimmed, pageSize: perEntityLimit } }).then(r => r.data),
    ]);

    const results: SearchResult[] = [];

    // Map account results
    if (accounts.status === 'fulfilled') {
      const items = Array.isArray(accounts.value) ? accounts.value : accounts.value?.items || [];
      items.slice(0, perEntityLimit).forEach((a: Record<string, unknown>) => {
        results.push({
          id: a.id as number,
          type: 'account',
          title: (a.name || a.companyName || `Account #${a.id}`) as string,
          subtitle: (a.industry || a.email || '') as string,
        });
      });
    }

    // Map contact results
    if (contacts.status === 'fulfilled') {
      const items = Array.isArray(contacts.value) ? contacts.value : contacts.value?.items || [];
      items.slice(0, perEntityLimit).forEach((c: Record<string, unknown>) => {
        results.push({
          id: c.id as number,
          type: 'contact',
          title: `${c.firstName || ''} ${c.lastName || ''}`.trim() || `Contact #${c.id}`,
          subtitle: (c.email || c.company || '') as string,
        });
      });
    }

    // Map opportunity results
    if (opportunities.status === 'fulfilled') {
      const items = Array.isArray(opportunities.value) ? opportunities.value : opportunities.value?.items || [];
      items.slice(0, perEntityLimit).forEach((o: Record<string, unknown>) => {
        results.push({
          id: o.id as number,
          type: 'opportunity',
          title: (o.name || o.title || `Opportunity #${o.id}`) as string,
          subtitle: o.amount ? `$${Number(o.amount).toLocaleString()}` : (o.stage as string) || '',
        });
      });
    }

    // Map lead results
    if (leads.status === 'fulfilled') {
      const items = Array.isArray(leads.value) ? leads.value : leads.value?.items || [];
      items.slice(0, perEntityLimit).forEach((l: Record<string, unknown>) => {
        results.push({
          id: l.id as number,
          type: 'lead',
          title: `${l.firstName || ''} ${l.lastName || ''}`.trim() || (l.company as string) || `Lead #${l.id}`,
          subtitle: (l.company || l.email || '') as string,
        });
      });
    }

    // Map ticket/service request results
    if (tickets.status === 'fulfilled') {
      const items = Array.isArray(tickets.value) ? tickets.value : tickets.value?.items || [];
      items.slice(0, perEntityLimit).forEach((t: Record<string, unknown>) => {
        results.push({
          id: t.id as number,
          type: 'ticket',
          title: (t.subject || t.title || `Ticket #${t.id}`) as string,
          subtitle: (t.status || t.priority || '') as string,
        });
      });
    }

    return results.slice(0, limit);
  },
};

export default searchService;
