# CRM Database Seed Data Sets

This folder contains organized SQL seed data files for the CRM Solution database.

## Data Set Structure

### 1. Essential Data (`01_essential_data.sql`)
**Required for system operation** - the system will not function properly without this data.

Contains:
- System settings and configuration
- Departments (Sales, Marketing, Support, Engineering, Operations, Finance, HR, Executive)
- User groups with full permission matrix (8 roles)
- Lookup categories (23 categories)
- Lookup items (100+ values for dropdowns)
- Default admin user

**When to use:** Always run first. Required for any deployment.

### 2. Basic Data (`02_basic_data.sql`)
**Sample entries for testing** - minimal but complete data set.

Contains:
- 3 sample users (Sales Manager, Sales Rep, Support Agent)
- 3 customers (Individual, Small Business, Enterprise)
- 5 contacts across business customers
- 6 products (license, subscriptions, usage-based, services, support)
- 3 leads (New, Qualified, Hot)
- 3 opportunities (Discovery, Proposal, Negotiation)
- 3 campaigns (Email, Event, Webinar)
- 2 accounts (billing records)
- 3 tasks
- 3 service requests

**When to use:** Development/testing environments needing sample data.

### 3. Demo Data (`03_demo_data.sql`)
**Full demonstration data** - comprehensive data set for demos and training.

Contains:
- 10 users across all departments
- 20 customers (various industries, sizes, regions including international)
- 30 contacts
- 16 products (all categories and types)
- 15 leads (all stages and sources)
- 17 opportunities (all stages including won/lost)
- 12 campaigns (all types and statuses)
- 8 accounts
- 13 tasks (various priorities and statuses)
- 13 service requests (all types and statuses)

**When to use:** Demo environments, training, sales presentations.

## Usage

### PostgreSQL (psql)
```bash
# Essential only
psql -h localhost -U crm_user -d crm_db -f 01_essential_data.sql

# Essential + Basic
psql -h localhost -U crm_user -d crm_db -f 01_essential_data.sql
psql -h localhost -U crm_user -d crm_db -f 02_basic_data.sql

# Essential + Basic + Demo
psql -h localhost -U crm_user -d crm_db -f 01_essential_data.sql
psql -h localhost -U crm_user -d crm_db -f 02_basic_data.sql
psql -h localhost -U crm_user -d crm_db -f 03_demo_data.sql
```

### Using the Shell Script
```bash
./load-data-sets.sh essential   # Essential only
./load-data-sets.sh basic       # Essential + Basic
./load-data-sets.sh demo        # Essential + Basic + Demo
```

### Using Docker
```bash
# With docker-compose
docker-compose exec postgres psql -U crm_user -d crm_db -f /seed/01_essential_data.sql
```

## Default Credentials

| Username | Password | Role |
|----------|----------|------|
| admin | Admin@123 | System Administrator |
| jsmith | Admin@123 | Sales Manager |
| mjohnson | Admin@123 | Sales Representative |
| rwilliams | Admin@123 | Support Agent |

**Note:** All users have the same default password for testing. In production, passwords should be changed immediately.

## Data Dependencies

```
01_essential_data.sql
   └── 02_basic_data.sql
         └── 03_demo_data.sql
```

Each file depends on the previous one. Always load in order.

## Customization

To modify seed data:

1. **Add new lookup values:** Edit `01_essential_data.sql` in the LookupItems section
2. **Add sample entities:** Edit `02_basic_data.sql` or `03_demo_data.sql`
3. **Add new roles:** Edit `01_essential_data.sql` in the UserGroups section

## Schema Assumptions

These scripts assume the following table names (PostgreSQL with EF Core conventions):
- `"SystemSettings"`, `"Departments"`, `"UserGroups"`, `"LookupCategories"`, `"LookupItems"`
- `"Users"`, `"Customers"`, `"Contacts"`, `"Products"`, `"Leads"`, `"Opportunities"`
- `"Campaigns"`, `"Accounts"`, `"Tasks"`, `"ServiceRequests"`

Adjust table/column names if your schema differs.

## License

Copyright (C) 2024-2026 Abhishek Lal  
Source-available — Commercial use requires a license. See LICENSE file.
