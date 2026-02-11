#!/usr/bin/env python3
"""Fix file headers across all .cs files to match StyleCop settings."""

import os
import re

CORRECT_HEADER = """// CRM Solution - Customer Relationship Management System
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
"""

HEADER_LINES = CORRECT_HEADER.strip().split('\n')

def has_correct_header(content):
    """Check if file already has the correct header."""
    lines = content.split('\n')
    if len(lines) < len(HEADER_LINES):
        return False
    for i, header_line in enumerate(HEADER_LINES):
        if lines[i].rstrip() != header_line.rstrip():
            return False
    return True

def strip_existing_header(content):
    """Remove existing file header comment block from top of file."""
    lines = content.split('\n')
    i = 0
    # Skip BOM if present
    if lines and lines[0].startswith('\ufeff'):
        lines[0] = lines[0][1:]

    # Skip leading blank lines
    while i < len(lines) and lines[i].strip() == '':
        i += 1

    # Check if there's a comment block at the top
    if i < len(lines) and lines[i].strip().startswith('//'):
        # Consume all leading // comment lines (the old header)
        while i < len(lines) and (lines[i].strip().startswith('//') or lines[i].strip() == ''):
            # Stop if we hit a using statement or namespace or other code
            next_non_empty = None
            for j in range(i + 1, min(i + 3, len(lines))):
                stripped = lines[j].strip()
                if stripped and not stripped.startswith('//'):
                    next_non_empty = stripped
                    break

            if lines[i].strip() == '' and next_non_empty and (
                next_non_empty.startswith('using ') or
                next_non_empty.startswith('namespace ') or
                next_non_empty.startswith('[') or
                next_non_empty.startswith('#') or
                next_non_empty.startswith('global ')
            ):
                break
            if lines[i].strip().startswith('//') or lines[i].strip() == '':
                i += 1
            else:
                break
        # Skip any blank lines after the header
        while i < len(lines) and lines[i].strip() == '':
            i += 1
        return '\n'.join(lines[i:])
    return content

def fix_file(filepath):
    """Fix the header of a single file."""
    try:
        with open(filepath, 'r', encoding='utf-8-sig') as f:
            content = f.read()
    except Exception as e:
        print(f"  ERROR reading {filepath}: {e}")
        return False

    if not content.strip():
        return False

    if has_correct_header(content):
        return False

    # Strip existing header
    body = strip_existing_header(content)

    # Add correct header
    new_content = CORRECT_HEADER + '\n' + body

    # Ensure file ends with newline
    if not new_content.endswith('\n'):
        new_content += '\n'

    try:
        with open(filepath, 'w', encoding='utf-8') as f:
            f.write(new_content)
        return True
    except Exception as e:
        print(f"  ERROR writing {filepath}: {e}")
        return False

def main():
    base_dir = os.path.join(os.path.dirname(os.path.dirname(os.path.abspath(__file__))), 'CRM.Backend')
    fixed = 0
    skipped = 0
    errors = 0

    for root, dirs, files in os.walk(base_dir):
        # Skip bin/obj directories
        dirs[:] = [d for d in dirs if d not in ('bin', 'obj', 'publish')]
        for f in files:
            if f.endswith('.cs'):
                filepath = os.path.join(root, f)
                result = fix_file(filepath)
                if result:
                    fixed += 1
                elif result is False:
                    skipped += 1
                else:
                    errors += 1

    print(f"Header fix complete: {fixed} files fixed, {skipped} files already correct/skipped, {errors} errors")

if __name__ == '__main__':
    main()
