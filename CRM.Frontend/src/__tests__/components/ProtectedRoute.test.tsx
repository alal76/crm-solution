import React from 'react';
import { screen } from '@testing-library/react';
import ProtectedRoute from '../../components/ProtectedRoute';
import { renderWithProviders } from '../../test-utils/renderWithProviders';

const mockUseAuth = jest.fn();

jest.mock('../../contexts/AuthContext', () => ({
  useAuth: () => mockUseAuth(),
}));

describe('ProtectedRoute', () => {
  it('renders children when authenticated', () => {
    mockUseAuth.mockReturnValue({ isAuthenticated: true });

    renderWithProviders(
      <ProtectedRoute>
        <div>Secret</div>
      </ProtectedRoute>
    );

    expect(screen.getByText('Secret')).toBeInTheDocument();
  });

  it('does not render children when unauthenticated', () => {
    mockUseAuth.mockReturnValue({ isAuthenticated: false });

    renderWithProviders(
      <ProtectedRoute>
        <div>Secret</div>
      </ProtectedRoute>
    );

    expect(screen.queryByText('Secret')).not.toBeInTheDocument();
  });
});
