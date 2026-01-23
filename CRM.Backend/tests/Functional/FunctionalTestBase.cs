using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace CRM.Tests.Functional;

/// <summary>
/// Base class for functional tests that run against a live API instance.
/// </summary>
public abstract class FunctionalTestBase : IAsyncLifetime
{
    protected HttpClient Client { get; private set; } = null!;
    protected string? AuthToken { get; private set; }
    
    protected virtual string BaseUrl => "http://localhost:5000";
    
    public virtual async Task InitializeAsync()
    {
        Client = new HttpClient
        {
            BaseAddress = new Uri(BaseUrl),
            Timeout = TimeSpan.FromSeconds(30)
        };
        
        // Check if API is available
        var healthCheck = await Client.GetAsync("/health");
        if (!healthCheck.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"API at {BaseUrl} is not available. Make sure the server is running. " +
                $"Status: {healthCheck.StatusCode}");
        }
    }

    public virtual Task DisposeAsync()
    {
        Client?.Dispose();
        return Task.CompletedTask;
    }
    
    /// <summary>
    /// Authenticate with the API and get a JWT token.
    /// </summary>
    protected async Task<bool> AuthenticateAsync(string email = "abhi.lal@gmail.com", string password = "Admin@123")
    {
        var response = await Client.PostAsJsonAsync("/api/auth/login", new
        {
            email,
            password
        });
        
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
            if (result?.Token != null)
            {
                AuthToken = result.Token;
                Client.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", AuthToken);
                return true;
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// Asserts that a response indicates success.
    /// </summary>
    protected static void AssertSuccess(HttpResponseMessage response)
    {
        Assert.True(response.IsSuccessStatusCode, 
            $"Expected success but got {response.StatusCode}: {response.ReasonPhrase}");
    }
    
    /// <summary>
    /// Asserts that a response has a specific status code.
    /// </summary>
    protected static void AssertStatusCode(HttpResponseMessage response, System.Net.HttpStatusCode expected)
    {
        Assert.Equal(expected, response.StatusCode);
    }
    
    private record LoginResponse(string Token, string RefreshToken, DateTime ExpiresAt);
}

/// <summary>
/// Test result tracking for functional test runs.
/// </summary>
public class FunctionalTestResult
{
    public string TestName { get; set; } = string.Empty;
    public bool Passed { get; set; }
    public string? ErrorMessage { get; set; }
    public TimeSpan Duration { get; set; }
    public int StatusCode { get; set; }
}
