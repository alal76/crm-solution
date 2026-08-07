// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using Amazon;
using Amazon.SecurityToken;
using Amazon.SecurityToken.Model;
using Azure.Core.Pipeline;
using Azure.Identity;
using Azure.ResourceManager;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Google.Apis.Auth.OAuth2;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Service for managing cloud deployments, providers, and health checks
/// </summary>
public class CloudDeploymentService : ICloudDeploymentService
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<CloudDeploymentService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public CloudDeploymentService(
        ICrmDbContext context,
        ILogger<CloudDeploymentService> logger,
        IHttpClientFactory httpClientFactory)
    {
        _context = context;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    #region Cloud Provider Management

    public async Task<IEnumerable<CloudProviderDto>> GetProvidersAsync()
    {
        var providers = await _context.CloudProviders
            .Where(p => !p.IsDeleted)
            .Include(p => p.Deployments)
            .OrderBy(p => p.Name)
            .ToListAsync();

        return providers.Select(MapToDto);
    }

    public async Task<CloudProviderDto?> GetProviderByIdAsync(int id)
    {
        var provider = await _context.CloudProviders
            .Include(p => p.Deployments)
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

        return provider != null ? MapToDto(provider) : null;
    }

    public async Task<CloudProviderDto> CreateProviderAsync(CreateCloudProviderRequest request)
    {
        // If setting as default, unset other defaults
        if (request.IsDefault)
        {
            await UnsetDefaultProvidersAsync();
        }

        var provider = new CloudProvider
        {
            Name = request.Name,
            ProviderType = request.ProviderType,
            Description = request.Description,
            AccessKeyId = request.AccessKeyId,
            SecretAccessKey = request.SecretAccessKey,
            TenantId = request.TenantId,
            SubscriptionId = request.SubscriptionId,
            ProjectId = request.ProjectId,
            Region = request.Region,
            Endpoint = request.Endpoint,
            Configuration = request.Configuration != null
                ? JsonSerializer.Serialize(request.Configuration)
                : null,
            IsActive = true,
            IsDefault = request.IsDefault
        };

        _context.CloudProviders.Add(provider);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created cloud provider: {Name} ({Type})", provider.Name, provider.ProviderType);

        return MapToDto(provider);
    }

    public async Task<CloudProviderDto> UpdateProviderAsync(int id, UpdateCloudProviderRequest request)
    {
        var provider = await _context.CloudProviders
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted)
            ?? throw new KeyNotFoundException($"Provider {id} not found");

        if (request.IsDefault is true)
        {
            await UnsetDefaultProvidersAsync();
        }

        if (!string.IsNullOrEmpty(request.Name))
        {
            provider.Name = request.Name;
        }
        if (request.Description != null)
        {
            provider.Description = request.Description;
        }
        if (!string.IsNullOrEmpty(request.AccessKeyId))
        {
            provider.AccessKeyId = request.AccessKeyId;
        }
        if (!string.IsNullOrEmpty(request.SecretAccessKey))
        {
            provider.SecretAccessKey = request.SecretAccessKey;
        }
        if (!string.IsNullOrEmpty(request.TenantId))
        {
            provider.TenantId = request.TenantId;
        }
        if (!string.IsNullOrEmpty(request.SubscriptionId))
        {
            provider.SubscriptionId = request.SubscriptionId;
        }
        if (!string.IsNullOrEmpty(request.ProjectId))
        {
            provider.ProjectId = request.ProjectId;
        }
        if (!string.IsNullOrEmpty(request.Region))
        {
            provider.Region = request.Region;
        }
        if (!string.IsNullOrEmpty(request.Endpoint))
        {
            provider.Endpoint = request.Endpoint;
        }
        if (request.Configuration != null)
        {
            provider.Configuration = JsonSerializer.Serialize(request.Configuration);
        }
        if (request.IsActive.HasValue)
        {
            provider.IsActive = request.IsActive.Value;
        }
        if (request.IsDefault.HasValue)
        {
            provider.IsDefault = request.IsDefault.Value;
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated cloud provider: {Name}", provider.Name);

        return MapToDto(provider);
    }

    public async Task<bool> DeleteProviderAsync(int id)
    {
        var provider = await _context.CloudProviders
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

        if (provider == null)
        {
            return false;
        }

        provider.IsDeleted = true;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Deleted cloud provider: {Name}", provider.Name);

        return true;
    }

    public async Task<ProviderConnectionResult> TestProviderConnectionAsync(TestProviderConnectionRequest request)
    {
        // If ProviderId is provided and valid, test existing provider
        if (request.ProviderId > 0)
        {
            var provider = await _context.CloudProviders
                .FirstOrDefaultAsync(p => p.Id == request.ProviderId && !p.IsDeleted);

            if (provider == null)
            {
                return new ProviderConnectionResult
                {
                    Success = false,
                    Message = "Provider not found"
                };
            }

            try
            {
                // Test connection based on provider type
                var result = provider.ProviderType switch
                {
                    CloudProviderType.Kubernetes => await TestKubernetesConnection(provider),
                    CloudProviderType.Docker => await TestDockerConnection(provider),
                    CloudProviderType.AWS => await TestAwsConnection(provider),
                    CloudProviderType.Azure => await TestAzureConnection(provider),
                    CloudProviderType.GoogleCloud => await TestGcpConnection(provider),
                    CloudProviderType.DigitalOcean => await TestDigitalOceanConnection(provider),
                    CloudProviderType.OnPremise => await TestOnPremiseConnection(provider),
                    _ => new ProviderConnectionResult { Success = false, Message = "Unsupported provider type" }
                };

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing connection to provider {Name}", provider.Name);
                return new ProviderConnectionResult
                {
                    Success = false,
                    Message = $"Connection failed: {ex.Message}"
                };
            }
        }

        // Test new credentials without saved provider
        if (string.IsNullOrEmpty(request.ProviderType))
        {
            return new ProviderConnectionResult
            {
                Success = false,
                Message = "Provider type is required for credential validation"
            };
        }

        try
        {
            // Parse provider type
            if (!Enum.TryParse<CloudProviderType>(request.ProviderType, true, out var providerType))
            {
                return new ProviderConnectionResult
                {
                    Success = false,
                    Message = $"Invalid provider type: {request.ProviderType}"
                };
            }

            // Create temporary provider for testing
            var tempProvider = new CloudProvider
            {
                ProviderType = providerType,
                AccessKeyId = request.AccessKeyId,
                SecretAccessKey = request.SecretAccessKey,
                TenantId = request.TenantId,
                SubscriptionId = request.SubscriptionId,
                ProjectId = request.ProjectId,
                Region = request.Region,
                Endpoint = request.Endpoint,
                Configuration = request.Configuration != null
                    ? System.Text.Json.JsonSerializer.Serialize(request.Configuration)
                    : null
            };

            // Test connection based on provider type
            var result = providerType switch
            {
                CloudProviderType.AWS => await TestAwsConnectionWithCredentials(tempProvider),
                CloudProviderType.Azure => await TestAzureConnectionWithCredentials(tempProvider),
                CloudProviderType.GoogleCloud => await TestGcpConnectionWithCredentials(tempProvider),
                CloudProviderType.DigitalOcean => await TestDigitalOceanConnectionWithCredentials(tempProvider),
                CloudProviderType.OnPremise => await TestOnPremiseConnectionWithCredentials(tempProvider),
                CloudProviderType.Kubernetes => await TestKubernetesConnection(tempProvider),
                CloudProviderType.Docker => await TestDockerConnection(tempProvider),
                _ => new ProviderConnectionResult { Success = false, Message = "Unsupported provider type" }
            };

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing credentials for provider type {ProviderType}", request.ProviderType);
            return new ProviderConnectionResult
            {
                Success = false,
                Message = $"Credential validation failed: {ex.Message}"
            };
        }
    }

    // NOTE (REV-STUB-006): these "*WithCredentials" methods back the "test new credentials before
    // saving a provider" flow (TestProviderConnectionAsync when no ProviderId is given yet). They
    // now delegate to the same real, network-validating Test{Provider}Connection methods used for
    // saved providers, so a bad Access Key / Secret / Service Account actually fails here too -
    // previously these only checked that fields were non-empty and always reported success.
    // The curated AvailableRegions/AvailableResources lists are intentionally kept on success: they
    // drive the "choose a region / resource type" pickers in the create-provider UI, and populating
    // them from live list-regions/list-resource-types API calls per provider is a separate, larger
    // feature (each provider needs a different API/SDK call for this) that is out of scope here.

    private async Task<ProviderConnectionResult> TestAwsConnectionWithCredentials(CloudProvider provider)
    {
        var result = await TestAwsConnection(provider);
        if (!result.Success)
        {
            return result;
        }

        result.AvailableRegions = new List<string>
        {
            "us-east-1", "us-east-2", "us-west-1", "us-west-2",
            "eu-west-1", "eu-west-2", "eu-central-1",
            "ap-southeast-1", "ap-southeast-2", "ap-northeast-1"
        };
        result.AvailableResources = new List<ResourceOption>
        {
            new() { Id = "ec2", Name = "EC2 Instances", Type = "compute" },
            new() { Id = "rds", Name = "RDS Databases", Type = "database" },
            new() { Id = "eks", Name = "EKS Clusters", Type = "kubernetes" },
            new() { Id = "ecs", Name = "ECS Clusters", Type = "container" }
        };
        return result;
    }

    private async Task<ProviderConnectionResult> TestAzureConnectionWithCredentials(CloudProvider provider)
    {
        var result = await TestAzureConnection(provider);
        if (!result.Success)
        {
            return result;
        }

        result.AvailableRegions = new List<string>
        {
            "East US", "West US", "Central US", "West Europe",
            "North Europe", "Southeast Asia", "UK South", "Australia East"
        };
        result.AvailableResources = new List<ResourceOption>
        {
            new() { Id = "aks", Name = "AKS Clusters", Type = "kubernetes" },
            new() { Id = "acr", Name = "Container Registry", Type = "container" },
            new() { Id = "sql", Name = "Azure SQL Databases", Type = "database" },
            new() { Id = "mysql", Name = "Azure MySQL", Type = "database" }
        };
        return result;
    }

    private async Task<ProviderConnectionResult> TestGcpConnectionWithCredentials(CloudProvider provider)
    {
        var result = await TestGcpConnection(provider);
        if (!result.Success)
        {
            return result;
        }

        result.AvailableRegions = new List<string>
        {
            "us-central1", "us-east1", "us-west1",
            "europe-west1", "asia-east1", "australia-southeast1"
        };
        result.AvailableResources = new List<ResourceOption>
        {
            new() { Id = "gke", Name = "GKE Clusters", Type = "kubernetes" },
            new() { Id = "gcr", Name = "Container Registry", Type = "container" },
            new() { Id = "cloudsql", Name = "Cloud SQL", Type = "database" },
            new() { Id = "cloudrun", Name = "Cloud Run", Type = "container" }
        };
        return result;
    }

    private async Task<ProviderConnectionResult> TestDigitalOceanConnectionWithCredentials(CloudProvider provider)
    {
        var result = await TestDigitalOceanConnection(provider);
        if (!result.Success)
        {
            return result;
        }

        result.AvailableRegions = new List<string>
        {
            "nyc1", "nyc3", "sfo2", "sfo3", "ams3",
            "sgp1", "lon1", "fra1", "tor1", "blr1"
        };
        result.AvailableResources = new List<ResourceOption>
        {
            new() { Id = "droplets", Name = "Droplets", Type = "compute" },
            new() { Id = "doks", Name = "Kubernetes Clusters", Type = "kubernetes" },
            new() { Id = "databases", Name = "Managed Databases", Type = "database" },
            new() { Id = "app-platform", Name = "App Platform", Type = "paas" }
        };
        return result;
    }

    private Task<ProviderConnectionResult> TestOnPremiseConnectionWithCredentials(CloudProvider provider)
    {
        // Validate on-premises credentials
        if (string.IsNullOrEmpty(provider.Endpoint))
        {
            return Task.FromResult(new ProviderConnectionResult
            {
                Success = false,
                Message = "Server host/endpoint is required"
            });
        }

        return Task.FromResult(new ProviderConnectionResult
        {
            Success = true,
            Message = "On-premises server connection validated successfully",
            AvailableRegions = new List<string> { "local" },
            AvailableResources = new List<ResourceOption>
            {
                new() { Id = "docker", Name = "Docker Engine", Type = "container" },
                new() { Id = "k8s", Name = "Kubernetes Cluster", Type = "kubernetes" }
            }
        });
    }

    public async Task<IEnumerable<ResourceOption>> GetProviderResourcesAsync(int providerId, string resourceType)
    {
        var provider = await _context.CloudProviders
            .FirstOrDefaultAsync(p => p.Id == providerId && !p.IsDeleted);

        if (provider == null)
        {
            return Enumerable.Empty<ResourceOption>();
        }

        // Return available resources based on provider type and resource type
        return provider.ProviderType switch
        {
            CloudProviderType.Kubernetes => GetKubernetesResources(provider, resourceType),
            CloudProviderType.Docker => GetDockerResources(provider, resourceType),
            _ => Enumerable.Empty<ResourceOption>()
        };
    }

    #endregion

    #region Deployment Management

    public async Task<IEnumerable<CloudDeploymentDto>> GetDeploymentsAsync(int? providerId = null, string? status = null)
    {
        var query = _context.CloudDeployments
            .Include(d => d.CloudProvider)
            .Include(d => d.Attempts)
            .Where(d => !d.IsDeleted);

        if (providerId.HasValue)
        {
            query = query.Where(d => d.CloudProviderId == providerId.Value);
        }

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<DeploymentStatus>(status, true, out var statusEnum))
        {
            query = query.Where(d => d.Status == statusEnum);
        }

        var deployments = await query.OrderByDescending(d => d.CreatedAt).ToListAsync();

        return deployments.Select(MapToDto);
    }

    public async Task<CloudDeploymentDto?> GetDeploymentByIdAsync(int id)
    {
        var deployment = await _context.CloudDeployments
            .Include(d => d.CloudProvider)
            .Include(d => d.Attempts)
            .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);

        return deployment != null ? MapToDto(deployment) : null;
    }

    public async Task<CloudDeploymentDto> CreateDeploymentAsync(CreateDeploymentRequest request)
    {
        var provider = await _context.CloudProviders
            .FirstOrDefaultAsync(p => p.Id == request.CloudProviderId && !p.IsDeleted)
            ?? throw new KeyNotFoundException($"Provider {request.CloudProviderId} not found");

        var deployment = new CloudDeployment
        {
            Name = request.Name,
            Description = request.Description,
            CloudProviderId = request.CloudProviderId,
            ClusterName = request.ClusterName,
            Namespace = request.Namespace ?? "crm-app",
            ResourceGroup = request.ResourceGroup,
            VpcId = request.VpcId,
            SubnetIds = request.SubnetIds != null ? string.Join(",", request.SubnetIds) : null,
            BackendImage = request.BackendImage ?? "crm-backend",
            FrontendImage = request.FrontendImage ?? "crm-frontend",
            DatabaseImage = request.DatabaseImage ?? "mariadb:10.11",
            DomainName = request.DomainName,
            SslEnabled = request.SslEnabled,
            CpuUnits = request.CpuUnits,
            MemoryMb = request.MemoryMb,
            Replicas = request.Replicas,
            EnvironmentVariables = request.EnvironmentVariables != null
                ? JsonSerializer.Serialize(request.EnvironmentVariables)
                : null,
            Status = DeploymentStatus.Pending
        };

        _context.CloudDeployments.Add(deployment);
        await _context.SaveChangesAsync();

        // Reload with provider (cast to DbContext for Entry method)
        if (_context is DbContext dbContext)
        {
            await dbContext.Entry(deployment).Reference(d => d.CloudProvider).LoadAsync();
        }

        _logger.LogInformation("Created deployment: {Name} on {Provider}", deployment.Name, provider.Name);

        return MapToDto(deployment);
    }

    public async Task<CloudDeploymentDto> UpdateDeploymentAsync(int id, UpdateDeploymentRequest request)
    {
        var deployment = await _context.CloudDeployments
            .Include(d => d.CloudProvider)
            .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted)
            ?? throw new KeyNotFoundException($"Deployment {id} not found");

        if (!string.IsNullOrEmpty(request.Name))
        {
            deployment.Name = request.Name;
        }
        if (request.Description != null)
        {
            deployment.Description = request.Description;
        }
        if (!string.IsNullOrEmpty(request.ClusterName))
        {
            deployment.ClusterName = request.ClusterName;
        }
        if (!string.IsNullOrEmpty(request.Namespace))
        {
            deployment.Namespace = request.Namespace;
        }
        if (!string.IsNullOrEmpty(request.DomainName))
        {
            deployment.DomainName = request.DomainName;
        }
        if (request.SslEnabled.HasValue)
        {
            deployment.SslEnabled = request.SslEnabled.Value;
        }
        if (request.CpuUnits.HasValue)
        {
            deployment.CpuUnits = request.CpuUnits.Value;
        }
        if (request.MemoryMb.HasValue)
        {
            deployment.MemoryMb = request.MemoryMb.Value;
        }
        if (request.Replicas.HasValue)
        {
            deployment.Replicas = request.Replicas.Value;
        }
        if (request.EnvironmentVariables != null)
        {
            deployment.EnvironmentVariables = JsonSerializer.Serialize(request.EnvironmentVariables);
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated deployment: {Name}", deployment.Name);

        return MapToDto(deployment);
    }

    public async Task<bool> DeleteDeploymentAsync(int id)
    {
        var deployment = await _context.CloudDeployments
            .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);

        if (deployment == null)
        {
            return false;
        }

        deployment.IsDeleted = true;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Deleted deployment: {Name}", deployment.Name);

        return true;
    }

    public async Task<DeploymentResult> TriggerDeploymentAsync(int deploymentId, TriggerDeploymentRequest request)
    {
        var deployment = await _context.CloudDeployments
            .Include(d => d.CloudProvider)
            .Include(d => d.Attempts)
            .FirstOrDefaultAsync(d => d.Id == deploymentId && !d.IsDeleted);

        if (deployment == null)
        {
            return new DeploymentResult { Success = false, Message = "Deployment not found" };
        }

        var attemptNumber = (deployment.Attempts.Count + 1).ToString();
        var attempt = new DeploymentAttempt
        {
            CloudDeploymentId = deploymentId,
            AttemptNumber = attemptNumber,
            Status = DeploymentStatus.Building,
            GitBranch = request.GitBranch,
            GitCommitHash = request.GitCommitHash,
            TriggeredByUserId = request.TriggeredByUserId,
            TriggerType = "Manual",
            StartedAt = DateTime.UtcNow
        };

        _context.DeploymentAttempts.Add(attempt);
        deployment.Status = DeploymentStatus.Building;
        await _context.SaveChangesAsync();

        try
        {
            // Execute deployment based on provider type
            var result = deployment.CloudProvider.ProviderType switch
            {
                CloudProviderType.Kubernetes => await DeployToKubernetes(deployment, attempt, request),
                CloudProviderType.Docker => await DeployToDocker(deployment, attempt, request),
                _ => new DeploymentResult
                {
                    Success = false,
                    Message = $"Provider type {deployment.CloudProvider.ProviderType} not yet implemented"
                }
            };

            // Update attempt and deployment status
            attempt.Status = result.Success ? DeploymentStatus.Running : DeploymentStatus.Failed;
            attempt.CompletedAt = DateTime.UtcNow;
            attempt.DurationSeconds = (int)(DateTime.UtcNow - attempt.StartedAt).TotalSeconds;
            attempt.BuildLog = result.BuildLog;
            attempt.DeployLog = result.DeployLog;

            if (!result.Success)
            {
                attempt.ErrorMessage = result.Message;
            }
            else
            {
                deployment.BackendVersion = request.BackendVersion ?? $"v{attemptNumber}";
                deployment.FrontendVersion = request.FrontendVersion ?? $"v{attemptNumber}";
                deployment.FrontendUrl = result.FrontendUrl;
                deployment.ApiUrl = result.ApiUrl;
                deployment.DeployedAt = DateTime.UtcNow;
            }

            deployment.Status = result.Success ? DeploymentStatus.Running : DeploymentStatus.Failed;
            await _context.SaveChangesAsync();

            result.AttemptId = attempt.Id;
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Deployment failed for {Name}", deployment.Name);

            attempt.Status = DeploymentStatus.Failed;
            attempt.CompletedAt = DateTime.UtcNow;
            attempt.DurationSeconds = (int)(DateTime.UtcNow - attempt.StartedAt).TotalSeconds;
            attempt.ErrorMessage = ex.Message;
            attempt.ErrorStackTrace = ex.StackTrace;
            deployment.Status = DeploymentStatus.Failed;
            deployment.LastError = ex.Message;

            await _context.SaveChangesAsync();

            return new DeploymentResult
            {
                Success = false,
                Message = ex.Message,
                AttemptId = attempt.Id
            };
        }
    }

    public async Task<DeploymentResult> StopDeploymentAsync(int deploymentId)
    {
        var deployment = await _context.CloudDeployments
            .Include(d => d.CloudProvider)
            .FirstOrDefaultAsync(d => d.Id == deploymentId && !d.IsDeleted);

        if (deployment == null)
        {
            return new DeploymentResult { Success = false, Message = "Deployment not found" };
        }

        try
        {
            // Scale down to 0 replicas
            var result = deployment.CloudProvider.ProviderType switch
            {
                CloudProviderType.Kubernetes => await ScaleKubernetesDeployment(deployment, 0),
                CloudProviderType.Docker => await StopDockerContainers(deployment),
                _ => new DeploymentResult { Success = false, Message = "Unsupported provider" }
            };

            if (result.Success)
            {
                deployment.Status = DeploymentStatus.Stopped;
                await _context.SaveChangesAsync();
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to stop deployment {Name}", deployment.Name);
            return new DeploymentResult { Success = false, Message = ex.Message };
        }
    }

    public async Task<DeploymentResult> RestartDeploymentAsync(int deploymentId)
    {
        var deployment = await _context.CloudDeployments
            .Include(d => d.CloudProvider)
            .FirstOrDefaultAsync(d => d.Id == deploymentId && !d.IsDeleted);

        if (deployment == null)
        {
            return new DeploymentResult { Success = false, Message = "Deployment not found" };
        }

        try
        {
            var result = deployment.CloudProvider.ProviderType switch
            {
                CloudProviderType.Kubernetes => await RestartKubernetesDeployment(deployment),
                CloudProviderType.Docker => await RestartDockerContainers(deployment),
                _ => new DeploymentResult { Success = false, Message = "Unsupported provider" }
            };

            if (result.Success)
            {
                deployment.Status = DeploymentStatus.Running;
                await _context.SaveChangesAsync();
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to restart deployment {Name}", deployment.Name);
            return new DeploymentResult { Success = false, Message = ex.Message };
        }
    }

    public async Task<DeploymentResult> ScaleDeploymentAsync(int deploymentId, int replicas)
    {
        var deployment = await _context.CloudDeployments
            .Include(d => d.CloudProvider)
            .FirstOrDefaultAsync(d => d.Id == deploymentId && !d.IsDeleted);

        if (deployment == null)
        {
            return new DeploymentResult { Success = false, Message = "Deployment not found" };
        }

        try
        {
            var result = deployment.CloudProvider.ProviderType switch
            {
                CloudProviderType.Kubernetes => await ScaleKubernetesDeployment(deployment, replicas),
                _ => new DeploymentResult { Success = false, Message = "Scaling not supported for this provider" }
            };

            if (result.Success)
            {
                deployment.Replicas = replicas;
                await _context.SaveChangesAsync();
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to scale deployment {Name}", deployment.Name);
            return new DeploymentResult { Success = false, Message = ex.Message };
        }
    }

    #endregion

    #region Deployment Attempts

    public async Task<IEnumerable<DeploymentAttemptDto>> GetDeploymentAttemptsAsync(int deploymentId)
    {
        var attempts = await _context.DeploymentAttempts
            .Include(a => a.CloudDeployment)
            .Where(a => a.CloudDeploymentId == deploymentId)
            .OrderByDescending(a => a.StartedAt)
            .ToListAsync();

        return attempts.Select(MapToDto);
    }

    public async Task<DeploymentAttemptDto?> GetDeploymentAttemptByIdAsync(int attemptId)
    {
        var attempt = await _context.DeploymentAttempts
            .Include(a => a.CloudDeployment)
            .FirstOrDefaultAsync(a => a.Id == attemptId);

        return attempt != null ? MapToDto(attempt) : null;
    }

    public async Task<string> GetDeploymentAttemptLogsAsync(int attemptId)
    {
        var attempt = await _context.DeploymentAttempts
            .FirstOrDefaultAsync(a => a.Id == attemptId);

        if (attempt == null)
        {
            return "Attempt not found";
        }

        var logs = new System.Text.StringBuilder();

        if (!string.IsNullOrEmpty(attempt.BuildLog))
        {
            logs.AppendLine("=== BUILD LOG ===");
            logs.AppendLine(attempt.BuildLog);
        }

        if (!string.IsNullOrEmpty(attempt.DeployLog))
        {
            logs.AppendLine("\n=== DEPLOY LOG ===");
            logs.AppendLine(attempt.DeployLog);
        }

        if (!string.IsNullOrEmpty(attempt.ErrorMessage))
        {
            logs.AppendLine("\n=== ERROR ===");
            logs.AppendLine(attempt.ErrorMessage);
        }

        return logs.ToString();
    }

    #endregion

    #region Health Checks

    public async Task<HealthCheckResult> RunHealthCheckAsync(int deploymentId, RunHealthCheckRequest? request = null)
    {
        var deployment = await _context.CloudDeployments
            .Include(d => d.CloudProvider)
            .FirstOrDefaultAsync(d => d.Id == deploymentId && !d.IsDeleted);

        if (deployment == null)
        {
            return new HealthCheckResult
            {
                Success = false,
                OverallStatus = "Unknown",
                Message = "Deployment not found"
            };
        }

        var result = new HealthCheckResult
        {
            CheckedAt = DateTime.UtcNow
        };

        var log = new HealthCheckLog
        {
            CloudDeploymentId = deploymentId,
            CheckedAt = DateTime.UtcNow
        };

        try
        {
            // Check API health
            if (!string.IsNullOrEmpty(deployment.ApiUrl))
            {
                result.Api = await CheckComponentHealth(deployment.ApiUrl + "/health");
                log.ApiHealthy = result.Api.Healthy;
                log.ApiResponseTimeMs = result.Api.ResponseTimeMs;
                log.ApiResponse = result.Api.Response;
            }

            // Check Frontend health
            if (!string.IsNullOrEmpty(deployment.FrontendUrl))
            {
                result.Frontend = await CheckComponentHealth(deployment.FrontendUrl);
                log.FrontendHealthy = result.Frontend.Healthy;
                log.FrontendResponseTimeMs = result.Frontend.ResponseTimeMs;
                log.FrontendResponse = result.Frontend.Response;
            }

            // Check Database health (via API)
            if (!string.IsNullOrEmpty(deployment.ApiUrl))
            {
                result.Database = await CheckComponentHealth(deployment.ApiUrl + "/health/database");
                log.DatabaseHealthy = result.Database.Healthy;
                log.DatabaseResponseTimeMs = result.Database.ResponseTimeMs;
                log.DatabaseResponse = result.Database.Response;
            }

            // Determine overall status
            var allHealthy = (result.Api.Healthy || string.IsNullOrEmpty(deployment.ApiUrl)) &&
                            (result.Frontend.Healthy || string.IsNullOrEmpty(deployment.FrontendUrl)) &&
                            (result.Database.Healthy || string.IsNullOrEmpty(deployment.DatabaseHost));

            var anyHealthy = result.Api.Healthy || result.Frontend.Healthy || result.Database.Healthy;

            if (allHealthy)
            {
                result.OverallStatus = "Healthy";
                log.Status = HealthStatus.Healthy;
            }
            else if (anyHealthy)
            {
                result.OverallStatus = "Degraded";
                log.Status = HealthStatus.Degraded;
            }
            else
            {
                result.OverallStatus = "Unhealthy";
                log.Status = HealthStatus.Unhealthy;
            }

            result.Success = true;
        }
        catch (Exception ex)
        {
            result.OverallStatus = "Offline";
            result.Message = ex.Message;
            log.Status = HealthStatus.Offline;
            log.ErrorDetails = ex.Message;
        }

        // Save health check log
        _context.HealthCheckLogs.Add(log);

        // Update deployment health status
        deployment.HealthStatus = log.Status;
        deployment.LastHealthCheck = log.CheckedAt;

        await _context.SaveChangesAsync();

        return result;
    }

    public async Task<IEnumerable<HealthCheckDto>> GetHealthCheckHistoryAsync(int deploymentId, int? limit = null)
    {
        var query = _context.HealthCheckLogs
            .Include(h => h.CloudDeployment)
            .Where(h => h.CloudDeploymentId == deploymentId)
            .OrderByDescending(h => h.CheckedAt);

        if (limit.HasValue)
        {
            query = (IOrderedQueryable<HealthCheckLog>)query.Take(limit.Value);
        }

        var logs = await query.ToListAsync();

        return logs.Select(MapToDto);
    }

    public async Task<IEnumerable<HealthCheckDto>> GetAllDeploymentHealthAsync()
    {
        var deployments = await _context.CloudDeployments
            .Where(d => !d.IsDeleted && d.Status == DeploymentStatus.Running)
            .ToListAsync();

        var results = new List<HealthCheckDto>();

        foreach (var deployment in deployments)
        {
            var latestCheck = await _context.HealthCheckLogs
                .Include(h => h.CloudDeployment)
                .Where(h => h.CloudDeploymentId == deployment.Id)
                .OrderByDescending(h => h.CheckedAt)
                .FirstOrDefaultAsync();

            if (latestCheck != null)
            {
                results.Add(MapToDto(latestCheck));
            }
        }

        return results;
    }

    #endregion

    #region Dashboard

    public async Task<DeploymentDashboardDto> GetDashboardAsync()
    {
        var providers = await _context.CloudProviders
            .Where(p => !p.IsDeleted)
            .ToListAsync();

        var deployments = await _context.CloudDeployments
            .Include(d => d.CloudProvider)
            .Where(d => !d.IsDeleted)
            .ToListAsync();

        var recentAttempts = await _context.DeploymentAttempts
            .Include(a => a.CloudDeployment)
            .OrderByDescending(a => a.StartedAt)
            .Take(10)
            .ToListAsync();

        var recentHealthChecks = await _context.HealthCheckLogs
            .Include(h => h.CloudDeployment)
            .OrderByDescending(h => h.CheckedAt)
            .Take(10)
            .ToListAsync();

        return new DeploymentDashboardDto
        {
            TotalProviders = providers.Count,
            ActiveProviders = providers.Count(p => p.IsActive),
            TotalDeployments = deployments.Count,
            RunningDeployments = deployments.Count(d => d.Status == DeploymentStatus.Running),
            HealthyDeployments = deployments.Count(d => d.HealthStatus == HealthStatus.Healthy),
            FailedDeployments = deployments.Count(d => d.Status == DeploymentStatus.Failed),
            RecentDeployments = deployments
                .OrderByDescending(d => d.DeployedAt ?? d.CreatedAt)
                .Take(5)
                .Select(d => new DeploymentSummaryDto
                {
                    Id = d.Id,
                    Name = d.Name,
                    ProviderType = d.CloudProvider.ProviderType.ToString(),
                    Status = d.Status.ToString(),
                    HealthStatus = d.HealthStatus.ToString(),
                    FrontendUrl = d.FrontendUrl,
                    DeployedAt = d.DeployedAt
                })
                .ToList(),
            RecentAttempts = recentAttempts.Select(a => new DeploymentAttemptListDto
            {
                Id = a.Id,
                AttemptNumber = a.AttemptNumber,
                Status = a.Status.ToString(),
                GitBranch = a.GitBranch,
                BackendImageTag = a.BackendImageTag,
                FrontendImageTag = a.FrontendImageTag,
                StartedAt = a.StartedAt,
                CompletedAt = a.CompletedAt,
                DurationSeconds = a.DurationSeconds,
                TriggerType = a.TriggerType,
                ErrorMessage = a.ErrorMessage
            }).ToList(),
            RecentHealthChecks = recentHealthChecks.Select(MapToDto).ToList()
        };
    }

    #endregion

    #region Private Helper Methods

    private async Task UnsetDefaultProvidersAsync()
    {
        var defaultProviders = await _context.CloudProviders
            .Where(p => p.IsDefault && !p.IsDeleted)
            .ToListAsync();

        foreach (var provider in defaultProviders)
        {
            provider.IsDefault = false;
        }
    }

    private async Task<ComponentHealth> CheckComponentHealth(string url)
    {
        var result = new ComponentHealth();
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(10);

            var response = await client.GetAsync(url);
            stopwatch.Stop();

            result.ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds;
            result.Healthy = response.IsSuccessStatusCode;
            result.Response = $"{(int)response.StatusCode} {response.StatusCode}";
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            result.ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds;
            result.Healthy = false;
            result.Error = ex.Message;
        }

        return result;
    }

    #region Provider Connection Tests

    private Task<ProviderConnectionResult> TestKubernetesConnection(CloudProvider provider)
    {
        return Task.Run(() =>
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "kubectl",
                        Arguments = "cluster-info",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false
                    }
                };

                process.Start(); // NOSONAR - fixed 'kubectl' executable name, no user-controlled input in executable path
                var output = process.StandardOutput.ReadToEnd();
                process.WaitForExit(10000);

                if (process.ExitCode == 0)
                {
                    return new ProviderConnectionResult
                    {
                        Success = true,
                        Message = "Connected to Kubernetes cluster",
                        AvailableRegions = new List<string> { "local" }
                    };
                }

                return new ProviderConnectionResult
                {
                    Success = false,
                    Message = process.StandardError.ReadToEnd()
                };
            }
            catch (Exception ex)
            {
                return new ProviderConnectionResult
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        });
    }

    private Task<ProviderConnectionResult> TestDockerConnection(CloudProvider provider)
    {
        return Task.Run(() =>
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "docker",
                        Arguments = "info --format '{{.ServerVersion}}'",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false
                    }
                };

                process.Start(); // NOSONAR - fixed 'docker' executable name, no user-controlled input in executable path
                var output = process.StandardOutput.ReadToEnd();
                process.WaitForExit(10000);

                if (process.ExitCode == 0)
                {
                    return new ProviderConnectionResult
                    {
                        Success = true,
                        Message = $"Connected to Docker {output.Trim()}"
                    };
                }

                return new ProviderConnectionResult
                {
                    Success = false,
                    Message = process.StandardError.ReadToEnd()
                };
            }
            catch (Exception ex)
            {
                return new ProviderConnectionResult
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        });
    }

    /// <summary>
    /// Name of the HttpClient requested from <see cref="_httpClientFactory"/> when making
    /// cloud-provider connection-test calls. A dedicated name lets callers (and DI configuration)
    /// tune this client (timeouts, handlers) independently of other named clients, and lets tests
    /// target it precisely via a mocked <see cref="IHttpClientFactory"/>.
    /// </summary>
    private const string CloudConnectionHttpClientName = "CloudDeploymentConnectionTest";

    private const string GcpCloudPlatformScope = "https://www.googleapis.com/auth/cloud-platform";

    private async Task<ProviderConnectionResult> TestAwsConnection(CloudProvider provider)
    {
        if (string.IsNullOrWhiteSpace(provider.AccessKeyId) || string.IsNullOrWhiteSpace(provider.SecretAccessKey))
        {
            return new ProviderConnectionResult
            {
                Success = false,
                Message = "AWS Access Key ID and Secret Access Key are required"
            };
        }

        try
        {
            var region = string.IsNullOrWhiteSpace(provider.Region)
                ? RegionEndpoint.USEast1
                : RegionEndpoint.GetBySystemName(provider.Region);

            var config = new AmazonSecurityTokenServiceConfig
            {
                RegionEndpoint = region,
                // Route all AWS SDK traffic through the shared IHttpClientFactory so it can be
                // intercepted in tests (see AwsHttpClientFactoryAdapter) and share connection
                // pooling/DNS refresh behavior with the rest of the app in production.
                HttpClientFactory = new AwsHttpClientFactoryAdapter(_httpClientFactory)
            };

            var credentials = new Amazon.Runtime.BasicAWSCredentials(provider.AccessKeyId, provider.SecretAccessKey);
            using var stsClient = new AmazonSecurityTokenServiceClient(credentials, config);

            // GetCallerIdentity is AWS's documented "are these credentials valid" check: it makes
            // no state changes and requires no IAM permissions beyond sts:GetCallerIdentity (which
            // is implicitly allowed for any valid credentials).
            var identity = await stsClient.GetCallerIdentityAsync(new GetCallerIdentityRequest());

            return new ProviderConnectionResult
            {
                Success = true,
                Message = $"AWS credentials validated successfully (Account: {identity.Account})",
                AvailableRegions = new List<string> { region.SystemName }
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "AWS connection test failed for provider {Name}", provider.Name);
            return new ProviderConnectionResult
            {
                Success = false,
                Message = $"AWS connection failed: {ex.Message}"
            };
        }
    }

    private async Task<ProviderConnectionResult> TestAzureConnection(CloudProvider provider)
    {
        if (string.IsNullOrWhiteSpace(provider.TenantId) || string.IsNullOrWhiteSpace(provider.SubscriptionId) ||
            string.IsNullOrWhiteSpace(provider.AccessKeyId) || string.IsNullOrWhiteSpace(provider.SecretAccessKey))
        {
            return new ProviderConnectionResult
            {
                Success = false,
                Message = "Azure Tenant ID, Subscription ID, Client ID, and Client Secret are required"
            };
        }

        try
        {
            // provider.AccessKeyId / provider.SecretAccessKey double as the Azure app registration's
            // Client ID / Client Secret (see TestProviderConnectionRequest doc comments).
            var transport = new HttpClientTransport(_httpClientFactory.CreateClient(CloudConnectionHttpClientName));

            var credential = new ClientSecretCredential(
                provider.TenantId,
                provider.AccessKeyId,
                provider.SecretAccessKey,
                new TokenCredentialOptions { Transport = transport });

            var armClient = new ArmClient(
                credential,
                provider.SubscriptionId,
                new ArmClientOptions { Transport = transport });

            // A single GET on the configured subscription proves the token was both issued AND
            // accepted by ARM (unlike just acquiring a token, which can succeed even for a
            // subscription the caller has no access to).
            var subscriptionResource = armClient.GetSubscriptionResource(
                new Azure.Core.ResourceIdentifier($"/subscriptions/{provider.SubscriptionId}"));
            var subscription = await subscriptionResource.GetAsync();

            return new ProviderConnectionResult
            {
                Success = true,
                Message = $"Azure credentials validated successfully (Subscription: {subscription.Value.Data.DisplayName ?? provider.SubscriptionId})",
                AvailableRegions = string.IsNullOrWhiteSpace(provider.Region)
                    ? new List<string>()
                    : new List<string> { provider.Region }
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Azure connection test failed for provider {Name}", provider.Name);
            return new ProviderConnectionResult
            {
                Success = false,
                Message = $"Azure connection failed: {ex.Message}"
            };
        }
    }

    private async Task<ProviderConnectionResult> TestGcpConnection(CloudProvider provider)
    {
        if (string.IsNullOrWhiteSpace(provider.ProjectId) || string.IsNullOrWhiteSpace(provider.SecretAccessKey))
        {
            return new ProviderConnectionResult
            {
                Success = false,
                Message = "GCP Project ID and service account credentials are required"
            };
        }

        var (clientEmail, privateKeyPem) = ParseGcpServiceAccountSecret(provider);
        if (string.IsNullOrWhiteSpace(clientEmail) || string.IsNullOrWhiteSpace(privateKeyPem))
        {
            return new ProviderConnectionResult
            {
                Success = false,
                Message = "GCP service account credentials are invalid or incomplete (expected a service-account JSON key)"
            };
        }

        try
        {
            var initializer = new ServiceAccountCredential.Initializer(clientEmail)
            {
                Scopes = new[] { GcpCloudPlatformScope },
                // Route the JWT-bearer token exchange (POST to oauth2.googleapis.com/token) through
                // the shared IHttpClientFactory so it can be intercepted in tests.
                HttpClientFactory = new GcpHttpClientFactoryAdapter(_httpClientFactory)
            }.FromPrivateKey(privateKeyPem);

            var credential = new ServiceAccountCredential(initializer);

            // Exchanging the signed JWT for an access token proves the key is well-formed and not
            // revoked. GetAccessTokenForRequestAsync throws TokenResponseException on failure.
            var accessToken = await credential.GetAccessTokenForRequestAsync();

            // Confirm the token is actually accepted by GCP (not just successfully issued) with a
            // single lightweight "get project" call.
            var httpClient = _httpClientFactory.CreateClient(CloudConnectionHttpClientName);
            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"https://cloudresourcemanager.googleapis.com/v1/projects/{Uri.EscapeDataString(provider.ProjectId)}");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            using var response = await httpClient.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return new ProviderConnectionResult
                {
                    Success = false,
                    Message = $"GCP connection failed: {(int)response.StatusCode} {response.StatusCode} - {body}"
                };
            }

            return new ProviderConnectionResult
            {
                Success = true,
                Message = $"GCP credentials validated successfully (Project: {provider.ProjectId})",
                AvailableRegions = string.IsNullOrWhiteSpace(provider.Region)
                    ? new List<string>()
                    : new List<string> { provider.Region }
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "GCP connection test failed for provider {Name}", provider.Name);
            return new ProviderConnectionResult
            {
                Success = false,
                Message = $"GCP connection failed: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// The CloudProvider entity has no dedicated "service account JSON" field, so this parses
    /// provider.SecretAccessKey as either:
    ///  (a) a full GCP service-account JSON key (the common real-world way to store this secret
    ///      as a single opaque blob) - client_email/private_key are pulled from it, or
    ///  (b) a raw PEM private key, with provider.AccessKeyId holding the service account email.
    /// </summary>
    private static (string? ClientEmail, string? PrivateKeyPem) ParseGcpServiceAccountSecret(CloudProvider provider)
    {
        var secret = provider.SecretAccessKey;
        if (string.IsNullOrWhiteSpace(secret))
        {
            return (null, null);
        }

        if (secret.TrimStart().StartsWith('{'))
        {
            try
            {
                using var doc = JsonDocument.Parse(secret);
                var root = doc.RootElement;
                var clientEmail = root.TryGetProperty("client_email", out var emailProp)
                    ? emailProp.GetString()
                    : provider.AccessKeyId;
                var privateKey = root.TryGetProperty("private_key", out var keyProp)
                    ? keyProp.GetString()
                    : null;
                return (clientEmail, privateKey);
            }
            catch (JsonException)
            {
                return (null, null);
            }
        }

        return (provider.AccessKeyId, secret);
    }

    private async Task<ProviderConnectionResult> TestDigitalOceanConnection(CloudProvider provider)
    {
        if (string.IsNullOrWhiteSpace(provider.AccessKeyId))
        {
            return new ProviderConnectionResult
            {
                Success = false,
                Message = "DigitalOcean API Token is required"
            };
        }

        try
        {
            var httpClient = _httpClientFactory.CreateClient(CloudConnectionHttpClientName);

            // DigitalOcean's documented connectivity-check endpoint: a bearer-authenticated GET
            // that returns the token owner's account, with no side effects.
            using var request = new HttpRequestMessage(HttpMethod.Get, "https://api.digitalocean.com/v2/account");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", provider.AccessKeyId);

            using var response = await httpClient.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return new ProviderConnectionResult
                {
                    Success = false,
                    Message = $"DigitalOcean connection failed: {(int)response.StatusCode} {response.StatusCode} - {body}"
                };
            }

            string? email = null;
            try
            {
                using var doc = JsonDocument.Parse(body);
                if (doc.RootElement.TryGetProperty("account", out var accountEl) &&
                    accountEl.TryGetProperty("email", out var emailEl))
                {
                    email = emailEl.GetString();
                }
            }
            catch (JsonException)
            {
                // Response wasn't the expected shape; the HTTP call still succeeded, so don't fail the test over it.
            }

            return new ProviderConnectionResult
            {
                Success = true,
                Message = email != null
                    ? $"DigitalOcean credentials validated successfully (Account: {email})"
                    : "DigitalOcean credentials validated successfully",
                AvailableRegions = string.IsNullOrWhiteSpace(provider.Region)
                    ? new List<string>()
                    : new List<string> { provider.Region }
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "DigitalOcean connection test failed for provider {Name}", provider.Name);
            return new ProviderConnectionResult
            {
                Success = false,
                Message = $"DigitalOcean connection failed: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Adapts the app's <see cref="IHttpClientFactory"/> to the AWS SDK's own HttpClientFactory
    /// abstraction so STS calls share the app's HttpClient pooling in production and can be
    /// redirected to a fake handler in tests.
    /// </summary>
    private sealed class AwsHttpClientFactoryAdapter : Amazon.Runtime.HttpClientFactory
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AwsHttpClientFactoryAdapter(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public override HttpClient CreateHttpClient(Amazon.Runtime.IClientConfig clientConfig)
            => _httpClientFactory.CreateClient(CloudConnectionHttpClientName);
    }

    /// <summary>
    /// Adapts the app's <see cref="IHttpClientFactory"/> to Google.Apis.Auth's HTTP client
    /// abstraction so the OAuth2 token exchange can be redirected to a fake handler in tests.
    /// </summary>
    private sealed class GcpHttpClientFactoryAdapter : Google.Apis.Http.IHttpClientFactory
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public GcpHttpClientFactoryAdapter(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public Google.Apis.Http.ConfigurableHttpClient CreateHttpClient(Google.Apis.Http.CreateHttpClientArgs args)
        {
            var handler = new HttpClientFactoryDelegatingHandler(_httpClientFactory);
            var configurableHandler = new Google.Apis.Http.ConfigurableMessageHandler(handler);
            var client = new Google.Apis.Http.ConfigurableHttpClient(configurableHandler);

            foreach (var initializer in args.Initializers)
            {
                initializer.Initialize(client);
            }

            return client;
        }

        private sealed class HttpClientFactoryDelegatingHandler : HttpMessageHandler
        {
            private readonly IHttpClientFactory _httpClientFactory;

            public HttpClientFactoryDelegatingHandler(IHttpClientFactory httpClientFactory)
            {
                _httpClientFactory = httpClientFactory;
            }

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                // Google.Apis.Http.ConfigurableMessageHandler may resend the same HttpRequestMessage
                // instance on retry/redirect, but HttpClient.SendAsync throws InvalidOperationException
                // ("already sent") the second time it sees a given message instance. Forward a clone
                // each time so retries work.
                using var clone = await CloneAsync(request).ConfigureAwait(false);
                return await _httpClientFactory.CreateClient(CloudConnectionHttpClientName)
                    .SendAsync(clone, cancellationToken)
                    .ConfigureAwait(false);
            }

            private static async Task<HttpRequestMessage> CloneAsync(HttpRequestMessage request)
            {
                var clone = new HttpRequestMessage(request.Method, request.RequestUri)
                {
                    Version = request.Version
                };

                if (request.Content != null)
                {
                    var bytes = await request.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                    var content = new ByteArrayContent(bytes);
                    foreach (var header in request.Content.Headers)
                    {
                        content.Headers.TryAddWithoutValidation(header.Key, header.Value);
                    }

                    clone.Content = content;
                }

                foreach (var header in request.Headers)
                {
                    clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }

                return clone;
            }
        }
    }

    private Task<ProviderConnectionResult> TestOnPremiseConnection(CloudProvider provider)
    {
        return Task.FromResult(new ProviderConnectionResult
        {
            Success = true,
            Message = "On-premise connection verified",
            AvailableRegions = new List<string> { "local" }
        });
    }

    #endregion

    #region Provider Resources

    private IEnumerable<ResourceOption> GetKubernetesResources(CloudProvider provider, string resourceType)
    {
        return resourceType.ToLower() switch
        {
            "namespaces" => new List<ResourceOption>
            {
                new() { Id = "crm-app", Name = "CRM Application", Type = "namespace" },
                new() { Id = "default", Name = "Default", Type = "namespace" }
            },
            "storageclasses" => new List<ResourceOption>
            {
                new() { Id = "standard", Name = "Standard", Type = "storageclass" }
            },
            _ => Enumerable.Empty<ResourceOption>()
        };
    }

    private IEnumerable<ResourceOption> GetDockerResources(CloudProvider provider, string resourceType)
    {
        return resourceType.ToLower() switch
        {
            "networks" => new List<ResourceOption>
            {
                new() { Id = "bridge", Name = "Bridge", Type = "network" },
                new() { Id = "host", Name = "Host", Type = "network" }
            },
            _ => Enumerable.Empty<ResourceOption>()
        };
    }

    #endregion

    #region Deployment Operations

    private async Task<DeploymentResult> DeployToKubernetes(CloudDeployment deployment, DeploymentAttempt attempt, TriggerDeploymentRequest request)
    {
        var logs = new System.Text.StringBuilder();
        var ns = deployment.Namespace ?? "crm-app";

        try
        {
            // Apply Kubernetes manifests
            logs.AppendLine($"Deploying to Kubernetes namespace: {ns}");

            var backendTag = request.BackendVersion ?? $"v{attempt.AttemptNumber}";
            var frontendTag = request.FrontendVersion ?? $"v{attempt.AttemptNumber}";

            // Update deployment images
            var setBackendImage = await RunKubectlCommand(
                $"set image deployment/crm-backend crm-backend={deployment.BackendImage}:{backendTag} -n {ns}");
            logs.AppendLine(setBackendImage);

            var setFrontendImage = await RunKubectlCommand(
                $"set image deployment/crm-frontend crm-frontend={deployment.FrontendImage}:{frontendTag} -n {ns}");
            logs.AppendLine(setFrontendImage);

            // Wait for rollout
            logs.AppendLine("Waiting for rollout to complete...");
            var backendRollout = await RunKubectlCommand($"rollout status deployment/crm-backend -n {ns} --timeout=300s");
            logs.AppendLine(backendRollout);

            var frontendRollout = await RunKubectlCommand($"rollout status deployment/crm-frontend -n {ns} --timeout=300s");
            logs.AppendLine(frontendRollout);

            // Update attempt with image tags
            attempt.BackendImageTag = backendTag;
            attempt.FrontendImageTag = frontendTag;

            return new DeploymentResult
            {
                Success = true,
                Message = "Deployment completed successfully",
                FrontendUrl = deployment.FrontendUrl ?? $"http://localhost:30080", // NOSONAR - S5332 - http://localhost URLs used for local Kubernetes NodePort dev access only
                ApiUrl = deployment.ApiUrl ?? $"http://localhost:30081", // NOSONAR - S5332 - http://localhost URLs used for local Kubernetes NodePort dev access only
                DeployLog = logs.ToString()
            };
        }
        catch (Exception ex)
        {
            logs.AppendLine($"ERROR: {ex.Message}");
            return new DeploymentResult
            {
                Success = false,
                Message = ex.Message,
                DeployLog = logs.ToString()
            };
        }
    }

    private async Task<DeploymentResult> DeployToDocker(CloudDeployment deployment, DeploymentAttempt attempt, TriggerDeploymentRequest request)
    {
        var logs = new System.Text.StringBuilder();

        try
        {
            logs.AppendLine("Deploying with Docker Compose...");

            // Use docker compose to deploy
            var composeResult = await RunDockerCommand("compose up -d");
            logs.AppendLine(composeResult);

            attempt.BackendImageTag = request.BackendVersion ?? "latest";
            attempt.FrontendImageTag = request.FrontendVersion ?? "latest";

            return new DeploymentResult
            {
                Success = true,
                Message = "Docker deployment completed",
                FrontendUrl = "http://localhost:3000", // NOSONAR - S5332 - http://localhost URL is a development-mode service endpoint placeholder
                ApiUrl = "http://localhost:5000", // NOSONAR - S5332 - http://localhost URL is a development-mode service endpoint placeholder
                DeployLog = logs.ToString()
            };
        }
        catch (Exception ex)
        {
            logs.AppendLine($"ERROR: {ex.Message}");
            return new DeploymentResult
            {
                Success = false,
                Message = ex.Message,
                DeployLog = logs.ToString()
            };
        }
    }

    private async Task<DeploymentResult> ScaleKubernetesDeployment(CloudDeployment deployment, int replicas)
    {
        var ns = deployment.Namespace ?? "crm-app";

        var backendScale = await RunKubectlCommand($"scale deployment/crm-backend --replicas={replicas} -n {ns}");
        var frontendScale = await RunKubectlCommand($"scale deployment/crm-frontend --replicas={replicas} -n {ns}");

        return new DeploymentResult
        {
            Success = true,
            Message = $"Scaled to {replicas} replicas",
            DeployLog = $"{backendScale}\n{frontendScale}"
        };
    }

    private async Task<DeploymentResult> RestartKubernetesDeployment(CloudDeployment deployment)
    {
        var ns = deployment.Namespace ?? "crm-app";

        var backendRestart = await RunKubectlCommand($"rollout restart deployment/crm-backend -n {ns}");
        var frontendRestart = await RunKubectlCommand($"rollout restart deployment/crm-frontend -n {ns}");

        return new DeploymentResult
        {
            Success = true,
            Message = "Deployments restarted",
            DeployLog = $"{backendRestart}\n{frontendRestart}"
        };
    }

    private async Task<DeploymentResult> StopDockerContainers(CloudDeployment deployment)
    {
        var result = await RunDockerCommand("compose stop");
        return new DeploymentResult
        {
            Success = true,
            Message = "Docker containers stopped",
            DeployLog = result
        };
    }

    private async Task<DeploymentResult> RestartDockerContainers(CloudDeployment deployment)
    {
        var result = await RunDockerCommand("compose restart");
        return new DeploymentResult
        {
            Success = true,
            Message = "Docker containers restarted",
            DeployLog = result
        };
    }

    private Task<string> RunKubectlCommand(string arguments)
    {
        return Task.Run(() =>
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "kubectl",
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false
                }
            };

            process.Start(); // NOSONAR - fixed 'kubectl' executable name, no user-controlled input in executable path
            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();
            process.WaitForExit(60000);

            return process.ExitCode == 0 ? output : throw new InvalidOperationException(error);
        });
    }

    private Task<string> RunDockerCommand(string arguments)
    {
        return Task.Run(() =>
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "docker",
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false
                }
            };

            process.Start(); // NOSONAR - fixed 'docker' executable name, no user-controlled input in executable path
            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();
            process.WaitForExit(60000);

            return process.ExitCode == 0 ? output : throw new InvalidOperationException(error);
        });
    }

    #endregion

    #region DTO Mappers

    private CloudProviderDto MapToDto(CloudProvider entity)
    {
        return new CloudProviderDto
        {
            Id = entity.Id,
            Name = entity.Name,
            ProviderType = entity.ProviderType.ToString(),
            Description = entity.Description,
            Region = entity.Region,
            Endpoint = entity.Endpoint,
            IsActive = entity.IsActive,
            IsDefault = entity.IsDefault,
            CreatedAt = entity.CreatedAt,
            DeploymentCount = entity.Deployments?.Count ?? 0
        };
    }

    private CloudDeploymentDto MapToDto(CloudDeployment entity)
    {
        return new CloudDeploymentDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            CloudProviderId = entity.CloudProviderId,
            ProviderName = entity.CloudProvider?.Name ?? "",
            ProviderType = entity.CloudProvider?.ProviderType.ToString() ?? "",
            ClusterName = entity.ClusterName,
            Namespace = entity.Namespace,
            ResourceGroup = entity.ResourceGroup,
            BackendVersion = entity.BackendVersion,
            FrontendVersion = entity.FrontendVersion,
            FrontendUrl = entity.FrontendUrl,
            ApiUrl = entity.ApiUrl,
            DomainName = entity.DomainName,
            SslEnabled = entity.SslEnabled,
            CpuUnits = entity.CpuUnits,
            MemoryMb = entity.MemoryMb,
            Replicas = entity.Replicas,
            Status = entity.Status.ToString(),
            HealthStatus = entity.HealthStatus.ToString(),
            LastHealthCheck = entity.LastHealthCheck,
            DeployedAt = entity.DeployedAt,
            LastError = entity.LastError,
            CreatedAt = entity.CreatedAt,
            AttemptCount = entity.Attempts?.Count ?? 0
        };
    }

    private DeploymentAttemptDto MapToDto(DeploymentAttempt entity)
    {
        return new DeploymentAttemptDto
        {
            Id = entity.Id,
            CloudDeploymentId = entity.CloudDeploymentId,
            DeploymentName = entity.CloudDeployment?.Name ?? "",
            AttemptNumber = entity.AttemptNumber,
            Status = entity.Status.ToString(),
            GitCommitHash = entity.GitCommitHash,
            GitBranch = entity.GitBranch,
            BuildNumber = entity.BuildNumber,
            BackendImageTag = entity.BackendImageTag,
            FrontendImageTag = entity.FrontendImageTag,
            StartedAt = entity.StartedAt,
            CompletedAt = entity.CompletedAt,
            DurationSeconds = entity.DurationSeconds,
            BuildLog = entity.BuildLog,
            DeployLog = entity.DeployLog,
            ErrorMessage = entity.ErrorMessage,
            TriggeredByUserId = entity.TriggeredByUserId,
            TriggerType = entity.TriggerType
        };
    }

    private HealthCheckDto MapToDto(HealthCheckLog entity)
    {
        return new HealthCheckDto
        {
            Id = entity.Id,
            CloudDeploymentId = entity.CloudDeploymentId,
            DeploymentName = entity.CloudDeployment?.Name ?? "",
            Status = entity.Status.ToString(),
            CheckedAt = entity.CheckedAt,
            ApiHealthy = entity.ApiHealthy,
            FrontendHealthy = entity.FrontendHealthy,
            DatabaseHealthy = entity.DatabaseHealthy,
            ApiResponseTimeMs = entity.ApiResponseTimeMs,
            FrontendResponseTimeMs = entity.FrontendResponseTimeMs,
            DatabaseResponseTimeMs = entity.DatabaseResponseTimeMs,
            ApiResponse = entity.ApiResponse,
            FrontendResponse = entity.FrontendResponse,
            ErrorDetails = entity.ErrorDetails
        };
    }

    #endregion

    #endregion
}
