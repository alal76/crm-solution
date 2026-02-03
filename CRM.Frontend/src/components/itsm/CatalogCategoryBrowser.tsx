// Catalog Category Browser - Category navigation for service catalog
// Part of ITSM Enhancement Plan - Phase 4.1

import React, { useState, useMemo } from 'react';
import {
  Box,
  Paper,
  Typography,
  List,
  ListItem,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  Collapse,
  Breadcrumbs,
  Link,
  Card,
  CardContent,
  CardActionArea,
  Grid,
  Stack,
  Chip,
  TextField,
  InputAdornment,
  Badge,
  IconButton,
  Divider,
  Tooltip,
} from '@mui/material';
import {
  Folder as FolderIcon,
  FolderOpen as FolderOpenIcon,
  Description as ItemIcon,
  ExpandMore as ExpandIcon,
  ExpandLess as CollapseIcon,
  Search as SearchIcon,
  Home as HomeIcon,
  NavigateNext as NavIcon,
  Star as FeaturedIcon,
  LocalOffer as TagIcon,
  AccessTime as TimeIcon,
  Category as CategoryIcon,
  Clear as ClearIcon,
} from '@mui/icons-material';

export interface CatalogCategory {
  id: number;
  name: string;
  description?: string;
  icon?: string;
  parentId?: number;
  itemCount: number;
  children?: CatalogCategory[];
  featured?: boolean;
}

export interface CatalogItem {
  id: number;
  name: string;
  description: string;
  categoryId: number;
  categoryName: string;
  icon?: string;
  price?: number;
  deliveryTime?: string;
  popular?: boolean;
  tags?: string[];
}

export interface CatalogCategoryBrowserProps {
  categories: CatalogCategory[];
  items?: CatalogItem[];
  selectedCategoryId?: number;
  onCategorySelect?: (categoryId: number | null) => void;
  onItemSelect?: (itemId: number) => void;
  variant?: 'tree' | 'grid' | 'combined';
  showSearch?: boolean;
  showBreadcrumbs?: boolean;
  maxDepth?: number;
}

// Helper to build category tree from flat list
const buildCategoryTree = (categories: CatalogCategory[]): CatalogCategory[] => {
  const categoryMap = new Map<number, CatalogCategory>();
  const rootCategories: CatalogCategory[] = [];

  // First pass: create map of all categories
  categories.forEach((cat) => {
    categoryMap.set(cat.id, { ...cat, children: [] });
  });

  // Second pass: build tree structure
  categories.forEach((cat) => {
    const category = categoryMap.get(cat.id)!;
    if (cat.parentId && categoryMap.has(cat.parentId)) {
      const parent = categoryMap.get(cat.parentId)!;
      parent.children = parent.children || [];
      parent.children.push(category);
    } else {
      rootCategories.push(category);
    }
  });

  return rootCategories;
};

// Helper to get category path (breadcrumb)
const getCategoryPath = (categoryId: number, categories: CatalogCategory[]): CatalogCategory[] => {
  const path: CatalogCategory[] = [];
  let current = categories.find((c) => c.id === categoryId);

  while (current) {
    path.unshift(current);
    current = current.parentId ? categories.find((c) => c.id === current!.parentId) : undefined;
  }

  return path;
};

// Tree view component
const CategoryTreeItem: React.FC<{
  category: CatalogCategory;
  selectedId?: number;
  onSelect: (id: number) => void;
  depth: number;
  maxDepth: number;
  expanded: Set<number>;
  onToggle: (id: number) => void;
}> = ({ category, selectedId, onSelect, depth, maxDepth, expanded, onToggle }) => {
  const isExpanded = expanded.has(category.id);
  const hasChildren = category.children && category.children.length > 0;
  const isSelected = selectedId === category.id;

  if (depth > maxDepth) return null;

  return (
    <>
      <ListItem disablePadding>
        <ListItemButton
          selected={isSelected}
          onClick={() => onSelect(category.id)}
          sx={{ pl: 2 + depth * 2 }}
        >
          <ListItemIcon sx={{ minWidth: 36 }}>
            {hasChildren ? (
              isExpanded ? (
                <FolderOpenIcon color={isSelected ? 'primary' : 'inherit'} />
              ) : (
                <FolderIcon color={isSelected ? 'primary' : 'inherit'} />
              )
            ) : (
              <CategoryIcon color={isSelected ? 'primary' : 'inherit'} />
            )}
          </ListItemIcon>
          <ListItemText
            primary={
              <Stack direction="row" alignItems="center" spacing={1}>
                <Typography
                  variant="body2"
                  fontWeight={isSelected ? 600 : 400}
                  noWrap
                >
                  {category.name}
                </Typography>
                {category.featured && (
                  <FeaturedIcon sx={{ fontSize: 14, color: '#ffc107' }} />
                )}
              </Stack>
            }
            secondary={
              <Typography variant="caption" color="text.secondary">
                {category.itemCount} item{category.itemCount !== 1 ? 's' : ''}
              </Typography>
            }
          />
          {hasChildren && (
            <IconButton
              size="small"
              onClick={(e) => {
                e.stopPropagation();
                onToggle(category.id);
              }}
            >
              {isExpanded ? <CollapseIcon /> : <ExpandIcon />}
            </IconButton>
          )}
        </ListItemButton>
      </ListItem>
      {hasChildren && (
        <Collapse in={isExpanded}>
          <List disablePadding>
            {category.children!.map((child) => (
              <CategoryTreeItem
                key={child.id}
                category={child}
                selectedId={selectedId}
                onSelect={onSelect}
                depth={depth + 1}
                maxDepth={maxDepth}
                expanded={expanded}
                onToggle={onToggle}
              />
            ))}
          </List>
        </Collapse>
      )}
    </>
  );
};

// Grid view component
const CategoryGridView: React.FC<{
  categories: CatalogCategory[];
  selectedId?: number;
  onSelect: (id: number) => void;
}> = ({ categories, selectedId, onSelect }) => (
  <Grid container spacing={2}>
    {categories.map((category) => (
      <Grid item xs={12} sm={6} md={4} lg={3} key={category.id}>
        <Card
          variant="outlined"
          sx={{
            borderColor: selectedId === category.id ? 'primary.main' : undefined,
            backgroundColor: selectedId === category.id ? 'action.selected' : undefined,
          }}
        >
          <CardActionArea onClick={() => onSelect(category.id)}>
            <CardContent>
              <Stack direction="row" alignItems="flex-start" spacing={2}>
                <Badge
                  badgeContent={category.itemCount}
                  color="primary"
                  max={99}
                >
                  <FolderIcon fontSize="large" color="action" />
                </Badge>
                <Box sx={{ flex: 1 }}>
                  <Stack direction="row" alignItems="center" spacing={1}>
                    <Typography variant="subtitle1" fontWeight={500}>
                      {category.name}
                    </Typography>
                    {category.featured && (
                      <Tooltip title="Featured">
                        <FeaturedIcon sx={{ fontSize: 16, color: '#ffc107' }} />
                      </Tooltip>
                    )}
                  </Stack>
                  {category.description && (
                    <Typography
                      variant="body2"
                      color="text.secondary"
                      sx={{
                        display: '-webkit-box',
                        WebkitLineClamp: 2,
                        WebkitBoxOrient: 'vertical',
                        overflow: 'hidden',
                      }}
                    >
                      {category.description}
                    </Typography>
                  )}
                </Box>
              </Stack>
            </CardContent>
          </CardActionArea>
        </Card>
      </Grid>
    ))}
  </Grid>
);

// Item card component
const CatalogItemCard: React.FC<{
  item: CatalogItem;
  onSelect: (id: number) => void;
}> = ({ item, onSelect }) => (
  <Card variant="outlined" sx={{ height: '100%' }}>
    <CardActionArea onClick={() => onSelect(item.id)} sx={{ height: '100%' }}>
      <CardContent>
        <Stack spacing={1}>
          <Stack direction="row" alignItems="flex-start" justifyContent="space-between">
            <Typography variant="subtitle2" fontWeight={500}>
              {item.name}
            </Typography>
            {item.popular && (
              <Chip label="Popular" size="small" color="secondary" sx={{ height: 20 }} />
            )}
          </Stack>
          <Typography
            variant="body2"
            color="text.secondary"
            sx={{
              display: '-webkit-box',
              WebkitLineClamp: 2,
              WebkitBoxOrient: 'vertical',
              overflow: 'hidden',
            }}
          >
            {item.description}
          </Typography>
          <Stack direction="row" alignItems="center" spacing={1} flexWrap="wrap" useFlexGap>
            {item.deliveryTime && (
              <Chip
                icon={<TimeIcon sx={{ fontSize: 14 }} />}
                label={item.deliveryTime}
                size="small"
                variant="outlined"
                sx={{ height: 24 }}
              />
            )}
            {item.tags?.slice(0, 2).map((tag) => (
              <Chip
                key={tag}
                icon={<TagIcon sx={{ fontSize: 14 }} />}
                label={tag}
                size="small"
                variant="outlined"
                sx={{ height: 24 }}
              />
            ))}
          </Stack>
        </Stack>
      </CardContent>
    </CardActionArea>
  </Card>
);

export const CatalogCategoryBrowser: React.FC<CatalogCategoryBrowserProps> = ({
  categories,
  items = [],
  selectedCategoryId,
  onCategorySelect,
  onItemSelect,
  variant = 'combined',
  showSearch = true,
  showBreadcrumbs = true,
  maxDepth = 5,
}) => {
  const [expanded, setExpanded] = useState<Set<number>>(new Set());
  const [searchQuery, setSearchQuery] = useState('');

  // Build category tree
  const categoryTree = useMemo(() => buildCategoryTree(categories), [categories]);

  // Get breadcrumb path
  const breadcrumbPath = useMemo(() => {
    if (!selectedCategoryId) return [];
    return getCategoryPath(selectedCategoryId, categories);
  }, [selectedCategoryId, categories]);

  // Filter items for selected category
  const filteredItems = useMemo(() => {
    let result = items;

    if (selectedCategoryId) {
      // Include items from selected category and its children
      const categoryIds = new Set<number>();
      const collectIds = (cats: CatalogCategory[]) => {
        cats.forEach((cat) => {
          if (cat.id === selectedCategoryId) {
            categoryIds.add(cat.id);
            if (cat.children) {
              const addChildren = (children: CatalogCategory[]) => {
                children.forEach((c) => {
                  categoryIds.add(c.id);
                  if (c.children) addChildren(c.children);
                });
              };
              addChildren(cat.children);
            }
          } else if (cat.children) {
            collectIds(cat.children);
          }
        });
      };
      collectIds(categoryTree);
      result = items.filter((item) => categoryIds.has(item.categoryId));
    }

    if (searchQuery) {
      const query = searchQuery.toLowerCase();
      result = result.filter(
        (item) =>
          item.name.toLowerCase().includes(query) ||
          item.description.toLowerCase().includes(query) ||
          item.tags?.some((tag) => tag.toLowerCase().includes(query))
      );
    }

    return result;
  }, [items, selectedCategoryId, searchQuery, categoryTree]);

  // Get current level categories for grid view
  const currentLevelCategories = useMemo(() => {
    if (!selectedCategoryId) return categoryTree;
    const findCategory = (cats: CatalogCategory[]): CatalogCategory | undefined => {
      for (const cat of cats) {
        if (cat.id === selectedCategoryId) return cat;
        if (cat.children) {
          const found = findCategory(cat.children);
          if (found) return found;
        }
      }
      return undefined;
    };
    const selected = findCategory(categoryTree);
    return selected?.children || [];
  }, [selectedCategoryId, categoryTree]);

  const handleToggle = (categoryId: number) => {
    setExpanded((prev) => {
      const next = new Set(prev);
      if (next.has(categoryId)) {
        next.delete(categoryId);
      } else {
        next.add(categoryId);
      }
      return next;
    });
  };

  const handleCategorySelect = (categoryId: number | null) => {
    onCategorySelect?.(categoryId);
    if (categoryId) {
      // Auto-expand parent categories
      const path = getCategoryPath(categoryId, categories);
      setExpanded((prev) => {
        const next = new Set(prev);
        path.forEach((cat) => next.add(cat.id));
        return next;
      });
    }
  };

  return (
    <Box>
      {/* Search */}
      {showSearch && (
        <TextField
          fullWidth
          size="small"
          placeholder="Search catalog items..."
          value={searchQuery}
          onChange={(e) => setSearchQuery(e.target.value)}
          InputProps={{
            startAdornment: (
              <InputAdornment position="start">
                <SearchIcon />
              </InputAdornment>
            ),
            endAdornment: searchQuery && (
              <InputAdornment position="end">
                <IconButton size="small" onClick={() => setSearchQuery('')}>
                  <ClearIcon fontSize="small" />
                </IconButton>
              </InputAdornment>
            ),
          }}
          sx={{ mb: 2 }}
        />
      )}

      {/* Breadcrumbs */}
      {showBreadcrumbs && (
        <Breadcrumbs
          separator={<NavIcon fontSize="small" />}
          sx={{ mb: 2 }}
        >
          <Link
            component="button"
            variant="body2"
            underline="hover"
            color={selectedCategoryId ? 'inherit' : 'primary'}
            onClick={() => handleCategorySelect(null)}
            sx={{ display: 'flex', alignItems: 'center' }}
          >
            <HomeIcon sx={{ mr: 0.5, fontSize: 18 }} />
            Catalog
          </Link>
          {breadcrumbPath.map((cat, index) => (
            <Link
              key={cat.id}
              component="button"
              variant="body2"
              underline="hover"
              color={index === breadcrumbPath.length - 1 ? 'primary' : 'inherit'}
              fontWeight={index === breadcrumbPath.length - 1 ? 600 : 400}
              onClick={() => handleCategorySelect(cat.id)}
            >
              {cat.name}
            </Link>
          ))}
        </Breadcrumbs>
      )}

      {/* Content based on variant */}
      {variant === 'tree' && (
        <Paper variant="outlined">
          <List>
            {categoryTree.map((category) => (
              <CategoryTreeItem
                key={category.id}
                category={category}
                selectedId={selectedCategoryId}
                onSelect={handleCategorySelect}
                depth={0}
                maxDepth={maxDepth}
                expanded={expanded}
                onToggle={handleToggle}
              />
            ))}
          </List>
        </Paper>
      )}

      {variant === 'grid' && (
        <CategoryGridView
          categories={currentLevelCategories.length > 0 ? currentLevelCategories : categoryTree}
          selectedId={selectedCategoryId}
          onSelect={handleCategorySelect}
        />
      )}

      {variant === 'combined' && (
        <Grid container spacing={3}>
          {/* Tree sidebar */}
          <Grid item xs={12} md={3}>
            <Paper variant="outlined">
              <List dense>
                {categoryTree.map((category) => (
                  <CategoryTreeItem
                    key={category.id}
                    category={category}
                    selectedId={selectedCategoryId}
                    onSelect={handleCategorySelect}
                    depth={0}
                    maxDepth={maxDepth}
                    expanded={expanded}
                    onToggle={handleToggle}
                  />
                ))}
              </List>
            </Paper>
          </Grid>

          {/* Items grid */}
          <Grid item xs={12} md={9}>
            {/* Subcategories */}
            {currentLevelCategories.length > 0 && (
              <Box sx={{ mb: 3 }}>
                <Typography variant="subtitle2" color="text.secondary" sx={{ mb: 1 }}>
                  Subcategories
                </Typography>
                <CategoryGridView
                  categories={currentLevelCategories}
                  onSelect={handleCategorySelect}
                />
              </Box>
            )}

            {/* Items */}
            {filteredItems.length > 0 && (
              <Box>
                <Typography variant="subtitle2" color="text.secondary" sx={{ mb: 1 }}>
                  Items ({filteredItems.length})
                </Typography>
                <Grid container spacing={2}>
                  {filteredItems.map((item) => (
                    <Grid item xs={12} sm={6} lg={4} key={item.id}>
                      <CatalogItemCard
                        item={item}
                        onSelect={onItemSelect || (() => {})}
                      />
                    </Grid>
                  ))}
                </Grid>
              </Box>
            )}

            {filteredItems.length === 0 && currentLevelCategories.length === 0 && (
              <Paper
                variant="outlined"
                sx={{ p: 4, textAlign: 'center', backgroundColor: 'grey.50' }}
              >
                <ItemIcon sx={{ fontSize: 48, color: 'grey.400', mb: 1 }} />
                <Typography color="text.secondary">
                  {searchQuery
                    ? 'No items match your search'
                    : 'No items in this category'}
                </Typography>
              </Paper>
            )}
          </Grid>
        </Grid>
      )}
    </Box>
  );
};

export default CatalogCategoryBrowser;
