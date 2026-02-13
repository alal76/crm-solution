import React from 'react';
import { screen } from '@testing-library/react';
import LoadingSpinner from '../../components/common/LoadingSpinner';
import { renderWithProviders } from '../../test-utils/renderWithProviders';

describe('LoadingSpinner', () => {
  it('renders the message when provided', () => {
    renderWithProviders(<LoadingSpinner message="Loading data" />);

    expect(screen.getByText('Loading data')).toBeInTheDocument();
  });

  it('renders full-page overlay when enabled', () => {
    renderWithProviders(<LoadingSpinner message="Please wait" fullPage />);

    expect(screen.getByText('Please wait')).toBeInTheDocument();
  });
});
