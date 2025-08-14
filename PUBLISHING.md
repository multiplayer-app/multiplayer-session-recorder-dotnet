# Publishing Guide

This document explains how to publish the SessionRecorder library to NuGet using GitHub Actions.

## Prerequisites

1. **NuGet API Key**: You need a NuGet API key to publish packages
2. **GitHub Repository**: The code must be in a GitHub repository
3. **GitHub Secrets**: The NuGet API key must be stored as a GitHub secret

## Setup

### 1. Get a NuGet API Key

1. Go to [NuGet.org](https://www.nuget.org/)
2. Sign in to your account
3. Go to your profile → API Keys
4. Create a new API key with "Push" permissions
5. Copy the API key

### 2. Add GitHub Secret

1. Go to your GitHub repository
2. Go to Settings → Secrets and variables → Actions
3. Click "New repository secret"
4. Name: `NUGET_API_KEY`
5. Value: Paste your NuGet API key
6. Click "Add secret"

## Publishing Process

### 1. Create a Git Tag

When you're ready to publish a new version, create and push a git tag:

```bash
# Create a new tag (use semantic versioning)
git tag v1.0.0

# Push the tag to GitHub
git push origin v1.0.0
```

### 2. GitHub Actions Workflow

The workflow will automatically:

1. **Trigger**: When you push a tag starting with `v` (e.g., `v1.0.0`, `v1.2.3`)
2. **Extract Version**: Remove the `v` prefix to get the version number
3. **Update Files**: 
   - Update `<Version>` in `SessionRecorder.csproj`
   - Update `SESSION_RECORDER_VERSION` in `src/Constants/Constants.cs`
4. **Build**: Compile the library in Release mode
5. **Test**: Run any tests
6. **Pack**: Create the NuGet package
7. **Publish**: Upload to NuGet.org
8. **Release**: Create a GitHub release

### 3. Version Format

Use semantic versioning for your tags:
- `v1.0.0` - Major release
- `v1.1.0` - Minor release  
- `v1.1.1` - Patch release
- `v2.0.0-beta.1` - Pre-release

## Example Workflow

```bash
# Make your changes and commit them
git add .
git commit -m "Add new feature"
git push origin main

# Create and push a tag to trigger the workflow
git tag v1.1.0
git push origin v1.1.0
```

## Monitoring

1. **GitHub Actions**: Go to your repository → Actions tab to monitor the workflow
2. **NuGet**: Check [NuGet.org](https://www.nuget.org/packages/SessionRecorder) for the published package
3. **GitHub Releases**: Check the Releases tab for the created release

## Troubleshooting

### Common Issues

1. **Workflow not triggered**: Make sure the tag starts with `v`
2. **Build fails**: Check the GitHub Actions logs for compilation errors
3. **Publish fails**: Verify the `NUGET_API_KEY` secret is set correctly
4. **Version not updated**: Check that the sed commands in the workflow are working

### Manual Publishing (if needed)

If you need to publish manually:

```bash
# Build and pack
dotnet build --configuration Release
dotnet pack --configuration Release --output nupkgs

# Publish to NuGet
dotnet nuget push "nupkgs/*.nupkg" --api-key YOUR_API_KEY --source https://api.nuget.org/v3/index.json
```

## Version Management

The workflow uses build parameters to set the version:
- Extracts version from git tag (e.g., `v1.2.3` → `1.2.3`)
- Passes version as build parameter: `-p:Version=1.2.3`
- All version properties (`AssemblyVersion`, `FileVersion`, `InformationalVersion`) automatically use this version

The `SESSION_RECORDER_VERSION` property automatically gets the version from the assembly, ensuring it's always in sync with the actual library version.
