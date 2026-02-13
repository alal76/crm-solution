#!/bin/bash
# Deploys the latest frontend build to the crm-frontend container and restarts it
# Usage: ./scripts/deploy-frontend.sh

set -e

FRONTEND_BUILD_DIR="$(dirname "$0")/../CRM.Frontend/build"
CONTAINER_NAME="crm-frontend"
CONTAINER_PATH="/usr/share/nginx/html/"

# Absolute path resolution
BUILD_DIR_ABS=$(cd "$FRONTEND_BUILD_DIR" && pwd)

if [ ! -d "$BUILD_DIR_ABS" ]; then
  echo "Error: Build directory not found: $BUILD_DIR_ABS"
  exit 1
fi

echo "Copying frontend build from $BUILD_DIR_ABS to $CONTAINER_NAME:$CONTAINER_PATH ..."
docker cp "$BUILD_DIR_ABS/." "$CONTAINER_NAME:$CONTAINER_PATH"

echo "Restarting $CONTAINER_NAME ..."
docker restart "$CONTAINER_NAME"

echo "Frontend deployment complete."
