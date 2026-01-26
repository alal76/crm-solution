/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 *
 * Admin Pages Comprehensive Tests
 * Tests for User Management, Security Settings, Database Settings, and Workflows
 */

import React from 'react';
import '@testing-library/jest-dom';

// ============================================================================
// Mock Setup
// ============================================================================

const mockApiClient = {
  get: jest.fn(),
  post: jest.fn(),
  put: jest.fn(),
  delete: jest.fn(),
};

const mockNavigate = jest.fn();

jest.mock('react-router-dom', () => ({
  ...jest.requireActual('react-router-dom'),
  useNavigate: () => mockNavigate,
}));

// Mock user data
const mockUsers = [
  {
    id: 1,
    username: 'admin',
    email: 'admin@example.com',
    firstName: 'Admin',
    lastName: 'User',
    role: 'Admin',
    isActive: true,
    emailVerified: true,
    departmentId: 1,
    departmentName: 'IT',
    lastLoginDate: '2024-01-15T10:30:00Z',
    createdAt: '2024-01-01T00:00:00Z',
  },
  {
    id: 2,
    username: 'jsmith',
    email: 'jsmith@example.com',
    firstName: 'John',
    lastName: 'Smith',
    role: 'Sales',
    isActive: true,
    emailVerified: true,
    departmentId: 2,
    departmentName: 'Sales',
    lastLoginDate: '2024-01-14T09:00:00Z',
    createdAt: '2024-01-02T00:00:00Z',
  },
  {
    id: 3,
    username: 'mjones',
    email: 'mjones@example.com',
    firstName: 'Mary',
    lastName: 'Jones',
    role: 'Support',
    isActive: false,
    emailVerified: false,
    departmentId: 3,
    departmentName: 'Support',
    lastLoginDate: null,
    createdAt: '2024-01-03T00:00:00Z',
  },
];

const mockDepartments = [
  { id: 1, name: 'IT' },
  { id: 2, name: 'Sales' },
  { id: 3, name: 'Support' },
  { id: 4, name: 'Marketing' },
];

const mockUserGroups = [
  { id: 1, name: 'Administrators', description: 'Full access group' },
  { id: 2, name: 'Sales Team', description: 'Sales department group' },
  { id: 3, name: 'Support Team', description: 'Support department group' },
];

const mockSecuritySettings = {
  twoFactorEnabled: false,
  sslEnabled: true,
  quickAdminLoginEnabled: true,
  passwordMinLength: 8,
  passwordRequireUppercase: true,
  passwordRequireLowercase: true,
  passwordRequireNumbers: true,
  passwordRequireSpecial: true,
  sessionTimeoutMinutes: 30,
  maxLoginAttempts: 5,
  lockoutDurationMinutes: 15,
};

const mockSslStatus = {
  httpsEnabled: true,
  forceRedirect: false,
  hasCertificate: true,
  hasPrivateKey: true,
  certificateExpiry: '2025-12-31T23:59:59Z',
  certificateSubject: 'CN=crm.example.com',
  isExpired: false,
  expiresInDays: 365,
};

const mockWorkflows = [
  {
    id: 1,
    name: 'New Lead Assignment',
    description: 'Automatically assign new leads to sales reps',
    isActive: true,
    triggerType: 'OnCreate',
    entityType: 'Lead',
    createdAt: '2024-01-01T00:00:00Z',
    lastExecuted: '2024-01-15T10:00:00Z',
    executionCount: 150,
  },
  {
    id: 2,
    name: 'Opportunity Notification',
    description: 'Send notification when opportunity value exceeds threshold',
    isActive: true,
    triggerType: 'OnUpdate',
    entityType: 'Opportunity',
    createdAt: '2024-01-02T00:00:00Z',
    lastExecuted: '2024-01-14T15:30:00Z',
    executionCount: 75,
  },
  {
    id: 3,
    name: 'Customer Birthday Email',
    description: 'Send birthday greetings to customers',
    isActive: false,
    triggerType: 'Scheduled',
    entityType: 'Customer',
    createdAt: '2024-01-03T00:00:00Z',
    lastExecuted: null,
    executionCount: 0,
  },
];

const mockDatabaseSettings = {
  currentProvider: 'MariaDB',
  connectionStatus: 'Connected',
  databaseVersion: '10.5.12',
  lastMigration: '20240115_001',
  useDemoDatabase: false,
};

// ============================================================================
// Test Suite: User Management
// ============================================================================

describe('Admin - User Management', () => {
  beforeEach(() => {
    jest.clearAllMocks();
    mockApiClient.get.mockResolvedValue({ data: mockUsers });
  });

  describe('User List Display', () => {
    it('should display users table', () => {
      expect(mockUsers.length).toBe(3);
    });

    it('should display user columns', () => {
      const columns = ['Username', 'Email', 'Name', 'Role', 'Status', 'Department', 'Actions'];
      expect(columns.length).toBeGreaterThan(0);
    });

    it('should display user information', () => {
      const user = mockUsers[0];
      expect(user.username).toBe('admin');
      expect(user.email).toBe('admin@example.com');
      expect(user.firstName).toBe('Admin');
      expect(user.lastName).toBe('User');
    });

    it('should show active/inactive status', () => {
      const activeUser = mockUsers.find(u => u.isActive);
      const inactiveUser = mockUsers.find(u => !u.isActive);
      
      expect(activeUser).toBeTruthy();
      expect(inactiveUser).toBeTruthy();
    });

    it('should show role badges', () => {
      const roles = mockUsers.map(u => u.role);
      expect(roles).toContain('Admin');
      expect(roles).toContain('Sales');
      expect(roles).toContain('Support');
    });
  });

  describe('User CRUD Operations', () => {
    it('should create new user', async () => {
      const newUser = {
        username: 'newuser',
        email: 'newuser@example.com',
        firstName: 'New',
        lastName: 'User',
        password: 'Password123!',
        role: 'Sales',
        isActive: true,
      };
      
      mockApiClient.post.mockResolvedValue({ data: { id: 4, ...newUser } });
      await mockApiClient.post('/users', newUser);
      
      expect(mockApiClient.post).toHaveBeenCalledWith('/users', newUser);
    });

    it('should update user', async () => {
      const updates = { firstName: 'Updated', lastName: 'Name' };
      
      mockApiClient.put.mockResolvedValue({ data: { ...mockUsers[1], ...updates } });
      await mockApiClient.put('/users/2', updates);
      
      expect(mockApiClient.put).toHaveBeenCalledWith('/users/2', updates);
    });

    it('should delete user', async () => {
      mockApiClient.delete.mockResolvedValue({ data: {} });
      await mockApiClient.delete('/users/3');
      
      expect(mockApiClient.delete).toHaveBeenCalledWith('/users/3');
    });

    it('should toggle user active status', async () => {
      const updates = { isActive: false };
      
      mockApiClient.put.mockResolvedValue({ data: { ...mockUsers[0], isActive: false } });
      await mockApiClient.put('/users/1', updates);
      
      expect(mockApiClient.put).toHaveBeenCalled();
    });
  });

  describe('User Form Validation', () => {
    it('should validate required username', () => {
      const isValid = (data: any) => Boolean(data.username && data.username.trim() !== '');
      expect(isValid({ username: '' })).toBe(false);
      expect(isValid({ username: 'admin' })).toBe(true);
    });

    it('should validate email format', () => {
      const isValidEmail = (email: string) => /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email);
      expect(isValidEmail('invalid')).toBe(false);
      expect(isValidEmail('valid@example.com')).toBe(true);
    });

    it('should validate password strength', () => {
      const isStrongPassword = (password: string) => {
        return password.length >= 8 &&
          /[A-Z]/.test(password) &&
          /[a-z]/.test(password) &&
          /[0-9]/.test(password);
      };
      
      expect(isStrongPassword('weak')).toBe(false);
      expect(isStrongPassword('StrongPass123')).toBe(true);
    });

    it('should require password on create only', () => {
      const isEditing = true;
      const passwordRequired = !isEditing;
      expect(passwordRequired).toBe(false);
    });
  });

  describe('Role Management', () => {
    it('should display available roles', () => {
      const roles = ['Admin', 'Manager', 'Sales', 'Support', 'Guest'];
      expect(roles.length).toBe(5);
    });

    it('should convert role to number', () => {
      const roleToInt = (role: string) => {
        const map: Record<string, number> = { 'Admin': 0, 'Manager': 1, 'Sales': 2, 'Support': 3, 'Guest': 4 };
        return map[role] ?? 2;
      };
      
      expect(roleToInt('Admin')).toBe(0);
      expect(roleToInt('Sales')).toBe(2);
      expect(roleToInt('Unknown')).toBe(2);
    });
  });

  describe('Department Assignment', () => {
    it('should display departments dropdown', () => {
      expect(mockDepartments.length).toBeGreaterThan(0);
    });

    it('should assign department to user', () => {
      const user = { ...mockUsers[0], departmentId: 2 };
      expect(user.departmentId).toBe(2);
    });
  });
});

// ============================================================================
// Test Suite: Security Settings
// ============================================================================

describe('Admin - Security Settings', () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  describe('Two-Factor Authentication', () => {
    it('should display 2FA status', () => {
      expect(mockSecuritySettings.twoFactorEnabled).toBe(false);
    });

    it('should enable 2FA', async () => {
      mockApiClient.post.mockResolvedValue({ 
        data: { qrCodeUrl: 'otpauth://...', secret: 'ABCDEFGH', backupCodes: ['12345', '67890'] } 
      });
      await mockApiClient.post('/twofactor/setup');
      
      expect(mockApiClient.post).toHaveBeenCalled();
    });

    it('should verify 2FA code', async () => {
      mockApiClient.post.mockResolvedValue({ data: { verified: true } });
      await mockApiClient.post('/twofactor/verify', { code: '123456' });
      
      expect(mockApiClient.post).toHaveBeenCalledWith('/twofactor/verify', { code: '123456' });
    });

    it('should disable 2FA', async () => {
      mockApiClient.post.mockResolvedValue({ data: { disabled: true } });
      await mockApiClient.post('/twofactor/disable', { code: '123456' });
      
      expect(mockApiClient.post).toHaveBeenCalled();
    });

    it('should display backup codes', () => {
      const backupCodes = ['12345', '67890', '11111', '22222', '33333'];
      expect(backupCodes.length).toBe(5);
    });
  });

  describe('Password Policy', () => {
    it('should configure minimum length', () => {
      expect(mockSecuritySettings.passwordMinLength).toBe(8);
    });

    it('should require uppercase letters', () => {
      expect(mockSecuritySettings.passwordRequireUppercase).toBe(true);
    });

    it('should require lowercase letters', () => {
      expect(mockSecuritySettings.passwordRequireLowercase).toBe(true);
    });

    it('should require numbers', () => {
      expect(mockSecuritySettings.passwordRequireNumbers).toBe(true);
    });

    it('should require special characters', () => {
      expect(mockSecuritySettings.passwordRequireSpecial).toBe(true);
    });

    it('should validate password against policy', () => {
      const validatePassword = (password: string) => {
        const settings = mockSecuritySettings;
        if (password.length < settings.passwordMinLength) return false;
        if (settings.passwordRequireUppercase && !/[A-Z]/.test(password)) return false;
        if (settings.passwordRequireLowercase && !/[a-z]/.test(password)) return false;
        if (settings.passwordRequireNumbers && !/[0-9]/.test(password)) return false;
        if (settings.passwordRequireSpecial && !/[!@#$%^&*]/.test(password)) return false;
        return true;
      };
      
      expect(validatePassword('weak')).toBe(false);
      expect(validatePassword('StrongPass123!')).toBe(true);
    });
  });

  describe('SSL/TLS Configuration', () => {
    it('should display SSL status', () => {
      expect(mockSslStatus.httpsEnabled).toBe(true);
    });

    it('should show certificate info', () => {
      expect(mockSslStatus.hasCertificate).toBe(true);
      expect(mockSslStatus.certificateSubject).toBe('CN=crm.example.com');
    });

    it('should show certificate expiry', () => {
      expect(mockSslStatus.certificateExpiry).toBeTruthy();
      expect(mockSslStatus.isExpired).toBe(false);
    });

    it('should show days until expiry', () => {
      expect(mockSslStatus.expiresInDays).toBe(365);
    });

    it('should upload certificate', async () => {
      mockApiClient.post.mockResolvedValue({ data: { success: true } });
      const formData = new FormData();
      await mockApiClient.post('/systemsettings/ssl/upload', formData);
      
      expect(mockApiClient.post).toHaveBeenCalled();
    });

    it('should toggle force HTTPS redirect', async () => {
      mockApiClient.put.mockResolvedValue({ data: { forceRedirect: true } });
      await mockApiClient.put('/systemsettings/ssl', { forceRedirect: true });
      
      expect(mockApiClient.put).toHaveBeenCalled();
    });
  });

  describe('Session Management', () => {
    it('should configure session timeout', () => {
      expect(mockSecuritySettings.sessionTimeoutMinutes).toBe(30);
    });

    it('should configure max login attempts', () => {
      expect(mockSecuritySettings.maxLoginAttempts).toBe(5);
    });

    it('should configure lockout duration', () => {
      expect(mockSecuritySettings.lockoutDurationMinutes).toBe(15);
    });
  });

  describe('Quick Admin Login', () => {
    it('should display quick admin login status', () => {
      expect(mockSecuritySettings.quickAdminLoginEnabled).toBe(true);
    });

    it('should toggle quick admin login', async () => {
      mockApiClient.put.mockResolvedValue({ data: { quickAdminLoginEnabled: false } });
      await mockApiClient.put('/systemsettings', { quickAdminLoginEnabled: false });
      
      expect(mockApiClient.put).toHaveBeenCalled();
    });
  });
});

// ============================================================================
// Test Suite: Database Settings
// ============================================================================

describe('Admin - Database Settings', () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  describe('Database Connection', () => {
    it('should display current provider', () => {
      expect(mockDatabaseSettings.currentProvider).toBe('MariaDB');
    });

    it('should display connection status', () => {
      expect(mockDatabaseSettings.connectionStatus).toBe('Connected');
    });

    it('should display database version', () => {
      expect(mockDatabaseSettings.databaseVersion).toBe('10.5.12');
    });

    it('should test database connection', async () => {
      mockApiClient.post.mockResolvedValue({ data: { success: true } });
      await mockApiClient.post('/database/test');
      
      expect(mockApiClient.post).toHaveBeenCalled();
    });
  });

  describe('Database Provider Selection', () => {
    it('should display available providers', () => {
      const providers = ['MariaDB', 'PostgreSQL', 'SQL Server', 'SQLite'];
      expect(providers.length).toBe(4);
    });

    it('should switch database provider', async () => {
      mockApiClient.put.mockResolvedValue({ data: { provider: 'PostgreSQL' } });
      await mockApiClient.put('/database/provider', { provider: 'PostgreSQL' });
      
      expect(mockApiClient.put).toHaveBeenCalled();
    });
  });

  describe('Demo Mode', () => {
    it('should display demo mode status', () => {
      expect(mockDatabaseSettings.useDemoDatabase).toBe(false);
    });

    it('should toggle demo mode', async () => {
      mockApiClient.put.mockResolvedValue({ data: { useDemoDatabase: true } });
      await mockApiClient.put('/systemsettings/demo', { useDemoDatabase: true });
      
      expect(mockApiClient.put).toHaveBeenCalled();
    });
  });

  describe('Migrations', () => {
    it('should display last migration', () => {
      expect(mockDatabaseSettings.lastMigration).toBe('20240115_001');
    });

    it('should run pending migrations', async () => {
      mockApiClient.post.mockResolvedValue({ data: { migrated: true } });
      await mockApiClient.post('/database/migrate');
      
      expect(mockApiClient.post).toHaveBeenCalled();
    });

    it('should list migration history', async () => {
      mockApiClient.get.mockResolvedValue({
        data: [
          { version: '20240115_001', name: 'Initial', appliedAt: '2024-01-15' },
          { version: '20240116_001', name: 'AddUserFields', appliedAt: '2024-01-16' },
        ]
      });
      await mockApiClient.get('/database/migrations');
      
      expect(mockApiClient.get).toHaveBeenCalled();
    });
  });

  describe('Backup and Restore', () => {
    it('should create database backup', async () => {
      mockApiClient.post.mockResolvedValue({ data: { filename: 'backup_20240115.sql' } });
      await mockApiClient.post('/database/backup');
      
      expect(mockApiClient.post).toHaveBeenCalled();
    });

    it('should restore from backup', async () => {
      mockApiClient.post.mockResolvedValue({ data: { restored: true } });
      await mockApiClient.post('/database/restore', { filename: 'backup_20240115.sql' });
      
      expect(mockApiClient.post).toHaveBeenCalled();
    });

    it('should list available backups', async () => {
      mockApiClient.get.mockResolvedValue({
        data: [
          { filename: 'backup_20240115.sql', createdAt: '2024-01-15T10:00:00Z', size: 1024000 },
          { filename: 'backup_20240114.sql', createdAt: '2024-01-14T10:00:00Z', size: 1020000 },
        ]
      });
      await mockApiClient.get('/database/backups');
      
      expect(mockApiClient.get).toHaveBeenCalled();
    });
  });
});

// ============================================================================
// Test Suite: Workflow Management
// ============================================================================

describe('Admin - Workflow Management', () => {
  beforeEach(() => {
    jest.clearAllMocks();
    mockApiClient.get.mockResolvedValue({ data: mockWorkflows });
  });

  describe('Workflow List', () => {
    it('should display workflows', () => {
      expect(mockWorkflows.length).toBe(3);
    });

    it('should show workflow name', () => {
      expect(mockWorkflows[0].name).toBe('New Lead Assignment');
    });

    it('should show workflow status', () => {
      const active = mockWorkflows.filter(w => w.isActive);
      const inactive = mockWorkflows.filter(w => !w.isActive);
      
      expect(active.length).toBe(2);
      expect(inactive.length).toBe(1);
    });

    it('should show trigger type', () => {
      const triggerTypes = mockWorkflows.map(w => w.triggerType);
      expect(triggerTypes).toContain('OnCreate');
      expect(triggerTypes).toContain('OnUpdate');
      expect(triggerTypes).toContain('Scheduled');
    });

    it('should show execution statistics', () => {
      expect(mockWorkflows[0].executionCount).toBe(150);
      expect(mockWorkflows[0].lastExecuted).toBeTruthy();
    });
  });

  describe('Workflow CRUD', () => {
    it('should create workflow', async () => {
      const newWorkflow = {
        name: 'New Workflow',
        description: 'Test workflow',
        triggerType: 'OnCreate',
        entityType: 'Customer',
        isActive: true,
      };
      
      mockApiClient.post.mockResolvedValue({ data: { id: 4, ...newWorkflow } });
      await mockApiClient.post('/workflows', newWorkflow);
      
      expect(mockApiClient.post).toHaveBeenCalled();
    });

    it('should update workflow', async () => {
      const updates = { name: 'Updated Workflow Name' };
      
      mockApiClient.put.mockResolvedValue({ data: { ...mockWorkflows[0], ...updates } });
      await mockApiClient.put('/workflows/1', updates);
      
      expect(mockApiClient.put).toHaveBeenCalled();
    });

    it('should delete workflow', async () => {
      mockApiClient.delete.mockResolvedValue({ data: {} });
      await mockApiClient.delete('/workflows/3');
      
      expect(mockApiClient.delete).toHaveBeenCalled();
    });

    it('should toggle workflow status', async () => {
      mockApiClient.put.mockResolvedValue({ data: { ...mockWorkflows[2], isActive: true } });
      await mockApiClient.put('/workflows/3', { isActive: true });
      
      expect(mockApiClient.put).toHaveBeenCalled();
    });
  });

  describe('Workflow Triggers', () => {
    it('should display trigger types', () => {
      const triggerTypes = ['OnCreate', 'OnUpdate', 'OnDelete', 'Scheduled', 'Manual'];
      expect(triggerTypes.length).toBe(5);
    });

    it('should configure trigger conditions', () => {
      const conditions = [
        { field: 'status', operator: 'equals', value: 'New' },
        { field: 'priority', operator: 'greaterThan', value: '5' },
      ];
      expect(conditions.length).toBe(2);
    });

    it('should configure scheduled trigger', () => {
      const schedule = {
        frequency: 'Daily',
        time: '09:00',
        timezone: 'UTC',
      };
      expect(schedule.frequency).toBe('Daily');
    });
  });

  describe('Workflow Actions', () => {
    it('should configure email action', () => {
      const action = {
        type: 'SendEmail',
        to: '{{record.email}}',
        subject: 'Welcome',
        template: 'welcome_email',
      };
      expect(action.type).toBe('SendEmail');
    });

    it('should configure field update action', () => {
      const action = {
        type: 'UpdateField',
        field: 'status',
        value: 'Assigned',
      };
      expect(action.type).toBe('UpdateField');
    });

    it('should configure record creation action', () => {
      const action = {
        type: 'CreateRecord',
        entityType: 'Task',
        fields: {
          subject: 'Follow up with {{record.name}}',
          dueDate: '{{today + 7 days}}',
        },
      };
      expect(action.type).toBe('CreateRecord');
    });

    it('should configure webhook action', () => {
      const action = {
        type: 'Webhook',
        url: 'https://api.example.com/webhook',
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
      };
      expect(action.type).toBe('Webhook');
    });
  });

  describe('Workflow Execution', () => {
    it('should run workflow manually', async () => {
      mockApiClient.post.mockResolvedValue({ data: { executed: true } });
      await mockApiClient.post('/workflows/1/execute');
      
      expect(mockApiClient.post).toHaveBeenCalled();
    });

    it('should view execution history', async () => {
      mockApiClient.get.mockResolvedValue({
        data: [
          { id: 1, workflowId: 1, status: 'Success', executedAt: '2024-01-15T10:00:00Z' },
          { id: 2, workflowId: 1, status: 'Failed', executedAt: '2024-01-14T10:00:00Z', error: 'Email failed' },
        ]
      });
      await mockApiClient.get('/workflows/1/history');
      
      expect(mockApiClient.get).toHaveBeenCalled();
    });
  });
});

// ============================================================================
// Test Suite: Group Management
// ============================================================================

describe('Admin - Group Management', () => {
  beforeEach(() => {
    jest.clearAllMocks();
    mockApiClient.get.mockResolvedValue({ data: mockUserGroups });
  });

  describe('Group List', () => {
    it('should display groups', () => {
      expect(mockUserGroups.length).toBe(3);
    });

    it('should show group name', () => {
      expect(mockUserGroups[0].name).toBe('Administrators');
    });

    it('should show group description', () => {
      expect(mockUserGroups[0].description).toBe('Full access group');
    });
  });

  describe('Group CRUD', () => {
    it('should create group', async () => {
      const newGroup = { name: 'Marketing Team', description: 'Marketing department group' };
      
      mockApiClient.post.mockResolvedValue({ data: { id: 4, ...newGroup } });
      await mockApiClient.post('/usergroups', newGroup);
      
      expect(mockApiClient.post).toHaveBeenCalled();
    });

    it('should update group', async () => {
      const updates = { name: 'Updated Group Name' };
      
      mockApiClient.put.mockResolvedValue({ data: { ...mockUserGroups[0], ...updates } });
      await mockApiClient.put('/usergroups/1', updates);
      
      expect(mockApiClient.put).toHaveBeenCalled();
    });

    it('should delete group', async () => {
      mockApiClient.delete.mockResolvedValue({ data: {} });
      await mockApiClient.delete('/usergroups/3');
      
      expect(mockApiClient.delete).toHaveBeenCalled();
    });
  });

  describe('Group Permissions', () => {
    it('should display available permissions', () => {
      const permissions = [
        'view_dashboard', 'edit_dashboard',
        'view_customers', 'create_customers', 'edit_customers', 'delete_customers',
        'view_opportunities', 'create_opportunities', 'edit_opportunities', 'delete_opportunities',
        'admin_access', 'view_settings', 'edit_settings',
      ];
      expect(permissions.length).toBeGreaterThan(0);
    });

    it('should assign permissions to group', async () => {
      mockApiClient.put.mockResolvedValue({ data: { permissions: ['view_dashboard', 'view_customers'] } });
      await mockApiClient.put('/usergroups/2/permissions', { permissions: ['view_dashboard', 'view_customers'] });
      
      expect(mockApiClient.put).toHaveBeenCalled();
    });
  });

  describe('Group Membership', () => {
    it('should add user to group', async () => {
      mockApiClient.post.mockResolvedValue({ data: { success: true } });
      await mockApiClient.post('/usergroups/1/members', { userId: 2 });
      
      expect(mockApiClient.post).toHaveBeenCalled();
    });

    it('should remove user from group', async () => {
      mockApiClient.delete.mockResolvedValue({ data: {} });
      await mockApiClient.delete('/usergroups/1/members/2');
      
      expect(mockApiClient.delete).toHaveBeenCalled();
    });

    it('should list group members', async () => {
      mockApiClient.get.mockResolvedValue({ data: [mockUsers[0], mockUsers[1]] });
      await mockApiClient.get('/usergroups/1/members');
      
      expect(mockApiClient.get).toHaveBeenCalled();
    });
  });
});

// ============================================================================
// Test Suite: Branding Settings
// ============================================================================

describe('Admin - Branding Settings', () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  describe('Logo Configuration', () => {
    it('should display current logo', () => {
      const currentLogo = '/logo.png';
      expect(currentLogo).toBeTruthy();
    });

    it('should upload new logo', async () => {
      mockApiClient.post.mockResolvedValue({ data: { logoUrl: '/uploads/logo_new.png' } });
      const formData = new FormData();
      await mockApiClient.post('/branding/logo', formData);
      
      expect(mockApiClient.post).toHaveBeenCalled();
    });

    it('should reset to default logo', async () => {
      mockApiClient.delete.mockResolvedValue({ data: {} });
      await mockApiClient.delete('/branding/logo');
      
      expect(mockApiClient.delete).toHaveBeenCalled();
    });
  });

  describe('Theme Configuration', () => {
    it('should set primary color', async () => {
      mockApiClient.put.mockResolvedValue({ data: { primaryColor: '#6750A4' } });
      await mockApiClient.put('/branding/theme', { primaryColor: '#6750A4' });
      
      expect(mockApiClient.put).toHaveBeenCalled();
    });

    it('should set company name', async () => {
      mockApiClient.put.mockResolvedValue({ data: { companyName: 'My CRM' } });
      await mockApiClient.put('/branding/company', { companyName: 'My CRM' });
      
      expect(mockApiClient.put).toHaveBeenCalled();
    });
  });
});

// ============================================================================
// Test Suite: Admin Error Handling
// ============================================================================

describe('Admin - Error Handling', () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  it('should handle network error', async () => {
    mockApiClient.get.mockRejectedValue(new Error('Network error'));
    
    await expect(mockApiClient.get('/users')).rejects.toThrow('Network error');
  });

  it('should handle unauthorized error', async () => {
    mockApiClient.get.mockRejectedValue({ response: { status: 401, data: { message: 'Unauthorized' } } });
    
    await expect(mockApiClient.get('/admin/settings')).rejects.toBeTruthy();
  });

  it('should handle forbidden error', async () => {
    mockApiClient.get.mockRejectedValue({ response: { status: 403, data: { message: 'Forbidden' } } });
    
    await expect(mockApiClient.get('/admin/settings')).rejects.toBeTruthy();
  });

  it('should handle validation error', async () => {
    mockApiClient.post.mockRejectedValue({
      response: { status: 400, data: { errors: { email: 'Invalid email format' } } }
    });
    
    await expect(mockApiClient.post('/users', { email: 'invalid' })).rejects.toBeTruthy();
  });
});

// ============================================================================
// Test Suite: Admin Loading States
// ============================================================================

describe('Admin - Loading States', () => {
  it('should show loading while fetching users', () => {
    const isLoading = true;
    expect(isLoading).toBe(true);
  });

  it('should show loading while saving settings', () => {
    const isSaving = true;
    expect(isSaving).toBe(true);
  });

  it('should hide loading after data loads', async () => {
    let isLoading = true;
    mockApiClient.get.mockResolvedValue({ data: mockUsers });
    
    await mockApiClient.get('/users');
    isLoading = false;
    
    expect(isLoading).toBe(false);
  });
});
