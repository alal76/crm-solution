import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import '@testing-library/jest-dom';
import ReportTemplatesPage from '../../pages/ReportTemplatesPage';
import { ReportTemplateDto } from '../../services/reportTemplateService';

const mockNavigate = jest.fn();
jest.mock('react-router-dom', () => ({
  ...jest.requireActual('react-router-dom'),
  useNavigate: () => mockNavigate,
}));

jest.mock('../../services/reportTemplateService', () => ({
  __esModule: true,
  default: {
    getTemplates: jest.fn(),
    applyTemplate: jest.fn(),
  },
}));

// eslint-disable-next-line @typescript-eslint/no-var-requires
import reportTemplateService from '../../services/reportTemplateService';

const mockTemplates: ReportTemplateDto[] = [
  {
    id: 1,
    name: 'Sales Pipeline Report',
    description: 'Comprehensive view of your sales pipeline.',
    category: 'Sales',
    author: 'CRM Solution Team',
    rating: 4.8,
    downloads: 1523,
    tags: ['sales', 'pipeline', 'forecasting'],
    reportConfig: { type: 'pipeline', groupBy: 'stage' },
    createdAt: '2026-01-15T00:00:00Z',
  },
  {
    id: 2,
    name: 'Campaign ROI Dashboard',
    description: 'Track marketing campaign performance.',
    category: 'Marketing',
    author: 'Marketing Ops',
    rating: 4.6,
    downloads: 982,
    tags: ['marketing', 'roi'],
    reportConfig: { type: 'campaign' },
    createdAt: '2026-01-22T00:00:00Z',
  },
];

describe('ReportTemplatesPage', () => {
  beforeEach(() => {
    jest.clearAllMocks();
    (reportTemplateService.getTemplates as jest.Mock).mockResolvedValue({ data: mockTemplates });
    (reportTemplateService.applyTemplate as jest.Mock).mockResolvedValue({
      data: {
        templateId: 1,
        templateName: 'Sales Pipeline Report',
        reportConfig: { type: 'pipeline', groupBy: 'stage' },
        downloads: 1524,
      },
    });
  });

  it('loads report templates from the backend on mount', async () => {
    render(<ReportTemplatesPage />);

    await waitFor(() => expect(reportTemplateService.getTemplates).toHaveBeenCalledTimes(1));
    expect(await screen.findByText('Sales Pipeline Report')).toBeInTheDocument();
    expect(screen.getByText('Campaign ROI Dashboard')).toBeInTheDocument();
  });

  it('shows an error alert when loading templates fails', async () => {
    (reportTemplateService.getTemplates as jest.Mock).mockRejectedValue(new Error('network error'));

    render(<ReportTemplatesPage />);

    expect(await screen.findByText(/failed to load report templates/i)).toBeInTheDocument();
  });

  it('filters templates by search query', async () => {
    render(<ReportTemplatesPage />);
    await screen.findByText('Sales Pipeline Report');

    const searchBox = screen.getByPlaceholderText('Search templates...');
    fireEvent.change(searchBox, { target: { value: 'campaign' } });

    await waitFor(() => {
      expect(screen.queryByText('Sales Pipeline Report')).not.toBeInTheDocument();
    });
    expect(screen.getByText('Campaign ROI Dashboard')).toBeInTheDocument();
  });

  it('applies a template via the backend and navigates to the report designer', async () => {
    render(<ReportTemplatesPage />);
    await screen.findByText('Sales Pipeline Report');

    const useButtons = screen.getAllByRole('button', { name: /use template/i });
    fireEvent.click(useButtons[0]);

    await waitFor(() => expect(reportTemplateService.applyTemplate).toHaveBeenCalledWith(1));
    await waitFor(() =>
      expect(mockNavigate).toHaveBeenCalledWith('/reports/designer', {
        state: {
          templateConfig: { type: 'pipeline', groupBy: 'stage' },
          templateName: 'Sales Pipeline Report',
        },
      })
    );

    // Downloads count is updated from the server response, not an optimistic local increment.
    expect(await screen.findByText('1524 downloads')).toBeInTheDocument();
  });
});
