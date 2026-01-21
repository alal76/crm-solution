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

import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import { ThemeProvider } from '@mui/material/styles';
import { Container, CssBaseline, Box } from '@mui/material';
import { AuthProvider } from './contexts/AuthContext';
import { LayoutProvider } from './contexts/LayoutContext';
import { ProfileProvider } from './contexts/ProfileContext';
import { BrandingProvider } from './contexts/BrandingContext';
import Navigation from './components/Navigation';
import BreadcrumbsComponent from './components/Breadcrumbs';
import Footer from './components/Footer';
import ProtectedRoute from './components/ProtectedRoute';
import RoleBasedRoute from './components/RoleBasedRoute';
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
import WorkflowsPage from './pages/WorkflowsPage';
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
import './App.css';

function App() {
  return (
    <ThemeProvider theme={muiTheme}>
      <CssBaseline />
      <Router>
        <AuthProvider>
          <ProfileProvider>
            <BrandingProvider>
              <LayoutProvider>
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
                path="/workflows"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute>
                      <WorkflowsPage />
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
                path="/tasks"
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
              <Route path="/licenses" element={<LicensesPage />} />
                </Routes>
                    </Container>
                  </Box>
                  <Footer />
                </Box>
              </LayoutProvider>
            </BrandingProvider>
          </ProfileProvider>
        </AuthProvider>
      </Router>
    </ThemeProvider>
  );
}

export default App;
