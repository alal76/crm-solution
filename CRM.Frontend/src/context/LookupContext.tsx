import React, { createContext, useState, useEffect } from 'react';
import lookupService, { LookupItem } from '../services/lookupService';

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

  useEffect(() => {
    let mounted = true;
    const preload = async () => {
      const categories = [
        'QuoteStatus', 'LeadSource', 'LeadStatus', 'OpportunityStage',
        'ProductCategory', 'ProductStatus', 'CustomerType', 'PreferredContactMethod',
        'BillingCycle', 'Currency', 'Priority', 'Industry', 'Salutation', 'Gender'
      ];
      try {
        await Promise.all(categories.map(async (cat) => {
          try {
            const items = await lookupService.getLookupItems(cat).catch(() => []);
            if (mounted && items && items.length) set(cat, items);
          } catch {
            // ignore individual category failures
          }
        }));
      } catch {
        // ignore preload errors
      }
    };
    preload();
    return () => { mounted = false; };
  }, []);

  return (
    <LookupContext.Provider value={{ get, set }}>
      {children}
    </LookupContext.Provider>
  );
};

export default LookupContext;
