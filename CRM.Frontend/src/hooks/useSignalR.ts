import { useEffect, useCallback, useRef } from 'react';
import signalRService, { RecordNotification, UserEditingNotification } from '../services/signalRService';

/**
 * Hook to subscribe to real-time updates for a specific record.
 * Automatically subscribes when the component mounts and unsubscribes on unmount.
 * 
 * @param entityType - The type of entity (e.g., 'Customer', 'Lead')
 * @param entityId - The ID of the entity (null/undefined to skip subscription)
 * @param callbacks - Object with callback functions for different events
 */
export function useRecordSubscription(
  entityType: string,
  entityId: number | null | undefined,
  callbacks: {
    onUpdated?: (data: RecordNotification) => void;
    onDeleted?: (data: RecordNotification) => void;
    onUserEditing?: (data: UserEditingNotification) => void;
  }
) {
  const callbacksRef = useRef(callbacks);
  callbacksRef.current = callbacks;

  useEffect(() => {
    if (!entityId) return;

    const unsubscribers: (() => void)[] = [];

    // Subscribe to SignalR group
    signalRService.subscribeToRecord(entityType, entityId);

    // Register callbacks
    const key = `${entityType}:${entityId}`;
    
    if (callbacksRef.current.onUpdated) {
      unsubscribers.push(
        signalRService.onRecordUpdated(key, callbacksRef.current.onUpdated)
      );
    }
    
    if (callbacksRef.current.onDeleted) {
      unsubscribers.push(
        signalRService.onRecordDeleted(key, callbacksRef.current.onDeleted)
      );
    }
    
    if (callbacksRef.current.onUserEditing) {
      unsubscribers.push(
        signalRService.onUserEditing(entityType, entityId, callbacksRef.current.onUserEditing)
      );
    }

    return () => {
      // Unsubscribe from SignalR group
      signalRService.unsubscribeFromRecord(entityType, entityId);
      
      // Clean up callbacks
      unsubscribers.forEach(unsub => unsub());
    };
  }, [entityType, entityId]);
}

/**
 * Hook to subscribe to all updates for an entity type.
 * Useful for list views that need to know about new/updated/deleted records.
 * 
 * @param entityType - The type of entity (e.g., 'Customer', 'Lead')
 * @param callbacks - Object with callback functions for different events
 */
export function useEntityTypeSubscription(
  entityType: string,
  callbacks: {
    onCreated?: (data: RecordNotification) => void;
    onUpdated?: (data: RecordNotification) => void;
    onDeleted?: (data: RecordNotification) => void;
  }
) {
  const callbacksRef = useRef(callbacks);
  callbacksRef.current = callbacks;

  useEffect(() => {
    const unsubscribers: (() => void)[] = [];

    // Subscribe to SignalR group
    signalRService.subscribeToEntityType(entityType);

    // Register callbacks
    if (callbacksRef.current.onCreated) {
      unsubscribers.push(
        signalRService.onRecordCreated(entityType, callbacksRef.current.onCreated)
      );
    }
    
    if (callbacksRef.current.onUpdated) {
      unsubscribers.push(
        signalRService.onRecordUpdated(entityType, callbacksRef.current.onUpdated)
      );
    }
    
    if (callbacksRef.current.onDeleted) {
      unsubscribers.push(
        signalRService.onRecordDeleted(entityType, callbacksRef.current.onDeleted)
      );
    }

    return () => {
      // Unsubscribe from SignalR group
      signalRService.unsubscribeFromEntityType(entityType);
      
      // Clean up callbacks
      unsubscribers.forEach(unsub => unsub());
    };
  }, [entityType]);
}

/**
 * Hook to notify other users that you're editing a record.
 * Returns functions to start and stop editing notifications.
 * 
 * @param entityType - The type of entity
 * @param entityId - The ID of the entity
 */
export function useEditingNotification(
  entityType: string,
  entityId: number | null | undefined
) {
  const startEditing = useCallback(() => {
    if (entityId) {
      signalRService.startEditing(entityType, entityId);
    }
  }, [entityType, entityId]);

  const stopEditing = useCallback(() => {
    if (entityId) {
      signalRService.stopEditing(entityType, entityId);
    }
  }, [entityType, entityId]);

  // Auto stop editing on unmount
  useEffect(() => {
    return () => {
      if (entityId) {
        signalRService.stopEditing(entityType, entityId);
      }
    };
  }, [entityType, entityId]);

  return { startEditing, stopEditing };
}

/**
 * Hook to get SignalR connection state.
 * Useful for showing connection status in UI.
 */
export function useSignalRConnection() {
  const isConnected = useCallback(() => {
    return signalRService.isConnected();
  }, []);

  const getState = useCallback(() => {
    return signalRService.getConnectionState();
  }, []);

  return { isConnected, getState };
}

export default {
  useRecordSubscription,
  useEntityTypeSubscription,
  useEditingNotification,
  useSignalRConnection,
};
