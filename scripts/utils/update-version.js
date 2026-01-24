#!/usr/bin/env node
/**
 * Automatic version management script (Node.js version)
 * Usage: node update-version.js [major|minor|patch] "Description of changes"
 */

const fs = require('fs');
const path = require('path');

const args = process.argv.slice(2);
const updateType = args[0];
const description = args.slice(1).join(' ');

if (!['major', 'minor', 'patch'].includes(updateType)) {
    console.error('❌ Invalid update type. Use: major, minor, or patch');
    process.exit(1);
}

if (!description) {
    console.error('❌ Please provide a description of changes');
    process.exit(1);
}

const versionFile = path.join(__dirname, 'version.json');
const packageJsonFile = path.join(__dirname, 'CRM.Frontend', 'package.json');
const csprojFile = path.join(__dirname, 'CRM.Backend', 'src', 'CRM.Api', 'CRM.Api.csproj');

try {
    // Read current version
    const versionData = JSON.parse(fs.readFileSync(versionFile, 'utf8'));
    let { major, minor, patch } = versionData;

    // Update version based on type
    switch (updateType) {
        case 'major':
            major++;
            minor = 0;
            patch = 0;
            console.log('📈 Major version bump: ' + major + '.0.0 (New Feature)');
            break;
        case 'minor':
            minor++;
            patch = 0;
            console.log('📊 Minor version bump: ' + major + '.' + minor + '.0 (Bug Fix)');
            break;
        case 'patch':
            patch++;
            console.log('🔧 Patch version bump: ' + major + '.' + minor + '.' + patch + ' (Small Fix)');
            break;
    }

    const newVersion = major + '.' + minor + '.' + patch;
    const assemblyVersion = major + '.' + minor + '.' + patch + '.0';
    const now = new Date().toISOString().split('T')[0];

    // Update version.json
    versionData.major = major;
    versionData.minor = minor;
    versionData.patch = patch;
    versionData.lastUpdate = now;
    versionData.description = description;
    
    fs.writeFileSync(versionFile, JSON.stringify(versionData, null, 2));
    console.log('✅ Updated version.json to ' + newVersion);

    // Update package.json
    const packageJson = JSON.parse(fs.readFileSync(packageJsonFile, 'utf8'));
    packageJson.version = newVersion;
    fs.writeFileSync(packageJsonFile, JSON.stringify(packageJson, null, 2));
    console.log('✅ Updated package.json to ' + newVersion);

    // Update .csproj
    let csprojContent = fs.readFileSync(csprojFile, 'utf8');
    csprojContent = csprojContent.replace(/<Version>.*?<\/Version>/, '<Version>' + newVersion + '</Version>');
    csprojContent = csprojContent.replace(/<AssemblyVersion>.*?<\/AssemblyVersion>/, '<AssemblyVersion>' + assemblyVersion + '</AssemblyVersion>');
    csprojContent = csprojContent.replace(/<FileVersion>.*?<\/FileVersion>/, '<FileVersion>' + assemblyVersion + '</FileVersion>');
    fs.writeFileSync(csprojFile, csprojContent);
    console.log('✅ Updated CRM.Api.csproj to ' + newVersion);

    console.log('');
    console.log('🎉 Version update complete!');
    console.log('   Version: ' + newVersion);
    console.log('   Type: ' + updateType);
    console.log('   Description: ' + description);
    console.log('');
    console.log('Next steps:');
    console.log('  1. Review changes: git diff');
    console.log('  2. Commit changes: git commit -m "Version ' + newVersion + ': ' + description + '"');
    console.log('  3. Build: npm run build (frontend) / dotnet build (backend)');
    console.log('  4. Deploy: ./deploy.ps1 or npm run deploy');

} catch (error) {
    console.error('❌ Error:', error.message);
    process.exit(1);
}
