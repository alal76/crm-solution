#!/usr/bin/env python3
"""
DTO Test Generator - Scans DTOs for DataAnnotations and generates parameterized tests.

Usage:
    python3 scripts/generate_dto_tests.py --namespace CRM.Core.DTOs --output tests/Dtos/Generated/
"""

import os
import re
import argparse
from pathlib import Path

def find_dto_files(src_path, pattern="*Dto.cs"):
    """Find all DTO files in the source directory."""
    dto_files = list(Path(src_path).rglob(pattern))
    return [str(f) for f in dto_files if not any(x in str(f) for x in ['obj', 'bin', 'Tests'])]

def parse_dto_class(file_path):
    """Extract DTO class information including properties and validation attributes."""
    with open(file_path, 'r', encoding='utf-8') as f:
        content = f.read()
    
    # Extract namespace
    namespace_match = re.search(r'namespace\s+([\w.]+)', content)
    namespace = namespace_match.group(1) if namespace_match else "Unknown"
    
    # Extract class name
    class_match = re.search(r'public\s+(?:class|record)\s+(\w+)', content)
    if not class_match:
        return None
    class_name = class_match.group(1)
    
    # Extract properties with validation attributes
    properties = []
    property_pattern = r'\[(?P<attrs>.*?)\]\s*public\s+(?P<type>[\w<>?]+)\s+(?P<name>\w+)\s*\{'
    
    for match in re.finditer(property_pattern, content, re.MULTILINE | re.DOTALL):
        attrs = match.group('attrs')
        prop_type = match.group('type')
        prop_name = match.group('name')
        
        validations = {
            'required': 'Required' in attrs,
            'min_length': re.search(r'MinLength\((\d+)\)', attrs),
            'max_length': re.search(r'MaxLength\((\d+)\)', attrs),
            'string_length': re.search(r'StringLength\((\d+)(?:,\s*MinimumLength\s*=\s*(\d+))?\)', attrs),
            'range': re.search(r'Range\(([^,)]+),\s*([^)]+)\)', attrs),
            'email': 'EmailAddress' in attrs,
            'phone': 'Phone' in attrs,
            'url': 'Url' in attrs,
            'regex': re.search(r'RegularExpression\("([^"]+)"\)', attrs)
        }
        
        properties.append({
            'name': prop_name,
            'type': prop_type,
            'validations': validations
        })
    
    return {
        'namespace': namespace,
        'class_name': class_name,
        'properties': properties,
        'file_path': file_path
    }

def generate_test_class(dto_info):
    """Generate xUnit test class for a DTO."""
    class_name = dto_info['class_name']
    namespace = dto_info['namespace']
    properties = dto_info['properties']
    
    test_code = f"""using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CRM.Tests.Helpers;
using Xunit;
using {namespace};

namespace CRM.Tests.Dtos.Generated
{{
    /// <summary>
    /// Auto-generated validation tests for {class_name}.
    /// Generated from: {dto_info['file_path']}
    /// </summary>
    public class {class_name}ValidationTests : ValidatorTestFixtureBase<object>
    {{
        protected override object CreateValidator() => new object(); // DTOs use DataAnnotations, no custom validator

        // Helper: Create valid instance
        private {class_name} CreateValid{class_name}()
        {{
            return new {class_name}
            {{
                // TODO: Set all required properties to valid values
"""
    
    # Generate valid default values
    for prop in properties:
        prop_name = prop['name']
        prop_type = prop['type']
        validations = prop['validations']
        
        if validations['required']:
            if 'string' in prop_type.lower():
                if validations['email']:
                    test_code += f"                {prop_name} = \"test@example.com\",\n"
                elif validations['phone']:
                    test_code += f"                {prop_name} = \"+1234567890\",\n"
                elif validations['url']:
                    test_code += f"                {prop_name} = \"https://example.com\",\n"
                else:
                    test_code += f"                {prop_name} = \"Valid {prop_name}\",\n"
            elif 'int' in prop_type.lower():
                test_code += f"                {prop_name} = 1,\n"
            elif 'decimal' in prop_type.lower() or 'double' in prop_type.lower():
                test_code += f"                {prop_name} = 1.0m,\n"
            elif 'bool' in prop_type.lower():
                test_code += f"                {prop_name} = true,\n"
    
    test_code += """            };
        }

"""
    
    # Generate test methods for each validated property
    for prop in properties:
        prop_name = prop['name']
        prop_type = prop['type']
        validations = prop['validations']
        
        # Required validation test
        if validations['required']:
            test_code += f"""        [Theory]
        [InlineData(null, false)]
        [InlineData("", false)]
        public void {prop_name}_WithInvalidValue_FailsValidation(string value, bool shouldBeValid)
        {{
            // Arrange
            var dto = CreateValid{class_name}();
            dto.{prop_name} = value;
            
            // Act
            var results = ValidateModel(dto);
            
            // Assert
            if (shouldBeValid)
                Assert.Empty(results);
            else
                Assert.NotEmpty(results);
        }}

"""
        
        # String length validation test
        if validations['string_length']:
            match = validations['string_length']
            max_len = match.group(1)
            min_len = match.group(2) if match.group(2) else "0"
            test_code += f"""        [Theory]
        [InlineData("", {min_len} == 0)] // Empty (valid if min=0)
        [InlineData("{"x" * int(min_len)}", true)]  // Min length
        [InlineData("{"x" * (int(max_len) - 1)}", true)]  // Within range
        [InlineData("{"x" * int(max_len)}", true)]  // Max length
        [InlineData("{"x" * (int(max_len) + 1)}", false)] // Too long
        public void {prop_name}_WithVariousLengths_ValidatesCorrectly(string value, bool shouldBeValid)
        {{
            // Arrange
            var dto = CreateValid{class_name}();
            dto.{prop_name} = value;
            
            // Act
            var results = ValidateModel(dto);
            
            // Assert
            Assert.Equal(shouldBeValid, !results.Any());
        }}

"""
        
        # Range validation test
        if validations['range']:
            match = validations['range']
            min_val = match.group(1)
            max_val = match.group(2)
            test_code += f"""        [Theory]
        [InlineData({min_val} - 1, false)] // Below min
        [InlineData({min_val}, true)]      // Min
        [InlineData({max_val}, true)]      // Max
        [InlineData({max_val} + 1, false)] // Above max
        public void {prop_name}_WithVariousValues_ValidatesCorrectly(int value, bool shouldBeValid)
        {{
            // Arrange
            var dto = CreateValid{class_name}();
            dto.{prop_name} = value;
            
            // Act
            var results = ValidateModel(dto);
            
            // Assert
            Assert.Equal(shouldBeValid, !results.Any());
        }}

"""
        
        # Email validation test
        if validations['email']:
            test_code += f"""        [Theory]
        [InlineData("valid@example.com", true)]
        [InlineData("invalid-email", false)]
        [InlineData("missing@", false)]
        [InlineData("@missing.com", false)]
        public void {prop_name}_WithVariousEmails_ValidatesCorrectly(string value, bool shouldBeValid)
        {{
            // Arrange
            var dto = CreateValid{class_name}();
            dto.{prop_name} = value;
            
            // Act
            var results = ValidateModel(dto);
            
            // Assert
            Assert.Equal(shouldBeValid, !results.Any());
        }}

"""
    
    test_code += """    }
}
"""
    
    return test_code

def main():
    parser = argparse.ArgumentParser(description='Generate DTO validation tests')
    parser.add_argument('--src', default='src/CRM.Core', help='Source directory to scan for DTOs')
    parser.add_argument('--output', default='tests/Dtos/Generated', help='Output directory for generated tests')
    parser.add_argument('--pattern', default='*Dto.cs', help='File pattern to match')
    args = parser.parse_args()
    
    # Find DTOs
    dto_files = find_dto_files(args.src, args.pattern)
    print(f"Found {len(dto_files)} DTO files")
    
    # Create output directory
    output_dir = Path(args.output)
    output_dir.mkdir(parents=True, exist_ok=True)
    
    generated = 0
    skipped = 0
    
    for dto_file in dto_files:
        dto_info = parse_dto_class(dto_file)
        if not dto_info or not dto_info['properties']:
            skipped += 1
            continue
        
        # Generate test class
        test_code = generate_test_class(dto_info)
        
        # Write to file
        output_file = output_dir / f"{dto_info['class_name']}ValidationTests.cs"
        with open(output_file, 'w', encoding='utf-8') as f:
            f.write(test_code)
        
        print(f"✅ Generated: {output_file.name}")
        generated += 1
    
    print(f"\n=== Summary ===")
    print(f"Generated: {generated} test files")
    print(f"Skipped: {skipped} files (no properties or not a DTO)")
    print(f"Output: {output_dir}")

if __name__ == '__main__':
    main()
