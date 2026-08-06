// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the Source-Available License (see LICENSE) as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// Source-Available License (see LICENSE) for more details.
//
// You should have received a copy of the Source-Available License (see LICENSE)
// along with this program. If not, see <https://www.gnu.org/licenses/>.

/**
 * IntegrationsSettingsPage tests (REV-FE-006)
 *
 * Confirms the integration cards render real provider status sourced from
 * GET /api/health/providers, instead of the previous static stub labels,
 * and that the page degrades gracefully while loading and on failure.
 */

import React from 'react';
import { render, screen, waitFor } from '@testing-library/react';
import '@testing-library/jest-dom';
import IntegrationsSettingsPage from '../pages/admin/IntegrationsSettingsPage';
import apiClient from '../services/apiClient';

jest.mock('../services/apiClient');

const mockApiClient = apiClient as jest.Mocked<typeof apiClient>;

const buildReport = (overrides?: Partial<Record<string, boolean>>) => ({
  timestamp: '2026-08-06T00:00:00Z',
  overallHealthy: true,
  providers: {
    Search: { activeProvider: 'Meilisearch', isHealthy: overrides?.Search ?? true, availableProviders: ['Meilisearch'], lastChecked: '2026-08-06T00:00:00Z' },
    Chat: { activeProvider: 'Chatwoot', isHealthy: overrides?.Chat ?? true, availableProviders: ['Chatwoot'], lastChecked: '2026-08-06T00:00:00Z' },
    Notifications: { activeProvider: 'Novu', isHealthy: overrides?.Notifications ?? true, availableProviders: ['Novu'], lastChecked: '2026-08-06T00:00:00Z' },
    Analytics: { activeProvider: 'Superset', isHealthy: overrides?.Analytics ?? true, availableProviders: ['Superset'], lastChecked: '2026-08-06T00:00:00Z' },
    Signatures: { activeProvider: 'DocuSeal', isHealthy: overrides?.Signatures ?? true, availableProviders: ['DocuSeal'], lastChecked: '2026-08-06T00:00:00Z' },
    AI: { activeProvider: 'Ollama', isHealthy: overrides?.AI ?? true, availableProviders: ['Ollama'], lastChecked: '2026-08-06T00:00:00Z' },
    Integrations: { activeProvider: 'n8n', isHealthy: overrides?.Integrations ?? true, availableProviders: ['n8n', 'Zapier'], lastChecked: '2026-08-06T00:00:00Z' },
  },
});

describe('IntegrationsSettingsPage', () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  it('calls the real provider health endpoint on mount', async () => {
    mockApiClient.get = jest.fn().mockResolvedValue({ data: buildReport() });

    render(<IntegrationsSettingsPage />);

    await waitFor(() => {
      expect(mockApiClient.get).toHaveBeenCalledWith(
        '/health/providers',
        expect.objectContaining({ validateStatus: expect.any(Function) })
      );
    });
  });

  it('shows a loading state before the health check resolves', () => {
    mockApiClient.get = jest.fn().mockReturnValue(new Promise(() => {})); // never resolves

    render(<IntegrationsSettingsPage />);

    expect(screen.getAllByText('Checking…').length).toBeGreaterThan(0);
  });

  it('renders Healthy status for each provider once the health check resolves', async () => {
    mockApiClient.get = jest.fn().mockResolvedValue({ data: buildReport() });

    render(<IntegrationsSettingsPage />);

    await waitFor(() => {
      expect(screen.queryAllByText('Checking…').length).toBe(0);
    });

    expect(screen.getAllByText('Healthy').length).toBe(7); // Search, Chat, Notifications, Analytics, Signatures, AI, Integrations(n8n)
    // Zapier is registered but not the active Integrations provider
    expect(screen.getByText('Not active')).toBeInTheDocument();
  });

  it('renders Unreachable for a provider category reported unhealthy', async () => {
    mockApiClient.get = jest.fn().mockResolvedValue({
      data: buildReport({ Search: false }),
    });

    render(<IntegrationsSettingsPage />);

    await waitFor(() => {
      expect(screen.getByText('Unreachable')).toBeInTheDocument();
    });
  });

  it('shows a "Status unavailable" fallback instead of crashing when the health check fails', async () => {
    mockApiClient.get = jest.fn().mockRejectedValue(new Error('Network Error'));

    render(<IntegrationsSettingsPage />);

    await waitFor(() => {
      expect(screen.getAllByText('Status unavailable').length).toBeGreaterThan(0);
    });

    // Page title and static content still render around the fallback status
    expect(screen.getByText('Integrations')).toBeInTheDocument();
  });
});
