# CRM Development Guide

## Getting Started

### Prerequisites
- .NET 8.0 SDK
- Node.js 18+ and npm
- Git
- Your choice of database (SQL Server, PostgreSQL, Oracle, or MariaDB)

### Initial Setup

1. **Clone and navigate**
```bash
git clone <repo-url>
cd CRM
```

2. **Install dependencies**

**Backend:**
```bash
cd CRM.Backend
dotnet restore
```

**Frontend:**
```bash
cd CRM.Frontend
npm install
```

3. **Configure database**
   - Edit `CRM.Backend/src/CRM.Api/appsettings.json`
   - Set `DatabaseProvider` and `ConnectionString`

4. **Run migrations**
```bash
cd CRM.Backend
dotnet ef database update --project src/CRM.Infrastructure --startup-project src/CRM.Api
```

5. **Start both applications**

**Terminal 1 - Backend:**
```bash
cd CRM.Backend/src/CRM.Api
dotnet run
```

**Terminal 2 - Frontend:**
```bash
cd CRM.Frontend
npm start
```

Application will be available at:
- Frontend: http://localhost:3000
- API: http://localhost:5000
- Swagger: http://localhost:5000/swagger

## Project Structure Guide

### Backend Architecture

```
CRM.Core/               # Business logic layer
├── Entities/           # Database models
├── Interfaces/         # Service contracts
└── Services/           # Business logic

CRM.Infrastructure/     # Data access layer
├── Data/               # DbContext
├── Repositories/       # Data access
└── Services/           # Infrastructure services

CRM.Api/                # API layer
├── Controllers/        # API endpoints
├── Middleware/         # Custom middleware
└── Program.cs          # Startup configuration
```

### Frontend Structure

```
src/
├── components/         # Reusable components
├── pages/              # Page components
├── services/           # API client
├── styles/             # CSS files
└── App.tsx             # Root component
```

## Adding New Features

### Add a New Entity (Backend)

1. Create entity in `CRM.Core/Entities/YourEntity.cs`
```csharp
public class YourEntity : BaseEntity
{
    public string Name { get; set; }
    // Add properties
}
```

2. Add DbSet in `CRM.Infrastructure/Data/CrmDbContext.cs`
```csharp
public DbSet<YourEntity> YourEntities { get; set; }
```

3. Create interface in `CRM.Core/Interfaces/IYourService.cs`
4. Implement service in `CRM.Infrastructure/Services/YourService.cs`
5. Create controller in `CRM.Api/Controllers/YourController.cs`
6. Register in `Program.cs`
```csharp
builder.Services.AddScoped<IYourService, YourService>();
```

7. Create database migration
```bash
dotnet ef migrations add AddYourEntity --project src/CRM.Infrastructure --startup-project src/CRM.Api
dotnet ef database update --project src/CRM.Infrastructure --startup-project src/CRM.Api
```

### Add a New Page (Frontend)

1. Create component in `src/pages/YourPage.tsx`
```tsx
function YourPage() {
  return <div>Your content</div>;
}
export default YourPage;
```

2. Add route in `src/App.tsx`
```tsx
<Route path="/yourpage" element={<YourPage />} />
```

3. Add navigation in `src/components/Navigation.tsx`

## API Usage Examples

### GET Request
```typescript
const response = await apiClient.get('/customers');
```

### POST Request
```typescript
const response = await apiClient.post('/customers', {
  firstName: 'John',
  lastName: 'Doe',
  email: 'john@example.com'
});
```

### PUT Request
```typescript
const response = await apiClient.put('/customers/1', updatedData);
```

### DELETE Request
```typescript
const response = await apiClient.delete('/customers/1');
```

## Database Operations

### Raw SQL Query
```csharp
var customers = await _context.Customers
    .FromSqlRaw("SELECT * FROM Customers WHERE IsDeleted = 0")
    .ToListAsync();
```

### LINQ Query
```csharp
var customers = await _context.Customers
    .Where(c => !c.IsDeleted)
    .OrderBy(c => c.LastName)
    .ToListAsync();
```

## Testing

### Backend Unit Tests
```bash
cd CRM.Backend
dotnet test
```

### Frontend Tests
```bash
cd CRM.Frontend
npm test
```

## Debugging

### Backend Debugging
- Set breakpoints in Visual Studio or VS Code
- Debug with `dotnet run` in debug configuration
- Check logs in `logs/` directory

### Frontend Debugging
- Use React Developer Tools extension
- Check browser console (F12)
- Use VS Code debugger

## Code Standards

### C# Naming
- Classes: `PascalCase`
- Methods: `PascalCase`
- Properties: `PascalCase`
- Private fields: `_camelCase`
- Local variables: `camelCase`

### TypeScript/React
- Components: `PascalCase` (files and exports)
- Functions: `camelCase`
- Constants: `UPPER_CASE`
- Interfaces: `IPascalCase` or `PascalCase`

## Performance Tips

1. Use pagination for large datasets
2. Implement caching for frequently accessed data
3. Optimize database queries with indexes
4. Use async/await for non-blocking operations
5. Implement lazy loading for components
6. Minimize bundle size with tree-shaking

## Deployment

### Backend
```bash
dotnet publish -c Release
# Deploy to IIS, Azure App Service, or Docker
```

### Frontend
```bash
npm run build
# Deploy build/ folder to web server
```

## Troubleshooting

### Database Connection Issues
- Verify connection string in appsettings.json
- Check database is running
- Verify credentials and permissions

### API Not Responding
- Check if backend is running on port 5000
- Verify CORS settings in Program.cs
- Check firewall settings

### Frontend Not Loading
- Verify npm packages installed
- Check API URL in .env file
- Clear browser cache and cookies

## Additional Resources

- [.NET Documentation](https://docs.microsoft.com/dotnet)
- [React Documentation](https://react.dev)
- [Entity Framework Core](https://docs.microsoft.com/ef/core)
- [Bootstrap Documentation](https://getbootstrap.com)
