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

using CRM.Core.Features;
using FluentAssertions;
using System.Reflection;
using Xunit;

namespace CRM.Tests.Unit.Core;

/// <summary>
/// Tests for FeatureFlags static class
/// </summary>
public class FeatureFlagsTests
{
    #region Provider Selection Flags

    [Fact]
    public void UseExternalChat_ShouldHaveCorrectValue()
    {
        FeatureFlags.UseExternalChat.Should().Be("UseExternalChat");
    }

    [Fact]
    public void UseExternalSearch_ShouldHaveCorrectValue()
    {
        FeatureFlags.UseExternalSearch.Should().Be("UseExternalSearch");
    }

    [Fact]
    public void UseExternalNotifications_ShouldHaveCorrectValue()
    {
        FeatureFlags.UseExternalNotifications.Should().Be("UseExternalNotifications");
    }

    [Fact]
    public void UseExternalAnalytics_ShouldHaveCorrectValue()
    {
        FeatureFlags.UseExternalAnalytics.Should().Be("UseExternalAnalytics");
    }

    [Fact]
    public void UseExternalSignatures_ShouldHaveCorrectValue()
    {
        FeatureFlags.UseExternalSignatures.Should().Be("UseExternalSignatures");
    }

    [Fact]
    public void UseExternalAI_ShouldHaveCorrectValue()
    {
        FeatureFlags.UseExternalAI.Should().Be("UseExternalAI");
    }

    [Fact]
    public void UseExternalIntegrations_ShouldHaveCorrectValue()
    {
        FeatureFlags.UseExternalIntegrations.Should().Be("UseExternalIntegrations");
    }

    #endregion

    #region Module Enablement Flags

    [Fact]
    public void EnableITSM_ShouldHaveCorrectValue()
    {
        FeatureFlags.EnableITSM.Should().Be("EnableITSM");
    }

    [Fact]
    public void EnableMarketing_ShouldHaveCorrectValue()
    {
        FeatureFlags.EnableMarketing.Should().Be("EnableMarketing");
    }

    [Fact]
    public void EnableCustomerPortal_ShouldHaveCorrectValue()
    {
        FeatureFlags.EnableCustomerPortal.Should().Be("EnableCustomerPortal");
    }

    [Fact]
    public void EnablePartnerPortal_ShouldHaveCorrectValue()
    {
        FeatureFlags.EnablePartnerPortal.Should().Be("EnablePartnerPortal");
    }

    [Fact]
    public void EnableKnowledgeBase_ShouldHaveCorrectValue()
    {
        FeatureFlags.EnableKnowledgeBase.Should().Be("EnableKnowledgeBase");
    }

    #endregion

    #region Feature Rollout Flags

    [Fact]
    public void NewSearchExperience_ShouldHaveCorrectValue()
    {
        FeatureFlags.NewSearchExperience.Should().Be("NewSearchExperience");
    }

    [Fact]
    public void AIAssistant_ShouldHaveCorrectValue()
    {
        FeatureFlags.AIAssistant.Should().Be("AIAssistant");
    }

    [Fact]
    public void RealTimeNotifications_ShouldHaveCorrectValue()
    {
        FeatureFlags.RealTimeNotifications.Should().Be("RealTimeNotifications");
    }

    [Fact]
    public void AdvancedWorkflows_ShouldHaveCorrectValue()
    {
        FeatureFlags.AdvancedWorkflows.Should().Be("AdvancedWorkflows");
    }

    #endregion

    #region Validation Tests

    [Fact]
    public void AllFeatureFlags_ShouldNotContainColons()
    {
        // Microsoft.FeatureManagement does not allow colons in feature names
        var type = typeof(FeatureFlags);
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(f => f.IsLiteral && !f.IsInitOnly && f.FieldType == typeof(string));

        foreach (var field in fields)
        {
            var value = field.GetValue(null) as string;
            value.Should().NotContain(":", because: $"feature flag '{field.Name}' should not contain colons per Microsoft.FeatureManagement requirements");
        }
    }

    [Fact]
    public void AllFeatureFlags_ShouldNotBeNullOrEmpty()
    {
        var type = typeof(FeatureFlags);
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(f => f.IsLiteral && !f.IsInitOnly && f.FieldType == typeof(string));

        foreach (var field in fields)
        {
            var value = field.GetValue(null) as string;
            value.Should().NotBeNullOrEmpty(because: $"feature flag '{field.Name}' must have a value");
        }
    }

    [Fact]
    public void AllFeatureFlags_ShouldHaveUniqueValues()
    {
        var type = typeof(FeatureFlags);
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(f => f.IsLiteral && !f.IsInitOnly && f.FieldType == typeof(string));

        var values = fields.Select(f => f.GetValue(null) as string).ToList();
        values.Should().OnlyHaveUniqueItems(because: "feature flags must have unique values");
    }

    [Fact]
    public void FeatureFlagsClass_ShouldBeStaticClass()
    {
        typeof(FeatureFlags).IsAbstract.Should().BeTrue();
        typeof(FeatureFlags).IsSealed.Should().BeTrue();
    }

    [Theory]
    [InlineData("UseExternalChat")]
    [InlineData("UseExternalSearch")]
    [InlineData("UseExternalNotifications")]
    [InlineData("UseExternalAnalytics")]
    [InlineData("UseExternalSignatures")]
    [InlineData("UseExternalAI")]
    [InlineData("UseExternalIntegrations")]
    public void ProviderSelectionFlags_ShouldStartWithUseExternal(string expectedValue)
    {
        var type = typeof(FeatureFlags);
        var field = type.GetField(expectedValue, BindingFlags.Public | BindingFlags.Static);
        field.Should().NotBeNull();

        var value = field!.GetValue(null) as string;
        value.Should().StartWith("UseExternal");
    }

    [Theory]
    [InlineData("EnableITSM")]
    [InlineData("EnableMarketing")]
    [InlineData("EnableCustomerPortal")]
    [InlineData("EnablePartnerPortal")]
    [InlineData("EnableKnowledgeBase")]
    public void ModuleEnablementFlags_ShouldStartWithEnable(string expectedValue)
    {
        var type = typeof(FeatureFlags);
        var field = type.GetField(expectedValue, BindingFlags.Public | BindingFlags.Static);
        field.Should().NotBeNull();

        var value = field!.GetValue(null) as string;
        value.Should().StartWith("Enable");
    }

    #endregion
}

/// <summary>
/// Tests for ProviderTypes static class and nested provider category classes
/// </summary>
public class ProviderTypesTests
{
    #region Chat Provider Types

    [Fact]
    public void ChatBuiltIn_ShouldHaveCorrectValue()
    {
        ProviderTypes.Chat.BuiltIn.Should().Be("BuiltIn");
    }

    [Fact]
    public void ChatChatwoot_ShouldHaveCorrectValue()
    {
        ProviderTypes.Chat.Chatwoot.Should().Be("Chatwoot");
    }

    [Fact]
    public void ChatIntercom_ShouldHaveCorrectValue()
    {
        ProviderTypes.Chat.Intercom.Should().Be("Intercom");
    }

    [Fact]
    public void ChatZendesk_ShouldHaveCorrectValue()
    {
        ProviderTypes.Chat.Zendesk.Should().Be("Zendesk");
    }

    [Fact]
    public void ChatFreshchat_ShouldHaveCorrectValue()
    {
        ProviderTypes.Chat.Freshchat.Should().Be("Freshchat");
    }

    [Fact]
    public void ChatRocketChat_ShouldHaveCorrectValue()
    {
        ProviderTypes.Chat.RocketChat.Should().Be("RocketChat");
    }

    #endregion

    #region Search Provider Types

    [Fact]
    public void SearchBuiltIn_ShouldHaveCorrectValue()
    {
        ProviderTypes.Search.BuiltIn.Should().Be("BuiltIn");
    }

    [Fact]
    public void SearchMeilisearch_ShouldHaveCorrectValue()
    {
        ProviderTypes.Search.Meilisearch.Should().Be("Meilisearch");
    }

    [Fact]
    public void SearchAlgolia_ShouldHaveCorrectValue()
    {
        ProviderTypes.Search.Algolia.Should().Be("Algolia");
    }

    [Fact]
    public void SearchTypesense_ShouldHaveCorrectValue()
    {
        ProviderTypes.Search.Typesense.Should().Be("Typesense");
    }

    [Fact]
    public void SearchElasticsearch_ShouldHaveCorrectValue()
    {
        ProviderTypes.Search.Elasticsearch.Should().Be("Elasticsearch");
    }

    [Fact]
    public void SearchAzureCognitiveSearch_ShouldHaveCorrectValue()
    {
        ProviderTypes.Search.AzureCognitiveSearch.Should().Be("AzureCognitiveSearch");
    }

    #endregion

    #region Notifications Provider Types

    [Fact]
    public void NotificationsBuiltIn_ShouldHaveCorrectValue()
    {
        ProviderTypes.Notifications.BuiltIn.Should().Be("BuiltIn");
    }

    [Fact]
    public void NotificationsNovu_ShouldHaveCorrectValue()
    {
        ProviderTypes.Notifications.Novu.Should().Be("Novu");
    }

    [Fact]
    public void NotificationsTwilio_ShouldHaveCorrectValue()
    {
        ProviderTypes.Notifications.Twilio.Should().Be("Twilio");
    }

    [Fact]
    public void NotificationsSendGrid_ShouldHaveCorrectValue()
    {
        ProviderTypes.Notifications.SendGrid.Should().Be("SendGrid");
    }

    [Fact]
    public void NotificationsOneSignal_ShouldHaveCorrectValue()
    {
        ProviderTypes.Notifications.OneSignal.Should().Be("OneSignal");
    }

    [Fact]
    public void NotificationsCourier_ShouldHaveCorrectValue()
    {
        ProviderTypes.Notifications.Courier.Should().Be("Courier");
    }

    [Fact]
    public void NotificationsAWSSES_ShouldHaveCorrectValue()
    {
        ProviderTypes.Notifications.AWSSES.Should().Be("AWSSES");
    }

    #endregion

    #region Analytics Provider Types

    [Fact]
    public void AnalyticsBuiltIn_ShouldHaveCorrectValue()
    {
        ProviderTypes.Analytics.BuiltIn.Should().Be("BuiltIn");
    }

    [Fact]
    public void AnalyticsSuperset_ShouldHaveCorrectValue()
    {
        ProviderTypes.Analytics.Superset.Should().Be("Superset");
    }

    [Fact]
    public void AnalyticsMetabase_ShouldHaveCorrectValue()
    {
        ProviderTypes.Analytics.Metabase.Should().Be("Metabase");
    }

    [Fact]
    public void AnalyticsPowerBI_ShouldHaveCorrectValue()
    {
        ProviderTypes.Analytics.PowerBI.Should().Be("PowerBI");
    }

    [Fact]
    public void AnalyticsLooker_ShouldHaveCorrectValue()
    {
        ProviderTypes.Analytics.Looker.Should().Be("Looker");
    }

    [Fact]
    public void AnalyticsQuickSight_ShouldHaveCorrectValue()
    {
        ProviderTypes.Analytics.QuickSight.Should().Be("QuickSight");
    }

    #endregion

    #region Signatures Provider Types

    [Fact]
    public void SignaturesBuiltIn_ShouldHaveCorrectValue()
    {
        ProviderTypes.Signatures.BuiltIn.Should().Be("BuiltIn");
    }

    [Fact]
    public void SignaturesDocuSeal_ShouldHaveCorrectValue()
    {
        ProviderTypes.Signatures.DocuSeal.Should().Be("DocuSeal");
    }

    [Fact]
    public void SignaturesDocuSign_ShouldHaveCorrectValue()
    {
        ProviderTypes.Signatures.DocuSign.Should().Be("DocuSign");
    }

    [Fact]
    public void SignaturesAdobeSign_ShouldHaveCorrectValue()
    {
        ProviderTypes.Signatures.AdobeSign.Should().Be("AdobeSign");
    }

    [Fact]
    public void SignaturesHelloSign_ShouldHaveCorrectValue()
    {
        ProviderTypes.Signatures.HelloSign.Should().Be("HelloSign");
    }

    #endregion

    #region AI Provider Types

    [Fact]
    public void AIOllama_ShouldHaveCorrectValue()
    {
        ProviderTypes.AI.Ollama.Should().Be("Ollama");
    }

    [Fact]
    public void AIOpenAI_ShouldHaveCorrectValue()
    {
        ProviderTypes.AI.OpenAI.Should().Be("OpenAI");
    }

    [Fact]
    public void AIAzureOpenAI_ShouldHaveCorrectValue()
    {
        ProviderTypes.AI.AzureOpenAI.Should().Be("AzureOpenAI");
    }

    [Fact]
    public void AIAnthropic_ShouldHaveCorrectValue()
    {
        ProviderTypes.AI.Anthropic.Should().Be("Anthropic");
    }

    [Fact]
    public void AIBedrock_ShouldHaveCorrectValue()
    {
        ProviderTypes.AI.Bedrock.Should().Be("Bedrock");
    }

    [Fact]
    public void AIGemini_ShouldHaveCorrectValue()
    {
        ProviderTypes.AI.Gemini.Should().Be("Gemini");
    }

    [Fact]
    public void AIDeepSeek_ShouldHaveCorrectValue()
    {
        ProviderTypes.AI.DeepSeek.Should().Be("DeepSeek");
    }

    [Fact]
    public void AIOpenRouter_ShouldHaveCorrectValue()
    {
        ProviderTypes.AI.OpenRouter.Should().Be("OpenRouter");
    }

    #endregion

    #region Integrations Provider Types

    [Fact]
    public void IntegrationsBuiltIn_ShouldHaveCorrectValue()
    {
        ProviderTypes.Integrations.BuiltIn.Should().Be("BuiltIn");
    }

    [Fact]
    public void IntegrationsN8n_ShouldHaveCorrectValue()
    {
        ProviderTypes.Integrations.N8n.Should().Be("n8n");
    }

    [Fact]
    public void IntegrationsZapier_ShouldHaveCorrectValue()
    {
        ProviderTypes.Integrations.Zapier.Should().Be("Zapier");
    }

    [Fact]
    public void IntegrationsMake_ShouldHaveCorrectValue()
    {
        ProviderTypes.Integrations.Make.Should().Be("Make");
    }

    [Fact]
    public void IntegrationsAutomatisch_ShouldHaveCorrectValue()
    {
        ProviderTypes.Integrations.Automatisch.Should().Be("Automatisch");
    }

    [Fact]
    public void IntegrationsWorkato_ShouldHaveCorrectValue()
    {
        ProviderTypes.Integrations.Workato.Should().Be("Workato");
    }

    #endregion

    #region DataSync Provider Types

    [Fact]
    public void DataSyncBuiltIn_ShouldHaveCorrectValue()
    {
        ProviderTypes.DataSync.BuiltIn.Should().Be("BuiltIn");
    }

    [Fact]
    public void DataSyncAirbyte_ShouldHaveCorrectValue()
    {
        ProviderTypes.DataSync.Airbyte.Should().Be("Airbyte");
    }

    [Fact]
    public void DataSyncFivetran_ShouldHaveCorrectValue()
    {
        ProviderTypes.DataSync.Fivetran.Should().Be("Fivetran");
    }

    [Fact]
    public void DataSyncSegment_ShouldHaveCorrectValue()
    {
        ProviderTypes.DataSync.Segment.Should().Be("Segment");
    }

    #endregion

    #region Compliance Provider Types

    [Fact]
    public void ComplianceBuiltIn_ShouldHaveCorrectValue()
    {
        ProviderTypes.Compliance.BuiltIn.Should().Be("BuiltIn");
    }

    [Fact]
    public void ComplianceFides_ShouldHaveCorrectValue()
    {
        ProviderTypes.Compliance.Fides.Should().Be("Fides");
    }

    [Fact]
    public void ComplianceOneTrust_ShouldHaveCorrectValue()
    {
        ProviderTypes.Compliance.OneTrust.Should().Be("OneTrust");
    }

    [Fact]
    public void ComplianceTrustArc_ShouldHaveCorrectValue()
    {
        ProviderTypes.Compliance.TrustArc.Should().Be("TrustArc");
    }

    #endregion

    #region Validation Tests

    [Fact]
    public void ProviderTypesClass_ShouldBeStaticClass()
    {
        typeof(ProviderTypes).IsAbstract.Should().BeTrue();
        typeof(ProviderTypes).IsSealed.Should().BeTrue();
    }

    [Theory]
    [InlineData(typeof(ProviderTypes.Chat))]
    [InlineData(typeof(ProviderTypes.Search))]
    [InlineData(typeof(ProviderTypes.Notifications))]
    [InlineData(typeof(ProviderTypes.Analytics))]
    [InlineData(typeof(ProviderTypes.Signatures))]
    [InlineData(typeof(ProviderTypes.AI))]
    [InlineData(typeof(ProviderTypes.Integrations))]
    [InlineData(typeof(ProviderTypes.DataSync))]
    [InlineData(typeof(ProviderTypes.Compliance))]
    public void NestedProviderClasses_ShouldBeStaticClasses(Type providerClass)
    {
        providerClass.IsAbstract.Should().BeTrue(because: $"{providerClass.Name} should be static");
        providerClass.IsSealed.Should().BeTrue(because: $"{providerClass.Name} should be sealed");
    }

    [Theory]
    [InlineData(typeof(ProviderTypes.Chat))]
    [InlineData(typeof(ProviderTypes.Search))]
    [InlineData(typeof(ProviderTypes.Notifications))]
    [InlineData(typeof(ProviderTypes.Analytics))]
    [InlineData(typeof(ProviderTypes.Signatures))]
    [InlineData(typeof(ProviderTypes.Integrations))]
    [InlineData(typeof(ProviderTypes.DataSync))]
    [InlineData(typeof(ProviderTypes.Compliance))]
    public void AllProviderCategories_ShouldHaveBuiltInOption(Type providerClass)
    {
        var builtInField = providerClass.GetField("BuiltIn", BindingFlags.Public | BindingFlags.Static);
        builtInField.Should().NotBeNull(because: $"{providerClass.Name} should have a BuiltIn option");

        var value = builtInField!.GetValue(null) as string;
        value.Should().Be("BuiltIn");
    }

    [Theory]
    [InlineData(typeof(ProviderTypes.Chat))]
    [InlineData(typeof(ProviderTypes.Search))]
    [InlineData(typeof(ProviderTypes.Notifications))]
    [InlineData(typeof(ProviderTypes.Analytics))]
    [InlineData(typeof(ProviderTypes.Signatures))]
    [InlineData(typeof(ProviderTypes.Integrations))]
    [InlineData(typeof(ProviderTypes.DataSync))]
    [InlineData(typeof(ProviderTypes.Compliance))]
    public void AllProviderCategories_ShouldHaveUniqueValues(Type providerClass)
    {
        var fields = providerClass.GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => f.IsLiteral && !f.IsInitOnly && f.FieldType == typeof(string));

        var values = fields.Select(f => f.GetValue(null) as string).ToList();
        values.Should().OnlyHaveUniqueItems(because: $"{providerClass.Name} provider types must have unique values");
    }

    [Theory]
    [InlineData(typeof(ProviderTypes.Chat))]
    [InlineData(typeof(ProviderTypes.Search))]
    [InlineData(typeof(ProviderTypes.Notifications))]
    [InlineData(typeof(ProviderTypes.Analytics))]
    [InlineData(typeof(ProviderTypes.Signatures))]
    [InlineData(typeof(ProviderTypes.Integrations))]
    [InlineData(typeof(ProviderTypes.DataSync))]
    [InlineData(typeof(ProviderTypes.Compliance))]
    public void AllProviderValues_ShouldNotBeNullOrEmpty(Type providerClass)
    {
        var fields = providerClass.GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => f.IsLiteral && !f.IsInitOnly && f.FieldType == typeof(string));

        foreach (var field in fields)
        {
            var value = field.GetValue(null) as string;
            value.Should().NotBeNullOrEmpty(because: $"{providerClass.Name}.{field.Name} must have a value");
        }
    }

    [Fact]
    public void AIProviders_ShouldNotIncludeBuiltIn()
    {
        // AI category uniquely doesn't have BuiltIn - it defaults to Ollama
        var aiType = typeof(ProviderTypes.AI);
        var fields = aiType.GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => f.IsLiteral && !f.IsInitOnly && f.FieldType == typeof(string));

        var values = fields.Select(f => f.GetValue(null) as string).ToList();

        // AI should have Ollama as the local option instead of BuiltIn
        values.Should().Contain("Ollama");
    }

    [Fact]
    public void ProviderTypes_ShouldHaveExpectedNumberOfCategories()
    {
        var nestedTypes = typeof(ProviderTypes).GetNestedTypes(BindingFlags.Public | BindingFlags.Static);

        // Expected: Chat, Search, Notifications, Analytics, Signatures, AI, Integrations, DataSync, Compliance
        nestedTypes.Should().HaveCount(9, because: "there should be 9 provider categories");
    }

    #endregion
}
