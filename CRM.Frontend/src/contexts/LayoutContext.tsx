import React, { createContext, useContext, useState, useEffect, ReactNode } from 'react';

interface LayoutContextType {
  isMobileLayout: boolean;
  toggleLayout: () => void;
}

const LayoutContext = createContext<LayoutContextType | undefined>(undefined);

export const LayoutProvider: React.FC<{ children: ReactNode }> = ({ children }) => {
  const [isMobileLayout, setIsMobileLayout] = useState(false);

  // Initialize from localStorage on mount
  useEffect(() => {
    const savedLayout = localStorage.getItem('layoutMode');
    
    // Only apply mobile-layout class if explicitly saved as 'mobile'
    // Default is desktop (no class)
    if (savedLayout === 'mobile') {
      setIsMobileLayout(true);
      document.documentElement.classList.add('mobile-layout');
    } else {
      setIsMobileLayout(false);
      document.documentElement.classList.remove('mobile-layout');
    }
  }, []);

  const toggleLayout = () => {
    setIsMobileLayout((prev) => {
      const newValue = !prev;
      const layoutMode = newValue ? 'mobile' : 'desktop';
      localStorage.setItem('layoutMode', layoutMode);
      if (newValue) {
        document.documentElement.classList.add('mobile-layout');
      } else {
        document.documentElement.classList.remove('mobile-layout');
      }
      return newValue;
    });
  };

  return (
    <LayoutContext.Provider value={{ isMobileLayout, toggleLayout }}>
      {children}
    </LayoutContext.Provider>
  );
};

export const useLayout = () => {
  const context = useContext(LayoutContext);
  if (context === undefined) {
    throw new Error('useLayout must be used within a LayoutProvider');
  }
  return context;
};
