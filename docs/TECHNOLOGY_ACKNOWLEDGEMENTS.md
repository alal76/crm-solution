# CRM Solution - Technology Acknowledgements

## Overview

This document acknowledges all the technologies, frameworks, libraries, tools, and companies that provide the foundational pillars upon which the CRM Solution is built. We extend our sincere gratitude to these organizations and open-source communities for their contributions to the software industry.

---

## 📋 Table of Contents

- [Programming Languages](#programming-languages)
- [Backend Framework & Libraries](#backend-framework--libraries)
- [Frontend Framework & Libraries](#frontend-framework--libraries)
- [Database Systems](#database-systems)
- [Data Sources](#data-sources)
- [Build Tools & Compilers](#build-tools--compilers)
- [Container & Orchestration](#container--orchestration)
- [Operating Systems](#operating-systems)
- [Hosting Platforms](#hosting-platforms)
- [Development Tools](#development-tools)
- [Testing Frameworks](#testing-frameworks)
- [Color Theme & Design](#color-theme--design)
- [Security & Authentication](#security--authentication)
- [Monitoring & Logging](#monitoring--logging)

---

## 🖥️ Programming Languages

| Language | Version | Provider | Purpose | Website |
|----------|---------|----------|---------|---------|
| **C#** | 12.0 | Microsoft Corporation | Backend API, Business Logic, Services | [docs.microsoft.com](https://docs.microsoft.com/en-us/dotnet/csharp/) |
| **TypeScript** | 4.9.5 | Microsoft Corporation | Frontend Type Safety, React Components | [typescriptlang.org](https://www.typescriptlang.org/) |
| **JavaScript** | ES2022 | Ecma International | Frontend Runtime, Build Scripts | [ecma-international.org](https://www.ecma-international.org/) |
| **SQL** | ANSI | Various | Database Queries, Migrations | - |
| **Bash/Zsh** | 5.x | GNU Project / Apple Inc. | Build Scripts, Automation | [gnu.org/software/bash](https://www.gnu.org/software/bash/) |
| **Python** | 3.x | Python Software Foundation | Data Processing Scripts | [python.org](https://www.python.org/) |
| **YAML** | 1.2 | yaml.org | Configuration Files, Kubernetes Manifests | [yaml.org](https://yaml.org/) |
| **JSON** | RFC 8259 | IETF | API Payloads, Configuration | [json.org](https://www.json.org/) |
| **Markdown** | CommonMark | John Gruber | Documentation | [commonmark.org](https://commonmark.org/) |

---

## ⚙️ Backend Framework & Libraries

### Core Framework

| Component | Version | Provider | Purpose |
|-----------|---------|----------|---------|
| **.NET** | 8.0 LTS | Microsoft Corporation | Runtime Platform |
| **ASP.NET Core** | 8.0 | Microsoft Corporation | Web API Framework |
| **Entity Framework Core** | 8.0 | Microsoft Corporation | Object-Relational Mapper (ORM) |

### NuGet Packages

| Package | Version | Provider | Purpose |
|---------|---------|----------|---------|
| **Serilog** | 3.1.1 | Serilog Contributors | Structured Logging |
| **Serilog.AspNetCore** | 8.0.0 | Serilog Contributors | ASP.NET Core Integration |
| **Serilog.Sinks.Console** | 5.0.1 | Serilog Contributors | Console Output |
| **Serilog.Sinks.File** | 5.0.0 | Serilog Contributors | File Logging |
| **Swashbuckle.AspNetCore** | 6.5.0 | Swashbuckle Contributors | Swagger/OpenAPI |
| **Microsoft.AspNetCore.Authentication.JwtBearer** | 8.0.0 | Microsoft Corporation | JWT Authentication |
| **BCrypt.Net-Next** | 4.0.3 | Chris McKee | Password Hashing |
| **Google.Apis.Auth** | 1.64.0 | Google LLC | OAuth2 Authentication |
| **Pomelo.EntityFrameworkCore.MySql** | 8.0.0 | Pomelo Foundation | MariaDB/MySQL Provider |
| **Npgsql.EntityFrameworkCore.PostgreSQL** | 8.0.0 | Npgsql Contributors | PostgreSQL Provider |
| **Oracle.EntityFrameworkCore** | 8.21.121 | Oracle Corporation | Oracle Provider |
| **QRCoder** | 1.4.3 | Raffael Herrmann | QR Code Generation |

---

## 🎨 Frontend Framework & Libraries

### Core Framework

| Component | Version | Provider | Purpose |
|-----------|---------|----------|---------|
| **React** | 18.2.0 | Meta Platforms, Inc. | UI Component Library |
| **React DOM** | 18.2.0 | Meta Platforms, Inc. | DOM Rendering |
| **React Router DOM** | 6.18.0 | Remix Software Inc. | Client-Side Routing |

### UI Component Libraries

| Package | Version | Provider | Purpose |
|---------|---------|----------|---------|
| **@mui/material** | 5.14.15 | MUI (Material-UI) | Material Design Components |
| **@mui/icons-material** | 5.14.15 | MUI (Material-UI) | Material Icons |
| **@mui/lab** | 5.0.0-alpha.61 | MUI (Material-UI) | Experimental Components |
| **@mui/x-data-grid** | 7.3.0 | MUI (Material-UI) | Advanced Data Grid |
| **@emotion/react** | 11.11.1 | Emotion Contributors | CSS-in-JS Styling |
| **@emotion/styled** | 11.11.0 | Emotion Contributors | Styled Components |

### Utility Libraries

| Package | Version | Provider | Purpose |
|---------|---------|----------|---------|
| **Axios** | 1.6.0 | Matt Zabriskie & Contributors | HTTP Client |
| **Formik** | 2.4.5 | Jared Palmer | Form State Management |
| **Yup** | 1.3.3 | Jason Quense | Schema Validation |
| **Recharts** | 2.10.3 | Recharts Contributors | Data Visualization |
| **react-beautiful-dnd** | 13.1.1 | Atlassian | Drag and Drop |
| **react-icons** | 4.12.0 | React Icons Contributors | Icon Library |
| **qrcode.react** | 3.1.0 | Paul O'Shannessy | QR Code Rendering |
| **@react-oauth/google** | 0.12.1 | @react-oauth Contributors | Google OAuth |
| **ajv** | 8.17.1 | Evgeny Poberezkin | JSON Schema Validation |

### Build Tools

| Package | Version | Provider | Purpose |
|---------|---------|----------|---------|
| **@craco/craco** | 7.1.0 | CRACO Contributors | Create React App Configuration Override |
| **react-scripts** | 5.0.1 | Meta Platforms, Inc. | Build Toolchain |
| **Webpack** | 5.x | Webpack Contributors | Module Bundler (via react-scripts) |
| **Babel** | 7.x | Babel Contributors | JavaScript Compiler (via react-scripts) |

---

## 🗄️ Database Systems

| Database | Version | Provider | Purpose | License |
|----------|---------|----------|---------|---------|
| **MariaDB** | 10.11+ | MariaDB Foundation | Primary Production Database | GPL v2 |
| **MySQL** | 8.0+ | Oracle Corporation | Alternative RDBMS | GPL v2 |
| **PostgreSQL** | 15+ | PostgreSQL Global Development Group | Alternative RDBMS | PostgreSQL License |
| **SQL Server** | 2019+ | Microsoft Corporation | Enterprise RDBMS | Commercial |
| **Oracle Database** | 19c+ | Oracle Corporation | Enterprise RDBMS | Commercial |
| **SQLite** | 3.x | SQLite Consortium | Development/Testing | Public Domain |

---

## 📊 Data Sources

### Geographic Data

| Data Source | Provider | Purpose | License |
|-------------|----------|---------|---------|
| **GeoNames** | GeoNames.org | Worldwide Postal Codes, Cities, Countries | Creative Commons Attribution 4.0 |
| **Country Codes (ISO 3166-1)** | ISO | Country Identification | ISO Standard |
| **State/Province Data** | GeoNames.org | Administrative Divisions | CC BY 4.0 |

### Zip Code Data Structure

| Field | Source | Coverage |
|-------|--------|----------|
| **Postal Codes** | GeoNames | 80+ Countries, 4M+ Records |
| **City Names** | GeoNames | Global Coverage |
| **State/Province** | GeoNames | All Countries |
| **Latitude/Longitude** | GeoNames | Precise Coordinates |
| **Country Codes** | ISO 3166-1 | All UN Member States |

---

## 🔧 Build Tools & Compilers

### .NET Build Chain

| Tool | Version | Provider | Purpose |
|------|---------|----------|---------|
| **.NET SDK** | 8.0 | Microsoft Corporation | Build, Publish, Test |
| **MSBuild** | 17.x | Microsoft Corporation | Project Build System |
| **NuGet** | 6.x | Microsoft Corporation | Package Manager |
| **dotnet CLI** | 8.0 | Microsoft Corporation | Command Line Interface |
| **Roslyn Compiler** | 4.x | Microsoft Corporation | C# Compiler |

### Frontend Build Chain

| Tool | Version | Provider | Purpose |
|------|---------|----------|---------|
| **Node.js** | 18.x / 20.x | OpenJS Foundation | JavaScript Runtime |
| **npm** | 9.x / 10.x | npm, Inc. | Package Manager |
| **Webpack** | 5.x | Webpack Contributors | Module Bundler |
| **Babel** | 7.x | Babel Contributors | JavaScript Transpiler |
| **TypeScript Compiler** | 4.9.5 | Microsoft Corporation | TypeScript to JavaScript |
| **ESLint** | 8.x | OpenJS Foundation | Code Linting |
| **PostCSS** | 8.x | PostCSS Contributors | CSS Processing |

---

## 🐳 Container & Orchestration

### Container Platform

| Technology | Version | Provider | Purpose |
|------------|---------|----------|---------|
| **Docker** | 24.x+ | Docker, Inc. | Container Runtime |
| **Docker Compose** | 2.x | Docker, Inc. | Multi-Container Orchestration |
| **containerd** | 1.7+ | CNCF | Container Runtime |
| **BuildKit** | Latest | Docker, Inc. | Build System |

### Container Registries

| Registry | Provider | Purpose |
|----------|----------|---------|
| **Docker Hub** | Docker, Inc. | Public Container Registry |
| **GitHub Container Registry** | GitHub (Microsoft) | CI/CD Integration |
| **Azure Container Registry** | Microsoft Corporation | Azure Hosting |
| **Amazon ECR** | Amazon Web Services | AWS Hosting |
| **Google Container Registry** | Google LLC | GCP Hosting |

### Orchestration

| Technology | Version | Provider | Purpose |
|------------|---------|----------|---------|
| **Kubernetes** | 1.28+ | CNCF | Container Orchestration |
| **Minikube** | 1.32+ | Kubernetes SIG | Local Development Cluster |
| **kubectl** | 1.28+ | Kubernetes SIG | Kubernetes CLI |
| **Helm** | 3.x | CNCF | Kubernetes Package Manager |

### Kubernetes Components Used

| Component | Purpose |
|-----------|---------|
| **Deployments** | Workload Management |
| **Services** | Network Exposure |
| **ConfigMaps** | Configuration |
| **Secrets** | Sensitive Data |
| **PersistentVolumeClaims** | Storage |
| **Ingress** | External Access |
| **Namespaces** | Resource Isolation |

---

## 💻 Operating Systems

### Development Environments

| OS | Version | Provider | Purpose |
|----|---------|----------|---------|
| **macOS** | 13+ (Ventura/Sonoma) | Apple Inc. | Primary Development |
| **Windows** | 10/11 | Microsoft Corporation | Alternative Development |
| **Ubuntu** | 22.04/24.04 LTS | Canonical Ltd. | Linux Development |

### Production/Container Base Images

| Image | Version | Provider | Purpose |
|-------|---------|----------|---------|
| **Alpine Linux** | 3.18+ | Alpine Linux | Minimal Container Base |
| **mcr.microsoft.com/dotnet/aspnet** | 8.0 | Microsoft Corporation | .NET Runtime Image |
| **mcr.microsoft.com/dotnet/sdk** | 8.0 | Microsoft Corporation | .NET Build Image |
| **node** | 18-alpine / 20-alpine | OpenJS Foundation | Frontend Build Image |
| **nginx** | alpine | NGINX Inc. / F5 | Web Server |
| **mariadb** | 10.11 | MariaDB Foundation | Database Image |

---

## ☁️ Hosting Platforms

### Cloud Providers

| Provider | Services Used | Purpose |
|----------|---------------|---------|
| **Amazon Web Services (AWS)** | EKS, EC2, RDS, S3 | Cloud Hosting |
| **Microsoft Azure** | AKS, App Service, Azure SQL | Cloud Hosting |
| **Google Cloud Platform (GCP)** | GKE, Cloud Run, Cloud SQL | Cloud Hosting |
| **DigitalOcean** | Kubernetes, Droplets | Cloud Hosting |
| **Linode (Akamai)** | LKE, Compute | Cloud Hosting |

### Local Development

| Platform | Purpose |
|----------|---------|
| **Minikube** | Local Kubernetes Cluster |
| **Docker Desktop** | Local Container Runtime |
| **kind (Kubernetes in Docker)** | Lightweight Local Cluster |

---

## 🛠️ Development Tools

### IDEs & Editors

| Tool | Provider | Purpose |
|------|----------|---------|
| **Visual Studio Code** | Microsoft Corporation | Primary IDE |
| **Visual Studio** | Microsoft Corporation | .NET Development |
| **JetBrains Rider** | JetBrains | .NET Development |
| **WebStorm** | JetBrains | Frontend Development |

### VS Code Extensions

| Extension | Provider | Purpose |
|-----------|----------|---------|
| **C#** | Microsoft | C# Language Support |
| **ESLint** | Microsoft | JavaScript Linting |
| **Prettier** | Prettier | Code Formatting |
| **Docker** | Microsoft | Container Management |
| **Kubernetes** | Microsoft | K8s Management |
| **GitLens** | GitKraken | Git Integration |
| **REST Client** | Huachao Mao | API Testing |

### Version Control

| Tool | Provider | Purpose |
|------|----------|---------|
| **Git** | Software Freedom Conservancy | Version Control |
| **GitHub** | GitHub (Microsoft) | Repository Hosting |
| **GitHub Actions** | GitHub (Microsoft) | CI/CD Pipeline |

---

## 🧪 Testing Frameworks

### Backend Testing

| Framework | Version | Provider | Purpose |
|-----------|---------|----------|---------|
| **xUnit** | 2.6.2 | .NET Foundation | Unit Test Framework |
| **Moq** | 4.20.70 | Moq Contributors | Mocking Framework |
| **FluentAssertions** | 6.12.0 | Dennis Doomen | Assertion Library |
| **Microsoft.NET.Test.Sdk** | 17.8.0 | Microsoft Corporation | Test SDK |
| **coverlet.collector** | 6.0.0 | Coverlet Contributors | Code Coverage |
| **EF Core InMemory** | 8.0.0 | Microsoft Corporation | In-Memory Testing |

### Frontend Testing

| Framework | Version | Provider | Purpose |
|-----------|---------|----------|---------|
| **Jest** | 29.x | Meta Platforms, Inc. | Test Runner |
| **React Testing Library** | 14.1.2 | Testing Library Contributors | Component Testing |
| **@testing-library/jest-dom** | 6.9.1 | Testing Library Contributors | DOM Matchers |
| **@testing-library/user-event** | 14.6.1 | Testing Library Contributors | User Interaction |

---

## 🎨 Color Theme & Design

### Material Design System

| Aspect | Implementation | Provider |
|--------|----------------|----------|
| **Design System** | Material Design 3 | Google LLC |
| **Component Library** | MUI v5 | MUI |
| **Icons** | Material Icons | Google LLC |

### Color Palette

| Color | Hex Code | Usage |
|-------|----------|-------|
| **Primary Blue** | `#1976d2` | Primary Actions, Headers |
| **Primary Dark** | `#1565c0` | Hover States |
| **Primary Light** | `#42a5f5` | Highlights |
| **Secondary Purple** | `#9c27b0` | Secondary Actions |
| **Success Green** | `#2e7d32` | Success States |
| **Warning Orange** | `#ed6c02` | Warning States |
| **Error Red** | `#d32f2f` | Error States |
| **Info Blue** | `#0288d1` | Informational |
| **Background Light** | `#f5f5f5` | Light Mode Background |
| **Background Dark** | `#121212` | Dark Mode Background |
| **Text Primary** | `rgba(0,0,0,0.87)` | Primary Text |
| **Text Secondary** | `rgba(0,0,0,0.6)` | Secondary Text |

### Typography

| Font | Provider | Usage |
|------|----------|-------|
| **Roboto** | Google Fonts | Primary Font Family |
| **Roboto Mono** | Google Fonts | Code Display |

---

## 🔐 Security & Authentication

| Technology | Provider | Purpose |
|------------|----------|---------|
| **JWT (JSON Web Tokens)** | IETF (RFC 7519) | Authentication Tokens |
| **BCrypt** | Niels Provos & David Mazières | Password Hashing |
| **OAuth 2.0** | IETF (RFC 6749) | Authorization Framework |
| **OpenID Connect** | OpenID Foundation | Identity Layer |
| **Google OAuth** | Google LLC | Social Login |
| **HTTPS/TLS** | IETF | Transport Security |

---

## 📈 Monitoring & Logging

| Technology | Provider | Purpose |
|------------|----------|---------|
| **Serilog** | Serilog Contributors | Structured Logging |
| **Prometheus** | CNCF | Metrics Collection |
| **Grafana** | Grafana Labs | Visualization |
| **Health Checks** | Microsoft Corporation | Service Health Monitoring |

---

## 📄 License Acknowledgements

### Open Source Licenses Used

| License | Packages Using |
|---------|----------------|
| **MIT License** | React, MUI, Axios, Formik, Yup, most npm packages |
| **Apache License 2.0** | .NET Core, Entity Framework Core, Kubernetes |
| **BSD License** | Flask-related utilities, some Python packages |
| **GPL v2/v3** | MariaDB, MySQL |
| **PostgreSQL License** | PostgreSQL |
| **ISC License** | Various npm packages |
| **Creative Commons** | GeoNames data |

---

## 🙏 Special Thanks

We extend our heartfelt gratitude to:

1. **The .NET Team at Microsoft** - For creating an exceptional cross-platform development framework
2. **Meta (Facebook) React Team** - For revolutionizing frontend development
3. **The MUI Team** - For providing beautiful, accessible components
4. **MariaDB Foundation** - For maintaining an excellent open-source database
5. **Cloud Native Computing Foundation (CNCF)** - For Kubernetes and cloud-native technologies
6. **GeoNames.org** - For comprehensive geographic data
7. **All Open Source Contributors** - Whose countless hours make projects like this possible

---

## 📝 Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.0.0 | January 2026 | Initial acknowledgements document |
| 1.3.1 | January 25, 2026 | Comprehensive update with all components |

---

*This document is maintained as part of the CRM Solution and updated with each major release.*

**CRM Solution Version**: 1.3.1  
**Last Updated**: January 25, 2026  
**Maintainer**: Abhishek Lal

