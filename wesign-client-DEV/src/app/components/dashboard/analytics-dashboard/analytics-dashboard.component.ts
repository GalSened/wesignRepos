import { Component, OnInit, OnDestroy } from '@angular/core';
import { interval, Subscription, combineLatest, merge } from 'rxjs';
import { finalize, takeUntil, filter, debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { Subject } from 'rxjs';
import { AnalyticsApiService, HealthResponse } from '@services/analytics-api.service';
import { AnalyticsLoadingService, LoadingState } from '@services/analytics-loading.service';
import { AnalyticsErrorHandlerService } from '@services/analytics-error-handler.service';
import { SharedService } from '@services/shared.service';
import {
  DashboardKPIs,
  UsageAnalytics,
  SegmentationData,
  ProcessFlowData,
  AnalyticsFilterRequest
} from '@models/analytics/analytics-models';

@Component({
  selector: 'sgn-analytics-dashboard',
  templateUrl: './analytics-dashboard.component.html',
  styleUrls: ['./analytics-dashboard.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
  animations: [
    trigger('valueChange', [
      transition(':increment', [
        style({ transform: 'scale(1.1)', color: 'var(--success-color)' }),
        animate('300ms ease-out', style({ transform: 'scale(1)', color: '*' }))
      ]),
      transition(':decrement', [
        style({ transform: 'scale(1.1)', color: 'var(--danger-color)' }),
        animate('300ms ease-out', style({ transform: 'scale(1)', color: '*' }))
      ])
    ]),
    trigger('connectionPulse', [
      state('connected', style({ opacity: 1 })),
      state('disconnected', style({ opacity: 0.5 })),
      transition('* => connected', [
        animate('500ms ease-in', keyframes([
          style({ opacity: 0.5, offset: 0 }),
          style({ opacity: 1, transform: 'scale(1.05)', offset: 0.5 }),
          style({ opacity: 1, transform: 'scale(1)', offset: 1 })
        ]))
      ])
    ])
  ]
})
export class AnalyticsDashboardComponent implements OnInit, OnDestroy {
  // Enhanced data properties with new models
  kpiData$ = new BehaviorSubject<KpiSnapshot | null>(null);
  usageAnalytics$ = new BehaviorSubject<UsageAnalytics | null>(null);
  segmentationData$ = new BehaviorSubject<SegmentationData | null>(null);
  processFlowData$ = new BehaviorSubject<ProcessFlowData | null>(null);

  // Real-time state management
  connectionState$ = new BehaviorSubject<ConnectionState>({ status: 'disconnected', reconnectAttempts: 0 });
  dataFreshness$ = new BehaviorSubject<DataFreshness>({ 
    age: 0, 
    status: 'fresh', 
    lastUpdated: new Date(),
    source: 'cached'
  });
  healthStatus$ = new BehaviorSubject<HealthStatus | null>(null);
  isLoading$ = new BehaviorSubject<boolean>(true);

  // User controls
  selectedTimeRange: TimeRange = '24h';
  selectedOrganization = 'all';
  autoRefreshEnabled = true;
  realtimeEnabled = true;

  // Error handling
  hasErrors = false;
  errorMessage = '';

  // Cleanup
  private destroy$ = new Subject<void>();
  private refreshInterval$ = new Subject<void>();

  constructor(
    private analyticsApiService: AnalyticsApiService,
    private loadingService: AnalyticsLoadingService,
    private errorHandler: AnalyticsErrorHandlerService,
    private sharedService: SharedService,
    private cdr: ChangeDetectorRef,
    private translateService: TranslateService,
    private liveAnnouncer: LiveAnnouncer
  ) {}

  async ngOnInit(): Promise<void> {
    await this.initializeComponent();
    this.setupAutoRefresh();
    this.setupRealtimeFeatures();
    this.monitorDataFreshness();
    this.setupKeyboardShortcuts();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    this.analyticsApiService.disconnect();
  }

  private async initializeComponent(): Promise<void> {
    try {
      this.isLoading$.next(true);
      
      // Load initial dashboard data
      await this.loadDashboardData();

      // Initialize real-time connections if enabled
      if (this.realtimeEnabled) {
        await this.analyticsApiService.initializeSignalRConnection();
      }

      this.isLoading$.next(false);
      this.hasErrors = false;
      
    } catch (error) {
      this.handleInitializationError(error);
    }
  }

  private async loadDashboardData(): Promise<void> {
    const filters: AnalyticsFilters = {
      timeRange: this.selectedTimeRange,
      organizationId: this.selectedOrganization === 'all' ? undefined : this.selectedOrganization
    };

    try {
      // Load all dashboard components in parallel
      const [kpiData, usageData, segmentationData, processFlowData] = await Promise.all([
        this.analyticsApiService.getLatestKPIs(filters).toPromise(),
        this.analyticsApiService.getUsageAnalytics(filters).toPromise(),
        this.analyticsApiService.getSegmentationData(filters).toPromise(),
        this.analyticsApiService.getProcessFlowData(filters).toPromise()
      ]);

      // Update all data streams
      this.kpiData$.next(kpiData);
      this.usageAnalytics$.next(usageData);
      this.segmentationData$.next(segmentationData);
      this.processFlowData$.next(processFlowData);

      // Update data freshness
      this.updateDataFreshness(kpiData?.timestamp);
      
      console.log('Dashboard data loaded successfully');
      
    } catch (error) {
      console.error('Failed to load dashboard data:', error);
      this.handleDataLoadError(error);
    }
  }

  private setupAutoRefresh(): void {
    // Auto-refresh every 30 seconds when enabled and not using real-time
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

  private setupRealtimeFeatures(): void {
    // Subscribe to real-time updates
    this.analyticsApiService.getRealtimeUpdates()
      .pipe(
        takeUntil(this.destroy$),
        filter(update => this.realtimeEnabled)
      )
      .subscribe(update => this.handleRealtimeUpdate(update));

    // Monitor connection state
    this.analyticsApiService.getConnectionState()
      .pipe(takeUntil(this.destroy$))
      .subscribe(state => {
        this.connectionState$.next(state);
        this.cdr.markForCheck();
      });

    // Monitor data freshness
    this.analyticsApiService.getDataFreshness()
      .pipe(takeUntil(this.destroy$))
      .subscribe(freshness => {
        this.dataFreshness$.next(freshness);
        this.cdr.markForCheck();
      });

    // Monitor health status
    this.analyticsApiService.getHealthStatusStream()
      .pipe(takeUntil(this.destroy$))
      .subscribe(health => {
        this.healthStatus$.next(health);
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
      case 'insights_update':
        // Handle insights updates when insights component is implemented
        break;
    }

    this.updateDataFreshness(update.timestamp);
    this.cdr.markForCheck();
  }

  private mergeKpiUpdate(newData: Partial<KpiSnapshot>): void {
    const currentData = this.kpiData$.value;
    if (!currentData) return;

    // Store old values for animation
    const oldValues = {
      dau: currentData.dau,
      mau: currentData.mau,
      successRate: currentData.successRate,
      avgTimeToSign: currentData.avgTimeToSign
    };

    // Merge the updates
    const mergedData = { ...currentData, ...newData };
    this.kpiData$.next(mergedData);

    // Trigger animations and announcements for changed values
    this.triggerValueChangeAnimations(oldValues, mergedData);
  }

  private triggerValueChangeAnimations(oldValues: any, newValues: KpiSnapshot): void {
    Object.keys(oldValues).forEach(key => {
      if (oldValues[key] !== newValues[key]) {
        // Announce changes for screen readers
        this.announceKpiChange(key, oldValues[key], newValues[key]);
        
        // Trigger visual animations
        const element = document.querySelector(`[data-metric="${key}"]`);
        if (element) {
          element.classList.add('value-changed');
          setTimeout(() => element.classList.remove('value-changed'), 300);
        }
      }
    });
  }

  private announceKpiChange(metric: string, oldValue: number, newValue: number): void {
    const change = newValue - oldValue;
    const direction = change > 0 ? 'increased' : 'decreased';
    const announcement = this.translateService.instant('ANALYTICS.ANNOUNCEMENTS.KPI_CHANGE', {
      metric: this.translateService.instant(`ANALYTICS.KPI.${metric.toUpperCase()}`),
      direction: this.translateService.instant(`ANALYTICS.DIRECTION.${direction.toUpperCase()}`),
      change: Math.abs(change)
    });

    this.liveAnnouncer.announce(announcement);
  }

  private monitorDataFreshness(): void {
    interval(5000) // Check every 5 seconds
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        const currentData = this.kpiData$.value;
        if (currentData?.timestamp) {
          this.updateDataFreshness(currentData.timestamp);
        }
      });
  }

  private updateDataFreshness(timestamp: string): void {
    if (!timestamp) return;

    const dataTime = new Date(timestamp);
    const now = new Date();
    const age = Math.floor((now.getTime() - dataTime.getTime()) / 1000);

    let status: 'fresh' | 'stale' | 'error' = 'fresh';
    if (age > 300) { // 5 minutes
      status = 'error';
    } else if (age > 90) { // 90 seconds
      status = 'stale';
    }

    this.dataFreshness$.next({
      age,
      status,
      lastUpdated: dataTime,
      source: this.connectionState$.value.status === 'connected' ? 'realtime' : 'polling'
    });
  }

  private setupKeyboardShortcuts(): void {
    // Will be handled by @HostListener in the template
  }

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

  // Event handlers
  onTimeRangeChange(timeRange: TimeRange): void {
    this.selectedTimeRange = timeRange;
    this.refreshInterval$.next();
  }

  onOrganizationChange(orgId: string): void {
    this.selectedOrganization = orgId;
    this.refreshInterval$.next();
  }

  onRefreshClick(): void {
    this.refreshInterval$.next();
  }

  toggleAutoRefresh(): void {
    this.autoRefreshEnabled = !this.autoRefreshEnabled;
    const message = this.autoRefreshEnabled ? 
      this.translateService.instant('ANALYTICS.MESSAGES.AUTO_REFRESH_ENABLED') :
      this.translateService.instant('ANALYTICS.MESSAGES.AUTO_REFRESH_DISABLED');
    
    this.sharedService.setSuccessAlert(message);
  }

  toggleRealtimeUpdates(): void {
    this.realtimeEnabled = !this.realtimeEnabled;
    
    if (this.realtimeEnabled) {
      this.analyticsApiService.initializeSignalRConnection();
    } else {
      this.analyticsApiService.disconnect();
    }

    const message = this.realtimeEnabled ?
      this.translateService.instant('ANALYTICS.MESSAGES.REALTIME_ENABLED') :
      this.translateService.instant('ANALYTICS.MESSAGES.REALTIME_DISABLED');
    
    this.sharedService.setSuccessAlert(message);
  }

  async forceReconnect(): Promise<void> {
    try {
      await this.analyticsApiService.reconnectSignalR();
      this.sharedService.setSuccessAlert(
        this.translateService.instant('ANALYTICS.MESSAGES.RECONNECTED')
      );
    } catch (error) {
      this.sharedService.setErrorAlert(
        this.translateService.instant('ANALYTICS.MESSAGES.RECONNECT_FAILED')
      );
    }
  }

  async exportDashboard(format: ExportFormat): Promise<void> {
    try {
      this.isLoading$.next(true);

      const filters: ExportFilters = {
        timeRange: this.selectedTimeRange,
        organizationId: this.selectedOrganization === 'all' ? undefined : this.selectedOrganization,
        includeCharts: true,
        includeInsights: true,
        format
      };

      const blob = await this.analyticsApiService.exportDashboard(format, filters).toPromise();

      // Create download
      const url = window.URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.download = `analytics-dashboard-${this.selectedTimeRange}-${Date.now()}.${format}`;
      link.click();

      window.URL.revokeObjectURL(url);

      this.sharedService.setSuccessAlert(
        this.translateService.instant('ANALYTICS.EXPORT.SUCCESS', { format: format.toUpperCase() })
      );

    } catch (error) {
      console.error('Export failed:', error);
      this.sharedService.setErrorAlert(
        this.translateService.instant('ANALYTICS.EXPORT.ERROR')
      );
    } finally {
      this.isLoading$.next(false);
    }
  }

  showExportMenu(): void {
    // Focus on export dropdown for keyboard users
    const exportButton = document.querySelector('[data-testid="export-dropdown"]');
    if (exportButton) {
      (exportButton as HTMLElement).focus();
    }
  }

  onKpiValueChange(change: KpiValueChange): void {
    // Handle KPI value changes from child components
    console.log('KPI value changed:', change);
  }

  // Error handling
  private handleInitializationError(error: any): void {
    this.isLoading$.next(false);
    this.hasErrors = true;
    this.errorMessage = this.errorHandler.getUserFriendlyMessage(error);

    this.sharedService.setErrorAlert(
      this.translateService.instant('ANALYTICS.ERRORS.INITIALIZATION_FAILED')
    );

    console.error('Analytics dashboard initialization failed:', error);
  }

  private handleDataLoadError(error: any): void {
    this.hasErrors = true;
    this.errorMessage = this.errorHandler.getUserFriendlyMessage(error);
    
    this.dataFreshness$.next({
      age: 0,
      status: 'error',
      lastUpdated: new Date(),
      source: 'cached'
    });

    this.sharedService.setErrorAlert(
      this.translateService.instant('ANALYTICS.ERRORS.DATA_LOAD_FAILED')
    );
  }

  // Utility methods for template
  getDataAgeText(): string {
    const freshness = this.dataFreshness$.value;
    const seconds = freshness.age;
    
    if (seconds < 60) return `${seconds}s ago`;
    const minutes = Math.floor(seconds / 60);
    if (minutes < 60) return `${minutes}m ago`;
    const hours = Math.floor(minutes / 60);
    return `${hours}h ago`;
  }

  getDataAgeAriaLabel(): string {
    const freshness = this.dataFreshness$.value;
    return this.translateService.instant('ANALYTICS.ARIA.DATA_AGE', {
      age: this.getDataAgeText(),
      status: this.translateService.instant(`ANALYTICS.STATUS.${freshness.status.toUpperCase()}`)
    });
  }

  getDataAgeIcon(): string {
    const freshness = this.dataFreshness$.value;
    return freshness.status === 'fresh' ? 'check-circle' : 
           freshness.status === 'stale' ? 'clock' : 'alert-circle';
  }

  getDataAgeStatus(): string {
    return this.dataFreshness$.value.status;
  }

  getConnectionStatusText(): string {
    const state = this.connectionState$.value;
    const keys = {
      connected: 'ANALYTICS.CONNECTION.CONNECTED',
      disconnected: 'ANALYTICS.CONNECTION.DISCONNECTED',
      reconnecting: 'ANALYTICS.CONNECTION.RECONNECTING',
      error: 'ANALYTICS.CONNECTION.ERROR'
    };

    return this.translateService.instant(keys[state.status] || keys.error);
  }

  getConnectionStatusClass(): string {
    return `connection-${this.connectionState$.value.status}`;
  }

  getConnectionIcon(): string {
    const state = this.connectionState$.value;
    return state.status === 'connected' ? 'wifi' : 'wifi-off';
  }

  getHealthStatusText(): string {
    const health = this.healthStatus$.value;
    if (!health) return this.translateService.instant('ANALYTICS.HEALTH.CHECKING');

    const keys = {
      healthy: 'ANALYTICS.HEALTH.HEALTHY',
      warning: 'ANALYTICS.HEALTH.WARNING',
      critical: 'ANALYTICS.HEALTH.CRITICAL'
    };

    return this.translateService.instant(keys[health.status] || keys.critical);
  }

  getHealthStatusClass(): string {
    const health = this.healthStatus$.value;
    return health ? `health-${health.status}` : 'health-unknown';
  }

  getHealthIcon(): string {
    const health = this.healthStatus$.value;
    return health?.status === 'healthy' ? 'check-circle' : 'alert-circle';
  }
}