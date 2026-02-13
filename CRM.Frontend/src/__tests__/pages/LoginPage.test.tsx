import React from 'react';
import { screen } from '@testing-library/react';
import LoginPage from '../../pages/LoginPage';
import { renderWithProviders } from '../../test-utils/renderWithProviders';

const mockLogin = jest.fn();
const mockVerifyTwoFactor = jest.fn();
const mockGoogleLogin = jest.fn();

jest.mock('../../contexts/AuthContext', () => ({
  useAuth: () => ({
    login: mockLogin,
    verifyTwoFactor: mockVerifyTwoFactor,
    googleLogin: mockGoogleLogin,
  }),
}));

jest.mock('@mui/material', () => {
  const actual = jest.requireActual('@mui/material');
  return {
    ...actual,
    useMediaQuery: () => true,
  };
});

describe('LoginPage', () => {
  beforeEach(() => {
    global.fetch = jest.fn().mockResolvedValue({
      ok: true,
      json: async () => ({ quickAdminLoginEnabled: false }),
    }) as unknown as typeof fetch;
  });

  it('renders login form fields and submit button', () => {
    renderWithProviders(<LoginPage />);

    expect(screen.getByLabelText(/email/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/password/i, { selector: 'input' })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /sign in/i })).toBeInTheDocument();
  });
});
