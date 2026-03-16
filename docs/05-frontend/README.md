# Frontend Documentation

> **Last Updated:** March 2026 | **Version:** 0.625.0

---

## Table of Contents

1. [Overview](#1-overview)
2. [Project Structure](#2-project-structure)
3. [Pages](#3-pages)
4. [Components](#4-components)
5. [State Management](#5-state-management)
6. [Services](#6-services)
7. [Routing](#7-routing)
8. [Theme & Styling](#8-theme--styling)
9. [Dependencies](#9-dependencies)

---

## 1. Overview

The frontend is a React Single Page Application (SPA):

- **React 18** with functional components and hooks
- **TypeScript** for type safety
- **Material-UI 5** (MUI) for UI components
- **React Router 6** for routing
- **Axios** for API communication
- **SignalR** for real-time updates

---

## 2. Project Structure

```
CRM.Frontend/
├── public/
│   └── index.html
├── src/
│   ├── App.tsx                     # Main app component
│   ├── main.tsx                    # Entry point
│   │
│   ├── components/                 # Reusable components
│   │   ├── common/                 # Generic UI components
│   │   │   ├── Button.tsx
│   │   │   ├── Card.tsx
│   │   │   ├── DataTable.tsx
│   │   │   ├── Dialog.tsx
│   │   │   ├── LoadingSpinner.tsx
│   │   │   └── ...
│   │   ├── forms/                  # Form components
│   │   │   ├── FormField.tsx
│   │   │   ├── AutocompleteField.tsx
│   │   │   └── ...
│   │   ├── layout/                 # Layout components
│   │   │   ├── AppLayout.tsx
│   │   │   ├── Sidebar.tsx
│   │   │   ├── Header.tsx
│   │   │   └── Footer.tsx
│   │   └── modules/                # Module-specific components
│   │       ├── customers/
│   │       ├── campaigns/
│   │       └── ...
│   │
│   ├── pages/                      # Page components
│   │   ├── Dashboard/
│   │   ├── Customers/
│   │   ├── Contacts/
│   │   ├── Leads/
│   │   ├── Opportunities/
│   │   ├── Products/
│   │   ├── Campaigns/
│   │   ├── Quotes/
│   │   ├── ServiceRequests/
│   │   ├── Tasks/
│   │   ├── Settings/
│   │   ├── Login/
│   │   └── SetupPassword/
│   │
│   ├── services/                   # API services
│   │   ├── api.ts                  # Axios instance
│   │   ├── authService.ts
│   │   ├── customerService.ts
│   │   ├── contactService.ts
│   │   └── ...
│   │
│   ├── contexts/                   # React contexts
│   │   ├── AuthContext.tsx
│   │   ├── ThemeContext.tsx
│   │   └── SignalRContext.tsx
│   │
│   ├── hooks/                      # Custom hooks
│   │   ├── useAuth.ts
│   │   ├── usePagination.ts
│   │   ├── useDebounce.ts
│   │   └── useSignalR.ts
│   │
│   ├── types/                      # TypeScript types
│   │   ├── entities.ts
│   │   ├── api.ts
│   │   └── ...
│   │
│   ├── theme/                      # MUI theme
│   │   └── theme.ts
│   │
│   └── utils/                      # Utilities
│       ├── formatters.ts
│       ├── validators.ts
│       └── helpers.ts
│
├── package.json
├── tsconfig.json
└── vite.config.ts
```

---

## 3. Pages

### 3.1 Page List

| Page | Path | File | Description |
|------|------|------|-------------|
| **Login** | `/login` | `pages/Login/LoginPage.tsx` | User login |
| **Setup Password** | `/setup-password` | `pages/SetupPassword/SetupPasswordPage.tsx` | First-time password setup |
| **Dashboard** | `/` | `pages/Dashboard/DashboardPage.tsx` | Main dashboard |
| **Customers** | `/customers` | `pages/Customers/CustomersPage.tsx` | Customer list |
| **Customer Detail** | `/customers/:id` | `pages/Customers/CustomerDetailPage.tsx` | Customer detail |
| **Contacts** | `/contacts` | `pages/Contacts/ContactsPage.tsx` | Contact list |
| **Contact Detail** | `/contacts/:id` | `pages/Contacts/ContactDetailPage.tsx` | Contact detail |
| **Leads** | `/leads` | `pages/Leads/LeadsPage.tsx` | Lead list |
| **Opportunities** | `/opportunities` | `pages/Opportunities/OpportunitiesPage.tsx` | Opportunity list |
| **Products** | `/products` | `pages/Products/ProductsPage.tsx` | Product catalog |
| **Services** | `/services` | `pages/Services/ServicesPage.tsx` | Service catalog |
| **Campaigns** | `/campaigns` | `pages/Campaigns/CampaignsPage.tsx` | Campaign list |
| **Campaign Detail** | `/campaigns/:id` | `pages/Campaigns/CampaignDetailPage.tsx` | Campaign detail |
| **Quotes** | `/quotes` | `pages/Quotes/QuotesPage.tsx` | Quote list |
| **Service Requests** | `/service-requests` | `pages/ServiceRequests/ServiceRequestsPage.tsx` | Support tickets |
| **Tasks** | `/tasks` | `pages/Tasks/TasksPage.tsx` | Task list |
| **Notes** | `/notes` | `pages/Notes/NotesPage.tsx` | Notes |
| **Activities** | `/activities` | `pages/Activities/ActivitiesPage.tsx` | Activity log |
| **Workflows** | `/workflows` | `pages/Workflows/WorkflowsPage.tsx` | Workflow automation |
| **Settings** | `/settings` | `pages/Settings/SettingsPage.tsx` | Admin settings |

### 3.2 Page Pattern

```typescript
// Standard page structure
import React, { useState, useEffect } from 'react';
import { Box, Typography, Button } from '@mui/material';
import { DataGrid } from '@mui/x-data-grid';
import { customerService } from '../../services/customerService';
import { Customer } from '../../types/entities';

const CustomersPage: React.FC = () => {
  const [customers, setCustomers] = useState<Customer[]>([]);
  const [loading, setLoading] = useState(true);
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(20);
  const [totalCount, setTotalCount] = useState(0);

  useEffect(() => {
    loadCustomers();
  }, [page, pageSize]);

  const loadCustomers = async () => {
    setLoading(true);
    try {
      const result = await customerService.getAll({ page, pageSize });
      setCustomers(result.items);
      setTotalCount(result.totalCount);
    } catch (error) {
      console.error('Failed to load customers:', error);
    } finally {
      setLoading(false);
    }
  };

  return (
    <Box sx={{ p: 3 }}>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 2 }}>
        <Typography variant="h4">Customers</Typography>
        <Button variant="contained" onClick={handleAdd}>
          Add Customer
        </Button>
      </Box>
      <DataGrid
        rows={customers}
        columns={columns}
        loading={loading}
        pagination
        page={page - 1}
        pageSize={pageSize}
        rowCount={totalCount}
        onPageChange={(newPage) => setPage(newPage + 1)}
        onPageSizeChange={(newPageSize) => setPageSize(newPageSize)}
      />
    </Box>
  );
};

export default CustomersPage;
```

---

## 4. Components

### 4.1 Common Components

| Component | File | Description |
|-----------|------|-------------|
| `DataTable` | `components/common/DataTable.tsx` | Generic data grid |
| `FormDialog` | `components/common/FormDialog.tsx` | Modal form |
| `ConfirmDialog` | `components/common/ConfirmDialog.tsx` | Confirmation modal |
| `LoadingSpinner` | `components/common/LoadingSpinner.tsx` | Loading indicator |
| `PageHeader` | `components/common/PageHeader.tsx` | Page title with actions |
| `SearchBar` | `components/common/SearchBar.tsx` | Search input |
| `StatusChip` | `components/common/StatusChip.tsx` | Status badge |
| `EmptyState` | `components/common/EmptyState.tsx` | Empty list state |

### 4.2 Layout Components

| Component | File | Description |
|-----------|------|-------------|
| `AppLayout` | `components/layout/AppLayout.tsx` | Main app shell |
| `Sidebar` | `components/layout/Sidebar.tsx` | Navigation sidebar |
| `Header` | `components/layout/Header.tsx` | Top header bar |
| `Footer` | `components/layout/Footer.tsx` | Footer |
| `Breadcrumbs` | `components/layout/Breadcrumbs.tsx` | Page breadcrumbs |

### 4.3 Form Components

| Component | File | Description |
|-----------|------|-------------|
| `FormField` | `components/forms/FormField.tsx` | Generic form field |
| `TextField` | `components/forms/TextField.tsx` | Text input |
| `SelectField` | `components/forms/SelectField.tsx` | Dropdown select |
| `AutocompleteField` | `components/forms/AutocompleteField.tsx` | Autocomplete input |
| `DatePickerField` | `components/forms/DatePickerField.tsx` | Date picker |
| `CheckboxField` | `components/forms/CheckboxField.tsx` | Checkbox |
| `RichTextField` | `components/forms/RichTextField.tsx` | Rich text editor |

---

## 5. State Management

### 5.1 React Context

The app uses React Context for global state:

#### AuthContext

```typescript
// contexts/AuthContext.tsx
interface AuthContextType {
  user: User | null;
  isAuthenticated: boolean;
  login: (email: string, password: string) => Promise<LoginResult>;
  logout: () => void;
  loading: boolean;
}

export const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [user, setUser] = useState<User | null>(null);
  const [loading, setLoading] = useState(true);

  const login = async (email: string, password: string): Promise<LoginResult> => {
    const response = await authService.login({ email, password });
    if (response.requiresPasswordSetup || response.passwordExpired || response.mustChangePassword) {
      return {
        success: false,
        requiresPasswordSetup: response.requiresPasswordSetup,
        passwordExpired: response.passwordExpired,
        mustChangePassword: response.mustChangePassword,
        setupToken: response.passwordSetupToken,
        email
      };
    }
    setUser(response.user);
    localStorage.setItem('accessToken', response.accessToken);
    localStorage.setItem('refreshToken', response.refreshToken);
    return { success: true };
  };

  // ...
};
```

#### ThemeContext

```typescript
// contexts/ThemeContext.tsx
interface ThemeContextType {
  mode: 'light' | 'dark' | 'system';
  setMode: (mode: 'light' | 'dark' | 'system') => void;
  primaryColor: string;
  setPrimaryColor: (color: string) => void;
}
```

#### SignalRContext

```typescript
// contexts/SignalRContext.tsx
interface SignalRContextType {
  connection: HubConnection | null;
  isConnected: boolean;
  subscribe: (event: string, handler: (data: any) => void) => void;
  unsubscribe: (event: string) => void;
}
```

### 5.2 Custom Hooks

| Hook | File | Purpose |
|------|------|---------|
| `useAuth` | `hooks/useAuth.ts` | Access auth context |
| `usePagination` | `hooks/usePagination.ts` | Pagination state |
| `useDebounce` | `hooks/useDebounce.ts` | Debounce values |
| `useSignalR` | `hooks/useSignalR.ts` | SignalR subscription |
| `useLocalStorage` | `hooks/useLocalStorage.ts` | Local storage state |

---

## 6. Services

### 6.1 API Instance

```typescript
// services/api.ts
import axios from 'axios';

const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL || '/api',
  timeout: 30000,
  headers: {
    'Content-Type': 'application/json'
  }
});

// Request interceptor - add auth token
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('accessToken');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Response interceptor - handle errors, refresh token
api.interceptors.response.use(
  (response) => response,
  async (error) => {
    if (error.response?.status === 401) {
      // Attempt token refresh or redirect to login
    }
    return Promise.reject(error);
  }
);

export default api;
```

### 6.2 Service Pattern

```typescript
// services/customerService.ts
import api from './api';
import { Customer, CreateCustomerDto, UpdateCustomerDto, PagedResult } from '../types';

export const customerService = {
  getAll: async (params?: { page?: number; pageSize?: number; search?: string }) => {
    const response = await api.get<PagedResult<Customer>>('/accounts', { params });
    return response.data;
  },

  getById: async (id: number) => {
    const response = await api.get<Customer>(`/accounts/${id}`);
    return response.data;
  },

  create: async (data: CreateCustomerDto) => {
    const response = await api.post<Customer>('/accounts', data);
    return response.data;
  },

  update: async (id: number, data: UpdateCustomerDto) => {
    const response = await api.put<Customer>(`/accounts/${id}`, data);
    return response.data;
  },

  delete: async (id: number) => {
    await api.delete(`/accounts/${id}`);
  }
};
```

### 6.3 Service List

| Service | File | Entity |
|---------|------|--------|
| `authService` | `services/authService.ts` | Authentication |
| `customerService` | `services/customerService.ts` | Customers |
| `contactService` | `services/contactService.ts` | Contacts |
| `leadService` | `services/leadService.ts` | Leads |
| `opportunityService` | `services/opportunityService.ts` | Opportunities |
| `productService` | `services/productService.ts` | Products |
| `campaignService` | `services/campaignService.ts` | Campaigns |
| `quoteService` | `services/quoteService.ts` | Quotes |
| `taskService` | `services/taskService.ts` | Tasks |
| `noteService` | `services/noteService.ts` | Notes |
| `serviceRequestService` | `services/serviceRequestService.ts` | Service Requests |
| `systemSettingsService` | `services/systemSettingsService.ts` | Settings |
| `userService` | `services/userService.ts` | Users |
| `userGroupService` | `services/userGroupService.ts` | User Groups |

---

## 7. Routing

### 7.1 Route Configuration

```typescript
// App.tsx
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider } from './contexts/AuthContext';
import ProtectedRoute from './components/ProtectedRoute';
import AppLayout from './components/layout/AppLayout';

// Pages
import LoginPage from './pages/Login/LoginPage';
import SetupPasswordPage from './pages/SetupPassword/SetupPasswordPage';
import DashboardPage from './pages/Dashboard/DashboardPage';
import CustomersPage from './pages/Customers/CustomersPage';
// ... more imports

function App() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <Routes>
          {/* Public routes */}
          <Route path="/login" element={<LoginPage />} />
          <Route path="/setup-password" element={<SetupPasswordPage />} />

          {/* Protected routes */}
          <Route element={<ProtectedRoute />}>
            <Route element={<AppLayout />}>
              <Route path="/" element={<DashboardPage />} />
              <Route path="/customers" element={<CustomersPage />} />
              <Route path="/customers/:id" element={<CustomerDetailPage />} />
              <Route path="/contacts" element={<ContactsPage />} />
              <Route path="/contacts/:id" element={<ContactDetailPage />} />
              <Route path="/leads" element={<LeadsPage />} />
              <Route path="/opportunities" element={<OpportunitiesPage />} />
              <Route path="/campaigns" element={<CampaignsPage />} />
              <Route path="/campaigns/:id" element={<CampaignDetailPage />} />
              <Route path="/settings" element={<SettingsPage />} />
              {/* ... more routes */}
            </Route>
          </Route>

          {/* Fallback */}
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </AuthProvider>
    </BrowserRouter>
  );
}
```

### 7.2 Protected Route

```typescript
// components/ProtectedRoute.tsx
import { Navigate, Outlet, useLocation } from 'react-router-dom';
import { useAuth } from '../hooks/useAuth';
import LoadingSpinner from './common/LoadingSpinner';

const ProtectedRoute: React.FC = () => {
  const { isAuthenticated, loading } = useAuth();
  const location = useLocation();

  if (loading) {
    return <LoadingSpinner />;
  }

  if (!isAuthenticated) {
    return <Navigate to="/login" state={{ from: location }} replace />;
  }

  return <Outlet />;
};

export default ProtectedRoute;
```

---

## 8. Theme & Styling

### 8.1 MUI Theme

```typescript
// theme/theme.ts
import { createTheme } from '@mui/material/styles';

export const createAppTheme = (mode: 'light' | 'dark', primaryColor: string) => {
  return createTheme({
    palette: {
      mode,
      primary: {
        main: primaryColor || '#6750A4',
      },
      secondary: {
        main: '#625B71',
      },
      background: {
        default: mode === 'dark' ? '#121212' : '#FAFAFA',
        paper: mode === 'dark' ? '#1E1E1E' : '#FFFFFF',
      },
    },
    typography: {
      fontFamily: '"Roboto", "Helvetica", "Arial", sans-serif',
      h1: { fontSize: '2.5rem', fontWeight: 500 },
      h2: { fontSize: '2rem', fontWeight: 500 },
      h3: { fontSize: '1.75rem', fontWeight: 500 },
      h4: { fontSize: '1.5rem', fontWeight: 500 },
      h5: { fontSize: '1.25rem', fontWeight: 500 },
      h6: { fontSize: '1rem', fontWeight: 500 },
    },
    components: {
      MuiButton: {
        defaultProps: {
          disableElevation: true,
        },
        styleOverrides: {
          root: {
            textTransform: 'none',
            borderRadius: 8,
          },
        },
      },
      MuiCard: {
        styleOverrides: {
          root: {
            borderRadius: 12,
          },
        },
      },
    },
  });
};
```

### 8.2 Responsive Design

The app uses MUI's responsive breakpoints:

```typescript
// Breakpoints
// xs: 0px
// sm: 600px
// md: 900px
// lg: 1200px
// xl: 1536px

// Usage
<Box sx={{ 
  display: { xs: 'none', md: 'block' },  // Hidden on mobile
  width: { xs: '100%', md: '50%' }        // Full width on mobile, half on desktop
}} />
```

---

## 9. Dependencies

### 9.1 NPM Packages

| Package | Version | Purpose |
|---------|---------|---------|
| `react` | ^18.2.0 | UI library |
| `react-dom` | ^18.2.0 | React DOM |
| `react-router-dom` | ^6.x | Routing |
| `@mui/material` | ^5.x | UI components |
| `@mui/icons-material` | ^5.x | Icons |
| `@mui/x-data-grid` | ^6.x | Data grid |
| `@mui/x-date-pickers` | ^6.x | Date pickers |
| `axios` | ^1.x | HTTP client |
| `@microsoft/signalr` | ^8.x | Real-time |
| `react-hook-form` | ^7.x | Form management |
| `zod` | ^3.x | Validation |
| `date-fns` | ^2.x | Date utilities |
| `recharts` | ^2.x | Charts |

### 9.2 Dev Dependencies

| Package | Purpose |
|---------|---------|
| `typescript` | Type checking |
| `vite` | Build tool |
| `@vitejs/plugin-react` | React plugin |
| `eslint` | Linting |
| `prettier` | Code formatting |
| `@testing-library/react` | Testing |
| `vitest` | Test runner |

---

## Related Documentation

- [API Reference](../04-api/README.md)
- [Standards](../06-standards/README.md)
- [Testing](../07-testing/README.md)
