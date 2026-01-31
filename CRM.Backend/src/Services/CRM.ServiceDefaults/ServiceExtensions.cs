using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Net;
using System.Text;
using System.Text.Json;

namespace CRM.ServiceDefaults;

/// <summary>
/// Extension methods for configuring common service defaults across all microservices
/// </summary>
public static class ServiceExtensions
{
    /// <summary>
    /// Add common service defaults including logging, auth, CORS, and health checks
    /// </summary>
    public static WebApplicationBuilder AddServiceDefaults(this WebApplicationBuilder builder)
    {
        // Configure Serilog
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("ServiceName", builder.Environment.ApplicationName)
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{ServiceName}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        builder.Host.UseSerilog();

        // Add CORS
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });

        // Add JWT Authentication
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

        // Add memory cache - required by AuthenticationService and response caching
        builder.Services.AddMemoryCache();

        // Add response caching
        builder.Services.AddResponseCaching();

        // Add output caching for better performance
        builder.Services.AddOutputCache(options =>
        {
            options.AddBasePolicy(builder => builder.Expire(TimeSpan.FromMinutes(5)));
            options.AddPolicy("NoCache", builder => builder.NoCache());
            options.AddPolicy("ShortCache", builder => builder.Expire(TimeSpan.FromSeconds(30)));
            options.AddPolicy("MediumCache", builder => builder.Expire(TimeSpan.FromMinutes(5)));
            options.AddPolicy("LongCache", builder => builder.Expire(TimeSpan.FromMinutes(30)));
        });

        // Add health checks
        builder.Services.AddHealthChecks();

        // Add controllers
        builder.Services.AddControllers();

        // Add Swagger
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        return builder;
    }

    /// <summary>
    /// Configure common middleware pipeline
    /// </summary>
    public static WebApplication UseServiceDefaults(this WebApplication app)
    {
        // Global exception handler - must be first
        app.UseExceptionHandler(errorApp =>
        {
            errorApp.Run(async context =>
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = "application/json";

                var exceptionFeature = context.Features.Get<IExceptionHandlerFeature>();
                if (exceptionFeature != null)
                {
                    var exception = exceptionFeature.Error;
                    Log.Error(exception, "Unhandled exception occurred: {Message}", exception.Message);

                    var error = new
                    {
                        error = app.Environment.IsDevelopment() 
                            ? exception.Message 
                            : "An internal server error occurred",
                        traceId = context.TraceIdentifier,
                        timestamp = DateTime.UtcNow
                    };

                    await context.Response.WriteAsync(
                        JsonSerializer.Serialize(error, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })
                    );
                }
            });
        });

        app.UseSerilogRequestLogging();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseCors("AllowAll");

        // Response caching for GET requests
        app.UseResponseCaching();
        app.UseOutputCache();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapHealthChecks("/health");
        app.MapControllers();

        return app;
    }
}
