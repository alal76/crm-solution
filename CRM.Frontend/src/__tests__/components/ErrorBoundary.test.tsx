import React from 'react';
import { screen } from '@testing-library/react';
import ErrorBoundary from '../../components/ErrorBoundary';
import { renderWithProviders } from '../../test-utils/renderWithProviders';

describe('ErrorBoundary', () => {
  const originalError = console.error;

  beforeEach(() => {
    console.error = jest.fn();
  });

  afterEach(() => {
    console.error = originalError;
  });

  it('renders fallback UI when a child throws', () => {
    const Thrower = () => {
      throw new Error('Boom');
    };

    renderWithProviders(
      <ErrorBoundary>
        <Thrower />
      </ErrorBoundary>
    );

    expect(screen.getByText(/something went wrong/i)).toBeInTheDocument();
  });
});
