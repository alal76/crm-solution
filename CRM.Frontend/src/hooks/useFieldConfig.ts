import { useState, useEffect, useCallback } from 'react';
import apiClient from '../services/apiClient';

export interface ModuleFieldConfiguration {
  id: number;
  moduleName: string;
  fieldName: string;
  fieldLabel: string;
  fieldType: string;
  tabIndex: number;
  tabName: string;
  displayOrder: number;
  isEnabled: boolean;
  isRequired: boolean;
  gridSize: number;
  placeholder?: string;
  helpText?: string;
  options?: string;
  parentField?: string;
  parentFieldValue?: string;
  isReorderable: boolean;
  isRequiredConfigurable: boolean;
  isHideable: boolean;
}

export interface TabConfig {
  index: number;
  name: string;
  fields: ModuleFieldConfiguration[];
}

// Global event for field config updates
export const FIELD_CONFIG_UPDATED_EVENT = 'fieldConfigUpdated';

export const dispatchFieldConfigUpdate = (moduleName: string) => {
  window.dispatchEvent(new CustomEvent(FIELD_CONFIG_UPDATED_EVENT, { detail: { moduleName } }));
};

interface UseFieldConfigResult {
  fieldConfigs: ModuleFieldConfiguration[];
  tabs: TabConfig[];
  loading: boolean;
  error: string | null;
  refresh: () => Promise<void>;
  getTabFields: (tabIndex: number, categoryValue?: number, formData?: any) => ModuleFieldConfiguration[];
  isFieldVisible: (config: ModuleFieldConfiguration, formData: any) => boolean;
}

export function useFieldConfig(moduleName: string): UseFieldConfigResult {
  const [fieldConfigs, setFieldConfigs] = useState<ModuleFieldConfiguration[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetchConfigurations = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      const response = await apiClient.get(`/modulefieldconfigurations/${moduleName}`);
      setFieldConfigs(response.data || []);
    } catch (err: any) {
      console.error(`Error fetching field configurations for ${moduleName}:`, err);
      setError(err.response?.data?.message || 'Failed to load field configurations');
      setFieldConfigs([]);
    } finally {
      setLoading(false);
    }
  }, [moduleName]);

  // Initial load
  useEffect(() => {
    fetchConfigurations();
  }, [fetchConfigurations]);

  // Listen for field config update events
  useEffect(() => {
    const handleUpdate = (event: CustomEvent) => {
      if (event.detail?.moduleName === moduleName || !event.detail?.moduleName) {
        fetchConfigurations();
      }
    };

    window.addEventListener(FIELD_CONFIG_UPDATED_EVENT, handleUpdate as EventListener);
    return () => {
      window.removeEventListener(FIELD_CONFIG_UPDATED_EVENT, handleUpdate as EventListener);
    };
  }, [moduleName, fetchConfigurations]);

  // Group fields into tabs
  const tabs: TabConfig[] = fieldConfigs.reduce((acc, field) => {
    const existing = acc.find(t => t.index === field.tabIndex);
    if (existing) {
      existing.fields.push(field);
    } else {
      acc.push({
        index: field.tabIndex,
        name: field.tabName,
        fields: [field]
      });
    }
    return acc;
  }, [] as TabConfig[]).sort((a, b) => a.index - b.index);

  // Check if a field should be visible based on parent field conditions
  const isFieldVisible = useCallback((config: ModuleFieldConfiguration, formData: any): boolean => {
    if (!config.isEnabled) return false;
    
    // Handle category-based visibility
    if (config.parentField === 'category' && config.parentFieldValue !== undefined) {
      const categoryValue = formData?.category;
      return String(categoryValue) === String(config.parentFieldValue);
    }
    
    // Handle other parent field conditions
    if (config.parentField && config.parentFieldValue) {
      const currentValue = formData?.[config.parentField];
      return String(currentValue ?? '') === String(config.parentFieldValue);
    }
    
    return true;
  }, []);

  // Get fields for a specific tab, optionally filtered by category
  const getTabFields = useCallback((tabIndex: number, categoryValue?: number, formData?: any): ModuleFieldConfiguration[] => {
    const effectiveFormData = formData || { category: categoryValue ?? 0 };
    
    return fieldConfigs
      .filter(f => f.tabIndex === tabIndex)
      .filter(f => isFieldVisible(f, effectiveFormData))
      .sort((a, b) => a.displayOrder - b.displayOrder);
  }, [fieldConfigs, isFieldVisible]);

  return {
    fieldConfigs,
    tabs,
    loading,
    error,
    refresh: fetchConfigurations,
    getTabFields,
    isFieldVisible
  };
}

export default useFieldConfig;
