# CI/CD Pipeline Documentation

## Overview

The UserEventsApp project uses GitHub Actions to automate building, testing, code quality checks, security scanning, Docker builds, releases, and documentation deployment.

## Workflows

### 1. Build and Test (`build-test.yml`)
**Triggers:** Push to `main`/`develop`, Pull Requests

**Jobs:**
- Restore dependencies
- Build in Release configuration
- Run all 23 unit tests
- Upload test results as artifacts
- Publish test results to PR/commit

**Requirements:** None (uses default GitHub Actions environment)

### 2. Code Quality (`code-quality.yml`)
**Triggers:** Push to `main`/`develop`, Pull Requests

**Jobs:**
- Enforce code style rules
- Check code formatting
- Run static code analysis
- Treat warnings as errors (enforced)

**Best Practice:** Review code quality checks before merging PRs

### 3. Docker Build (`docker-build.yml`)
**Triggers:** 
- Push to `main` branch or tags (`v*`)
- Successful completion of Build and Test workflow

**Jobs:**
- Set up Docker Buildx with cache layers
- Authenticate with GitHub Container Registry (ghcr.io)
- Build multi-platform Docker image
- Push to registry with tags (branch, semver, SHA)

**Container Registry:** `ghcr.io/dotsharpfx-dotnet/usereventsapp`

**Tags Applied:**
- `latest` (on main branch)
- `v1.0.0` (from git tag)
- `sha-abc123def` (commit SHA)

### 4. Security Scan (`security-scan.yml`)
**Triggers:** 
- Push to `main`/`develop`
- Pull Requests
- Weekly schedule (Sunday 2 AM UTC)

**Jobs:**
- Check for vulnerable NuGet dependencies
- SonarCloud code quality analysis (requires SONAR_TOKEN)
- Trivy filesystem vulnerability scanning
- SARIF upload to GitHub Security tab

**Secrets Required:**
- `SONAR_TOKEN` - SonarCloud authentication token

**Reports:** Available in GitHub Security → Code scanning alerts

### 5. Release (`release.yml`)
**Triggers:** Push to git tags matching `v*` (e.g., `v1.0.0`)

**Jobs:**
- Extract version from git tag
- Build in Release configuration
- Run all tests
- Publish application
- Create GitHub Release with artifacts
- Pack and publish NuGet packages

**GitHub Release:** Automatically created with published binaries

**NuGet Packages:**
- `UserEvents.Models`
- `UserEvents.Infra`

**Usage:**
```bash
git tag v1.0.0
git push origin v1.0.0
```

### 6. Documentation (`documentation.yml`)
**Triggers:** 
- Push to `main` with changes to README.md, TESTS.md, or ARTICLE.md
- Manual workflow dispatch

**Jobs:**
- Aggregate documentation files
- Deploy to GitHub Pages
- Make docs available at: `https://github.com/pages/dotsharpfx-dotnet/UserEventsApp`

**Published Docs:**
- `index.md` - Project overview (README.md)
- `testing.md` - Testing guide (TESTS.md)
- `article.md` - Technical article (ARTICLE.md)

## Pipeline Status

All workflows include status badges that can be added to README.md:

```markdown
![Build and Test](https://github.com/dotsharpfx-dotnet/UserEventsApp/actions/workflows/build-test.yml/badge.svg)
![Code Quality](https://github.com/dotsharpfx-dotnet/UserEventsApp/actions/workflows/code-quality.yml/badge.svg)
![Docker Build](https://github.com/dotsharpfx-dotnet/UserEventsApp/actions/workflows/docker-build.yml/badge.svg)
![Security Scan](https://github.com/dotsharpfx-dotnet/UserEventsApp/actions/workflows/security-scan.yml/badge.svg)
```

## Docker Image Usage

### Pull and Run
```bash
docker pull ghcr.io/dotsharpfx-dotnet/usereventsapp:main
docker run -e KAFKA_BOOTSTRAP_SERVERS=localhost:9092 \
           -e SCHEMA_REGISTRY_URL=http://localhost:8081 \
           ghcr.io/dotsharpfx-dotnet/usereventsapp:main
```

### Docker Compose with GitHub Image
```yaml
services:
  user-events-app:
    image: ghcr.io/dotsharpfx-dotnet/usereventsapp:main
    environment:
      - KAFKA_BOOTSTRAP_SERVERS=kafka:9092
      - SCHEMA_REGISTRY_URL=http://schema-registry:8081
    depends_on:
      - kafka
      - schema-registry
```

## Required Secrets

For full CI/CD functionality, configure these GitHub Secrets:

| Secret | Purpose | Source |
|--------|---------|--------|
| `SONAR_TOKEN` | SonarCloud authentication | https://sonarcloud.io |
| `GITHUB_TOKEN` | GitHub Actions (auto-provided) | N/A - auto-generated |

### Adding Secrets
1. Go to Settings → Secrets and variables → Actions
2. Click "New repository secret"
3. Add secret name and value
4. Workflows can reference via `${{ secrets.SECRET_NAME }}`

## Workflow Configuration

### Build Matrix Strategy
Currently configured for .NET 10.0 only. To add more versions:

```yaml
strategy:
  matrix:
    dotnet-version: ['10.0.x', '11.0.x']
```

### Caching
- Docker Buildx cache is preserved between builds
- NuGet package cache speeds up restore operations

### Conditional Steps
- `continue-on-error: true` - Step failures don't block workflow
- `if: always()` - Run step regardless of previous failures
- `if: ${{ github.event_name != 'pull_request' }}` - Only push to registry on non-PR events

## Troubleshooting

### Build Failures
1. Check workflow run logs in GitHub Actions
2. Look for compilation errors in "Build" step
3. Verify .NET SDK version compatibility

### Test Failures
1. Review test output in "Run tests" step
2. Check test results artifact for detailed reports
3. Run tests locally: `dotnet test`

### Docker Build Issues
1. Verify Dockerfile exists in repository root
2. Check Docker Buildx setup in logs
3. Ensure no secrets are embedded in Dockerfile

### Security Scan Issues
1. SonarCloud: Configure project key and organization
2. Trivy: False positives can be ignored with `.trivyignore`
3. Dependency vulnerabilities: Update packages or add exemptions

## Best Practices

1. **Branch Protection:** Require status checks before merge
   - Settings → Branches → Require status checks to pass

2. **Pull Request Reviews:** Require approval before merge
   - Settings → Code and automation → Pull requests

3. **Semantic Versioning:** Follow `v<major>.<minor>.<patch>` for releases
   - Example: `v1.0.0`, `v1.2.3`

4. **Commit Messages:** Use conventional commits for automation
   - `feat:`, `fix:`, `docs:`, `refactor:`, etc.

5. **Environment Variables:** Configure per-environment secrets
   - Separate secrets for staging vs. production

## Monitoring

### GitHub Actions Dashboard
- View all workflows: Settings → Actions → General
- Monitor usage and quotas
- Download logs for debugging

### Notifications
- Configure notifications: Settings → Notifications
- Receive alerts on workflow failures
- Subscribe to release notifications

## Next Steps

1. **Integrate SonarCloud** for code quality metrics
2. **Set up container registry authentication** for private repositories
3. **Configure branch protection rules** to enforce checks
4. **Add deployment workflows** for staging/production environments
5. **Set up automated dependency updates** with Dependabot

---

For more information:
- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [dotnet tooling in GitHub Actions](https://github.com/actions/setup-dotnet)
- [Docker Build Action](https://github.com/docker/build-push-action)
