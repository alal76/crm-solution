# Scripts Directory

This directory contains all automation scripts for the CRM Solution.

## Structure

```
scripts/
├── build/          # Build automation scripts
│   ├── build.sh    # Main build script (Linux/macOS)
│   └── build.ps1   # Main build script (Windows)
│
├── deploy/         # Deployment scripts
│   ├── deploy.sh           # Kubernetes deployment (Linux/macOS)
│   ├── deploy.ps1          # Kubernetes deployment (Windows)
│   ├── deploy-auto.ps1     # Automated deployment
│   ├── deploy-final.ps1    # Final production deployment
│   ├── deploy-now.ps1      # Quick deployment
│   ├── deploy-remote.ps1   # Remote server deployment
│   ├── quick-deploy.ps1    # Fast local deployment
│   ├── remote-ops.ps1      # Remote operations
│   └── start-containers.sh # Start Docker containers
│
├── database/       # Database scripts
│   ├── insert-admin.sh             # Insert admin user
│   ├── insert-admin-user.sql       # Admin user SQL
│   ├── create_backup_schedules.sql # Backup schedule setup
│   ├── create_service_request_types.sql
│   ├── rebuild-configs.sh          # Rebuild configurations
│   ├── run-tests.sh                # Run database tests
│   ├── seed-modules.sql            # Seed module data
│   ├── validate-tests.sh           # Validate test results
│   └── verify-build.sh             # Verify build
│
└── utils/          # Utility scripts
    ├── check-status.bat        # Check status (Windows)
    ├── frontend_login_test.js  # Frontend login tests
    ├── git-commit.ps1          # Git commit helper
    ├── hash-generator.py       # Password hash generator
    ├── setup-ssh-key.ps1       # SSH key setup
    ├── test-login.ps1          # Test login functionality
    ├── test-server.ps1         # Test server connectivity
    ├── update-version.js       # Update version (Node.js)
    └── update-version.ps1      # Update version (PowerShell)
```

## Usage

### Build Scripts

```bash
# From project root
./scripts/build/build.sh Development Debug true

# Windows
powershell -ExecutionPolicy Bypass -File scripts/build/build.ps1
```

### Deploy Scripts

```bash
# Deploy to Kubernetes
./scripts/deploy/deploy.sh

# Windows
powershell -ExecutionPolicy Bypass -File scripts/deploy/deploy.ps1
```

### Database Scripts

```bash
# Insert admin user
./scripts/database/insert-admin.sh
```

### Utility Scripts

```bash
# Generate password hash
python3 scripts/utils/hash-generator.py
```
