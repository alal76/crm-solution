#!/usr/bin/env python3
"""Find S1854 (dead stores) and S1481 (unused local variables) in C# source files."""

import os
import re
import sys

src_dir = os.path.join(os.path.dirname(os.path.dirname(os.path.abspath(__file__))), 'CRM.Backend', 'src')

# Collect all .cs files
files = []
for root, dirs, fnames in os.walk(src_dir):
    dirs[:] = [d for d in dirs if d not in ('obj', 'bin', 'Migrations')]
    for f in fnames:
        if f.endswith('.cs'):
            files.append(os.path.join(root, f))

findings = []

def extract_method_blocks(lines):
    """Extract rough method blocks based on brace counting."""
    blocks = []
    depth = 0
    start = None
    for i, line in enumerate(lines):
        for ch in line:
            if ch == '{':
                if depth == 1 and start is None:
                    start = i
                depth += 1
            elif ch == '}':
                depth -= 1
                if depth == 1 and start is not None:
                    blocks.append((start, i))
                    start = None
    return blocks

def find_dead_stores_in_block(lines, start, end, filepath):
    """Find variables assigned but never used after assignment within a code block."""
    results = []
    # Find all local variable declarations
    var_pattern = re.compile(r'^\s*(?:var|int|string|bool|double|float|long|decimal|DateTime|List<[^>]+>|Dictionary<[^>]+>|IEnumerable<[^>]+>|IList<[^>]+>)\s+(\w+)\s*=\s*(.+);')
    
    for i in range(start, end + 1):
        line = lines[i]
        stripped = line.strip()
        
        # Skip comments
        if stripped.startswith('//') or stripped.startswith('/*') or stripped.startswith('*'):
            continue
        
        # Skip using var (disposal pattern)
        if 'using var' in stripped or 'using (' in stripped:
            continue
            
        # Skip discards
        if stripped.startswith('_ ='):
            continue
        
        m = var_pattern.match(stripped)
        if not m:
            continue
        
        varname = m.group(1)
        
        # Check if variable is used in the remaining lines of the block
        used = False
        reassigned_before_use = False
        
        for j in range(i + 1, end + 1):
            check_line = lines[j].strip()
            
            # Skip comments
            if check_line.startswith('//') or check_line.startswith('/*') or check_line.startswith('*'):
                continue
            
            # Check if variable appears in this line
            # Use word boundary to avoid partial matches
            if re.search(r'\b' + re.escape(varname) + r'\b', check_line):
                # Check if this is a pure reassignment (varname = ...) vs usage
                reassign_match = re.match(r'^' + re.escape(varname) + r'\s*=\s*.+;$', check_line)
                if reassign_match:
                    # It's a reassignment before any use - the first assignment is dead
                    reassigned_before_use = True
                    break
                else:
                    used = True
                    break
        
        if not used and not reassigned_before_use:
            # Variable declared but never used after declaration
            results.append((filepath, i + 1, varname, 'S1481', f'Unused local variable: "{varname}" is declared but never referenced'))
        elif reassigned_before_use:
            results.append((filepath, i + 1, varname, 'S1854', f'Dead store: "{varname}" assigned a value that is overwritten before being read'))
    
    return results

for filepath in sorted(files):
    with open(filepath, 'r', errors='replace') as fh:
        content = fh.read()
    lines = content.split('\n')
    
    # Simple approach: scan through the file for variable declarations
    # and check if they are used within a reasonable scope
    
    # Track brace depth to find method-level scope
    brace_depth = 0
    method_start = None
    in_class = False
    
    i = 0
    while i < len(lines):
        stripped = lines[i].strip()
        
        # Skip comments
        if stripped.startswith('//') or stripped.startswith('/*'):
            i += 1
            continue
        
        # Count braces
        open_braces = stripped.count('{')
        close_braces = stripped.count('}')
        
        # Detect method starts (depth transitions from class-level to method-level)
        old_depth = brace_depth
        brace_depth += open_braces - close_braces
        
        if old_depth == 2 and open_braces > 0:
            # Likely entering a method body
            method_start = i
        
        if brace_depth < old_depth and method_start is not None and brace_depth <= 2:
            # Method ended, analyze it
            method_end = i
            found = find_dead_stores_in_block(lines, method_start, method_end, filepath)
            findings.extend(found)
            method_start = None
        
        i += 1

# Print findings
s1854_count = 0
s1481_count = 0
for filepath, line, varname, rule, msg in sorted(findings):
    rel_path = os.path.relpath(filepath, os.path.dirname(src_dir))
    print(f'{rel_path}:{line}: [{rule}] {msg}')
    if rule == 'S1854':
        s1854_count += 1
    else:
        s1481_count += 1

print(f'\nTotal: {len(findings)} issues ({s1854_count} S1854 dead stores, {s1481_count} S1481 unused variables)')
print(f'Scanned {len(files)} files')
