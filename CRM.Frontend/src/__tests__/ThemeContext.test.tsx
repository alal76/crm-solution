/**
 * ThemeContext Tests — task TODO-UX-11
 * Tests dark mode toggle, persistence, and system preference detection.
 */
import React from 'react';
import { render, act, screen } from '@testing-library/react';
import { AppThemeProvider, useTheme } from '../contexts/ThemeContext';

// ─── Helper component that reads theme context ───────────────────────────────

const ThemeConsumer: React.FC = () => {
  const { themeMode, effectiveTheme, setThemeMode } = useTheme();
  return (
    <div>
      <span data-testid="themeMode">{themeMode}</span>
      <span data-testid="effectiveTheme">{effectiveTheme}</span>
      <button data-testid="setLight" onClick={() => setThemeMode('light')}>
        Light
      </button>
      <button data-testid="setDark" onClick={() => setThemeMode('dark')}>
        Dark
      </button>
      <button data-testid="setSystem" onClick={() => setThemeMode('system')}>
        System
      </button>
    </div>
  );
};

// ─── Tests ───────────────────────────────────────────────────────────────────

describe('AppThemeProvider / useTheme', () => {
  beforeEach(() => {
    localStorage.clear();
    // Reset matchMedia mock
    Object.defineProperty(window, 'matchMedia', {
      writable: true,
      value: jest.fn().mockImplementation((query: string) => ({
        matches: query === '(prefers-color-scheme: dark)' ? false : false,
        media: query,
        onchange: null,
        addListener: jest.fn(),
        removeListener: jest.fn(),
        addEventListener: jest.fn(),
        removeEventListener: jest.fn(),
        dispatchEvent: jest.fn(),
      })),
    });
  });

  it('defaults to "system" theme when no localStorage key is set', () => {
    render(
      <AppThemeProvider>
        <ThemeConsumer />
      </AppThemeProvider>
    );
    expect(screen.getByTestId('themeMode').textContent).toBe('system');
  });

  it('reads saved theme from localStorage on mount', () => {
    localStorage.setItem('crm_theme_preference', 'dark');
    render(
      <AppThemeProvider>
        <ThemeConsumer />
      </AppThemeProvider>
    );
    expect(screen.getByTestId('themeMode').textContent).toBe('dark');
  });

  it('persists theme mode to localStorage when setThemeMode is called', () => {
    render(
      <AppThemeProvider>
        <ThemeConsumer />
      </AppThemeProvider>
    );

    act(() => {
      screen.getByTestId('setDark').click();
    });

    expect(localStorage.getItem('crm_theme_preference')).toBe('dark');
    expect(screen.getByTestId('themeMode').textContent).toBe('dark');
  });

  it('toggles from dark back to light and persists', () => {
    localStorage.setItem('crm_theme_preference', 'dark');
    render(
      <AppThemeProvider>
        <ThemeConsumer />
      </AppThemeProvider>
    );

    act(() => {
      screen.getByTestId('setLight').click();
    });

    expect(localStorage.getItem('crm_theme_preference')).toBe('light');
    expect(screen.getByTestId('themeMode').textContent).toBe('light');
    expect(screen.getByTestId('effectiveTheme').textContent).toBe('light');
  });

  it('resolves "system" to the OS light preference', () => {
    // Mock system: light
    Object.defineProperty(window, 'matchMedia', {
      writable: true,
      value: jest.fn().mockImplementation((query: string) => ({
        matches: false, // dark not preferred → light
        media: query,
        onchange: null,
        addListener: jest.fn(),
        removeListener: jest.fn(),
        addEventListener: jest.fn(),
        removeEventListener: jest.fn(),
        dispatchEvent: jest.fn(),
      })),
    });

    render(
      <AppThemeProvider>
        <ThemeConsumer />
      </AppThemeProvider>
    );

    act(() => {
      screen.getByTestId('setSystem').click();
    });

    expect(screen.getByTestId('themeMode').textContent).toBe('system');
    expect(screen.getByTestId('effectiveTheme').textContent).toBe('light');
  });

  it('throws when useTheme is used outside AppThemeProvider', () => {
    const consoleError = jest.spyOn(console, 'error').mockImplementation(() => {});
    expect(() => render(<ThemeConsumer />)).toThrow(
      'useTheme must be used within an AppThemeProvider'
    );
    consoleError.mockRestore();
  });
});
