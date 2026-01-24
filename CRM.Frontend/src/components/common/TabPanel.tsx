import React from 'react';
import { Box } from '@mui/material';

/**
 * Props for the TabPanel component
 * Supports both number and string indexes for flexibility
 */
export interface TabPanelProps {
  children?: React.ReactNode;
  index: number | string;
  value: number | string;
  /** Optional padding override (default: 3) */
  padding?: number;
  /** Optional Box props to customize the container */
  boxProps?: React.ComponentProps<typeof Box>;
}

/**
 * Reusable TabPanel component for Material-UI tab interfaces.
 * Provides consistent tab content rendering with proper accessibility attributes.
 * 
 * @example
 * ```tsx
 * <Tabs value={tabValue} onChange={handleTabChange}>
 *   <Tab label="Tab 1" />
 *   <Tab label="Tab 2" />
 * </Tabs>
 * <TabPanel value={tabValue} index={0}>
 *   Content for Tab 1
 * </TabPanel>
 * <TabPanel value={tabValue} index={1}>
 *   Content for Tab 2
 * </TabPanel>
 * ```
 */
export const TabPanel: React.FC<TabPanelProps> = ({
  children,
  value,
  index,
  padding = 3,
  boxProps,
  ...other
}) => {
  return (
    <div
      role="tabpanel"
      hidden={value !== index}
      id={`tabpanel-${index}`}
      aria-labelledby={`tab-${index}`}
      {...other}
    >
      {value === index && (
        <Box sx={{ p: padding, ...boxProps?.sx }} {...boxProps}>
          {children}
        </Box>
      )}
    </div>
  );
};

/**
 * Helper function to generate accessibility props for tabs.
 * Use this with the Tab component's props.
 * Supports both number and string indexes for flexibility.
 * 
 * @example
 * ```tsx
 * <Tabs value={tabValue} onChange={handleTabChange}>
 *   <Tab label="Tab 1" {...a11yProps(0)} />
 *   <Tab label="Tab 2" {...a11yProps(1)} />
 * </Tabs>
 * ```
 */
export function a11yProps(index: number | string) {
  return {
    id: `tab-${index}`,
    'aria-controls': `tabpanel-${index}`,
  };
}

export default TabPanel;
