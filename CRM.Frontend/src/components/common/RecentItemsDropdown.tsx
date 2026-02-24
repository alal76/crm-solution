/**
 * RecentItemsDropdown - Quick access dropdown of recently viewed records
 * TODO-UX-15: Stored in localStorage via RecentItemsContext
 */

import React, { useState, useMemo } from 'react';
import {
  Box,
  IconButton,
  Menu,
  MenuItem,
  ListItemIcon,
  ListItemText,
  Typography,
  Divider,
  Tooltip,
  Chip,
  Stack,
  Button,
  Badge,
  useTheme,
} from '@mui/material';
import {
  History as HistoryIcon,
  OpenInNew as OpenIcon,
  Delete as ClearIcon,
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
} from '@mui/icons-material';
import { useNavigate } from 'react-router-dom';
import { useRecentItems, RecentItem, RecentItemType } from '../../contexts/RecentItemsContext';
import { formatDistanceToNow } from 'date-fns';

// --------------------------------------------------------------------------
// Types
// --------------------------------------------------------------------------

export interface RecentItemsDropdownProps {
  /** Maximum items to show in the dropdown */
  maxVisible?: number;
  /** Show badge with count */
  showBadge?: boolean;
  /** Button size */
  size?: 'small' | 'medium';
  /** Filter by entity type */
  filterType?: RecentItemType;
}

// --------------------------------------------------------------------------
// Helpers
// --------------------------------------------------------------------------

const typeIcons: Record<RecentItemType, React.ReactElement> = {
  account: <AccountIcon fontSize="small" />,
  contact: <ContactIcon fontSize="small" />,
  lead: <LeadIcon fontSize="small" />,
  opportunity: <OpportunityIcon fontSize="small" />,
  product: <ProductIcon fontSize="small" />,
  campaign: <CampaignIcon fontSize="small" />,
  quote: <QuoteIcon fontSize="small" />,
  order: <OrderIcon fontSize="small" />,
  invoice: <InvoiceIcon fontSize="small" />,
  ticket: <TicketIcon fontSize="small" />,
  incident: <IncidentIcon fontSize="small" />,
  knowledge: <KnowledgeIcon fontSize="small" />,
  user: <UserIcon fontSize="small" />,
  other: <HistoryIcon fontSize="small" />,
};

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

// --------------------------------------------------------------------------
// Component
// --------------------------------------------------------------------------

export const RecentItemsDropdown: React.FC<RecentItemsDropdownProps> = ({
  maxVisible = 10,
  showBadge = true,
  size = 'medium',
  filterType,
}) => {
  const theme = useTheme();
  const navigate = useNavigate();
  const { recentItems, clearRecentItems, removeRecentItem, getRecentItemsByType } = useRecentItems();
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
  const open = Boolean(anchorEl);

  const items = useMemo(() => {
    const source = filterType ? getRecentItemsByType(filterType) : recentItems;
    return source.slice(0, maxVisible);
  }, [recentItems, filterType, maxVisible, getRecentItemsByType]);

  const handleOpen = (event: React.MouseEvent<HTMLElement>) => {
    setAnchorEl(event.currentTarget);
  };

  const handleClose = () => {
    setAnchorEl(null);
  };

  const handleNavigate = (item: RecentItem) => {
    navigate(item.path);
    handleClose();
  };

  return (
    <>
      <Tooltip title="Recent items">
        <IconButton
          onClick={handleOpen}
          size={size}
          aria-label="Open recent items"
          aria-haspopup="true"
          aria-expanded={open}
        >
          <Badge
            badgeContent={showBadge ? items.length : 0}
            color="primary"
            max={99}
            invisible={!showBadge || items.length === 0}
          >
            <HistoryIcon />
          </Badge>
        </IconButton>
      </Tooltip>

      <Menu
        anchorEl={anchorEl}
        open={open}
        onClose={handleClose}
        PaperProps={{
          sx: { width: 360, maxHeight: 480 },
        }}
        transformOrigin={{ horizontal: 'right', vertical: 'top' }}
        anchorOrigin={{ horizontal: 'right', vertical: 'bottom' }}
      >
        {/* Header */}
        <Box sx={{ px: 2, py: 1 }}>
          <Stack direction="row" justifyContent="space-between" alignItems="center">
            <Typography variant="subtitle2" fontWeight={600}>
              Recent Items
            </Typography>
            {items.length > 0 && (
              <Button
                size="small"
                color="error"
                startIcon={<ClearIcon fontSize="small" />}
                onClick={() => {
                  clearRecentItems();
                  handleClose();
                }}
              >
                Clear
              </Button>
            )}
          </Stack>
        </Box>
        <Divider />

        {items.length === 0 ? (
          <Box sx={{ px: 2, py: 3, textAlign: 'center' }}>
            <HistoryIcon sx={{ fontSize: 40, color: 'text.disabled', mb: 1 }} />
            <Typography variant="body2" color="text.secondary">
              No recent items
            </Typography>
          </Box>
        ) : (
          items.map((item) => (
            <MenuItem
              key={`${item.type}-${item.id}`}
              onClick={() => handleNavigate(item)}
              sx={{ py: 1 }}
            >
              <ListItemIcon>{typeIcons[item.type]}</ListItemIcon>
              <ListItemText
                primary={
                  <Stack direction="row" spacing={1} alignItems="center">
                    <Typography variant="body2" noWrap sx={{ maxWidth: 200 }}>
                      {item.title}
                    </Typography>
                    <Chip
                      label={typeLabels[item.type]}
                      size="small"
                      variant="outlined"
                      sx={{ height: 20, fontSize: '0.7rem' }}
                    />
                  </Stack>
                }
                secondary={
                  <Typography variant="caption" color="text.secondary">
                    {item.subtitle
                      ? `${item.subtitle} · `
                      : ''}
                    {formatDistanceToNow(new Date(item.timestamp), { addSuffix: true })}
                  </Typography>
                }
              />
            </MenuItem>
          ))
        )}
      </Menu>
    </>
  );
};

export default RecentItemsDropdown;
