# CRM Application Kubernetes Deployment Script (PowerShell)
# This script deploys the entire CRM application to a Kubernetes cluster

param(
    [Parameter(Position=0)]
    [ValidateSet("deploy", "forward", "verify", "logs", "scale", "update-images", "cleanup")]
    [string]$Command = "deploy",
    
    [Parameter(Position=1)]
    [string]$Param1,
    
    [Parameter(Position=2)]
    [string]$Param2
)

# Configuration
$Namespace = $env:KUBE_NAMESPACE -or "crm-app"
$Registry = $env:DOCKER_REGISTRY
$ImageTag = $env:IMAGE_TAG -or "latest"
$Environment = $env:ENVIRONMENT -or "production"

# Color output functions
function Write-Status {
    param([string]$Message)
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    Write-Host "[$timestamp] " -NoNewline -ForegroundColor Green
    Write-Host $Message
}

function Write-Error {
    param([string]$Message)
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    Write-Host "[$timestamp] ERROR: " -NoNewline -ForegroundColor Red
    Write-Host $Message
}

function Write-Warning {
    param([string]$Message)
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    Write-Host "[$timestamp] WARNING: " -NoNewline -ForegroundColor Yellow
    Write-Host $Message
}

# Prompt for registry
function Prompt-Registry {
    if ([string]::IsNullOrEmpty($Registry)) {
        Write-Host ""
        Write-Host "Docker Registry Configuration" -ForegroundColor Yellow
        Write-Host "Please enter your Docker registry address"
        Write-Host "Examples: docker.io/username, ghcr.io/username, myregistry.azurecr.io"
        $Registry = Read-Host "Docker Registry Address"
        
        if ([string]::IsNullOrEmpty($Registry)) {
            Write-Error "Docker registry address is required"
            exit 1
        }
        
        Write-Status "Using registry: $Registry"
    }
}

# Check prerequisites
function Check-Prerequisites {
    Write-Status "Checking prerequisites..."
    
    # Check kubectl
    if (-not (Get-Command kubectl -ErrorAction SilentlyContinue)) {
        Write-Error "kubectl is not installed"
        exit 1
    }
    
    # Check cluster connectivity
    kubectl cluster-info 2>&1 | Out-Null
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Cannot connect to Kubernetes cluster"
        exit 1
    }
    
    # Check metrics-server
    $metricsServer = kubectl get deployment metrics-server -n kube-system -ErrorAction SilentlyContinue
    if (-not $metricsServer) {
        Write-Warning "Metrics Server not found. HPA may not work."
        Write-Warning "Install with: kubectl apply -f https://github.com/kubernetes-sigs/metrics-server/releases/latest/download/components.yaml"
    }
    
    Write-Status "Prerequisites check passed"
}

# Create namespace
function Create-Namespace {
    Write-Status "Creating namespace: $Namespace"
    kubectl create namespace $Namespace --dry-run=client -o yaml | kubectl apply -f -
}

# Apply manifests
function Apply-Manifests {
    Write-Status "Applying Kubernetes manifests..."
    
    kubectl apply -f kubernetes/00-namespace-config.yaml
    Write-Status "Applied: namespace and config"
    
    kubectl apply -f kubernetes/01-database-tier.yaml
    Write-Status "Applied: database tier"
    
    kubectl apply -f kubernetes/02-application-tier.yaml
    Write-Status "Applied: application tier"
    
    kubectl apply -f kubernetes/03-presentation-tier.yaml
    Write-Status "Applied: presentation tier"
    
    kubectl apply -f kubernetes/04-ingress-network.yaml
    Write-Status "Applied: ingress and network policies"
}

# Wait for rollout
function Wait-For-Rollout {
    Write-Status "Waiting for deployments to be ready..."
    
    kubectl rollout status deployment/crm-api -n $Namespace --timeout=5m
    kubectl rollout status deployment/crm-frontend -n $Namespace --timeout=5m
    
    Write-Status "Deployments are ready"
}

# Verify deployment
function Verify-Deployment {
    Write-Status "Verifying deployment..."
    
    Write-Host ""
    Write-Status "Pods status:"
    kubectl get pods -n $Namespace -o wide
    
    Write-Host ""
    Write-Status "Services:"
    kubectl get services -n $Namespace
    
    Write-Host ""
    Write-Status "Deployments:"
    kubectl get deployments -n $Namespace
    
    Write-Host ""
    Write-Status "HPA status:"
    kubectl get hpa -n $Namespace
    
    Write-Host ""
    Write-Status "PVC status:"
    kubectl get pvc -n $Namespace
}

# Port forward
function Port-Forward-Services {
    Write-Status "Setting up port forwarding..."
    Write-Status "API available at: http://localhost:5000"
    Write-Status "Frontend available at: http://localhost:3000"
    Write-Host "Press Ctrl+C to stop port forwarding"
    
    Start-Job -ScriptBlock {
        kubectl port-forward -n $using:Namespace svc/crm-api 5000:5000
    } | Out-Null
    
    Start-Job -ScriptBlock {
        kubectl port-forward -n $using:Namespace svc/crm-frontend 3000:3000
    } | Out-Null
    
    Write-Status "Port forwarding active. Press any key to stop..."
    $null = Read-Host
    Get-Job | Stop-Job
}

# Show logs
function Show-Logs {
    param([string]$Deployment, [int]$Lines = 100)
    
    Write-Status "Last $Lines logs from ${Deployment}:"
    kubectl logs -n $Namespace -l app=crm,tier=$Deployment --tail=$Lines -f
}

# Scale deployment
function Scale-Deployment {
    param([string]$Name, [int]$Replicas)
    
    Write-Status "Scaling $Name to $Replicas replicas..."
    kubectl scale deployment/$Name -n $Namespace --replicas=$Replicas
}

# Update images
function Update-Images {
    Write-Status "Updating container images..."
    
    kubectl set image deployment/crm-api crm-api=$Registry/crm-api:$ImageTag -n $Namespace
    kubectl set image deployment/crm-frontend crm-frontend=$Registry/crm-frontend:$ImageTag -n $Namespace
    
    Write-Status "Waiting for rollout to complete..."
    kubectl rollout status deployment/crm-api -n $Namespace --timeout=5m
    kubectl rollout status deployment/crm-frontend -n $Namespace --timeout=5m
    
    Write-Status "Update complete"
}

# Cleanup
function Cleanup-Deployment {
    Write-Status "Cleaning up deployment..."
    kubectl delete namespace $Namespace --ignore-not-found=true
    Write-Status "Cleanup complete"
}

# Show help
function Show-Help {
    Write-Host @"
CRM Application Kubernetes Deployment Script

Usage: .\deploy.ps1 [Command] [Param1] [Param2]

Commands:
  deploy           Deploy the entire application (default)
  forward          Setup port forwarding to services
  verify           Verify deployment status
  logs             Show logs (requires 'api' or 'frontend' parameter)
  scale            Scale deployment (requires 'api'/'frontend' and replica count)
  update-images    Update container images
  cleanup          Remove all resources

Environment Variables:
  KUBE_NAMESPACE   Kubernetes namespace (default: crm-app)
  DOCKER_REGISTRY  Docker registry URL (default: your-registry)
  IMAGE_TAG        Image tag (default: latest)
  ENVIRONMENT      Environment (default: production)

Examples:
  .\deploy.ps1 deploy
  .\deploy.ps1 forward
  .\deploy.ps1 logs api 100
  .\deploy.ps1 scale api 3
  .\deploy.ps1 cleanup
"@
}

# Main execution
try {
    Write-Status "CRM Application Kubernetes Deployment"
    Write-Host "Namespace: $Namespace"
    Write-Host "Environment: $Environment"
    Write-Host "Registry: $Registry"
    Write-Host "Image Tag: $ImageTag"
    Write-Host ""
    
    switch ($Command) {
        "deploy" {
            Prompt-Registry
            Check-Prerequisites
            Create-Namespace
            Apply-Manifests
            Wait-For-Rollout
            Verify-Deployment
        }
        "forward" {
            Port-Forward-Services
        }
        "verify" {
            Verify-Deployment
        }
        "logs" {
            if ([string]::IsNullOrEmpty($Param1)) {
                Write-Error "Usage: .\deploy.ps1 logs [api|frontend] [lines]"
                exit 1
            }
            Show-Logs -Deployment $Param1 -Lines ([int]($Param2 -or 100))
        }
        "scale" {
            if ([string]::IsNullOrEmpty($Param1) -or [string]::IsNullOrEmpty($Param2)) {
                Write-Error "Usage: .\deploy.ps1 scale [api|frontend] <replicas>"
                exit 1
            }
            Scale-Deployment -Name "crm-$Param1" -Replicas ([int]$Param2)
        }
        "update-images" {
            Prompt-Registry
            Update-Images
        }
        "cleanup" {
            Cleanup-Deployment
        }
        default {
            Write-Error "Unknown command: $Command"
            Write-Host ""
            Show-Help
            exit 1
        }
    }
}
catch {
    Write-Error "An error occurred: $_"
    exit 1
}
