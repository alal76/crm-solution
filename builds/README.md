# Builds Directory

This directory contains compiled build outputs organized by target architecture.

## Directory Structure

```
builds/
├── amd64/           # AMD64/x86_64 compiled outputs
│   ├── backend/     # .NET API publish output
│   ├── frontend/    # React production build
│   └── release/     # Release packages
│
├── arm64/           # ARM64/aarch64 compiled outputs
│   ├── backend/     # .NET API publish output
│   ├── frontend/    # React production build
│   └── release/     # Release packages
│
├── x86/             # 32-bit x86 outputs (legacy)
│   └── .gitkeep
│
└── x86_64/          # Alias for amd64
    └── .gitkeep
```

## Build Commands

### Backend (.NET)

```bash
# Build for current platform
dotnet publish -c Release -o builds/$(uname -m)/backend

# Build for specific runtime
dotnet publish -c Release -r linux-x64 -o builds/amd64/backend
dotnet publish -c Release -r linux-arm64 -o builds/arm64/backend
```

### Frontend (React)

```bash
# Build frontend
cd CRM.Frontend
npm run build
cp -r build ../builds/amd64/frontend/
```

### Full Build Script

Use the `build.sh` script to build for specific architectures:

```bash
# Build for amd64
./build.sh Production Release true amd64

# Build for arm64
./build.sh Production Release true arm64
```

## Runtime Identifiers (RID)

| Architecture | .NET RID | Docker Platform |
|-------------|----------|-----------------|
| amd64 | linux-x64 | linux/amd64 |
| arm64 | linux-arm64 | linux/arm64 |
| x86 | linux-x86 | linux/386 |

## Cleaning Builds

```bash
# Clean all builds
rm -rf builds/*/backend builds/*/frontend

# Clean specific architecture
rm -rf builds/amd64/*
```

## CI/CD Integration

In CI/CD pipelines, build outputs are placed in the appropriate architecture folder:

```yaml
# GitHub Actions example
- name: Build for AMD64
  run: |
    dotnet publish -c Release -r linux-x64 -o builds/amd64/backend
    
- name: Build for ARM64
  run: |
    dotnet publish -c Release -r linux-arm64 -o builds/arm64/backend
```
