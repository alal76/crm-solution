import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import { ThemeProvider } from '@mui/material/styles';
import { Container, CssBaseline, Box } from '@mui/material';
import { AuthProvider } from './contexts/AuthContext';
import { LayoutProvider } from './contexts/LayoutContext';
import { ProfileProvider } from './contexts/ProfileContext';
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
import './App.css';

function App() {
  return (
    <ThemeProvider theme={muiTheme}>
      <CssBaseline />
      <Router>
        <AuthProvider>
          <ProfileProvider>
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
                </Routes>
                  </Container>
                </Box>
                <Footer />
              </Box>
            </LayoutProvider>
          </ProfileProvider>
        </AuthProvider>
      </Router>
    </ThemeProvider>
  );
}

export default App;
