# Database Configuration Management

This document describes the database-as-code infrastructure for the CRM solution.

## Overview

The database configuration is now part of the solution and includes:
- **Schema files** - Table definitions for all entities
- **Seed data** - Initial data for system settings, color palettes, navigation, and service request types
- **Master data** - ZIP code data for address auto-population

## Directory Structure

```
/database/
├── schema/                           # Database schema files
│   ├── 001_core_tables.sql          # Users, Customers, Contacts, Addresses
│   ├── 002_master_data_tables.sql   # ZipCodes, ColorPalettes, SystemSettings
│   ├── 003_service_request_tables.sql
│   ├── 004_products_opportunities.sql
│   ├── 005_workflow_tables.sql
│   └── 006_activities_communication.sql
├── seed/                             # Seed data files
│   ├── 001_color_palettes.sql       # 30 color palettes in 6 categories
│   ├── 002_module_ui_configs.sql    # Module navigation settings
│   ├── 003_system_settings.sql      # System configuration
│   └── 004_service_request_types.sql # 93 service request types
├── master_data/                      # Master data files
│   └── 001_zipcode_data_sample.sql  # 130 sample US ZIP codes
├── deploy.sh                         # Deployment script
└── README.md                         # Detailed documentation
```

## Seed Data Included

### Color Palettes (30 palettes)
Six categories of professionally designed color themes:
- **Professional** - Corporate Blue, Executive Gray, Modern Slate, Business Navy, Classic Black
- **Nature** - Forest Green, Ocean Breeze, Sunset Orange, Lavender Fields, Earth Tones
- **Vibrant** - Electric Purple, Neon Pink, Citrus Yellow, Cyber Blue, Hot Coral
- **Warm** - Autumn Gold, Rustic Red, Terracotta, Cream & Brown, Sunset Blend
- **Cool** - Arctic Blue, Mint Fresh, Silver Lining, Twilight Purple, Steel Gray
- **Dark** - Midnight Black, Dark Charcoal, Deep Navy, Obsidian, Shadow Gray

### Module UI Configurations (15 modules)
All CRM modules with navigation enabled:
- Dashboard, Customers, Contacts, Leads
- Opportunities, Service Requests, Products
- Reports, Calendar, Settings, User Management
- Departments, User Groups, Activity Logs, Email Templates

### System Settings
- All modules enabled by default
- Full navigation access
- Default field configurations

### Service Request Types (93 types)
Organized in 8 categories with 28 subcategories:
- Technical Support, Billing, Account Management
- Sales, Human Resources, Facilities, Legal, Other

## ZipCode Master Data & API

### Database Table
The `ZipCodes` table stores postal code data for address auto-population:
- PostalCode, City, State, StateCode
- County, Country, CountryCode
- Latitude, Longitude (for mapping)

### API Endpoints

| Endpoint | Description |
|----------|-------------|
| `GET /api/zipcodes/lookup/{postalCode}` | Lookup address by ZIP code |
| `GET /api/zipcodes/search/city?city=name` | Search cities by name |
| `GET /api/zipcodes/states/{countryCode}` | Get all states for a country |
| `GET /api/zipcodes/cities/{countryCode}/{stateCode}` | Get cities in a state |
| `GET /api/zipcodes/count` | Get total ZIP code count (requires auth) |

### Usage Example
```javascript
// When user enters ZIP code in address form
const response = await fetch('/api/zipcodes/lookup/10001');
const data = await response.json();
// Returns: [{ postalCode: "10001", city: "New York", state: "New York", stateCode: "NY", ... }]

// Auto-fill address fields
cityInput.value = data[0].city;
stateInput.value = data[0].stateCode;
countyInput.value = data[0].county;
```

## Deployment

### Quick Deploy
```bash
cd /database
./deploy.sh
```

### Options
```bash
./deploy.sh --schema-only      # Only create tables
./deploy.sh --seed-only        # Only insert seed data
./deploy.sh --master-data-only # Only load ZIP codes
./deploy.sh --prod-only        # Only deploy to production
./deploy.sh --demo-only        # Only deploy to demo
```

### Manual Deploy in Kubernetes
```bash
# Get database pod
DB_POD=$(kubectl get pods -n crm-app -l app=crm-db -o jsonpath='{.items[0].metadata.name}')

# Apply schema
cat database/schema/*.sql | kubectl exec -i $DB_POD -n crm-app -- mysql -u root -p'RootPass@Dev2024' crm_db

# Apply seed data
cat database/seed/*.sql | kubectl exec -i $DB_POD -n crm-app -- mysql -u root -p'RootPass@Dev2024' crm_db

# Load master data
cat database/master_data/*.sql | kubectl exec -i $DB_POD -n crm-app -- mysql -u root -p'RootPass@Dev2024' crm_db
```

## Current Data Counts

| Table | Records |
|-------|---------|
| ZipCodes | 130 (sample) |
| ColorPalettes | 30 |
| ModuleUIConfigs | 14 |
| ServiceRequestTypes | 93 |
| ServiceRequestCategories | 8 |
| ServiceRequestSubcategories | 28 |

## Expanding ZIP Code Data

The current dataset contains 130 sample US ZIP codes for major cities. To load the full ZIP code database:

1. Download from: https://github.com/Zeeshanahmad4/Zip-code-of-all-countries-cities-in-the-world-CSV-TXT-SQL-DATABASE
2. Convert to SQL insert format matching the ZipCodes table schema
3. Place in `/database/master_data/`
4. Run: `./deploy.sh --master-data-only`

## Backend Version

- **Image**: crm-backend:v13
- **Includes**: ZipCode entity, service, and API controller

## Related Documentation

- [database/README.md](database/README.md) - Detailed database configuration guide
- [DEPLOYMENT_README.md](DEPLOYMENT_README.md) - Kubernetes deployment guide
- [ADMIN_SETTINGS_GUIDE.md](ADMIN_SETTINGS_GUIDE.md) - Admin settings documentation
