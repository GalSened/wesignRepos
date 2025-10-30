# Step H: CI/CD Configuration - Export Functionality

## Overview
Comprehensive CI/CD pipeline configuration for Export functionality deployment, including automated testing, security scanning, performance validation, and multi-environment deployment strategies.

## 1. GitHub Actions Workflow Configuration

### Main CI/CD Pipeline
```yaml
# .github/workflows/export-functionality-ci-cd.yml
name: Export Functionality CI/CD Pipeline

on:
  push:
    branches: [main, develop]
    paths:
      - 'src/app/export/**'
      - 'src/app/shared/services/export*'
      - 'e2e/export-functionality/**'
      - '.github/workflows/export-functionality-ci-cd.yml'
  pull_request:
    branches: [main, develop]
    paths:
      - 'src/app/export/**'
      - 'src/app/shared/services/export*'
      - 'e2e/export-functionality/**'
  workflow_dispatch:
    inputs:
      environment:
        description: 'Target environment for deployment'
        required: true
        default: 'staging'
        type: choice
        options:
          - staging
          - production
      skip_tests:
        description: 'Skip test execution (emergency deployment only)'
        required: false
        default: false
        type: boolean

env:
  NODE_VERSION: '18.x'
  CACHE_VERSION: 'v1'
  REGISTRY_URL: 'ghcr.io'
  IMAGE_NAME: 'wesign-client'

jobs:
  # ============================================================================
  # VALIDATION PHASE
  # ============================================================================

  code-quality:
    name: 'Code Quality & Linting'
    runs-on: ubuntu-latest
    timeout-minutes: 10

    steps:
      - name: 'Checkout Code'
        uses: actions/checkout@v4
        with:
          fetch-depth: 0

      - name: 'Setup Node.js'
        uses: actions/setup-node@v4
        with:
          node-version: ${{ env.NODE_VERSION }}
          cache: 'npm'

      - name: 'Install Dependencies'
        run: npm ci --prefer-offline --no-audit

      - name: 'Lint TypeScript Code'
        run: |
          npm run lint:export
          npm run lint:shared-export

      - name: 'Check Code Formatting'
        run: npm run format:check -- src/app/export

      - name: 'Type Check'
        run: npm run type-check -- --project src/app/export/tsconfig.json

      - name: 'Dependency Vulnerability Scan'
        run: npm audit --audit-level=high

      - name: 'Upload Lint Results'
        if: always()
        uses: actions/upload-artifact@v3
        with:
          name: lint-results
          path: lint-results.json
          retention-days: 7

  # ============================================================================
  # TESTING PHASE
  # ============================================================================

  unit-tests:
    name: 'Unit Tests'
    runs-on: ubuntu-latest
    timeout-minutes: 15
    needs: [code-quality]

    strategy:
      matrix:
        test-group:
          - components
          - services
          - state-management
          - utilities

    steps:
      - name: 'Checkout Code'
        uses: actions/checkout@v4

      - name: 'Setup Node.js'
        uses: actions/setup-node@v4
        with:
          node-version: ${{ env.NODE_VERSION }}
          cache: 'npm'

      - name: 'Install Dependencies'
        run: npm ci --prefer-offline

      - name: 'Run Unit Tests - ${{ matrix.test-group }}'
        run: |
          npm run test:export:${{ matrix.test-group }} -- \
            --watch=false \
            --browsers=ChromeHeadless \
            --code-coverage \
            --progress=false

      - name: 'Upload Test Results'
        if: always()
        uses: actions/upload-artifact@v3
        with:
          name: unit-test-results-${{ matrix.test-group }}
          path: |
            coverage/
            test-results/unit-tests-${{ matrix.test-group }}.xml
          retention-days: 7

      - name: 'Comment Test Results on PR'
        if: github.event_name == 'pull_request' && always()
        uses: dorny/test-reporter@v1
        with:
          name: 'Unit Tests - ${{ matrix.test-group }}'
          path: 'test-results/unit-tests-${{ matrix.test-group }}.xml'
          reporter: 'jest-junit'

  integration-tests:
    name: 'Integration Tests'
    runs-on: ubuntu-latest
    timeout-minutes: 20
    needs: [unit-tests]

    services:
      redis:
        image: redis:7-alpine
        ports:
          - 6379:6379
        options: >-
          --health-cmd "redis-cli ping"
          --health-interval 10s
          --health-timeout 5s
          --health-retries 5

    steps:
      - name: 'Checkout Code'
        uses: actions/checkout@v4

      - name: 'Setup Node.js'
        uses: actions/setup-node@v4
        with:
          node-version: ${{ env.NODE_VERSION }}
          cache: 'npm'

      - name: 'Install Dependencies'
        run: npm ci --prefer-offline

      - name: 'Start Backend Services'
        run: |
          docker-compose -f docker-compose.test.yml up -d
          sleep 30

      - name: 'Run Integration Tests'
        env:
          TEST_API_URL: 'http://localhost:5000'
          TEST_REDIS_URL: 'redis://localhost:6379'
        run: |
          npm run test:export:integration -- \
            --watch=false \
            --browsers=ChromeHeadless \
            --timeout=30000

      - name: 'Cleanup Test Services'
        if: always()
        run: docker-compose -f docker-compose.test.yml down -v

      - name: 'Upload Integration Test Results'
        if: always()
        uses: actions/upload-artifact@v3
        with:
          name: integration-test-results
          path: |
            test-results/integration-tests.xml
            screenshots/
          retention-days: 7

  e2e-tests:
    name: 'End-to-End Tests'
    runs-on: ubuntu-latest
    timeout-minutes: 30
    needs: [integration-tests]

    strategy:
      matrix:
        browser: [chromium, firefox, webkit]
        test-suite:
          - export-workflow
          - email-delivery
          - template-management
          - error-handling

    steps:
      - name: 'Checkout Code'
        uses: actions/checkout@v4

      - name: 'Setup Node.js'
        uses: actions/setup-node@v4
        with:
          node-version: ${{ env.NODE_VERSION }}
          cache: 'npm'

      - name: 'Install Dependencies'
        run: npm ci --prefer-offline

      - name: 'Install Playwright Browsers'
        run: npx playwright install --with-deps ${{ matrix.browser }}

      - name: 'Build Application'
        run: npm run build:staging

      - name: 'Start Application Server'
        run: |
          npm run serve:staging &
          npx wait-on http://localhost:4200 --timeout=60000

      - name: 'Run E2E Tests - ${{ matrix.test-suite }} on ${{ matrix.browser }}'
        run: |
          npx playwright test \
            --project=${{ matrix.browser }} \
            e2e/export-functionality/${{ matrix.test-suite }}.spec.ts \
            --reporter=junit

      - name: 'Upload E2E Test Results'
        if: always()
        uses: actions/upload-artifact@v3
        with:
          name: e2e-results-${{ matrix.browser }}-${{ matrix.test-suite }}
          path: |
            test-results/
            playwright-report/
            test-results/videos/
            test-results/screenshots/
          retention-days: 7

  # ============================================================================
  # SECURITY PHASE
  # ============================================================================

  security-scan:
    name: 'Security Scanning'
    runs-on: ubuntu-latest
    timeout-minutes: 15
    needs: [code-quality]

    steps:
      - name: 'Checkout Code'
        uses: actions/checkout@v4
        with:
          fetch-depth: 0

      - name: 'Setup Node.js'
        uses: actions/setup-node@v4
        with:
          node-version: ${{ env.NODE_VERSION }}
          cache: 'npm'

      - name: 'Install Dependencies'
        run: npm ci --prefer-offline

      - name: 'Run Semgrep Security Analysis'
        uses: semgrep/semgrep-action@v1
        with:
          config: >-
            p/security-audit
            p/typescript
            p/angular
          publishToken: ${{ secrets.SEMGREP_APP_TOKEN }}
          publishDeployment: ${{ github.sha }}
          generateSarif: "1"

      - name: 'Run CodeQL Analysis'
        uses: github/codeql-action/init@v2
        with:
          languages: typescript, javascript
          queries: security-and-quality

      - name: 'Perform CodeQL Analysis'
        uses: github/codeql-action/analyze@v2

      - name: 'Dependency Security Audit'
        run: |
          npm audit --audit-level=moderate --output=json > security-audit.json || true
          npx audit-ci --moderate

      - name: 'License Compliance Check'
        run: |
          npx license-checker --production --json --out license-report.json
          npx license-compliance-checker license-report.json

      - name: 'Upload Security Reports'
        if: always()
        uses: actions/upload-artifact@v3
        with:
          name: security-reports
          path: |
            security-audit.json
            license-report.json
            semgrep-report.sarif
          retention-days: 30

  # ============================================================================
  # PERFORMANCE PHASE
  # ============================================================================

  performance-tests:
    name: 'Performance Testing'
    runs-on: ubuntu-latest
    timeout-minutes: 25
    needs: [e2e-tests]
    if: github.ref == 'refs/heads/main' || github.event_name == 'workflow_dispatch'

    steps:
      - name: 'Checkout Code'
        uses: actions/checkout@v4

      - name: 'Setup Node.js'
        uses: actions/setup-node@v4
        with:
          node-version: ${{ env.NODE_VERSION }}
          cache: 'npm'

      - name: 'Install Dependencies'
        run: npm ci --prefer-offline

      - name: 'Build Production Application'
        run: npm run build:production

      - name: 'Start Production Server'
        run: |
          npm run serve:production &
          npx wait-on http://localhost:4200 --timeout=60000

      - name: 'Run Lighthouse Performance Audit'
        run: |
          npx lighthouse-ci autorun \
            --config=lighthouserc.json \
            --upload.target=filesystem \
            --upload.outputDir=lighthouse-results

      - name: 'Run Load Testing with Artillery'
        run: |
          npx artillery run \
            performance-tests/export-load-test.yml \
            --output performance-results.json

      - name: 'Generate Performance Report'
        run: |
          npx artillery report performance-results.json \
            --output performance-report.html

      - name: 'Bundle Size Analysis'
        run: |
          npx webpack-bundle-analyzer \
            dist/wesign-client/stats.json \
            --report bundle-analysis.html \
            --mode static

      - name: 'Upload Performance Results'
        if: always()
        uses: actions/upload-artifact@v3
        with:
          name: performance-results
          path: |
            lighthouse-results/
            performance-report.html
            bundle-analysis.html
            performance-results.json
          retention-days: 30

      - name: 'Comment Performance Results on PR'
        if: github.event_name == 'pull_request'
        uses: treosh/lighthouse-ci-action@v9
        with:
          configPath: './lighthouserc.json'
          uploadArtifacts: true
          temporaryPublicStorage: true

  # ============================================================================
  # BUILD PHASE
  # ============================================================================

  build-artifacts:
    name: 'Build Application Artifacts'
    runs-on: ubuntu-latest
    timeout-minutes: 15
    needs: [unit-tests, security-scan]

    strategy:
      matrix:
        environment: [staging, production]

    steps:
      - name: 'Checkout Code'
        uses: actions/checkout@v4

      - name: 'Setup Node.js'
        uses: actions/setup-node@v4
        with:
          node-version: ${{ env.NODE_VERSION }}
          cache: 'npm'

      - name: 'Install Dependencies'
        run: npm ci --prefer-offline

      - name: 'Build Application - ${{ matrix.environment }}'
        run: npm run build:${{ matrix.environment }}
        env:
          NODE_ENV: ${{ matrix.environment }}

      - name: 'Generate Build Metadata'
        run: |
          echo "BUILD_VERSION=$(cat package.json | jq -r .version)" >> $GITHUB_ENV
          echo "BUILD_SHA=${{ github.sha }}" >> $GITHUB_ENV
          echo "BUILD_TIMESTAMP=$(date -u +%Y-%m-%dT%H:%M:%SZ)" >> $GITHUB_ENV

      - name: 'Create Build Info'
        run: |
          cat > dist/wesign-client/build-info.json << EOF
          {
            "version": "${{ env.BUILD_VERSION }}",
            "sha": "${{ env.BUILD_SHA }}",
            "timestamp": "${{ env.BUILD_TIMESTAMP }}",
            "environment": "${{ matrix.environment }}",
            "branch": "${{ github.ref_name }}",
            "workflow": "${{ github.run_id }}"
          }
          EOF

      - name: 'Upload Build Artifacts'
        uses: actions/upload-artifact@v3
        with:
          name: build-artifacts-${{ matrix.environment }}
          path: |
            dist/
            build-info.json
          retention-days: 30

  # ============================================================================
  # CONTAINER BUILD PHASE
  # ============================================================================

  build-container:
    name: 'Build Container Images'
    runs-on: ubuntu-latest
    timeout-minutes: 20
    needs: [build-artifacts]
    if: github.ref == 'refs/heads/main' || github.event_name == 'workflow_dispatch'

    steps:
      - name: 'Checkout Code'
        uses: actions/checkout@v4

      - name: 'Download Build Artifacts'
        uses: actions/download-artifact@v3
        with:
          name: build-artifacts-production
          path: dist/

      - name: 'Set up Docker Buildx'
        uses: docker/setup-buildx-action@v3

      - name: 'Log in to Container Registry'
        uses: docker/login-action@v3
        with:
          registry: ${{ env.REGISTRY_URL }}
          username: ${{ github.actor }}
          password: ${{ secrets.GITHUB_TOKEN }}

      - name: 'Extract Metadata'
        id: meta
        uses: docker/metadata-action@v5
        with:
          images: ${{ env.REGISTRY_URL }}/${{ github.repository }}/${{ env.IMAGE_NAME }}
          tags: |
            type=ref,event=branch
            type=ref,event=pr
            type=sha,prefix={{branch}}-
            type=raw,value=latest,enable={{is_default_branch}}

      - name: 'Build and Push Container Image'
        uses: docker/build-push-action@v5
        with:
          context: .
          file: ./docker/Dockerfile.export-enhanced
          push: true
          tags: ${{ steps.meta.outputs.tags }}
          labels: ${{ steps.meta.outputs.labels }}
          cache-from: type=gha
          cache-to: type=gha,mode=max
          platforms: linux/amd64,linux/arm64
          build-args: |
            BUILD_VERSION=${{ env.BUILD_VERSION }}
            BUILD_SHA=${{ github.sha }}
            BUILD_TIMESTAMP=${{ env.BUILD_TIMESTAMP }}

  # ============================================================================
  # DEPLOYMENT PHASE
  # ============================================================================

  deploy-staging:
    name: 'Deploy to Staging'
    runs-on: ubuntu-latest
    timeout-minutes: 10
    needs: [e2e-tests, build-container]
    if: github.ref == 'refs/heads/develop' || (github.event_name == 'workflow_dispatch' && github.event.inputs.environment == 'staging')
    environment:
      name: staging
      url: https://staging.wesign.comda.co.il

    steps:
      - name: 'Checkout Code'
        uses: actions/checkout@v4

      - name: 'Download Build Artifacts'
        uses: actions/download-artifact@v3
        with:
          name: build-artifacts-staging
          path: dist/

      - name: 'Deploy to AWS S3'
        uses: aws-actions/configure-aws-credentials@v4
        with:
          aws-access-key-id: ${{ secrets.AWS_ACCESS_KEY_ID_STAGING }}
          aws-secret-access-key: ${{ secrets.AWS_SECRET_ACCESS_KEY_STAGING }}
          aws-region: us-east-1

      - name: 'Sync to S3 Bucket'
        run: |
          aws s3 sync dist/wesign-client/ s3://${{ secrets.S3_BUCKET_STAGING }}/ \
            --delete \
            --exclude "*.map" \
            --cache-control "public, max-age=31536000" \
            --metadata-directive REPLACE

      - name: 'Invalidate CloudFront Distribution'
        run: |
          aws cloudfront create-invalidation \
            --distribution-id ${{ secrets.CLOUDFRONT_DISTRIBUTION_ID_STAGING }} \
            --paths "/*"

      - name: 'Update Deployment Status'
        run: |
          curl -X POST "${{ secrets.DEPLOYMENT_WEBHOOK_STAGING }}" \
            -H "Content-Type: application/json" \
            -d '{
              "environment": "staging",
              "version": "${{ env.BUILD_VERSION }}",
              "sha": "${{ github.sha }}",
              "status": "deployed",
              "url": "https://staging.wesign.comda.co.il"
            }'

      - name: 'Run Smoke Tests'
        run: |
          npx playwright test \
            e2e/smoke-tests/export-functionality.spec.ts \
            --config=playwright.staging.config.ts

  deploy-production:
    name: 'Deploy to Production'
    runs-on: ubuntu-latest
    timeout-minutes: 15
    needs: [performance-tests, build-container]
    if: github.ref == 'refs/heads/main' || (github.event_name == 'workflow_dispatch' && github.event.inputs.environment == 'production')
    environment:
      name: production
      url: https://app.wesign.comda.co.il

    steps:
      - name: 'Checkout Code'
        uses: actions/checkout@v4

      - name: 'Download Build Artifacts'
        uses: actions/download-artifact@v3
        with:
          name: build-artifacts-production
          path: dist/

      - name: 'Deploy to AWS S3'
        uses: aws-actions/configure-aws-credentials@v4
        with:
          aws-access-key-id: ${{ secrets.AWS_ACCESS_KEY_ID_PRODUCTION }}
          aws-secret-access-key: ${{ secrets.AWS_SECRET_ACCESS_KEY_PRODUCTION }}
          aws-region: us-east-1

      - name: 'Blue/Green Deployment Setup'
        run: |
          # Create new version directory
          NEW_VERSION="v${{ env.BUILD_VERSION }}-${{ github.sha }}"

          # Upload to versioned directory first
          aws s3 sync dist/wesign-client/ s3://${{ secrets.S3_BUCKET_PRODUCTION }}/${NEW_VERSION}/ \
            --cache-control "public, max-age=31536000"

          # Test new version
          curl -f "https://d1234567890.cloudfront.net/${NEW_VERSION}/index.html" || exit 1

          # Switch to new version (atomic update)
          aws s3 sync s3://${{ secrets.S3_BUCKET_PRODUCTION }}/${NEW_VERSION}/ s3://${{ secrets.S3_BUCKET_PRODUCTION }}/ \
            --delete \
            --exclude "v*/*"

      - name: 'Invalidate CloudFront Distribution'
        run: |
          INVALIDATION_ID=$(aws cloudfront create-invalidation \
            --distribution-id ${{ secrets.CLOUDFRONT_DISTRIBUTION_ID_PRODUCTION }} \
            --paths "/*" \
            --query 'Invalidation.Id' \
            --output text)

          # Wait for invalidation to complete
          aws cloudfront wait invalidation-completed \
            --distribution-id ${{ secrets.CLOUDFRONT_DISTRIBUTION_ID_PRODUCTION }} \
            --id $INVALIDATION_ID

      - name: 'Run Production Smoke Tests'
        run: |
          npx playwright test \
            e2e/smoke-tests/export-functionality.spec.ts \
            --config=playwright.production.config.ts \
            --timeout=60000

      - name: 'Update Production Monitoring'
        run: |
          # Update DataDog deployment marker
          curl -X POST "https://api.datadoghq.com/api/v1/events" \
            -H "Content-Type: application/json" \
            -H "DD-API-KEY: ${{ secrets.DATADOG_API_KEY }}" \
            -d '{
              "title": "Export Functionality Deployed",
              "text": "Version ${{ env.BUILD_VERSION }} deployed to production",
              "tags": ["environment:production", "service:wesign-client", "feature:export"],
              "alert_type": "info"
            }'

      - name: 'Notify Stakeholders'
        uses: 8398a7/action-slack@v3
        with:
          status: success
          channel: '#deployments'
          text: |
            🚀 *Export Functionality Deployed to Production*

            Version: `${{ env.BUILD_VERSION }}`
            SHA: `${{ github.sha }}`
            Environment: Production
            URL: https://app.wesign.comda.co.il

            Deployment completed successfully ✅
        env:
          SLACK_WEBHOOK_URL: ${{ secrets.SLACK_WEBHOOK_URL }}

  # ============================================================================
  # NOTIFICATION PHASE
  # ============================================================================

  notify-completion:
    name: 'Notify Pipeline Completion'
    runs-on: ubuntu-latest
    needs: [deploy-staging, deploy-production]
    if: always()

    steps:
      - name: 'Calculate Pipeline Status'
        id: status
        run: |
          if [[ "${{ needs.deploy-staging.result }}" == "success" ]] || [[ "${{ needs.deploy-production.result }}" == "success" ]]; then
            echo "pipeline_status=success" >> $GITHUB_OUTPUT
          else
            echo "pipeline_status=failure" >> $GITHUB_OUTPUT
          fi

      - name: 'Send Teams Notification'
        if: always()
        uses: skitionek/notify-microsoft-teams@master
        with:
          webhook_url: ${{ secrets.TEAMS_WEBHOOK_URL }}
          overwrite: |
            {
              "themeColor": "${{ steps.status.outputs.pipeline_status == 'success' && '00ff00' || 'ff0000' }}",
              "summary": "Export Functionality CI/CD Pipeline",
              "sections": [{
                "activityTitle": "Pipeline ${{ steps.status.outputs.pipeline_status == 'success' && 'Completed' || 'Failed' }}",
                "activitySubtitle": "Export Functionality Deployment",
                "facts": [
                  {"name": "Repository", "value": "${{ github.repository }}"},
                  {"name": "Branch", "value": "${{ github.ref_name }}"},
                  {"name": "Commit", "value": "${{ github.sha }}"},
                  {"name": "Triggered by", "value": "${{ github.actor }}"},
                  {"name": "Status", "value": "${{ steps.status.outputs.pipeline_status }}"}
                ]
              }]
            }

      - name: 'Update JIRA Issues'
        if: steps.status.outputs.pipeline_status == 'success'
        run: |
          # Extract JIRA issue keys from commit messages
          JIRA_ISSUES=$(git log --oneline --since="1 hour ago" | grep -oP '[A-Z]+-\d+' | sort -u || true)

          for issue in $JIRA_ISSUES; do
            curl -X POST \
              -H "Authorization: Basic ${{ secrets.JIRA_AUTH_TOKEN }}" \
              -H "Content-Type: application/json" \
              "${{ secrets.JIRA_BASE_URL }}/rest/api/3/issue/${issue}/comment" \
              -d "{
                \"body\": {
                  \"type\": \"doc\",
                  \"version\": 1,
                  \"content\": [{
                    \"type\": \"paragraph\",
                    \"content\": [{
                      \"type\": \"text\",
                      \"text\": \"Export functionality deployed to production. Version: ${{ env.BUILD_VERSION }}, SHA: ${{ github.sha }}\"
                    }]
                  }]
                }
              }"
          done
```

## 2. Environment-Specific Configuration

### Staging Environment Configuration
```yaml
# .github/environments/staging.yml
name: staging
protection_rules:
  required_reviewers:
    count: 1
    users: [dev-team-lead, qa-lead]
  wait_timer: 0
  prevent_self_review: true

environment_variables:
  API_URL: https://api-staging.wesign.comda.co.il
  CDN_URL: https://cdn-staging.wesign.comda.co.il
  SENTRY_ENVIRONMENT: staging
  LOG_LEVEL: debug
  FEATURE_FLAGS: export-beta-testing,advanced-filters

secrets:
  AWS_ACCESS_KEY_ID_STAGING: ${{ secrets.AWS_ACCESS_KEY_ID_STAGING }}
  AWS_SECRET_ACCESS_KEY_STAGING: ${{ secrets.AWS_SECRET_ACCESS_KEY_STAGING }}
  S3_BUCKET_STAGING: wesign-client-staging
  CLOUDFRONT_DISTRIBUTION_ID_STAGING: E1234567890ABC
  DEPLOYMENT_WEBHOOK_STAGING: https://hooks.staging.wesign.comda.co.il/deploy
```

### Production Environment Configuration
```yaml
# .github/environments/production.yml
name: production
protection_rules:
  required_reviewers:
    count: 2
    users: [tech-lead, product-owner, security-officer]
  wait_timer: 30
  prevent_self_review: true
  required_deployment_branch_policy:
    branches: [main]

environment_variables:
  API_URL: https://api.wesign.comda.co.il
  CDN_URL: https://cdn.wesign.comda.co.il
  SENTRY_ENVIRONMENT: production
  LOG_LEVEL: warn
  FEATURE_FLAGS: export-production

secrets:
  AWS_ACCESS_KEY_ID_PRODUCTION: ${{ secrets.AWS_ACCESS_KEY_ID_PRODUCTION }}
  AWS_SECRET_ACCESS_KEY_PRODUCTION: ${{ secrets.AWS_SECRET_ACCESS_KEY_PRODUCTION }}
  S3_BUCKET_PRODUCTION: wesign-client-production
  CLOUDFRONT_DISTRIBUTION_ID_PRODUCTION: E0987654321ZYX
```

## 3. Docker Configuration

### Enhanced Dockerfile for Export Functionality
```dockerfile
# docker/Dockerfile.export-enhanced
FROM node:18-alpine AS builder

# Build arguments
ARG BUILD_VERSION
ARG BUILD_SHA
ARG BUILD_TIMESTAMP

# Set working directory
WORKDIR /app

# Copy package files
COPY package*.json ./
COPY angular.json ./
COPY tsconfig*.json ./

# Install dependencies
RUN npm ci --only=production --silent

# Copy source code
COPY src/ ./src/
COPY e2e/ ./e2e/

# Build application
ENV NODE_ENV=production
ENV BUILD_VERSION=${BUILD_VERSION}
ENV BUILD_SHA=${BUILD_SHA}
ENV BUILD_TIMESTAMP=${BUILD_TIMESTAMP}

RUN npm run build:production

# Production stage
FROM nginx:1.25-alpine AS production

# Install security updates
RUN apk update && apk upgrade && \
    apk add --no-cache curl

# Copy custom nginx configuration
COPY docker/nginx.conf /etc/nginx/nginx.conf
COPY docker/nginx-export.conf /etc/nginx/conf.d/default.conf

# Copy built application
COPY --from=builder /app/dist/wesign-client /usr/share/nginx/html

# Create health check endpoint
RUN echo '{"status":"healthy","timestamp":"'$(date -u +%Y-%m-%dT%H:%M:%SZ)'"}' > /usr/share/nginx/html/health

# Security hardening
RUN addgroup -g 1001 -S nginx && \
    adduser -S -D -H -u 1001 -h /var/cache/nginx -s /sbin/nologin -G nginx -g nginx nginx && \
    chown -R nginx:nginx /usr/share/nginx/html && \
    chown -R nginx:nginx /var/cache/nginx && \
    chown -R nginx:nginx /var/log/nginx && \
    chown -R nginx:nginx /etc/nginx/conf.d

# Switch to non-root user
USER 1001

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

# Expose port
EXPOSE 8080

# Start nginx
CMD ["nginx", "-g", "daemon off;"]
```

### Nginx Configuration for Export Features
```nginx
# docker/nginx-export.conf
server {
    listen 8080;
    listen [::]:8080;
    server_name _;

    root /usr/share/nginx/html;
    index index.html;

    # Security headers
    add_header X-Frame-Options DENY always;
    add_header X-Content-Type-Options nosniff always;
    add_header X-XSS-Protection "1; mode=block" always;
    add_header Referrer-Policy "strict-origin-when-cross-origin" always;
    add_header Permissions-Policy "camera=(), microphone=(), geolocation=()" always;

    # CSP for export functionality
    add_header Content-Security-Policy "
        default-src 'self';
        script-src 'self' 'unsafe-inline' 'unsafe-eval' *.wesign.comda.co.il;
        style-src 'self' 'unsafe-inline' fonts.googleapis.com;
        font-src 'self' fonts.gstatic.com;
        img-src 'self' data: blob: *.wesign.comda.co.il;
        connect-src 'self' *.wesign.comda.co.il wss://*.wesign.comda.co.il;
        frame-src 'none';
        object-src 'none';
        base-uri 'self';
        form-action 'self';
    " always;

    # Gzip compression
    gzip on;
    gzip_vary on;
    gzip_min_length 1024;
    gzip_proxied any;
    gzip_comp_level 6;
    gzip_types
        application/atom+xml
        application/geo+json
        application/javascript
        application/x-javascript
        application/json
        application/ld+json
        application/manifest+json
        application/rdf+xml
        application/rss+xml
        application/xhtml+xml
        application/xml
        font/eot
        font/otf
        font/ttf
        image/svg+xml
        text/css
        text/javascript
        text/plain
        text/xml;

    # Export-specific file handling
    location ~* \.(pdf|xlsx|csv|json|xml)$ {
        expires 1h;
        add_header Cache-Control "public, no-transform";
        add_header X-Content-Type-Options nosniff;

        # Prevent direct access to generated files
        location ~* /tmp/ {
            deny all;
            return 404;
        }
    }

    # Static assets caching
    location ~* \.(js|css|png|jpg|jpeg|gif|ico|svg|woff|woff2|ttf|eot)$ {
        expires 1y;
        add_header Cache-Control "public, immutable";
        access_log off;
    }

    # Angular routing
    location / {
        try_files $uri $uri/ /index.html;

        # Cache control for HTML
        location ~* \.html$ {
            expires 1h;
            add_header Cache-Control "public, must-revalidate";
        }
    }

    # Health check endpoint
    location /health {
        access_log off;
        return 200 "healthy\n";
        add_header Content-Type text/plain;
    }

    # API proxy for export endpoints
    location /api/exports {
        proxy_pass http://backend-api;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;

        # Timeout configuration for large exports
        proxy_connect_timeout 10s;
        proxy_send_timeout 300s;
        proxy_read_timeout 300s;

        # Buffer configuration
        proxy_buffering on;
        proxy_buffer_size 128k;
        proxy_buffers 4 256k;
        proxy_busy_buffers_size 256k;
    }

    # Error pages
    error_page 404 /404.html;
    error_page 500 502 503 504 /50x.html;

    location = /404.html {
        internal;
    }

    location = /50x.html {
        internal;
    }
}
```

## 4. Performance Testing Configuration

### Lighthouse CI Configuration
```json
// lighthouserc.json
{
  "ci": {
    "collect": {
      "url": [
        "http://localhost:4200/analytics-dashboard",
        "http://localhost:4200/analytics-dashboard?export-dialog=true"
      ],
      "numberOfRuns": 3,
      "settings": {
        "chromeFlags": "--disable-gpu --no-sandbox --no-first-run --disable-dev-shm-usage",
        "preset": "desktop",
        "throttling": {
          "rttMs": 40,
          "throughputKbps": 10240,
          "cpuSlowdownMultiplier": 1,
          "requestLatencyMs": 0,
          "downloadThroughputKbps": 0,
          "uploadThroughputKbps": 0
        }
      }
    },
    "assert": {
      "assertions": {
        "categories:performance": ["error", {"minScore": 0.9}],
        "categories:accessibility": ["error", {"minScore": 0.95}],
        "categories:best-practices": ["error", {"minScore": 0.9}],
        "categories:seo": ["error", {"minScore": 0.8}],
        "first-contentful-paint": ["error", {"maxNumericValue": 2000}],
        "largest-contentful-paint": ["error", {"maxNumericValue": 3000}],
        "cumulative-layout-shift": ["error", {"maxNumericValue": 0.1}],
        "total-blocking-time": ["error", {"maxNumericValue": 300}]
      }
    },
    "upload": {
      "target": "filesystem",
      "outputDir": "./lighthouse-results"
    }
  }
}
```

### Artillery Load Testing Configuration
```yaml
# performance-tests/export-load-test.yml
config:
  target: 'http://localhost:4200'
  phases:
    - duration: 60
      arrivalRate: 5
      name: "Warm up"
    - duration: 180
      arrivalRate: 10
      name: "Ramp up load"
    - duration: 300
      arrivalRate: 20
      name: "Sustained load"
    - duration: 120
      arrivalRate: 30
      name: "Peak load"
  payload:
    path: './export-test-data.csv'
    fields:
      - userId
      - dataSource
      - format
  processor: './export-load-processor.js'

scenarios:
  - name: "Export Dialog Load Test"
    weight: 60
    flow:
      - get:
          url: "/analytics-dashboard"
          headers:
            Authorization: "Bearer {{ $processEnvironment.TEST_AUTH_TOKEN }}"
      - think: 2
      - post:
          url: "/api/exports/estimate"
          json:
            filters: {}
            dateRange:
              startDate: "2024-01-01"
              endDate: "2024-01-31"
          capture:
            - json: "$.sizeBytes"
              as: estimatedSize
      - think: 3
      - post:
          url: "/api/exports/initiate"
          json:
            format: "{{ format }}"
            dateRange:
              startDate: "2024-01-01"
              endDate: "2024-01-31"
            filters: {}
            deliveryOptions:
              method: "download"
          capture:
            - json: "$.id"
              as: jobId
      - think: 5
      - get:
          url: "/api/exports/{{ jobId }}/progress"
          capture:
            - json: "$.percentage"
              as: progress

  - name: "Large Export Test"
    weight: 20
    flow:
      - post:
          url: "/api/exports/initiate"
          json:
            format: "Excel"
            dateRange:
              startDate: "2023-01-01"
              endDate: "2024-01-31"
            filters: {}
            deliveryOptions:
              method: "email"
              emailRecipients: ["test@example.com"]

  - name: "Template Management Test"
    weight: 20
    flow:
      - get:
          url: "/api/exports/templates"
      - post:
          url: "/api/exports/templates"
          json:
            name: "Load Test Template"
            config:
              format: "PDF"
              filters: {}
```

## 5. Security and Compliance

### Security Scanning Configuration
```yaml
# .github/workflows/security-scan.yml
name: Security Scan

on:
  schedule:
    - cron: '0 2 * * 1'  # Weekly on Monday at 2 AM
  workflow_dispatch:

jobs:
  dependency-check:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - name: Run OWASP Dependency Check
        uses: dependency-check/Dependency-Check_Action@main
        with:
          project: 'wesign-export-functionality'
          path: '.'
          format: 'ALL'
          args: >
            --enableRetired
            --enableExperimental
            --out reports
            --exclude node_modules
      - name: Upload Results
        uses: actions/upload-artifact@v3
        with:
          name: dependency-check-report
          path: reports/

  secret-scan:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
        with:
          fetch-depth: 0
      - name: TruffleHog OSS
        uses: trufflesecurity/trufflehog@main
        with:
          path: ./
          base: main
          head: HEAD
          extra_args: --debug --only-verified
```

## 6. Monitoring and Alerting

### DataDog Monitoring Configuration
```yaml
# monitoring/datadog-export-monitoring.yml
apiVersion: v1
kind: ConfigMap
metadata:
  name: datadog-export-config
data:
  export-functionality.yaml: |
    logs:
      - type: file
        path: /var/log/nginx/access.log
        service: wesign-client
        source: nginx
        tags:
          - component:export
          - environment:production

      - type: file
        path: /var/log/nginx/error.log
        service: wesign-client
        source: nginx
        tags:
          - component:export
          - environment:production
          - level:error

    init_config:

    instances:
      - url: https://app.wesign.comda.co.il/health
        timeout: 10
        tags:
          - service:wesign-client
          - component:export

      - url: https://app.wesign.comda.co.il/api/exports/health
        timeout: 15
        tags:
          - service:export-api
          - component:backend
```

### Alerting Rules
```json
{
  "alerts": [
    {
      "name": "Export Functionality High Error Rate",
      "query": "sum(rate(export_errors_total[5m])) / sum(rate(export_requests_total[5m])) > 0.05",
      "for": "2m",
      "labels": {
        "severity": "warning",
        "component": "export"
      },
      "annotations": {
        "summary": "Export functionality error rate is above 5%",
        "description": "The export functionality is experiencing a high error rate of {{ $value | humanizePercentage }}."
      }
    },
    {
      "name": "Export Processing Time High",
      "query": "histogram_quantile(0.95, rate(export_processing_duration_seconds_bucket[5m])) > 300",
      "for": "5m",
      "labels": {
        "severity": "warning",
        "component": "export"
      },
      "annotations": {
        "summary": "Export processing time is high",
        "description": "95th percentile export processing time is {{ $value }}s."
      }
    },
    {
      "name": "Export Queue Length High",
      "query": "export_queue_length > 100",
      "for": "3m",
      "labels": {
        "severity": "critical",
        "component": "export"
      },
      "annotations": {
        "summary": "Export queue is getting long",
        "description": "Export queue has {{ $value }} pending jobs."
      }
    }
  ]
}
```

This comprehensive CI/CD configuration ensures robust deployment pipelines with automated testing, security scanning, performance validation, and monitoring for the Export functionality.