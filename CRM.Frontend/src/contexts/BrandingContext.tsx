import React, { createContext, useState, useContext, useEffect, useCallback } from 'react';
import { getApiBaseUrl } from '../config/ports';
import { useAuth } from './AuthContext';

interface BrandingSettings {
  companyName: string;
  companyLogoUrl: string | null;
  companyLoginLogoUrl: string | null;
  primaryColor: string;
  secondaryColor: string;
  companyWebsite: string | null;
  companyEmail: string | null;
  companyPhone: string | null;
}

interface BrandingContextType {
  branding: BrandingSettings;
  isLoading: boolean;
  refreshBranding: () => Promise<void>;
  updateBranding: (settings: Partial<BrandingSettings>) => Promise<void>;
}

const defaultBranding: BrandingSettings = {
  companyName: 'CRM System',
  companyLogoUrl: null,
  companyLoginLogoUrl: null,
  primaryColor: '#6750A4',
  secondaryColor: '#625B71',
  companyWebsite: null,
  companyEmail: null,
  companyPhone: null,
};

const BrandingContext = createContext<BrandingContextType | undefined>(undefined);

export const BrandingProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [branding, setBranding] = useState<BrandingSettings>(defaultBranding);
  const [isLoading, setIsLoading] = useState(true);
  const { isAuthenticated } = useAuth();

  const refreshBranding = useCallback(async () => {
    if (!isAuthenticated) {
      setBranding(defaultBranding);
      setIsLoading(false);
      return;
    }

    try {
      const apiBase = getApiBaseUrl();
      const token = localStorage.getItem('accessToken');
      
      const response = await fetch(`${apiBase}/api/systemsettings`, {
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json',
        },
      });

      if (response.ok) {
        const data = await response.json();
        setBranding({
          companyName: data.companyName || 'CRM System',
          companyLogoUrl: data.companyLogoUrl || null,
          companyLoginLogoUrl: data.companyLoginLogoUrl || null,
          primaryColor: data.primaryColor || '#6750A4',
          secondaryColor: data.secondaryColor || '#625B71',
          companyWebsite: data.companyWebsite || null,
          companyEmail: data.companyEmail || null,
          companyPhone: data.companyPhone || null,
        });
      }
    } catch (error) {
      console.error('Failed to fetch branding settings:', error);
    } finally {
      setIsLoading(false);
    }
  }, [isAuthenticated]);

  const updateBranding = useCallback(async (settings: Partial<BrandingSettings>) => {
    try {
      const apiBase = getApiBaseUrl();
      const token = localStorage.getItem('accessToken');
      
      const response = await fetch(`${apiBase}/api/systemsettings`, {
        method: 'PUT',
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(settings),
      });

      if (response.ok) {
        // Update local state immediately
        setBranding(prev => ({ ...prev, ...settings }));
        // Dispatch custom event so other components can refresh
        window.dispatchEvent(new CustomEvent('brandingUpdated', { detail: settings }));
        // Re-fetch to ensure we have the latest from server
        await refreshBranding();
      }
    } catch (error) {
      console.error('Failed to update branding settings:', error);
      throw error;
    }
  }, [refreshBranding]);

  useEffect(() => {
    refreshBranding();
  }, [refreshBranding]);

  // Listen for branding updates from other components (e.g., SettingsPage)
  useEffect(() => {
    const handleBrandingUpdated = () => {
      refreshBranding();
    };
    window.addEventListener('brandingUpdated', handleBrandingUpdated);
    return () => {
      window.removeEventListener('brandingUpdated', handleBrandingUpdated);
    };
  }, [refreshBranding]);

  return (
    <BrandingContext.Provider value={{ branding, isLoading, refreshBranding, updateBranding }}>
      {children}
    </BrandingContext.Provider>
  );
};

export const useBranding = (): BrandingContextType => {
  const context = useContext(BrandingContext);
  if (context === undefined) {
    throw new Error('useBranding must be used within a BrandingProvider');
  }
  return context;
};

export default BrandingContext;
