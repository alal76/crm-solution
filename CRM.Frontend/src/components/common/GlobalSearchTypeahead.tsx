// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
import React, { useState, useEffect, useCallback, useRef } from 'react';
import {
  Autocomplete,
  TextField,
  CircularProgress,
  Typography,
  Box,
  Chip,
  InputAdornment,
  Paper,
} from '@mui/material';
import SearchIcon from '@mui/icons-material/Search';
import BusinessIcon from '@mui/icons-material/Business';
import PersonIcon from '@mui/icons-material/Person';
import TrendingUpIcon from '@mui/icons-material/TrendingUp';
import ConfirmationNumberIcon from '@mui/icons-material/ConfirmationNumber';
import PersonSearchIcon from '@mui/icons-material/PersonSearch';
import searchService, { SearchResult } from '../../services/searchService';

// ============================================================================
// Global Search Typeahead Component (TODO-UX-06)
// ============================================================================

export interface GlobalSearchTypeaheadProps {
  /** Callback when a search result is selected. */
  onSelect: (result: SearchResult) => void;
  /** Placeholder text for the search input. */
  placeholder?: string;
  /** Width of the autocomplete field. */
  width?: number | string;
  /** Whether the component should take focus on mount. */
  autoFocus?: boolean;
}

/** Color configuration for each entity type. */
const typeConfig: Record<
  SearchResult['type'],
  { color: string; label: string; icon: React.ReactElement }
> = {
  account: { color: '#1976d2', label: 'Account', icon: <BusinessIcon fontSize="small" /> },
  contact: { color: '#388e3c', label: 'Contact', icon: <PersonIcon fontSize="small" /> },
  opportunity: { color: '#f57c00', label: 'Opportunity', icon: <TrendingUpIcon fontSize="small" /> },
  ticket: { color: '#d32f2f', label: 'Ticket', icon: <ConfirmationNumberIcon fontSize="small" /> },
  lead: { color: '#7b1fa2', label: 'Lead', icon: <PersonSearchIcon fontSize="small" /> },
};

/**
 * Global search typeahead component that searches across all CRM entity types.
 *
 * Features:
 * - Debounced input (300ms)
 * - Grouped results by entity type
 * - MUI Autocomplete with custom renderOption showing icon + type chip + title
 * - Keyboard navigation support
 * - "No results" state
 * - Loading spinner
 */
const GlobalSearchTypeahead: React.FC<GlobalSearchTypeaheadProps> = ({
  onSelect,
  placeholder = 'Search accounts, contacts, opportunities…',
  width = 400,
  autoFocus = false,
}) => {
  const [inputValue, setInputValue] = useState('');
  const [options, setOptions] = useState<SearchResult[]>([]);
  const [loading, setLoading] = useState(false);
  const [open, setOpen] = useState(false);
  const debounceRef = useRef<ReturnType<typeof setTimeout> | null>(null);

  // Debounced search handler
  const handleSearch = useCallback((query: string) => {
    if (debounceRef.current) {
      clearTimeout(debounceRef.current);
    }

    if (!query || query.trim().length < 2) {
      setOptions([]);
      setLoading(false);
      return;
    }

    setLoading(true);

    debounceRef.current = setTimeout(async () => {
      try {
        const results = await searchService.globalSearch(query, 10);
        setOptions(results);
      } catch (error) {
        console.error('Global search failed:', error);
        setOptions([]);
      } finally {
        setLoading(false);
      }
    }, 300);
  }, []);

  // Trigger search on input change
  useEffect(() => {
    handleSearch(inputValue);
    return () => {
      if (debounceRef.current) {
        clearTimeout(debounceRef.current);
      }
    };
  }, [inputValue, handleSearch]);

  // Group results by type for better display
  const groupedOptions = [...options].sort((a, b) => {
    const order = ['account', 'contact', 'opportunity', 'lead', 'ticket'];
    return order.indexOf(a.type) - order.indexOf(b.type);
  });

  return (
    <Autocomplete<SearchResult, false, false, true>
      freeSolo
      open={open}
      onOpen={() => setOpen(true)}
      onClose={() => setOpen(false)}
      options={groupedOptions}
      groupBy={(option) => (typeof option === 'string' ? '' : typeConfig[option.type]?.label || option.type)}
      getOptionLabel={(option) => (typeof option === 'string' ? option : option.title)}
      isOptionEqualToValue={(option, value) =>
        typeof option !== 'string' && typeof value !== 'string' && option.id === value.id && option.type === value.type
      }
      inputValue={inputValue}
      onInputChange={(_event, newInputValue) => {
        setInputValue(newInputValue);
      }}
      onChange={(_event, newValue) => {
        if (newValue && typeof newValue !== 'string') {
          onSelect(newValue);
          setInputValue('');
          setOpen(false);
        }
      }}
      loading={loading}
      noOptionsText={
        inputValue.length >= 2
          ? 'No results found'
          : 'Type at least 2 characters to search'
      }
      PaperComponent={(props) => (
        <Paper {...props} elevation={8} sx={{ borderRadius: 2 }} />
      )}
      renderOption={(props, option) => {
        if (typeof option === 'string') return null;
        const config = typeConfig[option.type] || typeConfig.account;

        return (
          <Box
            component="li"
            {...props}
            key={`${option.type}-${option.id}`}
            sx={{
              display: 'flex',
              alignItems: 'center',
              gap: 1.5,
              py: 1,
              px: 2,
              '&:hover': { backgroundColor: 'action.hover' },
            }}
          >
            <Box sx={{ color: config.color, display: 'flex', alignItems: 'center' }}>
              {config.icon}
            </Box>
            <Box sx={{ flexGrow: 1, minWidth: 0 }}>
              <Typography variant="body2" noWrap fontWeight={500}>
                {option.title}
              </Typography>
              {option.subtitle && (
                <Typography variant="caption" color="text.secondary" noWrap>
                  {option.subtitle}
                </Typography>
              )}
            </Box>
            <Chip
              label={config.label}
              size="small"
              sx={{
                backgroundColor: `${config.color}15`,
                color: config.color,
                fontWeight: 500,
                fontSize: '0.7rem',
                height: 22,
              }}
            />
          </Box>
        );
      }}
      renderInput={(params) => (
        <TextField
          {...params}
          placeholder={placeholder}
          size="small"
          autoFocus={autoFocus}
          InputProps={{
            ...params.InputProps,
            startAdornment: (
              <InputAdornment position="start">
                <SearchIcon color="action" />
              </InputAdornment>
            ),
            endAdornment: (
              <>
                {loading ? <CircularProgress color="inherit" size={18} /> : null}
                {params.InputProps.endAdornment}
              </>
            ),
          }}
          sx={{ width }}
        />
      )}
      sx={{ width }}
    />
  );
};

export default GlobalSearchTypeahead;
