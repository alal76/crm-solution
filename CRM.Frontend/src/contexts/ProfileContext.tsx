import React, { createContext, useState, useContext, useEffect } from 'react';

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
}

interface ProfileContextType {
  profile: UserProfile | null;
  isLoading: boolean;
  canAccessPage: (pageName: string) => boolean;
  hasPermission: (permission: keyof UserPermissions) => boolean;
  updateProfile: (profile: UserProfile) => void;
}

const ProfileContext = createContext<ProfileContextType | undefined>(undefined);

export const ProfileProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [profile, setProfile] = useState<UserProfile | null>(null);
  const [isLoading, setIsLoading] = useState(false);

  useEffect(() => {
    // Load profile from auth context or localStorage
    loadProfileFromStorage();
  }, []);

  const loadProfileFromStorage = () => {
    try {
      const storedProfile = localStorage.getItem('userProfile');
      if (storedProfile) {
        setProfile(JSON.parse(storedProfile));
      }
    } catch (error) {
      console.error('Failed to load profile from storage', error);
    }
  };

  const canAccessPage = (pageName: string): boolean => {
    // If profile not loaded yet, allow access (will check again when loaded)
    if (!profile) return true;
    // If no accessible pages are specified, allow access to all pages (admin/default behavior)
    if (profile.accessiblePages.length === 0) return true;
    // Otherwise, check if the page is in the accessible pages list
    return profile.accessiblePages.includes(pageName);
  };

  const hasPermission = (permission: keyof UserPermissions): boolean => {
    if (!profile) return false;
    return profile.permissions[permission] || false;
  };

  const updateProfile = (newProfile: UserProfile) => {
    setProfile(newProfile);
    try {
      localStorage.setItem('userProfile', JSON.stringify(newProfile));
    } catch (error) {
      console.error('Failed to save profile to storage', error);
    }
  };

  const value: ProfileContextType = {
    profile,
    isLoading,
    canAccessPage,
    hasPermission,
    updateProfile
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
