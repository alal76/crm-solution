# Automatic Versioning System

This CRM Solution uses semantic versioning with automatic version management for both frontend and backend.

## Version Format

`MAJOR.MINOR.PATCH` (e.g., 1.1.0)

- **MAJOR**: Increment when adding significant new features
- **MINOR**: Increment when fixing bugs or making small improvements
- **PATCH**: Increment for critical hotfixes

## Updating Version

### Using PowerShell (Recommended for Windows)

```powershell
# Major version update (new feature)
.\update-version.ps1 -Type major -Description "Added company branding settings"

# Minor version update (bug fix)
.\update-version.ps1 -Type minor -Description "Fixed login button alignment"

# Patch version update (critical fix)
.\update-version.ps1 -Type patch -Description "Fixed security vulnerability in auth"
```

### Using Node.js

```bash
# Major version update (new feature)
node update-version.js major "Added company branding settings"

# Minor version update (bug fix)
node update-version.js minor "Fixed login button alignment"

# Patch version update (critical fix)
node update-version.js patch "Fixed security vulnerability in auth"
```

### Using npm (from CRM.Frontend directory)

```bash
# Major version update
npm run version:major -- "Added company branding settings"

# Minor version update
npm run version:minor -- "Fixed login button alignment"

# Patch version update
npm run version:patch -- "Fixed security vulnerability in auth"
```

## What Gets Updated

The version update scripts automatically update:

1. **version.json** (root)
   - Major, minor, patch numbers
   - Last update date
   - Change description

2. **package.json** (CRM.Frontend)
   - npm package version

3. **CRM.Api.csproj** (CRM.Backend)
   - `<Version>` tag
   - `<AssemblyVersion>` tag
   - `<FileVersion>` tag

## Version History

Version updates are tracked in `version.json`:

```json
{
  "major": 1,
  "minor": 1,
  "patch": 0,
  "lastUpdate": "2026-01-19",
  "description": "Cleaned code, added unit tests, created admin user, added settings link to footer"
}
```

## Workflow Example

### Adding a New Feature

1. Implement the feature in both frontend and backend
2. Update version:
   ```powershell
   .\update-version.ps1 -Type major -Description "Added new dashboard analytics widget"
   ```
3. Build both projects:
   ```bash
   npm run build          # Frontend
   dotnet build          # Backend
   ```
4. Commit changes:
   ```bash
   git commit -m "Version 2.0.0: Added new dashboard analytics widget"
   ```
5. Deploy:
   ```powershell
   ./deploy.ps1
   ```

### Fixing a Bug

1. Implement the bug fix
2. Update version:
   ```powershell
   .\update-version.ps1 -Type minor -Description "Fixed customer search filter not working"
   ```
3. Build, commit, and deploy

## Viewing Current Version

- In application footer: Shows current version (e.g., "1.1.0")
- In version.json: Full version information
- In package.json: Frontend version
- In CRM.Api.csproj: Backend version

## Displaying Version in Application

Frontend displays the version in the footer via environment variable:

```typescript
process.env.REACT_APP_VERSION || '1.1.0'
```

Backend includes version in assembly attributes:

```csharp
[assembly: AssemblyVersion("1.1.0.0")]
[assembly: FileVersion("1.1.0.0")]
```

## Best Practices

✅ **Do:**
- Update version before building new release
- Use descriptive change descriptions
- Commit version changes with meaningful commit message
- Use MAJOR for significant features
- Use MINOR for bug fixes and improvements

❌ **Don't:**
- Manually edit version numbers (use script instead)
- Update version without making actual changes
- Mix multiple change types without incrementing version
- Forget to rebuild after version update

## Semantic Versioning Guidelines

### MAJOR Version (1.x.x → 2.0.0)
- New major features
- Breaking changes
- Significant UI redesigns
- Major API changes
- Examples:
  - "Added complete dashboard redesign"
  - "Implemented new authentication system"
  - "Added multi-tenant support"

### MINOR Version (1.0.x → 1.1.0)
- Bug fixes
- Small features
- Performance improvements
- UI refinements
- Examples:
  - "Fixed login page responsive layout"
  - "Added settings link to footer"
  - "Improved database query performance"

### PATCH Version (1.1.0 → 1.1.1)
- Critical hotfixes
- Security patches
- Emergency rollback fixes
- Examples:
  - "Fixed critical security vulnerability"
  - "Rollback database migration"

## Continuous Integration

For CI/CD pipelines, version can be read from `version.json`:

```bash
# Get current version for Docker tags
VERSION=$(jq -r '.major + "." + .minor + "." + .patch' version.json)
docker build -t crm-app:${VERSION} .
```

## Support

For issues with versioning scripts:
1. Verify version.json exists in root directory
2. Check file permissions on update scripts
3. Ensure both package.json and .csproj files are valid JSON/XML
4. Review error messages for specific issues
