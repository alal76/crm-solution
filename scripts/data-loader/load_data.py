#!/usr/bin/env python3
"""
CRM Data Loader - Loads seed data via API endpoints

This script creates three levels of data:
1. Essential: Minimum data required for system to function
2. Basic: Sample entries in each table for testing
3. Demo: Full demonstration data with multiple variations

Usage:
    python load_data.py essential   # Load essential data only
    python load_data.py basic       # Load essential + basic sample data
    python load_data.py demo        # Load all data including full demo set
    python load_data.py --api-base=http://192.168.0.9:5000/api demo

Copyright (C) 2024-2026 Abhishek Lal
Licensed under the GNU Affero General Public License v3.0
"""

import argparse
import json
import random
import sys
from datetime import datetime, timedelta
from typing import Any, Dict, List, Optional
import urllib.request
import urllib.error

# Configuration
DEFAULT_API_BASE = "http://192.168.0.9:5000/api"
DEFAULT_ADMIN_USERNAME = "admin"
DEFAULT_ADMIN_PASSWORD = "Admin@123"

# ============================================================================
# API Client
# ============================================================================

class APIClient:
    """Simple HTTP client for CRM API"""
    
    def __init__(self, base_url: str):
        self.base_url = base_url.rstrip('/')
        self.token: Optional[str] = None
        self.created_ids: Dict[str, List[int]] = {}
    
    def _request(self, method: str, endpoint: str, data: Optional[dict] = None) -> dict:
        """Make HTTP request to API"""
        url = f"{self.base_url}{endpoint}"
        headers = {"Content-Type": "application/json"}
        
        if self.token:
            headers["Authorization"] = f"Bearer {self.token}"
        
        body = json.dumps(data).encode('utf-8') if data else None
        
        request = urllib.request.Request(url, data=body, headers=headers, method=method)
        
        try:
            with urllib.request.urlopen(request, timeout=30) as response:
                response_body = response.read().decode('utf-8')
                return json.loads(response_body) if response_body else {}
        except urllib.error.HTTPError as e:
            error_body = e.read().decode('utf-8') if e.fp else ""
            print(f"  ❌ API Error {e.code}: {error_body[:200]}")
            raise
        except urllib.error.URLError as e:
            print(f"  ❌ Connection Error: {e.reason}")
            raise
    
    def get(self, endpoint: str) -> dict:
        return self._request("GET", endpoint)
    
    def post(self, endpoint: str, data: dict) -> dict:
        return self._request("POST", endpoint, data)
    
    def put(self, endpoint: str, data: dict) -> dict:
        return self._request("PUT", endpoint, data)
    
    def delete(self, endpoint: str) -> dict:
        return self._request("DELETE", endpoint)
    
    def login(self, username: str, password: str) -> bool:
        """Authenticate and store token (accepts username or email)"""
        try:
            # API login endpoint expects either 'email' or 'username' field depending on configuration.
            payload = {"password": password}
            if "@" in username:
                payload["email"] = username
            else:
                payload["username"] = username
            response = self.post("/auth/login", payload)
            self.token = response.get("token") or response.get("accessToken")
            return bool(self.token)
        except Exception as e:
            print(f"  ❌ Login failed: {e}")
            return False
    
    def track_created(self, entity_type: str, entity_id: int):
        """Track created entity IDs for reference"""
        if entity_type not in self.created_ids:
            self.created_ids[entity_type] = []
        self.created_ids[entity_type].append(entity_id)
    
    def get_random_id(self, entity_type: str) -> Optional[int]:
        """Get random ID from created entities"""
        ids = self.created_ids.get(entity_type, [])
        return random.choice(ids) if ids else None


# ============================================================================
# Data Generators
# ============================================================================

class DataGenerator:
    """Generates realistic sample data"""
    
    FIRST_NAMES = [
        "James", "John", "Robert", "Michael", "William", "David", "Richard", "Joseph",
        "Thomas", "Charles", "Mary", "Patricia", "Jennifer", "Linda", "Elizabeth",
        "Barbara", "Susan", "Jessica", "Sarah", "Karen", "Emily", "Ashley", "Amanda",
        "Sophia", "Isabella", "Mia", "Charlotte", "Amelia", "Harper", "Evelyn"
    ]
    
    LAST_NAMES = [
        "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis",
        "Rodriguez", "Martinez", "Hernandez", "Lopez", "Gonzalez", "Wilson", "Anderson",
        "Thomas", "Taylor", "Moore", "Jackson", "Martin", "Lee", "Perez", "Thompson",
        "White", "Harris", "Sanchez", "Clark", "Ramirez", "Lewis", "Robinson"
    ]
    
    COMPANY_PREFIXES = [
        "Global", "United", "Premier", "Advanced", "Strategic", "Dynamic", "Innovative",
        "Precision", "Apex", "Summit", "Elite", "Prime", "Nexus", "Synergy", "Vertex",
        "Quantum", "Alpha", "Omega", "Delta", "Pacific", "Atlantic", "Mountain", "Valley"
    ]
    
    COMPANY_SUFFIXES = [
        "Technologies", "Solutions", "Industries", "Systems", "Enterprises", "Group",
        "Corp", "Inc", "Partners", "Holdings", "Services", "Consulting", "Digital",
        "Innovations", "Networks", "Software", "Hardware", "Manufacturing", "Logistics"
    ]
    
    INDUSTRIES = [
        "Technology", "Healthcare", "Finance", "Manufacturing", "Retail", "Education",
        "Real Estate", "Construction", "Transportation", "Energy", "Telecommunications",
        "Media", "Hospitality", "Agriculture", "Automotive", "Aerospace"
    ]
    
    CITIES = [
        ("New York", "NY", "10001"), ("Los Angeles", "CA", "90001"),
        ("Chicago", "IL", "60601"), ("Houston", "TX", "77001"),
        ("Phoenix", "AZ", "85001"), ("Philadelphia", "PA", "19101"),
        ("San Antonio", "TX", "78201"), ("San Diego", "CA", "92101"),
        ("Dallas", "TX", "75201"), ("San Jose", "CA", "95101"),
        ("Austin", "TX", "78701"), ("Seattle", "WA", "98101"),
        ("Denver", "CO", "80201"), ("Boston", "MA", "02101"),
        ("Miami", "FL", "33101"), ("Atlanta", "GA", "30301")
    ]
    
    @classmethod
    def name(cls) -> tuple:
        """Generate random first and last name"""
        return random.choice(cls.FIRST_NAMES), random.choice(cls.LAST_NAMES)
    
    @classmethod
    def company(cls) -> str:
        """Generate random company name"""
        return f"{random.choice(cls.COMPANY_PREFIXES)} {random.choice(cls.COMPANY_SUFFIXES)}"
    
    @classmethod
    def email(cls, first_name: str, last_name: str, company: str = None) -> str:
        """Generate email from name"""
        domain = company.lower().replace(" ", "")[:15] + ".com" if company else "example.com"
        return f"{first_name.lower()}.{last_name.lower()}@{domain}"
    
    @classmethod
    def phone(cls) -> str:
        """Generate random US phone number"""
        return f"+1-{random.randint(200,999)}-{random.randint(200,999)}-{random.randint(1000,9999)}"
    
    @classmethod
    def address(cls) -> dict:
        """Generate random address"""
        city, state, zip_code = random.choice(cls.CITIES)
        return {
            "address": f"{random.randint(100, 9999)} {random.choice(['Main', 'Oak', 'Pine', 'Elm', 'Cedar'])} {random.choice(['Street', 'Avenue', 'Boulevard', 'Drive', 'Lane'])}",
            "city": city,
            "state": state,
            "postalCode": zip_code,
            "country": "United States"
        }
    
    @classmethod
    def industry(cls) -> str:
        return random.choice(cls.INDUSTRIES)
    
    @classmethod
    def website(cls, company: str) -> str:
        return f"https://www.{company.lower().replace(' ', '')}.com"
    
    @classmethod
    def date_in_past(cls, max_days: int = 365) -> str:
        """Generate random date in the past"""
        days = random.randint(1, max_days)
        return (datetime.now() - timedelta(days=days)).strftime("%Y-%m-%d")
    
    @classmethod
    def date_in_future(cls, max_days: int = 365) -> str:
        """Generate random date in the future"""
        days = random.randint(1, max_days)
        return (datetime.now() + timedelta(days=days)).strftime("%Y-%m-%d")
    
    @classmethod
    def currency(cls) -> float:
        """Generate random currency amount"""
        return round(random.uniform(1000, 500000), 2)


# ============================================================================
# Essential Data (Required for system to function)
# ============================================================================

def load_essential_data(api: APIClient):
    """Load minimum data required for system operation"""
    print("\n" + "="*60)
    print("📋 LOADING ESSENTIAL DATA")
    print("="*60)
    
    # Check if essential data already exists
    try:
        users = api.get("/users")
        if isinstance(users, list) and len(users) > 1:
            print("  ℹ️  Users already exist, skipping essential setup")
            return True
    except:
        pass
    
    # Create default departments
    print("\n🏢 Creating departments...")
    departments = [
        {"name": "Sales", "description": "Sales department", "code": "SALES", "isActive": True},
        {"name": "Marketing", "description": "Marketing department", "code": "MKT", "isActive": True},
        {"name": "Customer Support", "description": "Customer support department", "code": "SUP", "isActive": True},
        {"name": "Engineering", "description": "Engineering department", "code": "ENG", "isActive": True},
        {"name": "Operations", "description": "Operations department", "code": "OPS", "isActive": True},
    ]
    
    for dept in departments:
        try:
            result = api.post("/departments", dept)
            api.track_created("department", result.get("id", 0))
            print(f"  ✓ Created department: {dept['name']}")
        except Exception as e:
            print(f"  ⚠️ Department {dept['name']} may already exist")
    
    # Create lookup items (currencies, countries, etc.)
    print("\n💱 Creating lookup data via admin seed endpoint...")
    # The public API does not support CRUD for lookups; use admin seeder instead.
    try:
        api.post("/admin/seed/lookups", {})
        print("  ✓ Invoked admin lookup seeder")
    except Exception as e:
        print(f"  ⚠️ Lookup seeder call failed: {e}")
        # fall back to individual posts if necessary
        lookups = [
            # Currencies
            {"category": "Currency", "code": "USD", "value": "US Dollar", "displayOrder": 1, "isActive": True},
            {"category": "Currency", "code": "EUR", "value": "Euro", "displayOrder": 2, "isActive": True},
            {"category": "Currency", "code": "GBP", "value": "British Pound", "displayOrder": 3, "isActive": True},
            {"category": "Currency", "code": "CAD", "value": "Canadian Dollar", "displayOrder": 4, "isActive": True},
            {"category": "Currency", "code": "AUD", "value": "Australian Dollar", "displayOrder": 5, "isActive": True},
            
            # Lead Sources
            {"category": "LeadSource", "code": "WEB", "value": "Website", "displayOrder": 1, "isActive": True},
            {"category": "LeadSource", "code": "REF", "value": "Referral", "displayOrder": 2, "isActive": True},
            {"category": "LeadSource", "code": "TRD", "value": "Trade Show", "displayOrder": 3, "isActive": True},
            {"category": "LeadSource", "code": "ADV", "value": "Advertisement", "displayOrder": 4, "isActive": True},
            {"category": "LeadSource", "code": "SOC", "value": "Social Media", "displayOrder": 5, "isActive": True},
            {"category": "LeadSource", "code": "EML", "value": "Email Campaign", "displayOrder": 6, "isActive": True},
            
            # Industries
            {"category": "Industry", "code": "TECH", "value": "Technology", "displayOrder": 1, "isActive": True},
            {"category": "Industry", "code": "FIN", "value": "Finance", "displayOrder": 2, "isActive": True},
            {"category": "Industry", "code": "HLTH", "value": "Healthcare", "displayOrder": 3, "isActive": True},
            {"category": "Industry", "code": "MFG", "value": "Manufacturing", "displayOrder": 4, "isActive": True},
            {"category": "Industry", "code": "RET", "value": "Retail", "displayOrder": 5, "isActive": True},
            {"category": "Industry", "code": "EDU", "value": "Education", "displayOrder": 6, "isActive": True},
            
            # Contact Roles
            {"category": "ContactRole", "code": "DM", "value": "Decision Maker", "displayOrder": 1, "isActive": True},
            {"category": "ContactRole", "code": "INF", "value": "Influencer", "displayOrder": 2, "isActive": True},
            {"category": "ContactRole", "code": "TEC", "value": "Technical Evaluator", "displayOrder": 3, "isActive": True},
            {"category": "ContactRole", "code": "USR", "value": "End User", "displayOrder": 4, "isActive": True},
            {"category": "ContactRole", "code": "OTH", "value": "Other", "displayOrder": 5, "isActive": True},
        ]
        for lookup in lookups:
            try:
                result = api.post("/lookups", lookup)
                api.track_created("lookup", result.get("id", 0))
            except:
                pass
        print(f"  ✓ Created {len(lookups)} lookup items (fallback)")
    
    print("\n✅ Essential data loaded successfully!")
    return True


# ============================================================================
# Basic Data (Sample entries for each entity)
# ============================================================================

def load_basic_data(api: APIClient):
    """Load basic sample data - a few entries in each table"""
    print("\n" + "="*60)
    print("📦 LOADING BASIC DATA")
    print("="*60)
    
    # Create sample users
    print("\n👥 Creating sample users...")
    users = [
        {"username": "jsmith", "email": "john.smith@crm.local", "password": "User@123",
         "firstName": "John", "lastName": "Smith", "role": 2, "isActive": True},
        {"username": "mjohnson", "email": "mary.johnson@crm.local", "password": "User@123",
         "firstName": "Mary", "lastName": "Johnson", "role": 2, "isActive": True},
        {"username": "rwilliams", "email": "robert.williams@crm.local", "password": "User@123",
         "firstName": "Robert", "lastName": "Williams", "role": 3, "isActive": True},
    ]
    
    for user in users:
        try:
            result = api.post("/users", user)
            api.track_created("user", result.get("id", 0))
            print(f"  ✓ Created user: {user['username']}")
        except:
            print(f"  ⚠️ User {user['username']} may already exist")
    
    # Create sample accounts (customers) using current CreateAccountDto
    print("\n👤 Creating sample accounts...")
    # Category enum: 0=Individual, 1=Organization
    accounts = [
        # Individual
        {
            "category": 0,
            "firstName": "Alice",
            "lastName": "Brown",
            "email": "alice.brown@email.com",
            "phone": "+1-555-0101",
            "address": DataGenerator.address()["address"],
            "city": DataGenerator.address()["city"],
            "state": DataGenerator.address()["state"],
            "zipCode": DataGenerator.address()["postalCode"],
            "country": DataGenerator.address()["country"],
            "lifecycleStage": 2,    # Customer enum value maybe
            "priority": 1           # Medium
        },
        # Organization (small business)
        {
            "category": 1,
            "company": "Acme Solutions LLC",
            "legalName": "Acme Solutions Limited Liability Company",
            "email": "info@acmesolutions.com",
            "phone": "+1-555-0102",
            "industry": "Technology",
            "website": "https://acmesolutions.com",
            "annualRevenue": 1500000,
            "lifecycleStage": 2,
            "priority": 1,
            "address": DataGenerator.address()["address"],
            "city": DataGenerator.address()["city"],
            "state": DataGenerator.address()["state"],
            "zipCode": DataGenerator.address()["postalCode"],
            "country": DataGenerator.address()["country"]
        },
        # Organization (enterprise)
        {
            "category": 1,
            "company": "Global Industries Inc",
            "legalName": "Global Industries Incorporated",
            "email": "contact@globalindustries.com",
            "phone": "+1-555-0103",
            "industry": "Manufacturing",
            "website": "https://globalindustries.com",
            "annualRevenue": 50000000,
            "lifecycleStage": 2,
            "priority": 2,
            "address": DataGenerator.address()["address"],
            "city": DataGenerator.address()["city"],
            "state": DataGenerator.address()["state"],
            "zipCode": DataGenerator.address()["postalCode"],
            "country": DataGenerator.address()["country"]
        },
    ]
    
    # Create sample products using Product entity structure (needed for later financial records)
    print("\n📦 Creating sample products...")
    products = [
        {"name": "CRM Professional License", "category": "Software", "unitPrice": 999.00, "sku": "CRM-PRO-001"},
        {"name": "CRM Cloud - Monthly", "category": "Subscription", "unitPrice": 99.00, "sku": "CRM-CLOUD-M"},
        {"name": "API Calls Package", "category": "Usage", "unitPrice": 49.00, "sku": "API-1000"},
        {"name": "Implementation Services", "category": "Services", "unitPrice": 2500.00, "sku": "SVC-IMPL"},
    ]
    product_ids = []
    # fetch existing products to avoid duplicates
    try:
        existing_raw = api.get("/products")
        print(f"  ℹ️ Existing products raw response: {existing_raw}")
        if isinstance(existing_raw, dict) and "items" in existing_raw:
            existing = existing_raw.get("items", [])
        elif isinstance(existing_raw, list):
            existing = existing_raw
        else:
            existing = []
    except Exception:
        existing = []
    existing_by_sku = {p.get("sku"): p.get("id") for p in existing}
    for product in products:
        sku = product.get("sku")
        if sku in existing_by_sku and existing_by_sku[sku]:
            pid = existing_by_sku[sku]
            product_ids.append(pid)
            api.track_created("product", pid)
            print(f"  ℹ️ Skipping existing product {sku} (ID {pid})")
            continue
        try:
            result = api.post("/products", product)
            pid = result.get("id", 0)
            api.track_created("product", pid)
            product_ids.append(pid)
            print(f"  ✓ Created product: {product['name']}")
        except Exception as e:
            print(f"  ❌ Failed to create product {product['name']}: {e}")

    # Create sample accounts (customers) using current CreateAccountDto
    print("\n👤 Creating sample accounts...")
    # Category enum: 0=Individual, 1=Organization
    
    for acct in accounts:
        try:
            result = api.post("/accounts", acct)
            acct_id = result.get("id", 0)
            api.track_created("account", acct_id)
            name = acct.get("company") or f"{acct.get('firstName','')} {acct.get('lastName','')}"
            print(f"  ✓ Created account: {name}")
        except Exception as e:
            print(f"  ❌ Failed to create account: {e}")
            acct_id = None

        # create a series of related financial/doc records for the account
        if acct_id:
            prod_id = random.choice(product_ids) if product_ids else api.get_random_id("product")
            if not prod_id:
                print("    ⚠️ No product available to create financial records")
            else:
                try:
                    # create a quote with at least one line item so validation passes
                    # generate a unique quote number (server requires it due to [Required] attribute)
                    def gen_quote_number():
                        year = datetime.now().year
                        return f"Q-{year}-{random.randint(1,99999):05d}"

                    quote = {"accountId": acct_id,
                             "quoteNumber": gen_quote_number(),
                             "name": "Initial Quote",
                             "currency": "USD",
                             "quoteLineItems": [{"productId": prod_id, "quantity": 1, "unitPrice": 5000}]}
                    qres = api.post("/quotes", quote)
                    api.track_created("quote", qres.get("id", 0))
                    print(f"    ✓ Created quote {qres.get('id')}")

                    order = {"accountId": acct_id, "quoteId": qres.get("id", 0), "status": "Processing", "currency": "USD", "totalAmount": 5000,
                             "orderDate": datetime.now().strftime("%Y-%m-%d")}
                    ores = api.post("/orders", order)
                    api.track_created("order", ores.get("id", 0))
                    print(f"    ✓ Created order {ores.get('id')}")

                    invoice = {"accountId": acct_id, "orderId": ores.get("id", 0), "invoiceNumber": f"INV-{random.randint(1000,9999)}", "totalAmount": 5000, "currency": "USD", "dueDate": DataGenerator.date_in_future(30)}
                    ires = api.post("/invoices", invoice)
                    api.track_created("invoice", ires.get("id", 0))
                    print(f"    ✓ Created invoice {ires.get('id')}")

                    payment = {"invoiceId": ires.get("id", 0), "accountId": acct_id, "amount": ires.get("totalAmount", 0), "paymentDate": DataGenerator.date_in_past(10), "method": "CreditCard"}
                    pres = api.post("/payments", payment)
                    api.track_created("payment", pres.get("id", 0))
                    print(f"    ✓ Created payment for invoice {ires.get('id')}")

                    contract = {"accountId": acct_id, "name": "Support Contract", "startDate": DataGenerator.date_in_past(30), "endDate": DataGenerator.date_in_future(365), "status": "Active", "totalValue": 12000, "currency": "USD"}
                    cres = api.post("/contracts", contract)
                    api.track_created("contract", cres.get("id", 0))
                    print(f"    ✓ Created contract {cres.get('id')}")

                    subscription = {"accountId": acct_id, "contractId": cres.get("id", 0), "productId": prod_id, "startDate": DataGenerator.date_in_past(30), "billingCycle": "Monthly", "status": "Active"}
                    sres = api.post("/subscriptions", subscription)
                    api.track_created("subscription", sres.get("id", 0))
                    print(f"    ✓ Created subscription {sres.get('id')}")
                except Exception as e:
                    print(f"    ⚠️ Related financial record failed: {e}")
    
    # Create sample leads (basic CreateLeadDto)
    print("\n🎯 Creating sample leads...")
    leads = [
        {"firstName": "David", "lastName": "Lee", "companyName": "Tech Startup Inc", "email": "david.lee@prospect.com", "phone": "+1-555-0201"},
        {"firstName": "Sarah", "lastName": "Chen", "companyName": "BigCorp International", "email": "sarah.chen@bigcorp.com", "phone": "+1-555-0202"},
        {"firstName": "Michael", "lastName": "Garcia", "companyName": "Enterprise Solutions", "email": "m.garcia@enterprise.com", "phone": "+1-555-0203"},
    ]
    
    for lead in leads:
        try:
            result = api.post("/leads", lead)
            api.track_created("lead", result.get("id", 0))
            print(f"  ✓ Created lead: {lead['firstName']} {lead['lastName']}")
        except Exception as e:
            print(f"  ❌ Failed to create lead: {e}")
    
    # Create sample opportunities (3 stages)
    print("\n💰 Creating sample opportunities...")
    
    customer_id = api.get_random_id("account")
    if customer_id:
        opportunities = [
            # Discovery stage
            {
                "name": "CRM Implementation - Acme", "stage": 0, "probability": 10,
                "amount": 25000, "currency": "USD",
                "expectedCloseDate": DataGenerator.date_in_future(90),
                "accountId": customer_id
            },
            # Proposal stage
            {
                "name": "Cloud Migration - Global", "stage": 2, "probability": 50,
                "amount": 75000, "currency": "USD",
                "expectedCloseDate": DataGenerator.date_in_future(60),
                "accountId": customer_id
            },
            # Negotiation stage
            {
                "name": "Enterprise License Deal", "stage": 3, "probability": 75,
                "amount": 150000, "currency": "USD",
                "expectedCloseDate": DataGenerator.date_in_future(30),
                "accountId": customer_id
            },
        ]
        
        for opp in opportunities:
            try:
                result = api.post("/opportunities", opp)
                api.track_created("opportunity", result.get("id", 0))
                print(f"  ✓ Created opportunity: {opp['name']}")
            except Exception as e:
                print(f"  ❌ Failed to create opportunity: {e}")
    
    # Create sample lookups, departments and groups
    print("\n🔧 Creating configuration data (lookup, department, user group)")
    try:
        lres = api.post("/lookups", {"category":"Currency","code":"USD","value":"US Dollar","displayOrder":1,"isActive":True})
        api.track_created("lookup", lres.get("id",0))
        print("  ✓ Created lookup USD")
    except Exception as e:
        print(f"  ❌ Lookup error: {e}")
    try:
        dres = api.post("/departments", {"name":"Sales","code":"SALES","isActive":True})
        api.track_created("department", dres.get("id",0))
        print("  ✓ Created department Sales")
    except Exception as e:
        print(f"  ❌ Department error: {e}")
    try:
        gres = api.post("/usergroups", {"name":"Sales Team","description":"All sales users"})
        api.track_created("usergroup", gres.get("id",0))
        print("  ✓ Created user group Sales Team")
    except Exception as e:
        print(f"  ❌ User group error: {e}")

    # Create sample campaigns (3 types) using minimal CreateCampaignDto
    print("\n📣 Creating sample campaigns...")
    campaigns = [
        {"name": "Q1 Newsletter Campaign", "campaignType": 0, "budget": 5000, "targetRoi": 1.2},
        {"name": "Annual User Conference", "campaignType": 1, "budget": 50000, "targetRoi": 2.0},
        {"name": "Product Demo Webinar Series", "campaignType": 2, "budget": 2000, "targetRoi": 0.5},
    ]

    for campaign in campaigns:
        try:
            result = api.post("/campaigns", campaign)
            api.track_created("campaign", result.get("id", 0))
            print(f"  ✓ Created campaign: {campaign['name']}")
            # add a recipient and metric to each
            rid = api.post(f"/campaigns/{result.get('id')}/recipients", {"accountId": api.get_random_id("account"), "contactId": api.get_random_id("contact")}).get("id",0)
            api.track_created("campaignrecipient", rid)
            m = api.post(f"/campaigns/{result.get('id')}/metrics", {"metricType":"OpenRate","value":0.5})
            api.track_created("campaignmetric", m.get("id",0))
        except Exception as e:
            msg = str(e)
            if "405" in msg or "Method Not Allowed" in msg:
                print(f"  ⚠️ Campaign POST not allowed (405); skipping campaign: {campaign['name']}")
                continue
            print(f"  ❌ Failed to create campaign: {e}")
    
    
    print("\n✅ Basic data loaded successfully!")
    return True


# ============================================================================
# Demo Data (Full test data with 3+ types per entity)
# ============================================================================

def load_demo_data(api: APIClient):
    """Load comprehensive demo data with multiple variations"""
    print("\n" + "="*60)
    print("🎭 LOADING DEMO DATA")
    print("="*60)
    
    # Create 20+ demo accounts with variety
    print("\n👤 Creating demo accounts (20+)...")
    
    for i in range(20):
        is_org = i % 3 != 0
        if is_org:
            company = DataGenerator.company()
            acct = {
                "category": 1,
                "company": company,
                "legalName": f"{company} LLC",
                "email": f"info@{company.lower().replace(' ', '')}.com",
                "phone": DataGenerator.phone(),
                "lifecycleStage": 2,
                "priority": random.choice([0,1,2,3]),
                "industry": DataGenerator.industry(),
                "website": DataGenerator.website(company),
                "annualRevenue": random.randint(100000,10000000),
                **DataGenerator.address()
            }
        else:
            first, last = DataGenerator.name()
            acct = {
                "category": 0,
                "firstName": first,
                "lastName": last,
                "email": DataGenerator.email(first, last),
                "phone": DataGenerator.phone(),
                "lifecycleStage": random.choice([1,2,3]),
                "priority": random.choice([0,1,2]),
                **DataGenerator.address()
            }
        try:
            result = api.post("/accounts", acct)
            api.track_created("account", result.get("id", 0))
            # also attach a phone and email record
            aid = result.get("id",0)
            # link a new phone number using proper DTO
            api.post("/contactinfo/phones/link", {"entityType":"Account","entityId":aid,
                      "newPhone": {"number": DataGenerator.phone(), "countryCode": "+1"}})
            # link a new email address using proper DTO
            api.post("/contactinfo/emails/link", {"entityType":"Account","entityId":aid,
                      "newEmail": {"email": DataGenerator.email(first if not is_org else company,'','info')}})
        except:
            pass
    
    print(f"  ✓ Created {len(api.created_ids.get('account', []))} accounts")
    # demo quotes/orders/invoices/payments/contract/subscription
    for acct in api.created_ids.get("account", [])[:5]:
        prod = api.get_random_id("product")
        quote_id = None
        if not prod:
            print(f"  ⚠️ No product available for quote creation on acct {acct}")
            continue
        # payload must match CreateQuoteDto and include quoteNumber
        def gen_quote_number():
            year = datetime.now().year
            return f"Q-{year}-{random.randint(1,99999):05d}"

        quote = {"accountId": acct,
                 "quoteNumber": gen_quote_number(),
                 "name": "Demo Quote",
                 "currency": "USD",
                 "quoteLineItems": [{"productId": prod, "quantity":1, "unitPrice":5000}]}
        print(f"    Debug quote payload for acct {acct}: {quote}")
        try:
            q = api.post("/quotes", quote)
            quote_id = q.get("id",0)
            api.track_created("quote", quote_id)
        except Exception as e:
            print(f"  ❌ Failed to create quote for acct {acct}: {e}")
        if quote_id:
            # create order
            try:
                order = {
                    "accountId": acct,
                    "quoteId": quote_id,
                    "name": quote.get("name", "Order from Quote"),
                    "orderDate": DataGenerator.date_in_past(30),
                    "status": "Processing",
                    "totalAmount": 5000,
                    "currency": "USD",
                }
                print(f"    ➤ Order payload: {order}")
                o = api.post("/orders", order)
                api.track_created("order", o.get("id",0))
                print(f"    ✓ Order created (id={o.get('id')})")
            except Exception as e:
                print(f"    ❌ Order creation failed: {e}")
                return
            # create invoice
            try:
                inv = {"accountId":acct,"orderId":o.get("id",0),"invoiceNumber":"INV-"+str(o.get("id",0)),"totalAmount":5000,"currency":"USD","dueDate":DataGenerator.date_in_future(30)}
                print(f"    ➤ Invoice payload: {inv}")
                i = api.post("/invoices", inv); api.track_created("invoice", i.get("id",0))
                print(f"    ✓ Invoice created (id={i.get('id')})")
            except Exception as e:
                print(f"    ❌ Invoice creation failed: {e}")
                return
            # create payment
            try:
                pay = {"invoiceId":i.get("id",0),"accountId":acct,"amount":5000,"paymentDate":DataGenerator.date_in_future(35),"method":"CreditCard"}
                print(f"    ➤ Payment payload: {pay}")
                api.post("/payments", pay)
                print(f"    ✓ Payment recorded")
            except Exception as e:
                print(f"    ❌ Payment creation failed: {e}")
                return
            # create contract
            try:
                con = {"accountId":acct,"name":"Demo Contract","startDate":DataGenerator.date_in_past(60),"endDate":DataGenerator.date_in_future(300),"status":"Active","totalValue":12000,"currency":"USD"}
                print(f"    ➤ Contract payload: {con}")
                c = api.post("/contracts", con); api.track_created("contract", c.get("id",0))
                print(f"    ✓ Contract created (id={c.get('id')})")
            except Exception as e:
                print(f"    ❌ Contract creation failed: {e}")
                return
            # create subscription
            try:
                sub = {"accountId":acct,"contractId":c.get("id",0),"productId":prod,"startDate":DataGenerator.date_in_past(60),"billingCycle":"Monthly","status":"Active"}
                print(f"    ➤ Subscription payload: {sub}")
                api.post("/subscriptions", sub)
                print(f"    ✓ Subscription created")
            except Exception as e:
                print(f"    ❌ Subscription creation failed: {e}")
                return
    # debug output of account IDs
    print(f"    Account IDs: {api.created_ids.get('account', [])}")
    
    # Create contacts for each organization customer
    print("\n📇 Creating demo contacts...")
    contact_count = 0
    
    for account_id in api.created_ids.get("account", []):
        # Create 1-3 contacts per account
        for _ in range(random.randint(1, 3)):
            first, last = DataGenerator.name()
            contact = {
                "firstName": first,
                "lastName": last,
                "email": DataGenerator.email(first, last),
                "phone": DataGenerator.phone(),
                "title": random.choice(["CEO", "CTO", "VP Sales", "Director", "Manager", "Engineer"]),
                "accountId": account_id,
                "isPrimary": contact_count == 0
            }
            try:
                result = api.post("/contacts", contact)
                api.track_created("contact", result.get("id", 0))
                contact_count += 1
            except:
                pass
    
    print(f"  ✓ Created {contact_count} contacts")
    
    # Create 15+ leads with variety
    print("\n🎯 Creating demo leads (15+)...")
    
    for i in range(15):
        first, last = DataGenerator.name()
        lead = {
            "firstName": first,
            "lastName": last,
            "companyName": DataGenerator.company(),
            "email": DataGenerator.email(first, last, "prospect"),
            "phone": DataGenerator.phone()
        }
        try:
            result = api.post("/leads", lead)
            api.track_created("lead", result.get("id", 0))
        except:
            pass
    
    print(f"  ✓ Created {len(api.created_ids.get('lead', []))} leads")
    
    # Create 10+ opportunities across all stages
    print("\n💰 Creating demo opportunities (10+)...")
    
    stages = [0, 1, 2, 3, 4, 5]  # Discovery to Closed Lost
    probabilities = {0: 10, 1: 25, 2: 50, 3: 75, 4: 100, 5: 0}
    
    for i in range(10):
        customer_id = api.get_random_id("account")
        if not customer_id:
            continue
            
        stage = random.choice(stages)
        opp = {
            "name": f"{DataGenerator.company()} - {random.choice(['Cloud', 'Enterprise', 'Pro', 'Standard'])} Deal",
            "stage": stage,
            "probability": probabilities[stage],
            "amount": random.randint(10000, 500000),
            "currency": random.choice(["USD", "EUR", "GBP"]),
            "expectedCloseDate": DataGenerator.date_in_future(random.randint(30, 180)),
            "accountId": customer_id,
            "pricingModel": random.choice([0, 1, 2, 3]),
            "termLengthMonths": random.choice([12, 24, 36])
        }
        print(f"    Attempting opportunity for account_id={customer_id}")
        try:
            result = api.post("/opportunities", opp)
            api.track_created("opportunity", result.get("id", 0))
        except:
            pass
    
    print(f"  ✓ Created {len(api.created_ids.get('opportunity', []))} opportunities")
    
    # Create service requests
    print("\n🎫 Creating demo service requests...")
    
    priorities = [0, 1, 2, 3]  # Low, Medium, High, Critical
    statuses = [0, 1, 2, 3, 4]  # New, Open, InProgress, Resolved, Closed
    
    for i in range(10):
        customer_id = api.get_random_id("account")
        if not customer_id:
            continue
            
        sr = {
            "subject": random.choice([
                "Login issues", "Feature request", "Billing inquiry",
                "Technical support needed", "Integration help",
                "Performance problems", "Data export request"
            ]),
            "description": "Detailed description of the issue or request.",
            "priority": random.choice(priorities),
            "status": random.choice(statuses),
            "customerId": customer_id,
            "category": random.choice(["Technical", "Billing", "General", "Feature Request"])
        }
        try:
            # endpoint is 'servicerequests' (no hyphen)
            result = api.post("/servicerequests", sr)
            api.track_created("servicerequest", result.get("id", 0))
        except:
            pass
    
    print(f"  ✓ Created {len(api.created_ids.get('servicerequest', []))} service requests")
    
    # Create tasks
    print("\n✅ Creating demo tasks...")
    
    task_types = [0, 1, 2, 3, 4]  # Call, Email, Meeting, Follow-up, Other
    
    for i in range(15):
        customer_id = api.get_random_id("customer")
        
        task = {
            "title": random.choice([
                "Follow up on proposal", "Schedule demo call",
                "Send pricing information", "Check on implementation",
                "Quarterly review meeting", "Contract renewal discussion",
                "Technical consultation", "Onboarding call"
            ]),
            "description": "Task details and notes.",
            "dueDate": DataGenerator.date_in_future(random.randint(1, 30)),
            "priority": random.choice([0, 1, 2]),
            "status": random.choice([0, 1, 2]),  # Not Started, In Progress, Completed
            "taskType": random.choice(task_types),
            "accountId": customer_id
        }
        try:
            result = api.post("/tasks", task)
            api.track_created("task", result.get("id", 0))
        except:
            pass
    
    print(f"  ✓ Created {len(api.created_ids.get('task', []))} tasks")
    
    print("\n✅ Demo data loaded successfully!")
    # apply some updates
    print("\n🔁 Applying update examples to created records...")
    accs = api.created_ids.get("account", [])
    if accs:
        try:
            api.put(f"/accounts/{accs[0]}", {"company":"Updated Co.","phone":DataGenerator.phone()})
            print(f"  ✓ Updated account {accs[0]}")
        except Exception as e:
            print(f"  ❌ Failed to update account: {e}")
    prods = api.created_ids.get("product", [])
    if prods:
        try:
            api.put(f"/products/{prods[0]}", {"price": prods[0] * 10 + 1})
            print(f"  ✓ Updated product {prods[0]}")
        except Exception as e:
            print(f"  ❌ Failed to update product: {e}")
    opps = api.created_ids.get("opportunity", [])
    if opps:
        try:
            api.put(f"/opportunities/{opps[0]}", {"stage": 4})
            print(f"  ✓ Updated opportunity {opps[0]} to ClosedWon")
        except Exception as e:
            print(f"  ❌ Failed to update opportunity: {e}")
    return True


# ============================================================================
# Main Entry Point
# ============================================================================

def main():
    parser = argparse.ArgumentParser(
        description="CRM Data Loader - Load seed data via API",
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog="""
Data Levels:
  essential  - Minimum data required for system to function (roles, lookups)
  basic      - Essential + sample entries in each table
  demo       - Essential + Basic + comprehensive test data (20+ per entity)
        """
    )
    
    parser.add_argument("level", nargs="?", default="demo",
                       choices=["essential", "basic", "demo"],
                       help="Data level to load (default: demo)")
    parser.add_argument("--api-base", default=DEFAULT_API_BASE,
                       help=f"API base URL (default: {DEFAULT_API_BASE})")
    parser.add_argument("--username", default=DEFAULT_ADMIN_USERNAME,
                       help=f"Admin username (default: {DEFAULT_ADMIN_USERNAME})")
    parser.add_argument("--password", default=DEFAULT_ADMIN_PASSWORD,
                       help=f"Admin password (default: {DEFAULT_ADMIN_PASSWORD})")
    parser.add_argument("--dry-run", action="store_true",
                       help="Show what would be created without making API calls")
    
    args = parser.parse_args()
    
    print("╔════════════════════════════════════════════════════════════╗")
    print("║            CRM Data Loader - API-Based Seeding             ║")
    print("╚════════════════════════════════════════════════════════════╝")
    print()
    print(f"  API Base:    {args.api_base}")
    print(f"  Data Level:  {args.level}")
    print()
    
    if args.dry_run:
        print("  ⚠️  DRY RUN MODE - No data will be created")
        return 0
    
    # Initialize API client
    api = APIClient(args.api_base)
    
    # Authenticate
    print("🔐 Authenticating...")
    if not api.login(args.username, args.password):
        print("❌ Authentication failed. Please check credentials.")
        return 1
    print("  ✓ Authentication successful")
    
    # Load data based on level
    try:
        if args.level in ["essential", "basic", "demo"]:
            load_essential_data(api)
        
        if args.level in ["basic", "demo"]:
            load_basic_data(api)
        
        if args.level == "demo":
            load_demo_data(api)
        
        # Print summary
        print("\n" + "="*60)
        print("📊 DATA LOADING SUMMARY")
        print("="*60)
        
        for entity_type, ids in api.created_ids.items():
            print(f"  {entity_type.capitalize():20s}: {len(ids)} records")
        
        total = sum(len(ids) for ids in api.created_ids.values())
        print(f"  {'TOTAL':20s}: {total} records")
        print()
        
        return 0
        
    except KeyboardInterrupt:
        print("\n\n⚠️  Operation cancelled by user")
        return 1
    except Exception as e:
        print(f"\n❌ Error: {e}")
        return 1


if __name__ == "__main__":
    sys.exit(main())
