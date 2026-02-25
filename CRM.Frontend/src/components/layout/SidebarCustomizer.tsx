/**
 * SidebarCustomizer — allows users to show/hide and reorder navigation categories and items.
 * Preferences are persisted to localStorage key `crm_sidebar_config`.
 * Dispatches `navigationUpdated` window event so Navigation refreshes immediately.
 */
import React, { useState, useEffect, useCallback } from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  Box,
  Typography,
  Switch,
  IconButton,
  Divider,
  List,
  ListItem,
  ListItemText,
  Collapse,
  Alert,
  Tooltip,
} from '@mui/material';
import {
  ArrowUpward as ArrowUpIcon,
  ArrowDownward as ArrowDownIcon,
  ExpandLess,
  ExpandMore,
  RestoreOutlined as ResetIcon,
  Tune as TuneIcon,
} from '@mui/icons-material';

// ─── Types ──────────────────────────────────────────────────────────────────

export interface NavItemEntry {
  id: string;
  visible: boolean;
  order: number;
  category: string;
  customLabel?: string;
}

export interface CategoryEntry {
  id: string;
  label: string;
  order: number;
}

interface SidebarCustomizerProps {
  open: boolean;
  onClose: () => void;
  /** All categories in their current order */
  categories: CategoryEntry[];
  /** All nav items with visibility and order */
  navItems: NavItemEntry[];
  /** Mapping of itemId → display label */
  itemLabels: Record<string, string>;
}

const STORAGE_KEY = 'crm_sidebar_config';

// ─── Helpers ─────────────────────────────────────────────────────────────────

function reorder<T>(arr: T[], fromIndex: number, toIndex: number): T[] {
  const copy = [...arr];
  const [removed] = copy.splice(fromIndex, 1);
  copy.splice(toIndex, 0, removed);
  return copy;
}

function assignOrders<T extends { order: number }>(arr: T[]): T[] {
  return arr.map((item, idx) => ({ ...item, order: idx }));
}

// ─── Component ───────────────────────────────────────────────────────────────

const SidebarCustomizer: React.FC<SidebarCustomizerProps> = ({
  open,
  onClose,
  categories,
  navItems,
  itemLabels,
}) => {
  const [localCategories, setLocalCategories] = useState<CategoryEntry[]>([]);
  const [localItems, setLocalItems] = useState<NavItemEntry[]>([]);
  const [expandedCats, setExpandedCats] = useState<Record<string, boolean>>({});
  const [saved, setSaved] = useState(false);

  // Initialise from localStorage or from props on open
  useEffect(() => {
    if (!open) return;
    setSaved(false);

    try {
      const stored = localStorage.getItem(STORAGE_KEY);
      if (stored) {
        const parsed: { categories?: CategoryEntry[]; navItems?: NavItemEntry[] } = JSON.parse(stored);
        // Merge stored visibility/order with current props (props may have new items)
        const storedItemMap = new Map((parsed.navItems || []).map((i) => [i.id, i]));
        const mergedItems: NavItemEntry[] = navItems.map((item) => {
          const s = storedItemMap.get(item.id);
          return s ? { ...item, visible: s.visible, order: s.order } : item;
        });
        mergedItems.sort((a, b) => a.order - b.order);

        const storedCatMap = new Map((parsed.categories || []).map((c) => [c.id, c]));
        const mergedCats: CategoryEntry[] = categories.map((cat) => {
          const s = storedCatMap.get(cat.id);
          return s ? { ...cat, order: s.order } : cat;
        });
        mergedCats.sort((a, b) => a.order - b.order);

        setLocalCategories(mergedCats);
        setLocalItems(mergedItems);
        return;
      }
    } catch {
      // fall through to defaults
    }

    setLocalCategories([...categories].sort((a, b) => a.order - b.order));
    setLocalItems([...navItems].sort((a, b) => a.order - b.order));
  }, [open, categories, navItems]);

  // Default all cats expanded
  useEffect(() => {
    const initial: Record<string, boolean> = {};
    categories.forEach((c) => { initial[c.id] = true; });
    setExpandedCats(initial);
  }, [categories]);

  // ── Category actions ──────────────────────────────────────────────────────

  const toggleCategoryExpanded = (catId: string) => {
    setExpandedCats((prev) => ({ ...prev, [catId]: !prev[catId] }));
  };

  const moveCategoryUp = useCallback((index: number) => {
    if (index === 0) return;
    setLocalCategories((prev) => assignOrders(reorder(prev, index, index - 1)));
  }, []);

  const moveCategoryDown = useCallback((index: number) => {
    setLocalCategories((prev) => {
      if (index >= prev.length - 1) return prev;
      return assignOrders(reorder(prev, index, index + 1));
    });
  }, []);

  // ── Item actions ──────────────────────────────────────────────────────────

  const toggleItemVisible = useCallback((itemId: string) => {
    setLocalItems((prev) =>
      prev.map((item) =>
        item.id === itemId ? { ...item, visible: !item.visible } : item
      )
    );
  }, []);

  const moveItemUp = useCallback((catId: string, itemIndex: number) => {
    setLocalItems((prev) => {
      const catItems = prev.filter((i) => i.category === catId);
      const rest = prev.filter((i) => i.category !== catId);
      if (itemIndex === 0) return prev;
      const reordered = assignOrders(reorder(catItems, itemIndex, itemIndex - 1));
      return [...rest, ...reordered].sort((a, b) => {
        const ai = localCategories.findIndex((c) => c.id === a.category);
        const bi = localCategories.findIndex((c) => c.id === b.category);
        if (ai !== bi) return ai - bi;
        return a.order - b.order;
      });
    });
  }, [localCategories]);

  const moveItemDown = useCallback((catId: string, itemIndex: number, catItems: NavItemEntry[]) => {
    setLocalItems((prev) => {
      const currentCatItems = prev.filter((i) => i.category === catId);
      const rest = prev.filter((i) => i.category !== catId);
      if (itemIndex >= currentCatItems.length - 1) return prev;
      const reordered = assignOrders(reorder(currentCatItems, itemIndex, itemIndex + 1));
      return [...rest, ...reordered].sort((a, b) => {
        const ai = localCategories.findIndex((c) => c.id === a.category);
        const bi = localCategories.findIndex((c) => c.id === b.category);
        if (ai !== bi) return ai - bi;
        return a.order - b.order;
      });
    });
  }, [localCategories]);

  // ── Save / Reset ──────────────────────────────────────────────────────────

  const handleSave = () => {
    const config = {
      categories: localCategories,
      navItems: localItems,
    };
    localStorage.setItem(STORAGE_KEY, JSON.stringify(config));
    // Also keep backward-compat key used by Navigation
    localStorage.setItem('crm_nav_order', JSON.stringify(config));
    window.dispatchEvent(new Event('navigationUpdated'));
    setSaved(true);
    setTimeout(() => onClose(), 800);
  };

  const handleReset = () => {
    localStorage.removeItem(STORAGE_KEY);
    localStorage.removeItem('crm_nav_order');
    setLocalCategories([...categories].sort((a, b) => a.order - b.order));
    setLocalItems([...navItems].sort((a, b) => a.order - b.order));
    window.dispatchEvent(new Event('navigationUpdated'));
  };

  // ── Render ────────────────────────────────────────────────────────────────

  return (
    <Dialog
      open={open}
      onClose={onClose}
      maxWidth="sm"
      fullWidth
      aria-labelledby="sidebar-customizer-title"
      PaperProps={{ sx: { borderRadius: 2, maxHeight: '85vh' } }}
    >
      <DialogTitle
        id="sidebar-customizer-title"
        sx={{ display: 'flex', alignItems: 'center', gap: 1, pb: 1 }}
      >
        <TuneIcon color="primary" />
        <Typography variant="h6" component="span">
          Customize Navigation
        </Typography>
      </DialogTitle>

      <Divider />

      <DialogContent sx={{ p: 0, overflow: 'auto' }}>
        {saved && (
          <Alert severity="success" sx={{ m: 2, mb: 0 }}>
            Navigation saved!
          </Alert>
        )}

        <Typography variant="caption" color="text.secondary" sx={{ px: 2, py: 1, display: 'block' }}>
          Toggle visibility and drag items up/down to reorder. Changes apply immediately.
        </Typography>

        <List disablePadding>
          {localCategories.map((cat, catIdx) => {
            const catItems = localItems
              .filter((i) => i.category === cat.id)
              .sort((a, b) => a.order - b.order);

            return (
              <React.Fragment key={cat.id}>
                {catIdx > 0 && <Divider />}

                {/* Category row */}
                <ListItem
                  sx={{ bgcolor: 'grey.100', py: 0.5 }}
                  secondaryAction={
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
                      <Tooltip title="Move category up">
                        <span>
                          <IconButton
                            size="small"
                            onClick={() => moveCategoryUp(catIdx)}
                            disabled={catIdx === 0}
                            aria-label={`Move ${cat.label} category up`}
                          >
                            <ArrowUpIcon fontSize="small" />
                          </IconButton>
                        </span>
                      </Tooltip>
                      <Tooltip title="Move category down">
                        <span>
                          <IconButton
                            size="small"
                            onClick={() => moveCategoryDown(catIdx)}
                            disabled={catIdx === localCategories.length - 1}
                            aria-label={`Move ${cat.label} category down`}
                          >
                            <ArrowDownIcon fontSize="small" />
                          </IconButton>
                        </span>
                      </Tooltip>
                      <IconButton
                        size="small"
                        onClick={() => toggleCategoryExpanded(cat.id)}
                        aria-label={expandedCats[cat.id] ? 'Collapse' : 'Expand'}
                        aria-expanded={expandedCats[cat.id]}
                      >
                        {expandedCats[cat.id] ? <ExpandLess fontSize="small" /> : <ExpandMore fontSize="small" />}
                      </IconButton>
                    </Box>
                  }
                >
                  <ListItemText
                    primary={
                      <Typography variant="subtitle2" sx={{ fontWeight: 600, textTransform: 'uppercase', fontSize: '0.75rem', color: 'text.secondary' }}>
                        {cat.label}
                        <Typography component="span" variant="caption" sx={{ ml: 1, color: 'text.disabled' }}>
                          ({catItems.filter((i) => i.visible).length}/{catItems.length} visible)
                        </Typography>
                      </Typography>
                    }
                  />
                </ListItem>

                {/* Items within this category */}
                <Collapse in={expandedCats[cat.id] ?? true} timeout="auto">
                  <List disablePadding dense>
                    {catItems.map((item, itemIdx) => {
                      const label = item.customLabel || itemLabels[item.id] || item.id;
                      return (
                        <ListItem
                          key={item.id}
                          sx={{ pl: 4, py: 0.25, opacity: item.visible ? 1 : 0.4 }}
                          secondaryAction={
                            <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
                              <Tooltip title="Move item up">
                                <span>
                                  <IconButton
                                    size="small"
                                    onClick={() => moveItemUp(cat.id, itemIdx)}
                                    disabled={itemIdx === 0}
                                    aria-label={`Move ${label} up`}
                                  >
                                    <ArrowUpIcon sx={{ fontSize: '0.9rem' }} />
                                  </IconButton>
                                </span>
                              </Tooltip>
                              <Tooltip title="Move item down">
                                <span>
                                  <IconButton
                                    size="small"
                                    onClick={() => moveItemDown(cat.id, itemIdx, catItems)}
                                    disabled={itemIdx === catItems.length - 1}
                                    aria-label={`Move ${label} down`}
                                  >
                                    <ArrowDownIcon sx={{ fontSize: '0.9rem' }} />
                                  </IconButton>
                                </span>
                              </Tooltip>
                              <Tooltip title={item.visible ? 'Hide item' : 'Show item'}>
                                <Switch
                                  size="small"
                                  checked={item.visible}
                                  onChange={() => toggleItemVisible(item.id)}
                                  inputProps={{ 'aria-label': `Toggle visibility of ${label}` }}
                                />
                              </Tooltip>
                            </Box>
                          }
                        >
                          <ListItemText
                            primary={
                              <Typography variant="body2" sx={{ fontSize: '0.82rem' }}>
                                {label}
                              </Typography>
                            }
                          />
                        </ListItem>
                      );
                    })}

                    {catItems.length === 0 && (
                      <ListItem sx={{ pl: 4 }}>
                        <ListItemText
                          secondary={<Typography variant="caption" color="text.disabled">No items</Typography>}
                        />
                      </ListItem>
                    )}
                  </List>
                </Collapse>
              </React.Fragment>
            );
          })}
        </List>
      </DialogContent>

      <Divider />
      <DialogActions sx={{ justifyContent: 'space-between', px: 2, py: 1.5 }}>
        <Button
          startIcon={<ResetIcon />}
          onClick={handleReset}
          size="small"
          color="inherit"
        >
          Reset to Defaults
        </Button>
        <Box sx={{ display: 'flex', gap: 1 }}>
          <Button onClick={onClose} size="small">Cancel</Button>
          <Button variant="contained" onClick={handleSave} size="small">
            Save
          </Button>
        </Box>
      </DialogActions>
    </Dialog>
  );
};

export default SidebarCustomizer;
