/**
 * FontSizeContext - Font size adjustment for accessibility
 * Provides Small/Medium/Large font size options
 */

import React, { createContext, useContext, useState, useEffect, useMemo, ReactNode, useCallback } from 'react';

// Font size scale
export type FontSize = 'small' | 'medium' | 'large';

// Font size multipliers
const fontSizeMultipliers: Record<FontSize, number> = {
  small: 0.875,   // 14px base
  medium: 1,      // 16px base
  large: 1.25,    // 20px base
};

// Font size labels
export const fontSizeLabels: Record<FontSize, string> = {
  small: 'Small',
  medium: 'Medium',
  large: 'Large',
};

interface FontSizeContextType {
  fontSize: FontSize;
  fontSizeMultiplier: number;
  setFontSize: (size: FontSize) => void;
  increaseFontSize: () => void;
  decreaseFontSize: () => void;
}

const FontSizeContext = createContext<FontSizeContextType | undefined>(undefined);

const STORAGE_KEY = 'crm_font_size_preference';

interface FontSizeProviderProps {
  children: ReactNode;
  defaultSize?: FontSize;
}

export const FontSizeProvider: React.FC<FontSizeProviderProps> = ({
  children,
  defaultSize = 'medium',
}) => {
  // Initialize from localStorage
  const [fontSize, setFontSizeState] = useState<FontSize>(() => {
    if (typeof window !== 'undefined') {
      const stored = localStorage.getItem(STORAGE_KEY);
      if (stored && ['small', 'medium', 'large'].includes(stored)) {
        return stored as FontSize;
      }
    }
    return defaultSize;
  });

  // Calculate multiplier
  const fontSizeMultiplier = useMemo(() => fontSizeMultipliers[fontSize], [fontSize]);

  // Set font size and persist
  const setFontSize = useCallback((size: FontSize) => {
    setFontSizeState(size);
    if (typeof window !== 'undefined') {
      localStorage.setItem(STORAGE_KEY, size);
    }
  }, []);

  // Increase font size
  const increaseFontSize = useCallback(() => {
    setFontSizeState((current) => {
      const sizes: FontSize[] = ['small', 'medium', 'large'];
      const currentIndex = sizes.indexOf(current);
      const newSize = sizes[Math.min(currentIndex + 1, sizes.length - 1)];
      if (typeof window !== 'undefined') {
        localStorage.setItem(STORAGE_KEY, newSize);
      }
      return newSize;
    });
  }, []);

  // Decrease font size
  const decreaseFontSize = useCallback(() => {
    setFontSizeState((current) => {
      const sizes: FontSize[] = ['small', 'medium', 'large'];
      const currentIndex = sizes.indexOf(current);
      const newSize = sizes[Math.max(currentIndex - 1, 0)];
      if (typeof window !== 'undefined') {
        localStorage.setItem(STORAGE_KEY, newSize);
      }
      return newSize;
    });
  }, []);

  // Apply font size to document
  useEffect(() => {
    if (typeof document !== 'undefined') {
      const baseFontSize = 16 * fontSizeMultiplier;
      document.documentElement.style.fontSize = `${baseFontSize}px`;
      document.documentElement.dataset.fontSize = fontSize;
    }
  }, [fontSize, fontSizeMultiplier]);

  const value = useMemo(
    () => ({
      fontSize,
      fontSizeMultiplier,
      setFontSize,
      increaseFontSize,
      decreaseFontSize,
    }),
    [fontSize, fontSizeMultiplier, setFontSize, increaseFontSize, decreaseFontSize]
  );

  return (
    <FontSizeContext.Provider value={value}>
      {children}
    </FontSizeContext.Provider>
  );
};

export const useFontSize = (): FontSizeContextType => {
  const context = useContext(FontSizeContext);
  if (!context) {
    throw new Error('useFontSize must be used within a FontSizeProvider');
  }
  return context;
};

export default FontSizeContext;
