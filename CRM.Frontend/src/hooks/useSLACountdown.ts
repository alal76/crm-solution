// SLA Countdown Real-Time Hook
// Connects to the SLA SignalR hub for real-time SLA countdown updates
// Part of ITSM Enhancement Plan - Phase 1.3 (TODO-SD003-012)

import { useState, useEffect, useCallback, useRef } from 'react';
import * as signalR from '@microsoft/signalr';
import { getApiBaseUrl } from '../config/ports';

/**
 * Real-time SLA countdown data received from the SignalR hub.
 */
export interface SLACountdown {
  serviceRequestId: number;
  status: 'OnTrack' | 'AtRisk' | 'Breached';
  responseTimeRemaining: string | null;
  resolutionTimeRemaining: string | null;
  responseDeadline: string | null;
  resolutionDeadline: string | null;
  responsePercentageUsed: number;
  resolutionPercentageUsed: number;
}

/**
 * SLA breach event data received from the SignalR hub.
 */
export interface SLABreachEvent {
  serviceRequestId: number;
  breachType: string;
  timestamp: string;
}

/**
 * SLA warning event data received from the SignalR hub.
 */
export interface SLAWarningEvent {
  serviceRequestId: number;
  warningType: string;
  timeRemaining: string;
  timeRemainingMinutes: number;
  timestamp: string;
}

interface UseSLACountdownResult {
  /** Current SLA countdown data, updated in real-time */
  countdown: SLACountdown | null;
  /** Whether the SignalR connection is active */
  isConnected: boolean;
  /** Any connection error message */
  error: string | null;
  /** Latest breach event, if any */
  lastBreach: SLABreachEvent | null;
  /** Latest warning event, if any */
  lastWarning: SLAWarningEvent | null;
}

/**
 * Hook that connects to the SLA SignalR hub and subscribes to real-time
 * SLA countdown updates for a specific service request.
 *
 * @param serviceRequestId - The service request ID to subscribe to
 * @returns SLA countdown state, connection status, and event data
 *
 * @example
 * ```tsx
 * const { countdown, isConnected, error } = useSLACountdown(ticketId);
 * if (countdown) {
 *   console.log(`SLA Status: ${countdown.status}`);
 *   console.log(`Resolution remaining: ${countdown.resolutionTimeRemaining}`);
 * }
 * ```
 */
export function useSLACountdown(serviceRequestId: number): UseSLACountdownResult {
  const [countdown, setCountdown] = useState<SLACountdown | null>(null);
  const [isConnected, setIsConnected] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [lastBreach, setLastBreach] = useState<SLABreachEvent | null>(null);
  const [lastWarning, setLastWarning] = useState<SLAWarningEvent | null>(null);
  const connectionRef = useRef<signalR.HubConnection | null>(null);
  const serviceRequestIdRef = useRef(serviceRequestId);

  serviceRequestIdRef.current = serviceRequestId;

  const getAccessToken = useCallback((): string => {
    return localStorage.getItem('accessToken') || '';
  }, []);

  useEffect(() => {
    if (!serviceRequestId || serviceRequestId <= 0) {
      return;
    }

    const token = getAccessToken();
    if (!token) {
      setError('No authentication token available');
      return;
    }

    const apiUrl = getApiBaseUrl();
    const hubUrl = `${apiUrl}/hubs/sla`;

    const connection = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl, {
        accessTokenFactory: () => getAccessToken(),
        transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.LongPolling,
      })
      .withAutomaticReconnect({
        nextRetryDelayInMilliseconds: (retryContext) => {
          if (retryContext.previousRetryCount >= 5) {
            return null; // Stop retrying after 5 attempts
          }
          return Math.min(1000 * Math.pow(2, retryContext.previousRetryCount), 30000);
        },
      })
      .configureLogging(signalR.LogLevel.Warning)
      .build();

    connectionRef.current = connection;

    // Handle SLA countdown updates
    connection.on('SLAUpdate', (data: SLACountdown) => {
      if (data.serviceRequestId === serviceRequestIdRef.current) {
        setCountdown(data);
      }
    });

    // Handle SLA breach notifications
    connection.on('SLABreach', (data: SLABreachEvent) => {
      if (data.serviceRequestId === serviceRequestIdRef.current) {
        setLastBreach(data);
        // Also update countdown status to Breached
        setCountdown((prev) =>
          prev ? { ...prev, status: 'Breached' } : null
        );
      }
    });

    // Handle SLA warning notifications
    connection.on('SLAWarning', (data: SLAWarningEvent) => {
      if (data.serviceRequestId === serviceRequestIdRef.current) {
        setLastWarning(data);
        // Also update countdown status to AtRisk if currently OnTrack
        setCountdown((prev) =>
          prev && prev.status === 'OnTrack' ? { ...prev, status: 'AtRisk' } : prev
        );
      }
    });

    // Connection state change handlers
    connection.onreconnecting(() => {
      setIsConnected(false);
      setError('Reconnecting to SLA hub...');
    });

    connection.onreconnected(() => {
      setIsConnected(true);
      setError(null);
      // Re-subscribe to the ticket group after reconnection
      connection
        .invoke('SubscribeToTicket', serviceRequestIdRef.current)
        .catch((err) => console.warn('Failed to re-subscribe after reconnect:', err));
    });

    connection.onclose((err) => {
      setIsConnected(false);
      if (err) {
        setError(`SLA hub connection closed: ${(err as Error).message}`);
      }
    });

    // Start connection and subscribe
    const startConnection = async () => {
      try {
        await connection.start();
        setIsConnected(true);
        setError(null);

        // Subscribe to the specific ticket's SLA updates
        await connection.invoke('SubscribeToTicket', serviceRequestId);
      } catch (err) {
        const errorMessage = err instanceof Error ? (err as Error).message : 'Unknown connection error';
        setError(`Failed to connect to SLA hub: ${errorMessage}`);
        setIsConnected(false);
      }
    };

    startConnection();

    // Cleanup on unmount or serviceRequestId change
    return () => {
      const cleanup = async () => {
        if (connection.state === signalR.HubConnectionState.Connected) {
          try {
            await connection.invoke('UnsubscribeFromTicket', serviceRequestId);
          } catch {
            // Ignore errors during cleanup
          }
        }
        try {
          await connection.stop();
        } catch {
          // Ignore errors during cleanup
        }
      };
      cleanup();
      connectionRef.current = null;
    };
  }, [serviceRequestId, getAccessToken]);

  return { countdown, isConnected, error, lastBreach, lastWarning };
}

/**
 * Hook that connects to the SLA SignalR hub and subscribes to ALL SLA updates.
 * Useful for SLA dashboard views that need to monitor all tickets.
 *
 * @returns Object with updates map, connection status, and events
 */
export function useSLADashboardUpdates(): {
  updates: Map<number, SLACountdown>;
  isConnected: boolean;
  error: string | null;
  breaches: SLABreachEvent[];
  warnings: SLAWarningEvent[];
} {
  const [updates, setUpdates] = useState<Map<number, SLACountdown>>(new Map());
  const [isConnected, setIsConnected] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [breaches, setBreaches] = useState<SLABreachEvent[]>([]);
  const [warnings, setWarnings] = useState<SLAWarningEvent[]>([]);
  const connectionRef = useRef<signalR.HubConnection | null>(null);

  const getAccessToken = useCallback((): string => {
    return localStorage.getItem('accessToken') || '';
  }, []);

  useEffect(() => {
    const token = getAccessToken();
    if (!token) {
      setError('No authentication token available');
      return;
    }

    const apiUrl = getApiBaseUrl();
    const hubUrl = `${apiUrl}/hubs/sla`;

    const connection = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl, {
        accessTokenFactory: () => getAccessToken(),
        transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.LongPolling,
      })
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Warning)
      .build();

    connectionRef.current = connection;

    connection.on('SLAUpdate', (data: SLACountdown) => {
      setUpdates((prev) => {
        const next = new Map(prev);
        next.set(data.serviceRequestId, data);
        return next;
      });
    });

    connection.on('SLABreach', (data: SLABreachEvent) => {
      setBreaches((prev) => [...prev.slice(-99), data]); // Keep last 100
    });

    connection.on('SLAWarning', (data: SLAWarningEvent) => {
      setWarnings((prev) => [...prev.slice(-99), data]); // Keep last 100
    });

    connection.onreconnecting(() => {
      setIsConnected(false);
    });

    connection.onreconnected(() => {
      setIsConnected(true);
      setError(null);
      connection
        .invoke('SubscribeToAllSLA')
        .catch((err) => console.warn('Failed to re-subscribe to all SLA:', err));
    });

    connection.onclose(() => {
      setIsConnected(false);
    });

    const startConnection = async () => {
      try {
        await connection.start();
        setIsConnected(true);
        setError(null);
        await connection.invoke('SubscribeToAllSLA');
      } catch (err) {
        const errorMessage = err instanceof Error ? (err as Error).message : 'Unknown connection error';
        setError(`Failed to connect to SLA dashboard hub: ${errorMessage}`);
        setIsConnected(false);
      }
    };

    startConnection();

    return () => {
      const cleanup = async () => {
        if (connection.state === signalR.HubConnectionState.Connected) {
          try {
            await connection.invoke('UnsubscribeFromAllSLA');
          } catch {
            // Ignore
          }
        }
        try {
          await connection.stop();
        } catch {
          // Ignore
        }
      };
      cleanup();
      connectionRef.current = null;
    };
  }, [getAccessToken]);

  return { updates, isConnected, error, breaches, warnings };
}

export default useSLACountdown;
