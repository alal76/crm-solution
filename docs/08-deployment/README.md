# Deployment Documentation

> **Last Updated:** March 2026 | **Version:** 0.625.0

Complete deployment guide covering Docker, Kubernetes, build scripts, and environment configuration.

---

## Table of Contents

1. [Deployment Options](#1-deployment-options)
2. [Docker Deployment](#2-docker-deployment)
3. [Kubernetes Deployment](#3-kubernetes-deployment)
4. [Build Scripts](#4-build-scripts)
5. [Environment Configuration](#5-environment-configuration)
6. [Database Deployment](#6-database-deployment)

---

## 1. Deployment Options

### 1.1 Architecture Options

| Option | Description | Best For |
|--------|-------------|----------|
| **Monolith (Primary)** | Single API + Frontend with pluggable OSS providers | Development and production baseline |
| **Microservices (Optional)** | Separate services per domain | Targeted scale-out and domain extraction |
| **Hybrid** | Core monolith + specific microservices | Migration path |

### 1.2 Deployment Targets

```
┌─────────────────────────────────────────────────────────────────────────┐
│                        DEPLOYMENT TARGETS                                │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│  ┌─────────────────┐   ┌─────────────────┐   ┌─────────────────┐       │
│  │   Development   │   │    Staging      │   │   Production    │       │
│  │                 │   │                 │   │                 │       │
│  │  docker-compose │   │  Docker Swarm   │   │   Kubernetes    │       │
│  │  localhost      │   │  Single Node    │   │   Multi-Node    │       │
│  │  Hot Reload     │   │  Test Data      │   │   HA Config     │       │
│  └─────────────────┘   └─────────────────┘   └─────────────────┘       │
│                                                                          │
│  Database:             Database:             Database:                   │
│  MariaDB (container)   MariaDB (container)   MariaDB/SQLServer (managed)│
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 2. Docker Deployment

### 2.1 Docker Files

| File | Purpose |
|------|---------|
| `docker/docker-compose.yml` | Main compose file |
| `docker/docker-compose.app.yml` | Application services |
| `docker/docker-compose.databases.yml` | Database services |
| `docker/docker-compose.unified.yml` | Full stack |
| `docker/docker-compose.microservices.unified.yml` | Microservices mode |
| `docker/Dockerfile.backend` | Backend API image |
| `docker/Dockerfile.frontend` | Frontend build image |
| `docker/Dockerfile.frontend.prebuilt` | Frontend nginx image |

### 2.2 Quick Start

```bash
# Development - Full Stack
cd docker
docker-compose -f docker-compose.unified.yml up -d

# View logs
docker-compose -f docker-compose.unified.yml logs -f

# Stop
docker-compose -f docker-compose.unified.yml down
```

### 2.3 Service URLs (Docker)

| Service | URL | Credentials |
|---------|-----|-------------|
| Frontend | http://localhost:3000 | admin / admin123 |
| Backend API | http://localhost:5000 | - |
| Swagger UI | http://localhost:5000/swagger | - |
| MariaDB | localhost:3306 | root / rootpassword |

### 2.4 Docker Compose - Unified

```yaml
# docker/docker-compose.unified.yml
version: '3.8'

services:
  mariadb:
    image: mariadb:10.11
    environment:
      MYSQL_ROOT_PASSWORD: rootpassword
      MYSQL_DATABASE: crm
    ports:
      - "3306:3306"
    volumes:
      - mariadb_data:/var/lib/mysql
      - ./init-scripts:/docker-entrypoint-initdb.d
    healthcheck:
      test: ["CMD", "healthcheck.sh", "--connect", "--innodb_initialized"]
      interval: 10s
      timeout: 5s
      retries: 5

  backend:
    build:
      context: ../CRM.Backend
      dockerfile: ../docker/Dockerfile.backend
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__DefaultConnection=Server=mariadb;Database=crm;User=root;Password=rootpassword;
    ports:
      - "5000:80"
    depends_on:
      mariadb:
        condition: service_healthy

  frontend:
    build:
      context: ../CRM.Frontend
      dockerfile: ../docker/Dockerfile.frontend
    environment:
      - REACT_APP_API_URL=http://localhost:5000
    ports:
      - "3000:80"
    depends_on:
      - backend

volumes:
  mariadb_data:
```

### 2.5 Microservices Mode

```bash
# Build and start microservices
./build-microservices.sh
docker-compose -f docker/docker-compose.microservices.unified.yml up -d
```

**Microservices Endpoints:**

| Service | Port | Path |
|---------|------|------|
| API Gateway | 5000 | / |
| Identity Service | 5001 | /api/auth |
| Customer Service | 5002 | /api/customers |
| Sales Service | 5003 | /api/sales |
| Marketing Service | 5004 | /api/marketing |
| ServiceDesk Service | 5005 | /api/servicedesk |

---

## 3. Kubernetes Deployment

### 3.1 Kubernetes Files

```
kubernetes/
├── 00-namespace-config.yaml      # Namespace and ConfigMaps
├── 01-database-tier.yaml         # Database StatefulSet
├── 02-application-tier.yaml      # Backend Deployment
├── 03-presentation-tier.yaml     # Frontend Deployment
├── 04-ingress-network.yaml       # Ingress configuration
├── local/                        # Local cluster configs
├── microservices/                # Microservices deployment
└── production/                   # Production configs
```

### 3.2 Quick Start (Local)

```bash
# Create namespace
kubectl apply -f kubernetes/00-namespace-config.yaml

# Deploy database
kubectl apply -f kubernetes/01-database-tier.yaml

# Wait for database to be ready
kubectl wait --for=condition=ready pod -l app=mariadb -n crm --timeout=120s

# Deploy backend
kubectl apply -f kubernetes/02-application-tier.yaml

# Deploy frontend
kubectl apply -f kubernetes/03-presentation-tier.yaml

# Configure ingress
kubectl apply -f kubernetes/04-ingress-network.yaml
```

### 3.3 Namespace Configuration

```yaml
# kubernetes/00-namespace-config.yaml
apiVersion: v1
kind: Namespace
metadata:
  name: crm

---
apiVersion: v1
kind: ConfigMap
metadata:
  name: crm-config
  namespace: crm
data:
  ASPNETCORE_ENVIRONMENT: "Production"
  ConnectionStrings__DefaultConnection: "Server=mariadb;Database=crm;User=root;Password=${DB_PASSWORD};"

---
apiVersion: v1
kind: Secret
metadata:
  name: crm-secrets
  namespace: crm
type: Opaque
stringData:
  db-password: "your-secure-password"
  jwt-secret: "your-jwt-secret-key"
```

### 3.4 Database Tier

```yaml
# kubernetes/01-database-tier.yaml
apiVersion: apps/v1
kind: StatefulSet
metadata:
  name: mariadb
  namespace: crm
spec:
  serviceName: mariadb
  replicas: 1
  selector:
    matchLabels:
      app: mariadb
  template:
    metadata:
      labels:
        app: mariadb
    spec:
      containers:
      - name: mariadb
        image: mariadb:10.11
        env:
        - name: MYSQL_ROOT_PASSWORD
          valueFrom:
            secretKeyRef:
              name: crm-secrets
              key: db-password
        - name: MYSQL_DATABASE
          value: crm
        ports:
        - containerPort: 3306
        volumeMounts:
        - name: mariadb-data
          mountPath: /var/lib/mysql
  volumeClaimTemplates:
  - metadata:
      name: mariadb-data
    spec:
      accessModes: ["ReadWriteOnce"]
      resources:
        requests:
          storage: 10Gi
```

### 3.5 Application Tier

```yaml
# kubernetes/02-application-tier.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: crm-backend
  namespace: crm
spec:
  replicas: 2
  selector:
    matchLabels:
      app: crm-backend
  template:
    metadata:
      labels:
        app: crm-backend
    spec:
      containers:
      - name: backend
        image: crm-backend:latest
        ports:
        - containerPort: 80
        envFrom:
        - configMapRef:
            name: crm-config
        env:
        - name: DB_PASSWORD
          valueFrom:
            secretKeyRef:
              name: crm-secrets
              key: db-password
        resources:
          requests:
            memory: "256Mi"
            cpu: "250m"
          limits:
            memory: "512Mi"
            cpu: "500m"
        livenessProbe:
          httpGet:
            path: /health
            port: 80
          initialDelaySeconds: 30
          periodSeconds: 10
        readinessProbe:
          httpGet:
            path: /health/ready
            port: 80
          initialDelaySeconds: 5
          periodSeconds: 5
```

### 3.6 Ingress Configuration

```yaml
# kubernetes/04-ingress-network.yaml
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: crm-ingress
  namespace: crm
  annotations:
    nginx.ingress.kubernetes.io/rewrite-target: /
spec:
  rules:
  - host: crm.local
    http:
      paths:
      - path: /api
        pathType: Prefix
        backend:
          service:
            name: crm-backend
            port:
              number: 80
      - path: /
        pathType: Prefix
        backend:
          service:
            name: crm-frontend
            port:
              number: 80
```

---

## 4. Build Scripts

### 4.1 Main Build Script

```bash
# build.sh
#!/bin/bash
set -e

echo "Building CRM Solution..."

# Build Backend
echo "Building Backend..."
cd CRM.Backend
dotnet restore
dotnet build --configuration Release
dotnet publish -c Release -o ../artifacts/backend

# Build Frontend
echo "Building Frontend..."
cd ../CRM.Frontend
npm ci
npm run build
cp -r build ../artifacts/frontend

echo "Build complete! Artifacts in ./artifacts/"
```

### 4.2 Microservices Build

```bash
# build-microservices.sh
#!/bin/bash
set -e

SERVICES=("Identity" "Customer" "Sales" "Marketing" "ServiceDesk" "Gateway")

for service in "${SERVICES[@]}"; do
    echo "Building ${service} service..."
    cd "CRM.Backend/src/CRM.${service}"
    dotnet publish -c Release -o "../../../artifacts/microservices/${service}"
    cd ../../..
done

echo "All microservices built!"
```

### 4.3 Docker Build

```bash
# Build all Docker images
docker build -t crm-backend:latest -f docker/Dockerfile.backend CRM.Backend/
docker build -t crm-frontend:latest -f docker/Dockerfile.frontend CRM.Frontend/

# Push to registry
docker tag crm-backend:latest registry.example.com/crm/backend:latest
docker push registry.example.com/crm/backend:latest
```

---

## 5. Environment Configuration

### 5.1 Environment Files

| Environment | File | Purpose |
|-------------|------|---------|
| Development | `appsettings.Development.json` | Local dev settings |
| Staging | `appsettings.Staging.json` | Staging environment |
| Production | `appsettings.Production.json` | Production settings |
| Docker | `config/infrastructure.env` | Docker compose vars |

### 5.2 Backend Configuration

```json
// appsettings.Production.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=${DB_SERVER};Database=crm;User=${DB_USER};Password=${DB_PASSWORD};"
  },
  "Jwt": {
    "Secret": "${JWT_SECRET}",
    "Issuer": "CRM",
    "Audience": "CRM-Client",
    "ExpirationMinutes": 60
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

### 5.3 Frontend Configuration

```env
# .env.production
REACT_APP_API_URL=https://api.crm.example.com
REACT_APP_SIGNALR_URL=https://api.crm.example.com/hubs
REACT_APP_ENVIRONMENT=production
```

### 5.4 Infrastructure Environment

```env
# config/infrastructure.env
# Database
DB_HOST=mariadb
DB_PORT=3306
DB_NAME=crm
DB_USER=root
DB_PASSWORD=secure_password

# API
API_PORT=5000
ASPNETCORE_ENVIRONMENT=Production

# Frontend
FRONTEND_PORT=3000
REACT_APP_API_URL=http://localhost:5000

# JWT
JWT_SECRET=your-super-secret-key-min-32-chars
JWT_EXPIRATION_MINUTES=60

# SSL
SSL_ENABLED=true
SSL_CERT_PATH=/etc/ssl/certs/crm.crt
SSL_KEY_PATH=/etc/ssl/private/crm.key
```

---

## 6. Database Deployment

### 6.1 Schema Deployment

```bash
# Deploy database schema
cd database
./deploy.sh

# Manual deployment
mysql -h localhost -u root -p crm < schema/001_core_tables.sql
mysql -h localhost -u root -p crm < schema/002_master_data_tables.sql
mysql -h localhost -u root -p crm < schema/003_service_request_tables.sql
# ... continue with all schema files
```

### 6.2 Schema Files Order

| Order | File | Description |
|-------|------|-------------|
| 1 | `001_core_tables.sql` | Users, Accounts, Contacts |
| 2 | `002_master_data_tables.sql` | Lookup tables, Settings |
| 3 | `003_service_request_tables.sql` | Service tickets |
| 4 | `004_products_opportunities.sql` | Products, Quotes, Opportunities |
| 5 | `005_workflow_tables.sql` | Workflow engine |
| 6 | `006_activities_communication.sql` | Tasks, Notes, Activities |
| 7 | `007_consolidated_contact_info_v2.sql` | Contact info system |
| 8 | `008_security_enhancements.sql` | Password management |

### 6.3 Seed Data

```bash
# Load seed data
mysql -h localhost -u root -p crm < seed/seed_data.sql

# Load master data (countries, states, etc.)
mysql -h localhost -u root -p crm < master_data/countries.sql
mysql -h localhost -u root -p crm < master_data/states.sql
mysql -h localhost -u root -p crm < master_data/zipcodes.sql
```

### 6.4 EF Core Migrations

```bash
# Apply EF Core migrations
cd CRM.Backend/src/CRM.Api
dotnet ef database update --connection "Server=localhost;Database=crm;User=root;Password=password;"

# Generate new migration
dotnet ef migrations add MigrationName --project ../CRM.Infrastructure
```

---

## Deployment Checklist

### Pre-Deployment

- [ ] Backup existing database
- [ ] Review configuration changes
- [ ] Update environment variables
- [ ] Run all tests
- [ ] Review security settings

### Deployment

- [ ] Deploy database changes
- [ ] Deploy backend services
- [ ] Deploy frontend
- [ ] Update load balancer/ingress
- [ ] Verify health endpoints

### Post-Deployment

- [ ] Verify all services are healthy
- [ ] Run smoke tests
- [ ] Check logs for errors
- [ ] Verify user access
- [ ] Update version documentation

---

## Related Documentation

- [INFRASTRUCTURE_GUIDE.md](../INFRASTRUCTURE_GUIDE.md)
- [DATABASE_SETUP.md](../DATABASE_SETUP.md)
- [Docker README](../../docker/README.md)
