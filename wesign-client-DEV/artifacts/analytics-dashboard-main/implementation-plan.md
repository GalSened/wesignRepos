# Implementation Plan - Analytics Dashboard Main Page

**PAGE_KEY**: analytics-dashboard-main
**DATE**: 2025-01-29
**ESTIMATED EFFORT**: 3-4 development days

## Implementation Overview

This plan details the step-by-step implementation of the Analytics Dashboard main page following the component design and acceptance criteria. The implementation will focus on production-ready code with real-time capabilities and comprehensive error handling.

## Phase 1: Core Foundation (Day 1)

### 1.1 Update Data Models
**File**: `src/app/models/analytics/analytics-models.ts`
**Duration**: 2 hours

```typescript
// Enhanced models based on real WeSign data structures
export interface KpiSnapshot {
  timestamp: string;
  organizationId?: string; // For role-based filtering
  dau: number;
  mau: number;
  successRate: number;
  avgTimeToSign: number; // in seconds
  totalDocuments: number;
  activeOrganizations: number;
  trends: {
    [key: string]: TrendData;
  };
  metadata: {
    dataAge: number;
    queryDuration: number;
    cacheHit: boolean;
  };
}

export interface RealtimeUpdate {
  type: 'kpi_update' | 'health_change' | 'connection_status';
  data: any;
  timestamp: string;
  targetRoles?: string[];
}

export interface AnalyticsFilters {
  timeRange: TimeRange;
  organizationId?: string;
  documentTypes?: string[];
  userRoles?: string[];
}
```

### 1.2 Enhanced Analytics API Service
**File**: `src/app/services/analytics-api.service.ts`
**Duration**: 4 hours

```typescript
@Injectable({
  providedIn: 'root'
})
export class AnalyticsApiService {
  private signalRConnection: HubConnection | null = null;
  private connectionState$ = new BehaviorSubject<ConnectionState>({
    status: 'disconnected',
    reconnectAttempts: 0
  });
  private realtimeUpdates$ = new Subject<RealtimeUpdate>();

  constructor(
    private http: HttpClient,
    private authService: AuthService,
    private logger: LoggerService,
    private cdr: ChangeDetectorRef
  ) {}

  // Core API methods with error handling
  getLatestKPIs(filters: AnalyticsFilters): Observable<KpiSnapshot> {
    const params = new HttpParams()
      .set('timeRange', filters.timeRange)
      .set('orgId', filters.organizationId || 'all');

    return this.http.get<KpiSnapshot>('/api/analytics/kpi/latest', { params })
      .pipe(
        timeout(5000),
        retry({ count: 2, delay: 1000 }),
        catchError(this.handleApiError.bind(this))
      );
  }

  // SignalR real-time implementation
  async initializeSignalRConnection(): Promise<void> {
    try {
      this.signalRConnection = new HubConnectionBuilder()
        .withUrl('/analyticsHub', {
          accessTokenFactory: () => this.authService.getToken()
        })
        .withAutomaticReconnect([0, 2000, 10000, 30000])
        .configureLogging(LogLevel.Information)
        .build();

      this.setupSignalRHandlers();
      await this.signalRConnection.start();

      await this.signalRConnection.invoke('JoinAnalyticsStream');
      this.connectionState$.next({ status: 'connected', reconnectAttempts: 0 });

    } catch (error) {
      this.handleSignalRError(error);
    }
  }

  private setupSignalRHandlers(): void {
    if (!this.signalRConnection) return;

    this.signalRConnection.on('AnalyticsUpdate', (update: RealtimeUpdate) => {
      this.realtimeUpdates$.next(update);
    });

    this.signalRConnection.on('HealthStatusChanged', (health: HealthStatus) => {
      this.realtimeUpdates$.next({
        type: 'health_change',
        data: health,
        timestamp: new Date().toISOString()
      });
    });

    this.signalRConnection.onreconnecting(() => {
      this.connectionState$.next({ status: 'reconnecting', reconnectAttempts: 0 });
    });

    this.signalRConnection.onreconnected(() => {
      this.connectionState$.next({ status: 'connected', reconnectAttempts: 0 });
      this.signalRConnection?.invoke('JoinAnalyticsStream');
    });
  }
}
```

### 1.3 Main Dashboard Component Foundation
**File**: `src/app/components/dashboard/analytics-dashboard/analytics-dashboard.component.ts`
**Duration**: 3 hours

```typescript
@Component({
  selector: 'sgn-analytics-dashboard',
  templateUrl: './analytics-dashboard.component.html',
  styleUrls: ['./analytics-dashboard.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
  animations: [AnalyticsAnimations.valueChange, AnalyticsAnimations.connectionPulse]
})
export class AnalyticsDashboardComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();
  private refreshInterval$ = new Subject<void>();

  // State management
  kpiData$ = new BehaviorSubject<KpiSnapshot | null>(null);
  connectionState$ = new BehaviorSubject<ConnectionState>({ status: 'disconnected', reconnectAttempts: 0 });
  dataFreshness$ = new BehaviorSubject<DataFreshness>({ age: 0, status: 'fresh', lastUpdated: new Date() });
  healthStatus$ = new BehaviorSubject<HealthStatus | null>(null);
  isLoading$ = new BehaviorSubject<boolean>(true);

  // User controls
  selectedTimeRange = '24h';
  selectedOrganization = 'all';
  autoRefreshEnabled = true;
  realtimeEnabled = true;

  constructor(
    private analyticsService: AnalyticsApiService,
    private cdr: ChangeDetectorRef,
    private translateService: TranslateService,
    private notificationService: NotificationService
  ) {}

  async ngOnInit(): Promise<void> {
    await this.initializeComponent();
    this.setupAutoRefresh();
    this.setupRealtimeFeatures();
    this.monitorDataFreshness();
  }

  private async initializeComponent(): Promise<void> {
    try {
      // Load initial data
      await this.loadDashboardData();

      // Initialize real-time if enabled
      if (this.realtimeEnabled) {
        await this.analyticsService.initializeSignalRConnection();
      }

      this.isLoading$.next(false);
    } catch (error) {
      this.handleInitializationError(error);
    }
  }

  private async loadDashboardData(): Promise<void> {
    const filters: AnalyticsFilters = {
      timeRange: this.selectedTimeRange as TimeRange,
      organizationId: this.selectedOrganization === 'all' ? undefined : this.selectedOrganization
    };

    const kpiData = await this.analyticsService.getLatestKPIs(filters).toPromise();
    this.kpiData$.next(kpiData);
    this.updateDataFreshness(kpiData?.timestamp);
  }
}
```

## Phase 2: Real-time Features (Day 2)

### 2.1 SignalR Integration
**Duration**: 4 hours

Implement comprehensive real-time update handling:

```typescript
private setupRealtimeFeatures(): void {
  // Subscribe to real-time updates
  this.analyticsService.getRealtimeUpdates()
    .pipe(takeUntil(this.destroy$))
    .subscribe(update => this.handleRealtimeUpdate(update));

  // Monitor connection state
  this.analyticsService.getConnectionState()
    .pipe(takeUntil(this.destroy$))
    .subscribe(state => {
      this.connectionState$.next(state);
      this.cdr.markForCheck();
    });
}

private handleRealtimeUpdate(update: RealtimeUpdate): void {
  switch (update.type) {
    case 'kpi_update':
      this.mergeKpiUpdate(update.data);
      break;
    case 'health_change':
      this.healthStatus$.next(update.data);
      break;
    case 'connection_status':
      this.connectionState$.next(update.data);
      break;
  }

  this.updateDataFreshness(update.timestamp);
  this.cdr.markForCheck();
}

private mergeKpiUpdate(newData: Partial<KpiSnapshot>): void {
  const currentData = this.kpiData$.value;
  if (!currentData) return;

  const mergedData = { ...currentData, ...newData };
  this.kpiData$.next(mergedData);

  // Trigger animations for changed values
  this.triggerValueChangeAnimations(currentData, mergedData);
}
```

### 2.2 Auto-refresh Mechanism
**Duration**: 2 hours

```typescript
private setupAutoRefresh(): void {
  // Auto-refresh every 30 seconds when enabled
  interval(30000)
    .pipe(
      takeUntil(this.destroy$),
      filter(() => this.autoRefreshEnabled && this.connectionState$.value.status !== 'connected'),
      switchMap(() => this.loadDashboardData())
    )
    .subscribe();

  // Manual refresh trigger
  this.refreshInterval$
    .pipe(
      takeUntil(this.destroy$),
      debounceTime(1000),
      switchMap(() => this.loadDashboardData())
    )
    .subscribe();
}

onRefreshClick(): void {
  this.refreshInterval$.next();
}

toggleAutoRefresh(): void {
  this.autoRefreshEnabled = !this.autoRefreshEnabled;
  this.notificationService.showInfo(
    this.autoRefreshEnabled ? 'Auto-refresh enabled' : 'Auto-refresh disabled'
  );
}
```

### 2.3 Data Freshness Monitoring
**Duration**: 2 hours

```typescript
private monitorDataFreshness(): void {
  interval(5000) // Check every 5 seconds
    .pipe(takeUntil(this.destroy$))
    .subscribe(() => {
      const currentData = this.kpiData$.value;
      if (!currentData) return;

      const dataAge = Math.floor((Date.now() - new Date(currentData.timestamp).getTime()) / 1000);
      let status: DataFreshnessStatus = 'fresh';

      if (dataAge > 300) { // 5 minutes
        status = 'error';
      } else if (dataAge > 90) { // 90 seconds
        status = 'stale';
      }

      this.dataFreshness$.next({
        age: dataAge,
        status,
        lastUpdated: new Date(currentData.timestamp)
      });

      this.cdr.markForCheck();
    });
}
```

## Phase 3: UI Enhancement (Day 3)

### 3.1 Enhanced Template
**File**: `src/app/components/dashboard/analytics-dashboard/analytics-dashboard.component.html`
**Duration**: 4 hours

Update the existing template with real-time status indicators and improved accessibility:

```html
<!-- Real-time Status Bar Enhancement -->
<div class="status-bar">
  <div class="data-status"
       [ngClass]="(dataFreshness$ | async)?.status"
       [attr.aria-label]="getDataAgeAriaLabel()">
    <i class="feather" [data-feather]="getDataAgeIcon()"></i>
    <span>{{ getDataAgeText() }}</span>
    <span class="sr-only">{{ getDataAgeAriaLabel() }}</span>
  </div>

  <div class="connection-status"
       [ngClass]="(connectionState$ | async)?.status"
       [@connectionPulse]="(connectionState$ | async)?.status">
    <i class="feather" [data-feather]="getConnectionIcon()"></i>
    <span>{{ getConnectionStatusText() }}</span>
  </div>

  <div class="health-status"
       [ngClass]="(healthStatus$ | async)?.status">
    <i class="feather" [data-feather]="getHealthIcon()"></i>
    <span>{{ getHealthStatusText() }}</span>
    <span class="health-score" *ngIf="(healthStatus$ | async)?.overallScore">
      ({{ (healthStatus$ | async)?.overallScore }}%)
    </span>
  </div>
</div>

<!-- Enhanced Grid with Loading States -->
<div class="analytics-dashboard-grid"
     [class.loading]="isLoading$ | async"
     [attr.aria-busy]="isLoading$ | async">

  <!-- KPI Cards with Real-time Updates -->
  <div class="grid-area kpi-area">
    <sgn-kpi-cards
      [kpiData]="kpiData$ | async"
      [timeRange]="selectedTimeRange"
      [connectionState]="connectionState$ | async"
      [dataFreshness]="dataFreshness$ | async"
      [@valueChange]="(kpiData$ | async)?.dau"
      (kpiValueChange)="onKpiValueChange($event)">
    </sgn-kpi-cards>
  </div>

  <!-- Usage Charts -->
  <div class="grid-area usage-charts-area">
    <sgn-usage-charts
      [usageData]="usageAnalytics$ | async"
      [timeRange]="selectedTimeRange"
      [isRealTimeEnabled]="realtimeEnabled">
    </sgn-usage-charts>
  </div>
</div>
```

### 3.2 Enhanced Styling
**File**: `src/app/components/dashboard/analytics-dashboard/analytics-dashboard.component.scss`
**Duration**: 2 hours

```scss
.analytics-dashboard {
  // Real-time status indicators
  .status-bar {
    display: flex;
    gap: 16px;
    align-items: center;
    padding: 12px 16px;
    background: var(--surface-color);
    border-radius: 8px;
    margin-bottom: 24px;

    .data-status, .connection-status, .health-status {
      display: flex;
      align-items: center;
      gap: 8px;
      padding: 8px 12px;
      border-radius: 6px;
      font-weight: 500;
      font-size: 0.875rem;
      transition: all 0.3s ease;

      i.feather {
        width: 16px;
        height: 16px;
      }

      &.fresh, &.connected, &.healthy {
        color: var(--success-color);
        background: rgba(var(--success-rgb), 0.1);
      }

      &.stale, &.reconnecting, &.warning {
        color: var(--warning-color);
        background: rgba(var(--warning-rgb), 0.1);
      }

      &.error, &.disconnected, &.critical {
        color: var(--danger-color);
        background: rgba(var(--danger-rgb), 0.1);
      }
    }

    .health-score {
      font-size: 0.75rem;
      opacity: 0.8;
    }
  }

  // Loading states
  .analytics-dashboard-grid {
    &.loading {
      opacity: 0.7;
      pointer-events: none;

      .grid-area {
        position: relative;

        &::before {
          content: '';
          position: absolute;
          top: 0;
          left: 0;
          right: 0;
          bottom: 0;
          background: linear-gradient(90deg, transparent, rgba(255,255,255,0.1), transparent);
          animation: shimmer 1.5s infinite;
          z-index: 1;
        }
      }
    }
  }

  // RTL support
  [dir="rtl"] & {
    .status-bar {
      flex-direction: row-reverse;
    }

    .header-controls {
      .action-buttons {
        flex-direction: row-reverse;
      }
    }
  }
}

@keyframes shimmer {
  0% { transform: translateX(-100%); }
  100% { transform: translateX(100%); }
}
```

## Phase 4: Export & Advanced Features (Day 4)

### 4.1 Export Functionality
**Duration**: 3 hours

```typescript
async exportDashboard(format: ExportFormat): Promise<void> {
  try {
    this.isLoading$.next(true);

    const filters: AnalyticsFilters = {
      timeRange: this.selectedTimeRange as TimeRange,
      organizationId: this.selectedOrganization === 'all' ? undefined : this.selectedOrganization
    };

    const exportData = await this.analyticsService.exportDashboard(format, filters).toPromise();

    // Create download link
    const blob = new Blob([exportData], {
      type: this.getExportMimeType(format)
    });

    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = `analytics-dashboard-${this.selectedTimeRange}-${Date.now()}.${format}`;
    link.click();

    window.URL.revokeObjectURL(url);

    this.notificationService.showSuccess(
      this.translateService.instant('ANALYTICS.EXPORT.SUCCESS', { format: format.toUpperCase() })
    );

  } catch (error) {
    this.notificationService.showError(
      this.translateService.instant('ANALYTICS.EXPORT.ERROR')
    );
  } finally {
    this.isLoading$.next(false);
  }
}

private getExportMimeType(format: ExportFormat): string {
  const mimeTypes = {
    csv: 'text/csv',
    excel: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
    pdf: 'application/pdf'
  };
  return mimeTypes[format];
}
```

### 4.2 Error Handling & Recovery
**Duration**: 2 hours

```typescript
private handleInitializationError(error: any): void {
  this.isLoading$.next(false);

  this.notificationService.showError(
    this.translateService.instant('ANALYTICS.ERRORS.INITIALIZATION_FAILED'),
    {
      action: this.translateService.instant('ANALYTICS.ACTIONS.RETRY'),
      actionCallback: () => this.ngOnInit()
    }
  );

  this.logger.error('Analytics dashboard initialization failed', error);
}

private handleApiError(error: HttpErrorResponse): Observable<never> {
  let userMessage = this.translateService.instant('ANALYTICS.ERRORS.GENERIC');

  if (error.status === 401) {
    userMessage = this.translateService.instant('ANALYTICS.ERRORS.UNAUTHORIZED');
  } else if (error.status === 403) {
    userMessage = this.translateService.instant('ANALYTICS.ERRORS.FORBIDDEN');
  } else if (error.status === 0) {
    userMessage = this.translateService.instant('ANALYTICS.ERRORS.NETWORK');
  }

  this.notificationService.showError(userMessage);
  this.logger.error('Analytics API error', { status: error.status, url: error.url });

  return EMPTY;
}

async forceReconnect(): Promise<void> {
  try {
    await this.analyticsService.reconnectSignalR();
    this.notificationService.showSuccess(
      this.translateService.instant('ANALYTICS.CONNECTION.RECONNECTED')
    );
  } catch (error) {
    this.notificationService.showError(
      this.translateService.instant('ANALYTICS.CONNECTION.RECONNECT_FAILED')
    );
  }
}
```

### 4.3 Accessibility Enhancement
**Duration**: 1 hour

```typescript
// Keyboard navigation support
@HostListener('keydown', ['$event'])
handleKeyboardNavigation(event: KeyboardEvent): void {
  if (event.altKey || event.metaKey) return;

  switch (event.key) {
    case 'r':
      if (event.ctrlKey) {
        event.preventDefault();
        this.onRefreshClick();
      }
      break;
    case 'e':
      if (event.ctrlKey) {
        event.preventDefault();
        this.showExportMenu();
      }
      break;
    case 'p':
      if (event.ctrlKey) {
        event.preventDefault();
        this.toggleAutoRefresh();
      }
      break;
    case 't':
      if (event.ctrlKey) {
        event.preventDefault();
        this.toggleRealtimeUpdates();
      }
      break;
  }
}

// Screen reader announcements
private announceKpiChange(metric: string, oldValue: number, newValue: number): void {
  const change = newValue - oldValue;
  const direction = change > 0 ? 'increased' : 'decreased';
  const announcement = this.translateService.instant('ANALYTICS.ANNOUNCEMENTS.KPI_CHANGE', {
    metric,
    direction,
    change: Math.abs(change)
  });

  this.liveAnnouncer.announce(announcement);
}
```

## Testing Strategy

### Unit Tests
**File**: `src/app/components/dashboard/analytics-dashboard/analytics-dashboard.component.spec.ts`

```typescript
describe('AnalyticsDashboardComponent', () => {
  let component: AnalyticsDashboardComponent;
  let mockAnalyticsService: jasmine.SpyObj<AnalyticsApiService>;

  beforeEach(() => {
    const spy = jasmine.createSpyObj('AnalyticsApiService', [
      'getLatestKPIs', 'initializeSignalRConnection', 'getRealtimeUpdates'
    ]);

    TestBed.configureTestingModule({
      declarations: [AnalyticsDashboardComponent],
      providers: [
        { provide: AnalyticsApiService, useValue: spy }
      ]
    });

    mockAnalyticsService = TestBed.inject(AnalyticsApiService) as jasmine.SpyObj<AnalyticsApiService>;
  });

  it('should load initial KPI data on init', async () => {
    const mockKpiData: KpiSnapshot = {
      timestamp: new Date().toISOString(),
      dau: 150,
      mau: 1200,
      successRate: 0.85,
      avgTimeToSign: 3600,
      totalDocuments: 500,
      activeOrganizations: 25,
      trends: {},
      metadata: { dataAge: 30, queryDuration: 250, cacheHit: false }
    };

    mockAnalyticsService.getLatestKPIs.and.returnValue(of(mockKpiData));

    await component.ngOnInit();

    expect(component.kpiData$.value).toEqual(mockKpiData);
    expect(component.isLoading$.value).toBeFalse();
  });

  it('should handle real-time updates correctly', () => {
    const update: RealtimeUpdate = {
      type: 'kpi_update',
      data: { dau: 175 },
      timestamp: new Date().toISOString()
    };

    mockAnalyticsService.getRealtimeUpdates.and.returnValue(of(update));

    component.setupRealtimeFeatures();

    expect(component.kpiData$.value?.dau).toBe(175);
  });
});
```

### E2E Tests
**File**: `C:\Users\gals\seleniumpythontests-1\playwright_tests\tests\analytics\test_analytics_dashboard.py`

```python
def test_analytics_dashboard_loads_with_real_time_data(authenticated_page):
    """Test that analytics dashboard loads and displays real-time data"""
    page = authenticated_page

    # Navigate to analytics dashboard
    page.goto("/dashboard/analytics")

    # Verify page loads within 2 seconds
    start_time = time.time()
    expect(page.locator('[data-testid="analytics-dashboard"]')).to_be_visible()
    load_time = time.time() - start_time
    assert load_time < 2.0, f"Dashboard loaded in {load_time}s, expected <2s"

    # Verify KPI cards are present
    expect(page.locator('[data-testid="kpi-cards"]')).to_be_visible()
    expect(page.locator('[data-testid="dau-metric"]')).to_contain_text(/\d+/)

    # Verify real-time status indicator
    expect(page.locator('[data-testid="connection-status"]')).to_contain_text("connected")

    # Verify auto-refresh functionality
    initial_timestamp = page.locator('[data-testid="last-updated"]').text_content()
    page.wait_for_timeout(31000)  # Wait for 30s refresh + buffer
    updated_timestamp = page.locator('[data-testid="last-updated"]').text_content()
    assert initial_timestamp != updated_timestamp, "Data should refresh automatically"

def test_export_functionality_works(authenticated_page):
    """Test that export functionality generates files correctly"""
    page = authenticated_page
    page.goto("/dashboard/analytics")

    with page.expect_download() as download_info:
        page.click('[data-testid="export-dropdown"]')
        page.click('[data-testid="export-csv"]')

    download = download_info.value
    assert download.suggested_filename.endswith('.csv')
    assert download.suggested_filename.startswith('analytics-dashboard-')
```

## Deployment Checklist

### Production Readiness
- [ ] Environment variables configured
- [ ] SignalR hub scaling configured
- [ ] API rate limiting implemented
- [ ] Caching strategy deployed
- [ ] Error monitoring set up
- [ ] Performance metrics collection
- [ ] Security headers configured
- [ ] CORS policies set
- [ ] Health checks functional
- [ ] Database connections pooled

### Performance Validation
- [ ] Initial load time < 2 seconds
- [ ] API response time < 500ms
- [ ] Real-time update latency < 1 second
- [ ] Memory usage stable during extended use
- [ ] 1000+ concurrent connections supported

This implementation plan provides a comprehensive roadmap for building a production-ready analytics dashboard with real-time capabilities, proper error handling, and excellent user experience.