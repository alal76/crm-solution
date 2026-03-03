// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using CRM.Infrastructure.Providers.Superset;
using Microsoft.Extensions.Options;
using Xunit;

namespace CRM.Tests.Validators
{
    /// <summary>
    /// Comprehensive unit tests for <see cref="SupersetConfigurationValidator"/>.
    /// Tests validation of Apache Superset analytics provider configuration.
    /// </summary>
    public class SupersetConfigurationValidatorTests
    {
        // ═══════════════════════════════════════════════════════════════════════════
        // SupersetConfiguration.Validate() Tests
        // ═══════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Verifies that valid configuration passes validation.
        /// </summary>
        [Fact]
        public void Validate_ShouldReturnTrue_WhenConfigurationIsValid()
        {
            // Arrange
            var config = new SupersetConfiguration
            {
                BaseUrl = "https://superset.example.com",
                Username = "admin",
                Password = "password123"
            };

            // Act
            var (isValid, error) = config.Validate();

            // Assert
            Assert.True(isValid);
            Assert.Null(error);
        }

        /// <summary>
        /// Verifies that missing BaseUrl fails validation.
        /// </summary>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("   ")]
        public void Validate_ShouldReturnFalse_WhenBaseUrlIsNullOrEmpty(string baseUrl)
        {
            // Arrange
            var config = new SupersetConfiguration
            {
                BaseUrl = baseUrl,
                Username = "admin",
                Password = "password123"
            };

            // Act
            var (isValid, error) = config.Validate();

            // Assert
            Assert.False(isValid);
            Assert.NotNull(error);
            Assert.Contains("BaseUrl", error);
            Assert.Contains("required", error, System.StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Verifies that missing Username fails validation.
        /// </summary>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("   ")]
        public void Validate_ShouldReturnFalse_WhenUsernameIsNullOrEmpty(string username)
        {
            // Arrange
            var config = new SupersetConfiguration
            {
                BaseUrl = "https://superset.example.com",
                Username = username,
                Password = "password123"
            };

            // Act
            var (isValid, error) = config.Validate();

            // Assert
            Assert.False(isValid);
            Assert.NotNull(error);
            Assert.Contains("Username", error);
            Assert.Contains("required", error, System.StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Verifies that missing Password fails validation.
        /// </summary>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("   ")]
        public void Validate_ShouldReturnFalse_WhenPasswordIsNullOrEmpty(string password)
        {
            // Arrange
            var config = new SupersetConfiguration
            {
                BaseUrl = "https://superset.example.com",
                Username = "admin",
                Password = password
            };

            // Act
            var (isValid, error) = config.Validate();

            // Assert
            Assert.False(isValid);
            Assert.NotNull(error);
            Assert.Contains("Password", error);
            Assert.Contains("required", error, System.StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Verifies that invalid URL format fails validation.
        /// </summary>
        [Theory]
        [InlineData("not-a-url")]
        [InlineData("ftp://invalid")]
        [InlineData("htp://typo")]
        [InlineData("superset.com")]
        [InlineData("://missing-scheme")]
        public void Validate_ShouldReturnFalse_WhenBaseUrlIsInvalid(string baseUrl)
        {
            // Arrange
            var config = new SupersetConfiguration
            {
                BaseUrl = baseUrl,
                Username = "admin",
                Password = "password123"
            };

            // Act
            var (isValid, error) = config.Validate();

            // Assert
            Assert.False(isValid);
            Assert.NotNull(error);
            Assert.Contains("not a valid URL", error, System.StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Verifies that HTTP URL is valid.
        /// </summary>
        [Fact]
        public void Validate_ShouldReturnTrue_WhenHttpUrl()
        {
            // Arrange
            var config = new SupersetConfiguration
            {
                BaseUrl = "http://localhost:8088",
                Username = "admin",
                Password = "password123"
            };

            // Act
            var (isValid, error) = config.Validate();

            // Assert
            Assert.True(isValid);
            Assert.Null(error);
        }

        /// <summary>
        /// Verifies that HTTPS URL is valid.
        /// </summary>
        [Fact]
        public void Validate_ShouldReturnTrue_WhenHttpsUrl()
        {
            // Arrange
            var config = new SupersetConfiguration
            {
                BaseUrl = "https://superset.example.com",
                Username = "admin",
                Password = "password123"
            };

            // Act
            var (isValid, error) = config.Validate();

            // Assert
            Assert.True(isValid);
            Assert.Null(error);
        }

        /// <summary>
        /// Verifies that URL with port number is valid.
        /// </summary>
        [Theory]
        [InlineData("http://localhost:8088")]
        [InlineData("https://superset.example.com:443")]
        [InlineData("http://192.168.1.100:8080")]
        public void Validate_ShouldReturnTrue_WhenUrlWithPort(string baseUrl)
        {
            // Arrange
            var config = new SupersetConfiguration
            {
                BaseUrl = baseUrl,
                Username = "admin",
                Password = "password123"
            };

            // Act
            var (isValid, error) = config.Validate();

            // Assert
            Assert.True(isValid);
            Assert.Null(error);
        }

        /// <summary>
        /// Verifies that URL with path is valid.
        /// </summary>
        [Theory]
        [InlineData("https://example.com/superset")]
        [InlineData("http://localhost:8088/api")]
        public void Validate_ShouldReturnTrue_WhenUrlWithPath(string baseUrl)
        {
            // Arrange
            var config = new SupersetConfiguration
            {
                BaseUrl = baseUrl,
                Username = "admin",
                Password = "password123"
            };

            // Act
            var (isValid, error) = config.Validate();

            // Assert
            Assert.True(isValid);
            Assert.Null(error);
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // SupersetConfigurationValidator.Validate() Tests
        // ═══════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Verifies that validator returns success for valid configuration.
        /// </summary>
        [Fact]
        public void ValidatorValidate_ShouldReturnSuccess_WhenConfigurationIsValid()
        {
            // Arrange
            var validator = new SupersetConfigurationValidator();
            var config = new SupersetConfiguration
            {
                BaseUrl = "https://superset.example.com",
                Username = "admin",
                Password = "password123"
            };

            // Act
            var result = validator.Validate(null, config);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Succeeded);
        }

        /// <summary>
        /// Verifies that validator returns failure for missing BaseUrl.
        /// </summary>
        [Fact]
        public void ValidatorValidate_ShouldReturnFailure_WhenBaseUrlMissing()
        {
            // Arrange
            var validator = new SupersetConfigurationValidator();
            var config = new SupersetConfiguration
            {
                BaseUrl = "",
                Username = "admin",
                Password = "password123"
            };

            // Act
            var result = validator.Validate(null, config);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Succeeded);
            Assert.NotNull(result.FailureMessage);
            Assert.Contains("BaseUrl", result.FailureMessage);
        }

        /// <summary>
        /// Verifies that validator returns failure for missing Username.
        /// </summary>
        [Fact]
        public void ValidatorValidate_ShouldReturnFailure_WhenUsernameMissing()
        {
            // Arrange
            var validator = new SupersetConfigurationValidator();
            var config = new SupersetConfiguration
            {
                BaseUrl = "https://superset.example.com",
                Username = "",
                Password = "password123"
            };

            // Act
            var result = validator.Validate(null, config);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Succeeded);
            Assert.NotNull(result.FailureMessage);
            Assert.Contains("Username", result.FailureMessage);
        }

        /// <summary>
        /// Verifies that validator returns failure for missing Password.
        /// </summary>
        [Fact]
        public void ValidatorValidate_ShouldReturnFailure_WhenPasswordMissing()
        {
            // Arrange
            var validator = new SupersetConfigurationValidator();
            var config = new SupersetConfiguration
            {
                BaseUrl = "https://superset.example.com",
                Username = "admin",
                Password = ""
            };

            // Act
            var result = validator.Validate(null, config);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Succeeded);
            Assert.NotNull(result.FailureMessage);
            Assert.Contains("Password", result.FailureMessage);
        }

        /// <summary>
        /// Verifies that validator returns failure for invalid URL.
        /// </summary>
        [Fact]
        public void ValidatorValidate_ShouldReturnFailure_WhenBaseUrlInvalid()
        {
            // Arrange
            var validator = new SupersetConfigurationValidator();
            var config = new SupersetConfiguration
            {
                BaseUrl = "not-a-valid-url",
                Username = "admin",
                Password = "password123"
            };

            // Act
            var result = validator.Validate(null, config);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Succeeded);
            Assert.NotNull(result.FailureMessage);
            Assert.Contains("not a valid URL", result.FailureMessage, System.StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Verifies that validator name parameter is optional (can be null).
        /// </summary>
        [Fact]
        public void ValidatorValidate_ShouldHandleNullName()
        {
            // Arrange
            var validator = new SupersetConfigurationValidator();
            var config = new SupersetConfiguration
            {
                BaseUrl = "https://superset.example.com",
                Username = "admin",
                Password = "password123"
            };

            // Act
            var result = validator.Validate(null, config);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Succeeded);
        }

        /// <summary>
        /// Verifies that validator works with named options.
        /// </summary>
        [Fact]
        public void ValidatorValidate_ShouldWorkWithNamedOptions()
        {
            // Arrange
            var validator = new SupersetConfigurationValidator();
            var config = new SupersetConfiguration
            {
                BaseUrl = "https://superset.example.com",
                Username = "admin",
                Password = "password123"
            };

            // Act
            var result = validator.Validate("SupersetOptions", config);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Succeeded);
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // SupersetConfiguration Properties Tests
        // ═══════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Verifies that default property values are set correctly.
        /// </summary>
        [Fact]
        public void SupersetConfiguration_ShouldHaveDefaultValues()
        {
            // Arrange & Act
            var config = new SupersetConfiguration();

            // Assert
            Assert.Equal(string.Empty, config.BaseUrl);
            Assert.Equal(string.Empty, config.Username);
            Assert.Equal(string.Empty, config.Password);
            Assert.Equal("db", config.Provider);
            Assert.True(config.AutoRefreshToken);
            Assert.Equal(50, config.TokenRefreshIntervalMinutes);
            Assert.NotNull(config.GuestToken);
            Assert.NotNull(config.DefaultRlsFilters);
            Assert.NotNull(config.DashboardMappings);
            Assert.NotNull(config.ChartMappings);
            Assert.Equal(30, config.TimeoutSeconds);
            Assert.False(config.SkipSslValidation);
        }

        /// <summary>
        /// Verifies that SectionName constant is correct.
        /// </summary>
        [Fact]
        public void SupersetConfiguration_ShouldHaveCorrectSectionName()
        {
            // Assert
            Assert.Equal("Providers:Analytics:Superset", SupersetConfiguration.SectionName);
        }

        /// <summary>
        /// Verifies that GuestToken has default values.
        /// </summary>
        [Fact]
        public void GuestTokenSettings_ShouldHaveDefaultValues()
        {
            // Arrange & Act
            var settings = new GuestTokenSettings();

            // Assert
            Assert.Equal(300, settings.DefaultExpirationMinutes);
            Assert.NotNull(settings.AllowedDomains);
            Assert.Empty(settings.AllowedDomains);
            Assert.NotNull(settings.AllowedResourceTypes);
            Assert.Single(settings.AllowedResourceTypes);
            Assert.Contains("dashboard", settings.AllowedResourceTypes);
            Assert.False(settings.AllowAllDatasets);
        }

        /// <summary>
        /// Verifies that configuration properties can be set and retrieved.
        /// </summary>
        [Fact]
        public void SupersetConfiguration_ShouldAllowPropertyModification()
        {
            // Arrange
            var config = new SupersetConfiguration();

            // Act
            config.BaseUrl = "https://test.example.com";
            config.Username = "testuser";
            config.Password = "testpass";
            config.Provider = "ldap";
            config.AutoRefreshToken = false;
            config.TokenRefreshIntervalMinutes = 60;
            config.TimeoutSeconds = 45;
            config.SkipSslValidation = true;

            // Assert
            Assert.Equal("https://test.example.com", config.BaseUrl);
            Assert.Equal("testuser", config.Username);
            Assert.Equal("testpass", config.Password);
            Assert.Equal("ldap", config.Provider);
            Assert.False(config.AutoRefreshToken);
            Assert.Equal(60, config.TokenRefreshIntervalMinutes);
            Assert.Equal(45, config.TimeoutSeconds);
            Assert.True(config.SkipSslValidation);
        }

        /// <summary>
        /// Verifies that dashboard mappings can be added.
        /// </summary>
        [Fact]
        public void SupersetConfiguration_ShouldAllowDashboardMappings()
        {
            // Arrange
            var config = new SupersetConfiguration();

            // Act
            config.DashboardMappings["SalesDashboard"] = 1;
            config.DashboardMappings["MarketingDashboard"] = 2;

            // Assert
            Assert.Equal(2, config.DashboardMappings.Count);
            Assert.Equal(1, config.DashboardMappings["SalesDashboard"]);
            Assert.Equal(2, config.DashboardMappings["MarketingDashboard"]);
        }

        /// <summary>
        /// Verifies that chart mappings can be added.
        /// </summary>
        [Fact]
        public void SupersetConfiguration_ShouldAllowChartMappings()
        {
            // Arrange
            var config = new SupersetConfiguration();

            // Act
            config.ChartMappings["RevenueChart"] = 101;
            config.ChartMappings["LeadsChart"] = 102;

            // Assert
            Assert.Equal(2, config.ChartMappings.Count);
            Assert.Equal(101, config.ChartMappings["RevenueChart"]);
            Assert.Equal(102, config.ChartMappings["LeadsChart"]);
        }

        /// <summary>
        /// Verifies that RLS filters can be added.
        /// </summary>
        [Fact]
        public void SupersetConfiguration_ShouldAllowRlsFilters()
        {
            // Arrange
            var config = new SupersetConfiguration();

            // Act
            config.DefaultRlsFilters["user_id"] = "{userId}";
            config.DefaultRlsFilters["tenant_id"] = "{tenantId}";

            // Assert
            Assert.Equal(2, config.DefaultRlsFilters.Count);
            Assert.Equal("{userId}", config.DefaultRlsFilters["user_id"]);
            Assert.Equal("{tenantId}", config.DefaultRlsFilters["tenant_id"]);
        }

        /// <summary>
        /// Verifies that guest token settings can be modified.
        /// </summary>
        [Fact]
        public void GuestTokenSettings_ShouldAllowModification()
        {
            // Arrange
            var config = new SupersetConfiguration();

            // Act
            config.GuestToken.DefaultExpirationMinutes = 600;
            config.GuestToken.AllowedDomains.Add("example.com");
            config.GuestToken.AllowAllDatasets = true;

            // Assert
            Assert.Equal(600, config.GuestToken.DefaultExpirationMinutes);
            Assert.Single(config.GuestToken.AllowedDomains);
            Assert.Contains("example.com", config.GuestToken.AllowedDomains);
            Assert.True(config.GuestToken.AllowAllDatasets);
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // Edge Cases and Complex Scenarios Tests
        // ═══════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Verifies that validation checks BaseUrl first (fails fast).
        /// </summary>
        [Fact]
        public void Validate_ShouldCheckBaseUrlFirst_WhenAllFieldsInvalid()
        {
            // Arrange
            var config = new SupersetConfiguration
            {
                BaseUrl = "",
                Username = "",
                Password = ""
            };

            // Act
            var (isValid, error) = config.Validate();

            // Assert
            Assert.False(isValid);
            Assert.NotNull(error);
            Assert.Contains("BaseUrl", error);
        }

        /// <summary>
        /// Verifies that valid configuration with all optional fields passes.
        /// </summary>
        [Fact]
        public void Validate_ShouldReturnTrue_WhenOptionalFieldsPopulated()
        {
            // Arrange
            var config = new SupersetConfiguration
            {
                BaseUrl = "https://superset.example.com",
                Username = "admin",
                Password = "password123",
                Provider = "oauth",
                AutoRefreshToken = false,
                TokenRefreshIntervalMinutes = 60,
                TimeoutSeconds = 60,
                SkipSslValidation = true
            };
            config.DashboardMappings["Dashboard1"] = 1;
            config.ChartMappings["Chart1"] = 1;
            config.DefaultRlsFilters["filter1"] = "value1";

            // Act
            var (isValid, error) = config.Validate();

            // Assert
            Assert.True(isValid);
            Assert.Null(error);
        }

        /// <summary>
        /// Verifies that validator implements IValidateOptions interface correctly.
        /// </summary>
        [Fact]
        public void SupersetConfigurationValidator_ShouldImplementIValidateOptions()
        {
            // Arrange & Act
            var validator = new SupersetConfigurationValidator();

            // Assert
            Assert.IsAssignableFrom<IValidateOptions<SupersetConfiguration>>(validator);
        }
    }
}
