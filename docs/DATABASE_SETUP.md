# CRM Database Configuration Guide

**Version:** 0.0.25  
**Last Updated:** January 2025

---

## Overview

The CRM Solution supports multiple database providers. **MariaDB** is the default and recommended option for production deployments.

| Provider | Recommended For |
|----------|-----------------|
| **MariaDB** | Production (default) |
| **PostgreSQL** | Production (alternative) |
| **SQL Server** | Enterprise Windows environments |

---

## Current Production Configuration

### Server: 192.168.0.9

```bash
# Database connection details
Host: 192.168.0.9
Port: 3306
Database: crm_db
User: crm_user
Password: crm_password
```

### Table Count: 89 Tables

The database contains tables organized into domains:

| Domain | Tables |
|--------|--------|
| Customer | Customers, Contacts, Accounts, CustomerContacts |
| Contact Info | EmailAddresses, PhoneNumbers, Addresses, SocialMediaLinks |
| Sales | Opportunities, Products, Quotes, QuoteLineItems |
| Marketing | MarketingCampaigns, Leads, CampaignRecipients |
| Service Desk | ServiceRequests, Categories, Subcategories |
| Workflow | WorkflowDefinitions, WorkflowInstances, WorkflowTasks |
| System | Users, UserGroups, SystemSettings |

---

## MariaDB Setup (Recommended)

### Docker (Fastest)

```bash
# Run MariaDB container
docker run -d \
  --name crm-mariadb \
  -e MYSQL_ROOT_PASSWORD=root \
  -e MYSQL_DATABASE=crm_db \
  -e MYSQL_USER=crm_user \
  -e MYSQL_PASSWORD=crm_password \
  -p 3306:3306 \
  mariadb:10.11

# Verify container
docker logs crm-mariadb
```

### Local Installation

**macOS:**
```bash
brew install mariadb
brew services start mariadb
mysql_secure_installation
```

**Linux (Ubuntu/Debian):**
```bash
sudo apt update
sudo apt install mariadb-server
sudo systemctl start mariadb
sudo mysql_secure_installation
```

**Windows:**
```powershell
# Download from https://mariadb.org/download/
# Run installer and follow wizard
```

### Create Database

```sql
-- Connect as root
mysql -u root -p

-- Create database and user
CREATE DATABASE crm_db CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
CREATE USER 'crm_user'@'%' IDENTIFIED BY 'crm_password';
GRANT ALL PRIVILEGES ON crm_db.* TO 'crm_user'@'%';
FLUSH PRIVILEGES;

-- Verify
SHOW DATABASES;
```

### Connection String

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=crm_db;User=crm_user;Password=crm_password;AllowUserVariables=true;UseAffectedRows=false"
  }
}
```

---

## PostgreSQL Setup

### Docker

```bash
docker run -d \
  --name crm-postgres \
  -e POSTGRES_DB=crm_db \
  -e POSTGRES_USER=crm_user \
  -e POSTGRES_PASSWORD=crm_password \
  -p 5432:5432 \
  postgres:16
```

### Local Installation

**macOS:**
```bash
brew install postgresql@16
brew services start postgresql@16
```

**Linux:**
```bash
sudo apt install postgresql postgresql-contrib
sudo systemctl start postgresql
```

### Create Database

```sql
-- Connect as postgres user
sudo -u postgres psql

-- Create database and user
CREATE DATABASE crm_db;
CREATE USER crm_user WITH ENCRYPTED PASSWORD 'crm_password';
GRANT ALL PRIVILEGES ON DATABASE crm_db TO crm_user;
\q
```

### Connection String

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=crm_db;Username=crm_user;Password=crm_password"
  }
}
```

---

## SQL Server Setup

### Docker

```bash
docker run -d \
  --name crm-sqlserver \
  -e 'ACCEPT_EULA=Y' \
  -e 'SA_PASSWORD=YourStrong@Passw0rd' \
  -p 1433:1433 \
  mcr.microsoft.com/mssql/server:2022-latest
```

### Create Database

```sql
-- Connect as SA
CREATE DATABASE crm_db;
USE crm_db;

-- Create login
CREATE LOGIN crm_user WITH PASSWORD = 'crm_password';
CREATE USER crm_user FOR LOGIN crm_user;
EXEC sp_addrolemember 'db_owner', 'crm_user';
```

### Connection String

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=crm_db;User Id=crm_user;Password=crm_password;TrustServerCertificate=true"
  }
}
```

---

## Database Schema

### Entity Relationship Overview

```
┌─────────────┐     ┌─────────────────┐     ┌─────────────┐
│  Customers  │────<│ CustomerContacts│>────│  Contacts   │
└─────────────┘     └─────────────────┘     └─────────────┘
      │                                            │
      │ 1:N                                       │ 1:N
      ▼                                           ▼
┌─────────────┐                           ┌───────────────┐
│  Accounts   │                           │EmailAddresses │
└─────────────┘                           │PhoneNumbers   │
      │                                   │Addresses      │
      │ 1:N                               │SocialMedia    │
      ▼                                   └───────────────┘
┌─────────────┐
│Opportunities│
└─────────────┘
      │
      │ 1:N
      ▼
┌─────────────┐     ┌─────────────────┐
│   Quotes    │────<│ QuoteLineItems  │
└─────────────┘     └─────────────────┘
```

### Key Tables

| Table | Description |
|-------|-------------|
| Customers | Customer records |
| Contacts | Contact individuals |
| CustomerContacts | Many-to-many link |
| Accounts | Business accounts |
| Opportunities | Sales opportunities |
| Products | Product catalog |
| Quotes | Sales quotes |
| MarketingCampaigns | Marketing campaigns |
| Leads | Sales leads |
| ServiceRequests | Support tickets |
| WorkflowDefinitions | Workflow templates |
| Users | System users |

---

## Database Migrations

### Using EF Core Migrations

```bash
cd CRM.Backend

# Create migration
dotnet ef migrations add MigrationName \
  --project src/CRM.Infrastructure \
  --startup-project src/CRM.Api

# Apply migrations
dotnet ef database update \
  --project src/CRM.Infrastructure \
  --startup-project src/CRM.Api

# Remove last migration
dotnet ef migrations remove \
  --project src/CRM.Infrastructure \
  --startup-project src/CRM.Api

# Generate SQL script
dotnet ef migrations script \
  --project src/CRM.Infrastructure \
  --startup-project src/CRM.Api \
  -o migration.sql
```

### Manual Migration Files

SQL migration files are located in:
```
CRM.Backend/migrations/
├── 001_initial_schema.sql
├── 002_add_contacts.sql
├── 003_add_accounts.sql
├── ...
└── 009_create_workflow_engine_tables.sql
```

Apply manually:
```bash
mysql -u crm_user -p crm_db < migrations/007_create_module_field_configurations.sql
```

---

## Database Seeding

### Using Database Seeder

```bash
cd CRM.Backend/src/CRM.DatabaseSeeder
dotnet run -- --seed all
```

### Seed Data Location

```
database/
├── seed/
│   ├── customers.sql
│   ├── contacts.sql
│   ├── products.sql
│   └── ...
└── master_data/
    ├── industries.sql
    ├── countries.sql
    ├── states.sql
    └── zipcodes.sql
```

### Production Seed Data (192.168.0.9)

| Entity | Count |
|--------|-------|
| Customers | 53 |
| Contacts | 105 |
| Products | 12 |
| Accounts | 25 |
| Opportunities | 20 |
| Marketing Campaigns | 5 |
| Leads | 10 |
| Service Requests | 10 |
| User Groups | 5 |
| Users | 1 |

---

## Backup and Restore

### MariaDB Backup

```bash
# Backup
mysqldump -u crm_user -p crm_db > crm_backup_$(date +%Y%m%d).sql

# Docker backup
docker exec crm-mariadb mysqldump -u crm_user -pcrm_password crm_db > backup.sql

# Restore
mysql -u crm_user -p crm_db < crm_backup.sql

# Docker restore
docker exec -i crm-mariadb mysql -u crm_user -pcrm_password crm_db < backup.sql
```

### PostgreSQL Backup

```bash
# Backup
pg_dump -U crm_user crm_db > crm_backup.sql

# Restore
psql -U crm_user crm_db < crm_backup.sql
```

### SQL Server Backup

```sql
-- Backup
BACKUP DATABASE crm_db TO DISK = '/var/opt/mssql/backup/crm_backup.bak'

-- Restore
RESTORE DATABASE crm_db FROM DISK = '/var/opt/mssql/backup/crm_backup.bak'
```

---

## Performance Tuning

### MariaDB Configuration

```ini
# /etc/mysql/my.cnf or my.ini

[mysqld]
# Memory settings
innodb_buffer_pool_size = 1G
innodb_log_file_size = 256M
innodb_flush_log_at_trx_commit = 2

# Connection settings
max_connections = 200
wait_timeout = 600

# Query cache
query_cache_type = 1
query_cache_size = 64M
```

### Key Indexes

The following indexes are automatically created:

```sql
-- Customer indexes
CREATE INDEX IX_Customers_Email ON Customers(Email);
CREATE INDEX IX_Customers_Name ON Customers(Name);
CREATE INDEX IX_Customers_Type ON Customers(CustomerType);

-- Contact indexes
CREATE INDEX IX_Contacts_Email ON Contacts(Email);
CREATE INDEX IX_Contacts_LastName ON Contacts(LastName);

-- Opportunity indexes
CREATE INDEX IX_Opportunities_CustomerId ON Opportunities(CustomerId);
CREATE INDEX IX_Opportunities_Stage ON Opportunities(Stage);
CREATE INDEX IX_Opportunities_CloseDate ON Opportunities(ExpectedCloseDate);
```

---

## Troubleshooting

### Connection Issues

```bash
# Test MariaDB connection
mysql -h localhost -u crm_user -pcrm_password -e "SELECT 1"

# Check if service is running
systemctl status mariadb

# Check Docker container
docker ps | grep crm-mariadb
docker logs crm-mariadb
```

### Migration Issues

```bash
# Reset migrations (development only)
dotnet ef database drop --force
dotnet ef database update
```

### Permission Issues

```sql
-- Grant all permissions
GRANT ALL PRIVILEGES ON crm_db.* TO 'crm_user'@'%';
GRANT PROCESS ON *.* TO 'crm_user'@'%';
FLUSH PRIVILEGES;
```

---

## Related Documentation

- [DEVELOPMENT.md](DEVELOPMENT.md) - Developer guide
- [../ARCHITECTURE_OVERVIEW.md](../ARCHITECTURE_OVERVIEW.md) - System architecture
- [deployment/DOCKER_SETUP.md](deployment/DOCKER_SETUP.md) - Docker configuration
