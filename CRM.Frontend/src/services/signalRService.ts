import * as signalR from '@microsoft/signalr';

/**
 * SignalR connection manager for CRM real-time notifications.
 * Provides singleton access to the SignalR hub connection.
 */
class SignalRService {
  private connection: signalR.HubConnection | null = null;
  private isConnecting = false;
  private reconnectAttempts = 0;
  private maxReconnectAttempts = 5;
  
  // Event handlers
  private onRecordUpdatedCallbacks: Map<string, Set<(data: RecordNotification) => void>> = new Map();
  private onRecordCreatedCallbacks: Map<string, Set<(data: RecordNotification) => void>> = new Map();
  private onRecordDeletedCallbacks: Map<string, Set<(data: RecordNotification) => void>> = new Map();
  private onUserEditingCallbacks: Map<string, Set<(data: UserEditingNotification) => void>> = new Map();
  private onConnectionStateCallbacks: Set<(state: signalR.HubConnectionState) => void> = new Set();

  /**
   * Get the SignalR hub URL based on current API endpoint
   */
  private getHubUrl(): string {
    // Use the same base URL as the API
    const apiUrl = process.env.REACT_APP_API_URL || 
      (window.location.hostname === 'localhost' 
        ? 'http://localhost:5000' 
        : `${window.location.protocol}//${window.location.hostname}:5000`);
    
    return `${apiUrl}/hubs/notifications`;
  }

  /**
   * Initialize the SignalR connection with JWT authentication
   */
  async connect(accessToken: string): Promise<boolean> {
    if (this.connection?.state === signalR.HubConnectionState.Connected) {
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
      console.log('SignalR connected to CRM notifications hub');
      this.reconnectAttempts = 0;
      this.notifyConnectionState(this.connection.state);
      
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
   * Disconnect from the SignalR hub
   */
  async disconnect(): Promise<void> {
    if (this.connection) {
      await this.connection.stop();
      this.connection = null;
      console.log('SignalR disconnected');
    }
  }

  /**
   * Set up internal event handlers for SignalR messages
   */
  private setupEventHandlers(): void {
    if (!this.connection) return;

    // Handle reconnection events
    this.connection.onreconnecting(() => {
      console.log('SignalR reconnecting...');
      this.notifyConnectionState(signalR.HubConnectionState.Reconnecting);
    });

    this.connection.onreconnected(() => {
      console.log('SignalR reconnected');
      this.notifyConnectionState(signalR.HubConnectionState.Connected);
    });

    this.connection.onclose(() => {
      console.log('SignalR connection closed');
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
      console.log('User viewing record:', notification);
    });

    this.connection.on('UserLeftRecord', (notification: UserEditingNotification) => {
      console.log('User left record:', notification);
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
}

// Types
export interface RecordNotification {
  action: 'Created' | 'Updated' | 'Deleted';
  entityType: string;
  entityId: number;
  record?: any;
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

// Export singleton instance
export const signalRService = new SignalRService();
export default signalRService;
