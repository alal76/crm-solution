# Frontend Developer Guide

> **CRM Solution React Frontend - Architecture & Development Guide**  
> Version: 0.614.84  
> Last Updated: March 3, 2026  
> Framework: React 18.2 + TypeScript 4.9 + Material-UI 5.14

---

## Table of Contents

1. [Overview](#overview)
2. [Technology Stack](#technology-stack)
3. [Project Structure](#project-structure)
4. [Getting Started](#getting-started)
5. [Architecture](#architecture)
6. [Pages & Routing](#pages--routing)
7. [Services Layer](#services-layer)
8. [State Management](#state-management)
9. [Components](#components)
10. [Custom Hooks](#custom-hooks)
11. [Styling & Theming](#styling--theming)
12. [API Integration](#api-integration)
13. [Testing](#testing)
14. [Best Practices](#best-practices)
15. [Troubleshooting](#troubleshooting)

---

## Overview

The CRM Frontend is a **modern React 18 single-page application** built with TypeScript and Material-UI:

- **216,416 lines of code** across 612 total files
- **457 TSX components** (React components)
- **155 TypeScript files** (services, utilities, types)
- **186+ pages** covering all CRM modules
- **78 service classes** for API communication
- **42 test files** (Jest + React Testing Library)

### Key Features

- 📱 **Responsive Design** - Mobile-first approach with Material-UI Grid
- 🎨 **Dark/Light Themes** - User-selectable theme with persistence
- 🔐 **Authentication** - JWT-based auth with refresh tokens
- 📡 **Real-Time Updates** - SignalR WebSocket integration
- 🌍 **Internationalization** - Multi-language support (i18next)
- ♿ **Accessibility** - WCAG 2.1 AA compliant
- 🚀 **Performance** - Code splitting, lazy loading, memoization

---

## Technology Stack

| Category | Technology | Version |
|----------|-----------|---------|
| **UI Framework** | React | 18.2.0 |
| **Language** | TypeScript | 4.9.5 |
| **Component Library** | Material-UI (MUI) | 5.14.20 |
| **Icons** | Material Icons, Font Awesome | 5.16.5 |
| **State Management** | React Context API | Built-in |
| **Routing** | React Router | 6.20.1 |
| **HTTP Client** | Axios | 1.6.2 |
| **Real-Time** | SignalR | @microsoft/signalr@8.0.0 |
| **Forms** | Formik + Yup | 2.4.5 / 1.3.3 |
| **Charts** | Recharts | 2.10.3 |
| **Date Handling** | date-fns | 2.30.0 |
| **Rich Text** | TinyMCE React | 4.3.2 |
| **Testing** | Jest + React Testing Library | 29.7.0 / 14.1.2 |
| **Build Tool** | Create React App | 5.0.1 |
| **Bundler** | Webpack | 5.x (via CRA) |

### Development Dependencies

```json
{
  "react": "^18.2.0",
  "typescript": "^4.9.5",
  "@mui/material": "^5.14.20",
  "@mui/icons-material": "^5.14.19",
  "react-router-dom": "^6.20.1",
  "axios": "^1.6.2",
  "@microsoft/signalr": "^8.0.0",
  "formik": "^2.4.5",
  "yup": "^1.3.3",
  "recharts": "^2.10.3",
  "date-fns": "^2.30.0"
}
```

---

## Project Structure

```
CRM.Frontend/
├── package.json               # Dependencies & scripts
├── tsconfig.json              # TypeScript configuration
├── craco.config.js            # Create React App override
├── jest.config.json           # Jest configuration
│
├── public/
│   ├── index.html            # HTML template
│   ├── favicon.ico
│   └── assets/               # Static assets
│
└── src/
    ├── index.tsx             # Application entry point
    ├── App.tsx               # Root component (Router + Theme)
    ├── routes.tsx            # Route definitions (130+ routes)
    │
    ├── pages/                # Route-level components (186 files)
    │   ├── DashboardPage.tsx
    │   ├── CustomersPage.tsx         # Accounts list
    │   ├── CustomerDetailPage.tsx
    │   ├── ContactsPage.tsx
    │   ├── LeadsPage.tsx
    │   ├── OpportunitiesPage.tsx
    │   ├── ServiceRequestsPage.tsx
    │   ├── CampaignsPage.tsx
    │   ├── QuotesPage.tsx
    │   ├── OrdersPage.tsx
    │   └── ... (177 more pages)
    │
    ├── components/           # Reusable UI components (29 directories)
    │   ├── common/           # DataGrid, Card, Dialog, etc.
    │   ├── forms/            # Form inputs, validation
    │   ├── layout/           # Sidebar, AppBar, Footer
    │   ├── sales/            # Sales-specific components
    │   ├── itsm/             # ITSM module components
    │   └── dashboard/        # Dashboard widgets
    │
    ├── services/             # API service layer (78 files)
    │   ├── apiClient.ts      # Axios instance with interceptors
    │   ├── accountService.ts
    │   ├── contactService.ts
    │   ├── leadService.ts
    │   ├── opportunityService.ts
    │   ├── serviceRequestService.ts
    │   └── ... (73 more services)
    │
    ├── contexts/             # React Context providers (13 files)
    │   ├── AuthContext.tsx          # Authentication state
    │   ├── ThemeContext.tsx         # Theme switching
    │   ├── SignalRContext.tsx       # Real-time connections
    │   ├── NotificationContext.tsx  # Toast notifications
    │   ├── LoadingContext.tsx       # Global loading state
    │   └── ... (8 more contexts)
    │
    ├── hooks/                # Custom React hooks (14 files)
    │   ├── useAuth.ts               # Authentication hook
    │   ├── useSignalR.ts            # SignalR hook
    │   ├── usePagination.ts         # Pagination hook
    │   ├── useDebounce.ts           # Debounce hook
    │   └── ... (10 more hooks)
    │
    ├── types/                # TypeScript types & interfaces
    │   ├── Account.ts
    │   ├── Contact.ts
    │   ├── Lead.ts
    │   ├── Opportunity.ts
    │   └── ... (50+ type files)
    │
    ├── utils/                # Utility functions
    │   ├── formatters.ts     # Date, currency, phone formatting
    │   ├── validators.ts     # Validation helpers
    │   ├── constants.ts      # App constants
    │   └── helpers.ts        # General helpers
    │
    ├── styles/               # Global styles
    │   ├── theme.ts          # MUI theme configuration
    │   ├── global.css        # Global CSS
    │   └── variables.css     # CSS variables
    │
    └── assets/               # Images, fonts, icons
```

### File Count Breakdown

| Directory | TSX Files | TS Files | Total |
|-----------|-----------|----------|-------|
| `pages/` | 186 | 0 | 186 |
| `components/` | 89 | 5 | 94 |
| `services/` | 0 | 78 | 78 |
| `contexts/` | 13 | 0 | 13 |
| `hooks/` | 0 | 14 | 14 |
| `types/` | 0 | 50+ | 50+ |
| `utils/` | 0 | 8 | 8 |
| **Total** | **457** | **155+** | **612+** |

---

## Getting Started

### Prerequisites

```bash
# Required
- Node.js 18+ (LTS)
- npm 9+ or yarn 1.22+
- Git

# Optional
- VS Code with ESLint & Prettier extensions
```

### Initial Setup

1. **Navigate to frontend directory:**
   ```bash
   cd /Users/alal/Code/Git\ CRM\ Solution/crm-solution/CRM.Frontend
   ```

2. **Install dependencies:**
   ```bash
   npm install
   # or
   yarn install
   ```

3. **Configure environment:**
   ```bash
   # Create .env.local file
   REACT_APP_API_BASE_URL=http://localhost:5000
   REACT_APP_SIGNALR_HUB_URL=http://localhost:5000/hubs
   REACT_APP_ENABLE_MOCK_API=false
   ```

4. **Start development server:**
   ```bash
   npm start
   # Runs on http://localhost:3000
   ```

### Quick Start Script

```bash
# Use the root start script (starts both API + Frontend)
cd /Users/alal/Code/Git\ CRM\ Solution/crm-solution
./start-dev.sh
```

### Available Scripts

| Script | Command | Purpose |
|--------|---------|---------|
| **Start** | `npm start` | Development server (port 3000) |
| **Build** | `npm run build` | Production build → `build/` |
| **Test** | `npm test` | Run Jest tests |
| **Test Coverage** | `npm test -- --coverage` | Generate coverage report |
| **Lint** | `npm run lint` | Run ESLint |
| **Format** | `npm run format` | Format with Prettier |

---

## Architecture

### Application Flow

```
┌─────────────────────────────────────────────────────────────┐
│                      index.tsx                               │
│  - ReactDOM.render(<App />)                                  │
└─────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│                        App.tsx                               │
│  ┌────────────────────────────────────────────────────────┐ │
│  │  Context Providers (nested)                             │ │
│  │  ├── AuthProvider                                       │ │
│  │  ├── ThemeProvider                                      │ │
│  │  ├── NotificationProvider                               │ │
│  │  ├── LoadingProvider                                    │ │
│  │  └── SignalRProvider                                    │ │
│  └────────────────────────────────────────────────────────┘ │
│  ┌────────────────────────────────────────────────────────┐ │
│  │  Router (React Router v6)                               │ │
│  │  - Public routes (login, register)                      │ │
│  │  - Protected routes (dashboard, CRM pages)              │ │
│  └────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│                    Page Components                           │
│  (186 pages: Dashboard, Customers, Leads, etc.)             │
│                                                               │
│  Each page:                                                  │
│  1. Fetches data via services                                │
│  2. Manages local state                                      │
│  3. Renders child components                                 │
│  4. Handles user interactions                                │
└─────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│                 Reusable Components                          │
│  (Common, Forms, Layout, Domain-specific)                    │
└─────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│                    Service Layer                             │
│  (78 services: accountService, contactService, etc.)         │
│  - API calls via Axios                                       │
│  - Error handling                                            │
│  - Response transformation                                   │
└─────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│                   Backend REST API                           │
│  (http://localhost:5000/api)                                 │
└─────────────────────────────────────────────────────────────┘
```

### Component Hierarchy Example

```jsx
<App>
  <AuthProvider>
    <ThemeProvider>
      <NotificationProvider>
        <Router>
          <ProtectedRoute path="/accounts">
            <CustomersPage>                    {/* Page Component */}
              <PageHeader />
              <FilterBar />
              <DataGrid>                       {/* Reusable Component */}
                <GridToolbar />
                <GridTable />
                <Pagination />
              </DataGrid>
              <CreateAccountDialog />
            </CustomersPage>
          </ProtectedRoute>
        </Router>
      </NotificationProvider>
    </ThemeProvider>
  </AuthProvider>
</App>
```

---

## Pages & Routing

### Route Configuration

**Location:** `src/routes.tsx`

**130+ routes** organized by module:

```tsx
import { Routes, Route, Navigate } from 'react-router-dom';
import ProtectedRoute from './components/auth/ProtectedRoute';

// Core Pages
import DashboardPage from './pages/DashboardPage';
import CustomersPage from './pages/CustomersPage';
import CustomerDetailPage from './pages/CustomerDetailPage';
import ContactsPage from './pages/ContactsPage';
import LeadsPage from './pages/LeadsPage';
import OpportunitiesPage from './pages/OpportunitiesPage';

// Auth Pages
import LoginPage from './pages/auth/LoginPage';
import RegisterPage from './pages/auth/RegisterPage';

// ... 180+ more imports

export const AppRoutes: React.FC = () => {
  return (
    <Routes>
      {/* Public Routes */}
      <Route path="/login" element={<LoginPage />} />
      <Route path="/register" element={<RegisterPage />} />
      
      {/* Protected Routes */}
      <Route element={<ProtectedRoute />}>
        <Route path="/" element={<Navigate to="/dashboard" replace />} />
        <Route path="/dashboard" element={<DashboardPage />} />
        
        {/* CRM Module */}
        <Route path="/accounts" element={<CustomersPage />} />
        <Route path="/accounts/:id" element={<CustomerDetailPage />} />
        <Route path="/accounts/new" element={<CreateCustomerPage />} />
        <Route path="/contacts" element={<ContactsPage />} />
        <Route path="/contacts/:id" element={<ContactDetailPage />} />
        <Route path="/leads" element={<LeadsPage />} />
        <Route path="/leads/:id" element={<LeadDetailPage />} />
        <Route path="/opportunities" element={<OpportunitiesPage />} />
        <Route path="/opportunities/:id" element={<OpportunityDetailPage />} />
        
        {/* ITSM Module */}
        <Route path="/service-requests" element={<ServiceRequestsPage />} />
        <Route path="/service-requests/:id" element={<ServiceRequestDetailPage />} />
        <Route path="/knowledge-articles" element={<KnowledgeArticlesPage />} />
        <Route path="/workflows" element={<WorkflowsPage />} />
        
        {/* Sales Module */}
        <Route path="/quotes" element={<QuotesPage />} />
        <Route path="/orders" element={<OrdersPage />} />
        <Route path="/invoices" element={<InvoicesPage />} />
        <Route path="/payments" element={<PaymentsPage />} />
        
        {/* Marketing Module */}
        <Route path="/campaigns" element={<CampaignsPage />} />
        <Route path="/email-templates" element={<EmailTemplatesPage />} />
        
        {/* Settings */}
        <Route path="/settings" element={<SettingsPage />} />
        <Route path="/profile" element={<ProfilePage />} />
        
        {/* ... 100+ more routes ... */}
      </Route>
      
      {/* 404 */}
      <Route path="*" element={<NotFoundPage />} />
    </Routes>
  );
};
```

### Page Component Pattern

**Standard page structure:**

```tsx
import React, { useEffect, useState } from 'react';
import { Box, Button, Typography } from '@mui/material';
import { useNavigate } from 'react-router-dom';
import { DataGrid } from '../components/common/DataGrid';
import { accountService } from '../services/accountService';
import { Account } from '../types/Account';
import { useNotification } from '../contexts/NotificationContext';

const CustomersPage: React.FC = () => {
  const [accounts, setAccounts] = useState<Account[]>([]);
  const [loading, setLoading] = useState(false);
  const navigate = useNavigate();
  const { showNotification } = useNotification();
  
  useEffect(() => {
    loadAccounts();
  }, []);
  
  const loadAccounts = async () => {
    try {
      setLoading(true);
      const data = await accountService.getAll();
      setAccounts(data);
    } catch (error) {
      showNotification('Failed to load accounts', 'error');
    } finally {
      setLoading(false);
    }
  };
  
  const handleCreate = () => {
    navigate('/accounts/new');
  };
  
  const handleEdit = (id: number) => {
    navigate(`/accounts/${id}`);
  };
  
  const handleDelete = async (id: number) => {
    if (!window.confirm('Delete this account?')) return;
    
    try {
      await accountService.delete(id);
      showNotification('Account deleted', 'success');
      loadAccounts();
    } catch (error) {
      showNotification('Failed to delete account', 'error');
    }
  };
  
  return (
    <Box>
      <Box display="flex" justifyContent="space-between" mb={3}>
        <Typography variant="h4">Accounts</Typography>
        <Button variant="contained" onClick={handleCreate}>
          Create Account
        </Button>
      </Box>
      
      <DataGrid
        rows={accounts}
        columns={[
          { field: 'id', headerName: 'ID', width: 70 },
          { field: 'name', headerName: 'Name', width: 200 },
          { field: 'email', headerName: 'Email', width: 200 },
          { field: 'status', headerName: 'Status', width: 120 }
        ]}
        loading={loading}
        onRowClick={(row) => handleEdit(row.id)}
        onDelete={handleDelete}
      />
    </Box>
  );
};

export default CustomersPage;
```

### Page Types

| Category | Count | Examples |
|----------|-------|----------|
| **Dashboard** | 1 | DashboardPage |
| **CRM Pages** | 32 | Accounts, Contacts, Leads, Opportunities |
| **ITSM Pages** | 24 | ServiceRequests, KnowledgeArticles, Workflows |
| **Sales Pages** | 28 | Quotes, Orders, Invoices, Payments, Contracts |
| **Marketing Pages** | 18 | Campaigns, EmailTemplates, EmailSequences |
| **Settings Pages** | 14 | Users, Groups, SystemSettings, Integrations |
| **Reports Pages** | 12 | SalesReports, CampaignReports, ITSMReports |
| **Admin Pages** | 10 | FeatureFlags, Monitoring, AuditLogs |
| **Auth Pages** | 5 | Login, Register, ForgotPassword, ResetPassword |
| **Error Pages** | 3 | 404, 403, 500 |

---

## Services Layer

### API Client Configuration

**Location:** `src/services/apiClient.ts`

Centralized Axios instance with interceptors:

```typescript
import axios, { AxiosInstance, AxiosError, AxiosRequestConfig } from 'axios';

const API_BASE_URL = process.env.REACT_APP_API_BASE_URL || 'http://localhost:5000';

// Create Axios instance
const apiClient: AxiosInstance = axios.create({
  baseURL: API_BASE_URL,
  timeout: 30000,
  headers: {
    'Content-Type': 'application/json'
  }
});

// Request interceptor - Add JWT token
apiClient.interceptors.request.use(
  (config: AxiosRequestConfig) => {
    const token = localStorage.getItem('access_token');
    if (token && config.headers) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error: AxiosError) => {
    return Promise.reject(error);
  }
);

// Response interceptor - Handle errors & token refresh
apiClient.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const originalRequest = error.config as AxiosRequestConfig & { _retry?: boolean };
    
    // Handle 401 - Attempt token refresh
    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true;
      
      try {
        const refreshToken = localStorage.getItem('refresh_token');
        const response = await axios.post(`${API_BASE_URL}/api/auth/refresh`, {
          refreshToken
        });
        
        const { accessToken, refreshToken: newRefreshToken } = response.data;
        localStorage.setItem('access_token', accessToken);
        localStorage.setItem('refresh_token', newRefreshToken);
        
        // Retry original request with new token
        if (originalRequest.headers) {
          originalRequest.headers.Authorization = `Bearer ${accessToken}`;
        }
        return apiClient(originalRequest);
      } catch (refreshError) {
        // Refresh failed - logout user
        localStorage.removeItem('access_token');
        localStorage.removeItem('refresh_token');
        window.location.href = '/login';
        return Promise.reject(refreshError);
      }
    }
    
    return Promise.reject(error);
  }
);

export default apiClient;
```

### Service Pattern

**Standard service structure:**

```typescript
// src/services/accountService.ts
import apiClient from './apiClient';
import { Account, CreateAccountDto, UpdateAccountDto } from '../types/Account';

export const accountService = {
  /**
   * Get all accounts
   */
  getAll: async (): Promise<Account[]> => {
    const response = await apiClient.get<Account[]>('/api/accounts');
    return response.data;
  },
  
  /**
   * Get account by ID
   */
  getById: async (id: number): Promise<Account> => {
    const response = await apiClient.get<Account>(`/api/accounts/${id}`);
    return response.data;
  },
  
  /**
   * Create new account
   */
  create: async (dto: CreateAccountDto): Promise<Account> => {
    const response = await apiClient.post<Account>('/api/accounts', dto);
    return response.data;
  },
  
  /**
   * Update account
   */
  update: async (id: number, dto: UpdateAccountDto): Promise<Account> => {
    const response = await apiClient.put<Account>(`/api/accounts/${id}`, dto);
    return response.data;
  },
  
  /**
   * Delete account (soft delete)
   */
  delete: async (id: number): Promise<void> => {
    await apiClient.delete(`/api/accounts/${id}`);
  },
  
  /**
   * Search accounts
   */
  search: async (query: string): Promise<Account[]> => {
    const response = await apiClient.get<Account[]>('/api/accounts/search', {
      params: { q: query }
    });
    return response.data;
  }
};
```

### 78 Service Files

| Service Category | Count | Examples |
|------------------|-------|----------|
| **CRM Services** | 8 | accountService, contactService, leadService |
| **ITSM Services** | 12 | serviceRequestService, knowledgeArticleService |
| **Sales Services** | 10 | quoteService, orderService, invoiceService |
| **Marketing Services** | 8 | campaignService, emailTemplateService |
| **System Services** | 12 | userService, settingsService, auditLogService |
| **Provider Services** | 6 | aiService, searchService, chatService |
| **Integration Services** | 4 | webhookService, oauthService |
| **Report Services** | 8 | salesReportService, campaignReportService |
| **Dashboard Services** | 4 | dashboardService, widgetService |
| **Admin Services** | 6 | featureFlagService, monitoringService |

---

## State Management

### Context Providers

The application uses **React Context API** for state management with **13 context providers**:

#### 1. AuthContext

**Location:** `src/contexts/AuthContext.tsx`

Manages authentication state:

```tsx
import React, { createContext, useState, useContext, useEffect } from 'react';
import { User } from '../types/User';
import { authService } from '../services/authService';

interface AuthContextType {
  user: User | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  login: (email: string, password: string) => Promise<void>;
  logout: () => void;
  refreshUser: () => Promise<void>;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [user, setUser] = useState<User | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  
  useEffect(() => {
    checkAuth();
  }, []);
  
  const checkAuth = async () => {
    const token = localStorage.getItem('access_token');
    if (token) {
      try {
        const userData = await authService.getCurrentUser();
        setUser(userData);
      } catch (error) {
        logout();
      }
    }
    setIsLoading(false);
  };
  
  const login = async (email: string, password: string) => {
    const response = await authService.login(email, password);
    localStorage.setItem('access_token', response.accessToken);
    localStorage.setItem('refresh_token', response.refreshToken);
    setUser(response.user);
  };
  
  const logout = () => {
    localStorage.removeItem('access_token');
    localStorage.removeItem('refresh_token');
    setUser(null);
  };
  
  const refreshUser = async () => {
    const userData = await authService.getCurrentUser();
    setUser(userData);
  };
  
  return (
    <AuthContext.Provider value={{ user, isAuthenticated: !!user, isLoading, login, logout, refreshUser }}>
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) throw new Error('useAuth must be used within AuthProvider');
  return context;
};
```

#### 2. ThemeContext

**Location:** `src/contexts/ThemeContext.tsx`

Manages theme switching (light/dark):

```tsx
import React, { createContext, useState, useContext, useMemo } from 'react';
import { ThemeProvider as MUIThemeProvider, createTheme, PaletteMode } from '@mui/material';

interface ThemeContextType {
  mode: PaletteMode;
  toggleTheme: () => void;
}

const ThemeContext = createContext<ThemeContextType | undefined>(undefined);

export const ThemeProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [mode, setMode] = useState<PaletteMode>(() => {
    const saved = localStorage.getItem('theme_mode');
    return (saved as PaletteMode) || 'light';
  });
  
  const toggleTheme = () => {
    setMode((prev) => {
      const newMode = prev === 'light' ? 'dark' : 'light';
      localStorage.setItem('theme_mode', newMode);
      return newMode;
    });
  };
  
  const theme = useMemo(() => createTheme({
    palette: {
      mode,
      primary: { main: mode === 'light' ? '#1976d2' : '#90caf9' },
      secondary: { main: mode === 'light' ? '#dc004e' : '#f48fb1' }
    }
  }), [mode]);
  
  return (
    <ThemeContext.Provider value={{ mode, toggleTheme }}>
      <MUIThemeProvider theme={theme}>
        {children}
      </MUIThemeProvider>
    </ThemeContext.Provider>
  );
};

export const useTheme = () => {
  const context = useContext(ThemeContext);
  if (!context) throw new Error('useTheme must be used within ThemeProvider');
  return context;
};
```

#### 3. SignalRContext

**Location:** `src/contexts/SignalRContext.tsx`

Real-time WebSocket communication:

```tsx
import React, { createContext, useContext, useEffect, useState } from 'react';
import * as signalR from '@microsoft/signalr';

interface SignalRContextType {
  connection: signalR.HubConnection | null;
  connected: boolean;
}

const SignalRContext = createContext<SignalRContextType | undefined>(undefined);

const HUB_URL = process.env.REACT_APP_SIGNALR_HUB_URL || 'http://localhost:5000/hubs';

export const SignalRProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [connection, setConnection] = useState<signalR.HubConnection | null>(null);
  const [connected, setConnected] = useState(false);
  
  useEffect(() => {
    const token = localStorage.getItem('access_token');
    if (!token) return;
    
    const newConnection = new signalR.HubConnectionBuilder()
      .withUrl(`${HUB_URL}/crm`, {
        accessTokenFactory: () => token
      })
      .withAutomaticReconnect()
      .build();
    
    newConnection.start()
      .then(() => {
        console.log('SignalR connected');
        setConnected(true);
      })
      .catch((err) => console.error('SignalR connection error:', err));
    
    setConnection(newConnection);
    
    return () => {
      newConnection.stop();
    };
  }, []);
  
  return (
    <SignalRContext.Provider value={{ connection, connected }}>
      {children}
    </SignalRContext.Provider>
  );
};

export const useSignalR = () => {
  const context = useContext(SignalRContext);
  if (!context) throw new Error('useSignalR must be used within SignalRProvider');
  return context;
};
```

#### 4. NotificationContext

Toast notifications:

```tsx
import React, { createContext, useContext, useState } from 'react';
import { Snackbar, Alert, AlertColor } from '@mui/material';

interface NotificationContextType {
  showNotification: (message: string, severity?: AlertColor) => void;
}

const NotificationContext = createContext<NotificationContextType | undefined>(undefined);

export const NotificationProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [open, setOpen] = useState(false);
  const [message, setMessage] = useState('');
  const [severity, setSeverity] = useState<AlertColor>('info');
  
  const showNotification = (msg: string, sev: AlertColor = 'info') => {
    setMessage(msg);
    setSeverity(sev);
    setOpen(true);
  };
  
  return (
    <NotificationContext.Provider value={{ showNotification }}>
      {children}
      <Snackbar open={open} autoHideDuration={6000} onClose={() => setOpen(false)}>
        <Alert severity={severity} onClose={() => setOpen(false)}>
          {message}
        </Alert>
      </Snackbar>
    </NotificationContext.Provider>
  );
};

export const useNotification = () => {
  const context = useContext(NotificationContext);
  if (!context) throw new Error('useNotification must be used within NotificationProvider');
  return context;
};
```

### All 13 Context Providers

| Context | Purpose | State Managed |
|---------|---------|---------------|
| **AuthContext** | Authentication | user, token, isAuthenticated |
| **ThemeContext** | Theme switching | mode (light/dark) |
| **SignalRContext** | Real-time updates | connection, connected |
| **NotificationContext** | Toast messages | message, severity, open |
| **LoadingContext** | Global loading | isLoading |
| **LanguageContext** | i18n | language, translations |
| **SettingsContext** | App settings | settings |
| **FeatureFlagContext** | Feature flags | flags, isEnabled() |
| **PermissionContext** | User permissions | permissions, hasPermission() |
| **NavigationContext** | Navigation state | breadcrumbs, menu |
| **FilterContext** | List filters | filters, updateFilter() |
| **PaginationContext** | Pagination state | page, pageSize, totalPages |
| **SortContext** | Sort state | sortBy, sortOrder |

---

## Components

### Component Categories

**29 component directories** with reusable UI components:

```
components/
├── common/           # 18 general-purpose components
├── forms/            # 12 form-related components
├── layout/           # 8 layout components
├── dashboard/        # 10 dashboard widgets
├── sales/            # 6 sales-specific components
├── itsm/             # 8 ITSM components
├── marketing/        # 5 marketing components
├── charts/           # 4 chart components
├── auth/             # 3 auth components
└── ... (20+ more)
```

### Key Reusable Components

#### DataGrid

**Location:** `src/components/common/DataGrid.tsx`

Advanced data table with sorting, filtering, pagination:

```tsx
import React from 'react';
import { DataGrid as MUIDataGrid, GridColDef } from '@mui/x-data-grid';

interface DataGridProps {
  rows: any[];
  columns: GridColDef[];
  loading?: boolean;
  pageSize?: number;
  onRowClick?: (row: any) => void;
  onDelete?: (id: number) => void;
}

export const DataGrid: React.FC<DataGridProps> = ({
  rows,
  columns,
  loading = false,
  pageSize = 25,
  onRowClick,
  onDelete
}) => {
  return (
    <MUIDataGrid
      rows={rows}
      columns={columns}
      loading={loading}
      pageSize={pageSize}
      autoHeight
      disableSelectionOnClick
      onRowClick={(params) => onRowClick?.(params.row)}
      // ... additional props
    />
  );
};
```

#### Card

**Location:** `src/components/common/Card.tsx`

Consistent card component:

```tsx
import React from 'react';
import { Card as MUICard, CardContent, CardHeader, CardActions } from '@mui/material';

interface CardProps {
  title?: string;
  subtitle?: string;
  actions?: React.ReactNode;
  children: React.ReactNode;
}

export const Card: React.FC<CardProps> = ({ title, subtitle, actions, children }) => {
  return (
    <MUICard>
      {(title || subtitle) && <CardHeader title={title} subheader={subtitle} />}
      <CardContent>{children}</CardContent>
      {actions && <CardActions>{actions}</CardActions>}
    </MUICard>
  );
};
```

#### FormInput

**Location:** `src/components/forms/FormInput.tsx`

Formik-integrated input:

```tsx
import React from 'react';
import { TextField } from '@mui/material';
import { useField } from 'formik';

interface FormInputProps {
  name: string;
  label: string;
  type?: string;
  multiline?: boolean;
  rows?: number;
}

export const FormInput: React.FC<FormInputProps> = ({ name, label, type = 'text', ...props }) => {
  const [field, meta] = useField(name);
  
  return (
    <TextField
      {...field}
      label={label}
      type={type}
      error={meta.touched && Boolean(meta.error)}
      helperText={meta.touched && meta.error}
      fullWidth
      variant="outlined"
      {...props}
    />
  );
};
```

---

## Custom Hooks

### 14 Custom Hooks

| Hook | Purpose | Returns |
|------|---------|---------|
| **useAuth** | Authentication state | user, login, logout |
| **useSignalR** | SignalR connection | connection, on, off |
| **usePagination** | Pagination | page, pageSize, totalPages, setPage |
| **useDebounce** | Debounced value | debouncedValue |
| **useLocalStorage** | LocalStorage state | value, setValue |
| **useAsync** | Async operation state | data, loading, error, execute |
| **useQuery** | URL query params | query, setQuery |
| **useForm** | Form state | values, errors, handleChange |
| **useTable** | Table state | rows, columns, sort, filter |
| **useModal** | Modal state | open, close, isOpen |
| **useToast** | Toast notifications | show, hide |
| **usePermission** | Permission check | hasPermission, canAccess |
| **useFeatureFlag** | Feature flag check | isEnabled |
| **useThrottle** | Throttled value | throttledValue |

### Example Hook - useDebounce

```typescript
import { useState, useEffect } from 'react';

export function useDebounce<T>(value: T, delay: number = 500): T {
  const [debouncedValue, setDebouncedValue] = useState<T>(value);
  
  useEffect(() => {
    const handler = setTimeout(() => {
      setDebouncedValue(value);
    }, delay);
    
    return () => {
      clearTimeout(handler);
    };
  }, [value, delay]);
  
  return debouncedValue;
}

// Usage:
const [searchTerm, setSearchTerm] = useState('');
const debouncedSearch = useDebounce(searchTerm, 500);

useEffect(() => {
  if (debouncedSearch) {
    performSearch(debouncedSearch);
  }
}, [debouncedSearch]);
```

---

## Styling & Theming

### Material-UI Theme Configuration

**Location:** `src/styles/theme.ts`

```typescript
import { createTheme, PaletteMode } from '@mui/material';

export const getTheme = (mode: PaletteMode) => createTheme({
  palette: {
    mode,
    primary: {
      main: mode === 'light' ? '#1976d2' : '#90caf9',
      light: mode === 'light' ? '#42a5f5' : '#e3f2fd',
      dark: mode === 'light' ? '#1565c0' : '#42a5f5'
    },
    secondary: {
      main: mode === 'light' ? '#dc004e' : '#f48fb1',
      light: mode === 'light' ? '#f48fb1' : '#fce4ec',
      dark: mode === 'light' ? '#c51162' : '#f06292'
    },
    background: {
      default: mode === 'light' ? '#f5f5f5' : '#121212',
      paper: mode === 'light' ? '#ffffff' : '#1e1e1e'
    }
  },
  typography: {
    fontFamily: '"Roboto", "Helvetica", "Arial", sans-serif',
    h1: { fontSize: '2.5rem', fontWeight: 500 },
    h2: { fontSize: '2rem', fontWeight: 500 },
    h3: { fontSize: '1.75rem', fontWeight: 500 },
    h4: { fontSize: '1.5rem', fontWeight: 500 },
    h5: { fontSize: '1.25rem', fontWeight: 500 },
    h6: { fontSize: '1rem', fontWeight: 500 }
  },
  shape: {
    borderRadius: 8
  },
  spacing: 8
});
```

### Global Styles

**Location:** `src/styles/global.css`

```css
* {
  margin: 0;
  padding: 0;
  box-sizing: border-box;
}

body {
  font-family: 'Roboto', 'Helvetica', 'Arial', sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
}

a {
  text-decoration: none;
  color: inherit;
}

.MuiDataGrid-root {
  border: none;
}
```

---

## API Integration

### REST API Communication

All API calls go through services using the centralized `apiClient`:

```typescript
// Example: accountService.ts
import apiClient from './apiClient';

export const accountService = {
  getAll: () => apiClient.get('/api/accounts'),
  getById: (id: number) => apiClient.get(`/api/accounts/${id}`),
  create: (data: CreateAccountDto) => apiClient.post('/api/accounts', data),
  update: (id: number, data: UpdateAccountDto) => apiClient.put(`/api/accounts/${id}`, data),
  delete: (id: number) => apiClient.delete(`/api/accounts/${id}`)
};
```

### Error Handling Pattern

```typescript
const loadData = async () => {
  try {
    setLoading(true);
    const data = await accountService.getAll();
    setAccounts(data);
  } catch (error: any) {
    if (error.response) {
      // Server responded with error
      const message = error.response.data?.message || 'Server error';
      showNotification(message, 'error');
    } else if (error.request) {
      // Request made but no response
      showNotification('Network error. Please check your connection.', 'error');
    } else {
      // Other errors
      showNotification('An unexpected error occurred', 'error');
    }
  } finally {
    setLoading(false);
  }
};
```

---

## Testing

### Test Structure

**42 test files** using **Jest** and **React Testing Library**:

```
src/
├── __tests__/
│   ├── components/
│   │   ├── DataGrid.test.tsx
│   │   └── Card.test.tsx
│   ├── services/
│   │   └── accountService.test.ts
│   ├── hooks/
│   │   └── useDebounce.test.ts
│   └── pages/
│       └── CustomersPage.test.tsx
```

### Test Example

```tsx
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { CustomersPage } from '../pages/CustomersPage';
import { accountService } from '../services/accountService';

// Mock service
jest.mock('../services/accountService');

describe('CustomersPage', () => {
  it('renders account list', async () => {
    const mockAccounts = [
      { id: 1, name: 'Account 1', email: 'test1@example.com' },
      { id: 2, name: 'Account 2', email: 'test2@example.com' }
    ];
    
    (accountService.getAll as jest.Mock).mockResolvedValue(mockAccounts);
    
    render(<CustomersPage />);
    
    await waitFor(() => {
      expect(screen.getByText('Account 1')).toBeInTheDocument();
      expect(screen.getByText('Account 2')).toBeInTheDocument();
    });
  });
  
  it('handles create button click', async () => {
    render(<CustomersPage />);
    
    const createButton = screen.getByText('Create Account');
    await userEvent.click(createButton);
    
    // Assert navigation occurred
  });
});
```

### Running Tests

```bash
# Run all tests
npm test

# Run with coverage
npm test -- --coverage

# Run specific test file
npm test CustomersPage.test.tsx

# Watch mode
npm test -- --watch
```

---

## Best Practices

### 1. Component Design

✅ **DO:**
- Keep components small and focused
- Use functional components with hooks
- Implement proper TypeScript types
- Memoize expensive computations with `useMemo`
- Use `React.memo` for expensive child components

```tsx
const ExpensiveList = React.memo<{ items: Item[] }>(({ items }) => {
  return <div>{items.map(item => <Item key={item.id} {...item} />)}</div>;
});
```

### 2. State Management

✅ **DO:**
- Use local state for component-specific data
- Use Context for shared state
- Keep state minimal and derived
- Avoid prop drilling with Context

❌ **DON'T:**
- Store derived data in state
- Use Context for everything

### 3. Performance

✅ **DO:**
- Use React.lazy for code splitting
- Implement virtualization for long lists
- Debounce search inputs
- Use memoization appropriately

```tsx
const LazyCustomerDetail = React.lazy(() => import('./pages/CustomerDetailPage'));

<Suspense fallback={<Loading />}>
  <LazyCustomerDetail />
</Suspense>
```

### 4. Error Handling

✅ **DO:**
- Implement error boundaries
- Show user-friendly error messages
- Log errors for debugging
- Handle loading states

---

## Troubleshooting

### Common Issues

#### 1. Port 3000 Already in Use

```bash
# Kill process on port 3000
lsof -ti:3000 | xargs kill -9

# Or use different port
PORT=3001 npm start
```

#### 2. API Connection Failed

- Check `REACT_APP_API_BASE_URL` in `.env.local`
- Verify backend is running on http://localhost:5000
- Check CORS configuration in backend

#### 3. Blank Page After Build

- Check browser console for errors
- Verify all environment variables are set
- Check for missing `public/index.html`

#### 4. TypeScript Errors

```bash
# Clear TypeScript cache
rm -rf node_modules/.cache
npm start
```

---

## Additional Resources

- **Backend Developer Guide:** `docs/backend/DEVELOPER_GUIDE.md`
- **Testing Guide:** `docs/testing/TESTING_GUIDE.md`
- **Docker Guide:** `docs/infrastructure/DOCKER_GUIDE.md`
- **Material-UI Documentation:** https://mui.com/
- **React Documentation:** https://react.dev/

---

**Document Version:** 1.0  
**Last Updated:** March 3, 2026  
**Maintained By:** CRM Development Team
