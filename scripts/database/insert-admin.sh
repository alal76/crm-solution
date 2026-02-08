#!/bin/bash

# Calculate SHA256 hash
HASH=$(echo -n "Microsoft@1" | sha256sum | awk '{print $1}' | xxd -r -p | base64)

echo "Hash: $HASH"

# Insert admin user
mariadb -u crm_user -pCrmPass@2024 crm_db <<EOF
INSERT INTO Users (Username, Email, FirstName, LastName, PasswordHash, Role, IsActive, IsDeleted, CreatedAt, UpdatedAt, EmailVerified)
VALUES ('admin', 'admin@crm.local', 'Admin', 'User', '${HASH}', 1, 1, 0, NOW(), NOW(), 1);
EOF
