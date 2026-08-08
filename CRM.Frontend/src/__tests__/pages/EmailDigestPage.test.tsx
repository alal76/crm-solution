import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import '@testing-library/jest-dom';
import EmailDigestPage from '../../pages/EmailDigestPage';
import { EmailDigestConfigDto } from '../../services/emailDigestService';

jest.mock('../../services/emailDigestService', () => ({
  __esModule: true,
  default: {
    getConfig: jest.fn(),
    updateConfig: jest.fn(),
    sendPreview: jest.fn(),
  },
}));

// eslint-disable-next-line @typescript-eslint/no-var-requires
import emailDigestService from '../../services/emailDigestService';

const mockConfig: EmailDigestConfigDto = {
  enabled: true,
  frequency: 'weekly',
  dayOfWeek: 2,
  timeOfDay: '09:00',
  timezone: 'America/New_York',
  sections: {
    newLeads: true,
    openOpportunities: true,
    recentActivities: false,
    upcomingTasks: true,
    overdueTasks: true,
    teamPerformance: false,
    kpiSummary: true,
  },
};

describe('EmailDigestPage', () => {
  beforeEach(() => {
    jest.clearAllMocks();
    (emailDigestService.getConfig as jest.Mock).mockResolvedValue(mockConfig);
    (emailDigestService.updateConfig as jest.Mock).mockResolvedValue(mockConfig);
    (emailDigestService.sendPreview as jest.Mock).mockResolvedValue(undefined);
  });

  it('loads the digest config from the backend on mount', async () => {
    render(<EmailDigestPage />);

    await waitFor(() => expect(emailDigestService.getConfig).toHaveBeenCalledTimes(1));
    expect(await screen.findByText(/Weekly at 09:00/i)).toBeInTheDocument();
  });

  it('saves the config via the backend when Save Settings is clicked', async () => {
    render(<EmailDigestPage />);

    await waitFor(() => expect(emailDigestService.getConfig).toHaveBeenCalledTimes(1));

    const saveButton = await screen.findByRole('button', { name: /save settings/i });
    fireEvent.click(saveButton);

    await waitFor(() => expect(emailDigestService.updateConfig).toHaveBeenCalledTimes(1));
    expect(emailDigestService.updateConfig).toHaveBeenCalledWith(mockConfig);
    expect(await screen.findByText(/saved successfully/i)).toBeInTheDocument();
  });

  it('sends a preview via the backend when Send Preview is clicked', async () => {
    render(<EmailDigestPage />);

    await waitFor(() => expect(emailDigestService.getConfig).toHaveBeenCalledTimes(1));

    const previewButton = await screen.findByRole('button', { name: /send preview/i });
    fireEvent.click(previewButton);

    await waitFor(() => expect(emailDigestService.sendPreview).toHaveBeenCalledTimes(1));
  });

  it('logs but does not crash when loading the config fails', async () => {
    (emailDigestService.getConfig as jest.Mock).mockRejectedValue(new Error('network error'));

    render(<EmailDigestPage />);

    await waitFor(() => expect(emailDigestService.getConfig).toHaveBeenCalledTimes(1));
    expect(screen.getByText(/Email Digest Settings/i)).toBeInTheDocument();
  });
});
