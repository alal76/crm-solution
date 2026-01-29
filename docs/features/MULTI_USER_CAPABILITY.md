# Multi-User Capability Implementation

This document describes the multi-user features implemented in the CRM system to handle concurrent editing, real-time updates, and conflict resolution.

## Overview

The multi-user capability provides:

1. **Optimistic Concurrency Control** - Prevents data loss from concurrent edits
2. **ETag-based Caching** - Efficient conditional updates and caching
3. **Real-time Notifications** - SignalR-based updates for live collaboration
4. **Conflict Resolution UI** - User-friendly dialogs for handling conflicts

## Architecture

```
┌─────────────────────────────────────────────────────────────────────────┐
│                           Frontend (React)                               │
├─────────────────────────────────────────────────────────────────────────┤
│  SignalRContext   │  useSignalR Hooks  │  ConcurrencyConflictDialog    │
│  signalRService   │  useConcurrencyControl  │  UserEditingIndicator    │
└────────────────────────────────────────┬────────────────────────────────┘
                                         │ WebSocket / HTTP
                                         ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                           Backend (.NET 8)                               │
├─────────────────────────────────────────────────────────────────────────┤
│  CrmNotificationHub  │  ETagHelper  │  ErrorHandlingMiddleware         │
│  Controllers (If-Match/If-None-Match) │  ConcurrencyConflictResponse   │
└────────────────────────────────────────┬────────────────────────────────┘
                                         │
                                         ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                         Entity Framework Core                            │
├─────────────────────────────────────────────────────────────────────────┤
│  BaseEntity.RowVersion  │  IsConcurrencyToken()  │  DbUpdateConcurrency │
└────────────────────────────────────────┬────────────────────────────────┘
                                         │
                                         ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                            MariaDB                                       │
├─────────────────────────────────────────────────────────────────────────┤
│  RowVersion BINARY(8)  │  BEFORE UPDATE Triggers  │  Auto-increment     │
└─────────────────────────────────────────────────────────────────────────┘
```

## Backend Implementation

### 1. BaseEntity RowVersion

All entities inherit from `BaseEntity` which includes:

```csharp
// CRM.Core/Entities/BaseEntity.cs
[Timestamp]
public byte[]? RowVersion { get; set; }
```

### 2. Database Migration

Migration `017_add_rowversion_concurrency.sql` adds:
- `RowVersion` column (BINARY(8)) to all entity tables
- `BEFORE UPDATE` triggers to auto-increment RowVersion
- Initialization of existing records with RowVersion = 1

### 3. Concurrency Conflict Handling

The `ErrorHandlingMiddleware` catches `DbUpdateConcurrencyException` and returns:

```json
{
  "type": "ConcurrencyConflict",
  "message": "The record has been modified by another user",
  "conflicts": [
    {
      "entityType": "Customer",
      "entityId": "123",
      "currentValues": { ... },
      "databaseValues": { ... }
    }
  ]
}
```

HTTP Status: **409 Conflict**

### 4. ETag Support

Controllers use `ETagHelper` to:
- Generate ETags from RowVersion: `GET /api/customers/{id}` returns `ETag` header
- Validate conditional updates: `PUT /api/customers/{id}` with `If-Match` header
- Support caching: `If-None-Match` returns `304 Not Modified` if unchanged

Example:
```http
GET /api/customers/123
Response Headers:
  ETag: "AAAAAAAAB9M="

PUT /api/customers/123
Request Headers:
  If-Match: "AAAAAAAAB9M="
```

### 5. SignalR Hub

`CrmNotificationHub` provides real-time notifications:

**Server Methods:**
- `SubscribeToRecord(entityType, entityId)` - Join record-specific group
- `UnsubscribeFromRecord(entityType, entityId)` - Leave group
- `SubscribeToEntityType(entityType)` - Join entity type group
- `StartEditing(entityType, entityId)` - Notify others you're editing
- `StopEditing(entityType, entityId)` - Stop editing notification

**Client Events:**
- `RecordUpdated` - Record was modified
- `RecordCreated` - New record created
- `RecordDeleted` - Record was deleted
- `UserEditing` - Another user started/stopped editing

### 6. Notification Service

`ICrmNotificationService` interface for broadcasting from services:

```csharp
await _notificationService.NotifyRecordUpdated("Customer", customerId, updatedBy);
```

## Frontend Implementation

### 1. SignalR Service

`signalRService.ts` - Singleton service managing connection:

```typescript
import signalRService from '../services/signalRService';

// Connect with JWT token
await signalRService.connect(accessToken);

// Subscribe to record updates
signalRService.subscribeToRecord('Customer', 123);

// Listen for updates
signalRService.onRecordUpdated('Customer:123', (data) => {
  console.log('Record updated by', data.updatedBy);
});
```

### 2. SignalR Context

`SignalRContext.tsx` - React context for connection management:

```tsx
import { SignalRProvider } from '../contexts/SignalRContext';

// In App.tsx
<AuthProvider>
  <SignalRProvider>
    <App />
  </SignalRProvider>
</AuthProvider>
```

### 3. SignalR Hooks

**useRecordSubscription** - Subscribe to a specific record:

```tsx
import { useRecordSubscription } from '../hooks/useSignalR';

useRecordSubscription('Customer', customerId, {
  onUpdated: (data) => {
    // Refresh data or show notification
    refetch();
  },
  onDeleted: (data) => {
    // Navigate away or show message
  },
  onUserEditing: (data) => {
    // Show "user is editing" indicator
  },
});
```

**useEntityTypeSubscription** - Subscribe to all records of a type:

```tsx
useEntityTypeSubscription('Customer', {
  onCreated: (data) => {
    // Add new record to list
  },
  onUpdated: (data) => {
    // Update record in list
  },
  onDeleted: (data) => {
    // Remove from list
  },
});
```

**useEditingNotification** - Notify others you're editing:

```tsx
const { startEditing, stopEditing } = useEditingNotification('Customer', customerId);

// When form gets focus
startEditing();

// When form loses focus or saves
stopEditing();
```

### 4. Concurrency Control Hook

`useConcurrencyControl` - Handle 409 Conflict responses:

```tsx
import { useConcurrencyControl } from '../hooks/useConcurrencyControl';

const {
  conflictDialogOpen,
  conflictData,
  handleConflictError,
  handleCloseDialog,
  handleReload,
  handleOverwrite,
} = useConcurrencyControl({
  onReload: () => api.get(`/customers/${id}`),
  onForceSave: () => api.put(`/customers/${id}`, data, { 
    headers: { 'X-Force-Overwrite': 'true' } 
  }),
  onReloadSuccess: (freshData) => setFormData(freshData),
});

// In save handler
try {
  await api.put(`/customers/${id}`, data, {
    headers: { 'If-Match': currentETag }
  });
} catch (error) {
  if (error.response?.status === 409) {
    handleConflictError(error, 'Customer', id);
  }
}
```

### 5. UI Components

**ConcurrencyConflictDialog** - Modal for conflict resolution:

```tsx
import { ConcurrencyConflictDialog } from '../components/common';

<ConcurrencyConflictDialog
  open={conflictDialogOpen}
  onClose={handleCloseDialog}
  onReload={handleReload}
  onOverwrite={handleOverwrite}
  conflictData={conflictData}
  entityName="customer"
/>
```

**UserEditingIndicator** - Show who's editing:

```tsx
import { UserEditingIndicator } from '../components/common';

<UserEditingIndicator
  entityType="Customer"
  entityId={customerId}
  currentUserId={user.id}
/>
```

## Configuration

### Backend (appsettings.json)

```json
{
  "SignalR": {
    "EnableDetailedErrors": true,
    "KeepAliveInterval": "00:00:15",
    "ClientTimeoutInterval": "00:00:30"
  }
}
```

### Frontend Environment

```env
REACT_APP_API_URL=https://api.example.com
# SignalR hub automatically uses {API_URL}/hubs/notifications
```

## Testing

### Unit Tests

```csharp
[Fact]
public async Task Update_WithStaleRowVersion_Returns409()
{
    // Arrange
    var customer = await _context.Customers.FindAsync(1);
    customer.Name = "Updated";
    
    // Simulate another user's update
    await SimulateOtherUserUpdate(1);
    
    // Act
    var response = await _controller.Update(1, customer);
    
    // Assert
    Assert.IsType<ConflictObjectResult>(response);
}
```

### Integration Tests

```typescript
describe('Concurrency Conflict', () => {
  it('shows conflict dialog on 409 response', async () => {
    // Mock 409 response
    server.use(
      rest.put('/api/customers/1', (req, res, ctx) => {
        return res(ctx.status(409), ctx.json({
          type: 'ConcurrencyConflict',
          message: 'Record modified by another user'
        }));
      })
    );
    
    // Trigger save
    fireEvent.click(screen.getByText('Save'));
    
    // Verify dialog appears
    expect(await screen.findByText('Concurrency Conflict Detected')).toBeVisible();
  });
});
```

## Migration Guide

### Running the Database Migration

```bash
cd CRM.Backend/migrations
mysql -u root -p crm_database < 017_add_rowversion_concurrency.sql
```

### Installing Frontend Dependencies

```bash
cd CRM.Frontend
npm install @microsoft/signalr
```

### Updating Controllers

Add ETag support to existing controllers (see CustomersController for example):

1. Add `ETagHelper` usage
2. Return ETag header in GET responses
3. Validate If-Match header in PUT/PATCH requests
4. Map RowVersion to DTOs

## Best Practices

1. **Always include RowVersion in DTOs** for entities that can be updated
2. **Use `useRecordSubscription`** on detail/edit pages
3. **Use `useEntityTypeSubscription`** on list pages
4. **Call `startEditing()`** when user begins editing a form
5. **Handle 409 errors** gracefully with the conflict dialog
6. **Test concurrent scenarios** in development

## Troubleshooting

### SignalR Connection Issues

```typescript
// Check connection state
const { getState } = useSignalRConnection();
console.log('SignalR State:', getState());
```

### RowVersion Not Updating

Check if the database trigger is working:
```sql
SELECT RowVersion FROM Customers WHERE Id = 1;
UPDATE Customers SET Name = 'Test' WHERE Id = 1;
SELECT RowVersion FROM Customers WHERE Id = 1;
```

### ETag Mismatch

Ensure the RowVersion is being mapped to the DTO:
```csharp
CustomerDto dto = new() {
  RowVersion = entity.RowVersion // Don't forget this!
};
```
