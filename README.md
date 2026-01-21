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

## 🧪 Testing

### Test Structure

The solution includes comprehensive unit tests and Build Verification Tests (BVT):

```
CRM.Backend/tests/
├── CRM.Tests.csproj                    # Test project configuration
├── CRM.Tests/
│   ├── EntityTests.cs                  # Entity creation tests
│   └── UserEntityTests.cs              # User entity tests
├── Controllers/
│   ├── CustomersControllerTests.cs     # Customer API endpoint tests
│   └── DepartmentsControllerTests.cs   # Department API tests
├── Services/
│   └── CustomerServiceTests.cs         # CustomerService business logic tests
└── BVT/
    └── CriticalPathTests.cs            # Build Verification Tests
```

### Running Tests

```bash
# Run all tests
cd CRM.Backend/tests
dotnet test

# Run with verbose output
dotnet test --logger "console;verbosity=detailed"

# Run specific test category
dotnet test --filter "FullyQualifiedName~BVT"
```

### Test Categories

| Category | Description | Count |
|----------|-------------|-------|
| Entity Tests | Verify entity creation and defaults | 15 |
| Controller Tests | Test API endpoint responses | 22 |
| Service Tests | Test business logic | 12 |
| BVT Tests | Critical path verification | 12 |

### Build Verification Tests (BVT)

BVT tests should be run before every deployment:
- BVT-001: Customer Entity Creation
- BVT-002: Organization Customer with Company
- BVT-003: User Entity Creation
- BVT-004: CustomerDto Mapping
- BVT-005: Category Enum Values
- BVT-006: UserRole Enum Values
- BVT-007: Lifecycle Stage Defaults
- BVT-008: Product Entity
- BVT-009: Opportunity Entity
- BVT-010: Department Entity
- BVT-011: AuthResponse DTO
- BVT-012: CustomerContact Junction

## 📖 Code Documentation

### Documentation Standards

All code includes XML documentation with two perspectives:

1. **Functional View**: Business context - what the feature does from a user's perspective
2. **Technical View**: Implementation details - how it works for developers

### Example Documentation

```csharp
/// <summary>
/// Customer service implementation providing CRUD operations.
/// 
/// FUNCTIONAL VIEW:
/// This service handles all customer-related business operations including:
/// - Creating Individual and Organization customers
/// - Managing customer lifecycle
/// 
/// TECHNICAL VIEW:
/// - Implements ICustomerService interface
/// - Uses IRepository pattern for data access
/// </summary>
public class CustomerService : ICustomerService
```

### Key Documented Components

| Component | Location | Purpose |
|-----------|----------|---------|
| User Entity | `CRM.Core/Entities/User.cs` | User management with roles |
| Customer Entity | `CRM.Core/Entities/Customer.cs` | Individual/Organization customers |
| CustomerService | `CRM.Infrastructure/Services/CustomerService.cs` | Customer business logic |
| CustomersController | `CRM.Api/Controllers/CustomersController.cs` | REST API endpoints |

## 🔐 Security Considerations

- CORS enabled (configure as needed)
- Input validation on both backend and frontend
- Database connection strings in configuration
- Recommend using Azure KeyVault or similar for production secrets
- JWT Bearer authentication with refresh tokens
- Password hashing using BCrypt
- Two-factor authentication support

## 📄 License

**CRM Solution** is free software licensed under the **GNU Affero General Public License v3.0 (AGPL-3.0)**.

Copyright (C) 2024-2026 Abhishek Lal

This program is free software: you can redistribute it and/or modify it under the terms of the GNU Affero General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

This program is distributed in the hope that it will be useful, but **WITHOUT ANY WARRANTY**; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

### Copyleft Notice

This software is copyleft. Any derivative works must be distributed under the same license terms. If you modify this software and make it available to users over a network, you must provide them with access to the source code of your modified version under the terms of the AGPL-3.0.

### No Liability

In no event shall Abhishek Lal or any contributors be liable for any direct, indirect, incidental, special, exemplary, or consequential damages arising out of the use of this software.

### Free Use

This software is provided free of charge for any use, including commercial use, subject to the terms of the AGPL-3.0 license.

## 🤝 Support

For issues and questions, contact the development team.

---

**Version**: 1.3.0  
**Last Updated**: January 2026  
**Author**: Abhishek Lal
