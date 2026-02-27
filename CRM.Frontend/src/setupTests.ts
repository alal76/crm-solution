// jest-setup.js - Jest configuration and setup

// Suppress console errors/warnings during tests - IMMEDIATELY on module load
const originalError = console.error;
const originalWarn = console.warn;

const shouldSuppressMessage = (args: unknown[]): boolean => {
  if (typeof args[0] !== 'string') return false;
  const msg = args[0];
  return (
    msg.includes('Warning: ReactDOM.render') ||
    msg.includes('not wrapped in act(') ||
    msg.includes('React Router Future Flag Warning') ||
    msg.includes('unique "key" prop') ||
    msg.includes('Failed prop type') ||
    msg.includes('Function components cannot be given refs') ||
    msg.includes('Failed to load dynamic navigation config')
  );
};

console.error = (...args: unknown[]) => {
  if (shouldSuppressMessage(args)) return;
  originalError.call(console, ...args);
};
console.warn = (...args: unknown[]) => {
  if (shouldSuppressMessage(args)) return;
  originalWarn.call(console, ...args);
};

// Add custom matchers from testing library
import '@testing-library/jest-dom';

// Import MSW server for API mocking
import { server } from './mocks/server';
import { resetAllFactories } from './mocks/factories';

// Establish API mocking before all tests
beforeAll(() => server.listen({ onUnhandledRequest: 'bypass' }));

// Reset handlers and factories after each test
afterEach(() => {
  server.resetHandlers();
  resetAllFactories();
});

// Clean up after all tests - restore console
afterAll(() => {
  server.close();
  console.error = originalError;
  console.warn = originalWarn;
});

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
  length: 0,
  key: jest.fn(),
};
global.localStorage = localStorageMock as unknown as Storage;
