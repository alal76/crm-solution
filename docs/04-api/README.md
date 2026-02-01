# API Reference

> **Last Updated:** February 1, 2026 | **Base URL:** `/api`

---

## Table of Contents

1. [Overview](#1-overview)
2. [Authentication](#2-authentication)
3. [Common Patterns](#3-common-patterns)
4. [Endpoints by Module](#4-endpoints-by-module)

---

## 1. Overview

### 1.1 Base URL

- **Development:** `http://localhost:5000/api`
- **Production:** `https://crm.yourdomain.com/api`

### 1.2 Authentication

All endpoints except `/auth/login`, `/health`, and `/version` require authentication:

```
Authorization: Bearer <access_token>
```

### 1.3 Response Formats

**Success Response:**
```json
{
  "id": 1,
  "name": "Example",
  "createdAt": "2026-01-01T00:00:00Z"
}
```

**Paginated Response:**
```json
{
  "items": [...],
  "totalCount": 100,
  "pageNumber": 1,
  "pageSize": 20,
  "totalPages": 5
}
```

**Error Response:**
```json
{
  "error": "Error message",
  "details": "Additional details",
  "statusCode": 400
}
```

---

## 2. Authentication

### 2.1 Login

```http
POST /api/auth/login
```

**Request:**
```json
{
  "email": "user@example.com",
  "password": "password123"
}
```

**Response:**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "dGhpcyBpcyBhIHJlZnJlc2ggdG9rZW4=",
  "expiresAt": "2026-01-01T01:00:00Z",
  "user": {
    "id": 1,
    "email": "user@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "role": "Admin"
  },
  "requiresPasswordSetup": false,
  "passwordExpired": false,
  "mustChangePassword": false
}
```

### 2.2 Refresh Token

```http
POST /api/auth/refresh
```

**Request:**
```json
{
  "refreshToken": "dGhpcyBpcyBhIHJlZnJlc2ggdG9rZW4="
}
```

### 2.3 Setup Password

```http
POST /api/auth/setup-password
```

**Request:**
```json
{
  "email": "user@example.com",
  "token": "setup-token-from-login",
  "newPassword": "NewSecurePassword123!",
  "confirmPassword": "NewSecurePassword123!"
}
```

### 2.4 Get Password Requirements

```http
GET /api/auth/password-requirements
```

**Response:**
```json
{
  "minLength": 8,
  "maxLength": 128,
  "requireUppercase": true,
  "requireLowercase": true,
  "requireNumbers": true,
  "requireSpecialChars": false
}
```

### 2.5 Logout

```http
POST /api/auth/logout
```

---

## 3. Common Patterns

### 3.1 Pagination

All list endpoints support pagination:

```http
GET /api/customers?page=1&pageSize=20
```

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `page` | int | 1 | Page number (1-indexed) |
| `pageSize` | int | 20 | Items per page (max 100) |

### 3.2 Sorting

```http
GET /api/customers?sortBy=name&sortOrder=asc
```

| Parameter | Type | Options | Description |
|-----------|------|---------|-------------|
| `sortBy` | string | Field name | Field to sort by |
| `sortOrder` | string | `asc`, `desc` | Sort direction |

### 3.3 Filtering

```http
GET /api/customers?search=acme&status=active
```

### 3.4 Standard CRUD Operations

| Operation | HTTP Method | URL Pattern | Description |
|-----------|-------------|-------------|-------------|
| List | GET | `/api/{entity}` | Get paginated list |
| Get One | GET | `/api/{entity}/{id}` | Get by ID |
| Create | POST | `/api/{entity}` | Create new |
| Update | PUT | `/api/{entity}/{id}` | Update existing |
| Delete | DELETE | `/api/{entity}/{id}` | Soft delete |

---

## 4. Endpoints by Module

### 4.1 Core

#### Customers (Accounts)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/accounts` | List customers |
| GET | `/api/accounts/{id}` | Get customer by ID |
| POST | `/api/accounts` | Create customer |
| PUT | `/api/accounts/{id}` | Update customer |
| DELETE | `/api/accounts/{id}` | Delete customer |
| GET | `/api/accounts/{id}/contacts` | Get customer contacts |
| GET | `/api/accounts/{id}/opportunities` | Get customer opportunities |

#### Contacts

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/contacts` | List contacts |
| GET | `/api/contacts/{id}` | Get contact by ID |
| POST | `/api/contacts` | Create contact |
| PUT | `/api/contacts/{id}` | Update contact |
| DELETE | `/api/contacts/{id}` | Delete contact |
| POST | `/api/contacts/{id}/link-account` | Link to account |

#### Contact Info

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/contactinfo/{entityType}/{entityId}` | Get all contact info |
| POST | `/api/contactinfo/email` | Add email |
| POST | `/api/contactinfo/phone` | Add phone |
| POST | `/api/contactinfo/address` | Add address |
| PUT | `/api/contactinfo/{type}/{id}` | Update contact info |
| DELETE | `/api/contactinfo/{type}/{id}` | Delete contact info |

### 4.2 Sales

#### Leads

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/leads` | List leads |
| GET | `/api/leads/{id}` | Get lead by ID |
| POST | `/api/leads` | Create lead |
| PUT | `/api/leads/{id}` | Update lead |
| DELETE | `/api/leads/{id}` | Delete lead |
| POST | `/api/leads/{id}/convert` | Convert to opportunity |
| GET | `/api/leads/pipeline` | Get lead pipeline stats |

#### Opportunities

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/opportunities` | List opportunities |
| GET | `/api/opportunities/{id}` | Get opportunity by ID |
| POST | `/api/opportunities` | Create opportunity |
| PUT | `/api/opportunities/{id}` | Update opportunity |
| DELETE | `/api/opportunities/{id}` | Delete opportunity |
| POST | `/api/opportunities/{id}/close-won` | Mark as won |
| POST | `/api/opportunities/{id}/close-lost` | Mark as lost |
| GET | `/api/opportunities/pipeline` | Get pipeline stats |

#### Quotes

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/quotes` | List quotes |
| GET | `/api/quotes/{id}` | Get quote by ID |
| POST | `/api/quotes` | Create quote |
| PUT | `/api/quotes/{id}` | Update quote |
| DELETE | `/api/quotes/{id}` | Delete quote |
| POST | `/api/quotes/{id}/line-items` | Add line item |
| DELETE | `/api/quotes/{id}/line-items/{itemId}` | Remove line item |
| POST | `/api/quotes/{id}/send` | Send to customer |
| POST | `/api/quotes/{id}/accept` | Mark as accepted |

#### Products

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/products` | List products |
| GET | `/api/products/{id}` | Get product by ID |
| POST | `/api/products` | Create product |
| PUT | `/api/products/{id}` | Update product |
| DELETE | `/api/products/{id}` | Delete product |
| GET | `/api/products/categories` | Get categories |

### 4.3 Marketing

#### Campaigns

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/campaigns` | List campaigns |
| GET | `/api/campaigns/{id}` | Get campaign by ID |
| POST | `/api/campaigns` | Create campaign |
| PUT | `/api/campaigns/{id}` | Update campaign |
| DELETE | `/api/campaigns/{id}` | Delete campaign |
| GET | `/api/campaigns/{id}/metrics` | Get performance metrics |
| GET | `/api/campaigns/{id}/recipients` | Get recipients |
| POST | `/api/campaigns/{id}/recipients` | Add recipients |

#### Campaign Execution

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/campaign-execution/{id}/start` | Start campaign |
| POST | `/api/campaign-execution/{id}/pause` | Pause campaign |
| POST | `/api/campaign-execution/{id}/resume` | Resume campaign |
| POST | `/api/campaign-execution/{id}/stop` | Stop campaign |
| GET | `/api/campaign-execution/{id}/progress` | Get execution progress |
| GET | `/api/campaign-execution/{id}/analytics` | Get real-time analytics |

### 4.4 Service

#### Service Requests

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/service-requests` | List service requests |
| GET | `/api/service-requests/{id}` | Get by ID |
| POST | `/api/service-requests` | Create service request |
| PUT | `/api/service-requests/{id}` | Update service request |
| DELETE | `/api/service-requests/{id}` | Delete service request |
| POST | `/api/service-requests/{id}/assign` | Assign to user |
| POST | `/api/service-requests/{id}/close` | Close request |

#### Tasks

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/tasks` | List tasks |
| GET | `/api/tasks/{id}` | Get task by ID |
| POST | `/api/tasks` | Create task |
| PUT | `/api/tasks/{id}` | Update task |
| DELETE | `/api/tasks/{id}` | Delete task |
| POST | `/api/tasks/{id}/complete` | Mark complete |
| GET | `/api/tasks/my-tasks` | Get current user's tasks |

#### Notes

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/notes` | List notes |
| GET | `/api/notes/{id}` | Get note by ID |
| POST | `/api/notes` | Create note |
| PUT | `/api/notes/{id}` | Update note |
| DELETE | `/api/notes/{id}` | Delete note |
| GET | `/api/notes/entity/{type}/{id}` | Get notes for entity |

### 4.5 Administration

#### Users

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/users` | List users |
| GET | `/api/users/{id}` | Get user by ID |
| POST | `/api/users` | Create user |
| PUT | `/api/users/{id}` | Update user |
| DELETE | `/api/users/{id}` | Deactivate user |
| POST | `/api/users/{id}/reset-password` | Reset password |
| POST | `/api/users/{id}/activate` | Activate user |
| POST | `/api/users/{id}/deactivate` | Deactivate user |

#### User Groups

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/usergroups` | List groups |
| GET | `/api/usergroups/{id}` | Get group by ID |
| POST | `/api/usergroups` | Create group |
| PUT | `/api/usergroups/{id}` | Update group |
| DELETE | `/api/usergroups/{id}` | Delete group |
| GET | `/api/usergroups/{id}/members` | Get group members |
| POST | `/api/usergroups/{id}/members` | Add member |
| DELETE | `/api/usergroups/{id}/members/{userId}` | Remove member |

#### System Settings

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/system-settings` | Get all settings |
| PUT | `/api/system-settings` | Update settings |
| GET | `/api/system-settings/modules` | Get module config |
| PUT | `/api/system-settings/modules` | Update module config |
| GET | `/api/system-settings/security` | Get security settings |
| PUT | `/api/system-settings/security` | Update security settings |

### 4.6 Utilities

#### Health Check

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/health` | Basic health check |
| GET | `/api/health/db` | Database health |
| GET | `/api/health/detailed` | Detailed health |

#### Dashboard

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/dashboard/stats` | Get dashboard stats |
| GET | `/api/dashboard/recent-activities` | Recent activities |
| GET | `/api/dashboard/pipeline-summary` | Sales pipeline |
| GET | `/api/dashboard/charts/{type}` | Get chart data |

#### Lookups

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/lookups` | List all lookup categories |
| GET | `/api/lookups/{category}` | Get items by category |
| POST | `/api/lookups/{category}` | Add lookup item |

---

## 5. WebSocket (SignalR)

### 5.1 Connection

```javascript
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/hubs/crm", {
        accessTokenFactory: () => authToken
    })
    .build();
```

### 5.2 Events

| Event | Payload | Description |
|-------|---------|-------------|
| `EntityCreated` | `{entityType, entityId, data}` | New entity |
| `EntityUpdated` | `{entityType, entityId, data}` | Entity modified |
| `EntityDeleted` | `{entityType, entityId}` | Entity deleted |
| `UserEditing` | `{entityType, entityId, userId, userName}` | User editing |
| `UserStoppedEditing` | `{entityType, entityId, userId}` | User stopped |
| `CampaignProgress` | `{campaignId, progress, status}` | Campaign status |
| `Notification` | `{title, message, type}` | User notification |

---

## Related Documentation

- [Backend Details](../03-backend/README.md)
- [Authentication](../01-architecture/README.md#5-security-architecture)
