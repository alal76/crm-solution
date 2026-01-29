#!/bin/bash
# Seed Test Data Script
# Generates sample data via CRM API:
# - 25 customers with 1-30 contacts each
# - 25 opportunities
# - 35 leads
# - 5 campaigns
# - 50 service requests
# - 150 quotes
# - 250 interactions

set -e

API_BASE="${API_BASE:-http://192.168.0.9:5000/api}"
AUTH_TOKEN=""

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

log_info() { echo -e "${GREEN}[INFO]${NC} $1"; }
log_warn() { echo -e "${YELLOW}[WARN]${NC} $1"; }
log_error() { echo -e "${RED}[ERROR]${NC} $1"; }

# Login and get JWT token
login() {
    log_info "Authenticating as admin..."
    local response=$(curl -s -X POST "${API_BASE}/auth/login" \
        -H "Content-Type: application/json" \
        -d '{"username": "admin", "password": "Admin@123"}')
    
    AUTH_TOKEN=$(echo "$response" | jq -r '.token // .accessToken // empty')
    
    if [ -z "$AUTH_TOKEN" ]; then
        log_error "Failed to authenticate. Response: $response"
        exit 1
    fi
    
    log_info "Authentication successful"
}

# API call helper
api_call() {
    local method=$1
    local endpoint=$2
    local data=$3
    
    if [ -n "$data" ]; then
        curl -s -X "$method" "${API_BASE}${endpoint}" \
            -H "Authorization: Bearer $AUTH_TOKEN" \
            -H "Content-Type: application/json" \
            -d "$data"
    else
        curl -s -X "$method" "${API_BASE}${endpoint}" \
            -H "Authorization: Bearer $AUTH_TOKEN"
    fi
}

# Random data generators
random_name() {
    local first_names=("James" "John" "Robert" "Michael" "William" "David" "Richard" "Joseph" "Thomas" "Charles" "Mary" "Patricia" "Jennifer" "Linda" "Elizabeth" "Barbara" "Susan" "Jessica" "Sarah" "Karen")
    local last_names=("Smith" "Johnson" "Williams" "Brown" "Jones" "Garcia" "Miller" "Davis" "Rodriguez" "Martinez" "Hernandez" "Lopez" "Gonzalez" "Wilson" "Anderson" "Thomas" "Taylor" "Moore" "Jackson" "Martin")
    echo "${first_names[$RANDOM % ${#first_names[@]}]} ${last_names[$RANDOM % ${#last_names[@]}]}"
}

random_company() {
    local prefixes=("Global" "United" "Premier" "Advanced" "Strategic" "Dynamic" "Innovative" "Precision" "Apex" "Summit" "Elite" "Prime" "Nexus" "Synergy" "Vertex")
    local suffixes=("Technologies" "Solutions" "Industries" "Systems" "Enterprises" "Group" "Corp" "Inc" "Partners" "Holdings" "Services" "Consulting" "Digital" "Innovations" "Networks")
    echo "${prefixes[$RANDOM % ${#prefixes[@]}]} ${suffixes[$RANDOM % ${#suffixes[@]}]}"
}

random_email() {
    local name=$1
    local company=$2
    local clean_name=$(echo "$name" | tr '[:upper:]' '[:lower:]' | tr ' ' '.')
    local clean_company=$(echo "$company" | tr '[:upper:]' '[:lower:]' | tr ' ' '' | cut -c1-10)
    echo "${clean_name}@${clean_company}.com"
}

random_phone() {
    echo "+1-$(shuf -i 200-999 -n 1)-$(shuf -i 100-999 -n 1)-$(shuf -i 1000-9999 -n 1)"
}

random_category() {
    local categories=("Enterprise" "SMB" "Startup" "Government" "NonProfit" "Individual")
    echo "${categories[$RANDOM % ${#categories[@]}]}"
}

random_industry() {
    local industries=("Technology" "Healthcare" "Finance" "Retail" "Manufacturing" "Education" "RealEstate" "Legal" "Consulting" "Other")
    echo "${industries[$RANDOM % ${#industries[@]}]}"
}

# Seed Customers
declare -a CUSTOMER_IDS=()
seed_customers() {
    log_info "Creating 25 customers..."
    
    for i in $(seq 1 25); do
        local name=$(random_name)
        local company=$(random_company)
        local email=$(random_email "$name" "$company")
        local phone=$(random_phone)
        local category=$(random_category)
        
        local data=$(cat <<EOF
{
    "firstName": "$(echo $name | cut -d' ' -f1)",
    "lastName": "$(echo $name | cut -d' ' -f2)",
    "company": "$company",
    "email": "$email",
    "phone": "$phone",
    "category": "$category",
    "industry": "$(random_industry)",
    "status": "Active",
    "notes": "Seed data customer #$i"
}
EOF
)
        local response=$(api_call POST "/customers" "$data")
        local customer_id=$(echo "$response" | jq -r '.id // empty')
        
        if [ -n "$customer_id" ]; then
            CUSTOMER_IDS+=("$customer_id")
            log_info "  Created customer #$i: $company (ID: $customer_id)"
        else
            log_warn "  Failed to create customer #$i"
        fi
    done
    
    log_info "Created ${#CUSTOMER_IDS[@]} customers"
}

# Seed Contacts for customers
seed_contacts() {
    log_info "Creating contacts for customers (1-30 per customer)..."
    local total_contacts=0
    
    for customer_id in "${CUSTOMER_IDS[@]}"; do
        local contact_count=$((RANDOM % 30 + 1))
        
        for j in $(seq 1 $contact_count); do
            local name=$(random_name)
            local first_name=$(echo $name | cut -d' ' -f1)
            local last_name=$(echo $name | cut -d' ' -f2)
            local titles=("CEO" "CTO" "CFO" "VP Sales" "Manager" "Director" "Engineer" "Analyst" "Coordinator" "Specialist")
            local title="${titles[$RANDOM % ${#titles[@]}]}"
            
            local data=$(cat <<EOF
{
    "contactType": "Customer",
    "firstName": "$first_name",
    "lastName": "$last_name",
    "jobTitle": "$title",
    "customerId": $customer_id,
    "notes": "Contact for customer $customer_id"
}
EOF
)
            api_call POST "/contacts" "$data" > /dev/null
            ((total_contacts++))
        done
    done
    
    log_info "Created $total_contacts contacts"
}

# Seed Leads
seed_leads() {
    log_info "Creating 35 leads..."
    
    for i in $(seq 1 35); do
        local name=$(random_name)
        local company=$(random_company)
        local statuses=("New" "Contacted" "Qualified" "Proposal" "Negotiation")
        local status="${statuses[$RANDOM % ${#statuses[@]}]}"
        local sources=("Website" "Referral" "Social Media" "Trade Show" "Cold Call" "Email Campaign")
        local source="${sources[$RANDOM % ${#sources[@]}]}"
        
        local data=$(cat <<EOF
{
    "firstName": "$(echo $name | cut -d' ' -f1)",
    "lastName": "$(echo $name | cut -d' ' -f2)",
    "email": "$(random_email "$name" "$company")",
    "phone": "$(random_phone)",
    "companyName": "$company",
    "title": "Decision Maker",
    "status": "$status",
    "source": "$source",
    "estimatedValue": $((RANDOM % 100000 + 10000)),
    "notes": "Seed data lead #$i"
}
EOF
)
        api_call POST "/leads" "$data" > /dev/null
        log_info "  Created lead #$i"
    done
}

# Seed Campaigns
declare -a CAMPAIGN_IDS=()
seed_campaigns() {
    log_info "Creating 5 campaigns..."
    
    local campaign_names=("Q1 Product Launch" "Summer Sales Blitz" "Holiday Promotion" "Customer Appreciation" "New Market Expansion")
    local types=("Email" "Social" "Event" "Direct Mail" "Webinar")
    
    for i in $(seq 0 4); do
        local data=$(cat <<EOF
{
    "name": "${campaign_names[$i]}",
    "description": "Marketing campaign for ${campaign_names[$i]}",
    "type": "${types[$i]}",
    "status": "Active",
    "startDate": "$(date -v-${i}m +%Y-%m-%d)",
    "endDate": "$(date -v+$((3-i))m +%Y-%m-%d)",
    "budget": $((RANDOM % 50000 + 10000)),
    "expectedRevenue": $((RANDOM % 200000 + 50000))
}
EOF
)
        local response=$(api_call POST "/campaigns" "$data")
        local campaign_id=$(echo "$response" | jq -r '.id // empty')
        
        if [ -n "$campaign_id" ]; then
            CAMPAIGN_IDS+=("$campaign_id")
            log_info "  Created campaign: ${campaign_names[$i]} (ID: $campaign_id)"
        fi
    done
}

# Seed Opportunities
seed_opportunities() {
    log_info "Creating 25 opportunities..."
    
    local stages=("Prospecting" "Qualification" "NeedsAnalysis" "ValueProposition" "Proposal" "Negotiation")
    
    for i in $(seq 1 25); do
        local customer_id="${CUSTOMER_IDS[$((RANDOM % ${#CUSTOMER_IDS[@]}))]}"
        local stage="${stages[$RANDOM % ${#stages[@]}]}"
        local amount=$((RANDOM % 200000 + 10000))
        local probability=$((RANDOM % 80 + 10))
        
        local data=$(cat <<EOF
{
    "name": "Opportunity #$i - $(random_company) Deal",
    "customerId": $customer_id,
    "stage": "$stage",
    "amount": $amount,
    "probability": $probability,
    "expectedCloseDate": "$(date -v+$((RANDOM % 90 + 30))d +%Y-%m-%d)",
    "description": "Seed data opportunity #$i"
}
EOF
)
        api_call POST "/opportunities" "$data" > /dev/null
        log_info "  Created opportunity #$i"
    done
}

# Seed Service Requests
seed_service_requests() {
    log_info "Creating 50 service requests..."
    
    local priorities=("Low" "Medium" "High" "Critical")
    local statuses=("New" "InProgress" "Pending" "Resolved" "Closed")
    local subjects=("Login Issue" "Feature Request" "Bug Report" "Performance Problem" "Integration Help" "Training Request" "Billing Question" "Account Update" "Data Export" "Configuration Change")
    
    for i in $(seq 1 50); do
        local customer_id="${CUSTOMER_IDS[$((RANDOM % ${#CUSTOMER_IDS[@]}))]}"
        local priority="${priorities[$RANDOM % ${#priorities[@]}]}"
        local status="${statuses[$RANDOM % ${#statuses[@]}]}"
        local subject="${subjects[$RANDOM % ${#subjects[@]}]} - Ticket #$i"
        
        local data=$(cat <<EOF
{
    "subject": "$subject",
    "description": "This is a seed data service request (#$i) for testing purposes.",
    "customerId": $customer_id,
    "priority": "$priority",
    "status": "$status"
}
EOF
)
        api_call POST "/servicerequests" "$data" > /dev/null
        
        if [ $((i % 10)) -eq 0 ]; then
            log_info "  Created $i service requests..."
        fi
    done
    
    log_info "Created 50 service requests"
}

# Seed Quotes
seed_quotes() {
    log_info "Creating 150 quotes..."
    
    local statuses=(0 1 2 3 4 5 6)  # New, Draft, UnderApproval, Approved, Shared, Viewed, Accepted
    
    for i in $(seq 1 150); do
        local customer_id="${CUSTOMER_IDS[$((RANDOM % ${#CUSTOMER_IDS[@]}))]}"
        local status="${statuses[$RANDOM % ${#statuses[@]}]}"
        local subtotal=$((RANDOM % 50000 + 1000))
        local tax=$((subtotal * 8 / 100))
        local total=$((subtotal + tax))
        
        local data=$(cat <<EOF
{
    "name": "Quote #$i - $(random_company) Proposal",
    "quoteNumber": "QT-$((1000 + i))",
    "customerId": $customer_id,
    "status": $status,
    "subtotal": $subtotal,
    "tax": $tax,
    "total": $total,
    "validityDays": 30,
    "expirationDate": "$(date -v+30d +%Y-%m-%d)",
    "notes": "Seed data quote #$i"
}
EOF
)
        api_call POST "/quotes" "$data" > /dev/null
        
        if [ $((i % 25)) -eq 0 ]; then
            log_info "  Created $i quotes..."
        fi
    done
    
    log_info "Created 150 quotes"
}

# Seed Interactions
seed_interactions() {
    log_info "Creating 250 interactions..."
    
    local types=("Call" "Email" "Meeting" "Note" "Task" "Demo" "Presentation")
    local subjects=("Follow-up call" "Initial contact" "Product demo" "Contract discussion" "Technical review" "Status update" "Quarterly review" "Onboarding session" "Support call" "Sales presentation")
    
    for i in $(seq 1 250); do
        local customer_id="${CUSTOMER_IDS[$((RANDOM % ${#CUSTOMER_IDS[@]}))]}"
        local type="${types[$RANDOM % ${#types[@]}]}"
        local subject="${subjects[$RANDOM % ${#subjects[@]}]}"
        
        local data=$(cat <<EOF
{
    "type": "$type",
    "subject": "$subject - #$i",
    "description": "Seed data interaction #$i for testing purposes.",
    "customerId": $customer_id,
    "interactionDate": "$(date -v-$((RANDOM % 90))d +%Y-%m-%dT%H:%M:%S)",
    "duration": $((RANDOM % 60 + 5))
}
EOF
)
        api_call POST "/interactions" "$data" > /dev/null
        
        if [ $((i % 50)) -eq 0 ]; then
            log_info "  Created $i interactions..."
        fi
    done
    
    log_info "Created 250 interactions"
}

# Main execution
main() {
    echo "========================================"
    echo "CRM Test Data Seeding Script"
    echo "API Base: $API_BASE"
    echo "========================================"
    echo ""
    
    # Check for jq
    if ! command -v jq &> /dev/null; then
        log_error "jq is required but not installed. Please install jq first."
        exit 1
    fi
    
    login
    
    echo ""
    log_info "Starting data seeding..."
    echo ""
    
    seed_customers
    echo ""
    seed_contacts
    echo ""
    seed_leads
    echo ""
    seed_campaigns
    echo ""
    seed_opportunities
    echo ""
    seed_service_requests
    echo ""
    seed_quotes
    echo ""
    seed_interactions
    
    echo ""
    echo "========================================"
    log_info "Seed data generation complete!"
    echo "========================================"
    echo "Summary:"
    echo "  - 25 customers with contacts"
    echo "  - 35 leads"
    echo "  - 5 campaigns"
    echo "  - 25 opportunities"
    echo "  - 50 service requests"
    echo "  - 150 quotes"
    echo "  - 250 interactions"
    echo "========================================"
}

main "$@"
