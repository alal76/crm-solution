/**
 * CRM Solution - Uptime Kuma Monitor Auto-Setup
 * 
 * This script automatically configures Uptime Kuma with monitors
 * for all CRM services and creates a public status page.
 * 
 * Usage:
 *   node setup-uptime-kuma.js
 * 
 * Environment Variables:
 *   UPTIME_KUMA_URL      - Uptime Kuma URL (default: http://localhost:3001)
 *   UPTIME_KUMA_USERNAME - Admin username (default: admin)
 *   UPTIME_KUMA_PASSWORD - Admin password (default: admin123)
 *   CRM_API_URL          - CRM API base URL (default: http://crm-api:5000)
 *   CRM_FRONTEND_URL     - CRM Frontend URL (default: http://crm-frontend:80)
 */

const io = require('socket.io-client');

// Configuration from environment
const config = {
    uptimeKumaUrl: process.env.UPTIME_KUMA_URL || 'http://localhost:3001',
    username: process.env.UPTIME_KUMA_USERNAME || 'admin',
    password: process.env.UPTIME_KUMA_PASSWORD || 'admin123',
    crmApiUrl: process.env.CRM_API_URL || 'http://crm-api:5000',
    crmFrontendUrl: process.env.CRM_FRONTEND_URL || 'http://crm-frontend:80',
    mariadbHost: process.env.MARIADB_HOST || 'crm-mariadb',
    redisHost: process.env.REDIS_HOST || 'crm-redis',
    externalApiUrl: process.env.EXTERNAL_API_URL || '',
    externalFrontendUrl: process.env.EXTERNAL_FRONTEND_URL || ''
};

// Define all CRM monitors
const getMonitors = () => [
    // Internal monitors (Docker network)
    {
        name: 'CRM API - Health',
        type: 'http',
        url: `${config.crmApiUrl}/api/monitoring/health`,
        method: 'GET',
        interval: 60,
        retryInterval: 60,
        maxretries: 3,
        accepted_statuscodes: ['200-299'],
        notificationIDList: [],
        ignoreTls: false,
        upsideDown: false,
        maxredirects: 10,
        resendInterval: 0,
        description: 'Main CRM API health endpoint - checks basic service status'
    },
    {
        name: 'CRM API - Ready Check',
        type: 'http',
        url: `${config.crmApiUrl}/api/monitoring/health/ready`,
        method: 'GET',
        interval: 60,
        retryInterval: 60,
        maxretries: 3,
        accepted_statuscodes: ['200-299'],
        notificationIDList: [],
        ignoreTls: false,
        upsideDown: false,
        maxredirects: 10,
        description: 'CRM API readiness - includes database connectivity check'
    },
    {
        name: 'CRM API - Liveness',
        type: 'http',
        url: `${config.crmApiUrl}/api/monitoring/health/live`,
        method: 'GET',
        interval: 30,
        retryInterval: 30,
        maxretries: 3,
        accepted_statuscodes: ['200-299'],
        notificationIDList: [],
        description: 'CRM API liveness probe - quick health check'
    },
    {
        name: 'CRM Frontend',
        type: 'http',
        url: config.crmFrontendUrl,
        method: 'GET',
        interval: 60,
        retryInterval: 60,
        maxretries: 3,
        accepted_statuscodes: ['200-299'],
        notificationIDList: [],
        description: 'CRM React frontend application'
    },
    {
        name: 'CRM API - Swagger Docs',
        type: 'http',
        url: `${config.crmApiUrl}/swagger/index.html`,
        method: 'GET',
        interval: 300,
        retryInterval: 120,
        maxretries: 2,
        accepted_statuscodes: ['200-299'],
        notificationIDList: [],
        description: 'API documentation endpoint'
    },
    {
        name: 'CRM API - Version',
        type: 'http',
        url: `${config.crmApiUrl}/api/monitoring/environment`,
        method: 'GET',
        interval: 300,
        retryInterval: 120,
        maxretries: 2,
        accepted_statuscodes: ['200-299'],
        notificationIDList: [],
        description: 'Environment and version information'
    },
    {
        name: 'MariaDB Database',
        type: 'port',
        hostname: config.mariadbHost,
        port: 3306,
        interval: 60,
        retryInterval: 60,
        maxretries: 3,
        notificationIDList: [],
        active: true,
        portAuthority: '',
        description: 'MariaDB database server TCP check'
    },
    {
        name: 'Redis Cache',
        type: 'port',
        hostname: config.redisHost,
        port: 6379,
        interval: 60,
        retryInterval: 60,
        maxretries: 3,
        notificationIDList: [],
        active: true,
        portAuthority: '',
        description: 'Redis cache server TCP check'
    },
    // Authentication endpoints
    {
        name: 'CRM API - Auth Endpoint',
        type: 'http',
        url: `${config.crmApiUrl}/api/auth/status`,
        method: 'GET',
        interval: 120,
        retryInterval: 60,
        maxretries: 3,
        accepted_statuscodes: ['200-299', '401'],
        notificationIDList: [],
        description: 'Authentication service availability'
    },
    // Customer API
    {
        name: 'CRM API - Customers',
        type: 'http',
        url: `${config.crmApiUrl}/api/customers`,
        method: 'GET',
        interval: 300,
        retryInterval: 120,
        maxretries: 2,
        accepted_statuscodes: ['200-299', '401'],
        notificationIDList: [],
        description: 'Customer API endpoint availability'
    }
];

// External monitors (if external URLs are provided)
const getExternalMonitors = () => {
    const monitors = [];
    
    if (config.externalApiUrl) {
        monitors.push({
            name: 'CRM API (External)',
            type: 'http',
            url: `${config.externalApiUrl}/api/monitoring/health`,
            method: 'GET',
            interval: 60,
            retryInterval: 60,
            maxretries: 3,
            accepted_statuscodes: ['200-299'],
            notificationIDList: [],
            description: 'CRM API health - external access'
        });
    }
    
    if (config.externalFrontendUrl) {
        monitors.push({
            name: 'CRM Frontend (External)',
            type: 'http',
            url: config.externalFrontendUrl,
            method: 'GET',
            interval: 60,
            retryInterval: 60,
            maxretries: 3,
            accepted_statuscodes: ['200-299'],
            notificationIDList: [],
            description: 'CRM Frontend - external access'
        });
    }
    
    return monitors;
};

// Status page configuration
const statusPageConfig = {
    slug: 'crm',
    title: 'CRM System Status',
    description: 'Real-time status of all CRM Solution services and infrastructure',
    icon: '/icon.svg',
    theme: 'auto',
    published: true,
    showTags: true,
    showCertificateExpiry: false,
    domainNameList: [],
    googleAnalyticsId: '',
    customCSS: `
        .badge { border-radius: 4px; }
        .overall-status { font-size: 1.5rem; }
    `,
    footerText: 'CRM Solution © 2024-2026 | Powered by Uptime Kuma',
    showPoweredBy: true
};

class UptimeKumaSetup {
    constructor() {
        this.socket = null;
        this.isLoggedIn = false;
        this.existingMonitors = [];
        this.createdMonitorIds = [];
    }

    async connect() {
        return new Promise((resolve, reject) => {
            console.log(`\n📡 Connecting to Uptime Kuma at ${config.uptimeKumaUrl}...`);
            
            this.socket = io(config.uptimeKumaUrl, {
                transports: ['websocket'],
                reconnection: true,
                reconnectionAttempts: 5,
                reconnectionDelay: 1000,
                timeout: 10000
            });

            this.socket.on('connect', () => {
                console.log('✅ Connected to Uptime Kuma\n');
                resolve();
            });

            this.socket.on('connect_error', (error) => {
                console.error('❌ Connection error:', error.message);
                reject(error);
            });

            // Timeout after 30 seconds
            setTimeout(() => {
                reject(new Error('Connection timeout'));
            }, 30000);
        });
    }

    async login() {
        return new Promise((resolve, reject) => {
            console.log(`🔐 Logging in as ${config.username}...`);
            
            this.socket.emit('login', {
                username: config.username,
                password: config.password,
                token: ''
            }, (response) => {
                if (response.ok) {
                    console.log('✅ Login successful\n');
                    this.isLoggedIn = true;
                    resolve();
                } else {
                    // Try first-time setup
                    console.log('⚙️  First-time setup - creating admin user...');
                    this.socket.emit('setup', config.username, config.password, (setupResponse) => {
                        if (setupResponse.ok) {
                            console.log('✅ First-time setup completed\n');
                            this.isLoggedIn = true;
                            resolve();
                        } else {
                            reject(new Error(`Login/Setup failed: ${setupResponse.msg}`));
                        }
                    });
                }
            });
        });
    }

    async getExistingMonitors() {
        return new Promise((resolve) => {
            this.socket.emit('getMonitorList', (data) => {
                this.existingMonitors = Object.values(data || {});
                console.log(`📋 Found ${this.existingMonitors.length} existing monitors\n`);
                resolve();
            });
        });
    }

    async createMonitor(monitor) {
        return new Promise((resolve) => {
            // Check if monitor already exists
            const existing = this.existingMonitors.find(m => m.name === monitor.name);
            if (existing) {
                console.log(`  ⏭️  "${monitor.name}" already exists (ID: ${existing.id})`);
                this.createdMonitorIds.push(existing.id);
                resolve({ skipped: true, id: existing.id });
                return;
            }

            this.socket.emit('add', monitor, (response) => {
                if (response.ok) {
                    console.log(`  ✅ Created: ${monitor.name} (ID: ${response.monitorID})`);
                    this.createdMonitorIds.push(response.monitorID);
                    resolve({ created: true, id: response.monitorID });
                } else {
                    console.log(`  ❌ Failed: ${monitor.name} - ${response.msg}`);
                    resolve({ failed: true, error: response.msg });
                }
            });
        });
    }

    async createAllMonitors() {
        console.log('📊 Creating CRM monitors...\n');
        
        const allMonitors = [...getMonitors(), ...getExternalMonitors()];
        let created = 0, skipped = 0, failed = 0;

        for (const monitor of allMonitors) {
            const result = await this.createMonitor(monitor);
            if (result.created) created++;
            else if (result.skipped) skipped++;
            else if (result.failed) failed++;
            
            // Small delay between creations
            await new Promise(r => setTimeout(r, 300));
        }

        console.log(`\n📈 Monitor Summary:`);
        console.log(`   Created: ${created}`);
        console.log(`   Skipped: ${skipped} (already exist)`);
        console.log(`   Failed: ${failed}\n`);
        
        return { created, skipped, failed };
    }

    async createStatusPage() {
        return new Promise((resolve) => {
            console.log('📄 Creating status page...\n');

            // Use the monitor IDs we collected during creation
            if (this.createdMonitorIds.length === 0) {
                console.log('  ⚠️  No monitor IDs available, skipping status page creation');
                resolve();
                return;
            }
            
            // Get fresh monitor list with a slight delay to ensure monitors are registered
            setTimeout(() => {
                this.socket.emit('getMonitorList', (data) => {
                    // Debug: log what we received
                    console.log(`  ℹ️  Received ${Object.keys(data || {}).length} monitors from API`);
                    
                    const monitorList = [];
                    
                    // Handle both array and object formats
                    if (Array.isArray(data)) {
                        monitorList.push(...data.filter(m => m && m.name));
                    } else if (data && typeof data === 'object') {
                        for (const key of Object.keys(data)) {
                            const monitor = data[key];
                            if (monitor && monitor.name) {
                                monitorList.push(monitor);
                            }
                        }
                    }
                    
                    console.log(`  ℹ️  Filtered to ${monitorList.length} valid monitors`);
                    
                    if (monitorList.length === 0) {
                        // Fallback: create simple group with our known IDs
                        console.log('  ℹ️  Using fallback with collected monitor IDs');
                        const publicGroupList = [{
                            name: 'CRM Services',
                            weight: 1,
                            monitorList: this.createdMonitorIds.map(id => ({ id }))
                        }];
                        
                        this.saveStatusPage(publicGroupList, resolve);
                        return;
                    }
                    
                    // Group monitors by type
                    const apiMonitors = monitorList.filter(m => m.name && m.name.includes('API'));
                    const infraMonitors = monitorList.filter(m => 
                        m.name && (m.name.includes('Database') || 
                        m.name.includes('Redis') || 
                        m.name.includes('Cache'))
                    );
                    const frontendMonitors = monitorList.filter(m => m.name && m.name.includes('Frontend'));
                    const otherMonitors = monitorList.filter(m => 
                        m.name &&
                        !apiMonitors.includes(m) && 
                        !infraMonitors.includes(m) && 
                        !frontendMonitors.includes(m)
                    );

                    // Create public groups
                    const publicGroupList = [];
                    
                    if (apiMonitors.length > 0) {
                        publicGroupList.push({
                            name: '🔌 API Services',
                            weight: 1,
                            monitorList: apiMonitors.map(m => ({ id: m.id, name: m.name }))
                        });
                    }
                    
                    if (frontendMonitors.length > 0) {
                        publicGroupList.push({
                            name: '🖥️ Frontend',
                            weight: 2,
                            monitorList: frontendMonitors.map(m => ({ id: m.id, name: m.name }))
                        });
                    }
                    
                    if (infraMonitors.length > 0) {
                        publicGroupList.push({
                            name: '🏗️ Infrastructure',
                            weight: 3,
                            monitorList: infraMonitors.map(m => ({ id: m.id, name: m.name }))
                        });
                    }
                    
                    if (otherMonitors.length > 0) {
                        publicGroupList.push({
                            name: '📦 Other Services',
                            weight: 4,
                            monitorList: otherMonitors.map(m => ({ id: m.id, name: m.name }))
                        });
                    }

                    this.saveStatusPage(publicGroupList, resolve);
                });
            }, 1000);
        });
    }

    saveStatusPage(publicGroupList, resolve) {
        const statusPage = {
            ...statusPageConfig,
            publicGroupList
        };

        // Try to add the status page first
        this.socket.emit('addStatusPage', statusPageConfig.title, statusPageConfig.slug, (addResponse) => {
            // Now save the status page configuration
            this.socket.emit('saveStatusPage', statusPageConfig.slug, statusPage, null, (response) => {
                if (response.ok) {
                    console.log(`  ✅ Status page created: /status/${statusPageConfig.slug}`);
                } else {
                    console.log(`  ⚠️  Status page: ${response.msg || 'May already exist'}`);
                }
                resolve();
            });
        });
    }

    async disconnect() {
        if (this.socket) {
            this.socket.disconnect();
            console.log('\n🔌 Disconnected from Uptime Kuma');
        }
    }

    async run() {
        console.log('╔═══════════════════════════════════════════════════════════╗');
        console.log('║     CRM Solution - Uptime Kuma Monitor Auto-Setup         ║');
        console.log('╚═══════════════════════════════════════════════════════════╝');
        console.log('\nConfiguration:');
        console.log(`  • Uptime Kuma: ${config.uptimeKumaUrl}`);
        console.log(`  • CRM API: ${config.crmApiUrl}`);
        console.log(`  • CRM Frontend: ${config.crmFrontendUrl}`);
        
        try {
            await this.connect();
            await this.login();
            await this.getExistingMonitors();
            await this.createAllMonitors();
            await this.createStatusPage();
            
            console.log('\n✨ Setup Complete!');
            console.log(`\n🔗 Access your status page at:`);
            console.log(`   ${config.uptimeKumaUrl}/status/${statusPageConfig.slug}\n`);
            
            await this.disconnect();
            process.exit(0);
        } catch (error) {
            console.error('\n❌ Setup failed:', error.message);
            await this.disconnect();
            process.exit(1);
        }
    }
}

// Run setup
const setup = new UptimeKumaSetup();
setup.run();
