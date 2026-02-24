import * as signalR from '@microsoft/signalr';
import { getApiBaseUrl } from '../config/ports';
import logger from './logger';

/**
 * SignalR connection manager for CRM real-time notifications.
 * Provides singleton access to the SignalR hub connection.
 */
class SignalRService {
  private connection: signalR.HubConnection | null = null;
  private approvalConnection: signalR.HubConnection | null = null;
  private isConnecting = false;
  private reconnectAttempts = 0;
  private maxReconnectAttempts = 5;
  
  // Event handlers
  private onRecordUpdatedCallbacks: Map<string, Set<(data: RecordNotification) => void>> = new Map();
  private onRecordCreatedCallbacks: Map<string, Set<(data: RecordNotification) => void>> = new Map();
  private onRecordDeletedCallbacks: Map<string, Set<(data: RecordNotification) => void>> = new Map();
  private onUserEditingCallbacks: Map<string, Set<(data: UserEditingNotification) => void>> = new Map();
  private onConnectionStateCallbacks: Set<(state: signalR.HubConnectionState) => void> = new Set();

  // Agent approval event handlers
  private onApprovalRequestCallbacks: Set<(data: ApprovalRequestNotification) => void> = new Set();
  private onApprovalResultCallbacks: Set<(data: ApprovalResultNotification) => void> = new Set();

  /**
   * Get the SignalR hub URL based on current API endpoint
   */
  private getHubUrl(): string {
    // Use the same base URL as the API from centralized config
    const apiUrl = getApiBaseUrl();
    return `${apiUrl}/hubs/notifications`;
  }

  /**
   * Get the Agent Approval SignalR hub URL
   */
  private getApprovalHubUrl(): string {
    const apiUrl = getApiBaseUrl();
    return `${apiUrl}/hubs/agent-approvals`;
  }

  /**
   * Initialize the SignalR connection with JWT authentication
   */
  async connect(accessToken: string): Promise<boolean> {
    if (this.connection?.state === signalR.HubConnectionState.Connected) {
      // Also connect approval hub if not connected
      this.connectApprovalHub(accessToken);
      return true;
    }

    if (this.isConnecting) {
      return false;
    }

    this.isConnecting = true;

    try {
      this.connection = new signalR.HubConnectionBuilder()
        .withUrl(this.getHubUrl(), {
          accessTokenFactory: () => accessToken,
          transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.LongPolling,
        })
        .withAutomaticReconnect({
          nextRetryDelayInMilliseconds: (retryContext) => {
            // Exponential backoff: 0, 2, 4, 8, 16 seconds
            if (retryContext.previousRetryCount >= this.maxReconnectAttempts) {
              return null; // Stop retrying
            }
            return Math.min(1000 * Math.pow(2, retryContext.previousRetryCount), 30000);
          }
        })
        .configureLogging(signalR.LogLevel.Information)
        .build();

      // Set up event handlers
      this.setupEventHandlers();

      // Connect
      await this.connection.start();
      logger.debug('SignalR connected to CRM notifications hub');
      this.reconnectAttempts = 0;
      this.notifyConnectionState(this.connection.state);

      // Also connect the approval hub (non-blocking)
      this.connectApprovalHub(accessToken);
      
      return true;
    } catch (error) {
      console.error('SignalR connection failed:', error);
      this.reconnectAttempts++;
      return false;
    } finally {
      this.isConnecting = false;
    }
  }

  /**
   * Connect to the Agent Approval SignalR hub
   */
  private async connectApprovalHub(accessToken: string): Promise<void> {
    if (this.approvalConnection?.state === signalR.HubConnectionState.Connected) {
      return;
    }

    try {
      this.approvalConnection = new signalR.HubConnectionBuilder()
        .withUrl(this.getApprovalHubUrl(), {
          accessTokenFactory: () => accessToken,
          transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.LongPolling,
        })
        .withAutomaticReconnect({
          nextRetryDelayInMilliseconds: (retryContext) => {
            if (retryContext.previousRetryCount >= this.maxReconnectAttempts) {
              return null;
            }
            return Math.min(1000 * Math.pow(2, retryContext.previousRetryCount), 30000);
          }
        })
        .configureLogging(signalR.LogLevel.Information)
        .build();

      this.setupApprovalEventHandlers();
      await this.approvalConnection.start();
      logger.debug('SignalR connected to Agent Approval hub');
    } catch (error) {
      console.warn('SignalR approval hub connection failed (non-critical):', error);
      this.approvalConnection = null;
    }
  }

  /**
   * Disconnect from all SignalR hubs
   */
  async disconnect(): Promise<void> {
    if (this.approvalConnection) {
      await this.approvalConnection.stop();
      this.approvalConnection = null;
      logger.debug('SignalR approval hub disconnected');
    }
    if (this.connection) {
      await this.connection.stop();
      this.connection = null;
      logger.debug('SignalR disconnected');
    }
  }

  /**
   * Set up internal event handlers for SignalR messages
   */
  private setupEventHandlers(): void {
    if (!this.connection) return;

    // Handle reconnection events
    this.connection.onreconnecting(() => {
      logger.debug('SignalR reconnecting...');
      this.notifyConnectionState(signalR.HubConnectionState.Reconnecting);
    });

    this.connection.onreconnected(() => {
      logger.debug('SignalR reconnected');
      this.notifyConnectionState(signalR.HubConnectionState.Connected);
    });

    this.connection.onclose(() => {
      logger.debug('SignalR connection closed');
      this.notifyConnectionState(signalR.HubConnectionState.Disconnected);
    });

    // Handle server-pushed events
    this.connection.on('RecordUpdated', (notification: RecordNotification) => {
      const key = `${notification.entityType}:${notification.entityId}`;
      const typeCallbacks = this.onRecordUpdatedCallbacks.get(notification.entityType.toLowerCase());
      const recordCallbacks = this.onRecordUpdatedCallbacks.get(key.toLowerCase());
      
      typeCallbacks?.forEach(cb => cb(notification));
      recordCallbacks?.forEach(cb => cb(notification));
    });

    this.connection.on('RecordCreated', (notification: RecordNotification) => {
      const typeCallbacks = this.onRecordCreatedCallbacks.get(notification.entityType.toLowerCase());
      typeCallbacks?.forEach(cb => cb(notification));
    });

    this.connection.on('RecordDeleted', (notification: RecordNotification) => {
      const key = `${notification.entityType}:${notification.entityId}`;
      const typeCallbacks = this.onRecordDeletedCallbacks.get(notification.entityType.toLowerCase());
      const recordCallbacks = this.onRecordDeletedCallbacks.get(key.toLowerCase());
      
      typeCallbacks?.forEach(cb => cb(notification));
      recordCallbacks?.forEach(cb => cb(notification));
    });

    this.connection.on('UserEditingRecord', (notification: UserEditingNotification) => {
      const key = `${notification.entityType}:${notification.entityId}`;
      const callbacks = this.onUserEditingCallbacks.get(key.toLowerCase());
      callbacks?.forEach(cb => cb(notification));
    });

    this.connection.on('UserViewingRecord', (notification: UserEditingNotification) => {
      // Can be used to show who's viewing a record
      logger.debug('User viewing record:', notification);
    });

    this.connection.on('UserLeftRecord', (notification: UserEditingNotification) => {
      logger.debug('User left record:', notification);
    });
  }

  /**
   * Subscribe to updates for a specific record
   */
  async subscribeToRecord(entityType: string, entityId: number): Promise<void> {
    if (this.connection?.state === signalR.HubConnectionState.Connected) {
      await this.connection.invoke('SubscribeToRecord', entityType, entityId);
    }
  }

  /**
   * Unsubscribe from updates for a specific record
   */
  async unsubscribeFromRecord(entityType: string, entityId: number): Promise<void> {
    if (this.connection?.state === signalR.HubConnectionState.Connected) {
      await this.connection.invoke('UnsubscribeFromRecord', entityType, entityId);
    }
  }

  /**
   * Subscribe to all updates for an entity type (useful for list views)
   */
  async subscribeToEntityType(entityType: string): Promise<void> {
    if (this.connection?.state === signalR.HubConnectionState.Connected) {
      await this.connection.invoke('SubscribeToEntityType', entityType);
    }
  }

  /**
   * Unsubscribe from all updates for an entity type
   */
  async unsubscribeFromEntityType(entityType: string): Promise<void> {
    if (this.connection?.state === signalR.HubConnectionState.Connected) {
      await this.connection.invoke('UnsubscribeFromEntityType', entityType);
    }
  }

  /**
   * Notify others that you're editing a record
   */
  async startEditing(entityType: string, entityId: number): Promise<void> {
    if (this.connection?.state === signalR.HubConnectionState.Connected) {
      await this.connection.invoke('StartEditing', entityType, entityId);
    }
  }

  /**
   * Notify others that you stopped editing a record
   */
  async stopEditing(entityType: string, entityId: number): Promise<void> {
    if (this.connection?.state === signalR.HubConnectionState.Connected) {
      await this.connection.invoke('StopEditing', entityType, entityId);
    }
  }

  /**
   * Register a callback for record update events
   */
  onRecordUpdated(entityTypeOrKey: string, callback: (data: RecordNotification) => void): () => void {
    const key = entityTypeOrKey.toLowerCase();
    if (!this.onRecordUpdatedCallbacks.has(key)) {
      this.onRecordUpdatedCallbacks.set(key, new Set());
    }
    this.onRecordUpdatedCallbacks.get(key)!.add(callback);
    
    return () => {
      this.onRecordUpdatedCallbacks.get(key)?.delete(callback);
    };
  }

  /**
   * Register a callback for record creation events
   */
  onRecordCreated(entityType: string, callback: (data: RecordNotification) => void): () => void {
    const key = entityType.toLowerCase();
    if (!this.onRecordCreatedCallbacks.has(key)) {
      this.onRecordCreatedCallbacks.set(key, new Set());
    }
    this.onRecordCreatedCallbacks.get(key)!.add(callback);
    
    return () => {
      this.onRecordCreatedCallbacks.get(key)?.delete(callback);
    };
  }

  /**
   * Register a callback for record deletion events
   */
  onRecordDeleted(entityTypeOrKey: string, callback: (data: RecordNotification) => void): () => void {
    const key = entityTypeOrKey.toLowerCase();
    if (!this.onRecordDeletedCallbacks.has(key)) {
      this.onRecordDeletedCallbacks.set(key, new Set());
    }
    this.onRecordDeletedCallbacks.get(key)!.add(callback);
    
    return () => {
      this.onRecordDeletedCallbacks.get(key)?.delete(callback);
    };
  }

  /**
   * Register a callback for user editing notifications
   */
  onUserEditing(entityType: string, entityId: number, callback: (data: UserEditingNotification) => void): () => void {
    const key = `${entityType}:${entityId}`.toLowerCase();
    if (!this.onUserEditingCallbacks.has(key)) {
      this.onUserEditingCallbacks.set(key, new Set());
    }
    this.onUserEditingCallbacks.get(key)!.add(callback);
    
    return () => {
      this.onUserEditingCallbacks.get(key)?.delete(callback);
    };
  }

  /**
   * Register a callback for connection state changes
   */
  onConnectionStateChange(callback: (state: signalR.HubConnectionState) => void): () => void {
    this.onConnectionStateCallbacks.add(callback);
    return () => {
      this.onConnectionStateCallbacks.delete(callback);
    };
  }

  private notifyConnectionState(state: signalR.HubConnectionState): void {
    this.onConnectionStateCallbacks.forEach(cb => cb(state));
  }

  /**
   * Get current connection state
   */
  getConnectionState(): signalR.HubConnectionState | null {
    return this.connection?.state ?? null;
  }

  /**
   * Check if connected
   */
  isConnected(): boolean {
    return this.connection?.state === signalR.HubConnectionState.Connected;
  }

  /**
   * Check if the approval hub is connected
   */
  isApprovalConnected(): boolean {
    return this.approvalConnection?.state === signalR.HubConnectionState.Connected;
  }

  // ── Agent Approval Hub ────────────────────────────────────────────

  /**
   * Set up event handlers for the approval hub
   */
  private setupApprovalEventHandlers(): void {
    if (!this.approvalConnection) return;

    this.approvalConnection.on('ReceiveApprovalRequest', (data: ApprovalRequestNotification) => {
      logger.debug('Approval request received:', data);
      this.onApprovalRequestCallbacks.forEach(cb => cb(data));
    });

    this.approvalConnection.on('ReceiveApprovalResult', (data: ApprovalResultNotification) => {
      logger.debug('Approval result received:', data);
      this.onApprovalResultCallbacks.forEach(cb => cb(data));
    });

    this.approvalConnection.onreconnecting(() => {
      logger.debug('SignalR approval hub reconnecting...');
    });

    this.approvalConnection.onreconnected(() => {
      logger.debug('SignalR approval hub reconnected');
    });

    this.approvalConnection.onclose(() => {
      logger.debug('SignalR approval hub closed');
    });
  }

  /**
   * Register a callback for approval request events (admin/approver only)
   */
  onApprovalRequest(callback: (data: ApprovalRequestNotification) => void): () => void {
    this.onApprovalRequestCallbacks.add(callback);
    return () => {
      this.onApprovalRequestCallbacks.delete(callback);
    };
  }

  /**
   * Register a callback for approval result events
   */
  onApprovalResult(callback: (data: ApprovalResultNotification) => void): () => void {
    this.onApprovalResultCallbacks.add(callback);
    return () => {
      this.onApprovalResultCallbacks.delete(callback);
    };
  }
}

// Types
export interface RecordNotification {
  action: 'Created' | 'Updated' | 'Deleted';
  entityType: string;
  entityId: number;
  record?: Record<string, unknown>;
  userId?: string;
  timestamp: string;
}

export interface UserEditingNotification {
  entityType: string;
  entityId: number;
  userId: string;
  userName: string;
  timestamp: string;
  isEditing: boolean;
}

export interface ApprovalRequestNotification {
  approvalId: number;
  actionDescription: string;
  tier: number;
  timestamp: string;
}

export interface ApprovalResultNotification {
  approvalId: number;
  approved: boolean;
  reason?: string;
  timestamp: string;
}

// Export singleton instance
export const signalRService = new SignalRService();
export default signalRService;
