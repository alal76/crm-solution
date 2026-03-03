#!/usr/bin/env python3
"""
Analyze SonarQube code duplication and generate refactoring recommendations.
"""
import re
import os
from pathlib import Path
from collections import defaultdict
from typing import Dict, List, Tuple

def find_controller_patterns(base_path: Path) -> Dict[str, List[str]]:
    """Find common patterns in controllers."""
    patterns = defaultdict(list)
    
    controller_dir = base_path / "CRM.Backend/src/CRM.Api/Controllers"
    if not controller_dir.exists():
        print(f"Controller directory not found: {controller_dir}")
        return patterns
    
    for cs_file in controller_dir.rglob("*Controller.cs"):
        content = cs_file.read_text(encoding='utf-8')
        
        # Pattern 1: Try-catch with 500 status code
        if re.search(r'catch.*\n.*StatusCode\(500', content, re.MULTILINE):
            patterns['try_catch_500'].append(str(cs_file.relative_to(base_path)))
        
        # Pattern 2: NotFound with message object
        if re.search(r'NotFound\(new\s*{\s*message\s*=', content):
            patterns['notfound_message_object'].append(str(cs_file.relative_to(base_path)))
        
        # Pattern 3: GetAll with pagination
        if re.search(r'GetAll.*\[FromQuery\].*page.*pageSize', content, re.DOTALL):
            patterns['getall_pagination'].append(str(cs_file.relative_to(base_path)))
        
        # Pattern 4: CreatedAtAction pattern
        if re.search(r'CreatedAtAction\(nameof\(Get.*ById\)', content):
            patterns['created_at_action'].append(str(cs_file.relative_to(base_path)))
        
        # Pattern 5: ETag handling
        if 'ETagHelper' in content and 'If-None-Match' in content:
            patterns['etag_handling'].append(str(cs_file.relative_to(base_path)))
        
        # Pattern 6: ModelState validation
        if 'if (!ModelState.IsValid)' in content and 'return BadRequest(ModelState)' in content:
            patterns['modelstate_validation'].append(str(cs_file.relative_to(base_path)))
    
    return patterns

def find_test_patterns(base_path: Path) -> Dict[str, List[str]]:
    """Find common patterns in test files."""
    patterns = defaultdict(list)
    
    test_dir = base_path / "CRM.Backend/tests"
    if not test_dir.exists():
        print(f"Test directory not found: {test_dir}")
        return patterns
    
    for cs_file in test_dir.rglob("*Tests.cs"):
        content = cs_file.read_text(encoding='utf-8')
        
        # Pattern 1: Mock<ICrmDbContext>
        if 'Mock<ICrmDbContext>' in content and 'Mock<ILogger<' in content:
            patterns['mock_context_logger'].append(str(cs_file.relative_to(base_path)))
        
        # Pattern 2: SetupDbSet pattern
        if re.search(r'\.Setup\(.*\.Set<.*>\(\)\)', content):
            patterns['setup_dbset'].append(str(cs_file.relative_to(base_path)))
        
        # Pattern 3: Arrange-Act-Assert comments
        if '// Arrange' in content and '// Act' in content and '// Assert' in content:
            patterns['aaa_pattern'].append(str(cs_file.relative_to(base_path)))
    
    return patterns

def find_dto_patterns(base_path: Path) -> Dict[str, List[Tuple[str, int]]]:
    """Find DTOs with similar property patterns."""
    patterns = defaultdict(list)
    
    dto_dir = base_path / "CRM.Backend/src/CRM.Core/DTOs"
    if not dto_dir.exists():
        print(f"DTO directory not found: {dto_dir}")
        return patterns
    
    for cs_file in dto_dir.rglob("*.cs"):
        content = cs_file.read_text(encoding='utf-8')
        
        # Count common property patterns
        prop_count = len(re.findall(r'public\s+\w+\???\s+\w+\s*{\s*get;\s*set;\s*}', content))
        if prop_count > 5:
            patterns['simple_properties'].append((str(cs_file.relative_to(base_path)), prop_count))
        
        # Audit fields pattern
        if all(field in content for field in ['CreatedAt', 'UpdatedAt', 'CreatedBy']):
            patterns['audit_fields'].append((str(cs_file.relative_to(base_path)), 1))
    
    return patterns

def analyze_codebase(base_path: Path):
    """Main analysis function."""
    print("=== SonarQube Code Duplication Analysis ===\n")
    
    print("1. Controller Patterns:")
    controller_patterns = find_controller_patterns(base_path)
    for pattern_name, files in sorted(controller_patterns.items()):
        print(f"\n   {pattern_name}: {len(files)} occurrences")
        if len(files) > 3:
            print(f"      - {files[0]}")
            print(f"      - {files[1]}")
            print(f"      - ... and {len(files) - 2} more")
    
    print("\n\n2. Test Patterns:")
    test_patterns = find_test_patterns(base_path)
    for pattern_name, files in sorted(test_patterns.items()):
        print(f"\n   {pattern_name}: {len(files)} occurrences")
        if len(files) > 3:
            print(f"      - {files[0]}")
            print(f"      - {files[1]}")
            print(f"      - ... and {len(files) - 2} more")
    
    print("\n\n3. DTO Patterns:")
    dto_patterns = find_dto_patterns(base_path)
    for pattern_name, files in sorted(dto_patterns.items()):
        print(f"\n   {pattern_name}: {len(files)} occurrences")
        if len(files) > 3:
            print(f"      - {files[0][0]} ({files[0][1]} properties)")
            print(f"      - {files[1][0]} ({files[1][1]} properties)")
            print(f"      - ... and {len(files) - 2} more")
    
    print("\n\n=== REFACTORING RECOMMENDATIONS ===\n")
    
    # Actionable recommendations
    recommendations = []
    
    if len(controller_patterns.get('try_catch_500', [])) > 5:
        recommendations.append({
            'priority': 'HIGH',
            'pattern': 'Exception handling with 500 status',
            'count': len(controller_patterns['try_catch_500']),
            'solution': 'Create ExceptionFilterAttribute or middleware for global exception handling',
            'impact': f'~{len(controller_patterns["try_catch_500"]) * 8} lines saved'
        })
    
    if len(controller_patterns.get('modelstate_validation', [])) > 5:
        recommendations.append({
            'priority': 'HIGH',
            'pattern': 'ModelState validation',
            'count': len(controller_patterns['modelstate_validation']),
            'solution': 'Use [ApiController] attribute which auto-validates ModelState',
            'impact': f'~{len(controller_patterns["modelstate_validation"]) * 4} lines saved'
        })
    
    if len(controller_patterns.get('etag_handling', [])) > 2:
        recommendations.append({
            'priority': 'MEDIUM',
            'pattern': 'ETag handling logic',
            'count': len(controller_patterns['etag_handling']),
            'solution': 'Create ETagActionFilter attribute for reusable ETag logic',
            'impact': f'~{len(controller_patterns["etag_handling"]) * 12} lines saved'
        })
    
    if len(test_patterns.get('mock_context_logger', [])) > 10:
        recommendations.append({
            'priority': 'MEDIUM',
            'pattern': 'Test mock setup (DbContext + Logger)',
            'count': len(test_patterns['mock_context_logger']),
            'solution': 'Create TestFixtureBase<T> with common mock setup',
            'impact': f'~{len(test_patterns["mock_context_logger"]) * 5} lines saved'
        })
    
    # Print recommendations
    for i, rec in enumerate(recommendations, 1):
        print(f"{i}. [{rec['priority']}] {rec['pattern']}")
        print(f"   Occurrences: {rec['count']}")
        print(f"   Solution: {rec['solution']}")
        print(f"   Impact: {rec['impact']}\n")
    
    return recommendations

if __name__ == '__main__':
    base_path = Path(__file__).parent
    recommendations = analyze_codebase(base_path)
    
    print(f"\n=== SUMMARY ===")
    print(f"Total HIGH priority items: {sum(1 for r in recommendations if r['priority'] == 'HIGH')}")
    print(f"Total MEDIUM priority items: {sum(1 for r in recommendations if r['priority'] == 'MEDIUM')}")
    print(f"\nEstimated total duplication reduction: ~{sum(int(r['impact'].split('~')[1].split()[0]) for r in recommendations if '~' in r['impact'])} lines")
