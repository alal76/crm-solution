-- Seed Departments
-- This script populates the Departments table with standard organizational departments

INSERT INTO Departments (Name, Description, DepartmentCode, IsActive, CreatedAt, IsDeleted) VALUES
('Executive', 'Executive Leadership', 'EXEC', 1, NOW(), 0),
('Sales', 'Sales Department', 'SALES', 1, NOW(), 0),
('Marketing', 'Marketing Department', 'MKTG', 1, NOW(), 0),
('Engineering', 'Engineering and Development', 'ENG', 1, NOW(), 0),
('Finance', 'Finance and Accounting', 'FIN', 1, NOW(), 0),
('Human Resources', 'Human Resources', 'HR', 1, NOW(), 0),
('IT', 'Information Technology', 'IT', 1, NOW(), 0),
('Operations', 'Operations and Logistics', 'OPS', 1, NOW(), 0),
('Customer Support', 'Customer Support and Service', 'SUPPORT', 1, NOW(), 0),
('Legal', 'Legal and Compliance', 'LEGAL', 1, NOW(), 0)
ON DUPLICATE KEY UPDATE Name = VALUES(Name);

-- Seed Default User Groups
-- These groups provide role-based access control
INSERT INTO UserGroups (Name, Description, IsActive, IsDefault, IsSystemAdmin, CreatedAt, IsDeleted) VALUES
('User', 'Basic user access with read permissions', 1, 1, 0, NOW(), 0),
('Manager', 'Manager level with team oversight permissions', 1, 0, 0, NOW(), 0),
('CRM Admin', 'Full CRM administration access', 1, 0, 0, NOW(), 0),
('User Admin', 'User and group management permissions', 1, 0, 0, NOW(), 0)
ON DUPLICATE KEY UPDATE Name = VALUES(Name);
