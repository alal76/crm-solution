/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 *
 * Test Utilities - Shared mocks, providers, and test data
 */

import React, { ReactElement, ReactNode } from 'react';
import { render, RenderOptions } from '@testing-library/react';
import { BrowserRouter, MemoryRouter } from 'react-router-dom';
import { ThemeProvider, createTheme } from '@mui/material/styles';
import '@testing-library/jest-dom';

// ============================================================================
// Mock Theme
// ============================================================================
const mockTheme = createTheme({
  palette: {
    mode: 'light',
    primary: {
      main: '#6750A4',
    },
    secondary: {
      main: '#625B71',
    },
  },
});

// ============================================================================
// Mock Auth Context
// ============================================================================
export const mockUser = {
  userId: 1,
  email: 'abhi.lal@gmail.com',
  firstName: 'Abhishek',
  lastName: 'Lal',
  role: 'Admin',
  username: 'admin',
};

export const mockAuthContext = {
  isAuthenticated: true,
  user: mockUser,
  login: jest.fn().mockResolvedValue({ success: true }),
  logout: jest.fn(),
  verifyTwoFactor: jest.fn().mockResolvedValue({ success: true }),
  googleLogin: jest.fn().mockResolvedValue({ success: true }),
  refreshToken: jest.fn().mockResolvedValue('new-token'),
  loading: false,
};

export const mockUnauthenticatedContext = {
  ...mockAuthContext,
  isAuthenticated: false,
  user: null,
};

export const AuthContext = React.createContext(mockAuthContext);

// ============================================================================
// Mock Profile Context
// ============================================================================
export const mockProfile = {
  id: 1,
  email: 'abhi.lal@gmail.com',
  firstName: 'Abhishek',
  lastName: 'Lal',
  role: 'Admin',
  permissions: ['view_customers', 'edit_customers', 'delete_customers', 'view_opportunities', 'edit_opportunities'],
  menuAccess: ['Dashboard', 'Customers', 'Contacts', 'Opportunities', 'Products', 'Campaigns', 'Settings'],
};

export const mockProfileContext = {
  profile: mockProfile,
  hasPermission: jest.fn().mockReturnValue(true),
  canAccessMenu: jest.fn().mockReturnValue(true),
  loading: false,
  refreshProfile: jest.fn(),
};

export const ProfileContext = React.createContext(mockProfileContext);

// ============================================================================
// Mock Layout Context
// ============================================================================
export const mockLayoutContext = {
  sidebarOpen: true,
  toggleSidebar: jest.fn(),
  layoutMode: 'standard',
  setLayoutMode: jest.fn(),
};

export const LayoutContext = React.createContext(mockLayoutContext);

// ============================================================================
// Mock Branding Context
// ============================================================================
export const mockBrandingContext = {
  branding: {
    logoUrl: '/logo.png',
    companyName: 'CRM Solution',
    primaryColor: '#6750A4',
    secondaryColor: '#625B71',
  },
  loading: false,
  refreshBranding: jest.fn(),
};

export const BrandingContext = React.createContext(mockBrandingContext);

// ============================================================================
// Mock Test Data
// ============================================================================
export const mockCustomers = [
  {
    id: 1,
    firstName: 'John',
    lastName: 'Doe',
    company: 'Acme Corp',
    email: 'john@acme.com',
    phone: '555-0101',
    city: 'New York',
    state: 'NY',
    customerType: 0,
    lifecycleStage: 1,
    priority: 2,
    annualRevenue: 1000000,
    createdAt: '2024-01-15T10:30:00Z',
    modifiedAt: '2024-01-20T14:45:00Z',
  },
  {
    id: 2,
    firstName: 'Jane',
    lastName: 'Smith',
    company: 'TechStart Inc',
    email: 'jane@techstart.com',
    phone: '555-0102',
    city: 'San Francisco',
    state: 'CA',
    customerType: 1,
    lifecycleStage: 2,
    priority: 1,
    annualRevenue: 5000000,
    createdAt: '2024-02-10T09:00:00Z',
    modifiedAt: '2024-02-15T11:30:00Z',
  },
];

export const mockContacts = [
  {
    id: 1,
    firstName: 'Alice',
    lastName: 'Johnson',
    email: 'alice@example.com',
    phone: '555-0201',
    contactType: 'Customer',
    jobTitle: 'CEO',
    company: 'Alpha Inc',
    department: 'Executive',
    createdAt: '2024-01-10T08:00:00Z',
  },
  {
    id: 2,
    firstName: 'Bob',
    lastName: 'Wilson',
    email: 'bob@example.com',
    phone: '555-0202',
    contactType: 'Partner',
    jobTitle: 'CTO',
    company: 'Beta LLC',
    department: 'Technology',
    createdAt: '2024-01-12T10:00:00Z',
  },
];

export const mockOpportunities = [
  {
    id: 1,
    name: 'Enterprise Deal',
    amount: 50000,
    stage: 2,
    probability: 60,
    customerId: 1,
    customerName: 'Acme Corp',
    expectedCloseDate: '2024-06-30',
    createdAt: '2024-01-15T10:00:00Z',
  },
  {
    id: 2,
    name: 'Startup Package',
    amount: 15000,
    stage: 1,
    probability: 40,
    customerId: 2,
    customerName: 'TechStart Inc',
    expectedCloseDate: '2024-05-15',
    createdAt: '2024-02-01T09:00:00Z',
  },
];

export const mockProducts = [
  {
    id: 1,
    name: 'Enterprise License',
    sku: 'ENT-001',
    price: 10000,
    category: 'Software',
    isActive: true,
    stockQuantity: 100,
    createdAt: '2024-01-01T00:00:00Z',
  },
  {
    id: 2,
    name: 'Basic License',
    sku: 'BAS-001',
    price: 1000,
    category: 'Software',
    isActive: true,
    stockQuantity: 500,
    createdAt: '2024-01-01T00:00:00Z',
  },
];

export const mockCampaigns = [
  {
    id: 1,
    name: 'Q1 Marketing Push',
    type: 'Email',
    status: 'Active',
    budget: 50000,
    startDate: '2024-01-01',
    endDate: '2024-03-31',
    createdAt: '2023-12-15T10:00:00Z',
  },
  {
    id: 2,
    name: 'Product Launch',
    type: 'Multi-Channel',
    status: 'Planning',
    budget: 100000,
    startDate: '2024-04-01',
    endDate: '2024-06-30',
    createdAt: '2024-01-20T09:00:00Z',
  },
];

export const mockLeads = [
  {
    id: 1,
    firstName: 'Mike',
    lastName: 'Brown',
    email: 'mike@prospect.com',
    company: 'Prospect Corp',
    source: 'Website',
    status: 'New',
    score: 75,
    createdAt: '2024-02-01T10:00:00Z',
  },
];

export const mockDashboardStats = {
  totalCustomers: 150,
  totalOpportunities: 45,
  totalRevenue: 2500000,
  openLeads: 25,
  activeDeals: 18,
  closedWonThisMonth: 5,
  closedLostThisMonth: 2,
  conversionRate: 0.72,
};

// ============================================================================
// Mock API Client
// ============================================================================
export const mockApiClient = {
  get: jest.fn().mockResolvedValue({ data: [] }),
  post: jest.fn().mockResolvedValue({ data: { success: true } }),
  put: jest.fn().mockResolvedValue({ data: { success: true } }),
  delete: jest.fn().mockResolvedValue({ data: { success: true } }),
  patch: jest.fn().mockResolvedValue({ data: { success: true } }),
};

// ============================================================================
// Mock Navigate
// ============================================================================
export const mockNavigate = jest.fn();

jest.mock('react-router-dom', () => ({
  ...jest.requireActual('react-router-dom'),
  useNavigate: () => mockNavigate,
}));

// ============================================================================
// Test Providers Wrapper
// ============================================================================
interface AllTheProvidersProps {
  children: ReactNode;
  initialRoute?: string;
}

const AllTheProviders: React.FC<AllTheProvidersProps> = ({ children, initialRoute = '/' }) => {
  return (
    <MemoryRouter initialEntries={[initialRoute]}>
      <ThemeProvider theme={mockTheme}>
        <AuthContext.Provider value={mockAuthContext}>
          <ProfileContext.Provider value={mockProfileContext}>
            <LayoutContext.Provider value={mockLayoutContext}>
              <BrandingContext.Provider value={mockBrandingContext}>
                {children}
              </BrandingContext.Provider>
            </LayoutContext.Provider>
          </ProfileContext.Provider>
        </AuthContext.Provider>
      </ThemeProvider>
    </MemoryRouter>
  );
};

// ============================================================================
// Custom Render
// ============================================================================
interface CustomRenderOptions extends Omit<RenderOptions, 'wrapper'> {
  initialRoute?: string;
}

const customRender = (
  ui: ReactElement,
  options?: CustomRenderOptions
) => {
  const { initialRoute, ...renderOptions } = options || {};
  
  return render(ui, {
    wrapper: ({ children }) => (
      <AllTheProviders initialRoute={initialRoute}>
        {children}
      </AllTheProviders>
    ),
    ...renderOptions,
  });
};

// Re-export everything from testing-library
export * from '@testing-library/react';
export { customRender as render };

// ============================================================================
// Helper Functions
// ============================================================================
export const waitForLoadingToFinish = async () => {
  // Wait for any loading spinners to disappear
  await new Promise(resolve => setTimeout(resolve, 100));
};

export const mockLocalStorage = () => {
  const store: Record<string, string> = {};
  return {
    getItem: jest.fn((key: string) => store[key] || null),
    setItem: jest.fn((key: string, value: string) => {
      store[key] = value;
    }),
    removeItem: jest.fn((key: string) => {
      delete store[key];
    }),
    clear: jest.fn(() => {
      Object.keys(store).forEach(key => delete store[key]);
    }),
    get length() {
      return Object.keys(store).length;
    },
    key: jest.fn((index: number) => Object.keys(store)[index] || null),
  };
};

export const mockSessionStorage = () => {
  const store: Record<string, string> = {};
  return {
    getItem: jest.fn((key: string) => store[key] || null),
    setItem: jest.fn((key: string, value: string) => {
      store[key] = value;
    }),
    removeItem: jest.fn((key: string) => {
      delete store[key];
    }),
    clear: jest.fn(() => {
      Object.keys(store).forEach(key => delete store[key]);
    }),
    get length() {
      return Object.keys(store).length;
    },
    key: jest.fn((index: number) => Object.keys(store)[index] || null),
  };
};

// ============================================================================
// Form Testing Helpers
// ============================================================================
export const fillForm = async (
  userEvent: any,
  fields: Record<string, string>
) => {
  for (const [fieldName, value] of Object.entries(fields)) {
    const input = document.querySelector(`[name="${fieldName}"]`) as HTMLInputElement;
    if (input) {
      await userEvent.clear(input);
      await userEvent.type(input, value);
    }
  }
};

export const submitForm = async (userEvent: any, buttonText: string = 'Submit') => {
  const button = document.querySelector(`button[type="submit"]`) || 
                 document.querySelector(`button:contains("${buttonText}")`);
  if (button) {
    await userEvent.click(button);
  }
};

// ============================================================================
// Table Testing Helpers
// ============================================================================
export const getTableRows = () => {
  return document.querySelectorAll('table tbody tr');
};

export const getTableHeaders = () => {
  return document.querySelectorAll('table thead th');
};

export const getCellByRowAndColumn = (rowIndex: number, colIndex: number) => {
  const rows = getTableRows();
  if (rows[rowIndex]) {
    const cells = rows[rowIndex].querySelectorAll('td');
    return cells[colIndex];
  }
  return null;
};
