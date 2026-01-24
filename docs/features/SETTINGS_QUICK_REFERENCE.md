# CRM Settings System - Quick Reference

## 🎯 Access Settings
- **URL**: `http://192.168.0.9:8070/settings`
- **Required Role**: Admin
- **Navigation**: Click "Admin Settings" in top menu

## 📋 Available Tabs

### 1. User Approval (👤)
Manage new user signup requests
- View pending/approved/rejected requests
- Auto-generate passwords for approved users
- Document rejection reasons

### 2. Group Management (👥)
Organize users into groups
- Create/edit/delete groups
- Add/remove group members
- Manage group permissions

### 3. Database Management (💾)
Backup and migrate database
- **Backup**: Save current database state
- **Migrate**: Move to PostgreSQL/SQL Server/Oracle
- **Restore**: Recover from backup
- **Seed Script**: Generate DDL for new environments

### 4. Master Data (📊)
Entity lifecycle management framework (expandable)

## 🔌 API Base URL
```
http://192.168.0.9:5000/api/adminsettings
```

## 📦 Main API Endpoints

### User Approval
```
GET    /approval-requests                    List all approval requests
GET    /approval-requests/{id}               Get specific request
POST   /approval-requests/{id}/approve       Approve user
POST   /approval-requests/{id}/reject        Reject user
```

### Group Management
```
GET    /groups                               List all groups
POST   /groups                               Create group
GET    /groups/{id}                          Get group details
PUT    /groups/{id}                          Update group
DELETE /groups/{id}                          Delete group
GET    /groups/{id}/members                  List group members
POST   /groups/{groupId}/members/{userId}    Add member
DELETE /groups/{groupId}/members/{userId}    Remove member
```

### Database Operations
```
POST   /database/backup                      Create backup
GET    /database/backups                     List backups
POST   /database/restore                     Restore backup
POST   /database/migrate                     Migrate to other database
GET    /database/seed-script                 Get seed script
```

## 🔒 Authentication
All endpoints require:
- Valid JWT token in Authorization header
- Admin role assignment
- Active user session

## 🚀 Common Tasks

### Create a New User Group
1. Go to Settings → Group Management
2. Click "New Group"
3. Enter group name and description
4. Click "Create"

### Approve a Pending User
1. Go to Settings → User Approval
2. Find request in Pending list
3. Click "Approve"
4. Set role and optional department
5. Temporary password auto-generated

### Backup Database
1. Go to Settings → Database Management
2. Click "Create Backup"
3. Enter backup name
4. Choose source database
5. Click "Backup"
6. Monitor backup progress

### Migrate to PostgreSQL
1. Go to Settings → Database Management
2. Click "Migrate"
3. Select PostgreSQL as target
4. Enter connection details (host, port, credentials)
5. Review and execute migration
6. Verify data in new database

### Restore from Backup
1. Go to Settings → Database Management
2. Select backup from list
3. Choose target database
4. Verify checksum
5. Click "Restore"
6. Confirm when complete

## 💡 Tips

✨ **Auto-Generated Passwords**: When approving users, passwords are automatically generated (12 characters, mixed case + numbers + special chars)

✨ **Backup Integrity**: All backups have SHA256 checksums for corruption detection

✨ **Database Support**: Supports 4 database engines with automatic DDL generation for each

✨ **Audit Trail**: All admin actions are logged with timestamp and user information

## 🐛 Troubleshooting

| Issue | Solution |
|-------|----------|
| Can't access settings | Check if logged in as Admin user |
| API returns 401 | Verify JWT token in Authorization header |
| Backup fails | Check database connectivity and disk space |
| Migration fails | Verify target database credentials and engine version |
| Group operations slow | Check user count in group (pagination may help) |

## 📊 System Status

**Last Deployment**: 2026-01-19  
**Backend**: ✅ Running (http://192.168.0.9:5000)  
**Frontend**: ✅ Running (http://192.168.0.9:8070)  
**Database**: ✅ Running (MariaDB on Docker)  
**All Features**: ✅ Operational  

---

For detailed documentation, see: [ADMIN_SETTINGS_GUIDE.md](ADMIN_SETTINGS_GUIDE.md)
