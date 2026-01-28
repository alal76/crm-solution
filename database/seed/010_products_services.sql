-- ============================================================================
-- CRM Solution Database Seed Data - Products and Services Catalog
-- Version: 2.0
-- Date: 2026-01-28
-- Description: Comprehensive IT products and services catalog with pricing
-- ============================================================================

SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

-- Define a comprehensive INSERT statement with all required columns
-- ProductType: 0=Product, 1=Service, 2=Bundle
-- Status: 0=Draft, 1=Active, 2=Discontinued
-- ServiceTier: 0=None, 1=Basic, 2=Standard, 3=Premium, 4=Enterprise
-- UnitOfMeasure: 0=Each, 1=Hour, 2=Day, 3=Month, 4=Year, 5=License
-- BillingFrequency: 0=OneTime, 1=Monthly, 2=Quarterly, 3=Annual, 4=Hourly
-- PricingModel: 0=Fixed, 1=Tiered, 2=Volume, 3=UsageBased
-- RevenueRecognition: 0=Immediate, 1=Deferred, 2=ProRated

-- ============================================================================
-- HARDWARE PRODUCTS
-- ============================================================================

-- ----------------------------------------------------------------------------
-- Hardware - End-User Computing
-- ----------------------------------------------------------------------------
INSERT INTO Products (
    Name, Description, ShortDescription, SKU, ProductCode, Category, SubCategory,
    ProductType, Status, ServiceTier, IsService, IsSubscription,
    Price, Cost, ListPrice, Margin, UnitOfMeasure, MinimumQuantity, QuantityIncrement,
    BillingFrequency, PricingModel, Tags,
    DefaultContractTerm, MinimumContractTerm,
    WeeklyTermDiscount, MonthlyTermDiscount, QuarterlyTermDiscount, SemiAnnualTermDiscount,
    AnnualTermDiscount, TwoYearTermDiscount, ThreeYearTermDiscount, MaxTermDiscount,
    MaxVolumeDiscount, MaxTotalDiscount, BillableHourIncrement,
    IncludesOnsiteWork, TravelIncluded, MaterialsIncluded,
    IsTaxable, RevenueRecognition, Quantity, TrackInventory, AllowBackorder,
    IsHazardous, AutoRenewal, ExtendedWarrantyAvailable,
    TotalSold, TotalRevenue, AverageRating, ReviewCount,
    IsFeatured, IsBestSeller, IsNew, IsOnSale, IsActive, IsVisible, IsPurchasable,
    ImageUrl, CreatedAt, IsDeleted
) VALUES
-- Desktops
('Desktop Workstation - Entry Level', 'Entry-level desktop workstation for general office use. Includes Intel i5 processor, 8GB RAM, 256GB SSD.', 'Entry desktop for office use', 'HW-EUC-DT-001', 'DT-ENTRY', 'Hardware', 'End-User Computing', 0, 1, 0, 0, 0, 599.99, 479.99, 699.99, 25.00, 0, 1, 1, 0, 0, 'desktop,workstation,entry,office', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.25, 0, 0, 0, 1, 0, 50, 1, 1, 0, 0, 1, 0, 0, 4.2, 0, 0, 0, 1, 0, 1, 1, 1, '/images/products/desktop-entry.jpg', NOW(), 0),
('Desktop Workstation - Professional', 'Professional desktop workstation with Intel i7 processor, 16GB RAM, 512GB SSD, dedicated graphics.', 'Professional desktop workstation', 'HW-EUC-DT-002', 'DT-PRO', 'Hardware', 'End-User Computing', 0, 1, 0, 0, 0, 1299.99, 999.99, 1499.99, 30.00, 0, 1, 1, 0, 0, 'desktop,workstation,professional', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.25, 0, 0, 0, 1, 0, 30, 1, 1, 0, 0, 1, 0, 0, 4.5, 0, 0, 1, 1, 0, 1, 1, 1, '/images/products/desktop-pro.jpg', NOW(), 0),
('Desktop Workstation - High Performance', 'High-performance desktop for CAD/engineering. Intel i9, 32GB RAM, 1TB NVMe SSD, RTX graphics.', 'High-performance engineering desktop', 'HW-EUC-DT-003', 'DT-PERF', 'Hardware', 'End-User Computing', 0, 1, 0, 0, 0, 2499.99, 1874.99, 2999.99, 33.33, 0, 1, 1, 0, 0, 'desktop,workstation,cad,engineering', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.25, 0, 0, 0, 1, 0, 15, 1, 1, 0, 0, 1, 0, 0, 4.8, 0, 1, 0, 1, 0, 1, 1, 1, '/images/products/desktop-perf.jpg', NOW(), 0),
-- Laptops
('Laptop - Business Standard', 'Business laptop with 14" display, Intel i5, 8GB RAM, 256GB SSD. 3-year warranty.', 'Standard business laptop', 'HW-EUC-LT-001', 'LT-STD', 'Hardware', 'End-User Computing', 0, 1, 0, 0, 0, 899.99, 719.99, 1099.99, 25.00, 0, 1, 1, 0, 0, 'laptop,business,standard', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.25, 0, 0, 0, 1, 0, 75, 1, 1, 0, 0, 1, 0, 0, 4.3, 0, 0, 1, 0, 0, 1, 1, 1, '/images/products/laptop-std.jpg', NOW(), 0),
('Laptop - Business Professional', 'Professional laptop with 15.6" display, Intel i7, 16GB RAM, 512GB SSD. Premium build quality.', 'Professional business laptop', 'HW-EUC-LT-002', 'LT-PRO', 'Hardware', 'End-User Computing', 0, 1, 0, 0, 0, 1599.99, 1199.99, 1899.99, 33.33, 0, 1, 1, 0, 0, 'laptop,business,professional', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.25, 0, 0, 0, 1, 0, 50, 1, 1, 0, 0, 1, 0, 0, 4.6, 0, 0, 0, 1, 0, 1, 1, 1, '/images/products/laptop-pro.jpg', NOW(), 0),
('Laptop - Executive Ultrabook', 'Premium ultrabook with 14" 4K display, Intel i7, 32GB RAM, 1TB SSD. Carbon fiber chassis.', 'Executive ultrabook laptop', 'HW-EUC-LT-003', 'LT-EXEC', 'Hardware', 'End-User Computing', 0, 1, 0, 0, 0, 2799.99, 2099.99, 3299.99, 33.33, 0, 1, 1, 0, 0, 'laptop,ultrabook,executive', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.25, 0, 0, 0, 1, 0, 20, 1, 1, 0, 0, 1, 0, 0, 4.9, 0, 1, 0, 1, 0, 1, 1, 1, '/images/products/laptop-exec.jpg', NOW(), 0),
-- Thin Clients and Peripherals
('Thin Client Device', 'Thin client for VDI environments. Dual-core processor, 4GB RAM, flash storage.', 'Thin client for VDI', 'HW-EUC-TC-001', 'TC-VDI', 'Hardware', 'End-User Computing', 0, 1, 0, 0, 0, 399.99, 319.99, 499.99, 25.00, 0, 1, 1, 0, 0, 'thin client,vdi,endpoint', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.25, 0, 0, 0, 1, 0, 100, 1, 1, 0, 0, 0, 0, 0, 4.1, 0, 0, 0, 0, 0, 1, 1, 1, '/images/products/thin-client.jpg', NOW(), 0),
('Monitor - 24" Full HD', '24-inch Full HD IPS monitor with HDMI and DisplayPort. Adjustable stand.', '24" Full HD monitor', 'HW-EUC-MON-001', 'MON-24FHD', 'Hardware', 'End-User Computing', 0, 1, 0, 0, 0, 249.99, 199.99, 299.99, 25.00, 0, 1, 1, 0, 0, 'monitor,display,24inch', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.25, 0, 0, 0, 1, 0, 150, 1, 1, 0, 0, 0, 0, 0, 4.4, 0, 0, 1, 0, 0, 1, 1, 1, '/images/products/monitor-24.jpg', NOW(), 0),
('Monitor - 27" 4K Professional', '27-inch 4K UHD IPS monitor with USB-C docking. Color accurate for creative work.', '27" 4K professional monitor', 'HW-EUC-MON-002', 'MON-27-4K', 'Hardware', 'End-User Computing', 0, 1, 0, 0, 0, 599.99, 449.99, 749.99, 33.33, 0, 1, 1, 0, 0, 'monitor,display,4k,professional', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.25, 0, 0, 0, 1, 0, 60, 1, 1, 0, 0, 0, 0, 0, 4.7, 0, 1, 0, 0, 0, 1, 1, 1, '/images/products/monitor-27-4k.jpg', NOW(), 0),
('Docking Station - USB-C', 'Universal USB-C docking station with dual display support, Ethernet, and USB ports.', 'USB-C docking station', 'HW-EUC-DOCK-001', 'DOCK-USBC', 'Hardware', 'End-User Computing', 0, 1, 0, 0, 0, 249.99, 187.49, 299.99, 33.33, 0, 1, 1, 0, 0, 'docking,usb-c,accessories', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.25, 0, 0, 0, 1, 0, 80, 1, 1, 0, 0, 0, 0, 0, 4.5, 0, 0, 0, 0, 0, 1, 1, 1, '/images/products/docking-usbc.jpg', NOW(), 0);

-- ----------------------------------------------------------------------------
-- Hardware - Networking Equipment
-- ----------------------------------------------------------------------------
INSERT INTO Products (
    Name, Description, ShortDescription, SKU, ProductCode, Category, SubCategory,
    ProductType, Status, ServiceTier, IsService, IsSubscription,
    Price, Cost, ListPrice, Margin, UnitOfMeasure, MinimumQuantity, QuantityIncrement,
    BillingFrequency, PricingModel, Tags,
    DefaultContractTerm, MinimumContractTerm,
    WeeklyTermDiscount, MonthlyTermDiscount, QuarterlyTermDiscount, SemiAnnualTermDiscount,
    AnnualTermDiscount, TwoYearTermDiscount, ThreeYearTermDiscount, MaxTermDiscount,
    MaxVolumeDiscount, MaxTotalDiscount, BillableHourIncrement,
    IncludesOnsiteWork, TravelIncluded, MaterialsIncluded,
    IsTaxable, RevenueRecognition, Quantity, TrackInventory, AllowBackorder,
    IsHazardous, AutoRenewal, ExtendedWarrantyAvailable,
    TotalSold, TotalRevenue, AverageRating, ReviewCount,
    IsFeatured, IsBestSeller, IsNew, IsOnSale, IsActive, IsVisible, IsPurchasable,
    ImageUrl, CreatedAt, IsDeleted
) VALUES
('Managed Switch - 24 Port', '24-port Gigabit managed switch with 4 SFP+ uplink ports. Layer 2/3 switching.', '24-port managed switch', 'HW-NET-SW-001', 'SW-24G', 'Hardware', 'Networking Equipment', 0, 1, 0, 0, 0, 799.99, 639.99, 999.99, 25.00, 0, 1, 1, 0, 0, 'switch,network,managed', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.25, 0, 0, 0, 1, 0, 25, 1, 1, 0, 0, 1, 0, 0, 4.5, 0, 0, 1, 0, 0, 1, 1, 1, '/images/products/switch-24.jpg', NOW(), 0),
('Managed Switch - 48 Port PoE', '48-port Gigabit managed switch with 4 SFP+ uplink ports. PoE+ on all ports.', '48-port PoE managed switch', 'HW-NET-SW-002', 'SW-48G-POE', 'Hardware', 'Networking Equipment', 0, 1, 0, 0, 0, 2499.99, 1874.99, 2999.99, 33.33, 0, 1, 1, 0, 0, 'switch,network,poe,48port', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.25, 0, 0, 0, 1, 0, 15, 1, 1, 0, 0, 1, 0, 0, 4.6, 0, 0, 0, 1, 0, 1, 1, 1, '/images/products/switch-48-poe.jpg', NOW(), 0),
('Wireless Access Point - Enterprise', 'Enterprise Wi-Fi 6 access point. Tri-band, MU-MIMO, supports 500+ clients.', 'Enterprise Wi-Fi 6 AP', 'HW-NET-AP-001', 'AP-WIFI6', 'Hardware', 'Networking Equipment', 0, 1, 0, 0, 0, 599.99, 449.99, 749.99, 33.33, 0, 1, 1, 0, 0, 'wireless,wifi6,access point', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.25, 0, 0, 0, 1, 0, 40, 1, 1, 0, 0, 1, 0, 0, 4.7, 0, 1, 0, 1, 0, 1, 1, 1, '/images/products/ap-wifi6.jpg', NOW(), 0),
('Wireless Access Point - Outdoor', 'Outdoor Wi-Fi 6 access point with IP67 rating. Long range, directional antenna.', 'Outdoor Wi-Fi 6 AP', 'HW-NET-AP-002', 'AP-OUTDOOR', 'Hardware', 'Networking Equipment', 0, 1, 0, 0, 0, 899.99, 674.99, 1099.99, 33.33, 0, 1, 1, 0, 0, 'wireless,outdoor,access point', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.25, 0, 0, 0, 1, 0, 20, 1, 1, 0, 0, 1, 0, 0, 4.4, 0, 0, 0, 0, 0, 1, 1, 1, '/images/products/ap-outdoor.jpg', NOW(), 0),
('Firewall - SMB', 'SMB firewall with UTM features. 1Gbps throughput, VPN, IDS/IPS, content filtering.', 'SMB unified threat management firewall', 'HW-NET-FW-001', 'FW-SMB', 'Hardware', 'Networking Equipment', 0, 1, 0, 0, 0, 1999.99, 1499.99, 2499.99, 33.33, 0, 1, 1, 0, 0, 'firewall,security,smb', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.25, 0, 0, 0, 1, 0, 18, 1, 1, 0, 0, 1, 0, 0, 4.6, 0, 0, 1, 0, 0, 1, 1, 1, '/images/products/firewall-smb.jpg', NOW(), 0),
('Firewall - Enterprise', 'Enterprise firewall with 10Gbps throughput. Advanced threat protection, SSL inspection.', 'Enterprise next-gen firewall', 'HW-NET-FW-002', 'FW-ENT', 'Hardware', 'Networking Equipment', 0, 1, 0, 0, 0, 9999.99, 6999.99, 12999.99, 43.00, 0, 1, 1, 0, 0, 'firewall,security,enterprise', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.25, 0, 0, 0, 1, 0, 5, 1, 1, 0, 0, 1, 0, 0, 4.8, 0, 1, 0, 0, 0, 1, 1, 1, '/images/products/firewall-ent.jpg', NOW(), 0),
('Router - Enterprise SD-WAN', 'Enterprise router with SD-WAN capability. Multi-WAN, BGP/OSPF support.', 'Enterprise SD-WAN router', 'HW-NET-RT-001', 'RT-ENT', 'Hardware', 'Networking Equipment', 0, 1, 0, 0, 0, 4999.99, 3749.99, 5999.99, 33.33, 0, 1, 1, 0, 0, 'router,sd-wan,enterprise', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.25, 0, 0, 0, 1, 0, 10, 1, 1, 0, 0, 1, 0, 0, 4.7, 0, 0, 0, 1, 0, 1, 1, 1, '/images/products/router-sdwan.jpg', NOW(), 0),
('Network Patch Panel - 24 Port Cat6A', '24-port Cat6A patch panel for rack mounting. Shielded, TIA/EIA compliant.', '24-port Cat6A patch panel', 'HW-NET-PP-001', 'PP-24-6A', 'Hardware', 'Networking Equipment', 0, 1, 0, 0, 0, 149.99, 112.49, 199.99, 33.33, 0, 1, 1, 0, 0, 'patch panel,cabling,network', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.25, 0, 0, 0, 1, 0, 50, 1, 1, 0, 0, 0, 0, 0, 4.3, 0, 0, 0, 0, 0, 1, 1, 1, '/images/products/patch-panel.jpg', NOW(), 0);

-- ----------------------------------------------------------------------------
-- Hardware - Server Infrastructure
-- ----------------------------------------------------------------------------
INSERT INTO Products (
    Name, Description, ShortDescription, SKU, ProductCode, Category, SubCategory,
    ProductType, Status, ServiceTier, IsService, IsSubscription,
    Price, Cost, ListPrice, Margin, UnitOfMeasure, MinimumQuantity, QuantityIncrement,
    BillingFrequency, PricingModel, Tags,
    DefaultContractTerm, MinimumContractTerm,
    WeeklyTermDiscount, MonthlyTermDiscount, QuarterlyTermDiscount, SemiAnnualTermDiscount,
    AnnualTermDiscount, TwoYearTermDiscount, ThreeYearTermDiscount, MaxTermDiscount,
    MaxVolumeDiscount, MaxTotalDiscount, BillableHourIncrement,
    IncludesOnsiteWork, TravelIncluded, MaterialsIncluded,
    IsTaxable, RevenueRecognition, Quantity, TrackInventory, AllowBackorder,
    IsHazardous, AutoRenewal, ExtendedWarrantyAvailable,
    TotalSold, TotalRevenue, AverageRating, ReviewCount,
    IsFeatured, IsBestSeller, IsNew, IsOnSale, IsActive, IsVisible, IsPurchasable,
    ImageUrl, CreatedAt, IsDeleted
) VALUES
('Rack Server - Entry 1U', '1U rack server with Intel Xeon Silver, 32GB RAM, 2x480GB SSD. iLO management.', 'Entry rack server', 'HW-SRV-RK-001', 'SRV-1U-ENT', 'Hardware', 'Server Infrastructure', 0, 1, 0, 0, 0, 3999.99, 2799.99, 4999.99, 42.86, 0, 1, 1, 0, 0, 'server,rack,1u,entry', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.25, 0, 0, 0, 1, 0, 8, 1, 1, 0, 0, 1, 0, 0, 4.6, 0, 0, 1, 0, 0, 1, 1, 1, '/images/products/server-1u.jpg', NOW(), 0),
('Rack Server - Standard 2U', '2U rack server with dual Intel Xeon Gold, 128GB RAM, 4x1.2TB SAS. Redundant PSU.', 'Standard 2U rack server', 'HW-SRV-RK-002', 'SRV-2U-STD', 'Hardware', 'Server Infrastructure', 0, 1, 0, 0, 0, 12999.99, 9099.99, 15999.99, 42.86, 0, 1, 1, 0, 0, 'server,rack,2u,standard', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.25, 0, 0, 0, 1, 0, 5, 1, 1, 0, 0, 1, 0, 0, 4.7, 0, 1, 0, 0, 0, 1, 1, 1, '/images/products/server-2u.jpg', NOW(), 0),
('Tower Server - SMB', 'Tower server for small business. Intel Xeon E, 16GB RAM, 2x1TB HDD. Quiet operation.', 'SMB tower server', 'HW-SRV-TW-001', 'SRV-TWR-SMB', 'Hardware', 'Server Infrastructure', 0, 1, 0, 0, 0, 2499.99, 1749.99, 2999.99, 42.86, 0, 1, 1, 0, 0, 'server,tower,smb', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.25, 0, 0, 0, 1, 0, 12, 1, 1, 0, 0, 1, 0, 0, 4.4, 0, 0, 0, 0, 0, 1, 1, 1, '/images/products/server-tower.jpg', NOW(), 0),
('NAS Storage - 4 Bay', '4-bay NAS with 16TB raw capacity. RAID 5/6 support, 2.5GbE networking.', '4-bay NAS storage', 'HW-SRV-NAS-001', 'NAS-4BAY', 'Hardware', 'Server Infrastructure', 0, 1, 0, 0, 0, 1499.99, 1124.99, 1799.99, 33.33, 0, 1, 1, 0, 0, 'nas,storage,4bay', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.25, 0, 0, 0, 1, 0, 20, 1, 1, 0, 0, 1, 0, 0, 4.5, 0, 0, 1, 0, 0, 1, 1, 1, '/images/products/nas-4bay.jpg', NOW(), 0),
('NAS Storage - 8 Bay Enterprise', '8-bay enterprise NAS with 10GbE, SSD caching, and enterprise data protection.', '8-bay enterprise NAS', 'HW-SRV-NAS-002', 'NAS-8BAY-ENT', 'Hardware', 'Server Infrastructure', 0, 1, 0, 0, 0, 4999.99, 3749.99, 5999.99, 33.33, 0, 1, 1, 0, 0, 'nas,storage,enterprise,8bay', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.25, 0, 0, 0, 1, 0, 8, 1, 1, 0, 0, 1, 0, 0, 4.7, 0, 1, 0, 0, 0, 1, 1, 1, '/images/products/nas-8bay.jpg', NOW(), 0),
('UPS - 1500VA Rack Mount', '1500VA/1000W rack-mount UPS with LCD display. 10 outlets, USB/serial connectivity.', '1500VA rack UPS', 'HW-SRV-UPS-001', 'UPS-1500-RM', 'Hardware', 'Server Infrastructure', 0, 1, 0, 0, 0, 699.99, 524.99, 849.99, 33.33, 0, 1, 1, 0, 0, 'ups,power,rack', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.25, 0, 0, 0, 1, 0, 25, 1, 1, 0, 0, 0, 0, 0, 4.4, 0, 0, 0, 0, 0, 1, 1, 1, '/images/products/ups-1500.jpg', NOW(), 0),
('UPS - 3000VA Rack Mount', '3000VA/2700W rack-mount UPS with network management. Pure sine wave, hot-swappable batteries.', '3000VA rack UPS', 'HW-SRV-UPS-002', 'UPS-3000-RM', 'Hardware', 'Server Infrastructure', 0, 1, 0, 0, 0, 1599.99, 1199.99, 1999.99, 33.33, 0, 1, 1, 0, 0, 'ups,power,rack,3000va', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.25, 0, 0, 0, 1, 0, 15, 1, 1, 0, 0, 0, 0, 0, 4.6, 0, 0, 1, 0, 0, 1, 1, 1, '/images/products/ups-3000.jpg', NOW(), 0),
('Server Rack - 42U', '42U server rack with cable management, PDU mounting, and airflow optimization.', '42U server rack', 'HW-SRV-RACK-001', 'RACK-42U', 'Hardware', 'Server Infrastructure', 0, 1, 0, 0, 0, 1999.99, 1499.99, 2499.99, 33.33, 0, 1, 1, 0, 0, 'rack,datacenter,42u', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.25, 0, 0, 0, 1, 0, 10, 1, 1, 0, 0, 0, 0, 0, 4.5, 0, 0, 0, 0, 0, 1, 1, 1, '/images/products/rack-42u.jpg', NOW(), 0);

-- ============================================================================
-- SOFTWARE PRODUCTS
-- ============================================================================

-- ----------------------------------------------------------------------------
-- Software - Operating Systems
-- ----------------------------------------------------------------------------
INSERT INTO Products (
    Name, Description, ShortDescription, SKU, ProductCode, Category, SubCategory,
    ProductType, Status, ServiceTier, IsService, IsSubscription,
    Price, Cost, ListPrice, Margin, UnitOfMeasure, MinimumQuantity, QuantityIncrement,
    BillingFrequency, PricingModel, Tags,
    DefaultContractTerm, MinimumContractTerm,
    WeeklyTermDiscount, MonthlyTermDiscount, QuarterlyTermDiscount, SemiAnnualTermDiscount,
    AnnualTermDiscount, TwoYearTermDiscount, ThreeYearTermDiscount, MaxTermDiscount,
    MaxVolumeDiscount, MaxTotalDiscount, BillableHourIncrement,
    IncludesOnsiteWork, TravelIncluded, MaterialsIncluded,
    IsTaxable, RevenueRecognition, Quantity, TrackInventory, AllowBackorder,
    IsHazardous, AutoRenewal, ExtendedWarrantyAvailable,
    TotalSold, TotalRevenue, AverageRating, ReviewCount,
    IsFeatured, IsBestSeller, IsNew, IsOnSale, IsActive, IsVisible, IsPurchasable,
    ImageUrl, CreatedAt, IsDeleted
) VALUES
('Windows 11 Pro License', 'Microsoft Windows 11 Professional license. Volume licensing available.', 'Windows 11 Pro license', 'SW-OS-WIN-001', 'WIN11-PRO', 'Software', 'Operating Systems', 0, 1, 0, 0, 0, 199.99, 149.99, 249.99, 33.33, 5, 1, 1, 0, 0, 'windows,license,os', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 15, 15, 0.25, 0, 0, 0, 1, 0, 500, 0, 1, 0, 0, 0, 0, 0, 4.5, 0, 0, 1, 0, 0, 1, 1, 1, '/images/products/windows11.jpg', NOW(), 0),
('Windows Server 2022 Standard', 'Windows Server 2022 Standard edition. 16-core license with 2 VM rights.', 'Windows Server 2022 Standard', 'SW-OS-WSV-001', 'WSRV-2022-STD', 'Software', 'Operating Systems', 0, 1, 0, 0, 0, 1069.99, 802.49, 1299.99, 33.33, 5, 1, 1, 0, 0, 'windows server,license,datacenter', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 20, 20, 0.25, 0, 0, 0, 1, 0, 100, 0, 1, 0, 0, 0, 0, 0, 4.6, 0, 0, 0, 1, 0, 1, 1, 1, '/images/products/windows-server.jpg', NOW(), 0),
('Windows Server 2022 Datacenter', 'Windows Server 2022 Datacenter edition. Unlimited VMs and containers.', 'Windows Server 2022 Datacenter', 'SW-OS-WSV-002', 'WSRV-2022-DC', 'Software', 'Operating Systems', 0, 1, 0, 0, 0, 6155.99, 4616.99, 7499.99, 33.33, 5, 1, 1, 0, 0, 'windows server,datacenter,license', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 20, 20, 0.25, 0, 0, 0, 1, 0, 50, 0, 1, 0, 0, 0, 0, 0, 4.7, 0, 1, 0, 0, 0, 1, 1, 1, '/images/products/windows-server-dc.jpg', NOW(), 0),
('Windows Server CAL - User', 'Windows Server User CAL for accessing Windows Server.', 'Windows Server User CAL', 'SW-OS-CAL-001', 'WSRV-CAL-USR', 'Software', 'Operating Systems', 0, 1, 0, 0, 0, 38.99, 29.24, 49.99, 33.33, 5, 5, 5, 0, 0, 'cal,windows server,license', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 25, 25, 0.25, 0, 0, 0, 1, 0, 1000, 0, 1, 0, 0, 0, 0, 0, 4.4, 0, 0, 1, 0, 0, 1, 1, 1, '/images/products/windows-cal.jpg', NOW(), 0);

-- ----------------------------------------------------------------------------
-- Software - Productivity & Collaboration (Subscription)
-- ----------------------------------------------------------------------------
INSERT INTO Products (
    Name, Description, ShortDescription, SKU, ProductCode, Category, SubCategory,
    ProductType, Status, ServiceTier, IsService, IsSubscription,
    Price, Cost, ListPrice, Margin, UnitOfMeasure, MinimumQuantity, QuantityIncrement,
    BillingFrequency, PricingModel, Tags,
    DefaultContractTerm, MinimumContractTerm,
    WeeklyTermDiscount, MonthlyTermDiscount, QuarterlyTermDiscount, SemiAnnualTermDiscount,
    AnnualTermDiscount, TwoYearTermDiscount, ThreeYearTermDiscount, MaxTermDiscount,
    MaxVolumeDiscount, MaxTotalDiscount, BillableHourIncrement,
    IncludesOnsiteWork, TravelIncluded, MaterialsIncluded,
    IsTaxable, RevenueRecognition, Quantity, TrackInventory, AllowBackorder,
    IsHazardous, AutoRenewal, ExtendedWarrantyAvailable,
    TotalSold, TotalRevenue, AverageRating, ReviewCount,
    IsFeatured, IsBestSeller, IsNew, IsOnSale, IsActive, IsVisible, IsPurchasable,
    ImageUrl, CreatedAt, IsDeleted
) VALUES
('Microsoft 365 Business Basic', 'Microsoft 365 Business Basic - Web and mobile apps, Teams, SharePoint, OneDrive.', 'M365 Business Basic', 'SW-M365-BB-001', 'M365-BUS-BASIC', 'Software', 'Productivity & Collaboration', 0, 1, 1, 0, 1, 6.00, 4.50, 8.00, 33.33, 5, 1, 1, 1, 0, 'm365,teams,sharepoint,subscription', 12, 1, 0, 0, 0, 0, 17, 20, 25, 25, 10, 30, 0.25, 0, 0, 0, 1, 2, 0, 0, 1, 0, 1, 0, 0, 0, 4.5, 0, 0, 1, 0, 0, 1, 1, 1, '/images/products/m365-basic.jpg', NOW(), 0),
('Microsoft 365 Business Standard', 'Microsoft 365 Business Standard - Desktop apps, Teams, SharePoint, OneDrive, Exchange.', 'M365 Business Standard', 'SW-M365-BS-001', 'M365-BUS-STD', 'Software', 'Productivity & Collaboration', 0, 1, 2, 0, 1, 12.50, 9.38, 15.00, 33.33, 5, 1, 1, 1, 0, 'm365,office,teams,subscription', 12, 1, 0, 0, 0, 0, 17, 20, 25, 25, 10, 30, 0.25, 0, 0, 0, 1, 2, 0, 0, 1, 0, 1, 0, 0, 0, 4.7, 0, 1, 0, 0, 0, 1, 1, 1, '/images/products/m365-standard.jpg', NOW(), 0),
('Microsoft 365 Business Premium', 'Microsoft 365 Business Premium - All Standard features plus advanced security and device management.', 'M365 Business Premium', 'SW-M365-BP-001', 'M365-BUS-PREM', 'Software', 'Productivity & Collaboration', 0, 1, 3, 0, 1, 22.00, 16.50, 28.00, 33.33, 5, 1, 1, 1, 0, 'm365,security,intune,subscription', 12, 1, 0, 0, 0, 0, 17, 20, 25, 25, 10, 30, 0.25, 0, 0, 0, 1, 2, 0, 0, 1, 0, 1, 0, 0, 0, 4.8, 0, 1, 1, 0, 0, 1, 1, 1, '/images/products/m365-premium.jpg', NOW(), 0),
('Microsoft 365 E3', 'Microsoft 365 E3 - Enterprise productivity with advanced compliance and security.', 'M365 E3 Enterprise', 'SW-M365-E3-001', 'M365-E3', 'Software', 'Productivity & Collaboration', 0, 1, 3, 0, 1, 36.00, 27.00, 45.00, 33.33, 5, 1, 1, 1, 0, 'm365,enterprise,compliance,subscription', 12, 12, 0, 0, 0, 0, 17, 20, 25, 25, 15, 35, 0.25, 0, 0, 0, 1, 2, 0, 0, 1, 0, 1, 0, 0, 0, 4.7, 0, 0, 0, 0, 0, 1, 1, 1, '/images/products/m365-e3.jpg', NOW(), 0),
('Microsoft 365 E5', 'Microsoft 365 E5 - Premium productivity with advanced security, compliance, and analytics.', 'M365 E5 Enterprise', 'SW-M365-E5-001', 'M365-E5', 'Software', 'Productivity & Collaboration', 0, 1, 4, 0, 1, 57.00, 42.75, 70.00, 33.33, 5, 1, 1, 1, 0, 'm365,enterprise,e5,premium,subscription', 12, 12, 0, 0, 0, 0, 17, 20, 25, 25, 15, 35, 0.25, 0, 0, 0, 1, 2, 0, 0, 1, 0, 1, 0, 0, 0, 4.9, 0, 1, 0, 0, 0, 1, 1, 1, '/images/products/m365-e5.jpg', NOW(), 0),
('Google Workspace Business Starter', 'Google Workspace Business Starter - Gmail, Drive, Meet, Chat for small teams.', 'Google Workspace Starter', 'SW-GWS-BS-001', 'GWS-BUS-START', 'Software', 'Productivity & Collaboration', 0, 1, 1, 0, 1, 6.00, 4.50, 7.20, 33.33, 5, 1, 1, 1, 0, 'google,workspace,gmail,subscription', 12, 1, 0, 0, 0, 0, 17, 20, 25, 25, 10, 30, 0.25, 0, 0, 0, 1, 2, 0, 0, 1, 0, 1, 0, 0, 0, 4.4, 0, 0, 0, 0, 0, 1, 1, 1, '/images/products/gws-starter.jpg', NOW(), 0),
('Google Workspace Business Standard', 'Google Workspace Business Standard - Enhanced storage, recording, and admin controls.', 'Google Workspace Standard', 'SW-GWS-STD-001', 'GWS-BUS-STD', 'Software', 'Productivity & Collaboration', 0, 1, 2, 0, 1, 12.00, 9.00, 14.40, 33.33, 5, 1, 1, 1, 0, 'google,workspace,business,subscription', 12, 1, 0, 0, 0, 0, 17, 20, 25, 25, 10, 30, 0.25, 0, 0, 0, 1, 2, 0, 0, 1, 0, 1, 0, 0, 0, 4.5, 0, 0, 1, 0, 0, 1, 1, 1, '/images/products/gws-standard.jpg', NOW(), 0);

-- ----------------------------------------------------------------------------
-- Software - Security Solutions
-- ----------------------------------------------------------------------------
INSERT INTO Products (
    Name, Description, ShortDescription, SKU, ProductCode, Category, SubCategory,
    ProductType, Status, ServiceTier, IsService, IsSubscription,
    Price, Cost, ListPrice, Margin, UnitOfMeasure, MinimumQuantity, QuantityIncrement,
    BillingFrequency, PricingModel, Tags,
    DefaultContractTerm, MinimumContractTerm,
    WeeklyTermDiscount, MonthlyTermDiscount, QuarterlyTermDiscount, SemiAnnualTermDiscount,
    AnnualTermDiscount, TwoYearTermDiscount, ThreeYearTermDiscount, MaxTermDiscount,
    MaxVolumeDiscount, MaxTotalDiscount, BillableHourIncrement,
    IncludesOnsiteWork, TravelIncluded, MaterialsIncluded,
    IsTaxable, RevenueRecognition, Quantity, TrackInventory, AllowBackorder,
    IsHazardous, AutoRenewal, ExtendedWarrantyAvailable,
    TotalSold, TotalRevenue, AverageRating, ReviewCount,
    IsFeatured, IsBestSeller, IsNew, IsOnSale, IsActive, IsVisible, IsPurchasable,
    ImageUrl, CreatedAt, IsDeleted
) VALUES
('Endpoint Protection - Basic', 'Basic endpoint protection with antivirus, anti-malware, and web filtering.', 'Basic endpoint protection', 'SW-SEC-EP-001', 'EP-BASIC', 'Software', 'Security Software', 0, 1, 1, 0, 1, 3.50, 2.63, 4.50, 33.33, 5, 5, 1, 1, 0, 'endpoint,antivirus,security,subscription', 12, 12, 0, 0, 0, 0, 15, 18, 22, 22, 20, 35, 0.25, 0, 0, 0, 1, 2, 0, 0, 1, 0, 1, 0, 0, 0, 4.3, 0, 0, 1, 0, 0, 1, 1, 1, '/images/products/endpoint-basic.jpg', NOW(), 0),
('Endpoint Protection - Advanced', 'Advanced EDR with behavioral analysis, threat hunting, and incident response.', 'Advanced EDR protection', 'SW-SEC-EP-002', 'EP-ADV-EDR', 'Software', 'Security Software', 0, 1, 2, 0, 1, 8.00, 6.00, 10.00, 33.33, 5, 5, 1, 1, 0, 'edr,endpoint,security,advanced,subscription', 12, 12, 0, 0, 0, 0, 15, 18, 22, 22, 20, 35, 0.25, 0, 0, 0, 1, 2, 0, 0, 1, 0, 1, 0, 0, 0, 4.6, 0, 1, 0, 0, 0, 1, 1, 1, '/images/products/endpoint-advanced.jpg', NOW(), 0),
('Email Security Gateway', 'Cloud email security with spam filtering, phishing protection, and DLP.', 'Email security gateway', 'SW-SEC-EM-001', 'EMAIL-SEC', 'Software', 'Security Software', 0, 1, 2, 0, 1, 4.00, 3.00, 5.00, 33.33, 5, 10, 1, 1, 0, 'email,security,spam,phishing,subscription', 12, 12, 0, 0, 0, 0, 15, 18, 22, 22, 20, 35, 0.25, 0, 0, 0, 1, 2, 0, 0, 1, 0, 1, 0, 0, 0, 4.5, 0, 0, 1, 0, 0, 1, 1, 1, '/images/products/email-security.jpg', NOW(), 0),
('Password Manager - Business', 'Business password manager with SSO integration, reporting, and admin controls.', 'Business password manager', 'SW-SEC-PM-001', 'PWD-MGR-BUS', 'Software', 'Security Software', 0, 1, 2, 0, 1, 5.00, 3.75, 6.00, 33.33, 5, 5, 1, 1, 0, 'password,security,sso,subscription', 12, 12, 0, 0, 0, 0, 17, 20, 25, 25, 15, 30, 0.25, 0, 0, 0, 1, 2, 0, 0, 1, 0, 1, 0, 0, 0, 4.7, 0, 0, 0, 0, 0, 1, 1, 1, '/images/products/password-manager.jpg', NOW(), 0),
('SIEM Solution - Cloud', 'Cloud-based SIEM with log aggregation, threat detection, and compliance reporting.', 'Cloud SIEM solution', 'SW-SEC-SIEM-001', 'SIEM-CLOUD', 'Software', 'Security Software', 0, 1, 3, 0, 1, 15.00, 11.25, 18.00, 33.33, 5, 50, 10, 1, 0, 'siem,security,logs,compliance,subscription', 12, 12, 0, 0, 0, 0, 17, 20, 25, 25, 15, 35, 0.25, 0, 0, 0, 1, 2, 0, 0, 1, 0, 1, 0, 0, 0, 4.6, 0, 1, 0, 0, 0, 1, 1, 1, '/images/products/siem-cloud.jpg', NOW(), 0);

-- ----------------------------------------------------------------------------
-- Software - Backup & Recovery
-- ----------------------------------------------------------------------------
INSERT INTO Products (
    Name, Description, ShortDescription, SKU, ProductCode, Category, SubCategory,
    ProductType, Status, ServiceTier, IsService, IsSubscription,
    Price, Cost, ListPrice, Margin, UnitOfMeasure, MinimumQuantity, QuantityIncrement,
    BillingFrequency, PricingModel, Tags,
    DefaultContractTerm, MinimumContractTerm,
    WeeklyTermDiscount, MonthlyTermDiscount, QuarterlyTermDiscount, SemiAnnualTermDiscount,
    AnnualTermDiscount, TwoYearTermDiscount, ThreeYearTermDiscount, MaxTermDiscount,
    MaxVolumeDiscount, MaxTotalDiscount, BillableHourIncrement,
    IncludesOnsiteWork, TravelIncluded, MaterialsIncluded,
    IsTaxable, RevenueRecognition, Quantity, TrackInventory, AllowBackorder,
    IsHazardous, AutoRenewal, ExtendedWarrantyAvailable,
    TotalSold, TotalRevenue, AverageRating, ReviewCount,
    IsFeatured, IsBestSeller, IsNew, IsOnSale, IsActive, IsVisible, IsPurchasable,
    ImageUrl, CreatedAt, IsDeleted
) VALUES
('Cloud Backup - Workstation', 'Cloud backup for workstations with 500GB storage per device.', 'Workstation cloud backup', 'SW-BKP-WS-001', 'BKP-WS-500GB', 'Software', 'Backup & Recovery', 0, 1, 1, 0, 1, 5.00, 3.75, 6.00, 33.33, 5, 5, 1, 1, 0, 'backup,cloud,workstation,subscription', 12, 12, 0, 0, 0, 0, 17, 20, 25, 25, 20, 35, 0.25, 0, 0, 0, 1, 2, 0, 0, 1, 0, 1, 0, 0, 0, 4.4, 0, 0, 1, 0, 0, 1, 1, 1, '/images/products/backup-workstation.jpg', NOW(), 0),
('Cloud Backup - Server', 'Cloud backup for servers with 1TB storage and bare-metal recovery.', 'Server cloud backup', 'SW-BKP-SRV-001', 'BKP-SRV-1TB', 'Software', 'Backup & Recovery', 0, 1, 2, 0, 1, 30.00, 22.50, 40.00, 33.33, 5, 1, 1, 1, 0, 'backup,cloud,server,disaster recovery,subscription', 12, 12, 0, 0, 0, 0, 17, 20, 25, 25, 20, 35, 0.25, 0, 0, 0, 1, 2, 0, 0, 1, 0, 1, 0, 0, 0, 4.6, 0, 0, 0, 1, 0, 1, 1, 1, '/images/products/backup-server.jpg', NOW(), 0),
('Microsoft 365 Backup', 'Backup for Microsoft 365 mailboxes, OneDrive, SharePoint, and Teams.', 'M365 backup solution', 'SW-BKP-M365-001', 'BKP-M365', 'Software', 'Backup & Recovery', 0, 1, 2, 0, 1, 4.00, 3.00, 5.00, 33.33, 5, 5, 1, 1, 0, 'backup,m365,office365,cloud,subscription', 12, 12, 0, 0, 0, 0, 17, 20, 25, 25, 20, 35, 0.25, 0, 0, 0, 1, 2, 0, 0, 1, 0, 1, 0, 0, 0, 4.7, 0, 1, 0, 0, 0, 1, 1, 1, '/images/products/backup-m365.jpg', NOW(), 0),
('Disaster Recovery as a Service', 'DRaaS with instant VM recovery, orchestrated failover, and compliance reporting.', 'DRaaS solution', 'SW-BKP-DR-001', 'DRAAS', 'Software', 'Backup & Recovery', 0, 1, 3, 0, 1, 50.00, 37.50, 65.00, 33.33, 5, 1, 1, 1, 0, 'draas,disaster recovery,cloud,business continuity,subscription', 12, 12, 0, 0, 0, 0, 17, 20, 25, 25, 15, 30, 0.25, 0, 0, 0, 1, 2, 0, 0, 1, 0, 1, 0, 0, 0, 4.8, 0, 1, 1, 0, 0, 1, 1, 1, '/images/products/draas.jpg', NOW(), 0);

-- ============================================================================
-- PROFESSIONAL SERVICES
-- ============================================================================

-- ----------------------------------------------------------------------------
-- Consulting & Strategy Services
-- ----------------------------------------------------------------------------
INSERT INTO Products (
    Name, Description, ShortDescription, SKU, ProductCode, Category, SubCategory,
    ProductType, Status, ServiceTier, IsService, IsSubscription,
    Price, Cost, ListPrice, Margin, UnitOfMeasure, MinimumQuantity, QuantityIncrement,
    BillingFrequency, PricingModel, Tags,
    DefaultContractTerm, MinimumContractTerm,
    WeeklyTermDiscount, MonthlyTermDiscount, QuarterlyTermDiscount, SemiAnnualTermDiscount,
    AnnualTermDiscount, TwoYearTermDiscount, ThreeYearTermDiscount, MaxTermDiscount,
    MaxVolumeDiscount, MaxTotalDiscount, BillableHourIncrement,
    HourlyRate, DailyRate, MinimumBillableHours,
    IncludesOnsiteWork, TravelIncluded, MaterialsIncluded,
    IsTaxable, RevenueRecognition, Quantity, TrackInventory, AllowBackorder,
    IsHazardous, AutoRenewal, ExtendedWarrantyAvailable,
    TotalSold, TotalRevenue, AverageRating, ReviewCount,
    IsFeatured, IsBestSeller, IsNew, IsOnSale, IsActive, IsVisible, IsPurchasable,
    ImageUrl, CreatedAt, IsDeleted
) VALUES
('IT Strategy Assessment', 'Comprehensive IT strategy assessment with technology roadmap and recommendations.', 'IT strategy assessment', 'SVC-CON-ITS-001', 'CON-IT-STRAT', 'Consulting & Strategy', 'IT Strategy & Roadmap', 1, 1, 3, 1, 0, 15000.00, 10500.00, 18000.00, 42.86, 0, 1, 1, 0, 0, 'consulting,strategy,assessment,roadmap', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 10, 15, 0.25, 250.00, 2000.00, 4.0, 1, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 0, 0, 4.7, 0, 0, 0, 0, 0, 1, 1, 1, '/images/products/it-strategy.jpg', NOW(), 0),
('Technology Audit', 'Complete technology infrastructure audit with security and compliance review.', 'Technology audit service', 'SVC-CON-AUD-001', 'CON-TECH-AUD', 'Consulting & Strategy', 'Technology Assessment & Audits', 1, 1, 2, 1, 0, 8000.00, 5600.00, 10000.00, 42.86, 0, 1, 1, 0, 0, 'audit,technology,compliance,assessment', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 10, 15, 0.25, 200.00, 1600.00, 4.0, 1, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 0, 0, 4.6, 0, 0, 1, 0, 0, 1, 1, 1, '/images/products/tech-audit.jpg', NOW(), 0),
('Cloud Migration Assessment', 'Cloud readiness assessment with migration strategy and cost analysis.', 'Cloud migration assessment', 'SVC-CON-CLD-001', 'CON-CLOUD-MIG', 'Consulting & Strategy', 'Cloud Migration Planning', 1, 1, 2, 1, 0, 12000.00, 8400.00, 15000.00, 42.86, 0, 1, 1, 0, 0, 'cloud,migration,assessment,azure,aws', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 10, 15, 0.25, 225.00, 1800.00, 4.0, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 0, 0, 4.8, 0, 1, 0, 0, 0, 1, 1, 1, '/images/products/cloud-assess.jpg', NOW(), 0),
('Security Risk Assessment', 'Comprehensive security risk assessment with threat modeling and remediation plan.', 'Security risk assessment', 'SVC-CON-SEC-001', 'CON-SEC-RISK', 'Consulting & Strategy', 'Cybersecurity Consulting', 1, 1, 3, 1, 0, 18000.00, 12600.00, 22000.00, 42.86, 0, 1, 1, 0, 0, 'security,risk,assessment,compliance', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 10, 15, 0.25, 275.00, 2200.00, 4.0, 1, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 0, 0, 4.9, 0, 1, 0, 0, 0, 1, 1, 1, '/images/products/security-assess.jpg', NOW(), 0),
('Compliance Readiness Assessment', 'Compliance readiness assessment for HIPAA, PCI-DSS, SOC2, or GDPR.', 'Compliance assessment', 'SVC-CON-CMP-001', 'CON-COMPLIANCE', 'Consulting & Strategy', 'Compliance & Regulatory', 1, 1, 3, 1, 0, 20000.00, 14000.00, 25000.00, 42.86, 0, 1, 1, 0, 0, 'compliance,hipaa,pci,soc2,gdpr', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 10, 15, 0.25, 300.00, 2400.00, 4.0, 1, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 0, 0, 4.7, 0, 0, 0, 1, 0, 1, 1, 1, '/images/products/compliance.jpg', NOW(), 0),
('vCIO Services - Quarterly', 'Virtual CIO services with quarterly strategy sessions and ongoing advisory.', 'vCIO quarterly services', 'SVC-CON-VCIO-001', 'CON-VCIO-QTR', 'Consulting & Strategy', 'IT Strategy & Roadmap', 1, 1, 3, 1, 1, 5000.00, 3500.00, 6500.00, 42.86, 0, 1, 1, 2, 0, 'vcio,strategy,advisory,consulting,subscription', 4, 4, 0, 0, 10, 15, 20, 0, 0, 20, 10, 25, 0.25, 300.00, 2400.00, 4.0, 1, 0, 0, 1, 2, 0, 0, 1, 0, 1, 0, 0, 0, 4.8, 0, 1, 0, 0, 0, 1, 1, 1, '/images/products/vcio.jpg', NOW(), 0);

-- ----------------------------------------------------------------------------
-- Implementation & Integration Services
-- ----------------------------------------------------------------------------
INSERT INTO Products (
    Name, Description, ShortDescription, SKU, ProductCode, Category, SubCategory,
    ProductType, Status, ServiceTier, IsService, IsSubscription,
    Price, Cost, ListPrice, Margin, UnitOfMeasure, MinimumQuantity, QuantityIncrement,
    BillingFrequency, PricingModel, Tags,
    DefaultContractTerm, MinimumContractTerm,
    WeeklyTermDiscount, MonthlyTermDiscount, QuarterlyTermDiscount, SemiAnnualTermDiscount,
    AnnualTermDiscount, TwoYearTermDiscount, ThreeYearTermDiscount, MaxTermDiscount,
    MaxVolumeDiscount, MaxTotalDiscount, BillableHourIncrement,
    HourlyRate, DailyRate, MinimumBillableHours,
    IncludesOnsiteWork, TravelIncluded, MaterialsIncluded,
    IsTaxable, RevenueRecognition, Quantity, TrackInventory, AllowBackorder,
    IsHazardous, AutoRenewal, ExtendedWarrantyAvailable,
    TotalSold, TotalRevenue, AverageRating, ReviewCount,
    IsFeatured, IsBestSeller, IsNew, IsOnSale, IsActive, IsVisible, IsPurchasable,
    ImageUrl, CreatedAt, IsDeleted
) VALUES
('Microsoft 365 Migration', 'Full Microsoft 365 migration including email, files, and identity integration.', 'M365 migration service', 'SVC-IMP-M365-001', 'IMP-M365-MIG', 'Implementation & Integration', 'Software Implementation', 1, 1, 2, 1, 0, 150.00, 105.00, 200.00, 42.86, 5, 10, 1, 0, 0, 'm365,migration,email,implementation', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 20, 25, 0.25, 175.00, 1400.00, 2.0, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 0, 0, 4.7, 0, 1, 0, 0, 0, 1, 1, 1, '/images/products/m365-migration.jpg', NOW(), 0),
('Server Migration', 'Physical to virtual or cloud server migration with minimal downtime.', 'Server migration service', 'SVC-IMP-SRV-001', 'IMP-SRV-MIG', 'Implementation & Integration', 'Data Migration Services', 1, 1, 2, 1, 0, 2500.00, 1750.00, 3000.00, 42.86, 0, 1, 1, 0, 0, 'server,migration,p2v,cloud', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 10, 15, 0.25, 200.00, 1600.00, 4.0, 1, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 0, 0, 4.6, 0, 0, 1, 0, 0, 1, 1, 1, '/images/products/server-migration.jpg', NOW(), 0),
('Network Infrastructure Deployment', 'Complete network infrastructure deployment including switches, APs, and firewalls.', 'Network deployment', 'SVC-IMP-NET-001', 'IMP-NET-DEPLOY', 'Implementation & Integration', 'Network Design & Implementation', 1, 1, 2, 1, 0, 5000.00, 3500.00, 6500.00, 42.86, 0, 1, 1, 0, 0, 'network,deployment,infrastructure,switches', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 10, 15, 0.25, 175.00, 1400.00, 8.0, 1, 1, 0, 1, 0, 0, 0, 1, 0, 0, 0, 0, 0, 4.7, 0, 0, 0, 1, 0, 1, 1, 1, '/images/products/network-deploy.jpg', NOW(), 0),
('API Integration Development', 'Custom API integration development between business applications.', 'API integration', 'SVC-IMP-API-001', 'IMP-API-INT', 'Implementation & Integration', 'API Integration', 1, 1, 2, 1, 0, 175.00, 122.50, 225.00, 42.86, 1, 20, 4, 0, 0, 'api,integration,development,custom', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 10, 15, 0.25, 175.00, 1400.00, 4.0, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 0, 0, 4.5, 0, 0, 0, 0, 0, 1, 1, 1, '/images/products/api-integration.jpg', NOW(), 0),
('CRM Implementation', 'Full CRM implementation including data migration, customization, and training.', 'CRM implementation', 'SVC-IMP-CRM-001', 'IMP-CRM', 'Implementation & Integration', 'Software Implementation', 1, 1, 2, 1, 0, 25000.00, 17500.00, 32000.00, 42.86, 0, 1, 1, 0, 0, 'crm,implementation,salesforce,dynamics', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 10, 15, 0.25, 200.00, 1600.00, 40.0, 1, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 0, 0, 4.6, 0, 0, 0, 0, 0, 1, 1, 1, '/images/products/crm-impl.jpg', NOW(), 0),
('Azure Cloud Deployment', 'Azure infrastructure deployment including VMs, networking, and security configuration.', 'Azure deployment', 'SVC-IMP-AZ-001', 'IMP-AZURE', 'Implementation & Integration', 'Infrastructure Deployment', 1, 1, 3, 1, 0, 10000.00, 7000.00, 12500.00, 42.86, 0, 1, 1, 0, 0, 'azure,cloud,deployment,infrastructure', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 10, 15, 0.25, 225.00, 1800.00, 16.0, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 0, 0, 4.8, 0, 1, 0, 0, 0, 1, 1, 1, '/images/products/azure-deploy.jpg', NOW(), 0),
('AWS Cloud Deployment', 'AWS infrastructure deployment including EC2, VPC, S3, and security configuration.', 'AWS deployment', 'SVC-IMP-AWS-001', 'IMP-AWS', 'Implementation & Integration', 'Infrastructure Deployment', 1, 1, 3, 1, 0, 10000.00, 7000.00, 12500.00, 42.86, 0, 1, 1, 0, 0, 'aws,cloud,deployment,infrastructure', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 10, 15, 0.25, 225.00, 1800.00, 16.0, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 0, 0, 4.8, 0, 1, 0, 0, 0, 1, 1, 1, '/images/products/aws-deploy.jpg', NOW(), 0);

-- ============================================================================
-- MANAGED SERVICES
-- ============================================================================

-- ----------------------------------------------------------------------------
-- Managed IT Services (MSP)
-- ----------------------------------------------------------------------------
INSERT INTO Products (
    Name, Description, ShortDescription, SKU, ProductCode, Category, SubCategory,
    ProductType, Status, ServiceTier, IsService, IsSubscription,
    Price, Cost, ListPrice, Margin, UnitOfMeasure, MinimumQuantity, QuantityIncrement,
    BillingFrequency, PricingModel, Tags,
    DefaultContractTerm, MinimumContractTerm,
    WeeklyTermDiscount, MonthlyTermDiscount, QuarterlyTermDiscount, SemiAnnualTermDiscount,
    AnnualTermDiscount, TwoYearTermDiscount, ThreeYearTermDiscount, MaxTermDiscount,
    MaxVolumeDiscount, MaxTotalDiscount, BillableHourIncrement,
    IncludesOnsiteWork, TravelIncluded, MaterialsIncluded,
    IsTaxable, RevenueRecognition, Quantity, TrackInventory, AllowBackorder,
    IsHazardous, AutoRenewal, ExtendedWarrantyAvailable,
    TotalSold, TotalRevenue, AverageRating, ReviewCount,
    IsFeatured, IsBestSeller, IsNew, IsOnSale, IsActive, IsVisible, IsPurchasable,
    ImageUrl, CreatedAt, IsDeleted
) VALUES
('Managed IT - Per Device Basic', 'Basic managed IT services per device with RMM, patching, and monitoring.', 'Basic managed IT per device', 'SVC-MSP-DEV-001', 'MSP-DEV-BASIC', 'Managed Services', 'Managed IT Services (MSP)', 1, 1, 1, 1, 1, 35.00, 24.50, 45.00, 42.86, 0, 10, 1, 1, 0, 'msp,managed,rmm,monitoring,subscription', 12, 12, 0, 0, 0, 0, 10, 15, 20, 20, 15, 30, 0.25, 0, 0, 0, 1, 2, 0, 0, 1, 0, 1, 0, 0, 0, 4.4, 0, 0, 1, 0, 0, 1, 1, 1, '/images/products/msp-basic.jpg', NOW(), 0),
('Managed IT - Per Device Standard', 'Standard managed IT with RMM, patching, help desk, and security monitoring.', 'Standard managed IT per device', 'SVC-MSP-DEV-002', 'MSP-DEV-STD', 'Managed Services', 'Managed IT Services (MSP)', 1, 1, 2, 1, 1, 65.00, 45.50, 85.00, 42.86, 0, 10, 1, 1, 0, 'msp,managed,helpdesk,security,subscription', 12, 12, 0, 0, 0, 0, 10, 15, 20, 20, 15, 30, 0.25, 0, 0, 0, 1, 2, 0, 0, 1, 0, 1, 0, 0, 0, 4.6, 0, 1, 0, 0, 0, 1, 1, 1, '/images/products/msp-standard.jpg', NOW(), 0),
('Managed IT - Per Device Premium', 'Premium managed IT with unlimited support, on-site visits, and priority response.', 'Premium managed IT per device', 'SVC-MSP-DEV-003', 'MSP-DEV-PREM', 'Managed Services', 'Managed IT Services (MSP)', 1, 1, 3, 1, 1, 125.00, 87.50, 160.00, 42.86, 0, 10, 1, 1, 0, 'msp,managed,premium,unlimited,subscription', 12, 12, 0, 0, 0, 0, 10, 15, 20, 20, 15, 30, 0.25, 1, 1, 0, 1, 2, 0, 0, 1, 0, 1, 0, 0, 0, 4.8, 0, 1, 0, 0, 0, 1, 1, 1, '/images/products/msp-premium.jpg', NOW(), 0),
('Managed IT - Per User All-Inclusive', 'All-inclusive managed IT per user including all devices, support, and security.', 'All-inclusive managed IT per user', 'SVC-MSP-USR-001', 'MSP-USR-ALL', 'Managed Services', 'Managed IT Services (MSP)', 1, 1, 3, 1, 1, 175.00, 122.50, 225.00, 42.86, 5, 5, 1, 1, 0, 'msp,managed,per user,all inclusive,subscription', 12, 12, 0, 0, 0, 0, 10, 15, 20, 20, 15, 30, 0.25, 1, 1, 0, 1, 2, 0, 0, 1, 0, 1, 0, 0, 0, 4.9, 0, 1, 1, 0, 0, 1, 1, 1, '/images/products/msp-user.jpg', NOW(), 0);

-- ----------------------------------------------------------------------------
-- Managed Security Services (MSSP)
-- ----------------------------------------------------------------------------
INSERT INTO Products (
    Name, Description, ShortDescription, SKU, ProductCode, Category, SubCategory,
    ProductType, Status, ServiceTier, IsService, IsSubscription,
    Price, Cost, ListPrice, Margin, UnitOfMeasure, MinimumQuantity, QuantityIncrement,
    BillingFrequency, PricingModel, Tags,
    DefaultContractTerm, MinimumContractTerm,
    WeeklyTermDiscount, MonthlyTermDiscount, QuarterlyTermDiscount, SemiAnnualTermDiscount,
    AnnualTermDiscount, TwoYearTermDiscount, ThreeYearTermDiscount, MaxTermDiscount,
    MaxVolumeDiscount, MaxTotalDiscount, BillableHourIncrement,
    IncludesOnsiteWork, TravelIncluded, MaterialsIncluded,
    IsTaxable, RevenueRecognition, Quantity, TrackInventory, AllowBackorder,
    IsHazardous, AutoRenewal, ExtendedWarrantyAvailable,
    TotalSold, TotalRevenue, AverageRating, ReviewCount,
    IsFeatured, IsBestSeller, IsNew, IsOnSale, IsActive, IsVisible, IsPurchasable,
    ImageUrl, CreatedAt, IsDeleted
) VALUES
('Managed SOC - Basic', 'Basic managed SOC with 8x5 monitoring, threat detection, and incident alerting.', 'Basic managed SOC', 'SVC-MSSP-SOC-001', 'MSSP-SOC-BASIC', 'Managed Services', 'Managed Security Services (MSSP)', 1, 1, 1, 1, 1, 1500.00, 1050.00, 2000.00, 42.86, 0, 1, 1, 1, 0, 'mssp,soc,security,monitoring,subscription', 12, 12, 0, 0, 0, 0, 10, 15, 20, 20, 10, 25, 0.25, 0, 0, 0, 1, 2, 0, 0, 1, 0, 1, 0, 0, 0, 4.5, 0, 0, 0, 0, 0, 1, 1, 1, '/images/products/soc-basic.jpg', NOW(), 0),
('Managed SOC - 24x7', '24x7 managed SOC with real-time monitoring, threat hunting, and incident response.', '24x7 managed SOC', 'SVC-MSSP-SOC-002', 'MSSP-SOC-247', 'Managed Services', 'Managed Security Services (MSSP)', 1, 1, 3, 1, 1, 5000.00, 3500.00, 6500.00, 42.86, 0, 1, 1, 1, 0, 'mssp,soc,24x7,security,subscription', 12, 12, 0, 0, 0, 0, 10, 15, 20, 20, 10, 25, 0.25, 0, 0, 0, 1, 2, 0, 0, 1, 0, 1, 0, 0, 0, 4.8, 0, 1, 0, 0, 0, 1, 1, 1, '/images/products/soc-247.jpg', NOW(), 0),
('Managed Vulnerability Scanning', 'Continuous vulnerability scanning with monthly reports and remediation guidance.', 'Managed vulnerability scanning', 'SVC-MSSP-VUL-001', 'MSSP-VULN', 'Managed Services', 'Managed Security Services (MSSP)', 1, 1, 2, 1, 1, 15.00, 10.50, 20.00, 42.86, 5, 20, 5, 1, 0, 'vulnerability,scanning,security,subscription', 12, 12, 0, 0, 0, 0, 15, 20, 25, 25, 20, 35, 0.25, 0, 0, 0, 1, 2, 0, 0, 1, 0, 1, 0, 0, 0, 4.6, 0, 0, 1, 0, 0, 1, 1, 1, '/images/products/vuln-scan.jpg', NOW(), 0),
('Managed Firewall Service', 'Managed firewall with 24x7 monitoring, rule management, and threat updates.', 'Managed firewall', 'SVC-MSSP-FW-001', 'MSSP-FW', 'Managed Services', 'Managed Security Services (MSSP)', 1, 1, 2, 1, 1, 350.00, 245.00, 450.00, 42.86, 0, 1, 1, 1, 0, 'firewall,managed,security,subscription', 12, 12, 0, 0, 0, 0, 10, 15, 20, 20, 10, 25, 0.25, 0, 0, 0, 1, 2, 0, 0, 1, 0, 1, 0, 0, 0, 4.7, 0, 0, 0, 0, 0, 1, 1, 1, '/images/products/managed-firewall.jpg', NOW(), 0);

-- ============================================================================
-- SUPPORT & MAINTENANCE SERVICES
-- ============================================================================

INSERT INTO Products (
    Name, Description, ShortDescription, SKU, ProductCode, Category, SubCategory,
    ProductType, Status, ServiceTier, IsService, IsSubscription,
    Price, Cost, ListPrice, Margin, UnitOfMeasure, MinimumQuantity, QuantityIncrement,
    BillingFrequency, PricingModel, Tags,
    DefaultContractTerm, MinimumContractTerm,
    WeeklyTermDiscount, MonthlyTermDiscount, QuarterlyTermDiscount, SemiAnnualTermDiscount,
    AnnualTermDiscount, TwoYearTermDiscount, ThreeYearTermDiscount, MaxTermDiscount,
    MaxVolumeDiscount, MaxTotalDiscount, BillableHourIncrement,
    HourlyRate, MinimumBillableHours,
    IncludesOnsiteWork, TravelIncluded, MaterialsIncluded,
    IsTaxable, RevenueRecognition, Quantity, TrackInventory, AllowBackorder,
    IsHazardous, AutoRenewal, ExtendedWarrantyAvailable,
    TotalSold, TotalRevenue, AverageRating, ReviewCount,
    IsFeatured, IsBestSeller, IsNew, IsOnSale, IsActive, IsVisible, IsPurchasable,
    ImageUrl, CreatedAt, IsDeleted
) VALUES
('Remote Support - Per Incident', 'Remote technical support charged per incident. Business hours.', 'Per incident remote support', 'SVC-SUP-REM-001', 'SUP-REMOTE-INC', 'Support & Maintenance', 'Remote Support & Troubleshooting', 1, 1, 1, 1, 0, 125.00, 87.50, 150.00, 42.86, 0, 1, 1, 0, 0, 'support,remote,incident,helpdesk', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 10, 15, 0.25, 125.00, 1.0, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 0, 0, 4.5, 0, 0, 0, 0, 0, 1, 1, 1, '/images/products/remote-support.jpg', NOW(), 0),
('Remote Support - Hourly', 'Remote technical support billed hourly. Minimum 30 minutes.', 'Hourly remote support', 'SVC-SUP-REM-002', 'SUP-REMOTE-HR', 'Support & Maintenance', 'Remote Support & Troubleshooting', 1, 1, 1, 1, 0, 150.00, 105.00, 185.00, 42.86, 1, 1, 1, 4, 0, 'support,remote,hourly,helpdesk', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 10, 15, 0.50, 150.00, 0.5, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 0, 0, 4.6, 0, 0, 1, 0, 0, 1, 1, 1, '/images/products/remote-hourly.jpg', NOW(), 0),
('On-Site Support - Hourly', 'On-site technical support billed hourly. Travel time included.', 'Hourly on-site support', 'SVC-SUP-ONS-001', 'SUP-ONSITE-HR', 'Support & Maintenance', 'On-Site Support Services', 1, 1, 2, 1, 0, 175.00, 122.50, 225.00, 42.86, 1, 2, 1, 4, 0, 'support,onsite,hourly,field', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 10, 15, 0.25, 175.00, 2.0, 1, 1, 0, 1, 0, 0, 0, 1, 0, 0, 0, 0, 0, 4.7, 0, 1, 0, 0, 0, 1, 1, 1, '/images/products/onsite-support.jpg', NOW(), 0),
('After-Hours Support - Hourly', 'After-hours emergency support billed hourly. 1.5x standard rate.', 'After-hours support', 'SVC-SUP-AFT-001', 'SUP-AFTERHRS', 'Support & Maintenance', 'After-Hours & Emergency Support', 1, 1, 2, 1, 0, 250.00, 175.00, 325.00, 42.86, 1, 1, 1, 4, 0, 'support,afterhours,emergency,urgent', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 10, 0.25, 250.00, 1.0, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 0, 0, 4.8, 0, 0, 0, 0, 0, 1, 1, 1, '/images/products/afterhours.jpg', NOW(), 0),
('Help Desk Support Block - 10 Hours', 'Pre-purchased block of 10 help desk support hours. Never expires.', '10-hour support block', 'SVC-SUP-BLK-001', 'SUP-BLOCK-10', 'Support & Maintenance', 'Help Desk & Technical Support', 1, 1, 1, 1, 0, 1350.00, 945.00, 1500.00, 42.86, 0, 1, 1, 0, 0, 'support,block,prepaid,helpdesk', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 10, 15, 0.25, 135.00, 0.25, 0, 0, 0, 1, 1, 0, 0, 1, 0, 0, 0, 0, 0, 4.5, 0, 0, 1, 0, 0, 1, 1, 1, '/images/products/support-block.jpg', NOW(), 0),
('Help Desk Support Block - 25 Hours', 'Pre-purchased block of 25 help desk support hours at discounted rate.', '25-hour support block', 'SVC-SUP-BLK-002', 'SUP-BLOCK-25', 'Support & Maintenance', 'Help Desk & Technical Support', 1, 1, 1, 1, 0, 3000.00, 2100.00, 3750.00, 42.86, 0, 1, 1, 0, 0, 'support,block,prepaid,discounted', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 10, 15, 0.25, 120.00, 0.25, 0, 0, 0, 1, 1, 0, 0, 1, 0, 0, 0, 0, 0, 4.6, 0, 1, 0, 0, 0, 1, 1, 1, '/images/products/support-block-25.jpg', NOW(), 0),
('User Training - Per Session', 'End-user training session for common IT tools. 1-hour minimum.', 'Per session user training', 'SVC-SUP-TRN-001', 'SUP-TRAIN-SES', 'Support & Maintenance', 'End-User Training & Documentation', 1, 1, 1, 1, 0, 250.00, 175.00, 300.00, 42.86, 1, 1, 1, 0, 0, 'training,user,education,session', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 10, 15, 1.0, 250.00, 1.0, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 0, 0, 4.7, 0, 0, 0, 0, 0, 1, 1, 1, '/images/products/training.jpg', NOW(), 0);

-- ============================================================================
-- SECURITY SERVICES
-- ============================================================================

INSERT INTO Products (
    Name, Description, ShortDescription, SKU, ProductCode, Category, SubCategory,
    ProductType, Status, ServiceTier, IsService, IsSubscription,
    Price, Cost, ListPrice, Margin, UnitOfMeasure, MinimumQuantity, QuantityIncrement,
    BillingFrequency, PricingModel, Tags,
    DefaultContractTerm, MinimumContractTerm,
    WeeklyTermDiscount, MonthlyTermDiscount, QuarterlyTermDiscount, SemiAnnualTermDiscount,
    AnnualTermDiscount, TwoYearTermDiscount, ThreeYearTermDiscount, MaxTermDiscount,
    MaxVolumeDiscount, MaxTotalDiscount, BillableHourIncrement,
    HourlyRate, MinimumBillableHours,
    IncludesOnsiteWork, TravelIncluded, MaterialsIncluded,
    IsTaxable, RevenueRecognition, Quantity, TrackInventory, AllowBackorder,
    IsHazardous, AutoRenewal, ExtendedWarrantyAvailable,
    TotalSold, TotalRevenue, AverageRating, ReviewCount,
    IsFeatured, IsBestSeller, IsNew, IsOnSale, IsActive, IsVisible, IsPurchasable,
    ImageUrl, CreatedAt, IsDeleted
) VALUES
('External Penetration Test', 'External network penetration testing with detailed findings report.', 'External pentest', 'SVC-SEC-PEN-001', 'SEC-PENTEST-EXT', 'Security Services', 'Penetration Testing', 1, 1, 2, 1, 0, 5000.00, 3500.00, 6500.00, 42.86, 0, 1, 1, 0, 0, 'pentest,security,assessment,external', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 10, 15, 0.25, 300.00, 16.0, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 0, 0, 4.8, 0, 0, 0, 0, 0, 1, 1, 1, '/images/products/pentest-ext.jpg', NOW(), 0),
('Internal Penetration Test', 'Internal network penetration testing simulating insider threat.', 'Internal pentest', 'SVC-SEC-PEN-002', 'SEC-PENTEST-INT', 'Security Services', 'Penetration Testing', 1, 1, 2, 1, 0, 7500.00, 5250.00, 9500.00, 42.86, 0, 1, 1, 0, 0, 'pentest,security,internal,assessment', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 10, 15, 0.25, 300.00, 24.0, 1, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 0, 0, 4.8, 0, 0, 0, 1, 0, 1, 1, 1, '/images/products/pentest-int.jpg', NOW(), 0),
('Web Application Pentest', 'Web application security testing following OWASP methodology.', 'Web app pentest', 'SVC-SEC-PEN-003', 'SEC-PENTEST-WEB', 'Security Services', 'Penetration Testing', 1, 1, 2, 1, 0, 8000.00, 5600.00, 10000.00, 42.86, 0, 1, 1, 0, 0, 'pentest,webapp,owasp,security', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 10, 15, 0.25, 300.00, 24.0, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 0, 0, 4.9, 0, 1, 0, 0, 0, 1, 1, 1, '/images/products/pentest-web.jpg', NOW(), 0),
('Security Awareness Training', 'Annual security awareness training program with phishing simulations.', 'Security awareness training', 'SVC-SEC-TRN-001', 'SEC-AWARE-TRAIN', 'Security Services', 'Security Awareness Training', 1, 1, 2, 1, 1, 5.00, 3.50, 7.00, 42.86, 5, 25, 5, 3, 0, 'security,training,awareness,phishing,subscription', 12, 12, 0, 0, 0, 0, 15, 20, 25, 25, 30, 45, 0.25, NULL, NULL, 0, 0, 0, 1, 2, 0, 0, 1, 0, 1, 0, 0, 0, 4.6, 0, 0, 1, 0, 0, 1, 1, 1, '/images/products/security-training.jpg', NOW(), 0),
('Incident Response Retainer', 'Annual incident response retainer with guaranteed 4-hour response time.', 'IR retainer', 'SVC-SEC-IR-001', 'SEC-IR-RETAIN', 'Security Services', 'Incident Response & Forensics', 1, 1, 3, 1, 1, 2500.00, 1750.00, 3200.00, 42.86, 0, 1, 1, 1, 0, 'incident response,security,retainer,subscription', 12, 12, 0, 0, 0, 0, 10, 15, 20, 20, 0, 15, 0.25, 400.00, 4.0, 1, 1, 0, 1, 2, 0, 0, 1, 0, 1, 0, 0, 0, 4.9, 0, 1, 0, 0, 0, 1, 1, 1, '/images/products/ir-retainer.jpg', NOW(), 0),
('Phishing Simulation Campaign', 'Quarterly phishing simulation campaign with training for failed users.', 'Phishing simulation', 'SVC-SEC-PHI-001', 'SEC-PHISH-SIM', 'Security Services', 'Security Awareness Training', 1, 1, 2, 1, 1, 500.00, 350.00, 650.00, 42.86, 0, 1, 1, 2, 0, 'phishing,simulation,security,training,subscription', 4, 4, 0, 0, 10, 15, 20, 0, 0, 20, 20, 35, 0.25, NULL, NULL, 0, 0, 0, 1, 2, 0, 0, 1, 0, 1, 0, 0, 0, 4.5, 0, 0, 0, 0, 0, 1, 1, 1, '/images/products/phishing-sim.jpg', NOW(), 0);

-- ============================================================================
-- SPECIALIZED SERVICES
-- ============================================================================

INSERT INTO Products (
    Name, Description, ShortDescription, SKU, ProductCode, Category, SubCategory,
    ProductType, Status, ServiceTier, IsService, IsSubscription,
    Price, Cost, ListPrice, Margin, UnitOfMeasure, MinimumQuantity, QuantityIncrement,
    BillingFrequency, PricingModel, Tags,
    DefaultContractTerm, MinimumContractTerm,
    WeeklyTermDiscount, MonthlyTermDiscount, QuarterlyTermDiscount, SemiAnnualTermDiscount,
    AnnualTermDiscount, TwoYearTermDiscount, ThreeYearTermDiscount, MaxTermDiscount,
    MaxVolumeDiscount, MaxTotalDiscount, BillableHourIncrement,
    HourlyRate, MinimumBillableHours,
    IncludesOnsiteWork, TravelIncluded, MaterialsIncluded,
    IsTaxable, RevenueRecognition, Quantity, TrackInventory, AllowBackorder,
    IsHazardous, AutoRenewal, ExtendedWarrantyAvailable,
    TotalSold, TotalRevenue, AverageRating, ReviewCount,
    IsFeatured, IsBestSeller, IsNew, IsOnSale, IsActive, IsVisible, IsPurchasable,
    ImageUrl, CreatedAt, IsDeleted
) VALUES
('VoIP Phone System - Per User', 'Hosted VoIP phone service with unlimited calling, voicemail, and mobile app.', 'VoIP per user', 'SVC-SPC-VIP-001', 'VOIP-USER', 'Specialized Services', 'VoIP & Unified Communications', 1, 1, 2, 1, 1, 25.00, 17.50, 32.00, 42.86, 5, 5, 1, 1, 0, 'voip,phone,ucaas,subscription', 12, 12, 0, 0, 0, 0, 15, 20, 25, 25, 20, 35, 0.25, NULL, NULL, 0, 0, 0, 1, 2, 0, 0, 1, 0, 1, 0, 0, 0, 4.6, 0, 0, 1, 0, 0, 1, 1, 1, '/images/products/voip-user.jpg', NOW(), 0),
('VoIP Phone System - Contact Center', 'Contact center features with IVR, call recording, and analytics.', 'VoIP contact center', 'SVC-SPC-VIP-002', 'VOIP-CC', 'Specialized Services', 'VoIP & Unified Communications', 1, 1, 3, 1, 1, 75.00, 52.50, 95.00, 42.86, 5, 5, 1, 1, 0, 'voip,contact center,ivr,subscription', 12, 12, 0, 0, 0, 0, 15, 20, 25, 25, 15, 30, 0.25, NULL, NULL, 0, 0, 0, 1, 2, 0, 0, 1, 0, 1, 0, 0, 0, 4.7, 0, 1, 0, 0, 0, 1, 1, 1, '/images/products/voip-cc.jpg', NOW(), 0),
('Wireless Site Survey', 'Professional wireless site survey with heat mapping and AP placement recommendations.', 'Wireless site survey', 'SVC-SPC-WSS-001', 'WIFI-SURVEY', 'Specialized Services', 'Wireless Site Surveys & Design', 1, 1, 2, 1, 0, 2500.00, 1750.00, 3200.00, 42.86, 0, 1, 1, 0, 0, 'wireless,survey,wifi,heatmap', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 10, 15, 0.25, 200.00, 8.0, 1, 1, 0, 1, 0, 0, 0, 1, 0, 0, 0, 0, 0, 4.7, 0, 0, 0, 0, 0, 1, 1, 1, '/images/products/wifi-survey.jpg', NOW(), 0),
('Network Cabling - Per Drop', 'Cat6A network cabling per drop including labor and materials.', 'Network cabling per drop', 'SVC-SPC-CBL-001', 'CABLE-DROP', 'Specialized Services', 'Cabling & Infrastructure', 1, 1, 1, 1, 0, 175.00, 122.50, 225.00, 42.86, 0, 4, 1, 0, 0, 'cabling,network,cat6a,infrastructure', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 20, 25, 0.25, NULL, NULL, 1, 0, 1, 1, 0, 0, 0, 1, 0, 0, 0, 0, 0, 4.5, 0, 0, 1, 0, 0, 1, 1, 1, '/images/products/cabling.jpg', NOW(), 0),
('IT Asset Discovery', 'Complete IT asset discovery and inventory with reporting and documentation.', 'IT asset discovery', 'SVC-SPC-AST-001', 'ASSET-DISC', 'Specialized Services', 'Asset Tracking & Management', 1, 1, 2, 1, 0, 3500.00, 2450.00, 4500.00, 42.86, 0, 1, 1, 0, 0, 'asset,discovery,inventory,documentation', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 10, 15, 0.25, 175.00, 16.0, 1, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 0, 0, 4.6, 0, 0, 0, 0, 0, 1, 1, 1, '/images/products/asset-discovery.jpg', NOW(), 0),
('IT Procurement Services', 'Hardware and software procurement with vendor management and volume discounts.', 'IT procurement', 'SVC-SPC-PRO-001', 'PROCURE', 'Specialized Services', 'IT Procurement Services', 1, 1, 2, 1, 0, 150.00, 105.00, 200.00, 42.86, 1, 1, 1, 0, 0, 'procurement,purchasing,vendor', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 10, 15, 0.25, 150.00, 1.0, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 0, 0, 4.4, 0, 0, 0, 0, 0, 1, 1, 1, '/images/products/procurement.jpg', NOW(), 0),
('E-Waste Disposal & Recycling', 'Certified e-waste disposal and recycling with certificate of destruction.', 'E-waste disposal', 'SVC-SPC-EWS-001', 'EWASTE', 'Specialized Services', 'Green IT & Sustainability', 1, 1, 1, 1, 0, 25.00, 17.50, 35.00, 42.86, 0, 5, 1, 0, 0, 'ewaste,recycling,disposal,green', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 20, 25, 0.25, NULL, NULL, 1, 1, 0, 1, 0, 0, 0, 1, 0, 0, 0, 0, 0, 4.3, 0, 0, 0, 0, 0, 1, 1, 1, '/images/products/ewaste.jpg', NOW(), 0);

SET FOREIGN_KEY_CHECKS = 1;

-- ============================================================================
-- END OF SEED DATA
-- ============================================================================
