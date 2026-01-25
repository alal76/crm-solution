# Dynamic Address UI Feature

## Overview

The Address Manager component has been enhanced to provide dynamic address selection using the integrated ZIP code database containing 2.16+ million postal codes across 121 countries.

## Features

### 1. Bidirectional Address Entry

The address form now supports two methods of entry:

#### Method 1: ZIP/Postal Code First (Quick Entry)
1. Enter a postal/ZIP code in the **Postal/ZIP Code** field
2. Matching locations appear as clickable chips
3. Click a location to auto-populate Country, State, City, and County fields

#### Method 2: Cascading Dropdowns (Traditional)
1. Select **Country** from the dropdown (shows postal code format for each country)
2. Select **State/Province** from the filtered dropdown
3. Select **City** from the filtered dropdown
4. Optionally select from available **Postal Codes** for that city

### 2. Real-Time Postal Code Validation

As you type a postal code, the system validates:
- **Format validation**: Checks if the format matches the expected pattern for the selected country
- **Database validation**: Confirms the postal code exists in the database

Visual indicators:
- ✓ Green checkmark: Valid format and exists in database
- ⚠ Warning icon: Valid format but not found in database (may be new or missing)
- ✗ Error icon: Invalid format for the selected country

### 3. Postal Code Format Support

The system includes format validation for 35+ countries:

| Country | Format Example | Description |
|---------|---------------|-------------|
| United States | 12345 or 12345-6789 | 5 digits or ZIP+4 |
| Canada | A1A 1A1 | Letter-Number alternating |
| United Kingdom | AA9A 9AA | Alphanumeric (various) |
| Germany | 12345 | 5 digits |
| France | 12345 | 5 digits |
| Japan | 123-4567 | 3 digits, hyphen, 4 digits |
| India | 123456 | 6 digits |
| Australia | 1234 | 4 digits |
| Brazil | 12345-678 | 5 digits, hyphen, 3 digits |
| And 26+ more countries... |

## API Endpoints

### New Endpoints Added

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/api/zipcodes/countries` | GET | Get all countries with postal code formats |
| `/api/zipcodes/postalcodes/{countryCode}/{stateCode}/{city}` | GET | Get postal codes for a city |
| `/api/zipcodes/validate` | GET | Validate postal code format and existence |

### Existing Endpoints

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/api/zipcodes/lookup/{postalCode}` | GET | Lookup by postal code |
| `/api/zipcodes/states/{countryCode}` | GET | Get states for a country |
| `/api/zipcodes/cities/{countryCode}/{stateCode}` | GET | Get cities in a state |
| `/api/zipcodes/search/city` | GET | Search cities by name |

## Database Coverage

The ZIP code database includes:

- **Total Records**: 2,160,606 postal codes
- **Countries**: 121 countries
- **Top Countries by Coverage**:
  - United Arab Emirates: 356,000+ records
  - Portugal: 207,000+ records
  - India: 156,000+ records
  - Japan: 147,000+ records
  - Mexico: 145,000+ records
  - United States: Comprehensive coverage

## Technical Implementation

### Backend Changes

1. **IZipCodeService.cs** - Added new interface methods:
   - `GetCountriesAsync()` - Returns all countries with postal code formats
   - `GetPostalCodesForCityAsync()` - Returns postal codes for a specific city
   - `ValidatePostalCodeAsync()` - Validates format and database existence

2. **ZipCodeService.cs** - Added implementation:
   - `PostalCodeFormats` dictionary with regex patterns for 35+ countries
   - Format validation using country-specific regex
   - Database lookup for existence check

3. **ZipCodesController.cs** - Added new endpoints:
   - `GET /countries` - List all countries
   - `GET /postalcodes/{countryCode}/{stateCode}/{city}` - Get postal codes for city
   - `GET /validate` - Validate a postal code

### Frontend Changes

1. **zipCodeService.ts** - New service for ZIP code API calls:
   - TypeScript interfaces for API responses
   - Methods for all ZIP code endpoints

2. **AddressManager.tsx** - Enhanced component:
   - Dynamic Autocomplete dropdowns for Country, State, City
   - Postal code quick-lookup with auto-population
   - Real-time format validation with visual feedback
   - Debounced API calls for performance
   - FreeSolo mode allowing manual entry when database doesn't have data

## Usage Example

```tsx
import AddressManager from './components/ContactInfo/AddressManager';

// In your component:
<AddressManager
  entityType="Contact"
  entityId={contactId}
  onAddressChange={() => refetchData()}
/>
```

## User Interface

The address dialog now shows:
1. **Quick Entry tip** explaining both entry methods
2. **Postal Code field** with validation indicators
3. **Country dropdown** with postal code format hints
4. **State dropdown** (filtered by country, freeSolo for manual entry)
5. **City dropdown** (filtered by state, freeSolo for manual entry)
6. **Matching locations chips** when entering ZIP code first

## Files Modified

- `CRM.Backend/src/CRM.Core/Interfaces/IZipCodeService.cs`
- `CRM.Backend/src/CRM.Infrastructure/Services/ZipCodeService.cs`
- `CRM.Backend/src/CRM.Api/Controllers/ZipCodesController.cs`
- `CRM.Frontend/src/services/zipCodeService.ts` (new)
- `CRM.Frontend/src/components/ContactInfo/AddressManager.tsx`

## Deployment

The feature was deployed to Kubernetes as:
- Backend: `crm-backend:v28`
- Frontend: `crm-frontend:v28`
