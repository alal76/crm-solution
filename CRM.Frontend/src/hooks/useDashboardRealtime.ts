import { useEffect, useRef, useState } from 'react';
import * as signalR from '@microsoft/signalr';
import { getApiBaseUrl } from '../config/ports';

/**
 * Shape of a metric update broadcast by the backend DashboardHub.
 * Maps to DashboardMetricUpdate in CRM.Api.Hubs.
 */
export interface DashboardMetricUpdate {
  MetricName: string;
  MetricLabel: string;
  Value: number;
  PreviousValue?: number;
  ChangePercent?: number;
  Trend?: string; // 'up' | 'down' | 'flat'
  Format?: string; // 'currency' | 'percentage' | 'number'
}

/**
 * Shape of the full SignalR payload sent on the "MetricUpdated" event.
 */
export interface DashboardUpdatePayload {
  DashboardId: string;
  Metric: DashboardMetricUpdate;
  Timestamp: string;
}

/**
 * Hook that connects to the /hubs/dashboard SignalR hub and listens for
 * real-time metric updates from the backend DashboardHubService.
 *
 * @param accessToken - JWT token for hub authentication.
 * @param dashboardId - Optional dashboard ID to subscribe to (subscribes to all if omitted).
 * @returns { latestUpdate, isConnected } — most recent metric payload and connection state.
 *
 * TODO-RPT-06
 */
export function useDashboardRealtime(accessToken: string | null, dashboardId?: string) {
  const [latestUpdate, setLatestUpdate] = useState<DashboardUpdatePayload | null>(null);
  const [isConnected, setIsConnected] = useState(false);
  const connectionRef = useRef<signalR.HubConnection | null>(null);

  useEffect(() => {
    if (!accessToken) return;

    const apiUrl = getApiBaseUrl();
    const hubUrl = `${apiUrl}/hubs/dashboard`;

    const connection = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl, {
        accessTokenFactory: () => accessToken,
        transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.LongPolling,
      })
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Warning)
      .build();

    connection.on('MetricUpdated', (payload: DashboardUpdatePayload) => {
      setLatestUpdate(payload);
    });

    connection.onclose(() => setIsConnected(false));
    connection.onreconnecting(() => setIsConnected(false));
    connection.onreconnected(() => setIsConnected(true));

    connection
      .start()
      .then(async () => {
        setIsConnected(true);
        connectionRef.current = connection;

        // Subscribe to a specific dashboard group or to all-dashboards
        try {
          const group = dashboardId
            ? `dashboard:${dashboardId}`
            : 'all-dashboards';
          await connection.invoke('JoinGroup', group).catch(() => {
            // Hub may not expose JoinGroup as a client-callable method — ignore gracefully
          });
        } catch {
          // Non-fatal: hub may auto-enroll clients
        }
      })
      .catch((err: Error) => {
        console.warn('[useDashboardRealtime] Connection failed:', err.message);
      });

    return () => {
      connection.stop().catch(() => undefined);
      setIsConnected(false);
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [accessToken, dashboardId]);

  return { latestUpdate, isConnected };
}
