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

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Add YARP Reverse Proxy
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Add CORS - environment-aware configuration
var configuredOrigins = builder.Configuration["AllowedOrigins"]?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
    ?? Array.Empty<string>();
var isProduction = builder.Environment.IsProduction();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Default", policy =>
    {
        if (isProduction && configuredOrigins.Length > 0)
        {
            // PRODUCTION: Strict whitelist only
            policy.WithOrigins(configuredOrigins)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        }
        else
        {
            // DEVELOPMENT/STAGING: Permissive for local development
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        }
    });
});

// Add JWT Authentication (pass-through for validation)
// JWT_KEY environment variable is REQUIRED - no hardcoded fallback for security
var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY")
    ?? builder.Configuration["JwtSettings:Key"]
    ?? throw new InvalidOperationException("JWT_KEY environment variable or JwtSettings:Key configuration is required");
var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER")
    ?? builder.Configuration["JwtSettings:Issuer"]
    ?? "CRM.Api";
var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE")
    ?? builder.Configuration["JwtSettings:Audience"]
    ?? "CRM.Client";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

// Add rate limiting - configurable via appsettings.json
var isDevelopment = builder.Environment.IsDevelopment();
var rateLimitingConfig = builder.Configuration.GetSection("RateLimiting");
var rateLimitingEnabled = rateLimitingConfig.GetValue("EnableEndpointRateLimiting", !isDevelopment);
var rejectionStatusCode = rateLimitingConfig.GetValue("HttpStatusCode", 429);
var quotaMessage = rateLimitingConfig.GetValue("QuotaExceededMessage", "API calls quota exceeded!");

TimeSpan ParseRateLimitPeriod(string period)
{
    if (string.IsNullOrWhiteSpace(period))
    {
        return TimeSpan.FromMinutes(1);
    }

    var normalized = period.Trim().ToLowerInvariant();
    if (normalized.EndsWith("ms") && int.TryParse(normalized[..^2], out var ms))
    {
        return TimeSpan.FromMilliseconds(ms);
    }
    if (normalized.EndsWith("s") && int.TryParse(normalized[..^1], out var seconds))
    {
        return TimeSpan.FromSeconds(seconds);
    }
    if (normalized.EndsWith("m") && int.TryParse(normalized[..^1], out var minutes))
    {
        return TimeSpan.FromMinutes(minutes);
    }
    if (normalized.EndsWith("h") && int.TryParse(normalized[..^1], out var hours))
    {
        return TimeSpan.FromHours(hours);
    }

    return TimeSpan.FromMinutes(1);
}

var generalRuleSection = rateLimitingConfig.GetSection("GeneralRules").GetChildren().FirstOrDefault();
var generalPermitLimit = generalRuleSection?.GetValue("Limit", 1000) ?? 1000;
var generalWindow = ParseRateLimitPeriod(generalRuleSection?.GetValue("Period", "1m") ?? "1m");

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = rejectionStatusCode;
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.ContentType = "text/plain";
        await context.HttpContext.Response.WriteAsync(quotaMessage, token);
    };

    if (!rateLimitingEnabled)
    {
        options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(_ =>
            RateLimitPartition.GetNoLimiter("rate-limiting-disabled"));
        return;
    }

    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
        var partitionKey = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey,
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = generalPermitLimit,
                Window = generalWindow,
                QueueLimit = 0,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst
            });
    });
});

// Health checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseSerilogRequestLogging();

app.UseCors("Default");

app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

// Health check endpoint
app.MapHealthChecks("/health");

// Map YARP reverse proxy
app.MapReverseProxy();

Log.Information("CRM API Gateway starting on port 5000");
app.Run();
