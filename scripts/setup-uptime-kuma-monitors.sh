#!/bin/bash
#
# CRM Solution - Uptime Kuma Monitor Auto-Setup Script
# Automatically configures monitoring for all CRM services
#
# This script uses the Uptime Kuma API to create monitors
# for all CRM endpoints and services.
#

set -e

# Configuration
UPTIME_KUMA_URL="${UPTIME_KUMA_URL:-http://localhost:3001}"
UPTIME_KUMA_USERNAME="${UPTIME_KUMA_USERNAME:-admin}"
UPTIME_KUMA_PASSWORD="${UPTIME_KUMA_PASSWORD:-admin123}"
CRM_API_URL="${CRM_API_URL:-http://crm-api:5000}"
CRM_FRONTEND_URL="${CRM_FRONTEND_URL:-http://crm-frontend:80}"
MARIADB_HOST="${MARIADB_HOST:-crm-mariadb}"
REDIS_HOST="${REDIS_HOST:-crm-redis}"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

log_info() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

log_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

log_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Check if Uptime Kuma is accessible
check_uptime_kuma() {
    log_info "Checking Uptime Kuma availability at ${UPTIME_KUMA_URL}..."
    
    for i in {1..30}; do
        if curl -s -o /dev/null -w "%{http_code}" "${UPTIME_KUMA_URL}" | grep -q "200\|302"; then
            log_success "Uptime Kuma is accessible"
            return 0
        fi
        log_info "Waiting for Uptime Kuma... (attempt $i/30)"
        sleep 2
    done
    
    log_error "Uptime Kuma is not accessible at ${UPTIME_KUMA_URL}"
    return 1
}

# Create monitors using the Uptime Kuma API
# Note: Uptime Kuma primarily uses Socket.IO, but we can use the push monitor feature
# or set up monitors via the database directly for automation

setup_monitors_via_api() {
    log_info "Setting up monitors via Uptime Kuma API..."
    
    # Uptime Kuma uses Socket.IO for its API, which is complex to script
    # For automation, we'll create a Node.js script that handles this properly
    
    cat > /tmp/setup-monitors.js << 'NODEJS_SCRIPT'
const io = require('socket.io-client');

const UPTIME_KUMA_URL = process.env.UPTIME_KUMA_URL || 'http://localhost:3001';
const USERNAME = process.env.UPTIME_KUMA_USERNAME || 'admin';
const PASSWORD = process.env.UPTIME_KUMA_PASSWORD || 'admin123';
const CRM_API_URL = process.env.CRM_API_URL || 'http://crm-api:5000';
const CRM_FRONTEND_URL = process.env.CRM_FRONTEND_URL || 'http://crm-frontend:80';

// Define all CRM monitors
const monitors = [
    {
        name: 'CRM API - Health',
        type: 'http',
        url: `${CRM_API_URL}/api/monitoring/health`,
        method: 'GET',
        interval: 60,
        retryInterval: 60,
        maxretries: 3,
        accepted_statuscodes: ['200-299'],
        description: 'Main CRM API health endpoint'
    },
    {
        name: 'CRM API - Ready',
        type: 'http',
        url: `${CRM_API_URL}/api/monitoring/health/ready`,
        method: 'GET',
        interval: 60,
        retryInterval: 60,
        maxretries: 3,
        accepted_statuscodes: ['200-299'],
        description: 'CRM API readiness check (includes database)'
    },
    {
        name: 'CRM API - Alive',
        type: 'http',
        url: `${CRM_API_URL}/api/monitoring/health/live`,
        method: 'GET',
        interval: 30,
        retryInterval: 30,
        maxretries: 3,
        accepted_statuscodes: ['200-299'],
        description: 'CRM API liveness check'
    },
    {
        name: 'CRM Frontend',
        type: 'http',
        url: CRM_FRONTEND_URL,
        method: 'GET',
        interval: 60,
        retryInterval: 60,
        maxretries: 3,
        accepted_statuscodes: ['200-299'],
        description: 'CRM React frontend'
    },
    {
        name: 'CRM API - Swagger',
        type: 'http',
        url: `${CRM_API_URL}/swagger/index.html`,
        method: 'GET',
        interval: 300,
        retryInterval: 60,
        maxretries: 2,
        accepted_statuscodes: ['200-299'],
        description: 'API documentation endpoint'
    },
    {
        name: 'CRM API - Environment',
        type: 'http',
        url: `${CRM_API_URL}/api/monitoring/environment`,
        method: 'GET',
        interval: 120,
        retryInterval: 60,
        maxretries: 3,
        accepted_statuscodes: ['200-299'],
        description: 'Environment info endpoint'
    },
    {
        name: 'MariaDB Database',
        type: 'port',
        hostname: process.env.MARIADB_HOST || 'crm-mariadb',
        port: 3306,
        interval: 60,
        retryInterval: 60,
        maxretries: 3,
        description: 'MariaDB database port check'
    },
    {
        name: 'Redis Cache',
        type: 'port',
        hostname: process.env.REDIS_HOST || 'crm-redis',
        port: 6379,
        interval: 60,
        retryInterval: 60,
        maxretries: 3,
        description: 'Redis cache port check'
    }
];

// Status page configuration
const statusPage = {
    slug: 'crm',
    title: 'CRM System Status',
    description: 'Real-time status of all CRM services',
    icon: '/icon.svg',
    theme: 'auto',
    published: true,
    showTags: true,
    domainNameList: [],
    googleAnalyticsId: '',
    customCSS: '',
    footerText: 'CRM Solution Monitoring',
    showPoweredBy: true
};

console.log('Connecting to Uptime Kuma at:', UPTIME_KUMA_URL);

const socket = io(UPTIME_KUMA_URL, {
    transports: ['websocket'],
    reconnection: true,
    reconnectionAttempts: 5,
    reconnectionDelay: 1000
});

let isLoggedIn = false;
let existingMonitors = [];

socket.on('connect', () => {
    console.log('Connected to Uptime Kuma');
    
    // Try to login
    socket.emit('login', {
        username: USERNAME,
        password: PASSWORD,
        token: ''
    }, async (response) => {
        if (response.ok) {
            console.log('Login successful');
            isLoggedIn = true;
            
            // Get existing monitors
            socket.emit('getMonitorList', (data) => {
                existingMonitors = Object.values(data || {});
                console.log(`Found ${existingMonitors.length} existing monitors`);
                
                // Create monitors
                createMonitors();
            });
        } else {
            console.log('Login failed, trying to setup first user...');
            
            // First time setup - create admin user
            socket.emit('setup', USERNAME, PASSWORD, (setupResponse) => {
                if (setupResponse.ok) {
                    console.log('First-time setup completed');
                    isLoggedIn = true;
                    createMonitors();
                } else {
                    console.error('Setup failed:', setupResponse.msg);
                    process.exit(1);
                }
            });
        }
    });
});

async function createMonitors() {
    console.log('\nCreating CRM monitors...\n');
    
    let created = 0;
    let skipped = 0;
    
    for (const monitor of monitors) {
        // Check if monitor already exists
        const exists = existingMonitors.some(m => m.name === monitor.name);
        
        if (exists) {
            console.log(`⏭️  Skipping "${monitor.name}" (already exists)`);
            skipped++;
            continue;
        }
        
        try {
            await new Promise((resolve, reject) => {
                socket.emit('add', monitor, (response) => {
                    if (response.ok) {
                        console.log(`✅ Created monitor: ${monitor.name}`);
                        created++;
                        resolve(response);
                    } else {
                        console.log(`❌ Failed to create "${monitor.name}": ${response.msg}`);
                        reject(new Error(response.msg));
                    }
                });
            });
            
            // Small delay between monitor creations
            await new Promise(r => setTimeout(r, 500));
        } catch (error) {
            console.error(`Error creating monitor ${monitor.name}:`, error.message);
        }
    }
    
    console.log(`\n📊 Summary: Created ${created} monitors, Skipped ${skipped} existing`);
    
    // Create status page
    await createStatusPage();
    
    console.log('\n✨ Uptime Kuma setup complete!');
    process.exit(0);
}

async function createStatusPage() {
    console.log('\nSetting up status page...');
    
    return new Promise((resolve) => {
        // Get all monitors to add to status page
        socket.emit('getMonitorList', (data) => {
            const monitorList = Object.values(data || {});
            
            // Create public group with all monitors
            const publicGroupList = [{
                name: 'CRM Services',
                weight: 1,
                monitorList: monitorList.map(m => ({
                    id: m.id,
                    name: m.name
                }))
            }];
            
            const statusPageData = {
                ...statusPage,
                publicGroupList
            };
            
            socket.emit('saveStatusPage', statusPage.slug, statusPageData, null, (response) => {
                if (response.ok) {
                    console.log('✅ Status page created/updated: /status/' + statusPage.slug);
                } else {
                    // Status page might already exist, try to update
                    console.log('Status page creation response:', response.msg || 'OK');
                }
                resolve();
            });
        });
    });
}

socket.on('connect_error', (error) => {
    console.error('Connection error:', error.message);
    process.exit(1);
});

socket.on('disconnect', () => {
    console.log('Disconnected from Uptime Kuma');
});

// Timeout after 60 seconds
setTimeout(() => {
    console.error('Timeout: Setup took too long');
    process.exit(1);
}, 60000);
NODEJS_SCRIPT

    # Check if Node.js is available
    if command -v node &> /dev/null; then
        log_info "Running monitor setup script..."
        
        # Check if socket.io-client is installed
        if ! npm list socket.io-client &> /dev/null 2>&1; then
            log_info "Installing socket.io-client..."
            npm install socket.io-client --no-save 2>/dev/null || true
        fi
        
        # Run the setup script
        UPTIME_KUMA_URL="${UPTIME_KUMA_URL}" \
        UPTIME_KUMA_USERNAME="${UPTIME_KUMA_USERNAME}" \
        UPTIME_KUMA_PASSWORD="${UPTIME_KUMA_PASSWORD}" \
        CRM_API_URL="${CRM_API_URL}" \
        CRM_FRONTEND_URL="${CRM_FRONTEND_URL}" \
        MARIADB_HOST="${MARIADB_HOST}" \
        REDIS_HOST="${REDIS_HOST}" \
        node /tmp/setup-monitors.js
    else
        log_warning "Node.js not available. Using Docker to run setup..."
        setup_via_docker
    fi
}

# Alternative: Setup via Docker container
setup_via_docker() {
    log_info "Running monitor setup via Docker..."
    
    # Create a temporary directory for the setup
    SETUP_DIR=$(mktemp -d)
    
    cat > "${SETUP_DIR}/package.json" << 'EOF'
{
  "name": "uptime-kuma-setup",
  "version": "1.0.0",
  "main": "setup.js",
  "dependencies": {
    "socket.io-client": "^4.7.0"
  }
}
EOF

    # Copy the setup script
    cp /tmp/setup-monitors.js "${SETUP_DIR}/setup.js" 2>/dev/null || true
    
    # Run in Docker
    docker run --rm \
        --network crm-network \
        -v "${SETUP_DIR}:/app" \
        -w /app \
        -e UPTIME_KUMA_URL="http://uptime-kuma:3001" \
        -e UPTIME_KUMA_USERNAME="${UPTIME_KUMA_USERNAME}" \
        -e UPTIME_KUMA_PASSWORD="${UPTIME_KUMA_PASSWORD}" \
        -e CRM_API_URL="${CRM_API_URL}" \
        -e CRM_FRONTEND_URL="${CRM_FRONTEND_URL}" \
        -e MARIADB_HOST="${MARIADB_HOST}" \
        -e REDIS_HOST="${REDIS_HOST}" \
        node:18-alpine sh -c "npm install && node setup.js"
    
    # Cleanup
    rm -rf "${SETUP_DIR}"
}

# Main function
main() {
    echo ""
    echo "╔═══════════════════════════════════════════════════════════╗"
    echo "║     CRM Solution - Uptime Kuma Monitor Auto-Setup         ║"
    echo "╚═══════════════════════════════════════════════════════════╝"
    echo ""
    
    log_info "Configuration:"
    echo "  • Uptime Kuma URL: ${UPTIME_KUMA_URL}"
    echo "  • CRM API URL: ${CRM_API_URL}"
    echo "  • CRM Frontend URL: ${CRM_FRONTEND_URL}"
    echo ""
    
    # Check if Uptime Kuma is accessible
    check_uptime_kuma || exit 1
    
    # Setup monitors
    setup_monitors_via_api
    
    log_success "Monitor setup completed!"
    echo ""
    echo "Access your status page at: ${UPTIME_KUMA_URL}/status/crm"
    echo ""
}

# Run main function
main "$@"
