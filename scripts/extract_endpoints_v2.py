#!/usr/bin/env python3
"""Extract all API endpoints from CRM.Backend controller files. Simplified regex."""
import re
import os
import sys

CONTROLLERS_DIR = sys.argv[1] if len(sys.argv) > 1 else "."

def main():
    results = []
    total_endpoints = 0
    
    for fname in sorted(os.listdir(CONTROLLERS_DIR)):
        if not fname.endswith('.cs'):
            continue
        
        filepath = os.path.join(CONTROLLERS_DIR, fname)
        with open(filepath) as f:
            lines = f.readlines()
        
        # Find routes and classes
        routes = []
        current_route = None
        current_class = None
        current_http = None
        
        for i, line in enumerate(lines):
            stripped = line.strip()
            
            # Detect Route attribute
            rm = re.search(r'\[Route\("(.+?)"\)\]', stripped)
            if rm:
                current_route = rm.group(1)
            
            # Detect class
            cm = re.search(r'public\s+class\s+(\w+Controller)', stripped)
            if cm:
                current_class = cm.group(1)
                controller_name = current_class.replace('Controller', '').lower()
                if current_route:
                    resolved = current_route.replace('[controller]', controller_name)
                else:
                    resolved = f"api/{controller_name}"
                results.append({
                    'class': current_class,
                    'file': fname,
                    'route': current_route or 'UNKNOWN',
                    'resolved': resolved,
                    'methods': []
                })
            
            # Detect HTTP verb
            hm = re.search(r'\[(Http(Get|Post|Put|Patch|Delete))(?:\("(.+?)"\))?\]', stripped)
            if hm:
                current_http = {
                    'verb': hm.group(2).upper(),
                    'template': hm.group(3) or ''
                }
                continue
            
            # Detect method signature after HTTP verb
            if current_http and results:
                mm = re.search(r'public\s+(?:async\s+)?(?:\S+\s+)?(\w+)\s*\((.*)$', stripped)
                if mm:
                    action = mm.group(1)
                    params_text = mm.group(2)
                    
                    # Look ahead for more params if line doesn't close
                    full_params = params_text
                    j = i + 1
                    while ')' not in full_params and j < len(lines) and j < i + 5:
                        full_params += ' ' + lines[j].strip()
                        j += 1
                    
                    dto = ''
                    dm = re.search(r'\[FromBody\]\s+(\w+)', full_params)
                    if dm:
                        dto = dm.group(1)
                    
                    tmpl = current_http['template']
                    base = results[-1]['resolved']
                    if tmpl:
                        full_path = f"/{base}/{tmpl}"
                    else:
                        full_path = f"/{base}"
                    full_path = re.sub(r'/+', '/', full_path)
                    
                    results[-1]['methods'].append({
                        'verb': current_http['verb'],
                        'template': tmpl,
                        'action': action,
                        'dto': dto,
                        'full_path': full_path
                    })
                    total_endpoints += 1
                    current_http = None
                elif not stripped.startswith('[') and not stripped.startswith('//') and stripped:
                    current_http = None
    
    # Output
    for ctrl in results:
        if not ctrl['methods']:
            continue
        print(f"\nCONTROLLER: {ctrl['class']}")
        print(f"FILE: {ctrl['file']}")
        print(f"ROUTE: /{ctrl['resolved']}")
        for m in ctrl['methods']:
            dto_str = f" ({m['dto']})" if m['dto'] else ""
            print(f"  {m['verb']:6s} {m['full_path']}{dto_str} -> {m['action']}()")
    
    # Empty controllers
    empty = [c for c in results if not c['methods']]
    if empty:
        print(f"\nNO ENDPOINTS DETECTED:")
        for ctrl in empty:
            print(f"  {ctrl['class']} ({ctrl['file']})")
    
    print(f"\nTOTAL: {len(results)} controllers, {total_endpoints} endpoints")

if __name__ == '__main__':
    main()
