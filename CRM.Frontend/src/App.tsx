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

import { useEffect, lazy, Suspense } from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { ThemeProvider } from '@mui/material/styles';
import { Container, CssBaseline, Box, CircularProgress, Typography } from '@mui/material';
import { AuthProvider } from './contexts/AuthContext';
import { LayoutProvider } from './contexts/LayoutContext';
import { ProfileProvider } from './contexts/ProfileContext';
import { BrandingProvider } from './contexts/BrandingContext';
import { AccountContextProvider } from './contexts/AccountContextProvider';
import { EntityContextProvider } from './contexts/EntityContext';
import { AppThemeProvider, useTheme } from './contexts/ThemeContext';
import { SignalRProvider } from './contexts/SignalRContext';
import Navigation from './components/Navigation';
import ContextFlyout from './components/ContextFlyout';
import BreadcrumbsComponent from './components/Breadcrumbs';
import Footer from './components/Footer';
import ProtectedRoute from './components/ProtectedRoute';
import RoleBasedRoute from './components/RoleBasedRoute';
import ErrorBoundary from './components/ErrorBoundary';
import { initializeErrorHandler } from './utils/errorHandler';
// Core/Auth Pages (loaded immediately for fast initial load)
import LoginPage from './pages/LoginPage';
import RegisterPage from './pages/RegisterPage';
import PasswordResetPage from './pages/PasswordResetPage';
import SetupPasswordPage from './pages/SetupPasswordPage';
import './App.css';

// ============================================================================
// LAZY LOADED MODULES - Code Splitting for Performance
// ============================================================================

// Loading fallback component
const LoadingFallback = () => (
  <Box
    sx={{
      display: 'flex',
      flexDirection: 'column',
      alignItems: 'center',
      justifyContent: 'center',
      minHeight: '200px',
      gap: 2,
    }}
  >
    <CircularProgress size={40} />
    <Typography variant="body2" color="text.secondary">
      Loading module...
    </Typography>
  </Box>
);

// ----------------------------------------------------------------------------
// Sales Module - Lazy Loaded
// ----------------------------------------------------------------------------
const OpportunitiesPage = lazy(() => import('./pages/OpportunitiesPage'));
const QuotesPage = lazy(() => import('./pages/QuotesPage'));
const ProductsPage = lazy(() => import('./pages/ProductsPage'));
const ContractsPage = lazy(() => import('./pages/ContractsPage'));
const InvoicesPage = lazy(() => import('./pages/InvoicesPage'));
const PaymentsPage = lazy(() => import('./pages/PaymentsPage'));
const OrdersPage = lazy(() => import('./pages/OrdersPage'));
const TeamsPage = lazy(() => import('./pages/TeamsPage'));
const SubscriptionsPage = lazy(() => import('./pages/SubscriptionsPage'));
const CommissionsPage = lazy(() => import('./pages/CommissionsPage'));
const TerritoriesPage = lazy(() => import('./pages/TerritoriesPage'));
const ApprovalsPage = lazy(() => import('./pages/ApprovalsPage'));

// ----------------------------------------------------------------------------
// Marketing Module - Lazy Loaded
// ----------------------------------------------------------------------------
const LeadsPage = lazy(() => import('./pages/LeadsPage'));
const CampaignsPage = lazy(() => import('./pages/CampaignsPage'));
const CampaignExecutionPage = lazy(() => import('./pages/CampaignExecutionPage'));
const EmailTemplatesPage = lazy(() => import('./pages/EmailTemplatesPage'));
const LandingPagesPage = lazy(() => import('./pages/LandingPagesPage'));
const LeadRoutingPage = lazy(() => import('./pages/LeadRoutingPage'));

// ----------------------------------------------------------------------------
// Service Module - Lazy Loaded
// ----------------------------------------------------------------------------
const ServiceRequestsPage = lazy(() => import('./pages/ServiceRequestsPage'));
const ServiceRequestSettingsPage = lazy(() => import('./pages/ServiceRequestSettingsPage'));
const KnowledgeBasePage = lazy(() => import('./pages/KnowledgeBasePage'));
const ServicesPage = lazy(() => import('./pages/ServicesPage'));

// ----------------------------------------------------------------------------
// ITSM Module - Lazy Loaded
// ----------------------------------------------------------------------------
const ITSMOverviewPage = lazy(() => import('./pages/itsm/ITSMOverviewPage'));
const ITSMMetricsPage = lazy(() => import('./pages/itsm/ITSMMetricsPage'));
const IncidentListPage = lazy(() => import('./pages/itsm/IncidentListPage'));
const IncidentFormPage = lazy(() => import('./pages/itsm/IncidentFormPage'));
const IncidentDetailPage = lazy(() => import('./pages/itsm/IncidentDetailPage'));
const ProblemListPage = lazy(() => import('./pages/itsm/ProblemListPage'));
const ProblemFormPage = lazy(() => import('./pages/itsm/ProblemFormPage'));
const ProblemDetailPage = lazy(() => import('./pages/itsm/ProblemDetailPage'));
const CMDBListPage = lazy(() => import('./pages/itsm/CMDBListPage'));
const CMDBFormPage = lazy(() => import('./pages/itsm/CMDBFormPage'));
const CMDBDetailPage = lazy(() => import('./pages/itsm/CMDBDetailPage'));
const CMDBRelationshipMapPage = lazy(() => import('./pages/itsm/CMDBRelationshipMapPage'));
const CMDBImpactAnalysisPage = lazy(() => import('./pages/itsm/CMDBImpactAnalysisPage'));
const ChangeListPage = lazy(() => import('./pages/itsm/ChangeListPage'));
const ChangeFormPage = lazy(() => import('./pages/itsm/ChangeFormPage'));
const ChangeDetailPage = lazy(() => import('./pages/itsm/ChangeDetailPage'));
const ChangeApprovalPage = lazy(() => import('./pages/itsm/ChangeApprovalPage'));
const ChangeCalendarPage = lazy(() => import('./pages/itsm/ChangeCalendarPage'));
const KnowledgeBaseListPage = lazy(() => import('./pages/itsm/KnowledgeBaseListPage'));
const KnowledgeArticleDetailPage = lazy(() => import('./pages/itsm/KnowledgeArticleDetailPage'));
const KnowledgeArticleEditorPage = lazy(() => import('./pages/itsm/KnowledgeArticleEditorPage'));
const KnowledgeArticleApprovalPage = lazy(() => import('./pages/itsm/KnowledgeArticleApprovalPage'));
const ServiceCatalogPage = lazy(() => import('./pages/itsm/ServiceCatalogPage'));
const ServiceCatalogAdminPage = lazy(() => import('./pages/itsm/ServiceCatalogAdminPage'));
const ServiceCatalogRequestListPage = lazy(() => import('./pages/itsm/ServiceCatalogRequestListPage'));
const ServiceCatalogRequestDetailPage = lazy(() => import('./pages/itsm/ServiceCatalogRequestDetailPage'));
const ServiceCatalogRequestCreatePage = lazy(() => import('./pages/itsm/ServiceCatalogRequestCreatePage'));
const SLADashboardPage = lazy(() => import('./pages/itsm/SLADashboardPage'));
const SLAPolicyListPage = lazy(() => import('./pages/itsm/SLAPolicyListPage'));
const SLAPolicyFormPage = lazy(() => import('./pages/itsm/SLAPolicyFormPage'));
const SLAInstanceListPage = lazy(() => import('./pages/itsm/SLAInstanceListPage'));

// ----------------------------------------------------------------------------
// Account Module - Lazy Loaded
// ----------------------------------------------------------------------------
const AccountsPage = lazy(() => import('./pages/AccountsPage'));
const ContactsPage = lazy(() => import('./pages/ContactsPage'));
const AccountOverviewPage = lazy(() => import('./pages/CustomerOverviewPage'));
const RelationshipsPage = lazy(() => import('./pages/RelationshipsPage'));

// ----------------------------------------------------------------------------
// Communication Module - Lazy Loaded
// ----------------------------------------------------------------------------
const CommunicationsPage = lazy(() => import('./pages/CommunicationsPage'));
const InteractionsPage = lazy(() => import('./pages/InteractionsPage'));
const ChannelSettingsPage = lazy(() => import('./pages/ChannelSettingsPage'));

// ----------------------------------------------------------------------------
// Productivity Module - Lazy Loaded
// ----------------------------------------------------------------------------
const DashboardPage = lazy(() => import('./pages/DashboardPage'));
const TasksPage = lazy(() => import('./pages/TasksPage'));
const NotesPage = lazy(() => import('./pages/NotesPage'));
const ActivitiesPage = lazy(() => import('./pages/ActivitiesPage'));
const ReportsPage = lazy(() => import('./pages/ReportsPage'));

// ----------------------------------------------------------------------------
// Account & Profile Pages - Lazy Loaded
// ----------------------------------------------------------------------------
const AccountPage = lazy(() => import('./pages/AccountPage'));
const TwoFactorPage = lazy(() => import('./pages/TwoFactorPage'));
const SettingsPage = lazy(() => import('./pages/SettingsPage'));
const UserManagementPage = lazy(() => import('./pages/UserManagementPage'));
const DepartmentManagementPage = lazy(() => import('./pages/DepartmentManagementPage'));
const ProfileManagementPage = lazy(() => import('./pages/ProfileManagementPage'));

// ----------------------------------------------------------------------------
// Info/Help Pages - Lazy Loaded
// ----------------------------------------------------------------------------
const AboutPage = lazy(() => import('./pages/AboutPage'));
const HelpPage = lazy(() => import('./pages/HelpPage'));
const LicensesPage = lazy(() => import('./pages/LicensesPage'));
const FormBuilderPage = lazy(() => import('./pages/FormBuilderPage'));

// ----------------------------------------------------------------------------
// Admin Module - Lazy Loaded (heavy components)
// ----------------------------------------------------------------------------
const DeploymentSettingsPage = lazy(() => import('./pages/admin/DeploymentSettingsPage'));
const MonitoringSettingsPage = lazy(() => import('./pages/admin/MonitoringSettingsPage'));
const SecuritySettingsPage = lazy(() => import('./pages/admin/SecuritySettingsPage'));
const FeatureManagementPage = lazy(() => import('./pages/admin/FeatureManagementPage'));
const UserManagementSettingsPage = lazy(() => import('./pages/admin/UserManagementSettingsPage'));
const UserApprovalPage = lazy(() => import('./pages/admin/UserApprovalPage'));
const GroupManagementPage = lazy(() => import('./pages/admin/GroupManagementPage'));
const SocialLoginSettingsPage = lazy(() => import('./pages/admin/SocialLoginSettingsPage'));
const BrandingSettingsPage = lazy(() => import('./pages/admin/BrandingSettingsPage'));
const NavigationSettingsPage = lazy(() => import('./pages/admin/NavigationSettingsPage'));
const ModuleFieldSettingsPage = lazy(() => import('./pages/admin/ModuleFieldSettingsPage'));
const ServiceRequestDefinitionsPage = lazy(() => import('./pages/admin/ServiceRequestDefinitionsPage'));
const MasterDataSettingsPage = lazy(() => import('./pages/admin/MasterDataSettingsPage'));
const DashboardSettingsPage = lazy(() => import('./pages/admin/DashboardSettingsPage'));
const WorkflowListPage = lazy(() => import('./pages/admin/WorkflowListPage'));
const WorkflowDesignerPage = lazy(() => import('./pages/admin/WorkflowDesignerPage'));
const WorkflowMonitorPage = lazy(() => import('./pages/admin/WorkflowMonitorPage'));
const WorkflowInstancesPage = lazy(() => import('./pages/admin/WorkflowInstancesPage'));
const WorkflowInstanceDetailPage = lazy(() => import('./pages/admin/WorkflowInstanceDetailPage'));
const WorkflowTemplatesPage = lazy(() => import('./pages/admin/WorkflowTemplatesPage'));
const WorkflowTasksPage = lazy(() => import('./pages/WorkflowTasksPage'));
const TestResultsPage = lazy(() => import('./pages/admin/TestResultsPage'));
const LLMSettingsPage = lazy(() => import('./pages/admin/LLMSettingsPage'));
const ApiDocumentationPage = lazy(() => import('./pages/admin/ApiDocumentationPage'));
const DatabaseSettingsPage = lazy(() => import('./pages/admin/DatabaseSettingsPage'));
const DuplicateRulesPage = lazy(() => import('./pages/admin/DuplicateRulesPage'));
const LeadScoreRulesPage = lazy(() => import('./pages/admin/LeadScoreRulesPage'));
const IntegrationsSettingsPage = lazy(() => import('./pages/admin/IntegrationsSettingsPage'));
const AnalyticsSettingsPage = lazy(() => import('./pages/admin/AnalyticsSettingsPage'));

// Analytics & Reports Pages (main navigation)
const AnalyticsPage = lazy(() => import('./pages/AnalyticsPage'));

// AI Agent Pages
const AgentDirectoryPage = lazy(() => import('./pages/AgentDirectoryPage'));
const AgentChatPage = lazy(() => import('./pages/AgentChatPage'));
const AgentManagementPage = lazy(() => import('./pages/AgentManagementPage'));
const AgentApprovalsPage = lazy(() => import('./pages/AgentApprovalsPage'));
const AgentAnalyticsPage = lazy(() => import('./pages/AgentAnalyticsPage'));
const ConversationHistoryPage = lazy(() => import('./pages/ConversationHistoryPage'));
const AgentCreatorPage = lazy(() => import('./pages/AgentCreatorPage'));

// Inner component that can access the theme context
function ThemedApp() {
  const { theme } = useTheme();

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
    <ThemeProvider theme={theme}>
      <CssBaseline />
      <ErrorBoundary>
        <Router>
          <AuthProvider>
            <SignalRProvider>
            <ProfileProvider>
              <BrandingProvider>
                <LayoutProvider>
                  <AccountContextProvider>
                    <EntityContextProvider>
                      <Box sx={{ display: 'flex', flexDirection: 'column', minHeight: '100vh' }}>
                        <Navigation />
                        <BreadcrumbsComponent />
                        <Box sx={{ flex: 1, py: 4, px: 2 }}>
                          <Container maxWidth="lg">
                    <Suspense fallback={<LoadingFallback />}>
                    <Routes>
              {/* Public Routes */}
              <Route path="/login" element={<LoginPage />} />
              <Route path="/register" element={<RegisterPage />} />
              <Route path="/password-reset" element={<PasswordResetPage />} />
              <Route path="/setup-password" element={<SetupPasswordPage />} />

              {/* Protected Routes */}
              <Route
                path="/"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute requiredPage="MyQueue">
                      <TasksPage />
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
                path="/tasks"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute requiredPage="MyQueue">
                      <WorkflowTasksPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/accounts"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute requiredPage="Accounts">
                      <AccountsPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/account-overview"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute requiredPage="Accounts">
                      <AccountOverviewPage />
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
                path="/email-templates"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute requiredPage="Campaigns">
                      <EmailTemplatesPage />
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
                path="/campaign-execution"
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
                    <RoleBasedRoute requiredPage="Accounts">
                      <RelationshipsPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/territories"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute requiredPage="Settings">
                      <TerritoriesPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/lead-routing"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute requiredPage="Leads">
                      <LeadRoutingPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/approvals"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute requiredPage="Settings">
                      <ApprovalsPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/leads"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute requiredPage="Leads">
                      <LeadsPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/services"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute requiredPage="Services">
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
                    <RoleBasedRoute requiredPage="Settings">
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
                    <RoleBasedRoute requiredPage="User Management">
                      <UserManagementPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/departments"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute requiredPage="User Management">
                      <DepartmentManagementPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/profiles"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute requiredPage="User Management">
                      <ProfileManagementPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/settings"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute requiredPage="Settings">
                      <SettingsPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/my-queue"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute requiredPage="Tasks">
                      <TasksPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/quotes"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute requiredPage="Quotes">
                      <QuotesPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/contracts"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute requiredPage="Contracts">
                      <ContractsPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/commissions"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute requiredPage="Commissions">
                      <CommissionsPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/invoices"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute requiredPage="Invoices">
                      <InvoicesPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/payments"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute requiredPage="Payments">
                      <PaymentsPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/orders"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute requiredPage="Orders">
                      <OrdersPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/subscriptions"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute requiredPage="Subscriptions">
                      <SubscriptionsPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/teams"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute requiredPage="Teams">
                      <TeamsPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/forms"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute requiredPage="Forms">
                      <FormBuilderPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/landing-pages"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute requiredPage="LandingPages">
                      <LandingPagesPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/knowledge-base"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute requiredPage="KnowledgeBase">
                      <KnowledgeBasePage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/notes"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute requiredPage="Notes">
                      <NotesPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/activities"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute requiredPage="Activities">
                      <ActivitiesPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/reports"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute requiredPage="Reports">
                      <ReportsPage />
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
                    <RoleBasedRoute requiredPage="Settings">
                      <ChannelSettingsPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />

              {/* ITSM Routes */}
              <Route
                path="/itsm"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute requiredPage="ITSMOverview" moduleName="ITSM">
                      <ITSMOverviewPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/itsm/metrics"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute requiredPage="ITSMMetrics" moduleName="ITSM">
                      <ITSMMetricsPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />

              <Route
                path="/itsm/incidents"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute requiredPage="ITSMIncidents" moduleName="ITSM">
                      <IncidentListPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/itsm/incidents/create"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute requiredPage="ITSMIncidents" moduleName="ITSM">
                      <IncidentFormPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/itsm/incidents/:id"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute requiredPage="ITSMIncidents" moduleName="ITSM">
                      <IncidentDetailPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />

              <Route
                path="/itsm/problems"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute requiredPage="ITSMProblems" moduleName="ITSM">
                      <ProblemListPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/itsm/problems/create"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute requiredPage="ITSMProblems" moduleName="ITSM">
                      <ProblemFormPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/itsm/problems/:id"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute requiredPage="ITSMProblems" moduleName="ITSM">
                      <ProblemDetailPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/itsm/problems/:id/edit"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute requiredPage="ITSMProblems" moduleName="ITSM">
                      <ProblemFormPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />

              <Route
                path="/itsm/cmdb"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute requiredPage="ITSMCMDB" moduleName="ITSM">
                      <CMDBListPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/itsm/cmdb/create"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute requiredPage="ITSMCMDB" moduleName="ITSM">
                      <CMDBFormPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/itsm/cmdb/:id"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute requiredPage="ITSMCMDB" moduleName="ITSM">
                      <CMDBDetailPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/itsm/cmdb/:id/relationships"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute requiredPage="ITSMCMDB" moduleName="ITSM">
                      <CMDBRelationshipMapPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/itsm/cmdb/:id/impact"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute requiredPage="ITSMCMDB" moduleName="ITSM">
                      <CMDBImpactAnalysisPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />

              <Route
                path="/itsm/changes"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute requiredPage="ITSMChanges" moduleName="ITSM">
                      <ChangeListPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/itsm/changes/create"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute requiredPage="ITSMChanges" moduleName="ITSM">
                      <ChangeFormPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/itsm/changes/calendar"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute requiredPage="ITSMChanges" moduleName="ITSM">
                      <ChangeCalendarPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/itsm/changes/:id"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute requiredPage="ITSMChanges" moduleName="ITSM">
                      <ChangeDetailPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/itsm/changes/:id/edit"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute requiredPage="ITSMChanges" moduleName="ITSM">
                      <ChangeFormPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/itsm/changes/:id/approval"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute requiredPage="ITSMChanges" moduleName="ITSM">
                      <ChangeApprovalPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />

              <Route
                path="/itsm/knowledge"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute requiredPage="ITSMKnowledge" moduleName="ITSM">
                      <KnowledgeBaseListPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/itsm/knowledge/editor"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute requiredPage="ITSMKnowledge" moduleName="ITSM">
                      <KnowledgeArticleEditorPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/itsm/knowledge/approvals"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute requiredPage="ITSMKnowledge" moduleName="ITSM">
                      <KnowledgeArticleApprovalPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/itsm/knowledge/:id"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute requiredPage="ITSMKnowledge" moduleName="ITSM">
                      <KnowledgeArticleDetailPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />

              <Route
                path="/itsm/catalog"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute requiredPage="ITSMCatalog" moduleName="ITSM">
                      <ServiceCatalogPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/itsm/catalog/admin"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute requiredPage="ITSMCatalog" moduleName="ITSM">
                      <ServiceCatalogAdminPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/itsm/catalog/requests"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute requiredPage="ITSMCatalog" moduleName="ITSM">
                      <ServiceCatalogRequestListPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/itsm/catalog/requests/:id"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute requiredPage="ITSMCatalog" moduleName="ITSM">
                      <ServiceCatalogRequestDetailPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/itsm/catalog/:id/request"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute requiredPage="ITSMCatalog" moduleName="ITSM">
                      <ServiceCatalogRequestCreatePage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />

              <Route
                path="/itsm/sla"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute requiredPage="ITSMSLA" moduleName="ITSM">
                      <SLADashboardPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/itsm/sla/policies"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute requiredPage="ITSMSLA" moduleName="ITSM">
                      <SLAPolicyListPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/itsm/sla/policies/create"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute requiredPage="ITSMSLA" moduleName="ITSM">
                      <SLAPolicyFormPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/itsm/sla/instances"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute requiredPage="ITSMSLA" moduleName="ITSM">
                      <SLAInstanceListPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />

              {/* Admin Settings Routes */}
              <Route
                path="/admin/database"
                element={<Navigate to="/admin/monitoring" replace />}
              />
              <Route
                path="/admin/deployment"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute requiredPage="Settings">
                      <DeploymentSettingsPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/admin/monitoring"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute requiredPage="Settings">
                      <MonitoringSettingsPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/admin/security"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute requiredPage="Settings">
                      <SecuritySettingsPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/admin/features"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute requiredPage="Settings">
                      <FeatureManagementPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/admin/users"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute requiredPage="User Management">
                      <UserManagementSettingsPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/admin/approvals"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute requiredPage="User Management">
                      <UserApprovalPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/admin/groups"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute requiredPage="User Management">
                      <GroupManagementPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/admin/social-login"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute requiredPage="Settings">
                      <SocialLoginSettingsPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/admin/branding"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute requiredPage="Settings">
                      <BrandingSettingsPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/admin/navigation"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute requiredPage="Settings">
                      <NavigationSettingsPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/admin/modules"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute requiredPage="Settings">
                      <ModuleFieldSettingsPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/admin/service-requests"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute requiredPage="Settings">
                      <ServiceRequestDefinitionsPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/admin/master-data"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute requiredPage="Settings">
                      <MasterDataSettingsPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/admin/dashboards"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute requiredPage="Settings">
                      <DashboardSettingsPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/admin/workflows"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute requiredPage="Settings">
                      <WorkflowListPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/admin/workflows/:id/designer"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute requiredPage="Settings">
                      <WorkflowDesignerPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/admin/workflows/monitor"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute requiredPage="Settings">
                      <WorkflowMonitorPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/admin/workflows/instances"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute requiredPage="Settings">
                      <WorkflowInstancesPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/admin/workflows/instances/:id"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute requiredPage="Settings">
                      <WorkflowInstanceDetailPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/admin/workflows/:workflowId/monitor"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute requiredPage="Settings">
                      <WorkflowMonitorPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/admin/workflows/templates"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute requiredPage="Settings">
                      <WorkflowTemplatesPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/admin/test-results"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute requiredPage="Settings">
                      <TestResultsPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/admin/llm"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute requiredPage="Settings">
                      <LLMSettingsPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/admin/database-settings"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute requiredPage="Settings">
                      <DatabaseSettingsPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/admin/duplicate-rules"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute requiredPage="Settings">
                      <DuplicateRulesPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/admin/lead-score-rules"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute requiredPage="Settings">
                      <LeadScoreRulesPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/admin/integrations"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute requiredPage="Settings">
                      <IntegrationsSettingsPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/admin/analytics"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute requiredPage="Settings">
                      <AnalyticsSettingsPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/analytics"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute requiredPage="Reports">
                      <AnalyticsPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/accounts/:id"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute requiredPage="Accounts">
                      <AccountPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              
              {/* AI Agents Routes */}
              <Route
                path="/agents"
                element={
                  <ProtectedRoute>
                    <AgentDirectoryPage />
                  </ProtectedRoute>
                }
              />
              <Route
                path="/agents/conversations"
                element={
                  <ProtectedRoute>
                    <ConversationHistoryPage />
                  </ProtectedRoute>
                }
              />
              <Route
                path="/agents/:agentId/chat"
                element={
                  <ProtectedRoute>
                    <AgentChatPage />
                  </ProtectedRoute>
                }
              />
              <Route
                path="/admin/agents/new"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute requiredPage="Settings">
                      <AgentCreatorPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/admin/agents"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute requiredPage="Settings">
                      <AgentManagementPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/admin/agents/approvals"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute requiredPage="Settings">
                      <AgentApprovalsPage />
                    </RoleBasedRoute>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/admin/agents/analytics"
                element={
                  <ProtectedRoute>
                    <RoleBasedRoute requiredPage="Settings">
                      <AgentAnalyticsPage />
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
                        </Suspense>
                        </Container>
                      </Box>
                      <Footer />
                      <ContextFlyout />
                    </Box>
                  </EntityContextProvider>
                  </AccountContextProvider>
                </LayoutProvider>
              </BrandingProvider>
            </ProfileProvider>
            </SignalRProvider>
          </AuthProvider>
        </Router>
      </ErrorBoundary>
    </ThemeProvider>
  );
}

// Main App component that wraps everything with the theme provider
function App() {
  return (
    <AppThemeProvider>
      <ThemedApp />
    </AppThemeProvider>
  );
}

export default App;
