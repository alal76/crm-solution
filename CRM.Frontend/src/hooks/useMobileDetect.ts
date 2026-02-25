import { useState, useEffect } from 'react';

interface MobileDetect {
  isMobile: boolean;
  isTablet: boolean;
  isDesktop: boolean;
  width: number;
}

/**
 * Hook to detect the current viewport size and return device-type flags.
 * - isMobile:  width < 768px
 * - isTablet:  768px <= width < 1024px
 * - isDesktop: width >= 1024px
 */
export const useMobileDetect = (): MobileDetect => {
  const [width, setWidth] = useState<number>(
    typeof window !== 'undefined' ? window.innerWidth : 1024
  );

  useEffect(() => {
    const handleResize = () => setWidth(window.innerWidth);
    window.addEventListener('resize', handleResize);
    return () => window.removeEventListener('resize', handleResize);
  }, []);

  return {
    isMobile: width < 768,
    isTablet: width >= 768 && width < 1024,
    isDesktop: width >= 1024,
    width,
  };
};

export default useMobileDetect;
