# CRM Solution - Infrastructure & Deployment Tools

This directory contains deployment tools, configuration templates, and infrastructure-as-code for the CRM Solution.

## Directory Structure

```
CRM.Infrastructure/
├── deployment-tool/           # GUI Deployment Tool (Python)
│   ├── main.py               # Entry point
│   ├── gui/                  # GUI components
│   ├── generators/           # Script generators
│   ├── templates/            # Configuration templates
│   └── config/               # Saved configurations
├── docker/                   # Docker configurations
├── kubernetes/               # Kubernetes manifests
└── scripts/                  # Utility scripts
```

## GUI Deployment Tool

A cross-platform GUI application for deploying the CRM Solution.

### Features

- **Hosting Options**: Local development or Cloud (AWS, Azure, GCP)
- **Container Orchestration**: Docker Compose or Kubernetes
- **Database Options**: PaaS (managed) or VM-hosted
- **Script Generation**: Unix (bash) or Windows (PowerShell)
- **Admin Setup**: Configure admin credentials (default: sysadmin/Password@123)
- **Seed Data**: Select data sets to seed
- **OAuth/JWT**: Generate JWT tokens, capture browser auth
- **Progress Tracking**: Real-time deployment progress
- **Smoke Tests**: Automatic verification after deployment

### Requirements

- Python 3.9+
- tkinter (included with Python)
- No additional pip packages required

### Running the Tool

```bash
cd deployment-tool
python main.py
```

### Configuration

Previous deployment choices are saved in `config/deployment_config.json` and can be restored on next run.

## Quick Start

1. Run the deployment tool
2. Select hosting platform and options
3. Configure admin credentials
4. Select seed data options
5. Generate deployment scripts
6. Execute the deployment
7. Verify with smoke tests
8. Access the application via generated links

## Default Credentials

- **Username**: sysadmin
- **Password**: Password@123

**Important**: Change these in production!

## License

AGPL-3.0 - See LICENSE file in the root directory.
