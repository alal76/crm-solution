/**
 * DynamicEntityForm - A universal component that renders entity form fields
 * from API-driven field configurations (ModuleFieldConfiguration).
 * 
 * This replaces hardcoded form fields across all entity pages (Contacts, Leads,
 * Opportunities, Products) with a single, configuration-driven component.
 * 
 * When fields are added/changed in the admin Module & Field Settings, the UI
 * updates automatically because useFieldConfig fetches from the API and listens
 * for FIELD_CONFIG_UPDATED_EVENT events.
 * 
 * Usage:
 *   <DynamicEntityForm
 *     moduleName="Contacts"
 *     formData={formData}
 *     onChange={handleChange}
 *     onSelectChange={handleSelectChange}
 *     setFormData={setFormData}
 *     activeTab={dialogTab}
 *     editingId={selectedId}
 *     extraTabs={[
 *       { index: 100, name: 'Contact Info', icon: <ContactPhoneIcon />, editOnly: true, render: () => <ContactInfoPanel /> },
 *     ]}
 *     fieldOverrides={{
 *       'category': { disabled: true },
 *     }}
 *     excludeFields={['tags', 'customFields']}
 *   />
 */
import React, { useMemo } from 'react';
import {
  Grid,
  Box,
  Typography,
  Tabs,
  Tab,
  CircularProgress,
  Alert,
  IconButton,
  Tooltip,
} from '@mui/material';
import RefreshIcon from '@mui/icons-material/Refresh';
import FieldRenderer from './FieldRenderer';
import { useFieldConfig, ModuleFieldConfiguration } from '../hooks/useFieldConfig';

// ─── Types ──────────────────────────────────────────────────────────────────────

export interface ExtraTab {
  /** A high index (e.g. 100+) so it doesn't collide with field-config tabs */
  index: number;
  /** Tab label */
  name: string;
  /** Optional icon */
  icon?: React.ReactElement;
  /** Only show when editing (editingId is set) */
  editOnly?: boolean;
  /** Render callback for the tab panel content */
  render: () => React.ReactNode;
}

export interface FieldOverride {
  disabled?: boolean;
  /** Replace the rendered field entirely */
  render?: (config: ModuleFieldConfiguration, formData: any) => React.ReactNode;
}

export interface DynamicEntityFormProps {
  /** Module name matching backend (e.g. "Accounts", "Contacts", "Leads") */
  moduleName: string;
  /** Form state object */
  formData: any;
  /** Standard onChange handler for text inputs */
  onChange: (e: any) => void;
  /** Handler for select/autocomplete changes */
  onSelectChange: (e: any) => void;
  /** State setter for form data (used by slider, checkbox etc.) */
  setFormData?: (fn: any) => void;
  /** Currently active tab index (controlled externally by parent Tabs) */
  activeTab: number;
  /** ID of the record being edited, or null/undefined for create mode */
  editingId?: number | null;
  /** Additional tabs appended after field-config tabs */
  extraTabs?: ExtraTab[];
  /** Per-field overrides keyed by fieldName */
  fieldOverrides?: Record<string, FieldOverride>;
  /** Field names to exclude from rendering */
  excludeFields?: Set<string> | string[];
  /** Callback when tab changes */
  onTabChange?: (newTab: number) => void;
  /** Show a refresh button for field configs */
  showRefreshButton?: boolean;
}

// ─── TabPanel ───────────────────────────────────────────────────────────────────

interface TabPanelProps {
  children?: React.ReactNode;
  value: number;
  index: number;
}

const TabPanel: React.FC<TabPanelProps> = ({ children, value, index }) => (
  <div role="tabpanel" hidden={value !== index} id={`dynamic-tabpanel-${index}`}>
    {value === index && <Box sx={{ pt: 2 }}>{children}</Box>}
  </div>
);

// ─── Component ──────────────────────────────────────────────────────────────────

const DynamicEntityForm: React.FC<DynamicEntityFormProps> = ({
  moduleName,
  formData,
  onChange,
  onSelectChange,
  setFormData,
  activeTab,
  editingId,
  extraTabs = [],
  fieldOverrides = {},
  excludeFields: excludeFieldsProp,
  onTabChange,
  showRefreshButton = true,
}) => {
  const {
    fieldConfigs,
    tabs,
    loading: fieldConfigsLoading,
    error: fieldConfigsError,
    refresh: refreshFieldConfigs,
    getTabFields,
    isFieldVisible,
  } = useFieldConfig(moduleName);

  // Normalize excludeFields to a Set
  const excludeFields = useMemo(() => {
    if (!excludeFieldsProp) return new Set<string>();
    if (excludeFieldsProp instanceof Set) return excludeFieldsProp;
    return new Set(excludeFieldsProp);
  }, [excludeFieldsProp]);

  // Build the combined tab list: field-config tabs + extra tabs
  const visibleTabs = useMemo(() => {
    const baseTabs = tabs.map(t => ({ index: t.index, name: t.name, isFieldTab: true }));

    for (const extra of extraTabs) {
      if (extra.editOnly && !editingId) continue;
      baseTabs.push({ index: extra.index, name: extra.name, isFieldTab: false });
    }

    return baseTabs;
  }, [tabs, extraTabs, editingId]);

  // Render fields for a field-config tab
  const renderTabFields = (tabIndex: number) => {
    const tabFields = getTabFields(tabIndex, formData.category, formData)
      .filter(cfg => !excludeFields.has(cfg.fieldName));

    if (!tabFields.length) {
      return (
        <Box sx={{ py: 4, textAlign: 'center' }}>
          <Typography color="textSecondary">No fields configured for this tab</Typography>
        </Box>
      );
    }

    return (
      <Grid container spacing={2}>
        {tabFields.map(config => {
          const override = fieldOverrides[config.fieldName];

          // Custom render override
          if (override?.render) {
            return (
              <Grid key={config.id} item xs={12} sm={config.gridSize || 12}>
                {override.render(config, formData)}
              </Grid>
            );
          }

          return (
            <Grid key={config.id} item xs={12} sm={config.gridSize || 12}>
              <FieldRenderer
                config={config}
                formData={formData}
                onChange={onChange}
                onSelectChange={onSelectChange}
                setFormData={setFormData}
                disabled={override?.disabled}
              />
            </Grid>
          );
        })}
      </Grid>
    );
  };

  // ─── Loading / Error states ─────────────────────────────────────────────────

  if (fieldConfigsLoading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
        <CircularProgress />
      </Box>
    );
  }

  if (fieldConfigsError) {
    return (
      <Alert severity="warning" sx={{ mb: 2 }}>
        {fieldConfigsError}. Showing default form layout.
        <IconButton size="small" onClick={refreshFieldConfigs} sx={{ ml: 1 }}>
          <RefreshIcon fontSize="small" />
        </IconButton>
      </Alert>
    );
  }

  // ─── Render ─────────────────────────────────────────────────────────────────

  return (
    <>
      {/* Tabs header */}
      <Box sx={{ borderBottom: 1, borderColor: 'divider' }}>
        <Box sx={{ display: 'flex', alignItems: 'center' }}>
          <Tabs
            value={activeTab}
            onChange={(_, v) => onTabChange?.(v)}
            variant="scrollable"
            scrollButtons="auto"
            sx={{ flex: 1 }}
          >
            {visibleTabs.map((tab, idx) => {
              const extra = extraTabs.find(e => e.index === tab.index);
              return (
                <Tab
                  key={tab.index}
                  label={tab.name}
                  icon={extra?.icon || undefined}
                  iconPosition="start"
                  id={`${moduleName.toLowerCase()}-tab-${idx}`}
                  aria-controls={`${moduleName.toLowerCase()}-tabpanel-${idx}`}
                />
              );
            })}
          </Tabs>
          {showRefreshButton && (
            <Tooltip title="Refresh field configurations">
              <IconButton
                onClick={refreshFieldConfigs}
                size="small"
                sx={{ color: '#6750A4', mr: 1 }}
              >
                <RefreshIcon fontSize="small" />
              </IconButton>
            </Tooltip>
          )}
        </Box>
      </Box>

      {/* Tab panels */}
      {visibleTabs.map((tab, idx) => {
        if (tab.isFieldTab) {
          return (
            <TabPanel key={tab.index} value={activeTab} index={idx}>
              {renderTabFields(tab.index)}
            </TabPanel>
          );
        }

        // Extra tab
        const extra = extraTabs.find(e => e.index === tab.index);
        return (
          <TabPanel key={tab.index} value={activeTab} index={idx}>
            {extra?.render() ?? null}
          </TabPanel>
        );
      })}
    </>
  );
};

export default DynamicEntityForm;
