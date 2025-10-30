# Real-time Charts Documentation - A→M Workflow Step K

**PAGE_KEY**: realtime-charts
**COMPLETION DATE**: 2025-01-29
**STATUS**: ✅ COMPLETE

## Comprehensive Documentation Suite

### Table of Contents

1. [User Documentation](#user-documentation)
2. [Developer Documentation](#developer-documentation)
3. [API Documentation](#api-documentation)
4. [Configuration Documentation](#configuration-documentation)
5. [Troubleshooting Guide](#troubleshooting-guide)
6. [Security Documentation](#security-documentation)
7. [Performance Documentation](#performance-documentation)
8. [Accessibility Documentation](#accessibility-documentation)

---

## User Documentation

### Real-time Charts User Guide

```markdown
# WeSign Real-time Charts - User Guide

## Overview
The Real-time Charts page provides interactive data visualization for WeSign analytics with live updates, drill-down capabilities, and comprehensive export functionality.

## Getting Started

### Accessing Charts
1. Navigate to **Analytics** → **Real-time Charts** from the main menu
2. Charts load automatically based on your role permissions
3. Real-time updates begin immediately upon page load

### Chart Types by Role

#### ProductManager Role
- **Usage Analytics**: Document lifecycle and user engagement metrics
- **Business Intelligence**: Revenue impact and conversion funnels
- **Performance Monitoring**: System performance and health metrics

#### Support Role
- **Error Rate Trends**: System error tracking and patterns
- **User Activity**: Customer engagement for troubleshooting
- **System Health**: Basic performance indicators

#### Operations Role
- **Performance Monitoring**: Detailed system metrics
- **Infrastructure Health**: Server and database performance
- **API Response Times**: Real-time service monitoring

## Using Charts

### Basic Navigation
- **Hover**: View detailed data points and values
- **Click**: Drill down into specific data segments
- **Zoom**: Use mouse wheel to zoom in/out on time ranges
- **Pan**: Click and drag to navigate within zoomed charts

### Filtering Data
1. Use the **Time Range** dropdown to select:
   - Last 1 hour
   - Last 24 hours
   - Last 7 days
   - Last 30 days
   - Custom date range

2. Apply **Chart Filters** for specific data:
   - User segments
   - Document types
   - Performance metrics
   - Geographic regions

### Cross-Chart Filtering
- Selecting data in one chart automatically filters related charts
- Active filters are displayed in the filter bar
- Click "Clear All Filters" to reset to default view

### Real-time Features
- Charts update automatically every 5 seconds
- Connection status indicator shows real-time connectivity
- Pause button to stop real-time updates temporarily
- Manual refresh button for immediate data reload

## Exporting Charts

### Single Chart Export
1. Click the **Export** icon on any chart
2. Choose format:
   - **PNG**: High-quality image (recommended for presentations)
   - **SVG**: Vector graphics (recommended for print)
   - **PDF**: Document format with metadata
   - **CSV**: Raw data export
   - **Excel**: Spreadsheet format with formatting

3. Configure export options:
   - Resolution (for images)
   - Include/exclude legend
   - Add watermark (automatic for compliance)

### Dashboard Export
1. Click **Export Dashboard** in the top toolbar
2. Select **PDF** for complete dashboard export
3. Choose options:
   - Current filters applied
   - Include summary page
   - Add custom notes

### Export Security
- All exports include automatic watermarks
- Export activity is logged for audit purposes
- Exported data respects your role-based access permissions

## Accessibility Features

### Keyboard Navigation
- **Tab**: Navigate between chart controls
- **Enter**: Activate buttons and drill-down
- **Arrow Keys**: Navigate chart data points
- **Escape**: Close drill-down panels

### Screen Reader Support
- Charts include descriptive text alternatives
- Data tables available for all visual charts
- Real-time updates announced via live regions
- Trend descriptions (e.g., "increasing", "stable")

### High Contrast Mode
- Automatic detection of system high contrast settings
- Pattern-based chart differentiation (not just color)
- Enhanced border visibility
- Adjustable font sizes

## Troubleshooting

### Connection Issues
**Problem**: "Real-time connection lost" warning
**Solution**:
1. Check internet connection
2. Refresh the page
3. Contact IT if problem persists

**Problem**: Charts not updating
**Solution**:
1. Verify connection status indicator
2. Click refresh button
3. Check if updates are paused

### Performance Issues
**Problem**: Slow chart loading
**Solution**:
1. Reduce time range for large datasets
2. Clear browser cache
3. Close unnecessary browser tabs

### Data Issues
**Problem**: Missing data in charts
**Solution**:
1. Verify your role has access to requested data
2. Check selected time range
3. Clear active filters
4. Contact support if data should be available

### Browser Compatibility
- **Supported**: Chrome 90+, Firefox 88+, Safari 14+, Edge 90+
- **Required**: JavaScript enabled
- **Recommended**: Hardware acceleration enabled for smooth animations

## Advanced Features

### Chart Customization
- Drag charts to reorder on dashboard
- Resize charts by dragging corners
- Hide/show specific charts
- Save custom layouts (persists across sessions)

### Collaborative Features
- Share chart configurations via URL
- Generate presentation-ready exports
- Schedule automated reports (admin feature)

### Mobile Usage
- Responsive design adapts to mobile screens
- Touch-friendly chart interactions
- Swipe navigation between chart views
- Simplified interface for small screens

## Data Interpretation Guide

### Understanding Metrics

#### Usage Analytics
- **Document Flow**: Shows document lifecycle stages
- **User Engagement**: Measures active user interactions
- **Adoption Rates**: Tracks feature usage over time

#### Performance Monitoring
- **Response Times**: API and page load performance
- **Throughput**: Requests processed per time period
- **Error Rates**: System reliability indicators

#### Business Intelligence
- **Conversion Funnels**: User journey success rates
- **Revenue Impact**: Financial performance metrics
- **User Segmentation**: Demographic and behavioral analysis

### Chart Patterns
- **Upward Trends**: Growth or increasing activity
- **Downward Trends**: Decline or optimization opportunities
- **Seasonal Patterns**: Regular cyclical changes
- **Anomalies**: Unusual spikes or drops requiring investigation

## Best Practices

### Daily Usage
1. Start with overview charts for general trends
2. Use drill-down for detailed investigation
3. Apply filters for specific analysis
4. Export key insights for reports

### Monitoring Workflows
1. Set up custom time ranges for regular reviews
2. Use real-time features during critical periods
3. Enable notifications for threshold alerts
4. Maintain consistent filter settings for comparisons

### Data Privacy
- Charts automatically anonymize sensitive data
- Export permissions are role-based
- Audit logs track all data access
- Report any data concerns to security team

## Getting Help

### Support Channels
- **Internal Help**: Click "?" icon in top navigation
- **Training Videos**: Available in Help Center
- **Email Support**: analytics-support@wesign.com
- **Documentation**: Complete guides at docs.wesign.com

### Feedback
- Use feedback button to suggest improvements
- Report bugs via support channel
- Request new chart types through product feedback

---
*Last Updated: January 29, 2025*
*Version: 1.0*
```

---

## Developer Documentation

### Technical Architecture Guide

```markdown
# Real-time Charts - Developer Documentation

## Architecture Overview

### Component Structure
```
realtime-charts/
├── realtime-charts-page.component.ts     # Main page component
├── components/
│   ├── chart-container/                  # Individual chart wrapper
│   ├── chart-filters/                    # Filtering interface
│   ├── chart-export/                     # Export functionality
│   └── connection-status/                # Real-time status indicator
├── services/
│   ├── chart-data.service.ts            # Data fetching and caching
│   ├── realtime.service.ts              # SignalR connection management
│   ├── chart-export.service.ts          # Export functionality
│   └── chart-security.service.ts        # Security and permissions
├── models/
│   ├── chart-config.interface.ts        # Chart configuration types
│   ├── chart-data.interface.ts          # Data structure definitions
│   └── filter-state.interface.ts        # Filter state management
└── store/
    ├── actions/                          # NgRx actions
    ├── reducers/                         # State reducers
    ├── effects/                          # Side effects
    └── selectors/                        # State selectors
```

### Key Technologies
- **Angular 15.2.10**: Core framework
- **NgRx**: State management
- **Chart.js 4.4.1**: Primary charting library
- **D3.js 7.8.5**: Custom visualizations
- **Plotly.js 2.26.0**: Interactive charts
- **SignalR**: Real-time communication
- **RxJS**: Reactive programming

## Component Development

### Chart Container Component

```typescript
// Basic chart container implementation
@Component({
  selector: 'app-chart-container',
  template: `
    <div class="chart-wrapper"
         [attr.aria-label]="chartConfig.title"
         [attr.data-testid]="'chart-container-' + chartConfig.id">

      <div class="chart-header">
        <h3>{{ chartConfig.title }}</h3>
        <div class="chart-controls">
          <button class="export-btn"
                  (click)="exportChart()"
                  [attr.aria-label]="'Export ' + chartConfig.title">
            <i class="icon-export"></i>
          </button>
        </div>
      </div>

      <div class="chart-content" #chartContainer>
        <canvas #chartCanvas
                [width]="canvasWidth"
                [height]="canvasHeight"
                [attr.aria-describedby]="chartConfig.id + '-description'">
        </canvas>

        <div [id]="chartConfig.id + '-description'" class="sr-only">
          {{ chartConfig.accessibleDescription }}
        </div>
      </div>

      <div class="chart-status" *ngIf="connectionStatus$ | async as status">
        <span class="status-indicator"
              [class.connected]="status === 'connected'"
              [class.disconnected]="status === 'disconnected'">
          {{ status }}
        </span>
      </div>
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ChartContainerComponent implements OnInit, OnDestroy {
  @Input() chartConfig!: ChartConfiguration;
  @Input() data$!: Observable<ChartDataPoint[]>;
  @ViewChild('chartCanvas') chartCanvas!: ElementRef<HTMLCanvasElement>;

  public connectionStatus$ = this.realtimeService.connectionState$;
  private chart: Chart | null = null;
  private destroy$ = new Subject<void>();

  constructor(
    private realtimeService: RealtimeService,
    private chartExportService: ChartExportService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.initializeChart();
    this.subscribeToDataUpdates();
  }

  private initializeChart(): void {
    const ctx = this.chartCanvas.nativeElement.getContext('2d');
    if (!ctx) return;

    this.chart = new Chart(ctx, {
      type: this.chartConfig.type,
      data: { datasets: [] },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        accessibility: {
          enabled: true,
          announceChart: true
        },
        plugins: {
          legend: {
            position: 'top'
          },
          tooltip: {
            mode: 'index',
            intersect: false
          }
        },
        interaction: {
          mode: 'nearest',
          axis: 'x',
          intersect: false
        },
        onHover: this.onChartHover.bind(this),
        onClick: this.onChartClick.bind(this)
      }
    });

    // Mark chart as ready for testing
    this.chartCanvas.nativeElement.setAttribute('data-chart-ready', 'true');
  }

  private subscribeToDataUpdates(): void {
    this.data$
      .pipe(takeUntil(this.destroy$))
      .subscribe(data => {
        this.updateChart(data);
      });
  }

  private updateChart(data: ChartDataPoint[]): void {
    if (!this.chart || !data) return;

    // Performance mark for monitoring
    performance.mark(`chart-update-start-${this.chartConfig.id}`);

    this.chart.data.datasets[0] = {
      label: this.chartConfig.title,
      data: data,
      ...this.chartConfig.styling
    };

    this.chart.update('none'); // No animation for real-time updates

    performance.mark(`chart-update-end-${this.chartConfig.id}`);
    performance.measure(
      `chart-update-${this.chartConfig.id}`,
      `chart-update-start-${this.chartConfig.id}`,
      `chart-update-end-${this.chartConfig.id}`
    );

    this.cdr.markForCheck();
  }

  exportChart(): void {
    this.chartExportService.exportChart(this.chart, this.chartConfig);
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();

    if (this.chart) {
      this.chart.destroy();
    }
  }
}
```

### State Management Patterns

```typescript
// Chart state management with NgRx
export interface ChartState {
  chartData: { [chartId: string]: ChartDataPoint[] };
  filters: FilterState;
  connectionStatus: ConnectionStatus;
  loading: boolean;
  error: string | null;
}

// Actions
export const loadChartData = createAction(
  '[Charts] Load Chart Data',
  props<{ chartId: string; timeRange: TimeRange }>()
);

export const chartDataLoaded = createAction(
  '[Charts] Chart Data Loaded',
  props<{ chartId: string; data: ChartDataPoint[] }>()
);

export const realtimeDataUpdate = createAction(
  '[Charts] Realtime Data Update',
  props<{ chartId: string; data: ChartDataPoint[] }>()
);

// Reducer
const chartReducer = createReducer(
  initialState,
  on(loadChartData, (state, { chartId }) => ({
    ...state,
    loading: true,
    error: null
  })),
  on(chartDataLoaded, (state, { chartId, data }) => ({
    ...state,
    chartData: { ...state.chartData, [chartId]: data },
    loading: false
  })),
  on(realtimeDataUpdate, (state, { chartId, data }) => ({
    ...state,
    chartData: { ...state.chartData, [chartId]: data }
  }))
);

// Effects
@Injectable()
export class ChartEffects {
  loadChartData$ = createEffect(() =>
    this.actions$.pipe(
      ofType(loadChartData),
      switchMap(({ chartId, timeRange }) =>
        this.chartDataService.loadChartData(chartId, timeRange).pipe(
          map(data => chartDataLoaded({ chartId, data })),
          catchError(error => of(chartDataLoadError({ error: error.message })))
        )
      )
    )
  );

  realtimeUpdates$ = createEffect(() =>
    this.realtimeService.chartUpdates$.pipe(
      map(({ chartId, data }) => realtimeDataUpdate({ chartId, data }))
    )
  );
}
```

### Testing Patterns

```typescript
// Component testing example
describe('ChartContainerComponent', () => {
  let component: ChartContainerComponent;
  let fixture: ComponentFixture<ChartContainerComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [ChartContainerComponent],
      providers: [
        { provide: RealtimeService, useClass: MockRealtimeService },
        { provide: ChartExportService, useClass: MockChartExportService }
      ]
    });

    fixture = TestBed.createComponent(ChartContainerComponent);
    component = fixture.componentInstance;

    // Setup test data
    component.chartConfig = {
      id: 'test-chart',
      type: 'line',
      title: 'Test Chart'
    };

    component.data$ = of([
      { x: 1, y: 10 },
      { x: 2, y: 20 }
    ]);
  });

  it('should initialize chart on ngOnInit', async () => {
    component.ngOnInit();
    fixture.detectChanges();
    await fixture.whenStable();

    expect(component.chart).toBeTruthy();
    expect(fixture.nativeElement.querySelector('canvas')).toBeTruthy();
  });

  it('should update chart when data changes', fakeAsync(() => {
    component.ngOnInit();
    fixture.detectChanges();
    tick();

    const newData = [{ x: 3, y: 30 }, { x: 4, y: 40 }];
    component.data$ = of(newData);

    tick();
    fixture.detectChanges();

    expect(component.chart?.data.datasets[0].data).toEqual(newData);
  }));
});
```

## Performance Optimization

### Data Decimation
```typescript
// Implement data decimation for large datasets
export class DataDecimationService {
  decimateData(data: ChartDataPoint[], maxPoints: number): ChartDataPoint[] {
    if (data.length <= maxPoints) return data;

    const step = Math.ceil(data.length / maxPoints);
    const decimated: ChartDataPoint[] = [];

    for (let i = 0; i < data.length; i += step) {
      decimated.push(data[i]);
    }

    // Always include the last point
    if (decimated[decimated.length - 1] !== data[data.length - 1]) {
      decimated.push(data[data.length - 1]);
    }

    return decimated;
  }
}
```

### Memory Management
```typescript
// Chart cleanup utility
export class ChartCleanupService {
  cleanupChart(chart: Chart): void {
    // Remove event listeners
    chart.canvas.removeEventListener('click', chart.onClick);
    chart.canvas.removeEventListener('mousemove', chart.onHover);

    // Clear animations
    chart.stop();

    // Destroy chart instance
    chart.destroy();
  }

  cleanupChartData(data: any[]): void {
    // Clear large arrays
    data.length = 0;

    // Force garbage collection if available
    if ((window as any).gc) {
      (window as any).gc();
    }
  }
}
```

## Security Implementation

### Data Sanitization
```typescript
// Chart data sanitization
export class ChartDataSanitizer {
  sanitizeChartData(data: any[]): any[] {
    return data.map(item => this.sanitizeItem(item));
  }

  private sanitizeItem(item: any): any {
    const sanitized: any = {};

    Object.keys(item).forEach(key => {
      if (typeof item[key] === 'string') {
        sanitized[key] = this.sanitizeString(item[key]);
      } else if (typeof item[key] === 'number') {
        sanitized[key] = this.sanitizeNumber(item[key]);
      } else {
        sanitized[key] = item[key];
      }
    });

    return sanitized;
  }

  private sanitizeString(value: string): string {
    // Remove HTML tags and script content
    return value
      .replace(/<script\b[^<]*(?:(?!<\/script>)<[^<]*)*<\/script>/gi, '')
      .replace(/javascript:/gi, '')
      .replace(/on\w+=/gi, '');
  }

  private sanitizeNumber(value: number): number {
    // Ensure number is finite and within reasonable bounds
    if (!isFinite(value) || isNaN(value)) return 0;
    return Math.max(-1e10, Math.min(1e10, value));
  }
}
```

## Deployment Guidelines

### Build Configuration
```json
{
  "production": {
    "optimization": true,
    "extractCss": true,
    "namedChunks": false,
    "aot": true,
    "extractLicenses": true,
    "vendorChunk": false,
    "buildOptimizer": true,
    "budgets": [
      {
        "type": "initial",
        "maximumWarning": "2mb",
        "maximumError": "5mb"
      },
      {
        "type": "anyComponentStyle",
        "maximumWarning": "6kb",
        "maximumError": "10kb"
      }
    ]
  }
}
```

### Environment Configuration
```typescript
// Chart-specific environment settings
export const environment = {
  production: true,
  charts: {
    apiUrl: 'https://api.wesign.com',
    signalRUrl: 'https://api.wesign.com/analyticsHub',
    maxDataPoints: 10000,
    updateInterval: 5000,
    enableDataDecimation: true,
    enablePerformanceMonitoring: true
  }
};
```

## Maintenance Procedures

### Regular Maintenance Tasks
1. **Weekly**: Review performance metrics and error logs
2. **Monthly**: Update chart libraries and dependencies
3. **Quarterly**: Performance optimization review
4. **Annually**: Security audit and penetration testing

### Monitoring Checklist
- [ ] Chart rendering performance within thresholds
- [ ] Memory usage stable (no leaks)
- [ ] Real-time connection reliability
- [ ] Error rates below 1%
- [ ] User accessibility compliance
- [ ] Security vulnerability scans

### Version Update Process
1. Test new chart library versions in staging
2. Validate all chart types render correctly
3. Run full performance test suite
4. Update security configuration if needed
5. Deploy with feature flags for gradual rollout

---
*Developer Documentation Version 1.0*
*Last Updated: January 29, 2025*
```

---

## API Documentation

### Chart Data API Reference

```yaml
# OpenAPI 3.0 Specification for Chart APIs
openapi: 3.0.3
info:
  title: WeSign Charts API
  description: Real-time chart data and configuration API
  version: 1.0.0
  contact:
    name: WeSign Development Team
    email: dev@wesign.com

servers:
  - url: https://api.wesign.com/v1
    description: Production server
  - url: https://staging-api.wesign.com/v1
    description: Staging server

paths:
  /analytics/charts/usage-trends:
    get:
      summary: Get usage analytics chart data
      description: Retrieves time-series data for document usage and user engagement metrics
      tags:
        - Charts
      security:
        - bearerAuth: []
      parameters:
        - name: timeRange
          in: query
          description: Time range for data retrieval
          required: false
          schema:
            type: string
            enum: [1h, 24h, 7d, 30d, custom]
            default: 24h
        - name: startDate
          in: query
          description: Start date for custom time range (ISO 8601)
          required: false
          schema:
            type: string
            format: date-time
        - name: endDate
          in: query
          description: End date for custom time range (ISO 8601)
          required: false
          schema:
            type: string
            format: date-time
        - name: granularity
          in: query
          description: Data point granularity
          required: false
          schema:
            type: string
            enum: [minute, hour, day, week]
            default: hour
        - name: segments
          in: query
          description: User segments to filter by
          required: false
          schema:
            type: array
            items:
              type: string
      responses:
        '200':
          description: Chart data successfully retrieved
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/ChartDataResponse'
        '400':
          description: Invalid request parameters
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/ErrorResponse'
        '401':
          description: Authentication required
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/ErrorResponse'
        '403':
          description: Insufficient permissions
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/ErrorResponse'

  /analytics/charts/performance-metrics:
    get:
      summary: Get performance monitoring chart data
      description: Retrieves system performance metrics including response times, throughput, and error rates
      tags:
        - Charts
      security:
        - bearerAuth: []
      parameters:
        - name: metrics
          in: query
          description: Specific metrics to retrieve
          required: false
          schema:
            type: array
            items:
              type: string
              enum: [response_time, throughput, error_rate, cpu_usage, memory_usage]
        - name: services
          in: query
          description: Services to monitor
          required: false
          schema:
            type: array
            items:
              type: string
      responses:
        '200':
          description: Performance data successfully retrieved
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/PerformanceDataResponse'

  /analytics/charts/export:
    post:
      summary: Export chart data or image
      description: Generates chart export in specified format with security controls
      tags:
        - Charts
        - Export
      security:
        - bearerAuth: []
      requestBody:
        required: true
        content:
          application/json:
            schema:
              $ref: '#/components/schemas/ExportRequest'
      responses:
        '200':
          description: Export generated successfully
          content:
            application/octet-stream:
              schema:
                type: string
                format: binary
          headers:
            Content-Disposition:
              description: Attachment filename
              schema:
                type: string
            X-Export-Audit-Id:
              description: Audit trail identifier
              schema:
                type: string
        '400':
          description: Invalid export request
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/ErrorResponse'

components:
  securitySchemes:
    bearerAuth:
      type: http
      scheme: bearer
      bearerFormat: JWT

  schemas:
    ChartDataResponse:
      type: object
      required:
        - data
        - metadata
      properties:
        data:
          type: array
          items:
            $ref: '#/components/schemas/ChartDataPoint'
        metadata:
          $ref: '#/components/schemas/ChartMetadata'
        pagination:
          $ref: '#/components/schemas/Pagination'

    ChartDataPoint:
      type: object
      required:
        - timestamp
        - value
      properties:
        timestamp:
          type: string
          format: date-time
          description: Data point timestamp
        value:
          type: number
          description: Numeric value for the data point
        label:
          type: string
          description: Human-readable label
        category:
          type: string
          description: Data category or segment
        metadata:
          type: object
          description: Additional data point metadata

    ChartMetadata:
      type: object
      required:
        - chartType
        - title
      properties:
        chartType:
          type: string
          enum: [line, bar, pie, doughnut, area, scatter]
        title:
          type: string
          description: Chart title
        description:
          type: string
          description: Chart description for accessibility
        unit:
          type: string
          description: Data unit (e.g., "milliseconds", "count", "percentage")
        aggregation:
          type: string
          enum: [sum, average, count, min, max]
        refreshRate:
          type: integer
          description: Refresh rate in seconds for real-time updates
        dataSource:
          type: string
          description: Source system for the data

    PerformanceDataResponse:
      type: object
      required:
        - metrics
        - timestamp
      properties:
        metrics:
          type: object
          additionalProperties:
            $ref: '#/components/schemas/MetricValue'
        timestamp:
          type: string
          format: date-time
        systemStatus:
          type: string
          enum: [healthy, warning, critical]

    MetricValue:
      type: object
      required:
        - value
        - unit
      properties:
        value:
          type: number
        unit:
          type: string
        threshold:
          $ref: '#/components/schemas/MetricThreshold'

    MetricThreshold:
      type: object
      properties:
        warning:
          type: number
        critical:
          type: number

    ExportRequest:
      type: object
      required:
        - chartId
        - format
      properties:
        chartId:
          type: string
          description: Identifier of chart to export
        format:
          type: string
          enum: [png, svg, pdf, csv, excel]
        options:
          $ref: '#/components/schemas/ExportOptions'

    ExportOptions:
      type: object
      properties:
        width:
          type: integer
          minimum: 100
          maximum: 4000
          default: 1200
        height:
          type: integer
          minimum: 100
          maximum: 3000
          default: 800
        includeTitle:
          type: boolean
          default: true
        includeLegend:
          type: boolean
          default: true
        backgroundColor:
          type: string
          pattern: '^#[0-9A-Fa-f]{6}$'
          default: '#FFFFFF'
        watermark:
          type: boolean
          description: Add security watermark (automatically applied based on role)

    Pagination:
      type: object
      properties:
        page:
          type: integer
          minimum: 1
        limit:
          type: integer
          minimum: 1
          maximum: 1000
        total:
          type: integer
        hasNext:
          type: boolean
        hasPrevious:
          type: boolean

    ErrorResponse:
      type: object
      required:
        - error
        - message
      properties:
        error:
          type: string
          description: Error code
        message:
          type: string
          description: Human-readable error message
        details:
          type: object
          description: Additional error details
        timestamp:
          type: string
          format: date-time
        requestId:
          type: string
          description: Request identifier for support

# WebSocket API for Real-time Updates
websocket:
  url: wss://api.wesign.com/analyticsHub
  protocol: signalr

  connection:
    headers:
      Authorization: Bearer {jwt_token}
      X-Client-Type: WeSign-Charts

  events:
    # Client to Server
    JoinGroup:
      description: Join chart update group
      parameters:
        groupName: string # Chart type identifier

    LeaveGroup:
      description: Leave chart update group
      parameters:
        groupName: string

    # Server to Client
    ChartDataUpdate:
      description: Real-time chart data update
      payload:
        chartId: string
        data: ChartDataPoint[]
        timestamp: string

    SystemStatus:
      description: System status notification
      payload:
        status: string
        message: string
        affectedCharts: string[]

    ConnectionValidated:
      description: Connection security validation
      payload:
        isValid: boolean
        allowedGroups: string[]
        expiresAt: string

# Rate Limiting
rateLimits:
  chartData:
    requests: 100
    window: 60 # seconds

  export:
    requests: 10
    window: 300 # seconds

  realtime:
    connections: 5
    perUser: true

# Authentication & Authorization
security:
  authentication:
    type: JWT
    location: Authorization header
    format: "Bearer {token}"

  authorization:
    roles:
      ProductManager:
        charts: [usage-trends, business-intelligence, performance-metrics]
        exports: [png, svg, pdf, csv, excel]

      Support:
        charts: [error-rates, user-activity, system-health]
        exports: [png]

      Operations:
        charts: [performance-metrics, system-health, infrastructure]
        exports: [png, csv]
```

---

## Configuration Documentation

### Environment Configuration Guide

```typescript
// Complete environment configuration reference
export interface ChartEnvironmentConfig {
  production: boolean;

  // API Configuration
  api: {
    baseUrl: string;
    chartEndpoint: string;
    exportEndpoint: string;
    timeout: number;
    retryAttempts: number;
  };

  // Real-time Configuration
  realtime: {
    hubUrl: string;
    reconnectPolicy: {
      maxAttempts: number;
      backoffMultiplier: number;
      maxDelay: number;
    };
    heartbeatInterval: number;
  };

  // Performance Configuration
  performance: {
    maxDataPoints: number;
    updateThrottleMs: number;
    enableDataDecimation: boolean;
    memoryCleanupInterval: number;
    enablePerformanceMonitoring: boolean;
  };

  // Security Configuration
  security: {
    enableDataSanitization: boolean;
    allowedDomains: string[];
    cspEnabled: boolean;
    auditLogging: boolean;
  };

  // Feature Flags
  features: {
    realTimeUpdates: boolean;
    chartExport: boolean;
    customCharts: boolean;
    aiInsights: boolean;
    collaborativeFilters: boolean;
  };

  // UI Configuration
  ui: {
    defaultTheme: string;
    enableAnimations: boolean;
    showConnectionStatus: boolean;
    autoRefreshInterval: number;
  };
}

// Production Environment
export const productionConfig: ChartEnvironmentConfig = {
  production: true,
  api: {
    baseUrl: 'https://api.wesign.com',
    chartEndpoint: '/v1/analytics/charts',
    exportEndpoint: '/v1/analytics/export',
    timeout: 30000,
    retryAttempts: 3
  },
  realtime: {
    hubUrl: 'wss://api.wesign.com/analyticsHub',
    reconnectPolicy: {
      maxAttempts: 10,
      backoffMultiplier: 2,
      maxDelay: 30000
    },
    heartbeatInterval: 30000
  },
  performance: {
    maxDataPoints: 10000,
    updateThrottleMs: 300,
    enableDataDecimation: true,
    memoryCleanupInterval: 60000,
    enablePerformanceMonitoring: true
  },
  security: {
    enableDataSanitization: true,
    allowedDomains: ['wesign.com', 'api.wesign.com'],
    cspEnabled: true,
    auditLogging: true
  },
  features: {
    realTimeUpdates: true,
    chartExport: true,
    customCharts: true,
    aiInsights: true,
    collaborativeFilters: true
  },
  ui: {
    defaultTheme: 'wesign-light',
    enableAnimations: true,
    showConnectionStatus: true,
    autoRefreshInterval: 5000
  }
};

// Development Environment
export const developmentConfig: ChartEnvironmentConfig = {
  production: false,
  api: {
    baseUrl: 'http://localhost:5000',
    chartEndpoint: '/api/analytics/charts',
    exportEndpoint: '/api/analytics/export',
    timeout: 10000,
    retryAttempts: 1
  },
  realtime: {
    hubUrl: 'ws://localhost:5000/analyticsHub',
    reconnectPolicy: {
      maxAttempts: 3,
      backoffMultiplier: 1.5,
      maxDelay: 10000
    },
    heartbeatInterval: 10000
  },
  performance: {
    maxDataPoints: 50000, // Allow more data for testing
    updateThrottleMs: 100,
    enableDataDecimation: false,
    memoryCleanupInterval: 30000,
    enablePerformanceMonitoring: true
  },
  security: {
    enableDataSanitization: true,
    allowedDomains: ['localhost', '127.0.0.1'],
    cspEnabled: false, // Disabled for development flexibility
    auditLogging: false
  },
  features: {
    realTimeUpdates: true,
    chartExport: true,
    customCharts: true,
    aiInsights: false, // Disabled in development
    collaborativeFilters: true
  },
  ui: {
    defaultTheme: 'wesign-light',
    enableAnimations: true,
    showConnectionStatus: true,
    autoRefreshInterval: 2000 // Faster refresh for development
  }
};
```

### Chart Library Configuration

```typescript
// Chart.js default configuration
export const CHART_JS_DEFAULTS = {
  responsive: true,
  maintainAspectRatio: false,

  // Performance optimizations
  animation: {
    duration: 750,
    easing: 'easeInOutQuart'
  },

  // Accessibility
  accessibility: {
    enabled: true,
    announceChart: true,
    announceNewDatasets: true
  },

  // Plugins
  plugins: {
    legend: {
      position: 'top' as const,
      labels: {
        usePointStyle: true,
        padding: 20
      }
    },
    tooltip: {
      mode: 'index' as const,
      intersect: false,
      backgroundColor: 'rgba(0, 0, 0, 0.8)',
      titleColor: '#fff',
      bodyColor: '#fff',
      borderColor: '#666',
      borderWidth: 1
    }
  },

  // Scales
  scales: {
    x: {
      display: true,
      grid: {
        display: true,
        color: 'rgba(0, 0, 0, 0.1)'
      }
    },
    y: {
      display: true,
      beginAtZero: true,
      grid: {
        display: true,
        color: 'rgba(0, 0, 0, 0.1)'
      }
    }
  },

  // Interaction
  interaction: {
    mode: 'nearest' as const,
    axis: 'x' as const,
    intersect: false
  }
};

// D3.js configuration for custom charts
export const D3_CHART_CONFIG = {
  margin: { top: 20, right: 30, bottom: 40, left: 40 },
  animation: {
    duration: 750,
    ease: 'cubicInOut'
  },
  colors: [
    '#1f77b4', '#ff7f0e', '#2ca02c', '#d62728',
    '#9467bd', '#8c564b', '#e377c2', '#7f7f7f'
  ]
};

// Plotly.js configuration
export const PLOTLY_CONFIG = {
  displayModeBar: true,
  modeBarButtonsToRemove: ['pan2d', 'lasso2d', 'select2d'],
  responsive: true,
  locale: 'en',
  displaylogo: false
};
```

---

## Troubleshooting Guide

### Common Issues and Solutions

```markdown
# Real-time Charts Troubleshooting Guide

## Connection Issues

### Problem: "Real-time connection failed"
**Symptoms:**
- Red connection indicator
- Charts not updating automatically
- "Offline" status displayed

**Diagnostic Steps:**
1. Check browser network tab for WebSocket errors
2. Verify authentication token validity
3. Test network connectivity to API endpoints

**Solutions:**
1. **Authentication Issue:**
   ```bash
   # Check token expiration
   jwt-decode YOUR_TOKEN_HERE

   # Refresh token if expired
   POST /api/auth/refresh
   ```

2. **Network Connectivity:**
   ```bash
   # Test API connectivity
   curl https://api.wesign.com/health

   # Test WebSocket connectivity
   wscat -c wss://api.wesign.com/analyticsHub
   ```

3. **Firewall/Proxy Issues:**
   - Ensure WebSocket traffic is allowed
   - Check for corporate proxy settings
   - Verify ports 80/443 are accessible

### Problem: "SignalR connection drops frequently"
**Symptoms:**
- Intermittent connection status changes
- Missed real-time updates
- Frequent reconnection attempts

**Solutions:**
1. **Increase Connection Timeout:**
   ```typescript
   // In environment config
   realtime: {
     heartbeatInterval: 60000, // Increase from 30s to 60s
     reconnectPolicy: {
       maxAttempts: 15, // Increase retry attempts
       maxDelay: 60000  // Increase max delay
     }
   }
   ```

2. **Server Configuration:**
   ```csharp
   // Server-side SignalR configuration
   services.AddSignalR(options =>
   {
       options.KeepAliveInterval = TimeSpan.FromSeconds(30);
       options.ClientTimeoutInterval = TimeSpan.FromSeconds(120);
   });
   ```

## Performance Issues

### Problem: "Charts loading slowly"
**Symptoms:**
- Page load time > 3 seconds
- Charts appear with delay
- Browser becomes unresponsive

**Diagnostic Steps:**
1. Open browser DevTools → Performance tab
2. Record page load and identify bottlenecks
3. Check Network tab for slow API requests
4. Monitor Memory tab for memory leaks

**Solutions:**
1. **Enable Data Decimation:**
   ```typescript
   performance: {
     enableDataDecimation: true,
     maxDataPoints: 5000 // Reduce from 10000
   }
   ```

2. **Optimize Chart Configuration:**
   ```typescript
   // Disable animations for better performance
   chartOptions: {
     animation: false,
     responsiveAnimationDuration: 0
   }
   ```

3. **Implement Lazy Loading:**
   ```typescript
   // Load charts on demand
   @Component({
     template: `
       <app-chart-container
         *ngIf="chartVisible"
         [data]="chartData$">
       </app-chart-container>
     `
   })
   ```

### Problem: "High memory usage"
**Symptoms:**
- Browser tab consuming > 200MB RAM
- Page becomes sluggish over time
- Browser crashes on large datasets

**Solutions:**
1. **Implement Proper Cleanup:**
   ```typescript
   ngOnDestroy(): void {
     // Destroy chart instances
     if (this.chart) {
       this.chart.destroy();
       this.chart = null;
     }

     // Unsubscribe from observables
     this.destroy$.next();
     this.destroy$.complete();

     // Clear large data arrays
     this.chartData.length = 0;
   }
   ```

2. **Use Object Pooling:**
   ```typescript
   // Reuse chart instances instead of creating new ones
   export class ChartPool {
     private pool: Chart[] = [];

     getChart(): Chart {
       return this.pool.pop() || this.createNewChart();
     }

     returnChart(chart: Chart): void {
       chart.clear();
       this.pool.push(chart);
     }
   }
   ```

## Data Issues

### Problem: "Missing or incorrect chart data"
**Symptoms:**
- Empty charts displayed
- Data doesn't match expected values
- Some chart types show "No data available"

**Diagnostic Steps:**
1. Check browser Network tab for API response status
2. Verify API response data structure
3. Check user role permissions
4. Review applied filters

**Solutions:**
1. **Verify API Response:**
   ```bash
   # Test chart data API directly
   curl -H "Authorization: Bearer YOUR_TOKEN" \
        https://api.wesign.com/v1/analytics/charts/usage-trends
   ```

2. **Check Data Transformation:**
   ```typescript
   // Add logging to data processing
   transformChartData(rawData: any[]): ChartDataPoint[] {
     console.log('Raw data:', rawData);

     const transformed = rawData.map(item => ({
       x: new Date(item.timestamp),
       y: item.value
     }));

     console.log('Transformed data:', transformed);
     return transformed;
   }
   ```

3. **Validate User Permissions:**
   ```typescript
   // Check if user has access to chart type
   canAccessChart(chartType: ChartType): boolean {
     const userRole = this.authService.getCurrentUserRole();
     const allowedCharts = this.securityService.getAllowedCharts(userRole);
     return allowedCharts.includes(chartType);
   }
   ```

## Export Issues

### Problem: "Chart export fails"
**Symptoms:**
- Export button unresponsive
- Download doesn't start
- Exported file is corrupted

**Solutions:**
1. **Check Export Permissions:**
   ```typescript
   async exportChart(format: string): Promise<void> {
     try {
       const hasPermission = await this.securityService
         .validateExportPermission(format).toPromise();

       if (!hasPermission) {
         throw new Error(`Export format ${format} not permitted`);
       }

       // Proceed with export
     } catch (error) {
       console.error('Export failed:', error);
       this.showErrorMessage(error.message);
     }
   }
   ```

2. **Handle Large Exports:**
   ```typescript
   // For large charts, use worker thread
   async exportLargeChart(chartData: any[]): Promise<Blob> {
     return new Promise((resolve, reject) => {
       const worker = new Worker('./chart-export.worker.ts');

       worker.postMessage({
         type: 'export',
         data: chartData
       });

       worker.onmessage = ({ data }) => {
         if (data.success) {
           resolve(data.blob);
         } else {
           reject(new Error(data.error));
         }
         worker.terminate();
       };
     });
   }
   ```

## Browser Compatibility Issues

### Problem: "Charts not displaying in older browsers"
**Symptoms:**
- Blank chart areas
- JavaScript errors in console
- Features not working

**Solutions:**
1. **Add Polyfills:**
   ```typescript
   // polyfills.ts
   import 'core-js/stable';
   import 'regenerator-runtime/runtime';

   // Chart.js polyfills
   import 'chartjs-adapter-date-fns';
   ```

2. **Feature Detection:**
   ```typescript
   // Check for required features
   canRenderCharts(): boolean {
     return !!(
       window.HTMLCanvasElement &&
       window.WebSocket &&
       window.IntersectionObserver
     );
   }
   ```

3. **Graceful Degradation:**
   ```typescript
   @Component({
     template: `
       <div *ngIf="canRenderCharts(); else fallbackTemplate">
         <app-chart-container [data]="chartData"></app-chart-container>
       </div>

       <ng-template #fallbackTemplate>
         <app-data-table [data]="chartData"></app-data-table>
       </ng-template>
     `
   })
   ```

## Development Debugging

### Debugging Real-time Updates
```typescript
// Enable debug logging for SignalR
const connection = new HubConnectionBuilder()
  .withUrl('/analyticsHub')
  .configureLogging(LogLevel.Debug) // Enable debug logs
  .build();

// Log all received messages
connection.on('ChartDataUpdate', (data) => {
  console.log('Chart update received:', {
    timestamp: new Date().toISOString(),
    data: data,
    size: JSON.stringify(data).length
  });
});
```

### Performance Debugging
```typescript
// Monitor chart render performance
class ChartPerformanceMonitor {
  measureRenderTime(chartId: string, renderFn: () => void): number {
    const startTime = performance.now();
    renderFn();
    const endTime = performance.now();

    const renderTime = endTime - startTime;
    console.log(`Chart ${chartId} render time: ${renderTime.toFixed(2)}ms`);

    return renderTime;
  }
}
```

### Memory Debugging
```typescript
// Monitor memory usage
function logMemoryUsage(label: string): void {
  if ('memory' in performance) {
    const memory = (performance as any).memory;
    console.log(`Memory [${label}]:`, {
      used: `${(memory.usedJSHeapSize / 1024 / 1024).toFixed(2)}MB`,
      total: `${(memory.totalJSHeapSize / 1024 / 1024).toFixed(2)}MB`,
      limit: `${(memory.jsHeapSizeLimit / 1024 / 1024).toFixed(2)}MB`
    });
  }
}
```

## Getting Help

### Support Escalation Process
1. **Level 1**: Check this troubleshooting guide
2. **Level 2**: Contact development team with:
   - Browser and version
   - Console error messages
   - Steps to reproduce
   - User role and permissions
3. **Level 3**: Create detailed bug report with:
   - Network HAR file
   - Performance profiling data
   - Environment configuration

### Useful Debug Commands
```bash
# Check application health
curl https://api.wesign.com/health

# Validate JWT token
echo "YOUR_TOKEN" | base64 -d | jq .

# Test WebSocket connection
wscat -c wss://api.wesign.com/analyticsHub

# Check bundle size
npx webpack-bundle-analyzer dist/stats.json

# Run performance tests
npm run test:performance
```

---
*Troubleshooting Guide Version 1.0*
*Last Updated: January 29, 2025*
```

---

## Next Steps

✅ **PROCEED TO STEP L: Deployment**

The comprehensive documentation suite for Real-time Charts is complete, covering all aspects from user guides to technical implementation details. The documentation includes user guides, developer documentation, API specifications, configuration guides, and troubleshooting procedures to support both end users and development teams.