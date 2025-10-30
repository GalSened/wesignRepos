# KPI Cards Component Design - A→M Workflow Step D

**PAGE_KEY**: kpi-cards
**COMPLETION DATE**: 2025-01-29
**STATUS**: ✅ COMPLETE

## Component Architecture Overview

The KPI Cards page implements a sophisticated real-time dashboard with interactive card-based KPI visualization. The design emphasizes performance, accessibility, and real-time capabilities while maintaining enterprise-grade code quality and scalability.

---

## 1. Core Component Structure

### KpiCardsPageComponent (Main Container)

```typescript
@Component({
  selector: 'app-kpi-cards',
  templateUrl: './kpi-cards-page.component.html',
  styleUrls: ['./kpi-cards-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
  animations: [
    trigger('cardEnter', [
      transition(':enter', [
        style({ opacity: 0, transform: 'translateY(20px)' }),
        animate('300ms ease-out', style({ opacity: 1, transform: 'translateY(0)' }))
      ])
    ]),
    trigger('gridReflow', [
      transition('* => *', [
        query(':enter', [
          style({ opacity: 0, transform: 'scale(0.8)' }),
          stagger(50, animate('200ms ease-out', style({ opacity: 1, transform: 'scale(1)' })))
        ], { optional: true })
      ])
    ])
  ]
})
export class KpiCardsPageComponent implements OnInit, OnDestroy {
  // Real-time state management
  private readonly kpisSubject = new BehaviorSubject<EnhancedKpiCard[]>([]);
  public readonly kpis$ = this.kpisSubject.asObservable();

  private readonly filtersSubject = new BehaviorSubject<KpiFilters>(new KpiFilters());
  public readonly filters$ = this.filtersSubject.asObservable();

  private readonly connectionStateSubject = new BehaviorSubject<ConnectionState>({
    status: 'disconnected',
    reconnectAttempts: 0
  });
  public readonly connectionState$ = this.connectionStateSubject.asObservable();

  // Component state
  public isLoading = true;
  public error: string | null = null;
  public lastUpdated: Date | null = null;
  public selectedCards: Set<string> = new Set();

  // Lifecycle and cleanup
  private destroy$ = new Subject<void>();
  private refreshTimer?: Timer;

  constructor(
    private analyticsService: AnalyticsApiService,
    private cdr: ChangeDetectorRef,
    private router: Router,
    private authService: AuthService,
    private notificationService: NotificationService
  ) {}

  ngOnInit(): void {
    this.initializeComponent();
    this.setupRealTimeConnection();
    this.setupErrorHandling();
    this.setupAccessibilityFeatures();
  }

  // Real-time connection management
  private async setupRealTimeConnection(): Promise<void> {
    try {
      await this.analyticsService.initializeSignalRConnection();
      this.subscribeToKpiUpdates();
      this.monitorConnectionHealth();
    } catch (error) {
      this.handleConnectionError(error);
      this.fallbackToPolling();
    }
  }

  // Responsive grid configuration
  public getGridColumns(): number {
    const width = window.innerWidth;
    if (width < 768) return 1;      // Mobile: single column
    if (width < 1200) return 2;     // Tablet: two columns
    if (width < 1600) return 3;     // Desktop: three columns
    return 4;                       // Large: four columns
  }
}
```

### Component Interface Definitions

```typescript
// Enhanced KPI Card with real-time capabilities
export interface EnhancedKpiCard extends KpiCard {
  // Real-time metadata
  lastUpdated: Date;
  dataFreshness: DataFreshness;
  animationState: 'idle' | 'updating' | 'error';

  // Interactive features
  isDrillDownAvailable: boolean;
  hasAlerts: boolean;
  isCustomizable: boolean;

  // Accessibility properties
  ariaLabel: string;
  ariaDescription: string;
  keyboardShortcut?: string;

  // Visual properties
  size: 'small' | 'medium' | 'large';
  priority: 'high' | 'medium' | 'low';
  colorScheme: 'default' | 'success' | 'warning' | 'error';
}

// Component configuration
export interface KpiCardConfig {
  refreshInterval: number;
  animationDuration: number;
  enableRealTime: boolean;
  enableDrillDown: boolean;
  enableExport: boolean;
  accessibilityMode: 'standard' | 'enhanced';
}

// Filter configuration
export interface KpiFilters {
  timeRange: TimeRange;
  organizationId?: string;
  metricTypes: string[];
  showTrends: boolean;
  groupBy: 'category' | 'priority' | 'role';
}
```

---

## 2. Individual KPI Card Component

### EnhancedKpiCardComponent

```typescript
@Component({
  selector: 'app-enhanced-kpi-card',
  templateUrl: './enhanced-kpi-card.component.html',
  styleUrls: ['./enhanced-kpi-card.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
  animations: [
    trigger('valueChange', [
      transition('* => *', [
        sequence([
          style({ transform: 'scale(1)' }),
          animate('150ms ease-in', style({ transform: 'scale(1.05)' })),
          animate('150ms ease-out', style({ transform: 'scale(1)' }))
        ])
      ])
    ]),
    trigger('trendIndicator', [
      state('up', style({ color: '#10B981', transform: 'rotate(0deg)' })),
      state('down', style({ color: '#EF4444', transform: 'rotate(180deg)' })),
      state('stable', style({ color: '#6B7280', transform: 'rotate(90deg)' })),
      transition('* => *', animate('300ms ease-out'))
    ])
  ],
  host: {
    '[class.kpi-card]': 'true',
    '[class.kpi-card--loading]': 'isLoading',
    '[class.kpi-card--error]': 'hasError',
    '[class.kpi-card--stale]': 'isDataStale',
    '[class.kpi-card--focused]': 'isFocused',
    '[attr.tabindex]': '0',
    '[attr.role]': '"button"',
    '[attr.aria-label]': 'accessibilityLabel',
    '[attr.aria-describedby]': 'descriptionId',
    '(click)': 'onCardClick()',
    '(keydown.enter)': 'onCardClick()',
    '(keydown.space)': 'onCardClick()',
    '(focus)': 'onFocus()',
    '(blur)': 'onBlur()'
  }
})
export class EnhancedKpiCardComponent implements OnInit, OnDestroy, OnChanges {
  @Input() kpiData!: EnhancedKpiCard;
  @Input() config: KpiCardConfig = DEFAULT_KPI_CONFIG;
  @Input() size: 'small' | 'medium' | 'large' = 'medium';

  @Output() drillDown = new EventEmitter<string>();
  @Output() valueChange = new EventEmitter<KpiValueChange>();
  @Output() error = new EventEmitter<string>();

  // Component state
  public isLoading = false;
  public hasError = false;
  public isFocused = false;
  public previousValue: number | null = null;
  public animatedValue: number = 0;

  // Animation control
  private animationFrameId?: number;
  private countUpAnimation?: Animation;

  // Accessibility
  public readonly descriptionId = `kpi-desc-${this.generateId()}`;
  public accessibilityLabel = '';

  constructor(
    private cdr: ChangeDetectorRef,
    private animationBuilder: AnimationBuilder,
    private zone: NgZone
  ) {}

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['kpiData'] && this.kpiData) {
      this.handleValueChange();
      this.updateAccessibilityLabels();
    }
  }

  // Real-time value updates with animation
  private handleValueChange(): void {
    const newValue = this.kpiData.value;
    const oldValue = this.previousValue;

    if (oldValue !== null && oldValue !== newValue) {
      this.animateValueChange(oldValue, newValue);
      this.valueChange.emit({
        metric: this.kpiData.name,
        oldValue,
        newValue,
        timestamp: new Date()
      });
    }

    this.previousValue = newValue;
    this.updateTrendIndicator();
  }

  // Smooth value animation
  private animateValueChange(from: number, to: number): void {
    const duration = this.config.animationDuration;
    const startTime = performance.now();

    const animate = (currentTime: number) => {
      const elapsed = currentTime - startTime;
      const progress = Math.min(elapsed / duration, 1);

      // Easing function for smooth animation
      const easeOutCubic = 1 - Math.pow(1 - progress, 3);
      this.animatedValue = from + (to - from) * easeOutCubic;

      if (progress < 1) {
        this.animationFrameId = requestAnimationFrame(animate);
      } else {
        this.animatedValue = to;
      }

      this.cdr.markForCheck();
    };

    this.animationFrameId = requestAnimationFrame(animate);
  }

  // Drill-down functionality
  public onCardClick(): void {
    if (this.kpiData.isDrillDownAvailable) {
      this.drillDown.emit(this.kpiData.id);
    }
  }

  // Accessibility support
  private updateAccessibilityLabels(): void {
    const trend = this.kpiData.trend?.direction || 'stable';
    const trendText = this.getTrendDescription(trend);

    this.accessibilityLabel = `${this.kpiData.name}: ${this.formatValue(this.kpiData.value)}. ${trendText}. Last updated ${this.formatLastUpdated()}`;
  }

  public get isDataStale(): boolean {
    if (!this.kpiData.dataFreshness) return false;
    return this.kpiData.dataFreshness.status === 'stale';
  }
}
```

---

## 3. Sparkline Visualization Component

### KpiSparklineComponent

```typescript
@Component({
  selector: 'app-kpi-sparkline',
  template: `
    <canvas
      #sparklineCanvas
      [width]="width"
      [height]="height"
      [attr.aria-label]="ariaLabel"
      (mousemove)="onMouseMove($event)"
      (mouseleave)="onMouseLeave()"
      class="sparkline-canvas">
    </canvas>

    <div
      *ngIf="tooltip.visible"
      class="sparkline-tooltip"
      [style.left.px]="tooltip.x"
      [style.top.px]="tooltip.y">
      <div class="tooltip-value">{{ tooltip.value | number:'1.0-2' }}</div>
      <div class="tooltip-time">{{ tooltip.time | date:'short' }}</div>
    </div>
  `,
  styleUrls: ['./kpi-sparkline.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class KpiSparklineComponent implements OnInit, OnDestroy, AfterViewInit {
  @ViewChild('sparklineCanvas', { static: true })
  canvasRef!: ElementRef<HTMLCanvasElement>;

  @Input() data: TimeSeriesPoint[] = [];
  @Input() width = 100;
  @Input() height = 30;
  @Input() color = '#3B82F6';
  @Input() fillColor = 'rgba(59, 130, 246, 0.1)';
  @Input() strokeWidth = 2;

  public tooltip = {
    visible: false,
    x: 0,
    y: 0,
    value: 0,
    time: new Date()
  };

  public ariaLabel = '';

  private ctx!: CanvasRenderingContext2D;
  private resizeObserver?: ResizeObserver;

  ngAfterViewInit(): void {
    this.ctx = this.canvasRef.nativeElement.getContext('2d')!;
    this.setupCanvas();
    this.renderSparkline();
    this.updateAriaLabel();
  }

  private renderSparkline(): void {
    if (!this.ctx || this.data.length === 0) return;

    const { width, height } = this.canvasRef.nativeElement;

    // Clear canvas
    this.ctx.clearRect(0, 0, width, height);

    // Calculate scales
    const values = this.data.map(d => d.value);
    const minValue = Math.min(...values);
    const maxValue = Math.max(...values);
    const valueRange = maxValue - minValue || 1;

    const xScale = width / (this.data.length - 1);
    const yScale = height / valueRange;

    // Create path
    this.ctx.beginPath();
    this.ctx.strokeStyle = this.color;
    this.ctx.lineWidth = this.strokeWidth;
    this.ctx.lineJoin = 'round';
    this.ctx.lineCap = 'round';

    this.data.forEach((point, index) => {
      const x = index * xScale;
      const y = height - ((point.value - minValue) * yScale);

      if (index === 0) {
        this.ctx.moveTo(x, y);
      } else {
        this.ctx.lineTo(x, y);
      }
    });

    this.ctx.stroke();

    // Fill area under curve
    if (this.fillColor) {
      this.ctx.fillStyle = this.fillColor;
      this.ctx.lineTo(width, height);
      this.ctx.lineTo(0, height);
      this.ctx.closePath();
      this.ctx.fill();
    }
  }

  public onMouseMove(event: MouseEvent): void {
    const rect = this.canvasRef.nativeElement.getBoundingClientRect();
    const x = event.clientX - rect.left;
    const dataIndex = Math.round((x / rect.width) * (this.data.length - 1));

    if (dataIndex >= 0 && dataIndex < this.data.length) {
      const dataPoint = this.data[dataIndex];
      this.tooltip = {
        visible: true,
        x: event.clientX,
        y: event.clientY - 40,
        value: dataPoint.value,
        time: dataPoint.timestamp
      };
      this.cdr.markForCheck();
    }
  }

  private updateAriaLabel(): void {
    if (this.data.length === 0) {
      this.ariaLabel = 'No trend data available';
      return;
    }

    const firstValue = this.data[0].value;
    const lastValue = this.data[this.data.length - 1].value;
    const change = ((lastValue - firstValue) / firstValue * 100).toFixed(1);
    const direction = lastValue > firstValue ? 'increased' : 'decreased';

    this.ariaLabel = `Trend sparkline showing data has ${direction} by ${Math.abs(Number(change))}% over the period`;
  }
}
```

---

## 4. Drill-Down Modal Component

### KpiDrillDownModalComponent

```typescript
@Component({
  selector: 'app-kpi-drill-down-modal',
  templateUrl: './kpi-drill-down-modal.component.html',
  styleUrls: ['./kpi-drill-down-modal.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
  animations: [
    trigger('modalSlide', [
      transition(':enter', [
        style({ transform: 'translateX(100%)', opacity: 0 }),
        animate('300ms ease-out', style({ transform: 'translateX(0)', opacity: 1 }))
      ]),
      transition(':leave', [
        animate('200ms ease-in', style({ transform: 'translateX(100%)', opacity: 0 }))
      ])
    ])
  ]
})
export class KpiDrillDownModalComponent implements OnInit, OnDestroy {
  @Input() kpiId!: string;
  @Input() isVisible = false;

  @Output() close = new EventEmitter<void>();
  @Output() export = new EventEmitter<ExportRequest>();

  public drillDownData: DrillDownData | null = null;
  public isLoading = true;
  public error: string | null = null;
  public activeTab = 'breakdown';

  public readonly tabs = [
    { id: 'breakdown', label: 'Breakdown', icon: 'pie-chart' },
    { id: 'trends', label: 'Trends', icon: 'trending-up' },
    { id: 'insights', label: 'Insights', icon: 'lightbulb' }
  ];

  constructor(
    private analyticsService: AnalyticsApiService,
    private cdr: ChangeDetectorRef
  ) {}

  @HostListener('document:keydown.escape')
  onEscapeKey(): void {
    this.onClose();
  }

  async ngOnInit(): Promise<void> {
    if (this.kpiId) {
      await this.loadDrillDownData();
    }
  }

  private async loadDrillDownData(): Promise<void> {
    try {
      this.isLoading = true;
      this.error = null;

      this.drillDownData = await this.analyticsService.getKpiDrillDown(this.kpiId);

    } catch (error) {
      this.error = 'Failed to load detailed data. Please try again.';
      console.error('Drill-down data loading error:', error);
    } finally {
      this.isLoading = false;
      this.cdr.markForCheck();
    }
  }

  public onTabChange(tabId: string): void {
    this.activeTab = tabId;
    this.cdr.markForCheck();
  }

  public onClose(): void {
    this.close.emit();
  }

  public onExport(format: ExportFormat): void {
    this.export.emit({
      kpiId: this.kpiId,
      format,
      includeBreakdown: true,
      includeTrends: true,
      includeInsights: true
    });
  }
}
```

---

## 5. Service Layer Design

### Enhanced Analytics API Service

```typescript
@Injectable({
  providedIn: 'root'
})
export class AnalyticsApiService {
  private hubConnection?: HubConnection;
  private readonly kpisSubject = new BehaviorSubject<EnhancedKpiCard[]>([]);
  private readonly connectionStateSubject = new BehaviorSubject<ConnectionState>({
    status: 'disconnected',
    reconnectAttempts: 0
  });

  public readonly kpis$ = this.kpisSubject.asObservable();
  public readonly connectionState$ = this.connectionStateSubject.asObservable();

  private pollingTimer?: Timer;
  private readonly maxReconnectAttempts = 5;
  private readonly reconnectDelay = 5000;

  constructor(
    private http: HttpClient,
    private authService: AuthService,
    private cacheService: CacheService
  ) {}

  // Initialize SignalR connection for real-time updates
  public async initializeSignalRConnection(): Promise<void> {
    try {
      this.hubConnection = new HubConnectionBuilder()
        .withUrl('/analyticsHub', {
          accessTokenFactory: () => this.authService.getToken()
        })
        .withAutomaticReconnect([0, 2000, 10000, 30000])
        .configureLogging(LogLevel.Information)
        .build();

      this.setupHubEventHandlers();
      await this.hubConnection.start();

      this.connectionStateSubject.next({
        status: 'connected',
        lastConnected: new Date(),
        reconnectAttempts: 0
      });

    } catch (error) {
      console.error('SignalR connection failed:', error);
      this.connectionStateSubject.next({
        status: 'error',
        reconnectAttempts: 0
      });
      throw error;
    }
  }

  // Real-time KPI updates handler
  private setupHubEventHandlers(): void {
    if (!this.hubConnection) return;

    this.hubConnection.on('KpiUpdate', (update: KpiUpdate) => {
      this.handleKpiUpdate(update);
    });

    this.hubConnection.on('HealthStatusChange', (status: HealthStatus) => {
      this.handleHealthStatusChange(status);
    });

    this.hubConnection.onreconnecting(() => {
      this.connectionStateSubject.next({
        status: 'reconnecting',
        reconnectAttempts: this.connectionStateSubject.value.reconnectAttempts + 1
      });
    });

    this.hubConnection.onreconnected(() => {
      this.connectionStateSubject.next({
        status: 'connected',
        lastConnected: new Date(),
        reconnectAttempts: 0
      });
    });
  }

  // Get detailed KPI data with caching
  public async getDetailedKpis(filters?: KpiFilters): Promise<EnhancedKpiCard[]> {
    const cacheKey = `kpis-detailed-${JSON.stringify(filters)}`;

    // Try cache first
    const cached = await this.cacheService.get<EnhancedKpiCard[]>(cacheKey);
    if (cached && this.isCacheValid(cached.timestamp)) {
      return cached.data;
    }

    // Fetch from API
    const params = this.buildFilterParams(filters);
    const response = await this.http.get<AnalyticsApiResponse<EnhancedKpiCard[]>>('/api/analytics/kpis/detailed', { params }).toPromise();

    if (response?.data) {
      // Cache the response
      await this.cacheService.set(cacheKey, response.data, 300); // 5 minutes
      this.kpisSubject.next(response.data);
      return response.data;
    }

    throw new Error('Failed to load KPI data');
  }

  // Get drill-down data for specific KPI
  public async getKpiDrillDown(kpiId: string): Promise<DrillDownData> {
    const response = await this.http.get<DrillDownData>(`/api/analytics/kpis/${kpiId}/drill-down`).toPromise();

    if (!response) {
      throw new Error('Failed to load drill-down data');
    }

    return response;
  }

  // Export KPI data
  public async exportKpiData(request: ExportRequest): Promise<Blob> {
    const response = await this.http.post('/api/analytics/kpis/export', request, {
      responseType: 'blob'
    }).toPromise();

    if (!response) {
      throw new Error('Failed to export data');
    }

    return response;
  }

  // Fallback to HTTP polling when SignalR fails
  public startPolling(interval: number = 30000): void {
    this.stopPolling();

    this.pollingTimer = setInterval(async () => {
      try {
        await this.getDetailedKpis();
      } catch (error) {
        console.error('Polling failed:', error);
      }
    }, interval);
  }

  public stopPolling(): void {
    if (this.pollingTimer) {
      clearInterval(this.pollingTimer);
      this.pollingTimer = undefined;
    }
  }

  // Handle real-time KPI updates
  private handleKpiUpdate(update: KpiUpdate): void {
    const currentKpis = this.kpisSubject.value;
    const updatedKpis = currentKpis.map(kpi => {
      if (kpi.id === update.kpiId) {
        return {
          ...kpi,
          value: update.newValue,
          trend: update.trend,
          lastUpdated: new Date(update.timestamp),
          dataFreshness: {
            age: 0,
            status: 'fresh' as const,
            lastUpdated: new Date(update.timestamp),
            source: 'realtime' as const
          }
        };
      }
      return kpi;
    });

    this.kpisSubject.next(updatedKpis);
  }
}
```

---

## 6. State Management with NgRx

### KPI Cards State Definition

```typescript
// State interface
export interface KpiCardsState {
  kpis: EnhancedKpiCard[];
  filters: KpiFilters;
  selectedCards: string[];
  drillDownData: Record<string, DrillDownData>;
  connectionState: ConnectionState;
  lastUpdated: Date | null;
  isLoading: boolean;
  error: string | null;
}

// Initial state
export const initialKpiCardsState: KpiCardsState = {
  kpis: [],
  filters: {
    timeRange: '30d',
    metricTypes: [],
    showTrends: true,
    groupBy: 'category'
  },
  selectedCards: [],
  drillDownData: {},
  connectionState: {
    status: 'disconnected',
    reconnectAttempts: 0
  },
  lastUpdated: null,
  isLoading: false,
  error: null
};

// Actions
export const KpiCardsActions = createActionGroup({
  source: 'KPI Cards',
  events: {
    'Load Kpis': props<{ filters?: KpiFilters }>(),
    'Load Kpis Success': props<{ kpis: EnhancedKpiCard[] }>(),
    'Load Kpis Failure': props<{ error: string }>(),

    'Update Kpi Value': props<{ kpiId: string; newValue: number; trend: TrendIndicator }>(),
    'Apply Filters': props<{ filters: KpiFilters }>(),
    'Select Cards': props<{ cardIds: string[] }>(),

    'Load Drill Down': props<{ kpiId: string }>(),
    'Load Drill Down Success': props<{ kpiId: string; data: DrillDownData }>(),

    'Update Connection State': props<{ state: ConnectionState }>(),
    'Start Real Time': emptyProps(),
    'Stop Real Time': emptyProps()
  }
});

// Reducer
export const kpiCardsReducer = createReducer(
  initialKpiCardsState,
  on(KpiCardsActions.loadKpis, (state) => ({
    ...state,
    isLoading: true,
    error: null
  })),

  on(KpiCardsActions.loadKpisSuccess, (state, { kpis }) => ({
    ...state,
    kpis,
    isLoading: false,
    lastUpdated: new Date(),
    error: null
  })),

  on(KpiCardsActions.updateKpiValue, (state, { kpiId, newValue, trend }) => ({
    ...state,
    kpis: state.kpis.map(kpi =>
      kpi.id === kpiId
        ? { ...kpi, value: newValue, trend, lastUpdated: new Date() }
        : kpi
    ),
    lastUpdated: new Date()
  })),

  on(KpiCardsActions.applyFilters, (state, { filters }) => ({
    ...state,
    filters,
    isLoading: true
  }))
);

// Selectors
export const selectKpiCardsState = createFeatureSelector<KpiCardsState>('kpiCards');

export const selectKpis = createSelector(
  selectKpiCardsState,
  (state) => state.kpis
);

export const selectFilteredKpis = createSelector(
  selectKpis,
  selectKpiCardsState,
  (kpis, state) => {
    const { filters } = state;
    return kpis.filter(kpi => {
      if (filters.metricTypes.length > 0 && !filters.metricTypes.includes(kpi.type)) {
        return false;
      }
      return true;
    });
  }
);

export const selectConnectionState = createSelector(
  selectKpiCardsState,
  (state) => state.connectionState
);

export const selectIsRealTimeActive = createSelector(
  selectConnectionState,
  (connectionState) => connectionState.status === 'connected'
);
```

---

## 7. Styling and Responsive Design

### Component Styles (SCSS)

```scss
// kpi-cards-page.component.scss
.kpi-cards-page {
  padding: 1.5rem;
  background: var(--background-subtle);
  min-height: 100vh;

  &__header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 2rem;
    gap: 1rem;
    flex-wrap: wrap;

    @media (max-width: 768px) {
      flex-direction: column;
      align-items: stretch;
      text-align: center;
    }
  }

  &__title {
    font-size: 2rem;
    font-weight: 600;
    color: var(--text-primary);
    margin: 0;
  }

  &__actions {
    display: flex;
    gap: 0.75rem;
    align-items: center;

    @media (max-width: 768px) {
      justify-content: center;
      flex-wrap: wrap;
    }
  }

  &__grid {
    display: grid;
    gap: 1.5rem;
    grid-template-columns: repeat(var(--grid-columns, 1), 1fr);

    @media (min-width: 768px) {
      --grid-columns: 2;
    }

    @media (min-width: 1200px) {
      --grid-columns: 3;
    }

    @media (min-width: 1600px) {
      --grid-columns: 4;
    }
  }

  &__connection-status {
    position: fixed;
    top: 1rem;
    right: 1rem;
    z-index: 1000;
    padding: 0.5rem 1rem;
    border-radius: 0.5rem;
    font-size: 0.875rem;
    font-weight: 500;

    &--connected {
      background: var(--success-subtle);
      color: var(--success-text);
      border: 1px solid var(--success-border);
    }

    &--disconnected {
      background: var(--error-subtle);
      color: var(--error-text);
      border: 1px solid var(--error-border);
    }

    &--reconnecting {
      background: var(--warning-subtle);
      color: var(--warning-text);
      border: 1px solid var(--warning-border);
    }
  }
}

// enhanced-kpi-card.component.scss
.kpi-card {
  background: var(--surface-primary);
  border: 1px solid var(--border-subtle);
  border-radius: 0.75rem;
  padding: 1.5rem;
  transition: all 0.2s ease;
  position: relative;
  cursor: pointer;

  &:hover {
    border-color: var(--border-interactive);
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
    transform: translateY(-2px);
  }

  &:focus {
    outline: 2px solid var(--focus-ring);
    outline-offset: 2px;
  }

  &--loading {
    pointer-events: none;
    opacity: 0.7;

    .kpi-card__content {
      &::after {
        content: '';
        position: absolute;
        top: 0;
        left: 0;
        right: 0;
        bottom: 0;
        background: linear-gradient(90deg, transparent, rgba(255,255,255,0.4), transparent);
        animation: shimmer 1.5s ease-in-out infinite;
      }
    }
  }

  &--error {
    border-color: var(--error-border);
    background: var(--error-subtle);
  }

  &--stale {
    border-color: var(--warning-border);

    &::before {
      content: 'Stale Data';
      position: absolute;
      top: 0.5rem;
      right: 0.5rem;
      font-size: 0.75rem;
      color: var(--warning-text);
      background: var(--warning-subtle);
      padding: 0.25rem 0.5rem;
      border-radius: 0.25rem;
    }
  }

  &__header {
    display: flex;
    justify-content: space-between;
    align-items: flex-start;
    margin-bottom: 1rem;
  }

  &__title {
    font-size: 0.875rem;
    font-weight: 500;
    color: var(--text-secondary);
    margin: 0;
    line-height: 1.4;
  }

  &__actions {
    display: flex;
    gap: 0.5rem;
    opacity: 0;
    transition: opacity 0.2s ease;
  }

  &:hover &__actions,
  &:focus &__actions {
    opacity: 1;
  }

  &__value {
    font-size: 2rem;
    font-weight: 700;
    color: var(--text-primary);
    margin: 0.5rem 0;
    line-height: 1.2;

    @media (max-width: 768px) {
      font-size: 1.75rem;
    }
  }

  &__trend {
    display: flex;
    align-items: center;
    gap: 0.5rem;
    font-size: 0.875rem;
    font-weight: 500;

    &--up {
      color: var(--success-text);
    }

    &--down {
      color: var(--error-text);
    }

    &--stable {
      color: var(--text-secondary);
    }
  }

  &__sparkline {
    margin: 1rem 0 0.5rem;
    height: 30px;
  }

  &__meta {
    display: flex;
    justify-content: space-between;
    align-items: center;
    font-size: 0.75rem;
    color: var(--text-tertiary);
    margin-top: 1rem;
  }

  &__last-updated {
    &--fresh {
      color: var(--success-text);
    }

    &--stale {
      color: var(--warning-text);
    }
  }
}

@keyframes shimmer {
  0% { transform: translateX(-100%); }
  100% { transform: translateX(100%); }
}

// Accessibility enhancements
@media (prefers-reduced-motion: reduce) {
  .kpi-card {
    transition: none;

    &:hover {
      transform: none;
    }
  }

  * {
    animation-duration: 0.01ms !important;
    animation-iteration-count: 1 !important;
    transition-duration: 0.01ms !important;
  }
}

// High contrast mode
@media (prefers-contrast: high) {
  .kpi-card {
    border-width: 2px;

    &:focus {
      outline-width: 3px;
    }
  }
}

// Dark mode support
@media (prefers-color-scheme: dark) {
  .kpi-card {
    background: var(--surface-primary-dark);
    border-color: var(--border-subtle-dark);

    &:hover {
      box-shadow: 0 4px 12px rgba(0, 0, 0, 0.3);
    }
  }
}

// RTL support
[dir="rtl"] {
  .kpi-cards-page__grid {
    direction: rtl;
  }

  .kpi-card__actions {
    left: 0.5rem;
    right: auto;
  }

  .kpi-card__trend {
    flex-direction: row-reverse;
  }
}
```

---

## 8. Accessibility Implementation

### ARIA Labels and Live Regions

```typescript
// Accessibility service
@Injectable({
  providedIn: 'root'
})
export class KpiAccessibilityService {
  private liveRegion: HTMLElement | null = null;

  constructor(@Inject(DOCUMENT) private document: Document) {
    this.setupLiveRegion();
  }

  private setupLiveRegion(): void {
    this.liveRegion = this.document.createElement('div');
    this.liveRegion.setAttribute('aria-live', 'polite');
    this.liveRegion.setAttribute('aria-atomic', 'true');
    this.liveRegion.style.position = 'absolute';
    this.liveRegion.style.left = '-10000px';
    this.liveRegion.style.width = '1px';
    this.liveRegion.style.height = '1px';
    this.liveRegion.style.overflow = 'hidden';
    this.document.body.appendChild(this.liveRegion);
  }

  public announceKpiUpdate(kpiName: string, oldValue: number, newValue: number): void {
    if (!this.liveRegion) return;

    const change = newValue - oldValue;
    const direction = change > 0 ? 'increased' : 'decreased';
    const percentage = Math.abs((change / oldValue) * 100).toFixed(1);

    const message = `${kpiName} has ${direction} to ${newValue}, a ${percentage}% change`;

    this.liveRegion.textContent = message;

    // Clear after announcement
    setTimeout(() => {
      if (this.liveRegion) {
        this.liveRegion.textContent = '';
      }
    }, 1000);
  }

  public announceConnectionStatus(status: ConnectionState['status']): void {
    if (!this.liveRegion) return;

    const messages = {
      connected: 'Real-time data connection established',
      disconnected: 'Real-time data connection lost, using cached data',
      reconnecting: 'Reconnecting to real-time data feed',
      error: 'Connection error, data may be outdated'
    };

    this.liveRegion.textContent = messages[status] || '';
  }

  public getKpiAriaLabel(kpi: EnhancedKpiCard): string {
    const trendText = this.getTrendDescription(kpi.trend);
    const freshnessText = this.getFreshnessDescription(kpi.dataFreshness);

    return `${kpi.name}: ${kpi.formattedValue}. ${trendText}. ${freshnessText}. ${kpi.isDrillDownAvailable ? 'Click for details' : ''}`;
  }

  private getTrendDescription(trend?: TrendIndicator): string {
    if (!trend) return 'No trend data';

    const direction = trend.direction === 'up' ? 'increasing' :
                     trend.direction === 'down' ? 'decreasing' : 'stable';
    const magnitude = Math.abs(trend.value);

    return `Trend ${direction} by ${magnitude.toFixed(1)}%`;
  }

  private getFreshnessDescription(freshness?: DataFreshness): string {
    if (!freshness) return 'Data freshness unknown';

    const ageMinutes = Math.floor(freshness.age / 60);

    if (ageMinutes < 1) return 'Data is current';
    if (ageMinutes < 5) return `Data is ${ageMinutes} minutes old`;
    return 'Data may be outdated';
  }
}
```

### Keyboard Navigation Implementation

```typescript
// Keyboard navigation directive
@Directive({
  selector: '[appKpiKeyboardNav]'
})
export class KpiKeyboardNavDirective implements OnInit, OnDestroy {
  @Input() gridColumns = 4;
  @Input() totalItems = 0;

  private currentIndex = 0;
  private keyboardListeners: (() => void)[] = [];

  constructor(
    private elementRef: ElementRef<HTMLElement>,
    private renderer: Renderer2
  ) {}

  ngOnInit(): void {
    this.setupKeyboardListeners();
  }

  private setupKeyboardListeners(): void {
    const element = this.elementRef.nativeElement;

    const arrowKeyListener = this.renderer.listen(element, 'keydown', (event: KeyboardEvent) => {
      switch (event.key) {
        case 'ArrowRight':
          event.preventDefault();
          this.moveToNext();
          break;
        case 'ArrowLeft':
          event.preventDefault();
          this.moveToPrevious();
          break;
        case 'ArrowDown':
          event.preventDefault();
          this.moveDown();
          break;
        case 'ArrowUp':
          event.preventDefault();
          this.moveUp();
          break;
        case 'Home':
          event.preventDefault();
          this.moveToFirst();
          break;
        case 'End':
          event.preventDefault();
          this.moveToLast();
          break;
      }
    });

    this.keyboardListeners.push(arrowKeyListener);
  }

  private moveToNext(): void {
    this.currentIndex = Math.min(this.currentIndex + 1, this.totalItems - 1);
    this.focusItem(this.currentIndex);
  }

  private moveToPrevious(): void {
    this.currentIndex = Math.max(this.currentIndex - 1, 0);
    this.focusItem(this.currentIndex);
  }

  private moveDown(): void {
    const newIndex = this.currentIndex + this.gridColumns;
    if (newIndex < this.totalItems) {
      this.currentIndex = newIndex;
      this.focusItem(this.currentIndex);
    }
  }

  private moveUp(): void {
    const newIndex = this.currentIndex - this.gridColumns;
    if (newIndex >= 0) {
      this.currentIndex = newIndex;
      this.focusItem(this.currentIndex);
    }
  }

  private focusItem(index: number): void {
    const items = this.elementRef.nativeElement.querySelectorAll('.kpi-card');
    const item = items[index] as HTMLElement;
    if (item) {
      item.focus();
    }
  }
}
```

---

## 9. Testing Strategy

### Component Unit Tests

```typescript
// enhanced-kpi-card.component.spec.ts
describe('EnhancedKpiCardComponent', () => {
  let component: EnhancedKpiCardComponent;
  let fixture: ComponentFixture<EnhancedKpiCardComponent>;
  let mockAnalyticsService: jasmine.SpyObj<AnalyticsApiService>;

  beforeEach(async () => {
    const spy = jasmine.createSpyObj('AnalyticsApiService', ['getKpiDrillDown']);

    await TestBed.configureTestingModule({
      declarations: [EnhancedKpiCardComponent],
      providers: [
        { provide: AnalyticsApiService, useValue: spy }
      ],
      imports: [CommonModule, BrowserAnimationsModule]
    }).compileComponents();

    fixture = TestBed.createComponent(EnhancedKpiCardComponent);
    component = fixture.componentInstance;
    mockAnalyticsService = TestBed.inject(AnalyticsApiService) as jasmine.SpyObj<AnalyticsApiService>;
  });

  describe('Real-time Value Updates', () => {
    it('should animate value changes smoothly', fakeAsync(() => {
      const initialValue = 100;
      const newValue = 150;

      component.kpiData = createMockKpiCard({ value: initialValue });
      component.ngOnChanges({ kpiData: new SimpleChange(null, component.kpiData, true) });

      // Update value
      component.kpiData = { ...component.kpiData, value: newValue };
      component.ngOnChanges({ kpiData: new SimpleChange(component.kpiData, component.kpiData, false) });

      // Animation should start
      expect(component.animatedValue).toBe(initialValue);

      // Fast-forward animation
      tick(500);
      expect(component.animatedValue).toBeCloseTo(newValue, 0);
    }));

    it('should emit value change events', () => {
      spyOn(component.valueChange, 'emit');

      component.kpiData = createMockKpiCard({ value: 100 });
      component.ngOnChanges({ kpiData: new SimpleChange(null, component.kpiData, true) });

      component.kpiData = { ...component.kpiData, value: 150 };
      component.ngOnChanges({ kpiData: new SimpleChange(component.kpiData, component.kpiData, false) });

      expect(component.valueChange.emit).toHaveBeenCalledWith({
        metric: component.kpiData.name,
        oldValue: 100,
        newValue: 150,
        timestamp: jasmine.any(Date)
      });
    });
  });

  describe('Accessibility', () => {
    it('should have proper ARIA labels', () => {
      component.kpiData = createMockKpiCard({
        name: 'Daily Active Users',
        value: 1250,
        trend: { direction: 'up', value: 15, isGood: true }
      });

      component.ngOnChanges({ kpiData: new SimpleChange(null, component.kpiData, true) });
      fixture.detectChanges();

      expect(component.accessibilityLabel).toContain('Daily Active Users: 1,250');
      expect(component.accessibilityLabel).toContain('increasing');
    });

    it('should handle keyboard navigation', () => {
      spyOn(component.drillDown, 'emit');

      component.kpiData = createMockKpiCard({ isDrillDownAvailable: true });
      fixture.detectChanges();

      const cardElement = fixture.debugElement.nativeElement;
      cardElement.dispatchEvent(new KeyboardEvent('keydown', { key: 'Enter' }));

      expect(component.drillDown.emit).toHaveBeenCalled();
    });

    it('should support screen reader announcements', () => {
      component.kpiData = createMockKpiCard({
        dataFreshness: { age: 30, status: 'fresh', lastUpdated: new Date(), source: 'realtime' }
      });

      fixture.detectChanges();

      expect(component.accessibilityLabel).toContain('Data is current');
    });
  });

  describe('Error Handling', () => {
    it('should display error state for stale data', () => {
      component.kpiData = createMockKpiCard({
        dataFreshness: { age: 300, status: 'stale', lastUpdated: new Date(), source: 'cached' }
      });

      fixture.detectChanges();

      expect(component.isDataStale).toBe(true);
      expect(fixture.debugElement.nativeElement).toHaveClass('kpi-card--stale');
    });

    it('should handle missing trend data gracefully', () => {
      component.kpiData = createMockKpiCard({ trend: undefined });

      expect(() => {
        component.ngOnChanges({ kpiData: new SimpleChange(null, component.kpiData, true) });
      }).not.toThrow();
    });
  });

  function createMockKpiCard(overrides: Partial<EnhancedKpiCard> = {}): EnhancedKpiCard {
    return {
      id: 'mock-kpi',
      name: 'Mock KPI',
      value: 100,
      formattedValue: '100',
      lastUpdated: new Date(),
      dataFreshness: {
        age: 10,
        status: 'fresh',
        lastUpdated: new Date(),
        source: 'realtime'
      },
      isDrillDownAvailable: true,
      hasAlerts: false,
      isCustomizable: true,
      ariaLabel: 'Mock KPI',
      ariaDescription: 'Mock description',
      size: 'medium',
      priority: 'medium',
      colorScheme: 'default',
      animationState: 'idle',
      type: 'metric',
      ...overrides
    };
  }
});
```

### Integration Tests

```typescript
// kpi-cards-integration.spec.ts
describe('KPI Cards Integration', () => {
  let component: KpiCardsPageComponent;
  let fixture: ComponentFixture<KpiCardsPageComponent>;
  let mockAnalyticsService: jasmine.SpyObj<AnalyticsApiService>;
  let store: MockStore;

  beforeEach(async () => {
    const analyticsSpy = jasmine.createSpyObj('AnalyticsApiService', [
      'initializeSignalRConnection',
      'getDetailedKpis',
      'startPolling',
      'stopPolling'
    ]);

    await TestBed.configureTestingModule({
      declarations: [KpiCardsPageComponent, EnhancedKpiCardComponent],
      providers: [
        { provide: AnalyticsApiService, useValue: analyticsSpy },
        provideMockStore({ initialState: { kpiCards: initialKpiCardsState } })
      ],
      imports: [CommonModule, BrowserAnimationsModule]
    }).compileComponents();

    store = TestBed.inject(MockStore);
    mockAnalyticsService = TestBed.inject(AnalyticsApiService) as jasmine.SpyObj<AnalyticsApiService>;
  });

  describe('Real-time Connection', () => {
    it('should initialize SignalR connection on component init', async () => {
      mockAnalyticsService.initializeSignalRConnection.and.returnValue(Promise.resolve());

      component = fixture.componentInstance;
      await component.ngOnInit();

      expect(mockAnalyticsService.initializeSignalRConnection).toHaveBeenCalled();
    });

    it('should fallback to polling when SignalR fails', async () => {
      mockAnalyticsService.initializeSignalRConnection.and.returnValue(Promise.reject(new Error('Connection failed')));
      mockAnalyticsService.startPolling.and.stub();

      component = fixture.componentInstance;
      await component.ngOnInit();

      expect(mockAnalyticsService.startPolling).toHaveBeenCalled();
    });
  });

  describe('State Management', () => {
    it('should dispatch load action on initialization', () => {
      spyOn(store, 'dispatch');

      component = fixture.componentInstance;
      component.ngOnInit();

      expect(store.dispatch).toHaveBeenCalledWith(
        KpiCardsActions.loadKpis({ filters: jasmine.any(Object) })
      );
    });

    it('should update component state when store changes', () => {
      const mockKpis = [createMockKpiCard()];

      store.setState({
        kpiCards: {
          ...initialKpiCardsState,
          kpis: mockKpis,
          isLoading: false
        }
      });

      component = fixture.componentInstance;
      component.kpis$.subscribe(kpis => {
        expect(kpis).toEqual(mockKpis);
      });
    });
  });
});
```

---

## 10. Performance Optimizations

### Lazy Loading and Code Splitting

```typescript
// kpi-cards.module.ts
@NgModule({
  declarations: [
    KpiCardsPageComponent,
    EnhancedKpiCardComponent,
    KpiSparklineComponent,
    KpiDrillDownModalComponent,
    KpiFiltersComponent
  ],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterModule.forChild([
      {
        path: '',
        component: KpiCardsPageComponent,
        data: { preload: true }
      }
    ]),
    // Lazy load chart library
    NgChartsModule.forRoot({
      defaults: {
        global: {
          responsive: true,
          maintainAspectRatio: false
        }
      }
    })
  ],
  providers: [
    KpiAccessibilityService,
    {
      provide: 'CHART_CONFIG',
      useFactory: () => import('./chart.config').then(m => m.chartConfig)
    }
  ]
})
export class KpiCardsModule {}

// Lazy loading route configuration
const routes: Routes = [
  {
    path: 'kpi-cards',
    loadChildren: () => import('./kpi-cards/kpi-cards.module').then(m => m.KpiCardsModule),
    data: { preload: true }
  }
];
```

### Virtual Scrolling for Large Datasets

```typescript
// Virtual scrolling implementation for large KPI lists
@Component({
  selector: 'app-virtual-kpi-grid',
  template: `
    <cdk-virtual-scroll-viewport
      [itemSize]="cardHeight"
      class="kpi-grid-viewport"
      [orientation]="orientation">
      <div
        *cdkVirtualFor="let kpi of kpis; trackBy: trackByKpiId"
        class="virtual-kpi-item">
        <app-enhanced-kpi-card
          [kpiData]="kpi"
          [config]="cardConfig"
          (drillDown)="onDrillDown($event)"
          (valueChange)="onValueChange($event)">
        </app-enhanced-kpi-card>
      </div>
    </cdk-virtual-scroll-viewport>
  `
})
export class VirtualKpiGridComponent {
  @Input() kpis: EnhancedKpiCard[] = [];
  @Input() cardHeight = 200;
  @Input() orientation: 'vertical' | 'horizontal' = 'vertical';

  public trackByKpiId(index: number, kpi: EnhancedKpiCard): string {
    return kpi.id;
  }
}
```

### Memory Management and Cleanup

```typescript
// Memory management service
@Injectable({
  providedIn: 'root'
})
export class KpiMemoryManagerService {
  private subscriptions = new Map<string, Subscription>();
  private timers = new Map<string, Timer>();
  private animationFrames = new Map<string, number>();

  public addSubscription(key: string, subscription: Subscription): void {
    this.cleanupSubscription(key);
    this.subscriptions.set(key, subscription);
  }

  public addTimer(key: string, timer: Timer): void {
    this.cleanupTimer(key);
    this.timers.set(key, timer);
  }

  public addAnimationFrame(key: string, frameId: number): void {
    this.cleanupAnimationFrame(key);
    this.animationFrames.set(key, frameId);
  }

  public cleanup(key?: string): void {
    if (key) {
      this.cleanupSubscription(key);
      this.cleanupTimer(key);
      this.cleanupAnimationFrame(key);
    } else {
      this.cleanupAll();
    }
  }

  private cleanupAll(): void {
    this.subscriptions.forEach(sub => sub.unsubscribe());
    this.timers.forEach(timer => clearInterval(timer));
    this.animationFrames.forEach(frameId => cancelAnimationFrame(frameId));

    this.subscriptions.clear();
    this.timers.clear();
    this.animationFrames.clear();
  }

  private cleanupSubscription(key: string): void {
    const existing = this.subscriptions.get(key);
    if (existing) {
      existing.unsubscribe();
      this.subscriptions.delete(key);
    }
  }

  private cleanupTimer(key: string): void {
    const existing = this.timers.get(key);
    if (existing) {
      clearInterval(existing);
      this.timers.delete(key);
    }
  }

  private cleanupAnimationFrame(key: string): void {
    const existing = this.animationFrames.get(key);
    if (existing) {
      cancelAnimationFrame(existing);
      this.animationFrames.delete(key);
    }
  }
}
```

---

This comprehensive component design provides a production-ready foundation for the KPI Cards page with enterprise-grade features including real-time capabilities, accessibility compliance, performance optimizations, and comprehensive testing strategies. The design emphasizes maintainability, scalability, and user experience while following Angular best practices and modern web development standards.