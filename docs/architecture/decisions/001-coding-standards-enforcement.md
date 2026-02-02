# ADR-001: Enforce Coding Standards with Linters and Formatters

**Date:** 2026-02-02  
**Status:** Accepted  
**Deciders:** Architecture Review Team  
**Technical Story:** Comprehensive Architecture Review

## Context

The codebase has grown to over 50,000 lines of code with multiple contributors. Code quality and consistency issues were identified:

- **Frontend Issues**:
  - 56 instances of TypeScript `as any` type casts
  - Inconsistent code formatting
  - No enforcement of TypeScript best practices
  - ESLint rules too permissive

- **Backend Issues**:
  - Inconsistent C# code style
  - No automated code analysis
  - Missing XML documentation on public APIs
  - Varying formatting across files

Without enforced standards, the codebase becomes harder to maintain, code reviews become subjective, and bugs are more likely to slip through.

## Decision

We will enforce coding standards using automated tools:

### Frontend (React/TypeScript)
1. **ESLint** with strict configuration
   - Enable `@typescript-eslint/no-explicit-any: error`
   - Add complexity limits (max 15)
   - Add max-depth limits (4 levels)
   - Enforce React best practices

2. **Prettier** for consistent formatting
   - Single quotes
   - 2-space indentation
   - 100 character line length
   - Automatic formatting on save

3. **TypeScript** strict mode
   - Enable `strict: true`
   - Enable `noImplicitAny: true`
   - Enable `strictNullChecks: true`

### Backend (.NET 8.0)
1. **EditorConfig** for C# formatting
   - 4-space indentation
   - Allman brace style
   - Consistent naming conventions

2. **StyleCop.Analyzers**
   - Enforce C# code style rules
   - Disabled rules: SA1101, SA1633, SA1309, SA1600
   - Treat warnings as suggestions (not errors)

3. **Built-in .NET Analyzers**
   - Enable all recommended analyzers
   - Analysis level: latest

### CI/CD Integration
- Add linting to GitHub Actions workflow
- Add formatting checks to CI pipeline
- Fail builds on linting errors
- Add pre-commit hooks (optional)

## Consequences

### Positive Consequences
- **Consistency**: Uniform code style across entire codebase
- **Quality**: Catch common mistakes early
- **Maintainability**: Easier for developers to read and understand code
- **Automation**: Reduce subjective code review comments
- **Onboarding**: Clear standards for new team members
- **TypeScript Safety**: Eliminate unsafe `any` casts

### Negative Consequences
- **Initial Effort**: Fixing existing violations will take time
- **Build Times**: Slightly longer due to analysis
- **Learning Curve**: Team needs to learn new rules
- **False Positives**: Some rules may need adjustment
- **Friction**: Developers may resist strict rules initially

## Implementation Plan

### Phase 1: Configuration (Week 1)
- [x] Create `.editorconfig` file
- [x] Configure ESLint with strict rules
- [x] Add Prettier configuration
- [x] Create `Directory.Build.props` with StyleCop
- [x] Update CI/CD workflows

### Phase 2: Documentation (Week 1)
- [x] Create `CODING_STANDARDS.md`
- [x] Document TypeScript standards
- [x] Document C# standards
- [x] Add examples of good/bad code

### Phase 3: Remediation (Weeks 2-4)
- [ ] Fix TypeScript `any` violations (56 instances)
- [ ] Run Prettier on entire frontend
- [ ] Fix backend StyleCop violations
- [ ] Add missing XML documentation

### Phase 4: Enforcement (Week 5+)
- [ ] Enable strict TypeScript mode
- [ ] Enable ESLint errors in CI
- [ ] Monitor and adjust rules as needed

## Alternatives Considered

### Alternative 1: Manual Code Reviews Only
**Pros:**
- No tooling overhead
- Flexibility in standards
- No false positives

**Cons:**
- Time-consuming
- Subjective
- Inconsistent enforcement
- Doesn't scale

**Why Rejected:** Not sustainable as team and codebase grow

### Alternative 2: Less Strict Rules
**Pros:**
- Easier to adopt
- Less initial work
- Less friction

**Cons:**
- Doesn't solve the problem
- Quality issues remain
- Technical debt accumulates

**Why Rejected:** Defeats the purpose of having standards

### Alternative 3: Different Tools (TSLint, JSLint)
**Pros:**
- More options available

**Cons:**
- TSLint is deprecated
- ESLint is industry standard
- Better TypeScript support in ESLint

**Why Rejected:** ESLint is the modern standard

## Migration Strategy

1. **Week 1**: Add tooling and documentation
2. **Week 2-3**: Fix high-priority violations
3. **Week 4**: Fix remaining violations
4. **Week 5**: Enable strict enforcement in CI
5. **Ongoing**: Monitor and adjust rules

## Success Metrics

- Zero `as any` casts in production code
- 100% ESLint compliance
- 100% Prettier formatting compliance
- Zero StyleCop violations in new code
- Reduced code review time on style issues

## References

- [ESLint TypeScript Plugin](https://typescript-eslint.io/)
- [Prettier](https://prettier.io/)
- [StyleCop.Analyzers](https://github.com/DotNetAnalyzers/StyleCopAnalyzers)
- [EditorConfig](https://editorconfig.org/)
- [TypeScript Handbook](https://www.typescriptlang.org/docs/handbook/intro.html)
- [C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)

## Review History

- **2026-02-02**: Initial decision - Accepted
