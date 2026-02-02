# Build Types - Developer Guide

## Overview

The CRM Solution build system supports four distinct build types, each optimized for different stages of development and deployment. This guide explains when and how to use each build type.

## Quick Reference

| Build Type | Purpose | .NET Config | Log Level | Best For |
|------------|---------|-------------|-----------|----------|
| `dev` | Debugging | Debug | Debug | Local development, bug fixing |
| `test` | QA Testing | Debug | Debug | Automated tests, QA validation |
| `integration` | API Testing | Release | Info | External integrations, API testing |
| `production` | Deployment | Release | Warning | Production environments |

## Build Type Details

### 1. Development (`dev`)

**Command:**
```bash
./scripts/build-parameterized.sh --build-type dev
```

**Purpose:** Maximum visibility into application behavior for debugging

**Features:**
- ✅ **Extensive Logging:** Debug level for all components
- ✅ **Source Maps:** Full source maps for debugging
- ✅ **SQL Logging:** All database queries logged
- ✅ **Request/Response Logging:** Full HTTP request/response details
- ✅ **Performance Metrics:** Timing information for all operations
- ✅ **Stack Traces:** Detailed error stack traces
- ✅ **Console Statements:** All console.log retained in frontend
- ✅ **No Minification:** Easy-to-read code

**Configuration Files:**
- Backend: `appsettings.Dev.json`
- Frontend: `.env.dev`
- .NET: Debug configuration
- Environment: `ASPNETCORE_ENVIRONMENT=Dev`

**Log Files:**
- `logs/dev-YYYYMMDD.log`

**When to Use:**
- Local development on your machine
- Investigating bugs or unexpected behavior
- Understanding application flow
- Learning the codebase
- Performance profiling

**Example Scenarios:**
```bash
# Local development with hot reload
./scripts/build-parameterized.sh --build-type dev --skip-tests

# Debugging a specific issue
./scripts/build-parameterized.sh --build-type dev --components backend

# Full dev build with tests
./scripts/build-parameterized.sh --build-type dev
```

---

### 2. Test (`test`)

**Command:**
```bash
./scripts/build-parameterized.sh --build-type test
```

**Purpose:** Comprehensive testing with full instrumentation

**Features:**
- ✅ **Code Coverage:** Hooks for coverage collection
- ✅ **Test Instrumentation:** Special test hooks and data
- ✅ **Performance Monitoring:** Detailed performance tracking
- ✅ **Mock Data Support:** Test data generation enabled
- ✅ **Test Endpoints:** Special endpoints for testing
- ✅ **Analytics:** Usage tracking for test analysis
- ✅ **Separate Logs:** Performance metrics in separate file
- ✅ **Debug Level Logging:** Full visibility for test debugging

**Configuration Files:**
- Backend: `appsettings.Test.json`
- Frontend: `.env.test`
- .NET: Debug configuration (with optimizations)
- Environment: `ASPNETCORE_ENVIRONMENT=Test`

**Log Files:**
- `logs/test-YYYYMMDD.log`
- `logs/test-performance-YYYYMMDD.log`

**When to Use:**
- Running automated test suites
- QA validation and testing
- Performance benchmarking
- Code coverage analysis
- Load testing preparation
- CI/CD test stage

**Example Scenarios:**
```bash
# Build for QA team
./scripts/build-parameterized.sh --build-type test --env staging

# Build with code coverage
./scripts/build-parameterized.sh --build-type test --tag test-$(date +%Y%m%d)

# Full test build for CI/CD
./scripts/build-parameterized.sh --build-type test --arch microservices --push
```

**Testing Features:**
- Mock data generation
- Test user accounts
- Special test endpoints (e.g., `/api/test/reset`)
- Performance metrics collection
- Request/response recording

---

### 3. Integration (`integration`)

**Command:**
```bash
./scripts/build-parameterized.sh --build-type integration
```

**Purpose:** Focus on external integrations and API interactions

**Features:**
- ✅ **API Logging:** Verbose logging for API calls
- ✅ **External API Tracking:** Log all external service calls
- ✅ **Middleware Logging:** Request pipeline visibility
- ✅ **HTTP Logging:** Full HTTP request/response logging
- ✅ **Circuit Breaker Logs:** Failure and retry tracking
- ✅ **Integration Points:** All external touchpoints logged
- ✅ **Release Build:** Optimized performance
- ✅ **Separate API Logs:** Dedicated log file for API calls

**Configuration Files:**
- Backend: `appsettings.Integration.json`
- Frontend: `.env.integration`
- .NET: Release configuration
- Environment: `ASPNETCORE_ENVIRONMENT=Integration`

**Log Files:**
- `logs/integration-YYYYMMDD.log`
- `logs/integration-api-YYYYMMDD.log`

**When to Use:**
- Testing third-party API integrations
- OAuth/authentication flow testing
- External service integration validation
- Webhook testing
- API contract verification
- Integration environment deployment

**Example Scenarios:**
```bash
# Test OAuth integrations
./scripts/build-parameterized.sh --build-type integration --env staging

# Validate external APIs
./scripts/build-parameterized.sh --build-type integration --components backend

# Deploy to integration environment
./scripts/build-parameterized.sh --build-type integration --push --tag integration-v1.2
```

**Integration Features:**
- Logs all external API calls
- Tracks retry attempts
- Records circuit breaker state
- Monitors webhook deliveries
- Logs OAuth flows

---

### 4. Production (`production`)

**Command:**
```bash
./scripts/build-parameterized.sh --build-type production
```

**Purpose:** Optimized, secure, performant code for production deployment

**Features:**
- ✅ **Minimal Logging:** Warning and Error only
- ✅ **No Source Maps:** Security and size optimization
- ✅ **Full Minification:** Smallest possible bundle
- ✅ **Console Removal:** All console statements stripped
- ✅ **Aggressive Optimization:** Maximum performance
- ✅ **No Sensitive Data:** No sensitive info in logs
- ✅ **Release Build:** Optimized compilation
- ✅ **Security Hardened:** Minimal attack surface

**Configuration Files:**
- Backend: `appsettings.Production.json`
- Frontend: `.env.production`
- .NET: Release configuration
- Environment: `ASPNETCORE_ENVIRONMENT=Production`

**Log Files:**
- `logs/production-YYYYMMDD.log` (errors/warnings only)

**When to Use:**
- Production deployments
- Public-facing applications
- Performance-critical systems
- Security-sensitive environments
- Customer-facing services
- Live production systems

**Example Scenarios:**
```bash
# Production release
./scripts/build-parameterized.sh --build-type production --env production --tag v1.0.0 --push

# Production hotfix
./scripts/build-parameterized.sh --build-type production --tag v1.0.1-hotfix --push

# Production microservices
./scripts/build-parameterized.sh --build-type production --arch microservices --push
```

**Production Optimizations:**
- Tree shaking enabled
- Dead code elimination
- Bundle size minimization
- Gzip compression
- Cache optimization
- CDN-ready assets

---

## Choosing the Right Build Type

### Decision Tree

```
Need to debug code? → dev
Need to run tests? → test
Testing integrations? → integration
Deploying to production? → production
```

### By Environment

| Environment | Recommended Build Type |
|-------------|----------------------|
| Local Developer Machine | `dev` |
| CI/CD Test Stage | `test` |
| Integration Test Environment | `integration` |
| Staging (Pre-Production) | `production` or `integration` |
| Production | `production` |

### By Activity

| Activity | Build Type |
|----------|-----------|
| Writing new features | `dev` |
| Fixing bugs | `dev` |
| Running unit tests | `test` |
| Running integration tests | `integration` |
| Performance testing | `test` |
| API testing | `integration` |
| Security testing | `production` |
| Production deployment | `production` |

---

## Configuration Details

### Backend (.NET)

Each build type loads a specific `appsettings.{BuildType}.json` file:

```
dev → appsettings.Dev.json
test → appsettings.Test.json
integration → appsettings.Integration.json
production → appsettings.Production.json
```

The file is selected automatically based on `ASPNETCORE_ENVIRONMENT`.

### Frontend (React)

Each build type uses a specific `.env` file:

```
dev → .env.dev
test → .env.test
integration → .env.integration
production → .env.production
```

Build commands:
```bash
npm run build:dev         # Uses .env.dev
npm run build:test        # Uses .env.test
npm run build:integration # Uses .env.integration
npm run build:production  # Uses .env.production
```

---

## Advanced Usage

### Combining Options

```bash
# Dev build for microservices
./scripts/build-parameterized.sh --build-type dev --arch microservices

# Test build without tests (for speed)
./scripts/build-parameterized.sh --build-type test --skip-tests

# Production build with specific tag
./scripts/build-parameterized.sh --build-type production --tag v2.0.0

# Integration build for specific components
./scripts/build-parameterized.sh --build-type integration --components backend
```

### Custom Configuration

You can override build type settings in `.env`:

```bash
# Force a specific .NET configuration
DOTNET_CONFIGURATION=Release

# Override log level
LOG_LEVEL=Information

# Set custom environment
ASPNETCORE_ENVIRONMENT=CustomEnv
```

---

## Troubleshooting

### Issue: Wrong log level in output

**Solution:** Ensure the correct `appsettings.{BuildType}.json` file exists and `ASPNETCORE_ENVIRONMENT` is set correctly.

```bash
# Check environment
./scripts/build-parameterized.sh --build-type dev | grep "ASPNETCORE Environment"
```

### Issue: Frontend not using correct env file

**Solution:** Verify the build script in `package.json`:

```json
"build:dev": "env-cmd -f .env.dev craco build"
```

Ensure `env-cmd` package is installed:
```bash
cd CRM.Frontend && npm install env-cmd --save-dev
```

### Issue: Source maps in production

**Solution:** Verify `GENERATE_SOURCEMAP=false` in `.env.production`

### Issue: Console logs in production

**Solution:** Ensure `babel-plugin-transform-remove-console` is configured in production build

---

## FAQ

**Q: Can I create custom build types?**

A: Yes! Add a new case in `scripts/build-parameterized.sh` and create corresponding `appsettings` and `.env` files.

**Q: Which build type should I use for staging?**

A: Use `production` for a production-like environment, or `integration` if you're testing external services.

**Q: Do build types affect deployment?**

A: Build types affect compilation and configuration. Deployment is controlled by `--env` and `--platform` parameters.

**Q: Can I mix build types (e.g., dev backend with production frontend)?**

A: Not directly with the build script, but you can build components separately with different types.

**Q: How do I add custom logging to a build type?**

A: Edit the corresponding `appsettings.{BuildType}.json` file to add custom Serilog sinks or adjust log levels.

---

## Summary

- ✅ **4 Build Types:** dev, test, integration, production
- ✅ **Automatic Configuration:** Each type loads appropriate settings
- ✅ **Optimized for Purpose:** Each type tailored to specific use case
- ✅ **Easy to Use:** Single `--build-type` parameter
- ✅ **Well Documented:** Clear guidance on when to use each type

For more information, see:
- [Parameterized CI/CD Guide](PARAMETERIZED_CICD_GUIDE.md)
- [Configuration Documentation](.cicd-config.yml)
- [Build Script](../scripts/build-parameterized.sh)
