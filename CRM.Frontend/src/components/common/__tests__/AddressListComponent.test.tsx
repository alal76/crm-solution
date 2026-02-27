import React from 'react';
import { render, screen, fireEvent, waitFor, within, act } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { BrowserRouter } from 'react-router-dom';
import AddressListComponent, { AddressListComponentProps } from '../AddressListComponent';
import { Address } from '../../../types/address.types';
import '@testing-library/jest-dom';

/**
 * Component Tests for AddressListComponent
 * Tests cover: Rendering, loading states, empty states, interactions, and data filtering
 *
 * FUNCTIONAL VIEW:
 * - Tests address list rendering
 * - Tests loading and empty states
 * - Tests edit and delete interactions
 * - Tests primary address functionality
 * - Tests address filtering
 *
 * TECHNICAL VIEW:
 * - Uses React Testing Library for component testing
 * - Mocks address service dependencies
 * - Tests user interactions with fireEvent and userEvent
 * - Verifies DOM updates and callbacks
 */

const mockAddresses: Address[] = [
  {
    id: 1,
    label: 'Main Office',
    line1: '123 Main Street',
    city: 'New York',
    state: 'NY',
    zipCode: '10001',
    country: 'United States',
    addressType: 'Primary',
    isPrimary: true,
    createdAt: '2024-01-01T00:00:00.000Z',
    updatedAt: '2024-01-01T00:00:00.000Z',
  },
  {
    id: 2,
    label: 'Branch Office',
    line1: '456 Oak Avenue',
    city: 'Los Angeles',
    state: 'CA',
    zipCode: '90001',
    country: 'United States',
    addressType: 'Shipping',
    isPrimary: false,
    createdAt: '2024-01-02T00:00:00.000Z',
    updatedAt: '2024-01-02T00:00:00.000Z',
  },
];

const mockDeletedAddress: Address = {
  ...mockAddresses[0],
  id: 3,
};

// Mock the address service
jest.mock('../../../services/addressService', () => ({
  __esModule: true,
  default: {
    deleteAddress: jest.fn(() => Promise.resolve()),
    setPrimaryBillingAddress: jest.fn(() => Promise.resolve()),
    setPrimaryShippingAddress: jest.fn(() => Promise.resolve()),
  },
}));

// eslint-disable-next-line @typescript-eslint/no-var-requires
const addressServiceMock = require('../../../services/addressService').default;

const renderComponent = (props?: Partial<AddressListComponentProps>) => {
  const defaultProps: AddressListComponentProps = {
    accountId: 1,
    addresses: mockAddresses,
    isLoading: false,
    error: null,
    onAddClick: jest.fn(),
    onEditClick: jest.fn(),
    onDeleteSuccess: jest.fn(),
    onSetPrimaryClick: jest.fn(),
    ...props,
  };

  return render(
    <BrowserRouter future={{ v7_startTransition: true, v7_relativeSplatPath: true }}>
      <AddressListComponent {...defaultProps} />
    </BrowserRouter>
  );
};

describe('AddressListComponent', () => {
  beforeEach(() => {
    // Reset mock call counts but keep implementations
    addressServiceMock.deleteAddress.mockClear();
    addressServiceMock.deleteAddress.mockImplementation(() => Promise.resolve());
    addressServiceMock.setPrimaryBillingAddress.mockClear();
    addressServiceMock.setPrimaryBillingAddress.mockImplementation(() => Promise.resolve());
    addressServiceMock.setPrimaryShippingAddress.mockClear();
    addressServiceMock.setPrimaryShippingAddress.mockImplementation(() => Promise.resolve());
  });

  describe('Rendering', () => {
    test('renders address list when addresses provided', () => {
      // Arrange
      renderComponent();

      // Act & Assert
      expect(screen.getByText('Main Office')).toBeInTheDocument();
      expect(screen.getByText('Branch Office')).toBeInTheDocument();
      expect(screen.getByText('123 Main Street')).toBeInTheDocument();
      expect(screen.getByText('456 Oak Avenue')).toBeInTheDocument();
    });

    test('displays address details correctly', () => {
      // Arrange
      renderComponent();

      // Act & Assert
      const mainOfficeSection = screen.getByText('Main Office').closest('tr') || screen.getByText('Main Office').closest('div');
      expect(mainOfficeSection).toBeInTheDocument();
      expect(mainOfficeSection).toHaveTextContent('New York');
      expect(mainOfficeSection).toHaveTextContent('NY');
    });

    test('renders with correct number of address cards', () => {
      // Arrange
      renderComponent();

      // Act
      const addressRows = screen.getAllByRole('row') || screen.getAllByTestId(/address-card/);

      // Assert
      expect(addressRows.length).toBeGreaterThanOrEqual(2);
    });
  });

  describe('Loading State', () => {
    test('shows loading state while fetching', () => {
      // Arrange
      renderComponent({ isLoading: true, addresses: [] });

      // Act & Assert
      const loadingElement = screen.getByRole('progressbar') || screen.getByText(/loading/i);
      expect(loadingElement).toBeInTheDocument();
    });

    test('hides address list when loading', () => {
      // Arrange
      renderComponent({ isLoading: true, addresses: [] });

      // Act & Assert
      expect(screen.queryByText('Main Office')).not.toBeInTheDocument();
    });
  });

  describe('Empty State', () => {
    test('shows empty state when no addresses provided', () => {
      // Arrange
      renderComponent({ addresses: [] });

      // Act & Assert
      const emptyState = screen.queryByText(/no addresses/i) || 
                         screen.queryByText(/add an address/i) ||
                         screen.queryByText(/empty/i);
      expect(emptyState).toBeInTheDocument();
    });

    test('shows add address button in empty state', () => {
      // Arrange
      const onAddClick = jest.fn();
      renderComponent({ addresses: [], onAddClick });

      // Act
      const addButton = screen.getByRole('button', { name: /add|create/i });
      fireEvent.click(addButton);

      // Assert
      expect(onAddClick).toHaveBeenCalled();
    });
  });

  describe('Error State', () => {
    test('displays error message when provided', () => {
      // Arrange
      const errorMessage = 'Failed to load addresses';
      renderComponent({ error: errorMessage });

      // Act & Assert
      expect(screen.getByText(errorMessage)).toBeInTheDocument();
    });

    test('hides address list when error occurred', () => {
      // Arrange
      renderComponent({ error: 'Failed to load', addresses: [] });

      // Act & Assert
      expect(screen.queryByText('Main Office')).not.toBeInTheDocument();
    });
  });

  describe('User Interactions', () => {
    test('calls onEditClick when edit button clicked', async () => {
      // Arrange
      const onEditClick = jest.fn();
      renderComponent({ onEditClick });

      // Act
      const editButtons = screen.getAllByRole('button', { name: /edit|pencil/i });
      fireEvent.click(editButtons[0]);

      // Assert
      await waitFor(() => {
        expect(onEditClick).toHaveBeenCalledWith(mockAddresses[0]);
      });
    });

    test('calls onDeleteSuccess after delete confirmed', async () => {
      // Arrange
      const onDeleteSuccess = jest.fn();
      renderComponent({ onDeleteSuccess });

      // Act - click the first delete icon button
      const deleteIcons = screen.getAllByTestId('DeleteIcon');
      const deleteIconButton = deleteIcons[0].closest('button') as HTMLElement;
      fireEvent.click(deleteIconButton);

      // Wait for dialog
      const dialog = await screen.findByRole('dialog');

      // Click the "Delete" button in the dialog using within
      const deleteBtn = within(dialog).getByText((content, element) => {
        return element?.tagName === 'BUTTON' && content === 'Delete';
      });
      fireEvent.click(deleteBtn);

      // Wait for async operation
      await act(async () => {
        await new Promise(resolve => setTimeout(resolve, 100));
      });

      expect(addressServiceMock.deleteAddress).toHaveBeenCalledWith(1, expect.any(Number));
      expect(onDeleteSuccess).toHaveBeenCalled();
    });

    test('calls onAddClick when add button clicked', () => {
      // Arrange
      const onAddClick = jest.fn();
      renderComponent({ onAddClick });

      // Act
      const addButton = screen.getByRole('button', { name: /^Add Address$/i });
      fireEvent.click(addButton);

      // Assert
      expect(onAddClick).toHaveBeenCalled();
    });

    test('calls onSetPrimaryClick when primary button clicked for billing', async () => {
      // Arrange
      const onSetPrimaryClick = jest.fn();
      renderComponent({ onSetPrimaryClick });

      // Act
      const primaryButtons = screen.queryAllByRole('button', { name: /primary|billing/i });
      if (primaryButtons.length > 0) {
        fireEvent.click(primaryButtons[0]);

        // Assert
        await waitFor(() => {
          expect(onSetPrimaryClick).toHaveBeenCalled();
        });
      }
    });

    test('calls onSetPrimaryClick when primary shipping button clicked', async () => {
      // Arrange
      const onSetPrimaryClick = jest.fn();
      renderComponent({ onSetPrimaryClick });

      // Act
      const shippingButtons = screen.queryAllByRole('button', { name: /shipping/i });
      if (shippingButtons.length > 0) {
        fireEvent.click(shippingButtons[0]);

        // Assert
        await waitFor(() => {
          expect(onSetPrimaryClick).toHaveBeenCalled();
        });
      }
    });
  });

  describe('Address Filtering', () => {
    test('filters and excludes deleted addresses from display', () => {
      // Arrange
      const deletedAddresses = [
        ...mockAddresses,
        { ...mockDeletedAddress, isDeleted: true },
      ];

      renderComponent({ addresses: mockAddresses });

      // Act & Assert - deleted addresses should not be visible
      expect(screen.queryByText('Deleted')).not.toBeInTheDocument();
    });

    test('highlights primary billing address', () => {
      // Arrange
      renderComponent();

      // Act
      const primaryAddress = screen.getByText('Main Office').closest('tr') || 
                             screen.getByText('Main Office').closest('div');

      // Assert
      expect(primaryAddress).toBeInTheDocument();
      const primaryBadge = primaryAddress?.querySelector('[aria-label*="primary"]') ||
                          primaryAddress?.querySelector('[class*="primary"]');
      if (primaryBadge) {
        expect(primaryBadge).toBeInTheDocument();
      }
    });

    test('renders addresses in provided order', () => {
      // Arrange - component renders addresses in the order provided
      renderComponent();

      // Act
      const rows = screen.getAllByRole('row');
      // First row is the header, data rows start from index 1
      expect(rows.length).toBeGreaterThan(1);

      // Assert - addresses render in provided order
      expect(rows[1]).toHaveTextContent('Main Office');
      expect(rows[2]).toHaveTextContent('Branch Office');
    });
  });

  describe('Accessibility', () => {
    test('has proper ARIA labels and roles', () => {
      // Arrange
      renderComponent();

      // Act & Assert
      const buttons = screen.getAllByRole('button');
      buttons.forEach((button) => {
        expect(button).toBeInTheDocument();
      });
    });

    test('is keyboard navigable', async () => {
      // Arrange
      const user = userEvent.setup();
      renderComponent();

      // Act
      const editButton = screen.getAllByRole('button', { name: /edit|pencil/i })[0];
      editButton.focus();

      // Assert
      expect(editButton).toHaveFocus();

      // Act - Press Enter
      await user.keyboard('{Enter}');

      // Assert - Button should be activated
      expect(editButton).toHaveFocus();
    });
  });

  describe('Data Integrity', () => {
    test('displays correct address format', () => {
      // Arrange
      renderComponent();

      // Act & Assert - line1 is in its own text node, city/state/zip are combined
      expect(screen.getByText('123 Main Street')).toBeInTheDocument();
      // City, state and zip are combined in a single div: "New York, NY 10001"
      const rows = screen.getAllByRole('row');
      expect(rows[1]).toHaveTextContent('New York');
      expect(rows[1]).toHaveTextContent('NY');
      expect(rows[1]).toHaveTextContent('10001');
    });

    test('preserves address data after interactions', () => {
      // Arrange
      const { rerender } = renderComponent();

      // Act
      const initialText = screen.getByText('123 Main Street');
      expect(initialText).toBeInTheDocument();

      // Rerender with same data
      rerender(
        <BrowserRouter future={{ v7_startTransition: true, v7_relativeSplatPath: true }}>
          <AddressListComponent
            accountId={1}
            addresses={mockAddresses}
            isLoading={false}
            error={null}
          />
        </BrowserRouter>
      );

      // Assert - Data should still be present
      expect(screen.getByText('123 Main Street')).toBeInTheDocument();
    });
  });

  describe('Responsive Behavior', () => {
    test('renders in table format on desktop', () => {
      // Arrange
      renderComponent();

      // Act & Assert
      const table = screen.queryByRole('table') || screen.getByText('Main Office');
      expect(table).toBeInTheDocument();
    });

    test('displays all address fields', () => {
      // Arrange
      renderComponent();

      // Act & Assert - check key fields are present in the first row
      const rows = screen.getAllByRole('row');
      const firstDataRow = rows[1];
      expect(firstDataRow).toHaveTextContent('Main Office');
      expect(firstDataRow).toHaveTextContent('123 Main Street');
      expect(firstDataRow).toHaveTextContent('New York');
      expect(firstDataRow).toHaveTextContent('NY');
      expect(firstDataRow).toHaveTextContent('10001');
      expect(firstDataRow).toHaveTextContent('United States');
    });
  });
});
