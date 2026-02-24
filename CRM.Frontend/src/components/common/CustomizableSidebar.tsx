/**
 * CustomizableSidebar - Customizable navigation sidebar with drag-drop reordering
 */

import React, { useState, useCallback, useEffect, useMemo, DragEvent } from 'react';
import {
  Box,
  Drawer,
  List,
  ListItem,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  Collapse,
  IconButton,
  Tooltip,
  Typography,
  Switch,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  Divider,
  Stack,
  useTheme,
  alpha,
} from '@mui/material';
import {
  DragIndicator as DragIcon,
  ExpandMore as ExpandIcon,
  ExpandLess as CollapseIcon,
  Settings as SettingsIcon,
  Visibility as VisibleIcon,
  VisibilityOff as HiddenIcon,
  RestartAlt as ResetIcon,
} from '@mui/icons-material';

// Sidebar section configuration
export interface SidebarSection {
  id: string;
  title: string;
  icon: React.ReactNode;
  path?: string;
  children?: SidebarItem[];
  visible?: boolean;
  order?: number;
}

export interface SidebarItem {
  id: string;
  title: string;
  icon?: React.ReactNode;
  path: string;
  badge?: number | string;
  visible?: boolean;
}

// User preferences
export interface SidebarPreferences {
  sectionOrder: string[];
  hiddenSections: string[];
  expandedSections: string[];
}

// Props
export interface CustomizableSidebarProps {
  sections: SidebarSection[];
  defaultPreferences?: SidebarPreferences;
  // Display
  width?: number;
  collapsedWidth?: number;
  collapsed?: boolean;
  // Callbacks
  onNavigate?: (path: string) => void;
  onPreferencesChange?: (preferences: SidebarPreferences) => void;
  // Active state
  activePath?: string;
  // Feature flags
  enableDragDrop?: boolean;
  enableCustomization?: boolean;
  // Styling
  ariaLabel?: string;
}

const STORAGE_KEY = 'crm_sidebar_preferences';

export const CustomizableSidebar: React.FC<CustomizableSidebarProps> = ({
  sections,
  defaultPreferences,
  width = 280,
  collapsedWidth = 72,
  collapsed = false,
  onNavigate,
  onPreferencesChange,
  activePath,
  enableDragDrop = true,
  enableCustomization = true,
  ariaLabel = 'Main navigation',
}) => {
  const theme = useTheme();

  // Load preferences from storage or use defaults
  const [preferences, setPreferences] = useState<SidebarPreferences>(() => {
    if (typeof window !== 'undefined') {
      const stored = localStorage.getItem(STORAGE_KEY);
      if (stored) {
        try {
          return JSON.parse(stored);
        } catch {
          // Invalid stored data
        }
      }
    }
    return defaultPreferences || {
      sectionOrder: sections.map((s) => s.id),
      hiddenSections: [],
      expandedSections: [],
    };
  });

  const [settingsOpen, setSettingsOpen] = useState(false);
  const [draggedItem, setDraggedItem] = useState<string | null>(null);
  const [dragOverItem, setDragOverItem] = useState<string | null>(null);

  // Persist preferences
  useEffect(() => {
    if (typeof window !== 'undefined') {
      localStorage.setItem(STORAGE_KEY, JSON.stringify(preferences));
    }
    onPreferencesChange?.(preferences);
  }, [preferences, onPreferencesChange]);

  // Sort sections by preference order
  const orderedSections = useMemo(() => {
    const sectionMap = new Map(sections.map((s) => [s.id, s]));
    const ordered: SidebarSection[] = [];

    // Add sections in preference order
    preferences.sectionOrder.forEach((id) => {
      const section = sectionMap.get(id);
      if (section) {
        ordered.push({
          ...section,
          visible: !preferences.hiddenSections.includes(id),
        });
        sectionMap.delete(id);
      }
    });

    // Add any new sections not in preferences
    sectionMap.forEach((section) => {
      ordered.push({
        ...section,
        visible: !preferences.hiddenSections.includes(section.id),
      });
    });

    return ordered;
  }, [sections, preferences]);

  // Visible sections only
  const visibleSections = useMemo(
    () => orderedSections.filter((s) => s.visible),
    [orderedSections]
  );

  // Toggle section visibility
  const toggleSectionVisibility = useCallback((sectionId: string) => {
    setPreferences((prev) => ({
      ...prev,
      hiddenSections: prev.hiddenSections.includes(sectionId)
        ? prev.hiddenSections.filter((id) => id !== sectionId)
        : [...prev.hiddenSections, sectionId],
    }));
  }, []);

  // Toggle section expanded
  const toggleExpanded = useCallback((sectionId: string) => {
    setPreferences((prev) => ({
      ...prev,
      expandedSections: prev.expandedSections.includes(sectionId)
        ? prev.expandedSections.filter((id) => id !== sectionId)
        : [...prev.expandedSections, sectionId],
    }));
  }, []);

  // Drag and drop handlers
  const handleDragStart = useCallback((e: DragEvent<HTMLDivElement>, sectionId: string) => {
    setDraggedItem(sectionId);
    e.dataTransfer.effectAllowed = 'move';
    e.dataTransfer.setData('text/plain', sectionId);
  }, []);

  const handleDragOver = useCallback((e: DragEvent<HTMLDivElement>, sectionId: string) => {
    e.preventDefault();
    if (draggedItem && draggedItem !== sectionId) {
      setDragOverItem(sectionId);
    }
  }, [draggedItem]);

  const handleDragEnd = useCallback(() => {
    if (draggedItem && dragOverItem) {
      setPreferences((prev) => {
        const newOrder = [...prev.sectionOrder];
        const fromIndex = newOrder.indexOf(draggedItem);
        const toIndex = newOrder.indexOf(dragOverItem);

        if (fromIndex !== -1 && toIndex !== -1) {
          newOrder.splice(fromIndex, 1);
          newOrder.splice(toIndex, 0, draggedItem);
        }

        return { ...prev, sectionOrder: newOrder };
      });
    }
    setDraggedItem(null);
    setDragOverItem(null);
  }, [draggedItem, dragOverItem]);

  // Reset to defaults
  const resetPreferences = useCallback(() => {
    setPreferences(
      defaultPreferences || {
        sectionOrder: sections.map((s) => s.id),
        hiddenSections: [],
        expandedSections: [],
      }
    );
  }, [defaultPreferences, sections]);

  // Render section
  const renderSection = (section: SidebarSection, index: number) => {
    const isExpanded = preferences.expandedSections.includes(section.id);
    const hasChildren = section.children && section.children.length > 0;
    const isActive = section.path === activePath ||
      section.children?.some((c) => c.path === activePath);
    const isDragging = draggedItem === section.id;
    const isDragOver = dragOverItem === section.id;

    return (
      <Box
        key={section.id}
        draggable={enableDragDrop && !collapsed}
        onDragStart={(e) => handleDragStart(e, section.id)}
        onDragOver={(e) => handleDragOver(e, section.id)}
        onDragEnd={handleDragEnd}
        sx={{
          opacity: isDragging ? 0.5 : 1,
          borderTop: isDragOver ? `2px solid ${theme.palette.primary.main}` : undefined,
          transition: 'opacity 0.2s, border-top 0.2s',
        }}
      >
        <ListItem
          disablePadding
          secondaryAction={
            enableDragDrop && !collapsed ? (
              <Box sx={{ cursor: 'grab', color: 'text.secondary', pr: 1 }}>
                <DragIcon fontSize="small" />
              </Box>
            ) : hasChildren && !collapsed ? (
              <IconButton
                size="small"
                onClick={() => toggleExpanded(section.id)}
                aria-label={isExpanded ? 'Collapse section' : 'Expand section'}
              >
                {isExpanded ? <CollapseIcon /> : <ExpandIcon />}
              </IconButton>
            ) : undefined
          }
        >
          <Tooltip title={collapsed ? section.title : ''} placement="right">
            <ListItemButton
              onClick={() => {
                if (section.path) {
                  onNavigate?.(section.path);
                } else if (hasChildren) {
                  toggleExpanded(section.id);
                }
              }}
              selected={isActive}
              sx={{
                minHeight: 48,
                justifyContent: collapsed ? 'center' : 'initial',
                px: 2.5,
                '&.Mui-selected': {
                  backgroundColor: alpha(theme.palette.primary.main, 0.1),
                  borderRight: `3px solid ${theme.palette.primary.main}`,
                },
              }}
            >
              <ListItemIcon
                sx={{
                  minWidth: 0,
                  mr: collapsed ? 0 : 3,
                  justifyContent: 'center',
                  color: isActive ? 'primary.main' : 'text.secondary',
                }}
              >
                {section.icon}
              </ListItemIcon>
              {!collapsed && (
                <ListItemText
                  primary={section.title}
                  primaryTypographyProps={{
                    fontWeight: isActive ? 600 : 400,
                  }}
                />
              )}
            </ListItemButton>
          </Tooltip>
        </ListItem>

        {/* Children */}
        {hasChildren && !collapsed && (
          <Collapse in={isExpanded} timeout="auto" unmountOnExit>
            <List component="div" disablePadding>
              {section.children
                ?.filter((item) => item.visible !== false)
                .map((item) => {
                  const childActive = item.path === activePath;
                  return (
                    <ListItemButton
                      key={item.id}
                      onClick={() => onNavigate?.(item.path)}
                      selected={childActive}
                      sx={{
                        pl: 6,
                        py: 0.5,
                        '&.Mui-selected': {
                          backgroundColor: alpha(theme.palette.primary.main, 0.08),
                        },
                      }}
                    >
                      {item.icon && (
                        <ListItemIcon sx={{ minWidth: 36 }}>
                          {item.icon}
                        </ListItemIcon>
                      )}
                      <ListItemText
                        primary={item.title}
                        primaryTypographyProps={{
                          variant: 'body2',
                          fontWeight: childActive ? 600 : 400,
                        }}
                      />
                    </ListItemButton>
                  );
                })}
            </List>
          </Collapse>
        )}
      </Box>
    );
  };

  return (
    <>
      <Drawer
        variant="permanent"
        sx={{
          width: collapsed ? collapsedWidth : width,
          flexShrink: 0,
          '& .MuiDrawer-paper': {
            width: collapsed ? collapsedWidth : width,
            boxSizing: 'border-box',
            transition: 'width 0.2s',
            overflowX: 'hidden',
          },
        }}
      >
        <Box
          role="navigation"
          aria-label={ariaLabel}
          sx={{ display: 'flex', flexDirection: 'column', height: '100%' }}
        >
          {/* Sections list */}
          <List sx={{ flex: 1, py: 0 }}>
            {visibleSections.map((section, index) => renderSection(section, index))}
          </List>

          <Divider />

          {/* Settings button */}
          {enableCustomization && (
            <ListItem disablePadding>
              <Tooltip title={collapsed ? 'Customize sidebar' : ''} placement="right">
                <ListItemButton
                  onClick={() => setSettingsOpen(true)}
                  sx={{
                    minHeight: 48,
                    justifyContent: collapsed ? 'center' : 'initial',
                    px: 2.5,
                  }}
                >
                  <ListItemIcon
                    sx={{
                      minWidth: 0,
                      mr: collapsed ? 0 : 3,
                      justifyContent: 'center',
                    }}
                  >
                    <SettingsIcon />
                  </ListItemIcon>
                  {!collapsed && <ListItemText primary="Customize" />}
                </ListItemButton>
              </Tooltip>
            </ListItem>
          )}
        </Box>
      </Drawer>

      {/* Settings dialog */}
      <Dialog
        open={settingsOpen}
        onClose={() => setSettingsOpen(false)}
        maxWidth="sm"
        fullWidth
      >
        <DialogTitle>
          <Stack direction="row" justifyContent="space-between" alignItems="center">
            <Typography variant="h6">Customize Sidebar</Typography>
            <Tooltip title="Reset to defaults">
              <IconButton onClick={resetPreferences} size="small">
                <ResetIcon />
              </IconButton>
            </Tooltip>
          </Stack>
        </DialogTitle>
        <DialogContent dividers>
          <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
            Toggle sections visibility and drag to reorder.
          </Typography>
          <List>
            {orderedSections.map((section) => (
              <ListItem
                key={section.id}
                secondaryAction={
                  <Switch
                    checked={section.visible}
                    onChange={() => toggleSectionVisibility(section.id)}
                    inputProps={{
                      'aria-label': `Toggle ${section.title} visibility`,
                    }}
                  />
                }
              >
                <ListItemIcon>{section.icon}</ListItemIcon>
                <ListItemText primary={section.title} />
              </ListItem>
            ))}
          </List>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setSettingsOpen(false)}>Done</Button>
        </DialogActions>
      </Dialog>
    </>
  );
};

export default CustomizableSidebar;
