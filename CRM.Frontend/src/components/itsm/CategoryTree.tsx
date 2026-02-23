// Category Tree - Hierarchical tree view for KB article categories
// Part of Knowledge Base Enhancement - Phase 3

import React, { useCallback } from 'react';
import {
  Box,
  Typography,
  List,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  Collapse,
  Chip,
  Skeleton,
  Paper,
  Stack,
} from '@mui/material';
import {
  Folder as FolderIcon,
  FolderOpen as FolderOpenIcon,
  ExpandMore as ExpandIcon,
  ChevronRight as CollapseIcon,
  Article as ArticleIcon,
} from '@mui/icons-material';

export interface CategoryNode {
  id: number;
  name: string;
  articleCount: number;
  children?: CategoryNode[];
}

export interface CategoryTreeProps {
  categories: CategoryNode[];
  selectedCategoryId?: number;
  onCategorySelect: (categoryId: number) => void;
  loading?: boolean;
}

interface CategoryItemProps {
  node: CategoryNode;
  depth: number;
  selectedCategoryId?: number;
  expandedIds: Set<number>;
  onToggle: (id: number) => void;
  onSelect: (id: number) => void;
}

const CategoryItem: React.FC<CategoryItemProps> = ({
  node,
  depth,
  selectedCategoryId,
  expandedIds,
  onToggle,
  onSelect,
}) => {
  const hasChildren = node.children && node.children.length > 0;
  const isExpanded = expandedIds.has(node.id);
  const isSelected = selectedCategoryId === node.id;

  const handleClick = () => {
    onSelect(node.id);
    if (hasChildren) {
      onToggle(node.id);
    }
  };

  return (
    <>
      <ListItemButton
        selected={isSelected}
        onClick={handleClick}
        sx={{
          pl: 2 + depth * 2,
          borderRadius: 1,
          mb: 0.5,
          '&.Mui-selected': {
            backgroundColor: 'primary.light',
            color: 'primary.contrastText',
            '&:hover': { backgroundColor: 'primary.main' },
          },
        }}
      >
        <ListItemIcon sx={{ minWidth: 32 }}>
          {hasChildren ? (
            isExpanded ? <FolderOpenIcon color={isSelected ? 'inherit' : 'primary'} fontSize="small" /> : <FolderIcon color={isSelected ? 'inherit' : 'action'} fontSize="small" />
          ) : (
            <ArticleIcon color={isSelected ? 'inherit' : 'action'} fontSize="small" />
          )}
        </ListItemIcon>
        {hasChildren && (
          <Box sx={{ mr: 0.5, display: 'flex', alignItems: 'center' }}>
            {isExpanded ? <ExpandIcon fontSize="small" /> : <CollapseIcon fontSize="small" />}
          </Box>
        )}
        <ListItemText
          primary={node.name}
          primaryTypographyProps={{ variant: 'body2', noWrap: true }}
        />
        <Chip
          label={node.articleCount}
          size="small"
          variant={isSelected ? 'filled' : 'outlined'}
          color={isSelected ? 'default' : 'primary'}
          sx={{ height: 20, fontSize: '0.7rem', minWidth: 28 }}
        />
      </ListItemButton>
      {hasChildren && (
        <Collapse in={isExpanded} timeout="auto" unmountOnExit>
          <List disablePadding>
            {node.children!.map((child) => (
              <CategoryItem
                key={child.id}
                node={child}
                depth={depth + 1}
                selectedCategoryId={selectedCategoryId}
                expandedIds={expandedIds}
                onToggle={onToggle}
                onSelect={onSelect}
              />
            ))}
          </List>
        </Collapse>
      )}
    </>
  );
};

const CategoryTree: React.FC<CategoryTreeProps> = ({
  categories,
  selectedCategoryId,
  onCategorySelect,
  loading = false,
}) => {
  const [expandedIds, setExpandedIds] = React.useState<Set<number>>(new Set());

  const handleToggle = useCallback((id: number) => {
    setExpandedIds((prev) => {
      const next = new Set(prev);
      if (next.has(id)) {
        next.delete(id);
      } else {
        next.add(id);
      }
      return next;
    });
  }, []);

  if (loading) {
    return (
      <Paper variant="outlined" sx={{ p: 2 }}>
        <Stack spacing={1}>
          {[1, 2, 3, 4, 5].map((i) => (
            <Skeleton key={i} variant="rectangular" height={36} sx={{ borderRadius: 1 }} />
          ))}
        </Stack>
      </Paper>
    );
  }

  if (categories.length === 0) {
    return (
      <Paper variant="outlined" sx={{ p: 3, textAlign: 'center' }}>
        <FolderIcon sx={{ fontSize: 40, color: 'text.disabled', mb: 1 }} />
        <Typography variant="body2" color="text.secondary">
          No categories found
        </Typography>
      </Paper>
    );
  }

  return (
    <Paper variant="outlined" sx={{ p: 1 }}>
      <Typography variant="subtitle2" sx={{ px: 1, py: 0.5, color: 'text.secondary' }}>
        Categories
      </Typography>
      <List dense disablePadding>
        {categories.map((category) => (
          <CategoryItem
            key={category.id}
            node={category}
            depth={0}
            selectedCategoryId={selectedCategoryId}
            expandedIds={expandedIds}
            onToggle={handleToggle}
            onSelect={onCategorySelect}
          />
        ))}
      </List>
    </Paper>
  );
};

export default CategoryTree;
