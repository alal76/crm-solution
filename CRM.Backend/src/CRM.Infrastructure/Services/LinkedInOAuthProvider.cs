using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CRM.Infrastructure.Services;

/// <summary>
/// OAuth provider for LinkedIn authentication.
/// Implements professional profile-based sign-in.
/// </summary>
public class LinkedInOAuthProvider
{
    private readonly HttpClient _httpClient;
    private readonly LinkedInOAuthOptions _options;
    private readonly ILogger<LinkedInOAuthProvider> _logger;

    private const string AuthorizeUrl = "https://www.linkedin.com/oauth/v2/authorization";
    private const string TokenUrl = "https://www.linkedin.com/oauth/v2/accessToken";
    private const string ProfileUrl = "https://api.linkedin.com/v2/me";
    private const string EmailUrl = "https://api.linkedin.com/v2/emailAddress?q=members&projection=(elements*(handle~))";

    public LinkedInOAuthProvider(
        HttpClient httpClient,
        IOptions<LinkedInOAuthOptions> options,
        ILogger<LinkedInOAuthProvider> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get the LinkedIn authorization URL.
    /// </summary>
    public string GetAuthorizationUrl(string state, string redirectUri)
    {
        var scopes = string.Join("%20", _options.Scopes ?? new[] { "r_liteprofile", "r_emailaddress" });
        return $"{AuthorizeUrl}?response_type=code&client_id={_options.ClientId}&redirect_uri={Uri.EscapeDataString(redirectUri)}&scope={scopes}&state={state}";
    }

    /// <summary>
    /// Exchange authorization code for access token.
    /// </summary>
    public async Task<LinkedInTokenResponse> ExchangeCodeForTokenAsync(
        string code,
        string redirectUri,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new LinkedInTokenRequest
            {
                grant_type = "authorization_code",
                code = code,
                redirect_uri = redirectUri,
                client_id = _options.ClientId,
                client_secret = _options.ClientSecret
            };

            var response = await _httpClient.PostAsJsonAsync(TokenUrl, request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var tokenResponse = await response.Content.ReadFromJsonAsync<LinkedInTokenResponse>(cancellationToken);
            _logger.LogInformation("Successfully exchanged LinkedIn auth code for access token");
            return tokenResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to exchange LinkedIn auth code for token");
            throw;
        }
    }

    /// <summary>
    /// Get LinkedIn user profile using access token.
    /// </summary>
    public async Task<LinkedInUserProfile> GetUserProfileAsync(
        string accessToken,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Get basic profile
            var request = new HttpRequestMessage(HttpMethod.Get, ProfileUrl);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var profile = await response.Content.ReadFromJsonAsync<LinkedInUserProfile>(cancellationToken);

            // Get email address
            var emailRequest = new HttpRequestMessage(HttpMethod.Get, EmailUrl);
            emailRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            try
            {
                var emailResponse = await _httpClient.SendAsync(emailRequest, cancellationToken);
                if (emailResponse.IsSuccessStatusCode)
                {
                    var emailData = await emailResponse.Content.ReadFromJsonAsync<LinkedInEmailResponse>(cancellationToken);
                    profile.Email = emailData?.elements?.FirstOrDefault()?.handle?.emailAddress;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to retrieve email from LinkedIn profile");
            }

            _logger.LogInformation($"Successfully retrieved LinkedIn profile for user {profile.id}");
            return profile;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get LinkedIn user profile");
            throw;
        }
    }

    /// <summary>
    /// Refresh access token.
    /// </summary>
    public async Task<LinkedInTokenResponse> RefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new LinkedInRefreshRequest
            {
                grant_type = "refresh_token",
                refresh_token = refreshToken,
                client_id = _options.ClientId,
                client_secret = _options.ClientSecret
            };

            var response = await _httpClient.PostAsJsonAsync(TokenUrl, request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var tokenResponse = await response.Content.ReadFromJsonAsync<LinkedInTokenResponse>(cancellationToken);
            _logger.LogInformation("Successfully refreshed LinkedIn access token");
            return tokenResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to refresh LinkedIn token");
            throw;
        }
    }
}

/// <summary>
/// Configuration options for LinkedIn OAuth provider.
/// </summary>
public class LinkedInOAuthOptions
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string[] Scopes { get; set; } = { "r_liteprofile", "r_emailaddress" };

    public void Validate()
    {
        if (string.IsNullOrEmpty(ClientId))
            throw new InvalidOperationException("LinkedIn ClientId is required");
        if (string.IsNullOrEmpty(ClientSecret))
            throw new InvalidOperationException("LinkedIn ClientSecret is required");
    }
}

/// <summary>
/// LinkedIn OAuth token response.
/// </summary>
public class LinkedInTokenResponse
{
    [JsonPropertyName("access_token")]
    public string access_token { get; set; } = string.Empty;

    [JsonPropertyName("expires_in")]
    public int expires_in { get; set; }

    [JsonPropertyName("refresh_token")]
    public string? refresh_token { get; set; }

    [JsonPropertyName("refresh_token_expires_in")]
    public int? refresh_token_expires_in { get; set; }
}

/// <summary>
/// Request to exchange authorization code for token.
/// </summary>
public class LinkedInTokenRequest
{
    [JsonPropertyName("grant_type")]
    public string grant_type { get; set; } = "authorization_code";

    [JsonPropertyName("code")]
    public string code { get; set; } = string.Empty;

    [JsonPropertyName("redirect_uri")]
    public string redirect_uri { get; set; } = string.Empty;

    [JsonPropertyName("client_id")]
    public string client_id { get; set; } = string.Empty;

    [JsonPropertyName("client_secret")]
    public string client_secret { get; set; } = string.Empty;
}

/// <summary>
/// Request to refresh access token.
/// </summary>
public class LinkedInRefreshRequest
{
    [JsonPropertyName("grant_type")]
    public string grant_type { get; set; } = "refresh_token";

    [JsonPropertyName("refresh_token")]
    public string refresh_token { get; set; } = string.Empty;

    [JsonPropertyName("client_id")]
    public string client_id { get; set; } = string.Empty;

    [JsonPropertyName("client_secret")]
    public string client_secret { get; set; } = string.Empty;
}

/// <summary>
/// LinkedIn user profile response.
/// </summary>
public class LinkedInUserProfile
{
    [JsonPropertyName("id")]
    public string id { get; set; } = string.Empty;

    [JsonPropertyName("localizedFirstName")]
    public string? FirstName { get; set; }

    [JsonPropertyName("localizedLastName")]
    public string? LastName { get; set; }

    public string? Email { get; set; }

    [JsonPropertyName("profilePicture")]
    public LinkedInProfilePicture? ProfilePicture { get; set; }
}

/// <summary>
/// LinkedIn profile picture details.
/// </summary>
public class LinkedInProfilePicture
{
    [JsonPropertyName("displayImage")]
    public string? DisplayImage { get; set; }
}

/// <summary>
/// LinkedIn email response wrapper.
/// </summary>
public class LinkedInEmailResponse
{
    [JsonPropertyName("elements")]
    public LinkedInEmailElement[]? elements { get; set; }
}

/// <summary>
/// LinkedIn email element.
/// </summary>
public class LinkedInEmailElement
{
    [JsonPropertyName("handle~")]
    public LinkedInEmailHandle? handle { get; set; }
}

/// <summary>
/// LinkedIn email handle.
/// </summary>
public class LinkedInEmailHandle
{
    [JsonPropertyName("emailAddress")]
    public string? emailAddress { get; set; }
}
