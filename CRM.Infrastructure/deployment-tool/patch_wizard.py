#!/usr/bin/env python3
"""
Patch wizard.html with:
 1. New provider cards: Typesense, RocketChat, Metabase, Gemini, OpenRouter, Workato
 2. pv2-provider-cfg placeholders after each provider grid
 3. renderProviderConfigForm JS + setProviderConfig + clearProviderConfig
 4. Updated selectProvider to call renderProviderConfigForm
 5. Updated applyProviderPreset to call renderProviderConfigForm
 6. Updated saveDraft to include provider_configs
"""
import sys
import os

WIZARD = "/Users/alal/Code/Git CRM Solution/crm-solution/CRM.Infrastructure/deployment-tool/gui/templates/wizard.html"

html = open(WIZARD, encoding="utf-8").read()
original_len = len(html)
changes = []

# ---------------------------------------------------------------------------
# 1. SEARCH GRID: Add Typesense after Algolia + config placeholder
# ---------------------------------------------------------------------------
OLD_SEARCH = '''                        <button type="button" class="pv2-card" onclick="selectProvider('search','algolia')">
                            <div class="pv2-card-top"><span class="pv2-card-icon">🔎</span><span class="pv2-card-title">Algolia</span></div>
                            <div class="pv2-card-desc">Managed SaaS search with analytics &amp; personalization. Usage-based billing.</div>
                            <div class="pv2-tags"><span class="pv2-tag saas">SaaS</span></div>
                        </button>
                    </div>
                </div>

                <!-- Chat -->'''
NEW_SEARCH = '''                        <button type="button" class="pv2-card" onclick="selectProvider('search','algolia')">
                            <div class="pv2-card-top"><span class="pv2-card-icon">🔎</span><span class="pv2-card-title">Algolia</span></div>
                            <div class="pv2-card-desc">Managed SaaS search with analytics &amp; personalization. Usage-based billing.</div>
                            <div class="pv2-tags"><span class="pv2-tag saas">SaaS</span></div>
                        </button>
                        <button type="button" class="pv2-card" onclick="selectProvider('search','typesense')">
                            <div class="pv2-card-top"><span class="pv2-card-icon">🔍</span><span class="pv2-card-title">Typesense</span></div>
                            <div class="pv2-card-desc">Typo-tolerant open-source search. Faster &amp; simpler than Elasticsearch, self-hostable.</div>
                            <div class="pv2-tags"><span class="pv2-tag free">Free</span><span class="pv2-tag selfhost">Self-hosted</span></div>
                        </button>
                    </div>
                    <div class="pv2-provider-cfg" id="pv2-cfg-search"></div>
                </div>

                <!-- Chat -->'''
if OLD_SEARCH in html:
    html = html.replace(OLD_SEARCH, NEW_SEARCH, 1)
    changes.append("✅ Search: added Typesense + cfg placeholder")
else:
    changes.append("❌ Search: target not found")

# ---------------------------------------------------------------------------
# 2. CHAT GRID: Add RocketChat after Intercom + config placeholder
# ---------------------------------------------------------------------------
OLD_CHAT = '''                        <button type="button" class="pv2-card" onclick="selectProvider('chat','intercom')">
                            <div class="pv2-card-top"><span class="pv2-card-icon">🌐</span><span class="pv2-card-title">Intercom</span></div>
                            <div class="pv2-card-desc">Enterprise SaaS chat with AI-assisted support &amp; product tours.</div>
                            <div class="pv2-tags"><span class="pv2-tag saas">SaaS</span></div>
                        </button>
                    </div>
                </div>

                <!-- Notifications -->'''
NEW_CHAT = '''                        <button type="button" class="pv2-card" onclick="selectProvider('chat','intercom')">
                            <div class="pv2-card-top"><span class="pv2-card-icon">🌐</span><span class="pv2-card-title">Intercom</span></div>
                            <div class="pv2-card-desc">Enterprise SaaS chat with AI-assisted support &amp; product tours.</div>
                            <div class="pv2-tags"><span class="pv2-tag saas">SaaS</span></div>
                        </button>
                        <button type="button" class="pv2-card" onclick="selectProvider('chat','rocketchat')">
                            <div class="pv2-card-top"><span class="pv2-card-icon">🚀</span><span class="pv2-card-title">Rocket.Chat</span></div>
                            <div class="pv2-card-desc">OSS team chat + customer support platform. Self-hosted, GDPR-ready, Slack-compatible.</div>
                            <div class="pv2-tags"><span class="pv2-tag free">Free</span><span class="pv2-tag selfhost">Self-hosted</span></div>
                        </button>
                    </div>
                    <div class="pv2-provider-cfg" id="pv2-cfg-chat"></div>
                </div>

                <!-- Notifications -->'''
if OLD_CHAT in html:
    html = html.replace(OLD_CHAT, NEW_CHAT, 1)
    changes.append("✅ Chat: added RocketChat + cfg placeholder")
else:
    changes.append("❌ Chat: target not found")

# ---------------------------------------------------------------------------
# 3. NOTIFICATIONS GRID: Add config placeholder
# ---------------------------------------------------------------------------
OLD_NOTIF = '''                        <button type="button" class="pv2-card" onclick="selectProvider('notification','sendgrid')">
                            <div class="pv2-card-top"><span class="pv2-card-icon">✉️</span><span class="pv2-card-title">SendGrid</span></div>
                            <div class="pv2-card-desc">Scalable transactional email platform by Twilio. 100 emails/day free.</div>
                            <div class="pv2-tags"><span class="pv2-tag saas">SaaS</span></div>
                        </button>
                    </div>
                </div>

                <!-- Analytics -->'''
NEW_NOTIF = '''                        <button type="button" class="pv2-card" onclick="selectProvider('notification','sendgrid')">
                            <div class="pv2-card-top"><span class="pv2-card-icon">✉️</span><span class="pv2-card-title">SendGrid</span></div>
                            <div class="pv2-card-desc">Scalable transactional email platform by Twilio. 100 emails/day free.</div>
                            <div class="pv2-tags"><span class="pv2-tag saas">SaaS</span></div>
                        </button>
                    </div>
                    <div class="pv2-provider-cfg" id="pv2-cfg-notification"></div>
                </div>

                <!-- Analytics -->'''
if OLD_NOTIF in html:
    html = html.replace(OLD_NOTIF, NEW_NOTIF, 1)
    changes.append("✅ Notifications: added cfg placeholder")
else:
    changes.append("❌ Notifications: target not found")

# ---------------------------------------------------------------------------
# 4. ANALYTICS GRID: Add Metabase + config placeholder
# ---------------------------------------------------------------------------
OLD_ANALYTICS = '''                        <button type="button" class="pv2-card" onclick="selectProvider('analytics','powerbi')">
                            <div class="pv2-card-top"><span class="pv2-card-icon">📈</span><span class="pv2-card-title">Power BI</span></div>
                            <div class="pv2-card-desc">Microsoft enterprise BI with AI insights and Office 365 integration.</div>
                            <div class="pv2-tags"><span class="pv2-tag saas">SaaS</span></div>
                        </button>
                    </div>
                </div>

                <!-- E-Signatures -->'''
NEW_ANALYTICS = '''                        <button type="button" class="pv2-card" onclick="selectProvider('analytics','powerbi')">
                            <div class="pv2-card-top"><span class="pv2-card-icon">📈</span><span class="pv2-card-title">Power BI</span></div>
                            <div class="pv2-card-desc">Microsoft enterprise BI with AI insights and Office 365 integration.</div>
                            <div class="pv2-tags"><span class="pv2-tag saas">SaaS</span></div>
                        </button>
                        <button type="button" class="pv2-card" onclick="selectProvider('analytics','metabase')">
                            <div class="pv2-card-top"><span class="pv2-card-icon">📉</span><span class="pv2-card-title">Metabase</span></div>
                            <div class="pv2-card-desc">Loved for simplicity. OSS BI with easy SQL &amp; visual questions, embeddable dashboards.</div>
                            <div class="pv2-tags"><span class="pv2-tag free">Free</span><span class="pv2-tag selfhost">Self-hosted</span></div>
                        </button>
                    </div>
                    <div class="pv2-provider-cfg" id="pv2-cfg-analytics"></div>
                </div>

                <!-- E-Signatures -->'''
if OLD_ANALYTICS in html:
    html = html.replace(OLD_ANALYTICS, NEW_ANALYTICS, 1)
    changes.append("✅ Analytics: added Metabase + cfg placeholder")
else:
    changes.append("❌ Analytics: target not found")

# ---------------------------------------------------------------------------
# 5. SIGNATURE GRID: Add config placeholder
# ---------------------------------------------------------------------------
OLD_SIG = '''                        <button type="button" class="pv2-card" onclick="selectProvider('signature','docusign')">
                            <div class="pv2-card-top"><span class="pv2-card-icon">✍️</span><span class="pv2-card-title">DocuSign</span></div>
                            <div class="pv2-card-desc">Industry-leading e-signature SaaS. Legally binding in 180+ countries.</div>
                            <div class="pv2-tags"><span class="pv2-tag saas">SaaS</span></div>
                        </button>
                    </div>
                </div>

                <!-- AI -->'''
NEW_SIG = '''                        <button type="button" class="pv2-card" onclick="selectProvider('signature','docusign')">
                            <div class="pv2-card-top"><span class="pv2-card-icon">✍️</span><span class="pv2-card-title">DocuSign</span></div>
                            <div class="pv2-card-desc">Industry-leading e-signature SaaS. Legally binding in 180+ countries.</div>
                            <div class="pv2-tags"><span class="pv2-tag saas">SaaS</span></div>
                        </button>
                    </div>
                    <div class="pv2-provider-cfg" id="pv2-cfg-signature"></div>
                </div>

                <!-- AI -->'''
if OLD_SIG in html:
    html = html.replace(OLD_SIG, NEW_SIG, 1)
    changes.append("✅ Signatures: added cfg placeholder")
else:
    changes.append("❌ Signatures: target not found")

# ---------------------------------------------------------------------------
# 6. AI GRID: Add Gemini + OpenRouter after Anthropic + config placeholder
# ---------------------------------------------------------------------------
OLD_AI = '''                        <button type="button" class="pv2-card" onclick="selectProvider('ai','anthropic')">
                            <div class="pv2-card-top"><span class="pv2-card-icon">🧠</span><span class="pv2-card-title">Anthropic Claude</span></div>
                            <div class="pv2-card-desc">Claude 3.5 Sonnet with 200K context window. Excellent for long documents.</div>
                            <div class="pv2-tags"><span class="pv2-tag saas">SaaS</span></div>
                        </button>
                    </div>
                </div>

                <!-- Integrations -->'''
NEW_AI = '''                        <button type="button" class="pv2-card" onclick="selectProvider('ai','anthropic')">
                            <div class="pv2-card-top"><span class="pv2-card-icon">🧠</span><span class="pv2-card-title">Anthropic Claude</span></div>
                            <div class="pv2-card-desc">Claude 3.5 Sonnet with 200K context window. Excellent for long documents.</div>
                            <div class="pv2-tags"><span class="pv2-tag saas">SaaS</span></div>
                        </button>
                        <button type="button" class="pv2-card" onclick="selectProvider('ai','gemini')">
                            <div class="pv2-card-top"><span class="pv2-card-icon">♊</span><span class="pv2-card-title">Google Gemini</span></div>
                            <div class="pv2-card-desc">Gemini 1.5 Pro with 1M token context. Multimodal, fast &amp; cost-effective via Google AI APIs.</div>
                            <div class="pv2-tags"><span class="pv2-tag saas">SaaS</span><span class="pv2-tag cloud">Cloud</span></div>
                        </button>
                        <button type="button" class="pv2-card" onclick="selectProvider('ai','openrouter')">
                            <div class="pv2-card-top"><span class="pv2-card-icon">🔀</span><span class="pv2-card-title">OpenRouter</span></div>
                            <div class="pv2-card-desc">Route to 100+ LLMs (OpenAI, Claude, Gemini, Llama) via a single unified API.</div>
                            <div class="pv2-tags"><span class="pv2-tag saas">SaaS</span></div>
                        </button>
                    </div>
                    <div class="pv2-provider-cfg" id="pv2-cfg-ai"></div>
                </div>

                <!-- Integrations -->'''
if OLD_AI in html:
    html = html.replace(OLD_AI, NEW_AI, 1)
    changes.append("✅ AI: added Gemini + OpenRouter + cfg placeholder")
else:
    changes.append("❌ AI: target not found")

# ---------------------------------------------------------------------------
# 7. INTEGRATION GRID: Add Workato after Make + config placeholder
# ---------------------------------------------------------------------------
OLD_INTEG = '''                        <button type="button" class="pv2-card" onclick="selectProvider('integration','make')">
                            <div class="pv2-card-top"><span class="pv2-card-icon">🔗</span><span class="pv2-card-title">Make</span></div>
                            <div class="pv2-card-desc">Visual automation platform (formerly Integromat). Complex workflows.</div>
                            <div class="pv2-tags"><span class="pv2-tag saas">SaaS</span></div>
                        </button>
                    </div>
                </div>
            
                </div>

                <!-- Secrets & Registry sub-tab -->'''
NEW_INTEG = '''                        <button type="button" class="pv2-card" onclick="selectProvider('integration','make')">
                            <div class="pv2-card-top"><span class="pv2-card-icon">🔗</span><span class="pv2-card-title">Make</span></div>
                            <div class="pv2-card-desc">Visual automation platform (formerly Integromat). Complex workflows.</div>
                            <div class="pv2-tags"><span class="pv2-tag saas">SaaS</span></div>
                        </button>
                        <button type="button" class="pv2-card" onclick="selectProvider('integration','workato')">
                            <div class="pv2-card-top"><span class="pv2-card-icon">🏭</span><span class="pv2-card-title">Workato</span></div>
                            <div class="pv2-card-desc">Enterprise iPaaS with 1000+ connectors, AI-powered automation, governance &amp; compliance.</div>
                            <div class="pv2-tags"><span class="pv2-tag saas">SaaS</span></div>
                        </button>
                    </div>
                    <div class="pv2-provider-cfg" id="pv2-cfg-integration"></div>
                </div>
            
                </div>

                <!-- Secrets & Registry sub-tab -->'''
if OLD_INTEG in html:
    html = html.replace(OLD_INTEG, NEW_INTEG, 1)
    changes.append("✅ Integration: added Workato + cfg placeholder")
else:
    changes.append("❌ Integration: target not found")

# ---------------------------------------------------------------------------
# 8. UPDATE selectProvider to call renderProviderConfigForm
# ---------------------------------------------------------------------------
OLD_SELECT = '''        function selectProvider(type, provider) {
            config[`${type}_provider`] = provider;
            const grid = document.getElementById(`${type}ProviderGrid`);
            if (!grid) return;
            // Support both legacy .provider-option and new .pv2-card buttons
            grid.querySelectorAll('.provider-option, .pv2-card').forEach(el => el.classList.remove('selected'));
            const clicked = event && event.target ? event.target.closest('.provider-option, .pv2-card') : null;
            if (clicked) clicked.classList.add('selected');
            updateSidebar();
        }'''
NEW_SELECT = '''        function selectProvider(type, provider) {
            config[`${type}_provider`] = provider;
            const grid = document.getElementById(`${type}ProviderGrid`);
            if (!grid) return;
            // Support both legacy .provider-option and new .pv2-card buttons
            grid.querySelectorAll('.provider-option, .pv2-card').forEach(el => el.classList.remove('selected'));
            const clicked = event && event.target ? event.target.closest('.provider-option, .pv2-card') : null;
            if (clicked) clicked.classList.add('selected');
            renderProviderConfigForm(type, provider);
            updateSidebar();
        }'''
if OLD_SELECT in html:
    html = html.replace(OLD_SELECT, NEW_SELECT, 1)
    changes.append("✅ selectProvider: added renderProviderConfigForm call")
else:
    changes.append("❌ selectProvider: target not found")

# ---------------------------------------------------------------------------
# 9. UPDATE applyProviderPreset to also call renderProviderConfigForm
# ---------------------------------------------------------------------------
OLD_PRESET_END = '''                    if (btn) btn.classList.add('selected');
                }
            });
        }

        // ── Rich review summary ───────────────────────────────────────────────'''
NEW_PRESET_END = '''                    if (btn) btn.classList.add('selected');
                }
                renderProviderConfigForm(type, p[type]);
            });
        }

        // ── Rich review summary ───────────────────────────────────────────────'''
if OLD_PRESET_END in html:
    html = html.replace(OLD_PRESET_END, NEW_PRESET_END, 1)
    changes.append("✅ applyProviderPreset: added renderProviderConfigForm calls")
else:
    changes.append("❌ applyProviderPreset: target not found")

# ---------------------------------------------------------------------------
# 10. UPDATE saveDraft to include provider_configs
# ---------------------------------------------------------------------------
OLD_SAVE = '''                providers: {
                    search:       config.search_provider       || 'builtin',
                    chat:         config.chat_provider         || 'builtin',
                    notification: config.notification_provider || 'builtin',
                    analytics:    config.analytics_provider    || 'builtin',
                    signature:    config.signature_provider    || 'builtin',
                    ai:           config.ai_provider           || 'ollama',
                    integration:  config.integration_provider  || 'builtin',
                },'''
NEW_SAVE = '''                providers: {
                    search:       config.search_provider       || 'builtin',
                    chat:         config.chat_provider         || 'builtin',
                    notification: config.notification_provider || 'builtin',
                    analytics:    config.analytics_provider    || 'builtin',
                    signature:    config.signature_provider    || 'builtin',
                    ai:           config.ai_provider           || 'ollama',
                    integration:  config.integration_provider  || 'builtin',
                },
                provider_configs: config.provider_configs || {},'''
if OLD_SAVE in html:
    html = html.replace(OLD_SAVE, NEW_SAVE, 1)
    changes.append("✅ saveDraft: added provider_configs")
else:
    changes.append("❌ saveDraft: target not found")

# ---------------------------------------------------------------------------
# 11. ADD renderProviderConfigForm JS BEFORE selectProvider function
# ---------------------------------------------------------------------------
RENDER_JS = '''        // ── Provider Configuration Forms ─────────────────────────────────────────
        // Field definitions per provider — rendered dynamically when a card is selected
        const _CFG_FIELDS = {
            meilisearch: [
                {k:'url',      l:'URL',           t:'text',     ph:'http://crm-meilisearch:7700', req:true},
                {k:'apiKey',   l:'Master API Key', t:'password', ph:'masterKey'},
            ],
            algolia: [
                {k:'appId',       l:'Application ID',   t:'text',     req:true},
                {k:'apiKey',      l:'Search API Key',   t:'password', req:true},
                {k:'adminApiKey', l:'Admin API Key',    t:'password'},
            ],
            typesense: [
                {k:'url',    l:'URL',     t:'text',     ph:'http://localhost:8108', req:true},
                {k:'apiKey', l:'API Key', t:'password', req:true},
            ],
            chatwoot: [
                {k:'baseUrl',   l:'Base URL',          t:'text',     ph:'http://crm-chatwoot:3000', req:true},
                {k:'apiKey',    l:'API Access Token',  t:'password', req:true},
                {k:'accountId', l:'Account ID',        t:'text',     ph:'1'},
                {k:'inboxId',   l:'Inbox ID',          t:'text',     ph:'1'},
            ],
            intercom: [
                {k:'appId',  l:'App ID',   t:'text',     req:true},
                {k:'apiKey', l:'API Key',  t:'password', req:true},
            ],
            rocketchat: [
                {k:'url',           l:'URL',            t:'text',     ph:'http://crm-rocketchat:3000', req:true},
                {k:'adminUser',     l:'Admin Username', t:'text'},
                {k:'adminPassword', l:'Admin Password', t:'password'},
            ],
            novu: [
                {k:'apiKey',        l:'API Key',            t:'password', req:true},
                {k:'applicationId', l:'Application ID',     t:'text'},
                {k:'baseUrl',       l:'Base URL (self-hosted)', t:'text', ph:'http://crm-novu:3000'},
            ],
            twilio: [
                {k:'accountSid', l:'Account SID',  t:'text',     req:true},
                {k:'authToken',  l:'Auth Token',   t:'password', req:true},
                {k:'fromNumber', l:'From Number',  t:'text',     ph:'+15551234567'},
            ],
            sendgrid: [
                {k:'apiKey',    l:'API Key',    t:'password', req:true},
                {k:'fromEmail', l:'From Email', t:'email',    ph:'noreply@example.com'},
                {k:'fromName',  l:'From Name',  t:'text',     ph:'CRM System'},
            ],
            superset: [
                {k:'url',      l:'URL',      t:'text',     ph:'http://crm-superset:8088', req:true},
                {k:'username', l:'Username', t:'text',     ph:'admin'},
                {k:'password', l:'Password', t:'password'},
            ],
            powerbi: [
                {k:'tenantId',     l:'Tenant ID',     t:'text',     req:true},
                {k:'clientId',     l:'Client ID',     t:'text',     req:true},
                {k:'clientSecret', l:'Client Secret', t:'password', req:true},
                {k:'workspaceId',  l:'Workspace ID',  t:'text'},
            ],
            metabase: [
                {k:'url',      l:'URL',      t:'text',     ph:'http://crm-metabase:3000', req:true},
                {k:'username', l:'Username', t:'text',     ph:'admin'},
                {k:'password', l:'Password', t:'password'},
            ],
            docuseal: [
                {k:'url',           l:'URL',            t:'text',     ph:'http://crm-docuseal:3000', req:true},
                {k:'apiKey',        l:'API Key',        t:'password', req:true},
                {k:'webhookSecret', l:'Webhook Secret', t:'password'},
            ],
            docusign: [
                {k:'accountId',      l:'Account ID',      t:'text',     req:true},
                {k:'integrationKey', l:'Integration Key', t:'text',     req:true},
                {k:'baseUrl',        l:'Base URL',        t:'text',     ph:'https://demo.docusign.net/restapi'},
                {k:'privateKey',     l:'RSA Private Key Path', t:'text'},
            ],
            ollama: [
                {k:'url',            l:'URL',             t:'text',     ph:'http://crm-ollama:11434', req:true},
                {k:'model',          l:'Model',           t:'text',     ph:'llama3.1:8b'},
                {k:'embeddingModel', l:'Embedding Model', t:'text',     ph:'nomic-embed-text'},
            ],
            openai: [
                {k:'apiKey',         l:'API Key',          t:'password', req:true},
                {k:'model',          l:'Default Model',    t:'text',     ph:'gpt-4o'},
                {k:'organizationId', l:'Organization ID',  t:'text'},
            ],
            azure_openai: [
                {k:'endpoint',       l:'Endpoint',        t:'text',     ph:'https://xxx.openai.azure.com/', req:true},
                {k:'apiKey',         l:'API Key',         t:'password', req:true},
                {k:'deploymentName', l:'Deployment Name', t:'text',     ph:'gpt-4o'},
                {k:'apiVersion',     l:'API Version',     t:'text',     ph:'2024-02-01'},
            ],
            anthropic: [
                {k:'apiKey', l:'API Key', t:'password', req:true},
                {k:'model',  l:'Model',   t:'text',     ph:'claude-3-5-sonnet-20241022'},
            ],
            gemini: [
                {k:'apiKey', l:'API Key', t:'password', req:true},
                {k:'model',  l:'Model',   t:'text',     ph:'gemini-1.5-pro'},
            ],
            openrouter: [
                {k:'apiKey',  l:'API Key',              t:'password', req:true},
                {k:'model',   l:'Default Model',         t:'text',     ph:'anthropic/claude-3.5-sonnet'},
                {k:'siteUrl', l:'Site URL (for rankings)', t:'text'},
            ],
            n8n: [
                {k:'baseUrl',        l:'Base URL',         t:'text',     ph:'http://crm-n8n:5678', req:true},
                {k:'apiKey',         l:'API Key',          t:'password'},
                {k:'webhookBaseUrl', l:'Webhook Base URL', t:'text',     ph:'http://crm-n8n:5678/webhook'},
            ],
            zapier: [
                {k:'webhookUrl', l:'Catch Hook URL', t:'text', req:true, ph:'https://hooks.zapier.com/hooks/catch/...'},
            ],
            make: [
                {k:'webhookUrl', l:'Webhook URL',        t:'text',     req:true, ph:'https://hook.eu1.make.com/...'},
                {k:'apiKey',     l:'API Token (optional)', t:'password'},
            ],
            workato: [
                {k:'webhookUrl', l:'Webhook URL',          t:'text',     req:true, ph:'https://www.workato.com/webhooks/...'},
                {k:'apiToken',   l:'API Token (optional)', t:'password'},
            ],
        };

        function renderProviderConfigForm(type, provider) {
            const container = document.getElementById('pv2-cfg-' + type);
            if (!container) return;
            if (!provider || provider === 'builtin') { container.innerHTML = ''; return; }
            const fields = _CFG_FIELDS[provider];
            if (!fields || !fields.length) { container.innerHTML = ''; return; }
            const saved = ((config.provider_configs || {})[type]) || {};
            const title = provider.charAt(0).toUpperCase() + provider.slice(1).replace(/_/g, ' ');
            let rows = '';
            for (const f of fields) {
                const val = (saved[f.k] || '').replace(/"/g, '&quot;');
                const reqMark = f.req ? ' <span class="text-danger">*</span>' : '';
                const eyeBtn = f.t === 'password'
                    ? `<button class="btn btn-outline-secondary btn-sm" type="button" tabindex="-1"\n                         onclick="this.previousElementSibling.type=(this.previousElementSibling.type==='password'?'text':'password')"><i class="bi bi-eye"></i></button>`
                    : '';
                const wrap = f.t === 'password' ? '<div class="input-group input-group-sm">' : '';
                const wrapEnd = f.t === 'password' ? `${eyeBtn}</div>` : '';
                const inputType = f.t === 'password' ? 'password' : (f.t === 'email' ? 'email' : 'text');
                rows += `
                <div class="col-md-6">
                  <label class="form-label small mb-1">${f.l}${reqMark}</label>
                  ${wrap}<input type="${inputType}" class="form-control form-control-sm"
                    placeholder="${f.ph || ''}" value="${val}"
                    oninput="setProviderConfig('${type}','${f.k}',this.value)">${wrapEnd}
                </div>`;
            }
            container.innerHTML = `
            <div class="card border-primary mt-2 mb-1">
              <div class="card-header py-2 bg-primary bg-opacity-10 d-flex justify-content-between align-items-center">
                <span class="fw-semibold text-primary" style="font-size:0.82rem"><i class="bi bi-key me-1"></i>${title} Configuration</span>
                <button type="button" class="btn-close" style="font-size:0.65rem" onclick="clearProviderSelection('${type}')"></button>
              </div>
              <div class="card-body py-2 px-3">
                <div class="row g-2">${rows}
                </div>
              </div>
            </div>`;
        }

        function setProviderConfig(type, key, value) {
            if (!config.provider_configs) config.provider_configs = {};
            if (!config.provider_configs[type]) config.provider_configs[type] = {};
            config.provider_configs[type][key] = value;
        }

        function clearProviderSelection(type) {
            const container = document.getElementById('pv2-cfg-' + type);
            if (container) container.innerHTML = '';
            if (config.provider_configs) delete config.provider_configs[type];
            config[type + '_provider'] = 'builtin';
            const grid = document.getElementById(type + 'ProviderGrid');
            if (grid) {
                grid.querySelectorAll('.pv2-card, .provider-option').forEach(el => el.classList.remove('selected'));
                const builtinBtn = Array.from(grid.querySelectorAll('.pv2-card')).find(
                    b => (b.getAttribute('onclick') || '').includes("'builtin'"));
                if (builtinBtn) builtinBtn.classList.add('selected');
            }
            updateSidebar();
        }

'''
INSERT_BEFORE = '''        function selectProvider(type, provider) {'''
if INSERT_BEFORE in html:
    html = html.replace(INSERT_BEFORE, RENDER_JS + INSERT_BEFORE, 1)
    changes.append("✅ Added renderProviderConfigForm + setProviderConfig + clearProviderSelection JS")
else:
    changes.append("❌ renderProviderConfigForm insertion: selectProvider not found")

# ---------------------------------------------------------------------------
# Write back
# ---------------------------------------------------------------------------
if html == open(WIZARD, encoding="utf-8").read():
    print("WARNING: No changes were made to wizard.html")
    for c in changes:
        print(" ", c)
    sys.exit(1)

with open(WIZARD, "w", encoding="utf-8") as f:
    f.write(html)

print(f"wizard.html patched: {original_len} -> {len(html)} chars (+{len(html)-original_len})")
for c in changes:
    print(" ", c)
