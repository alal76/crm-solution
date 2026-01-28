#!/bin/bash
# Script to create 10 customers and link them to contacts via API

TOKEN=$(curl -s -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"abhi.lal@gmail.com","password":"Admin@123"}' | \
  grep -o '"accessToken":"[^"]*"' | cut -d'"' -f4)

if [ -z "$TOKEN" ]; then
  echo "Failed to get authentication token"
  exit 1
fi

echo "Token obtained successfully"
echo ""

# Array of customer data (company, firstName, lastName for individual, industry, city)
declare -a CUSTOMERS=(
  '{"company":"Acme Industries","firstName":"","lastName":"","email":"info@acme-ind.com","phone":"555-0101","address":"100 Industrial Way","city":"Chicago","state":"Illinois","zipCode":"60601","country":"USA","industry":"Manufacturing","annualRevenue":5000000}'
  '{"company":"TechFlow Solutions","firstName":"","lastName":"","email":"contact@techflow.io","phone":"555-0102","address":"200 Tech Park","city":"San Francisco","state":"California","zipCode":"94102","country":"USA","industry":"Technology","annualRevenue":2500000}'
  '{"company":"Green Earth Consulting","firstName":"","lastName":"","email":"hello@greenearth.com","phone":"555-0103","address":"75 Sustainability Blvd","city":"Portland","state":"Oregon","zipCode":"97201","country":"USA","industry":"Consulting","annualRevenue":1200000}'
  '{"company":"Midwest Healthcare Group","firstName":"","lastName":"","email":"admin@midwesthc.org","phone":"555-0104","address":"400 Medical Center Dr","city":"Minneapolis","state":"Minnesota","zipCode":"55401","country":"USA","industry":"Healthcare","annualRevenue":8500000}'
  '{"company":"Pacific Retail Partners","firstName":"","lastName":"","email":"sales@pacificretail.com","phone":"555-0105","address":"888 Commerce St","city":"Seattle","state":"Washington","zipCode":"98101","country":"USA","industry":"Retail","annualRevenue":3200000}'
  '{"company":"Summit Financial Advisors","firstName":"","lastName":"","email":"info@summitfa.com","phone":"555-0106","address":"1 Financial Plaza","city":"Denver","state":"Colorado","zipCode":"80202","country":"USA","industry":"Financial Services","annualRevenue":4100000}'
  '{"company":"Atlantic Shipping Co","firstName":"","lastName":"","email":"ops@atlanticship.com","phone":"555-0107","address":"500 Harbor View","city":"Boston","state":"Massachusetts","zipCode":"02101","country":"USA","industry":"Transportation","annualRevenue":12000000}'
  '{"company":"Desert Sun Energy","firstName":"","lastName":"","email":"contact@desertsun.energy","phone":"555-0108","address":"1200 Solar Ave","city":"Phoenix","state":"Arizona","zipCode":"85001","country":"USA","industry":"Energy","annualRevenue":6700000}'
  '{"company":"Creative Media Studios","firstName":"","lastName":"","email":"hello@creativemedia.co","phone":"555-0109","address":"42 Art District","city":"Austin","state":"Texas","zipCode":"78701","country":"USA","industry":"Media & Entertainment","annualRevenue":900000}'
  '{"company":"Precision Manufacturing Inc","firstName":"","lastName":"","email":"info@precisionmfg.com","phone":"555-0110","address":"777 Factory Ln","city":"Detroit","state":"Michigan","zipCode":"48201","country":"USA","industry":"Manufacturing","annualRevenue":15000000}'
)

# Contact IDs that are available for linking (4, 5, 6, 7 - unlinked contacts)
AVAILABLE_CONTACTS=(4 5 6 7)

echo "Creating 10 customers..."
echo ""

CREATED_CUSTOMERS=()

for i in "${!CUSTOMERS[@]}"; do
  CUSTOMER_DATA="${CUSTOMERS[$i]}"
  
  # Add required fields
  FULL_DATA=$(echo "$CUSTOMER_DATA" | python3 -c "
import sys, json
data = json.load(sys.stdin)
data['category'] = 1  # Organization
data['customerType'] = 0  # Prospect
data['priority'] = 1  # Medium
data['lifecycleStage'] = 0  # Lead
data['notes'] = 'Created via API script'
print(json.dumps(data))
")

  echo "Creating customer $((i+1)): $(echo "$CUSTOMER_DATA" | python3 -c "import sys,json; print(json.load(sys.stdin)['company'])")"
  
  RESPONSE=$(curl -s -X POST http://localhost:5000/api/customers \
    -H "Authorization: Bearer $TOKEN" \
    -H "Content-Type: application/json" \
    -d "$FULL_DATA")
  
  CUSTOMER_ID=$(echo "$RESPONSE" | python3 -c "import sys,json; print(json.load(sys.stdin).get('id', 'ERROR'))" 2>/dev/null)
  
  if [ "$CUSTOMER_ID" != "ERROR" ] && [ -n "$CUSTOMER_ID" ]; then
    echo "  ✓ Created with ID: $CUSTOMER_ID"
    CREATED_CUSTOMERS+=("$CUSTOMER_ID")
  else
    echo "  ✗ Failed: $RESPONSE"
  fi
done

echo ""
echo "Linking contacts to customers..."
echo ""

# Link contacts randomly to customers
# Customer 3 -> Contact 4
# Customer 4 -> Contacts 5, 6
# Customer 5 -> Contact 7
# Customer 6 -> Contacts 4, 5
# Customer 7 -> Contact 6
# Customer 8 -> Contacts 4, 7
# Customer 9 -> Contact 5
# Customer 10 -> Contacts 6, 7
# Customer 11 -> Contact 4
# Customer 12 -> All 4 contacts

LINK_DATA='[
  {"customerId": 3, "contacts": [4]},
  {"customerId": 4, "contacts": [5, 6]},
  {"customerId": 5, "contacts": [7]},
  {"customerId": 6, "contacts": [4, 5]},
  {"customerId": 7, "contacts": [6]},
  {"customerId": 8, "contacts": [4, 7]},
  {"customerId": 9, "contacts": [5]},
  {"customerId": 10, "contacts": [6, 7]},
  {"customerId": 11, "contacts": [4]},
  {"customerId": 12, "contacts": [4, 5, 6, 7]}
]'

echo "$LINK_DATA" | python3 -c "
import sys, json, subprocess

data = json.load(sys.stdin)
token = '$TOKEN'

roles = ['Primary Contact', 'Account Manager', 'Technical Lead', 'Billing Contact', 'Decision Maker']

for item in data:
    customer_id = item['customerId']
    for i, contact_id in enumerate(item['contacts']):
        link_data = {
            'contactId': contact_id,
            'role': i,  # 0-4 role enum
            'isPrimaryContact': i == 0,
            'isDecisionMaker': i == 0,
            'receivesBillingNotifications': i == 0,
            'receivesMarketingEmails': True,
            'receivesTechnicalUpdates': i > 0
        }
        
        cmd = [
            'curl', '-s', '-X', 'POST',
            f'http://localhost:5000/api/customers/{customer_id}/contacts',
            '-H', f'Authorization: Bearer {token}',
            '-H', 'Content-Type: application/json',
            '-d', json.dumps(link_data)
        ]
        
        result = subprocess.run(cmd, capture_output=True, text=True)
        
        if result.returncode == 0 and 'error' not in result.stdout.lower():
            print(f'  ✓ Linked Contact {contact_id} to Customer {customer_id}')
        else:
            print(f'  ✗ Failed to link Contact {contact_id} to Customer {customer_id}: {result.stdout}')
"

echo ""
echo "Done! Summary:"
echo "  Created ${#CREATED_CUSTOMERS[@]} customers"
echo "  Linked contacts to customers"
echo ""
echo "Customer IDs created: ${CREATED_CUSTOMERS[*]}"
