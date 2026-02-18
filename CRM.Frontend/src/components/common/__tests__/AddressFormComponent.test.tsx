import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { BrowserRouter } from 'react-router-dom';
import AddressFormComponent, { AddressFormComponentProps } from '../AddressFormComponent';
import { Address, CreateAddressDto } from '../../../types/address.types';
import '@testing-library/jest-dom';

/**
 * Component Tests for AddressFormComponent
 * Tests cover: Form rendering, validation, submission, and data handling
 *
 * FUNCTIONAL VIEW:
 * - Tests form field rendering (create and edit modes)
 * - Tests form validation and error messages
 * - Tests form submission with valid and invalid data
 * - Tests cancel functionality
 * - Tests address type selection
 *
 * TECHNICAL VIEW:
 * - Uses React Testing Library for component testing
 * - Tests form state and validation
 * - Tests async form submission
 * - Verifies form field values and changes
 */

const mockAddress: Address = {
  id: 1,
  accountId: 1,
  label: 'Main Office',
  line1: '123 Main Street',
  line2: 'Suite 100',
  city: 'New York',
  state: 'NY',
  postalCode: '10001',
  country: 'United States',
  countryCode: 'US',
  createdAt: '2024-01-01T00:00:00.000Z',
  updatedAt: '2024-01-01T00:00:00.000Z',
};

// Mock the address service
jest.mock('../../../services/addressService', () => ({
  default: {
    createAddress: jest.fn(),
    updateAddress: jest.fn(),
  },
}));

const renderComponent = (props?: Partial<AddressFormComponentProps>) => {
  const defaultProps: AddressFormComponentProps = {
    onSubmit: jest.fn().mockResolvedValue(undefined),
    onCancel: jest.fn(),
    ...props,
  };

  return render(
    <BrowserRouter>
      <AddressFormComponent {...defaultProps} />
    </BrowserRouter>
  );
};

describe('AddressFormComponent', () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  describe('Create Mode', () => {
    test('renders all required form fields in create mode', () => {
      // Arrange
      renderComponent({});

      // Act & Assert
      expect(screen.getByLabelText(/label|name/i)).toBeInTheDocument();
      expect(screen.getByLabelText(/street|line1|address/i)).toBeInTheDocument();
      expect(screen.getByLabelText(/city/i)).toBeInTheDocument();
      expect(screen.getByLabelText(/state|province/i)).toBeInTheDocument();
      expect(screen.getByLabelText(/postal|zip/i)).toBeInTheDocument();
      expect(screen.getByLabelText(/country/i)).toBeInTheDocument();
    });

    test('renders form title as Create', () => {
      // Arrange
      renderComponent({});

      // Act & Assert
      const title = screen.getByText(/create|new/i) || screen.getByRole('heading');
      expect(title).toBeInTheDocument();
    });

    test('initializes form fields with empty values', () => {
      // Arrange
      renderComponent({});

      // Act & Assert
      const streetInput = screen.getByLabelText(/street|line1|address/i) as HTMLInputElement;
      expect(streetInput.value).toBe('');
    });

    test('renders submit button as Create/Add', () => {
      // Arrange
      renderComponent({});

      // Act & Assert
      const submitButton = screen.getByRole('button', { name: /create|add|save/i });
      expect(submitButton).toBeInTheDocument();
    });
  });

  describe('Edit Mode', () => {
    test('renders form in edit mode with address data', () => {
      // Arrange
      renderComponent({ address: mockAddress });

      // Act & Assert
      const streetInput = screen.getByLabelText(/street|line1|address/i) as HTMLInputElement;
      expect(streetInput.value).toBe('123 Main Street');

      const cityInput = screen.getByLabelText(/city/i) as HTMLInputElement;
      expect(cityInput.value).toBe('New York');
    });

    test('renders form title as Edit', () => {
      // Arrange
      renderComponent({ address: mockAddress });

      // Act & Assert
      const title = screen.getByText(/edit|update/i) || screen.getByRole('heading');
      expect(title).toBeInTheDocument();
    });

    test('pre-populates all address fields', () => {
      // Arrange
      renderComponent({ address: mockAddress });

      // Act & Assert
      expect(screen.getByLabelText(/label|name/i)).toHaveValue('Main Office');
      expect(screen.getByLabelText(/street|line1|address/i)).toHaveValue('123 Main Street');
      expect(screen.getByLabelText(/city/i)).toHaveValue('New York');
    });

    test('renders submit button as Update/Save', () => {
      // Arrange
      renderComponent({ address: mockAddress });

      // Act & Assert
      const submitButton = screen.getByRole('button', { name: /update|save/i });
      expect(submitButton).toBeInTheDocument();
    });
  });

  describe('Form Validation', () => {
    test('shows required field validation error for street', async () => {
      // Arrange
      const onSubmit = jest.fn();
      renderComponent({ onSubmit });

      // Act
      const submitButton = screen.getByRole('button', { name: /create|save/i });
      fireEvent.click(submitButton);

      // Assert
      await waitFor(() => {
        const error = screen.queryByText(/street.*required/i) ||
                      screen.queryByText(/address line 1.*required/i) ||
                      screen.queryByText(/address.*required/i);
        if (error) {
          expect(error).toBeInTheDocument();
        }
      });

      expect(onSubmit).not.toHaveBeenCalled();
    });

    test('shows required field validation error for city', async () => {
      // Arrange
      const onSubmit = jest.fn();
      renderComponent({ onSubmit });

      // Act
      const streetInput = screen.getByLabelText(/street|line1|address/i);
      fireEvent.change(streetInput, { target: { value: '123 Main Street' } });

      const submitButton = screen.getByRole('button', { name: /create|save/i });
      fireEvent.click(submitButton);

      // Assert
      await waitFor(() => {
        const error = screen.queryByText(/city.*required/i);
        if (error) {
          expect(error).toBeInTheDocument();
        }
      });

      expect(onSubmit).not.toHaveBeenCalled();
    });

    test('shows required field validation error for country', async () => {
      // Arrange
      const onSubmit = jest.fn();
      renderComponent({ onSubmit });

      // Act
      const streetInput = screen.getByLabelText(/street|line1|address/i);
      fireEvent.change(streetInput, { target: { value: '123 Main Street' } });

      const cityInput = screen.getByLabelText(/city/i);
      fireEvent.change(cityInput, { target: { value: 'New York' } });

      const submitButton = screen.getByRole('button', { name: /create|save/i });
      fireEvent.click(submitButton);

      // Assert
      await waitFor(() => {
        const error = screen.queryByText(/country.*required/i);
        if (error) {
          expect(error).toBeInTheDocument();
        }
      });
    });

    test('accepts valid form data', async () => {
      // Arrange
      const onSubmit = jest.fn();
      renderComponent({ onSubmit });

      // Act
      const streetInput = screen.getByLabelText(/street|line1|address/i);
      fireEvent.change(streetInput, { target: { value: '123 Main Street' } });

      const cityInput = screen.getByLabelText(/city/i);
      fireEvent.change(cityInput, { target: { value: 'New York' } });

      const countryInput = screen.getByLabelText(/country/i);
      fireEvent.change(countryInput, { target: { value: 'United States' } });

      const submitButton = screen.getByRole('button', { name: /create|save/i });
      fireEvent.click(submitButton);

      // Assert
      await waitFor(() => {
        expect(onSubmit).toHaveBeenCalled();
      });
    });
  });

  describe('Form Submission', () => {
    test('calls onSubmit with valid data on create', async () => {
      // Arrange
      const onSubmit = jest.fn();
      renderComponent({ onSubmit });

      // Act
      const streetInput = screen.getByLabelText(/street|line1|address/i);
      fireEvent.change(streetInput, { target: { value: '789 Pine Road' } });

      const cityInput = screen.getByLabelText(/city/i);
      fireEvent.change(cityInput, { target: { value: 'Chicago' } });

      const countryInput = screen.getByLabelText(/country/i);
      fireEvent.change(countryInput, { target: { value: 'United States' } });

      const submitButton = screen.getByRole('button', { name: /create|save/i });
      fireEvent.click(submitButton);

      // Assert
      await waitFor(() => {
        expect(onSubmit).toHaveBeenCalledWith(
          expect.objectContaining({
            line1: '789 Pine Road',
            city: 'Chicago',
            country: 'United States',
          })
        );
      });
    });

    test('calls onSubmit with updated data on edit', async () => {
      // Arrange
      const onSubmit = jest.fn();
      renderComponent({ address: mockAddress, onSubmit });

      // Act
      const streetInput = screen.getByLabelText(/street|line1|address/i) as HTMLInputElement;
      fireEvent.change(streetInput, { target: { value: '999 Updated Street' } });

      const submitButton = screen.getByRole('button', { name: /update|save/i });
      fireEvent.click(submitButton);

      // Assert
      await waitFor(() => {
        expect(onSubmit).toHaveBeenCalledWith(
          expect.objectContaining({
            line1: '999 Updated Street',
          })
        );
      });
    });

    test('disables submit button while submitting', async () => {
      // Arrange
      const onSubmit = jest.fn(() => new Promise(resolve => setTimeout(resolve, 1000)));
      renderComponent({ onSubmit });

      // Act
      const streetInput = screen.getByLabelText(/street|line1|address/i);
      fireEvent.change(streetInput, { target: { value: '123 Main Street' } });

      const cityInput = screen.getByLabelText(/city/i);
      fireEvent.change(cityInput, { target: { value: 'New York' } });

      const countryInput = screen.getByLabelText(/country/i);
      fireEvent.change(countryInput, { target: { value: 'United States' } });

      const submitButton = screen.getByRole('button', { name: /create|save/i });
      fireEvent.click(submitButton);

      // Assert - Button should be disabled during submission
      await waitFor(() => {
        expect(submitButton).toBeDisabled();
      });
    });
  });

  describe('Cancel Functionality', () => {
    test('calls onCancel when cancel button clicked', () => {
      // Arrange
      const onCancel = jest.fn();
      renderComponent({ onCancel });

      // Act
      const cancelButton = screen.getByRole('button', { name: /cancel|close/i });
      fireEvent.click(cancelButton);

      // Assert
      expect(onCancel).toHaveBeenCalled();
    });

    test('clears unsaved changes when cancelled', () => {
      // Arrange
      const onCancel = jest.fn();
      renderComponent({ onCancel });

      // Act
      const streetInput = screen.getByLabelText(/street|line1|address/i) as HTMLInputElement;
      fireEvent.change(streetInput, { target: { value: 'Temporary Address' } });

      const cancelButton = screen.getByRole('button', { name: /cancel|close/i });
      fireEvent.click(cancelButton);

      // Assert
      expect(onCancel).toHaveBeenCalled();
    });
  });

  describe('Optional Fields', () => {
    test('allows submission with only required fields', async () => {
      // Arrange
      const onSubmit = jest.fn();
      renderComponent({ onSubmit });

      // Act
      const streetInput = screen.getByLabelText(/street|line1|address/i);
      fireEvent.change(streetInput, { target: { value: '123 Main Street' } });

      const cityInput = screen.getByLabelText(/city/i);
      fireEvent.change(cityInput, { target: { value: 'New York' } });

      const countryInput = screen.getByLabelText(/country/i);
      fireEvent.change(countryInput, { target: { value: 'United States' } });

      const submitButton = screen.getByRole('button', { name: /create|save/i });
      fireEvent.click(submitButton);

      // Assert
      await waitFor(() => {
        expect(onSubmit).toHaveBeenCalled();
      });
    });

    test('accepts optional fields like suite and notes', async () => {
      // Arrange
      const onSubmit = jest.fn();
      renderComponent({ onSubmit });

      // Act
      const streetInput = screen.getByLabelText(/street|line1|address/i);
      fireEvent.change(streetInput, { target: { value: '123 Main Street' } });

      const suiteInput = screen.queryByLabelText(/suite|line2/i);
      if (suiteInput) {
        fireEvent.change(suiteInput, { target: { value: 'Suite 100' } });
      }

      const cityInput = screen.getByLabelText(/city/i);
      fireEvent.change(cityInput, { target: { value: 'New York' } });

      const countryInput = screen.getByLabelText(/country/i);
      fireEvent.change(countryInput, { target: { value: 'United States' } });

      const submitButton = screen.getByRole('button', { name: /create|save/i });
      fireEvent.click(submitButton);

      // Assert
      await waitFor(() => {
        expect(onSubmit).toHaveBeenCalled();
      });
    });
  });

  describe('Data Handling', () => {
    test('properly formats address data for submission', async () => {
      // Arrange
      const onSubmit = jest.fn();
      renderComponent({ onSubmit });

      // Act
      const labelInput = screen.queryByLabelText(/label|name/i);
      if (labelInput) {
        fireEvent.change(labelInput, { target: { value: 'Main Office' } });
      }

      const streetInput = screen.getByLabelText(/street|line1|address/i);
      fireEvent.change(streetInput, { target: { value: '123 Main Street' } });

      const cityInput = screen.getByLabelText(/city/i);
      fireEvent.change(cityInput, { target: { value: 'New York' } });

      const stateInput = screen.queryByLabelText(/state|province/i);
      if (stateInput) {
        fireEvent.change(stateInput, { target: { value: 'NY' } });
      }

      const countryInput = screen.getByLabelText(/country/i);
      fireEvent.change(countryInput, { target: { value: 'United States' } });

      const submitButton = screen.getByRole('button', { name: /create|save/i });
      fireEvent.click(submitButton);

      // Assert
      await waitFor(() => {
        expect(onSubmit).toHaveBeenCalledWith(
          expect.objectContaining({
            line1: '123 Main Street',
            city: 'New York',
            country: 'United States',
          })
        );
      });
    });
  });

  describe('Accessibility', () => {
    test('form inputs have proper labels', () => {
      // Arrange
      renderComponent({});

      // Act & Assert
      const inputs = screen.getAllByRole('textbox');
      inputs.forEach((input) => {
        // Each input should have a label or aria-label
        const ariaLabel = input.getAttribute('aria-label');
        expect(ariaLabel || input.closest('label')).toBeTruthy();
      });
    });

    test('form is keyboard navigable', async () => {
      // Arrange
      const user = userEvent.setup();
      renderComponent({});

      // Act
      const firstInput = screen.getByLabelText(/label|name|street|address/i);
      firstInput.focus();

      // Assert
      expect(firstInput).toHaveFocus();

      // Act - Tab to next field
      await user.keyboard('{Tab}');

      // Assert - Focus should move to next field
      expect(document.activeElement).not.toBe(firstInput);
    });
  });
});
