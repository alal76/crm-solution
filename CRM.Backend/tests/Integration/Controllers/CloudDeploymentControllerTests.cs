// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Tests.Helpers;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace CRM.Backend.Tests.Integration.Controllers
{
    public class CloudDeploymentControllerTests : IClassFixture<ApiTestFactory>
    {
        private readonly HttpClient _client;
        public CloudDeploymentControllerTests(ApiTestFactory factory) => _client = factory.CreateClient();

        [Fact]
        public async Task Crud_CloudDeployment_Succeeds()
        {
            var create = new
            {
                Name = "Test",
                ProviderType = "Test",
                Description = "Test",
                Region = "Test",
                Endpoint = "Test",
                IsActive = true,
                IsDefault = true,
                DeploymentCount = 1,
                AccessKeyId = "Test",
                SecretAccessKey = "Test",
                TenantId = "Test",
                SubscriptionId = "Test",
                ProjectId = "Test",
                ProviderId = 1,
                Success = true,
                Message = "Test",
                Type = "Test",
                Status = "Test",
                CloudProviderId = 1,
                ProviderName = "Test",
                ClusterName = "Test",
                Namespace = "Test",
                ResourceGroup = "Test",
                BackendVersion = "Test",
                FrontendVersion = "Test",
                FrontendUrl = "Test",
                ApiUrl = "Test",
                DomainName = "Test",
                SslEnabled = true,
                CpuUnits = 1,
                MemoryMb = 1,
                Replicas = 1,
                HealthStatus = "Test",
                LastHealthCheck = DateTime.UtcNow,
                DeployedAt = DateTime.UtcNow,
                LastError = "Test",
                AttemptCount = 1,
                VpcId = "Test",
                BackendImage = "Test",
                FrontendImage = "Test",
                DatabaseImage = "Test",
                DeploymentId = 1,
                GitBranch = "Test",
                GitCommitHash = "Test",
                ForceBuild = true,
                TriggeredByUserId = 1,
                AttemptId = 1,
                BuildLog = "Test",
                DeployLog = "Test",
                CloudDeploymentId = 1,
                DeploymentName = "Test",
                AttemptNumber = "Test",
                BuildNumber = "Test",
                BackendImageTag = "Test",
                FrontendImageTag = "Test",
                StartedAt = DateTime.UtcNow,
                CompletedAt = DateTime.UtcNow,
                DurationSeconds = 1,
                ErrorMessage = "Test",
                TriggeredByUser = "Test",
                TriggerType = "Test",
                CheckedAt = DateTime.UtcNow,
                ApiHealthy = true,
                FrontendHealthy = true,
                DatabaseHealthy = true,
                ApiResponseTimeMs = 1,
                FrontendResponseTimeMs = 1,
                DatabaseResponseTimeMs = 1,
                ApiResponse = "Test",
                FrontendResponse = "Test",
                ErrorDetails = "Test",
                OverallStatus = "Test",
                Api = (object?)null,
                Frontend = (object?)null,
                Database = (object?)null,
                Healthy = true,
                ResponseTimeMs = 1,
                Response = "Test",
                Error = "Test",
                TotalProviders = 1,
                ActiveProviders = 1,
                TotalDeployments = 1,
                RunningDeployments = 1,
                HealthyDeployments = 1,
                FailedDeployments = 1
            };
            var cRes = await _client.PostAsJsonAsync("/api/clouddeployment", create);
            cRes.StatusCode.Should().Be(HttpStatusCode.Created);
            var item = await cRes.Content.ReadFromJsonAsync<JsonElement>();
            var id = item.GetProperty("id").GetInt32();

            var getRes = await _client.GetAsync($"/api/clouddeployment/{id}");
            getRes.StatusCode.Should().Be(HttpStatusCode.OK);
            var patch = new
            {
                Name = "Test2",
                ProviderType = "Test",
                Description = "Test",
                Region = "Test",
                Endpoint = "Test",
                IsActive = true,
                IsDefault = true,
                DeploymentCount = 1,
                AccessKeyId = "Test",
                SecretAccessKey = "Test",
                TenantId = "Test",
                SubscriptionId = "Test",
                ProjectId = "Test",
                ProviderId = 1,
                Success = true,
                Message = "Test",
                Type = "Test",
                Status = "Test",
                CloudProviderId = 1,
                ProviderName = "Test",
                ClusterName = "Test",
                Namespace = "Test",
                ResourceGroup = "Test",
                BackendVersion = "Test",
                FrontendVersion = "Test",
                FrontendUrl = "Test",
                ApiUrl = "Test",
                DomainName = "Test",
                SslEnabled = true,
                CpuUnits = 1,
                MemoryMb = 1,
                Replicas = 1,
                HealthStatus = "Test",
                LastHealthCheck = DateTime.UtcNow,
                DeployedAt = DateTime.UtcNow,
                LastError = "Test",
                AttemptCount = 1,
                VpcId = "Test",
                BackendImage = "Test",
                FrontendImage = "Test",
                DatabaseImage = "Test",
                DeploymentId = 1,
                GitBranch = "Test",
                GitCommitHash = "Test",
                ForceBuild = true,
                TriggeredByUserId = 1,
                AttemptId = 1,
                BuildLog = "Test",
                DeployLog = "Test",
                CloudDeploymentId = 1,
                DeploymentName = "Test",
                AttemptNumber = "Test",
                BuildNumber = "Test",
                BackendImageTag = "Test",
                FrontendImageTag = "Test",
                StartedAt = DateTime.UtcNow,
                CompletedAt = DateTime.UtcNow,
                DurationSeconds = 1,
                ErrorMessage = "Test",
                TriggeredByUser = "Test",
                TriggerType = "Test",
                CheckedAt = DateTime.UtcNow,
                ApiHealthy = true,
                FrontendHealthy = true,
                DatabaseHealthy = true,
                ApiResponseTimeMs = 1,
                FrontendResponseTimeMs = 1,
                DatabaseResponseTimeMs = 1,
                ApiResponse = "Test",
                FrontendResponse = "Test",
                ErrorDetails = "Test",
                OverallStatus = "Test",
                Api = (object?)null,
                Frontend = (object?)null,
                Database = (object?)null,
                Healthy = true,
                ResponseTimeMs = 1,
                Response = "Test",
                Error = "Test",
                TotalProviders = 1,
                ActiveProviders = 1,
                TotalDeployments = 1,
                RunningDeployments = 1,
                HealthyDeployments = 1,
                FailedDeployments = 1
            };
            var pRes = await _client.PatchAsJsonAsync($"/api/clouddeployment/{id}", patch);
            pRes.StatusCode.Should().Be(HttpStatusCode.OK);
            var del = await _client.DeleteAsync($"/api/clouddeployment/{id}");
            del.StatusCode.Should().Be(HttpStatusCode.NoContent);
            var nf = await _client.GetAsync($"/api/clouddeployment/{id}");
            nf.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Get_Nonexistent_Returns404()
        {
            var res = await _client.GetAsync("/api/clouddeployment/999999");
            Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
        }
    }
}
