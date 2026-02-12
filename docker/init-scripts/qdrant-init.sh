#!/bin/bash
# CRM Solution - Customer Relationship Management System
# Copyright (C) 2024-2026 Abhishek Lal
#
# This program is free software: you can redistribute it and/or modify
# it under the terms of the GNU Affero General Public License as published by
# the Free Software Foundation, either version 3 of the License, or
# (at your option) any later version.
#
# This program is distributed in the hope that it will be useful,
# but WITHOUT ANY WARRANTY; without even the implied warranty of
# MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
# GNU Affero General Public License for more details.
#
# You should have received a copy of the GNU Affero General Public License
# along with this program. If not, see <https://www.gnu.org/licenses/>.

# Initialize Qdrant vector collections for CRM AI agents
QDRANT_URL=${QDRANT_URL:-http://crm-qdrant:6333}

echo "Initializing Qdrant collections at ${QDRANT_URL}..."

for COLLECTION in crm-accounts crm-contacts crm-kb-articles crm-emails crm-conversations crm-agent-memory; do
    echo "Creating collection: ${COLLECTION}"
    curl -s -X PUT "${QDRANT_URL}/collections/${COLLECTION}" \
        -H "Content-Type: application/json" \
        -d '{
            "vectors": {
                "size": 1536,
                "distance": "Cosine"
            },
            "optimizers_config": {
                "indexing_threshold": 20000
            }
        }' || echo "Collection ${COLLECTION} may already exist"
done

echo "Qdrant initialization complete."
