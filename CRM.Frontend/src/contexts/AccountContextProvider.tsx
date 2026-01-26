/**
 * Account Context Provider
 * 
 * Manages the selected accounts for contextual filtering across the CRM.
 * When accounts are selected, relevant pages will filter data to show
 * only information related to those accounts.
 */
import React, { createContext, useContext, useState, useEffect, ReactNode, useCallback } from 'react';
import { Account } from '../services/accountService';

interface AccountContextType {
  /** Currently selected accounts for filtering */
  selectedAccounts: Account[];
  /** Add an account to selection */
  addAccount: (account: Account) => void;
  /** Remove an account from selection */
  removeAccount: (accountId: number) => void;
  /** Set all selected accounts (replaces current selection) */
  setSelectedAccounts: (accounts: Account[]) => void;
  /** Clear all selected accounts */
  clearAccounts: () => void;
  /** Check if an account is selected */
  isAccountSelected: (accountId: number) => boolean;
  /** Whether context filtering is active */
  isContextActive: boolean;
  /** Get account IDs as array (for API filtering) */
  getAccountIds: () => number[];
  /** Whether the context flyout is open */
  isFlyoutOpen: boolean;
  /** Toggle flyout visibility */
  toggleFlyout: () => void;
  /** Set flyout visibility */
  setFlyoutOpen: (open: boolean) => void;
}

const AccountContext = createContext<AccountContextType | undefined>(undefined);

const STORAGE_KEY = 'crm_selected_accounts';
const FLYOUT_STORAGE_KEY = 'crm_flyout_open';

export const AccountContextProvider: React.FC<{ children: ReactNode }> = ({ children }) => {
  const [selectedAccounts, setSelectedAccountsState] = useState<Account[]>([]);
  const [isFlyoutOpen, setIsFlyoutOpenState] = useState(false);

  // Load from localStorage on mount
  useEffect(() => {
    try {
      const saved = localStorage.getItem(STORAGE_KEY);
      if (saved) {
        const parsed = JSON.parse(saved);
        if (Array.isArray(parsed)) {
          setSelectedAccountsState(parsed);
        }
      }
      
      const flyoutState = localStorage.getItem(FLYOUT_STORAGE_KEY);
      if (flyoutState === 'true') {
        setIsFlyoutOpenState(true);
      }
    } catch (error) {
      console.error('Error loading account context from storage:', error);
    }
  }, []);

  // Save to localStorage when selection changes
  useEffect(() => {
    try {
      localStorage.setItem(STORAGE_KEY, JSON.stringify(selectedAccounts));
    } catch (error) {
      console.error('Error saving account context to storage:', error);
    }
  }, [selectedAccounts]);

  const addAccount = useCallback((account: Account) => {
    setSelectedAccountsState(prev => {
      if (prev.some(a => a.id === account.id)) {
        return prev; // Already selected
      }
      return [...prev, account];
    });
  }, []);

  const removeAccount = useCallback((accountId: number) => {
    setSelectedAccountsState(prev => prev.filter(a => a.id !== accountId));
  }, []);

  const setSelectedAccounts = useCallback((accounts: Account[]) => {
    setSelectedAccountsState(accounts);
  }, []);

  const clearAccounts = useCallback(() => {
    setSelectedAccountsState([]);
  }, []);

  const isAccountSelected = useCallback((accountId: number) => {
    return selectedAccounts.some(a => a.id === accountId);
  }, [selectedAccounts]);

  const getAccountIds = useCallback(() => {
    return selectedAccounts.map(a => a.id).filter((id): id is number => id !== undefined);
  }, [selectedAccounts]);

  const toggleFlyout = useCallback(() => {
    setIsFlyoutOpenState(prev => {
      const newState = !prev;
      localStorage.setItem(FLYOUT_STORAGE_KEY, String(newState));
      return newState;
    });
  }, []);

  const setFlyoutOpen = useCallback((open: boolean) => {
    setIsFlyoutOpenState(open);
    localStorage.setItem(FLYOUT_STORAGE_KEY, String(open));
  }, []);

  const isContextActive = selectedAccounts.length > 0;

  return (
    <AccountContext.Provider
      value={{
        selectedAccounts,
        addAccount,
        removeAccount,
        setSelectedAccounts,
        clearAccounts,
        isAccountSelected,
        isContextActive,
        getAccountIds,
        isFlyoutOpen,
        toggleFlyout,
        setFlyoutOpen,
      }}
    >
      {children}
    </AccountContext.Provider>
  );
};

export const useAccountContext = () => {
  const context = useContext(AccountContext);
  if (context === undefined) {
    throw new Error('useAccountContext must be used within an AccountContextProvider');
  }
  return context;
};

export default AccountContext;
