import { Component, Input, OnInit, OnChanges, SimpleChanges, ChangeDetectionStrategy } from '@angular/core';
import { DashboardKPIs, TrendIndicator } from '@models/analytics/analytics-models';
import { trigger, state, style, transition, animate } from '@angular/animations';

@Component({
  selector: 'sgn-kpi-cards',
  templateUrl: './kpi-cards.component.html',
  styleUrls: ['./kpi-cards.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
  animations: [
    trigger('cardUpdate', [
      state('idle', style({ transform: 'scale(1)', backgroundColor: 'inherit' })),
      state('updated', style({ transform: 'scale(1.02)', backgroundColor: 'rgba(0, 123, 255, 0.1)' })),
      transition('idle => updated', animate('300ms ease-in')),
      transition('updated => idle', animate('500ms ease-out'))
    ]),
    trigger('valueCounter', [
      transition(':increment', [
        style({ color: '#28a745', transform: 'scale(1.1)' }),
        animate('600ms ease-out', style({ color: 'inherit', transform: 'scale(1)' }))
      ]),
      transition(':decrement', [
        style({ color: '#dc3545', transform: 'scale(1.1)' }),
        animate('600ms ease-out', style({ color: 'inherit', transform: 'scale(1)' }))
      ])
    ]),
    trigger('sparklineUpdate', [
      transition('* => *', [
        query('.sparkline-path', [
          style({ strokeDashoffset: '100%' }),
          animate('800ms ease-out', style({ strokeDashoffset: '0%' }))
        ], { optional: true })
      ])
    ]),
    trigger('connectionPulse', [
      state('connected', style({ opacity: 1, transform: 'scale(1)' })),
      state('disconnected', style({ opacity: 0.7, transform: 'scale(0.98)' })),
      state('error', style({ opacity: 0.5, transform: 'scale(0.95)' })),
      transition('* => connected', [
        animate('300ms ease-in', style({ opacity: 1, transform: 'scale(1.02)' })),
        animate('200ms ease-out', style({ opacity: 1, transform: 'scale(1)' }))
      ]),
      transition('* => disconnected', animate('300ms ease-out')),
      transition('* => error', animate('300ms ease-out'))
    ])
  ]
})
export class KpiCardsComponent implements OnInit, OnChanges, OnDestroy {
  @Input() kpiData: KpiSnapshot | null = null;
  @Input() timeRange: TimeRange = '24h';
  @Input() connectionState: ConnectionState | null = null;
  @Input() dataFreshness: DataFreshness | null = null;

  @Output() kpiValueChange = new EventEmitter<KpiValueChange>();

  // Enhanced KPI cards with sparkline support
  kpiCards: EnhancedKpiCard[] = [];
  previousValues: { [key: string]: number } = {};
  cardStates: { [key: string]: 'idle' | 'updated' } = {};
  animationQueue: KpiValueChange[] = [];

  // Component lifecycle
  private destroy$ = new Subject<void>();
  private animationInterval?: number;

  constructor(
    private cdr: ChangeDetectorRef,
    private translateService: TranslateService,
    private liveAnnouncer: LiveAnnouncer
  ) {}

  ngOnInit(): void {
    this.buildKpiCards();
    this.initializePreviousValues();
    this.setupAnimationQueue();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['kpiData'] && !changes['kpiData'].isFirstChange()) {
      this.detectValueChanges();
    }
    this.buildKpiCards();
    this.cdr.markForCheck();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    
    if (this.animationInterval) {
      clearInterval(this.animationInterval);
    }
  }

  private initializePreviousValues(): void {
    if (!this.kpiData) return;

    this.previousValues = {
      dau: this.kpiData.dau,
      mau: this.kpiData.mau,
      successRate: this.kpiData.successRate,
      avgTimeToSign: this.kpiData.avgTimeToSign,
      totalDocuments: this.kpiData.totalDocuments,
      activeOrganizations: this.kpiData.activeOrganizations
    };
  }

  private detectValueChanges(): void {
    if (!this.kpiData) return;

    const changes: KpiValueChange[] = [];
    const currentValues = {
      dau: this.kpiData.dau,
      mau: this.kpiData.mau,
      successRate: this.kpiData.successRate,
      avgTimeToSign: this.kpiData.avgTimeToSign,
      totalDocuments: this.kpiData.totalDocuments,
      activeOrganizations: this.kpiData.activeOrganizations
    };

    Object.keys(currentValues).forEach(key => {
      const currentValue = currentValues[key];
      const previousValue = this.previousValues[key];

      if (currentValue !== previousValue && previousValue !== undefined) {
        const change: KpiValueChange = {
          metric: key,
          oldValue: previousValue,
          newValue: currentValue,
          timestamp: new Date()
        };

        changes.push(change);
        this.previousValues[key] = currentValue;

        // Trigger card update animation
        const cardId = this.getCardIdForMetric(key);
        if (cardId) {
          this.triggerCardAnimation(cardId);
          this.announceValueChange(change);
        }
      }
    });

    // Emit changes to parent component
    changes.forEach(change => {
      this.kpiValueChange.emit(change);
      this.animationQueue.push(change);
    });

    if (changes.length > 0) {
      console.log('KPI values updated:', changes);
    }
  }

  private getCardIdForMetric(metric: string): string | null {
    const mapping: { [key: string]: string } = {
      dau: 'dau',
      mau: 'mau',
      successRate: 'success-rate',
      avgTimeToSign: 'time-to-sign',
      totalDocuments: 'total-documents',
      activeOrganizations: 'active-orgs'
    };
    return mapping[metric] || null;
  }

  private triggerCardAnimation(cardId: string): void {
    this.cardStates[cardId] = 'updated';
    setTimeout(() => {
      this.cardStates[cardId] = 'idle';
      this.cdr.markForCheck();
    }, 800);
  }

  private announceValueChange(change: KpiValueChange): void {
    const direction = change.newValue > change.oldValue ? 'increased' : 'decreased';
    const announcement = this.translateService.instant('ANALYTICS.ANNOUNCEMENTS.KPI_CHANGE', {
      metric: this.translateService.instant(`ANALYTICS.KPI.${change.metric.toUpperCase()}`),
      direction: this.translateService.instant(`ANALYTICS.DIRECTION.${direction.toUpperCase()}`),
      oldValue: this.formatValueForAnnouncement(change.oldValue, change.metric),
      newValue: this.formatValueForAnnouncement(change.newValue, change.metric)
    });

    this.liveAnnouncer.announce(announcement);
  }

  private formatValueForAnnouncement(value: number, metric: string): string {
    switch (metric) {
      case 'successRate':
        return `${value.toFixed(1)} percent`;
      case 'avgTimeToSign':
        return this.formatTimeToSign(value);
      default:
        return this.formatNumber(value);
    }
  }

  private setupAnimationQueue(): void {
    // Process animation queue every 100ms for smooth animations
    this.animationInterval = window.setInterval(() => {
      if (this.animationQueue.length > 0) {
        this.animationQueue.shift(); // Remove processed animation
      }
    }, 100);
  }

  getCardAnimationState(cardId: string): string {
    return this.cardStates[cardId] || 'idle';
  }

  getConnectionAnimationState(): string {
    if (!this.connectionState) return 'disconnected';
    
    if (this.dataFreshness?.status === 'error') return 'error';
    return this.connectionState.status === 'connected' ? 'connected' : 'disconnected';
  }

  private buildKpiCards(): void {
    if (!this.kpiData) {
      this.kpiCards = [];
      return;
    }

    this.kpiCards = [
      {
        id: 'dau',
        title: this.translateService.instant('ANALYTICS.KPI.DAU'),
        value: this.kpiData.dau,
        format: 'number',
        icon: 'users',
        color: 'primary',
        trend: this.kpiData.trends?.dau,
        sparklineData: this.kpiData.trends?.dau?.sparklineData || [],
        description: this.translateService.instant('ANALYTICS.KPI.DAU_DESC'),
        benchmark: this.kpiData.trends?.dau?.benchmarkValue,
        confidence: this.kpiData.trends?.dau?.confidence || 100,
        ariaLabel: this.translateService.instant('ANALYTICS.ARIA.KPI_CARD', {
          title: this.translateService.instant('ANALYTICS.KPI.DAU'),
          value: this.formatNumber(this.kpiData.dau)
        })
      },
      {
        id: 'mau',
        title: this.translateService.instant('ANALYTICS.KPI.MAU'),
        value: this.kpiData.mau,
        format: 'number',
        icon: 'user-check',
        color: 'info',
        trend: this.kpiData.trends?.mau,
        sparklineData: this.kpiData.trends?.mau?.sparklineData || [],
        description: this.translateService.instant('ANALYTICS.KPI.MAU_DESC'),
        benchmark: this.kpiData.trends?.mau?.benchmarkValue,
        confidence: this.kpiData.trends?.mau?.confidence || 100,
        ariaLabel: this.translateService.instant('ANALYTICS.ARIA.KPI_CARD', {
          title: this.translateService.instant('ANALYTICS.KPI.MAU'),
          value: this.formatNumber(this.kpiData.mau)
        })
      },
      {
        id: 'success-rate',
        title: this.translateService.instant('ANALYTICS.KPI.SUCCESS_RATE'),
        value: this.kpiData.successRate,
        format: 'percentage',
        icon: 'check-circle',
        color: 'success',
        trend: this.kpiData.trends?.successRate,
        sparklineData: this.kpiData.trends?.successRate?.sparklineData || [],
        description: this.translateService.instant('ANALYTICS.KPI.SUCCESS_RATE_DESC'),
        benchmark: this.kpiData.trends?.successRate?.benchmarkValue,
        confidence: this.kpiData.trends?.successRate?.confidence || 100,
        ariaLabel: this.translateService.instant('ANALYTICS.ARIA.KPI_CARD', {
          title: this.translateService.instant('ANALYTICS.KPI.SUCCESS_RATE'),
          value: `${this.kpiData.successRate.toFixed(1)}%`
        })
      },
      {
        id: 'time-to-sign',
        title: this.translateService.instant('ANALYTICS.KPI.TIME_TO_SIGN'),
        value: this.kpiData.avgTimeToSign,
        format: 'time',
        icon: 'clock',
        color: 'warning',
        trend: this.kpiData.trends?.avgTimeToSign,
        sparklineData: this.kpiData.trends?.avgTimeToSign?.sparklineData || [],
        description: this.translateService.instant('ANALYTICS.KPI.TIME_TO_SIGN_DESC'),
        benchmark: this.kpiData.trends?.avgTimeToSign?.benchmarkValue,
        confidence: this.kpiData.trends?.avgTimeToSign?.confidence || 100,
        ariaLabel: this.translateService.instant('ANALYTICS.ARIA.KPI_CARD', {
          title: this.translateService.instant('ANALYTICS.KPI.TIME_TO_SIGN'),
          value: this.formatTimeToSign(this.kpiData.avgTimeToSign)
        })
      },
      {
        id: 'total-documents',
        title: this.translateService.instant('ANALYTICS.KPI.TOTAL_DOCUMENTS'),
        value: this.kpiData.totalDocuments,
        format: 'number',
        icon: 'file-text',
        color: 'primary',
        sparklineData: [],
        description: this.translateService.instant('ANALYTICS.KPI.TOTAL_DOCUMENTS_DESC', {
          timeRange: this.getTimeRangeText()
        }),
        confidence: 100,
        ariaLabel: this.translateService.instant('ANALYTICS.ARIA.KPI_CARD', {
          title: this.translateService.instant('ANALYTICS.KPI.TOTAL_DOCUMENTS'),
          value: this.formatNumber(this.kpiData.totalDocuments)
        })
      },
      {
        id: 'active-orgs',
        title: this.translateService.instant('ANALYTICS.KPI.ACTIVE_ORGS'),
        value: this.kpiData.activeOrganizations,
        format: 'number',
        icon: 'building',
        color: 'info',
        sparklineData: [],
        description: this.translateService.instant('ANALYTICS.KPI.ACTIVE_ORGS_DESC'),
        confidence: 100,
        ariaLabel: this.translateService.instant('ANALYTICS.ARIA.KPI_CARD', {
          title: this.translateService.instant('ANALYTICS.KPI.ACTIVE_ORGS'),
          value: this.formatNumber(this.kpiData.activeOrganizations)
        })
      }
    ];
  }

  private getTimeRangeText(): string {
    const translations = {
      '24h': 'ANALYTICS.TIME.24H',
      '7d': 'ANALYTICS.TIME.7D',
      '30d': 'ANALYTICS.TIME.30D',
      '90d': 'ANALYTICS.TIME.90D'
    };

    return this.translateService.instant(translations[this.timeRange] || translations['24h']);
  }

  formatValue(value: number, format: string): string {
    switch (format) {
      case 'number':
        return this.formatNumber(value);
      case 'percentage':
        return `${value.toFixed(1)}%`;
      case 'time':
        return this.formatTimeToSign(value);
      default:
        return value.toString();
    }
  }

  private formatNumber(value: number): string {
    if (value >= 1000000) {
      return `${(value / 1000000).toFixed(1)}M`;
    } else if (value >= 1000) {
      return `${(value / 1000).toFixed(1)}K`;
    }
    return value.toLocaleString();
  }

  private formatTimeToSign(seconds: number): string {
    const minutes = Math.floor(seconds / 60);
    const hours = Math.floor(minutes / 60);
    const days = Math.floor(hours / 24);

    if (days > 0) {
      const remainingHours = hours % 24;
      return remainingHours > 0 ? `${days}d ${remainingHours}h` : `${days}d`;
    } else if (hours > 0) {
      const remainingMinutes = minutes % 60;
      return remainingMinutes > 0 ? `${hours}h ${remainingMinutes}m` : `${hours}h`;
    } else if (minutes > 0) {
      return `${minutes}m`;
    } else {
      return `${seconds}s`;
    }
  }

  getTrendIcon(trend?: EnhancedTrendData): string {
    if (!trend) return 'minus';

    switch (trend.direction) {
      case 'up': return 'trending-up';
      case 'down': return 'trending-down';
      default: return 'minus';
    }
  }

  getTrendClass(trend?: EnhancedTrendData): string {
    if (!trend || trend.direction === 'stable') return 'trend-stable';
    return trend.isGood ? 'trend-positive' : 'trend-negative';
  }

  getTrendText(trend?: EnhancedTrendData): string {
    if (!trend || trend.direction === 'stable') {
      return this.translateService.instant('ANALYTICS.TREND.STABLE');
    }

    const sign = trend.direction === 'up' ? '+' : '';
    const percentage = `${sign}${trend.changePercent.toFixed(1)}%`;
    
    return this.translateService.instant('ANALYTICS.TREND.CHANGE', {
      percentage,
      direction: this.translateService.instant(`ANALYTICS.TREND.${trend.direction.toUpperCase()}`)
    });
  }

  getTrendAriaLabel(trend?: EnhancedTrendData): string {
    if (!trend) return this.translateService.instant('ANALYTICS.TREND.NO_DATA');

    const direction = this.translateService.instant(`ANALYTICS.TREND.${trend.direction.toUpperCase()}`);
    const confidence = this.translateService.instant('ANALYTICS.TREND.CONFIDENCE', {
      value: trend.confidence
    });

    return `${this.getTrendText(trend)}, ${direction}, ${confidence}`;
  }

  getCardStatusClass(card: EnhancedKpiCard): string {
    const classes = ['kpi-card'];

    // Connection status
    if (this.connectionState?.status === 'connected') {
      classes.push('status-realtime');
    } else {
      classes.push('status-polling');
    }

    // Data freshness
    if (this.dataFreshness) {
      classes.push(`freshness-${this.dataFreshness.status}`);
    }

    // Confidence level
    if (card.confidence < 70) {
      classes.push('confidence-low');
    } else if (card.confidence < 90) {
      classes.push('confidence-medium');
    } else {
      classes.push('confidence-high');
    }

    return classes.join(' ');
  }

  getSparklineViewBox(data: number[]): string {
    return `0 0 ${data.length * 4} 50`;
  }

  getSparklinePath(data: number[]): string {
    if (data.length === 0) return '';

    const max = Math.max(...data);
    const min = Math.min(...data);
    const range = max - min || 1;

    const points = data.map((value, index) => {
      const x = index * 4;
      const y = 50 - ((value - min) / range) * 40; // Normalize to 0-40, then flip for SVG
      return `${x},${y}`;
    });

    return `M ${points.join(' L ')}`;
  }

  getConnectionIndicatorClass(): string {
    if (!this.connectionState) return 'connection-indicator unknown';

    const classes = ['connection-indicator'];
    classes.push(this.connectionState.status);

    if (this.connectionState.latency !== undefined) {
      if (this.connectionState.latency < 100) {
        classes.push('latency-excellent');
      } else if (this.connectionState.latency < 300) {
        classes.push('latency-good');
      } else {
        classes.push('latency-poor');
      }
    }

    return classes.join(' ');
  }

  getDataFreshnessIndicator(): string {
    if (!this.dataFreshness) return '⚪';

    switch (this.dataFreshness.status) {
      case 'fresh': return '🟢';
      case 'stale': return '🟡';
      case 'error': return '🔴';
      default: return '⚪';
    }
  }

  getDataFreshnessText(): string {
    if (!this.dataFreshness) {
      return this.translateService.instant('ANALYTICS.FRESHNESS.UNKNOWN');
    }

    const key = `ANALYTICS.FRESHNESS.${this.dataFreshness.status.toUpperCase()}`;
    return this.translateService.instant(key);
  }

  getDataFreshnessAriaLabel(): string {
    if (!this.dataFreshness) {
      return this.translateService.instant('ANALYTICS.ARIA.FRESHNESS_UNKNOWN');
    }

    return this.translateService.instant('ANALYTICS.ARIA.FRESHNESS', {
      status: this.getDataFreshnessText(),
      age: this.dataFreshness.age,
      source: this.translateService.instant(`ANALYTICS.SOURCE.${this.dataFreshness.source.toUpperCase()}`)
    });
  }

  // Enhanced KPI card interface
  interface EnhancedKpiCard extends KpiCard {
    sparklineData: number[];
    benchmark?: number;
    confidence: number;
    ariaLabel: string;
  }
}

interface KpiCard {
  id: string;
  title: string;
  value: number;
  format: 'number' | 'percentage' | 'time';
  icon: string;
  color: 'primary' | 'success' | 'warning' | 'danger' | 'info';
  trend?: TrendIndicator;
  description: string;
}

// Enhanced KPI card interface with real-time features
interface EnhancedKpiCard extends KpiCard {
  sparklineData: number[];
  benchmark?: number;
  confidence: number;
  ariaLabel: string;
}