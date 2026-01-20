# User Management System - Quick Start Guide

## Access the User Management Page

1. **Navigate to:** http://localhost:3000
2. **Login** with your credentials
3. **Go to:** Settings → User Management (from sidebar)

## Key Features

### 1. Create New User
- Click **"Add User"** button
- Fill in required fields:
  - First Name
  - Last Name  
  - Username (unique)
  - Email
  - Password (min 6 characters)
- Select **Role** (Admin, Manager, Sales, Support, Guest)
- Optionally assign **Department** and **User Profile**
- Click **"Create"**

### 2. Edit User
- Click **"Edit"** button in user row
- Update any field (except username)
- Click **"Update"**

### 3. Reset User Password
- Click **"Reset"** button in user row
- Enter new password (min 6 characters)
- Confirm password
- Click **"Reset Password"**

### 4. Manage User Status
- Click **"Activate"** or **"Deactivate"** button
- Changes take effect immediately

### 5. Delete User
- Click **"Delete"** button in user row
- Confirm deletion
- User is soft-deleted (recoverable)

---

## User Roles & Permissions

| Role    | Use Case |
|---------|----------|
| Admin   | System administration, user management |
| Manager | Team oversight, reporting |
| Sales   | Customer & opportunity management |
| Support | Customer support operations |
| Guest   | Read-only access |

---

## Department & Profile Assignment

- **Department** - Organizes users into teams/groups
- **User Profile** - Defines specific permissions:
  - Page access (which modules user can view)
  - Feature permissions (create, edit, delete capabilities)
  - Custom role-based restrictions

---

## Contact Linking

Users can be linked to existing contacts through:
1. User Profile system (inherited from profile assignments)
2. Email matching (automatic in contact integration)
3. Manual assignment during profile setup

---

## API Endpoints (Authenticated Required)

### User Management
- `GET /api/users` - List all users
- `GET /api/users/{id}` - Get user by ID
- `GET /api/users/department/{deptId}` - Get users in department
- `POST /api/users` - Create new user
- `PUT /api/users/{id}` - Update user
- `DELETE /api/users/{id}` - Delete user

### User Profiles
- `GET /api/user-profiles` - List profiles
- `POST /api/user-profiles` - Create profile
- `PUT /api/user-profiles/{id}` - Update profile
- `DELETE /api/user-profiles/{id}` - Delete profile

### Authentication
- `POST /api/auth/reset-password/{userId}` - Admin password reset

---

## Troubleshooting

### User Cannot Login
1. Check if user is "Active" (green chip)
2. Verify password is correct (case-sensitive)
3. Reset password using Admin reset feature

### Cannot Edit User
1. Verify you have Admin role
2. Check user is not deleted
3. Refresh page and try again

### Department/Profile Not Showing
1. Ensure departments are created in Settings → Departments
2. Ensure profiles are created in Settings → User Profiles
3. Refresh page to reload data

---

## Best Practices

✅ **Do:**
- Assign users to appropriate departments
- Use meaningful profile permissions
- Regularly audit active users
- Use strong passwords (8+ characters recommended)
- Document role assignments

❌ **Don't:**
- Share admin accounts
- Leave accounts active after employee departure
- Grant excessive permissions
- Reuse identical passwords across users

---

## Support

For issues or questions:
1. Check application logs: `docker logs crm-api`
2. Verify API health: http://localhost:5001/health
3. Check browser console for frontend errors
4. Review USER_MANAGEMENT_FEATURE.md for technical details
