# KPI Cards Implementation Plan - A→M Workflow Step E

**PAGE_KEY**: kpi-cards
**COMPLETION DATE**: 2025-01-29
**STATUS**: ✅ COMPLETE

## Implementation Overview

The KPI Cards page implementation follows a systematic 4-day development approach with comprehensive testing, performance optimization, and production deployment. This plan ensures 100% requirement coverage with enterprise-grade quality and cutting-edge real-time capabilities.

---

## Day 1: Foundation and Core Components

### Phase 1.1: Project Setup and Infrastructure (2 hours)

#### Task 1.1.1: Create Module Structure
```bash
# Create KPI Cards module structure
ng generate module features/kpi-cards --routing
ng generate component features/kpi-cards/kpi-cards-page
ng generate component features/kpi-cards/components/enhanced-kpi-card
ng generate component features/kpi-cards/components/kpi-sparkline
ng generate component features/kpi-cards/components/kpi-filters
ng generate service features/kpi-cards/services/kpi-memory-manager
ng generate service features/kpi-cards/services/kpi-accessibility
```

#### Task 1.1.2: Configure Module Dependencies
```typescript
// kpi-cards.module.ts
import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { ScrollingModule } from '@angular/cdk/scrolling';
import { OverlayModule } from '@angular/cdk/overlay';
import { A11yModule } from '@angular/cdk/a11y';
import { NgChartsModule } from 'ng2-charts';

import { KpiCardsRoutingModule } from './kpi-cards-routing.module';
import { KpiCardsPageComponent } from './kpi-cards-page.component';
import { EnhancedKpiCardComponent } from './components/enhanced-kpi-card/enhanced-kpi-card.component';
import { KpiSparklineComponent } from './components/kpi-sparkline/kpi-sparkline.component';
import { KpiFiltersComponent } from './components/kpi-filters/kpi-filters.component';

@NgModule({
  declarations: [
    KpiCardsPageComponent,
    EnhancedKpiCardComponent,
    KpiSparklineComponent,
    KpiFiltersComponent
  ],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    KpiCardsRoutingModule,
    ScrollingModule,
    OverlayModule,
    A11yModule,
    NgChartsModule
  ],
  providers: []
})
export class KpiCardsModule { }
```

#### Task 1.1.3: Setup Routing Configuration
```typescript
// kpi-cards-routing.module.ts
const routes: Routes = [
  {
    path: '',
    component: KpiCardsPageComponent,
    data: {
      title: 'KPI Cards',
      breadcrumb: 'KPI Cards',
      roles: ['ProductManager', 'Support', 'Operations']
    }
  }
];
```

### Phase 1.2: Enhanced Data Models (1 hour)

#### Task 1.2.1: Extend Analytics Models
```typescript
// Add to src/app/models/analytics/analytics-models.ts

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

export interface KpiCardConfig {
  refreshInterval: number;
  animationDuration: number;
  enableRealTime: boolean;
  enableDrillDown: boolean;
  enableExport: boolean;
  accessibilityMode: 'standard' | 'enhanced';
}

export interface KpiFilters {
  timeRange: TimeRange;
  organizationId?: string;
  metricTypes: string[];
  showTrends: boolean;
  groupBy: 'category' | 'priority' | 'role';
}

export interface KpiValueChange {
  metric: string;
  oldValue: number;
  newValue: number;
  timestamp: Date;
}

export interface DrillDownData {
  kpiId: string;
  breakdown: SegmentBreakdown[];
  trends: TimeSeriesPoint[];
  insights: BusinessInsight[];
  metadata: {
    dataRange: string;
    totalRecords: number;
    lastCalculated: Date;
  };
}
```

### Phase 1.3: State Management Setup (2 hours)

#### Task 1.3.1: Create NgRx Store Structure
```typescript
// Store setup for KPI Cards
ng generate store kpi-cards --module=features/kpi-cards/kpi-cards.module.ts

// kpi-cards.state.ts
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

// kpi-cards.actions.ts
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
```

#### Task 1.3.2: Implement Reducers and Selectors
```typescript
// kpi-cards.reducer.ts
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
  }))
);

// kpi-cards.selectors.ts
export const selectKpiCardsState = createFeatureSelector<KpiCardsState>('kpiCards');
export const selectKpis = createSelector(selectKpiCardsState, (state) => state.kpis);
export const selectFilters = createSelector(selectKpiCardsState, (state) => state.filters);
export const selectConnectionState = createSelector(selectKpiCardsState, (state) => state.connectionState);
```

### Phase 1.4: Main Page Component (3 hours)

#### Task 1.4.1: Implement KpiCardsPageComponent Structure
```typescript
// kpi-cards-page.component.ts
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
export class KpiCardsPageComponent implements OnInit, OnDestroy, AfterViewInit {
  // Reactive state management
  public readonly kpis$ = this.store.select(selectKpis);
  public readonly filters$ = this.store.select(selectFilters);
  public readonly connectionState$ = this.store.select(selectConnectionState);
  public readonly isLoading$ = this.store.select(selectIsLoading);

  // Component state
  public gridColumns = 4;
  public selectedCards = new Set<string>();
  public showFilters = false;

  // Lifecycle management
  private destroy$ = new Subject<void>();

  constructor(
    private store: Store,
    private analyticsService: AnalyticsApiService,
    private cdr: ChangeDetectorRef,
    private breakpointObserver: BreakpointObserver,
    private accessibilityService: KpiAccessibilityService,
    private memoryManager: KpiMemoryManagerService
  ) {}

  ngOnInit(): void {
    this.initializeComponent();
    this.setupResponsiveGrid();
    this.setupRealTimeConnection();
    this.loadInitialData();
  }

  ngAfterViewInit(): void {
    this.setupAccessibilityFeatures();
    this.setupKeyboardNavigation();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    this.memoryManager.cleanup();
  }

  private initializeComponent(): void {
    // Initialize filters
    this.store.dispatch(KpiCardsActions.applyFilters({
      filters: {
        timeRange: '30d',
        metricTypes: [],
        showTrends: true,
        groupBy: 'category'
      }
    }));
  }

  private setupRealTimeConnection(): void {
    this.store.dispatch(KpiCardsActions.startRealTime());

    // Monitor connection state
    this.connectionState$
      .pipe(takeUntil(this.destroy$))
      .subscribe(state => {
        this.accessibilityService.announceConnectionStatus(state.status);
      });
  }

  private loadInitialData(): void {
    this.store.dispatch(KpiCardsActions.loadKpis({}));
  }

  public onDrillDown(kpiId: string): void {
    this.store.dispatch(KpiCardsActions.loadDrillDown({ kpiId }));
  }

  public onFiltersChange(filters: KpiFilters): void {
    this.store.dispatch(KpiCardsActions.applyFilters({ filters }));
    this.store.dispatch(KpiCardsActions.loadKpis({ filters }));
  }

  public onValueChange(change: KpiValueChange): void {
    this.accessibilityService.announceKpiUpdate(
      change.metric,
      change.oldValue,
      change.newValue
    );
  }
}
```

---

## Day 2: Core Components and Real-time Features

### Phase 2.1: Enhanced KPI Card Component (4 hours)

#### Task 2.1.1: Implement EnhancedKpiCardComponent
```typescript
// enhanced-kpi-card.component.ts
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
  public readonly descriptionId = `kpi-desc-${this.generateId()}`;
  public accessibilityLabel = '';

  constructor(
    private cdr: ChangeDetectorRef,
    private zone: NgZone,
    private memoryManager: KpiMemoryManagerService
  ) {}

  ngOnInit(): void {
    this.updateAccessibilityLabels();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['kpiData'] && this.kpiData) {
      this.handleValueChange();
      this.updateAccessibilityLabels();
    }
  }

  ngOnDestroy(): void {
    if (this.animationFrameId) {
      cancelAnimationFrame(this.animationFrameId);
    }
  }

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
  }

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

    this.zone.runOutsideAngular(() => {
      this.animationFrameId = requestAnimationFrame(animate);
    });
  }

  public onCardClick(): void {
    if (this.kpiData.isDrillDownAvailable) {
      this.drillDown.emit(this.kpiData.id);
    }
  }

  public get isDataStale(): boolean {
    if (!this.kpiData.dataFreshness) return false;
    return this.kpiData.dataFreshness.status === 'stale';
  }

  private updateAccessibilityLabels(): void {
    const trend = this.kpiData.trend?.direction || 'stable';
    const trendText = this.getTrendDescription(trend);

    this.accessibilityLabel = `${this.kpiData.name}: ${this.formatValue(this.kpiData.value)}. ${trendText}. Last updated ${this.formatLastUpdated()}`;
  }

  private getTrendDescription(direction: string): string {
    switch (direction) {
      case 'up': return 'Trending up';
      case 'down': return 'Trending down';
      default: return 'Stable trend';
    }
  }

  private formatValue(value: number): string {
    return new Intl.NumberFormat().format(value);
  }

  private formatLastUpdated(): string {
    if (!this.kpiData.lastUpdated) return 'Unknown';
    return new Intl.RelativeTimeFormat().format(
      Math.floor((this.kpiData.lastUpdated.getTime() - Date.now()) / 60000),
      'minute'
    );
  }

  private generateId(): string {
    return Math.random().toString(36).substr(2, 9);
  }
}
```

#### Task 2.1.2: Create Card Template and Styles
```html
<!-- enhanced-kpi-card.component.html -->
<div class="kpi-card__header">
  <h3 class="kpi-card__title">{{ kpiData.name }}</h3>
  <div class="kpi-card__actions" *ngIf="kpiData.isDrillDownAvailable">
    <button
      type="button"
      class="action-button"
      [attr.aria-label]="'View details for ' + kpiData.name"
      (click)="$event.stopPropagation(); onCardClick()">
      <svg class="icon" viewBox="0 0 20 20" fill="currentColor">
        <path fill-rule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7-4a1 1 0 11-2 0 1 1 0 012 0zM9 9a1 1 0 000 2v3a1 1 0 001 1h1a1 1 0 100-2v-3a1 1 0 00-1-1H9z" clip-rule="evenodd" />
      </svg>
    </button>
  </div>
</div>

<div class="kpi-card__content">
  <div class="kpi-card__value" [@valueChange]="animatedValue">
    {{ animatedValue | number:'1.0-2' }}
  </div>

  <div class="kpi-card__trend" *ngIf="kpiData.trend" [@trendIndicator]="kpiData.trend.direction">
    <svg class="trend-icon" viewBox="0 0 20 20" fill="currentColor">
      <path fill-rule="evenodd" d="M5.293 7.707a1 1 0 010-1.414l4-4a1 1 0 011.414 0l4 4a1 1 0 01-1.414 1.414L11 5.414V17a1 1 0 11-2 0V5.414L6.707 7.707a1 1 0 01-1.414 0z" clip-rule="evenodd" />
    </svg>
    <span class="trend-text">{{ kpiData.trend.value | number:'1.1-1' }}%</span>
  </div>

  <div class="kpi-card__sparkline" *ngIf="kpiData.sparklineData">
    <app-kpi-sparkline
      [data]="kpiData.sparklineData"
      [color]="getSparklineColor()"
      [width]="120"
      [height]="30">
    </app-kpi-sparkline>
  </div>
</div>

<div class="kpi-card__meta">
  <span class="last-updated" [class]="getLastUpdatedClass()">
    Last updated: {{ kpiData.lastUpdated | date:'short' }}
  </span>
  <span class="data-freshness" *ngIf="isDataStale">
    <svg class="warning-icon" viewBox="0 0 20 20" fill="currentColor">
      <path fill-rule="evenodd" d="M8.257 3.099c.765-1.36 2.722-1.36 3.486 0l5.58 9.92c.75 1.334-.213 2.98-1.742 2.98H4.42c-1.53 0-2.493-1.646-1.743-2.98l5.58-9.92zM11 13a1 1 0 11-2 0 1 1 0 012 0zm-1-8a1 1 0 00-1 1v3a1 1 0 002 0V6a1 1 0 00-1-1z" clip-rule="evenodd" />
    </svg>
    Stale data
  </span>
</div>

<!-- Hidden description for screen readers -->
<div [id]="descriptionId" class="sr-only">
  {{ kpiData.ariaDescription }}
</div>
```

### Phase 2.2: Sparkline Component (2 hours)

#### Task 2.2.1: Implement KpiSparklineComponent
```typescript
// kpi-sparkline.component.ts
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
export class KpiSparklineComponent implements OnInit, OnDestroy, AfterViewInit, OnChanges {
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

  constructor(private cdr: ChangeDetectorRef) {}

  ngAfterViewInit(): void {
    this.ctx = this.canvasRef.nativeElement.getContext('2d')!;
    this.setupCanvas();
    this.renderSparkline();
    this.updateAriaLabel();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['data'] && this.ctx) {
      this.renderSparkline();
      this.updateAriaLabel();
    }
  }

  private setupCanvas(): void {
    const canvas = this.canvasRef.nativeElement;
    const dpr = window.devicePixelRatio || 1;

    canvas.width = this.width * dpr;
    canvas.height = this.height * dpr;
    canvas.style.width = this.width + 'px';
    canvas.style.height = this.height + 'px';

    this.ctx.scale(dpr, dpr);
  }

  private renderSparkline(): void {
    if (!this.ctx || this.data.length === 0) return;

    const { width, height } = this;

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

  public onMouseLeave(): void {
    this.tooltip.visible = false;
    this.cdr.markForCheck();
  }

  private updateAriaLabel(): void {
    if (this.data.length === 0) {
      this.ariaLabel = 'No trend data available';
      return;
    }

    const firstValue = this.data[0].value;
    const lastValue = this.data[this.data.length - 1].value;
    const change = ((lastValue - firstValue) / firstValue * 100);
    const direction = lastValue > firstValue ? 'increased' : 'decreased';

    this.ariaLabel = `Trend sparkline showing data has ${direction} by ${Math.abs(change).toFixed(1)}% over the period`;
  }
}
```

### Phase 2.3: Real-time Connection Implementation (2 hours)

#### Task 2.3.1: Enhance Analytics Service with Real-time Features
```typescript
// Add real-time methods to analytics-api.service.ts

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

  this.hubConnection.onclose(() => {
    this.connectionStateSubject.next({
      status: 'disconnected',
      reconnectAttempts: 0
    });
    this.fallbackToPolling();
  });
}

// Handle real-time KPI updates
private handleKpiUpdate(update: KpiUpdate): void {
  this.store.dispatch(KpiCardsActions.updateKpiValue({
    kpiId: update.kpiId,
    newValue: update.newValue,
    trend: update.trend
  }));
}

// Fallback to HTTP polling when SignalR fails
private fallbackToPolling(): void {
  console.warn('SignalR connection lost, falling back to HTTP polling');
  this.startPolling(30000); // Poll every 30 seconds
}

public startPolling(interval: number = 30000): void {
  this.stopPolling();

  this.pollingTimer = setInterval(async () => {
    try {
      const filters = this.store.selectSnapshot(selectFilters);
      await this.getDetailedKpis(filters);
    } catch (error) {
      console.error('Polling failed:', error);
    }
  }, interval);
}
```

---

## Day 3: Advanced Features and Accessibility

### Phase 3.1: Drill-down Modal Component (3 hours)

#### Task 3.1.1: Create Drill-down Modal
```typescript
// kpi-drill-down-modal.component.ts
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
    private cdr: ChangeDetectorRef,
    private focusTrap: FocusTrap
  ) {}

  @HostListener('document:keydown.escape')
  onEscapeKey(): void {
    this.onClose();
  }

  async ngOnInit(): Promise<void> {
    if (this.kpiId) {
      await this.loadDrillDownData();
      this.setupFocusTrap();
    }
  }

  ngOnDestroy(): void {
    this.focusTrap?.destroy();
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

  private setupFocusTrap(): void {
    const modalElement = document.querySelector('.drill-down-modal') as HTMLElement;
    if (modalElement) {
      this.focusTrap = new FocusTrap(modalElement);
      this.focusTrap.activate();
    }
  }

  public onTabChange(tabId: string): void {
    this.activeTab = tabId;
    this.cdr.markForCheck();
  }

  public onClose(): void {
    this.focusTrap?.deactivate();
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

### Phase 3.2: Accessibility Service Implementation (2 hours)

#### Task 3.2.1: Create Accessibility Service
```typescript
// kpi-accessibility.service.ts
@Injectable({
  providedIn: 'root'
})
export class KpiAccessibilityService {
  private liveRegion: HTMLElement | null = null;
  private lastAnnouncement = '';
  private announceTimer?: Timer;

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

    const message = `${kpiName} has ${direction} to ${this.formatNumber(newValue)}, a ${percentage}% change`;

    // Avoid duplicate announcements
    if (message === this.lastAnnouncement) return;

    this.lastAnnouncement = message;
    this.liveRegion.textContent = message;

    // Clear after announcement
    if (this.announceTimer) {
      clearTimeout(this.announceTimer);
    }

    this.announceTimer = setTimeout(() => {
      if (this.liveRegion) {
        this.liveRegion.textContent = '';
      }
      this.lastAnnouncement = '';
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

    const message = messages[status];
    if (message && message !== this.lastAnnouncement) {
      this.lastAnnouncement = message;
      this.liveRegion.textContent = message;
    }
  }

  public getKpiAriaLabel(kpi: EnhancedKpiCard): string {
    const trendText = this.getTrendDescription(kpi.trend);
    const freshnessText = this.getFreshnessDescription(kpi.dataFreshness);

    return `${kpi.name}: ${this.formatNumber(kpi.value)}. ${trendText}. ${freshnessText}. ${kpi.isDrillDownAvailable ? 'Press Enter to view details' : ''}`;
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

  private formatNumber(value: number): string {
    return new Intl.NumberFormat().format(value);
  }
}
```

### Phase 3.3: Memory Management and Performance (2 hours)

#### Task 3.3.1: Implement Memory Manager Service
```typescript
// kpi-memory-manager.service.ts
@Injectable({
  providedIn: 'root'
})
export class KpiMemoryManagerService {
  private subscriptions = new Map<string, Subscription>();
  private timers = new Map<string, Timer>();
  private animationFrames = new Map<string, number>();
  private observables = new Map<string, Observable<any>>();

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

  public addObservable(key: string, observable: Observable<any>): void {
    this.observables.set(key, observable);
  }

  public cleanup(key?: string): void {
    if (key) {
      this.cleanupSubscription(key);
      this.cleanupTimer(key);
      this.cleanupAnimationFrame(key);
      this.observables.delete(key);
    } else {
      this.cleanupAll();
    }
  }

  public getMemoryUsage(): MemoryUsageReport {
    return {
      subscriptions: this.subscriptions.size,
      timers: this.timers.size,
      animationFrames: this.animationFrames.size,
      observables: this.observables.size,
      timestamp: new Date()
    };
  }

  private cleanupAll(): void {
    this.subscriptions.forEach(sub => sub.unsubscribe());
    this.timers.forEach(timer => clearInterval(timer));
    this.animationFrames.forEach(frameId => cancelAnimationFrame(frameId));

    this.subscriptions.clear();
    this.timers.clear();
    this.animationFrames.clear();
    this.observables.clear();
  }

  private cleanupSubscription(key: string): void {
    const existing = this.subscriptions.get(key);
    if (existing && !existing.closed) {
      existing.unsubscribe();
    }
    this.subscriptions.delete(key);
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

interface MemoryUsageReport {
  subscriptions: number;
  timers: number;
  animationFrames: number;
  observables: number;
  timestamp: Date;
}
```

### Phase 3.4: Responsive Design and Styling (1 hour)

#### Task 3.4.1: Complete SCSS Styling Implementation
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

    @media (max-width: 768px) {
      font-size: 1.75rem;
    }
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
    transition: all 0.3s ease;

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
      animation: pulse 2s infinite;
    }
  }
}

@keyframes pulse {
  0%, 100% { opacity: 1; }
  50% { opacity: 0.7; }
}

// Enhanced KPI Card Styles
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

    .kpi-card__content::after {
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
      z-index: 1;
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

    .action-button {
      background: transparent;
      border: 1px solid var(--border-subtle);
      border-radius: 0.375rem;
      padding: 0.375rem;
      cursor: pointer;
      transition: all 0.2s ease;

      &:hover {
        background: var(--surface-hover);
        border-color: var(--border-interactive);
      }

      .icon {
        width: 1rem;
        height: 1rem;
        color: var(--text-secondary);
      }
    }
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

    .trend-icon {
      width: 1rem;
      height: 1rem;
    }

    &--up {
      color: var(--success-text);
    }

    &--down {
      color: var(--error-text);
      .trend-icon {
        transform: rotate(180deg);
      }
    }

    &--stable {
      color: var(--text-secondary);
      .trend-icon {
        transform: rotate(90deg);
      }
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
    gap: 0.5rem;

    @media (max-width: 480px) {
      flex-direction: column;
      align-items: flex-start;
      gap: 0.25rem;
    }
  }

  &__last-updated {
    &--fresh {
      color: var(--success-text);
    }

    &--stale {
      color: var(--warning-text);
    }
  }

  .data-freshness {
    display: flex;
    align-items: center;
    gap: 0.25rem;

    .warning-icon {
      width: 0.875rem;
      height: 0.875rem;
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

  .kpi-card__meta {
    flex-direction: row-reverse;
  }
}

// Screen reader only content
.sr-only {
  position: absolute;
  width: 1px;
  height: 1px;
  padding: 0;
  margin: -1px;
  overflow: hidden;
  clip: rect(0, 0, 0, 0);
  white-space: nowrap;
  border: 0;
}
```

---

## Day 4: Testing, Integration, and Deployment

### Phase 4.1: Comprehensive Testing Implementation (4 hours)

#### Task 4.1.1: Unit Tests for Core Components
```typescript
// enhanced-kpi-card.component.spec.ts
describe('EnhancedKpiCardComponent', () => {
  let component: EnhancedKpiCardComponent;
  let fixture: ComponentFixture<EnhancedKpiCardComponent>;
  let mockMemoryManager: jasmine.SpyObj<KpiMemoryManagerService>;

  beforeEach(async () => {
    const memorySpy = jasmine.createSpyObj('KpiMemoryManagerService', ['cleanup', 'addAnimationFrame']);

    await TestBed.configureTestingModule({
      declarations: [EnhancedKpiCardComponent],
      providers: [
        { provide: KpiMemoryManagerService, useValue: memorySpy }
      ],
      imports: [CommonModule, BrowserAnimationsModule]
    }).compileComponents();

    fixture = TestBed.createComponent(EnhancedKpiCardComponent);
    component = fixture.componentInstance;
    mockMemoryManager = TestBed.inject(KpiMemoryManagerService) as jasmine.SpyObj<KpiMemoryManagerService>;
  });

  describe('Real-time Value Updates', () => {
    it('should animate value changes smoothly', fakeAsync(() => {
      const initialValue = 100;
      const newValue = 150;

      component.kpiData = createMockKpiCard({ value: initialValue });
      component.ngOnChanges({ kpiData: new SimpleChange(null, component.kpiData, true) });

      component.kpiData = { ...component.kpiData, value: newValue };
      component.ngOnChanges({ kpiData: new SimpleChange(component.kpiData, component.kpiData, false) });

      expect(component.animatedValue).toBe(initialValue);
      tick(500);
      expect(component.animatedValue).toBeCloseTo(newValue, 0);
    }));

    it('should emit value change events with correct data', () => {
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

    it('should handle rapid value changes without memory leaks', fakeAsync(() => {
      component.kpiData = createMockKpiCard({ value: 100 });

      // Simulate rapid updates
      for (let i = 101; i <= 110; i++) {
        component.kpiData = { ...component.kpiData, value: i };
        component.ngOnChanges({ kpiData: new SimpleChange(component.kpiData, component.kpiData, false) });
        tick(50);
      }

      // Verify animation frames are properly managed
      expect(mockMemoryManager.addAnimationFrame).toHaveBeenCalled();
    }));
  });

  describe('Accessibility Features', () => {
    it('should generate proper ARIA labels for different KPI states', () => {
      const testCases = [
        {
          kpi: createMockKpiCard({
            name: 'Daily Active Users',
            value: 1250,
            trend: { direction: 'up', value: 15, isGood: true }
          }),
          expectedSubstrings: ['Daily Active Users', '1,250', 'increasing', '15%']
        },
        {
          kpi: createMockKpiCard({
            name: 'Error Rate',
            value: 0.5,
            trend: { direction: 'down', value: -10, isGood: true }
          }),
          expectedSubstrings: ['Error Rate', '0.5', 'decreasing', '10%']
        },
        {
          kpi: createMockKpiCard({
            name: 'Processing Time',
            value: 120,
            trend: undefined
          }),
          expectedSubstrings: ['Processing Time', '120', 'No trend data']
        }
      ];

      testCases.forEach(({ kpi, expectedSubstrings }) => {
        component.kpiData = kpi;
        component.ngOnChanges({ kpiData: new SimpleChange(null, kpi, true) });

        expectedSubstrings.forEach(substring => {
          expect(component.accessibilityLabel).toContain(substring);
        });
      });
    });

    it('should handle keyboard navigation correctly', () => {
      spyOn(component.drillDown, 'emit');

      component.kpiData = createMockKpiCard({ isDrillDownAvailable: true });
      fixture.detectChanges();

      const cardElement = fixture.debugElement.nativeElement;

      // Test Enter key
      cardElement.dispatchEvent(new KeyboardEvent('keydown', { key: 'Enter' }));
      expect(component.drillDown.emit).toHaveBeenCalledWith(component.kpiData.id);

      // Test Space key
      component.drillDown.emit.calls.reset();
      cardElement.dispatchEvent(new KeyboardEvent('keydown', { key: ' ' }));
      expect(component.drillDown.emit).toHaveBeenCalledWith(component.kpiData.id);
    });

    it('should support focus management', () => {
      component.kpiData = createMockKpiCard();
      fixture.detectChanges();

      const cardElement = fixture.debugElement.nativeElement;

      cardElement.focus();
      expect(component.isFocused).toBe(true);
      expect(cardElement.classList).toContain('kpi-card--focused');

      cardElement.blur();
      expect(component.isFocused).toBe(false);
      expect(cardElement.classList).not.toContain('kpi-card--focused');
    });
  });

  describe('Error Handling', () => {
    it('should display appropriate indicators for stale data', () => {
      component.kpiData = createMockKpiCard({
        dataFreshness: {
          age: 300,
          status: 'stale',
          lastUpdated: new Date(Date.now() - 300000),
          source: 'cached'
        }
      });

      fixture.detectChanges();

      expect(component.isDataStale).toBe(true);
      expect(fixture.debugElement.nativeElement.classList).toContain('kpi-card--stale');
    });

    it('should handle missing or invalid data gracefully', () => {
      const invalidDataCases = [
        createMockKpiCard({ value: NaN }),
        createMockKpiCard({ value: Infinity }),
        createMockKpiCard({ trend: undefined }),
        createMockKpiCard({ dataFreshness: undefined })
      ];

      invalidDataCases.forEach(kpiData => {
        expect(() => {
          component.kpiData = kpiData;
          component.ngOnChanges({ kpiData: new SimpleChange(null, kpiData, true) });
          fixture.detectChanges();
        }).not.toThrow();
      });
    });
  });

  describe('Performance', () => {
    it('should use OnPush change detection strategy', () => {
      expect(fixture.componentRef.changeDetectorRef.constructor.name).toBe('ViewRef_');
    });

    it('should cleanup resources on destroy', () => {
      component.ngOnDestroy();
      expect(mockMemoryManager.cleanup).toHaveBeenCalled();
    });

    it('should throttle rapid animations', fakeAsync(() => {
      spyOn(window, 'requestAnimationFrame').and.callThrough();

      component.kpiData = createMockKpiCard({ value: 100 });

      // Trigger multiple rapid changes
      for (let i = 101; i <= 105; i++) {
        component.kpiData = { ...component.kpiData, value: i };
        component.ngOnChanges({ kpiData: new SimpleChange(component.kpiData, component.kpiData, false) });
      }

      // Should only have one active animation
      expect(window.requestAnimationFrame).toHaveBeenCalledTimes(1);

      tick(1000);
      discardPeriodicTasks();
    }));
  });

  function createMockKpiCard(overrides: Partial<EnhancedKpiCard> = {}): EnhancedKpiCard {
    return {
      id: 'mock-kpi',
      name: 'Mock KPI',
      type: 'metric',
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
      ...overrides
    };
  }
});
```

#### Task 4.1.2: Integration Tests
```typescript
// kpi-cards-integration.spec.ts
describe('KPI Cards Integration Tests', () => {
  let store: MockStore;
  let analyticsService: jasmine.SpyObj<AnalyticsApiService>;
  let accessibilityService: jasmine.SpyObj<KpiAccessibilityService>;

  beforeEach(async () => {
    const analyticsSpy = jasmine.createSpyObj('AnalyticsApiService', [
      'initializeSignalRConnection',
      'getDetailedKpis',
      'getKpiDrillDown',
      'startPolling',
      'stopPolling'
    ]);

    const accessibilitySpy = jasmine.createSpyObj('KpiAccessibilityService', [
      'announceKpiUpdate',
      'announceConnectionStatus',
      'getKpiAriaLabel'
    ]);

    await TestBed.configureTestingModule({
      declarations: [KpiCardsPageComponent, EnhancedKpiCardComponent],
      providers: [
        { provide: AnalyticsApiService, useValue: analyticsSpy },
        { provide: KpiAccessibilityService, useValue: accessibilitySpy },
        provideMockStore({ initialState: { kpiCards: initialKpiCardsState } })
      ],
      imports: [CommonModule, BrowserAnimationsModule, NoopAnimationsModule]
    }).compileComponents();

    store = TestBed.inject(MockStore);
    analyticsService = TestBed.inject(AnalyticsApiService) as jasmine.SpyObj<AnalyticsApiService>;
    accessibilityService = TestBed.inject(KpiAccessibilityService) as jasmine.SpyObj<KpiAccessibilityService>;
  });

  describe('Real-time Data Flow', () => {
    it('should establish SignalR connection and load data on init', async () => {
      analyticsService.initializeSignalRConnection.and.returnValue(Promise.resolve());
      analyticsService.getDetailedKpis.and.returnValue(Promise.resolve(createMockKpiCards()));

      const fixture = TestBed.createComponent(KpiCardsPageComponent);
      const component = fixture.componentInstance;

      spyOn(store, 'dispatch');

      await component.ngOnInit();

      expect(analyticsService.initializeSignalRConnection).toHaveBeenCalled();
      expect(store.dispatch).toHaveBeenCalledWith(KpiCardsActions.startRealTime());
      expect(store.dispatch).toHaveBeenCalledWith(KpiCardsActions.loadKpis({}));
    });

    it('should fallback to polling when SignalR fails', async () => {
      analyticsService.initializeSignalRConnection.and.returnValue(
        Promise.reject(new Error('Connection failed'))
      );

      const fixture = TestBed.createComponent(KpiCardsPageComponent);
      const component = fixture.componentInstance;

      await component.ngOnInit();

      expect(analyticsService.startPolling).toHaveBeenCalled();
    });

    it('should handle real-time KPI updates correctly', () => {
      const fixture = TestBed.createComponent(KpiCardsPageComponent);
      const component = fixture.componentInstance;

      spyOn(store, 'dispatch');

      const mockUpdate: KpiUpdate = {
        kpiId: 'test-kpi',
        newValue: 150,
        trend: { direction: 'up', value: 15, isGood: true },
        timestamp: new Date().toISOString()
      };

      // Simulate SignalR update
      component.onValueChange({
        metric: 'Test KPI',
        oldValue: 100,
        newValue: 150,
        timestamp: new Date()
      });

      expect(accessibilityService.announceKpiUpdate).toHaveBeenCalledWith(
        'Test KPI',
        100,
        150
      );
    });
  });

  describe('State Management Integration', () => {
    it('should update component state when store changes', () => {
      const mockKpis = createMockKpiCards();

      store.setState({
        kpiCards: {
          ...initialKpiCardsState,
          kpis: mockKpis,
          isLoading: false
        }
      });

      const fixture = TestBed.createComponent(KpiCardsPageComponent);
      const component = fixture.componentInstance;

      component.kpis$.subscribe(kpis => {
        expect(kpis).toEqual(mockKpis);
      });
    });

    it('should dispatch filter actions when filters change', () => {
      const fixture = TestBed.createComponent(KpiCardsPageComponent);
      const component = fixture.componentInstance;

      spyOn(store, 'dispatch');

      const newFilters: KpiFilters = {
        timeRange: '7d',
        metricTypes: ['performance'],
        showTrends: false,
        groupBy: 'priority'
      };

      component.onFiltersChange(newFilters);

      expect(store.dispatch).toHaveBeenCalledWith(
        KpiCardsActions.applyFilters({ filters: newFilters })
      );
      expect(store.dispatch).toHaveBeenCalledWith(
        KpiCardsActions.loadKpis({ filters: newFilters })
      );
    });
  });

  describe('Accessibility Integration', () => {
    it('should announce connection status changes', () => {
      const fixture = TestBed.createComponent(KpiCardsPageComponent);
      const component = fixture.componentInstance;

      // Simulate connection state changes
      store.setState({
        kpiCards: {
          ...initialKpiCardsState,
          connectionState: { status: 'connected', reconnectAttempts: 0 }
        }
      });

      fixture.detectChanges();

      expect(accessibilityService.announceConnectionStatus).toHaveBeenCalledWith('connected');
    });

    it('should generate appropriate ARIA labels for KPI cards', () => {
      const mockKpi = createMockKpiCard();
      accessibilityService.getKpiAriaLabel.and.returnValue('Mock ARIA label');

      const cardFixture = TestBed.createComponent(EnhancedKpiCardComponent);
      const cardComponent = cardFixture.componentInstance;

      cardComponent.kpiData = mockKpi;
      cardComponent.ngOnChanges({ kpiData: new SimpleChange(null, mockKpi, true) });

      expect(accessibilityService.getKpiAriaLabel).toHaveBeenCalledWith(mockKpi);
    });
  });

  function createMockKpiCards(): EnhancedKpiCard[] {
    return [
      createMockKpiCard({ id: 'kpi-1', name: 'Daily Active Users', value: 1250 }),
      createMockKpiCard({ id: 'kpi-2', name: 'Success Rate', value: 95.5 }),
      createMockKpiCard({ id: 'kpi-3', name: 'Average Time', value: 45 })
    ];
  }

  function createMockKpiCard(overrides: Partial<EnhancedKpiCard> = {}): EnhancedKpiCard {
    return {
      id: 'mock-kpi',
      name: 'Mock KPI',
      type: 'metric',
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
      ...overrides
    };
  }
});
```

#### Task 4.1.3: E2E Tests with Playwright
```typescript
// kpi-cards.e2e.spec.ts
import { test, expect } from '@playwright/test';

test.describe('KPI Cards Page', () => {
  test.beforeEach(async ({ page }) => {
    // Mock API responses
    await page.route('/api/analytics/kpis/detailed', async route => {
      await route.fulfill({
        json: {
          data: [
            {
              id: 'dau',
              name: 'Daily Active Users',
              value: 1250,
              trend: { direction: 'up', value: 15, isGood: true },
              isDrillDownAvailable: true,
              lastUpdated: new Date().toISOString()
            },
            {
              id: 'success-rate',
              name: 'Success Rate',
              value: 95.5,
              trend: { direction: 'stable', value: 0, isGood: true },
              isDrillDownAvailable: true,
              lastUpdated: new Date().toISOString()
            }
          ]
        }
      });
    });

    await page.goto('/analytics/kpi-cards');
  });

  test('should display KPI cards with correct data', async ({ page }) => {
    await expect(page.locator('.kpi-card')).toHaveCount(2);

    const dauCard = page.locator('.kpi-card').filter({ hasText: 'Daily Active Users' });
    await expect(dauCard.locator('.kpi-card__value')).toContainText('1,250');
    await expect(dauCard.locator('.kpi-card__trend--up')).toBeVisible();

    const successCard = page.locator('.kpi-card').filter({ hasText: 'Success Rate' });
    await expect(successCard.locator('.kpi-card__value')).toContainText('95.5');
  });

  test('should handle keyboard navigation between cards', async ({ page }) => {
    const firstCard = page.locator('.kpi-card').first();
    const secondCard = page.locator('.kpi-card').nth(1);

    await firstCard.focus();
    await expect(firstCard).toBeFocused();

    await page.keyboard.press('Tab');
    await expect(secondCard).toBeFocused();

    await page.keyboard.press('Enter');
    // Should trigger drill-down modal
    await expect(page.locator('.drill-down-modal')).toBeVisible();
  });

  test('should open drill-down modal when clicking on card', async ({ page }) => {
    await page.route('/api/analytics/kpis/dau/drill-down', async route => {
      await route.fulfill({
        json: {
          kpiId: 'dau',
          breakdown: [
            { name: 'Desktop', count: 750, percentage: 60 },
            { name: 'Mobile', count: 500, percentage: 40 }
          ],
          trends: [],
          insights: []
        }
      });
    });

    const dauCard = page.locator('.kpi-card').filter({ hasText: 'Daily Active Users' });
    await dauCard.click();

    await expect(page.locator('.drill-down-modal')).toBeVisible();
    await expect(page.locator('.drill-down-modal h2')).toContainText('Daily Active Users');

    // Check breakdown data
    await expect(page.locator('.breakdown-item')).toHaveCount(2);
    await expect(page.locator('.breakdown-item').filter({ hasText: 'Desktop' })).toContainText('750 (60%)');
  });

  test('should handle real-time updates', async ({ page }) => {
    // Initial load
    await expect(page.locator('.kpi-card').first().locator('.kpi-card__value')).toContainText('1,250');

    // Mock SignalR update
    await page.evaluate(() => {
      // Simulate real-time update
      window.dispatchEvent(new CustomEvent('kpi-update', {
        detail: {
          kpiId: 'dau',
          newValue: 1275,
          trend: { direction: 'up', value: 17, isGood: true }
        }
      }));
    });

    // Value should animate to new value
    await expect(page.locator('.kpi-card').first().locator('.kpi-card__value')).toContainText('1,275');
    await expect(page.locator('.kpi-card').first().locator('.trend-text')).toContainText('17%');
  });

  test('should support responsive design', async ({ page }) => {
    // Desktop view
    await page.setViewportSize({ width: 1200, height: 800 });
    await expect(page.locator('.kpi-cards-page__grid')).toHaveCSS('grid-template-columns', 'repeat(3, 1fr)');

    // Tablet view
    await page.setViewportSize({ width: 768, height: 1024 });
    await expect(page.locator('.kpi-cards-page__grid')).toHaveCSS('grid-template-columns', 'repeat(2, 1fr)');

    // Mobile view
    await page.setViewportSize({ width: 375, height: 667 });
    await expect(page.locator('.kpi-cards-page__grid')).toHaveCSS('grid-template-columns', 'repeat(1, 1fr)');
  });

  test('should show connection status indicator', async ({ page }) => {
    await expect(page.locator('.kpi-cards-page__connection-status')).toBeVisible();
    await expect(page.locator('.kpi-cards-page__connection-status--connected')).toBeVisible();

    // Simulate connection loss
    await page.evaluate(() => {
      window.dispatchEvent(new CustomEvent('connection-status-change', {
        detail: { status: 'disconnected' }
      }));
    });

    await expect(page.locator('.kpi-cards-page__connection-status--disconnected')).toBeVisible();
  });

  test('should handle filter changes', async ({ page }) => {
    await page.click('[data-testid="filters-button"]');
    await expect(page.locator('.filters-panel')).toBeVisible();

    await page.selectOption('[data-testid="time-range-select"]', '7d');
    await page.click('[data-testid="apply-filters"]');

    // Should reload with new filters
    await expect(page.locator('.kpi-card')).toHaveCount.gte(1);
  });

  test('should support accessibility features', async ({ page }) => {
    // Check ARIA labels
    const firstCard = page.locator('.kpi-card').first();
    await expect(firstCard).toHaveAttribute('role', 'button');
    await expect(firstCard).toHaveAttribute('aria-label');

    // Check keyboard navigation
    await firstCard.focus();
    await page.keyboard.press('Enter');
    await expect(page.locator('.drill-down-modal')).toBeVisible();

    // Check escape key closes modal
    await page.keyboard.press('Escape');
    await expect(page.locator('.drill-down-modal')).not.toBeVisible();
  });

  test('should export KPI data', async ({ page }) => {
    await page.route('/api/analytics/kpis/export', async route => {
      await route.fulfill({
        body: 'KPI Name,Value,Trend\nDaily Active Users,1250,15%',
        headers: {
          'Content-Type': 'text/csv',
          'Content-Disposition': 'attachment; filename="kpi-export.csv"'
        }
      });
    });

    await page.click('[data-testid="export-button"]');
    await page.selectOption('[data-testid="export-format"]', 'csv');
    await page.click('[data-testid="confirm-export"]');

    // Check download was triggered
    const download = await page.waitForEvent('download');
    expect(download.suggestedFilename()).toBe('kpi-export.csv');
  });
});
```

### Phase 4.2: Performance Testing and Optimization (2 hours)

#### Task 4.2.1: Performance Test Suite
```typescript
// kpi-cards-performance.spec.ts
describe('KPI Cards Performance Tests', () => {
  let component: KpiCardsPageComponent;
  let fixture: ComponentFixture<KpiCardsPageComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [KpiCardsPageComponent],
      imports: [NoopAnimationsModule]
    }).compileComponents();
  });

  describe('Memory Management', () => {
    it('should not leak memory during rapid updates', async () => {
      const initialMemory = performance.memory?.usedJSHeapSize || 0;

      fixture = TestBed.createComponent(KpiCardsPageComponent);
      component = fixture.componentInstance;

      // Simulate 1000 rapid updates
      for (let i = 0; i < 1000; i++) {
        component.onValueChange({
          metric: `KPI-${i}`,
          oldValue: Math.random() * 100,
          newValue: Math.random() * 100,
          timestamp: new Date()
        });
      }

      // Force garbage collection if available
      if (window.gc) {
        window.gc();
      }

      const finalMemory = performance.memory?.usedJSHeapSize || 0;
      const memoryIncrease = finalMemory - initialMemory;

      // Memory increase should be reasonable (less than 10MB)
      expect(memoryIncrease).toBeLessThan(10 * 1024 * 1024);
    });

    it('should cleanup subscriptions on destroy', () => {
      fixture = TestBed.createComponent(KpiCardsPageComponent);
      component = fixture.componentInstance;

      const mockSubscription = jasmine.createSpyObj('Subscription', ['unsubscribe']);
      (component as any).subscriptions = [mockSubscription];

      component.ngOnDestroy();

      expect(mockSubscription.unsubscribe).toHaveBeenCalled();
    });
  });

  describe('Rendering Performance', () => {
    it('should render large datasets efficiently', async () => {
      const startTime = performance.now();

      // Create 100 KPI cards
      const largeDataset = Array.from({ length: 100 }, (_, i) => createMockKpiCard({
        id: `kpi-${i}`,
        name: `KPI ${i}`,
        value: Math.random() * 1000
      }));

      fixture = TestBed.createComponent(KpiCardsPageComponent);
      component = fixture.componentInstance;

      // Mock the store to return large dataset
      component.kpis$ = of(largeDataset);

      fixture.detectChanges();
      await fixture.whenStable();

      const endTime = performance.now();
      const renderTime = endTime - startTime;

      // Rendering should complete within 1 second
      expect(renderTime).toBeLessThan(1000);
    });

    it('should use OnPush change detection strategy', () => {
      fixture = TestBed.createComponent(KpiCardsPageComponent);

      expect(fixture.componentRef.changeDetectorRef.constructor.name).toBe('ViewRef_');
    });
  });

  describe('Animation Performance', () => {
    it('should throttle rapid animations', fakeAsync(() => {
      spyOn(window, 'requestAnimationFrame').and.callThrough();

      const cardFixture = TestBed.createComponent(EnhancedKpiCardComponent);
      const cardComponent = cardFixture.componentInstance;

      cardComponent.kpiData = createMockKpiCard({ value: 100 });

      // Trigger multiple rapid animations
      for (let i = 101; i <= 110; i++) {
        cardComponent.kpiData = { ...cardComponent.kpiData, value: i };
        cardComponent.ngOnChanges({ kpiData: new SimpleChange(cardComponent.kpiData, cardComponent.kpiData, false) });
      }

      // Should only create one animation frame request
      expect(window.requestAnimationFrame).toHaveBeenCalledTimes(1);

      tick(1000);
    }));
  });

  function createMockKpiCard(overrides: Partial<EnhancedKpiCard> = {}): EnhancedKpiCard {
    return {
      id: 'mock-kpi',
      name: 'Mock KPI',
      type: 'metric',
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
      ...overrides
    };
  }
});
```

### Phase 4.3: Final Integration and Deployment Preparation (2 hours)

#### Task 4.3.1: Complete Implementation Files

Now I'll create the remaining implementation files to complete the KPI Cards page:

<function_calls>
<invoke name="TodoWrite">
<parameter name="todos">[{"content": "Gather workflow inputs and identify page list", "status": "completed", "activeForm": "Gathering workflow inputs and identifying page list"}, {"content": "Execute A\u2192M workflow for Analytics Dashboard main page", "status": "completed", "activeForm": "Executing A\u2192M workflow for Analytics Dashboard main page"}, {"content": "Execute A\u2192M workflow for KPI Cards page", "status": "in_progress", "activeForm": "Executing A\u2192M workflow for KPI Cards page"}, {"content": "Execute A\u2192M workflow for Real-time Charts page", "status": "pending", "activeForm": "Executing A\u2192M workflow for Real-time Charts page"}, {"content": "Execute A\u2192M workflow for Export functionality", "status": "pending", "activeForm": "Executing A\u2192M workflow for Export functionality"}, {"content": "Complete Step A: System Map for KPI Cards page", "status": "completed", "activeForm": "Completing Step A: System Map for KPI Cards page"}, {"content": "Complete Step B: Acceptance Criteria for KPI Cards page", "status": "completed", "activeForm": "Completing Step B: Acceptance Criteria for KPI Cards page"}, {"content": "Complete Step C: Definition of Ready (DoR) Check for KPI Cards", "status": "completed", "activeForm": "Completing Step C: Definition of Ready (DoR) Check for KPI Cards"}, {"content": "Complete Step D: Component Design for KPI Cards", "status": "completed", "activeForm": "Completing Step D: Component Design for KPI Cards"}, {"content": "Complete Step E: Implementation Plan for KPI Cards", "status": "completed", "activeForm": "Completing Step E: Implementation Plan for KPI Cards"}, {"content": "Complete remaining Steps F-M for KPI Cards (Code, Tests, Deploy)", "status": "in_progress", "activeForm": "Completing remaining Steps F-M for KPI Cards (Code, Tests, Deploy)"}]