import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import '@testing-library/jest-dom';
import InvoiceDetailsPage from '../../pages/InvoiceDetailsPage';
import { InvoiceStatus } from '../../services/invoiceService';

// Mock router
jest.mock('react-router-dom', () => ({
  ...jest.requireActual('react-router-dom'),
  useNavigate: () => jest.fn(),
  useParams: () => ({ id: '1' }),
}));

const mockInvoice = {
  id: 1,
  invoiceNumber: 'INV-0001',
  customerId: 42,
  status: InvoiceStatus.Sent,
  issueDate: '2026-01-01T00:00:00Z',
  dueDate: '2026-02-01T00:00:00Z',
  subtotal: 100,
  taxAmount: 10,
  discountAmount: 0,
  totalAmount: 110,
  amountPaid: 0,
  amountDue: 110,
  createdAt: '2026-01-01T00:00:00Z',
};

const mockPdfBlobData = new Blob(['pdf-bytes'], { type: 'application/pdf' });

jest.mock('../../services/invoiceService', () => {
  const actual = jest.requireActual('../../services/invoiceService');
  return {
    __esModule: true,
    ...actual,
    default: {
      getById: jest.fn(),
      getLineItems: jest.fn(),
      generatePdf: jest.fn(),
    },
  };
});

// eslint-disable-next-line @typescript-eslint/no-var-requires
import invoiceService from '../../services/invoiceService';

describe('InvoiceDetailsPage', () => {
  beforeEach(() => {
    jest.clearAllMocks();
    (invoiceService.getById as jest.Mock).mockResolvedValue({ data: mockInvoice });
    (invoiceService.getLineItems as jest.Mock).mockResolvedValue({ data: [] });
    (invoiceService.generatePdf as jest.Mock).mockResolvedValue({ data: mockPdfBlobData });
  });

  it('renders invoice details after loading', async () => {
    render(<InvoiceDetailsPage />);

    expect(await screen.findByText(/Invoice INV-0001/i)).toBeInTheDocument();
  });

  it('downloads a PDF when the Download PDF button is clicked', async () => {
    const createObjectURL = jest.fn().mockReturnValue('blob:mock-url');
    const revokeObjectURL = jest.fn();
    Object.defineProperty(window.URL, 'createObjectURL', { value: createObjectURL, writable: true });
    Object.defineProperty(window.URL, 'revokeObjectURL', { value: revokeObjectURL, writable: true });

    const clickSpy = jest.fn();
    const appendChildSpy = jest.spyOn(document.body, 'appendChild');
    const originalCreateElement = document.createElement.bind(document);
    jest.spyOn(document, 'createElement').mockImplementation((tagName: string) => {
      const element = originalCreateElement(tagName);
      if (tagName === 'a') {
        element.click = clickSpy;
      }
      return element;
    });

    render(<InvoiceDetailsPage />);

    const downloadButton = await screen.findByRole('button', { name: /download pdf/i });
    fireEvent.click(downloadButton);

    await waitFor(() => {
      expect(invoiceService.generatePdf).toHaveBeenCalledWith(mockInvoice.id);
    });

    expect(createObjectURL).toHaveBeenCalled();
    expect(clickSpy).toHaveBeenCalled();
    expect(appendChildSpy).toHaveBeenCalled();

    (document.createElement as jest.Mock).mockRestore();
    appendChildSpy.mockRestore();
  });

  it('shows an error message when the PDF download fails', async () => {
    (invoiceService.generatePdf as jest.Mock).mockRejectedValueOnce(new Error('network error'));

    render(<InvoiceDetailsPage />);

    const downloadButton = await screen.findByRole('button', { name: /download pdf/i });
    fireEvent.click(downloadButton);

    expect(await screen.findByText(/failed to download pdf/i)).toBeInTheDocument();
  });
});
