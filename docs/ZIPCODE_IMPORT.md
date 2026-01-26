# ZIP Code Import Feature

## Overview

The CRM Solution includes a comprehensive ZIP code import service that can pull postal code data from external sources like GeoNames and GitHub, and populate the database automatically.

## Features

- **GeoNames Integration**: Import postal codes from the GeoNames database (https://download.geonames.org)
- **GitHub Integration**: Import from custom GitHub repositories with JSON-formatted ZIP code data
- **Scheduled Imports**: Background service for automated periodic imports
- **On-Startup Import**: Automatically import data if the ZipCodes table is empty
- **Progress Tracking**: Real-time import status and progress monitoring
- **Deduplication**: Automatically skips existing records to avoid duplicates
- **Batch Processing**: Efficient batch inserts for large datasets

## API Endpoints

All import endpoints require **Admin role** authorization.

### Get Import Status
```http
GET /api/zipcodes/import/status
Authorization: Bearer {token}
```

Response:
```json
{
  "isRunning": false,
  "totalRecords": 43000,
  "processedRecords": 43000,
  "progressPercent": 100,
  "currentSource": null,
  "currentCountry": null,
  "lastImportAt": "2026-01-26T15:30:00Z",
  "lastResult": {
    "success": true,
    "recordsImported": 43000,
    "recordsSkipped": 0,
    "duration": "00:02:15"
  }
}
```

### Import from GeoNames (All Countries)
```http
POST /api/zipcodes/import/geonames
Authorization: Bearer {token}
```

This imports **all countries** from GeoNames (~2 million records). This may take several minutes.

### Import from GeoNames (Single Country)
```http
POST /api/zipcodes/import/geonames/{countryCode}
Authorization: Bearer {token}
```

Example for US only:
```http
POST /api/zipcodes/import/geonames/US
```

Supported country codes include: US, CA, GB, AU, DE, FR, IT, ES, NL, BE, CH, AT, IE, NZ, MX, BR, AR, IN, JP, and many more.

### Import from GitHub
```http
POST /api/zipcodes/import/github
Authorization: Bearer {token}
Content-Type: application/json

{
  "url": "https://raw.githubusercontent.com/your-repo/main/zipcodes.json"
}
```

The JSON format should be:
```json
[
  {
    "countryCode": "US",
    "postalCode": "10001",
    "city": "New York",
    "state": "New York",
    "stateCode": "NY",
    "county": "New York",
    "latitude": 40.7506,
    "longitude": -73.9971,
    "accuracy": 4
  }
]
```

### Get ZIP Code Statistics
```http
GET /api/zipcodes/stats
```

Response:
```json
{
  "totalRecords": 43000,
  "countryCount": 1
}
```

## Configuration

Add the following to your `appsettings.json`:

```json
{
  "ZipCodeImport": {
    "EnableScheduledImport": false,
    "CronExpression": "0 0 1 * *",
    "ImportSource": "GeoNames",
    "GitHubUrl": null,
    "CountryCodes": ["US"],
    "ImportOnStartupIfEmpty": true,
    "MinimumHoursBetweenImports": 168
  }
}
```

### Configuration Options

| Option | Type | Default | Description |
|--------|------|---------|-------------|
| `EnableScheduledImport` | bool | false | Enable automatic scheduled imports |
| `CronExpression` | string | "0 0 1 * *" | Cron schedule (default: monthly on 1st at midnight) |
| `ImportSource` | string | "GeoNames" | Source: "GeoNames" or "GitHub" |
| `GitHubUrl` | string | null | Custom GitHub URL for JSON data |
| `CountryCodes` | string[] | ["US"] | Countries to import (empty = all) |
| `ImportOnStartupIfEmpty` | bool | true | Auto-import on startup if table empty |
| `MinimumHoursBetweenImports` | int | 168 | Minimum hours between scheduled imports |

## Environment Variables

For Kubernetes/Docker deployments, you can override settings:

```yaml
env:
  - name: ZipCodeImport__EnableScheduledImport
    value: "true"
  - name: ZipCodeImport__CountryCodes__0
    value: "US"
  - name: ZipCodeImport__CountryCodes__1
    value: "CA"
```

## Usage Examples

### Manual Import via API

```bash
# Login and get token
TOKEN=$(curl -s -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@example.com","password":"password"}' \
  | jq -r '.accessToken')

# Import US ZIP codes
curl -X POST http://localhost:5000/api/zipcodes/import/geonames/US \
  -H "Authorization: Bearer $TOKEN"

# Check import status
curl http://localhost:5000/api/zipcodes/import/status \
  -H "Authorization: Bearer $TOKEN"
```

### Workflow Integration

The import can be triggered from a workflow task:

```csharp
// In a workflow step handler
var importService = serviceProvider.GetRequiredService<IZipCodeImportService>();
var result = await importService.ImportCountryFromGeoNamesAsync("US");
```

## Data Sources

### GeoNames
- URL: https://download.geonames.org/export/zip/
- License: Creative Commons Attribution 4.0
- Updates: Weekly
- Coverage: 100+ countries, ~2 million records

### Creating Custom GitHub Data

1. Create a JSON file with the required format
2. Host on GitHub (raw content URL)
3. Configure `GitHubUrl` in settings or pass via API

Example repository structure:
```
your-repo/
  data/
    zipcodes.json
    zipcodes-us.json
    zipcodes-ca.json
```

## Performance Notes

- **Full GeoNames Import**: ~2 million records, takes 5-10 minutes
- **Single Country (US)**: ~43,000 records, takes ~1 minute
- **Batch Size**: 5,000 records per transaction
- **Memory**: ~500MB peak during full import

## Troubleshooting

### Import hangs or times out
- Check network connectivity to download.geonames.org
- Increase HttpClient timeout in production
- Try importing single country instead of all

### Duplicate records
- The import service automatically deduplicates based on CountryCode + PostalCode
- Existing records are skipped, not updated

### Memory issues
- Consider importing countries individually
- Increase container memory limits for full imports

## Related APIs

- `GET /api/zipcodes/countries` - List available countries
- `GET /api/zipcodes/lookup/{postalCode}` - Lookup by postal code
- `GET /api/zipcodes/states/{countryCode}` - Get states for country
- `GET /api/zipcodes/cities/{countryCode}/{stateCode}` - Get cities in state
