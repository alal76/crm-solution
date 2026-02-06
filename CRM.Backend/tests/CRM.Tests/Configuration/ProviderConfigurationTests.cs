// CRM Solution - Provider Configuration Tests
// Tests for all provider configuration classes

using System;
using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace CRM.Tests.Configuration;

/// <summary>
/// Unit tests for provider configuration classes.
/// Tests validation, default values, and configuration binding.
/// </summary>
public class ProviderConfigurationTests
{
    #region Meilisearch Configuration Tests

    [Fact]
    public void MeilisearchConfiguration_HasCorrectSectionName()
    {
        // Assert
        CRM.Infrastructure.Providers.Meilisearch.MeilisearchConfiguration.SectionName
            .Should().Be("Providers:Search:Meilisearch");
    }

    [Fact]
    public void MeilisearchConfiguration_HasCorrectDefaults()
    {
        // Arrange & Act
        var config = new CRM.Infrastructure.Providers.Meilisearch.MeilisearchConfiguration();

        // Assert
        config.Url.Should().Be("http://localhost:7700");
        config.ApiKey.Should().BeEmpty();
        config.IndexPrefix.Should().Be("crm_");
        config.DefaultPageSize.Should().Be(20);
        config.MaxPageSize.Should().Be(100);
        config.TimeoutSeconds.Should().Be(30);
        config.EnableHighlighting.Should().BeTrue();
    }

    [Fact]
    public void MeilisearchConfiguration_AllowsPropertyOverrides()
    {
        // Arrange & Act
        var config = new CRM.Infrastructure.Providers.Meilisearch.MeilisearchConfiguration
        {
            Url = "https://meilisearch.example.com",
            ApiKey = "master-key-123",
            IndexPrefix = "prod_crm_",
            DefaultPageSize = 50,
            MaxPageSize = 500,
            TimeoutSeconds = 60,
            EnableHighlighting = false
        };

        // Assert
        config.Url.Should().Be("https://meilisearch.example.com");
        config.ApiKey.Should().Be("master-key-123");
        config.IndexPrefix.Should().Be("prod_crm_");
        config.DefaultPageSize.Should().Be(50);
        config.MaxPageSize.Should().Be(500);
        config.TimeoutSeconds.Should().Be(60);
        config.EnableHighlighting.Should().BeFalse();
    }

    #endregion

    #region Algolia Configuration Tests

    [Fact]
    public void AlgoliaConfiguration_HasCorrectDefaults()
    {
        // Arrange & Act
        var config = new CRM.Infrastructure.Providers.Algolia.AlgoliaConfiguration();

        // Assert
        config.ApplicationId.Should().BeEmpty();
        config.ApiKey.Should().BeEmpty();
        config.IndexPrefix.Should().Be("crm_");
    }

    [Fact]
    public void AlgoliaConfiguration_AllowsPropertyOverrides()
    {
        // Arrange & Act
        var config = new CRM.Infrastructure.Providers.Algolia.AlgoliaConfiguration
        {
            ApplicationId = "app-123",
            ApiKey = "search-key-456",
            IndexPrefix = "prod_"
        };

        // Assert
        config.ApplicationId.Should().Be("app-123");
        config.ApiKey.Should().Be("search-key-456");
        config.IndexPrefix.Should().Be("prod_");
    }

    #endregion

    #region Novu Configuration Tests

    [Fact]
    public void NovuConfiguration_HasCorrectDefaults()
    {
        // Arrange & Act
        var config = new CRM.Infrastructure.Providers.Novu.NovuConfiguration();

        // Assert
        config.ApiKey.Should().BeEmpty();
        config.BackendUrl.Should().Be("https://api.novu.co");
    }

    [Fact]
    public void NovuConfiguration_AllowsPropertyOverrides()
    {
        // Arrange & Act
        var config = new CRM.Infrastructure.Providers.Novu.NovuConfiguration
        {
            ApiKey = "novu-key-123",
            BackendUrl = "http://localhost:3000"
        };

        // Assert
        config.ApiKey.Should().Be("novu-key-123");
        config.BackendUrl.Should().Be("http://localhost:3000");
    }

    #endregion

    #region Twilio Configuration Tests

    [Fact]
    public void TwilioConfiguration_HasCorrectDefaults()
    {
        // Arrange & Act
        var config = new CRM.Infrastructure.Providers.Twilio.TwilioConfiguration();

        // Assert
        config.AccountSid.Should().BeEmpty();
        config.AuthToken.Should().BeEmpty();
        config.FromNumber.Should().BeEmpty();
    }

    [Fact]
    public void TwilioConfiguration_AllowsPropertyOverrides()
    {
        // Arrange & Act
        var config = new CRM.Infrastructure.Providers.Twilio.TwilioConfiguration
        {
            AccountSid = "AC123456789",
            AuthToken = "auth-token-123",
            FromNumber = "+15551234567"
        };

        // Assert
        config.AccountSid.Should().Be("AC123456789");
        config.AuthToken.Should().Be("auth-token-123");
        config.FromNumber.Should().Be("+15551234567");
    }

    #endregion

    #region SendGrid Configuration Tests

    [Fact]
    public void SendGridConfiguration_HasCorrectDefaults()
    {
        // Arrange & Act
        var config = new CRM.Infrastructure.Providers.SendGrid.SendGridConfiguration();

        // Assert
        config.ApiKey.Should().BeEmpty();
        config.FromEmail.Should().BeEmpty();
    }

    [Fact]
    public void SendGridConfiguration_AllowsPropertyOverrides()
    {
        // Arrange & Act
        var config = new CRM.Infrastructure.Providers.SendGrid.SendGridConfiguration
        {
            ApiKey = "SG.key-123",
            FromEmail = "noreply@example.com",
            FromName = "CRM System"
        };

        // Assert
        config.ApiKey.Should().Be("SG.key-123");
        config.FromEmail.Should().Be("noreply@example.com");
        config.FromName.Should().Be("CRM System");
    }

    #endregion

    #region Chatwoot Configuration Tests

    [Fact]
    public void ChatwootConfiguration_HasCorrectDefaults()
    {
        // Arrange & Act
        var config = new CRM.Infrastructure.Providers.Chatwoot.ChatwootConfiguration();

        // Assert
        config.BaseUrl.Should().BeEmpty();
        config.ApiKey.Should().BeEmpty();
        config.AccountId.Should().BeEmpty();
    }

    [Fact]
    public void ChatwootConfiguration_AllowsPropertyOverrides()
    {
        // Arrange & Act
        var config = new CRM.Infrastructure.Providers.Chatwoot.ChatwootConfiguration
        {
            BaseUrl = "https://chatwoot.example.com",
            ApiKey = "cw-api-key-123",
            AccountId = "1"
        };

        // Assert
        config.BaseUrl.Should().Be("https://chatwoot.example.com");
        config.ApiKey.Should().Be("cw-api-key-123");
        config.AccountId.Should().Be("1");
    }

    #endregion

    #region Intercom Configuration Tests

    [Fact]
    public void IntercomConfiguration_HasCorrectDefaults()
    {
        // Arrange & Act
        var config = new CRM.Infrastructure.Providers.Intercom.IntercomConfiguration();

        // Assert
        config.AccessToken.Should().BeEmpty();
        config.AppId.Should().BeEmpty();
    }

    [Fact]
    public void IntercomConfiguration_AllowsPropertyOverrides()
    {
        // Arrange & Act
        var config = new CRM.Infrastructure.Providers.Intercom.IntercomConfiguration
        {
            AccessToken = "intercom-token-123",
            AppId = "app-456"
        };

        // Assert
        config.AccessToken.Should().Be("intercom-token-123");
        config.AppId.Should().Be("app-456");
    }

    #endregion

    #region DocuSeal Configuration Tests

    [Fact]
    public void DocuSealConfiguration_HasCorrectDefaults()
    {
        // Arrange & Act
        var config = new CRM.Infrastructure.Providers.DocuSeal.DocuSealConfiguration();

        // Assert
        config.BaseUrl.Should().BeEmpty();
        config.ApiKey.Should().BeEmpty();
    }

    [Fact]
    public void DocuSealConfiguration_AllowsPropertyOverrides()
    {
        // Arrange & Act
        var config = new CRM.Infrastructure.Providers.DocuSeal.DocuSealConfiguration
        {
            BaseUrl = "https://docuseal.example.com",
            ApiKey = "ds-key-123"
        };

        // Assert
        config.BaseUrl.Should().Be("https://docuseal.example.com");
        config.ApiKey.Should().Be("ds-key-123");
    }

    #endregion

    #region DocuSign Configuration Tests

    [Fact]
    public void DocuSignConfiguration_HasCorrectDefaults()
    {
        // Arrange & Act
        var config = new CRM.Infrastructure.Providers.DocuSign.DocuSignConfiguration();

        // Assert
        config.IntegrationKey.Should().BeEmpty();
        config.AccountId.Should().BeEmpty();
        config.UserId.Should().BeEmpty();
        config.BasePath.Should().Be("https://demo.docusign.net/restapi");
    }

    [Fact]
    public void DocuSignConfiguration_AllowsProductionBasePath()
    {
        // Arrange & Act
        var config = new CRM.Infrastructure.Providers.DocuSign.DocuSignConfiguration
        {
            IntegrationKey = "integration-key-123",
            AccountId = "account-456",
            UserId = "user-789",
            BasePath = "https://www.docusign.net/restapi"
        };

        // Assert
        config.BasePath.Should().Be("https://www.docusign.net/restapi");
    }

    #endregion

    #region Superset Configuration Tests

    [Fact]
    public void SupersetConfiguration_HasCorrectDefaults()
    {
        // Arrange & Act
        var config = new CRM.Infrastructure.Providers.Superset.SupersetConfiguration();

        // Assert
        config.BaseUrl.Should().BeEmpty();
        config.Username.Should().BeEmpty();
        config.Password.Should().BeEmpty();
    }

    [Fact]
    public void SupersetConfiguration_AllowsPropertyOverrides()
    {
        // Arrange & Act
        var config = new CRM.Infrastructure.Providers.Superset.SupersetConfiguration
        {
            BaseUrl = "http://superset.example.com:8088",
            Username = "admin",
            Password = "admin-password"
        };

        // Assert
        config.BaseUrl.Should().Be("http://superset.example.com:8088");
        config.Username.Should().Be("admin");
        config.Password.Should().Be("admin-password");
    }

    #endregion

    #region PowerBI Configuration Tests

    [Fact]
    public void PowerBIConfiguration_HasCorrectDefaults()
    {
        // Arrange & Act
        var config = new CRM.Infrastructure.Providers.PowerBI.PowerBIConfiguration();

        // Assert
        config.TenantId.Should().BeEmpty();
        config.ClientId.Should().BeEmpty();
        config.ClientSecret.Should().BeEmpty();
        config.WorkspaceId.Should().BeEmpty();
    }

    [Fact]
    public void PowerBIConfiguration_AllowsPropertyOverrides()
    {
        // Arrange & Act
        var config = new CRM.Infrastructure.Providers.PowerBI.PowerBIConfiguration
        {
            TenantId = "tenant-123",
            ClientId = "client-456",
            ClientSecret = "secret-789",
            WorkspaceId = "workspace-abc"
        };

        // Assert
        config.TenantId.Should().Be("tenant-123");
        config.ClientId.Should().Be("client-456");
        config.ClientSecret.Should().Be("secret-789");
        config.WorkspaceId.Should().Be("workspace-abc");
    }

    #endregion
}
