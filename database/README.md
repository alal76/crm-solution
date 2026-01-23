# CRM Solution Database Configuration

This directory contains the complete database schema and seed data for the CRM Solution. The database structure is maintained as part of the solution configuration, allowing for consistent deployments across environments.

## Directory Structure

```
database/
├── schema/                        # Database schema files (run in order)
│   ├── 001_core_tables.sql        # Users, UserGroups, Customers, Contacts
│   ├── 002_master_data_tables.sql # ZipCodes, ColorPalettes, SystemSettings
│   ├── 003_service_request_tables.sql # Service request management
│   ├── 004_products_opportunities.sql # Products, Opportunities, Quotes
│   ├── 005_workflow_tables.sql    # Workflow automation
│   └── 006_activities_communication.sql # Activities, Notes, Emails
├── seed/                          # Seed data files
│   ├── 001_color_palettes.sql     # System color palettes (30 palettes)
│   ├── 002_module_ui_configs.sql  # Module navigation config (15 modules)
│   ├── 003_system_settings.sql    # Default system settings
│   └── 004_service_request_types.sql # Service request categories/types
├── master_data/                   # Master data (large datasets)
│   └── zipcode_data.sql           # ZIP/postal code data
├── deploy.sh                      # Main deployment script
└── README.md                      # This file
```

## Deployment

### Quick Deploy

Deploy to both production and demo databases:

```bash
./deploy.sh
```

### Selective Deployment

```bash
# Schema only
./deploy.sh --schema-only

# Seed data only
./deploy.sh --seed-only

# Master data only (ZIP codes)
./deploy.sh --master-data-only

# Production database only
./deploy.sh --prod-only

# Demo database only
./deploy.sh --demo-only
```

### Environment Variables

| Variable | Default | Description |
|----------|---------|-------------|
| `DB_HOST` | localhost | Database host |
| `DB_PORT` | 3306 | Database port |
| `DB_ROOT_USER` | root | Root user for admin operations |
| `DB_ROOT_PASS` | - | Root password |
| `DB_USER` | crm_user | Application database user |
| `DB_USER_PASS` | - | Application user password |
| `PROD_DB` | crm_db | Production database name |
| `DEMO_DB` | crm_demodb | Demo database name |

### Kubernetes Deployment

For Kubernetes deployments, port-forward to the database pod first:

```bash
# Get the database pod name
kubectl get pods -n crm-app | grep crm-db

# Port-forward
kubectl port-forward -n crm-app <pod-name> 3306:3306

# Then run the deploy script
DB_HOST=localhost DB_PORT=3306 ./deploy.sh
```

## Schema Files

### 001_core_tables.sql
Core CRM entity tables:
- `Users` - User authentication and profile
- `UserGroups` - Permission groups with 50+ permission columns
- `UserGroupMembers` - User-group membership
- `Departments` - Department hierarchy
- `UserProfiles` - Extended user profiles
- `Customers` - Customer/account management
- `Contacts` - Contact information
- `CustomerContacts` - Customer-contact relationships
- `Addresses` - Address information

### 002_master_data_tables.sql
Master data and configuration tables:
- `ZipCodes` - Postal code database for address auto-population
- `ColorPalettes` - UI color theme palettes
- `SystemSettings` - System configuration
- `ModuleUIConfigs` - Module navigation configuration
- `ModuleFieldConfigurations` - Field-level configuration
- `LookupCategories` - Lookup/dropdown categories
- `LookupItems` - Lookup values

### 003_service_request_tables.sql
Service request/ticket management:
- `ServiceRequestCategories` - Request categories
- `ServiceRequestSubcategories` - Subcategories
- `ServiceRequestTypes` - Specific request types with workflows
- `ServiceRequests` - The actual tickets
- `ServiceRequestCustomFieldDefinitions` - Custom field definitions
- `ServiceRequestCustomFieldValues` - Custom field values

### 004_products_opportunities.sql
Sales pipeline:
- `Products` - Product catalog
- `Opportunities` - Sales opportunities
- `OpportunityProducts` - Products in opportunities
- `Quotes` - Quotations
- `QuoteLineItems` - Quote line items

### 005_workflow_tables.sql
Workflow automation:
- `Workflows` - Workflow definitions
- `WorkflowSteps` - Individual workflow steps
- `WorkflowInstances` - Running workflow instances
- `WorkflowStepExecutions` - Step execution history
- `WorkflowTriggers` - Workflow triggers

### 006_activities_communication.sql
Activities and communication:
- `Activities` - Tasks, calls, meetings
- `Notes` - Notes on entities
- `EmailTemplates` - Email templates
- `EmailLogs` - Sent email history
- `Attachments` - File attachments
- `AuditLogs` - Change audit trail

## Seed Data

### Color Palettes (001_color_palettes.sql)
30 pre-configured color palettes in categories:
- **Professional** (5 palettes): Material Purple, Ocean Blue, Corporate Teal, etc.
- **Nature** (5 palettes): Forest Green, Autumn Harvest, Desert Sand, etc.
- **Vibrant** (5 palettes): Sunset Orange, Electric Blue, Hot Pink, etc.
- **Warm** (5 palettes): Terracotta, Golden Amber, Rose Garden, etc.
- **Cool** (5 palettes): Arctic Blue, Midnight Navy, Mint Fresh, etc.
- **Dark** (5 palettes): Dark Material, Dark Ocean, Dark Forest, etc.

### Module UI Configs (002_module_ui_configs.sql)
15 fully configured modules:
1. Dashboard
2. Customers
3. Contacts
4. Leads
5. Opportunities
6. Products
7. Services
8. Campaigns
9. Quotes
10. Tasks
11. Activities
12. Notes
13. Workflows
14. Reports
15. ServiceRequests

### System Settings (003_system_settings.sql)
Default system settings with:
- All modules enabled
- Material Purple default theme
- Security defaults (8 char min password, 60 min session timeout)
- Localization (US date format, USD currency, America/New_York timezone)
- Full navigation configuration

### Service Request Types (004_service_request_types.sql)
93 service request types across:
- 8 categories
- 28 subcategories
- Complete workflow names and resolution options

## Master Data

### ZIP Code Data
The `master_data/zipcode_data.sql` file contains postal code data for address auto-population.

**Table structure:**
```sql
CREATE TABLE ZipCodes (
  PostalCode VARCHAR(20),
  City VARCHAR(200),
  State VARCHAR(100),
  StateCode VARCHAR(10),
  County VARCHAR(100),
  Country VARCHAR(100),
  CountryCode VARCHAR(10),
  Latitude DECIMAL(10,6),
  Longitude DECIMAL(10,6)
);
```

**API Endpoints:**
- `GET /api/zipcodes/lookup/{postalCode}` - Lookup by postal code
- `GET /api/zipcodes/search/city?city=name` - Search by city name
- `GET /api/zipcodes/states/{countryCode}` - Get states for country
- `GET /api/zipcodes/cities/{countryCode}/{stateCode}` - Get cities in state

## Adding New Tables

1. Create a new schema file in `schema/` with the next number prefix
2. Follow the existing pattern for table creation
3. Include proper indexes and foreign keys
4. Add seed data in `seed/` if needed
5. Test on a local database before deploying

## Migrations

For existing databases that need updates:
1. Create a migration file in `CRM.Backend/migrations/`
2. Follow the naming convention: `NNN_description.sql`
3. The application will apply migrations automatically on startup

## Data Sources

### ZIP Code Data Source
Data structure based on [Zeeshanahmad4/Zip-code-of-all-countries](https://github.com/Zeeshanahmad4/Zip-code-of-all-countries-cities-in-the-world-CSV-TXT-SQL-DATABASE):
- Headers: COUNTRY, POSTAL_CODE, CITY, STATE, SHORT_STATE, COUNTY, LATITUDE, LONGITUDE, ACCURACY
