# CRM Data Loader

API-based data seeding tool for the CRM Solution. This script loads data via the REST API endpoints, making it suitable for production deployments where direct database access isn't available.

## Data Levels

The loader supports three levels of data:

### 1. Essential (Required for system operation)
- User groups and roles
- Departments
- Lookup values (currencies, industries, lead sources)
- Contact roles and categories
- **Without this data, the system cannot function properly**

### 2. Basic (Sample entries)
Includes Essential data plus:
- 3 sample users
- 3 sample customers (individual, small business, enterprise)
- 4 sample products (license, subscription, usage-based, services)
- 3 sample leads (different stages)
- 3 sample opportunities (different stages)
- 3 sample campaigns (email, event, webinar)

### 3. Demo (Full demonstration data)
Includes Essential + Basic plus:
- 20+ customers with variety
- 40+ contacts across customers
- 15+ leads in various stages
- 10+ opportunities across all pipeline stages
- 10+ service requests
- 15+ tasks
- All entities have 3+ variations of each type/status/category

## Usage

### Quick Start
```bash
# Load demo data (default)
./run-loader.sh demo

# Load only essential data
./run-loader.sh essential

# Load basic sample data
./run-loader.sh basic
```

### Direct Python Usage
```bash
# Load demo data
python3 load_data.py demo

# Load with custom API URL
python3 load_data.py --api-base=http://localhost:5000/api demo

# Load with custom credentials
python3 load_data.py --username=admin --password=MyPassword123 basic

# Dry run (show what would be created)
python3 load_data.py --dry-run demo
```

### Environment Variables
```bash
export API_BASE=http://localhost:5000/api
export ADMIN_USER=admin
export ADMIN_PASS=Admin@123
./run-loader.sh demo
```

## Prerequisites

1. **Python 3.6+** - Uses only standard library (no pip install required)
2. **Running CRM API** - The API must be accessible
3. **Admin credentials** - Valid admin user for authentication

## API Endpoints Used

The loader uses these API endpoints:

| Entity | Endpoint | Methods |
|--------|----------|---------|
| Auth | `/api/auth/login` | POST |
| Departments | `/api/departments` | POST |
| Lookups | `/api/lookups` | POST |
| Users | `/api/users` | POST |
| Customers | `/api/customers` | POST |
| Contacts | `/api/contacts` | POST |
| Products | `/api/products` | POST |
| Leads | `/api/leads` | POST |
| Opportunities | `/api/opportunities` | POST |
| Campaigns | `/api/campaigns` | POST |
| Service Requests | `/api/service-requests` | POST |
| Tasks | `/api/tasks` | POST |

## Data Details

### Essential Data Created

**Departments:**
- Sales, Marketing, Customer Support, Engineering, Operations

**Lookup Values:**

| Category | Values |
|----------|--------|
| Currency | USD, EUR, GBP, CAD, AUD |
| LeadSource | Website, Referral, Trade Show, Advertisement, Social Media, Email Campaign |
| Industry | Technology, Finance, Healthcare, Manufacturing, Retail, Education |
| ContactRole | Decision Maker, Influencer, Technical Evaluator, End User, Other |

### Basic Sample Data

**Customers (3 types):**
1. Individual - Alice Brown
2. Small Business - Acme Solutions LLC
3. Enterprise - Global Industries Inc

**Products (4 types):**
1. One-time purchase - CRM Professional License
2. Subscription - CRM Cloud Monthly
3. Usage-based - API Calls Package
4. Service - Implementation Services

**Leads (3 stages):**
1. New lead (score 45)
2. Qualified lead (score 78)
3. Hot lead (score 92)

**Opportunities (3 stages):**
1. Discovery stage (10% probability)
2. Proposal stage (50% probability)
3. Negotiation stage (75% probability)

## Troubleshooting

### Authentication Failed
```
❌ Authentication failed. Please check credentials.
```
- Verify the admin user exists
- Check username/password are correct
- Ensure the API is running

### Connection Error
```
❌ Connection Error: [Errno 111] Connection refused
```
- Verify the API_BASE URL is correct
- Check the API server is running
- Verify network connectivity

### API Error 401
- Token may have expired
- User may lack permissions

### API Error 400
- Data validation failed
- Check the API logs for details

## Extending the Loader

To add new entity types:

1. Add a new function in `load_data.py`:
```python
def load_custom_entity(api: APIClient):
    data = [
        {"field1": "value1", "field2": "value2"},
    ]
    for item in data:
        result = api.post("/custom-entities", item)
        api.track_created("customentity", result.get("id", 0))
```

2. Call the function in the appropriate data level function.

## License

Copyright (C) 2024-2026 Abhishek Lal  
Source-available — Commercial use requires a license. See LICENSE file.
