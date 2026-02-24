/**
 * SearchBar - Accessible search input component
 * Implements WCAG 2.1 AA accessibility requirements
 */

import React, { useState, useRef, useCallback, useEffect } from 'react';
import {
  Box,
  TextField,
  InputAdornment,
  IconButton,
  Paper,
  CircularProgress,
  Typography,
  Popper,
  List,
  ListItem,
  ListItemButton,
  ListItemText,
  ListItemIcon,
  ClickAwayListener,
  useTheme,
} from '@mui/material';
import {
  Search as SearchIcon,
  Clear as ClearIcon,
  History as HistoryIcon,
} from '@mui/icons-material';

export interface SearchSuggestion {
  id: string | number;
  label: string;
  description?: string;
  icon?: React.ReactNode;
  type?: string;
}

export interface SearchBarProps {
  value?: string;
  onChange?: (value: string) => void;
  onSearch?: (value: string) => void;
  onClear?: () => void;
  placeholder?: string;
  // Suggestions
  suggestions?: SearchSuggestion[];
  onSuggestionSelect?: (suggestion: SearchSuggestion) => void;
  showSuggestions?: boolean;
  // Recent searches
  recentSearches?: string[];
  showRecentSearches?: boolean;
  onRecentSearchClear?: () => void;
  // State
  loading?: boolean;
  disabled?: boolean;
  autoFocus?: boolean;
  // Debounce
  debounceMs?: number;
  // Accessibility
  ariaLabel?: string;
  ariaDescribedBy?: string;
  // Styling
  fullWidth?: boolean;
  size?: 'small' | 'medium';
  variant?: 'outlined' | 'filled' | 'standard';
}

export const SearchBar: React.FC<SearchBarProps> = ({
  value: controlledValue,
  onChange,
  onSearch,
  onClear,
  placeholder = 'Search...',
  suggestions = [],
  onSuggestionSelect,
  showSuggestions = true,
  recentSearches = [],
  showRecentSearches = true,
  onRecentSearchClear,
  loading = false,
  disabled = false,
  autoFocus = false,
  debounceMs = 300,
  ariaLabel = 'Search',
  ariaDescribedBy,
  fullWidth = true,
  size = 'medium',
  variant = 'outlined',
}) => {
  const theme = useTheme();
  const inputRef = useRef<HTMLInputElement>(null);
  const [internalValue, setInternalValue] = useState(controlledValue ?? '');
  const [isOpen, setIsOpen] = useState(false);
  const [selectedIndex, setSelectedIndex] = useState(-1);
  const anchorRef = useRef<HTMLDivElement>(null);
  const debounceRef = useRef<NodeJS.Timeout>();

  // Sync with controlled value
  useEffect(() => {
    if (controlledValue !== undefined) {
      setInternalValue(controlledValue);
    }
  }, [controlledValue]);

  // Debounced search
  useEffect(() => {
    if (debounceRef.current) {
      clearTimeout(debounceRef.current);
    }

    if (internalValue && onSearch) {
      debounceRef.current = setTimeout(() => {
        onSearch(internalValue);
      }, debounceMs);
    }

    return () => {
      if (debounceRef.current) {
        clearTimeout(debounceRef.current);
      }
    };
  }, [internalValue, onSearch, debounceMs]);

  // Combined suggestions
  const allSuggestions: SearchSuggestion[] = React.useMemo(() => {
    if (!showSuggestions && !showRecentSearches) return [];
    
    const items: SearchSuggestion[] = [];
    
    // Add recent searches
    if (showRecentSearches && recentSearches.length > 0 && !internalValue) {
      recentSearches.slice(0, 5).forEach((search, index) => {
        items.push({
          id: `recent-${index}`,
          label: search,
          type: 'recent',
          icon: <HistoryIcon fontSize="small" color="action" />,
        });
      });
    }
    
    // Add suggestions
    if (showSuggestions && suggestions.length > 0) {
      items.push(...suggestions);
    }
    
    return items;
  }, [suggestions, recentSearches, showSuggestions, showRecentSearches, internalValue]);

  // Handle input change
  const handleChange = useCallback((e: React.ChangeEvent<HTMLInputElement>) => {
    const newValue = e.target.value;
    setInternalValue(newValue);
    onChange?.(newValue);
    setIsOpen(newValue.length > 0 || (showRecentSearches && recentSearches.length > 0));
    setSelectedIndex(-1);
  }, [onChange, showRecentSearches, recentSearches.length]);

  // Handle clear
  const handleClear = useCallback(() => {
    setInternalValue('');
    onChange?.('');
    onClear?.();
    setIsOpen(false);
    inputRef.current?.focus();
  }, [onChange, onClear]);

  // Handle suggestion selection
  const handleSuggestionSelect = useCallback((suggestion: SearchSuggestion) => {
    setInternalValue(suggestion.label);
    onChange?.(suggestion.label);
    onSuggestionSelect?.(suggestion);
    setIsOpen(false);
    inputRef.current?.focus();
  }, [onChange, onSuggestionSelect]);

  // Handle keyboard navigation
  const handleKeyDown = useCallback((e: React.KeyboardEvent<HTMLInputElement>) => {
    if (!isOpen || allSuggestions.length === 0) {
      if (e.key === 'Enter') {
        onSearch?.(internalValue);
      }
      return;
    }

    switch (e.key) {
      case 'ArrowDown':
        e.preventDefault();
        setSelectedIndex((prev) => 
          prev < allSuggestions.length - 1 ? prev + 1 : 0
        );
        break;
      case 'ArrowUp':
        e.preventDefault();
        setSelectedIndex((prev) => 
          prev > 0 ? prev - 1 : allSuggestions.length - 1
        );
        break;
      case 'Enter':
        e.preventDefault();
        if (selectedIndex >= 0 && selectedIndex < allSuggestions.length) {
          handleSuggestionSelect(allSuggestions[selectedIndex]);
        } else {
          onSearch?.(internalValue);
          setIsOpen(false);
        }
        break;
      case 'Escape':
        e.preventDefault();
        setIsOpen(false);
        setSelectedIndex(-1);
        break;
    }
  }, [isOpen, allSuggestions, selectedIndex, handleSuggestionSelect, internalValue, onSearch]);

  // Handle focus
  const handleFocus = useCallback(() => {
    if (allSuggestions.length > 0 || (showRecentSearches && recentSearches.length > 0)) {
      setIsOpen(true);
    }
  }, [allSuggestions.length, showRecentSearches, recentSearches.length]);

  // Handle click away
  const handleClickAway = useCallback(() => {
    setIsOpen(false);
    setSelectedIndex(-1);
  }, []);

  // Generate unique IDs for accessibility
  const searchId = React.useId();
  const listboxId = `${searchId}-listbox`;
  const descriptionId = `${searchId}-description`;

  return (
    <ClickAwayListener onClickAway={handleClickAway}>
      <Box
        ref={anchorRef}
        role="search"
        aria-label={ariaLabel}
        sx={{ position: 'relative', width: fullWidth ? '100%' : 'auto' }}
      >
        <TextField
          inputRef={inputRef}
          value={internalValue}
          onChange={handleChange}
          onKeyDown={handleKeyDown}
          onFocus={handleFocus}
          placeholder={placeholder}
          disabled={disabled}
          autoFocus={autoFocus}
          fullWidth={fullWidth}
          size={size}
          variant={variant}
          InputProps={{
            startAdornment: (
              <InputAdornment position="start">
                <SearchIcon color="action" aria-hidden="true" />
              </InputAdornment>
            ),
            endAdornment: (
              <InputAdornment position="end">
                {loading ? (
                  <CircularProgress size={20} aria-label="Searching" />
                ) : internalValue ? (
                  <IconButton
                    size="small"
                    onClick={handleClear}
                    aria-label="Clear search"
                    edge="end"
                  >
                    <ClearIcon fontSize="small" />
                  </IconButton>
                ) : null}
              </InputAdornment>
            ),
            'aria-label': ariaLabel,
            'aria-describedby': ariaDescribedBy || descriptionId,
            'aria-autocomplete': 'list',
            'aria-controls': isOpen ? listboxId : undefined,
            'aria-expanded': isOpen,
            'aria-activedescendant': selectedIndex >= 0 ? `${listboxId}-option-${selectedIndex}` : undefined,
            role: 'combobox',
          }}
          sx={{
            '& .MuiOutlinedInput-root': {
              borderRadius: 2,
            },
          }}
        />

        {/* Hidden description for screen readers */}
        <Typography id={descriptionId} sx={{ position: 'absolute', width: 1, height: 1, overflow: 'hidden' }}>
          {allSuggestions.length > 0
            ? `${allSuggestions.length} suggestions available. Use arrow keys to navigate.`
            : 'Type to search'}
        </Typography>

        {/* Suggestions dropdown */}
        <Popper
          open={isOpen && allSuggestions.length > 0}
          anchorEl={anchorRef.current}
          placement="bottom-start"
          style={{ width: anchorRef.current?.clientWidth, zIndex: theme.zIndex.modal }}
        >
          <Paper
            elevation={8}
            sx={{ mt: 1, maxHeight: 400, overflow: 'auto' }}
          >
            <List
              id={listboxId}
              role="listbox"
              aria-label="Search suggestions"
              dense
            >
              {/* Clear recent searches button */}
              {showRecentSearches && 
               recentSearches.length > 0 && 
               !internalValue && 
               onRecentSearchClear && (
                <ListItem
                  sx={{
                    justifyContent: 'space-between',
                    borderBottom: 1,
                    borderColor: 'divider',
                  }}
                >
                  <Typography variant="caption" color="text.secondary">
                    Recent Searches
                  </Typography>
                  <IconButton
                    size="small"
                    onClick={onRecentSearchClear}
                    aria-label="Clear recent searches"
                  >
                    <ClearIcon fontSize="small" />
                  </IconButton>
                </ListItem>
              )}

              {allSuggestions.map((suggestion, index) => (
                <ListItemButton
                  key={suggestion.id}
                  id={`${listboxId}-option-${index}`}
                  role="option"
                  aria-selected={index === selectedIndex}
                  selected={index === selectedIndex}
                  onClick={() => handleSuggestionSelect(suggestion)}
                  sx={{
                    '&.Mui-selected': {
                      backgroundColor: theme.palette.action.selected,
                    },
                    '&:hover': {
                      backgroundColor: theme.palette.action.hover,
                    },
                  }}
                >
                  {suggestion.icon && (
                    <ListItemIcon sx={{ minWidth: 36 }}>
                      {suggestion.icon}
                    </ListItemIcon>
                  )}
                  <ListItemText
                    primary={suggestion.label}
                    secondary={suggestion.description}
                    primaryTypographyProps={{
                      variant: 'body2',
                    }}
                    secondaryTypographyProps={{
                      variant: 'caption',
                      noWrap: true,
                    }}
                  />
                  {suggestion.type && (
                    <Typography variant="caption" color="text.secondary" sx={{ ml: 1 }}>
                      {suggestion.type}
                    </Typography>
                  )}
                </ListItemButton>
              ))}
            </List>
          </Paper>
        </Popper>
      </Box>
    </ClickAwayListener>
  );
};

export default SearchBar;
