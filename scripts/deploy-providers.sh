#!/bin/bash

# deploy-providers.sh
# Deploy CRM provider containers on remote server via SSH

REMOTE_HOST="192.168.0.9"
REMOTE_USER="root"
REMOTE_DIR="/opt/crm-solution/docker"
CONTAINERS=("n8n" "superset" "chatwoot" "novu" "docuseal")

echo "Starting provider deployment to $REMOTE_HOST..."

ssh -o BatchMode=yes -o ConnectTimeout=10 "${REMOTE_USER}@${REMOTE_HOST}" << 'EOF'
set -e

cd /opt/crm-solution/docker || { echo "Directory not found"; exit 1; }

echo "Pulling latest images..."
for c in n8n superset chatwoot novu docuseal; do
    case $c in
        n8n)
            IMAGE="n8nio/n8n:latest";;
        superset)
            IMAGE="apache/superset:latest";;
        chatwoot)
            IMAGE="chatwoot/chatwoot:latest";;
        novu)
            IMAGE="ghcr.io/novuhq/novu/api:latest";;
        docuseal)
            IMAGE="docuseal/docuseal:latest";;
        *)
            IMAGE="$c:latest";;
    esac
    if docker pull "$IMAGE"; then
        echo "Pulled $IMAGE successfully."
    else
        echo "Failed to pull $IMAGE."
        exit 1
    fi
done

echo "Starting containers..."
for c in n8n superset chatwoot novu docuseal; do
    if docker compose up -d "$c"; then
        echo "Started $c."
    else
        echo "Failed to start $c."
        exit 1
    fi
done

echo "Provider container status:"
docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}" | grep -E "n8n|superset|chatwoot|novu|docuseal"

echo "Provider deployment complete."
EOF

if [[ $? -eq 0 ]]; then
    echo "Deployment finished successfully."
else
    echo "Deployment failed. Check logs for details."
    exit 1
fi
