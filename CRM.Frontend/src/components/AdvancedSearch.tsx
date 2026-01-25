import React, { useState, useEffect } from 'react';
import {
  Box,
  TextField,
  InputAdornment,
  IconButton,
  Collapse,
  Card,
  CardContent,
  Grid,
  Button,
  Chip,
  Stack,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Typography,
  Tooltip,
  Divider,
  Paper,
} from '@mui/material';
import SearchIcon from '@mui/icons-material/Search';
import FilterListIcon from '@mui/icons-material/FilterList';
import ClearIcon from '@mui/icons-material/Clear';
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';
import ExpandLessIcon from '@mui/icons-material/ExpandLess';

export interface SearchField {
  name: string;
  label: string;
  type: 'text' | 'select' | 'date' | 'dateRange' | 'number' | 'numberRange' | 'boolean';
  options?: { value: string | number; label: string }[];
  placeholder?: string;
}

export interface SearchFilter {
  field: string;
  value: any;
  operator?: 'equals' | 'contains' | 'startsWith' | 'endsWith' | 'gt' | 'lt' | 'gte' | 'lte' | 'between';
}

interface AdvancedSearchProps {
  fields: SearchField[];
  onSearch: (filters: SearchFilter[], searchText: string) => void;
  placeholder?: string;
  showAdvanced?: boolean;
  savedSearches?: { name: string; filters: SearchFilter[] }[];
  onSaveSearch?: (name: string, filters: SearchFilter[]) => void;
}

export const AdvancedSearch: React.FC<AdvancedSearchProps> = ({
  fields,
  onSearch,
  placeholder = 'Search...',
  showAdvanced = true,
  savedSearches = [],
  onSaveSearch,
}) => {
  const [searchText, setSearchText] = useState('');
  const [advancedOpen, setAdvancedOpen] = useState(false);
  const [filters, setFilters] = useState<Record<string, any>>({});
  const [activeFilters, setActiveFilters] = useState<SearchFilter[]>([]);

  // Debounced search for quick search text
  useEffect(() => {
    const timer = setTimeout(() => {
      if (searchText || activeFilters.length > 0) {
        onSearch(activeFilters, searchText);
      } else {
        onSearch([], '');
      }
    }, 300);
    return () => clearTimeout(timer);
  }, [searchText]);

  const handleFieldChange = (fieldName: string, value: any) => {
    setFilters((prev) => ({
      ...prev,
      [fieldName]: value,
    }));
  };

  const handleApplyFilters = () => {
    const newFilters: SearchFilter[] = [];
    
    Object.entries(filters).forEach(([key, value]) => {
      if (value !== undefined && value !== null && value !== '') {
        // Handle date range
        if (key.endsWith('_start') || key.endsWith('_end')) {
          const baseName = key.replace(/_start$|_end$/, '');
          const existingFilter = newFilters.find((f) => f.field === baseName);
          
          if (existingFilter) {
            if (key.endsWith('_start')) {
              existingFilter.value = { ...existingFilter.value, start: value };
            } else {
              existingFilter.value = { ...existingFilter.value, end: value };
            }
          } else {
            newFilters.push({
              field: baseName,
              operator: 'between',
              value: key.endsWith('_start') ? { start: value } : { end: value },
            });
          }
        } else {
          const field = fields.find((f) => f.name === key);
          newFilters.push({
            field: key,
            operator: field?.type === 'text' ? 'contains' : 'equals',
            value: value,
          });
        }
      }
    });

    setActiveFilters(newFilters);
    onSearch(newFilters, searchText);
  };

  const handleClearFilters = () => {
    setFilters({});
    setActiveFilters([]);
    setSearchText('');
    onSearch([], '');
  };

  const handleRemoveFilter = (fieldName: string) => {
    const newFilters = { ...filters };
    delete newFilters[fieldName];
    delete newFilters[`${fieldName}_start`];
    delete newFilters[`${fieldName}_end`];
    setFilters(newFilters);
    
    const newActiveFilters = activeFilters.filter((f) => f.field !== fieldName);
    setActiveFilters(newActiveFilters);
    onSearch(newActiveFilters, searchText);
  };

  const getFilterDisplayValue = (filter: SearchFilter): string => {
    const field = fields.find((f) => f.name === filter.field);
    if (!field) return String(filter.value);

    if (field.type === 'select' && field.options) {
      const option = field.options.find((o) => o.value === filter.value);
      return option?.label || String(filter.value);
    }

    if (field.type === 'dateRange' || field.type === 'numberRange') {
      const { start, end } = filter.value || {};
      if (start && end) return `${start} - ${end}`;
      if (start) return `From ${start}`;
      if (end) return `To ${end}`;
    }

    if (field.type === 'boolean') {
      return filter.value ? 'Yes' : 'No';
    }

    return String(filter.value);
  };

  const renderField = (field: SearchField) => {
    const value = filters[field.name];

    switch (field.type) {
      case 'select':
        return (
          <FormControl fullWidth size="small">
            <InputLabel>{field.label}</InputLabel>
            <Select
              value={value || ''}
              onChange={(e) => handleFieldChange(field.name, e.target.value)}
              label={field.label}
            >
              <MenuItem value="">
                <em>Any</em>
              </MenuItem>
              {field.options?.map((option) => (
                <MenuItem key={option.value} value={option.value}>
                  {option.label}
                </MenuItem>
              ))}
            </Select>
          </FormControl>
        );

      case 'date':
        return (
          <TextField
            fullWidth
            size="small"
            type="date"
            label={field.label}
            value={value || ''}
            onChange={(e) => handleFieldChange(field.name, e.target.value)}
            InputLabelProps={{ shrink: true }}
          />
        );

      case 'dateRange':
        return (
          <Stack direction="row" spacing={1}>
            <TextField
              fullWidth
              size="small"
              type="date"
              label={`${field.label} From`}
              value={filters[`${field.name}_start`] || ''}
              onChange={(e) => handleFieldChange(`${field.name}_start`, e.target.value)}
              InputLabelProps={{ shrink: true }}
            />
            <TextField
              fullWidth
              size="small"
              type="date"
              label={`${field.label} To`}
              value={filters[`${field.name}_end`] || ''}
              onChange={(e) => handleFieldChange(`${field.name}_end`, e.target.value)}
              InputLabelProps={{ shrink: true }}
            />
          </Stack>
        );

      case 'number':
        return (
          <TextField
            fullWidth
            size="small"
            type="number"
            label={field.label}
            value={value || ''}
            onChange={(e) => handleFieldChange(field.name, e.target.value)}
            placeholder={field.placeholder}
          />
        );

      case 'numberRange':
        return (
          <Stack direction="row" spacing={1}>
            <TextField
              fullWidth
              size="small"
              type="number"
              label={`${field.label} Min`}
              value={filters[`${field.name}_start`] || ''}
              onChange={(e) => handleFieldChange(`${field.name}_start`, e.target.value)}
            />
            <TextField
              fullWidth
              size="small"
              type="number"
              label={`${field.label} Max`}
              value={filters[`${field.name}_end`] || ''}
              onChange={(e) => handleFieldChange(`${field.name}_end`, e.target.value)}
            />
          </Stack>
        );

      case 'boolean':
        return (
          <FormControl fullWidth size="small">
            <InputLabel>{field.label}</InputLabel>
            <Select
              value={value === undefined ? '' : value}
              onChange={(e) => handleFieldChange(field.name, e.target.value === '' ? undefined : e.target.value === 'true')}
              label={field.label}
            >
              <MenuItem value="">
                <em>Any</em>
              </MenuItem>
              <MenuItem value="true">Yes</MenuItem>
              <MenuItem value="false">No</MenuItem>
            </Select>
          </FormControl>
        );

      default:
        return (
          <TextField
            fullWidth
            size="small"
            label={field.label}
            value={value || ''}
            onChange={(e) => handleFieldChange(field.name, e.target.value)}
            placeholder={field.placeholder}
          />
        );
    }
  };

  return (
    <Box sx={{ mb: 3 }}>
      {/* Quick Search Bar */}
      <Paper sx={{ p: 1, mb: 2 }}>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <TextField
            fullWidth
            size="small"
            placeholder={placeholder}
            value={searchText}
            onChange={(e) => setSearchText(e.target.value)}
            InputProps={{
              startAdornment: (
                <InputAdornment position="start">
                  <SearchIcon color="action" />
                </InputAdornment>
              ),
              endAdornment: searchText && (
                <InputAdornment position="end">
                  <IconButton
                    size="small"
                    onClick={() => setSearchText('')}
                  >
                    <ClearIcon fontSize="small" />
                  </IconButton>
                </InputAdornment>
              ),
            }}
          />
          
          {showAdvanced && (
            <Tooltip title={advancedOpen ? 'Hide advanced filters' : 'Show advanced filters'}>
              <Button
                variant={advancedOpen ? 'contained' : 'outlined'}
                size="small"
                startIcon={<FilterListIcon />}
                endIcon={advancedOpen ? <ExpandLessIcon /> : <ExpandMoreIcon />}
                onClick={() => setAdvancedOpen(!advancedOpen)}
                sx={{ minWidth: 140 }}
              >
                Filters
                {activeFilters.length > 0 && (
                  <Chip
                    label={activeFilters.length}
                    size="small"
                    color="primary"
                    sx={{ ml: 1, height: 20, minWidth: 20 }}
                  />
                )}
              </Button>
            </Tooltip>
          )}
        </Box>
      </Paper>

      {/* Active Filters Display */}
      {activeFilters.length > 0 && (
        <Box sx={{ mb: 2 }}>
          <Stack direction="row" spacing={1} flexWrap="wrap" useFlexGap>
            {activeFilters.map((filter) => {
              const field = fields.find((f) => f.name === filter.field);
              return (
                <Chip
                  key={filter.field}
                  label={`${field?.label || filter.field}: ${getFilterDisplayValue(filter)}`}
                  onDelete={() => handleRemoveFilter(filter.field)}
                  size="small"
                  color="primary"
                  variant="outlined"
                />
              );
            })}
            <Chip
              label="Clear All"
              onClick={handleClearFilters}
              size="small"
              color="secondary"
              variant="outlined"
              deleteIcon={<ClearIcon />}
            />
          </Stack>
        </Box>
      )}

      {/* Advanced Filters Panel */}
      <Collapse in={advancedOpen}>
        <Card variant="outlined" sx={{ mb: 2 }}>
          <CardContent>
            <Typography variant="subtitle2" gutterBottom color="text.secondary">
              Advanced Filters
            </Typography>
            <Grid container spacing={2}>
              {fields.map((field) => (
                <Grid item xs={12} sm={6} md={4} key={field.name}>
                  {renderField(field)}
                </Grid>
              ))}
            </Grid>
            
            <Divider sx={{ my: 2 }} />
            
            <Box sx={{ display: 'flex', justifyContent: 'flex-end', gap: 1 }}>
              <Button
                variant="outlined"
                size="small"
                onClick={handleClearFilters}
                startIcon={<ClearIcon />}
              >
                Clear
              </Button>
              <Button
                variant="contained"
                size="small"
                onClick={handleApplyFilters}
                startIcon={<SearchIcon />}
              >
                Apply Filters
              </Button>
            </Box>
          </CardContent>
        </Card>
      </Collapse>
    </Box>
  );
};

// Helper function to filter data locally
export const filterData = <T extends Record<string, any>>(
  data: T[],
  filters: SearchFilter[],
  searchText: string,
  searchableFields: string[]
): T[] => {
  let filtered = [...data];

  // Apply quick search text
  if (searchText) {
    const lowerSearch = searchText.toLowerCase();
    filtered = filtered.filter((item) =>
      searchableFields.some((field) => {
        const value = item[field];
        return value && String(value).toLowerCase().includes(lowerSearch);
      })
    );
  }

  // Apply advanced filters
  filters.forEach((filter) => {
    filtered = filtered.filter((item) => {
      const value = item[filter.field];
      
      if (value === undefined || value === null) {
        return false;
      }

      switch (filter.operator) {
        case 'equals':
          return value === filter.value;
        case 'contains':
          return String(value).toLowerCase().includes(String(filter.value).toLowerCase());
        case 'startsWith':
          return String(value).toLowerCase().startsWith(String(filter.value).toLowerCase());
        case 'endsWith':
          return String(value).toLowerCase().endsWith(String(filter.value).toLowerCase());
        case 'gt':
          return Number(value) > Number(filter.value);
        case 'lt':
          return Number(value) < Number(filter.value);
        case 'gte':
          return Number(value) >= Number(filter.value);
        case 'lte':
          return Number(value) <= Number(filter.value);
        case 'between':
          const { start, end } = filter.value || {};
          if (start && end) {
            return value >= start && value <= end;
          } else if (start) {
            return value >= start;
          } else if (end) {
            return value <= end;
          }
          return true;
        default:
          return String(value).toLowerCase().includes(String(filter.value).toLowerCase());
      }
    });
  });

  return filtered;
};

export default AdvancedSearch;
