# GitHub Actions CI/CD Workflows

This directory contains the active CI/CD workflows for the CRM Solution project.

## Active Workflows

### 1. **ci-cd.yml** — Main CI/CD Pipeline
- **Triggers**: Push and PR to `main` and `develop` branches
- **Purpose**: Complete CI/CD pipeline with comprehensive testing and Docker builds
- **Jobs**:
  - **Frontend Tests & Build** — Node 18.x + 20.x matrix, TypeScript check, unit tests, bundle build
  - **Backend Tests & Build** — .NET 8.0, all 3 test projects (Core must pass, others continue-on-error)
  - **BVT (Build Verification Tests)** — Playwright API tests against live API + MariaDB (118 tests, must pass)
  - **Docker Build & Push** — API and Frontend images to GHCR (only after all tests pass)
  - **Code Quality** — ESLint, StyleCop analysis with threshold
  - **Security Scan** — npm audit, .NET vulnerability report
  - **Test Report** — TRX → GitHub check summary

### 2. **docker-build-deploy.yml** — Build and Deploy
- **Triggers**: Push and PR to `main` and `develop` branches
- **Purpose**: Docker build + Kubernetes deployment with full test gates
- **Jobs**:
  - **Build** — Docker images (API + Frontend) to GHCR
  - **Backend Tests** — All 3 test projects (Core must pass)
  - **BVT Tests** — Playwright API tests against live API + MariaDB
  - **Frontend Tests** — Unit tests + production build
  - **Deploy** — Kubernetes (main branch only, requires `KUBE_CONFIG` secret)

### 3. **copilot-swe-agent/copilot** — GitHub Copilot Agent
- **Managed by**: GitHub Copilot automation

## Test Projects

All 3 backend test projects are included in `CRM.sln` and run on every build:

| Project | Assembly | Path | Tests | Description |
|---------|----------|------|-------|-------------|
| CRM.Tests.Unit.Core | CRM.Tests.Unit.Core.dll | `tests/Unit/Core/` | ~2,854 | Entity & DTO unit tests (**all pass**) |
| CRM.Tests.Services | CRM.Tests.Services.dll | `tests/CRM.Tests/` | ~483 | Service & integration tests (23 pre-existing failures) |
| CRM.Tests | CRM.Tests.dll | `tests/` | ~1,766 | Functional & provider tests (68 pre-existing failures) |

**BVT Tests** (Playwright, in `e2e-tests/`):
- 118 API-level tests — no browser required
- Run against live API + MariaDB in CI
- Config: `playwright.bvt.config.ts`

## Workflow Comparison

| Feature | ci-cd.yml | docker-build-deploy.yml |
|---------|-----------|------------------------|
| Frontend Tests | ✅ Matrix (18.x, 20.x) | ✅ Node 20.x |
| Backend Tests | ✅ All 3 test projects | ✅ All 3 test projects |
| BVT Tests | ✅ Playwright + MariaDB | ✅ Playwright + MariaDB |
| Docker Build | ✅ After all tests pass | ✅ Parallel with tests |
| Code Quality | ✅ ESLint, StyleCop | ❌ |
| Security Scan | ✅ npm audit, .NET vuln | ❌ |
| Kubernetes Deploy | ❌ | ✅ If secrets configured |
| Test Reports | ✅ TRX + BVT artifacts | ❌ |
| Build Caching | ✅ GHA cache | ❌ |

## Recommended Usage

- **For Pull Requests**: Both workflows run automatically to validate changes
- **For Main Branch**: 
  - `ci-cd.yml` provides comprehensive testing, quality gates, and Docker image publishing
  - `docker-build-deploy.yml` handles Kubernetes deployment if `KUBE_CONFIG` secret is configured

## Legacy CI/CD Files

The following Azure DevOps pipeline files are preserved for reference but are **NOT actively used**:

- `azure-pipelines.yml` - Legacy Azure DevOps CI/CD Pipeline
- `azure-pipelines-aks.yml` - Legacy Azure Kubernetes Service deployment pipeline

These files are maintained for historical reference and can be reactivated if Azure DevOps integration is needed in the future.

## Secrets Required

| Secret | Workflow | Purpose | Required |
|--------|----------|---------|----------|
| `GITHUB_TOKEN` | Both | Container registry auth | Auto-provided |
| `KUBE_CONFIG` | docker-build-deploy | K8s deployment (base64) | Optional — deploy skipped if absent |

## Container Registry

- **Registry**: `ghcr.io` (GitHub Container Registry)
- **API Image**: `ghcr.io/<owner>/crm-solution/crm-api`
- **Frontend Image**: `ghcr.io/<owner>/crm-solution/crm-frontend`
- **Tags**: branch name, commit SHA, semver (if tagged)

## Maintenance Notes

- Both active workflows use the same base images and build configurations
- Frontend uses `npm ci --legacy-peer-deps` for reproducible installs
- StyleCop analysis runs in **Debug** mode (Release suppresses StyleCop analyzers)
- BVT tests start a live API + MariaDB service container and run 118 Playwright API tests
- Pre-existing test failures in Services/Main use `continue-on-error`; Core and BVT must pass

---

Last Updated: 2026-02-21
