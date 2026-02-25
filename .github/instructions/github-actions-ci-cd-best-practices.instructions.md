---
applyTo: '.github/workflows/*.yml,.github/workflows/*.yaml'
description: 'GitHub Actions CI/CD guidance for SwiftJot—focus on Native AOT builds, Windows compilation, and GitHub Releases.'
---

# SwiftJot: GitHub Actions CI/CD Guidelines

## Core Workflow Structure

**Typical SwiftJot Pipeline:**
- **Build:** Compile in Debug and Release modes
- **Test:** Run unit tests (if present)
- **AOT Publish:** Publish to win-x64 with Native AOT enabled—**mandatory before release**
- **Release:** Create GitHub Release with compiled .exe

**Workflow Triggers:**
- `push` to `main` or `v*` tags → trigger build, test, and release
- `pull_request` to `main` → trigger build and test only
- `workflow_dispatch` → manual build/release trigger

## Native AOT Build Requirements

**Critical:** Every workflow must validate Native AOT compilation. Desktop users expect sub-second startup and minimal memory.

**Build Targets:**
- **Debug:** `dotnet build` for fast iteration (includes reflection/dynamic code)
- **Release:** `dotnet publish -c Release -r win-x64` with AOT (strict, no reflection)

**AOT Validation Steps:**
```yaml
- name: Restore dependencies
  run: dotnet restore

- name: Build for testing
  run: dotnet build -c Debug

- name: Run tests (if applicable)
  run: dotnet test -c Debug --no-build

- name: Publish AOT binary
  run: dotnet publish -c Release -r win-x64
  
- name: Verify AOT executable
  run: |
    $exe = ".\bin\Release\net8.0-windows\win-x64\publish\SwiftJot.exe"
    if (Test-Path $exe) {
      Write-Host "✓ AOT executable created: $exe"
      $size = (Get-Item $exe).Length / 1MB
      Write-Host "  File size: $([Math]::Round($size, 2)) MB"
    } else {
      Write-Error "AOT build failed: executable not found"
      exit 1
    }
```

**Key Points:**
- Always test AOT publish before creating releases
- Failures in AOT publish are deal-breakers; fix immediately
- Monitor binary size (target: sub-100MB)

## GitHub Releases & Distribution

**Release Workflow Pattern:**
```yaml
name: Build and Release
on:
  push:
    tags:
      - 'v*'

permissions:
  contents: write

jobs:
  build:
    runs-on: windows-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: 8.0.x
      
      - name: Publish AOT
        run: dotnet publish -c Release -r win-x64
      
      - name: Create release artifact
        shell: pwsh
        run: |
          $version = "${{ github.ref }}" -replace 'refs/tags/v', ''
          $exe = "bin/Release/net8.0-windows/win-x64/publish/SwiftJot.exe"
          $artifact = "SwiftJot-$version-win-x64.exe"
          Copy-Item $exe $artifact
          Write-Host "Created: $artifact"
      
      - name: Upload to Release
        uses: softprops/action-gh-release@v1
        with:
          files: SwiftJot-*.exe
          draft: false
          prerelease: false
```

**Key Points:**
- Tag format: `v1.0.0` (semantic versioning)
- Always publish with AOT before releasing
- Upload only the compiled `.exe`
- Use `softprops/action-gh-release` for GitHub Releases

## Security Essentials

**Permissions (Least Privilege):**
```yaml
permissions:
  contents: read  # Default for build/test jobs

jobs:
  build:
    permissions:
      contents: read
    steps:
      # ... build steps
  
  release:
    permissions:
      contents: write  # Only release job needs write
```

**Sensitive Data:**
- Never hardcode credentials, API keys, or secrets
- Use `secrets.<SECRET_NAME>` context for all sensitive data
- GitHub Actions automatically masks secrets in logs

## Performance & Caching

**Cache NuGet Packages:**
```yaml
- name: Cache NuGet
  uses: actions/cache@v3
  with:
    path: ~/.nuget/packages
    key: ${{ runner.os }}-nuget-${{ hashFiles('**/packages.lock.json') }}
    restore-keys: |
      ${{ runner.os }}-nuget-
```

**Cache dotnet:**
```yaml
- name: Setup .NET
  uses: actions/setup-dotnet@v4
  with:
    dotnet-version: 8.0.x
    cache: true
```

## Workflow Best Practices

1. **Use `windows-latest` runner** — ensures Windows-native build compatibility
2. **Pin action versions** — use `@v4` or full commit SHA, never `@main` or `@latest`
3. **Clean checkout** — use `fetch-depth: 1` for faster clones on non-release builds
4. **Sequential steps** — combine commands with `&&` for efficiency
5. **Explicit names** — give each step a clear, descriptive name for logging
6. **Fail fast** — exit immediately on build/test failures before expensive operations

## Troubleshooting

**AOT Build Failures:**
- Review error logs; code uses reflection or incompatible library
- Refactor to avoid runtime reflection (use compile-time approaches)
- Check NuGet packages for AOT compatibility

**Release Upload Fails:**
- Verify `permissions: contents: write` at workflow/job level
- Confirm AOT publish succeeded and `.exe` exists at expected path
- Check tag format matches trigger filter

**Workflow Not Running:**
- Verify tag format (`v*` pattern) matches trigger
- Check branch protection rules aren't blocking workflow
- Use `workflow_dispatch` to manually trigger for testing

## SwiftJot CI/CD Checklist

- [ ] AOT publish succeeds without reflection errors
- [ ] Executable file size is sub-100MB
- [ ] Tag format follows semantic versioning (`v1.0.0`)
- [ ] Release artifacts contain only the `.exe` file
- [ ] `permissions: contents: write` set for release jobs
- [ ] `runs-on: windows-latest` used for all build jobs
- [ ] Workflow uses pinned action versions (`@v4`, not `@latest`)
- [ ] All steps have clear, descriptive names
- [ ] Failed AOT build blocks release immediately
