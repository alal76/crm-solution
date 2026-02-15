import React, { createContext, useState, useContext, useEffect, useCallback } from 'react';
import { getApiBaseUrl } from '../config/ports';
import { useAuth } from './AuthContext';

interface BrandingSettings {
  solutionName?: string;
  customLogoUrl?: string | null;
  softwareLogoUrl?: string | null;
  brandingLogoUrl?: string | null;
  faviconUrl?: string | null;
  isCustomBrandingEnabled?: boolean;
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
  solutionName: 'CRM Solution',
  customLogoUrl: null,
  softwareLogoUrl: '/assets/logo.png',
  brandingLogoUrl: '/assets/logo.png',
  faviconUrl: null,
  isCustomBrandingEnabled: true,
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
    try {
      const apiBase = getApiBaseUrl();
      const token = localStorage.getItem('accessToken');

      const brandingRequest = fetch(`${apiBase}/api/branding`, {
        headers: {
          'Content-Type': 'application/json',
          ...(token ? { 'Authorization': `Bearer ${token}` } : {}),
        },
      });

      const systemSettingsRequest = isAuthenticated
        ? fetch(`${apiBase}/api/systemsettings`, {
            headers: {
              'Authorization': `Bearer ${token}`,
              'Content-Type': 'application/json',
            },
          })
        : Promise.resolve(null);

      const [brandingResponse, systemSettingsResponse] = await Promise.all([
        brandingRequest,
        systemSettingsRequest,
      ]);

      const nextBranding: BrandingSettings = { ...defaultBranding };

      if (systemSettingsResponse && systemSettingsResponse.ok) {
        const data = await systemSettingsResponse.json();
        nextBranding.companyName = data.companyName || nextBranding.companyName;
        nextBranding.companyLogoUrl = data.companyLogoUrl || nextBranding.companyLogoUrl;
        nextBranding.companyLoginLogoUrl = data.companyLoginLogoUrl || nextBranding.companyLoginLogoUrl;
        nextBranding.primaryColor = data.primaryColor || nextBranding.primaryColor;
        nextBranding.secondaryColor = data.secondaryColor || nextBranding.secondaryColor;
        nextBranding.companyWebsite = data.companyWebsite || nextBranding.companyWebsite;
        nextBranding.companyEmail = data.companyEmail || nextBranding.companyEmail;
        nextBranding.companyPhone = data.companyPhone || nextBranding.companyPhone;
      }

      if (brandingResponse.ok) {
        const brandingData = await brandingResponse.json();
        const customLogoUrl = brandingData.customLogoPath || null;
        const softwareLogoUrl = brandingData.softwareLogoPath || nextBranding.softwareLogoUrl;
        const isCustomBrandingEnabled = brandingData.isCustomBrandingEnabled ?? true;
        const brandingLogoUrl = isCustomBrandingEnabled
          ? (customLogoUrl || softwareLogoUrl)
          : softwareLogoUrl;

        nextBranding.solutionName = brandingData.solutionName || nextBranding.solutionName;
        nextBranding.customLogoUrl = customLogoUrl;
        nextBranding.softwareLogoUrl = softwareLogoUrl;
        nextBranding.brandingLogoUrl = brandingLogoUrl || null;
        nextBranding.faviconUrl = brandingData.faviconPath || brandingData.faviconDataUrl || null;
        nextBranding.isCustomBrandingEnabled = isCustomBrandingEnabled;
        nextBranding.companyName = brandingData.solutionName || nextBranding.companyName;
        nextBranding.companyLogoUrl = brandingLogoUrl || nextBranding.companyLogoUrl;
      }

      setBranding(nextBranding);
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

  useEffect(() => {
    const title = branding.solutionName || branding.companyName || 'CRM System';
    if (typeof document !== 'undefined') {
      document.title = title;
    }

    const faviconPath = branding.faviconUrl;
    if (!faviconPath || typeof document === 'undefined') {
      return;
    }

    const faviconUrl = faviconPath.startsWith('/uploads')
      ? `${getApiBaseUrl()}${faviconPath}`
      : faviconPath;

    let link = document.querySelector("link[rel~='icon']") as HTMLLinkElement | null;
    if (!link) {
      link = document.createElement('link');
      link.rel = 'icon';
      document.head.appendChild(link);
    }
    link.href = faviconUrl;
  }, [branding]);

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
