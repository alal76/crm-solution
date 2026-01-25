import React, { useState, useEffect, useCallback, useMemo } from 'react';
import {
  Box,
  Button,
  Card,
  CardContent,
  CardHeader,
  Chip,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Grid,
  IconButton,
  List,
  ListItem,
  ListItemSecondaryAction,
  ListItemText,
  MenuItem,
  Select,
  TextField,
  Tooltip,
  Typography,
  FormControlLabel,
  Checkbox,
  Alert,
  CircularProgress,
  Autocomplete,
  InputAdornment,
  FormControl,
  InputLabel,
  Collapse,
} from '@mui/material';
import {
  Add as AddIcon,
  Delete as DeleteIcon,
  Edit as EditIcon,
  Star as StarIcon,
  StarOutline as StarOutlineIcon,
  Share as ShareIcon,
  LocationOn as LocationIcon,
  Home as HomeIcon,
  Business as BusinessIcon,
  Search as SearchIcon,
  Check as CheckIcon,
  Error as ErrorIcon,
  Info as InfoIcon,
} from '@mui/icons-material';
import { contactInfoService, EntityType, LinkedAddressDto, CreateAddressDto, AddressType } from '../../services/contactInfoService';
import zipCodeService, { CountryInfo, StateInfo, ZipCodeLookupResult, ZipCodeValidationResult, LocalityInfo } from '../../services/zipCodeService';
import { debounce } from 'lodash';

interface AddressManagerProps {
  entityType: EntityType;
  entityId: number;
  readOnly?: boolean;
  onAddressChange?: () => void;
}

const ADDRESS_TYPES: AddressType[] = ['Primary', 'Billing', 'Shipping', 'Mailing', 'Headquarters', 'Branch', 'Warehouse', 'Other'];

const AddressManager: React.FC<AddressManagerProps> = ({
  entityType,
  entityId,
  readOnly = false,
  onAddressChange,
}) => {
  const [addresses, setAddresses] = useState<LinkedAddressDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [dialogOpen, setDialogOpen] = useState(false);
  const [editingAddress, setEditingAddress] = useState<LinkedAddressDto | null>(null);
  
  // Zip code database state
  const [countries, setCountries] = useState<CountryInfo[]>([]);
  const [states, setStates] = useState<StateInfo[]>([]);
  const [cities, setCities] = useState<string[]>([]);
  const [postalCodes, setPostalCodes] = useState<ZipCodeLookupResult[]>([]);
  const [localities, setLocalities] = useState<LocalityInfo[]>([]);
  const [loadingCountries, setLoadingCountries] = useState(false);
  const [loadingStates, setLoadingStates] = useState(false);
  const [loadingCities, setLoadingCities] = useState(false);
  const [loadingPostalCodes, setLoadingPostalCodes] = useState(false);
  const [loadingLocalities, setLoadingLocalities] = useState(false);
  const [postalCodeValidation, setPostalCodeValidation] = useState<ZipCodeValidationResult | null>(null);
  const [zipLookupResults, setZipLookupResults] = useState<ZipCodeLookupResult[]>([]);
  const [showZipLookupResults, setShowZipLookupResults] = useState(false);
  const [showAddLocality, setShowAddLocality] = useState(false);
  const [newLocalityName, setNewLocalityName] = useState('');

  const [formData, setFormData] = useState<CreateAddressDto & { addressType: AddressType; isPrimary: boolean }>({
    line1: '',
    line2: '',
    line3: '',
    city: '',
    state: '',
    postalCode: '',
    county: '',
    countryCode: 'US',
    country: 'United States',
    locality: '',
    localityId: undefined,
    zipCodeId: undefined,
    isResidential: false,
    deliveryInstructions: '',
    accessHours: '',
    siteContactName: '',
    siteContactPhone: '',
    notes: '',
    addressType: 'Primary',
    isPrimary: false,
  });

  // Load countries on mount
  useEffect(() => {
    const loadCountries = async () => {
      try {
        setLoadingCountries(true);
        const data = await zipCodeService.getCountries();
        setCountries(data);
      } catch (err) {
        console.error('Error loading countries:', err);
      } finally {
        setLoadingCountries(false);
      }
    };
    loadCountries();
  }, []);

  // Load states when country changes
  useEffect(() => {
    if (formData.countryCode) {
      const loadStates = async () => {
        try {
          setLoadingStates(true);
          setStates([]);
          setCities([]);
          setPostalCodes([]);
          const data = await zipCodeService.getStates(formData.countryCode);
          setStates(data);
        } catch (err) {
          console.error('Error loading states:', err);
        } finally {
          setLoadingStates(false);
        }
      };
      loadStates();
    }
  }, [formData.countryCode]);

  // Load cities when state changes
  useEffect(() => {
    const selectedState = states.find(s => s.stateName === formData.state || s.stateCode === formData.state);
    if (formData.countryCode && selectedState) {
      const loadCities = async () => {
        try {
          setLoadingCities(true);
          setCities([]);
          setPostalCodes([]);
          const data = await zipCodeService.getCities(formData.countryCode, selectedState.stateCode);
          setCities(data);
        } catch (err) {
          console.error('Error loading cities:', err);
        } finally {
          setLoadingCities(false);
        }
      };
      loadCities();
    }
  }, [formData.countryCode, formData.state, states]);

  // Load postal codes when city changes
  useEffect(() => {
    const selectedState = states.find(s => s.stateName === formData.state || s.stateCode === formData.state);
    if (formData.countryCode && selectedState && formData.city) {
      const loadPostalCodes = async () => {
        try {
          setLoadingPostalCodes(true);
          const data = await zipCodeService.getPostalCodes(formData.countryCode, selectedState.stateCode, formData.city);
          setPostalCodes(data);
        } catch (err) {
          console.error('Error loading postal codes:', err);
        } finally {
          setLoadingPostalCodes(false);
        }
      };
      loadPostalCodes();
    }
  }, [formData.countryCode, formData.state, formData.city, states]);

  // Load localities when city changes
  useEffect(() => {
    if (formData.city && formData.countryCode) {
      const loadLocalities = async () => {
        try {
          setLoadingLocalities(true);
          setLocalities([]);
          const data = await zipCodeService.getLocalitiesByCity(formData.city, formData.countryCode);
          setLocalities(data);
        } catch (err) {
          console.error('Error loading localities:', err);
        } finally {
          setLoadingLocalities(false);
        }
      };
      loadLocalities();
    }
  }, [formData.city, formData.countryCode]);

  // Handler for creating new locality
  const handleCreateLocality = async () => {
    if (!newLocalityName.trim()) return;
    
    try {
      const selectedState = states.find(s => s.stateName === formData.state || s.stateCode === formData.state);
      const locality = await zipCodeService.createLocality({
        name: newLocalityName.trim(),
        city: formData.city,
        stateCode: selectedState?.stateCode || formData.state,
        countryCode: formData.countryCode,
        zipCodeId: formData.zipCodeId,
      });
      setLocalities([...localities, locality]);
      setFormData({ ...formData, locality: locality.name, localityId: locality.id });
      setNewLocalityName('');
      setShowAddLocality(false);
    } catch (err) {
      console.error('Error creating locality:', err);
    }
  };

  // Debounced postal code validation
  const validatePostalCode = useMemo(
    () =>
      debounce(async (postalCode: string, countryCode: string) => {
        if (postalCode.length >= 3 && countryCode) {
          try {
            const result = await zipCodeService.validatePostalCode(postalCode, countryCode);
            setPostalCodeValidation(result);
          } catch (err) {
            console.error('Error validating postal code:', err);
          }
        } else {
          setPostalCodeValidation(null);
        }
      }, 500),
    []
  );

  // Debounced postal code lookup for auto-population
  const lookupPostalCode = useMemo(
    () =>
      debounce(async (postalCode: string, countryCode?: string) => {
        if (postalCode.length >= 3) {
          try {
            const results = await zipCodeService.lookupByPostalCode(postalCode, countryCode);
            setZipLookupResults(results);
            if (results.length > 0) {
              setShowZipLookupResults(true);
            }
          } catch (err) {
            console.error('Error looking up postal code:', err);
            setZipLookupResults([]);
          }
        } else {
          setZipLookupResults([]);
          setShowZipLookupResults(false);
        }
      }, 300),
    []
  );

  const handlePostalCodeChange = (value: string) => {
    setFormData({ ...formData, postalCode: value });
    validatePostalCode(value, formData.countryCode);
    lookupPostalCode(value, formData.countryCode || undefined);
  };

  const applyZipLookupResult = (result: ZipCodeLookupResult) => {
    setFormData({
      ...formData,
      city: result.city,
      state: result.state,
      county: result.county,
      country: result.country,
      countryCode: result.countryCode,
      postalCode: result.postalCode,
      zipCodeId: result.id,
      locality: '',
      localityId: undefined,
    });
    setShowZipLookupResults(false);
    setPostalCodeValidation({
      isValid: true,
      isFormatValid: true,
      existsInDatabase: true,
      message: 'Valid postal code',
      expectedFormat: null,
    });
  };

  const handleCountryChange = (countryCode: string) => {
    const selectedCountry = countries.find(c => c.countryCode === countryCode);
    setFormData({
      ...formData,
      countryCode,
      country: selectedCountry?.countryName || '',
      state: '',
      city: '',
      postalCode: '',
      county: '',
    });
    setStates([]);
    setCities([]);
    setPostalCodes([]);
    setPostalCodeValidation(null);
  };

  const handleStateChange = (stateName: string) => {
    setFormData({
      ...formData,
      state: stateName,
      city: '',
      postalCode: '',
      county: '',
    });
    setCities([]);
    setPostalCodes([]);
  };

  const handleCityChange = (city: string) => {
    setFormData({
      ...formData,
      city,
      postalCode: '',
    });
    setPostalCodes([]);
  };

  const getPostalCodeHelperText = () => {
    if (!postalCodeValidation) {
      const country = countries.find(c => c.countryCode === formData.countryCode);
      if (country?.postalCodeFormat) {
        return `Format: ${country.postalCodeFormat}`;
      }
      return '';
    }

    return postalCodeValidation.message;
  };

  const getPostalCodeColor = () => {
    if (!postalCodeValidation) return undefined;
    if (postalCodeValidation.isValid) return 'success';
    if (!postalCodeValidation.isFormatValid) return 'error';
    return 'warning';
  };

  const fetchAddresses = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      const response = await contactInfoService.getAddresses(entityType, entityId);
      setAddresses(response.data || []);
    } catch (err) {
      setError('Failed to load addresses');
      console.error('Error fetching addresses:', err);
    } finally {
      setLoading(false);
    }
  }, [entityType, entityId]);

  useEffect(() => {
    if (entityId) {
      fetchAddresses();
    }
  }, [entityId, fetchAddresses]);

  const handleOpenDialog = (address?: LinkedAddressDto) => {
    // Reset zip lookup state
    setZipLookupResults([]);
    setShowZipLookupResults(false);
    setPostalCodeValidation(null);

    if (address) {
      setEditingAddress(address);
      setFormData({
        line1: address.line1,
        line2: address.line2 || '',
        line3: address.line3 || '',
        city: address.city,
        state: address.state,
        postalCode: address.postalCode,
        county: address.county || '',
        countryCode: address.countryCode || 'US',
        country: address.country || 'United States',
        isResidential: address.isResidential || false,
        deliveryInstructions: address.deliveryInstructions || '',
        accessHours: address.accessHours || '',
        siteContactName: address.siteContactName || '',
        siteContactPhone: address.siteContactPhone || '',
        notes: address.notes || '',
        addressType: address.addressType,
        isPrimary: address.isPrimary,
      });
    } else {
      setEditingAddress(null);
      setFormData({
        line1: '',
        line2: '',
        line3: '',
        city: '',
        state: '',
        postalCode: '',
        county: '',
        countryCode: 'US',
        country: 'United States',
        isResidential: false,
        deliveryInstructions: '',
        accessHours: '',
        siteContactName: '',
        siteContactPhone: '',
        notes: '',
        addressType: 'Primary',
        isPrimary: addresses.length === 0, // First address is primary by default
      });
    }
    setDialogOpen(true);
  };

  const handleCloseDialog = () => {
    setDialogOpen(false);
    setEditingAddress(null);
    setZipLookupResults([]);
    setShowZipLookupResults(false);
    setPostalCodeValidation(null);
  };

  const handleSave = async () => {
    try {
      const { addressType, isPrimary, ...addressData } = formData;

      if (editingAddress) {
        // Update existing address
        await contactInfoService.updateAddress(editingAddress.id, addressData);
        // Update link if needed
        if (isPrimary && !editingAddress.isPrimary) {
          await contactInfoService.setPrimaryAddress(entityType, entityId, editingAddress.id, addressType);
        }
      } else {
        // Create and link new address
        await contactInfoService.linkAddress({
          newAddress: addressData,
          entityType,
          entityId,
          addressType,
          isPrimary,
        });
      }

      await fetchAddresses();
      handleCloseDialog();
      onAddressChange?.();
    } catch (err) {
      setError('Failed to save address');
      console.error('Error saving address:', err);
    }
  };

  const handleDelete = async (address: LinkedAddressDto) => {
    if (window.confirm('Are you sure you want to remove this address?')) {
      try {
        await contactInfoService.unlinkAddress(address.linkId);
        await fetchAddresses();
        onAddressChange?.();
      } catch (err) {
        setError('Failed to remove address');
        console.error('Error deleting address:', err);
      }
    }
  };

  const handleSetPrimary = async (address: LinkedAddressDto) => {
    try {
      await contactInfoService.setPrimaryAddress(entityType, entityId, address.id, address.addressType);
      await fetchAddresses();
      onAddressChange?.();
    } catch (err) {
      setError('Failed to set primary address');
      console.error('Error setting primary address:', err);
    }
  };

  const getAddressIcon = (addressType: AddressType) => {
    switch (addressType) {
      case 'Headquarters':
      case 'Branch':
      case 'Billing':
        return <BusinessIcon fontSize="small" />;
      case 'Shipping':
      case 'Warehouse':
        return <LocationIcon fontSize="small" />;
      default:
        return <HomeIcon fontSize="small" />;
    }
  };

  if (loading) {
    return (
      <Box display="flex" justifyContent="center" p={3}>
        <CircularProgress size={24} />
      </Box>
    );
  }

  return (
    <Card variant="outlined">
      <CardHeader
        title="Addresses"
        titleTypographyProps={{ variant: 'h6' }}
        action={
          !readOnly && (
            <Button
              startIcon={<AddIcon />}
              size="small"
              onClick={() => handleOpenDialog()}
            >
              Add Address
            </Button>
          )
        }
      />
      <CardContent>
        {error && (
          <Alert severity="error" sx={{ mb: 2 }}>
            {error}
          </Alert>
        )}

        {addresses.length === 0 ? (
          <Typography variant="body2" color="textSecondary">
            No addresses on file
          </Typography>
        ) : (
          <List dense>
            {addresses.map((address) => (
              <ListItem key={address.linkId} divider>
                <Box mr={1}>{getAddressIcon(address.addressType)}</Box>
                <ListItemText
                  primary={
                    <Box display="flex" alignItems="center" gap={1}>
                      <Typography variant="body1">
                        {address.line1}
                        {address.line2 && `, ${address.line2}`}
                      </Typography>
                      <Chip
                        label={address.addressType}
                        size="small"
                        variant="outlined"
                      />
                      {address.isPrimary && (
                        <Chip
                          label="Primary"
                          size="small"
                          color="primary"
                        />
                      )}
                    </Box>
                  }
                  secondary={
                    <>
                      {address.city}, {address.state} {address.postalCode}
                      {address.country && `, ${address.country}`}
                      {address.isVerified && (
                        <Chip label="Verified" size="small" color="success" sx={{ ml: 1 }} />
                      )}
                    </>
                  }
                />
                {!readOnly && (
                  <ListItemSecondaryAction>
                    {!address.isPrimary && (
                      <Tooltip title="Set as Primary">
                        <IconButton size="small" onClick={() => handleSetPrimary(address)}>
                          <StarOutlineIcon fontSize="small" />
                        </IconButton>
                      </Tooltip>
                    )}
                    {address.isPrimary && (
                      <Tooltip title="Primary Address">
                        <IconButton size="small" disabled>
                          <StarIcon fontSize="small" color="primary" />
                        </IconButton>
                      </Tooltip>
                    )}
                    <Tooltip title="Edit">
                      <IconButton size="small" onClick={() => handleOpenDialog(address)}>
                        <EditIcon fontSize="small" />
                      </IconButton>
                    </Tooltip>
                    <Tooltip title="Remove">
                      <IconButton size="small" onClick={() => handleDelete(address)}>
                        <DeleteIcon fontSize="small" />
                      </IconButton>
                    </Tooltip>
                  </ListItemSecondaryAction>
                )}
              </ListItem>
            ))}
          </List>
        )}
      </CardContent>

      {/* Add/Edit Dialog */}
      <Dialog open={dialogOpen} onClose={handleCloseDialog} maxWidth="md" fullWidth>
        <DialogTitle>
          {editingAddress ? 'Edit Address' : 'Add Address'}
        </DialogTitle>
        <DialogContent>
          <Grid container spacing={2} sx={{ mt: 1 }}>
            <Grid item xs={12} sm={6}>
              <FormControl fullWidth size="small">
                <InputLabel>Address Type</InputLabel>
                <Select
                  value={formData.addressType}
                  onChange={(e) => setFormData({ ...formData, addressType: e.target.value as AddressType })}
                  label="Address Type"
                >
                  {ADDRESS_TYPES.map((type) => (
                    <MenuItem key={type} value={type}>
                      {type}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12} sm={6}>
              <FormControlLabel
                control={
                  <Checkbox
                    checked={formData.isPrimary}
                    onChange={(e) => setFormData({ ...formData, isPrimary: e.target.checked })}
                  />
                }
                label="Primary Address"
              />
            </Grid>

            {/* Postal Code Lookup Section */}
            <Grid item xs={12}>
              <Alert severity="info" sx={{ mb: 1 }}>
                <Typography variant="body2">
                  <strong>Quick Entry:</strong> Enter a postal/ZIP code to auto-populate address fields, 
                  or select Country → State → City for dropdown selection.
                </Typography>
              </Alert>
            </Grid>

            {/* Postal Code Field - First for quick lookup */}
            <Grid item xs={12} sm={4}>
              <TextField
                fullWidth
                label="Postal/ZIP Code"
                value={formData.postalCode}
                onChange={(e) => handlePostalCodeChange(e.target.value)}
                size="small"
                placeholder="Enter to auto-fill"
                helperText={getPostalCodeHelperText()}
                color={getPostalCodeColor()}
                error={postalCodeValidation ? !postalCodeValidation.isFormatValid : false}
                InputProps={{
                  endAdornment: (
                    <InputAdornment position="end">
                      {postalCodeValidation?.isValid && <CheckIcon color="success" fontSize="small" />}
                      {postalCodeValidation && !postalCodeValidation.isFormatValid && <ErrorIcon color="error" fontSize="small" />}
                      {postalCodeValidation && postalCodeValidation.isFormatValid && !postalCodeValidation.existsInDatabase && (
                        <InfoIcon color="warning" fontSize="small" />
                      )}
                    </InputAdornment>
                  ),
                }}
              />
            </Grid>

            {/* Country Dropdown */}
            <Grid item xs={12} sm={8}>
              <Autocomplete
                options={countries}
                getOptionLabel={(option) => option.countryName}
                value={countries.find(c => c.countryCode === formData.countryCode) || null}
                onChange={(_, value) => handleCountryChange(value?.countryCode || '')}
                loading={loadingCountries}
                size="small"
                renderInput={(params) => (
                  <TextField
                    {...params}
                    label="Country"
                    required
                    InputProps={{
                      ...params.InputProps,
                      endAdornment: (
                        <>
                          {loadingCountries ? <CircularProgress size={16} /> : null}
                          {params.InputProps.endAdornment}
                        </>
                      ),
                    }}
                  />
                )}
                renderOption={(props, option) => (
                  <li {...props} key={option.countryCode}>
                    <Box>
                      <Typography variant="body2">{option.countryName}</Typography>
                      {option.postalCodeFormat && (
                        <Typography variant="caption" color="textSecondary">
                          Format: {option.postalCodeFormat}
                        </Typography>
                      )}
                    </Box>
                  </li>
                )}
              />
            </Grid>

            {/* Zip Lookup Results - Show when available */}
            <Grid item xs={12}>
              <Collapse in={showZipLookupResults && zipLookupResults.length > 0}>
                <Card variant="outlined" sx={{ p: 1, bgcolor: 'grey.50' }}>
                  <Typography variant="subtitle2" gutterBottom>
                    <SearchIcon fontSize="small" sx={{ verticalAlign: 'middle', mr: 0.5 }} />
                    Matching Locations ({zipLookupResults.length})
                  </Typography>
                  <Box sx={{ maxHeight: 150, overflowY: 'auto' }}>
                    {zipLookupResults.slice(0, 10).map((result, index) => (
                      <Chip
                        key={`${result.postalCode}-${result.city}-${index}`}
                        label={`${result.city}, ${result.state} ${result.postalCode}`}
                        size="small"
                        onClick={() => applyZipLookupResult(result)}
                        sx={{ m: 0.5, cursor: 'pointer' }}
                        variant="outlined"
                        color="primary"
                      />
                    ))}
                  </Box>
                  <Button size="small" onClick={() => setShowZipLookupResults(false)} sx={{ mt: 1 }}>
                    Dismiss
                  </Button>
                </Card>
              </Collapse>
            </Grid>

            {/* State Dropdown */}
            <Grid item xs={12} sm={6}>
              <Autocomplete
                options={states}
                getOptionLabel={(option) => typeof option === 'string' ? option : option.stateName}
                value={states.find(s => s.stateName === formData.state || s.stateCode === formData.state) || null}
                onChange={(_, value) => handleStateChange(typeof value === 'string' ? value : value?.stateName || '')}
                loading={loadingStates}
                disabled={!formData.countryCode}
                size="small"
                freeSolo
                onInputChange={(_, value, reason) => {
                  if (reason === 'input') {
                    setFormData({ ...formData, state: value });
                  }
                }}
                renderInput={(params) => (
                  <TextField
                    {...params}
                    label="State/Province"
                    required
                    helperText={states.length === 0 && formData.countryCode ? 'No states found - type manually' : ''}
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
              />
            </Grid>

            {/* City Dropdown */}
            <Grid item xs={12} sm={6}>
              <Autocomplete
                options={cities}
                getOptionLabel={(option) => option}
                value={cities.includes(formData.city) ? formData.city : null}
                onChange={(_, value) => handleCityChange(value || '')}
                loading={loadingCities}
                disabled={!formData.state}
                size="small"
                freeSolo
                onInputChange={(_, value, reason) => {
                  if (reason === 'input') {
                    setFormData({ ...formData, city: value });
                  }
                }}
                inputValue={formData.city}
                renderInput={(params) => (
                  <TextField
                    {...params}
                    label="City"
                    required
                    helperText={cities.length === 0 && formData.state ? 'No cities found - type manually' : ''}
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

            {/* Locality/Neighborhood Dropdown */}
            <Grid item xs={12} sm={6}>
              <Box display="flex" gap={1} alignItems="flex-start">
                <Autocomplete
                  options={localities}
                  getOptionLabel={(option) => typeof option === 'string' ? option : option.name}
                  value={localities.find(l => l.name === formData.locality || l.id === formData.localityId) || null}
                  onChange={(_, value) => {
                    if (typeof value === 'string') {
                      setFormData({ ...formData, locality: value, localityId: undefined });
                    } else if (value) {
                      setFormData({ ...formData, locality: value.name, localityId: value.id });
                    } else {
                      setFormData({ ...formData, locality: '', localityId: undefined });
                    }
                  }}
                  loading={loadingLocalities}
                  disabled={!formData.city}
                  size="small"
                  freeSolo
                  fullWidth
                  onInputChange={(_, value, reason) => {
                    if (reason === 'input') {
                      setFormData({ ...formData, locality: value, localityId: undefined });
                    }
                  }}
                  inputValue={formData.locality || ''}
                  renderInput={(params) => (
                    <TextField
                      {...params}
                      label="Locality / Neighborhood"
                      helperText={!formData.city ? 'Select city first' : localities.length === 0 ? 'Type or add new' : ''}
                      InputProps={{
                        ...params.InputProps,
                        endAdornment: (
                          <>
                            {loadingLocalities ? <CircularProgress size={16} /> : null}
                            {params.InputProps.endAdornment}
                          </>
                        ),
                      }}
                    />
                  )}
                  renderOption={(props, option) => (
                    <li {...props} key={option.id}>
                      <Box>
                        <Typography variant="body2">{option.name}</Typography>
                        {option.alternateName && (
                          <Typography variant="caption" color="textSecondary">
                            Also known as: {option.alternateName}
                          </Typography>
                        )}
                        {option.isUserCreated && (
                          <Chip label="Custom" size="small" sx={{ ml: 1 }} />
                        )}
                      </Box>
                    </li>
                  )}
                />
                <Tooltip title="Add new locality">
                  <IconButton
                    size="small"
                    onClick={() => setShowAddLocality(true)}
                    disabled={!formData.city}
                  >
                    <AddIcon fontSize="small" />
                  </IconButton>
                </Tooltip>
              </Box>
            </Grid>

            {/* Add Locality Dialog */}
            <Grid item xs={12}>
              <Collapse in={showAddLocality}>
                <Card variant="outlined" sx={{ p: 2, bgcolor: 'grey.50' }}>
                  <Typography variant="subtitle2" gutterBottom>
                    Add New Locality / Neighborhood
                  </Typography>
                  <Box display="flex" gap={1} alignItems="center">
                    <TextField
                      size="small"
                      label="Locality Name"
                      value={newLocalityName}
                      onChange={(e) => setNewLocalityName(e.target.value)}
                      placeholder="e.g., Downtown, Westside, etc."
                      fullWidth
                    />
                    <Button
                      variant="contained"
                      size="small"
                      onClick={handleCreateLocality}
                      disabled={!newLocalityName.trim()}
                    >
                      Add
                    </Button>
                    <Button
                      size="small"
                      onClick={() => {
                        setShowAddLocality(false);
                        setNewLocalityName('');
                      }}
                    >
                      Cancel
                    </Button>
                  </Box>
                </Card>
              </Collapse>
            </Grid>

            {/* Address Lines */}
            <Grid item xs={12}>
              <TextField
                fullWidth
                label="Address Line 1"
                value={formData.line1}
                onChange={(e) => setFormData({ ...formData, line1: e.target.value })}
                required
                size="small"
                placeholder="Street address, P.O. box, company name"
              />
            </Grid>
            <Grid item xs={12}>
              <TextField
                fullWidth
                label="Address Line 2"
                value={formData.line2}
                onChange={(e) => setFormData({ ...formData, line2: e.target.value })}
                size="small"
                placeholder="Apartment, suite, unit, building, floor, etc."
              />
            </Grid>
            <Grid item xs={12}>
              <TextField
                fullWidth
                label="Address Line 3"
                value={formData.line3}
                onChange={(e) => setFormData({ ...formData, line3: e.target.value })}
                size="small"
              />
            </Grid>

            {/* County/Additional Fields */}
            <Grid item xs={12} sm={6}>
              <TextField
                fullWidth
                label="County/District"
                value={formData.county}
                onChange={(e) => setFormData({ ...formData, county: e.target.value })}
                size="small"
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <FormControlLabel
                control={
                  <Checkbox
                    checked={formData.isResidential || false}
                    onChange={(e) => setFormData({ ...formData, isResidential: e.target.checked })}
                  />
                }
                label="Residential Address"
              />
            </Grid>

            <Grid item xs={12}>
              <TextField
                fullWidth
                label="Delivery Instructions"
                value={formData.deliveryInstructions}
                onChange={(e) => setFormData({ ...formData, deliveryInstructions: e.target.value })}
                multiline
                rows={2}
                size="small"
              />
            </Grid>

            <Grid item xs={12} sm={6}>
              <TextField
                fullWidth
                label="Access Hours"
                value={formData.accessHours}
                onChange={(e) => setFormData({ ...formData, accessHours: e.target.value })}
                placeholder="e.g., 9AM-5PM Mon-Fri"
                size="small"
              />
            </Grid>

            <Grid item xs={12} sm={6}>
              <TextField
                fullWidth
                label="Site Contact Name"
                value={formData.siteContactName}
                onChange={(e) => setFormData({ ...formData, siteContactName: e.target.value })}
                size="small"
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField
                fullWidth
                label="Site Contact Phone"
                value={formData.siteContactPhone}
                onChange={(e) => setFormData({ ...formData, siteContactPhone: e.target.value })}
                size="small"
              />
            </Grid>

            <Grid item xs={12}>
              <TextField
                fullWidth
                label="Notes"
                value={formData.notes}
                onChange={(e) => setFormData({ ...formData, notes: e.target.value })}
                multiline
                rows={2}
                size="small"
              />
            </Grid>
          </Grid>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCloseDialog}>Cancel</Button>
          <Button onClick={handleSave} variant="contained" color="primary">
            {editingAddress ? 'Save Changes' : 'Add Address'}
          </Button>
        </DialogActions>
      </Dialog>
    </Card>
  );
};

export default AddressManager;
