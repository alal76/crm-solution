# CRM Solution Deployment Tool v2.0.0

A comprehensive GUI and CLI deployment tool for the CRM Solution that provides complete control over architecture, components, network configuration, and deployment automation.

## Features

### 🏗️ Architecture Selection
- **Monolithic**: Single API container with all services
- **Microservices**: Separate containers for Identity, Customer, Sales, Marketing, and ServiceDesk services

### 🖥️ Build Server Configuration
- **Local Machine**: Deploy directly from your development machine
- **Remote Server (SSH)**: Deploy to remote Linux servers via SSH
- Prerequisite checking (Docker, Docker Compose, Node.js, .NET)
- SSH key authentication support

### 📦 Component Selection
Deploy any combination of components:
- Database (MariaDB, MySQL, PostgreSQL, SQL Server)
- Redis Cache
- API Server(s)
- Frontend (React)
- Monitoring Stack (optional)

### 🌐 Network Configuration
- Custom ports for API, Frontend, Redis
- Domain/hostname configuration
- SSL/TLS support
- Automatic network analysis and issue detection
- Docker network auto-configuration

### 🔐 Credentials Management
- Admin user setup (username, email, password)
- JWT secret generation
- CORS configuration

### 🌱 Data Seeding
- Master data (required)
- Demo data (optional)
- US Zip Codes database (optional)

### ✅ Testing
- Smoke tests (health checks)
- BVT tests
- Test result export

## Quick Start

### GUI Mode

```bash
# From the project root
./CRM.Infrastructure/run-deployment-tool.sh
```

### CLI Mode

```bash
# Generate deployment scripts
python main.py --generate --domain 192.168.0.9 --api-port 5000

# Generate and deploy
python main.py --deploy --domain myserver.local

# Use custom config
python main.py --config my-config.json --generate
```

## CLI Options

| Option | Default | Description |
|--------|---------|-------------|
| `--generate` | - | Generate deployment scripts |
| `--deploy` | - | Run full deployment |
| `--config` | - | Path to config JSON file |
| `--output` | `./generated` | Output directory for scripts |
| `--domain` | `localhost` | Domain/hostname |
| `--api-port` | `5000` | API port |
| `--frontend-port` | `80` | Frontend port |
| `--db-host` | `crm-mariadb` | Database host |
| `--db-password` | - | Database password |
| `--admin-email` | `admin@crm.local` | Admin email |
| `--admin-password` | `Admin@123` | Admin password |

## Generated Files

The tool generates the following files in the output directory:

### `.env`
Environment configuration file containing:
- Database connection settings
- API configuration
- JWT secrets
- CORS settings
- Admin credentials

### `docker-compose.yml`
Docker Compose file with:
- Service definitions for all components
- Health checks
- Network configuration
- Volume mounts

## Naming Conventions

The tool follows these naming conventions:

| Component | Container Name |
|-----------|----------------|
| Database | `crm-mariadb` |
| Redis | `crm-redis` |
| API | `crm-api` |
| Frontend | `crm-frontend` |
| Network | `crm-network` |

## Requirements

### For GUI Mode
- Python 3.9+
- tkinter (usually bundled with Python)
- macOS: May require Homebrew Python for tkinter compatibility

### For CLI Mode
- Python 3.9+

### For Deployment
- Docker and Docker Compose (on target machine)
- SSH access (for remote deployments)

## Troubleshooting

### tkinter Not Working on macOS

If you see "macOS 26 (2602) or later required" error:

```bash
# Install Python via Homebrew
brew install python-tk@3.11
brew link python@3.11

# Or use CLI mode
python main.py --generate
```

### Connection to Remote Server Failed

1. Check SSH port is accessible: `nc -zv hostname 22`
2. Verify SSH key permissions: `chmod 600 ~/.ssh/id_rsa`
3. Test SSH connection: `ssh user@hostname`

### Docker Network Issues

Run the Network Analysis from the GUI, or manually:

```bash
docker network create crm-network
docker network connect crm-network crm-mariadb
docker network connect crm-network crm-api
docker network connect crm-network crm-frontend
```

## Version History

### v2.0.0
- Complete rewrite with modular architecture
- Added CLI mode
- Build server configuration
- Network analyzer
- Prerequisite checker
- Real-time deployment logs
- Test results with export

### v0.0.24
- Initial tkinter GUI
- Basic deployment configuration

## License

AGPL-3.0
