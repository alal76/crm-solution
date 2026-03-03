#!/usr/bin/env python3
"""
Automated refactoring tool to eliminate test code duplication by applying ServiceTestFixtureBase.
Targets 89 test files with Mock<ICrmDbContext> + Mock<ILogger<T>> pattern.
"""
import re
from pathlib import Path
from typing import List, Tuple

def refactor_test_file(file_path: Path) -> Tuple[bool, str]:
    """
    Refactor a single test file to use ServiceTestFixtureBase.
    Returns (success, message).
    """
    try:
        content = file_path.read_text(encoding='utf-8')
        original_content = content
        
        # Skip if already inherits from ServiceTestFixtureBase
        if ': ServiceTestFixtureBase<' in content:
            return False, f"SKIP: {file_path.name} - already refactored"
        
        # Pattern 1: Detect the service type (from constructor new ServiceName(...))
        service_match = re.search(r'new\s+(\w+Service)\s*\(', content)
        if not service_match:
            return False, f"SKIP: {file_path.name} - couldn't detect service type"
        
        service_type = service_match.group(1)
        
        # Pattern 2: Check if it has both Mock<ICrmDbContext> and Mock<ILogger<T>>
        has_mock_context = 'Mock<ICrmDbContext>' in content
        has_mock_logger = f'Mock<ILogger<{service_type}>>' in content
        
        if not (has_mock_context and has_mock_logger):
            return False, f"SKIP: {file_path.name} - doesn't match mock pattern"
        
        # Pattern 3: Find the test class declaration
        class_match = re.search(r'public class (\w+)\s*(?::\s*(\w+))?\s*\{', content)
        if not class_match:
            return False, f"SKIP: {file_path.name} - couldn't find class declaration"
        
        class_name = class_match.group(1)
        existing_base = class_match.group(2)
        
        # Don't touch classes that already inherit or use InMemory database
        if existing_base in ('IDisposable', 'IAsyncLifetime') or 'UseInMemoryDatabase' in content:
            return False, f"SKIP: {file_path.name} - has special base class or uses InMemory DB"
        
        # Start refactoring
        lines_saved = 0
        
        # Step 1: Update class declaration to inherit from ServiceTestFixtureBase
        old_class_decl = class_match.group(0)
        if existing_base:
            new_class_decl = f'public class {class_name} : ServiceTestFixtureBase<{service_type}>, {existing_base}\n{{'
        else:
            new_class_decl = f'public class {class_name} : ServiceTestFixtureBase<{service_type}>\n{{'
        content = content.replace(old_class_decl, new_class_decl)
        
        # Step 2: Remove private readonly Mock<ICrmDbContext> field
        context_field_pattern = r'\s*private readonly Mock<ICrmDbContext>\s+_mockContext;?\s*\n'
        if re.search(context_field_pattern, content):
            content = re.sub(context_field_pattern, '', content)
            lines_saved += 1
        
        # Step 3: Remove private readonly Mock<ILogger<T>> field
        logger_field_pattern = rf'\s*private readonly Mock<ILogger<{service_type}>>\s+_mockLogger;?\s*\n'
        if re.search(logger_field_pattern, content):
            content = re.sub(logger_field_pattern, '', content)
            lines_saved += 1
        
        # Step 4: Remove mock initialization from constructor
        # Pattern: _mockContext = new Mock<ICrmDbContext>();
        context_init_pattern = r'\s*_mockContext\s*=\s*new Mock<ICrmDbContext>\(\);?\s*\n'
        if re.search(context_init_pattern, content):
            content = re.sub(context_init_pattern, '', content)
            lines_saved += 1
        
        # Pattern: _mockLogger = new Mock<ILogger<T>>();
        logger_init_pattern = rf'\s*_mockLogger\s*=\s*new Mock<ILogger<{service_type}>>\(\);?\s*\n'
        if re.search(logger_init_pattern, content):
            content = re.sub(logger_init_pattern, '', content)
            lines_saved += 1
        
        # Step 5: Replace all _mockContext references with MockContext (inherited)
        content = content.replace('_mockContext', 'MockContext')
        
        # Step 6: Replace all _mockLogger references with MockLogger (inherited)
        content = content.replace('_mockLogger', 'MockLogger')
        
        # Only write if content changed
        if content != original_content:
            file_path.write_text(content, encoding='utf-8')
            return True, f"✅ {file_path.name} - saved ~{lines_saved} lines"
        else:
            return False, f"SKIP: {file_path.name} - no changes needed"
        
    except Exception as e:
        return False, f"ERROR: {file_path.name} - {str(e)}"

def main():
    base_path = Path(__file__).parent
    test_dir = base_path / "CRM.Backend/tests"
    
    if not test_dir.exists():
        print(f"Test directory not found: {test_dir}")
        return
    
    print("=== Automated Test Refactoring - ServiceTestFixtureBase ===\n")
    print("Scanning for test files with Mock<ICrmDbContext> + Mock<ILogger<T>> pattern...\n")
    
    test_files = list(test_dir.rglob("*Tests.cs"))
    refactored = []
    skipped = []
    errors = []
    total_lines_saved = 0
    
    for test_file in test_files:
        success, message = refactor_test_file(test_file)
        if success:
            refactored.append(message)
            # Extract lines saved
            match = re.search(r'~(\d+) lines', message)
            if match:
                total_lines_saved += int(match.group(1))
        elif message.startswith('ERROR'):
            errors.append(message)
        else:
            skipped.append(message)
    
    print(f"\n=== RESULTS ===")
    print(f"Total files scanned: {len(test_files)}")
    print(f"Successfully refactored: {len(refactored)}")
    print(f"Skipped: {len(skipped)}")
    print(f"Errors: {len(errors)}")
    print(f"Total lines eliminated: ~{total_lines_saved}")
    
    if refactored:
        print(f"\n✅ Refactored files:")
        for msg in refactored[:10]:
            print(f"   {msg}")
        if len(refactored) > 10:
            print(f"   ... and {len(refactored) - 10} more")
    
    if errors:
        print(f"\n❌ Errors:")
        for msg in errors[:5]:
            print(f"   {msg}")
    
    print(f"\n🎯 Duplication reduction: {total_lines_saved}/{total_lines_saved + len(skipped)*4} target (~{total_lines_saved * 100 // (total_lines_saved + len(skipped)*4 + 1)}%)")

if __name__ == '__main__':
    main()
