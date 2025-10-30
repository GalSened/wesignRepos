# Component Design - Analytics Dashboard Main Page

**PAGE_KEY**: analytics-dashboard-main
**DATE**: 2025-01-29

## Component Architecture

### Primary Component: AnalyticsDashboardComponent

```typescript
// src/app/components/dashboard/analytics-dashboard/analytics-dashboard.component.ts
export class AnalyticsDashboardComponent implements OnInit, OnDestroy {
  // State Management
  kpiData: KpiSnapshot | null = null;
  usageAnalytics: UsageAnalytics | null = null;
  segmentationData: SegmentationData | null = null;
  processFlowData: ProcessFlowData | null = null;

  // Real-time Features
  connectionState: ConnectionState = 'disconnected';
  dataFreshness: DataFreshness = { age: 0, status: 'fresh' };
  healthStatus: HealthStatus | null = null;

  // User Controls
  selectedTimeRange: TimeRange = '24h';
  selectedOrganization: string = 'all';
  autoRefreshEnabled: boolean = true;
  realtimeEnabled: boolean = true;
  isLoading: boolean = false;
}
```

### Child Components

#### 1. KpiCardsComponent
**Location**: `src/app/components/dashboard/analytics-dashboard/kpi-cards/`

```typescript
interface KpiCardsComponent {
  @Input() kpiData: KpiSnapshot | null;
  @Input() timeRange: TimeRange;
  @Input() connectionState: ConnectionState;
  @Input() dataFreshness: DataFreshness;

  // Animation features for real-time updates
  animateValueChange(metric: string, newValue: number): void;
  getTrendDirection(metric: string): 'up' | 'down' | 'stable';
  formatKpiValue(value: number, metric: string): string;
}
```

#### 2. UsageChartsComponent
**Location**: `src/app/components/dashboard/analytics-dashboard/usage-charts/`

```typescript
interface UsageChartsComponent {
  @Input() usageData: UsageAnalytics | null;
  @Input() timeRange: TimeRange;

  // Chart management
  initializeCharts(): void;
  updateChartsData(newData: UsageAnalytics): void;
  handleChartInteraction(event: ChartEvent): void;
}
```

#### 3. SegmentationChartsComponent
**Location**: `src/app/components/dashboard/analytics-dashboard/segmentation-charts/`

```typescript
interface SegmentationChartsComponent {
  @Input() segmentationData: SegmentationData | null;
  @Input() timeRange: TimeRange;

  // Segmentation visualization
  renderOrganizationBreakdown(): void;
  renderDocumentTypeDistribution(): void;
  renderUserRoleAnalysis(): void;
}
```

#### 4. ProcessFlowComponent
**Location**: `src/app/components/dashboard/analytics-dashboard/process-flow/`

```typescript
interface ProcessFlowComponent {
  @Input() processFlowData: ProcessFlowData | null;
  @Input() timeRange: TimeRange;

  // Process flow visualization
  renderFlowDiagram(): void;
  highlightBottlenecks(): void;
  showConversionRates(): void;
}
```

#### 5. AnalyticsInsightsComponent
**Location**: `src/app/components/dashboard/analytics-dashboard/insights/`

```typescript
interface AnalyticsInsightsComponent {
  @Input() kpiData: KpiSnapshot | null;
  @Input() usageData: UsageAnalytics | null;
  @Input() segmentationData: SegmentationData | null;
  @Input() processFlowData: ProcessFlowData | null;

  // AI-driven insights
  generateInsights(): AnalyticsInsight[];
  identifyTrends(): TrendInsight[];
  detectAnomalies(): AnomalyInsight[];
}
```

## Data Models

### Core Analytics Models

```typescript
// src/app/models/analytics/analytics-models.ts

export interface KpiSnapshot {
  timestamp: string;
  dau: number;
  mau: number;
  successRate: number;
  avgTimeToSign: number;
  totalDocuments: number;
  activeOrganizations: number;
  trends: {
    [key: string]: TrendData;
  };
}

export interface TrendData {
  value: number;
  change: number;
  changePercent: number;
  direction: 'up' | 'down' | 'stable';
  sparklineData: number[];
}

export interface UsageAnalytics {
  timeSeriesData: TimeSeriesPoint[];
  peakUsageHours: number[];
  deviceBreakdown: DeviceUsage[];
  geographicDistribution: GeographicData[];
}

export interface SegmentationData {
  organizationBreakdown: OrganizationSegment[];
  documentTypeDistribution: DocumentTypeSegment[];
  userRoleAnalysis: UserRoleSegment[];
  signatureMethodBreakdown: SignatureMethodSegment[];
}

export interface ProcessFlowData {
  conversionFunnel: ConversionStep[];
  averageTimeByStep: StepTiming[];
  dropoffPoints: DropoffAnalysis[];
  bottleneckAnalysis: BottleneckData[];
}
```

### Real-time Models

```typescript
export interface ConnectionState {
  status: 'connected' | 'disconnected' | 'reconnecting' | 'error';
  lastConnected?: Date;
  reconnectAttempts: number;
  latency?: number;
}

export interface DataFreshness {
  age: number; // seconds
  status: 'fresh' | 'stale' | 'error';
  lastUpdated: Date;
  nextUpdate?: Date;
}

export interface HealthStatus {
  status: 'healthy' | 'warning' | 'critical';
  services: {
    database: ServiceHealth;
    signalr: ServiceHealth;
    s3: ServiceHealth;
    analytics: ServiceHealth;
  };
  overallScore: number;
}
```

## Service Layer Design

### AnalyticsApiService Enhancement

```typescript
// src/app/services/analytics-api.service.ts
export class AnalyticsApiService {

  // Core API methods
  getLatestKPIs(timeRange: TimeRange, orgFilter?: string): Observable<KpiSnapshot>;
  getUsageAnalytics(timeRange: TimeRange): Observable<UsageAnalytics>;
  getSegmentationData(timeRange: TimeRange): Observable<SegmentationData>;
  getProcessFlowData(timeRange: TimeRange): Observable<ProcessFlowData>;

  // Real-time features
  initializeSignalRConnection(): Promise<void>;
  subscribeToRealtimeUpdates(): Observable<RealtimeUpdate>;
  getHealthStatus(): Observable<HealthStatus>;

  // Export functionality
  exportDashboard(format: ExportFormat, filters: ExportFilters): Observable<Blob>;

  // Connection management
  reconnectSignalR(): Promise<void>;
  getConnectionState(): Observable<ConnectionState>;
  monitorDataFreshness(): Observable<DataFreshness>;
}
```

## UI/UX Design Patterns

### Real-time Status Indicators

```scss
// Analytics dashboard specific styles
.status-bar {
  display: flex;
  gap: 16px;
  align-items: center;

  .data-status {
    &.fresh { color: var(--success-color); }
    &.stale { color: var(--warning-color); }
    &.error { color: var(--danger-color); }
  }

  .connection-status {
    &.connected { color: var(--success-color); }
    &.disconnected { color: var(--danger-color); }
    &.reconnecting { color: var(--warning-color); }
  }
}
```

### Animation Patterns

```typescript
// Animation utilities for real-time updates
export class AnalyticsAnimations {
  static valueChange = trigger('valueChange', [
    transition(':increment', [
      style({ transform: 'scale(1.1)', color: 'var(--success-color)' }),
      animate('300ms ease-out', style({ transform: 'scale(1)', color: '*' }))
    ]),
    transition(':decrement', [
      style({ transform: 'scale(1.1)', color: 'var(--danger-color)' }),
      animate('300ms ease-out', style({ transform: 'scale(1)', color: '*' }))
    ])
  ]);

  static connectionPulse = trigger('connectionPulse', [
    state('connected', style({ opacity: 1 })),
    state('disconnected', style({ opacity: 0.5 })),
    transition('* => connected', [
      animate('500ms ease-in', keyframes([
        style({ opacity: 0.5, offset: 0 }),
        style({ opacity: 1, transform: 'scale(1.05)', offset: 0.5 }),
        style({ opacity: 1, transform: 'scale(1)', offset: 1 })
      ]))
    ])
  ]);
}
```

### Grid Layout System

```scss
// Responsive grid layout
.analytics-dashboard-grid {
  display: grid;
  grid-template-areas:
    "kpi-area kpi-area"
    "usage-charts segmentation"
    "process-flow process-flow";
  grid-template-columns: 1fr 1fr;
  grid-template-rows: auto auto auto;
  gap: 24px;
  padding: 24px;

  @media (max-width: 768px) {
    grid-template-areas:
      "kpi-area"
      "usage-charts"
      "segmentation"
      "process-flow";
    grid-template-columns: 1fr;
  }
}
```

## Accessibility Features

### Screen Reader Support

```typescript
// Accessibility utilities
export class AnalyticsAccessibility {

  static announceKpiUpdate(metric: string, value: number, change: number): void {
    const announcement = `${metric} updated to ${value}, ${change > 0 ? 'increased' : 'decreased'} by ${Math.abs(change)}`;
    this.liveAnnouncer.announce(announcement);
  }

  static describeTrendDirection(direction: 'up' | 'down' | 'stable'): string {
    const descriptions = {
      up: 'trending upward',
      down: 'trending downward',
      stable: 'remaining stable'
    };
    return descriptions[direction];
  }
}
```

### Keyboard Navigation

```typescript
// Keyboard navigation support
@HostListener('keydown', ['$event'])
handleKeyboardNavigation(event: KeyboardEvent): void {
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
    case 'Tab':
      this.manageFocusFlow(event);
      break;
  }
}
```

## Internationalization Support

### RTL Layout Handling

```scss
// RTL support for Hebrew language
[dir="rtl"] {
  .analytics-dashboard-grid {
    direction: rtl;
  }

  .status-bar {
    flex-direction: row-reverse;
  }

  .header-controls {
    .control-group {
      text-align: right;
    }
  }
}
```

### Translation Keys

```typescript
// Translation key structure
export const ANALYTICS_TRANSLATIONS = {
  'ANALYTICS.DASHBOARD.TITLE': {
    en: 'Analytics Dashboard',
    he: 'לוח בקרה אנליטי'
  },
  'ANALYTICS.KPI.DAU': {
    en: 'Daily Active Users',
    he: 'משתמשים פעילים יומיים'
  },
  'ANALYTICS.STATUS.CONNECTED': {
    en: 'Real-time connected',
    he: 'מחובר בזמן אמת'
  }
};
```

## Error Handling Patterns

```typescript
// Comprehensive error handling
export class AnalyticsErrorHandler {

  handleApiError(error: HttpErrorResponse): Observable<never> {
    const userMessage = this.getUserFriendlyMessage(error);
    this.notificationService.showError(userMessage);

    // Log technical details
    this.logger.error('Analytics API Error', {
      status: error.status,
      url: error.url,
      message: error.message
    });

    return EMPTY;
  }

  handleSignalRError(error: Error): void {
    this.connectionState.next({
      status: 'error',
      lastError: error.message,
      reconnectAttempts: this.reconnectAttempts
    });

    this.scheduleReconnection();
  }
}
```

## Testing Strategy

### Component Testing Approach

```typescript
// Example test structure
describe('AnalyticsDashboardComponent', () => {

  it('should load KPI data on initialization', () => {
    // Test data loading
  });

  it('should establish SignalR connection for real-time updates', () => {
    // Test real-time connectivity
  });

  it('should handle connection failures gracefully', () => {
    // Test error scenarios
  });

  it('should animate KPI value changes', () => {
    // Test animation triggers
  });

  it('should export data in requested format', () => {
    // Test export functionality
  });
});
```

## Performance Optimization

### Lazy Loading Strategy

```typescript
// Lazy loading for chart components
const routes: Routes = [
  {
    path: 'analytics',
    loadComponent: () => import('./analytics-dashboard.component')
      .then(m => m.AnalyticsDashboardComponent)
  }
];
```

### Change Detection Optimization

```typescript
// OnPush change detection for performance
@Component({
  selector: 'sgn-analytics-dashboard',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AnalyticsDashboardComponent {
  // Immutable state updates only
  updateKpiData(newData: KpiSnapshot): void {
    this.kpiData = { ...newData };
    this.cdr.markForCheck();
  }
}
```

This component design provides a comprehensive foundation for the analytics dashboard with real-time capabilities, proper error handling, accessibility support, and performance optimizations.