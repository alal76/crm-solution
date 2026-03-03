#!/usr/bin/env python3
"""
Validator Test Generator - Creates test stubs for validator classes.

Usage:
    python3 scripts/generate_validator_tests.py --src src/CRM.Core/Validation --output tests/Validators/
"""

import os
import re
import argparse
from pathlib import Path

def find_validator_files(src_path):
    """Find all validator files in the source directory."""
    validator_files = list(Path(src_path).rglob("*Validator.cs"))
    return [str(f) for f in validator_files if not any(x in str(f) for x in ['obj', 'bin', 'Tests'])]

def parse_validator_class(file_path):
    """Extract validator class information including methods."""
    with open(file_path, 'r', encoding='utf-8') as f:
        content = f.read()
    
    # Extract namespace
    namespace_match = re.search(r'namespace\s+([\w.]+)', content)
    namespace = namespace_match.group(1) if namespace_match else "Unknown"
    
    # Extract class name
    class_match = re.search(r'public\s+(?:class|record)\s+(\w+Validator)', content)
    if not class_match:
        return None
    class_name = class_match.group(1)
    
    # Extract public methods
    method_pattern = r'public\s+(?:async\s+)?(?P<return>[\w<>?]+)\s+(?P<name>\w+)\s*\((?P<params>[^)]*)\)'
    methods = []
    
    for match in re.finditer(method_pattern, content):
        return_type = match.group('return')
        method_name = match.group('name')
        params = match.group('params')
        
        # Skip constructors and property accessors
        if method_name != class_name and method_name not in ['get_', 'set_']:
            methods.append({
                'name': method_name,
                'return_type': return_type,
                'params': params,
                'is_async': 'Task' in return_type
            })
    
    return {
        'namespace': namespace,
        'class_name': class_name,
        'methods': methods,
        'file_path': file_path
    }

def generate_test_class(validator_info):
    """Generate xUnit test class for a validator."""
    class_name = validator_info['class_name']
    namespace = validator_info['namespace']
    methods = validator_info['methods']
    
    test_code = f"""using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CRM.Tests.Helpers;
using Xunit;
using {namespace};

namespace CRM.Tests.Validators
{{
    /// <summary>
    /// Tests for {class_name}.
    /// Generated from: {validator_info['file_path']}
    /// TODO: Fill in test data and assertions.
    /// </summary>
    public class {class_name}Tests : ValidatorTestFixtureBase<{class_name}>
    {{
        protected override {class_name} CreateValidator()
        {{
            // TODO: Add any constructor dependencies
            return new {class_name}();
        }}

"""
    
    # Generate test methods for each validator method
    for method in methods:
        method_name = method['name']
        is_async = method['is_async']
        async_keyword = 'async ' if is_async else ''
        await_keyword = 'await ' if is_async else ''
        task_suffix = '()' if is_async else ''
        
        # Test: Valid input passes
        test_code += f"""        [Fact]
        public {async_keyword}Task {method_name}_WithValidInput_Passes{task_suffix}
        {{
            // Arrange
            // TODO: Create valid test data
            var validInput = "valid_test_data";
            
            // Act
            var result = {await_keyword}Validator.{method_name}(validInput);
            
            // Assert
            Assert.True(result.IsValid);
        }}

"""
        
        # Test: Invalid input fails
        test_code += f"""        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("invalid")]
        public {async_keyword}Task {method_name}_WithInvalidInput_Fails{task_suffix}(string invalidInput)
        {{
            // Arrange
            // Input provided by InlineData
            
            // Act
            var result = {await_keyword}Validator.{method_name}(invalidInput);
            
            // Assert
            Assert.False(result.IsValid);
            Assert.NotNull(result.ErrorMessage);
        }}

"""
        
        # Test: Boundary conditions
        test_code += f"""        [Theory]
        [InlineData("min_boundary_value", true)]
        [InlineData("max_boundary_value", true)]
        [InlineData("below_min", false)]
        [InlineData("above_max", false)]
        public {async_keyword}Task {method_name}_WithBoundaryValues_ValidatesCorrectly{task_suffix}(string input, bool shouldBeValid)
        {{
            // Arrange
            // TODO: Replace with actual boundary values
            
            // Act
            var result = {await_keyword}Validator.{method_name}(input);
            
            // Assert
            Assert.Equal(shouldBeValid, result.IsValid);
        }}

"""
    
    test_code += """    }
}
"""
    
    return test_code

def main():
    parser = argparse.ArgumentParser(description='Generate validator tests')
    parser.add_argument('--src', default='src/CRM.Core/Validation', help='Source directory to scan for validators')
    parser.add_argument('--output', default='tests/Validators', help='Output directory for generated tests')
    args = parser.parse_args()
    
    # Find validators
    validator_files = find_validator_files(args.src)
    print(f"Found {len(validator_files)} validator files")
    
    # Create output directory
    output_dir = Path(args.output)
    output_dir.mkdir(parents=True, exist_ok=True)
    
    generated = 0
    skipped = 0
    
    for validator_file in validator_files:
        validator_info = parse_validator_class(validator_file)
        if not validator_info or not validator_info['methods']:
            skipped += 1
            continue
        
        # Generate test class
        test_code = generate_test_class(validator_info)
        
        # Write to file
        output_file = output_dir / f"{validator_info['class_name']}Tests.cs"
        with open(output_file, 'w', encoding='utf-8') as f:
            f.write(test_code)
        
        print(f"✅ Generated: {output_file.name}")
        generated += 1
    
    print(f"\n=== Summary ===")
    print(f"Generated: {generated} test files")
    print(f"Skipped: {skipped} files (no methods or not a validator)")
    print(f"Output: {output_dir}")

if __name__ == '__main__':
    main()
