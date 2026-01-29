# CRM Development Guide

**Version:** 0.0.25  
**Last Updated:** January 2025

---

## 🚀 Getting Started

### Prerequisites

| Tool | Version | Purpose |
|------|---------|---------|
| .NET SDK | 8.0+ | Backend development |
| Node.js | 20+ | Frontend development |
| npm | 10+ | Package management |
| Docker | 24+ | Containerization |
| Git | 2.40+ | Version control |
| MariaDB | 10.11+ | Database (or Docker) |

### Quick Setup

```bash
# Clone repository
git clone <repository-url>
cd crm-solution

# Option 1: Docker Compose (Recommended)
docker-compose -f docker/docker-compose.yml up -d

# Option 2: Local Development
# Terminal 1 - Database
docker run -d --name crm-db -e MYSQL_ROOT_PASSWORD=root -e MYSQL_DATABASE=crm_db -p 3306:3306 mariadb:10.11

# Terminal 2 - Backend
cd CRM.Backend/src/CRM.Api
dotnet run

# Terminal 3 - Frontend
cd CRM.Frontend
npm install
npm start
```

### Access Points

| Service | URL |
|---------|-----|
| Frontend | http://localhost:3000 |
| API | http://localhost:5000 |
| Swagger | http://localhost:5000/swagger |

---

## 📁 Project Structure

### Backend Architecture (Clean Architecture)

```
CRM.Backend/
├── CRM.sln                    # Monolith solution
├── CRM.Microservices.sln      # Microservices solution
└── src/
    ├── CRM.Api/               # API Layer (Controllers, Middleware)
    │   ├── Controllers/       # REST API endpoints
    │   ├── Hubs/              # SignalR hubs
    │   └── Middleware/        # Custom middleware
    │
    ├── CRM.Core/              # Domain Layer (Entities, Interfaces)
    │   ├── Entities/          # Domain models
    │   ├── Enums/             # Enumerations
    │   ├── Interfaces/        # Service contracts
    │   └── DTOs/              # Data transfer objects
    │
    ├── CRM.Infrastructure/    # Infrastructure Layer
    │   ├── Data/              # DbContext, configurations
    │   ├── Repositories/      # Data access
    │   └── Services/          # Service implementations
    │
    └── Services/              # Microservices
        ├── CRM.ServiceDefaults/  # Shared configuration
        ├── CRM.Gateway/          # API Gateway
        ├── CRM.Identity/         # Auth service
        ├── CRM.CustomerService/  # Customer service
        ├── CRM.SalesService/     # Sales service
        ├── CRM.MarketingService/ # Marketing service
        ├── CRM.ServiceDeskService/ # Service desk
        └── CRM.CoreService/      # Core service
```

### Frontend Architecture (Component-Based)

```
CRM.Frontend/src/
├── components/          # Reusable UI components
│   ├── common/          # Shared components
│   ├── customers/       # Customer components
│   ├── contacts/        # Contact components
│   ├── opportunities/   # Opportunity components
│   └── ...
│
├── pages/               # Route-level components
│   ├── Dashboard/
│   ├── Customers/
│   ├── Contacts/
│   ├── Opportunities/
│   └── ...
│
├── services/            # API client services
│   ├── api.ts           # Axios configuration
│   ├── customerService.ts
│   ├── contactService.ts
│   └── ...
│
├── contexts/            # React contexts
│   ├── AuthContext.tsx
│   ├── ThemeContext.tsx
│   └── SignalRContext.tsx
│
├── hooks/               # Custom React hooks
│   ├── usePagination.ts
│   ├── useConcurrency.ts
│   └── ...
│
├── types/               # TypeScript types
│   ├── customer.ts
│   ├── contact.ts
│   └── ...
│
└── utils/               # Utility functions
```

---

## 🔧 Development Workflow

### Creating a New Entity

#### 1. Create Entity (CRM.Core)

```csharp
// CRM.Core/Entities/YourEntity.cs
public class YourEntity : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    
    // Relationships
    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }
}
```

#### 2. Add DbSet (CRM.Infrastructure)

```csharp
// CRM.Infrastructure/Data/CrmDbContext.cs
public DbSet<YourEntity> YourEntities => Set<YourEntity>();
```

#### 3. Create Service Interface (CRM.Core)

```csharp
// CRM.Core/Interfaces/IYourEntityService.cs
public interface IYourEntityService
{
    Task<IEnumerable<YourEntity>> GetAllAsync();
    Task<YourEntity?> GetByIdAsync(int id);
    Task<YourEntity> CreateAsync(YourEntity entity);
    Task<YourEntity> UpdateAsync(YourEntity entity);
    Task DeleteAsync(int id);
}
```

#### 4. Implement Service (CRM.Infrastructure)

```csharp
// CRM.Infrastructure/Services/YourEntityService.cs
public class YourEntityService : IYourEntityService
{
    private readonly CrmDbContext _context;
    
    public YourEntityService(CrmDbContext context)
    {
        _context = context;
    }
    
    public async Task<IEnumerable<YourEntity>> GetAllAsync()
    {
        return await _context.YourEntities
            .Include(e => e.Customer)
            .ToListAsync();
    }
    
    // ... implement other methods
}
```

#### 5. Register Service (Program.cs)

```csharp
builder.Services.AddScoped<IYourEntityService, YourEntityService>();
```

#### 6. Create Controller (CRM.Api)

```csharp
// CRM.Api/Controllers/YourEntitiesController.cs
[ApiController]
[Route("api/[controller]")]
public class YourEntitiesController : ControllerBase
{
    private readonly IYourEntityService _service;
    
    public YourEntitiesController(IYourEntityService service)
    {
        _service = service;
    }
    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<YourEntity>>> GetAll()
    {
        var entities = await _service.GetAllAsync();
        return Ok(entities);
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<YourEntity>> GetById(int id)
    {
        var entity = await _service.GetByIdAsync(id);
        return entity == null ? NotFound() : Ok(entity);
    }
    
    [HttpPost]
    public async Task<ActionResult<YourEntity>> Create(YourEntity entity)
    {
        var created = await _service.CreateAsync(entity);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }
    
    [HttpPut("{id}")]
    public async Task<ActionResult<YourEntity>> Update(int id, YourEntity entity)
    {
        if (id != entity.Id) return BadRequest();
        var updated = await _service.UpdateAsync(entity);
        return Ok(updated);
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }
}
```

#### 7. Create Migration

```bash
cd CRM.Backend
dotnet ef migrations add AddYourEntity --project src/CRM.Infrastructure --startup-project src/CRM.Api
dotnet ef database update --project src/CRM.Infrastructure --startup-project src/CRM.Api
```

---

### Creating a Frontend Component

#### 1. Create Type Definition

```typescript
// src/types/yourEntity.ts
export interface YourEntity {
  id: number;
  name: string;
  description: string;
  createdAt: string;
  isActive: boolean;
  customerId: number;
}
```

#### 2. Create API Service

```typescript
// src/services/yourEntityService.ts
import api from './api';
import { YourEntity } from '../types/yourEntity';

export const yourEntityService = {
  getAll: async (): Promise<YourEntity[]> => {
    const response = await api.get('/yourentities');
    return response.data;
  },
  
  getById: async (id: number): Promise<YourEntity> => {
    const response = await api.get(`/yourentities/${id}`);
    return response.data;
  },
  
  create: async (entity: Omit<YourEntity, 'id'>): Promise<YourEntity> => {
    const response = await api.post('/yourentities', entity);
    return response.data;
  },
  
  update: async (id: number, entity: YourEntity): Promise<YourEntity> => {
    const response = await api.put(`/yourentities/${id}`, entity);
    return response.data;
  },
  
  delete: async (id: number): Promise<void> => {
    await api.delete(`/yourentities/${id}`);
  }
};
```

#### 3. Create List Component

```tsx
// src/pages/YourEntities/YourEntityList.tsx
import React, { useState, useEffect } from 'react';
import { DataGrid, GridColDef } from '@mui/x-data-grid';
import { yourEntityService } from '../../services/yourEntityService';
import { YourEntity } from '../../types/yourEntity';

const YourEntityList: React.FC = () => {
  const [entities, setEntities] = useState<YourEntity[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    loadEntities();
  }, []);

  const loadEntities = async () => {
    try {
      const data = await yourEntityService.getAll();
      setEntities(data);
    } finally {
      setLoading(false);
    }
  };

  const columns: GridColDef[] = [
    { field: 'id', headerName: 'ID', width: 70 },
    { field: 'name', headerName: 'Name', flex: 1 },
    { field: 'description', headerName: 'Description', flex: 2 },
    { field: 'isActive', headerName: 'Active', width: 100, type: 'boolean' },
  ];

  return (
    <DataGrid
      rows={entities}
      columns={columns}
      loading={loading}
      autoHeight
    />
  );
};

export default YourEntityList;
```

#### 4. Add Route

```tsx
// src/App.tsx
import YourEntityList from './pages/YourEntities/YourEntityList';

// Add to routes
<Route path="/your-entities" element={<YourEntityList />} />
```

---

## 🧪 Testing

### Backend Tests

```bash
# Run all tests
cd CRM.Backend/tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific category
dotnet test --filter "FullyQualifiedName~BVT"
```

### Frontend Tests

```bash
cd CRM.Frontend

# Run all tests
npm test

# Run with coverage
npm test -- --coverage

# Run specific file
npm test -- CustomerForm.test.tsx
```

### E2E Tests

```bash
cd e2e-tests

# Install Playwright
npm install
npx playwright install

# Run tests
npx playwright test

# Run with UI
npx playwright test --ui

# Run headed
npx playwright test --headed
```

---

## 🔄 SignalR Real-Time Updates

### Backend Hub

```csharp
// Already implemented in CRM.Api/Hubs/CrmNotificationHub.cs
public class CrmNotificationHub : Hub
{
    public async Task JoinEntityGroup(string entityType, int entityId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"{entityType}:{entityId}");
    }
    
    public async Task LeaveEntityGroup(string entityType, int entityId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"{entityType}:{entityId}");
    }
}
```

### Frontend Context

```tsx
// Use SignalR context for real-time updates
import { useSignalR } from '../contexts/SignalRContext';

const MyComponent = () => {
  const { connection } = useSignalR();
  
  useEffect(() => {
    connection?.on('EntityUpdated', (entityType, entityId, data) => {
      // Handle update
    });
  }, [connection]);
};
```

---

## 📝 Code Standards

### C# Conventions
- Use PascalCase for public members
- Use camelCase for private fields with `_` prefix
- Async methods end with `Async`
- Use nullable reference types
- Use records for DTOs when appropriate

### TypeScript Conventions
- Use camelCase for variables and functions
- Use PascalCase for types and components
- Use interfaces over type aliases
- Use functional components with hooks
- Destructure props

### Git Workflow
```bash
# Create feature branch
git checkout -b feature/your-feature

# Make changes and commit
git add .
git commit -m "feat: add your feature"

# Push and create PR
git push origin feature/your-feature
```

### Commit Message Format
```
type: description

Types:
- feat: New feature
- fix: Bug fix
- docs: Documentation
- style: Formatting
- refactor: Code restructure
- test: Tests
- chore: Maintenance
```

---

## 🐛 Debugging

### Backend Debugging

```bash
# Run with detailed logging
cd CRM.Backend/src/CRM.Api
dotnet run --verbosity detailed

# Attach debugger in VS Code
# Use "Debug .NET Core" launch configuration
```

### Frontend Debugging

```bash
# Run in development mode
cd CRM.Frontend
npm start

# Open browser DevTools
# React DevTools extension recommended
```

### Database Debugging

```bash
# Connect to MariaDB
docker exec -it crm-mariadb mariadb -u crm_user -pcrm_password crm_db

# Show tables
SHOW TABLES;

# Query data
SELECT * FROM Customers LIMIT 10;
```

---

## 🔧 Environment Configuration

### Backend (appsettings.json)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=crm_db;User=crm_user;Password=crm_password;"
  },
  "Jwt": {
    "Secret": "your-secret-key-here",
    "Issuer": "CRMSolution",
    "ExpiryMinutes": 60
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```

### Frontend (.env)

```env
REACT_APP_API_URL=http://localhost:5000/api
REACT_APP_SIGNALR_URL=http://localhost:5000/hub
```

---

## 📚 Related Documentation

| Document | Description |
|----------|-------------|
| [DATABASE_SETUP.md](DATABASE_SETUP.md) | Database configuration |
| [../ARCHITECTURE_OVERVIEW.md](../ARCHITECTURE_OVERVIEW.md) | System architecture |
| [../MICROSERVICES_ARCHITECTURE.md](../MICROSERVICES_ARCHITECTURE.md) | Microservices |
| [../TESTING_SUMMARY.md](../TESTING_SUMMARY.md) | Testing guide |
| [deployment/](deployment/) | Deployment guides |
