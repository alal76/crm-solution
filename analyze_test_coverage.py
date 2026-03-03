#!/usr/bin/env python3
"""
Test Coverage Analysis Script
Analyzes existing coverage reports and identifies areas for improvement
"""

import xml.etree.ElementTree as ET
import sys
from collections import defaultdict

def analyze_coverage(coverage_file):
    tree = ET.parse(coverage_file)
    root = tree.getroot()
    
    # Get overall stats
    coverage = root.attrib
    print("=" * 80)
    print("=== OVERALL COVERAGE SUMMARY ===")
    print("=" * 80)
    line_rate = float(coverage.get('line-rate', 0)) * 100
    branch_rate = float(coverage.get('branch-rate', 0)) * 100
    print(f"Line Coverage: {line_rate:.2f}%")
    print(f"Branch Coverage: {branch_rate:.2f}%")
    print(f"Lines Covered: {coverage.get('lines-covered', 'N/A')} / {coverage.get('lines-valid', 'N/A')}")
    print(f"Branches Covered: {coverage.get('branches-covered', 'N/A')} / {coverage.get('branches-valid', 'N/A')}")
    print()
    
    # Collect class coverage data
    classes = []
    category_stats = defaultdict(lambda: {'total': 0, 'covered': 0, 'files': []})
    
    for pkg in root.findall('.//package'):
        for cls in pkg.findall('.//class'):
            line_rate = float(cls.attrib.get('line-rate', 0))
            filename = cls.attrib.get('filename', 'Unknown')
            name = cls.attrib.get('name', 'Unknown')
            
            # Count lines
            lines_valid = 0
            lines_covered = 0
            for line in cls.findall('.//line'):
                lines_valid += 1
                if int(line.attrib.get('hits', 0)) > 0:
                    lines_covered += 1
            
            classes.append({
                'name': name,
                'filename': filename,
                'line_rate': line_rate,
                'lines_valid': lines_valid,
                'lines_covered': lines_covered
            })
            
            # Categorize by directory
            if 'Controllers' in filename:
                category = 'Controllers'
            elif 'Services' in filename:
                category = 'Services'
            elif 'Validation' in filename:
                category = 'Validators'
            elif 'Middleware' in filename:
                category = 'Middleware'
            elif 'Providers' in filename:
                category = 'Providers'
            else:
                category = 'Other'
            
            category_stats[category]['total'] += lines_valid
            category_stats[category]['covered'] += lines_covered
            category_stats[category]['files'].append(name.split('.')[-1])
    
    # Sort classes by coverage (lowest first)
    classes.sort(key=lambda x: x['line_rate'])
    
    # Print lowest coverage classes
    print("=" * 80)
    print("=== TOP 40 CLASSES WITH LOWEST COVERAGE ===")
    print("=" * 80)
    for i, cls in enumerate(classes[:40], 1):
        short_name = cls['name'].split('.')[-1]
        short_file = cls['filename'].split('/')[-1]
        print(f"{i:2}. {short_name:50} {cls['line_rate']*100:5.1f}% ({cls['lines_covered']:3}/{cls['lines_valid']:3}) - {short_file}")
    
    print()
    print("=" * 80)
    print("=== COVERAGE BY CATEGORY ===")
    print("=" * 80)
    for category, stats in sorted(category_stats.items(), key=lambda x: x[1]['covered']/max(x[1]['total'], 1)):
        if stats['total'] > 0:
            coverage_pct = (stats['covered'] / stats['total']) * 100
            print(f"{category:20} {coverage_pct:5.1f}% ({stats['covered']:4}/{stats['total']:4} lines) - {len(stats['files'])} files")
    
    print()
    print("=" * 80)
    print("=== UNCOVERED CLASSES (0% coverage) ===")
    print("=" * 80)
    uncovered = [cls for cls in classes if cls['line_rate'] == 0]
    print(f"Total: {len(uncovered)} classes with 0% coverage")
    for i, cls in enumerate(uncovered[:20], 1):
        short_name = cls['name'].split('.')[-1]
        print(f"{i:2}. {short_name:50} ({cls['lines_valid']:3} lines)")
    
    if len(uncovered) > 20:
        print(f"... and {len(uncovered) - 20} more")
    
    return {
        'line_rate': line_rate,
        'branch_rate': branch_rate,
        'total_classes': len(classes),
        'uncovered_classes': len(uncovered),
        'categories': dict(category_stats)
    }

if __name__ == '__main__':
    coverage_file = 'TestResults/99022631-0683-4945-b18c-e53323cc1f33/coverage.cobertura.xml'
    stats = analyze_coverage(coverage_file)
    
    print()
    print("=" * 80)
    print("=== RECOMMENDATIONS ===")
    print("=" * 80)
    print(f"1. Start with {stats['uncovered_classes']} classes that have 0% coverage")
    print(f"2. Focus on Controllers and Services (most business logic)")
    print(f"3. Target: Increase coverage from {stats['line_rate']:.1f}% to at least 60%")
    print(f"4. Add integration tests for critical workflows")
    print(f"5. Add parameterized tests for validation logic")
