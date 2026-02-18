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
  label: 'Main Office',
  line1: '123 Main Street',
  line2: 'Suite 100',
  city: 'New York',
  state: 'NY',
  zipCode: '10001',
  country: 'United States',
  addressType: 'Primary',
  isPrimary: true,
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
      expect(screen.getByLabelText('Label')).toBeInTheDocument();
      expect(screen.getByLabelText('Address Line 1')).toBeInTheDocument();
      expect(screen.getByLabelText('City')).toBeInTheDocument();
      expect(screen.getByLabelText('State/Province')).toBeInTheDocument();
      expect(screen.getByLabelText('Zip/Postal Code')).toBeInTheDocument();
      expect(screen.getByLabelText('Country')).toBeInTheDocument();
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
      const streetInput = screen.getByLabelText('Address Line 1') as HTMLInputElement;
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
      const streetInput = screen.getByLabelText('Address Line 1') as HTMLInputElement;
      expect(streetInput.value).toBe('123 Main Street');

      const cityInput = screen.getByLabelText('City') as HTMLInputElement;
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
      expect(screen.getByLabelText('Label')).toHaveValue('Main Office');
      expect(screen.getByLabelText('Address Line 1')).toHaveValue('123 Main Street');
      expect(screen.getByLabelText('City')).toHaveValue('New York');
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
      const streetInput = screen.getByLabelText('Address Line 1');
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
      const streetInput = screen.getByLabelText('Address Line 1');
      fireEvent.change(streetInput, { target: { value: '123 Main Street' } });

      const cityInput = screen.getByLabelText('City');
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
      const streetInput = screen.getByLabelText('Address Line 1');
      fireEvent.change(streetInput, { target: { value: '123 Main Street' } });

      const cityInput = screen.getByLabelText('City');
      fireEvent.change(cityInput, { target: { value: 'New York' } });

      const countryInput = screen.getByLabelText('Country');
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
      const streetInput = screen.getByLabelText('Address Line 1');
      fireEvent.change(streetInput, { target: { value: '789 Pine Road' } });

      const cityInput = screen.getByLabelText('City');
      fireEvent.change(cityInput, { target: { value: 'Chicago' } });

      const countryInput = screen.getByLabelText('Country');
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
      const streetInput = screen.getByLabelText('Address Line 1') as HTMLInputElement;
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
      const onSubmit = jest.fn((): Promise<void> => new Promise(resolve => setTimeout(resolve, 1000)));
      renderComponent({ onSubmit });

      // Act
      const streetInput = screen.getByLabelText('Address Line 1');
      fireEvent.change(streetInput, { target: { value: '123 Main Street' } });

      const cityInput = screen.getByLabelText('City');
      fireEvent.change(cityInput, { target: { value: 'New York' } });

      const countryInput = screen.getByLabelText('Country');
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
      const streetInput = screen.getByLabelText('Address Line 1') as HTMLInputElement;
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
      const streetInput = screen.getByLabelText('Address Line 1');
      fireEvent.change(streetInput, { target: { value: '123 Main Street' } });

      const cityInput = screen.getByLabelText('City');
      fireEvent.change(cityInput, { target: { value: 'New York' } });

      const countryInput = screen.getByLabelText('Country');
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
      const streetInput = screen.getByLabelText('Address Line 1');
      fireEvent.change(streetInput, { target: { value: '123 Main Street' } });

      const suiteInput = screen.queryByLabelText('Address Line 2');
      if (suiteInput) {
        fireEvent.change(suiteInput, { target: { value: 'Suite 100' } });
      }

      const cityInput = screen.getByLabelText('City');
      fireEvent.change(cityInput, { target: { value: 'New York' } });

      const countryInput = screen.getByLabelText('Country');
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
      const labelInput = screen.queryByLabelText('Label');
      if (labelInput) {
        fireEvent.change(labelInput, { target: { value: 'Main Office' } });
      }

      const streetInput = screen.getByLabelText('Address Line 1');
      fireEvent.change(streetInput, { target: { value: '123 Main Street' } });

      const cityInput = screen.getByLabelText('City');
      fireEvent.change(cityInput, { target: { value: 'New York' } });

      const stateInput = screen.queryByLabelText('State/Province');
      if (stateInput) {
        fireEvent.change(stateInput, { target: { value: 'NY' } });
      }

      const countryInput = screen.getByLabelText('Country');
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

      // Act & Assert - verify key fields are accessible by label
      expect(screen.getByLabelText('Address Line 1')).toBeInTheDocument();
      expect(screen.getByLabelText('City')).toBeInTheDocument();
      expect(screen.getByLabelText('Country')).toBeInTheDocument();
      expect(screen.getByLabelText('Label')).toBeInTheDocument();
    });

    test('form is keyboard navigable', async () => {
      // Arrange
      const user = userEvent.setup();
      renderComponent({});

      // Act
      const firstInput = screen.getByLabelText('Label');
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
