import React, { createContext, useState } from 'react';
import { LookupItem } from '../services/lookupService';

type LookupMap = Record<string, LookupItem[]>;

interface LookupContextValue {
  get: (category: string) => LookupItem[] | undefined;
  set: (category: string, items: LookupItem[]) => void;
}

export const LookupContext = createContext<LookupContextValue | null>(null);

export const LookupProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [store, setStore] = useState<LookupMap>({});

  const get = (category: string) => store[category];
  const set = (category: string, items: LookupItem[]) => setStore(prev => ({ ...prev, [category]: items }));

  return (
    <LookupContext.Provider value={{ get, set }}>
      {children}
    </LookupContext.Provider>
  );
};

export default LookupContext;
