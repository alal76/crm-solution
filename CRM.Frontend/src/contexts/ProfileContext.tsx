import React, { createContext, useState, useContext, useEffect, useCallback } from 'react';
import { useAuth } from './AuthContext';
import { getApiEndpoint } from '../config/ports';

// Group permissions for menu and CRUD access
interface GroupPermissions {
  isSystemAdmin: boolean;
  
  // Menu Access
  canAccessDashboard: boolean;
  canAccessCustomers: boolean;
  canAccessContacts: boolean;
  canAccessLeads: boolean;
  canAccessOpportunities: boolean;
  canAccessProducts: boolean;
  canAccessServices: boolean;
  canAccessCampaigns: boolean;
  canAccessQuotes: boolean;
  canAccessTasks: boolean;
  canAccessActivities: boolean;
  canAccessNotes: boolean;
  canAccessWorkflows: boolean;
  canAccessServiceRequests: boolean;
  canAccessReports: boolean;
  canAccessSettings: boolean;
  canAccessUserManagement: boolean;
  
  // CRUD Permissions
  canCreateCustomers: boolean;
  canEditCustomers: boolean;
  canDeleteCustomers: boolean;
  canViewAllCustomers: boolean;
  
  canCreateContacts: boolean;
  canEditContacts: boolean;
  canDeleteContacts: boolean;
  
  canCreateLeads: boolean;
  canEditLeads: boolean;
  canDeleteLeads: boolean;
  canConvertLeads: boolean;
  
  canCreateOpportunities: boolean;
  canEditOpportunities: boolean;
  canDeleteOpportunities: boolean;
  canCloseOpportunities: boolean;
  
  canCreateProducts: boolean;
  canEditProducts: boolean;
  canDeleteProducts: boolean;
  canManagePricing: boolean;
  
  canCreateCampaigns: boolean;
  canEditCampaigns: boolean;
  canDeleteCampaigns: boolean;
  canLaunchCampaigns: boolean;
  
  canCreateQuotes: boolean;
  canEditQuotes: boolean;
  canDeleteQuotes: boolean;
  canApproveQuotes: boolean;
  
  canCreateTasks: boolean;
  canEditTasks: boolean;
  canDeleteTasks: boolean;
  canAssignTasks: boolean;
  
  canCreateWorkflows: boolean;
  canEditWorkflows: boolean;
  canDeleteWorkflows: boolean;
  canActivateWorkflows: boolean;
  
  // Data Access
  dataAccessScope: string;
  canExportData: boolean;
  canImportData: boolean;
  canBulkEdit: boolean;
  canBulkDelete: boolean;
}

// Module status from system settings
interface ModuleStatus {
  customersEnabled: boolean;
  contactsEnabled: boolean;
  leadsEnabled: boolean;
  opportunitiesEnabled: boolean;
  productsEnabled: boolean;
  servicesEnabled: boolean;
  campaignsEnabled: boolean;
  quotesEnabled: boolean;
  tasksEnabled: boolean;
  activitiesEnabled: boolean;
  notesEnabled: boolean;
  workflowsEnabled: boolean;
  reportsEnabled: boolean;
  dashboardEnabled: boolean;
}

// Legacy permissions interface for backward compatibility
interface UserPermissions {
  canCreateCustomers: boolean;
  canEditCustomers: boolean;
  canDeleteCustomers: boolean;
  canCreateOpportunities: boolean;
  canEditOpportunities: boolean;
  canDeleteOpportunities: boolean;
  canCreateProducts: boolean;
  canEditProducts: boolean;
  canDeleteProducts: boolean;
  canManageCampaigns: boolean;
  canViewReports: boolean;
  canManageUsers: boolean;
}

interface UserProfile {
  departmentId?: number;
  departmentName?: string;
  userProfileId?: number;
  userProfileName?: string;
  accessiblePages: string[];
  permissions: UserPermissions;
  groupPermissions?: GroupPermissions;
}

interface ProfileContextType {
  profile: UserProfile | null;
  moduleStatus: ModuleStatus | null;
  isLoading: boolean;
  canAccessPage: (pageName: string) => boolean;
  canAccessMenu: (menuName: string) => boolean;
  hasPermission: (permission: keyof UserPermissions | keyof GroupPermissions) => boolean;
  isModuleEnabled: (moduleName: string) => boolean;
  updateProfile: (profile: UserProfile) => void;
  refreshModuleStatus: () => Promise<void>;
}

const defaultModuleStatus: ModuleStatus = {
  customersEnabled: true,
  contactsEnabled: true,
  leadsEnabled: true,
  opportunitiesEnabled: true,
  productsEnabled: true,
  servicesEnabled: true,
  campaignsEnabled: true,
  quotesEnabled: true,
  tasksEnabled: true,
  activitiesEnabled: true,
  notesEnabled: true,
  workflowsEnabled: true,
  reportsEnabled: true,
  dashboardEnabled: true
};

const defaultGroupPermissions: GroupPermissions = {
  isSystemAdmin: false,
  canAccessDashboard: true,
  canAccessCustomers: false,
  canAccessContacts: false,
  canAccessLeads: false,
  canAccessOpportunities: false,
  canAccessProducts: false,
  canAccessServices: false,
  canAccessCampaigns: false,
  canAccessQuotes: false,
  canAccessTasks: false,
  canAccessActivities: false,
  canAccessNotes: false,
  canAccessWorkflows: false,
  canAccessServiceRequests: false,
  canAccessReports: false,
  canAccessSettings: false,
  canAccessUserManagement: false,
  canCreateCustomers: false,
  canEditCustomers: false,
  canDeleteCustomers: false,
  canViewAllCustomers: false,
  canCreateContacts: false,
  canEditContacts: false,
  canDeleteContacts: false,
  canCreateLeads: false,
  canEditLeads: false,
  canDeleteLeads: false,
  canConvertLeads: false,
  canCreateOpportunities: false,
  canEditOpportunities: false,
  canDeleteOpportunities: false,
  canCloseOpportunities: false,
  canCreateProducts: false,
  canEditProducts: false,
  canDeleteProducts: false,
  canManagePricing: false,
  canCreateCampaigns: false,
  canEditCampaigns: false,
  canDeleteCampaigns: false,
  canLaunchCampaigns: false,
  canCreateQuotes: false,
  canEditQuotes: false,
  canDeleteQuotes: false,
  canApproveQuotes: false,
  canCreateTasks: false,
  canEditTasks: false,
  canDeleteTasks: false,
  canAssignTasks: false,
  canCreateWorkflows: false,
  canEditWorkflows: false,
  canDeleteWorkflows: false,
  canActivateWorkflows: false,
  dataAccessScope: 'own',
  canExportData: false,
  canImportData: false,
  canBulkEdit: false,
  canBulkDelete: false
};

const ProfileContext = createContext<ProfileContextType | undefined>(undefined);

export const ProfileProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [profile, setProfile] = useState<UserProfile | null>(null);
  const [moduleStatus, setModuleStatus] = useState<ModuleStatus | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const { isAuthenticated } = useAuth();
  
  const getToken = () => localStorage.getItem('accessToken');

  const fetchModuleStatus = useCallback(async () => {
    const token = getToken();
    if (!token) return;
    
    try {
      const response = await fetch(getApiEndpoint('/systemsettings/modules'), {
        headers: {
          'Authorization': `Bearer ${token}`
        }
      });
      
      if (response.ok) {
        const data = await response.json();
        setModuleStatus(data);
        localStorage.setItem('moduleStatus', JSON.stringify(data));
      }
    } catch (error) {
      console.error('Failed to fetch module status', error);
      // Load from localStorage as fallback
      const stored = localStorage.getItem('moduleStatus');
      if (stored) {
        setModuleStatus(JSON.parse(stored));
      }
    }
  }, []);

  useEffect(() => {
    // Load profile from auth context or localStorage
    loadProfileFromStorage();
    
    // Fetch module status if we are authenticated
    if (isAuthenticated) {
      fetchModuleStatus();
    }
  }, [isAuthenticated, fetchModuleStatus]);

  const loadProfileFromStorage = () => {
    try {
      const storedProfile = localStorage.getItem('userProfile');
      if (storedProfile) {
        setProfile(JSON.parse(storedProfile));
      }
      
      const storedModuleStatus = localStorage.getItem('moduleStatus');
      if (storedModuleStatus) {
        setModuleStatus(JSON.parse(storedModuleStatus));
      }
    } catch (error) {
      console.error('Failed to load profile from storage', error);
    }
  };

  const canAccessPage = (pageName: string): boolean => {
    // If profile not loaded yet, allow access (will check again when loaded)
    if (!profile) return true;
    
    // System admins have access to everything
    if (profile.groupPermissions?.isSystemAdmin) return true;
    
    // If no accessible pages are specified, allow access to all pages (admin/default behavior)
    if (profile.accessiblePages.length === 0) return true;
    
    // Otherwise, check if the page is in the accessible pages list
    return profile.accessiblePages.includes(pageName);
  };
  
  const canAccessMenu = (menuName: string): boolean => {
    // Check if module is enabled globally first
    const modules = moduleStatus || defaultModuleStatus;
    const groupPerms = profile?.groupPermissions || defaultGroupPermissions;
    
    // System admins can access everything
    if (groupPerms.isSystemAdmin) return true;
    
    // Map menu name to module enabled status and group permission
    const menuMap: Record<string, { enabled: boolean; permitted: boolean }> = {
      'Dashboard': { enabled: modules.dashboardEnabled, permitted: groupPerms.canAccessDashboard },
      'Customers': { enabled: modules.customersEnabled, permitted: groupPerms.canAccessCustomers },
      'Contacts': { enabled: modules.contactsEnabled, permitted: groupPerms.canAccessContacts },
      'Leads': { enabled: modules.leadsEnabled, permitted: groupPerms.canAccessLeads },
      'Opportunities': { enabled: modules.opportunitiesEnabled, permitted: groupPerms.canAccessOpportunities },
      'Products': { enabled: modules.productsEnabled, permitted: groupPerms.canAccessProducts },
      'Services': { enabled: modules.servicesEnabled, permitted: groupPerms.canAccessServices },
      'Campaigns': { enabled: modules.campaignsEnabled, permitted: groupPerms.canAccessCampaigns },
      'Quotes': { enabled: modules.quotesEnabled, permitted: groupPerms.canAccessQuotes },
      'Tasks': { enabled: modules.tasksEnabled, permitted: groupPerms.canAccessTasks },
      'Activities': { enabled: modules.activitiesEnabled, permitted: groupPerms.canAccessActivities },
      'Notes': { enabled: modules.notesEnabled, permitted: groupPerms.canAccessNotes },
      'Workflows': { enabled: modules.workflowsEnabled, permitted: groupPerms.canAccessWorkflows },
      'ServiceRequests': { enabled: true, permitted: groupPerms.canAccessServiceRequests },
      'Reports': { enabled: modules.reportsEnabled, permitted: groupPerms.canAccessReports },
      'Settings': { enabled: true, permitted: groupPerms.canAccessSettings },
      'User Management': { enabled: true, permitted: groupPerms.canAccessUserManagement }
    };
    
    const access = menuMap[menuName];
    if (!access) return true; // Unknown menu, allow by default
    
    return access.enabled && access.permitted;
  };
  
  const isModuleEnabled = (moduleName: string): boolean => {
    const modules = moduleStatus || defaultModuleStatus;
    
    const moduleMap: Record<string, boolean> = {
      'customers': modules.customersEnabled,
      'contacts': modules.contactsEnabled,
      'leads': modules.leadsEnabled,
      'opportunities': modules.opportunitiesEnabled,
      'products': modules.productsEnabled,
      'services': modules.servicesEnabled,
      'campaigns': modules.campaignsEnabled,
      'quotes': modules.quotesEnabled,
      'tasks': modules.tasksEnabled,
      'activities': modules.activitiesEnabled,
      'notes': modules.notesEnabled,
      'workflows': modules.workflowsEnabled,
      'reports': modules.reportsEnabled,
      'dashboard': modules.dashboardEnabled
    };
    
    return moduleMap[moduleName.toLowerCase()] ?? true;
  };

  const hasPermission = (permission: keyof UserPermissions | keyof GroupPermissions): boolean => {
    if (!profile) return false;
    
    // System admins have all permissions
    if (profile.groupPermissions?.isSystemAdmin) return true;
    
    // Check group permissions first
    if (profile.groupPermissions && permission in profile.groupPermissions) {
      return (profile.groupPermissions as any)[permission] || false;
    }
    
    // Fall back to legacy permissions
    if (permission in profile.permissions) {
      return (profile.permissions as any)[permission] || false;
    }
    
    return false;
  };

  const updateProfile = (newProfile: UserProfile) => {
    setProfile(newProfile);
    try {
      localStorage.setItem('userProfile', JSON.stringify(newProfile));
    } catch (error) {
      console.error('Failed to save profile to storage', error);
    }
  };
  
  const refreshModuleStatus = async () => {
    setIsLoading(true);
    await fetchModuleStatus();
    setIsLoading(false);
  };

  const value: ProfileContextType = {
    profile,
    moduleStatus,
    isLoading,
    canAccessPage,
    canAccessMenu,
    hasPermission,
    isModuleEnabled,
    updateProfile,
    refreshModuleStatus
  };

  return (
    <ProfileContext.Provider value={value}>
      {children}
    </ProfileContext.Provider>
  );
};

export const useProfile = () => {
  const context = useContext(ProfileContext);
  if (context === undefined) {
    throw new Error('useProfile must be used within a ProfileProvider');
  }
  return context;
};
