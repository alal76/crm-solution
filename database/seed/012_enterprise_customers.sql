-- Enterprise Customers Seed Data
-- 7 Major Companies: Microsoft, Google, Amazon, LinkedIn, Infosys, TCS, Wipro
-- Generated: 2026-01-29

-- Insert Enterprise Customers
INSERT INTO Customers (
    Company, FirstName, LastName, Email, Phone, Website, 
    Industry, Address, City, State, Country, ZipCode,
    AnnualRevenue, Employees, Status, CustomerType,
    CreatedAt, IsDeleted
) VALUES
(
    'Microsoft Corporation', 'Satya', 'Nadella', 'satya.nadella@microsoft.com', '+1-425-882-8080',
    'https://www.microsoft.com', 'Technology', 'One Microsoft Way', 'Redmond', 'Washington',
    'United States', '98052', 198000000000.00, 221000, 1, 1, NOW(), 0
),
(
    'Alphabet Inc. (Google)', 'Sundar', 'Pichai', 'sundar.pichai@google.com', '+1-650-253-0000',
    'https://www.google.com', 'Technology', '1600 Amphitheatre Parkway', 'Mountain View', 'California',
    'United States', '94043', 282000000000.00, 182000, 1, 1, NOW(), 0
),
(
    'Amazon.com Inc.', 'Andy', 'Jassy', 'andy.jassy@amazon.com', '+1-206-266-1000',
    'https://www.amazon.com', 'E-commerce & Cloud', '410 Terry Avenue North', 'Seattle', 'Washington',
    'United States', '98109', 574000000000.00, 1540000, 1, 1, NOW(), 0
),
(
    'LinkedIn Corporation', 'Ryan', 'Roslansky', 'ryan.roslansky@linkedin.com', '+1-650-687-3600',
    'https://www.linkedin.com', 'Professional Networking', '1000 W Maude Avenue', 'Sunnyvale', 'California',
    'United States', '94085', 15000000000.00, 20000, 1, 1, NOW(), 0
),
(
    'Infosys Limited', 'Salil', 'Parekh', 'salil.parekh@infosys.com', '+91-80-2852-0261',
    'https://www.infosys.com', 'IT Services & Consulting', 'Electronics City', 'Bangalore', 'Karnataka',
    'India', '560100', 18200000000.00, 343000, 1, 1, NOW(), 0
),
(
    'Tata Consultancy Services', 'K', 'Krithivasan', 'k.krithivasan@tcs.com', '+91-22-6778-9999',
    'https://www.tcs.com', 'IT Services & Consulting', 'TCS House, Raveline Street', 'Mumbai', 'Maharashtra',
    'India', '400001', 29000000000.00, 615000, 1, 1, NOW(), 0
),
(
    'Wipro Limited', 'Srinivas', 'Pallia', 'srinivas.pallia@wipro.com', '+91-80-2844-0011',
    'https://www.wipro.com', 'IT Services & Consulting', 'Doddakannelli, Sarjapur Road', 'Bangalore', 'Karnataka',
    'India', '560035', 11300000000.00, 258000, 1, 1, NOW(), 0
);

-- Store customer IDs for reference
SET @microsoft_id = (SELECT Id FROM Customers WHERE Company = 'Microsoft Corporation' LIMIT 1);
SET @google_id = (SELECT Id FROM Customers WHERE Company = 'Alphabet Inc. (Google)' LIMIT 1);
SET @amazon_id = (SELECT Id FROM Customers WHERE Company = 'Amazon.com Inc.' LIMIT 1);
SET @linkedin_id = (SELECT Id FROM Customers WHERE Company = 'LinkedIn Corporation' LIMIT 1);
SET @infosys_id = (SELECT Id FROM Customers WHERE Company = 'Infosys Limited' LIMIT 1);
SET @tcs_id = (SELECT Id FROM Customers WHERE Company = 'Tata Consultancy Services' LIMIT 1);
SET @wipro_id = (SELECT Id FROM Customers WHERE Company = 'Wipro Limited' LIMIT 1);
