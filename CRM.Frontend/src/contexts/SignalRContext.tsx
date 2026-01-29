import React, { createContext, useContext, useEffect, useState, useCallback, ReactNode } from 'react';
import { HubConnectionState } from '@microsoft/signalr';
import signalRService from '../services/signalRService';
import { useAuth } from './AuthContext';

interface SignalRContextValue {
  isConnected: boolean;
  connectionState: HubConnectionState;
  connect: () => Promise<boolean>;
  disconnect: () => Promise<void>;
}

const SignalRContext = createContext<SignalRContextValue | undefined>(undefined);

interface SignalRProviderProps {
  children: ReactNode;
}

/**
 * Gets the access token from localStorage
 */
const getAccessToken = (): string | null => {
  return localStorage.getItem('accessToken');
};

/**
 * Provider component that manages the SignalR connection lifecycle.
 * Automatically connects when user is authenticated and disconnects on logout.
 */
export function SignalRProvider({ children }: SignalRProviderProps) {
  const { isAuthenticated } = useAuth();
  const [connectionState, setConnectionState] = useState<HubConnectionState>(
    signalRService.getConnectionState()
  );

  // Poll connection state (SignalR doesn't have a reliable state change event)
  useEffect(() => {
    const intervalId = setInterval(() => {
      const currentState = signalRService.getConnectionState();
      setConnectionState(prev => {
        if (prev !== currentState) {
          return currentState;
        }
        return prev;
      });
    }, 1000);

    return () => clearInterval(intervalId);
  }, []);

  // Auto-connect when authenticated
  useEffect(() => {
    const token = getAccessToken();
    if (isAuthenticated && token) {
      signalRService.connect(token).then(success => {
        if (success) {
          setConnectionState(signalRService.getConnectionState());
        }
      });
    } else {
      signalRService.disconnect().then(() => {
        setConnectionState(signalRService.getConnectionState());
      });
    }

    return () => {
      // Don't disconnect on unmount - let the service manage the connection
    };
  }, [isAuthenticated]);

  const connect = useCallback(async (): Promise<boolean> => {
    const token = getAccessToken();
    if (!token) return false;
    const success = await signalRService.connect(token);
    setConnectionState(signalRService.getConnectionState());
    return success;
  }, []);

  const disconnect = useCallback(async (): Promise<void> => {
    await signalRService.disconnect();
    setConnectionState(signalRService.getConnectionState());
  }, []);

  const value: SignalRContextValue = {
    isConnected: connectionState === HubConnectionState.Connected,
    connectionState,
    connect,
    disconnect,
  };

  return (
    <SignalRContext.Provider value={value}>
      {children}
    </SignalRContext.Provider>
  );
}

/**
 * Hook to access the SignalR connection context.
 */
export function useSignalRContext(): SignalRContextValue {
  const context = useContext(SignalRContext);
  if (!context) {
    throw new Error('useSignalRContext must be used within a SignalRProvider');
  }
  return context;
}

export default SignalRContext;
