# ADR-001: Coding Standards Enforcement

## Status

Accepted

## Date

2026-02-02

## Context

The CRM Solution codebase had inconsistent code formatting and style across different parts of the application. This caused:

1. **Code review friction**: Reviews focused on style issues rather than logic
2. **Merge conflicts**: Different formatting preferences caused unnecessary conflicts
3. **Onboarding difficulty**: New developers had no clear style guide
4. **Technical debt**: Inconsistent patterns made code harder to maintain

The team needed automated tooling to enforce consistent coding standards across:
- Backend (.NET 8.0 / C#)
- Frontend (React / TypeScript)
- Configuration files (JSON, YAML)

## Decision

We will implement automated coding standards enforcement using:

### Backend (.NET)
- **StyleCop.Analyzers**: Enforces C# coding style at build time
- **EditorConfig**: Defines formatting rules across IDEs
- **Directory.Build.props**: Centralized project settings

### Frontend (React/TypeScript)
- **ESLint**: Static analysis and style enforcement
- **Prettier**: Automated code formatting
- **TypeScript strict mode**: Type safety enforcement

### Cross-Platform
- **EditorConfig**: Consistent formatting rules for all file types
- **CI/CD integration**: Fail builds on style violations

### Configuration Files Created
1. `.editorconfig` - Cross-platform formatting rules
2. `.prettierrc.json` - Prettier configuration
3. `.prettierignore` - Files to exclude from formatting
4. `Directory.Build.props` - .NET analyzer configuration
5. `stylecop.json` - StyleCop rules

## Consequences

### Positive
- **Consistent codebase**: All code follows the same style
- **Automated enforcement**: No manual style checks needed
- **Faster reviews**: Focus on logic, not formatting
- **Better onboarding**: Clear standards for new developers
- **Reduced conflicts**: Formatting handled automatically

### Negative
- **Initial setup effort**: One-time configuration needed
- **Build time impact**: Minimal increase from analyzers
- **Learning curve**: Developers must learn new rules
- **Existing code**: May need formatting updates

### Neutral
- **IDE support**: Requires VS Code extensions or Visual Studio settings
- **CI/CD changes**: Pipeline updates for enforcement

## Implementation

1. ✅ Created `.editorconfig` for cross-platform formatting
2. ✅ Added `.prettierrc.json` and `.prettierignore`
3. ✅ Created `Directory.Build.props` with StyleCop
4. ✅ Added `stylecop.json` configuration
5. ⏳ Update CI/CD to enforce standards

## References

- [StyleCop Analyzers](https://github.com/DotNetAnalyzers/StyleCopAnalyzers)
- [ESLint](https://eslint.org/)
- [Prettier](https://prettier.io/)
- [EditorConfig](https://editorconfig.org/)
