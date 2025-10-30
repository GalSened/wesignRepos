# Step J: Performance Testing - Export Functionality

## Overview
Comprehensive performance testing strategy for Export functionality covering load testing, stress testing, memory profiling, and performance optimization validation to ensure scalable and responsive export operations.

## 1. Performance Testing Strategy

### Performance Requirements
```typescript
// performance/performance-requirements.ts
export const ExportPerformanceRequirements = {
  // Response Time Requirements
  responseTimes: {
    exportDialogLoad: 500, // ms
    formatSelection: 200, // ms
    dataEstimation: 2000, // ms
    exportInitiation: 1000, // ms
    progressUpdates: 100, // ms
    downloadInitiation: 500, // ms
  },

  // Throughput Requirements
  throughput: {
    concurrentUsers: 50, // simultaneous export users
    exportsPerMinute: 100, // system-wide exports
    peakConcurrentExports: 20, // parallel processing jobs
    dataProcessingRate: 10000, // records per second
  },

  // Resource Limits
  resources: {
    maxMemoryUsage: 2048, // MB per export process
    maxCpuUsage: 80, // percentage
    maxDiskSpace: 10240, // MB for temporary files
    maxNetworkBandwidth: 100, // Mbps per export
  },

  // Scalability Targets
  scalability: {
    maxExportSize: 500 * 1024 * 1024, // 500MB
    maxRecordCount: 1000000, // 1M records
    maxConcurrentDownloads: 100,
    maxQueueLength: 200, // pending export jobs
  },

  // Availability Requirements
  availability: {
    uptime: 99.5, // percentage
    maxDowntime: 4, // hours per month
    recoveryTime: 300, // seconds
    failoverTime: 60, // seconds
  }
};
```

## 2. Load Testing Implementation

### Artillery Load Testing Configuration
```yaml
# performance-tests/export-load-test.yml
config:
  target: 'http://localhost:4200'
  phases:
    # Warm-up phase
    - duration: 60
      arrivalRate: 2
      name: "Warm up - Basic load"

    # Ramp-up phase
    - duration: 120
      arrivalRate: 5
      rampTo: 15
      name: "Ramp up - Gradual increase"

    # Sustained load phase
    - duration: 300
      arrivalRate: 15
      name: "Sustained load - Target performance"

    # Peak load phase
    - duration: 180
      arrivalRate: 25
      name: "Peak load - Stress testing"

    # Spike test
    - duration: 60
      arrivalRate: 50
      name: "Spike test - Maximum load"

    # Cool down
    - duration: 60
      arrivalRate: 5
      name: "Cool down - Recovery validation"

  payload:
    path: './test-data/export-scenarios.csv'
    fields:
      - userId
      - dataSource
      - format
      - recordCount
      - complexity

  processor: './processors/export-load-processor.js'

  environments:
    staging:
      target: 'https://staging.wesign.comda.co.il'
      variables:
        apiUrl: 'https://api-staging.wesign.comda.co.il'

    production:
      target: 'https://app.wesign.comda.co.il'
      variables:
        apiUrl: 'https://api.wesign.comda.co.il'

scenarios:
  - name: "Quick Export Workflow"
    weight: 40
    flow:
      # Navigate to analytics dashboard
      - get:
          url: "/analytics-dashboard"
          headers:
            Authorization: "Bearer {{ authToken }}"
          expect:
            - statusCode: 200
          capture:
            - header: "x-request-id"
              as: "requestId"

      # Open export dialog
      - think: 1
      - get:
          url: "/api/exports/formats"
          expect:
            - statusCode: 200
          capture:
            - json: "$[0]"
              as: "defaultFormat"

      # Estimate data size
      - think: 2
      - post:
          url: "/api/exports/estimate"
          json:
            dataSource: "{{ dataSource }}"
            filters: {}
            dateRange:
              startDate: "2024-01-01"
              endDate: "2024-01-31"
          expect:
            - statusCode: 200
          capture:
            - json: "$.sizeBytes"
              as: "estimatedSize"
            - json: "$.recordCount"
              as: "estimatedRecords"

      # Initiate small export
      - think: 3
      - post:
          url: "/api/exports/initiate"
          json:
            format: "{{ format }}"
            dataSource: "{{ dataSource }}"
            dateRange:
              startDate: "2024-01-01"
              endDate: "2024-01-31"
            filters: {}
            deliveryOptions:
              method: "download"
          expect:
            - statusCode: 202
          capture:
            - json: "$.id"
              as: "jobId"

      # Monitor progress
      - loop:
          - get:
              url: "/api/exports/{{ jobId }}/progress"
              expect:
                - statusCode: 200
              capture:
                - json: "$.percentage"
                  as: "progress"
                - json: "$.status"
                  as: "status"
          - think: 2
        over:
          - "progress"
        while: "{{ status }} === 'processing'"

      # Download file (if completed)
      - ifTrue: "{{ status }} === 'completed'"
        then:
          - get:
              url: "/api/exports/{{ jobId }}/download"
              expect:
                - statusCode: 200

  - name: "Large Export Workflow"
    weight: 25
    flow:
      # Similar to quick export but with larger dataset
      - get:
          url: "/analytics-dashboard"
          headers:
            Authorization: "Bearer {{ authToken }}"

      - think: 2
      - post:
          url: "/api/exports/estimate"
          json:
            dataSource: "{{ dataSource }}"
            filters: {}
            dateRange:
              startDate: "2023-01-01"
              endDate: "2024-01-31"

      - think: 5
      - post:
          url: "/api/exports/initiate"
          json:
            format: "Excel"
            dataSource: "{{ dataSource }}"
            dateRange:
              startDate: "2023-01-01"
              endDate: "2024-01-31"
            filters: {}
            deliveryOptions:
              method: "email"
              emailRecipients: ["test@example.com"]

  - name: "Template Usage Workflow"
    weight: 20
    flow:
      # Load and apply templates
      - get:
          url: "/api/exports/templates"
          expect:
            - statusCode: 200
          capture:
            - json: "$[0].id"
              as: "templateId"

      - think: 1
      - get:
          url: "/api/exports/templates/{{ templateId }}"
          expect:
            - statusCode: 200
          capture:
            - json: "$.config"
              as: "templateConfig"

      - think: 2
      - post:
          url: "/api/exports/initiate"
          json: "{{ templateConfig }}"

  - name: "Concurrent Export Management"
    weight: 15
    flow:
      # Initiate multiple exports simultaneously
      - parallel:
          - post:
              url: "/api/exports/initiate"
              json:
                format: "CSV"
                dataSource: "analytics"
                dateRange:
                  startDate: "2024-01-01"
                  endDate: "2024-01-15"
          - post:
              url: "/api/exports/initiate"
              json:
                format: "PDF"
                dataSource: "reports"
                dateRange:
                  startDate: "2024-01-01"
                  endDate: "2024-01-15"
          - post:
              url: "/api/exports/initiate"
              json:
                format: "Excel"
                dataSource: "metrics"
                dateRange:
                  startDate: "2024-01-01"
                  endDate: "2024-01-15"

      # Monitor all exports
      - think: 3
      - get:
          url: "/api/exports/history"
          expect:
            - statusCode: 200
```

### Load Test Processor
```javascript
// performance-tests/processors/export-load-processor.js
const { faker } = require('@faker-js/faker');

// Generate test authentication tokens
function generateAuthToken() {
  return faker.string.alphanumeric(32);
}

// Simulate different user types and behaviors
function setupUserContext(userVars, context, events, done) {
  // Generate user profile
  userVars.userId = faker.string.uuid();
  userVars.userRole = faker.helpers.arrayElement(['ProductManager', 'Support', 'Operations', 'StandardUser']);
  userVars.authToken = generateAuthToken();

  // Set data source based on user role
  const dataSources = {
    'ProductManager': ['analytics', 'reports', 'metrics', 'users'],
    'Support': ['customer-data', 'support-tickets'],
    'Operations': ['system-metrics', 'operational-data'],
    'StandardUser': ['personal-data', 'user-analytics']
  };

  userVars.dataSource = faker.helpers.arrayElement(dataSources[userVars.userRole]);
  userVars.format = faker.helpers.arrayElement(['PDF', 'Excel', 'CSV', 'JSON']);

  // Set complexity based on format
  const complexityMap = {
    'PDF': faker.helpers.arrayElement(['simple', 'medium', 'complex']),
    'Excel': faker.helpers.arrayElement(['medium', 'complex']),
    'CSV': faker.helpers.arrayElement(['simple', 'medium']),
    'JSON': faker.helpers.arrayElement(['simple', 'medium'])
  };

  userVars.complexity = complexityMap[userVars.format];

  // Simulate realistic record counts
  const recordCounts = {
    'simple': faker.number.int({ min: 100, max: 1000 }),
    'medium': faker.number.int({ min: 1000, max: 10000 }),
    'complex': faker.number.int({ min: 10000, max: 100000 })
  };

  userVars.recordCount = recordCounts[userVars.complexity];

  return done();
}

// Custom metrics collection
function collectMetrics(requestParams, response, context, ee, next) {
  // Track export-specific metrics
  if (requestParams.url.includes('/api/exports/initiate')) {
    ee.emit('counter', 'exports.initiated', 1);

    if (response.statusCode === 202) {
      ee.emit('counter', 'exports.successful', 1);
    } else {
      ee.emit('counter', 'exports.failed', 1);
    }
  }

  if (requestParams.url.includes('/api/exports') && requestParams.url.includes('/progress')) {
    ee.emit('counter', 'progress.checks', 1);
  }

  if (requestParams.url.includes('/api/exports') && requestParams.url.includes('/download')) {
    ee.emit('counter', 'downloads.attempted', 1);

    if (response.statusCode === 200) {
      ee.emit('counter', 'downloads.successful', 1);

      // Track download size if available
      const contentLength = response.headers['content-length'];
      if (contentLength) {
        ee.emit('histogram', 'download.size.bytes', parseInt(contentLength));
      }
    }
  }

  return next();
}

// Error handling and recovery
function handleExportError(requestParams, response, context, ee, next) {
  if (response.statusCode >= 400) {
    ee.emit('counter', `errors.${response.statusCode}`, 1);

    // Log specific export errors
    if (requestParams.url.includes('/api/exports')) {
      console.log(`Export API Error: ${response.statusCode} - ${requestParams.url}`);

      // Implement retry logic for specific errors
      if (response.statusCode === 429) { // Rate limited
        ee.emit('counter', 'errors.rate_limit', 1);
        context.vars.retryAfter = response.headers['retry-after'] || 5;
      }

      if (response.statusCode === 503) { // Service unavailable
        ee.emit('counter', 'errors.service_unavailable', 1);
      }
    }
  }

  return next();
}

// Performance threshold validation
function validatePerformanceThresholds(requestParams, response, context, ee, next) {
  const responseTime = response.timings.response;

  // Define thresholds for different endpoints
  const thresholds = {
    '/api/exports/estimate': 2000,
    '/api/exports/initiate': 1000,
    '/api/exports/progress': 500,
    '/api/exports/download': 5000,
    '/api/exports/formats': 200
  };

  // Check if response time exceeds threshold
  for (const [endpoint, threshold] of Object.entries(thresholds)) {
    if (requestParams.url.includes(endpoint) && responseTime > threshold) {
      ee.emit('counter', `performance.threshold_exceeded.${endpoint.replace(/\//g, '_')}`, 1);
      ee.emit('histogram', `performance.response_time.${endpoint.replace(/\//g, '_')}`, responseTime);
    }
  }

  return next();
}

module.exports = {
  setupUserContext,
  collectMetrics,
  handleExportError,
  validatePerformanceThresholds
};
```

## 3. Lighthouse Performance Testing

### Lighthouse Configuration
```json
// performance-tests/lighthouse-config.json
{
  "extends": "lighthouse:default",
  "settings": {
    "onlyAudits": [
      "first-contentful-paint",
      "largest-contentful-paint",
      "first-meaningful-paint",
      "speed-index",
      "interactive",
      "total-blocking-time",
      "cumulative-layout-shift",
      "max-potential-fid",
      "render-blocking-resources",
      "uses-optimized-images",
      "uses-webp-images",
      "uses-text-compression",
      "unused-css-rules",
      "unused-javascript",
      "modern-image-formats",
      "uses-responsive-images",
      "efficient-animated-content",
      "preload-lcp-image",
      "unminified-css",
      "unminified-javascript"
    ],
    "throttling": {
      "rttMs": 150,
      "throughputKbps": 1638.4,
      "requestLatencyMs": 562.5,
      "downloadThroughputKbps": 1474.56,
      "uploadThroughputKbps": 675
    },
    "throttlingMethod": "simulate"
  }
}
```

### Lighthouse CI Configuration
```json
// performance-tests/lighthouserc.json
{
  "ci": {
    "collect": {
      "url": [
        "http://localhost:4200/analytics-dashboard",
        "http://localhost:4200/analytics-dashboard?autoOpenExport=true"
      ],
      "numberOfRuns": 5,
      "settings": {
        "configPath": "./performance-tests/lighthouse-config.json",
        "chromeFlags": "--disable-gpu --no-sandbox --disable-dev-shm-usage"
      }
    },
    "assert": {
      "assertions": {
        "categories:performance": ["error", {"minScore": 0.9}],
        "categories:accessibility": ["error", {"minScore": 0.95}],
        "first-contentful-paint": ["error", {"maxNumericValue": 2000}],
        "largest-contentful-paint": ["error", {"maxNumericValue": 3000}],
        "total-blocking-time": ["error", {"maxNumericValue": 300}],
        "cumulative-layout-shift": ["error", {"maxNumericValue": 0.1}],
        "speed-index": ["error", {"maxNumericValue": 4000}],
        "interactive": ["error", {"maxNumericValue": 4000}]
      }
    },
    "upload": {
      "target": "filesystem",
      "outputDir": "./lighthouse-results"
    }
  }
}
```

## 4. Memory and CPU Profiling

### Browser Performance Testing
```typescript
// performance-tests/browser-performance.spec.ts
import { test, expect } from '@playwright/test';

test.describe('Export Functionality Performance Tests', () => {
  test.beforeEach(async ({ page }) => {
    // Enable performance monitoring
    await page.addInitScript(() => {
      window.performance.mark('test-start');
    });

    await page.goto('/analytics-dashboard');
    await page.waitForLoadState('networkidle');
  });

  test('Export dialog performance benchmarks', async ({ page }) => {
    // Measure export dialog opening time
    await page.evaluate(() => window.performance.mark('dialog-open-start'));

    await page.click('[data-testid="export-button"]');
    await page.waitForSelector('[data-testid="export-dialog"]', { state: 'visible' });

    await page.evaluate(() => window.performance.mark('dialog-open-end'));

    const dialogOpenTime = await page.evaluate(() => {
      window.performance.measure('dialog-open', 'dialog-open-start', 'dialog-open-end');
      return window.performance.getEntriesByName('dialog-open')[0].duration;
    });

    expect(dialogOpenTime).toBeLessThan(500); // 500ms threshold
  });

  test('Format selection performance', async ({ page }) => {
    await page.click('[data-testid="export-button"]');
    await page.waitForSelector('[data-testid="export-dialog"]');

    // Measure format selection response time
    await page.evaluate(() => window.performance.mark('format-select-start'));

    await page.click('[data-testid="format-pdf"]');
    await page.waitForSelector('[data-testid="format-options"]', { state: 'visible' });

    await page.evaluate(() => window.performance.mark('format-select-end'));

    const formatSelectTime = await page.evaluate(() => {
      window.performance.measure('format-select', 'format-select-start', 'format-select-end');
      return window.performance.getEntriesByName('format-select')[0].duration;
    });

    expect(formatSelectTime).toBeLessThan(200); // 200ms threshold
  });

  test('Data estimation performance', async ({ page }) => {
    await page.click('[data-testid="export-button"]');
    await page.click('[data-testid="format-csv"]');

    // Set date range and measure estimation time
    await page.evaluate(() => window.performance.mark('estimation-start'));

    await page.click('[data-testid="date-range-preset-30days"]');
    await page.waitForSelector('[data-testid="estimated-records"]', { state: 'visible' });

    await page.evaluate(() => window.performance.mark('estimation-end'));

    const estimationTime = await page.evaluate(() => {
      window.performance.measure('estimation', 'estimation-start', 'estimation-end');
      return window.performance.getEntriesByName('estimation')[0].duration;
    });

    expect(estimationTime).toBeLessThan(2000); // 2s threshold
  });

  test('Memory usage during large export configuration', async ({ page }) => {
    // Monitor memory usage
    const initialMemory = await page.evaluate(() => {
      if (window.performance.memory) {
        return {
          used: window.performance.memory.usedJSHeapSize,
          total: window.performance.memory.totalJSHeapSize,
          limit: window.performance.memory.jsHeapSizeLimit
        };
      }
      return null;
    });

    if (!initialMemory) {
      console.log('Memory API not available, skipping memory test');
      return;
    }

    // Configure complex export
    await page.click('[data-testid="export-button"]');
    await page.click('[data-testid="format-excel"]');

    // Add multiple filters
    for (let i = 0; i < 10; i++) {
      await page.click('[data-testid="add-filter"]');
      await page.selectOption(`[data-testid="filter-column-${i}"]`, 'status');
      await page.fill(`[data-testid="filter-value-${i}"]`, `value-${i}`);
    }

    // Set large date range
    await page.click('[data-testid="date-range-custom"]');
    await page.fill('[data-testid="start-date"]', '2020-01-01');
    await page.fill('[data-testid="end-date"]', '2024-12-31');

    const finalMemory = await page.evaluate(() => {
      if (window.performance.memory) {
        return {
          used: window.performance.memory.usedJSHeapSize,
          total: window.performance.memory.totalJSHeapSize,
          limit: window.performance.memory.jsHeapSizeLimit
        };
      }
      return null;
    });

    if (finalMemory && initialMemory) {
      const memoryIncrease = finalMemory.used - initialMemory.used;
      const memoryIncreasePercent = (memoryIncrease / initialMemory.used) * 100;

      console.log(`Memory increase: ${memoryIncrease} bytes (${memoryIncreasePercent.toFixed(2)}%)`);

      // Memory increase should be reasonable (less than 50% for configuration)
      expect(memoryIncreasePercent).toBeLessThan(50);
    }
  });

  test('Progress update performance', async ({ page }) => {
    // Mock fast progress updates
    await page.route('**/api/exports/*/progress', async (route) => {
      const url = route.request().url();
      const jobId = url.split('/')[5];

      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          jobId,
          percentage: Math.floor(Math.random() * 100),
          currentStage: 'processing_data',
          estimatedTimeRemaining: 30000,
          processedRecords: 1000,
          totalRecords: 5000
        })
      });
    });

    await page.click('[data-testid="export-button"]');
    await page.click('[data-testid="format-csv"]');
    await page.click('[data-testid="date-range-preset-7days"]');
    await page.click('[data-testid="export-submit"]');

    // Measure progress update rendering performance
    const progressStartTime = Date.now();

    // Wait for multiple progress updates
    for (let i = 0; i < 10; i++) {
      await page.waitForSelector('[data-testid="progress-percentage"]');
      await page.waitForTimeout(100); // Simulate rapid updates
    }

    const progressEndTime = Date.now();
    const totalProgressTime = progressEndTime - progressStartTime;

    // Progress updates should be handled efficiently
    expect(totalProgressTime).toBeLessThan(2000); // 2s for 10 updates
  });

  test('Bundle size and loading performance', async ({ page }) => {
    const navigationPromise = page.waitForLoadState('networkidle');

    await page.goto('/analytics-dashboard');
    await navigationPromise;

    // Check resource loading performance
    const resourceTimings = await page.evaluate(() => {
      return performance.getEntriesByType('resource').map(entry => ({
        name: entry.name,
        duration: entry.duration,
        size: entry.transferSize,
        type: entry.initiatorType
      }));
    });

    // Analyze JavaScript bundle sizes
    const jsResources = resourceTimings.filter(r => r.name.includes('.js'));
    const cssResources = resourceTimings.filter(r => r.name.includes('.css'));

    const totalJsSize = jsResources.reduce((sum, r) => sum + (r.size || 0), 0);
    const totalCssSize = cssResources.reduce((sum, r) => sum + (r.size || 0), 0);

    console.log(`Total JS size: ${totalJsSize} bytes`);
    console.log(`Total CSS size: ${totalCssSize} bytes`);

    // Bundle size thresholds
    expect(totalJsSize).toBeLessThan(2 * 1024 * 1024); // 2MB JS threshold
    expect(totalCssSize).toBeLessThan(500 * 1024); // 500KB CSS threshold

    // Loading time thresholds
    const longLoadingResources = jsResources.filter(r => r.duration > 2000);
    expect(longLoadingResources.length).toBe(0); // No JS should take >2s to load
  });
});
```

## 5. Stress Testing Configuration

### Stress Test Scenarios
```yaml
# performance-tests/stress-test.yml
config:
  target: 'http://localhost:4200'
  phases:
    # Gradual stress increase
    - duration: 60
      arrivalRate: 1
      rampTo: 10
      name: "Stress ramp-up"

    # High load sustained
    - duration: 300
      arrivalRate: 50
      name: "High sustained load"

    # Extreme load spike
    - duration: 120
      arrivalRate: 100
      name: "Extreme load spike"

    # Recovery test
    - duration: 180
      arrivalRate: 5
      name: "Recovery validation"

  processor: './processors/stress-test-processor.js'

scenarios:
  - name: "Concurrent Large Exports"
    weight: 60
    flow:
      - post:
          url: "/api/exports/initiate"
          json:
            format: "Excel"
            dataSource: "analytics"
            dateRange:
              startDate: "2023-01-01"
              endDate: "2024-12-31"
            filters: {}
            deliveryOptions:
              method: "download"
          expect:
            - statusCode: [202, 429, 503] # Accept queue full or service busy

  - name: "Rapid Export Attempts"
    weight: 40
    flow:
      - loop:
          - post:
              url: "/api/exports/initiate"
              json:
                format: "CSV"
                dataSource: "metrics"
                dateRange:
                  startDate: "2024-01-01"
                  endDate: "2024-01-07"
          - think: 0.1
        count: 10
```

## 6. Performance Monitoring Setup

### Real-time Performance Monitoring
```typescript
// performance/performance-monitor.service.ts
import { Injectable } from '@angular/core';
import { BehaviorSubject, interval } from 'rxjs';
import { map, switchMap } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class PerformanceMonitorService {
  private performanceMetrics$ = new BehaviorSubject<PerformanceMetrics>({
    memory: this.getMemoryInfo(),
    timing: this.getTimingInfo(),
    network: this.getNetworkInfo(),
    exports: this.getExportMetrics()
  });

  constructor() {
    // Update metrics every 5 seconds
    interval(5000).pipe(
      map(() => ({
        memory: this.getMemoryInfo(),
        timing: this.getTimingInfo(),
        network: this.getNetworkInfo(),
        exports: this.getExportMetrics()
      }))
    ).subscribe(metrics => this.performanceMetrics$.next(metrics));
  }

  getPerformanceMetrics() {
    return this.performanceMetrics$.asObservable();
  }

  private getMemoryInfo(): MemoryInfo {
    if ('memory' in performance) {
      const memory = (performance as any).memory;
      return {
        used: memory.usedJSHeapSize,
        total: memory.totalJSHeapSize,
        limit: memory.jsHeapSizeLimit,
        percentage: (memory.usedJSHeapSize / memory.jsHeapSizeLimit) * 100
      };
    }
    return { used: 0, total: 0, limit: 0, percentage: 0 };
  }

  private getTimingInfo(): TimingInfo {
    const navigation = performance.getEntriesByType('navigation')[0] as PerformanceNavigationTiming;

    return {
      domContentLoaded: navigation.domContentLoadedEventEnd - navigation.navigationStart,
      loadComplete: navigation.loadEventEnd - navigation.navigationStart,
      firstPaint: this.getFirstPaint(),
      firstContentfulPaint: this.getFirstContentfulPaint(),
      largestContentfulPaint: this.getLargestContentfulPaint()
    };
  }

  private getNetworkInfo(): NetworkInfo {
    const connection = (navigator as any).connection;

    if (connection) {
      return {
        effectiveType: connection.effectiveType,
        downlink: connection.downlink,
        rtt: connection.rtt,
        saveData: connection.saveData
      };
    }

    return {
      effectiveType: 'unknown',
      downlink: 0,
      rtt: 0,
      saveData: false
    };
  }

  private getExportMetrics(): ExportMetrics {
    // Get export-specific performance metrics
    const exportTimings = performance.getEntriesByName('export-operation');
    const dialogTimings = performance.getEntriesByName('export-dialog');

    return {
      averageExportTime: this.calculateAverage(exportTimings.map(t => t.duration)),
      averageDialogTime: this.calculateAverage(dialogTimings.map(t => t.duration)),
      totalExports: exportTimings.length,
      failedExports: this.getFailedExportCount(),
      activeExports: this.getActiveExportCount()
    };
  }

  private getFirstPaint(): number {
    const fpEntry = performance.getEntriesByName('first-paint')[0];
    return fpEntry ? fpEntry.startTime : 0;
  }

  private getFirstContentfulPaint(): number {
    const fcpEntry = performance.getEntriesByName('first-contentful-paint')[0];
    return fcpEntry ? fcpEntry.startTime : 0;
  }

  private getLargestContentfulPaint(): number {
    return new Promise<number>(resolve => {
      new PerformanceObserver((list) => {
        const entries = list.getEntries();
        const lastEntry = entries[entries.length - 1];
        resolve(lastEntry.startTime);
      }).observe({ type: 'largest-contentful-paint', buffered: true });
    });
  }

  private calculateAverage(values: number[]): number {
    if (values.length === 0) return 0;
    return values.reduce((sum, val) => sum + val, 0) / values.length;
  }

  private getFailedExportCount(): number {
    // Implementation to track failed exports
    return 0; // Placeholder
  }

  private getActiveExportCount(): number {
    // Implementation to track active exports
    return 0; // Placeholder
  }

  // Performance alert system
  checkPerformanceThresholds(metrics: PerformanceMetrics): PerformanceAlert[] {
    const alerts: PerformanceAlert[] = [];

    // Memory usage alert
    if (metrics.memory.percentage > 80) {
      alerts.push({
        type: 'memory',
        severity: 'high',
        message: `Memory usage is ${metrics.memory.percentage.toFixed(1)}%`,
        threshold: 80,
        current: metrics.memory.percentage
      });
    }

    // Export performance alert
    if (metrics.exports.averageExportTime > 30000) {
      alerts.push({
        type: 'export-performance',
        severity: 'medium',
        message: `Average export time is ${metrics.exports.averageExportTime}ms`,
        threshold: 30000,
        current: metrics.exports.averageExportTime
      });
    }

    // Network performance alert
    if (metrics.network.rtt > 1000) {
      alerts.push({
        type: 'network',
        severity: 'medium',
        message: `Network RTT is ${metrics.network.rtt}ms`,
        threshold: 1000,
        current: metrics.network.rtt
      });
    }

    return alerts;
  }
}

interface PerformanceMetrics {
  memory: MemoryInfo;
  timing: TimingInfo;
  network: NetworkInfo;
  exports: ExportMetrics;
}

interface MemoryInfo {
  used: number;
  total: number;
  limit: number;
  percentage: number;
}

interface TimingInfo {
  domContentLoaded: number;
  loadComplete: number;
  firstPaint: number;
  firstContentfulPaint: number;
  largestContentfulPaint: number;
}

interface NetworkInfo {
  effectiveType: string;
  downlink: number;
  rtt: number;
  saveData: boolean;
}

interface ExportMetrics {
  averageExportTime: number;
  averageDialogTime: number;
  totalExports: number;
  failedExports: number;
  activeExports: number;
}

interface PerformanceAlert {
  type: string;
  severity: 'low' | 'medium' | 'high';
  message: string;
  threshold: number;
  current: number;
}
```

## 7. Performance Test Reporting

### Test Results Analysis
```typescript
// performance/test-reporter.ts
import * as fs from 'fs';
import * as path from 'path';

export class PerformanceTestReporter {
  generateReport(results: any): PerformanceTestReport {
    return {
      summary: this.generateSummary(results),
      loadTest: this.analyzeLoadTestResults(results.load),
      stressTest: this.analyzeStressTestResults(results.stress),
      lighthouse: this.analyzeLighthouseResults(results.lighthouse),
      recommendations: this.generateRecommendations(results),
      timestamp: new Date().toISOString()
    };
  }

  private generateSummary(results: any): TestSummary {
    return {
      overallScore: this.calculateOverallScore(results),
      passedTests: this.countPassedTests(results),
      failedTests: this.countFailedTests(results),
      criticalIssues: this.identifyCriticalIssues(results),
      performanceGrade: this.assignPerformanceGrade(results)
    };
  }

  private analyzeLoadTestResults(loadResults: any): LoadTestAnalysis {
    return {
      averageResponseTime: loadResults.aggregate.latency.mean,
      p95ResponseTime: loadResults.aggregate.latency.p95,
      p99ResponseTime: loadResults.aggregate.latency.p99,
      throughput: loadResults.aggregate.rps.count,
      errorRate: (loadResults.aggregate.counters.errors / loadResults.aggregate.counters.total) * 100,
      concurrentUsers: loadResults.config.phases.reduce((max: number, phase: any) =>
        Math.max(max, phase.arrivalRate), 0),
      totalRequests: loadResults.aggregate.counters.total,
      successfulRequests: loadResults.aggregate.counters.total - loadResults.aggregate.counters.errors
    };
  }

  private analyzeLighthouseResults(lighthouseResults: any): LighthouseAnalysis {
    const scores = lighthouseResults.lhr.categories;

    return {
      performanceScore: scores.performance.score * 100,
      accessibilityScore: scores.accessibility.score * 100,
      bestPracticesScore: scores['best-practices'].score * 100,
      seoScore: scores.seo.score * 100,
      metrics: {
        firstContentfulPaint: lighthouseResults.lhr.audits['first-contentful-paint'].numericValue,
        largestContentfulPaint: lighthouseResults.lhr.audits['largest-contentful-paint'].numericValue,
        totalBlockingTime: lighthouseResults.lhr.audits['total-blocking-time'].numericValue,
        cumulativeLayoutShift: lighthouseResults.lhr.audits['cumulative-layout-shift'].numericValue,
        speedIndex: lighthouseResults.lhr.audits['speed-index'].numericValue
      }
    };
  }

  private generateRecommendations(results: any): string[] {
    const recommendations: string[] = [];

    // Performance recommendations
    if (results.lighthouse.lhr.categories.performance.score < 0.9) {
      recommendations.push('Optimize JavaScript bundle size and loading');
      recommendations.push('Implement code splitting for export functionality');
      recommendations.push('Use lazy loading for large components');
    }

    // Load test recommendations
    if (results.load.aggregate.latency.p95 > 2000) {
      recommendations.push('Optimize API response times');
      recommendations.push('Implement caching for frequently accessed data');
      recommendations.push('Consider database query optimization');
    }

    // Memory recommendations
    if (results.browser?.memory?.percentage > 70) {
      recommendations.push('Optimize memory usage in export components');
      recommendations.push('Implement proper cleanup in component lifecycle');
      recommendations.push('Review for memory leaks in progress tracking');
    }

    return recommendations;
  }

  saveReport(report: PerformanceTestReport, outputPath: string): void {
    const reportJson = JSON.stringify(report, null, 2);
    fs.writeFileSync(path.join(outputPath, 'performance-report.json'), reportJson);

    // Generate HTML report
    const htmlReport = this.generateHtmlReport(report);
    fs.writeFileSync(path.join(outputPath, 'performance-report.html'), htmlReport);
  }

  private generateHtmlReport(report: PerformanceTestReport): string {
    return `
<!DOCTYPE html>
<html>
<head>
    <title>Export Functionality Performance Test Report</title>
    <style>
        body { font-family: Arial, sans-serif; margin: 20px; }
        .summary { background: #f0f8ff; padding: 20px; border-radius: 5px; }
        .metrics { display: grid; grid-template-columns: repeat(auto-fit, minmax(250px, 1fr)); gap: 20px; }
        .metric-card { background: white; border: 1px solid #ddd; padding: 15px; border-radius: 5px; }
        .score { font-size: 2em; font-weight: bold; }
        .pass { color: green; }
        .fail { color: red; }
        .warning { color: orange; }
        .recommendations { background: #fff3cd; padding: 15px; border-radius: 5px; }
    </style>
</head>
<body>
    <h1>Export Functionality Performance Test Report</h1>
    <p>Generated: ${report.timestamp}</p>

    <div class="summary">
        <h2>Summary</h2>
        <div class="score ${report.summary.overallScore >= 80 ? 'pass' : 'fail'}">
            Overall Score: ${report.summary.overallScore}/100
        </div>
        <p>Performance Grade: ${report.summary.performanceGrade}</p>
        <p>Tests Passed: ${report.summary.passedTests}</p>
        <p>Tests Failed: ${report.summary.failedTests}</p>
    </div>

    <h2>Test Results</h2>
    <div class="metrics">
        <div class="metric-card">
            <h3>Load Test Results</h3>
            <p>Average Response Time: ${report.loadTest.averageResponseTime}ms</p>
            <p>95th Percentile: ${report.loadTest.p95ResponseTime}ms</p>
            <p>Throughput: ${report.loadTest.throughput} req/s</p>
            <p>Error Rate: ${report.loadTest.errorRate.toFixed(2)}%</p>
        </div>

        <div class="metric-card">
            <h3>Lighthouse Scores</h3>
            <p>Performance: ${report.lighthouse.performanceScore}/100</p>
            <p>Accessibility: ${report.lighthouse.accessibilityScore}/100</p>
            <p>Best Practices: ${report.lighthouse.bestPracticesScore}/100</p>
            <p>SEO: ${report.lighthouse.seoScore}/100</p>
        </div>
    </div>

    <div class="recommendations">
        <h2>Recommendations</h2>
        <ul>
            ${report.recommendations.map(rec => `<li>${rec}</li>`).join('')}
        </ul>
    </div>
</body>
</html>
    `;
  }
}

interface PerformanceTestReport {
  summary: TestSummary;
  loadTest: LoadTestAnalysis;
  stressTest?: StressTestAnalysis;
  lighthouse: LighthouseAnalysis;
  recommendations: string[];
  timestamp: string;
}

interface TestSummary {
  overallScore: number;
  passedTests: number;
  failedTests: number;
  criticalIssues: string[];
  performanceGrade: string;
}

interface LoadTestAnalysis {
  averageResponseTime: number;
  p95ResponseTime: number;
  p99ResponseTime: number;
  throughput: number;
  errorRate: number;
  concurrentUsers: number;
  totalRequests: number;
  successfulRequests: number;
}

interface StressTestAnalysis {
  maxConcurrentUsers: number;
  breakingPoint: number;
  recoveryTime: number;
  resourceUtilization: {
    cpu: number;
    memory: number;
    network: number;
  };
}

interface LighthouseAnalysis {
  performanceScore: number;
  accessibilityScore: number;
  bestPracticesScore: number;
  seoScore: number;
  metrics: {
    firstContentfulPaint: number;
    largestContentfulPaint: number;
    totalBlockingTime: number;
    cumulativeLayoutShift: number;
    speedIndex: number;
  };
}
```

This comprehensive performance testing configuration ensures the Export functionality meets all performance requirements and can handle the expected load while maintaining optimal user experience.