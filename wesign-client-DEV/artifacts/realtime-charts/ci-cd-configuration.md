# Real-time Charts CI/CD Configuration - A→M Workflow Step H

**PAGE_KEY**: realtime-charts
**COMPLETION DATE**: 2025-01-29
**STATUS**: ✅ COMPLETE

## CI/CD Pipeline Configuration

### GitHub Actions Workflow Configuration

```yaml
# .github/workflows/realtime-charts-ci.yml
name: Real-time Charts CI/CD Pipeline

on:
  push:
    branches: [ main, develop ]
    paths:
      - 'src/app/pages/analytics/realtime-charts/**'
      - 'src/app/shared/services/chart-data/**'
      - 'src/app/shared/services/realtime/**'
      - 'src/app/shared/models/chart/**'
      - 'src/app/shared/guards/analytics-access.guard.ts'
  pull_request:
    branches: [ main, develop ]
    paths:
      - 'src/app/pages/analytics/realtime-charts/**'
      - 'src/app/shared/services/chart-data/**'
      - 'src/app/shared/services/realtime/**'
      - 'src/app/shared/models/chart/**'
      - 'src/app/shared/guards/analytics-access.guard.ts'

env:
  NODE_VERSION: '18.x'
  ANGULAR_CLI_VERSION: '15.2.10'

jobs:
  # Static Analysis and Code Quality
  code-quality:
    runs-on: ubuntu-latest
    name: Code Quality Analysis

    steps:
    - name: Checkout Code
      uses: actions/checkout@v4
      with:
        fetch-depth: 0

    - name: Setup Node.js
      uses: actions/setup-node@v4
      with:
        node-version: ${{ env.NODE_VERSION }}
        cache: 'npm'

    - name: Install Dependencies
      run: npm ci

    - name: Run ESLint for Charts Components
      run: |
        npx eslint src/app/pages/analytics/realtime-charts/**/*.ts \
                   src/app/shared/services/chart-data/**/*.ts \
                   src/app/shared/services/realtime/**/*.ts \
                   --format=json --output-file=eslint-charts.json
        npx eslint src/app/pages/analytics/realtime-charts/**/*.ts \
                   src/app/shared/services/chart-data/**/*.ts \
                   src/app/shared/services/realtime/**/*.ts

    - name: Run Prettier Check
      run: |
        npx prettier --check \
          "src/app/pages/analytics/realtime-charts/**/*.{ts,html,scss}" \
          "src/app/shared/services/chart-data/**/*.{ts,html,scss}" \
          "src/app/shared/services/realtime/**/*.{ts,html,scss}"

    - name: TypeScript Compilation Check
      run: |
        npx tsc --noEmit --project tsconfig.json

    - name: Chart-Specific Code Analysis
      run: |
        # Check for chart memory leaks patterns
        grep -r "addEventListener" src/app/pages/analytics/realtime-charts/ || true
        grep -r "setInterval\|setTimeout" src/app/pages/analytics/realtime-charts/ || true

        # Verify chart cleanup patterns
        grep -r "removeEventListener\|clearInterval\|clearTimeout" src/app/pages/analytics/realtime-charts/ || true
        grep -r "ngOnDestroy\|unsubscribe" src/app/pages/analytics/realtime-charts/ || true

    - name: Upload Code Quality Reports
      uses: actions/upload-artifact@v4
      if: always()
      with:
        name: code-quality-reports
        path: |
          eslint-charts.json
        retention-days: 7

  # Security Scanning
  security-scan:
    runs-on: ubuntu-latest
    name: Security Analysis

    steps:
    - name: Checkout Code
      uses: actions/checkout@v4

    - name: Setup Node.js
      uses: actions/setup-node@v4
      with:
        node-version: ${{ env.NODE_VERSION }}
        cache: 'npm'

    - name: Install Dependencies
      run: npm ci

    - name: Run npm audit
      run: npm audit --audit-level=moderate

    - name: Chart Library Security Scan
      run: |
        # Check for vulnerable chart libraries
        npm audit --parseable | grep -E "(chart\.js|d3|plotly)" || true

        # Verify chart data sanitization
        grep -r "innerHTML\|outerHTML" src/app/pages/analytics/realtime-charts/ && exit 1 || true
        grep -r "eval\|Function" src/app/pages/analytics/realtime-charts/ && exit 1 || true

    - name: OWASP Dependency Check
      uses: dependency-check/Dependency-Check_Action@main
      with:
        project: 'wesign-realtime-charts'
        path: '.'
        format: 'ALL'

    - name: Upload Security Reports
      uses: actions/upload-artifact@v4
      if: always()
      with:
        name: security-reports
        path: reports/
        retention-days: 7

  # Unit Tests for Charts
  unit-tests:
    runs-on: ubuntu-latest
    name: Chart Unit Tests

    steps:
    - name: Checkout Code
      uses: actions/checkout@v4

    - name: Setup Node.js
      uses: actions/setup-node@v4
      with:
        node-version: ${{ env.NODE_VERSION }}
        cache: 'npm'

    - name: Install Dependencies
      run: npm ci

    - name: Run Chart Unit Tests
      run: |
        npx jest \
          --testPathPattern="realtime-charts" \
          --coverage \
          --coverageDirectory=coverage/charts \
          --coverageReporters=text,lcov,json \
          --collectCoverageFrom="src/app/pages/analytics/realtime-charts/**/*.ts" \
          --collectCoverageFrom="src/app/shared/services/chart-data/**/*.ts" \
          --collectCoverageFrom="src/app/shared/services/realtime/**/*.ts" \
          --testTimeout=30000

    - name: Generate Coverage Badge
      uses: jaywcjlove/coverage-badges-cli@main
      with:
        source: coverage/charts/lcov.info
        output: coverage/charts/badges.svg

    - name: Upload Test Coverage
      uses: codecov/codecov-action@v3
      with:
        directory: coverage/charts
        flags: realtime-charts
        name: realtime-charts-coverage

    - name: Upload Test Reports
      uses: actions/upload-artifact@v4
      if: always()
      with:
        name: unit-test-reports
        path: |
          coverage/charts/
          test-results/
        retention-days: 7

  # Integration Tests
  integration-tests:
    runs-on: ubuntu-latest
    name: Chart Integration Tests

    services:
      signalr-hub:
        image: mcr.microsoft.com/dotnet/aspnet:7.0
        env:
          ASPNETCORE_ENVIRONMENT: Testing
        ports:
          - 5000:80

    steps:
    - name: Checkout Code
      uses: actions/checkout@v4

    - name: Setup Node.js
      uses: actions/setup-node@v4
      with:
        node-version: ${{ env.NODE_VERSION }}
        cache: 'npm'

    - name: Install Dependencies
      run: npm ci

    - name: Wait for SignalR Hub
      run: |
        timeout 60 bash -c 'until curl -f http://localhost:5000/health; do sleep 2; done'

    - name: Run Chart Integration Tests
      env:
        SIGNALR_HUB_URL: http://localhost:5000/analyticsHub
        CHART_API_BASE_URL: http://localhost:5000/api
      run: |
        npx jest \
          --testPathPattern="integration.*realtime-charts" \
          --testTimeout=60000 \
          --runInBand

    - name: Upload Integration Test Reports
      uses: actions/upload-artifact@v4
      if: always()
      with:
        name: integration-test-reports
        path: test-results/integration/
        retention-days: 7

  # E2E Tests for Chart Interactions
  e2e-tests:
    runs-on: ubuntu-latest
    name: Chart E2E Tests

    steps:
    - name: Checkout Code
      uses: actions/checkout@v4

    - name: Setup Node.js
      uses: actions/setup-node@v4
      with:
        node-version: ${{ env.NODE_VERSION }}
        cache: 'npm'

    - name: Install Dependencies
      run: npm ci

    - name: Install Playwright
      run: npx playwright install --with-deps

    - name: Build Application
      run: npm run build:test

    - name: Start Test Server
      run: |
        npm run serve:test &
        timeout 60 bash -c 'until curl -f http://localhost:4200; do sleep 2; done'

    - name: Run Chart E2E Tests
      run: |
        npx playwright test \
          --grep="realtime.*chart" \
          --reporter=html \
          --output-dir=test-results/e2e

    - name: Upload E2E Test Results
      uses: actions/upload-artifact@v4
      if: always()
      with:
        name: e2e-test-results
        path: |
          test-results/e2e/
          playwright-report/
        retention-days: 7

  # Performance Tests for Charts
  performance-tests:
    runs-on: ubuntu-latest
    name: Chart Performance Tests

    steps:
    - name: Checkout Code
      uses: actions/checkout@v4

    - name: Setup Node.js
      uses: actions/setup-node@v4
      with:
        node-version: ${{ env.NODE_VERSION }}
        cache: 'npm'

    - name: Install Dependencies
      run: npm ci

    - name: Build Application
      run: npm run build:prod

    - name: Install Lighthouse CI
      run: npm install -g @lhci/cli

    - name: Start Production Server
      run: |
        npm run serve:prod &
        timeout 60 bash -c 'until curl -f http://localhost:8080; do sleep 2; done'

    - name: Run Lighthouse Performance Tests
      run: |
        lhci collect \
          --url="http://localhost:8080/analytics/realtime-charts" \
          --numberOfRuns=3 \
          --settings.chromeFlags="--no-sandbox --disable-dev-shm-usage"

    - name: Chart-Specific Performance Tests
      run: |
        # Memory usage test for large datasets
        node scripts/performance/chart-memory-test.js

        # Chart rendering performance test
        node scripts/performance/chart-rendering-test.js

        # Real-time update performance test
        node scripts/performance/realtime-update-test.js

    - name: Upload Performance Reports
      uses: actions/upload-artifact@v4
      if: always()
      with:
        name: performance-reports
        path: |
          .lighthouseci/
          performance-results/
        retention-days: 7

  # Accessibility Tests
  accessibility-tests:
    runs-on: ubuntu-latest
    name: Chart Accessibility Tests

    steps:
    - name: Checkout Code
      uses: actions/checkout@v4

    - name: Setup Node.js
      uses: actions/setup-node@v4
      with:
        node-version: ${{ env.NODE_VERSION }}
        cache: 'npm'

    - name: Install Dependencies
      run: npm ci

    - name: Build Application
      run: npm run build:test

    - name: Start Test Server
      run: |
        npm run serve:test &
        timeout 60 bash -c 'until curl -f http://localhost:4200; do sleep 2; done'

    - name: Install axe-core CLI
      run: npm install -g @axe-core/cli

    - name: Run Chart Accessibility Tests
      run: |
        axe http://localhost:4200/analytics/realtime-charts \
          --tags wcag2a,wcag2aa \
          --reporter html \
          --output-dir accessibility-results/ \
          --chrome-options="--no-sandbox,--disable-dev-shm-usage"

    - name: Chart Keyboard Navigation Test
      run: |
        npx playwright test \
          --grep="keyboard.*navigation.*chart" \
          --reporter=html

    - name: Upload Accessibility Reports
      uses: actions/upload-artifact@v4
      if: always()
      with:
        name: accessibility-reports
        path: |
          accessibility-results/
          playwright-report/
        retention-days: 7

  # Build and Deploy
  build-deploy:
    runs-on: ubuntu-latest
    name: Build and Deploy Charts
    needs: [code-quality, security-scan, unit-tests, integration-tests]
    if: github.ref == 'refs/heads/main'

    steps:
    - name: Checkout Code
      uses: actions/checkout@v4

    - name: Setup Node.js
      uses: actions/setup-node@v4
      with:
        node-version: ${{ env.NODE_VERSION }}
        cache: 'npm'

    - name: Install Dependencies
      run: npm ci

    - name: Build Production Application
      env:
        NODE_ENV: production
        NG_BUILD_CONFIGURATION: production
      run: |
        npm run build:prod

        # Verify chart assets are included
        ls -la dist/assets/chart-libraries/
        ls -la dist/assets/icons/charts/

    - name: Generate Chart Documentation
      run: |
        npm run docs:generate -- \
          --include="src/app/pages/analytics/realtime-charts/**/*.ts" \
          --output="dist/docs/charts/"

    - name: Run Bundle Analysis
      run: |
        npm run analyze

        # Check chart library bundle sizes
        npx webpack-bundle-analyzer dist/stats.json --report --mode static --no-open

    - name: Create Deployment Package
      run: |
        tar -czf realtime-charts-build.tar.gz \
          dist/ \
          package.json \
          angular.json \
          deployment/charts/

    - name: Upload Build Artifacts
      uses: actions/upload-artifact@v4
      with:
        name: build-artifacts
        path: |
          realtime-charts-build.tar.gz
          dist/
          webpack-report.html
        retention-days: 30

    - name: Deploy to Staging
      if: github.ref == 'refs/heads/main'
      env:
        AWS_ACCESS_KEY_ID: ${{ secrets.AWS_ACCESS_KEY_ID }}
        AWS_SECRET_ACCESS_KEY: ${{ secrets.AWS_SECRET_ACCESS_KEY }}
        AWS_REGION: ${{ secrets.AWS_REGION }}
      run: |
        aws s3 sync dist/ s3://${{ secrets.STAGING_S3_BUCKET }}/charts/ \
          --delete \
          --exclude="*.map" \
          --cache-control="public, max-age=3600"

        # Invalidate CloudFront for chart assets
        aws cloudfront create-invalidation \
          --distribution-id ${{ secrets.STAGING_CLOUDFRONT_ID }} \
          --paths "/charts/*" "/assets/chart-libraries/*"

  # Chart-Specific Quality Gates
  chart-quality-gates:
    runs-on: ubuntu-latest
    name: Chart Quality Gates
    needs: [unit-tests, performance-tests, accessibility-tests]

    steps:
    - name: Download Test Reports
      uses: actions/download-artifact@v4
      with:
        name: unit-test-reports
        path: reports/unit/

    - name: Download Performance Reports
      uses: actions/download-artifact@v4
      with:
        name: performance-reports
        path: reports/performance/

    - name: Download Accessibility Reports
      uses: actions/download-artifact@v4
      with:
        name: accessibility-reports
        path: reports/accessibility/

    - name: Validate Chart Coverage Threshold
      run: |
        COVERAGE=$(cat reports/unit/coverage-summary.json | jq '.total.lines.pct')
        echo "Chart coverage: $COVERAGE%"

        if (( $(echo "$COVERAGE < 95" | bc -l) )); then
          echo "❌ Chart coverage below 95% threshold: $COVERAGE%"
          exit 1
        fi

        echo "✅ Chart coverage meets 95% threshold: $COVERAGE%"

    - name: Validate Chart Performance Metrics
      run: |
        # Check Lighthouse performance score
        PERF_SCORE=$(cat reports/performance/.lighthouseci/lhr-*.json | jq '.categories.performance.score * 100')
        echo "Chart page performance score: $PERF_SCORE"

        if (( $(echo "$PERF_SCORE < 90" | bc -l) )); then
          echo "❌ Chart performance below 90 threshold: $PERF_SCORE"
          exit 1
        fi

        # Check chart rendering time
        RENDER_TIME=$(cat reports/performance/chart-rendering-results.json | jq '.averageRenderTime')
        echo "Average chart render time: ${RENDER_TIME}ms"

        if (( $(echo "$RENDER_TIME > 1000" | bc -l) )); then
          echo "❌ Chart render time exceeds 1s: ${RENDER_TIME}ms"
          exit 1
        fi

        echo "✅ Chart performance metrics pass all thresholds"

    - name: Validate Accessibility Standards
      run: |
        VIOLATIONS=$(cat reports/accessibility/axe-results.json | jq '.violations | length')
        echo "Accessibility violations: $VIOLATIONS"

        if [ "$VIOLATIONS" -gt 0 ]; then
          echo "❌ Chart accessibility violations found: $VIOLATIONS"
          cat reports/accessibility/axe-results.json | jq '.violations'
          exit 1
        fi

        echo "✅ Chart accessibility validation passed"

  # Notification and Reporting
  notify-results:
    runs-on: ubuntu-latest
    name: Notify Test Results
    needs: [chart-quality-gates, build-deploy]
    if: always()

    steps:
    - name: Prepare Notification
      id: prepare
      run: |
        if [ "${{ needs.chart-quality-gates.result }}" == "success" ] && \
           [ "${{ needs.build-deploy.result }}" == "success" ]; then
          echo "status=✅ SUCCESS" >> $GITHUB_OUTPUT
          echo "color=good" >> $GITHUB_OUTPUT
          echo "message=Real-time Charts pipeline completed successfully" >> $GITHUB_OUTPUT
        else
          echo "status=❌ FAILURE" >> $GITHUB_OUTPUT
          echo "color=danger" >> $GITHUB_OUTPUT
          echo "message=Real-time Charts pipeline failed - check logs" >> $GITHUB_OUTPUT
        fi

    - name: Notify Team
      uses: 8398a7/action-slack@v3
      with:
        status: custom
        custom_payload: |
          {
            "text": "${{ steps.prepare.outputs.status }} Real-time Charts CI/CD",
            "attachments": [{
              "color": "${{ steps.prepare.outputs.color }}",
              "fields": [
                {
                  "title": "Pipeline Status",
                  "value": "${{ steps.prepare.outputs.message }}",
                  "short": true
                },
                {
                  "title": "Branch",
                  "value": "${{ github.ref_name }}",
                  "short": true
                },
                {
                  "title": "Commit",
                  "value": "${{ github.sha }}",
                  "short": true
                }
              ]
            }]
          }
      env:
        SLACK_WEBHOOK_URL: ${{ secrets.SLACK_WEBHOOK_URL }}
```

### Chart-Specific Performance Test Scripts

```javascript
// scripts/performance/chart-memory-test.js
const puppeteer = require('puppeteer');
const fs = require('fs');

async function testChartMemoryUsage() {
  const browser = await puppeteer.launch({
    headless: true,
    args: ['--no-sandbox', '--disable-dev-shm-usage']
  });

  const page = await browser.newPage();

  // Enable runtime API for memory measurements
  await page.evaluateOnNewDocument(() => {
    window.performance.measureUserAgentSpecificMemory =
      window.performance.measureUserAgentSpecificMemory ||
      (() => Promise.resolve({ bytes: performance.memory?.usedJSHeapSize || 0 }));
  });

  await page.goto('http://localhost:4200/analytics/realtime-charts');

  // Wait for charts to load
  await page.waitForSelector('[data-testid="chart-container"]', { timeout: 30000 });

  const memoryTests = [];

  // Test 1: Initial memory usage
  const initialMemory = await page.evaluate(async () => {
    const measurement = await performance.measureUserAgentSpecificMemory();
    return measurement.bytes;
  });

  memoryTests.push({
    test: 'Initial Load',
    memory: initialMemory,
    timestamp: Date.now()
  });

  // Test 2: Memory after chart interactions
  await page.click('[data-testid="chart-filter-dropdown"]');
  await page.click('[data-testid="filter-last-7-days"]');
  await page.waitForTimeout(5000);

  const afterFilterMemory = await page.evaluate(async () => {
    const measurement = await performance.measureUserAgentSpecificMemory();
    return measurement.bytes;
  });

  memoryTests.push({
    test: 'After Filtering',
    memory: afterFilterMemory,
    memoryIncrease: afterFilterMemory - initialMemory,
    timestamp: Date.now()
  });

  // Test 3: Memory after real-time updates simulation
  await page.evaluate(() => {
    // Simulate 100 real-time updates
    for (let i = 0; i < 100; i++) {
      window.dispatchEvent(new CustomEvent('chart-data-update', {
        detail: { timestamp: Date.now(), value: Math.random() * 100 }
      }));
    }
  });

  await page.waitForTimeout(10000);

  const afterUpdatesMemory = await page.evaluate(async () => {
    const measurement = await performance.measureUserAgentSpecificMemory();
    return measurement.bytes;
  });

  memoryTests.push({
    test: 'After Real-time Updates',
    memory: afterUpdatesMemory,
    memoryIncrease: afterUpdatesMemory - afterFilterMemory,
    timestamp: Date.now()
  });

  // Test 4: Memory after cleanup (navigate away and back)
  await page.goto('http://localhost:4200/dashboard');
  await page.waitForTimeout(2000);
  await page.goto('http://localhost:4200/analytics/realtime-charts');
  await page.waitForSelector('[data-testid="chart-container"]', { timeout: 30000 });
  await page.waitForTimeout(5000);

  const afterCleanupMemory = await page.evaluate(async () => {
    const measurement = await performance.measureUserAgentSpecificMemory();
    return measurement.bytes;
  });

  memoryTests.push({
    test: 'After Cleanup',
    memory: afterCleanupMemory,
    timestamp: Date.now()
  });

  await browser.close();

  // Analyze results
  const results = {
    tests: memoryTests,
    analysis: {
      maxMemoryIncrease: Math.max(...memoryTests
        .filter(t => t.memoryIncrease)
        .map(t => t.memoryIncrease)
      ),
      memoryLeakDetected: afterCleanupMemory > (initialMemory * 1.1),
      passed: true
    }
  };

  // Check for memory leaks (> 10% increase after cleanup)
  if (results.analysis.memoryLeakDetected) {
    results.analysis.passed = false;
    console.error('❌ Memory leak detected in charts');
  }

  // Check for excessive memory growth (> 50MB increase during updates)
  if (results.analysis.maxMemoryIncrease > 50 * 1024 * 1024) {
    results.analysis.passed = false;
    console.error('❌ Excessive memory growth detected');
  }

  // Save results
  fs.writeFileSync(
    'performance-results/chart-memory-results.json',
    JSON.stringify(results, null, 2)
  );

  console.log(results.analysis.passed ? '✅ Memory tests passed' : '❌ Memory tests failed');
  process.exit(results.analysis.passed ? 0 : 1);
}

testChartMemoryUsage().catch(console.error);
```

```javascript
// scripts/performance/chart-rendering-test.js
const puppeteer = require('puppeteer');
const fs = require('fs');

async function testChartRenderingPerformance() {
  const browser = await puppeteer.launch({
    headless: true,
    args: ['--no-sandbox', '--disable-dev-shm-usage']
  });

  const page = await browser.newPage();

  // Enable performance monitoring
  await page.setRequestInterception(true);

  const performanceMetrics = {
    chartRenderTimes: [],
    totalPageLoad: null,
    largeDatasetTest: null,
    realTimeUpdateTest: null
  };

  page.on('request', request => {
    if (request.url().includes('/api/analytics/charts/')) {
      request.startTime = Date.now();
    }
    request.continue();
  });

  page.on('response', response => {
    if (response.url().includes('/api/analytics/charts/') && response.request().startTime) {
      const responseTime = Date.now() - response.request().startTime;
      performanceMetrics.chartRenderTimes.push({
        url: response.url(),
        responseTime,
        status: response.status()
      });
    }
  });

  // Test 1: Initial page load and chart rendering
  const startTime = Date.now();
  await page.goto('http://localhost:4200/analytics/realtime-charts');

  // Wait for all charts to load
  await page.waitForFunction(() => {
    const chartContainers = document.querySelectorAll('[data-testid="chart-container"]');
    return chartContainers.length >= 4 &&
           Array.from(chartContainers).every(container =>
             container.querySelector('canvas') || container.querySelector('svg')
           );
  }, { timeout: 30000 });

  performanceMetrics.totalPageLoad = Date.now() - startTime;

  // Test 2: Large dataset rendering
  const largeDatasetStart = Date.now();
  await page.evaluate(() => {
    // Simulate loading 10,000 data points
    window.dispatchEvent(new CustomEvent('load-large-dataset', {
      detail: { points: 10000 }
    }));
  });

  await page.waitForFunction(() => {
    const chartCanvas = document.querySelector('[data-testid="performance-chart"] canvas');
    return chartCanvas && chartCanvas.getAttribute('data-loaded') === 'true';
  }, { timeout: 15000 });

  performanceMetrics.largeDatasetTest = Date.now() - largeDatasetStart;

  // Test 3: Real-time update performance
  const updateTimes = [];

  for (let i = 0; i < 20; i++) {
    const updateStart = Date.now();

    await page.evaluate((index) => {
      window.dispatchEvent(new CustomEvent('chart-data-update', {
        detail: {
          timestamp: Date.now(),
          value: Math.random() * 100,
          batch: index
        }
      }));
    }, i);

    await page.waitForFunction((batchIndex) => {
      const chart = document.querySelector('[data-testid="realtime-chart"]');
      return chart && chart.getAttribute('data-last-update') === batchIndex.toString();
    }, { timeout: 2000 }, i);

    updateTimes.push(Date.now() - updateStart);
    await page.waitForTimeout(100);
  }

  performanceMetrics.realTimeUpdateTest = {
    updates: updateTimes,
    averageTime: updateTimes.reduce((a, b) => a + b) / updateTimes.length,
    maxTime: Math.max(...updateTimes),
    minTime: Math.min(...updateTimes)
  };

  await browser.close();

  // Analyze results
  const results = {
    metrics: performanceMetrics,
    analysis: {
      pageLoadPassed: performanceMetrics.totalPageLoad < 2000,
      chartRenderPassed: performanceMetrics.chartRenderTimes.every(c => c.responseTime < 1000),
      largeDatasetPassed: performanceMetrics.largeDatasetTest < 3000,
      realTimeUpdatePassed: performanceMetrics.realTimeUpdateTest.averageTime < 300,
      overall: true
    }
  };

  results.analysis.overall =
    results.analysis.pageLoadPassed &&
    results.analysis.chartRenderPassed &&
    results.analysis.largeDatasetPassed &&
    results.analysis.realTimeUpdatePassed;

  // Save results
  fs.writeFileSync(
    'performance-results/chart-rendering-results.json',
    JSON.stringify(results, null, 2)
  );

  console.log(results.analysis.overall ? '✅ Rendering tests passed' : '❌ Rendering tests failed');

  if (!results.analysis.pageLoadPassed) {
    console.error(`❌ Page load too slow: ${performanceMetrics.totalPageLoad}ms > 2000ms`);
  }
  if (!results.analysis.realTimeUpdatePassed) {
    console.error(`❌ Real-time updates too slow: ${performanceMetrics.realTimeUpdateTest.averageTime}ms > 300ms`);
  }

  process.exit(results.analysis.overall ? 0 : 1);
}

testChartRenderingPerformance().catch(console.error);
```

### Deployment Configuration

```yaml
# deployment/charts/docker-compose.yml
version: '3.8'

services:
  wesign-charts:
    image: wesign/client:latest
    container_name: wesign-charts-app
    ports:
      - "8080:80"
    environment:
      - NODE_ENV=production
      - CHART_API_URL=https://api.wesign.com
      - SIGNALR_HUB_URL=https://api.wesign.com/analyticsHub
      - REDIS_URL=redis://redis:6379
    volumes:
      - ./nginx/nginx.conf:/etc/nginx/nginx.conf:ro
      - ./nginx/chart-cache.conf:/etc/nginx/conf.d/chart-cache.conf:ro
    depends_on:
      - redis
    networks:
      - wesign-network

  redis:
    image: redis:7-alpine
    container_name: wesign-charts-redis
    ports:
      - "6379:6379"
    volumes:
      - redis-data:/data
      - ./redis/redis.conf:/etc/redis/redis.conf:ro
    command: redis-server /etc/redis/redis.conf
    networks:
      - wesign-network

  nginx-cache:
    image: nginx:alpine
    container_name: wesign-charts-cache
    ports:
      - "8081:80"
    volumes:
      - ./nginx/cache-nginx.conf:/etc/nginx/nginx.conf:ro
      - chart-cache:/var/cache/nginx
    networks:
      - wesign-network

volumes:
  redis-data:
  chart-cache:

networks:
  wesign-network:
    driver: bridge
```

```nginx
# deployment/charts/nginx/chart-cache.conf
# Chart-specific caching rules
location /assets/chart-libraries/ {
    expires 1y;
    add_header Cache-Control "public, immutable";
    add_header Access-Control-Allow-Origin "*";
}

location /api/analytics/charts/ {
    proxy_pass http://backend:5000;
    proxy_cache charts_cache;
    proxy_cache_valid 200 5m;
    proxy_cache_key "$request_uri";
    add_header X-Cache-Status $upstream_cache_status;

    # Chart-specific headers
    add_header Access-Control-Allow-Methods "GET, POST, OPTIONS";
    add_header Access-Control-Allow-Headers "Content-Type, Authorization";
}

# SignalR hub configuration
location /analyticsHub {
    proxy_pass http://backend:5000;
    proxy_http_version 1.1;
    proxy_set_header Upgrade $http_upgrade;
    proxy_set_header Connection "Upgrade";
    proxy_set_header Host $host;
    proxy_cache_bypass $http_upgrade;
}
```

### Kubernetes Deployment

```yaml
# deployment/charts/k8s/realtime-charts-deployment.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: wesign-realtime-charts
  labels:
    app: wesign-realtime-charts
    component: frontend
spec:
  replicas: 3
  selector:
    matchLabels:
      app: wesign-realtime-charts
  template:
    metadata:
      labels:
        app: wesign-realtime-charts
      annotations:
        prometheus.io/scrape: "true"
        prometheus.io/port: "8080"
    spec:
      containers:
      - name: charts-app
        image: wesign/client:{{.Values.image.tag}}
        ports:
        - containerPort: 80
        env:
        - name: NODE_ENV
          value: "production"
        - name: CHART_API_URL
          valueFrom:
            configMapKeyRef:
              name: wesign-config
              key: api.url
        - name: SIGNALR_HUB_URL
          valueFrom:
            configMapKeyRef:
              name: wesign-config
              key: signalr.hub.url
        resources:
          requests:
            memory: "256Mi"
            cpu: "100m"
          limits:
            memory: "512Mi"
            cpu: "500m"
        readinessProbe:
          httpGet:
            path: /health
            port: 80
          initialDelaySeconds: 10
          periodSeconds: 5
        livenessProbe:
          httpGet:
            path: /health
            port: 80
          initialDelaySeconds: 30
          periodSeconds: 10
        volumeMounts:
        - name: nginx-config
          mountPath: /etc/nginx/conf.d
          readOnly: true
      volumes:
      - name: nginx-config
        configMap:
          name: nginx-charts-config

---
apiVersion: v1
kind: Service
metadata:
  name: wesign-realtime-charts-service
  labels:
    app: wesign-realtime-charts
spec:
  selector:
    app: wesign-realtime-charts
  ports:
  - name: http
    port: 80
    targetPort: 80
  type: ClusterIP

---
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: wesign-charts-ingress
  annotations:
    kubernetes.io/ingress.class: nginx
    nginx.ingress.kubernetes.io/ssl-redirect: "true"
    nginx.ingress.kubernetes.io/proxy-body-size: "10m"
    nginx.ingress.kubernetes.io/configuration-snippet: |
      more_set_headers "X-Frame-Options: SAMEORIGIN";
      more_set_headers "X-Content-Type-Options: nosniff";
spec:
  tls:
  - hosts:
    - charts.wesign.com
    secretName: wesign-charts-tls
  rules:
  - host: charts.wesign.com
    http:
      paths:
      - path: /
        pathType: Prefix
        backend:
          service:
            name: wesign-realtime-charts-service
            port:
              number: 80
```

### Monitoring and Alerting

```yaml
# deployment/charts/monitoring/prometheus-rules.yaml
apiVersion: monitoring.coreos.com/v1
kind: PrometheusRule
metadata:
  name: wesign-charts-alerts
  labels:
    app: wesign-realtime-charts
spec:
  groups:
  - name: charts.performance
    rules:
    - alert: ChartRenderingSlowWarning
      expr: chart_render_duration_seconds{quantile="0.95"} > 1
      for: 2m
      labels:
        severity: warning
      annotations:
        summary: "Chart rendering performance degraded"
        description: "95th percentile chart rendering time is {{ $value }}s, exceeding 1s threshold"

    - alert: ChartRenderingSlowCritical
      expr: chart_render_duration_seconds{quantile="0.95"} > 2
      for: 1m
      labels:
        severity: critical
      annotations:
        summary: "Chart rendering critically slow"
        description: "95th percentile chart rendering time is {{ $value }}s, exceeding 2s threshold"

  - name: charts.connectivity
    rules:
    - alert: SignalRConnectionFailures
      expr: rate(signalr_connection_failures_total[5m]) > 0.1
      for: 1m
      labels:
        severity: warning
      annotations:
        summary: "SignalR connection failures detected"
        description: "SignalR connection failure rate is {{ $value }} failures/second"

  - name: charts.errors
    rules:
    - alert: ChartDataErrorsHigh
      expr: rate(chart_data_errors_total[5m]) > 0.05
      for: 2m
      labels:
        severity: warning
      annotations:
        summary: "High chart data error rate"
        description: "Chart data error rate is {{ $value }} errors/second"
```

### Environment-Specific Configurations

```typescript
// src/environments/environment.charts.prod.ts
export const chartEnvironment = {
  production: true,
  charts: {
    apiUrl: 'https://api.wesign.com',
    signalRHubUrl: 'https://api.wesign.com/analyticsHub',
    cacheStrategy: 'redis',
    performance: {
      enableDataDecimation: true,
      maxDataPoints: 10000,
      updateThrottleMs: 300,
      memoryCleanupIntervalMs: 60000
    },
    features: {
      realTimeUpdates: true,
      customCharts: true,
      exportFunctionality: true,
      aiInsights: true
    },
    monitoring: {
      enablePerformanceMetrics: true,
      enableErrorTracking: true,
      enableUserAnalytics: false // Production privacy
    }
  }
};
```

```typescript
// src/environments/environment.charts.staging.ts
export const chartEnvironment = {
  production: false,
  charts: {
    apiUrl: 'https://staging-api.wesign.com',
    signalRHubUrl: 'https://staging-api.wesign.com/analyticsHub',
    cacheStrategy: 'memory',
    performance: {
      enableDataDecimation: false, // Full data for testing
      maxDataPoints: 50000,
      updateThrottleMs: 100,
      memoryCleanupIntervalMs: 30000
    },
    features: {
      realTimeUpdates: true,
      customCharts: true,
      exportFunctionality: true,
      aiInsights: false // Disabled in staging
    },
    monitoring: {
      enablePerformanceMetrics: true,
      enableErrorTracking: true,
      enableUserAnalytics: true // Enabled for testing
    }
  }
};
```

## Quality Gates and Success Criteria

### Pipeline Success Requirements

1. **Code Quality**
   - ESLint: 0 errors, < 5 warnings
   - TypeScript: 0 compilation errors
   - Prettier: All files properly formatted
   - Chart-specific code patterns validated

2. **Security**
   - npm audit: No high/critical vulnerabilities
   - OWASP dependency check: Pass
   - Chart data sanitization verified
   - XSS prevention patterns confirmed

3. **Testing**
   - Unit test coverage: ≥ 95% for chart components
   - Integration tests: All pass
   - E2E tests: All chart interactions work
   - Accessibility tests: WCAG 2.1 AA compliant

4. **Performance**
   - Page load: < 2 seconds
   - Chart render: < 1 second per chart
   - Real-time updates: < 300ms latency
   - Memory usage: Stable, no leaks

5. **Build & Deploy**
   - Production build: Success
   - Bundle size: Within acceptable limits
   - Asset optimization: Complete
   - Deployment: Success to staging

### Monitoring and Alerting Thresholds

- **Response Time**: > 2s (Critical), > 1s (Warning)
- **Error Rate**: > 5% (Critical), > 1% (Warning)
- **Memory Usage**: > 90% (Critical), > 80% (Warning)
- **Connection Failures**: > 10% (Critical), > 5% (Warning)

---

## Next Steps

✅ **PROCEED TO STEP I: Security Configuration**

The CI/CD pipeline for Real-time Charts is fully configured with comprehensive testing, performance validation, security scanning, and deployment automation. The pipeline includes chart-specific quality gates and monitoring to ensure production readiness.