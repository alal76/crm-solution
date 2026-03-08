// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Serilog;

namespace CRM.Api.Infrastructure;

/// <summary>
/// AP-039: JWT authentication and authorization service registrations extracted from Program.cs.
/// Configures the SmartScheme (Bearer JWT + API Key), JWT bearer validation parameters,
/// and the default authorization fallback policy.
/// </summary>
internal static class JwtAuthServiceExtensions
{
    /// <summary>
    /// Configures JWT bearer authentication, API-key scheme, and authorization policies.
    /// Extracted from Program.cs (AP-039) — behavior is functionally identical.
    /// </summary>
    internal static IServiceCollection AddJwtAuthServices(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        // Configure JWT Authentication
        var jwtSecret = configuration["Jwt:Secret"];
        if (string.IsNullOrEmpty(jwtSecret) || jwtSecret.Length < 32)
        {
            // Use a secure default for development only - in production, this should be configured
            if (environment.IsDevelopment())
            {
                jwtSecret = "development-only-jwt-secret-key-minimum-32-chars";
                Log.Warning("Using development JWT secret. Configure Jwt:Secret for production.");
            }
            else
            {
                throw new InvalidOperationException("JWT Secret must be configured in production. Set 'Jwt:Secret' with a secure key at least 32 characters long.");
            }
        }

        var jwtIssuer = configuration["Jwt:Issuer"] ?? "CRMApp";
        var jwtAudience = configuration["Jwt:Audience"] ?? "CRMUsers";
        var key = Encoding.UTF8.GetBytes(jwtSecret);

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = "SmartScheme";
            options.DefaultChallengeScheme = "SmartScheme";
        })
        .AddPolicyScheme("SmartScheme", "Bearer or API Key", options =>
        {
            options.ForwardDefaultSelector = context =>
            {
                // If X-Api-Key header is present, use ApiKey scheme; otherwise use Bearer JWT
                if (context.Request.Headers.ContainsKey("X-Api-Key"))
                    return CRM.Infrastructure.Authentication.ApiKeyAuthenticationHandler.SchemeName;
                return "Bearer";
            };
        })
        .AddJwtBearer("Bearer", options =>
        {
            // Require HTTPS in production, allow HTTP in development
            options.RequireHttpsMetadata = !environment.IsDevelopment();
            options.SaveToken = true;

            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = ctx =>
                {
                    Log.Warning(ctx.Exception, "JWT authentication failed");
                    return Task.CompletedTask;
                }
            };

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = jwtIssuer,
                ValidateAudience = true,
                ValidAudience = jwtAudience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        })
        .AddScheme<CRM.Infrastructure.Authentication.ApiKeyAuthenticationOptions,
            CRM.Infrastructure.Authentication.ApiKeyAuthenticationHandler>(
            CRM.Infrastructure.Authentication.ApiKeyAuthenticationHandler.SchemeName, options => { });

        // Add Authorization policies
        // Default policy: Authenticated users only (accepts both Bearer JWT and ApiKey)
        services.AddAuthorization(options =>
        {
            // Default policy requires authentication (SmartScheme auto-selects Bearer or ApiKey)
            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
        });

        return services;
    }
}
