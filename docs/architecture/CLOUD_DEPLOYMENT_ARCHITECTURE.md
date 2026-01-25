# Cloud Deployment Architecture

## Overview

The CRM Solution includes a comprehensive cloud deployment management system that allows administrators to:

1. **Configure Cloud Providers** - Set up multiple cloud providers (AWS, Azure, GCP, Kubernetes, Docker, On-Premise)
2. **Manage Deployments** - Create and manage deployment configurations
3. **Trigger Deployments** - Build and deploy applications to configured targets
4. **Track Build Attempts** - View history of all deployment attempts with logs
5. **Monitor Health** - Run on-demand health checks and view health history

## Architecture Diagram

```
┌─────────────────────────────────────────────────────────────────────────────────┐
│                          CLOUD DEPLOYMENT SYSTEM                                 │
└─────────────────────────────────────────────────────────────────────────────────┘
                                      │
     ┌────────────────────────────────┼────────────────────────────────┐
     │                                │                                │
     ▼                                ▼                                ▼
┌──────────────┐            ┌──────────────┐            ┌──────────────┐
│   Frontend   │            │   Backend    │            │   Database   │
│  React UI    │──────────▶│   .NET API   │──────────▶│   MariaDB    │
│              │            │              │            │              │
│ Dashboard    │            │ CloudDeploy- │            │ CloudProviders│
│ Build Status │            │ ment Service │            │ Deployments  │
│ Health Check │            │              │            │ Attempts     │
│ Logs Viewer  │            │ Controller   │            │ HealthLogs   │
└──────────────┘            └──────────────┘            └──────────────┘
                                   │
         ┌─────────────────────────┼─────────────────────────┐
         │                         │                         │
         ▼                         ▼                         ▼
   ┌───────────┐           ┌───────────┐           ┌───────────┐
   │ Kubernetes│           │  Docker   │           │   Cloud   │
   │  kubectl  │           │  CLI      │           │   APIs    │
   │           │           │           │           │ AWS/Azure │
   └───────────┘           └───────────┘           └───────────┘
```

## Database Schema

### CloudProvider Entity

Stores cloud provider configurations and credentials.

```
┌────────────────────────────────────────┐
│            CloudProvider               │
├────────────────────────────────────────┤
│ Id              INT (PK)               │
│ Name            VARCHAR(200)           │
│ ProviderType    ENUM(AWS, Azure,       │
│                 GoogleCloud, etc.)     │
│ Description     VARCHAR(1000)          │
│ AccessKeyId     VARCHAR(500)           │
│ SecretAccessKey VARCHAR(2000)          │
│ TenantId        VARCHAR(200)           │
│ SubscriptionId  VARCHAR(200)           │
│ ProjectId       VARCHAR(200)           │
│ Region          VARCHAR(100)           │
│ Endpoint        VARCHAR(500)           │
│ Configuration   TEXT (JSON)            │
│ IsActive        BOOLEAN                │
│ IsDefault       BOOLEAN                │
│ CreatedAt       DATETIME               │
│ UpdatedAt       DATETIME               │
│ IsDeleted       BOOLEAN                │
└────────────────────────────────────────┘
```

### CloudDeployment Entity

Stores deployment configurations and status.

```
┌────────────────────────────────────────┐
│           CloudDeployment              │
├────────────────────────────────────────┤
│ Id              INT (PK)               │
│ Name            VARCHAR(200)           │
│ Description     VARCHAR(1000)          │
│ CloudProviderId INT (FK)               │
│ ClusterName     VARCHAR(200)           │
│ Namespace       VARCHAR(100)           │
│ ResourceGroup   VARCHAR(200)           │
│ VpcId           VARCHAR(100)           │
│ SubnetIds       VARCHAR(500)           │
│ BackendImage    VARCHAR(500)           │
│ FrontendImage   VARCHAR(500)           │
│ DatabaseImage   VARCHAR(500)           │
│ BackendVersion  VARCHAR(50)            │
│ FrontendVersion VARCHAR(50)            │
│ FrontendUrl     VARCHAR(500)           │
│ ApiUrl          VARCHAR(500)           │
│ DatabaseHost    VARCHAR(200)           │
│ DatabasePort    INT                    │
│ SslEnabled      BOOLEAN                │
│ SslCertificateArn VARCHAR(500)         │
│ DomainName      VARCHAR(300)           │
│ CpuUnits        INT                    │
│ MemoryMb        INT                    │
│ Replicas        INT                    │
│ Status          ENUM(Pending,          │
│                 Provisioning,          │
│                 Building, Deploying,   │
│                 Running, Stopped,      │
│                 Failed, Terminated)    │
│ HealthStatus    ENUM(Unknown, Healthy, │
│                 Degraded, Unhealthy,   │
│                 Offline)               │
│ LastHealthCheck DATETIME               │
│ DeployedAt      DATETIME               │
│ LastError       VARCHAR(2000)          │
│ EnvironmentVariables TEXT (JSON)       │
│ ResourceConfiguration TEXT (JSON)      │
│ CreatedAt       DATETIME               │
│ UpdatedAt       DATETIME               │
│ IsDeleted       BOOLEAN                │
└────────────────────────────────────────┘
```

### DeploymentAttempt Entity

Tracks individual build and deployment attempts.

```
┌────────────────────────────────────────┐
│          DeploymentAttempt             │
├────────────────────────────────────────┤
│ Id              INT (PK)               │
│ CloudDeploymentId INT (FK)             │
│ AttemptNumber   VARCHAR(50)            │
│ Status          ENUM(...)              │
│ GitCommitHash   VARCHAR(100)           │
│ GitBranch       VARCHAR(200)           │
│ BuildNumber     VARCHAR(50)            │
│ BackendImageTag VARCHAR(100)           │
│ FrontendImageTag VARCHAR(100)          │
│ StartedAt       DATETIME               │
│ CompletedAt     DATETIME               │
│ DurationSeconds INT                    │
│ BuildLog        LONGTEXT               │
│ DeployLog       LONGTEXT               │
│ ErrorMessage    VARCHAR(2000)          │
│ ErrorStackTrace TEXT                   │
│ TriggeredByUserId INT                  │
│ TriggerType     VARCHAR(50)            │
│ CreatedAt       DATETIME               │
│ UpdatedAt       DATETIME               │
│ IsDeleted       BOOLEAN                │
└────────────────────────────────────────┘
```

### HealthCheckLog Entity

Records health check history.

```
┌────────────────────────────────────────┐
│           HealthCheckLog               │
├────────────────────────────────────────┤
│ Id              INT (PK)               │
│ CloudDeploymentId INT (FK)             │
│ Status          ENUM(...)              │
│ CheckedAt       DATETIME               │
│ ApiHealthy      BOOLEAN                │
│ FrontendHealthy BOOLEAN                │
│ DatabaseHealthy BOOLEAN                │
│ ApiResponseTimeMs INT                  │
│ FrontendResponseTimeMs INT             │
│ DatabaseResponseTimeMs INT             │
│ ApiResponse     VARCHAR(1000)          │
│ FrontendResponse VARCHAR(1000)         │
│ DatabaseResponse VARCHAR(1000)         │
│ ErrorDetails    VARCHAR(2000)          │
│ CreatedAt       DATETIME               │
│ UpdatedAt       DATETIME               │
│ IsDeleted       BOOLEAN                │
└────────────────────────────────────────┘
```

## API Endpoints

### Cloud Providers

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/clouddeployment/providers` | List all providers |
| GET | `/api/clouddeployment/providers/{id}` | Get provider by ID |
| POST | `/api/clouddeployment/providers` | Create provider |
| PUT | `/api/clouddeployment/providers/{id}` | Update provider |
| DELETE | `/api/clouddeployment/providers/{id}` | Delete provider |
| POST | `/api/clouddeployment/providers/test` | Test connection |
| GET | `/api/clouddeployment/providers/{id}/resources/{type}` | Get resources |

### Deployments

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/clouddeployment/deployments` | List deployments |
| GET | `/api/clouddeployment/deployments/{id}` | Get deployment |
| POST | `/api/clouddeployment/deployments` | Create deployment |
| PUT | `/api/clouddeployment/deployments/{id}` | Update deployment |
| DELETE | `/api/clouddeployment/deployments/{id}` | Delete deployment |
| POST | `/api/clouddeployment/deployments/{id}/deploy` | Trigger deployment |
| POST | `/api/clouddeployment/deployments/{id}/stop` | Stop deployment |
| POST | `/api/clouddeployment/deployments/{id}/restart` | Restart deployment |
| POST | `/api/clouddeployment/deployments/{id}/scale` | Scale replicas |

### Deployment Attempts

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/clouddeployment/deployments/{id}/attempts` | List attempts |
| GET | `/api/clouddeployment/attempts/{id}` | Get attempt |
| GET | `/api/clouddeployment/attempts/{id}/logs` | Get attempt logs |

### Health Checks

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/clouddeployment/deployments/{id}/health-check` | Run health check |
| GET | `/api/clouddeployment/deployments/{id}/health-history` | Get history |
| GET | `/api/clouddeployment/health` | Get all health status |

### Dashboard

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/clouddeployment/dashboard` | Get dashboard summary |

## Frontend UI

The deployment management UI is available in **Settings → Deployment & Hosting** and includes:

### Dashboard Tab
- Summary cards (providers, deployments, healthy, failed counts)
- Active deployments table with status, health, and actions
- Quick actions: Deploy, Stop, Restart, Health Check

### Build Attempts Tab
- Filterable list of all build/deployment attempts
- Status, timing, git info, and image tags
- View logs button for each attempt
- Error messages displayed inline

### Health Checks Tab
- Select deployment and run health check on demand
- Visual health result showing API, Frontend, Database status
- Response times for each component
- Health check history table

### Credentials Tab
- Configure provider credentials (AWS, Azure, GCP, On-Premise)
- Secure credential storage with visibility toggle
- Validation testing

### Scripts Tab
- Generate deployment scripts (Docker Compose, Kubernetes, Terraform, Shell)
- Download or copy generated scripts

### Replicate Tab
- Data replication between environments
- Selective sync (database, settings, users, customizations)

## Supported Cloud Providers

| Provider | Type | Features |
|----------|------|----------|
| AWS | Cloud | EC2, ECS, EKS, RDS |
| Azure | Cloud | ACI, AKS, Azure SQL |
| Google Cloud | Cloud | GKE, Cloud SQL |
| DigitalOcean | Cloud | Kubernetes, Droplets |
| Kubernetes | Self-hosted | kubectl integration |
| Docker | Local | Docker Compose |
| On-Premise | Self-hosted | SSH-based deployment |

## Deployment Flow

```
1. User triggers deployment
   │
   ▼
2. Create DeploymentAttempt (status: Building)
   │
   ▼
3. Execute provider-specific deployment
   │
   ├── Kubernetes: kubectl set image, rollout
   ├── Docker: docker compose up -d
   └── Cloud: Use respective SDK
   │
   ▼
4. Update status (Running or Failed)
   │
   ▼
5. Store endpoints (FrontendUrl, ApiUrl)
   │
   ▼
6. Log completion time and duration
```

## Health Check Flow

```
1. Trigger health check for deployment
   │
   ▼
2. Check API health endpoint
   ├── GET {apiUrl}/health
   └── Record response time
   │
   ▼
3. Check Frontend availability
   ├── GET {frontendUrl}
   └── Record response time
   │
   ▼
4. Check Database health
   ├── GET {apiUrl}/health/database
   └── Record response time
   │
   ▼
5. Calculate overall status
   ├── All healthy → Healthy
   ├── Some healthy → Degraded
   └── None healthy → Unhealthy
   │
   ▼
6. Save HealthCheckLog and update deployment
```

## Security Considerations

1. **Credentials Encryption**: Provider credentials should be encrypted at rest
2. **Role-Based Access**: Only Admin users can access deployment features
3. **Audit Logging**: All deployment actions are logged with user ID
4. **Secret Management**: Use environment variables for sensitive data
5. **Network Security**: Ensure proper VPC/network configuration

## Related Documentation

- [HEXAGONAL_ARCHITECTURE.md](HEXAGONAL_ARCHITECTURE.md) - Overall architecture
- [KUBERNETES_ARCHITECTURE.md](KUBERNETES_ARCHITECTURE.md) - Kubernetes deployment details
- [DATABASE_CONFIGURATION.md](DATABASE_CONFIGURATION.md) - Database setup
- [DEPLOYMENT_GUIDE.md](../deployment/README.md) - Deployment instructions
