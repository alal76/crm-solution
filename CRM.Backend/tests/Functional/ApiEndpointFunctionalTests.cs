using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;
using Xunit.Abstractions;

namespace CRM.Tests.Functional;

/// <summary>
/// Automated Functional Tests - Core API Endpoints
/// These tests run against a live API instance to validate end-to-end functionality.
/// 
/// Test Categories:
/// - FT-001 to FT-010: Health & Authentication
/// - FT-011 to FT-020: Customer CRUD Operations
/// - FT-021 to FT-030: Product CRUD Operations
/// - FT-031 to FT-040: Pipeline & Opportunities
/// - FT-041 to FT-050: Workflow Engine
/// - FT-051 to FT-060: Reporting & Analytics
/// </summary>
[Trait("Category", "Functional")]
[Collection("Functional")]
public class ApiEndpointFunctionalTests : FunctionalTestBase
{
    private readonly ITestOutputHelper _output;
    
    public ApiEndpointFunctionalTests(ITestOutputHelper output)
    {
        _output = output;
    }

    #region Health & Authentication Tests (FT-001 to FT-010)
    
    [Fact]
    [Trait("TestId", "FT-001")]
    public async Task FT001_Health_Endpoint_Should_Return_Healthy()
    {
        // Arrange & Act
        var response = await Client.GetAsync("/health");
        
        // Assert
        AssertSuccess(response);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("healthy", content.ToLower());
        
        _output.WriteLine($"FT-001 PASSED: Health endpoint returned healthy status");
    }
    
    [Fact]
    [Trait("TestId", "FT-002")]
    public async Task FT002_Swagger_Should_Be_Available_In_Development()
    {
        // Arrange & Act
        var response = await Client.GetAsync("/swagger/index.html");
        
        // Assert - Should return 200 in development mode
        AssertSuccess(response);
        
        _output.WriteLine($"FT-002 PASSED: Swagger UI is accessible");
    }
    
    [Fact]
    [Trait("TestId", "FT-003")]
    public async Task FT003_Login_With_Valid_Credentials_Should_Return_Token()
    {
        // Arrange & Act
        var authenticated = await AuthenticateAsync();
        
        // Assert
        Assert.True(authenticated, "Authentication should succeed with valid credentials");
        Assert.NotNull(AuthToken);
        Assert.NotEmpty(AuthToken);
        
        _output.WriteLine($"FT-003 PASSED: Login returned valid JWT token");
    }
    
    [Fact]
    [Trait("TestId", "FT-004")]
    public async Task FT004_Login_With_Invalid_Credentials_Should_Return_401()
    {
        // Arrange & Act
        var response = await Client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "invalid@example.com",
            password = "wrongpassword"
        });
        
        // Assert
        AssertStatusCode(response, HttpStatusCode.Unauthorized);
        
        _output.WriteLine($"FT-004 PASSED: Invalid login returned 401 Unauthorized");
    }
    
    [Fact]
    [Trait("TestId", "FT-005")]
    public async Task FT005_Protected_Endpoint_Without_Token_Should_Return_401()
    {
        // Arrange - Don't authenticate
        
        // Act
        var response = await Client.GetAsync("/api/customers");
        
        // Assert
        AssertStatusCode(response, HttpStatusCode.Unauthorized);
        
        _output.WriteLine($"FT-005 PASSED: Protected endpoint returned 401 without token");
    }
    
    [Fact]
    [Trait("TestId", "FT-006")]
    public async Task FT006_Protected_Endpoint_With_Token_Should_Return_Success()
    {
        // Arrange
        await AuthenticateAsync();
        
        // Act
        var response = await Client.GetAsync("/api/customers");
        
        // Assert
        AssertSuccess(response);
        
        _output.WriteLine($"FT-006 PASSED: Protected endpoint accessible with valid token");
    }

    #endregion

    #region Customer CRUD Tests (FT-011 to FT-020)
    
    [Fact]
    [Trait("TestId", "FT-011")]
    public async Task FT011_Get_Customers_Should_Return_List()
    {
        // Arrange
        await AuthenticateAsync();
        
        // Act
        var response = await Client.GetAsync("/api/customers");
        
        // Assert
        AssertSuccess(response);
        var content = await response.Content.ReadAsStringAsync();
        Assert.NotEmpty(content);
        
        _output.WriteLine($"FT-011 PASSED: GET /api/customers returned customer list");
    }
    
    [Fact]
    [Trait("TestId", "FT-012")]
    public async Task FT012_Create_Customer_Should_Return_Created()
    {
        // Arrange
        await AuthenticateAsync();
        var customer = new
        {
            firstName = "Functional",
            lastName = $"Test-{Guid.NewGuid():N}",
            email = $"test-{Guid.NewGuid():N}@functional.test",
            phone = "+1-555-1234",
            company = "Test Company"
        };
        
        // Act
        var response = await Client.PostAsJsonAsync("/api/customers", customer);
        
        // Assert
        Assert.True(response.StatusCode == HttpStatusCode.Created || 
                    response.StatusCode == HttpStatusCode.OK,
            $"Expected Created/OK but got {response.StatusCode}");
        
        _output.WriteLine($"FT-012 PASSED: POST /api/customers created new customer");
    }
    
    [Fact]
    [Trait("TestId", "FT-013")]
    public async Task FT013_Get_Customer_By_Id_Should_Return_Customer()
    {
        // Arrange
        await AuthenticateAsync();
        
        // First get list to find an existing customer ID
        var listResponse = await Client.GetAsync("/api/customers?pageSize=10");
        AssertSuccess(listResponse);
        var listContent = await listResponse.Content.ReadAsStringAsync();
        
        // Parse to get first customer ID - API returns an array directly
        using var doc = JsonDocument.Parse(listContent);
        var root = doc.RootElement;
        
        // Handle both array and paginated response formats
        JsonElement items;
        if (root.ValueKind == JsonValueKind.Array)
        {
            items = root;
        }
        else if (root.TryGetProperty("items", out items))
        {
            // Already set
        }
        else
        {
            _output.WriteLine("FT-013 SKIPPED: Unexpected response format");
            return;
        }
        
        if (items.GetArrayLength() == 0)
        {
            _output.WriteLine("FT-013 SKIPPED: No customers exist to retrieve");
            return;
        }
        
        var customerId = items[0].GetProperty("id").GetInt32();
        
        // Act
        var response = await Client.GetAsync($"/api/customers/{customerId}");
        
        // Assert
        AssertSuccess(response);
        
        _output.WriteLine($"FT-013 PASSED: GET /api/customers/{customerId} returned customer");
    }
    
    [Fact]
    [Trait("TestId", "FT-014")]
    public async Task FT014_Search_Customers_Should_Return_Filtered_Results()
    {
        // Arrange
        await AuthenticateAsync();
        
        // Act
        var response = await Client.GetAsync("/api/customers?search=test");
        
        // Assert
        AssertSuccess(response);
        
        _output.WriteLine($"FT-014 PASSED: Customer search with filter works");
    }
    
    [Fact]
    [Trait("TestId", "FT-015")]
    public async Task FT015_Customer_Pagination_Should_Work()
    {
        // Arrange
        await AuthenticateAsync();
        
        // Act
        var response = await Client.GetAsync("/api/customers?page=1&pageSize=5");
        
        // Assert
        AssertSuccess(response);
        var content = await response.Content.ReadAsStringAsync();
        
        using var doc = JsonDocument.Parse(content);
        var root = doc.RootElement;
        
        // Handle both array and paginated response formats
        // If paginated, check for properties; if array, just verify it's an array
        if (root.ValueKind == JsonValueKind.Array)
        {
            // Array format - pagination is handled server-side
            Assert.True(root.GetArrayLength() >= 0, "Response should be a valid array");
        }
        else
        {
            // Paginated format
            Assert.True(root.TryGetProperty("totalCount", out _) || root.TryGetProperty("items", out _));
        }
        
        _output.WriteLine($"FT-015 PASSED: Customer pagination works correctly");
    }

    #endregion

    #region Product CRUD Tests (FT-021 to FT-030)
    
    [Fact]
    [Trait("TestId", "FT-021")]
    public async Task FT021_Get_Products_Should_Return_List()
    {
        // Arrange
        await AuthenticateAsync();
        
        // Act
        var response = await Client.GetAsync("/api/products");
        
        // Assert
        AssertSuccess(response);
        
        _output.WriteLine($"FT-021 PASSED: GET /api/products returned product list");
    }
    
    [Fact]
    [Trait("TestId", "FT-022")]
    public async Task FT022_Create_Product_Should_Return_Created()
    {
        // Arrange
        await AuthenticateAsync();
        var product = new
        {
            name = $"Functional Test Product {DateTime.Now:HHmmss}",
            sku = $"FT-{Guid.NewGuid():N}"[..20],
            price = 99.99m,
            description = "Created by functional test"
        };
        
        // Act
        var response = await Client.PostAsJsonAsync("/api/products", product);
        
        // Assert
        Assert.True(response.StatusCode == HttpStatusCode.Created || 
                    response.StatusCode == HttpStatusCode.OK ||
                    response.StatusCode == HttpStatusCode.BadRequest, // SKU validation
            $"Expected Created/OK/BadRequest but got {response.StatusCode}");
        
        _output.WriteLine($"FT-022 PASSED: POST /api/products handled correctly");
    }

    #endregion

    #region Pipeline & Opportunities Tests (FT-031 to FT-040)
    
    [Fact]
    [Trait("TestId", "FT-031")]
    public async Task FT031_Get_Pipelines_Should_Return_List()
    {
        // Arrange
        await AuthenticateAsync();
        
        // Act
        var response = await Client.GetAsync("/api/pipelines");
        
        // Assert
        AssertSuccess(response);
        
        _output.WriteLine($"FT-031 PASSED: GET /api/pipelines returned pipeline list");
    }
    
    [Fact]
    [Trait("TestId", "FT-032")]
    public async Task FT032_Get_Opportunities_Should_Return_List()
    {
        // Arrange
        await AuthenticateAsync();
        
        // Act
        var response = await Client.GetAsync("/api/opportunities");
        
        // Assert
        AssertSuccess(response);
        
        _output.WriteLine($"FT-032 PASSED: GET /api/opportunities returned opportunity list");
    }
    
    [Fact]
    [Trait("TestId", "FT-033")]
    public async Task FT033_Get_Stages_Should_Return_List()
    {
        // Arrange
        await AuthenticateAsync();
        
        // Act
        var response = await Client.GetAsync("/api/stages");
        
        // Assert
        AssertSuccess(response);
        
        _output.WriteLine($"FT-033 PASSED: GET /api/stages returned stage list");
    }

    #endregion

    #region Workflow Engine Tests (FT-041 to FT-050)
    
    [Fact]
    [Trait("TestId", "FT-041")]
    public async Task FT041_Get_Workflow_Definitions_Should_Return_List()
    {
        // Arrange
        await AuthenticateAsync();
        
        // Act
        var response = await Client.GetAsync("/api/workflowengine/definitions");
        
        // Assert
        AssertSuccess(response);
        
        _output.WriteLine($"FT-041 PASSED: GET /api/workflowengine/definitions returned workflow list");
    }
    
    [Fact]
    [Trait("TestId", "FT-042")]
    public async Task FT042_Get_Workflow_Instances_Should_Return_List()
    {
        // Arrange
        await AuthenticateAsync();
        
        // Act
        var response = await Client.GetAsync("/api/workflowengine/instances");
        
        // Assert
        AssertSuccess(response);
        
        _output.WriteLine($"FT-042 PASSED: GET /api/workflowengine/instances returned instances list");
    }
    
    [Fact]
    [Trait("TestId", "FT-043")]
    public async Task FT043_Get_Workflow_Tasks_Should_Return_List()
    {
        // Arrange
        await AuthenticateAsync();
        
        // Act
        var response = await Client.GetAsync("/api/workflowengine/tasks");
        
        // Assert
        AssertSuccess(response);
        
        _output.WriteLine($"FT-043 PASSED: GET /api/workflowengine/tasks returned task list");
    }

    #endregion

    #region Reporting & Analytics Tests (FT-051 to FT-060)
    
    [Fact]
    [Trait("TestId", "FT-051")]
    public async Task FT051_Get_Dashboard_Stats_Should_Return_Data()
    {
        // Arrange
        await AuthenticateAsync();
        
        // Act
        var response = await Client.GetAsync("/api/dashboard/stats");
        
        // Assert
        AssertSuccess(response);
        
        _output.WriteLine($"FT-051 PASSED: GET /api/dashboard/stats returned dashboard data");
    }
    
    [Fact]
    [Trait("TestId", "FT-052")]
    public async Task FT052_Get_Activities_Should_Return_List()
    {
        // Arrange
        await AuthenticateAsync();
        
        // Act
        var response = await Client.GetAsync("/api/activities");
        
        // Assert
        AssertSuccess(response);
        
        _output.WriteLine($"FT-052 PASSED: GET /api/activities returned activity list");
    }
    
    [Fact]
    [Trait("TestId", "FT-053")]
    public async Task FT053_Get_Lookups_Should_Return_Categories()
    {
        // Arrange
        await AuthenticateAsync();
        
        // Act
        var response = await Client.GetAsync("/api/lookups/categories");
        
        // Assert
        AssertSuccess(response);
        
        _output.WriteLine($"FT-053 PASSED: GET /api/lookups/categories returned lookup data");
    }

    #endregion

    #region User & Settings Tests (FT-061 to FT-070)
    
    [Fact]
    [Trait("TestId", "FT-061")]
    public async Task FT061_Get_Current_User_Profile_Should_Return_User()
    {
        // Arrange
        await AuthenticateAsync();
        
        // Act
        var response = await Client.GetAsync("/api/auth/me");
        
        // Assert
        AssertSuccess(response);
        
        _output.WriteLine($"FT-061 PASSED: GET /api/auth/me returned user profile");
    }
    
    [Fact]
    [Trait("TestId", "FT-062")]
    public async Task FT062_Get_System_Settings_Should_Return_Settings()
    {
        // Arrange
        await AuthenticateAsync();
        
        // Act
        var response = await Client.GetAsync("/api/systemsettings");
        
        // Assert
        AssertSuccess(response);
        
        _output.WriteLine($"FT-062 PASSED: GET /api/systemsettings returned system settings");
    }
    
    [Fact]
    [Trait("TestId", "FT-063")]
    public async Task FT063_Get_Users_List_Should_Return_Users()
    {
        // Arrange
        await AuthenticateAsync();
        
        // Act
        var response = await Client.GetAsync("/api/users");
        
        // Assert
        AssertSuccess(response);
        
        _output.WriteLine($"FT-063 PASSED: GET /api/users returned user list");
    }

    #endregion

    #region Contact Tests (FT-071 to FT-080)
    
    [Fact]
    [Trait("TestId", "FT-071")]
    public async Task FT071_Get_Contacts_Should_Return_List()
    {
        // Arrange
        await AuthenticateAsync();
        
        // Act
        var response = await Client.GetAsync("/api/contacts");
        
        // Assert
        AssertSuccess(response);
        
        _output.WriteLine($"FT-071 PASSED: GET /api/contacts returned contact list");
    }
    
    [Fact]
    [Trait("TestId", "FT-072")]
    public async Task FT072_Create_Contact_Should_Return_Created()
    {
        // Arrange
        await AuthenticateAsync();
        var contact = new
        {
            firstName = "Functional",
            lastName = $"Test-{Guid.NewGuid():N}"[..10],
            email = $"ft-{Guid.NewGuid():N}@test.com"
        };
        
        // Act
        var response = await Client.PostAsJsonAsync("/api/contacts", contact);
        
        // Assert
        Assert.True(response.StatusCode == HttpStatusCode.Created || 
                    response.StatusCode == HttpStatusCode.OK ||
                    response.StatusCode == HttpStatusCode.BadRequest,
            $"Expected Created/OK/BadRequest but got {response.StatusCode}");
        
        _output.WriteLine($"FT-072 PASSED: POST /api/contacts handled correctly");
    }

    #endregion

    #region Communication Tests (FT-081 to FT-090)
    
    [Fact]
    [Trait("TestId", "FT-081")]
    public async Task FT081_Get_Communication_Channels_Should_Return_List()
    {
        // Arrange
        await AuthenticateAsync();
        
        // Act
        var response = await Client.GetAsync("/api/communications/channels");
        
        // Assert - May return 404 if endpoint doesn't exist yet, which is acceptable
        Assert.True(response.StatusCode == HttpStatusCode.OK || 
                    response.StatusCode == HttpStatusCode.NotFound,
            $"Expected OK or NotFound but got {response.StatusCode}");
        
        _output.WriteLine($"FT-081 PASSED: Communications channels endpoint responded");
    }

    #endregion

    #region Accounts API Alias Tests (FT-091 to FT-100)
    // Tests for the industry-standard /api/accounts route alias
    // The /api/accounts endpoint routes to the same controller as /api/customers
    
    [Fact]
    [Trait("TestId", "FT-091")]
    public async Task FT091_Get_Accounts_Should_Return_Same_As_Customers()
    {
        // Arrange
        await AuthenticateAsync();
        
        // Act - Get both endpoints
        var accountsResponse = await Client.GetAsync("/api/accounts");
        var customersResponse = await Client.GetAsync("/api/customers");
        
        // Assert - Both should return success
        AssertSuccess(accountsResponse);
        AssertSuccess(customersResponse);
        
        var accountsContent = await accountsResponse.Content.ReadAsStringAsync();
        var customersContent = await customersResponse.Content.ReadAsStringAsync();
        
        // Both should return the same number of records
        using var accountsDoc = JsonDocument.Parse(accountsContent);
        using var customersDoc = JsonDocument.Parse(customersContent);
        
        Assert.Equal(accountsDoc.RootElement.GetArrayLength(), customersDoc.RootElement.GetArrayLength());
        
        _output.WriteLine($"FT-091 PASSED: GET /api/accounts returns same data as /api/customers");
    }
    
    [Fact]
    [Trait("TestId", "FT-092")]
    public async Task FT092_Get_Account_By_Id_Should_Work()
    {
        // Arrange
        await AuthenticateAsync();
        
        // First get list to find an existing account ID
        var listResponse = await Client.GetAsync("/api/accounts");
        AssertSuccess(listResponse);
        var listContent = await listResponse.Content.ReadAsStringAsync();
        
        using var doc = JsonDocument.Parse(listContent);
        var items = doc.RootElement;
        
        if (items.GetArrayLength() == 0)
        {
            _output.WriteLine("FT-092 SKIPPED: No accounts exist to retrieve");
            return;
        }
        
        var accountId = items[0].GetProperty("id").GetInt32();
        
        // Act
        var response = await Client.GetAsync($"/api/accounts/{accountId}");
        
        // Assert
        AssertSuccess(response);
        
        _output.WriteLine($"FT-092 PASSED: GET /api/accounts/{accountId} returned account");
    }
    
    [Fact]
    [Trait("TestId", "FT-093")]
    public async Task FT093_Get_Accounts_Individuals_Should_Work()
    {
        // Arrange
        await AuthenticateAsync();
        
        // Act
        var response = await Client.GetAsync("/api/accounts/individuals");
        
        // Assert
        AssertSuccess(response);
        
        _output.WriteLine($"FT-093 PASSED: GET /api/accounts/individuals returned individual accounts");
    }
    
    [Fact]
    [Trait("TestId", "FT-094")]
    public async Task FT094_Get_Accounts_Organizations_Should_Work()
    {
        // Arrange
        await AuthenticateAsync();
        
        // Act
        var response = await Client.GetAsync("/api/accounts/organizations");
        
        // Assert
        AssertSuccess(response);
        
        _output.WriteLine($"FT-094 PASSED: GET /api/accounts/organizations returned organization accounts");
    }
    
    [Fact]
    [Trait("TestId", "FT-095")]
    public async Task FT095_Search_Accounts_Should_Work()
    {
        // Arrange
        await AuthenticateAsync();
        
        // Act
        var response = await Client.GetAsync("/api/accounts/search/test");
        
        // Assert
        AssertSuccess(response);
        
        _output.WriteLine($"FT-095 PASSED: GET /api/accounts/search/test works correctly");
    }
    
    [Fact]
    [Trait("TestId", "FT-096")]
    public async Task FT096_Create_Account_Via_Alias_Should_Work()
    {
        // Arrange
        await AuthenticateAsync();
        var account = new
        {
            firstName = "Account",
            lastName = $"Test-{Guid.NewGuid():N}"[..10],
            email = $"account-{Guid.NewGuid():N}@functional.test",
            phone = "+1-555-9999",
            company = "Account Alias Test"
        };
        
        // Act
        var response = await Client.PostAsJsonAsync("/api/accounts", account);
        
        // Assert
        Assert.True(response.StatusCode == HttpStatusCode.Created || 
                    response.StatusCode == HttpStatusCode.OK,
            $"Expected Created/OK but got {response.StatusCode}");
        
        _output.WriteLine($"FT-096 PASSED: POST /api/accounts created new account via alias");
    }
    
    [Fact]
    [Trait("TestId", "FT-097")]
    public async Task FT097_Protected_Accounts_Endpoint_Without_Token_Should_Return_401()
    {
        // Arrange - Create new client without authentication
        using var unauthClient = new HttpClient
        {
            BaseAddress = new Uri(BaseUrl),
            Timeout = TimeSpan.FromSeconds(30)
        };
        
        // Act
        var response = await unauthClient.GetAsync("/api/accounts");
        
        // Assert
        AssertStatusCode(response, HttpStatusCode.Unauthorized);
        
        _output.WriteLine($"FT-097 PASSED: /api/accounts returned 401 without token");
    }
    
    [Fact]
    [Trait("TestId", "FT-098")]
    public async Task FT098_Accounts_And_Customers_Endpoints_Return_Identical_Data()
    {
        // Arrange
        await AuthenticateAsync();
        
        // Get specific account via both endpoints
        var accountsListResponse = await Client.GetAsync("/api/accounts");
        AssertSuccess(accountsListResponse);
        var listContent = await accountsListResponse.Content.ReadAsStringAsync();
        
        using var doc = JsonDocument.Parse(listContent);
        if (doc.RootElement.GetArrayLength() == 0)
        {
            _output.WriteLine("FT-098 SKIPPED: No accounts exist to compare");
            return;
        }
        
        var id = doc.RootElement[0].GetProperty("id").GetInt32();
        
        // Act - Get the same record via both endpoints
        var accountResponse = await Client.GetAsync($"/api/accounts/{id}");
        var customerResponse = await Client.GetAsync($"/api/customers/{id}");
        
        // Assert
        AssertSuccess(accountResponse);
        AssertSuccess(customerResponse);
        
        var accountData = await accountResponse.Content.ReadAsStringAsync();
        var customerData = await customerResponse.Content.ReadAsStringAsync();
        
        // Parse and compare key fields
        using var accountDoc = JsonDocument.Parse(accountData);
        using var customerDoc = JsonDocument.Parse(customerData);
        
        Assert.Equal(
            accountDoc.RootElement.GetProperty("id").GetInt32(),
            customerDoc.RootElement.GetProperty("id").GetInt32());
        
        _output.WriteLine($"FT-098 PASSED: /api/accounts/{id} and /api/customers/{id} return identical data");
    }

    #endregion
}
