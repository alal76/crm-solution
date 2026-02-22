#!/usr/bin/env python3
"""Extract all API endpoints from CRM.Backend controller files."""
import re
import os
import sys

CONTROLLERS_DIR = os.path.join(
    os.path.dirname(os.path.dirname(os.path.abspath(__file__))),
    "CRM.Backend", "src", "CRM.Api", "Controllers"
)

def extract_endpoints():
    results = []
    
    for fname in sorted(os.listdir(CONTROLLERS_DIR)):
        if not fname.endswith('.cs'):
            continue
        
        filepath = os.path.join(CONTROLLERS_DIR, fname)
        with open(filepath) as f:
            content = f.read()
        
        # Find all class-level routes
        route_matches = re.findall(r'\[Route\("(.*?)"\)\]', content)
        
        # Find all class names (there may be multiple controllers in one file)
        class_matches = list(re.finditer(
            r'\[Route\("(.*?)"\)\]\s*(?:\[.*?\]\s*)*public\s+class\s+(\w+)',
            content, re.DOTALL
        ))
        
        if not class_matches:
            # Try simpler match
            class_match = re.search(r'public\s+class\s+(\w+)\s*:', content)
            if class_match:
                route = route_matches[0] if route_matches else 'UNKNOWN'
                class_name = class_match.group(1)
                process_class(content, fname, class_name, route, results)
            continue
        
        for cm in class_matches:
            route = cm.group(1)
            class_name = cm.group(2)
            
            # Get the content for this class (until next class or end)
            start = cm.start()
            # Find next class or end of file
            next_class = re.search(r'\npublic\s+class\s+', content[cm.end():])
            if next_class:
                end = cm.end() + next_class.start()
            else:
                end = len(content)
            
            class_content = content[start:end]
            process_class(class_content, fname, class_name, route, results)
    
    return results

def process_class(content, fname, class_name, route, results):
    """Extract HTTP methods from a class."""
    # Resolve [controller] in route
    controller_name = class_name.replace('Controller', '')
    resolved_route = route.replace('[controller]', controller_name.lower())
    if not resolved_route.startswith('api/'):
        resolved_route = 'api/' + resolved_route
    
    methods = []
    
    # Find all HTTP method attributes and the following method signature
    pattern = re.compile(
        r'\[(Http(Get|Post|Put|Patch|Delete))(?:\("(.*?)"\))?\]\s*'
        r'(?:\[.*?\]\s*)*'  # Skip other attributes
        r'public\s+(?:async\s+)?(?:Task<)?(?:ActionResult<)?(?:IActionResult|ActionResult|[\w<>,\s]+?)>?>?\s+'
        r'(\w+)\s*\(([^)]*)\)',
        re.DOTALL
    )
    
    for m in pattern.finditer(content):
        verb = m.group(2).upper()
        template = m.group(3) or ''
        action = m.group(4)
        params = m.group(5).strip()
        
        # Extract DTO from params
        dto = ''
        dto_match = re.search(r'\[FromBody\]\s+(\w+(?:<[^>]+>)?)', params)
        if dto_match:
            dto = dto_match.group(1)
        
        # Build full path
        if template:
            full_path = f"/{resolved_route}/{template}"
        else:
            full_path = f"/{resolved_route}"
        
        # Clean up double slashes
        full_path = re.sub(r'/+', '/', full_path)
        
        methods.append({
            'verb': verb,
            'template': template,
            'action': action,
            'dto': dto,
            'full_path': full_path
        })
    
    results.append({
        'file': fname,
        'class': class_name,
        'route': route,
        'resolved_route': resolved_route,
        'methods': methods
    })

def main():
    results = extract_endpoints()
    
    total_endpoints = 0
    
    for ctrl in results:
        if not ctrl['methods']:
            continue
        
        print(f"\n{'='*80}")
        print(f"CONTROLLER: {ctrl['class']}")
        print(f"FILE: {ctrl['file']}")
        print(f"ROUTE: /{ctrl['resolved_route']}")
        print(f"{'-'*80}")
        
        for m in ctrl['methods']:
            dto_str = f" ({m['dto']})" if m['dto'] else ""
            print(f"  {m['verb']:6s} {m['full_path']}{dto_str}")
            print(f"         -> {m['action']}()")
            total_endpoints += 1
    
    # Also print controllers with no detected methods
    empty = [c for c in results if not c['methods']]
    if empty:
        print(f"\n{'='*80}")
        print("CONTROLLERS WITH NO DETECTED ENDPOINTS (may have methods not caught by parser):")
        for ctrl in empty:
            print(f"  {ctrl['class']} ({ctrl['file']}) - route: {ctrl['route']}")
    
    print(f"\n{'='*80}")
    print(f"TOTAL: {len(results)} controllers, {total_endpoints} endpoints detected")
    print(f"{'='*80}")

if __name__ == '__main__':
    main()
