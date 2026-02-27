#!/bin/bash
# CRM Solution - Meilisearch Initialization Script
# Creates indexes and configures settings for CRM entities

set -e

MEILISEARCH_HOST="${MEILISEARCH_HOST:-http://localhost:7700}"
MEILISEARCH_API_KEY="${MEILISEARCH_MASTER_KEY:-masterKey}"

echo "Initializing Meilisearch indexes..."

# Wait for Meilisearch to be ready
echo "Waiting for Meilisearch to be available..."
until curl -s "${MEILISEARCH_HOST}/health" | grep -q '"status":"available"'; do
    echo "Meilisearch not ready, waiting 2 seconds..."
    sleep 2
done
echo "Meilisearch is ready!"

# Function to create an index with settings
create_index() {
    local index_name=$1
    local primary_key=$2
    local searchable_attrs=$3
    local filterable_attrs=$4
    local sortable_attrs=$5

    echo "Creating index: ${index_name}"

    # Create or update the index
    curl -s -X POST "${MEILISEARCH_HOST}/indexes" \
        -H "Authorization: Bearer ${MEILISEARCH_API_KEY}" \
        -H "Content-Type: application/json" \
        -d "{\"uid\": \"${index_name}\", \"primaryKey\": \"${primary_key}\"}" || true

    # Wait for task to complete
    sleep 1

    # Update searchable attributes
    if [[ -n "$searchable_attrs" ]]; then
        echo "  Setting searchable attributes..."
        curl -s -X PUT "${MEILISEARCH_HOST}/indexes/${index_name}/settings/searchable-attributes" \
            -H "Authorization: Bearer ${MEILISEARCH_API_KEY}" \
            -H "Content-Type: application/json" \
            -d "${searchable_attrs}"
    fi

    # Update filterable attributes
    if [[ -n "$filterable_attrs" ]]; then
        echo "  Setting filterable attributes..."
        curl -s -X PUT "${MEILISEARCH_HOST}/indexes/${index_name}/settings/filterable-attributes" \
            -H "Authorization: Bearer ${MEILISEARCH_API_KEY}" \
            -H "Content-Type: application/json" \
            -d "${filterable_attrs}"
    fi

    # Update sortable attributes
    if [[ -n "$sortable_attrs" ]]; then
        echo "  Setting sortable attributes..."
        curl -s -X PUT "${MEILISEARCH_HOST}/indexes/${index_name}/settings/sortable-attributes" \
            -H "Authorization: Bearer ${MEILISEARCH_API_KEY}" \
            -H "Content-Type: application/json" \
            -d "${sortable_attrs}"
    fi

    # Enable typo tolerance
    echo "  Enabling typo tolerance..."
    curl -s -X PATCH "${MEILISEARCH_HOST}/indexes/${index_name}/settings" \
        -H "Authorization: Bearer ${MEILISEARCH_API_KEY}" \
        -H "Content-Type: application/json" \
        -d '{
            "typoTolerance": {
                "enabled": true,
                "minWordSizeForTypos": {
                    "oneTypo": 4,
                    "twoTypos": 8
                }
            }
        }'

    echo "  Index ${index_name} configured successfully!"
}

# Create Accounts index
create_index "accounts" "id" \
    '["name", "company", "industry", "website", "description", "billingCity", "billingCountry"]' \
    '["industry", "accountType", "status", "ownerId", "billingCountry", "billingState"]' \
    '["name", "createdAt", "updatedAt", "annualRevenue"]'

# Create Contacts index
create_index "contacts" "id" \
    '["firstName", "lastName", "email", "title", "company", "phone", "description"]' \
    '["accountId", "ownerId", "status", "source", "city", "country"]' \
    '["lastName", "createdAt", "updatedAt"]'

# Create Opportunities index  
create_index "opportunities" "id" \
    '["name", "description", "source", "accountName", "contactName"]' \
    '["accountId", "ownerId", "stage", "status", "pipelineId", "probability"]' \
    '["name", "closeDate", "amount", "createdAt", "updatedAt"]'

# Create Products index
create_index "products" "id" \
    '["name", "sku", "description", "category", "productCode"]' \
    '["category", "isActive", "productFamily"]' \
    '["name", "price", "createdAt"]'

# Create Knowledge Articles index
create_index "knowledge_articles" "id" \
    '["title", "content", "summary", "keywords"]' \
    '["categoryId", "status", "authorId", "isPublished", "language"]' \
    '["title", "publishDate", "viewCount", "createdAt"]'

# Create Leads index
create_index "leads" "id" \
    '["firstName", "lastName", "email", "company", "title", "phone", "description"]' \
    '["ownerId", "status", "source", "rating", "industry", "country"]' \
    '["lastName", "company", "createdAt", "updatedAt"]'

echo ""
echo "=========================================="
echo "Meilisearch initialization complete!"
echo "Created 6 indexes: accounts, contacts, opportunities, products, knowledge_articles, leads"
echo "=========================================="
