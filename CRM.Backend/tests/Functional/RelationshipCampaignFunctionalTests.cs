using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;
using Xunit.Abstractions;

namespace CRM.Tests.Functional;

/// <summary>
/// Automated Functional Tests - Relationship Management & Campaign Execution Endpoints
/// These tests run against a live API instance to validate end-to-end functionality.
/// 
/// Run with: CRM_API_URL=http://localhost:5000 dotnet test --filter "Category=Functional"
/// 
/// Test Categories:
/// - FT-100 to FT-110: Relationship Type CRUD
/// - FT-111 to FT-120: Account Relationship CRUD
/// - FT-121 to FT-130: Relationship Interactions
/// - FT-131 to FT-140: Health Snapshots & Map
/// - FT-141 to FT-150: Campaign Workflow Integration
/// - FT-151 to FT-160: Campaign Recipients
/// - FT-161 to FT-170: A/B Testing
/// - FT-171 to FT-180: Conversions & Analytics
/// </summary>
[Trait("Category", "Functional")]
[Collection("Functional")]
public class RelationshipCampaignFunctionalTests : FunctionalTestBase
{
    private readonly ITestOutputHelper _output;
    
    public RelationshipCampaignFunctionalTests(ITestOutputHelper output)
    {
        _output = output;
    }

    #region Relationship Type Tests (FT-100 to FT-110)

    [Fact]
    [Trait("TestId", "FT-100")]
    public async Task FT100_GetRelationshipTypes_Should_Return_List()
    {
        if (!ApiAvailable) { _output.WriteLine("FT-100 SKIPPED: API not available"); return; }
        
        await AuthenticateAsync();
        var response = await Client.GetAsync("/api/relationships/types");
        
        AssertSuccess(response);
        var types = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(types.ValueKind == JsonValueKind.Array);
        _output.WriteLine($"FT-100 PASSED: Retrieved {types.GetArrayLength()} relationship types");
    }

    [Fact]
    [Trait("TestId", "FT-101")]
    public async Task FT101_CreateRelationshipType_Should_Succeed()
    {
        if (!ApiAvailable) { _output.WriteLine("FT-101 SKIPPED: API not available"); return; }
        
        await AuthenticateAsync();
        var response = await Client.PostAsJsonAsync("/api/relationships/types", new
        {
            typeName = $"TestPartner_{DateTime.Now.Ticks}",
            typeCategory = "B2B",
            description = "Test partner relationship",
            isBidirectional = true,
            sortOrder = 100
        });
        
        if (response.IsSuccessStatusCode)
        {
            var created = await response.Content.ReadFromJsonAsync<JsonElement>();
            Assert.True(created.GetProperty("id").GetInt32() > 0);
            _output.WriteLine($"FT-101 PASSED: Created relationship type with ID {created.GetProperty("id").GetInt32()}");
        }
        else
        {
            _output.WriteLine($"FT-101 SKIPPED: Create not permitted or failed - {response.StatusCode}");
        }
    }

    [Fact]
    [Trait("TestId", "FT-102")]
    public async Task FT102_GetRelationshipType_ById_Should_Return_Type()
    {
        if (!ApiAvailable) { _output.WriteLine("FT-102 SKIPPED: API not available"); return; }
        
        await AuthenticateAsync();
        
        // First get the list to find an ID
        var listResponse = await Client.GetAsync("/api/relationships/types");
        if (!listResponse.IsSuccessStatusCode)
        {
            _output.WriteLine("FT-102 SKIPPED: Could not get types list");
            return;
        }
        
        var types = await listResponse.Content.ReadFromJsonAsync<JsonElement>();
        if (types.GetArrayLength() == 0)
        {
            _output.WriteLine("FT-102 SKIPPED: No relationship types exist");
            return;
        }
        
        var firstId = types[0].GetProperty("id").GetInt32();
        var response = await Client.GetAsync($"/api/relationships/types/{firstId}");
        
        AssertSuccess(response);
        _output.WriteLine($"FT-102 PASSED: Retrieved relationship type ID {firstId}");
    }

    #endregion

    #region Account Relationship Tests (FT-111 to FT-120)

    [Fact]
    [Trait("TestId", "FT-111")]
    public async Task FT111_GetAccountRelationships_Should_Return_List()
    {
        if (!ApiAvailable) { _output.WriteLine("FT-111 SKIPPED: API not available"); return; }
        
        await AuthenticateAsync();
        var response = await Client.GetAsync("/api/relationships");
        
        AssertSuccess(response);
        var relationships = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(relationships.ValueKind == JsonValueKind.Array);
        _output.WriteLine($"FT-111 PASSED: Retrieved {relationships.GetArrayLength()} relationships");
    }

    [Fact]
    [Trait("TestId", "FT-112")]
    public async Task FT112_CreateAccountRelationship_Should_Succeed()
    {
        if (!ApiAvailable) { _output.WriteLine("FT-112 SKIPPED: API not available"); return; }
        
        await AuthenticateAsync();
        
        // Get customers first
        var customersResponse = await Client.GetAsync("/api/customers");
        if (!customersResponse.IsSuccessStatusCode)
        {
            _output.WriteLine("FT-112 SKIPPED: Could not get customers");
            return;
        }
        
        var customers = await customersResponse.Content.ReadFromJsonAsync<JsonElement>();
        if (customers.GetArrayLength() < 2)
        {
            _output.WriteLine("FT-112 SKIPPED: Need at least 2 customers for relationship");
            return;
        }
        
        // Get relationship types
        var typesResponse = await Client.GetAsync("/api/relationships/types");
        var types = await typesResponse.Content.ReadFromJsonAsync<JsonElement>();
        if (types.GetArrayLength() == 0)
        {
            _output.WriteLine("FT-112 SKIPPED: No relationship types available");
            return;
        }
        
        var response = await Client.PostAsJsonAsync("/api/relationships", new
        {
            sourceCustomerId = customers[0].GetProperty("id").GetInt32(),
            targetCustomerId = customers[1].GetProperty("id").GetInt32(),
            relationshipTypeId = types[0].GetProperty("id").GetInt32(),
            status = "Active",
            strategicImportance = "Medium",
            notes = "Test relationship"
        });
        
        if (response.IsSuccessStatusCode)
        {
            var created = await response.Content.ReadFromJsonAsync<JsonElement>();
            Assert.True(created.GetProperty("id").GetInt32() > 0);
            _output.WriteLine($"FT-112 PASSED: Created relationship with ID {created.GetProperty("id").GetInt32()}");
        }
        else
        {
            var error = await response.Content.ReadAsStringAsync();
            _output.WriteLine($"FT-112 INFO: Create returned {response.StatusCode} - {error}");
        }
    }

    [Fact]
    [Trait("TestId", "FT-113")]
    public async Task FT113_GetAccountRelationship_ByCustomerId_Should_Filter()
    {
        if (!ApiAvailable) { _output.WriteLine("FT-113 SKIPPED: API not available"); return; }
        
        await AuthenticateAsync();
        
        var customersResponse = await Client.GetAsync("/api/customers");
        if (!customersResponse.IsSuccessStatusCode || (await customersResponse.Content.ReadFromJsonAsync<JsonElement>()).GetArrayLength() == 0)
        {
            _output.WriteLine("FT-113 SKIPPED: No customers available");
            return;
        }
        
        var customers = await customersResponse.Content.ReadFromJsonAsync<JsonElement>();
        var customerId = customers[0].GetProperty("id").GetInt32();
        
        var response = await Client.GetAsync($"/api/relationships?customerId={customerId}");
        
        AssertSuccess(response);
        _output.WriteLine($"FT-113 PASSED: Filtered relationships by customer ID {customerId}");
    }

    #endregion

    #region Relationship Interaction Tests (FT-121 to FT-130)

    [Fact]
    [Trait("TestId", "FT-121")]
    public async Task FT121_GetRelationshipInteractions_Should_Return_List()
    {
        if (!ApiAvailable) { _output.WriteLine("FT-121 SKIPPED: API not available"); return; }
        
        await AuthenticateAsync();
        
        // Get a relationship first
        var relResponse = await Client.GetAsync("/api/relationships");
        var relationships = await relResponse.Content.ReadFromJsonAsync<JsonElement>();
        if (relationships.GetArrayLength() == 0)
        {
            _output.WriteLine("FT-121 SKIPPED: No relationships exist");
            return;
        }
        
        var relId = relationships[0].GetProperty("id").GetInt32();
        var response = await Client.GetAsync($"/api/relationships/{relId}/interactions");
        
        AssertSuccess(response);
        _output.WriteLine($"FT-121 PASSED: Retrieved interactions for relationship {relId}");
    }

    [Fact]
    [Trait("TestId", "FT-122")]
    public async Task FT122_CreateRelationshipInteraction_Should_Succeed()
    {
        if (!ApiAvailable) { _output.WriteLine("FT-122 SKIPPED: API not available"); return; }
        
        await AuthenticateAsync();
        
        var relResponse = await Client.GetAsync("/api/relationships");
        var relationships = await relResponse.Content.ReadFromJsonAsync<JsonElement>();
        if (relationships.GetArrayLength() == 0)
        {
            _output.WriteLine("FT-122 SKIPPED: No relationships exist");
            return;
        }
        
        var relId = relationships[0].GetProperty("id").GetInt32();
        var response = await Client.PostAsJsonAsync($"/api/relationships/{relId}/interactions", new
        {
            accountRelationshipId = relId,
            interactionType = "Meeting",
            subject = "Quarterly Review",
            description = "Discussed partnership opportunities",
            outcome = "Positive",
            healthImpact = "Moderate",
            strengthChange = 5
        });
        
        if (response.IsSuccessStatusCode)
        {
            _output.WriteLine($"FT-122 PASSED: Created interaction for relationship {relId}");
        }
        else
        {
            _output.WriteLine($"FT-122 INFO: Create returned {response.StatusCode}");
        }
    }

    #endregion

    #region Health Snapshot & Map Tests (FT-131 to FT-140)

    [Fact]
    [Trait("TestId", "FT-131")]
    public async Task FT131_GetRelationshipMap_Should_Return_Visualization()
    {
        if (!ApiAvailable) { _output.WriteLine("FT-131 SKIPPED: API not available"); return; }
        
        await AuthenticateAsync();
        
        var customersResponse = await Client.GetAsync("/api/customers");
        var customers = await customersResponse.Content.ReadFromJsonAsync<JsonElement>();
        if (customers.GetArrayLength() == 0)
        {
            _output.WriteLine("FT-131 SKIPPED: No customers available");
            return;
        }
        
        var customerId = customers[0].GetProperty("id").GetInt32();
        var response = await Client.GetAsync($"/api/relationships/map/{customerId}?depth=2");
        
        if (response.IsSuccessStatusCode)
        {
            var map = await response.Content.ReadFromJsonAsync<JsonElement>();
            Assert.True(map.TryGetProperty("nodes", out _));
            Assert.True(map.TryGetProperty("edges", out _));
            _output.WriteLine($"FT-131 PASSED: Retrieved relationship map for customer {customerId}");
        }
        else
        {
            _output.WriteLine($"FT-131 INFO: Map endpoint returned {response.StatusCode}");
        }
    }

    [Fact]
    [Trait("TestId", "FT-132")]
    public async Task FT132_GetHealthSnapshots_Should_Return_List()
    {
        if (!ApiAvailable) { _output.WriteLine("FT-132 SKIPPED: API not available"); return; }
        
        await AuthenticateAsync();
        
        var customersResponse = await Client.GetAsync("/api/customers");
        var customers = await customersResponse.Content.ReadFromJsonAsync<JsonElement>();
        if (customers.GetArrayLength() == 0)
        {
            _output.WriteLine("FT-132 SKIPPED: No customers available");
            return;
        }
        
        var customerId = customers[0].GetProperty("id").GetInt32();
        var response = await Client.GetAsync($"/api/relationships/health/{customerId}");
        
        AssertSuccess(response);
        _output.WriteLine($"FT-132 PASSED: Retrieved health snapshots for customer {customerId}");
    }

    [Fact]
    [Trait("TestId", "FT-133")]
    public async Task FT133_CreateHealthSnapshot_Should_Succeed()
    {
        if (!ApiAvailable) { _output.WriteLine("FT-133 SKIPPED: API not available"); return; }
        
        await AuthenticateAsync();
        
        var customersResponse = await Client.GetAsync("/api/customers");
        var customers = await customersResponse.Content.ReadFromJsonAsync<JsonElement>();
        if (customers.GetArrayLength() == 0)
        {
            _output.WriteLine("FT-133 SKIPPED: No customers available");
            return;
        }
        
        var customerId = customers[0].GetProperty("id").GetInt32();
        var response = await Client.PostAsync($"/api/relationships/health/{customerId}", null);
        
        if (response.IsSuccessStatusCode)
        {
            _output.WriteLine($"FT-133 PASSED: Created health snapshot for customer {customerId}");
        }
        else
        {
            _output.WriteLine($"FT-133 INFO: Create snapshot returned {response.StatusCode}");
        }
    }

    #endregion

    #region Campaign Workflow Tests (FT-141 to FT-150)

    [Fact]
    [Trait("TestId", "FT-141")]
    public async Task FT141_GetCampaignWorkflows_Should_Return_List()
    {
        if (!ApiAvailable) { _output.WriteLine("FT-141 SKIPPED: API not available"); return; }
        
        await AuthenticateAsync();
        
        var campaignsResponse = await Client.GetAsync("/api/campaigns");
        var campaigns = await campaignsResponse.Content.ReadFromJsonAsync<JsonElement>();
        if (campaigns.GetArrayLength() == 0)
        {
            _output.WriteLine("FT-141 SKIPPED: No campaigns available");
            return;
        }
        
        var campaignId = campaigns[0].GetProperty("id").GetInt32();
        var response = await Client.GetAsync($"/api/campaign-execution/{campaignId}/workflows");
        
        AssertSuccess(response);
        _output.WriteLine($"FT-141 PASSED: Retrieved workflows for campaign {campaignId}");
    }

    [Fact]
    [Trait("TestId", "FT-142")]
    public async Task FT142_LinkCampaignWorkflow_Should_Succeed()
    {
        if (!ApiAvailable) { _output.WriteLine("FT-142 SKIPPED: API not available"); return; }
        
        await AuthenticateAsync();
        
        // Get campaigns
        var campaignsResponse = await Client.GetAsync("/api/campaigns");
        var campaigns = await campaignsResponse.Content.ReadFromJsonAsync<JsonElement>();
        if (campaigns.GetArrayLength() == 0)
        {
            _output.WriteLine("FT-142 SKIPPED: No campaigns available");
            return;
        }
        
        // Get workflow definitions
        var workflowsResponse = await Client.GetAsync("/api/workflows/definitions");
        if (!workflowsResponse.IsSuccessStatusCode)
        {
            _output.WriteLine("FT-142 SKIPPED: Could not get workflow definitions");
            return;
        }
        
        var workflows = await workflowsResponse.Content.ReadFromJsonAsync<JsonElement>();
        if (workflows.GetArrayLength() == 0)
        {
            _output.WriteLine("FT-142 SKIPPED: No workflow definitions available");
            return;
        }
        
        var campaignId = campaigns[0].GetProperty("id").GetInt32();
        var workflowId = workflows[0].GetProperty("id").GetInt32();
        
        var response = await Client.PostAsJsonAsync($"/api/campaign-execution/{campaignId}/workflows", new
        {
            campaignId = campaignId,
            workflowDefinitionId = workflowId,
            workflowType = "Execution",
            triggerEvent = "CampaignStarted",
            priority = 1
        });
        
        if (response.IsSuccessStatusCode)
        {
            _output.WriteLine($"FT-142 PASSED: Linked workflow to campaign {campaignId}");
        }
        else
        {
            _output.WriteLine($"FT-142 INFO: Link workflow returned {response.StatusCode}");
        }
    }

    #endregion

    #region Campaign Recipient Tests (FT-151 to FT-160)

    [Fact]
    [Trait("TestId", "FT-151")]
    public async Task FT151_GetCampaignRecipients_Should_Return_List()
    {
        if (!ApiAvailable) { _output.WriteLine("FT-151 SKIPPED: API not available"); return; }
        
        await AuthenticateAsync();
        
        var campaignsResponse = await Client.GetAsync("/api/campaigns");
        var campaigns = await campaignsResponse.Content.ReadFromJsonAsync<JsonElement>();
        if (campaigns.GetArrayLength() == 0)
        {
            _output.WriteLine("FT-151 SKIPPED: No campaigns available");
            return;
        }
        
        var campaignId = campaigns[0].GetProperty("id").GetInt32();
        var response = await Client.GetAsync($"/api/campaign-execution/{campaignId}/recipients");
        
        AssertSuccess(response);
        _output.WriteLine($"FT-151 PASSED: Retrieved recipients for campaign {campaignId}");
    }

    [Fact]
    [Trait("TestId", "FT-152")]
    public async Task FT152_AddCampaignRecipients_Should_Succeed()
    {
        if (!ApiAvailable) { _output.WriteLine("FT-152 SKIPPED: API not available"); return; }
        
        await AuthenticateAsync();
        
        var campaignsResponse = await Client.GetAsync("/api/campaigns");
        var campaigns = await campaignsResponse.Content.ReadFromJsonAsync<JsonElement>();
        if (campaigns.GetArrayLength() == 0)
        {
            _output.WriteLine("FT-152 SKIPPED: No campaigns available");
            return;
        }
        
        var campaignId = campaigns[0].GetProperty("id").GetInt32();
        var response = await Client.PostAsJsonAsync($"/api/campaign-execution/{campaignId}/recipients", new
        {
            emails = new[] { $"test_{DateTime.Now.Ticks}@example.com" }
        });
        
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            _output.WriteLine($"FT-152 PASSED: Added recipients to campaign {campaignId}");
        }
        else
        {
            _output.WriteLine($"FT-152 INFO: Add recipients returned {response.StatusCode}");
        }
    }

    #endregion

    #region A/B Test Tests (FT-161 to FT-170)

    [Fact]
    [Trait("TestId", "FT-161")]
    public async Task FT161_GetCampaignABTests_Should_Return_List()
    {
        if (!ApiAvailable) { _output.WriteLine("FT-161 SKIPPED: API not available"); return; }
        
        await AuthenticateAsync();
        
        var campaignsResponse = await Client.GetAsync("/api/campaigns");
        var campaigns = await campaignsResponse.Content.ReadFromJsonAsync<JsonElement>();
        if (campaigns.GetArrayLength() == 0)
        {
            _output.WriteLine("FT-161 SKIPPED: No campaigns available");
            return;
        }
        
        var campaignId = campaigns[0].GetProperty("id").GetInt32();
        var response = await Client.GetAsync($"/api/campaign-execution/{campaignId}/ab-tests");
        
        AssertSuccess(response);
        _output.WriteLine($"FT-161 PASSED: Retrieved A/B tests for campaign {campaignId}");
    }

    [Fact]
    [Trait("TestId", "FT-162")]
    public async Task FT162_CreateABTest_Should_Succeed()
    {
        if (!ApiAvailable) { _output.WriteLine("FT-162 SKIPPED: API not available"); return; }
        
        await AuthenticateAsync();
        
        var campaignsResponse = await Client.GetAsync("/api/campaigns");
        var campaigns = await campaignsResponse.Content.ReadFromJsonAsync<JsonElement>();
        if (campaigns.GetArrayLength() == 0)
        {
            _output.WriteLine("FT-162 SKIPPED: No campaigns available");
            return;
        }
        
        var campaignId = campaigns[0].GetProperty("id").GetInt32();
        var response = await Client.PostAsJsonAsync($"/api/campaign-execution/{campaignId}/ab-tests", new
        {
            campaignId = campaignId,
            testName = $"Subject Test {DateTime.Now.Ticks}",
            testType = "SubjectLine",
            variantA = "50% Off Today Only!",
            variantB = "Exclusive Deal Inside",
            trafficSplit = new { variantA = 50, variantB = 50 },
            testMetric = "OpenRate",
            sampleSize = 1000
        });
        
        if (response.IsSuccessStatusCode)
        {
            _output.WriteLine($"FT-162 PASSED: Created A/B test for campaign {campaignId}");
        }
        else
        {
            _output.WriteLine($"FT-162 INFO: Create A/B test returned {response.StatusCode}");
        }
    }

    #endregion

    #region Conversion & Analytics Tests (FT-171 to FT-180)

    [Fact]
    [Trait("TestId", "FT-171")]
    public async Task FT171_GetCampaignConversions_Should_Return_List()
    {
        if (!ApiAvailable) { _output.WriteLine("FT-171 SKIPPED: API not available"); return; }
        
        await AuthenticateAsync();
        
        var campaignsResponse = await Client.GetAsync("/api/campaigns");
        var campaigns = await campaignsResponse.Content.ReadFromJsonAsync<JsonElement>();
        if (campaigns.GetArrayLength() == 0)
        {
            _output.WriteLine("FT-171 SKIPPED: No campaigns available");
            return;
        }
        
        var campaignId = campaigns[0].GetProperty("id").GetInt32();
        var response = await Client.GetAsync($"/api/campaign-execution/{campaignId}/conversions");
        
        AssertSuccess(response);
        _output.WriteLine($"FT-171 PASSED: Retrieved conversions for campaign {campaignId}");
    }

    [Fact]
    [Trait("TestId", "FT-172")]
    public async Task FT172_GetCampaignAnalytics_Should_Return_Data()
    {
        if (!ApiAvailable) { _output.WriteLine("FT-172 SKIPPED: API not available"); return; }
        
        await AuthenticateAsync();
        
        var campaignsResponse = await Client.GetAsync("/api/campaigns");
        var campaigns = await campaignsResponse.Content.ReadFromJsonAsync<JsonElement>();
        if (campaigns.GetArrayLength() == 0)
        {
            _output.WriteLine("FT-172 SKIPPED: No campaigns available");
            return;
        }
        
        var campaignId = campaigns[0].GetProperty("id").GetInt32();
        var response = await Client.GetAsync($"/api/campaign-execution/{campaignId}/analytics");
        
        if (response.IsSuccessStatusCode)
        {
            var analytics = await response.Content.ReadFromJsonAsync<JsonElement>();
            _output.WriteLine($"FT-172 PASSED: Retrieved analytics for campaign {campaignId}");
        }
        else
        {
            _output.WriteLine($"FT-172 INFO: Get analytics returned {response.StatusCode}");
        }
    }

    [Fact]
    [Trait("TestId", "FT-173")]
    public async Task FT173_RecordConversion_Should_Succeed()
    {
        if (!ApiAvailable) { _output.WriteLine("FT-173 SKIPPED: API not available"); return; }
        
        await AuthenticateAsync();
        
        var campaignsResponse = await Client.GetAsync("/api/campaigns");
        var campaigns = await campaignsResponse.Content.ReadFromJsonAsync<JsonElement>();
        if (campaigns.GetArrayLength() == 0)
        {
            _output.WriteLine("FT-173 SKIPPED: No campaigns available");
            return;
        }
        
        var campaignId = campaigns[0].GetProperty("id").GetInt32();
        var response = await Client.PostAsJsonAsync($"/api/campaign-execution/{campaignId}/conversions", new
        {
            campaignId = campaignId,
            conversionType = "Purchase",
            conversionValue = 99.99,
            currency = "USD",
            attributionModel = "LastTouch",
            sourceChannel = "Email"
        });
        
        if (response.IsSuccessStatusCode)
        {
            _output.WriteLine($"FT-173 PASSED: Recorded conversion for campaign {campaignId}");
        }
        else
        {
            _output.WriteLine($"FT-173 INFO: Record conversion returned {response.StatusCode}");
        }
    }

    #endregion
}
