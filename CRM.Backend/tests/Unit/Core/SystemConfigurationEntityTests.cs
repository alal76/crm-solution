// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using System;
using System.Collections.Generic;
using CRM.Core.Entities;
using FluentAssertions;
using Xunit;

namespace CRM.Tests.Unit.Core;

/// <summary>
/// Comprehensive unit tests for System Configuration entities:
/// - Module Field Configuration (ModuleFieldConfiguration, ModuleNames, FieldTypes)
/// - Module UI Configuration (ModuleUIConfig, DefaultModuleConfigs)
/// - Color Palette (ColorPalette)
/// - LLM Provider Settings (LLMProviderSetting, LLMSettingsDto, LLMProviderSettingsDto, etc.)
/// - Field Master Data Link (FieldMasterDataLink, MasterDataSourceTypes, MasterDataTables)
/// </summary>
public class SystemConfigurationEntityTests
{
    #region ModuleFieldConfiguration Entity Tests

    [Fact]
    public void ModuleFieldConfiguration_ShouldInitializeWithDefaults()
    {
        // Act
        var config = new ModuleFieldConfiguration();

        // Assert
        config.ModuleName.Should().BeEmpty();
        config.FieldName.Should().BeEmpty();
        config.FieldLabel.Should().BeEmpty();
        config.FieldType.Should().Be("text");
        config.TabIndex.Should().Be(0);
        config.TabName.Should().Be("Basic Info");
        config.DisplayOrder.Should().Be(0);
        config.IsEnabled.Should().BeTrue();
        config.IsRequired.Should().BeFalse();
        config.GridSize.Should().Be(6);
        config.IsReorderable.Should().BeTrue();
        config.IsRequiredConfigurable.Should().BeTrue();
        config.IsHideable.Should().BeTrue();
    }

    [Fact]
    public void ModuleFieldConfiguration_ShouldSetPropertiesCorrectly()
    {
        // Act
        var config = new ModuleFieldConfiguration
        {
            ModuleName = "Customers",
            FieldName = "companyName",
            FieldLabel = "Company Name",
            FieldType = "text",
            TabIndex = 0,
            TabName = "Business Info",
            DisplayOrder = 5,
            IsEnabled = true,
            IsRequired = true,
            GridSize = 12,
            Placeholder = "Enter company name",
            HelpText = "The legal name of the company",
            Options = null,
            ParentField = "accountType",
            ParentFieldValue = "Business",
            IsReorderable = true,
            IsRequiredConfigurable = false,
            IsHideable = false
        };

        // Assert
        config.ModuleName.Should().Be("Customers");
        config.FieldName.Should().Be("companyName");
        config.FieldLabel.Should().Be("Company Name");
        config.FieldType.Should().Be("text");
        config.TabIndex.Should().Be(0);
        config.TabName.Should().Be("Business Info");
        config.DisplayOrder.Should().Be(5);
        config.IsEnabled.Should().BeTrue();
        config.IsRequired.Should().BeTrue();
        config.GridSize.Should().Be(12);
        config.Placeholder.Should().Be("Enter company name");
        config.HelpText.Should().Be("The legal name of the company");
        config.ParentField.Should().Be("accountType");
        config.ParentFieldValue.Should().Be("Business");
        config.IsRequiredConfigurable.Should().BeFalse();
        config.IsHideable.Should().BeFalse();
    }

    [Fact]
    public void ModuleFieldConfiguration_ShouldSupportConditionalVisibility()
    {
        // Act
        var stateField = new ModuleFieldConfiguration
        {
            FieldName = "state",
            ParentField = "country",
            ParentFieldValue = "US"
        };

        // Assert
        stateField.ParentField.Should().Be("country");
        stateField.ParentFieldValue.Should().Be("US");
    }

    [Fact]
    public void ModuleFieldConfiguration_ShouldSupportSelectFieldWithOptions()
    {
        // Act
        var statusField = new ModuleFieldConfiguration
        {
            FieldName = "status",
            FieldType = "select",
            Options = "Active,Inactive,Pending,Archived"
        };

        // Assert
        statusField.FieldType.Should().Be("select");
        statusField.Options.Should().Contain("Active");
        statusField.Options.Should().Contain("Archived");
    }

    #endregion

    #region ModuleNames Static Class Tests

    [Fact]
    public void ModuleNames_ShouldHaveCorrectValues()
    {
        // Assert
        ModuleNames.Customers.Should().Be("Customers");
        ModuleNames.Contacts.Should().Be("Contacts");
        ModuleNames.Leads.Should().Be("Leads");
        ModuleNames.Opportunities.Should().Be("Opportunities");
        ModuleNames.Products.Should().Be("Products");
    }

    [Fact]
    public void ModuleNames_ShouldBeUsableInConfiguration()
    {
        // Act
        var config = new ModuleFieldConfiguration
        {
            ModuleName = ModuleNames.Customers
        };

        // Assert
        config.ModuleName.Should().Be("Customers");
    }

    #endregion

    #region FieldTypes Static Class Tests

    [Fact]
    public void FieldTypes_ShouldHaveCorrectValues()
    {
        // Assert
        FieldTypes.Text.Should().Be("text");
        FieldTypes.Email.Should().Be("email");
        FieldTypes.Number.Should().Be("number");
        FieldTypes.Date.Should().Be("date");
        FieldTypes.DateTime.Should().Be("datetime");
        FieldTypes.Select.Should().Be("select");
        FieldTypes.MultiSelect.Should().Be("multiselect");
        FieldTypes.Checkbox.Should().Be("checkbox");
        FieldTypes.TextArea.Should().Be("textarea");
        FieldTypes.Phone.Should().Be("phone");
        FieldTypes.Url.Should().Be("url");
        FieldTypes.Currency.Should().Be("currency");
    }

    [Fact]
    public void FieldTypes_ShouldBeUsableInConfiguration()
    {
        // Act
        var emailField = new ModuleFieldConfiguration
        {
            FieldName = "email",
            FieldType = FieldTypes.Email
        };

        // Assert
        emailField.FieldType.Should().Be("email");
    }

    #endregion

    #region ModuleUIConfig Entity Tests

    [Fact]
    public void ModuleUIConfig_ShouldInitializeWithDefaults()
    {
        // Act
        var config = new ModuleUIConfig();

        // Assert
        config.ModuleName.Should().BeEmpty();
        config.IsEnabled.Should().BeTrue();
        config.DisplayName.Should().BeEmpty();
        config.IconName.Should().Be("Folder");
        config.DisplayOrder.Should().Be(0);
    }

    [Fact]
    public void ModuleUIConfig_ShouldSetPropertiesCorrectly()
    {
        // Act
        var config = new ModuleUIConfig
        {
            ModuleName = "Customers",
            IsEnabled = true,
            DisplayName = "Customer Accounts",
            Description = "Manage customer and prospect accounts",
            IconName = "Business",
            DisplayOrder = 1,
            TabsConfig = "[{\"index\": 0, \"name\": \"Basic Info\", \"enabled\": true}]",
            LinkedEntitiesConfig = "[{\"entityName\": \"Contacts\", \"relationshipType\": \"one-to-many\"}]",
            ListViewConfig = "{\"columns\": [{\"field\": \"name\", \"width\": 200}]}",
            DetailViewConfig = "{\"layout\": \"tabs\", \"showRelated\": true}",
            QuickCreateConfig = "{\"fields\": [\"name\", \"email\"]}",
            SearchFilterConfig = "{\"searchFields\": [\"name\", \"email\"]}",
            ModuleSettings = "{\"defaultView\": \"list\", \"pageSize\": 25}"
        };

        // Assert
        config.ModuleName.Should().Be("Customers");
        config.IsEnabled.Should().BeTrue();
        config.DisplayName.Should().Be("Customer Accounts");
        config.Description.Should().Be("Manage customer and prospect accounts");
        config.IconName.Should().Be("Business");
        config.DisplayOrder.Should().Be(1);
        config.TabsConfig.Should().Contain("Basic Info");
        config.LinkedEntitiesConfig.Should().Contain("Contacts");
        config.ListViewConfig.Should().Contain("name");
        config.DetailViewConfig.Should().Contain("tabs");
        config.QuickCreateConfig.Should().Contain("email");
        config.SearchFilterConfig.Should().Contain("searchFields");
        config.ModuleSettings.Should().Contain("pageSize");
    }

    [Fact]
    public void ModuleUIConfig_ShouldSupportJsonConfigurations()
    {
        // Arrange
        var tabsConfig = @"[
            {""index"": 0, ""name"": ""Basic Info"", ""enabled"": true, ""order"": 0},
            {""index"": 1, ""name"": ""Business"", ""enabled"": true, ""order"": 1},
            {""index"": 2, ""name"": ""Contacts"", ""enabled"": false, ""order"": 2}
        ]";

        // Act
        var config = new ModuleUIConfig
        {
            TabsConfig = tabsConfig
        };

        // Assert
        config.TabsConfig.Should().Contain("Basic Info");
        config.TabsConfig.Should().Contain("Business");
        config.TabsConfig.Should().Contain("Contacts");
    }

    #endregion

    #region DefaultModuleConfigs Static Class Tests

    [Fact]
    public void DefaultModuleConfigs_AllModules_ShouldContainExpectedModules()
    {
        // Assert
        DefaultModuleConfigs.AllModules.Should().Contain("Customers");
        DefaultModuleConfigs.AllModules.Should().Contain("Contacts");
        DefaultModuleConfigs.AllModules.Should().Contain("Leads");
        DefaultModuleConfigs.AllModules.Should().Contain("Opportunities");
        DefaultModuleConfigs.AllModules.Should().Contain("Products");
        DefaultModuleConfigs.AllModules.Should().Contain("Services");
        DefaultModuleConfigs.AllModules.Should().Contain("Campaigns");
        DefaultModuleConfigs.AllModules.Should().Contain("Quotes");
        DefaultModuleConfigs.AllModules.Should().Contain("Tasks");
        DefaultModuleConfigs.AllModules.Should().Contain("Activities");
        DefaultModuleConfigs.AllModules.Should().Contain("Notes");
    }

    [Fact]
    public void DefaultModuleConfigs_AllModules_ShouldHaveElevenModules()
    {
        // Assert
        DefaultModuleConfigs.AllModules.Should().HaveCount(11);
    }

    [Fact]
    public void DefaultModuleConfigs_DefaultLinkedEntities_ShouldHaveCorrectCustomerLinks()
    {
        // Assert
        DefaultModuleConfigs.DefaultLinkedEntities[ModuleNames.Customers].Should().Contain("Contacts");
        DefaultModuleConfigs.DefaultLinkedEntities[ModuleNames.Customers].Should().Contain("Opportunities");
        DefaultModuleConfigs.DefaultLinkedEntities[ModuleNames.Customers].Should().Contain("Quotes");
        DefaultModuleConfigs.DefaultLinkedEntities[ModuleNames.Customers].Should().Contain("Tasks");
        DefaultModuleConfigs.DefaultLinkedEntities[ModuleNames.Customers].Should().Contain("Activities");
        DefaultModuleConfigs.DefaultLinkedEntities[ModuleNames.Customers].Should().Contain("Notes");
    }

    [Fact]
    public void DefaultModuleConfigs_DefaultLinkedEntities_ShouldHaveCorrectLeadLinks()
    {
        // Assert
        DefaultModuleConfigs.DefaultLinkedEntities[ModuleNames.Leads].Should().Contain("Tasks");
        DefaultModuleConfigs.DefaultLinkedEntities[ModuleNames.Leads].Should().Contain("Activities");
        DefaultModuleConfigs.DefaultLinkedEntities[ModuleNames.Leads].Should().Contain("Notes");
    }

    #endregion

    #region ColorPalette Entity Tests

    [Fact]
    public void ColorPalette_ShouldInitializeWithDefaults()
    {
        // Act
        var palette = new ColorPalette();

        // Assert
        palette.Name.Should().BeEmpty();
        palette.Color1.Should().Be("#000000");
        palette.Color2.Should().Be("#333333");
        palette.Color3.Should().Be("#666666");
        palette.Color4.Should().Be("#999999");
        palette.Color5.Should().Be("#CCCCCC");
        palette.IsUserDefined.Should().BeFalse();
    }

    [Fact]
    public void ColorPalette_ShouldSetPropertiesCorrectly()
    {
        // Act
        var palette = new ColorPalette
        {
            Name = "Ocean Breeze",
            Category = "Trending",
            Color1 = "#1E88E5",
            Color2 = "#42A5F5",
            Color3 = "#64B5F6",
            Color4 = "#90CAF9",
            Color5 = "#BBDEFB",
            IsUserDefined = false,
            CreatedByUserId = null
        };

        // Assert
        palette.Name.Should().Be("Ocean Breeze");
        palette.Category.Should().Be("Trending");
        palette.Color1.Should().Be("#1E88E5");
        palette.Color2.Should().Be("#42A5F5");
        palette.Color3.Should().Be("#64B5F6");
        palette.Color4.Should().Be("#90CAF9");
        palette.Color5.Should().Be("#BBDEFB");
        palette.IsUserDefined.Should().BeFalse();
    }

    [Fact]
    public void ColorPalette_ShouldSupportUserDefinedPalettes()
    {
        // Act
        var userPalette = new ColorPalette
        {
            Name = "My Custom Palette",
            Category = "Custom",
            IsUserDefined = true,
            CreatedByUserId = 123
        };

        // Assert
        userPalette.IsUserDefined.Should().BeTrue();
        userPalette.CreatedByUserId.Should().Be(123);
    }

    [Fact]
    public void ColorPalette_ShouldSupportDifferentCategories()
    {
        // Arrange & Act
        var trendingPalette = new ColorPalette { Category = "Trending" };
        var earthyPalette = new ColorPalette { Category = "Earthy" };
        var pastelPalette = new ColorPalette { Category = "Pastel" };
        var vibrantPalette = new ColorPalette { Category = "Vibrant" };
        var neonPalette = new ColorPalette { Category = "Neon" };

        // Assert
        trendingPalette.Category.Should().Be("Trending");
        earthyPalette.Category.Should().Be("Earthy");
        pastelPalette.Category.Should().Be("Pastel");
        vibrantPalette.Category.Should().Be("Vibrant");
        neonPalette.Category.Should().Be("Neon");
    }

    #endregion

    #region LLMProviderSetting Entity Tests

    [Fact]
    public void LLMProviderSetting_ShouldInitializeWithDefaults()
    {
        // Act
        var setting = new LLMProviderSetting();

        // Assert
        setting.SettingKey.Should().BeEmpty();
        setting.SettingValue.Should().BeEmpty();
        setting.ValueType.Should().Be("string");
        setting.Category.Should().Be("general");
        setting.IsEncrypted.Should().BeFalse();
    }

    [Fact]
    public void LLMProviderSetting_ShouldSetPropertiesCorrectly()
    {
        // Act
        var setting = new LLMProviderSetting
        {
            SettingKey = "OpenAI.DefaultModel",
            SettingValue = "gpt-4",
            ValueType = "string",
            Category = "provider.openai",
            Description = "The default model to use for OpenAI requests",
            IsEncrypted = false
        };

        // Assert
        setting.SettingKey.Should().Be("OpenAI.DefaultModel");
        setting.SettingValue.Should().Be("gpt-4");
        setting.ValueType.Should().Be("string");
        setting.Category.Should().Be("provider.openai");
        setting.Description.Should().Be("The default model to use for OpenAI requests");
        setting.IsEncrypted.Should().BeFalse();
    }

    [Fact]
    public void LLMProviderSetting_ShouldSupportDifferentValueTypes()
    {
        // Act
        var stringSetting = new LLMProviderSetting { ValueType = "string", SettingValue = "test" };
        var integerSetting = new LLMProviderSetting { ValueType = "integer", SettingValue = "100" };
        var decimalSetting = new LLMProviderSetting { ValueType = "decimal", SettingValue = "0.7" };
        var booleanSetting = new LLMProviderSetting { ValueType = "boolean", SettingValue = "true" };
        var jsonSetting = new LLMProviderSetting { ValueType = "json", SettingValue = "{\"key\":\"value\"}" };

        // Assert
        stringSetting.ValueType.Should().Be("string");
        integerSetting.ValueType.Should().Be("integer");
        decimalSetting.ValueType.Should().Be("decimal");
        booleanSetting.ValueType.Should().Be("boolean");
        jsonSetting.ValueType.Should().Be("json");
    }

    [Fact]
    public void LLMProviderSetting_ShouldSupportDifferentCategories()
    {
        // Act
        var generalSetting = new LLMProviderSetting { Category = "general" };
        var openaiSetting = new LLMProviderSetting { Category = "provider.openai" };
        var anthropicSetting = new LLMProviderSetting { Category = "provider.anthropic" };
        var azureSetting = new LLMProviderSetting { Category = "provider.azure" };

        // Assert
        generalSetting.Category.Should().Be("general");
        openaiSetting.Category.Should().Be("provider.openai");
        anthropicSetting.Category.Should().Be("provider.anthropic");
        azureSetting.Category.Should().Be("provider.azure");
    }

    #endregion

    #region LLMSettingsDto Tests

    [Fact]
    public void LLMSettingsDto_ShouldInitializeWithDefaults()
    {
        // Act
        var dto = new LLMSettingsDto();

        // Assert
        dto.DefaultProvider.Should().Be("local");
        dto.EnableFallback.Should().BeTrue();
        dto.FallbackOrder.Should().Contain("local");
        dto.FallbackOrder.Should().Contain("openai");
        dto.FallbackOrder.Should().Contain("azure");
        dto.FallbackOrder.Should().Contain("anthropic");
        dto.DefaultMaxTokens.Should().Be(1000);
        dto.DefaultTemperature.Should().Be(0.7);
        dto.TimeoutSeconds.Should().Be(60);
        dto.MaxRetries.Should().Be(3);
        dto.OpenAI.Should().NotBeNull();
        dto.Azure.Should().NotBeNull();
        dto.Anthropic.Should().NotBeNull();
        dto.Google.Should().NotBeNull();
        dto.Bedrock.Should().NotBeNull();
        dto.DeepSeek.Should().NotBeNull();
        dto.AllenAI.Should().NotBeNull();
        dto.Local.Should().NotBeNull();
    }

    [Fact]
    public void LLMSettingsDto_ShouldSetPropertiesCorrectly()
    {
        // Act
        var dto = new LLMSettingsDto
        {
            DefaultProvider = "openai",
            EnableFallback = false,
            FallbackOrder = new List<string> { "openai", "anthropic" },
            EffectiveFallbackOrder = new List<string> { "openai" },
            DefaultMaxTokens = 2000,
            DefaultTemperature = 0.5,
            TimeoutSeconds = 120,
            MaxRetries = 5
        };

        // Assert
        dto.DefaultProvider.Should().Be("openai");
        dto.EnableFallback.Should().BeFalse();
        dto.FallbackOrder.Should().HaveCount(2);
        dto.EffectiveFallbackOrder.Should().HaveCount(1);
        dto.DefaultMaxTokens.Should().Be(2000);
        dto.DefaultTemperature.Should().Be(0.5);
        dto.TimeoutSeconds.Should().Be(120);
        dto.MaxRetries.Should().Be(5);
    }

    #endregion

    #region LLMProviderSettingsDto Tests

    [Fact]
    public void LLMProviderSettingsDto_ShouldInitializeWithDefaults()
    {
        // Act
        var dto = new LLMProviderSettingsDto();

        // Assert
        dto.DefaultModel.Should().BeEmpty();
        dto.BaseUrl.Should().BeNull();
        dto.ApiVersion.Should().BeNull();
        dto.Location.Should().BeNull();
        dto.Region.Should().BeNull();
        dto.ApiFormat.Should().BeNull();
        dto.Enabled.Should().BeNull();
        dto.UseVertexAI.Should().BeNull();
        dto.UseDefaultCredentials.Should().BeNull();
        dto.IsConfigured.Should().BeFalse();
    }

    [Fact]
    public void LLMProviderSettingsDto_ShouldSetOpenAIPropertiesCorrectly()
    {
        // Act
        var dto = new LLMProviderSettingsDto
        {
            DefaultModel = "gpt-4-turbo",
            BaseUrl = "https://api.openai.com/v1",
            Enabled = true,
            IsConfigured = true
        };

        // Assert
        dto.DefaultModel.Should().Be("gpt-4-turbo");
        dto.BaseUrl.Should().Be("https://api.openai.com/v1");
        dto.Enabled.Should().BeTrue();
        dto.IsConfigured.Should().BeTrue();
    }

    [Fact]
    public void LLMProviderSettingsDto_ShouldSetAzurePropertiesCorrectly()
    {
        // Act
        var dto = new LLMProviderSettingsDto
        {
            DefaultModel = "gpt-4",
            BaseUrl = "https://my-resource.openai.azure.com",
            ApiVersion = "2024-02-01",
            Location = "eastus",
            Enabled = true,
            IsConfigured = true
        };

        // Assert
        dto.DefaultModel.Should().Be("gpt-4");
        dto.BaseUrl.Should().Contain("azure.com");
        dto.ApiVersion.Should().Be("2024-02-01");
        dto.Location.Should().Be("eastus");
    }

    [Fact]
    public void LLMProviderSettingsDto_ShouldSetBedrockPropertiesCorrectly()
    {
        // Act
        var dto = new LLMProviderSettingsDto
        {
            DefaultModel = "anthropic.claude-3-sonnet",
            Region = "us-east-1",
            UseDefaultCredentials = true,
            Enabled = true,
            IsConfigured = true
        };

        // Assert
        dto.DefaultModel.Should().Be("anthropic.claude-3-sonnet");
        dto.Region.Should().Be("us-east-1");
        dto.UseDefaultCredentials.Should().BeTrue();
    }

    [Fact]
    public void LLMProviderSettingsDto_ShouldSetGooglePropertiesCorrectly()
    {
        // Act
        var dto = new LLMProviderSettingsDto
        {
            DefaultModel = "gemini-1.5-pro",
            Location = "us-central1",
            UseVertexAI = true,
            Enabled = true,
            IsConfigured = true
        };

        // Assert
        dto.DefaultModel.Should().Be("gemini-1.5-pro");
        dto.Location.Should().Be("us-central1");
        dto.UseVertexAI.Should().BeTrue();
    }

    #endregion

    #region UpdateLLMSettingsRequest Tests

    [Fact]
    public void UpdateLLMSettingsRequest_ShouldSupportPartialUpdates()
    {
        // Act
        var request = new UpdateLLMSettingsRequest
        {
            DefaultProvider = "anthropic",
            DefaultMaxTokens = 4000
            // Other fields left null for partial update
        };

        // Assert
        request.DefaultProvider.Should().Be("anthropic");
        request.DefaultMaxTokens.Should().Be(4000);
        request.EnableFallback.Should().BeNull();
        request.FallbackOrder.Should().BeNull();
        request.DefaultTemperature.Should().BeNull();
    }

    [Fact]
    public void UpdateLLMSettingsRequest_ShouldSupportProviderUpdates()
    {
        // Arrange
        var providers = new Dictionary<string, LLMProviderUpdateDto>
        {
            ["openai"] = new LLMProviderUpdateDto
            {
                DefaultModel = "gpt-4-turbo",
                Enabled = true
            },
            ["anthropic"] = new LLMProviderUpdateDto
            {
                DefaultModel = "claude-3-opus",
                Enabled = true
            }
        };

        // Act
        var request = new UpdateLLMSettingsRequest
        {
            Providers = providers
        };

        // Assert
        request.Providers.Should().HaveCount(2);
        request.Providers["openai"].DefaultModel.Should().Be("gpt-4-turbo");
        request.Providers["anthropic"].DefaultModel.Should().Be("claude-3-opus");
    }

    #endregion

    #region LLMProviderUpdateDto Tests

    [Fact]
    public void LLMProviderUpdateDto_ShouldSupportAllProperties()
    {
        // Act
        var dto = new LLMProviderUpdateDto
        {
            DefaultModel = "gpt-4",
            BaseUrl = "https://custom.api.com",
            ApiVersion = "v2",
            Location = "westus",
            Region = "us-west-2",
            ApiFormat = "openai",
            Enabled = true,
            UseVertexAI = false,
            UseDefaultCredentials = true
        };

        // Assert
        dto.DefaultModel.Should().Be("gpt-4");
        dto.BaseUrl.Should().Be("https://custom.api.com");
        dto.ApiVersion.Should().Be("v2");
        dto.Location.Should().Be("westus");
        dto.Region.Should().Be("us-west-2");
        dto.ApiFormat.Should().Be("openai");
        dto.Enabled.Should().BeTrue();
        dto.UseVertexAI.Should().BeFalse();
        dto.UseDefaultCredentials.Should().BeTrue();
    }

    #endregion

    #region FieldMasterDataLink Entity Tests

    [Fact]
    public void FieldMasterDataLink_ShouldInitializeWithDefaults()
    {
        // Act
        var link = new FieldMasterDataLink();

        // Assert
        link.FieldConfigurationId.Should().Be(0);
        link.SourceType.Should().Be("LookupCategory");
        link.SourceName.Should().BeEmpty();
        link.DisplayField.Should().Be("Value");
        link.ValueField.Should().Be("Key");
        link.AllowFreeText.Should().BeFalse();
        link.SortOrder.Should().Be(0);
        link.IsActive.Should().BeTrue();
    }

    [Fact]
    public void FieldMasterDataLink_ShouldSetPropertiesCorrectly()
    {
        // Act
        var link = new FieldMasterDataLink
        {
            FieldConfigurationId = 1,
            SourceType = "Table",
            SourceName = "ZipCodes",
            DisplayField = "ZipCode",
            ValueField = "Id",
            FilterExpression = "{\"CountryCode\": \"US\"}",
            DependsOnField = "state",
            DependsOnSourceColumn = "StateCode",
            AllowFreeText = false,
            ValidationType = "Pattern",
            ValidationPattern = @"^\d{5}(-\d{4})?$",
            ValidationMessage = "Please enter a valid ZIP code",
            SortOrder = 1,
            IsActive = true
        };

        // Assert
        link.FieldConfigurationId.Should().Be(1);
        link.SourceType.Should().Be("Table");
        link.SourceName.Should().Be("ZipCodes");
        link.DisplayField.Should().Be("ZipCode");
        link.ValueField.Should().Be("Id");
        link.FilterExpression.Should().Contain("CountryCode");
        link.DependsOnField.Should().Be("state");
        link.DependsOnSourceColumn.Should().Be("StateCode");
        link.AllowFreeText.Should().BeFalse();
        link.ValidationType.Should().Be("Pattern");
        link.ValidationPattern.Should().Contain(@"\d{5}");
        link.ValidationMessage.Should().Be("Please enter a valid ZIP code");
        link.SortOrder.Should().Be(1);
        link.IsActive.Should().BeTrue();
    }

    [Fact]
    public void FieldMasterDataLink_ShouldSupportLookupCategorySource()
    {
        // Act
        var link = new FieldMasterDataLink
        {
            SourceType = MasterDataSourceTypes.LookupCategory,
            SourceName = "Currency",
            DisplayField = "Name",
            ValueField = "Code"
        };

        // Assert
        link.SourceType.Should().Be("LookupCategory");
        link.SourceName.Should().Be("Currency");
    }

    [Fact]
    public void FieldMasterDataLink_ShouldSupportApiSource()
    {
        // Act
        var link = new FieldMasterDataLink
        {
            SourceType = MasterDataSourceTypes.Api,
            SourceName = "/api/zipcodes/countries",
            DisplayField = "CountryName",
            ValueField = "CountryCode"
        };

        // Assert
        link.SourceType.Should().Be("Api");
        link.SourceName.Should().Contain("/api/");
    }

    [Fact]
    public void FieldMasterDataLink_ShouldSupportCascadingDropdowns()
    {
        // Arrange & Act - Country -> State -> City cascade
        var countryLink = new FieldMasterDataLink
        {
            SourceType = "Table",
            SourceName = "Countries",
            SortOrder = 0
        };

        var stateLink = new FieldMasterDataLink
        {
            SourceType = "Table",
            SourceName = "States",
            DependsOnField = "country",
            DependsOnSourceColumn = "CountryCode",
            SortOrder = 1
        };

        var cityLink = new FieldMasterDataLink
        {
            SourceType = "Table",
            SourceName = "Cities",
            DependsOnField = "state",
            DependsOnSourceColumn = "StateCode",
            SortOrder = 2
        };

        // Assert
        countryLink.DependsOnField.Should().BeNull();
        stateLink.DependsOnField.Should().Be("country");
        cityLink.DependsOnField.Should().Be("state");
        stateLink.SortOrder.Should().BeGreaterThan(countryLink.SortOrder);
        cityLink.SortOrder.Should().BeGreaterThan(stateLink.SortOrder);
    }

    #endregion

    #region MasterDataSourceTypes Static Class Tests

    [Fact]
    public void MasterDataSourceTypes_ShouldHaveCorrectValues()
    {
        // Assert
        MasterDataSourceTypes.LookupCategory.Should().Be("LookupCategory");
        MasterDataSourceTypes.Table.Should().Be("Table");
        MasterDataSourceTypes.Api.Should().Be("Api");
    }

    #endregion

    #region MasterDataTables Static Class Tests

    [Fact]
    public void MasterDataTables_ShouldHaveCorrectValues()
    {
        // Assert
        MasterDataTables.ZipCodes.Should().Be("ZipCodes");
        MasterDataTables.Products.Should().Be("Products");
        MasterDataTables.Customers.Should().Be("Customers");
        MasterDataTables.Contacts.Should().Be("Contacts");
        MasterDataTables.Users.Should().Be("Users");
        MasterDataTables.ServiceRequestTypes.Should().Be("ServiceRequestTypes");
        MasterDataTables.LookupItems.Should().Be("LookupItems");
    }

    [Fact]
    public void MasterDataTables_ShouldBeUsableInFieldMasterDataLink()
    {
        // Act
        var link = new FieldMasterDataLink
        {
            SourceType = MasterDataSourceTypes.Table,
            SourceName = MasterDataTables.ZipCodes
        };

        // Assert
        link.SourceName.Should().Be("ZipCodes");
    }

    #endregion
}
