/**
 * Entity Context Provider
 * 
 * Tracks the current entity context based on the URL path.
 * This context is used to:
 * 1. Tag notes to the specific entity being viewed
 * 2. Provide context to the AI chatbot
 * 3. Filter related data based on current entity
 */
import React, { createContext, useContext, useEffect, useState, useCallback, ReactNode } from 'react';
import { useLocation } from 'react-router-dom';

export type EntityType = 
  | 'Customer' 
  | 'Contact' 
  | 'Lead' 
  | 'Opportunity' 
  | 'Campaign' 
  | 'Quote' 
  | 'ServiceRequest' 
  | 'Product' 
  | 'Task' 
  | 'Interaction'
  | 'Note'
  | null;

export interface EntityContextInfo {
  entityType: EntityType;
  entityId: number | null;
  entityName?: string;
  contextPath: string;
}

interface EntityContextValue {
  // Current entity context from URL
  currentContext: EntityContextInfo;
  
  // Manual override for dialogs/modals
  setManualContext: (context: Partial<EntityContextInfo> | null) => void;
  manualContext: EntityContextInfo | null;
  
  // Effective context (manual takes precedence)
  effectiveContext: EntityContextInfo;
  
  // Helper to check if we're in a specific entity context
  isInContext: (entityType: EntityType) => boolean;
  
  // Get context for API calls
  getContextForApi: () => { entityType: string | null; entityId: number | null; contextPath: string };
}

const defaultContext: EntityContextInfo = {
  entityType: null,
  entityId: null,
  entityName: undefined,
  contextPath: '/',
};

const EntityContext = createContext<EntityContextValue>({
  currentContext: defaultContext,
  setManualContext: () => {},
  manualContext: null,
  effectiveContext: defaultContext,
  isInContext: () => false,
  getContextForApi: () => ({ entityType: null, entityId: null, contextPath: '/' }),
});

// Route patterns to entity type mapping
const routePatterns: { pattern: RegExp; entityType: EntityType }[] = [
  { pattern: /^\/customers\/(\d+)/, entityType: 'Customer' },
  { pattern: /^\/contacts\/(\d+)/, entityType: 'Contact' },
  { pattern: /^\/leads\/(\d+)/, entityType: 'Lead' },
  { pattern: /^\/opportunities\/(\d+)/, entityType: 'Opportunity' },
  { pattern: /^\/campaigns\/(\d+)/, entityType: 'Campaign' },
  { pattern: /^\/quotes\/(\d+)/, entityType: 'Quote' },
  { pattern: /^\/service-requests\/(\d+)/, entityType: 'ServiceRequest' },
  { pattern: /^\/products\/(\d+)/, entityType: 'Product' },
  { pattern: /^\/tasks\/(\d+)/, entityType: 'Task' },
  { pattern: /^\/interactions\/(\d+)/, entityType: 'Interaction' },
  { pattern: /^\/notes\/(\d+)/, entityType: 'Note' },
  // List pages (no specific ID)
  { pattern: /^\/customers$/, entityType: 'Customer' },
  { pattern: /^\/contacts$/, entityType: 'Contact' },
  { pattern: /^\/leads$/, entityType: 'Lead' },
  { pattern: /^\/opportunities$/, entityType: 'Opportunity' },
  { pattern: /^\/campaigns$/, entityType: 'Campaign' },
  { pattern: /^\/quotes$/, entityType: 'Quote' },
  { pattern: /^\/service-requests$/, entityType: 'ServiceRequest' },
  { pattern: /^\/products$/, entityType: 'Product' },
  { pattern: /^\/tasks$/, entityType: 'Task' },
  { pattern: /^\/interactions$/, entityType: 'Interaction' },
  { pattern: /^\/notes$/, entityType: 'Note' },
];

function parseContextFromPath(pathname: string): EntityContextInfo {
  for (const { pattern, entityType } of routePatterns) {
    const match = pathname.match(pattern);
    if (match) {
      const entityId = match[1] ? parseInt(match[1], 10) : null;
      return {
        entityType,
        entityId,
        contextPath: pathname,
      };
    }
  }
  
  return {
    entityType: null,
    entityId: null,
    contextPath: pathname,
  };
}

interface EntityContextProviderProps {
  children: ReactNode;
}

export const EntityContextProvider: React.FC<EntityContextProviderProps> = ({ children }) => {
  const location = useLocation();
  const [currentContext, setCurrentContext] = useState<EntityContextInfo>(defaultContext);
  const [manualContext, setManualContextState] = useState<EntityContextInfo | null>(null);

  // Parse context from URL path
  useEffect(() => {
    const parsedContext = parseContextFromPath(location.pathname);
    setCurrentContext(parsedContext);
  }, [location.pathname]);

  const setManualContext = useCallback((context: Partial<EntityContextInfo> | null) => {
    if (context === null) {
      setManualContextState(null);
    } else {
      setManualContextState({
        entityType: context.entityType ?? null,
        entityId: context.entityId ?? null,
        entityName: context.entityName,
        contextPath: context.contextPath ?? location.pathname,
      });
    }
  }, [location.pathname]);

  // Effective context: manual takes precedence
  const effectiveContext = manualContext ?? currentContext;

  const isInContext = useCallback((entityType: EntityType): boolean => {
    return effectiveContext.entityType === entityType;
  }, [effectiveContext.entityType]);

  const getContextForApi = useCallback(() => ({
    entityType: effectiveContext.entityType,
    entityId: effectiveContext.entityId,
    contextPath: effectiveContext.contextPath,
  }), [effectiveContext]);

  const value: EntityContextValue = {
    currentContext,
    setManualContext,
    manualContext,
    effectiveContext,
    isInContext,
    getContextForApi,
  };

  return (
    <EntityContext.Provider value={value}>
      {children}
    </EntityContext.Provider>
  );
};

export const useEntityContext = () => {
  const context = useContext(EntityContext);
  if (!context) {
    throw new Error('useEntityContext must be used within an EntityContextProvider');
  }
  return context;
};

export default EntityContext;
