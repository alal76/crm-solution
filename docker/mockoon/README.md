# Mockoon Mock API Server

This directory contains Mockoon environment files used by the `crm-mockoon` Docker container (dev-tools profile).

## Start

```bash
docker-compose -f docker/docker-compose.providers.yml --profile dev-tools up -d crm-mockoon
```

Mock API base URL: `http://localhost:3001`

## Files

| File | Purpose |
|------|---------|
| `crm-mock-env.json` | Main CRM mock environment — WhatsApp, Facebook, Twitter, QuickBooks webhook simulators |

## Mock Endpoints

| Method | Path | Simulates |
|--------|------|-----------|
| `POST` | `/whatsapp/webhook` | Meta WhatsApp Cloud API inbound message webhook |
| `POST` | `/facebook/webhook` | Facebook Messenger Graph API webhook |
| `POST` | `/twitter/webhook` | Twitter/X Account Activity API webhook |
| `GET`  | `/quickbooks/oauth/token` | QuickBooks OAuth2 token exchange (sandbox) |
| `GET`  | `/xero/api.xro/2.0/Contacts` | Xero Contacts API response |

## Adding New Mocks

1. Download and install [Mockoon Desktop](https://mockoon.com/download/).
2. Import `crm-mock-env.json`.
3. Add routes visually.
4. Export back to `crm-mock-env.json`.
5. Restart the container: `docker restart crm-mockoon`

Alternatively, edit `crm-mock-env.json` directly following the [Mockoon schema](https://mockoon.com/docs/latest/mockoon-data-files/data-storage-location/).
