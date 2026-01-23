#!/usr/bin/env python3
"""
Convert GeoNames ZIP code data to SQL insert statements for the CRM database.
GeoNames format (tab-separated):
  0: country code
  1: postal code
  2: place name (city)
  3: admin name 1 (state)
  4: admin code 1 (state code)
  5: admin name 2 (county)
  6: admin code 2 (county code)
  7: admin name 3 (community)
  8: admin code 3 (community code)
  9: latitude
  10: longitude
  11: accuracy
"""

import sys
import os

def escape_sql(value):
    """Escape single quotes for SQL"""
    if not value:
        return "NULL"
    return "'" + value.replace("'", "''").replace("\\", "\\\\") + "'"

def convert_to_sql(input_file, output_file, batch_size=1000):
    """Convert GeoNames data to SQL insert statements"""
    
    print(f"Reading {input_file}...")
    
    with open(input_file, 'r', encoding='utf-8') as infile:
        lines = infile.readlines()
    
    total_records = len(lines)
    print(f"Found {total_records:,} records")
    
    with open(output_file, 'w', encoding='utf-8') as outfile:
        # Write header
        outfile.write("-- GeoNames ZIP Code Data\n")
        outfile.write(f"-- Total Records: {total_records:,}\n")
        outfile.write("-- Source: https://download.geonames.org/export/zip/allCountries.zip\n")
        outfile.write("-- License: Creative Commons Attribution 4.0\n\n")
        
        # Clear existing data (optional)
        outfile.write("-- Clear existing data (uncomment if needed)\n")
        outfile.write("-- TRUNCATE TABLE ZipCodes;\n\n")
        
        outfile.write("-- Insert ZIP code data\n")
        
        batch = []
        record_count = 0
        
        for line in lines:
            parts = line.strip().split('\t')
            if len(parts) < 10:
                continue
            
            country_code = parts[0] if len(parts) > 0 else ""
            postal_code = parts[1] if len(parts) > 1 else ""
            city = parts[2] if len(parts) > 2 else ""
            
            # Skip records with empty required fields
            if not country_code or not postal_code or not city:
                continue
                
            state = parts[3] if len(parts) > 3 else ""
            state_code = parts[4] if len(parts) > 4 else ""
            county = parts[5] if len(parts) > 5 else ""
            county_code = parts[6] if len(parts) > 6 else ""
            community = parts[7] if len(parts) > 7 else ""
            community_code = parts[8] if len(parts) > 8 else ""
            
            try:
                latitude = float(parts[9]) if len(parts) > 9 and parts[9] else 0
                longitude = float(parts[10]) if len(parts) > 10 and parts[10] else 0
                accuracy = int(parts[11]) if len(parts) > 11 and parts[11] else 1
            except (ValueError, IndexError):
                latitude = 0
                longitude = 0
                accuracy = 1
            
            # Map country code to country name
            country_names = {
                'US': 'United States', 'CA': 'Canada', 'GB': 'United Kingdom',
                'AU': 'Australia', 'DE': 'Germany', 'FR': 'France', 'IT': 'Italy',
                'ES': 'Spain', 'NL': 'Netherlands', 'BE': 'Belgium', 'CH': 'Switzerland',
                'AT': 'Austria', 'IE': 'Ireland', 'NZ': 'New Zealand', 'MX': 'Mexico',
                'BR': 'Brazil', 'AR': 'Argentina', 'IN': 'India', 'JP': 'Japan',
                'CN': 'China', 'KR': 'South Korea', 'SG': 'Singapore', 'MY': 'Malaysia',
                'PH': 'Philippines', 'TH': 'Thailand', 'VN': 'Vietnam', 'ID': 'Indonesia',
                'ZA': 'South Africa', 'NG': 'Nigeria', 'EG': 'Egypt', 'AE': 'United Arab Emirates',
                'SA': 'Saudi Arabia', 'IL': 'Israel', 'TR': 'Turkey', 'RU': 'Russia',
                'PL': 'Poland', 'CZ': 'Czech Republic', 'HU': 'Hungary', 'RO': 'Romania',
                'SE': 'Sweden', 'NO': 'Norway', 'DK': 'Denmark', 'FI': 'Finland',
                'PT': 'Portugal', 'GR': 'Greece', 'HR': 'Croatia', 'SI': 'Slovenia',
                'SK': 'Slovakia', 'BG': 'Bulgaria', 'LT': 'Lithuania', 'LV': 'Latvia',
                'EE': 'Estonia', 'CY': 'Cyprus', 'MT': 'Malta', 'LU': 'Luxembourg',
                'AD': 'Andorra', 'MC': 'Monaco', 'SM': 'San Marino', 'VA': 'Vatican City',
                'CL': 'Chile', 'CO': 'Colombia', 'PE': 'Peru', 'VE': 'Venezuela',
                'EC': 'Ecuador', 'UY': 'Uruguay', 'PY': 'Paraguay', 'BO': 'Bolivia',
                'PA': 'Panama', 'CR': 'Costa Rica', 'GT': 'Guatemala', 'HN': 'Honduras',
                'SV': 'El Salvador', 'NI': 'Nicaragua', 'DO': 'Dominican Republic',
                'PR': 'Puerto Rico', 'JM': 'Jamaica', 'TT': 'Trinidad and Tobago',
                'PK': 'Pakistan', 'BD': 'Bangladesh', 'LK': 'Sri Lanka', 'NP': 'Nepal',
                'HK': 'Hong Kong', 'TW': 'Taiwan', 'MO': 'Macau'
            }
            
            country = country_names.get(country_code, country_code)
            
            values = f"({escape_sql(country)}, {escape_sql(country_code)}, {escape_sql(postal_code)}, " \
                    f"{escape_sql(city)}, {escape_sql(state)}, {escape_sql(state_code)}, " \
                    f"{escape_sql(county)}, {escape_sql(county_code)}, {escape_sql(community)}, " \
                    f"{escape_sql(community_code)}, {latitude}, {longitude}, {accuracy}, 1)"
            
            batch.append(values)
            record_count += 1
            
            if len(batch) >= batch_size:
                outfile.write("INSERT INTO ZipCodes (Country, CountryCode, PostalCode, City, State, StateCode, County, CountyCode, Community, CommunityCode, Latitude, Longitude, Accuracy, IsActive) VALUES\n")
                outfile.write(",\n".join(batch))
                outfile.write(";\n\n")
                batch = []
                
                if record_count % 100000 == 0:
                    print(f"Processed {record_count:,} records...")
        
        # Write remaining batch
        if batch:
            outfile.write("INSERT INTO ZipCodes (Country, CountryCode, PostalCode, City, State, StateCode, County, CountyCode, Community, CommunityCode, Latitude, Longitude, Accuracy, IsActive) VALUES\n")
            outfile.write(",\n".join(batch))
            outfile.write(";\n\n")
        
        outfile.write(f"\n-- Total records inserted: {record_count:,}\n")
    
    print(f"Conversion complete! {record_count:,} records written to {output_file}")
    return record_count

if __name__ == "__main__":
    script_dir = os.path.dirname(os.path.abspath(__file__))
    input_file = os.path.join(script_dir, "allCountries.txt")
    output_file = os.path.join(script_dir, "002_geonames_zipcodes.sql")
    
    if not os.path.exists(input_file):
        print(f"Error: {input_file} not found!")
        sys.exit(1)
    
    convert_to_sql(input_file, output_file)
