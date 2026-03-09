#!/usr/bin/env python3
"""Batch 25: General Knowledge Base — Sales, Service & Marketing Articles.

Creates extensive knowledge articles for the General Knowledge Base system
(separate from ITSM KB) covering:
- Sales: Playbooks, objection handling, competitive analysis, product positioning
- Service: Customer onboarding, troubleshooting guides, account management
- Marketing: Campaign guides, brand guidelines, content strategy, SEO

NOTE: The General KB backend (/api/knowledge) is NOT YET IMPLEMENTED.
This batch probes the endpoint first and skips gracefully if unavailable.
Once the General KB controller is built (KB-002→005 in MASTER_TODO_LIST),
these articles will be loaded automatically on the next data-loader run.

See: SPEC-SD-002-KnowledgeBase.md (System B — General Knowledge Base)
See: ADR-005-Knowledge-Base-Dual-System-Architecture.md
"""
from __future__ import annotations
import sys, os, time
sys.path.insert(0, os.path.dirname(__file__))
from loader_utils import (
    ApiClient, RunLogger, save_ids, load_ids,
    check_service_availability,
)

# ── General KB ArticleType enum (from KnowledgeBase/KnowledgeArticle.cs) ──
# These are the GENERAL KB ArticleType values (different from ITSM KB!)
# Values from CRM.Core/Entities/KnowledgeBase/KnowledgeArticle.cs
ARTICLE_TYPE_FAQ = 1
ARTICLE_TYPE_HOWTO = 2
ARTICLE_TYPE_TROUBLESHOOTING = 3
ARTICLE_TYPE_REFERENCE = 4
ARTICLE_TYPE_GUIDE = 5
ARTICLE_TYPE_POLICY = 6
ARTICLE_TYPE_TUTORIAL = 7

# ArticleVisibility
VISIBILITY_PUBLIC = 0
VISIBILITY_INTERNAL = 1
VISIBILITY_RESTRICTED = 2

# ArticleStatus (General KB workflow)
STATUS_DRAFT = 0
STATUS_IN_REVIEW = 1
STATUS_PUBLISHED = 2
STATUS_ARCHIVED = 3

# ── ITSM KB fallback: if General KB not available, load into ITSM KB ──
# ITSM ArticleType for cross-loading
ITSM_HOWTO = 1
ITSM_FAQ = 3
ITSM_REFERENCE = 5
ITSM_BEST_PRACTICE = 6


def _sales_articles(ts: int) -> list:
    """Sales knowledge articles — playbooks, competitive intel, methodology."""
    return [
        {
            "title": f"Sales Playbook: Enterprise Account Acquisition Strategy {ts}",
            "shortDescription": "Comprehensive playbook for pursuing and winning enterprise deals ($500K+).",
            "articleBody": (
                "## Target Profile\n"
                "- Company size: 1,000+ employees\n"
                "- Revenue: $100M+ annual\n"
                "- Industry: Technology, Financial Services, Healthcare, Manufacturing\n"
                "- Decision cycle: 6-18 months\n\n"
                "## Phase 1: Research & Qualification (Weeks 1-4)\n\n"
                "### Account Research Checklist\n"
                "- [ ] Annual report / 10-K filing review\n"
                "- [ ] LinkedIn research on key stakeholders (C-suite, VPs, Directors)\n"
                "- [ ] Technology stack (check BuiltWith, Wappalyzer, job postings)\n"
                "- [ ] Recent news, press releases, M&A activity\n"
                "- [ ] Current CRM/ERP vendor (competitor analysis)\n"
                "- [ ] Pain points from earnings calls or industry reports\n\n"
                "### Qualification Criteria (MEDDPICC)\n"
                "| Criteria | Question |\n"
                "|----------|----------|\n"
                "| Metrics | What business outcomes are they trying to achieve? |\n"
                "| Economic Buyer | Who controls the budget? |\n"
                "| Decision Criteria | What are their evaluation criteria? |\n"
                "| Decision Process | What is their buying process and timeline? |\n"
                "| Paper Process | What is their procurement / legal process? |\n"
                "| Identify Pain | What specific pain points does our solution address? |\n"
                "| Champion | Who is our internal advocate? |\n"
                "| Competition | Who else are they evaluating? |\n\n"
                "## Phase 2: Multi-Threading (Weeks 4-8)\n"
                "- Engage 5-7 stakeholders across different departments\n"
                "- Map the org chart and identify influencers\n"
                "- Tailor messaging for each persona (see Persona Cards below)\n"
                "- Host executive briefing or industry roundtable\n\n"
                "## Phase 3: Solution Design (Weeks 8-12)\n"
                "- Conduct discovery workshops with the prospect team\n"
                "- Build custom demo environment mirroring their workflow\n"
                "- Create ROI business case with their data\n"
                "- Propose phased implementation plan\n\n"
                "## Phase 4: Negotiation & Close (Weeks 12-18)\n"
                "- Present proposal with 3 pricing tiers\n"
                "- Negotiate terms with procurement\n"
                "- Address legal / security review (provide SOC 2, GDPR docs)\n"
                "- Secure executive sponsor commitment\n"
                "- Close and handoff to Customer Success\n\n"
                "## Persona Cards\n"
                "### CIO / CTO\n"
                "- Cares about: Integration, scalability, security, total cost of ownership\n"
                "- Language: 'Digital transformation', 'cloud-first', 'API ecosystem'\n\n"
                "### VP Sales\n"
                "- Cares about: Pipeline visibility, forecast accuracy, rep productivity\n"
                "- Language: 'Win rate', 'sales velocity', 'quota attainment'\n\n"
                "### CFO\n"
                "- Cares about: ROI, payback period, subscription vs perpetual\n"
                "- Language: 'TCO reduction', 'revenue uplift', 'cost per seat'\n"
            ),
            "articleType": ITSM_BEST_PRACTICE,
            "isInternal": True,
            "_category": "Sales",
        },
        {
            "title": f"Sales Guide: Objection Handling — Top 20 Common Objections and Responses {ts}",
            "shortDescription": "Battle-tested responses to the most common sales objections across all deal stages.",
            "articleBody": (
                "## Pricing Objections\n\n"
                "### 1. 'Your price is too high'\n"
                "**Response:** 'I understand budget is important. Let me share how our customers "
                "typically see a 3-5x ROI within the first year. Can we walk through the value "
                "calculator with your specific numbers?'\n\n"
                "### 2. 'Competitor X is cheaper'\n"
                "**Response:** 'That's a fair comparison. When our customers evaluated [Competitor], "
                "they found that implementation costs and ongoing maintenance made the total cost "
                "higher. Can I show you a TCO comparison from a similar deployment?'\n\n"
                "### 3. 'We don't have budget this quarter'\n"
                "**Response:** 'I understand. Many of our customers started with a pilot in the current "
                "quarter at a reduced scope, then expanded after proving ROI. Would a focused pilot "
                "approach work for your team?'\n\n"
                "## Timing Objections\n\n"
                "### 4. 'We're not ready to make a decision yet'\n"
                "**Response:** 'Absolutely, we want you to be confident. What additional information "
                "would help you move forward? Would connecting you with a reference customer in "
                "your industry be helpful?'\n\n"
                "### 5. 'Call me back in 6 months'\n"
                "**Response:** 'Of course. So I can make that call valuable — what would need to "
                "change in 6 months for this to become a priority? Is there a trigger event?'\n\n"
                "### 6. 'We just implemented a new system'\n"
                "**Response:** 'I hear that often. Our solution integrates alongside existing systems. "
                "In fact, [Customer X] added us alongside [System Y] and saw immediate value in "
                "[specific area]. Could we explore a complementary approach?'\n\n"
                "## Trust / Authority Objections\n\n"
                "### 7. 'I need to check with my team/boss'\n"
                "**Response:** 'That makes total sense for a decision this important. Would it be "
                "helpful if I joined that conversation to answer technical questions directly?'\n\n"
                "### 8. 'We had a bad experience with a similar product'\n"
                "**Response:** 'I'm sorry to hear that. Can you share what went wrong? We've invested "
                "heavily in [the specific area they had issues with]. Let me show you how we "
                "approach it differently.'\n\n"
                "### 9. 'Can you guarantee results?'\n"
                "**Response:** 'While I can't guarantee specific numbers, I can share that our "
                "average customer sees [X% improvement] in [metric]. We also offer a pilot program "
                "so you can validate results before full commitment.'\n\n"
                "## Technical Objections\n\n"
                "### 10. 'Does it integrate with [our stack]?'\n"
                "**Response:** 'Yes, we have native integrations with [relevant platforms] and a "
                "REST API for custom connections. Let me connect you with our solutions engineer "
                "for a technical deep-dive.'\n\n"
                "### 11. 'What about data security / compliance?'\n"
                "**Response:** 'Security is our top priority. We're SOC 2 Type II certified, GDPR "
                "compliant, and offer SSO/SAML integration. I can share our security whitepaper "
                "and connect you with our security team.'\n\n"
                "### 12. 'We'll just build it ourselves'\n"
                "**Response:** 'Some teams do go that route. From experience, the build cost is "
                "typically 5-10x the subscription cost once you factor in ongoing maintenance, "
                "updates, and engineering opportunity cost. Would a build vs. buy analysis "
                "be helpful?'\n\n"
                "## Status Quo Objections\n\n"
                "### 13. 'We're happy with our current solution'\n"
                "**Response:** 'That's great — means you have good processes in place. Curious "
                "though, if you could improve one thing about your current setup, what would it be?'\n\n"
                "### 14. 'Switching costs are too high'\n"
                "**Response:** 'I understand that concern. We offer free data migration and a "
                "dedicated onboarding team. Our average migration takes [X weeks] with zero "
                "downtime. Can I walk you through the transition plan?'\n\n"
                "### 15. 'This isn't a priority right now'\n"
                "**Response:** 'What is your top priority? Often our solution helps accelerate "
                "those priorities — for example, [Customer Y] was focused on [priority] and "
                "used our platform to achieve it 40% faster.'\n\n"
                "## Key Principles\n"
                "1. **Listen first** — Understand the real concern behind the objection\n"
                "2. **Acknowledge** — Show empathy, never dismiss their concern\n"
                "3. **Respond with value** — Connect back to their specific pain points\n"
                "4. **Provide proof** — Use customer stories, data, and case studies\n"
                "5. **Advance** — Always end with a next step\n"
            ),
            "articleType": ITSM_REFERENCE,
            "isInternal": True,
            "_category": "Sales",
        },
        {
            "title": f"Sales Reference: Competitive Analysis — CRM Market Landscape 2026 {ts}",
            "shortDescription": "Competitive positioning against major CRM vendors: Salesforce, HubSpot, Microsoft Dynamics, Zoho.",
            "articleBody": (
                "## Market Overview\n"
                "The global CRM market is projected at $96B in 2026 (Gartner).\n"
                "Key trends: AI-first experiences, vertical solutions, composable architecture.\n\n"
                "## Competitive Positioning Matrix\n"
                "| Feature | Our CRM | Salesforce | HubSpot | Dynamics 365 | Zoho |\n"
                "|---------|---------|------------|---------|--------------|------|\n"
                "| AI/LLM Integration | Multi-provider | Einstein | Breeze | Copilot | Zia |\n"
                "| Deployment | Self-hosted + Cloud | Cloud only | Cloud only | Cloud + On-prem | Cloud only |\n"
                "| Open Source | Yes (Core) | No | No | No | Partial |\n"
                "| Pricing (per user/mo) | Custom | $25-$500 | $20-$150 | $65-$210 | $14-$65 |\n"
                "| ITSM Built-in | Yes | No (addon) | No | Partial | No |\n"
                "| Customization | Full code access | Apex/LWC | Limited | Power Platform | Limited |\n"
                "| Data Sovereignty | Full control | Limited | Limited | Azure regions | Limited |\n\n"
                "## Win/Loss Themes (Last 12 Months)\n\n"
                "### We Win When:\n"
                "- Customer needs on-premise / data sovereignty\n"
                "- Customer wants integrated ITSM + CRM\n"
                "- Customer values open source and avoids vendor lock-in\n"
                "- Customer has strong internal dev team (customization)\n"
                "- Customer is cost-sensitive at scale (>500 users)\n\n"
                "### We Lose When:\n"
                "- Customer wants extensive ecosystem (AppExchange equivalent)\n"
                "- Customer is deeply embedded in Microsoft ecosystem\n"
                "- Customer needs pre-built industry-specific solutions\n"
                "- Customer lacks internal IT for self-hosted deployment\n\n"
                "## Battle Cards (Key Differentiators)\n\n"
                "### vs. Salesforce\n"
                "- **Our advantage**: Data sovereignty, no per-user licensing at scale, multi-LLM support\n"
                "- **Their advantage**: Ecosystem, AppExchange, industry clouds\n"
                "- **Trap question**: 'How does your AI compare to Einstein GPT?'\n"
                "- **Counter**: 'We support multiple AI providers including GPT-4, Claude, and local "
                "models — you're never locked into one vendor's AI.'\n\n"
                "### vs. HubSpot\n"
                "- **Our advantage**: Enterprise-grade ITSM, self-hosted option, deeper customization\n"
                "- **Their advantage**: Marketing automation, ease of use, free tier\n"
                "- **Trap question**: 'HubSpot is free to start'\n"
                "- **Counter**: 'HubSpot's enterprise features ($150/user/mo) cost more than our "
                "full platform, and you can't self-host.'\n"
            ),
            "articleType": ITSM_REFERENCE,
            "isInternal": True,
            "_category": "Sales",
        },
        {
            "title": f"Sales Guide: Discovery Call Framework — SPIN Methodology {ts}",
            "shortDescription": "Structured discovery call framework using SPIN selling: Situation, Problem, Implication, Need-Payoff.",
            "articleBody": (
                "## Pre-Call Preparation (15 Minutes)\n"
                "- Review LinkedIn profiles of attendees\n"
                "- Check company news (recent funding, M&A, leadership changes)\n"
                "- Review any existing interactions in CRM\n"
                "- Prepare 3 industry-specific insights to share\n\n"
                "## Call Structure (45 Minutes)\n\n"
                "### Opening (5 min)\n"
                "- Thank them for their time\n"
                "- Confirm attendees and their roles\n"
                "- Set agenda: 'I'd love to understand your current challenges before "
                "showing you anything. Is that okay?'\n\n"
                "### Situation Questions (10 min)\n"
                "Understand their current state:\n"
                "- 'What CRM/tools are you using today?'\n"
                "- 'How many users/reps do you have?'\n"
                "- 'Walk me through your typical sales process from lead to close.'\n"
                "- 'How do you currently track customer interactions?'\n"
                "- 'What does your technology stack look like?'\n\n"
                "### Problem Questions (10 min)\n"
                "Uncover pain points:\n"
                "- 'What's the biggest challenge your sales team faces today?'\n"
                "- 'Where do deals stall in your pipeline?'\n"
                "- 'How confident are you in your revenue forecasts?'\n"
                "- 'What happens when a rep leaves — do you lose customer history?'\n"
                "- 'How long does it take to onboard a new sales rep?'\n\n"
                "### Implication Questions (10 min)\n"
                "Quantify the impact of problems:\n"
                "- 'If deals are stalling at [stage], what's the revenue impact?'\n"
                "- 'How much time do reps spend on data entry vs. selling?'\n"
                "- 'What's the cost of inaccurate forecasting to your planning?'\n"
                "- 'If onboarding takes 6 months, how much quota capacity is lost?'\n\n"
                "### Need-Payoff Questions (5 min)\n"
                "Let them articulate the value:\n"
                "- 'If you could automate [painful process], what would that mean for the team?'\n"
                "- 'If forecast accuracy improved by 30%, how would that change your planning?'\n"
                "- 'What would it be worth if rep productivity increased by 20%?'\n\n"
                "### Next Steps (5 min)\n"
                "- Summarize what you heard (mirror their language)\n"
                "- Propose logical next step (demo, technical session, executive meeting)\n"
                "- Confirm stakeholders for next meeting\n"
                "- Set date before ending the call\n\n"
                "## Post-Call Actions\n"
                "1. Send summary email within 2 hours\n"
                "2. Update CRM with notes, pain points, next steps\n"
                "3. Update opportunity stage and close date\n"
                "4. Schedule internal strategy session if deal >$100K\n"
            ),
            "articleType": ITSM_HOWTO,
            "isInternal": True,
            "_category": "Sales",
        },
        {
            "title": f"Sales Reference: Pricing & Packaging Guide — How to Quote Correctly {ts}",
            "shortDescription": "Internal guide for sales reps on pricing tiers, discount authority, and quote approval workflows.",
            "articleBody": (
                "## Pricing Tiers (Effective Q1 2026)\n"
                "| Tier | Users | Monthly/User | Annual Discount | Includes |\n"
                "|------|-------|-------------|-----------------|----------|\n"
                "| Starter | 1-25 | $49 | 15% | Core CRM, 5GB storage |\n"
                "| Professional | 26-100 | $79 | 20% | + ITSM, AI, 25GB storage |\n"
                "| Enterprise | 101-500 | $119 | 25% | + API, integrations, 100GB |\n"
                "| Unlimited | 500+ | Custom | Negotiable | Full platform, unlimited storage |\n\n"
                "## Add-On Modules\n"
                "| Module | Price/User/Mo | Description |\n"
                "|--------|---------------|-------------|\n"
                "| Advanced Analytics | $20 | Superset dashboards, custom reports |\n"
                "| AI Premium | $30 | Multi-provider LLM, vector search |\n"
                "| ITSM Advanced | $25 | CMDB, change management, SLA |\n"
                "| Integration Hub | $15 | N8n workflows, 400+ connectors |\n"
                "| E-Signatures | $10 | DocuSeal integration |\n\n"
                "## Discount Authority Matrix\n"
                "| Discount | Approval Required |\n"
                "|----------|-------------------|\n"
                "| 0-10% | Sales Rep (self-approve) |\n"
                "| 11-20% | Sales Manager |\n"
                "| 21-30% | VP Sales |\n"
                "| 31-40% | CRO |\n"
                "| 40%+ | CEO (strategic accounts only) |\n\n"
                "## Multi-Year Discount\n"
                "| Term | Additional Discount |\n"
                "|------|---------------------|\n"
                "| 1 year | Standard pricing |\n"
                "| 2 years | +5% |\n"
                "| 3 years | +10% |\n\n"
                "## Quote Approval Process\n"
                "1. Create quote in CRM (Sales → Quotes → New)\n"
                "2. Select products and pricing tier\n"
                "3. Apply discounts (system enforces approval matrix)\n"
                "4. Submit for approval if discount exceeds your authority\n"
                "5. Approved quote generates PDF for customer\n"
                "6. Track quote status: Draft → Under Approval → Sent → Accepted/Rejected\n\n"
                "## Non-Standard Terms\n"
                "The following require Legal review (add 5 business days):\n"
                "- Payment terms beyond Net 30\n"
                "- Custom SLA commitments\n"
                "- Data processing addendum modifications\n"
                "- Indemnification clause changes\n"
                "- Source code escrow requests\n"
            ),
            "articleType": ITSM_REFERENCE,
            "isInternal": True,
            "_category": "Sales",
        },
        {
            "title": f"Sales Playbook: Upsell and Cross-Sell Strategies for Existing Accounts {ts}",
            "shortDescription": "Strategies and triggers for expanding revenue within the existing customer base.",
            "articleBody": (
                "## Expansion Revenue Framework\n"
                "Existing accounts are 5x more likely to buy than new prospects.\n"
                "Target: 30% of annual revenue from expansion.\n\n"
                "## Trigger Events (Monitor in CRM)\n"
                "| Trigger | Signal | Action |\n"
                "|---------|--------|---------|\n"
                "| User count approaching tier limit | Usage dashboard | Propose tier upgrade |\n"
                "| New department onboarded | Admin adding new user groups | Cross-sell modules |\n"
                "| High support ticket volume | Service metrics | Offer ITSM module |\n"
                "| Request for API access | Feature request ticket | Upsell Integration Hub |\n"
                "| CEO / CTO change | LinkedIn alerts | Executive briefing |\n"
                "| Fiscal year start | Calendar-based | Annual review meeting |\n"
                "| Merger / acquisition | News monitoring | Expansion proposal |\n\n"
                "## Upsell Plays\n\n"
                "### Play 1: Tier Upgrade\n"
                "When customer approaches user limit or needs features in next tier:\n"
                "- Schedule QBR (Quarterly Business Review)\n"
                "- Present usage analytics showing ROI\n"
                "- Show features they're missing in current tier\n"
                "- Offer seamless upgrade with pro-rated billing\n\n"
                "### Play 2: Module Add-On\n"
                "When customer's usage suggests need for additional modules:\n"
                "- AI Premium: Customers using built-in AI features heavily\n"
                "- ITSM Advanced: Customers who also have IT teams\n"
                "- Analytics: Customers asking for custom reports\n\n"
                "### Play 3: Multi-Year Lock-In\n"
                "At contract renewal:\n"
                "- Offer 2-year or 3-year term with additional discount\n"
                "- Bundle module add-ons into the renewal\n"
                "- Include professional services / training credits\n\n"
                "## Quarterly Business Review Template\n"
                "1. Review KPIs and usage metrics\n"
                "2. Celebrate wins (improved metrics since adoption)\n"
                "3. Roadmap preview (create excitement for upcoming features)\n"
                "4. Identify expansion opportunities\n"
                "5. Agree on action items and next meeting\n"
            ),
            "articleType": ITSM_BEST_PRACTICE,
            "isInternal": True,
            "_category": "Sales",
        },
    ]


def _service_articles(ts: int) -> list:
    """Service / Customer Success knowledge articles."""
    return [
        {
            "title": f"Customer Onboarding Guide: 90-Day Success Plan {ts}",
            "shortDescription": "Structured onboarding program to ensure customer adoption and time-to-value within 90 days.",
            "articleBody": (
                "## Day 0-7: Welcome & Setup\n"
                "- [ ] Send welcome email with dedicated CSM contact info\n"
                "- [ ] Schedule kickoff call (CSM + customer stakeholders)\n"
                "- [ ] Provision environment (tenant creation, admin accounts)\n"
                "- [ ] Share onboarding guide and video library\n"
                "- [ ] Identify customer success criteria and KPIs\n\n"
                "## Day 7-14: Technical Setup\n"
                "- [ ] Data migration workshop (review data export from old system)\n"
                "- [ ] Single Sign-On (SSO/SAML) configuration\n"
                "- [ ] Integration setup (email, calendar, phone system)\n"
                "- [ ] Import historical data (accounts, contacts, opportunities)\n"
                "- [ ] Configure roles, permissions, and team structure\n\n"
                "## Day 14-30: Core Training\n"
                "- [ ] Admin training (2 hours): Settings, users, customization\n"
                "- [ ] End-user training (3 sessions x 1 hour): Daily workflows\n"
                "- [ ] Manager training (1 hour): Reports, dashboards, pipeline\n"
                "- [ ] Create custom fields and validation rules per customer needs\n"
                "- [ ] Set up automation rules (lead assignment, notifications)\n\n"
                "## Day 30-60: Adoption & Optimization\n"
                "- [ ] Weekly check-in calls (30 min)\n"
                "- [ ] Monitor adoption metrics (login frequency, data entry rates)\n"
                "- [ ] Address user feedback and configuration refinements\n"
                "- [ ] Set up advanced features (workflows, email sequences)\n"
                "- [ ] Create customer-specific dashboards\n\n"
                "## Day 60-90: Expansion & Handoff\n"
                "- [ ] Conduct 90-day success review\n"
                "- [ ] Document achieved KPIs vs. success criteria\n"
                "- [ ] Identify expansion opportunities (additional modules/users)\n"
                "- [ ] Transition to ongoing support model\n"
                "- [ ] NPS survey and case study request (if successful)\n\n"
                "## Success Metrics Dashboard\n"
                "| Metric | Week 2 Target | Week 4 | Week 8 | Week 12 |\n"
                "|--------|---------------|--------|--------|----------|\n"
                "| Daily active users | 50% | 70% | 85% | 90% |\n"
                "| Data completeness | 30% | 60% | 80% | 90% |\n"
                "| Key workflows adopted | 2 | 5 | 8 | All |\n"
                "| Support tickets | Any | Decreasing | Low | Minimal |\n"
            ),
            "articleType": ITSM_HOWTO,
            "isInternal": True,
            "_category": "Service",
        },
        {
            "title": f"Service Guide: Customer Health Score — Calculation and Response Playbook {ts}",
            "shortDescription": "How the customer health score is calculated and what actions to take for each score band.",
            "articleBody": (
                "## Health Score Components\n"
                "| Component | Weight | Measurement |\n"
                "|-----------|--------|-------------|\n"
                "| Product Usage | 30% | DAU/MAU ratio, feature adoption, API calls |\n"
                "| Support Health | 20% | Ticket volume, CSAT, escalation rate |\n"
                "| Engagement | 20% | Logins, training completion, QBR attendance |\n"
                "| Financial | 15% | Payment timeliness, contract value trend |\n"
                "| Relationship | 15% | NPS score, executive sponsor engagement |\n\n"
                "## Score Bands\n"
                "| Score | Band | Color | Meaning |\n"
                "|-------|------|-------|---------|\n"
                "| 80-100 | Healthy | Green | Expanding, engaged, champion |\n"
                "| 60-79 | Neutral | Yellow | Stable, moderate risk |\n"
                "| 40-59 | At Risk | Orange | Declining engagement, action needed |\n"
                "| 0-39 | Critical | Red | High churn risk, immediate intervention |\n\n"
                "## Response Playbook\n\n"
                "### Green (80-100): Nurture & Expand\n"
                "- Monthly check-ins (light touch)\n"
                "- Share product roadmap preview\n"
                "- Request case study / testimonial\n"
                "- Identify expansion opportunities\n"
                "- Invite to customer advisory board\n\n"
                "### Yellow (60-79): Monitor & Engage\n"
                "- Bi-weekly check-ins\n"
                "- Review usage patterns — identify unused features\n"
                "- Schedule re-training for low-adoption areas\n"
                "- Address any open support issues\n"
                "- Confirm renewal timeline and stakeholders\n\n"
                "### Orange (40-59): Intervene\n"
                "- Weekly check-ins with CSM\n"
                "- Executive sponsor outreach (CSM Manager → VP)\n"
                "- Create 30-day recovery plan\n"
                "- Offer complimentary professional services\n"
                "- Investigate root cause of decline\n"
                "- Escalate internally (CS → Account Executive → Management)\n\n"
                "### Red (0-39): Save\n"
                "- Immediate executive outreach (VP CS → Customer VP/C-level)\n"
                "- Dedicated save team assigned\n"
                "- Full account review within 48 hours\n"
                "- Custom retention offer (discount, free months, services)\n"
                "- If churning: conduct exit interview and document reasons\n"
            ),
            "articleType": ITSM_REFERENCE,
            "isInternal": True,
            "_category": "Service",
        },
        {
            "title": f"Service FAQ: How to Handle Customer Escalation Requests {ts}",
            "shortDescription": "Process for handling customer escalations including communication templates and SLA expectations.",
            "articleBody": (
                "## When Does a Customer Escalate?\n"
                "- Issue unresolved past SLA target\n"
                "- Customer explicitly requests escalation\n"
                "- Business-critical issue with significant impact\n"
                "- Repeated issues indicating systemic problem\n\n"
                "## Escalation Levels\n"
                "| Level | Who Responds | Response SLA | Authority |\n"
                "|-------|-------------|-------------|------------|\n"
                "| L1 | Support Agent | 4 hours | Troubleshoot, workaround |\n"
                "| L2 | Senior Support / CSM | 2 hours | Configuration changes, credits |\n"
                "| L3 | Support Manager | 1 hour | SLA exceptions, priority fix |\n"
                "| L4 | VP Customer Success | 30 minutes | Custom terms, executive commitment |\n"
                "| L5 | C-Suite | ASAP | Strategic relationship decisions |\n\n"
                "## Escalation Process\n"
                "1. **Acknowledge immediately** (within 15 minutes)\n"
                "2. **Document the full history** — gather all ticket details, prior communications\n"
                "3. **Identify the real issue** — often the escalation trigger is symptoms, not root cause\n"
                "4. **Create action plan** with specific steps and timeline\n"
                "5. **Communicate proactively** — update before customer asks\n"
                "6. **Resolve and follow up** — check back 48 hours after resolution\n\n"
                "## Acknowledgment Template\n"
                "```\n"
                "Subject: [Escalation Acknowledged] RE: {Original Subject}\n\n"
                "Hi {Customer Name},\n\n"
                "I want you to know your concern has been escalated to our {Level} team. "
                "My name is {Your Name}, and I will be your point of contact until this "
                "is fully resolved.\n\n"
                "Here's what we know so far:\n"
                "- Issue: {Brief description}\n"
                "- Impact: {Customer impact}\n"
                "- Current Status: {What's being done}\n\n"
                "I will provide an update by {specific time}. If you need to reach me "
                "directly, my contact details are below.\n\n"
                "Best regards,\n"
                "{Name} | {Title}\n"
                "```\n\n"
                "## Post-Escalation Review\n"
                "Within 5 business days of resolution:\n"
                "- Document root cause and resolution in KB\n"
                "- Update runbooks if process gap found\n"
                "- Conduct internal mini-RCA if P1/P2\n"
                "- Send satisfaction follow-up to customer\n"
            ),
            "articleType": ITSM_FAQ,
            "isInternal": False,
            "_category": "Service",
        },
        {
            "title": f"Service Guide: Customer Success Metrics — QBR Dashboard Template {ts}",
            "shortDescription": "Template and data sources for preparing Quarterly Business Review dashboards for customers.",
            "articleBody": (
                "## QBR Dashboard Sections\n\n"
                "### 1. Executive Summary (1 slide)\n"
                "- Overall health score and trend\n"
                "- Key wins this quarter\n"
                "- Areas for improvement\n"
                "- Recommendations\n\n"
                "### 2. Adoption & Usage (2 slides)\n"
                "| Metric | Q1 | Q2 | Q3 | Q4 | Trend |\n"
                "|--------|----|----|----|----|-------|\n"
                "| Monthly Active Users | | | | | |\n"
                "| Daily Active Users | | | | | |\n"
                "| Features Adopted | | | | | |\n"
                "| API Calls/Month | | | | | |\n"
                "| Mobile Usage % | | | | | |\n\n"
                "Data source: CRM Analytics → User Activity Report\n\n"
                "### 3. Support Health (1 slide)\n"
                "| Metric | Target | Actual |\n"
                "|--------|--------|--------|\n"
                "| Avg Response Time | <4h | |\n"
                "| Avg Resolution Time | <24h | |\n"
                "| CSAT Score | >4.5 | |\n"
                "| Open Tickets | <5 | |\n"
                "| Escalation Rate | <5% | |\n\n"
                "Data source: Service Desk → Customer Report\n\n"
                "### 4. Business Impact (1 slide)\n"
                "- Pipeline value managed in CRM\n"
                "- Deals closed using CRM this quarter\n"
                "- Time saved per rep per week (self-reported or estimated)\n"
                "- Forecast accuracy improvement\n\n"
                "### 5. Roadmap Preview (1 slide)\n"
                "- Upcoming features relevant to this customer\n"
                "- Beta program invitations\n"
                "- Training opportunities\n\n"
                "### 6. Action Items (1 slide)\n"
                "| Action | Owner | Due Date | Status |\n"
                "|--------|-------|----------|--------|\n"
                "| | | | |\n\n"
                "## QBR Cadence\n"
                "- Enterprise customers: Quarterly\n"
                "- Professional customers: Semi-annually\n"
                "- Starter customers: Annual (self-service health report)\n"
            ),
            "articleType": ITSM_REFERENCE,
            "isInternal": True,
            "_category": "Service",
        },
        {
            "title": f"Service Troubleshooting: Customer Reports Data Sync Issues {ts}",
            "shortDescription": "Troubleshooting guide when customers report data not syncing between CRM and external systems.",
            "articleBody": (
                "## Symptoms\n"
                "- Customer reports data entered in CRM doesn't appear in integrated system\n"
                "- Or vice versa: external system data not appearing in CRM\n"
                "- Intermittent sync failures\n\n"
                "## Tier 1: Quick Checks\n"
                "1. **Verify integration is active**: Admin → Integrations → Check status\n"
                "2. **Check sync logs**: Admin → Integrations → [Integration Name] → Sync Logs\n"
                "3. **Verify credentials**: Integration credentials may have expired\n"
                "4. **Check rate limits**: External API may be throttling requests\n\n"
                "## Tier 2: Detailed Investigation\n"
                "1. **Check webhook delivery**: Admin → Webhooks → Recent Activity\n"
                "2. **Verify field mappings**: Integration → Field Mapping → Check for unmapped fields\n"
                "3. **Check for validation errors**: Sync log may show validation failures\n"
                "4. **Test connectivity**: Try manual sync from Admin panel\n\n"
                "## Common Root Causes\n"
                "| Issue | Solution |\n"
                "|-------|----------|\n"
                "| Expired API token | Regenerate token in external system, update in CRM |\n"
                "| Rate limit exceeded | Reduce sync frequency or batch requests |\n"
                "| Field validation mismatch | Update field mappings to match new schema |\n"
                "| Network timeout | Increase timeout setting, add retry logic |\n"
                "| Webhook URL changed | Update webhook endpoint in CRM settings |\n"
                "| Duplicate detection | Check for duplicate merge rules conflicting |\n\n"
                "## Escalation Criteria\n"
                "- Sync has been down >4 hours for critical integration\n"
                "- Data loss suspected (records missing)\n"
                "- Customer business process blocked\n"
                "- Issue is in our API (not external system)\n\n"
                "## Internal Notes\n"
                "For n8n integration issues, check n8n workflow execution logs at:\n"
                "`http://crm-n8n:5678/workflows` (admin access required).\n"
            ),
            "articleType": ITSM_HOWTO,
            "isInternal": True,
            "_category": "Service",
        },
    ]


def _marketing_articles(ts: int) -> list:
    """Marketing knowledge articles — campaigns, brand, content, SEO."""
    return [
        {
            "title": f"Marketing Guide: Campaign Planning and Execution Checklist {ts}",
            "shortDescription": "End-to-end checklist for planning, executing, and measuring marketing campaigns.",
            "articleBody": (
                "## Phase 1: Planning (2-4 Weeks Before Launch)\n\n"
                "### Strategy\n"
                "- [ ] Define campaign objective (awareness, leads, conversion, retention)\n"
                "- [ ] Identify target audience and segments\n"
                "- [ ] Set SMART goals (Specific, Measurable, Achievable, Relevant, Time-bound)\n"
                "- [ ] Choose channels (email, social, paid, content, events)\n"
                "- [ ] Define budget and allocate across channels\n"
                "- [ ] Create campaign brief and get stakeholder sign-off\n\n"
                "### Content\n"
                "- [ ] Develop key messaging and value propositions\n"
                "- [ ] Create content calendar\n"
                "- [ ] Write email copy (subject lines, body, CTAs)\n"
                "- [ ] Design creatives (banners, social images, landing pages)\n"
                "- [ ] Create landing pages with tracking pixels\n"
                "- [ ] Set up lead capture forms\n\n"
                "### Technical Setup\n"
                "- [ ] Create campaign in CRM (Marketing → Campaigns → New)\n"
                "- [ ] Build email templates in CRM (Marketing → Templates)\n"
                "- [ ] Set up tracking UTM parameters for all links\n"
                "- [ ] Configure lead scoring rules for the campaign\n"
                "- [ ] Test email deliverability (send to seed list)\n"
                "- [ ] Set up A/B tests if applicable\n\n"
                "## Phase 2: Execution\n"
                "- [ ] Final review of all content and links\n"
                "- [ ] Send test emails to internal stakeholders\n"
                "- [ ] Launch campaign on scheduled date\n"
                "- [ ] Monitor delivery and engagement in real-time (first 4 hours)\n"
                "- [ ] Post social media content per schedule\n"
                "- [ ] Activate paid media campaigns\n\n"
                "## Phase 3: Monitoring (During Campaign)\n"
                "- [ ] Daily check: email open rates, click rates, bounce rates\n"
                "- [ ] Weekly check: leads generated, cost per lead, conversion rate\n"
                "- [ ] Adjust targeting if underperforming\n"
                "- [ ] Respond to social engagement\n"
                "- [ ] Follow up on hot leads within 24 hours\n\n"
                "## Phase 4: Reporting (1 Week After End)\n"
                "| Metric | Target | Actual | Status |\n"
                "|--------|--------|--------|--------|\n"
                "| Leads Generated | | | |\n"
                "| Cost Per Lead | | | |\n"
                "| Email Open Rate | 25% | | |\n"
                "| Click-Through Rate | 3% | | |\n"
                "| Conversion Rate | 5% | | |\n"
                "| Pipeline Generated | | | |\n"
                "| ROI | 3x | | |\n\n"
                "Create campaign report in CRM → Reports → Campaign Analysis.\n"
            ),
            "articleType": ITSM_HOWTO,
            "isInternal": True,
            "_category": "Marketing",
        },
        {
            "title": f"Marketing Reference: Brand Guidelines and Style Guide {ts}",
            "shortDescription": "Official brand guidelines including logo usage, color palette, typography, and voice and tone.",
            "articleBody": (
                "## Brand Identity\n\n"
                "### Logo Usage\n"
                "- Primary logo: Full color on white/light backgrounds\n"
                "- Reversed logo: White on dark backgrounds\n"
                "- Minimum size: 120px wide (digital), 1 inch (print)\n"
                "- Clear space: Minimum 50% of logo height on all sides\n"
                "- **Never**: Stretch, rotate, recolor, or add effects to the logo\n\n"
                "### Color Palette\n"
                "| Color | Hex | Use |\n"
                "|-------|-----|-----|\n"
                "| Primary Blue | #1976D2 | Headers, buttons, links |\n"
                "| Secondary Navy | #0D47A1 | Navigation, footer |\n"
                "| Accent Orange | #FF6F00 | CTAs, highlights |\n"
                "| Success Green | #2E7D32 | Positive states, confirmations |\n"
                "| Error Red | #C62828 | Errors, warnings |\n"
                "| Neutral Gray | #616161 | Body text, secondary elements |\n"
                "| Background | #FAFAFA | Page backgrounds |\n\n"
                "### Typography\n"
                "- **Headings**: Inter, Bold (fallback: Helvetica Neue, sans-serif)\n"
                "- **Body**: Inter, Regular, 16px / 1.6 line height\n"
                "- **Code/Data**: JetBrains Mono (fallback: Consolas, monospace)\n"
                "- **Hierarchy**: H1 (32px), H2 (24px), H3 (20px), Body (16px), Small (14px)\n\n"
                "## Voice & Tone\n\n"
                "### Brand Voice (Always)\n"
                "- **Confident** — We know our product and market\n"
                "- **Clear** — No jargon, straightforward communication\n"
                "- **Helpful** — Always provide value, not just sell\n"
                "- **Approachable** — Professional but not stiff\n\n"
                "### Tone by Context\n"
                "| Context | Tone | Example |\n"
                "|---------|------|---------|\n"
                "| Marketing website | Inspiring, confident | 'Transform how your team sells' |\n"
                "| Product docs | Clear, instructive | 'To create a contact, click...' |\n"
                "| Error messages | Empathetic, helpful | 'Something went wrong. Try again.' |\n"
                "| Social media | Engaging, conversational | 'What's your top CRM challenge?' |\n"
                "| Support emails | Professional, empathetic | 'We understand this is frustrating' |\n\n"
                "## Asset Repository\n"
                "All approved brand assets are available at:\n"
                "SharePoint → Marketing → Brand Assets → Current Version\n"
                "Request custom assets via Marketing → Creative Request form.\n"
            ),
            "articleType": ITSM_REFERENCE,
            "isInternal": True,
            "_category": "Marketing",
        },
        {
            "title": f"Marketing Guide: Email Marketing Best Practices and Deliverability {ts}",
            "shortDescription": "Guidelines for maximizing email marketing effectiveness while maintaining high deliverability rates.",
            "articleBody": (
                "## Email Deliverability Checklist\n\n"
                "### Technical Setup\n"
                "- [ ] SPF record configured for sending domain\n"
                "- [ ] DKIM signing enabled\n"
                "- [ ] DMARC policy set (start with p=none, progress to p=reject)\n"
                "- [ ] Dedicated IP for sending (if volume > 50K/month)\n"
                "- [ ] IP warm-up completed (2-4 weeks for new IPs)\n"
                "- [ ] Custom tracking domain (e.g., click.ourcrm.com)\n\n"
                "### List Hygiene\n"
                "- Clean list monthly: remove bounces, unsubscribes, inactive\n"
                "- Never buy email lists — always opt-in only\n"
                "- Implement double opt-in for new subscribers\n"
                "- Re-engagement campaign for inactive subscribers (90+ days)\n"
                "- Sunset policy: remove after 2 failed re-engagement attempts\n\n"
                "## Content Best Practices\n\n"
                "### Subject Lines\n"
                "- Keep under 50 characters (mobile-friendly)\n"
                "- Use personalization: {FirstName}, {Company}\n"
                "- Create urgency without being spammy\n"
                "- A/B test subject lines (minimum 1000 per variant)\n"
                "- Avoid: ALL CAPS, excessive punctuation!!!, spam trigger words\n\n"
                "### Email Body\n"
                "- Single clear CTA (Call To Action)\n"
                "- Mobile-responsive design (60%+ opens are mobile)\n"
                "- Text-to-image ratio: 60/40 minimum\n"
                "- Alt text on all images\n"
                "- Preheader text (40-130 characters)\n"
                "- Unsubscribe link clearly visible (legal requirement)\n\n"
                "## Metrics Benchmarks (B2B SaaS Industry)\n"
                "| Metric | Good | Great | Investigate |\n"
                "|--------|------|-------|-------------|\n"
                "| Open Rate | 20-25% | 30%+ | <15% |\n"
                "| Click Rate | 2-3% | 5%+ | <1% |\n"
                "| Bounce Rate | <2% | <0.5% | >5% |\n"
                "| Unsubscribe | <0.3% | <0.1% | >0.5% |\n"
                "| Spam Complaint | <0.01% | 0% | >0.05% |\n\n"
                "## Sending Schedule\n"
                "- Best days: Tuesday-Thursday\n"
                "- Best times: 10am or 2pm recipient's timezone\n"
                "- Maximum frequency: 2 marketing emails per week\n"
                "- Minimum gap between emails: 3 days\n"
                "- Always check against transactional email volume\n"
            ),
            "articleType": ITSM_BEST_PRACTICE,
            "isInternal": True,
            "_category": "Marketing",
        },
        {
            "title": f"Marketing Reference: SEO Strategy and Content Optimization Guide {ts}",
            "shortDescription": "SEO best practices for the CRM website, blog, and product documentation.",
            "articleBody": (
                "## SEO Fundamentals\n\n"
                "### Keyword Strategy\n"
                "Target keywords by intent:\n"
                "| Intent | Example Keywords | Content Type |\n"
                "|--------|------------------|--------------|\n"
                "| Informational | 'what is CRM', 'CRM benefits' | Blog, guides |\n"
                "| Comparison | 'CRM vs ERP', 'best CRM 2026' | Comparison pages |\n"
                "| Commercial | 'CRM pricing', 'CRM demo' | Product pages |\n"
                "| Transactional | 'buy CRM software', 'CRM free trial' | Landing pages |\n\n"
                "### On-Page Optimization\n"
                "For every page:\n"
                "- [ ] Unique title tag (55-60 chars) with primary keyword\n"
                "- [ ] Meta description (150-160 chars) with CTA\n"
                "- [ ] H1 tag (one per page) containing primary keyword\n"
                "- [ ] H2-H3 tags with related keywords\n"
                "- [ ] Internal links to 2-3 related pages\n"
                "- [ ] Image alt tags with descriptive text\n"
                "- [ ] URL slug under 5 words, hyphen-separated\n"
                "- [ ] Schema markup (FAQ, HowTo, Product as appropriate)\n\n"
                "### Technical SEO\n"
                "- Core Web Vitals: LCP <2.5s, FID <100ms, CLS <0.1\n"
                "- Mobile-first indexing: responsive design mandatory\n"
                "- sitemap.xml updated for every content publish\n"
                "- robots.txt: block staging, admin, and API paths\n"
                "- Canonical tags on all pages\n"
                "- HTTPS enforced (301 redirect from HTTP)\n\n"
                "## Content Calendar\n"
                "| Frequency | Content Type | Length | Target |\n"
                "|-----------|-------------|--------|--------|\n"
                "| 3x/week | Blog posts | 1500-2500 words | Organic traffic |\n"
                "| Monthly | Pillar content | 4000+ words | Authority |\n"
                "| Monthly | Case study | 1000-1500 words | Social proof |\n"
                "| Quarterly | Whitepaper/eBook | 5000+ words | Lead generation |\n"
                "| Quarterly | Webinar | 45-60 minutes | Engagement |\n\n"
                "## Competitive Keywords to Target\n"
                "- 'open source CRM' (Vol: 12K, KD: 45)\n"
                "- 'self-hosted CRM' (Vol: 3.2K, KD: 32)\n"
                "- 'CRM with ITSM' (Vol: 1.8K, KD: 28)\n"
                "- 'Salesforce alternative' (Vol: 8.5K, KD: 52)\n"
                "- 'CRM for mid-market' (Vol: 2.1K, KD: 38)\n"
            ),
            "articleType": ITSM_REFERENCE,
            "isInternal": True,
            "_category": "Marketing",
        },
        {
            "title": f"Marketing Guide: Lead Scoring Model — Setup and Optimization {ts}",
            "shortDescription": "How to configure and optimize the CRM lead scoring model for marketing qualified leads (MQLs).",
            "articleBody": (
                "## Lead Scoring Model\n\n"
                "### Demographic Scoring (50 points max)\n"
                "| Attribute | Criteria | Points |\n"
                "|-----------|----------|--------|\n"
                "| Company Size | 500+ employees | +15 |\n"
                "| Company Size | 100-499 employees | +10 |\n"
                "| Company Size | 50-99 employees | +5 |\n"
                "| Job Title | C-Suite / VP | +15 |\n"
                "| Job Title | Director / Manager | +10 |\n"
                "| Job Title | Individual Contributor | +5 |\n"
                "| Industry | Target industry (Tech, Finance, Healthcare) | +10 |\n"
                "| Industry | Adjacent industry | +5 |\n"
                "| Geography | Tier 1 region (NA, UK, DACH) | +10 |\n"
                "| Geography | Tier 2 region | +5 |\n\n"
                "### Behavioral Scoring (50 points max)\n"
                "| Action | Points | Decay |\n"
                "|--------|--------|-------|\n"
                "| Visited pricing page | +10 | 30 days |\n"
                "| Downloaded whitepaper/eBook | +8 | 60 days |\n"
                "| Attended webinar | +10 | 30 days |\n"
                "| Requested demo | +15 | None |\n"
                "| Free trial signup | +15 | None |\n"
                "| Visited 5+ pages in session | +5 | 14 days |\n"
                "| Opened marketing email | +2 | 7 days |\n"
                "| Clicked email link | +5 | 14 days |\n"
                "| Returned visit (3+ times) | +5 | 30 days |\n"
                "| Blog subscriber | +3 | None |\n\n"
                "### Negative Scoring\n"
                "| Action | Points |\n"
                "|--------|---------|\n"
                "| Unsubscribed from email | -15 |\n"
                "| Competitor company | -50 |\n"
                "| Free email domain (gmail, yahoo) | -10 |\n"
                "| Student / academia | -20 |\n"
                "| No activity in 90 days | -15 |\n"
                "| Bounced email | -10 |\n\n"
                "## Scoring Thresholds\n"
                "| Score | Status | Action |\n"
                "|-------|--------|---------|\n"
                "| 0-30 | Cold | Nurture with educational content |\n"
                "| 31-59 | Warm | Targeted campaigns, sales awareness |\n"
                "| 60-79 | MQL | Auto-assign to SDR, respond within 4h |\n"
                "| 80+ | SQL | Direct handoff to Account Executive |\n\n"
                "## Configuration in CRM\n"
                "1. Navigate to Marketing → Lead Scoring → Rules\n"
                "2. Edit scoring rules per the table above\n"
                "3. Set MQL threshold to 60\n"
                "4. Configure auto-assignment rule for MQLs\n"
                "5. Enable Slack notification for new MQLs\n\n"
                "## Optimization\n"
                "- Review quarterly with sales team\n"
                "- Analyze converted vs. non-converted MQLs\n"
                "- Adjust weights based on conversion data\n"
                "- A/B test email scoring weights\n"
            ),
            "articleType": ITSM_HOWTO,
            "isInternal": True,
            "_category": "Marketing",
        },
        {
            "title": f"Marketing FAQ: Social Media Content Strategy and Calendar {ts}",
            "shortDescription": "Guidelines for social media content creation, scheduling, and engagement across platforms.",
            "articleBody": (
                "## Platform Strategy\n"
                "| Platform | Audience | Post Frequency | Content Type |\n"
                "|----------|----------|----------------|---------------|\n"
                "| LinkedIn | B2B decision makers | 5x/week | Thought leadership, case studies |\n"
                "| Twitter/X | Tech community, developers | Daily | Product updates, industry news |\n"
                "| YouTube | Prospects, customers | 2x/month | Demos, tutorials, webinars |\n"
                "| GitHub | Developers | As needed | Release notes, docs, community |\n"
                "| Blog | All audiences | 3x/week | SEO content, guides, news |\n\n"
                "## Content Mix (Rule of Thirds)\n"
                "- **1/3 Promotional**: Product features, case studies, announcements\n"
                "- **1/3 Educational**: Industry insights, how-to guides, tips\n"
                "- **1/3 Engaging**: Polls, questions, memes, team spotlights\n\n"
                "## LinkedIn Best Practices\n"
                "- Hook in first 2 lines (before 'See more')\n"
                "- Use 3-5 relevant hashtags\n"
                "- Tag relevant people and companies\n"
                "- Native video outperforms links (5x engagement)\n"
                "- Post between 8-10am or 5-6pm (prospect's timezone)\n"
                "- Carousel posts for educational content\n"
                "- Employee advocacy: share company posts from personal profiles\n\n"
                "## Content Ideas by Week\n"
                "| Day | Theme | Example |\n"
                "|-----|-------|---------|\n"
                "| Monday | Motivation / Industry Trend | 'This week in CRM: AI adoption hits 60%' |\n"
                "| Tuesday | Product Tip | '3 ways to automate lead assignment in our CRM' |\n"
                "| Wednesday | Customer Spotlight | 'How [Customer] increased sales 40%' |\n"
                "| Thursday | Thought Leadership | 'Why self-hosted CRM is the future' |\n"
                "| Friday | Team / Culture | 'Meet our engineering team' |\n\n"
                "## Engagement Rules\n"
                "- Respond to all comments within 4 hours\n"
                "- Like/acknowledge all positive mentions\n"
                "- Don't argue with negative comments — take to DM\n"
                "- Forward product feedback to Product team\n"
                "- Report security-related comments to Security team\n\n"
                "## Tools\n"
                "- Scheduling: Buffer or Hootsuite\n"
                "- Analytics: Platform native + CRM campaign tracking\n"
                "- Graphics: Canva (brand templates in shared folder)\n"
                "- Video: Loom for quick demos, Riverside for interviews\n"
            ),
            "articleType": ITSM_FAQ,
            "isInternal": True,
            "_category": "Marketing",
        },
        {
            "title": f"Marketing Guide: Product Launch Campaign Playbook {ts}",
            "shortDescription": "Complete playbook for planning and executing a product launch or major feature release campaign.",
            "articleBody": (
                "## Launch Timeline\n\n"
                "### T-6 Weeks: Planning\n"
                "- [ ] Product marketing brief from Product team\n"
                "- [ ] Define target audience and segments\n"
                "- [ ] Competitive positioning document\n"
                "- [ ] Key messaging framework (3 pillars + proof points)\n"
                "- [ ] Launch date confirmed with Engineering\n"
                "- [ ] Budget approved\n\n"
                "### T-4 Weeks: Content Creation\n"
                "- [ ] Blog post: 'Introducing [Feature]'\n"
                "- [ ] Product documentation / help articles\n"
                "- [ ] Landing page with demo video\n"
                "- [ ] Email announcement (3-email sequence)\n"
                "- [ ] Social media posts (LinkedIn, Twitter, YouTube)\n"
                "- [ ] Press release (if major launch)\n"
                "- [ ] Customer testimonial or beta feedback\n"
                "- [ ] Internal enablement: sales deck, battle cards, FAQ\n\n"
                "### T-2 Weeks: Pre-Launch\n"
                "- [ ] Teaser campaign on social media\n"
                "- [ ] Notify beta customers and advocates\n"
                "- [ ] Sales team training session\n"
                "- [ ] Customer Success team briefing\n"
                "- [ ] QA review of all launch assets and links\n"
                "- [ ] Set up tracking and analytics\n\n"
                "### Launch Day (T-0)\n"
                "- [ ] Publish blog post and landing page\n"
                "- [ ] Send announcement email (segment: all active users)\n"
                "- [ ] Social media posts go live (staggered by platform)\n"
                "- [ ] In-app notification for existing users\n"
                "- [ ] Press outreach (if applicable)\n"
                "- [ ] Update product demo environment\n"
                "- [ ] Monitor social mentions and support volume\n\n"
                "### T+1 to T+4 Weeks: Sustain\n"
                "- [ ] Follow-up emails to non-openers\n"
                "- [ ] Customer webinar: deep-dive on new feature\n"
                "- [ ] Customer case study (early adopter)\n"
                "- [ ] Paid promotion boost on top-performing content\n"
                "- [ ] Retargeting ads for landing page visitors\n"
                "- [ ] Feature adoption tracking dashboard\n\n"
                "## Launch Tier Classification\n"
                "| Tier | Scope | Example |\n"
                "|------|-------|---------|\n"
                "| Tier 1 (Major) | Full campaign | New product line, platform update |\n"
                "| Tier 2 (Feature) | Blog + email + social | New module, major feature |\n"
                "| Tier 3 (Minor) | Blog + in-app | UI improvement, integration |\n"
                "| Tier 4 (Patch) | Release notes only | Bug fixes, minor updates |\n"
            ),
            "articleType": ITSM_HOWTO,
            "isInternal": True,
            "_category": "Marketing",
        },
    ]


# ---------------------------------------------------------------------------
# Main batch runner
# ---------------------------------------------------------------------------

def run(api: ApiClient, log: RunLogger) -> None:
    log.section("BATCH 25: General Knowledge Base — Sales, Service & Marketing Articles")
    ts = int(time.time())

    # ── Check if General KB endpoint exists ──
    general_kb_available = check_service_availability(api, "/api/knowledge")

    if general_kb_available:
        log.log("General KB endpoint (/api/knowledge) detected — will load directly.")
    else:
        log.log(
            "General KB endpoint (/api/knowledge) NOT AVAILABLE — backend not yet implemented. "
            "Loading articles into ITSM KB (/api/itsm/knowledge) as cross-domain knowledge. "
            "See SPEC-SD-002 and ADR-005 for the dual-KB architecture plan."
        )

    # Collect all articles
    sales = _sales_articles(ts)
    service = _service_articles(ts)
    marketing = _marketing_articles(ts)
    all_articles = sales + service + marketing

    log.log(f"Total articles to load: {len(all_articles)} "
            f"(Sales={len(sales)}, Service={len(service)}, Marketing={len(marketing)})")

    article_ids = []

    if general_kb_available:
        # ── Load into General KB when available ──
        log.section("General KB — Loading Articles")
        for article in all_articles:
            payload = {
                "title": article["title"],
                "articleBody": article["articleBody"],
                "shortDescription": article.get("shortDescription"),
                "category": article.get("_category", "General"),
                "visibility": VISIBILITY_INTERNAL if article.get("isInternal", True) else VISIBILITY_PUBLIC,
                "status": STATUS_DRAFT,
            }
            eid = api.create_and_track("general_kb_articles", "/api/knowledge", payload)
            if eid:
                article_ids.append(eid)
        save_ids("general_kb_articles", article_ids)
    else:
        # ── Fallback: Load into ITSM KB as cross-domain knowledge ──
        log.section("ITSM KB — Loading Sales, Service & Marketing Articles (fallback)")
        for article in all_articles:
            payload = {
                "title": article["title"],
                "articleBody": article["articleBody"],
                "shortDescription": article.get("shortDescription"),
                "articleType": article.get("articleType", ITSM_REFERENCE),
                "isInternal": article.get("isInternal", True),
            }
            eid = api.create_and_track("knowledgearticles", "/api/itsm/knowledge", payload)
            if eid:
                article_ids.append(eid)
        save_ids("general_kb_articles_via_itsm", article_ids)

    log.log(f"Created {len(article_ids)} of {len(all_articles)} articles")

    # ── Verification queries ──
    log.section("KB — Verification Queries")
    if general_kb_available:
        api.get("/api/knowledge")
        api.get("/api/knowledge/search?searchTerm=sales+playbook")
        api.get("/api/knowledge/search?searchTerm=marketing+campaign")
        api.get("/api/knowledge/search?searchTerm=customer+onboarding")
    else:
        # Verify via ITSM KB search
        api.get("/api/itsm/knowledge/search?searchTerm=sales+playbook")
        api.get("/api/itsm/knowledge/search?searchTerm=marketing+campaign")
        api.get("/api/itsm/knowledge/search?searchTerm=customer+onboarding")
        api.get("/api/itsm/knowledge/search?searchTerm=objection+handling")
        api.get("/api/itsm/knowledge/search?searchTerm=brand+guidelines")

    # Publish a subset of articles
    if not general_kb_available and article_ids:
        log.section("ITSM KB — Publishing Sales/Service/Marketing Articles")
        for aid in article_ids[:8]:
            api.patch(f"/api/itsm/knowledge/{aid}/publish")

    log.log(f"BATCH 25 complete. IDs saved: {len(article_ids)}")
