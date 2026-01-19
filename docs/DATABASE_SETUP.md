# CRM Database Configuration Guide

## Database Setup Instructions

### SQL Server (Recommended for Windows)

#### Using LocalDB (Development)
```sql
-- LocalDB connection string
Server=(localdb)\mssqllocaldb;Database=CrmDatabase;Integrated Security=true;
```

#### Using SQL Server Express
```sql
-- Install SQL Server Express from Microsoft
-- Connection string
Server=.\SQLEXPRESS;Database=CrmDatabase;Integrated Security=true;
```

#### Using Azure SQL Database
```json
{
  "DatabaseProvider": "sqlserver",
  "ConnectionStrings": {
    "DefaultConnection": "Server=tcp:<server>.database.windows.net,1433;Initial Catalog=CrmDatabase;Persist Security Info=False;User ID=<username>;Password=<password>;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
  }
}
```

### PostgreSQL

#### Installation
```bash
# Windows - Download from postgresql.org
# Linux
sudo apt-get install postgresql postgresql-contrib

# macOS
brew install postgresql
```

#### Connection String
```json
{
  "DatabaseProvider": "postgresql",
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=crm_db;Username=postgres;Password=yourpassword"
  }
}
```

#### Create Database
```sql
CREATE DATABASE crm_db;
```

### Oracle Database

#### Connection String
```json
{
  "DatabaseProvider": "oracle",
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=localhost:1521/XE;User Id=system;Password=yourpassword"
  }
}
```

### MariaDB/MySQL

#### Installation
```bash
# Windows - Download from mariadb.org or mysql.com
# Linux
sudo apt-get install mariadb-server

# macOS
brew install mariadb
```

#### Connection String
```json
{
  "DatabaseProvider": "mysql",
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=crm_db;Uid=root;Pwd=yourpassword"
  }
}
```

#### Create Database
```sql
CREATE DATABASE crm_db;
USE crm_db;
```

## Database Migration

### First Time Setup

```bash
cd CRM.Backend

# Install EF Core tools if not installed
dotnet tool install --global dotnet-ef

# Create migration (if needed)
dotnet ef migrations add InitialCreate --project src/CRM.Infrastructure --startup-project src/CRM.Api

# Apply migration
dotnet ef database update --project src/CRM.Infrastructure --startup-project src/CRM.Api
```

### Creating New Migrations

```bash
# After modifying entities
dotnet ef migrations add MigrationName --project src/CRM.Infrastructure --startup-project src/CRM.Api

# Apply
dotnet ef database update --project src/CRM.Infrastructure --startup-project src/CRM.Api
```

## Database Backup & Recovery

### SQL Server
```sql
-- Backup
BACKUP DATABASE CrmDatabase TO DISK = 'C:\Backups\CrmDatabase.bak'

-- Restore
RESTORE DATABASE CrmDatabase FROM DISK = 'C:\Backups\CrmDatabase.bak'
```

### PostgreSQL
```bash
# Backup
pg_dump crm_db > crm_backup.sql

# Restore
psql crm_db < crm_backup.sql
```

### MySQL/MariaDB
```bash
# Backup
mysqldump -u root -p crm_db > crm_backup.sql

# Restore
mysql -u root -p crm_db < crm_backup.sql
```

## Performance Tuning

- Create indexes on frequently searched columns
- Monitor query performance using database tools
- Consider connection pooling for production
- Use read replicas for reporting if needed
