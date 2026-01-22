CREATE TABLE IF NOT EXISTS `Tags` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Name` varchar(200) NOT NULL,
  `CreatedAt` datetime(6) NOT NULL,
  `UpdatedAt` datetime(6) DEFAULT NULL,
  `IsDeleted` tinyint(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_uca1400_ai_ci;

CREATE TABLE IF NOT EXISTS `EntityTags` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `EntityType` varchar(100) NOT NULL,
  `EntityId` int NOT NULL,
  `TagId` int DEFAULT NULL,
  `Tag` varchar(500) NOT NULL,
  `CreatedAt` datetime(6) NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_EntityTags_Entity` (`EntityType`,`EntityId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_uca1400_ai_ci;

CREATE TABLE IF NOT EXISTS `CustomFields` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `EntityType` varchar(100) NOT NULL,
  `EntityId` int NOT NULL,
  `Key` varchar(200) DEFAULT NULL,
  `Value` TEXT DEFAULT NULL,
  `CreatedAt` datetime(6) NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_CustomFields_Entity` (`EntityType`,`EntityId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_uca1400_ai_ci;

-- Simple idempotent migration: copy whole Tags string as a single tag row when present
INSERT INTO EntityTags (EntityType, EntityId, Tag, CreatedAt)
SELECT 'Products', p.Id, TRIM(p.Tags), NOW()
FROM Products p
WHERE p.Tags IS NOT NULL AND p.Tags <> ''
  AND NOT EXISTS(SELECT 1 FROM EntityTags et WHERE et.EntityType='Products' AND et.EntityId = p.Id AND et.Tag = TRIM(p.Tags));

INSERT INTO EntityTags (EntityType, EntityId, Tag, CreatedAt)
SELECT 'MarketingCampaigns', m.Id, TRIM(m.Tags), NOW()
FROM MarketingCampaigns m
WHERE m.Tags IS NOT NULL AND m.Tags <> ''
  AND NOT EXISTS(SELECT 1 FROM EntityTags et WHERE et.EntityType='MarketingCampaigns' AND et.EntityId = m.Id AND et.Tag = TRIM(m.Tags));

INSERT INTO EntityTags (EntityType, EntityId, Tag, CreatedAt)
SELECT 'Notes', n.Id, TRIM(n.Tags), NOW()
FROM Notes n
WHERE n.Tags IS NOT NULL AND n.Tags <> ''
  AND NOT EXISTS(SELECT 1 FROM EntityTags et WHERE et.EntityType='Notes' AND et.EntityId = n.Id AND et.Tag = TRIM(n.Tags));

INSERT INTO EntityTags (EntityType, EntityId, Tag, CreatedAt)
SELECT 'ServiceRequests', s.Id, TRIM(s.Tags), NOW()
FROM ServiceRequests s
WHERE s.Tags IS NOT NULL AND s.Tags <> ''
  AND NOT EXISTS(SELECT 1 FROM EntityTags et WHERE et.EntityType='ServiceRequests' AND et.EntityId = s.Id AND et.Tag = TRIM(s.Tags));

-- Migrate CustomFields as raw values (idempotent)
INSERT INTO CustomFields (EntityType, EntityId, `Key`, `Value`, CreatedAt)
SELECT 'Products', p.Id, NULL, p.CustomFields, NOW()
FROM Products p
WHERE p.CustomFields IS NOT NULL AND p.CustomFields <> ''
  AND NOT EXISTS(SELECT 1 FROM CustomFields cf WHERE cf.EntityType='Products' AND cf.EntityId = p.Id AND cf.Value IS NOT NULL AND cf.Value = p.CustomFields);

INSERT INTO CustomFields (EntityType, EntityId, `Key`, `Value`, CreatedAt)
SELECT 'MarketingCampaigns', m.Id, NULL, m.CustomFields, NOW()
FROM MarketingCampaigns m
WHERE m.CustomFields IS NOT NULL AND m.CustomFields <> ''
  AND NOT EXISTS(SELECT 1 FROM CustomFields cf WHERE cf.EntityType='MarketingCampaigns' AND cf.EntityId = m.Id AND cf.Value IS NOT NULL AND cf.Value = m.CustomFields);

INSERT INTO CustomFields (EntityType, EntityId, `Key`, `Value`, CreatedAt)
SELECT 'Customers', c.Id, NULL, c.CustomFields, NOW()
FROM Customers c
WHERE c.CustomFields IS NOT NULL AND c.CustomFields <> ''
  AND NOT EXISTS(SELECT 1 FROM CustomFields cf WHERE cf.EntityType='Customers' AND cf.EntityId = c.Id AND cf.Value IS NOT NULL AND cf.Value = c.CustomFields);
