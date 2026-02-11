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

namespace CRM.Core.Features;

/// <summary>
/// Provider type constants for each pluggable category.
/// These are used in appsettings.json to specify which provider implementation to use.
/// </summary>
public static class ProviderTypes
{
    /// <summary>
    /// Chat/Messaging provider types.
    /// Used with: Providers:Chat:Type
    /// </summary>
    public static class Chat
    {
        /// <summary>Built-in chat functionality (basic, uses Activity entity)</summary>
        public const string BuiltIn = "BuiltIn";

        /// <summary>Chatwoot - Open source customer engagement platform</summary>
        public const string Chatwoot = "Chatwoot";

        /// <summary>Intercom - SaaS customer messaging platform</summary>
        public const string Intercom = "Intercom";

        /// <summary>Zendesk - SaaS customer service platform</summary>
        public const string Zendesk = "Zendesk";

        /// <summary>Freshchat - SaaS messaging platform by Freshworks</summary>
        public const string Freshchat = "Freshchat";

        /// <summary>Rocket.Chat - Open source team communication platform</summary>
        public const string RocketChat = "RocketChat";
    }

    /// <summary>
    /// Search provider types.
    /// Used with: Providers:Search:Type
    /// </summary>
    public static class Search
    {
        /// <summary>Built-in search using EF Core queries</summary>
        public const string BuiltIn = "BuiltIn";

        /// <summary>Meilisearch - Open source search engine</summary>
        public const string Meilisearch = "Meilisearch";

        /// <summary>Algolia - SaaS search platform</summary>
        public const string Algolia = "Algolia";

        /// <summary>Typesense - Open source search engine</summary>
        public const string Typesense = "Typesense";

        /// <summary>Elasticsearch - Open source search and analytics engine</summary>
        public const string Elasticsearch = "Elasticsearch";

        /// <summary>Azure Cognitive Search - Azure managed search service</summary>
        public const string AzureCognitiveSearch = "AzureCognitiveSearch";

        /// <summary>Azure Search - Alias for Azure Cognitive Search</summary>
        public const string AzureSearch = "AzureSearch";
    }

    /// <summary>
    /// Notification provider types.
    /// Used with: Providers:Notifications:Type
    /// </summary>
    public static class Notifications
    {
        /// <summary>Built-in notification using SMTP email</summary>
        public const string BuiltIn = "BuiltIn";

        /// <summary>Novu - Open source notification infrastructure</summary>
        public const string Novu = "Novu";

        /// <summary>Twilio - SaaS communications platform (SMS, Voice, Email)</summary>
        public const string Twilio = "Twilio";

        /// <summary>SendGrid - SaaS email delivery platform</summary>
        public const string SendGrid = "SendGrid";

        /// <summary>OneSignal - SaaS push notification platform</summary>
        public const string OneSignal = "OneSignal";

        /// <summary>Courier - SaaS multi-channel notification platform</summary>
        public const string Courier = "Courier";

        /// <summary>AWS SES - Amazon Simple Email Service</summary>
        public const string AWSSES = "AWSSES";

        /// <summary>AWS SNS - Amazon Simple Notification Service</summary>
        public const string AwsSns = "AwsSns";
    }

    /// <summary>
    /// Analytics/BI provider types.
    /// Used with: Providers:Analytics:Type
    /// </summary>
    public static class Analytics
    {
        /// <summary>Built-in reports and dashboards</summary>
        public const string BuiltIn = "BuiltIn";

        /// <summary>Apache Superset - Open source BI platform</summary>
        public const string Superset = "Superset";

        /// <summary>Metabase - Open source BI platform</summary>
        public const string Metabase = "Metabase";

        /// <summary>Microsoft Power BI - SaaS BI platform</summary>
        public const string PowerBI = "PowerBI";

        /// <summary>Google Looker - SaaS BI platform</summary>
        public const string Looker = "Looker";

        /// <summary>Amazon QuickSight - AWS BI service</summary>
        public const string QuickSight = "QuickSight";

        /// <summary>Tableau - SaaS visual analytics platform</summary>
        public const string Tableau = "Tableau";
    }

    /// <summary>
    /// E-Signature provider types.
    /// Used with: Providers:Signatures:Type
    /// </summary>
    public static class Signatures
    {
        /// <summary>Built-in signature (stub - no actual signing)</summary>
        public const string BuiltIn = "BuiltIn";

        /// <summary>DocuSeal - Open source document signing</summary>
        public const string DocuSeal = "DocuSeal";

        /// <summary>DocuSign - SaaS e-signature platform</summary>
        public const string DocuSign = "DocuSign";

        /// <summary>Adobe Sign - SaaS e-signature platform</summary>
        public const string AdobeSign = "AdobeSign";

        /// <summary>HelloSign - SaaS e-signature platform</summary>
        public const string HelloSign = "HelloSign";

        /// <summary>PandaDoc - SaaS document automation with e-signatures</summary>
        public const string PandaDoc = "PandaDoc";
    }

    /// <summary>
    /// AI/LLM provider types.
    /// Used with: Providers:AI:Type
    /// </summary>
    public static class AI
    {
        /// <summary>Ollama - Local LLM runtime</summary>
        public const string Ollama = "Ollama";

        /// <summary>OpenAI - GPT models API</summary>
        public const string OpenAI = "OpenAI";

        /// <summary>Azure OpenAI - Azure-hosted OpenAI models</summary>
        public const string AzureOpenAI = "AzureOpenAI";

        /// <summary>Anthropic - Claude models API</summary>
        public const string Anthropic = "Anthropic";

        /// <summary>AWS Bedrock - AWS foundation models</summary>
        public const string Bedrock = "Bedrock";

        /// <summary>Google Gemini - Google AI models</summary>
        public const string Gemini = "Gemini";

        /// <summary>DeepSeek - DeepSeek AI models</summary>
        public const string DeepSeek = "DeepSeek";

        /// <summary>OpenRouter - Multi-model AI gateway (access to 100+ models)</summary>
        public const string OpenRouter = "OpenRouter";
    }

    /// <summary>
    /// Integration platform provider types.
    /// Used with: Providers:Integrations:Type
    /// </summary>
    public static class Integrations
    {
        /// <summary>Built-in webhooks only</summary>
        public const string BuiltIn = "BuiltIn";

        /// <summary>n8n - Open source workflow automation</summary>
        public const string N8n = "n8n";

        /// <summary>Zapier - SaaS automation platform</summary>
        public const string Zapier = "Zapier";

        /// <summary>Make (Integromat) - SaaS automation platform</summary>
        public const string Make = "Make";

        /// <summary>Automatisch - Open source Zapier alternative</summary>
        public const string Automatisch = "Automatisch";

        /// <summary>Workato - Enterprise automation platform</summary>
        public const string Workato = "Workato";

        /// <summary>Tray.io - Enterprise automation platform</summary>
        public const string Tray = "Tray";
    }

    /// <summary>
    /// Data sync/ETL provider types.
    /// Used with: Providers:DataSync:Type
    /// </summary>
    public static class DataSync
    {
        /// <summary>Built-in import/export</summary>
        public const string BuiltIn = "BuiltIn";

        /// <summary>Airbyte - Open source data integration</summary>
        public const string Airbyte = "Airbyte";

        /// <summary>Fivetran - SaaS data integration</summary>
        public const string Fivetran = "Fivetran";

        /// <summary>Segment - SaaS customer data platform</summary>
        public const string Segment = "Segment";
    }

    /// <summary>
    /// Compliance/GDPR provider types.
    /// Used with: Providers:Compliance:Type
    /// </summary>
    public static class Compliance
    {
        /// <summary>Built-in GDPR features</summary>
        public const string BuiltIn = "BuiltIn";

        /// <summary>Fides - Open source privacy engineering platform</summary>
        public const string Fides = "Fides";

        /// <summary>OneTrust - SaaS privacy management platform</summary>
        public const string OneTrust = "OneTrust";

        /// <summary>TrustArc - SaaS privacy management platform</summary>
        public const string TrustArc = "TrustArc";
    }
}
