/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU Affero General Public License for more details.
 *
 * You should have received a copy of the GNU Affero General Public License
 * along with this program. If not, see <https://www.gnu.org/licenses/>.
 */

import { useEffect } from 'react';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import { ThemeProvider } from '@mui/material/styles';
import { Container, CssBaseline, Box } from '@mui/material';
import { AuthProvider } from './contexts/AuthContext';
import { LayoutProvider } from './contexts/LayoutContext';
import { ProfileProvider } from './contexts/ProfileContext';
import { BrandingProvider } from './contexts/BrandingContext';
import { AccountContextProvider } from './contexts/AccountContextProvider';
import Navigation from './components/Navigation';
import ContextFlyout from './components/ContextFlyout';
import BreadcrumbsComponent from './components/Breadcrumbs';
import Footer from './components/Footer';
import ProtectedRoute from './components/ProtectedRoute';
import RoleBasedRoute from './components/RoleBasedRoute';
import ErrorBoundary from './components/ErrorBoundary';
import { initializeErrorHandler } from './utils/errorHandler';
import { muiTheme } from './theme/muiTheme';
import LoginPage from './pages/LoginPage';
import RegisterPage from './pages/RegisterPage';
import PasswordResetPage from './pages/PasswordResetPage';
import DashboardPage from './pages/DashboardPage';
import CustomersPage from './pages/CustomersPage';
import ContactsPage from './pages/ContactsPage';
import OpportunitiesPage from './pages/OpportunitiesPage';
import ProductsPage from './pages/ProductsPage';
import CampaignsPage from './pages/CampaignsPage';
import LeadsPage from './pages/LeadsPage';
import ServicesPage from './pages/ServicesPage';
import TwoFactorPage from './pages/TwoFactorPage';
import UserManagementPage from './pages/UserManagementPage';
import DepartmentManagementPage from './pages/DepartmentManagementPage';
import ProfileManagementPage from './pages/ProfileManagementPage';
import SettingsPage from './pages/SettingsPage';
import TasksPage from './pages/TasksPage';
import QuotesPage from './pages/QuotesPage';
import NotesPage from './pages/NotesPage';
import ActivitiesPage from './pages/ActivitiesPage';
import AccountPage from './pages/AccountPage';
import AboutPage from './pages/AboutPage';
import HelpPage from './pages/HelpPage';
import LicensesPage from './pages/LicensesPage';
import ServiceRequestsPage from './pages/ServiceRequestsPage';
import ServiceRequestSettingsPage from './pages/ServiceRequestSettingsPage';
import CustomerOverviewPage from './pages/CustomerOverviewPage';
import CommunicationsPage from './pages/CommunicationsPage';
import InteractionsPage from './pages/InteractionsPage';
import ChannelSettingsPage from './pages/ChannelSettingsPage';
import RelationshipsPage from './pages/RelationshipsPage';
import CampaignExecutionPage from './pages/CampaignExecutionPage';
import {
  DatabaseSettingsPage,
  DeploymentSettingsPage,
  MonitoringSettingsPage,
  SecuritySettingsPage,
  FeatureManagementPage,
  UserManagementSettingsPage,
  UserApprovalPage,
  GroupManagementPage,
  SocialLoginSettingsPage,
  BrandingSettingsPage,
  NavigationSettingsPage,
  ModuleFieldSettingsPage,
  ServiceRequestDefinitionsPage,
  MasterDataSettingsPage,
  DashboardSettingsPage,
  WorkflowListPage,
  WorkflowDesignerPage,
  WorkflowMonitorPage,
  TestResultsPage,
  LLMSettingsPage,
  ApiDocumentationPage,
} from './pages/admin';
import './App.css';

function App() {
  // Initialize global error handler on mount
  useEffect(() => {
    initializeErrorHandler({
      enabled: true,
      logToConsole: true,
      logToLocalStorage: true,
      maxStoredLogs: 200,
      captureNetworkErrors: true,
      captureConsoleErrors: true,
    });
    
    // Log initialization
    console.log('%c🚀 CRM Solution initialized with debug mode enabled', 'color: green; font-weight: bold;');
    console.log('%c💡 Access debug tools via window.CRMDebug', 'color: blue;');
  }, []);

  return (
    <ThemeProvider theme={muiTheme}>
      <CssBaseline />
      <ErrorBoundary>
        <Router>
          <AuthProvider>
            <ProfileProvider>
              <BrandingProvider>
                <LayoutProvider>
                  <AccountContextProvider>
                    <Box sx={{ display: 'flex', flexDirection: 'column', minHeight: '100vh' }}>
                      <Navigation />
                      <BreadcrumbsComponent />
                      <Box sx={{ flex: 1, py: 4, px: 2 }}>
                        <Container maxWidth="lg">
                    <Routes>
              {/* Public Routes */}
              <Route path="/login" element={<LoginPage />} />
              <Route path="/register" element={<RegisterPage />} />
              <Route path="/password-reset" element={<PasswordResetPage />} />

              {/* Protected Routes */}
              <Route
                path="/"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute>
                      <DashboardPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/dashboard"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute requiredPage="Dashboard">
                      <DashboardPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/customers"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute requiredPage="Customers">
                      <CustomersPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/customer-overview"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute requiredPage="Customers">
                      <CustomerOverviewPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/contacts"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute requiredPage="Contacts">
                      <ContactsPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/opportunities"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute requiredPage="Opportunities">
                      <OpportunitiesPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/products"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute requiredPage="Products">
                      <ProductsPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/campaigns"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute requiredPage="Campaigns">
                      <CampaignsPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/campaigns/:campaignId/execution"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute requiredPage="Campaigns">
                      <CampaignExecutionPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/relationships"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute>
                      <RelationshipsPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/leads"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute>
                      <LeadsPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/services"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute>
                      <ServicesPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/service-requests"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute requiredPage="ServiceRequests">
                      <ServiceRequestsPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/service-request-settings"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute>
                      <ServiceRequestSettingsPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/2fa"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute>
                      <TwoFactorPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />

              {/* Management Routes */}
              <Route
                path="/users"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute>
                      <UserManagementPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/departments"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute>
                      <DepartmentManagementPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/profiles"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute>
                      <ProfileManagementPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/settings"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute>
                      <SettingsPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/my-queue"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute>
                      <TasksPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/quotes"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute>
                      <QuotesPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/notes"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute>
                      <NotesPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/activities"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute>
                      <ActivitiesPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/communications"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute requiredPage="Communications">
                      <CommunicationsPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/interactions"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute requiredPage="Interactions">
                      <InteractionsPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/channel-settings"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute>
                      <ChannelSettingsPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />

              {/* Admin Settings Routes */}
              <Route
                path="/admin/database"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute>
                      <DatabaseSettingsPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/admin/deployment"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute>
                      <DeploymentSettingsPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/admin/monitoring"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute>
                      <MonitoringSettingsPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/admin/security"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute>
                      <SecuritySettingsPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/admin/features"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute>
                      <FeatureManagementPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/admin/users"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute>
                      <UserManagementSettingsPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/admin/approvals"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute>
                      <UserApprovalPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/admin/groups"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute>
                      <GroupManagementPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/admin/social-login"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute>
                      <SocialLoginSettingsPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/admin/branding"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute>
                      <BrandingSettingsPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/admin/navigation"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute>
                      <NavigationSettingsPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/admin/modules"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute>
                      <ModuleFieldSettingsPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/admin/service-requests"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute>
                      <ServiceRequestDefinitionsPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/admin/master-data"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute>
                      <MasterDataSettingsPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/admin/dashboards"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute>
                      <DashboardSettingsPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/admin/workflows"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute>
                      <WorkflowListPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/admin/workflows/:id/designer"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute>
                      <WorkflowDesignerPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/admin/workflows/monitor"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute>
                      <WorkflowMonitorPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/admin/workflows/:workflowId/monitor"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute>
                      <WorkflowMonitorPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/admin/test-results"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute>
                      <TestResultsPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/admin/llm"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute>
                      <LLMSettingsPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/accounts/:id"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute>
                      <AccountPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              
              {/* Public Info Routes */}
              <Route path="/about" element={<AboutPage />} />
              <Route path="/help" element={<HelpPage />} />
              <Route path="/help/api" element={<ApiDocumentationPage />} />
              <Route path="/licenses" element={<LicensesPage />} />
                        </Routes>
                        </Container>
                      </Box>
                      <Footer />
                      <ContextFlyout />
                    </Box>
                  </AccountContextProvider>
                </LayoutProvider>
              </BrandingProvider>
            </ProfileProvider>
          </AuthProvider>
        </Router>
      </ErrorBoundary>
    </ThemeProvider>
  );
}

export default App;
