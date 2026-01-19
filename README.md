# CRM Solution - Comprehensive Customer Relationship Management System

A full-stack, enterprise-grade CRM solution built with **.NET Core** backend and **React** frontend with responsive UI supporting mobile and web platforms.

## 📋 Table of Contents

- [Features](#features)
- [Tech Stack](#tech-stack)
- [Project Structure](#project-structure)
- [Prerequisites](#prerequisites)
- [Installation & Setup](#installation--setup)
- [Configuration](#configuration)
- [Running the Application](#running-the-application)
- [API Documentation](#api-documentation)
- [Database Setup](#database-setup)
- [Development](#development)
- [License](#license)

## 🎯 Features

### Core Modules

#### 1. **Sales Management**
- Track and manage sales opportunities
- Pipeline visualization and forecasting
- Opportunity stage tracking (Prospecting → Closed)
- Probability and amount tracking
- Win/loss analysis

#### 2. **Customer Management**
- Complete customer profiles
- Lifecycle stage tracking (Lead, Customer, Inactive)
- Contact information and preferences
- Customer interaction history
- Search and filtering capabilities

#### 3. **Product Management**
- Product catalog with categories
- Pricing and cost tracking
- Inventory management
- SKU tracking
- Product associations with opportunities

#### 4. **Marketing Campaigns**
- Campaign creation and management
- Campaign type support (Email, Social, Event, etc.)
- Budget tracking
- Performance metrics and conversion tracking
- Active campaign dashboard

#### 5. **Interaction Tracking**
- Log customer interactions (Email, Phone, Meeting, Notes)
- Track interaction history
- Assignment to team members
- Interaction timeline

#### 6. **Analytics & Reporting**
- Sales pipeline visualization
- Campaign performance metrics
- Dashboard with key metrics
- Customizable reports

## 🛠 Tech Stack

### Backend
- **Framework**: ASP.NET Core 8.0
- **Database Support**: SQL Server, PostgreSQL, Oracle, MariaDB/MySQL
- **ORM**: Entity Framework Core 8.0
- **Logging**: Serilog
- **API**: RESTful API with Swagger/OpenAPI
- **Architecture**: Clean Architecture with Repository Pattern

### Frontend
- **Framework**: React 18
- **Language**: TypeScript
- **UI Library**: React Bootstrap 5
- **Routing**: React Router v6
- **HTTP Client**: Axios
- **Visualization**: Recharts
- **Form Handling**: Formik + Yup
- **Icons**: React Icons

### DevOps & Tools
- **Package Manager**: npm
- **Build Tools**: React Scripts, .NET CLI
- **Version Control**: Git
- **API Documentation**: Swagger UI

## 📁 Project Structure

```
CRM/
├── CRM.Backend/
│   ├── src/
│   │   ├── CRM.Api/                    # ASP.NET Core Web API
│   │   │   ├── Controllers/            # API endpoints
│   │   │   ├── Middleware/             # Custom middleware
│   │   │   ├── Program.cs              # Application startup
│   │   │   └── appsettings.json        # Configuration
│   │   ├── CRM.Core/                   # Business logic & entities
│   │   │   ├── Entities/               # Domain models
│   │   │   ├── Interfaces/             # Service contracts
│   │   │   └── Services/               # Business logic
│   │   └── CRM.Infrastructure/         # Data access layer
│   │       ├── Data/                   # DbContext
│   │       ├── Repositories/           # Repository implementations
│   │       └── Services/               # Infrastructure services
│   ├── tests/                          # Unit tests
│   └── CRM.sln                         # Solution file
│
├── CRM.Frontend/
│   ├── src/
│   │   ├── components/                 # Reusable React components
│   │   ├── pages/                      # Page components
│   │   ├── services/                   # API service layer
│   │   ├── styles/                     # Global styles
│   │   ├── App.tsx                     # Root component
│   │   └── main.tsx                    # Entry point
│   ├── public/                         # Static assets
│   ├── package.json                    # Dependencies
│   └── tsconfig.json                   # TypeScript config
│
└── docs/                               # Documentation
```

## 📦 Prerequisites

### System Requirements
- **Windows 10+** or **Linux** or **macOS**
- **.NET 8.0 SDK** or higher
- **Node.js 18+** and **npm 9+**
- One of supported databases: SQL Server, PostgreSQL, Oracle, MariaDB

### Database Setup
- SQL Server 2019+ (LocalDB for development)
- PostgreSQL 12+
- Oracle 21c+
- MariaDB 10.5+

## 🚀 Installation & Setup

### 1. Clone the Repository
```bash
git clone <repository-url>
cd CRM
```

### 2. Backend Setup

```bash
cd CRM.Backend

# Restore NuGet packages
dotnet restore

# Build the solution
dotnet build

# Install EF Core CLI (if not installed)
dotnet tool install --global dotnet-ef
```

### 3. Frontend Setup

```bash
cd CRM.Frontend

# Install npm packages
npm install

# Build (optional for production)
npm run build
```

## ⚙️ Configuration

### Backend Configuration (appsettings.json)

Edit `CRM.Backend/src/CRM.Api/appsettings.json`:

```json
{
  "DatabaseProvider": "sqlserver",  // Options: sqlserver, postgresql, oracle, mysql
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=CrmDatabase;Integrated Security=true;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```

### Database Provider Examples

#### SQL Server (LocalDB)
```json
{
  "DatabaseProvider": "sqlserver",
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=CrmDatabase;Trusted_Connection=true;"
  }
}
```

#### PostgreSQL
```json
{
  "DatabaseProvider": "postgresql",
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=crm_db;Username=postgres;Password=password"
  }
}
```

#### Oracle
```json
{
  "DatabaseProvider": "oracle",
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=localhost:1521/XE;User Id=system;Password=password"
  }
}
```

#### MariaDB/MySQL
```json
{
  "DatabaseProvider": "mysql",
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=crm_db;Uid=root;Pwd=password"
  }
}
```

### Frontend Configuration

Edit `CRM.Frontend/.env` (create if not exists):
```
REACT_APP_API_URL=http://localhost:5000/api
```

## 🏃 Running the Application

### 1. Database Migration

```bash
cd CRM.Backend

# Create database and apply migrations
dotnet ef database update --project src/CRM.Infrastructure --startup-project src/CRM.Api
```

### 2. Start Backend API

```bash
cd CRM.Backend/src/CRM.Api

# Development
dotnet run

# Production
dotnet run --configuration Release
```

The API will be available at `https://localhost:5001` and Swagger UI at `https://localhost:5001/swagger`

### 3. Start Frontend

```bash
cd CRM.Frontend

# Development with hot reload
npm start

# Production build
npm run build
npm install -g serve
serve -s build
```

The frontend will be available at `http://localhost:3000`

## 📚 API Documentation

### Available Endpoints

#### Customers
- `GET /api/customers` - List all customers
- `GET /api/customers/{id}` - Get customer by ID
- `GET /api/customers/search/{term}` - Search customers
- `POST /api/customers` - Create customer
- `PUT /api/customers/{id}` - Update customer
- `DELETE /api/customers/{id}` - Delete customer

#### Opportunities
- `GET /api/opportunities` - List open opportunities
- `GET /api/opportunities/{id}` - Get opportunity by ID
- `GET /api/opportunities/customer/{customerId}` - Get customer opportunities
- `GET /api/opportunities/pipeline/total` - Get total pipeline value
- `POST /api/opportunities` - Create opportunity
- `PUT /api/opportunities/{id}` - Update opportunity
- `DELETE /api/opportunities/{id}` - Delete opportunity

#### Products
- `GET /api/products` - List all products
- `GET /api/products/{id}` - Get product by ID
- `GET /api/products/category/{category}` - Get products by category
- `POST /api/products` - Create product
- `PUT /api/products/{id}` - Update product
- `DELETE /api/products/{id}` - Delete product

#### Campaigns
- `GET /api/campaigns` - List all campaigns
- `GET /api/campaigns/active` - List active campaigns
- `GET /api/campaigns/{id}` - Get campaign by ID
- `POST /api/campaigns` - Create campaign
- `PUT /api/campaigns/{id}` - Update campaign
- `DELETE /api/campaigns/{id}` - Delete campaign
- `POST /api/campaigns/{id}/metrics` - Add campaign metric

## 🗄️ Database Setup

### Creating Database

#### SQL Server (LocalDB)
```sql
CREATE DATABASE CrmDatabase;
USE CrmDatabase;
```

#### PostgreSQL
```sql
CREATE DATABASE crm_db;
```

#### Oracle
```sql
CREATE USER crm IDENTIFIED BY password;
GRANT CREATE SESSION, RESOURCE, UNLIMITED TABLESPACE TO crm;
```

#### MariaDB/MySQL
```sql
CREATE DATABASE crm_db;
USE crm_db;
```

## 🔨 Development

### Adding New Features

1. **Backend Feature**:
   - Create entity in `CRM.Core/Entities`
   - Define interface in `CRM.Core/Interfaces`
   - Implement service in `CRM.Infrastructure/Services`
   - Create controller in `CRM.Api/Controllers`

2. **Frontend Feature**:
   - Create component in `src/components` or page in `src/pages`
   - Add service method in `src/services/apiService.ts`
   - Add styling in respective CSS files

### Running Tests

```bash
cd CRM.Backend

# Run all tests
dotnet test

# Run specific test project
dotnet test tests/CRM.Tests
```

### Code Style

- **Backend**: Follow C# coding standards (PascalCase for public members)
- **Frontend**: ESLint configured (run `npm run lint`)

## 📱 Responsive Design

The frontend is fully responsive with breakpoints for:
- **Mobile**: < 576px
- **Tablet**: 576px - 991px
- **Desktop**: > 992px

Bootstrap 5 grid system ensures mobile-first approach.

## 🔐 Security Considerations

- CORS enabled (configure as needed)
- Input validation on both backend and frontend
- Database connection strings in configuration
- Recommend using Azure KeyVault or similar for production secrets

## 📄 License

This project is proprietary and confidential.

## 🤝 Support

For issues and questions, contact the development team.

---

**Version**: 1.0.0  
**Last Updated**: January 2026
