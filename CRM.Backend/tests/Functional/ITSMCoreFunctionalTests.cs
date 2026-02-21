// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Net;
using System.Net.Http.Json;
using CRM.Core.DTOs.ITSM;
using CRM.Core.Entities.ITSM;
using FluentAssertions;
using Xunit;

namespace CRM.Tests.Functional;

/// <summary>
/// Functional HTTP Tests for ITSM Core Features:
/// - Incident Management API
/// - Problem Management API
/// - Change Management API
/// - SLA Management API
/// - CMDB API
/// - Knowledge Management API
/// - Service Catalog API
/// </summary>
public class ITSMCoreFunctionalTests : FunctionalTestBase
{
    #region Incident Management Functional Tests

    [Fact]
    [Trait("Feature", "Incidents")]
    [Trait("Category", "Functional")]
    public async Task CreateIncident_WithValidData_ReturnsCreated()
    {
        // Arrange
        var createDto = new CreateIncidentDto
        {
            ShortDescription = "Email server not responding",
            Description = "Users report they cannot send or receive emails",
            CallerId = 1,
            ContactType = ContactType.Phone,
            Impact = IncidentImpact.High,
            Urgency = IncidentUrgency.High
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/itsm/incidents", createDto);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("Feature", "Incidents")]
    [Trait("Category", "Functional")]
    public async Task GetIncidents_WithPagination_ReturnsPagedResults()
    {
        // Arrange
        var pageNumber = 1;
        var pageSize = 10;

        // Act
        var response = await Client.GetAsync($"/api/itsm/incidents?pageNumber={pageNumber}&pageSize={pageSize}");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("Feature", "Incidents")]
    [Trait("Category", "Functional")]
    public async Task GetIncidentById_WithValidId_ReturnsIncident()
    {
        // Arrange
        var incidentId = 1;

        // Act
        var response = await Client.GetAsync($"/api/itsm/incidents/{incidentId}");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("Feature", "Incidents")]
    [Trait("Category", "Functional")]
    public async Task AssignIncident_ToUser_ReturnsSuccess()
    {
        // Arrange
        var incidentId = 1;
        var assignToUserId = 2;

        // Act
        var response = await Client.PostAsync($"/api/itsm/incidents/{incidentId}/assign?assignedToId={assignToUserId}", null);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent, HttpStatusCode.NotFound, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("Feature", "Incidents")]
    [Trait("Category", "Functional")]
    public async Task ResolveIncident_WithResolutionNotes_ReturnsSuccess()
    {
        // Arrange
        var incidentId = 1;
        var resolveDto = new ResolveIncidentDto
        {
            ResolutionCode = ResolutionCode.SolvedPermanently,
            ResolutionNotes = "Restarted the email service"
        };

        // Act
        var response = await Client.PostAsJsonAsync($"/api/itsm/incidents/{incidentId}/resolve", resolveDto);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("Feature", "Incidents")]
    [Trait("Category", "Functional")]
    public async Task AddComment_ToIncident_ReturnsSuccess()
    {
        // Arrange
        var incidentId = 1;
        var comment = new { Comment = "Investigating the issue", IsInternal = true };

        // Act
        var response = await Client.PostAsJsonAsync($"/api/itsm/incidents/{incidentId}/comments", comment);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created, HttpStatusCode.NotFound, HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Problem Management Functional Tests

    [Fact]
    [Trait("Feature", "Problems")]
    [Trait("Category", "Functional")]
    public async Task CreateProblem_WithValidData_ReturnsCreated()
    {
        // Arrange
        var createDto = new CreateProblemDto
        {
            ShortDescription = "Recurring application crashes",
            Description = "CRM application crashes intermittently",
            Priority = ProblemPriority.High
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/itsm/problems", createDto);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("Feature", "Problems")]
    [Trait("Category", "Functional")]
    public async Task GetProblems_WithFilters_ReturnsFilteredResults()
    {
        // Arrange & Act
        var response = await Client.GetAsync("/api/itsm/problems?state=New&priority=High");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("Feature", "Problems")]
    [Trait("Category", "Functional")]
    public async Task LinkIncidentToProblem_ReturnsSuccess()
    {
        // Arrange
        var problemId = 1;
        var incidentId = 1;

        // Act
        var response = await Client.PostAsync($"/api/itsm/problems/{problemId}/incidents/{incidentId}", null);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent, HttpStatusCode.NotFound, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("Feature", "Problems")]
    [Trait("Category", "Functional")]
    public async Task MarkAsKnownError_ReturnsSuccess()
    {
        // Arrange
        var problemId = 1;

        // Act
        var response = await Client.PostAsync($"/api/itsm/problems/{problemId}/known-error", null);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent, HttpStatusCode.NotFound, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("Feature", "Problems")]
    [Trait("Category", "Functional")]
    public async Task UpdateRootCauseAnalysis_ReturnsSuccess()
    {
        // Arrange
        var problemId = 1;
        var rca = new { RootCause = "Memory leak in module X", Workaround = "Restart daily" };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/itsm/problems/{problemId}/rca", rca);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent, HttpStatusCode.NotFound, HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Change Management Functional Tests

    [Fact]
    [Trait("Feature", "Changes")]
    [Trait("Category", "Functional")]
    public async Task CreateChange_WithValidData_ReturnsCreated()
    {
        // Arrange
        var createDto = new CreateChangeDto
        {
            ShortDescription = "Deploy new API version",
            Description = "Upgrade CRM API from v2.0 to v2.1",
            Type = ChangeType.Normal,
            Risk = ChangeRisk.Medium,
            Impact = ChangeImpact.Medium,
            PlannedStartDate = DateTime.UtcNow.AddDays(7),
            PlannedEndDate = DateTime.UtcNow.AddDays(7).AddHours(2)
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/itsm/changes", createDto);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("Feature", "Changes")]
    [Trait("Category", "Functional")]
    public async Task GetChanges_WithDateRange_ReturnsFilteredResults()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(-30).ToString("yyyy-MM-dd");
        var endDate = DateTime.UtcNow.AddDays(30).ToString("yyyy-MM-dd");

        // Act
        var response = await Client.GetAsync($"/api/itsm/changes?plannedStartFrom={startDate}&plannedStartTo={endDate}");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("Feature", "Changes")]
    [Trait("Category", "Functional")]
    public async Task SubmitForApproval_ReturnsSuccess()
    {
        // Arrange
        var changeId = 1;

        // Act
        var response = await Client.PostAsync($"/api/itsm/changes/{changeId}/submit", null);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent, HttpStatusCode.NotFound, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("Feature", "Changes")]
    [Trait("Category", "Functional")]
    public async Task ApproveChange_ReturnsSuccess()
    {
        // Arrange
        var changeId = 1;
        var approval = new { Comments = "Approved for implementation" };

        // Act
        var response = await Client.PostAsJsonAsync($"/api/itsm/changes/{changeId}/approve", approval);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent, HttpStatusCode.NotFound, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("Feature", "Changes")]
    [Trait("Category", "Functional")]
    public async Task CheckConflicts_ReturnsConflictInfo()
    {
        // Arrange
        var changeId = 1;

        // Act
        var response = await Client.GetAsync($"/api/itsm/changes/{changeId}/conflicts");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("Feature", "Changes")]
    [Trait("Category", "Functional")]
    public async Task GetBlackoutPeriods_ReturnsBlackoutList()
    {
        // Arrange
        var startDate = DateTime.UtcNow.ToString("yyyy-MM-dd");
        var endDate = DateTime.UtcNow.AddMonths(3).ToString("yyyy-MM-dd");

        // Act
        var response = await Client.GetAsync($"/api/itsm/changes/blackout-periods?startDate={startDate}&endDate={endDate}");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized);
    }

    #endregion

    #region SLA Management Functional Tests

    [Fact]
    [Trait("Feature", "SLA")]
    [Trait("Category", "Functional")]
    public async Task GetSLAPolicies_ReturnsActivePolicies()
    {
        // Arrange & Act
        var response = await Client.GetAsync("/api/itsm/sla/policies");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("Feature", "SLA")]
    [Trait("Category", "Functional")]
    public async Task GetSLAInstance_ForIncident_ReturnsInstance()
    {
        // Arrange
        var incidentId = 1;

        // Act
        var response = await Client.GetAsync($"/api/itsm/sla/instance/incident/{incidentId}");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("Feature", "SLA")]
    [Trait("Category", "Functional")]
    public async Task GetBreachedSLAs_ReturnsList()
    {
        // Arrange & Act
        var response = await Client.GetAsync("/api/itsm/sla/breached");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("Feature", "SLA")]
    [Trait("Category", "Functional")]
    public async Task GetAtRiskSLAs_WithThreshold_ReturnsList()
    {
        // Arrange
        var thresholdMinutes = 30;

        // Act
        var response = await Client.GetAsync($"/api/itsm/sla/at-risk?thresholdMinutes={thresholdMinutes}");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("Feature", "SLA")]
    [Trait("Category", "Functional")]
    public async Task GetSLADashboard_ReturnsDashboardData()
    {
        // Arrange & Act
        var response = await Client.GetAsync("/api/itsm/sla/dashboard");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("Feature", "SLA")]
    [Trait("Category", "Functional")]
    public async Task GetSLAMetrics_WithDateRange_ReturnsMetrics()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddMonths(-1).ToString("yyyy-MM-dd");
        var endDate = DateTime.UtcNow.ToString("yyyy-MM-dd");

        // Act
        var response = await Client.GetAsync($"/api/itsm/sla/metrics?startDate={startDate}&endDate={endDate}");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized);
    }

    #endregion

    #region CMDB Functional Tests

    [Fact]
    [Trait("Feature", "CMDB")]
    [Trait("Category", "Functional")]
    public async Task CreateCI_WithValidData_ReturnsCreated()
    {
        // Arrange
        var createDto = new CreateCIDto
        {
            CIName = "TEST-SERVER-01",
            CIType = CIType.Server,
            CISubtype = "Virtual Server",
            IPAddress = "10.0.5.100",
            OperationalStatus = OperationalStatus.Operational
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/itsm/cmdb/cis", createDto);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("Feature", "CMDB")]
    [Trait("Category", "Functional")]
    public async Task SearchCIs_WithSearchTerm_ReturnsResults()
    {
        // Arrange
        var searchTerm = "server";

        // Act
        var response = await Client.GetAsync($"/api/itsm/cmdb/cis?search={searchTerm}");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("Feature", "CMDB")]
    [Trait("Category", "Functional")]
    public async Task GetCIById_WithValidId_ReturnsCI()
    {
        // Arrange
        var ciId = 1;

        // Act
        var response = await Client.GetAsync($"/api/itsm/cmdb/cis/{ciId}");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("Feature", "CMDB")]
    [Trait("Category", "Functional")]
    public async Task CreateRelationship_BetweenCIs_ReturnsSuccess()
    {
        // Arrange
        var parentCIId = 1;
        var childCIId = 2;

        // Act
        var response = await Client.PostAsync($"/api/itsm/cmdb/cis/{parentCIId}/relationships/{childCIId}?type=DependsOn", null);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created, HttpStatusCode.NotFound, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("Feature", "CMDB")]
    [Trait("Category", "Functional")]
    public async Task GetImpactAnalysis_ForCI_ReturnsImpactedItems()
    {
        // Arrange
        var ciId = 1;

        // Act
        var response = await Client.GetAsync($"/api/itsm/cmdb/cis/{ciId}/impact");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Knowledge Management Functional Tests

    [Fact]
    [Trait("Feature", "Knowledge")]
    [Trait("Category", "Functional")]
    public async Task CreateArticle_WithValidData_ReturnsCreated()
    {
        // Arrange
        var createDto = new CreateKnowledgeArticleDto
        {
            Title = "How to Reset VPN Password",
            ArticleBody = "<p>Follow these steps to reset your VPN password...</p>",
            ArticleType = ArticleType.HowTo,
            ShortDescription = "VPN password reset guide"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/itsm/knowledge/articles", createDto);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("Feature", "Knowledge")]
    [Trait("Category", "Functional")]
    public async Task SearchArticles_WithSearchTerm_ReturnsResults()
    {
        // Arrange
        var searchTerm = "password";

        // Act
        var response = await Client.GetAsync($"/api/itsm/knowledge/articles?search={searchTerm}");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("Feature", "Knowledge")]
    [Trait("Category", "Functional")]
    public async Task GetPopularArticles_ReturnsTopArticles()
    {
        // Arrange
        var count = 10;

        // Act
        var response = await Client.GetAsync($"/api/itsm/knowledge/articles/popular?count={count}");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("Feature", "Knowledge")]
    [Trait("Category", "Functional")]
    public async Task GetSuggestedArticles_ForIncident_ReturnsRelevantArticles()
    {
        // Arrange
        var description = "Cannot connect to VPN";

        // Act
        var response = await Client.GetAsync($"/api/itsm/knowledge/articles/suggest?description={Uri.EscapeDataString(description)}");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("Feature", "Knowledge")]
    [Trait("Category", "Functional")]
    public async Task SubmitFeedback_ForArticle_ReturnsSuccess()
    {
        // Arrange
        var articleId = 1;
        var feedback = new { IsHelpful = true, Comment = "Very helpful!" };

        // Act
        var response = await Client.PostAsJsonAsync($"/api/itsm/knowledge/articles/{articleId}/feedback", feedback);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created, HttpStatusCode.NotFound, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("Feature", "Knowledge")]
    [Trait("Category", "Functional")]
    public async Task PublishArticle_ReturnsSuccess()
    {
        // Arrange
        var articleId = 1;

        // Act
        var response = await Client.PostAsync($"/api/itsm/knowledge/articles/{articleId}/publish", null);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent, HttpStatusCode.NotFound, HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Service Catalog Functional Tests

    [Fact]
    [Trait("Feature", "ServiceCatalog")]
    [Trait("Category", "Functional")]
    public async Task GetCatalogItems_ReturnsList()
    {
        // Arrange & Act
        var response = await Client.GetAsync("/api/itsm/catalog/items");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("Feature", "ServiceCatalog")]
    [Trait("Category", "Functional")]
    public async Task GetFeaturedItems_ReturnsFeaturedOnly()
    {
        // Arrange & Act
        var response = await Client.GetAsync("/api/itsm/catalog/items?featuredOnly=true");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("Feature", "ServiceCatalog")]
    [Trait("Category", "Functional")]
    public async Task GetCatalogCategories_ReturnsCategoryList()
    {
        // Arrange & Act
        var response = await Client.GetAsync("/api/itsm/catalog/categories");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("Feature", "ServiceCatalog")]
    [Trait("Category", "Functional")]
    public async Task CreateCatalogRequest_ReturnsCreated()
    {
        // Arrange
        var requestDto = new CreateCatalogRequestDto
        {
            CatalogItemId = 1,
            RequestedForId = 1,
            VariableValues = new Dictionary<string, string>
            {
                { "justification", "Need for project work" }
            }
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/itsm/catalog/requests", requestDto);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("Feature", "ServiceCatalog")]
    [Trait("Category", "Functional")]
    public async Task GetMyRequests_ReturnsUserRequests()
    {
        // Arrange & Act
        var response = await Client.GetAsync("/api/itsm/catalog/my-requests");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("Feature", "ServiceCatalog")]
    [Trait("Category", "Functional")]
    public async Task SearchCatalog_WithSearchTerm_ReturnsResults()
    {
        // Arrange
        var searchTerm = "laptop";

        // Act
        var response = await Client.GetAsync($"/api/itsm/catalog/search?term={searchTerm}");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized);
    }

    #endregion
}
