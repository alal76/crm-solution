-- Create ModuleUIConfigs table if not exists
CREATE TABLE IF NOT EXISTS ModuleUIConfigs (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    ModuleName VARCHAR(100) NOT NULL UNIQUE,
    IsEnabled TINYINT(1) NOT NULL DEFAULT 1,
    DisplayName VARCHAR(200) NOT NULL,
    Description VARCHAR(500) NULL,
    IconName VARCHAR(100) NOT NULL DEFAULT 'Folder',
    DisplayOrder INT NOT NULL DEFAULT 0,
    TabsConfig TEXT NULL,
    LinkedEntitiesConfig TEXT NULL,
    ListViewConfig TEXT NULL,
    DetailViewConfig TEXT NULL,
    QuickCreateConfig TEXT NULL,
    SearchFilterConfig TEXT NULL,
    ModuleSettings TEXT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME NULL ON UPDATE CURRENT_TIMESTAMP
);

-- Insert default modules
INSERT INTO ModuleUIConfigs (ModuleName, IsEnabled, DisplayName, Description, IconName, DisplayOrder) VALUES
('Customers', 1, 'Customers', 'Manage customer accounts and details', 'People', 1)
ON DUPLICATE KEY UPDATE DisplayName=VALUES(DisplayName);

INSERT INTO ModuleUIConfigs (ModuleName, IsEnabled, DisplayName, Description, IconName, DisplayOrder) VALUES
('Contacts', 1, 'Contacts', 'Manage contact information', 'Contacts', 2)
ON DUPLICATE KEY UPDATE DisplayName=VALUES(DisplayName);

INSERT INTO ModuleUIConfigs (ModuleName, IsEnabled, DisplayName, Description, IconName, DisplayOrder) VALUES
('Leads', 1, 'Leads', 'Track and manage sales leads', 'PersonAdd', 3)
ON DUPLICATE KEY UPDATE DisplayName=VALUES(DisplayName);

INSERT INTO ModuleUIConfigs (ModuleName, IsEnabled, DisplayName, Description, IconName, DisplayOrder) VALUES
('Opportunities', 1, 'Opportunities', 'Manage sales opportunities and deals', 'TrendingUp', 4)
ON DUPLICATE KEY UPDATE DisplayName=VALUES(DisplayName);

INSERT INTO ModuleUIConfigs (ModuleName, IsEnabled, DisplayName, Description, IconName, DisplayOrder) VALUES
('Products', 1, 'Products', 'Manage product catalog', 'Package', 5)
ON DUPLICATE KEY UPDATE DisplayName=VALUES(DisplayName);

INSERT INTO ModuleUIConfigs (ModuleName, IsEnabled, DisplayName, Description, IconName, DisplayOrder) VALUES
('Services', 1, 'Services', 'Manage service offerings', 'Build', 6)
ON DUPLICATE KEY UPDATE DisplayName=VALUES(DisplayName);

INSERT INTO ModuleUIConfigs (ModuleName, IsEnabled, DisplayName, Description, IconName, DisplayOrder) VALUES
('ServiceRequests', 1, 'Service Requests', 'Handle customer service requests', 'SupportAgent', 7)
ON DUPLICATE KEY UPDATE DisplayName=VALUES(DisplayName);

INSERT INTO ModuleUIConfigs (ModuleName, IsEnabled, DisplayName, Description, IconName, DisplayOrder) VALUES
('Campaigns', 1, 'Campaigns', 'Marketing campaign management', 'Campaign', 8)
ON DUPLICATE KEY UPDATE DisplayName=VALUES(DisplayName);

INSERT INTO ModuleUIConfigs (ModuleName, IsEnabled, DisplayName, Description, IconName, DisplayOrder) VALUES
('Quotes', 1, 'Quotes', 'Create and manage quotes', 'Description', 9)
ON DUPLICATE KEY UPDATE DisplayName=VALUES(DisplayName);

INSERT INTO ModuleUIConfigs (ModuleName, IsEnabled, DisplayName, Description, IconName, DisplayOrder) VALUES
('Tasks', 1, 'Tasks', 'Task management and tracking', 'Assignment', 10)
ON DUPLICATE KEY UPDATE DisplayName=VALUES(DisplayName);

INSERT INTO ModuleUIConfigs (ModuleName, IsEnabled, DisplayName, Description, IconName, DisplayOrder) VALUES
('Activities', 1, 'Activities', 'Track activities and interactions', 'Timeline', 11)
ON DUPLICATE KEY UPDATE DisplayName=VALUES(DisplayName);

INSERT INTO ModuleUIConfigs (ModuleName, IsEnabled, DisplayName, Description, IconName, DisplayOrder) VALUES
('Notes', 1, 'Notes', 'Notes and documentation', 'Note', 12)
ON DUPLICATE KEY UPDATE DisplayName=VALUES(DisplayName);

INSERT INTO ModuleUIConfigs (ModuleName, IsEnabled, DisplayName, Description, IconName, DisplayOrder) VALUES
('Workflows', 1, 'Workflows', 'Automation and workflow management', 'AutoAwesome', 13)
ON DUPLICATE KEY UPDATE DisplayName=VALUES(DisplayName);

INSERT INTO ModuleUIConfigs (ModuleName, IsEnabled, DisplayName, Description, IconName, DisplayOrder) VALUES
('CustomerOverview', 1, 'Customer Overview', 'Customer overview and insights', 'PersonSearch', 14)
ON DUPLICATE KEY UPDATE DisplayName=VALUES(DisplayName);
