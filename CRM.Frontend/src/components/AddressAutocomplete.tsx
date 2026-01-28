/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 * Licensed under the GNU Affero General Public License v3.0
 * 
 * AddressAutocomplete - Cascading address input with zip code validation
 * Validates Country → State → City → ZipCode against master data
 */

import React, { useState, useEffect, useCallback, useMemo } from 'react';
import {
  Autocomplete,
  TextField,
  Grid,
  Box,
  Typography,
  CircularProgress,
  Chip,
  Alert,
  InputAdornment,
} from '@mui/material';
import {
  CheckCircle as ValidIcon,
  Error as InvalidIcon,
  Warning as WarningIcon,
  LocationOn as LocationIcon,
} from '@mui/icons-material';
import zipCodeService, {
  CountryInfo,
  StateInfo,
  ZipCodeLookupResult,
  ZipCodeValidationResult,
} from '../services/zipCodeService';
import { debounce } from 'lodash';

// Cache for zip code data to avoid repeated API calls
const locationCache = {
  countries: null as CountryInfo[] | null,
  states: new Map<string, StateInfo[]>(),
  cities: new Map<string, string[]>(),
  zipCodes: new Map<string, ZipCodeLookupResult[]>(),
  lookups: new Map<string, ZipCodeLookupResult[]>(),
};

export interface AddressData {
  country: string;
  countryCode: string;
  state: string;
  stateCode: string;
  city: string;
  postalCode: string;
  county?: string;
  locality?: string;
  zipCodeId?: number;
}

interface AddressAutocompleteProps {
  value: AddressData;
  onChange: (address: AddressData) => void;
  disabled?: boolean;
  required?: boolean;
  size?: 'small' | 'medium';
  showValidation?: boolean;
  allowFreeText?: boolean; // Allow values not in database
  gridSpacing?: number;
  labels?: {
    country?: string;
    state?: string;
    city?: string;
    postalCode?: string;
  };
}

const defaultLabels = {
  country: 'Country',
  state: 'State/Province',
  city: 'City',
  postalCode: 'Postal Code',
};

const AddressAutocomplete: React.FC<AddressAutocompleteProps> = ({
  value,
  onChange,
  disabled = false,
  required = false,
  size = 'small',
  showValidation = true,
  allowFreeText = false,
  gridSpacing = 2,
  labels = defaultLabels,
}) => {
  // State for options
  const [countries, setCountries] = useState<CountryInfo[]>([]);
  const [states, setStates] = useState<StateInfo[]>([]);
  const [cities, setCities] = useState<string[]>([]);
  const [postalCodes, setPostalCodes] = useState<ZipCodeLookupResult[]>([]);
  
  // Loading states
  const [loadingCountries, setLoadingCountries] = useState(false);
  const [loadingStates, setLoadingStates] = useState(false);
  const [loadingCities, setLoadingCities] = useState(false);
  const [loadingPostalCodes, setLoadingPostalCodes] = useState(false);
  
  // Validation state
  const [validation, setValidation] = useState<ZipCodeValidationResult | null>(null);
  const [validating, setValidating] = useState(false);

  // Merged labels
  const fieldLabels = { ...defaultLabels, ...labels };

  // Load countries on mount
  useEffect(() => {
    const loadCountries = async () => {
      if (locationCache.countries) {
        setCountries(locationCache.countries);
        return;
      }
      
      setLoadingCountries(true);
      try {
        const data = await zipCodeService.getCountries();
        locationCache.countries = data;
        setCountries(data);
      } catch (error) {
        console.error('Failed to load countries:', error);
        // Fallback to common countries
        setCountries([
          { countryCode: 'US', countryName: 'United States', postalCodeFormat: '12345 or 12345-6789' },
          { countryCode: 'CA', countryName: 'Canada', postalCodeFormat: 'A1A 1A1' },
          { countryCode: 'GB', countryName: 'United Kingdom', postalCodeFormat: 'AA9A 9AA' },
        ]);
      } finally {
        setLoadingCountries(false);
      }
    };
    loadCountries();
  }, []);

  // Load states when country changes
  useEffect(() => {
    const loadStates = async () => {
      if (!value.countryCode) {
        setStates([]);
        return;
      }

      const cacheKey = value.countryCode;
      if (locationCache.states.has(cacheKey)) {
        setStates(locationCache.states.get(cacheKey)!);
        return;
      }

      setLoadingStates(true);
      try {
        const data = await zipCodeService.getStates(value.countryCode);
        locationCache.states.set(cacheKey, data);
        setStates(data);
      } catch (error) {
        console.error('Failed to load states:', error);
        setStates([]);
      } finally {
        setLoadingStates(false);
      }
    };
    loadStates();
  }, [value.countryCode]);

  // Load cities when state changes
  useEffect(() => {
    const loadCities = async () => {
      if (!value.countryCode || !value.stateCode) {
        setCities([]);
        return;
      }

      const cacheKey = `${value.countryCode}:${value.stateCode}`;
      if (locationCache.cities.has(cacheKey)) {
        setCities(locationCache.cities.get(cacheKey)!);
        return;
      }

      setLoadingCities(true);
      try {
        const data = await zipCodeService.getCities(value.countryCode, value.stateCode);
        locationCache.cities.set(cacheKey, data);
        setCities(data);
      } catch (error) {
        console.error('Failed to load cities:', error);
        setCities([]);
      } finally {
        setLoadingCities(false);
      }
    };
    loadCities();
  }, [value.countryCode, value.stateCode]);

  // Load postal codes when city changes
  useEffect(() => {
    const loadPostalCodes = async () => {
      if (!value.countryCode || !value.stateCode || !value.city) {
        setPostalCodes([]);
        return;
      }

      const cacheKey = `${value.countryCode}:${value.stateCode}:${value.city}`;
      if (locationCache.zipCodes.has(cacheKey)) {
        setPostalCodes(locationCache.zipCodes.get(cacheKey)!);
        return;
      }

      setLoadingPostalCodes(true);
      try {
        const data = await zipCodeService.getPostalCodes(value.countryCode, value.stateCode, value.city);
        locationCache.zipCodes.set(cacheKey, data);
        setPostalCodes(data);
      } catch (error) {
        console.error('Failed to load postal codes:', error);
        setPostalCodes([]);
      } finally {
        setLoadingPostalCodes(false);
      }
    };
    loadPostalCodes();
  }, [value.countryCode, value.stateCode, value.city]);

  // Validate postal code with debounce
  const validatePostalCode = useMemo(
    () =>
      debounce(async (postalCode: string, countryCode: string) => {
        if (!postalCode || !countryCode) {
          setValidation(null);
          return;
        }

        setValidating(true);
        try {
          const result = await zipCodeService.validatePostalCode(postalCode, countryCode);
          setValidation(result);
        } catch (error) {
          console.error('Validation failed:', error);
          setValidation(null);
        } finally {
          setValidating(false);
        }
      }, 500),
    []
  );

  useEffect(() => {
    if (showValidation && value.postalCode && value.countryCode) {
      validatePostalCode(value.postalCode, value.countryCode);
    }
    return () => validatePostalCode.cancel();
  }, [value.postalCode, value.countryCode, showValidation, validatePostalCode]);

  // Auto-populate from postal code lookup
  const handlePostalCodeLookup = useCallback(
    async (postalCode: string) => {
      if (!postalCode || postalCode.length < 3) return;

      const cacheKey = `lookup:${postalCode}:${value.countryCode || ''}`;
      let results: ZipCodeLookupResult[];

      if (locationCache.lookups.has(cacheKey)) {
        results = locationCache.lookups.get(cacheKey)!;
      } else {
        try {
          results = await zipCodeService.lookupByPostalCode(postalCode, value.countryCode || undefined);
          locationCache.lookups.set(cacheKey, results);
        } catch {
          return;
        }
      }

      if (results.length === 1) {
        const result = results[0];
        onChange({
          ...value,
          country: result.country,
          countryCode: result.countryCode,
          state: result.state,
          stateCode: result.stateCode,
          city: result.city,
          postalCode: result.postalCode,
          county: result.county,
          zipCodeId: result.id,
        });
      }
    },
    [value, onChange]
  );

  // Handlers
  const handleCountryChange = (_: unknown, newValue: CountryInfo | string | null) => {
    const country = typeof newValue === 'string' 
      ? countries.find(c => c.countryName === newValue) 
      : newValue;
    
    onChange({
      ...value,
      country: country?.countryName || (typeof newValue === 'string' ? newValue : ''),
      countryCode: country?.countryCode || '',
      state: '',
      stateCode: '',
      city: '',
      postalCode: '',
      zipCodeId: undefined,
    });
    setValidation(null);
  };

  const handleStateChange = (_: unknown, newValue: StateInfo | string | null) => {
    const state = typeof newValue === 'string'
      ? states.find(s => s.stateName === newValue)
      : newValue;
    
    onChange({
      ...value,
      state: state?.stateName || (typeof newValue === 'string' ? newValue : ''),
      stateCode: state?.stateCode || '',
      city: '',
      postalCode: '',
      zipCodeId: undefined,
    });
    setValidation(null);
  };

  const handleCityChange = (_: unknown, newValue: string | null) => {
    onChange({
      ...value,
      city: newValue || '',
      postalCode: '',
      zipCodeId: undefined,
    });
    setValidation(null);
  };

  const handlePostalCodeChange = (_: unknown, newValue: ZipCodeLookupResult | string | null) => {
    if (typeof newValue === 'string') {
      onChange({
        ...value,
        postalCode: newValue,
        zipCodeId: undefined,
      });
      // Trigger lookup for auto-population
      handlePostalCodeLookup(newValue);
    } else if (newValue) {
      onChange({
        ...value,
        postalCode: newValue.postalCode,
        city: newValue.city || value.city,
        state: newValue.state || value.state,
        stateCode: newValue.stateCode || value.stateCode,
        county: newValue.county,
        zipCodeId: newValue.id,
      });
    } else {
      onChange({
        ...value,
        postalCode: '',
        zipCodeId: undefined,
      });
    }
  };

  // Get validation icon
  const getValidationIcon = () => {
    if (validating) {
      return <CircularProgress size={16} />;
    }
    if (!validation) return null;
    
    if (validation.isValid && validation.existsInDatabase) {
      return <ValidIcon color="success" fontSize="small" />;
    }
    if (validation.isFormatValid && !validation.existsInDatabase) {
      return <WarningIcon color="warning" fontSize="small" />;
    }
    return <InvalidIcon color="error" fontSize="small" />;
  };

  const getPostalCodeFormat = () => {
    const country = countries.find(c => c.countryCode === value.countryCode);
    return country?.postalCodeFormat;
  };

  return (
    <Box>
      <Grid container spacing={gridSpacing}>
        {/* Country */}
        <Grid item xs={12} sm={6}>
          <Autocomplete<CountryInfo, false, false, boolean>
            value={countries.find(c => c.countryCode === value.countryCode) || null}
            onChange={handleCountryChange}
            options={countries}
            getOptionLabel={(option) => typeof option === 'string' ? option : option.countryName}
            loading={loadingCountries}
            disabled={disabled}
            freeSolo={allowFreeText}
            size={size}
            renderInput={(params) => (
              <TextField
                {...params}
                label={fieldLabels.country}
                required={required}
                InputProps={{
                  ...params.InputProps,
                  startAdornment: (
                    <InputAdornment position="start">
                      <LocationIcon fontSize="small" color="action" />
                    </InputAdornment>
                  ),
                  endAdornment: (
                    <>
                      {loadingCountries ? <CircularProgress size={16} /> : null}
                      {params.InputProps.endAdornment}
                    </>
                  ),
                }}
              />
            )}
            renderOption={(props, option) => {
              const countryOption = option as CountryInfo;
              return (
                <li {...props}>
                  <Box sx={{ display: 'flex', justifyContent: 'space-between', width: '100%' }}>
                    <span>{countryOption.countryName}</span>
                    <Chip label={countryOption.countryCode} size="small" variant="outlined" />
                  </Box>
                </li>
              );
            }}
          />
        </Grid>

        {/* State */}
        <Grid item xs={12} sm={6}>
          <Autocomplete<StateInfo, false, false, boolean>
            value={states.find(s => s.stateCode === value.stateCode) || null}
            onChange={handleStateChange}
            options={states}
            getOptionLabel={(option) => typeof option === 'string' ? option : option.stateName}
            loading={loadingStates}
            disabled={disabled || !value.countryCode}
            freeSolo={allowFreeText}
            size={size}
            renderInput={(params) => (
              <TextField
                {...params}
                label={fieldLabels.state}
                required={required}
                placeholder={!value.countryCode ? 'Select country first' : undefined}
                InputProps={{
                  ...params.InputProps,
                  endAdornment: (
                    <>
                      {loadingStates ? <CircularProgress size={16} /> : null}
                      {params.InputProps.endAdornment}
                    </>
                  ),
                }}
              />
            )}
            renderOption={(props, option) => {
              const stateOption = option as StateInfo;
              return (
                <li {...props}>
                  <Box sx={{ display: 'flex', justifyContent: 'space-between', width: '100%' }}>
                    <span>{stateOption.stateName}</span>
                    <Chip label={stateOption.stateCode} size="small" variant="outlined" />
                  </Box>
                </li>
              );
            }}
          />
        </Grid>

        {/* City */}
        <Grid item xs={12} sm={6}>
          <Autocomplete
            value={value.city || null}
            onChange={handleCityChange}
            options={cities}
            loading={loadingCities}
            disabled={disabled || !value.stateCode}
            freeSolo={allowFreeText}
            size={size}
            renderInput={(params) => (
              <TextField
                {...params}
                label={fieldLabels.city}
                required={required}
                placeholder={!value.stateCode ? 'Select state first' : undefined}
                InputProps={{
                  ...params.InputProps,
                  endAdornment: (
                    <>
                      {loadingCities ? <CircularProgress size={16} /> : null}
                      {params.InputProps.endAdornment}
                    </>
                  ),
                }}
              />
            )}
          />
        </Grid>

        {/* Postal Code */}
        <Grid item xs={12} sm={6}>
          <Autocomplete<ZipCodeLookupResult, false, false, true>
            value={postalCodes.find(p => p.postalCode === value.postalCode) || null}
            onChange={handlePostalCodeChange}
            inputValue={value.postalCode || ''}
            onInputChange={(_, newInputValue) => {
              if (newInputValue !== value.postalCode) {
                onChange({ ...value, postalCode: newInputValue, zipCodeId: undefined });
                handlePostalCodeLookup(newInputValue);
              }
            }}
            options={postalCodes}
            getOptionLabel={(option) => typeof option === 'string' ? option : option.postalCode}
            loading={loadingPostalCodes}
            disabled={disabled}
            freeSolo
            size={size}
            renderInput={(params) => (
              <TextField
                {...params}
                label={fieldLabels.postalCode}
                required={required}
                placeholder={getPostalCodeFormat() || undefined}
                helperText={
                  validation && !validation.isValid
                    ? validation.message
                    : getPostalCodeFormat()
                      ? `Format: ${getPostalCodeFormat()}`
                      : undefined
                }
                error={validation ? !validation.isValid : false}
                InputProps={{
                  ...params.InputProps,
                  endAdornment: (
                    <>
                      {showValidation && getValidationIcon()}
                      {loadingPostalCodes ? <CircularProgress size={16} /> : null}
                      {params.InputProps.endAdornment}
                    </>
                  ),
                }}
              />
            )}
            renderOption={(props, option) => {
              const zipOption = option as ZipCodeLookupResult;
              return (
                <li {...props}>
                  <Box>
                    <Typography variant="body2">{zipOption.postalCode}</Typography>
                    <Typography variant="caption" color="text.secondary">
                      {zipOption.city}, {zipOption.stateCode} {zipOption.county && `(${zipOption.county})`}
                    </Typography>
                  </Box>
                </li>
              );
            }}
          />
        </Grid>
      </Grid>

      {/* Validation feedback */}
      {showValidation && validation && !validation.isValid && (
        <Alert severity="warning" sx={{ mt: 1 }} icon={<WarningIcon />}>
          {validation.message}
          {validation.expectedFormat && (
            <Typography variant="caption" display="block">
              Expected format: {validation.expectedFormat}
            </Typography>
          )}
        </Alert>
      )}

      {/* Auto-populated info */}
      {value.zipCodeId && value.county && (
        <Typography variant="caption" color="text.secondary" sx={{ mt: 0.5, display: 'block' }}>
          County: {value.county}
        </Typography>
      )}
    </Box>
  );
};

// Export cache clear function for manual cache invalidation
export const clearLocationCache = () => {
  locationCache.countries = null;
  locationCache.states.clear();
  locationCache.cities.clear();
  locationCache.zipCodes.clear();
  locationCache.lookups.clear();
};

export default AddressAutocomplete;
