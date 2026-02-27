/* jest.setup.js - Jest setup for React Scripts */
import '@testing-library/jest-dom';

// Mock window.matchMedia
Object.defineProperty(window, 'matchMedia', {
  writable: true,
  value: jest.fn().mockImplementation(query => ({
    matches: false,
    media: query,
    onchange: null,
    addListener: jest.fn(),
    removeListener: jest.fn(),
    addEventListener: jest.fn(),
    removeEventListener: jest.fn(),
    dispatchEvent: jest.fn(),
  })),
});

// Mock localStorage
const localStorageMock = {
  getItem: jest.fn(),
  setItem: jest.fn(),
  removeItem: jest.fn(),
  clear: jest.fn(),
};
global.localStorage = localStorageMock as any;

// Suppress console errors/warnings in tests
const originalError = console.error;
const originalWarn = console.warn;

const shouldSuppressMessage = (args: any[]) => {
  if (typeof args[0] !== 'string') return false;
  const msg = args[0];
  return (
    msg.includes('Warning: ReactDOM.render') ||
    msg.includes('not wrapped in act(') ||
    msg.includes('⚠️ React Router Future Flag Warning')
  );
};

beforeAll(() => {
  console.error = (...args: any[]) => {
    if (shouldSuppressMessage(args)) return;
    originalError.call(console, ...args);
  };
  console.warn = (...args: any[]) => {
    if (shouldSuppressMessage(args)) return;
    originalWarn.call(console, ...args);
  };
});

afterAll(() => {
  console.error = originalError;
  console.warn = originalWarn;
});
