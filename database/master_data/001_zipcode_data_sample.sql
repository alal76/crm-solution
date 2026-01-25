-- ============================================================================
-- CRM Solution Master Data - ZIP Codes (Sample US Data)
-- Version: 1.0
-- Date: 2026-01-23
-- Description: Sample US postal code data for testing
-- Note: For production, import full dataset from external source
-- ============================================================================

SET NAMES utf8mb4;

-- Clear existing data for clean import
-- TRUNCATE TABLE ZipCodes;

-- Sample US ZIP Code Data (Major Cities)
INSERT INTO ZipCodes (Country, CountryCode, PostalCode, City, State, StateCode, County, Latitude, Longitude, Accuracy, IsActive) VALUES
-- New York
('United States', 'US', '10001', 'New York', 'New York', 'NY', 'New York', 40.7506, -73.9971, 4, 1),
('United States', 'US', '10002', 'New York', 'New York', 'NY', 'New York', 40.7157, -73.9863, 4, 1),
('United States', 'US', '10003', 'New York', 'New York', 'NY', 'New York', 40.7317, -73.9893, 4, 1),
('United States', 'US', '10004', 'New York', 'New York', 'NY', 'New York', 40.6989, -74.0393, 4, 1),
('United States', 'US', '10005', 'New York', 'New York', 'NY', 'New York', 40.7066, -74.0089, 4, 1),
('United States', 'US', '10006', 'New York', 'New York', 'NY', 'New York', 40.7094, -74.0131, 4, 1),
('United States', 'US', '10007', 'New York', 'New York', 'NY', 'New York', 40.7135, -74.0077, 4, 1),
('United States', 'US', '10010', 'New York', 'New York', 'NY', 'New York', 40.7390, -73.9826, 4, 1),
('United States', 'US', '10011', 'New York', 'New York', 'NY', 'New York', 40.7418, -74.0002, 4, 1),
('United States', 'US', '10012', 'New York', 'New York', 'NY', 'New York', 40.7258, -73.9981, 4, 1),
('United States', 'US', '10013', 'New York', 'New York', 'NY', 'New York', 40.7202, -74.0047, 4, 1),
('United States', 'US', '10014', 'New York', 'New York', 'NY', 'New York', 40.7338, -74.0057, 4, 1),
('United States', 'US', '10016', 'New York', 'New York', 'NY', 'New York', 40.7462, -73.9788, 4, 1),
('United States', 'US', '10017', 'New York', 'New York', 'NY', 'New York', 40.7529, -73.9722, 4, 1),
('United States', 'US', '10018', 'New York', 'New York', 'NY', 'New York', 40.7557, -73.9930, 4, 1),
('United States', 'US', '10019', 'New York', 'New York', 'NY', 'New York', 40.7652, -73.9864, 4, 1),
('United States', 'US', '10020', 'New York', 'New York', 'NY', 'New York', 40.7589, -73.9806, 4, 1),
('United States', 'US', '10021', 'New York', 'New York', 'NY', 'New York', 40.7692, -73.9587, 4, 1),
('United States', 'US', '10022', 'New York', 'New York', 'NY', 'New York', 40.7581, -73.9676, 4, 1),
('United States', 'US', '10023', 'New York', 'New York', 'NY', 'New York', 40.7764, -73.9824, 4, 1),

-- Los Angeles
('United States', 'US', '90001', 'Los Angeles', 'California', 'CA', 'Los Angeles', 33.9425, -118.2551, 4, 1),
('United States', 'US', '90002', 'Los Angeles', 'California', 'CA', 'Los Angeles', 33.9490, -118.2473, 4, 1),
('United States', 'US', '90003', 'Los Angeles', 'California', 'CA', 'Los Angeles', 33.9640, -118.2727, 4, 1),
('United States', 'US', '90004', 'Los Angeles', 'California', 'CA', 'Los Angeles', 34.0773, -118.3089, 4, 1),
('United States', 'US', '90005', 'Los Angeles', 'California', 'CA', 'Los Angeles', 34.0590, -118.3010, 4, 1),
('United States', 'US', '90006', 'Los Angeles', 'California', 'CA', 'Los Angeles', 34.0477, -118.2937, 4, 1),
('United States', 'US', '90007', 'Los Angeles', 'California', 'CA', 'Los Angeles', 34.0282, -118.2833, 4, 1),
('United States', 'US', '90008', 'Los Angeles', 'California', 'CA', 'Los Angeles', 34.0104, -118.3415, 4, 1),
('United States', 'US', '90010', 'Los Angeles', 'California', 'CA', 'Los Angeles', 34.0607, -118.3157, 4, 1),
('United States', 'US', '90012', 'Los Angeles', 'California', 'CA', 'Los Angeles', 34.0617, -118.2399, 4, 1),
('United States', 'US', '90013', 'Los Angeles', 'California', 'CA', 'Los Angeles', 34.0446, -118.2416, 4, 1),
('United States', 'US', '90014', 'Los Angeles', 'California', 'CA', 'Los Angeles', 34.0410, -118.2527, 4, 1),
('United States', 'US', '90015', 'Los Angeles', 'California', 'CA', 'Los Angeles', 34.0372, -118.2654, 4, 1),
('United States', 'US', '90016', 'Los Angeles', 'California', 'CA', 'Los Angeles', 34.0301, -118.3524, 4, 1),
('United States', 'US', '90017', 'Los Angeles', 'California', 'CA', 'Los Angeles', 34.0516, -118.2620, 4, 1),
('United States', 'US', '90018', 'Los Angeles', 'California', 'CA', 'Los Angeles', 34.0287, -118.3154, 4, 1),
('United States', 'US', '90019', 'Los Angeles', 'California', 'CA', 'Los Angeles', 34.0467, -118.3432, 4, 1),
('United States', 'US', '90020', 'Los Angeles', 'California', 'CA', 'Los Angeles', 34.0660, -118.3092, 4, 1),
('United States', 'US', '90210', 'Beverly Hills', 'California', 'CA', 'Los Angeles', 34.0901, -118.4065, 4, 1),
('United States', 'US', '90211', 'Beverly Hills', 'California', 'CA', 'Los Angeles', 34.0660, -118.3833, 4, 1),

-- Chicago
('United States', 'US', '60601', 'Chicago', 'Illinois', 'IL', 'Cook', 41.8819, -87.6278, 4, 1),
('United States', 'US', '60602', 'Chicago', 'Illinois', 'IL', 'Cook', 41.8830, -87.6288, 4, 1),
('United States', 'US', '60603', 'Chicago', 'Illinois', 'IL', 'Cook', 41.8798, -87.6276, 4, 1),
('United States', 'US', '60604', 'Chicago', 'Illinois', 'IL', 'Cook', 41.8783, -87.6275, 4, 1),
('United States', 'US', '60605', 'Chicago', 'Illinois', 'IL', 'Cook', 41.8660, -87.6185, 4, 1),
('United States', 'US', '60606', 'Chicago', 'Illinois', 'IL', 'Cook', 41.8776, -87.6382, 4, 1),
('United States', 'US', '60607', 'Chicago', 'Illinois', 'IL', 'Cook', 41.8720, -87.6516, 4, 1),
('United States', 'US', '60608', 'Chicago', 'Illinois', 'IL', 'Cook', 41.8541, -87.6719, 4, 1),
('United States', 'US', '60609', 'Chicago', 'Illinois', 'IL', 'Cook', 41.8132, -87.6544, 4, 1),
('United States', 'US', '60610', 'Chicago', 'Illinois', 'IL', 'Cook', 41.9067, -87.6328, 4, 1),
('United States', 'US', '60611', 'Chicago', 'Illinois', 'IL', 'Cook', 41.8965, -87.6193, 4, 1),
('United States', 'US', '60612', 'Chicago', 'Illinois', 'IL', 'Cook', 41.8802, -87.6929, 4, 1),
('United States', 'US', '60613', 'Chicago', 'Illinois', 'IL', 'Cook', 41.9566, -87.6568, 4, 1),
('United States', 'US', '60614', 'Chicago', 'Illinois', 'IL', 'Cook', 41.9221, -87.6519, 4, 1),
('United States', 'US', '60615', 'Chicago', 'Illinois', 'IL', 'Cook', 41.8016, -87.5962, 4, 1),
('United States', 'US', '60616', 'Chicago', 'Illinois', 'IL', 'Cook', 41.8417, -87.6324, 4, 1),
('United States', 'US', '60617', 'Chicago', 'Illinois', 'IL', 'Cook', 41.7259, -87.5543, 4, 1),
('United States', 'US', '60618', 'Chicago', 'Illinois', 'IL', 'Cook', 41.9467, -87.6987, 4, 1),
('United States', 'US', '60619', 'Chicago', 'Illinois', 'IL', 'Cook', 41.7469, -87.6074, 4, 1),
('United States', 'US', '60620', 'Chicago', 'Illinois', 'IL', 'Cook', 41.7415, -87.6541, 4, 1),

-- Houston
('United States', 'US', '77001', 'Houston', 'Texas', 'TX', 'Harris', 29.7588, -95.3564, 4, 1),
('United States', 'US', '77002', 'Houston', 'Texas', 'TX', 'Harris', 29.7542, -95.3583, 4, 1),
('United States', 'US', '77003', 'Houston', 'Texas', 'TX', 'Harris', 29.7413, -95.3445, 4, 1),
('United States', 'US', '77004', 'Houston', 'Texas', 'TX', 'Harris', 29.7305, -95.3644, 4, 1),
('United States', 'US', '77005', 'Houston', 'Texas', 'TX', 'Harris', 29.7159, -95.4227, 4, 1),
('United States', 'US', '77006', 'Houston', 'Texas', 'TX', 'Harris', 29.7416, -95.3932, 4, 1),
('United States', 'US', '77007', 'Houston', 'Texas', 'TX', 'Harris', 29.7729, -95.4041, 4, 1),
('United States', 'US', '77008', 'Houston', 'Texas', 'TX', 'Harris', 29.7939, -95.4158, 4, 1),
('United States', 'US', '77009', 'Houston', 'Texas', 'TX', 'Harris', 29.8039, -95.3656, 4, 1),
('United States', 'US', '77010', 'Houston', 'Texas', 'TX', 'Harris', 29.7589, -95.3542, 4, 1),

-- Phoenix
('United States', 'US', '85001', 'Phoenix', 'Arizona', 'AZ', 'Maricopa', 33.4488, -112.0771, 4, 1),
('United States', 'US', '85002', 'Phoenix', 'Arizona', 'AZ', 'Maricopa', 33.4519, -112.0853, 4, 1),
('United States', 'US', '85003', 'Phoenix', 'Arizona', 'AZ', 'Maricopa', 33.4500, -112.0858, 4, 1),
('United States', 'US', '85004', 'Phoenix', 'Arizona', 'AZ', 'Maricopa', 33.4520, -112.0688, 4, 1),
('United States', 'US', '85006', 'Phoenix', 'Arizona', 'AZ', 'Maricopa', 33.4614, -112.0459, 4, 1),
('United States', 'US', '85007', 'Phoenix', 'Arizona', 'AZ', 'Maricopa', 33.4428, -112.0925, 4, 1),
('United States', 'US', '85008', 'Phoenix', 'Arizona', 'AZ', 'Maricopa', 33.4559, -112.0025, 4, 1),
('United States', 'US', '85009', 'Phoenix', 'Arizona', 'AZ', 'Maricopa', 33.4560, -112.1317, 4, 1),
('United States', 'US', '85010', 'Phoenix', 'Arizona', 'AZ', 'Maricopa', 33.4172, -112.0644, 4, 1),
('United States', 'US', '85012', 'Phoenix', 'Arizona', 'AZ', 'Maricopa', 33.5042, -112.0644, 4, 1),

-- Miami
('United States', 'US', '33101', 'Miami', 'Florida', 'FL', 'Miami-Dade', 25.7848, -80.2095, 4, 1),
('United States', 'US', '33109', 'Miami Beach', 'Florida', 'FL', 'Miami-Dade', 25.7617, -80.1306, 4, 1),
('United States', 'US', '33125', 'Miami', 'Florida', 'FL', 'Miami-Dade', 25.7792, -80.2255, 4, 1),
('United States', 'US', '33126', 'Miami', 'Florida', 'FL', 'Miami-Dade', 25.7780, -80.3006, 4, 1),
('United States', 'US', '33127', 'Miami', 'Florida', 'FL', 'Miami-Dade', 25.8173, -80.1982, 4, 1),
('United States', 'US', '33128', 'Miami', 'Florida', 'FL', 'Miami-Dade', 25.7753, -80.2007, 4, 1),
('United States', 'US', '33129', 'Miami', 'Florida', 'FL', 'Miami-Dade', 25.7551, -80.2062, 4, 1),
('United States', 'US', '33130', 'Miami', 'Florida', 'FL', 'Miami-Dade', 25.7681, -80.1967, 4, 1),
('United States', 'US', '33131', 'Miami', 'Florida', 'FL', 'Miami-Dade', 25.7609, -80.1892, 4, 1),
('United States', 'US', '33132', 'Miami', 'Florida', 'FL', 'Miami-Dade', 25.7780, -80.1830, 4, 1),

-- Seattle
('United States', 'US', '98101', 'Seattle', 'Washington', 'WA', 'King', 47.6114, -122.3363, 4, 1),
('United States', 'US', '98102', 'Seattle', 'Washington', 'WA', 'King', 47.6336, -122.3209, 4, 1),
('United States', 'US', '98103', 'Seattle', 'Washington', 'WA', 'King', 47.6710, -122.3428, 4, 1),
('United States', 'US', '98104', 'Seattle', 'Washington', 'WA', 'King', 47.6002, -122.3305, 4, 1),
('United States', 'US', '98105', 'Seattle', 'Washington', 'WA', 'King', 47.6619, -122.2897, 4, 1),
('United States', 'US', '98106', 'Seattle', 'Washington', 'WA', 'King', 47.5398, -122.3546, 4, 1),
('United States', 'US', '98107', 'Seattle', 'Washington', 'WA', 'King', 47.6700, -122.3778, 4, 1),
('United States', 'US', '98108', 'Seattle', 'Washington', 'WA', 'King', 47.5400, -122.3043, 4, 1),
('United States', 'US', '98109', 'Seattle', 'Washington', 'WA', 'King', 47.6389, -122.3480, 4, 1),
('United States', 'US', '98112', 'Seattle', 'Washington', 'WA', 'King', 47.6313, -122.2973, 4, 1),

-- Boston
('United States', 'US', '02101', 'Boston', 'Massachusetts', 'MA', 'Suffolk', 42.3709, -71.0575, 4, 1),
('United States', 'US', '02102', 'Boston', 'Massachusetts', 'MA', 'Suffolk', 42.3476, -71.0729, 4, 1),
('United States', 'US', '02103', 'Boston', 'Massachusetts', 'MA', 'Suffolk', 42.3584, -71.0598, 4, 1),
('United States', 'US', '02104', 'Boston', 'Massachusetts', 'MA', 'Suffolk', 42.3548, -71.0615, 4, 1),
('United States', 'US', '02105', 'Boston', 'Massachusetts', 'MA', 'Suffolk', 42.3559, -71.0550, 4, 1),
('United States', 'US', '02108', 'Boston', 'Massachusetts', 'MA', 'Suffolk', 42.3576, -71.0635, 4, 1),
('United States', 'US', '02109', 'Boston', 'Massachusetts', 'MA', 'Suffolk', 42.3624, -71.0527, 4, 1),
('United States', 'US', '02110', 'Boston', 'Massachusetts', 'MA', 'Suffolk', 42.3574, -71.0514, 4, 1),
('United States', 'US', '02111', 'Boston', 'Massachusetts', 'MA', 'Suffolk', 42.3505, -71.0610, 4, 1),
('United States', 'US', '02113', 'Boston', 'Massachusetts', 'MA', 'Suffolk', 42.3649, -71.0544, 4, 1),

-- Denver
('United States', 'US', '80201', 'Denver', 'Colorado', 'CO', 'Denver', 39.7392, -104.9847, 4, 1),
('United States', 'US', '80202', 'Denver', 'Colorado', 'CO', 'Denver', 39.7509, -105.0004, 4, 1),
('United States', 'US', '80203', 'Denver', 'Colorado', 'CO', 'Denver', 39.7305, -104.9799, 4, 1),
('United States', 'US', '80204', 'Denver', 'Colorado', 'CO', 'Denver', 39.7376, -105.0256, 4, 1),
('United States', 'US', '80205', 'Denver', 'Colorado', 'CO', 'Denver', 39.7614, -104.9647, 4, 1),
('United States', 'US', '80206', 'Denver', 'Colorado', 'CO', 'Denver', 39.7364, -104.9540, 4, 1),
('United States', 'US', '80207', 'Denver', 'Colorado', 'CO', 'Denver', 39.7561, -104.9282, 4, 1),
('United States', 'US', '80209', 'Denver', 'Colorado', 'CO', 'Denver', 39.7077, -104.9630, 4, 1),
('United States', 'US', '80210', 'Denver', 'Colorado', 'CO', 'Denver', 39.6800, -104.9643, 4, 1),
('United States', 'US', '80211', 'Denver', 'Colorado', 'CO', 'Denver', 39.7658, -105.0190, 4, 1),

-- San Francisco
('United States', 'US', '94102', 'San Francisco', 'California', 'CA', 'San Francisco', 37.7813, -122.4167, 4, 1),
('United States', 'US', '94103', 'San Francisco', 'California', 'CA', 'San Francisco', 37.7726, -122.4107, 4, 1),
('United States', 'US', '94104', 'San Francisco', 'California', 'CA', 'San Francisco', 37.7915, -122.4016, 4, 1),
('United States', 'US', '94105', 'San Francisco', 'California', 'CA', 'San Francisco', 37.7892, -122.3953, 4, 1),
('United States', 'US', '94107', 'San Francisco', 'California', 'CA', 'San Francisco', 37.7621, -122.3971, 4, 1),
('United States', 'US', '94108', 'San Francisco', 'California', 'CA', 'San Francisco', 37.7925, -122.4080, 4, 1),
('United States', 'US', '94109', 'San Francisco', 'California', 'CA', 'San Francisco', 37.7952, -122.4209, 4, 1),
('United States', 'US', '94110', 'San Francisco', 'California', 'CA', 'San Francisco', 37.7488, -122.4155, 4, 1),
('United States', 'US', '94111', 'San Francisco', 'California', 'CA', 'San Francisco', 37.7971, -122.4006, 4, 1),
('United States', 'US', '94112', 'San Francisco', 'California', 'CA', 'San Francisco', 37.7208, -122.4458, 4, 1);

-- Note: For production use, import the complete ZIP code dataset from:
-- https://github.com/Zeeshanahmad4/Zip-code-of-all-countries-cities-in-the-world-CSV-TXT-SQL-DATABASE
-- The full dataset contains millions of records for all countries

SELECT CONCAT('Loaded ', COUNT(*), ' sample ZIP codes') AS Status FROM ZipCodes;
