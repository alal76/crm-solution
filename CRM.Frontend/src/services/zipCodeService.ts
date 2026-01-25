import apiClient from './apiClient';

// Types for Zip Code API responses
export interface CountryInfo {
  countryCode: string;
  countryName: string;
  postalCodeFormat: string | null;
}

export interface StateInfo {
  stateCode: string;
  stateName: string;
}

export interface ZipCodeLookupResult {
  id?: number;
  postalCode: string;
  city: string;
  state: string;
  stateCode: string;
  county: string;
  country: string;
  countryCode: string;
  latitude: number;
  longitude: number;
}

export interface ZipCodeValidationResult {
  isValid: boolean;
  isFormatValid: boolean;
  existsInDatabase: boolean;
  message: string;
  expectedFormat: string | null;
}

export interface LocalityInfo {
  id: number;
  name: string;
  alternateName?: string;
  localityType: string;
  city: string;
  stateCode: string;
  countryCode: string;
  zipCodeId?: number;
  latitude?: number;
  longitude?: number;
  isUserCreated: boolean;
}

export interface CreateLocalityRequest {
  name: string;
  alternateName?: string;
  localityType?: string;
  zipCodeId?: number;
  city: string;
  stateCode: string;
  countryCode: string;
}

const zipCodeService = {
  /**
   * Get all available countries with postal/zip code formats
   */
  getCountries: async (): Promise<CountryInfo[]> => {
    const response = await apiClient.get('/zipcodes/countries');
    return response.data;
  },

  /**
   * Get states/provinces for a country
   */
  getStates: async (countryCode: string): Promise<StateInfo[]> => {
    const response = await apiClient.get(`/zipcodes/states/${countryCode}`);
    return response.data;
  },

  /**
   * Get cities in a state
   */
  getCities: async (countryCode: string, stateCode: string): Promise<string[]> => {
    const response = await apiClient.get(`/zipcodes/cities/${countryCode}/${stateCode}`);
    return response.data;
  },

  /**
   * Get postal/zip codes for a specific city
   */
  getPostalCodes: async (countryCode: string, stateCode: string, city: string): Promise<ZipCodeLookupResult[]> => {
    const response = await apiClient.get(`/zipcodes/postalcodes/${countryCode}/${stateCode}/${encodeURIComponent(city)}`);
    return response.data;
  },

  /**
   * Lookup address information by postal/zip code
   */
  lookupByPostalCode: async (postalCode: string, countryCode?: string): Promise<ZipCodeLookupResult[]> => {
    const params = countryCode ? { countryCode } : {};
    const response = await apiClient.get(`/zipcodes/lookup/${encodeURIComponent(postalCode)}`, { params });
    return response.data;
  },

  /**
   * Search for cities by name
   */
  searchByCity: async (city: string, countryCode?: string, limit: number = 20): Promise<ZipCodeLookupResult[]> => {
    const params: Record<string, string | number> = { city, limit };
    if (countryCode) params.countryCode = countryCode;
    const response = await apiClient.get('/zipcodes/search/city', { params });
    return response.data;
  },

  /**
   * Validate a postal/zip code for a country
   */
  validatePostalCode: async (postalCode: string, countryCode: string): Promise<ZipCodeValidationResult> => {
    const response = await apiClient.get('/zipcodes/validate', { 
      params: { postalCode, countryCode } 
    });
    return response.data;
  },

  /**
   * Get localities (neighborhoods/subdivisions) for a specific postal/zip code
   */
  getLocalities: async (zipCodeId: number): Promise<LocalityInfo[]> => {
    const response = await apiClient.get(`/zipcodes/localities/${zipCodeId}`);
    return response.data;
  },

  /**
   * Get localities by city name
   */
  getLocalitiesByCity: async (city: string, countryCode?: string): Promise<LocalityInfo[]> => {
    const params: Record<string, string> = { city };
    if (countryCode) params.countryCode = countryCode;
    const response = await apiClient.get('/zipcodes/localities/city', { params });
    return response.data;
  },

  /**
   * Create a new locality (for user-defined neighborhoods not in master data)
   */
  createLocality: async (request: CreateLocalityRequest): Promise<LocalityInfo> => {
    const response = await apiClient.post('/zipcodes/localities', request);
    return response.data;
  },
};

export default zipCodeService;
