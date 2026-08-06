import React from 'react';
import { screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import '@testing-library/jest-dom';
import FeatureFlagsPanel from '../FeatureFlagsPanel';
import { renderWithProviders } from '../../../test-utils/renderWithProviders';
import { featureFlagService, FeatureFlagDto } from '../../../services/featureFlagService';

/**
 * Component tests for FeatureFlagsPanel.
 * Verifies the panel loads flags from the backend, lets an admin toggle them,
 * and saves changes via featureFlagService.updateFlag (REV-FE-005).
 */

jest.mock('../../../services/featureFlagService', () => ({
  featureFlagService: {
    getAllFlags: jest.fn(),
    getFlag: jest.fn(),
    updateFlag: jest.fn(),
  },
}));

jest.mock('../../../services/logger', () => ({
  __esModule: true,
  default: { info: jest.fn(), error: jest.fn(), warn: jest.fn(), debug: jest.fn() },
}));

const mockedFeatureFlagService = featureFlagService as jest.Mocked<typeof featureFlagService>;

const mockFlags: FeatureFlagDto[] = [
  {
    name: 'EnableITSM',
    displayName: 'ITSM Module',
    description: 'Enable/disable ITSM Module',
    enabled: true,
    category: 'Module',
    requiresRestart: true,
    rolloutPercentage: 100,
    targetedUserIds: [],
    targetedRoles: [],
  },
  {
    name: 'UseExternalSearch',
    displayName: 'External Search',
    description: 'Use external Search provider',
    enabled: false,
    category: 'Provider',
    providerCategory: 'Search',
    activeProvider: 'BuiltIn',
    requiresRestart: false,
    rolloutPercentage: 100,
    targetedUserIds: [],
    targetedRoles: [],
  },
];

describe('FeatureFlagsPanel', () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  it('loads and displays feature flags from the backend', async () => {
    mockedFeatureFlagService.getAllFlags.mockResolvedValue(mockFlags);

    renderWithProviders(<FeatureFlagsPanel />);

    expect(mockedFeatureFlagService.getAllFlags).toHaveBeenCalledTimes(1);

    await waitFor(() => expect(screen.getByText('ITSM Module')).toBeInTheDocument());
    expect(screen.getByText('External Search')).toBeInTheDocument();
  });

  it('shows an error alert when loading fails', async () => {
    mockedFeatureFlagService.getAllFlags.mockRejectedValue(new Error('network error'));

    renderWithProviders(<FeatureFlagsPanel />);

    await waitFor(() =>
      expect(screen.getByText(/Failed to load feature flags/i)).toBeInTheDocument()
    );
  });

  it('disables Save Changes until a flag is toggled, then calls updateFlag and shows success', async () => {
    mockedFeatureFlagService.getAllFlags.mockResolvedValue(mockFlags);
    mockedFeatureFlagService.updateFlag.mockResolvedValue(undefined);

    renderWithProviders(<FeatureFlagsPanel />);

    await waitFor(() => expect(screen.getByText('ITSM Module')).toBeInTheDocument());

    const saveButton = screen.getByRole('button', { name: /save changes/i });
    expect(saveButton).toBeDisabled();

    const rows = screen.getAllByRole('row');
    const searchRow = rows.find(row => within(row).queryByText('External Search'));
    expect(searchRow).toBeDefined();
    const toggle = within(searchRow as HTMLElement).getByRole('checkbox');

    await userEvent.click(toggle);
    expect(saveButton).not.toBeDisabled();

    await userEvent.click(saveButton);

    await waitFor(() =>
      expect(mockedFeatureFlagService.updateFlag).toHaveBeenCalledWith(
        'UseExternalSearch',
        expect.objectContaining({ name: 'UseExternalSearch', enabled: true })
      )
    );

    await waitFor(() =>
      expect(screen.getByText(/Feature flags saved successfully/i)).toBeInTheDocument()
    );

    // Only the changed flag should be sent to the backend.
    expect(mockedFeatureFlagService.updateFlag).toHaveBeenCalledTimes(1);
  });

  it('shows an error alert when saving fails', async () => {
    mockedFeatureFlagService.getAllFlags.mockResolvedValue(mockFlags);
    mockedFeatureFlagService.updateFlag.mockRejectedValue({
      response: { data: { error: 'Failed to update flag' } },
    });

    renderWithProviders(<FeatureFlagsPanel />);

    await waitFor(() => expect(screen.getByText('ITSM Module')).toBeInTheDocument());

    const rows = screen.getAllByRole('row');
    const itsmRow = rows.find(row => within(row).queryByText('ITSM Module'));
    const toggle = within(itsmRow as HTMLElement).getByRole('checkbox');

    await userEvent.click(toggle);
    await userEvent.click(screen.getByRole('button', { name: /save changes/i }));

    await waitFor(() =>
      expect(screen.getByText('Failed to update flag')).toBeInTheDocument()
    );
  });
});
