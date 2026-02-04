#!/bin/bash
# Add AGPL-3.0 file header to C# files missing it

HEADER="// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.
"

cd /Users/alal/Code/Git\ CRM\ Solution/crm-solution/CRM.Backend

# Find all .cs files that don't start with "// CRM Solution"
find src tests -name "*.cs" -type f | while read -r file; do
    if ! head -n 1 "$file" | grep -q "^// CRM Solution"; then
        echo "Adding header to: $file"
        # Create temp file with header + original content
        echo "$HEADER" > "$file.tmp"
        cat "$file" >> "$file.tmp"
        mv "$file.tmp" "$file"
    fi
done

echo "Done adding file headers!"
