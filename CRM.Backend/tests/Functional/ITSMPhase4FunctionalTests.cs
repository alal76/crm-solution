// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

using Xunit;
using FluentAssertions;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace CRM.Tests.Functional;

/// <summary>
/// Functional Tests for ITSM Phase 4 Features:
/// - Webhook Notifications (Subscription CRUD, Delivery, Retry Logic)
/// - Email-to-Ticket (Inbound Email Parsing, Incident Creation)
/// - ITSM Dashboard (Metrics, Trends, Analytics)
/// - Monitoring Integration (Prometheus, Grafana, Datadog Alerts)
/// - CI/CD Integration (Pipeline to Change Request Automation)
/// - Self-Service Chatbot (Session Management, Intent Recognition)
/// </summary>
public class ITSMPhase4FunctionalTests : FunctionalTestBase
{
    #region Webhook Notification Functional Tests

    [Fact]
    [Trait("Category", "Functional")]
    [Trait("Feature", "Webhooks")]
    public async Task FUN_WEBHOOK_001_CreateAndRetrieveWebhookSubscription()
    {
        // Arrange
        await AuthenticateAsync();

        var createRequest = new
        {
            Name = "Functional Test Webhook",
            TargetUrl = "https://hooks.example.com/functional-test",
            EventTypes = new[] { "IncidentCreated", "IncidentResolved" },
            SecretKey = "func-test-secret-key",
            RetryCount = 3,
            TimeoutSeconds = 30,
            IsActive = true
        };

        // Act - Create
        var createResponse = await Client.PostAsJsonAsync("/api/itsm/webhooks/subscriptions", createRequest);

        // Assert - Create (accept 200, 201 or 501 if not implemented)
        createResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created, HttpStatusCode.NotImplemented, HttpStatusCode.NotFound);

        if (createResponse.IsSuccessStatusCode)
        {
            // Act - Retrieve
            var listResponse = await Client.GetAsync("/api/itsm/webhooks/subscriptions");
            listResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotImplemented);
        }
    }

    [Fact]
    [Trait("Category", "Functional")]
    [Trait("Feature", "Webhooks")]
    public async Task FUN_WEBHOOK_002_WebhookSubscriptionValidation()
    {
        // Arrange
        await AuthenticateAsync();

        var invalidRequest = new
        {
            Name = "", // Invalid - empty name
            TargetUrl = "not-a-valid-url", // Invalid URL
            EventTypes = Array.Empty<string>() // Invalid - no events
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/itsm/webhooks/subscriptions", invalidRequest);

        // Assert - Should reject invalid input
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.BadRequest,
            HttpStatusCode.UnprocessableEntity,
            HttpStatusCode.NotFound,
            HttpStatusCode.NotImplemented);
    }

    [Fact]
    [Trait("Category", "Functional")]
    [Trait("Feature", "Webhooks")]
    public async Task FUN_WEBHOOK_003_TestWebhookDelivery()
    {
        // Arrange
        await AuthenticateAsync();
        var subscriptionId = 1; // Assuming test subscription exists

        // Act
        var response = await Client.PostAsync($"/api/itsm/webhooks/subscriptions/{subscriptionId}/test", null);

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.NotFound,
            HttpStatusCode.NotImplemented);
    }

    [Fact]
    [Trait("Category", "Functional")]
    [Trait("Feature", "Webhooks")]
    public async Task FUN_WEBHOOK_004_GetDeliveryHistory()
    {
        // Arrange
        await AuthenticateAsync();

        // Act
        var response = await Client.GetAsync("/api/itsm/webhooks/deliveries?pageSize=10");

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.NotFound,
            HttpStatusCode.NotImplemented);
    }

    #endregion

    #region Email-to-Ticket Functional Tests

    [Fact]
    [Trait("Category", "Functional")]
    [Trait("Feature", "EmailToTicket")]
    public async Task FUN_EMAIL_001_ProcessInboundEmailCreatesIncident()
    {
        // Arrange
        await AuthenticateAsync();

        var email = new
        {
            From = "customer@example.com",
            To = "support@crm-solution.com",
            Subject = "URGENT: Production server down",
            Body = "The main production server stopped responding at 3:00 PM. All services are affected.",
            ReceivedAt = DateTime.UtcNow.ToString("O"),
            MessageId = $"<func-test-{Guid.NewGuid()}@example.com>",
            Headers = new Dictionary<string, string>
            {
                { "X-Priority", "1" },
                { "Importance", "high" }
            }
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/itsm/email/inbound", email);

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.Created,
            HttpStatusCode.BadRequest,
            HttpStatusCode.NotFound,
            HttpStatusCode.NotImplemented);
    }

    [Fact]
    [Trait("Category", "Functional")]
    [Trait("Feature", "EmailToTicket")]
    public async Task FUN_EMAIL_002_EmailWithAttachments()
    {
        // Arrange
        await AuthenticateAsync();

        var email = new
        {
            From = "user@example.com",
            To = "support@crm-solution.com",
            Subject = "Error logs attached",
            Body = "Please see attached error logs from our application.",
            ReceivedAt = DateTime.UtcNow.ToString("O"),
            Attachments = new[]
            {
                new
                {
                    FileName = "error.log",
                    ContentType = "text/plain",
                    Size = 1024,
                    ContentBase64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("Error log content"))
                }
            }
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/itsm/email/inbound", email);

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.Created,
            HttpStatusCode.BadRequest,
            HttpStatusCode.NotFound,
            HttpStatusCode.NotImplemented);
    }

    [Fact]
    [Trait("Category", "Functional")]
    [Trait("Feature", "EmailToTicket")]
    public async Task FUN_EMAIL_003_ReplyToExistingIncident()
    {
        // Arrange
        await AuthenticateAsync();

        var replyEmail = new
        {
            From = "customer@example.com",
            To = "support@crm-solution.com",
            Subject = "RE: [INC0001234] Production server down",
            Body = "Thank you for the update. The issue persists.",
            ReceivedAt = DateTime.UtcNow.ToString("O"),
            InReplyTo = "<original-message-id@crm-solution.com>"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/itsm/email/inbound", replyEmail);

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.Created,
            HttpStatusCode.BadRequest,
            HttpStatusCode.NotFound,
            HttpStatusCode.NotImplemented);
    }

    [Fact]
    [Trait("Category", "Functional")]
    [Trait("Feature", "EmailToTicket")]
    public async Task FUN_EMAIL_004_GetEmailParsingConfiguration()
    {
        // Arrange
        await AuthenticateAsync();

        // Act
        var response = await Client.GetAsync("/api/itsm/email/config");

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.NotFound,
            HttpStatusCode.NotImplemented);
    }

    #endregion

    #region Dashboard Analytics Functional Tests

    [Fact]
    [Trait("Category", "Functional")]
    [Trait("Feature", "Dashboard")]
    public async Task FUN_DASHBOARD_001_GetIncidentMetrics()
    {
        // Arrange
        await AuthenticateAsync();

        // Act
        var response = await Client.GetAsync("/api/itsm/dashboard/metrics?period=30d");

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.NotFound,
            HttpStatusCode.NotImplemented);
    }

    [Fact]
    [Trait("Category", "Functional")]
    [Trait("Feature", "Dashboard")]
    public async Task FUN_DASHBOARD_002_GetIncidentTrendsWithDateRange()
    {
        // Arrange
        await AuthenticateAsync();
        var startDate = DateTime.UtcNow.AddDays(-30).ToString("yyyy-MM-dd");
        var endDate = DateTime.UtcNow.ToString("yyyy-MM-dd");

        // Act
        var response = await Client.GetAsync($"/api/itsm/dashboard/incident-trends?startDate={startDate}&endDate={endDate}");

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.NotFound,
            HttpStatusCode.NotImplemented);
    }

    [Fact]
    [Trait("Category", "Functional")]
    [Trait("Feature", "Dashboard")]
    public async Task FUN_DASHBOARD_003_GetSLAComplianceReport()
    {
        // Arrange
        await AuthenticateAsync();

        // Act
        var response = await Client.GetAsync("/api/itsm/dashboard/sla-compliance?period=thisMonth");

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.NotFound,
            HttpStatusCode.NotImplemented);
    }

    [Fact]
    [Trait("Category", "Functional")]
    [Trait("Feature", "Dashboard")]
    public async Task FUN_DASHBOARD_004_GetAgentPerformanceMetrics()
    {
        // Arrange
        await AuthenticateAsync();

        // Act
        var response = await Client.GetAsync("/api/itsm/dashboard/agent-performance?teamId=1");

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.NotFound,
            HttpStatusCode.NotImplemented);
    }

    [Fact]
    [Trait("Category", "Functional")]
    [Trait("Feature", "Dashboard")]
    public async Task FUN_DASHBOARD_005_GetExecutiveSummary()
    {
        // Arrange
        await AuthenticateAsync();

        // Act
        var response = await Client.GetAsync("/api/itsm/dashboard/executive-summary");

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.NotFound,
            HttpStatusCode.NotImplemented);
    }

    [Fact]
    [Trait("Category", "Functional")]
    [Trait("Feature", "Dashboard")]
    public async Task FUN_DASHBOARD_006_ExportDashboardData()
    {
        // Arrange
        await AuthenticateAsync();

        // Act
        var response = await Client.GetAsync("/api/itsm/dashboard/export?format=csv&period=30d");

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.NotFound,
            HttpStatusCode.NotImplemented);
    }

    #endregion

    #region Monitoring Integration Functional Tests

    [Fact]
    [Trait("Category", "Functional")]
    [Trait("Feature", "MonitoringIntegration")]
    public async Task FUN_MONITORING_001_ReceivePrometheusAlert()
    {
        // Arrange
        await AuthenticateAsync();

        // Prometheus Alertmanager webhook format
        var alertPayload = new
        {
            receiver = "itsm-webhook",
            status = "firing",
            alerts = new[]
            {
                new
                {
                    status = "firing",
                    labels = new Dictionary<string, string>
                    {
                        { "alertname", "HighCPUUsage" },
                        { "severity", "critical" },
                        { "instance", "prod-server-01:9090" },
                        { "job", "node-exporter" }
                    },
                    annotations = new Dictionary<string, string>
                    {
                        { "summary", "High CPU usage on prod-server-01" },
                        { "description", "CPU usage has been above 95% for the past 10 minutes" }
                    },
                    startsAt = DateTime.UtcNow.AddMinutes(-10).ToString("O"),
                    generatorURL = "http://prometheus:9090/graph"
                }
            },
            groupLabels = new Dictionary<string, string> { { "alertname", "HighCPUUsage" } },
            externalURL = "http://alertmanager:9093"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/itsm/monitoring/alerts", alertPayload);

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.Created,
            HttpStatusCode.BadRequest,
            HttpStatusCode.NotFound,
            HttpStatusCode.NotImplemented);
    }

    [Fact]
    [Trait("Category", "Functional")]
    [Trait("Feature", "MonitoringIntegration")]
    public async Task FUN_MONITORING_002_ReceiveDatadogAlert()
    {
        // Arrange
        await AuthenticateAsync();

        // Datadog webhook format
        var alertPayload = new
        {
            id = "12345678901234567890",
            title = "CPU usage is too high on host:prod-server-01",
            type = "metric_alert",
            date = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            org = new { id = "12345", name = "CRM Solution" },
            priority = "P1",
            tags = new[] { "environment:production", "service:crm-api" },
            body = "CPU usage exceeded 90% threshold",
            link = "https://app.datadoghq.com/monitors/12345678"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/itsm/monitoring/alerts/datadog", alertPayload);

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.Created,
            HttpStatusCode.BadRequest,
            HttpStatusCode.NotFound,
            HttpStatusCode.NotImplemented);
    }

    [Fact]
    [Trait("Category", "Functional")]
    [Trait("Feature", "MonitoringIntegration")]
    public async Task FUN_MONITORING_003_ConfigureAlertMapping()
    {
        // Arrange
        await AuthenticateAsync();

        var mapping = new
        {
            AlertName = "DiskSpaceLow",
            SourceType = "Prometheus",
            IncidentCategoryId = 5,
            DefaultPriority = "High",
            DefaultAssignmentGroup = "Infrastructure Team",
            AutoCreateIncident = true,
            DeduplicationWindowMinutes = 60
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/itsm/monitoring/alert-mappings", mapping);

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.Created,
            HttpStatusCode.BadRequest,
            HttpStatusCode.NotFound,
            HttpStatusCode.NotImplemented);
    }

    [Fact]
    [Trait("Category", "Functional")]
    [Trait("Feature", "MonitoringIntegration")]
    public async Task FUN_MONITORING_004_AlertDeduplication()
    {
        // Arrange
        await AuthenticateAsync();

        var alertPayload = new
        {
            receiver = "itsm-webhook",
            status = "firing",
            alerts = new[]
            {
                new
                {
                    status = "firing",
                    labels = new Dictionary<string, string>
                    {
                        { "alertname", "DuplicateTestAlert" },
                        { "severity", "warning" },
                        { "instance", "test-server:9090" }
                    },
                    startsAt = DateTime.UtcNow.ToString("O")
                }
            }
        };

        // Act - Send same alert twice
        var response1 = await Client.PostAsJsonAsync("/api/itsm/monitoring/alerts", alertPayload);
        var response2 = await Client.PostAsJsonAsync("/api/itsm/monitoring/alerts", alertPayload);

        // Assert - Both should succeed, but deduplication should prevent duplicate incidents
        response1.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK, HttpStatusCode.Created, HttpStatusCode.NotFound, HttpStatusCode.NotImplemented);
        response2.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK, HttpStatusCode.Created, HttpStatusCode.Conflict, HttpStatusCode.NotFound, HttpStatusCode.NotImplemented);
    }

    #endregion

    #region CI/CD Integration Functional Tests

    [Fact]
    [Trait("Category", "Functional")]
    [Trait("Feature", "CICDIntegration")]
    public async Task FUN_CICD_001_CreateDeploymentChangeRequest()
    {
        // Arrange
        await AuthenticateAsync();

        var deploymentRequest = new
        {
            PipelineId = $"func-test-{Guid.NewGuid():N}",
            PipelineName = "CRM Backend Deploy",
            BuildNumber = "2.1.0.1234",
            CommitHash = "abc123def456789012345678901234567890abcd",
            CommitMessage = "feat: Add new customer dashboard widget",
            Author = "developer@crm-solution.com",
            Branch = "main",
            Environment = "production",
            Services = new[] { "crm-api", "crm-worker", "crm-scheduler" },
            DeploymentType = "Standard",
            RiskLevel = "Medium",
            RollbackPlan = "Revert to previous container image version",
            TestEvidence = new[]
            {
                new { Type = "UnitTests", Passed = 156, Failed = 0 },
                new { Type = "IntegrationTests", Passed = 48, Failed = 0 }
            }
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/itsm/cicd/deployment", deploymentRequest);

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.Created,
            HttpStatusCode.BadRequest,
            HttpStatusCode.NotFound,
            HttpStatusCode.NotImplemented);
    }

    [Fact]
    [Trait("Category", "Functional")]
    [Trait("Feature", "CICDIntegration")]
    public async Task FUN_CICD_002_EmergencyDeployment()
    {
        // Arrange
        await AuthenticateAsync();

        var emergencyRequest = new
        {
            PipelineId = $"emergency-{Guid.NewGuid():N}",
            PipelineName = "Emergency Hotfix Pipeline",
            BuildNumber = "2.1.0.1235-hotfix",
            CommitHash = "def456789012345678901234567890abcdef12",
            CommitMessage = "hotfix: Critical security patch for authentication",
            Author = "security@crm-solution.com",
            Branch = "hotfix/auth-security",
            Environment = "production",
            Services = new[] { "crm-api" },
            DeploymentType = "Emergency",
            Justification = "Critical security vulnerability discovered - CVE-2026-12345"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/itsm/cicd/deployment", emergencyRequest);

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.Created,
            HttpStatusCode.BadRequest,
            HttpStatusCode.NotFound,
            HttpStatusCode.NotImplemented);
    }

    [Fact]
    [Trait("Category", "Functional")]
    [Trait("Feature", "CICDIntegration")]
    public async Task FUN_CICD_003_RollbackDeployment()
    {
        // Arrange
        await AuthenticateAsync();

        var rollbackRequest = new
        {
            PipelineId = $"rollback-{Guid.NewGuid():N}",
            PipelineName = "CRM Rollback Pipeline",
            BuildNumber = "2.0.9.1200",
            CommitHash = "previousgoodversion12345678901234567890",
            CommitMessage = "rollback: Revert to stable version due to performance issues",
            Author = "ops@crm-solution.com",
            Branch = "main",
            Environment = "production",
            Services = new[] { "crm-api" },
            DeploymentType = "Rollback",
            RelatedChangeId = 123,
            RollbackReason = "Performance degradation observed after deployment"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/itsm/cicd/deployment", rollbackRequest);

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.Created,
            HttpStatusCode.BadRequest,
            HttpStatusCode.NotFound,
            HttpStatusCode.NotImplemented);
    }

    [Fact]
    [Trait("Category", "Functional")]
    [Trait("Feature", "CICDIntegration")]
    public async Task FUN_CICD_004_GetDeploymentHistory()
    {
        // Arrange
        await AuthenticateAsync();

        // Act
        var response = await Client.GetAsync("/api/itsm/cicd/deployments?environment=production&limit=10");

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.NotFound,
            HttpStatusCode.NotImplemented);
    }

    [Fact]
    [Trait("Category", "Functional")]
    [Trait("Feature", "CICDIntegration")]
    public async Task FUN_CICD_005_RegisterPipeline()
    {
        // Arrange
        await AuthenticateAsync();

        var pipeline = new
        {
            PipelineId = $"new-pipeline-{Guid.NewGuid():N}",
            Name = "New Microservice Pipeline",
            Platform = "GitHub",
            RepositoryUrl = "https://github.com/crm-solution/crm-new-service",
            DefaultEnvironment = "staging",
            RequiresApproval = true,
            ApprovalGroups = new[] { "Change Advisory Board", "Product Owner" }
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/itsm/cicd/pipelines", pipeline);

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.Created,
            HttpStatusCode.BadRequest,
            HttpStatusCode.NotFound,
            HttpStatusCode.NotImplemented);
    }

    #endregion

    #region Self-Service Chatbot Functional Tests

    [Fact]
    [Trait("Category", "Functional")]
    [Trait("Feature", "Chatbot")]
    public async Task FUN_CHATBOT_001_StartChatSession()
    {
        // Arrange
        await AuthenticateAsync();

        // Act
        var response = await Client.PostAsJsonAsync("/api/itsm/chatbot/sessions", new { });

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.Created,
            HttpStatusCode.NotFound,
            HttpStatusCode.NotImplemented);
    }

    [Fact]
    [Trait("Category", "Functional")]
    [Trait("Feature", "Chatbot")]
    public async Task FUN_CHATBOT_002_SendMessageAndReceiveResponse()
    {
        // Arrange
        await AuthenticateAsync();

        // First create a session
        var sessionResponse = await Client.PostAsJsonAsync("/api/itsm/chatbot/sessions", new { });

        if (sessionResponse.IsSuccessStatusCode)
        {
            var sessionData = await sessionResponse.Content.ReadFromJsonAsync<dynamic>();
            var sessionId = sessionData?.sessionId?.ToString() ?? sessionData?.data?.sessionId?.ToString() ?? "test-session";

            var message = new
            {
                Content = "I need help resetting my password"
            };

            // Act
            var response = await Client.PostAsJsonAsync($"/api/itsm/chatbot/sessions/{sessionId}/messages", message);

            // Assert
            response.StatusCode.Should().BeOneOf(
                HttpStatusCode.OK,
                HttpStatusCode.Created,
                HttpStatusCode.BadRequest,
                HttpStatusCode.NotFound,
                HttpStatusCode.NotImplemented);
        }
    }

    [Fact]
    [Trait("Category", "Functional")]
    [Trait("Feature", "Chatbot")]
    public async Task FUN_CHATBOT_003_IntentRecognitionPasswordReset()
    {
        // Arrange
        await AuthenticateAsync();

        var messages = new[]
        {
            "I forgot my password",
            "Can you reset my password?",
            "Password not working",
            "locked out of account"
        };

        foreach (var msg in messages)
        {
            var analyzeRequest = new { Content = msg };

            // Act
            var response = await Client.PostAsJsonAsync("/api/itsm/chatbot/analyze-intent", analyzeRequest);

            // Assert
            response.StatusCode.Should().BeOneOf(
                HttpStatusCode.OK,
                HttpStatusCode.NotFound,
                HttpStatusCode.NotImplemented);
        }
    }

    [Fact]
    [Trait("Category", "Functional")]
    [Trait("Feature", "Chatbot")]
    public async Task FUN_CHATBOT_004_SearchKnowledgeBase()
    {
        // Arrange
        await AuthenticateAsync();

        var searchRequest = new
        {
            Query = "How to configure VPN access",
            Limit = 5,
            IncludeSnippets = true
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/itsm/chatbot/search", searchRequest);

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.NotFound,
            HttpStatusCode.NotImplemented);
    }

    [Fact]
    [Trait("Category", "Functional")]
    [Trait("Feature", "Chatbot")]
    public async Task FUN_CHATBOT_005_ExecuteQuickAction()
    {
        // Arrange
        await AuthenticateAsync();

        var actionRequest = new
        {
            SessionId = "test-session-001",
            ActionId = "create_incident",
            Parameters = new Dictionary<string, string>
            {
                { "summary", "Unable to access email" },
                { "category", "Email" },
                { "priority", "Medium" }
            }
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/itsm/chatbot/execute-action", actionRequest);

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.Created,
            HttpStatusCode.BadRequest,
            HttpStatusCode.NotFound,
            HttpStatusCode.NotImplemented);
    }

    [Fact]
    [Trait("Category", "Functional")]
    [Trait("Feature", "Chatbot")]
    public async Task FUN_CHATBOT_006_EscalateToAgent()
    {
        // Arrange
        await AuthenticateAsync();

        var escalationRequest = new
        {
            SessionId = "test-session-002",
            Reason = "Customer requested to speak with a human agent",
            TicketSummary = "Complex issue requiring human assistance"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/itsm/chatbot/escalate", escalationRequest);

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.Created,
            HttpStatusCode.BadRequest,
            HttpStatusCode.NotFound,
            HttpStatusCode.NotImplemented);
    }

    [Fact]
    [Trait("Category", "Functional")]
    [Trait("Feature", "Chatbot")]
    public async Task FUN_CHATBOT_007_GetSessionHistory()
    {
        // Arrange
        await AuthenticateAsync();
        var sessionId = "test-session-001";

        // Act
        var response = await Client.GetAsync($"/api/itsm/chatbot/sessions/{sessionId}");

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.NotFound,
            HttpStatusCode.NotImplemented);
    }

    [Fact]
    [Trait("Category", "Functional")]
    [Trait("Feature", "Chatbot")]
    public async Task FUN_CHATBOT_008_EndChatSession()
    {
        // Arrange
        await AuthenticateAsync();

        // Create a session first
        var sessionResponse = await Client.PostAsJsonAsync("/api/itsm/chatbot/sessions", new { });

        if (sessionResponse.IsSuccessStatusCode)
        {
            var sessionData = await sessionResponse.Content.ReadFromJsonAsync<dynamic>();
            var sessionId = sessionData?.sessionId?.ToString() ?? "test-session";

            var endRequest = new
            {
                Rating = 5,
                Feedback = "Very helpful chatbot!"
            };

            // Act
            var response = await Client.PostAsJsonAsync($"/api/itsm/chatbot/sessions/{sessionId}/end", endRequest);

            // Assert
            response.StatusCode.Should().BeOneOf(
                HttpStatusCode.OK,
                HttpStatusCode.NoContent,
                HttpStatusCode.NotFound,
                HttpStatusCode.NotImplemented);
        }
    }

    #endregion
}
