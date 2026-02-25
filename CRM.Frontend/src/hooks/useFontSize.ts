/**
 * useFontSize hook - standalone re-export for tree-shaking friendly imports.
 *
 * TODO-UX-05: Font size preference persisting to localStorage under the
 * key `crm-font-size`.
 *
 * The source of truth is FontSizeContext (key `crm_font_size_preference`).
 * This hook bridges to the `crm-font-size` key as required by the TODO spec
 * by keeping the two keys in sync on every change.
 *
 * Usage:
 *   import { useFontSize, FontSize } from 'hooks/useFontSize';
 *   const { fontSize, setFontSize, fontSizeLabels } = useFontSize();
 */

import { useEffect, useRef } from 'react';
import { useFontSize as useContextFontSize, FontSize, fontSizeLabels } from '../contexts/FontSizeContext';

// Legacy alias key required by TODO-UX-05 spec
export const FONT_SIZE_STORAGE_KEY = 'crm-font-size';

export type { FontSize };
export { fontSizeLabels };

export interface UseFontSizeReturn {
  fontSize: FontSize;
  fontSizeMultiplier: number;
  setFontSize: (size: FontSize) => void;
  increaseFontSize: () => void;
  decreaseFontSize: () => void;
}

export function useFontSize(): UseFontSizeReturn {
  const ctx = useContextFontSize();
  const bootstrapped = useRef(false);

  // On first mount: if `crm-font-size` has a value and the context key does
  // not yet have a saved preference, import from the alias key so both keys
  // are consistent.
  useEffect(() => {
    if (bootstrapped.current) return;
    bootstrapped.current = true;

    const alias = localStorage.getItem(FONT_SIZE_STORAGE_KEY) as FontSize | null;
    const primary = localStorage.getItem('crm_font_size_preference') as FontSize | null;

    if (alias && !primary) {
      ctx.setFontSize(alias);
    }
  }, []); // eslint-disable-line react-hooks/exhaustive-deps

  // Mirror every change from the context into the `crm-font-size` alias key.
  useEffect(() => {
    const current = localStorage.getItem(FONT_SIZE_STORAGE_KEY) as FontSize | null;
    if (current !== ctx.fontSize) {
      localStorage.setItem(FONT_SIZE_STORAGE_KEY, ctx.fontSize);
    }
  }, [ctx.fontSize]);

  return ctx;
}

export default useFontSize;
