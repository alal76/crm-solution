#!/bin/bash

# VS Code Git Configuration Setup Script
# Configures VS Code to use the system git installation

set -e  # Exit on error

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo -e "${BLUE}======================================${NC}"
echo -e "${BLUE}VS Code Git Configuration Setup${NC}"
echo -e "${BLUE}======================================${NC}"
echo ""

# Detect OS
if [[ "$OSTYPE" == "darwin"* ]]; then
    VSCODE_SETTINGS="$HOME/Library/Application Support/Code/User/settings.json"
    OS="macOS"
elif [[ "$OSTYPE" == "linux-gnu"* ]]; then
    VSCODE_SETTINGS="$HOME/.config/Code/User/settings.json"
    OS="Linux"
else
    echo -e "${RED}❌ Unsupported OS: $OSTYPE${NC}"
    exit 1
fi

echo -e "${YELLOW}Detected OS: $OS${NC}"
echo -e "${YELLOW}VS Code Settings File: $VSCODE_SETTINGS${NC}"
echo ""

# Find git executable
GIT_PATH=$(which git)
if [ -z "$GIT_PATH" ]; then
    echo -e "${RED}❌ Git not found in PATH${NC}"
    exit 1
fi

echo -e "${GREEN}✓ Git found at: $GIT_PATH${NC}"
echo -e "${GREEN}✓ Git version: $(git --version)${NC}"
echo ""

# Create settings directory if it doesn't exist
SETTINGS_DIR=$(dirname "$VSCODE_SETTINGS")
if [ ! -d "$SETTINGS_DIR" ]; then
    echo -e "${YELLOW}Creating VS Code settings directory...${NC}"
    mkdir -p "$SETTINGS_DIR"
fi

# Backup existing settings.json if it exists
if [ -f "$VSCODE_SETTINGS" ]; then
    BACKUP_FILE="$VSCODE_SETTINGS.backup.$(date +%s)"
    echo -e "${YELLOW}Backing up existing settings.json...${NC}"
    cp "$VSCODE_SETTINGS" "$BACKUP_FILE"
    echo -e "${GREEN}✓ Backup saved to: $BACKUP_FILE${NC}"
    echo ""
fi

# Create or update settings.json
echo -e "${YELLOW}Updating VS Code settings...${NC}"

# Use Python to safely update JSON (handles both existing and new files)
python3 << EOF
import json
import os

settings_file = r"$VSCODE_SETTINGS"
git_path = r"$GIT_PATH"

# Load existing settings or create empty dict
if os.path.exists(settings_file):
    with open(settings_file, 'r') as f:
        try:
            settings = json.load(f)
        except json.JSONDecodeError:
            print("Warning: Could not parse existing settings.json, starting fresh")
            settings = {}
else:
    settings = {}

# Update git.path setting
settings["git.path"] = git_path

# Write updated settings
with open(settings_file, 'w') as f:
    json.dump(settings, f, indent=2)

print(f"✓ git.path set to: {git_path}")
EOF

echo ""
echo -e "${GREEN}======================================${NC}"
echo -e "${GREEN}✓ Setup Complete!${NC}"
echo -e "${GREEN}======================================${NC}"
echo ""
echo -e "${YELLOW}Next steps:${NC}"
echo "1. Reload VS Code: Cmd+Shift+P → 'Developer: Reload Window'"
echo "2. Check Source Control panel (Ctrl+Shift+G)"
echo "3. Your git changes should now appear"
echo ""
echo -e "${YELLOW}Verification:${NC}"
echo -e "git.path = $(grep -o '"git.path": "[^"]*"' "$VSCODE_SETTINGS" || echo 'not set')"
