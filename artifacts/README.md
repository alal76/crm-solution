# Binary Files Directory

This directory contains architecture-specific binary files and Docker images.

## Directory Structure

```
bin/
├── amd64/           # AMD64/x86_64 Linux binaries and Docker images
│   ├── api/         # Backend API binaries
│   ├── frontend/    # Frontend build artifacts
│   └── *.tar.gz     # Docker image tarballs
│
├── arm64/           # ARM64/aarch64 binaries and Docker images
│   ├── api/         # Backend API binaries
│   ├── frontend/    # Frontend build artifacts
│   └── *.tar.gz     # Docker image tarballs
│
├── x86/             # 32-bit x86 binaries (if needed)
│   └── .gitkeep
│
└── x86_64/          # Alias for amd64 (for compatibility)
    └── .gitkeep
```

## Architecture Naming Convention

| Architecture | Common Names | Description |
|-------------|--------------|-------------|
| `amd64` | x86_64, x64, Intel 64 | 64-bit Intel/AMD processors |
| `arm64` | aarch64, ARM64 | 64-bit ARM processors (Apple M1/M2, AWS Graviton) |
| `x86` | i386, i686 | 32-bit Intel/AMD processors (legacy) |
| `x86_64` | amd64 | Alias for amd64 |

## Building for Specific Architectures

### Docker Multi-Platform Build

```bash
# Build for amd64
docker build --platform linux/amd64 -t crm-api:amd64 -f Dockerfile.backend .

# Build for arm64
docker build --platform linux/arm64 -t crm-api:arm64 -f Dockerfile.backend .
```

### Export Docker Images

```bash
# Export amd64 image
docker save crm-api:amd64 | gzip > bin/amd64/crm-api-amd64.tar.gz

# Export arm64 image
docker save crm-api:arm64 | gzip > bin/arm64/crm-api-arm64.tar.gz
```

## Usage

To deploy to a remote server, copy the appropriate architecture's tar.gz file:

```bash
# For amd64 servers (most Linux servers)
scp bin/amd64/*.tar.gz user@server:/path/to/deploy/

# For arm64 servers (AWS Graviton, Apple Silicon VMs)
scp bin/arm64/*.tar.gz user@server:/path/to/deploy/
```

## Note

- The `x86` folder is reserved for legacy 32-bit builds (rarely needed)
- The `x86_64` folder is an alias for `amd64` for compatibility with different naming conventions
