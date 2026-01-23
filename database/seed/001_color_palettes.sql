-- ============================================================================
-- CRM Solution Database Seed Data - Color Palettes (Master Data)
-- Version: 1.0
-- Date: 2026-01-23
-- Description: System color palettes for UI theming
-- ============================================================================

SET NAMES utf8mb4;

-- Clear existing palettes
DELETE FROM ColorPalettes WHERE IsDeleted = 0;

-- ----------------------------------------------------------------------------
-- System Color Palettes - Material Design 3 Inspired
-- ----------------------------------------------------------------------------
INSERT INTO ColorPalettes (Name, Category, Color1, Color2, Color3, Color4, Color5, IsUserDefined, CreatedByUserId, CreatedAt, IsDeleted) VALUES
-- Professional Palettes
('Material Purple', 'professional', '#6750A4', '#625B71', '#7D5260', '#FFFBFE', '#E8DEF8', 0, NULL, NOW(), 0),
('Ocean Blue', 'professional', '#1976D2', '#115293', '#42A5F5', '#E3F2FD', '#BBDEFB', 0, NULL, NOW(), 0),
('Corporate Teal', 'professional', '#00796B', '#004D40', '#26A69A', '#E0F2F1', '#B2DFDB', 0, NULL, NOW(), 0),
('Slate Gray', 'professional', '#455A64', '#263238', '#78909C', '#ECEFF1', '#CFD8DC', 0, NULL, NOW(), 0),
('Royal Indigo', 'professional', '#3F51B5', '#303F9F', '#5C6BC0', '#E8EAF6', '#C5CAE9', 0, NULL, NOW(), 0),

-- Nature Palettes
('Forest Green', 'nature', '#2E7D32', '#1B5E20', '#4CAF50', '#E8F5E9', '#C8E6C9', 0, NULL, NOW(), 0),
('Autumn Harvest', 'nature', '#E65100', '#BF360C', '#FF9800', '#FFF3E0', '#FFE0B2', 0, NULL, NOW(), 0),
('Desert Sand', 'nature', '#8D6E63', '#5D4037', '#A1887F', '#EFEBE9', '#D7CCC8', 0, NULL, NOW(), 0),
('Sky Dawn', 'nature', '#0288D1', '#01579B', '#03A9F4', '#E1F5FE', '#B3E5FC', 0, NULL, NOW(), 0),
('Lavender Fields', 'nature', '#7B1FA2', '#4A148C', '#9C27B0', '#F3E5F5', '#E1BEE7', 0, NULL, NOW(), 0),

-- Vibrant Palettes
('Sunset Orange', 'vibrant', '#F57C00', '#E65100', '#FF9800', '#FFF8E1', '#FFECB3', 0, NULL, NOW(), 0),
('Electric Blue', 'vibrant', '#2196F3', '#1565C0', '#64B5F6', '#E3F2FD', '#90CAF9', 0, NULL, NOW(), 0),
('Hot Pink', 'vibrant', '#C2185B', '#880E4F', '#E91E63', '#FCE4EC', '#F8BBD9', 0, NULL, NOW(), 0),
('Cyber Purple', 'vibrant', '#9C27B0', '#6A1B9A', '#AB47BC', '#F3E5F5', '#CE93D8', 0, NULL, NOW(), 0),
('Neon Green', 'vibrant', '#689F38', '#33691E', '#8BC34A', '#F1F8E9', '#DCEDC8', 0, NULL, NOW(), 0),

-- Warm Palettes
('Terracotta', 'warm', '#D84315', '#BF360C', '#FF5722', '#FBE9E7', '#FFCCBC', 0, NULL, NOW(), 0),
('Golden Amber', 'warm', '#FFA000', '#FF6F00', '#FFC107', '#FFFDE7', '#FFECB3', 0, NULL, NOW(), 0),
('Rose Garden', 'warm', '#AD1457', '#880E4F', '#D81B60', '#FCE4EC', '#F48FB1', 0, NULL, NOW(), 0),
('Burnt Sienna', 'warm', '#795548', '#4E342E', '#8D6E63', '#EFEBE9', '#BCAAA4', 0, NULL, NOW(), 0),
('Coral Reef', 'warm', '#FF7043', '#E64A19', '#FF8A65', '#FBE9E7', '#FFAB91', 0, NULL, NOW(), 0),

-- Cool Palettes
('Arctic Blue', 'cool', '#039BE5', '#0277BD', '#29B6F6', '#E1F5FE', '#81D4FA', 0, NULL, NOW(), 0),
('Midnight Navy', 'cool', '#1A237E', '#0D47A1', '#3949AB', '#E8EAF6', '#9FA8DA', 0, NULL, NOW(), 0),
('Mint Fresh', 'cool', '#00897B', '#00695C', '#26A69A', '#E0F2F1', '#80CBC4', 0, NULL, NOW(), 0),
('Steel Blue', 'cool', '#546E7A', '#37474F', '#78909C', '#ECEFF1', '#B0BEC5', 0, NULL, NOW(), 0),
('Sapphire', 'cool', '#283593', '#1A237E', '#5C6BC0', '#E8EAF6', '#9FA8DA', 0, NULL, NOW(), 0),

-- Dark Theme Palettes
('Dark Material', 'dark', '#BB86FC', '#03DAC6', '#CF6679', '#121212', '#1E1E1E', 0, NULL, NOW(), 0),
('Dark Ocean', 'dark', '#64B5F6', '#4DD0E1', '#81C784', '#0D1B2A', '#1B263B', 0, NULL, NOW(), 0),
('Dark Forest', 'dark', '#81C784', '#AED581', '#FFD54F', '#1B2A1B', '#2E3B2E', 0, NULL, NOW(), 0),
('Dark Sunset', 'dark', '#FF8A80', '#FFD180', '#FF80AB', '#2A1B1B', '#3B2E2E', 0, NULL, NOW(), 0),
('Dark Amethyst', 'dark', '#CE93D8', '#B39DDB', '#80DEEA', '#1B1B2A', '#2E2E3B', 0, NULL, NOW(), 0);
