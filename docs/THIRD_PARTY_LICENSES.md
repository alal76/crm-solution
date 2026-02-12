# CRM Solution — Third-Party Licenses & Dependency Inventory

> **Last Updated:** February 17, 2026  
> **CRM Solution License:** [AGPL-3.0-or-later](../LICENSE)  
> **Copyright:** 2024–2026 Abhishek Lal  
> **Purpose:** Complete inventory of all third-party dependencies, their licenses, and compatibility analysis.

---

## Table of Contents

1. [CRM Solution License](#1-crm-solution-license)
2. [License Compatibility Summary](#2-license-compatibility-summary)
3. [Backend Dependencies (NuGet)](#3-backend-dependencies-nuget)
4. [Frontend Dependencies (npm)](#4-frontend-dependencies-npm)
5. [Infrastructure — Docker Images](#5-infrastructure--docker-images)
6. [External Service Integrations (API-Only)](#6-external-service-integrations-api-only)
7. [Provider Licensing Matrix](#7-provider-licensing-matrix)
8. [Special Considerations](#8-special-considerations)
9. [License Texts Summary](#9-license-texts-summary)
10. [Compliance Checklist](#10-compliance-checklist)

---

## 1. CRM Solution License

The CRM Solution is licensed under the **GNU Affero General Public License v3.0 or later (AGPL-3.0-or-later)**.

| Attribute | Value |
|-----------|-------|
| **SPDX Identifier** | `AGPL-3.0-or-later` |
| **OSI Approved** | ✅ Yes |
| **Copyleft** | ✅ Strong (network copyleft) |
| **Patent Grant** | ✅ Yes |
| **Key Requirement** | Source code must be made available to users who interact with the software over a network |

### Additional Terms (per LICENSE file)

| Clause | Description |
|--------|-------------|
| **No Liability** | Software provided "as is" without warranty |
| **Free Use** | May be used without restrictions for personal and commercial purposes |
| **Copyleft Notice** | Modifications must be distributed under AGPL-3.0 or compatible |

---

## 2. License Compatibility Summary

All dependencies are compatible with AGPL-3.0-or-later distribution:

| License Type | Count | AGPL-3.0 Compatible | Notes |
|--------------|-------|----------------------|-------|
| MIT | ~65 | ✅ Yes | Permissive — no restrictions |
| Apache 2.0 | ~8 | ✅ Yes | Compatible with GPL v3+ |
| BSD 2-Clause / 3-Clause | ~5 | ✅ Yes | Permissive |
| ISC | ~3 | ✅ Yes | Permissive (MIT-equivalent) |
| PostgreSQL License | 2 | ✅ Yes | Permissive (MIT-like) |
| GPL-2.0 | 1 | ⚠️ See note | MariaDB — separate process, not linked |
| AGPL-3.0 | 1 | ✅ Yes | DocuSeal — same license family |
| SSPL-1.0 | 1 | ⚠️ See note | MongoDB — Novu internal dependency |
| Oracle Free Use T&C | 1 | ⚠️ See note | Optional database provider only |
| Sustainable Use License | 1 | ✅ N/A | n8n — API-only integration, not bundled |

**Overall Assessment: ✅ No licensing conflicts for distribution under AGPL-3.0.**

---

## 3. Backend Dependencies (NuGet)

### 3.1 Core Framework

| Package | Version | License | OSI Approved |
|---------|---------|---------|--------------|
| Microsoft.AspNetCore.* | 8.0.x | MIT | ✅ |
| Microsoft.EntityFrameworkCore | 8.0.x | MIT | ✅ |
| Microsoft.Extensions.* | 8.0.x–9.0.x | MIT | ✅ |
| Microsoft.FeatureManagement.AspNetCore | 3.5.0 | MIT | ✅ |
| Microsoft.AspNetCore.SignalR | 8.0.x | MIT | ✅ |
| System.IdentityModel.Tokens.Jwt | 7.x | MIT | ✅ |

### 3.2 Database Providers

| Package | Version | License | OSI Approved | Notes |
|---------|---------|---------|--------------|-------|
| Pomelo.EntityFrameworkCore.MySql | 8.0.0 | MIT | ✅ | MariaDB/MySQL provider |
| Npgsql.EntityFrameworkCore.PostgreSQL | 8.0.11 | PostgreSQL License | ✅ | PostgreSQL provider |
| Microsoft.EntityFrameworkCore.SqlServer | 8.0.x | MIT | ✅ | SQL Server provider |
| Oracle.EntityFrameworkCore | 8.21.121 | Oracle Free Use T&C | ⚠️ No | Optional — see [§8.1](#81-oracle-entityframeworkcore) |

### 3.3 Security & Authentication

| Package | Version | License | OSI Approved |
|---------|---------|---------|--------------|
| BCrypt.Net-Next | 4.0.3 | MIT | ✅ |
| Microsoft.AspNetCore.Authentication.JwtBearer | 8.0.x | MIT | ✅ |

### 3.4 Provider SDKs

| Package | Version | License | OSI Approved | Provider Category |
|---------|---------|---------|--------------|-------------------|
| Algolia.Search | 7.11.0 | MIT | ✅ | Search |
| Meilisearch | 0.15.0 | MIT | ✅ | Search |
| Twilio | 7.14.2 | MIT | ✅ | Notifications |
| SendGrid | 9.29.3 | MIT | ✅ | Notifications |
| DocuSign.eSign.dll | 8.0.6 | MIT | ✅ | E-Signatures |

### 3.5 Infrastructure & Utilities

| Package | Version | License | OSI Approved |
|---------|---------|---------|--------------|
| StackExchange.Redis | 2.7.33 | MIT | ✅ |
| Serilog.AspNetCore | 8.x | Apache 2.0 | ✅ |
| Serilog.Sinks.Console | 5.x | Apache 2.0 | ✅ |
| Serilog.Sinks.File | 5.x | Apache 2.0 | ✅ |
| Polly | 8.2.0 | BSD 3-Clause | ✅ |
| Cronos | 0.8.4 | MIT | ✅ |
| Swashbuckle.AspNetCore | 6.x | MIT | ✅ |
| AutoMapper | 12.x | MIT | ✅ |
| FluentValidation | 11.x | Apache 2.0 | ✅ |

---

## 4. Frontend Dependencies (npm)

### 4.1 Core Framework

| Package | Version | License | OSI Approved |
|---------|---------|---------|--------------|
| react | 18.x | MIT | ✅ |
| react-dom | 18.x | MIT | ✅ |
| react-router-dom | 6.x | MIT | ✅ |
| typescript | 5.x | Apache 2.0 | ✅ |

### 4.2 UI Components

| Package | Version | License | OSI Approved |
|---------|---------|---------|--------------|
| @mui/material | 5.x | MIT | ✅ |
| @mui/icons-material | 5.x | MIT | ✅ |
| @mui/x-data-grid | 6.x | MIT | ✅ |
| @mui/x-date-pickers | 6.x | MIT | ✅ |
| @emotion/react | 11.x | MIT | ✅ |
| @emotion/styled | 11.x | MIT | ✅ |
| recharts | 2.x | MIT | ✅ |
| @hello-pangea/dnd | 16.x | Apache 2.0 | ✅ |

### 4.3 Forms & Validation

| Package | Version | License | OSI Approved |
|---------|---------|---------|--------------|
| formik | 2.x | Apache 2.0 | ✅ |
| yup | 1.x | MIT | ✅ |
| zod | 3.x | MIT | ✅ |

### 4.4 HTTP & Real-Time

| Package | Version | License | OSI Approved |
|---------|---------|---------|--------------|
| axios | 1.x | MIT | ✅ |
| @microsoft/signalr | 8.x | MIT | ✅ |

### 4.5 Utilities

| Package | Version | License | OSI Approved |
|---------|---------|---------|--------------|
| date-fns | 2.x | MIT | ✅ |
| dompurify | 3.x | Apache 2.0 / MPL 2.0 | ✅ |
| qrcode.react | 3.x | ISC | ✅ |
| @react-oauth/google | 0.12.x | MIT | ✅ |
| react-quill | 2.x | MIT | ✅ |
| react-markdown | 9.x | MIT | ✅ |

### 4.6 Development Dependencies

| Package | Version | License | OSI Approved |
|---------|---------|---------|--------------|
| @craco/craco | 7.x | Apache 2.0 | ✅ |
| jest | 29.x | MIT | ✅ |
| @testing-library/react | 14.x | MIT | ✅ |
| eslint | 8.x | MIT | ✅ |

---

## 5. Infrastructure — Docker Images

### 5.1 Core Infrastructure (Always Required)

| Image | Version | License | OSI Approved | Notes |
|-------|---------|---------|--------------|-------|
| mariadb | 11.2 | GPL-2.0 | ✅ | Separate process — no linking. GPL does not infect AGPL app. |
| redis | 7-alpine | BSD 3-Clause | ✅ | Permissive. Used for caching. |
| getmeili/meilisearch | v1.6 | MIT | ✅ | Optional search engine. |
| ollama/ollama | latest | MIT | ✅ | Optional local LLM inference. |

### 5.2 Optional Provider Infrastructure (docker-compose.providers.yml)

| Image | Version | License | OSI Approved | Provider Category |
|-------|---------|---------|--------------|-------------------|
| ghcr.io/novuhq/novu/api | 0.24.0 | MIT | ✅ | Notifications |
| ghcr.io/novuhq/novu/worker | 0.24.0 | MIT | ✅ | Notifications |
| ghcr.io/novuhq/novu/web | 0.24.0 | MIT | ✅ | Notifications |
| ghcr.io/novuhq/novu/widget | 0.24.0 | MIT | ✅ | Notifications |
| ghcr.io/novuhq/novu/ws | 0.24.0 | MIT | ✅ | Notifications |
| mongo | 6 | SSPL-1.0 | ⚠️ No | Novu internal DB — see [§8.2](#82-mongodb-sspl-10) |
| chatwoot/chatwoot | v3.13.0 | MIT | ✅ | Chat |
| postgres | 15-alpine | PostgreSQL License | ✅ | Used by Chatwoot, DocuSeal, Superset |
| docuseal/docuseal | 1.5.7 | AGPL-3.0 | ✅ | E-Signatures |
| apache/superset | 3.1.0 | Apache 2.0 | ✅ | Analytics |

---

## 6. External Service Integrations (API-Only)

These services are accessed exclusively via REST API calls. **No code is bundled, embedded, or linked.** Integration is through HTTP clients using `HttpClient` in .NET.

| Service | Integration Method | Service License | Bundled? | Impact on CRM License |
|---------|-------------------|-----------------|----------|----------------------|
| **n8n** | REST API (`HttpClient`) | Sustainable Use License | ❌ No | ✅ None — API boundary |
| **Zapier** | REST API (webhooks) | Proprietary SaaS | ❌ No | ✅ None — SaaS boundary |
| **Make (Integromat)** | REST API (webhooks) | Proprietary SaaS | ❌ No | ✅ None — SaaS boundary |
| **Workato** | REST API (webhooks) | Proprietary SaaS | ❌ No | ✅ None — SaaS boundary |
| **OpenAI** | REST API (`HttpClient`) | Proprietary SaaS | ❌ No | ✅ None — SaaS boundary |
| **Azure OpenAI** | REST API (`HttpClient`) | Proprietary SaaS | ❌ No | ✅ None — SaaS boundary |
| **Anthropic** | REST API (`HttpClient`) | Proprietary SaaS | ❌ No | ✅ None — SaaS boundary |
| **AWS Bedrock** | REST API (`HttpClient`) | Proprietary SaaS | ❌ No | ✅ None — SaaS boundary |
| **OpenRouter** | REST API (`HttpClient`) | Proprietary SaaS | ❌ No | ✅ None — SaaS boundary |
| **Google Gemini** | REST API (`HttpClient`) | Proprietary SaaS | ❌ No | ✅ None — SaaS boundary |
| **Stripe** | REST API (webhooks) | Proprietary SaaS | ❌ No | ✅ None — SaaS boundary |
| **Intercom** | REST API (`HttpClient`) | Proprietary SaaS | ❌ No | ✅ None — SaaS boundary |
| **DocuSign** | SDK (`DocuSign.eSign.dll`, MIT) | Proprietary SaaS | SDK only | ✅ SDK is MIT-licensed |
| **Algolia** | SDK (`Algolia.Search`, MIT) | Proprietary SaaS | SDK only | ✅ SDK is MIT-licensed |
| **Power BI** | REST API (`HttpClient`) | Proprietary SaaS | ❌ No | ✅ None — SaaS boundary |

### Why API-Only Integrations Are License-Safe

The AGPL-3.0 copyleft applies to **linked, combined, or derivative works**. REST API calls across a network boundary are explicitly **not** considered linking under the GPL/AGPL. Each external service operates as a separate program. This is consistent with the [FSF's interpretation of GPL](https://www.gnu.org/licenses/gpl-faq.html#GPLAndPlugins) and established open-source legal consensus.

---

## 7. Provider Licensing Matrix

Complete matrix of all pluggable providers showing license status:

### 7.1 Search Providers

| Provider | Type | License | SDK/Integration | Compatible |
|----------|------|---------|-----------------|------------|
| **BuiltIn** (SQL LIKE) | Built-in | AGPL-3.0 (CRM) | — | ✅ |
| **Meilisearch** | Self-hosted OSS | MIT | NuGet SDK (MIT) | ✅ |
| **Algolia** | Cloud SaaS | Proprietary | NuGet SDK (MIT) | ✅ |
| **Typesense** | Self-hosted OSS | GPL-3.0 | REST API | ✅ |
| **Elasticsearch** | Self-hosted | SSPL-1.0 / Elastic License 2.0 | REST API | ⚠️ Operator responsibility |
| **Azure Cognitive Search** | Cloud SaaS | Proprietary | REST API | ✅ |

### 7.2 Notification Providers

| Provider | Type | License | SDK/Integration | Compatible |
|----------|------|---------|-----------------|------------|
| **BuiltIn** (SMTP) | Built-in | AGPL-3.0 (CRM) | — | ✅ |
| **Novu** | Self-hosted OSS | MIT | REST API | ✅ |
| **Twilio** | Cloud SaaS | Proprietary | NuGet SDK (MIT) | ✅ |
| **SendGrid** | Cloud SaaS | Proprietary | NuGet SDK (MIT) | ✅ |
| **OneSignal** | Cloud SaaS | Proprietary | REST API | ✅ |
| **Courier** | Cloud SaaS | Proprietary | REST API | ✅ |
| **AWS SES** | Cloud SaaS | Proprietary | REST API | ✅ |

### 7.3 Chat Providers

| Provider | Type | License | SDK/Integration | Compatible |
|----------|------|---------|-----------------|------------|
| **BuiltIn** (in-memory) | Built-in | AGPL-3.0 (CRM) | — | ✅ |
| **Chatwoot** | Self-hosted OSS | MIT | REST API | ✅ |
| **Intercom** | Cloud SaaS | Proprietary | REST API | ✅ |
| **Zendesk** | Cloud SaaS | Proprietary | REST API | ✅ |
| **Freshchat** | Cloud SaaS | Proprietary | REST API | ✅ |
| **Rocket.Chat** | Self-hosted OSS | MIT | REST API | ✅ |

### 7.4 E-Signature Providers

| Provider | Type | License | SDK/Integration | Compatible |
|----------|------|---------|-----------------|------------|
| **BuiltIn** (manual) | Built-in | AGPL-3.0 (CRM) | — | ✅ |
| **DocuSeal** | Self-hosted OSS | AGPL-3.0 | REST API | ✅ |
| **DocuSign** | Cloud SaaS | Proprietary | NuGet SDK (MIT) | ✅ |
| **Adobe Sign** | Cloud SaaS | Proprietary | REST API | ✅ |
| **HelloSign** | Cloud SaaS | Proprietary | REST API | ✅ |

### 7.5 Analytics Providers

| Provider | Type | License | SDK/Integration | Compatible |
|----------|------|---------|-----------------|------------|
| **BuiltIn** (dashboards) | Built-in | AGPL-3.0 (CRM) | — | ✅ |
| **Apache Superset** | Self-hosted OSS | Apache 2.0 | REST API | ✅ |
| **Metabase** | Self-hosted OSS | AGPL-3.0 | REST API | ✅ |
| **Power BI** | Cloud SaaS | Proprietary | REST API | ✅ |
| **Looker** | Cloud SaaS | Proprietary | REST API | ✅ |
| **AWS QuickSight** | Cloud SaaS | Proprietary | REST API | ✅ |

### 7.6 Integration Platforms

| Provider | Type | License | SDK/Integration | Compatible |
|----------|------|---------|-----------------|------------|
| **BuiltIn** (webhooks) | Built-in | AGPL-3.0 (CRM) | — | ✅ |
| **n8n** | Self-hosted | Sustainable Use License | REST API | ✅ (not bundled) |
| **Zapier** | Cloud SaaS | Proprietary | REST API / Webhooks | ✅ |
| **Make** | Cloud SaaS | Proprietary | REST API / Webhooks | ✅ |
| **Workato** | Cloud SaaS | Proprietary | REST API | ✅ |

### 7.7 AI/LLM Providers

| Provider | Type | License | SDK/Integration | Compatible |
|----------|------|---------|-----------------|------------|
| **Ollama** | Self-hosted OSS | MIT | REST API | ✅ |
| **OpenAI** | Cloud SaaS | Proprietary | REST API | ✅ |
| **Azure OpenAI** | Cloud SaaS | Proprietary | REST API | ✅ |
| **Anthropic (Claude)** | Cloud SaaS | Proprietary | REST API | ✅ |
| **AWS Bedrock** | Cloud SaaS | Proprietary | REST API | ✅ |
| **OpenRouter** | Cloud SaaS | Proprietary | REST API | ✅ |
| **Google Gemini** | Cloud SaaS | Proprietary | REST API | ✅ |

---

## 8. Special Considerations

### 8.1 Oracle EntityFrameworkCore

| Attribute | Detail |
|-----------|--------|
| **Package** | `Oracle.EntityFrameworkCore` 8.21.121 |
| **License** | Oracle Free Use Terms and Conditions |
| **OSI Approved** | ⚠️ No |
| **Risk** | Low — optional database provider |
| **Mitigation** | Only loaded when `DatabaseProvider=oracle` is configured. Not part of default deployment. Users choosing Oracle accept Oracle's license independently. |

**Recommendation:** Document that Oracle database support requires acceptance of Oracle's Free Use Terms and Conditions by the operator.

### 8.2 MongoDB (SSPL-1.0)

| Attribute | Detail |
|-----------|--------|
| **Image** | `mongo:6` |
| **License** | Server Side Public License 1.0 |
| **OSI Approved** | ⚠️ No (submitted but withdrawn) |
| **Usage** | Internal database for self-hosted Novu stack only |
| **Risk** | Low — CRM does not interact with MongoDB directly |
| **Mitigation** | MongoDB is a dependency of Novu, not of the CRM. Operators self-hosting Novu accept MongoDB's SSPL independently. The CRM communicates only with Novu's API. |

**Recommendation:** Operators who self-host Novu should review SSPL-1.0 compliance requirements. Alternatively, use a cloud-hosted MongoDB (Atlas) or switch to the BuiltIn notification provider.

### 8.3 n8n (Sustainable Use License)

| Attribute | Detail |
|-----------|--------|
| **Integration** | REST API via `HttpClient` in `N8nProvider.cs` |
| **License** | Sustainable Use License (changed from Apache 2.0 in 2022) |
| **OSI Approved** | ⚠️ No |
| **Key Restrictions** | Cannot compete with n8n; cannot offer n8n as a managed service |
| **Bundled in CRM?** | ❌ No — no code, no Docker image, no SDK |
| **Risk** | None to CRM codebase |

**Analysis:**
- The CRM integrates with n8n purely via REST API calls (`HttpClient`).
- No n8n code is included, compiled, or distributed with the CRM.
- The `docker-compose.providers.yml` does **not** include an n8n image (n8n is referenced only in configuration documentation).
- Under AGPL-3.0, GPL, and established legal interpretation, REST API communication across process boundaries does not create a derivative work.
- The CRM's integration architecture treats n8n identically to Zapier, Make, and Workato — all are interchangeable via the `IIntegrationPort` factory.

**Recommendation:** Operators who choose to deploy n8n should independently review the [Sustainable Use License](https://github.com/n8n-io/n8n/blob/master/LICENSE.md) to ensure their use case is permitted.

### 8.4 MariaDB (GPL-2.0)

| Attribute | Detail |
|-----------|--------|
| **Image** | `mariadb:11.2` |
| **License** | GPL-2.0 |
| **Risk** | None — separate process communication via TCP |
| **Mitigation** | The CRM communicates with MariaDB via the MySQL protocol over a network socket. This is not linking under the GPL. The Pomelo EF Core driver (MIT-licensed) handles the connection. |

### 8.5 DocuSeal (AGPL-3.0)

| Attribute | Detail |
|-----------|--------|
| **Image** | `docuseal/docuseal:1.5.7` |
| **License** | AGPL-3.0 |
| **Risk** | None — same license family as the CRM |
| **Mitigation** | Both the CRM and DocuSeal are AGPL-3.0. Communication is via REST API. No licensing conflict. |

---

## 9. License Texts Summary

| License | SPDX ID | Type | Key Terms |
|---------|---------|------|-----------|
| **MIT** | MIT | Permissive | Use/modify/distribute freely with copyright notice |
| **Apache 2.0** | Apache-2.0 | Permissive | Use/modify/distribute with notice + patent grant |
| **BSD 2-Clause** | BSD-2-Clause | Permissive | Use/modify/distribute with copyright notice |
| **BSD 3-Clause** | BSD-3-Clause | Permissive | Same as 2-Clause + no endorsement clause |
| **ISC** | ISC | Permissive | MIT-equivalent, simplified |
| **PostgreSQL** | PostgreSQL | Permissive | MIT-like, use freely with notice |
| **GPL-2.0** | GPL-2.0-only | Copyleft | Distribute source for derivatives |
| **AGPL-3.0** | AGPL-3.0-or-later | Strong Copyleft | Source for network users + derivatives |
| **SSPL-1.0** | SSPL-1.0 | Strong Copyleft | Source for "service" providers (all management layers) |
| **Oracle Free Use** | — | Restricted | Free use but non-OSI terms |
| **Sustainable Use** | — | Source-available | No competing with licensor |

---

## 10. Compliance Checklist

### For CRM Solution Distributors

- [x] Include AGPL-3.0 LICENSE file with all distributions
- [x] Provide corresponding source code (or written offer)
- [x] Preserve all copyright notices in source files
- [x] Document all third-party licenses (this file)
- [x] Identify non-OSI components as optional (Oracle, MongoDB)
- [ ] Include NOTICE file for Apache 2.0 dependencies (Serilog, FormIk, etc.)

### For CRM Solution Operators (Self-Hosted)

- [ ] Review AGPL-3.0 obligations for any modifications made
- [ ] If self-hosting Novu: review MongoDB SSPL-1.0 compliance
- [ ] If using Oracle database: accept Oracle Free Use Terms
- [ ] If deploying n8n: review Sustainable Use License terms
- [ ] If deploying Elasticsearch: review Elastic License 2.0 / SSPL terms
- [ ] Ensure source code availability for AGPL-3.0 if serving over network

### For SaaS-Only Provider Users

- [ ] No additional license concerns — SaaS providers are accessed via API
- [ ] SDK licenses (MIT) already permit bundling and distribution
- [ ] Operator is responsible for their own SaaS subscription agreements

---

**END OF THIRD-PARTY LICENSES DOCUMENT**
