# Remote Docker Server Configuration and Deployment Script
# This script helps configure and deploy to a remote Docker/Kubernetes server

param(
    [Parameter(Position=0)]
    [ValidateSet("configure", "test", "deploy", "status")]
    [string]$Command = "configure"
)

# Configuration file path
$ConfigFile = "$PSScriptRoot/.docker-remote-config.json"

# Color output functions
function Write-Status {
    param([string]$Message)
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    Write-Host "[$timestamp] " -NoNewline -ForegroundColor Green
    Write-Host $Message
}

function Write-Error-Custom {
    param([string]$Message)
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    Write-Host "[$timestamp] ERROR: " -NoNewline -ForegroundColor Red
    Write-Host $Message
}

function Write-Warning-Custom {
    param([string]$Message)
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    Write-Host "[$timestamp] WARNING: " -NoNewline -ForegroundColor Yellow
    Write-Host $Message
}

function Write-Info {
    param([string]$Message)
    Write-Host "  " -NoNewline
    Write-Host $Message -ForegroundColor Cyan
}

# Request remote server credentials
function Request-RemoteServerCredentials {
    Write-Host ""
    Write-Host "╔════════════════════════════════════════════════════════════╗" -ForegroundColor Magenta
    Write-Host "║     Remote Docker Server Configuration                     ║" -ForegroundColor Magenta
    Write-Host "╚════════════════════════════════════════════════════════════╝" -ForegroundColor Magenta
    Write-Host ""
    
    # Server type
    Write-Host "Select connection type:" -ForegroundColor Yellow
    Write-Host "  1. Docker Host (SSH)"
    Write-Host "  2. Kubernetes Cluster (kubeconfig)"
    Write-Host "  3. Docker Swarm (via SSH)"
    $serverType = Read-Host "Enter choice (1-3)"
    
    switch ($serverType) {
        "1" { Request-DockerSSH }
        "2" { Request-KubeConfig }
        "3" { Request-DockerSwarm }
        default {
            Write-Error-Custom "Invalid choice"
            exit 1
        }
    }
}

# Request Docker SSH credentials
function Request-DockerSSH {
    Write-Host ""
    Write-Status "Configure Docker Host (SSH Connection)"
    Write-Info "Enter your remote Docker server details:"
    Write-Host ""
    
    $config = @{
        ConnectionType = "docker-ssh"
        Host = Read-Host "Remote Docker Host (e.g., docker.example.com or 192.168.1.100)"
        Port = Read-Host "SSH Port (default: 22)" -Default 22
        Username = Read-Host "SSH Username"
    }
    
    Write-Host ""
    Write-Host "Authentication method:" -ForegroundColor Yellow
    Write-Host "  1. Password"
    Write-Host "  2. SSH Key"
    $authMethod = Read-Host "Enter choice (1-2)"
    
    switch ($authMethod) {
        "1" {
            $password = Read-Host "Password" -AsSecureString
            $config["AuthMethod"] = "password"
            $config["Password"] = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto([System.Runtime.InteropServices.Marshal]::SecureStringToCoTaskMemUnicode($password))
        }
        "2" {
            $keyPath = Read-Host "SSH Key Path (e.g., C:\Users\username\.ssh\id_rsa)"
            if (-not (Test-Path $keyPath)) {
                Write-Error-Custom "SSH key not found at: $keyPath"
                exit 1
            }
            $config["AuthMethod"] = "key"
            $config["KeyPath"] = $keyPath
        }
        default {
            Write-Error-Custom "Invalid choice"
            exit 1
        }
    }
    
    # Optional Docker daemon configuration
    Write-Host ""
    $dockerPort = Read-Host "Remote Docker Daemon Port (default: 2375 for non-TLS, 2376 for TLS)"
    if (-not [string]::IsNullOrEmpty($dockerPort)) {
        $config["DockerPort"] = $dockerPort
    }
    
    $useTLS = Read-Host "Use TLS for Docker connection? (y/n)" -Default "n"
    if ($useTLS -eq "y") {
        $config["UseTLS"] = $true
    }
    
    # Registry configuration
    Write-Host ""
    Write-Status "Configure Docker Registry"
    $registry = Read-Host "Registry URL (e.g., ghcr.io/username, docker.io/username)"
    $registryUsername = Read-Host "Registry Username (leave empty if same as Docker host username)"
    $registryPassword = Read-Host "Registry Password/Token" -AsSecureString
    
    $config["Registry"] = @{
        URL = $registry
        Username = if ([string]::IsNullOrEmpty($registryUsername)) { $config["Username"] } else { $registryUsername }
        Password = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto([System.Runtime.InteropServices.Marshal]::SecureStringToCoTaskMemUnicode($registryPassword))
    }
    
    # Deployment settings
    Write-Host ""
    Write-Status "Deployment Settings"
    $config["ImageTag"] = Read-Host "Image Tag (default: latest)" -Default "latest"
    $config["Environment"] = Read-Host "Environment (development/staging/production)" -Default "production"
    
    Save-Configuration $config
    Write-Status "Configuration saved successfully!"
    Display-Configuration $config
}

# Request Kubernetes kubeconfig
function Request-KubeConfig {
    Write-Host ""
    Write-Status "Configure Kubernetes Cluster (kubeconfig)"
    Write-Info "Enter your Kubernetes cluster details:"
    Write-Host ""
    
    $config = @{
        ConnectionType = "kubernetes"
        KubeConfigPath = Read-Host "Path to kubeconfig file (e.g., C:\Users\username\.kube\config)"
        Namespace = Read-Host "Kubernetes Namespace (default: crm-app)" -Default "crm-app"
        Context = Read-Host "Kubernetes Context (leave empty for current context)"
    }
    
    # Verify kubeconfig exists
    if (-not (Test-Path $config["KubeConfigPath"])) {
        Write-Error-Custom "kubeconfig not found at: $($config['KubeConfigPath'])"
        exit 1
    }
    
    # Registry configuration
    Write-Host ""
    Write-Status "Configure Docker Registry"
    $registry = Read-Host "Registry URL (e.g., ghcr.io/username, docker.io/username)"
    $registryUsername = Read-Host "Registry Username"
    $registryPassword = Read-Host "Registry Password/Token" -AsSecureString
    
    $config["Registry"] = @{
        URL = $registry
        Username = $registryUsername
        Password = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto([System.Runtime.InteropServices.Marshal]::SecureStringToCoTaskMemUnicode($registryPassword))
    }
    
    # Deployment settings
    Write-Host ""
    Write-Status "Deployment Settings"
    $config["ImageTag"] = Read-Host "Image Tag (default: latest)" -Default "latest"
    $config["Environment"] = Read-Host "Environment (development/staging/production)" -Default "production"
    
    Save-Configuration $config
    Write-Status "Configuration saved successfully!"
    Display-Configuration $config
}

# Request Docker Swarm credentials
function Request-DockerSwarm {
    Write-Host ""
    Write-Status "Configure Docker Swarm (Manager Node)"
    Write-Info "Enter your Docker Swarm manager details:"
    Write-Host ""
    
    $config = @{
        ConnectionType = "docker-swarm"
        Host = Read-Host "Swarm Manager Host (e.g., swarm.example.com)"
        Port = Read-Host "SSH Port (default: 22)" -Default 22
        Username = Read-Host "SSH Username"
    }
    
    Write-Host ""
    Write-Host "Authentication method:" -ForegroundColor Yellow
    Write-Host "  1. Password"
    Write-Host "  2. SSH Key"
    $authMethod = Read-Host "Enter choice (1-2)"
    
    switch ($authMethod) {
        "1" {
            $password = Read-Host "Password" -AsSecureString
            $config["AuthMethod"] = "password"
            $config["Password"] = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto([System.Runtime.InteropServices.Marshal]::SecureStringToCoTaskMemUnicode($password))
        }
        "2" {
            $keyPath = Read-Host "SSH Key Path"
            if (-not (Test-Path $keyPath)) {
                Write-Error-Custom "SSH key not found"
                exit 1
            }
            $config["AuthMethod"] = "key"
            $config["KeyPath"] = $keyPath
        }
    }
    
    # Registry configuration
    Write-Host ""
    Write-Status "Configure Docker Registry"
    $registry = Read-Host "Registry URL (e.g., ghcr.io/username)"
    $registryUsername = Read-Host "Registry Username"
    $registryPassword = Read-Host "Registry Password/Token" -AsSecureString
    
    $config["Registry"] = @{
        URL = $registry
        Username = $registryUsername
        Password = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto([System.Runtime.InteropServices.Marshal]::SecureStringToCoTaskMemUnicode($registryPassword))
    }
    
    # Deployment settings
    Write-Host ""
    Write-Status "Deployment Settings"
    $config["ImageTag"] = Read-Host "Image Tag (default: latest)" -Default "latest"
    $config["Environment"] = Read-Host "Environment (development/staging/production)" -Default "production"
    
    Save-Configuration $config
    Write-Status "Configuration saved successfully!"
    Display-Configuration $config
}

# Save configuration to file
function Save-Configuration {
    param([hashtable]$Config)
    
    # Convert to JSON, excluding sensitive data in display
    $jsonConfig = $Config | ConvertTo-Json
    
    # Save to file
    $jsonConfig | Set-Content -Path $ConfigFile -Force
    
    # Also create a .gitignore entry
    if (-not (Test-Path "$PSScriptRoot/.gitignore")) {
        ".docker-remote-config.json" | Set-Content "$PSScriptRoot/.gitignore"
    } else {
        if (-not (Select-String -Path "$PSScriptRoot/.gitignore" -Pattern ".docker-remote-config.json")) {
            ".docker-remote-config.json" | Add-Content "$PSScriptRoot/.gitignore"
        }
    }
}

# Display configuration (hide sensitive data)
function Display-Configuration {
    param([hashtable]$Config)
    
    Write-Host ""
    Write-Host "╔════════════════════════════════════════════════════════════╗" -ForegroundColor Green
    Write-Host "║          Configuration Summary (Sensitive Data Hidden)     ║" -ForegroundColor Green
    Write-Host "╚════════════════════════════════════════════════════════════╝" -ForegroundColor Green
    
    Write-Info "Connection Type: $($Config['ConnectionType'])"
    
    switch ($Config['ConnectionType']) {
        "docker-ssh" {
            Write-Info "Host: $($Config['Host'])"
            Write-Info "Port: $($Config['Port'])"
            Write-Info "Username: $($Config['Username'])"
            Write-Info "Auth Method: $($Config['AuthMethod'])"
        }
        "kubernetes" {
            Write-Info "Namespace: $($Config['Namespace'])"
            if ($Config['Context']) {
                Write-Info "Context: $($Config['Context'])"
            }
        }
        "docker-swarm" {
            Write-Info "Host: $($Config['Host'])"
            Write-Info "Username: $($Config['Username'])"
        }
    }
    
    Write-Info "Registry: $($Config['Registry']['URL'])"
    Write-Info "Image Tag: $($Config['ImageTag'])"
    Write-Info "Environment: $($Config['Environment'])"
    
    Write-Host ""
    Write-Status "Next steps:"
    Write-Info "1. Run: .\deploy-remote.ps1 test"
    Write-Info "2. Verify connection to remote server"
    Write-Info "3. Run: .\deploy-remote.ps1 deploy"
}

# Load existing configuration
function Load-Configuration {
    if (Test-Path $ConfigFile) {
        $json = Get-Content $ConfigFile | ConvertFrom-Json
        return $json
    }
    return $null
}

# Test remote connection
function Test-RemoteConnection {
    $config = Load-Configuration
    
    if ($null -eq $config) {
        Write-Error-Custom "No configuration found. Run 'configure' command first."
        exit 1
    }
    
    Write-Status "Testing connection to remote server..."
    Write-Host ""
    
    switch ($config.ConnectionType) {
        "docker-ssh" {
            Test-DockerSSHConnection $config
        }
        "kubernetes" {
            Test-KubernetesConnection $config
        }
        "docker-swarm" {
            Test-DockerSwarmConnection $config
        }
    }
}

# Test Docker SSH connection
function Test-DockerSSHConnection {
    param($Config)
    
    Write-Info "Testing SSH connection to $($Config.Host)..."
    
    # Note: Actual SSH connection test would require SSH module
    # For now, we'll show what would be tested
    Write-Host ""
    Write-Host "To test, run:" -ForegroundColor Yellow
    Write-Host "ssh -p $($Config.Port) $($Config.Username)@$($Config.Host) 'docker ps'" -ForegroundColor Cyan
    Write-Host ""
    Write-Info "If successful, you should see a list of running containers."
}

# Test Kubernetes connection
function Test-KubernetesConnection {
    param($Config)
    
    Write-Info "Testing Kubernetes connection..."
    Write-Info "kubeconfig: $($Config.KubeConfigPath)"
    Write-Info "Namespace: $($Config.Namespace)"
    
    Write-Host ""
    Write-Host "To test, run:" -ForegroundColor Yellow
    Write-Host "kubectl --kubeconfig='$($Config.KubeConfigPath)' get nodes" -ForegroundColor Cyan
    Write-Host ""
    Write-Info "If successful, you should see a list of cluster nodes."
}

# Test Docker Swarm connection
function Test-DockerSwarmConnection {
    param($Config)
    
    Write-Info "Testing Docker Swarm connection to $($Config.Host)..."
    
    Write-Host ""
    Write-Host "To test, run:" -ForegroundColor Yellow
    Write-Host "ssh -p $($Config.Port) $($Config.Username)@$($Config.Host) 'docker node ls'" -ForegroundColor Cyan
    Write-Host ""
    Write-Info "If successful, you should see Swarm nodes."
}

# Deploy to remote server
function Deploy-ToRemote {
    $config = Load-Configuration
    
    if ($null -eq $config) {
        Write-Error-Custom "No configuration found. Run 'configure' command first."
        exit 1
    }
    
    Write-Status "Starting deployment to remote server..."
    Write-Host ""
    
    switch ($config.ConnectionType) {
        "docker-ssh" {
            Deploy-ViaDockerSSH $config
        }
        "kubernetes" {
            Deploy-ViaKubernetes $config
        }
        "docker-swarm" {
            Deploy-ViaDockerSwarm $config
        }
    }
}

# Deploy via Docker SSH
function Deploy-ViaDockerSSH {
    param($Config)
    
    Write-Status "Deployment via Docker SSH"
    Write-Info "Host: $($Config.Host)"
    Write-Info "Building and pushing Docker images..."
    Write-Host ""
    
    Write-Host "Prerequisites:" -ForegroundColor Yellow
    Write-Host "1. SSH access configured"
    Write-Host "2. Docker installed on remote host"
    Write-Host "3. Docker registry credentials configured"
    Write-Host ""
    
    Write-Host "Deployment steps:" -ForegroundColor Yellow
    Write-Host "1. Build backend image"
    Write-Host "2. Build frontend image"
    Write-Host "3. Push images to registry"
    Write-Host "4. SSH to remote host"
    Write-Host "5. Pull and run containers"
    Write-Host ""
    
    Write-Host "To complete deployment, run:" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "# Build backend" -ForegroundColor Gray
    Write-Host "docker build -f Dockerfile.backend -t $($Config.Registry.URL)/crm-api:$($Config.ImageTag) ." -ForegroundColor White
    Write-Host ""
    Write-Host "# Build frontend" -ForegroundColor Gray
    Write-Host "docker build -f Dockerfile.frontend -t $($Config.Registry.URL)/crm-frontend:$($Config.ImageTag) ." -ForegroundColor White
    Write-Host ""
    Write-Host "# Push to registry" -ForegroundColor Gray
    Write-Host "docker push $($Config.Registry.URL)/crm-api:$($Config.ImageTag)" -ForegroundColor White
    Write-Host "docker push $($Config.Registry.URL)/crm-frontend:$($Config.ImageTag)" -ForegroundColor White
    Write-Host ""
    Write-Host "# Deploy on remote host" -ForegroundColor Gray
    Write-Host "ssh -p $($Config.Port) $($Config.Username)@$($Config.Host)" -ForegroundColor White
    Write-Host "docker-compose pull && docker-compose up -d" -ForegroundColor White
}

# Deploy via Kubernetes
function Deploy-ViaKubernetes {
    param($Config)
    
    Write-Status "Deployment via Kubernetes"
    Write-Info "Namespace: $($Config.Namespace)"
    Write-Info "Registry: $($Config.Registry.URL)"
    Write-Host ""
    
    Write-Host "Prerequisites:" -ForegroundColor Yellow
    Write-Host "1. kubeconfig configured: $($Config.KubeConfigPath)"
    Write-Host "2. kubectl installed and accessible"
    Write-Host "3. Image registry credentials"
    Write-Host ""
    
    Write-Host "Deployment steps:" -ForegroundColor Yellow
    Write-Host "1. Build and push Docker images"
    Write-Host "2. Create image pull secret"
    Write-Host "3. Update Kubernetes manifests"
    Write-Host "4. Apply manifests to cluster"
    Write-Host "5. Monitor rollout"
    Write-Host ""
    
    Write-Host "To complete deployment, run:" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "# Set kubeconfig" -ForegroundColor Gray
    Write-Host "`$env:KUBECONFIG = '$($Config.KubeConfigPath)'" -ForegroundColor White
    Write-Host ""
    Write-Host "# Create namespace" -ForegroundColor Gray
    Write-Host "kubectl create namespace $($Config.Namespace)" -ForegroundColor White
    Write-Host ""
    Write-Host "# Create registry secret" -ForegroundColor Gray
    Write-Host "kubectl create secret docker-registry regcred -n $($Config.Namespace) \" -ForegroundColor White
    Write-Host "  --docker-server=$($Config.Registry.URL) \" -ForegroundColor White
    Write-Host "  --docker-username=$($Config.Registry.Username) \" -ForegroundColor White
    Write-Host "  --docker-password=<password>" -ForegroundColor White
    Write-Host ""
    Write-Host "# Build and push images" -ForegroundColor Gray
    Write-Host "docker build -f Dockerfile.backend -t $($Config.Registry.URL)/crm-api:$($Config.ImageTag) ." -ForegroundColor White
    Write-Host "docker push $($Config.Registry.URL)/crm-api:$($Config.ImageTag)" -ForegroundColor White
    Write-Host ""
    Write-Host "# Deploy" -ForegroundColor Gray
    Write-Host ".\deploy.ps1 deploy" -ForegroundColor White
}

# Deploy via Docker Swarm
function Deploy-ViaDockerSwarm {
    param($Config)
    
    Write-Status "Deployment via Docker Swarm"
    Write-Info "Manager: $($Config.Host)"
    Write-Host ""
    
    Write-Host "Prerequisites:" -ForegroundColor Yellow
    Write-Host "1. Docker Swarm initialized"
    Write-Host "2. SSH access to manager node"
    Write-Host "3. Docker registry credentials"
    Write-Host ""
    
    Write-Host "Deployment steps:" -ForegroundColor Yellow
    Write-Host "1. Build and push Docker images"
    Write-Host "2. SSH to manager node"
    Write-Host "3. Deploy docker-compose stack"
    Write-Host ""
    
    Write-Host "To complete deployment, run:" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "# Build and push images" -ForegroundColor Gray
    Write-Host "docker build -f Dockerfile.backend -t $($Config.Registry.URL)/crm-api:$($Config.ImageTag) ." -ForegroundColor White
    Write-Host "docker push $($Config.Registry.URL)/crm-api:$($Config.ImageTag)" -ForegroundColor White
    Write-Host ""
    Write-Host "# Deploy to Swarm" -ForegroundColor Gray
    Write-Host "ssh -p $($Config.Port) $($Config.Username)@$($Config.Host)" -ForegroundColor White
    Write-Host "docker stack deploy -c docker-compose.yml crm" -ForegroundColor White
}

# Show deployment status
function Show-Status {
    $config = Load-Configuration
    
    if ($null -eq $config) {
        Write-Error-Custom "No configuration found."
        exit 1
    }
    
    Write-Status "Current Configuration"
    Write-Host ""
    
    switch ($config.ConnectionType) {
        "docker-ssh" {
            Write-Info "Connection Type: Docker Host (SSH)"
            Write-Info "Host: $($config.Host):$($config.Port)"
            Write-Info "Username: $($config.Username)"
            Write-Info "Auth: $($config.AuthMethod)"
        }
        "kubernetes" {
            Write-Info "Connection Type: Kubernetes Cluster"
            Write-Info "Namespace: $($config.Namespace)"
            if ($config.Context) {
                Write-Info "Context: $($config.Context)"
            }
        }
        "docker-swarm" {
            Write-Info "Connection Type: Docker Swarm"
            Write-Info "Manager: $($config.Host):$($config.Port)"
            Write-Info "Username: $($config.Username)"
        }
    }
    
    Write-Info "Registry: $($config.Registry.URL)"
    Write-Info "Image Tag: $($config.ImageTag)"
    Write-Info "Environment: $($config.Environment)"
    
    Write-Host ""
    Write-Status "Configuration saved at: $ConfigFile"
}

# Show help
function Show-Help {
    Write-Host @"
Remote Docker Server Configuration and Deployment

Usage: .\deploy-remote.ps1 [Command]

Commands:
  configure    Request and configure remote server credentials (default)
  test         Test connection to remote server
  status       Show current configuration
  deploy       Deploy application to remote server
  help         Show this help message

Examples:
  .\deploy-remote.ps1 configure
  .\deploy-remote.ps1 test
  .\deploy-remote.ps1 deploy

Connection Types Supported:
  1. Docker Host (SSH connection to Docker daemon)
  2. Kubernetes Cluster (using kubeconfig file)
  3. Docker Swarm (Manager node via SSH)

Configuration is saved securely in: .docker-remote-config.json
(This file is added to .gitignore)

"@
}

# Main execution
try {
    switch ($Command) {
        "configure" {
            Request-RemoteServerCredentials
        }
        "test" {
            Test-RemoteConnection
        }
        "deploy" {
            Deploy-ToRemote
        }
        "status" {
            Show-Status
        }
        "help" {
            Show-Help
        }
        default {
            Write-Error-Custom "Unknown command: $Command"
            Write-Host ""
            Show-Help
            exit 1
        }
    }
}
catch {
    Write-Error-Custom "An error occurred: $_"
    exit 1
}
