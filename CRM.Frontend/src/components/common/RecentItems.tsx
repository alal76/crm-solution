/**
 * RecentItems - Component showing recently viewed records
 */

import React, { useState, useMemo } from 'react';
import {
  Box,
  Paper,
  List,
  ListItem,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  ListSubheader,
  Typography,
  IconButton,
  Tooltip,
  Chip,
  Menu,
  MenuItem,
  Divider,
  Stack,
  alpha,
  useTheme,
} from '@mui/material';
import {
  History as HistoryIcon,
  Delete as DeleteIcon,
  Clear as ClearIcon,
  BusinessCenter as AccountIcon,
  Person as ContactIcon,
  PersonAdd as LeadIcon,
  TrendingUp as OpportunityIcon,
  Inventory as ProductIcon,
  Campaign as CampaignIcon,
  Description as QuoteIcon,
  ShoppingCart as OrderIcon,
  Receipt as InvoiceIcon,
  Support as TicketIcon,
  BugReport as IncidentIcon,
  MenuBook as KnowledgeIcon,
  AccountCircle as UserIcon,
  MoreVert as MoreIcon,
  Star as StarIcon,
  StarBorder as StarBorderIcon,
  OpenInNew as OpenIcon,
} from '@mui/icons-material';
import { useNavigate } from 'react-router-dom';
import { useRecentItems, RecentItem, RecentItemType } from '../../contexts/RecentItemsContext';
import { formatDistanceToNow } from 'date-fns';

// Icon mapping
const typeIcons: Record<RecentItemType, React.ReactElement> = {
  account: <AccountIcon />,
  contact: <ContactIcon />,
  lead: <LeadIcon />,
  opportunity: <OpportunityIcon />,
  product: <ProductIcon />,
  campaign: <CampaignIcon />,
  quote: <QuoteIcon />,
  order: <OrderIcon />,
  invoice: <InvoiceIcon />,
  ticket: <TicketIcon />,
  incident: <IncidentIcon />,
  knowledge: <KnowledgeIcon />,
  user: <UserIcon />,
  other: <HistoryIcon />,
};

// Type labels
const typeLabels: Record<RecentItemType, string> = {
  account: 'Account',
  contact: 'Contact',
  lead: 'Lead',
  opportunity: 'Opportunity',
  product: 'Product',
  campaign: 'Campaign',
  quote: 'Quote',
  order: 'Order',
  invoice: 'Invoice',
  ticket: 'Ticket',
  incident: 'Incident',
  knowledge: 'Article',
  user: 'User',
  other: 'Item',
};

export interface RecentItemsProps {
  // Display
  maxItems?: number;
  showType?: boolean | RecentItemType[];
  groupByType?: boolean;
  showTimestamp?: boolean;
  showClearAll?: boolean;
  // Styling
  variant?: 'list' | 'compact' | 'dropdown';
  elevation?: number;
  // Callbacks
  onItemClick?: (item: RecentItem) => void;
}

export const RecentItems: React.FC<RecentItemsProps> = ({
  maxItems = 10,
  showType = true,
  groupByType = false,
  showTimestamp = true,
  showClearAll = true,
  variant = 'list',
  elevation = 1,
  onItemClick,
}) => {
  const theme = useTheme();
  const navigate = useNavigate();
  const { recentItems, removeRecentItem, clearRecentItems } = useRecentItems();
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
  const [contextItem, setContextItem] = useState<RecentItem | null>(null);

  // Filter items based on showType
  const filteredItems = useMemo(() => {
    let items = recentItems;
    
    if (Array.isArray(showType)) {
      items = items.filter((item) => showType.includes(item.type));
    }

    return items.slice(0, maxItems);
  }, [recentItems, showType, maxItems]);

  // Group items by type if needed
  const groupedItems = useMemo(() => {
    if (!groupByType) return null;

    const groups: Record<RecentItemType, RecentItem[]> = {} as Record<RecentItemType, RecentItem[]>;
    
    filteredItems.forEach((item) => {
      if (!groups[item.type]) {
        groups[item.type] = [];
      }
      groups[item.type].push(item);
    });

    return groups;
  }, [filteredItems, groupByType]);

  // Handle item click
  const handleItemClick = (item: RecentItem) => {
    if (onItemClick) {
      onItemClick(item);
    } else {
      navigate(item.path);
    }
  };

  // Handle context menu
  const handleContextMenu = (event: React.MouseEvent, item: RecentItem) => {
    event.preventDefault();
    setAnchorEl(event.currentTarget as HTMLElement);
    setContextItem(item);
  };

  // Close context menu
  const handleCloseMenu = () => {
    setAnchorEl(null);
    setContextItem(null);
  };

  // Render single item
  const renderItem = (item: RecentItem, index: number) => (
    <ListItem
      key={`${item.type}-${item.id}`}
      disablePadding
      secondaryAction={
        <Stack direction="row" spacing={0.5}>
          <Tooltip title="Remove from recent">
            <IconButton
              size="small"
              edge="end"
              onClick={(e) => {
                e.stopPropagation();
                removeRecentItem(item.id, item.type);
              }}
              aria-label={`Remove ${item.title} from recent items`}
            >
              <DeleteIcon fontSize="small" />
            </IconButton>
          </Tooltip>
        </Stack>
      }
      onContextMenu={(e) => handleContextMenu(e, item)}
    >
      <ListItemButton
        onClick={() => handleItemClick(item)}
        sx={{
          '&:hover': {
            backgroundColor: alpha(theme.palette.primary.main, 0.08),
          },
        }}
      >
        <ListItemIcon sx={{ minWidth: 40 }}>
          {typeIcons[item.type]}
        </ListItemIcon>
        <ListItemText
          primary={
            <Stack direction="row" spacing={1} alignItems="center">
              <Typography variant="body2" noWrap sx={{ flex: 1 }}>
                {item.title}
              </Typography>
              {showType && !groupByType && (
                <Chip
                  label={typeLabels[item.type]}
                  size="small"
                  variant="outlined"
                  sx={{ height: 20, fontSize: '0.7rem' }}
                />
              )}
            </Stack>
          }
          secondary={
            <Stack direction="row" justifyContent="space-between" alignItems="center">
              <Typography variant="caption" color="text.secondary" noWrap>
                {item.subtitle}
              </Typography>
              {showTimestamp && (
                <Typography variant="caption" color="text.secondary" sx={{ ml: 1 }}>
                  {formatDistanceToNow(item.timestamp, { addSuffix: true })}
                </Typography>
              )}
            </Stack>
          }
          primaryTypographyProps={{ noWrap: true }}
          secondaryTypographyProps={{ component: 'div' }}
        />
      </ListItemButton>
    </ListItem>
  );

  // Render grouped list
  const renderGroupedList = () => {
    if (!groupedItems) return null;

    return Object.entries(groupedItems).map(([type, items]) => (
      <React.Fragment key={type}>
        <ListSubheader sx={{ bgcolor: 'transparent' }}>
          <Stack direction="row" spacing={1} alignItems="center">
            {typeIcons[type as RecentItemType]}
            <Typography variant="subtitle2">
              {typeLabels[type as RecentItemType]}s
            </Typography>
            <Chip label={items.length} size="small" sx={{ height: 20 }} />
          </Stack>
        </ListSubheader>
        {items.map((item, index) => renderItem(item, index))}
      </React.Fragment>
    ));
  };

  // Empty state
  if (filteredItems.length === 0) {
    return (
      <Paper elevation={elevation} sx={{ p: 3, textAlign: 'center' }}>
        <HistoryIcon sx={{ fontSize: 48, color: 'text.secondary', mb: 1 }} />
        <Typography variant="body1" color="text.secondary">
          No recent items
        </Typography>
        <Typography variant="caption" color="text.secondary">
          Items you view will appear here
        </Typography>
      </Paper>
    );
  }

  return (
    <Paper elevation={elevation}>
      {/* Header */}
      <Box
        sx={{
          p: 1.5,
          borderBottom: 1,
          borderColor: 'divider',
          display: 'flex',
          justifyContent: 'space-between',
          alignItems: 'center',
        }}
      >
        <Stack direction="row" spacing={1} alignItems="center">
          <HistoryIcon color="action" />
          <Typography variant="subtitle1" fontWeight={600}>
            Recent Items
          </Typography>
          <Chip label={filteredItems.length} size="small" />
        </Stack>
        
        {showClearAll && (
          <Tooltip title="Clear all recent items">
            <IconButton
              size="small"
              onClick={clearRecentItems}
              aria-label="Clear all recent items"
            >
              <ClearIcon fontSize="small" />
            </IconButton>
          </Tooltip>
        )}
      </Box>

      {/* List */}
      <List dense disablePadding sx={{ maxHeight: 400, overflow: 'auto' }}>
        {groupByType ? renderGroupedList() : filteredItems.map(renderItem)}
      </List>

      {/* Context menu */}
      <Menu
        anchorEl={anchorEl}
        open={Boolean(anchorEl)}
        onClose={handleCloseMenu}
      >
        <MenuItem
          onClick={() => {
            if (contextItem) {
              handleItemClick(contextItem);
            }
            handleCloseMenu();
          }}
        >
          <ListItemIcon>
            <OpenIcon fontSize="small" />
          </ListItemIcon>
          <ListItemText>Open</ListItemText>
        </MenuItem>
        <MenuItem
          onClick={() => {
            if (contextItem) {
              window.open(contextItem.path, '_blank');
            }
            handleCloseMenu();
          }}
        >
          <ListItemIcon>
            <OpenIcon fontSize="small" />
          </ListItemIcon>
          <ListItemText>Open in new tab</ListItemText>
        </MenuItem>
        <Divider />
        <MenuItem
          onClick={() => {
            if (contextItem) {
              removeRecentItem(contextItem.id, contextItem.type);
            }
            handleCloseMenu();
          }}
        >
          <ListItemIcon>
            <DeleteIcon fontSize="small" />
          </ListItemIcon>
          <ListItemText>Remove from recent</ListItemText>
        </MenuItem>
      </Menu>
    </Paper>
  );
};

export default RecentItems;
