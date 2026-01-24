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

const zipCodeService = {
  /**
   * Get all available countries with postal code formats
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
   * Get postal codes for a specific city
   */
  getPostalCodes: async (countryCode: string, stateCode: string, city: string): Promise<ZipCodeLookupResult[]> => {
    const response = await apiClient.get(`/zipcodes/postalcodes/${countryCode}/${stateCode}/${encodeURIComponent(city)}`);
    return response.data;
  },

  /**
   * Lookup address information by postal code
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
   * Validate a postal code for a country
   */
  validatePostalCode: async (postalCode: string, countryCode: string): Promise<ZipCodeValidationResult> => {
    const response = await apiClient.get('/zipcodes/validate', { 
      params: { postalCode, countryCode } 
    });
    return response.data;
  },
};

export default zipCodeService;
