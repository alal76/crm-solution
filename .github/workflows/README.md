# GitHub Actions CI/CD Workflows

This directory contains the active CI/CD workflows for the CRM Solution project.

## Active Workflows

### 1. **ci-cd.yml** — Main CI/CD Pipeline
- **Triggers**: Push and PR to `main` and `develop` branches
- **Purpose**: Complete CI/CD pipeline with comprehensive testing and Docker builds
- **Jobs**:
  - **Frontend Tests & Build** — Node 20.x, TypeScript check, unit tests, bundle build
  - **Backend Tests & Build** — .NET 10.0, all 3 test projects (all must pass)
  - **BVT (Build Verification Tests)** — Playwright API tests against live API + MariaDB (118 tests, must pass)
  - **Docker Build & Push** — API and Frontend images to GHCR (only after all tests pass)
  - **Code Quality** — ESLint, StyleCop analysis with threshold
  - **Security Scan** — npm audit, .NET vulnerability report
  - **Test Report** — TRX → GitHub check summary

### 2. **docker-build-deploy.yml** — Build and Deploy
- **Triggers**: Push and PR to `main` and `develop` branches
- **Purpose**: Docker build + Kubernetes deployment (testing handled by ci-cd.yml)
- **Jobs**:
  - **Build** — Docker images (API + Frontend) to GHCR with GHA build cache
  - **Deploy** — Kubernetes (main branch only, requires `KUBE_CONFIG` secret)

  ### 3. **release.yml** — GitHub Release + Helm Packages
  - **Triggers**: Git tags matching `v*.*.*` or manual dispatch
  - **Purpose**: Build and push Docker images to GHCR, package Helm charts, and publish a GitHub release
  - **Jobs**:
    - **Build & Push** — API + Frontend images to GHCR
    - **Helm Package** — Monolith and microservices charts with checksums
    - **Release** — GitHub release with chart artifacts attached

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

| Feature | ci-cd.yml | docker-build-deploy.yml | release.yml |
|---------|-----------|------------------------|------------|
| Frontend Tests | ✅ Matrix (18.x, 20.x) | ❌ (handled by ci-cd.yml) | ❌ |
| Backend Tests | ✅ All 3 test projects | ❌ (handled by ci-cd.yml) | ❌ |
| BVT Tests | ✅ Playwright + MariaDB | ❌ (handled by ci-cd.yml) | ❌ |
| Docker Build | ✅ After all tests pass | ✅ Parallel (GHA cache) | ✅ On tag |
| Code Quality | ✅ ESLint, StyleCop | ❌ | ❌ |
| Security Scan | ✅ npm audit, .NET vuln | ❌ | ❌ |
| Kubernetes Deploy | ❌ | ✅ If secrets configured | ❌ |
| Helm Package | ❌ | ❌ | ✅ |
| GitHub Release | ❌ | ❌ | ✅ |
| Test Reports | ✅ TRX + BVT artifacts | ❌ | ❌ |
| Build Caching | ✅ GHA cache | ✅ GHA cache | ✅ GHA cache |

## Recommended Usage

- **For Pull Requests**: `ci-cd.yml` runs all tests, quality checks, and Docker builds. `docker-build-deploy.yml` builds Docker images only.
- **For Main Branch**: 
  - `ci-cd.yml` provides comprehensive testing, quality gates, and Docker image publishing
  - `docker-build-deploy.yml` builds Docker images and handles Kubernetes deployment if `KUBE_CONFIG` secret is configured

## Legacy CI/CD Files

The following Azure DevOps pipeline files have been renamed to `.disabled` and are **NOT actively used**:

- `azure-pipelines.yml.disabled` - Legacy Azure DevOps CI/CD Pipeline
- `azure-pipelines-aks.yml.disabled` - Legacy Azure Kubernetes Service deployment pipeline

These files are maintained for historical reference and can be reactivated by removing the `.disabled` suffix if Azure DevOps integration is needed.

## Secrets Required

| Secret | Workflow | Purpose | Required |
|--------|----------|---------|----------|
| `GITHUB_TOKEN` | All | Container registry auth + releases | Auto-provided |
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
- All 3 backend test projects must pass; Core and BVT are critical gates

---

Last Updated: 2026-02-21
