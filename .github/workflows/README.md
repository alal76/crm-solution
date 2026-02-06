# GitHub Actions CI/CD Workflows

This directory contains the active CI/CD workflows for the CRM Solution project.

## Active Workflows

### 1. **ci-cd.yml** - Main CI/CD Pipeline
- **Status**: ✅ Active
- **Triggers**: Push and PR to `main` and `develop` branches
- **Purpose**: Complete CI/CD pipeline with comprehensive testing and Docker builds
- **Jobs**:
  - Frontend Tests & Build (Node 18.x, 20.x)
  - Backend Tests & Build (.NET 8.0)
  - Docker Build (API and Frontend images)
  - Code Quality Checks (ESLint, StyleCop)
  - Security Scan (npm audit, Dependency Check)
  - Integration Tests (with MariaDB)
  - Test Report Generation

### 2. **docker-build-deploy.yml** - Build and Deploy
- **Status**: ✅ Active
- **Triggers**: Push and PR to `main` and `develop` branches
- **Purpose**: Simplified Docker build and deployment workflow
- **Jobs**:
  - Build and push Docker images
  - Backend tests
  - Frontend tests
  - Deploy to Kubernetes (main branch only, with secrets)

### 3. **copilot-swe-agent/copilot** - GitHub Copilot Agent
- **Status**: ✅ Active (Dynamic)
- **Purpose**: GitHub Copilot coding agent workflow
- **Managed by**: GitHub Copilot automation

## Workflow Comparison

| Feature | ci-cd.yml | docker-build-deploy.yml |
|---------|-----------|------------------------|
| Frontend Tests | ✅ Matrix (18.x, 20.x) | ✅ Node 20.x |
| Backend Tests | ✅ .NET 8.0 | ✅ .NET 8.0 |
| Docker Build | ✅ After tests pass | ✅ Always |
| Code Quality | ✅ ESLint, StyleCop | ❌ |
| Security Scan | ✅ npm audit, OWASP | ❌ |
| Integration Tests | ✅ With MariaDB | ❌ |
| Kubernetes Deploy | ❌ | ✅ If secrets configured |
| Test Reports | ✅ Comprehensive | ❌ |

## Recommended Usage

- **For Pull Requests**: Both workflows run automatically
- **For Main Branch**: 
  - `ci-cd.yml` provides comprehensive testing and quality gates
  - `docker-build-deploy.yml` handles deployment if K8s secrets are configured

## Legacy CI/CD Files

The following Azure DevOps pipeline files are preserved for reference but are **NOT actively used**:

- `azure-pipelines.yml` - Legacy Azure DevOps CI/CD Pipeline
- `azure-pipelines-aks.yml` - Legacy Azure Kubernetes Service deployment pipeline

These files are maintained for historical reference and can be reactivated if Azure DevOps integration is needed in the future.

## Secrets Required for Deployment

The `docker-build-deploy.yml` workflow requires the following secrets for deployment:
- `GITHUB_TOKEN` - Automatically provided by GitHub Actions
- `KUBE_CONFIG` - Kubernetes configuration (base64 encoded) - Optional, deployment skipped if not set

## Maintenance Notes

- Both active workflows use the same base images and build configurations
- Docker image tags use Git commit SHA and branch names
- Container registry: `ghcr.io` (GitHub Container Registry)
- All workflows use GitHub Actions cache to speed up builds

## Future Improvements

Consider consolidating the two workflows or making `docker-build-deploy.yml` call `ci-cd.yml` as a prerequisite to avoid duplication and ensure consistent testing before deployment.

---

Last Updated: 2026-02-06
