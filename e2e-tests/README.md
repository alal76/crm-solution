# CRM Solution - E2E Test Suite

## Overview

Comprehensive end-to-end test suite for the CRM Solution using Playwright. This suite covers all major features including:

- **Authentication** (20 tests) - Login, logout, registration, password reset, 2FA, sessions
- **Customers** (26 tests) - CRUD operations, search, filter, contact info, export/import
- **Contacts** (14 tests) - CRUD operations, customer linking, multi-contact management
- **Opportunities** (13 tests) - CRUD operations, pipeline stages, probability calculations
- **Leads** (10 tests) - CRUD operations, status management, conversion to customer/opportunity
- **Service Requests** (15 tests) - CRUD operations, priority, status transitions, assignment
- **Campaigns** (13 tests) - CRUD operations, lifecycle management, metrics
- **Workflows** (18 tests) - Designer, nodes, AI nodes, execution
- **Admin Settings** (28 tests) - Users, roles, security, LLM settings, branding, audit
- **Dashboard** (32 tests) - Widgets, metrics, navigation, search, notifications

**Total: 189 Test Cases**

## Prerequisites

- Docker installed and running
- CRM Solution running and accessible (default: http://192.168.0.9)
- Valid test credentials

## Quick Start

### Using Docker (Recommended)

```bash
# Run all tests
./run-tests.sh

# Run specific test suite
./run-tests.sh --customers
./run-tests.sh --auth
./run-tests.sh --workflows

# Run with custom URL
./run-tests.sh --url http://localhost:3000

# Run in debug mode
./run-tests.sh --debug
```

### Using Docker Compose

```bash
# Run all tests
docker-compose up --build

# Run specific suite
docker-compose --profile auth up --build

# View logs
docker-compose logs -f
```

### Running Locally

```bash
# Install dependencies
npm install

# Install Playwright browsers
npx playwright install

# Run all tests
npm test

# Run in headed mode (see browser)
npm run test:headed

# Run with UI
npm run test:ui

# Run specific suite
npm run test:auth
npm run test:customers
npm run test:contacts
```

## Test Data Conventions

All test data is prefixed with `TEST_` or `_TEST` for easy identification and cleanup:

- Customer names: `TEST_Acme Corporation`
- Contact names: `TEST_John_Smith`
- Email addresses: `TEST_john@test.com`
- Workflow names: `TEST_Lead_Qualification_Workflow`

### Cleanup

Test data can be identified and cleaned up using:

```sql
-- Find test customers
SELECT * FROM customers WHERE name LIKE 'TEST_%';

-- Find test contacts
SELECT * FROM contacts WHERE first_name LIKE 'TEST_%';

-- Delete test data (use with caution)
DELETE FROM customers WHERE name LIKE 'TEST_%';
```

## Test Structure

```
e2e-tests/
├── package.json
├── playwright.config.ts
├── Dockerfile
├── docker-compose.yml
├── run-tests.sh
├── tests/
│   ├── test-data.ts           # Test data definitions
│   ├── fixtures.ts            # Page Objects and utilities
│   ├── auth.setup.ts          # Authentication setup
│   ├── auth/
│   │   └── authentication.spec.ts
│   ├── customers/
│   │   └── customers.spec.ts
│   ├── contacts/
│   │   └── contacts.spec.ts
│   ├── opportunities/
│   │   └── opportunities.spec.ts
│   ├── leads/
│   │   └── leads.spec.ts
│   ├── service-requests/
│   │   └── service-requests.spec.ts
│   ├── campaigns/
│   │   └── campaigns.spec.ts
│   ├── workflows/
│   │   └── workflows.spec.ts
│   ├── admin/
│   │   └── admin.spec.ts
│   └── dashboard/
│       └── dashboard.spec.ts
├── test-results/              # Screenshots, videos, traces
└── playwright-report/         # HTML reports
```

## Configuration

### Environment Variables

| Variable | Default | Description |
|----------|---------|-------------|
| `BASE_URL` | `http://192.168.0.9` | CRM application URL |
| `TEST_USERNAME` | `admin` | Test user username |
| `TEST_PASSWORD` | `admin` | Test user password |
| `CI` | `false` | Run in CI mode |

### Browser Support

Tests run on multiple browsers:
- Chromium (Desktop)
- Firefox (Desktop)
- WebKit/Safari (Desktop)
- Mobile Chrome
- Mobile Safari

## Reports

### HTML Report

```bash
npx playwright show-report playwright-report
```

### JUnit Report

JUnit XML reports are generated at `test-results/junit-results.xml` for CI integration.

### JSON Report

JSON reports are generated at `test-results/results.json` for programmatic processing.

## CI/CD Integration

### GitHub Actions

```yaml
- name: Run E2E Tests
  run: |
    cd e2e-tests
    docker-compose up --build --exit-code-from e2e-tests
  env:
    BASE_URL: ${{ secrets.CRM_URL }}
    TEST_USERNAME: ${{ secrets.TEST_USERNAME }}
    TEST_PASSWORD: ${{ secrets.TEST_PASSWORD }}

- name: Upload Test Results
  uses: actions/upload-artifact@v3
  if: always()
  with:
    name: test-results
    path: |
      e2e-tests/test-results/
      e2e-tests/playwright-report/
```

### Jenkins

```groovy
stage('E2E Tests') {
    steps {
        sh '''
            cd e2e-tests
            ./run-tests.sh --url ${CRM_URL}
        '''
    }
    post {
        always {
            archiveArtifacts artifacts: 'e2e-tests/playwright-report/**/*'
            junit 'e2e-tests/test-results/junit-results.xml'
        }
    }
}
```

## Troubleshooting

### Tests timing out

Increase timeout in `playwright.config.ts`:

```typescript
timeout: 60 * 1000, // 60 seconds
```

### Cannot connect to CRM

1. Verify the CRM is running: `curl http://192.168.0.9`
2. Check network mode in docker-compose.yml
3. Use `--url` flag to specify correct URL

### Authentication failing

1. Verify test credentials
2. Check if account is locked
3. Verify auth endpoint is responding

### Screenshot on failure

Screenshots are automatically taken on test failure and saved to `test-results/`.

## Development

### Adding New Tests

1. Create test file in appropriate directory
2. Import fixtures and test data
3. Use Page Object pattern for maintainability
4. Prefix all test data with `TEST_`

### Debugging

```bash
# Run with Playwright Inspector
npm run test:debug

# Run single test
npx playwright test -g "TC-CUST-001"

# Generate trace on failure
npx playwright test --trace on
```

## License

This test suite is part of the CRM Solution project.
