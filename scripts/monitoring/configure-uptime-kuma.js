#!/usr/bin/env node
/**
 * CRM Solution - Uptime Kuma Monitor Configuration
 * 
 * This script automatically configures Uptime Kuma with monitors
 * for all CRM services using the Socket.IO API.
 * 
 * Usage: node configure-uptime-kuma.js [--host HOST] [--port PORT]
 */

const { io } = require('socket.io-client');

// Configuration
const config = {
    uptimeKumaHost: process.env.UPTIME_KUMA_HOST || 'localhost',
    uptimeKumaPort: process.env.UPTIME_KUMA_PORT || '3001',
    username: process.env.UPTIME_KUMA_USER || 'admin',
    password: process.env.UPTIME_KUMA_PASS || 'CrmAdmin2024!',
    crmApiHost: process.env.CRM_API_HOST || 'crm-api',
    crmApiPort: process.env.CRM_API_PORT || '5000',
    crmFrontendHost: process.env.CRM_FRONTEND_HOST || 'crm-frontend',
    crmFrontendPort: process.env.CRM_FRONTEND_PORT || '80',
    mariadbHost: process.env.MARIADB_HOST || 'crm-mariadb',
    mariadbPort: process.env.MARIADB_PORT || '3306',
    redisHost: process.env.REDIS_HOST || 'crm-redis',
    redisPort: process.env.REDIS_PORT || '6379',
};

// Parse command line arguments
process.argv.forEach((arg, i) => {
    if (arg === '--host' && process.argv[i + 1]) {
        config.uptimeKumaHost = process.argv[i + 1];
    }
    if (arg === '--port' && process.argv[i + 1]) {
        config.uptimeKumaPort = process.argv[i + 1];
    }
});

const UPTIME_KUMA_URL = `http://${config.uptimeKumaHost}:${config.uptimeKumaPort}`;

// Monitor definitions for CRM services
const monitors = [
    // HTTP monitors
    {
        type: 'http',
        name: 'CRM API - Health',
        url: `http://${config.crmApiHost}:${config.crmApiPort}/api/monitoring/health`,
        method: 'GET',
        interval: 60,
        retryInterval: 30,
        maxretries: 3,
        accepted_statuscodes: ['200-299'],
        description: 'CRM API main health check endpoint',
    },
    {
        type: 'http',
        name: 'CRM API - Ready',
        url: `http://${config.crmApiHost}:${config.crmApiPort}/api/monitoring/health/ready`,
        method: 'GET',
        interval: 60,
        retryInterval: 30,
        maxretries: 3,
        accepted_statuscodes: ['200-299'],
        description: 'CRM API readiness check (includes database)',
    },
    {
        type: 'http',
        name: 'CRM API - Live',
        url: `http://${config.crmApiHost}:${config.crmApiPort}/api/monitoring/health/live`,
        method: 'GET',
        interval: 30,
        retryInterval: 15,
        maxretries: 3,
        accepted_statuscodes: ['200-299'],
        description: 'CRM API liveness check',
    },
    {
        type: 'http',
        name: 'CRM Frontend',
        url: `http://${config.crmFrontendHost}:${config.crmFrontendPort}/`,
        method: 'GET',
        interval: 60,
        retryInterval: 30,
        maxretries: 3,
        accepted_statuscodes: ['200-299'],
        description: 'CRM Frontend React application',
    },
    {
        type: 'http',
        name: 'CRM API - Swagger',
        url: `http://${config.crmApiHost}:${config.crmApiPort}/swagger/index.html`,
        method: 'GET',
        interval: 300,
        retryInterval: 60,
        maxretries: 2,
        accepted_statuscodes: ['200-299'],
        description: 'CRM API Swagger documentation',
    },
    {
        type: 'http',
        name: 'CRM API - Environment',
        url: `http://${config.crmApiHost}:${config.crmApiPort}/api/monitoring/environment`,
        method: 'GET',
        interval: 300,
        retryInterval: 60,
        maxretries: 2,
        accepted_statuscodes: ['200-299'],
        description: 'CRM API environment info endpoint',
    },
    // TCP/Port monitors for infrastructure
    {
        type: 'port',
        name: 'MariaDB Database',
        hostname: config.mariadbHost,
        port: parseInt(config.mariadbPort),
        interval: 60,
        retryInterval: 30,
        maxretries: 3,
        accepted_statuscodes: ['200-299'],
        description: 'MariaDB database server',
    },
    {
        type: 'port',
        name: 'Redis Cache',
        hostname: config.redisHost,
        port: parseInt(config.redisPort),
        interval: 60,
        retryInterval: 30,
        maxretries: 3,
        accepted_statuscodes: ['200-299'],
        description: 'Redis cache server',
    },
];

// Status page configuration
const statusPageConfig = {
    slug: 'crm-status',
    title: 'CRM Solution Status',
    description: 'Real-time status of CRM Solution services',
    theme: 'auto',
    published: true,
    showTags: true,
    customCSS: '',
    footerText: 'CRM Solution Monitoring',
    showPoweredBy: false,
};

class UptimeKumaConfigurator {
    constructor() {
        this.socket = null;
        this.token = null;
        this.createdMonitorIds = [];
        this.globalTimeout = null;
    }

    startGlobalTimeout() {
        // Force exit after 60 seconds max
        this.globalTimeout = setTimeout(() => {
            console.error('\n⏱ Global timeout reached (60s). Forcing exit...');
            process.exit(1);
        }, 60000);
    }

    async connect() {
        return new Promise((resolve, reject) => {
            console.log(`\n🔌 Connecting to Uptime Kuma at ${UPTIME_KUMA_URL}...`);
            
            this.socket = io(UPTIME_KUMA_URL, {
                transports: ['websocket', 'polling'],
                reconnection: false,
                timeout: 5000,
                forceNew: true,
            });

            const connectTimeout = setTimeout(() => {
                this.socket.disconnect();
                reject(new Error('Connection timeout (5s) - is Uptime Kuma running?'));
            }, 5000);

            this.socket.on('connect', () => {
                clearTimeout(connectTimeout);
                console.log('✓ Connected to Uptime Kuma');
                resolve();
            });

            this.socket.on('connect_error', (error) => {
                clearTimeout(connectTimeout);
                this.socket.disconnect();
                reject(new Error(`Connection error: ${error.message}`));
            });
        });
    }

    async login() {
        return new Promise((resolve, reject) => {
            console.log(`🔐 Logging in as ${config.username}...`);
            
            const loginTimeout = setTimeout(() => {
                reject(new Error('Login timeout (5s)'));
            }, 5000);
            
            this.socket.emit('login', {
                username: config.username,
                password: config.password,
                token: '',
            }, (response) => {
                clearTimeout(loginTimeout);
                if (response.ok) {
                    this.token = response.token;
                    console.log('✓ Login successful');
                    resolve(response);
                } else {
                    reject(new Error(`Login failed: ${response.msg || 'Unknown error'}`));
                }
            });
        });
    }

    async getExistingMonitors() {
        return new Promise((resolve) => {
            const timeout = setTimeout(() => {
                console.log('  ⚠ No monitor list response, assuming empty');
                resolve({});
            }, 3000);
            
            this.socket.emit('getMonitorList', (data) => {
                clearTimeout(timeout);
                resolve(data || {});
            });
        });
    }

    async addMonitor(monitorConfig) {
        return new Promise((resolve, reject) => {
            const timeout = setTimeout(() => {
                console.log(`  ⚠ Timeout: ${monitorConfig.name}`);
                resolve({ ok: false, msg: 'timeout' });
            }, 5000);

            this.socket.emit('add', monitorConfig, (response) => {
                clearTimeout(timeout);
                if (response.ok) {
                    console.log(`  ✓ Created: ${monitorConfig.name} (ID: ${response.monitorID})`);
                    this.createdMonitorIds.push(response.monitorID);
                    resolve(response);
                } else {
                    if (response.msg && response.msg.includes('already')) {
                        console.log(`  ⚠ Already exists: ${monitorConfig.name}`);
                        resolve(response);
                    } else {
                        console.log(`  ✗ Failed: ${monitorConfig.name} - ${response.msg || 'Unknown'}`);
                        resolve(response);
                    }
                }
            });
        });
    }

    async createMonitors() {
        console.log('\n📊 Creating monitors...');
        
        // Get existing monitors
        const existing = await this.getExistingMonitors();
        const existingNames = Object.values(existing).map(m => m.name);
        
        let created = 0;
        let skipped = 0;
        let failed = 0;

        for (const monitor of monitors) {
            // Skip if already exists
            if (existingNames.includes(monitor.name)) {
                console.log(`  ⏭ Skipping (exists): ${monitor.name}`);
                // Find and store the existing monitor ID
                const existingMonitor = Object.values(existing).find(m => m.name === monitor.name);
                if (existingMonitor) {
                    this.createdMonitorIds.push(existingMonitor.id);
                }
                skipped++;
                continue;
            }

            try {
                // Build monitor config based on type
                const monitorPayload = {
                    name: monitor.name,
                    description: monitor.description || '',
                    interval: monitor.interval || 60,
                    retryInterval: monitor.retryInterval || 30,
                    maxretries: monitor.maxretries || 3,
                    notificationIDList: {},
                    active: true,
                };

                if (monitor.type === 'http') {
                    Object.assign(monitorPayload, {
                        type: 'http',
                        url: monitor.url,
                        method: monitor.method || 'GET',
                        accepted_statuscodes: monitor.accepted_statuscodes || ['200-299'],
                    });
                } else if (monitor.type === 'port') {
                    Object.assign(monitorPayload, {
                        type: 'port',
                        hostname: monitor.hostname,
                        port: monitor.port,
                        accepted_statuscodes: ['200-299'],  // Required even for port type
                    });
                }

                await this.addMonitor(monitorPayload);
                created++;
            } catch (error) {
                console.log(`  ✗ Failed: ${monitor.name} - ${error.message}`);
                failed++;
            }
        }

        console.log(`\n📈 Monitor Summary: ${created} created, ${skipped} skipped, ${failed} failed`);
    }

    async createStatusPage() {
        console.log('\n📄 Creating/updating status page...');
        
        if (this.createdMonitorIds.length === 0) {
            console.log('  ⚠ No monitor IDs collected, skipping status page');
            return null;
        }

        return new Promise((resolve) => {
            const timeout = setTimeout(() => {
                console.log('  ⚠ Status page operation timeout');
                resolve(null);
            }, 20000);
            
            // First, get existing status page to see its format
            this.socket.emit('getStatusPage', statusPageConfig.slug, async (existing) => {
                console.log(`  📥 Retrieved status page config`);
                
                if (!existing || !existing.config) {
                    console.log('  ⚠ No existing config found');
                    clearTimeout(timeout);
                    resolve(null);
                    return;
                }
                
                // Build the public group list
                const publicGroupList = [{
                    name: 'CRM Services',
                    weight: 1,
                    monitorList: this.createdMonitorIds.map((id, index) => ({
                        id: id,
                        weight: index + 1,
                    })),
                }];
                
                console.log(`  📤 Saving with ${this.createdMonitorIds.length} monitors...`);
                
                // Try to save - use the existing config as base
                const saveConfig = {
                    id: existing.config.id,
                    slug: statusPageConfig.slug,
                    title: existing.config.title || statusPageConfig.title,
                    description: existing.config.description || statusPageConfig.description,
                    icon: existing.config.icon || '',
                    theme: existing.config.theme || 'auto',
                    published: true,
                    showTags: true,
                    domainNameList: existing.config.domainNameList || [],
                    googleAnalyticsId: existing.config.googleAnalyticsId || '',
                    customCSS: existing.config.customCSS || '',
                    footerText: statusPageConfig.footerText || '',
                    showPoweredBy: false,
                    publicGroupList: publicGroupList,
                };
                
                this.socket.emit('saveStatusPage', statusPageConfig.slug, saveConfig, publicGroupList, (saveResponse) => {
                    clearTimeout(timeout);
                    if (saveResponse && saveResponse.ok) {
                        console.log(`  ✓ Added ${this.createdMonitorIds.length} monitors to status page`);
                    } else {
                        console.log(`  ⚠ Save response: ${JSON.stringify(saveResponse)}`);
                    }
                    resolve(saveResponse);
                });
            });
        });
    }

    async disconnect() {
        if (this.globalTimeout) {
            clearTimeout(this.globalTimeout);
        }
        if (this.socket) {
            this.socket.disconnect();
            console.log('\n🔌 Disconnected from Uptime Kuma');
        }
    }

    async run() {
        console.log('═══════════════════════════════════════════════════════════');
        console.log('       CRM Solution - Uptime Kuma Monitor Configuration    ');
        console.log('═══════════════════════════════════════════════════════════');
        console.log(`Target: ${UPTIME_KUMA_URL}`);
        console.log(`User: ${config.username}`);

        this.startGlobalTimeout();

        try {
            await this.connect();
            await this.login();
            await this.createMonitors();
            await this.createStatusPage();
            
            console.log('\n═══════════════════════════════════════════════════════════');
            console.log('✓ Configuration complete!');
            console.log(`  Status Page: ${UPTIME_KUMA_URL}/status/${statusPageConfig.slug}`);
            console.log('═══════════════════════════════════════════════════════════\n');
            
            await this.disconnect();
            process.exit(0);
        } catch (error) {
            console.error(`\n✗ Error: ${error.message}`);
            await this.disconnect();
            process.exit(1);
        }
    }
}

// Run the configurator
const configurator = new UptimeKumaConfigurator();
configurator.run();
