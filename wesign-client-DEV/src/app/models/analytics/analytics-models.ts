// Analytics Data Models for WeSign Product Manager Dashboard

export class DashboardKPIs {
    // Active Users
    dailyActiveUsers: number = 0;
    weeklyActiveUsers: number = 0;
    monthlyActiveUsers: number = 0;

    // Document Metrics
    documentsCreated: number = 0;
    documentsSent: number = 0;
    documentsOpened: number = 0;
    documentsSigned: number = 0;
    documentsDeclined: number = 0;
    documentsExpired: number = 0;

    // Conversion Rates (%)
    sentToOpenedRate: number = 0;
    openedToSignedRate: number = 0;
    overallSuccessRate: number = 0;
    abandonmentRate: number = 0;

    // Time Metrics (in minutes)
    averageTimeToSign: number = 0;
    medianTimeToSign: number = 0;
    p95TimeToSign: number = 0;

    // Trends (compared to previous period)
    dauTrend: TrendIndicator = new TrendIndicator();
    mauTrend: TrendIndicator = new TrendIndicator();
    successRateTrend: TrendIndicator = new TrendIndicator();
    timeToSignTrend: TrendIndicator = new TrendIndicator();
}

export class TrendIndicator {
    value: number = 0;          // Percentage change
    direction: 'up' | 'down' | 'stable' = 'stable';
    isGood: boolean = true;     // Whether the trend is positive for business
}

export class UsageAnalytics {
    // Time series data for charts
    documentCreatedSeries: TimeSeriesPoint[] = [];
    documentSentSeries: TimeSeriesPoint[] = [];
    documentSignedSeries: TimeSeriesPoint[] = [];
    userActivitySeries: TimeSeriesPoint[] = [];

    // Peak usage times
    peakHours: HourlyUsage[] = [];
    peakDays: DailyUsage[] = [];
}

export class TimeSeriesPoint {
    timestamp: Date;
    value: number;
    label?: string;

    constructor(timestamp: Date, value: number, label?: string) {
        this.timestamp = timestamp;
        this.value = value;
        this.label = label;
    }
}

export class HourlyUsage {
    hour: number;           // 0-23
    documentsCount: number;
    usersCount: number;
}

export class DailyUsage {
    dayOfWeek: number;      // 0-6 (Sunday-Saturday)
    documentsCount: number;
    usersCount: number;
    averageTimeToSign: number;
}

export class SegmentationData {
    // Send Type Segmentation
    sendTypeBreakdown: SegmentBreakdown[] = [];

    // Organization Segmentation
    organizationBreakdown: OrganizationSegment[] = [];

    // Template Usage
    templateUsage: TemplateUsageData[] = [];

    // Device/Platform Segmentation
    deviceBreakdown: SegmentBreakdown[] = [];

    // Geographic Distribution (if available)
    geographicBreakdown: GeographicSegment[] = [];
}

export class SegmentBreakdown {
    name: string;
    count: number;
    percentage: number;
    successRate: number;    // Completion rate for this segment
    averageTimeToSign: number;
    color?: string;         // For chart visualization
}

export class OrganizationSegment {
    organizationId: string;
    organizationName: string;
    documentsCount: number;
    usersCount: number;
    successRate: number;
    averageTimeToSign: number;
    isHighVolume: boolean;  // Top 20% by volume
    tier: 'enterprise' | 'business' | 'standard';
}

export class TemplateUsageData {
    templateId: string;
    templateName: string;
    usageCount: number;
    successRate: number;
    averageTimeToSign: number;
    abandonmentRate: number;
    lastUsed: Date;
    isPopular: boolean;     // Top 10 by usage
    complexity: 'simple' | 'medium' | 'complex';
}

export class GeographicSegment {
    country: string;
    countryCode: string;
    documentsCount: number;
    usersCount: number;
    averageTimeToSign: number;
    preferredLanguage: string;
}

export class ProcessFlowData {
    // Funnel Analysis
    funnelStages: FunnelStage[] = [];

    // Drop-off Analysis
    dropOffPoints: DropOffAnalysis[] = [];

    // Stuck Documents
    stuckDocuments: StuckDocumentInfo[] = [];

    // Stage Duration Analysis
    stageDurations: StageDurationData[] = [];
}

export class FunnelStage {
    stageName: string;
    stageOrder: number;
    documentsCount: number;
    conversionRate: number;     // % from previous stage
    dropOffRate: number;        // % that don't proceed to next stage
    averageTimeInStage: number; // Minutes spent in this stage
}

export class DropOffAnalysis {
    fromStage: string;
    toStage: string;
    dropOffCount: number;
    dropOffPercentage: number;
    commonReasons: string[];    // If available (declined reasons, etc.)
    recoverableCount: number;   // Documents that could potentially be recovered
}

export class StuckDocumentInfo {
    documentId: string;         // Hashed for PM privacy
    organizationName: string;
    templateName: string;
    currentStage: string;
    timeStuck: number;          // Hours
    stuckReason: string;
    isRecoverable: boolean;
    priorityLevel: 'high' | 'medium' | 'low';
}

export class StageDurationData {
    stageName: string;
    averageDuration: number;    // Minutes
    medianDuration: number;
    p95Duration: number;
    benchmarkDuration: number;  // Expected/ideal duration
    performanceIndicator: 'good' | 'warning' | 'poor';
}

// Request/Filter Models
export class AnalyticsFilterRequest {
    timeRange: string;          // '24h', '7d', '30d', '90d'
    fromDate: Date;
    toDate: Date;
    organizationId?: string;
    templateId?: string;
    deviceType?: string;
    sendType?: string;
    includeArchived: boolean = false;
    includeTestData: boolean = false;
}

// Business Insights Models
export class BusinessInsight {
    type: 'opportunity' | 'warning' | 'success' | 'trend';
    title: string;
    description: string;
    impact: 'high' | 'medium' | 'low';
    actionable: boolean;
    suggestedActions: string[];
    metrics: InsightMetric[];
    confidence: number;         // 0-100 confidence score
}

export class InsightMetric {
    name: string;
    value: number;
    unit: string;
    change: number;             // % change from previous period
    benchmark?: number;         // Industry or internal benchmark
}

// Dashboard Configuration
export class DashboardConfig {
    refreshInterval: number = 30;      // Seconds
    defaultTimeRange: string = '30d';
    enabledInsights: string[] = [];
    kpiThresholds: KPIThresholds = new KPIThresholds();
    displayOptions: DisplayOptions = new DisplayOptions();
}

export class KPIThresholds {
    minSuccessRate: number = 80;       // %
    maxAverageTimeToSign: number = 60; // Minutes
    maxAbandonmentRate: number = 15;   // %
    minDAUGrowth: number = 5;          // % month-over-month
}

export class DisplayOptions {
    showTrends: boolean = true;
    showBenchmarks: boolean = true;
    enableDarkMode: boolean = false;
    chartAnimations: boolean = true;
    realTimeUpdates: boolean = true;
}

// API Response Models
export class AnalyticsApiResponse<T> {
    data: T;
    timestamp: Date;
    cacheAge: number;           // Seconds
    dataQuality: 'fresh' | 'stale' | 'cached';
    metadata: ResponseMetadata;
}

export class ResponseMetadata {
    recordsProcessed: number;
    queryDuration: number;      // Milliseconds
    filters: AnalyticsFilterRequest;
    version: string;
}

// Real-time update models for SignalR
export interface RealtimeUpdate {
    type: 'kpi_update' | 'health_change' | 'connection_status' | 'insights_update';
    data: any;
    timestamp: string;
    targetRoles?: string[];
    organizationId?: string;
}

export interface ConnectionState {
    status: 'connected' | 'disconnected' | 'reconnecting' | 'error';
    lastConnected?: Date;
    reconnectAttempts: number;
    latency?: number;
    connectionId?: string;
}

export interface DataFreshness {
    age: number; // seconds since last update
    status: 'fresh' | 'stale' | 'error';
    lastUpdated: Date;
    nextUpdate?: Date;
    source: 'realtime' | 'polling' | 'cached';
}

export interface HealthStatus {
    status: 'healthy' | 'warning' | 'critical';
    services: {
        database: ServiceHealth;
        signalr: ServiceHealth;
        s3: ServiceHealth;
        analytics: ServiceHealth;
    };
    overallScore: number; // 0-100
    lastChecked: Date;
}

export interface ServiceHealth {
    status: 'healthy' | 'warning' | 'critical';
    responseTime: number; // milliseconds
    lastChecked: Date;
    errorRate: number; // percentage
    details?: string;
}

// Enhanced KPI snapshot with real-time metadata
export interface KpiSnapshot {
    timestamp: string;
    organizationId?: string; // For role-based filtering
    
    // Core metrics
    dau: number;
    mau: number;
    successRate: number;
    avgTimeToSign: number; // in seconds
    totalDocuments: number;
    activeOrganizations: number;
    
    // Trend data with sparklines
    trends: {
        [key: string]: EnhancedTrendData;
    };
    
    // Real-time metadata
    metadata: {
        dataAge: number;
        queryDuration: number;
        cacheHit: boolean;
        recordCount: number;
        freshness: 'fresh' | 'stale' | 'error';
    };
}

// Enhanced trend data with sparkline support
export interface EnhancedTrendData extends TrendIndicator {
    sparklineData: number[]; // Last 24 data points for mini charts
    confidence: number; // 0-100 confidence in the trend
    benchmarkValue?: number; // Industry or internal benchmark
    changePercent: number;
}

// Export functionality models
export type ExportFormat = 'csv' | 'excel' | 'pdf';

export interface ExportFilters {
    timeRange: TimeRange;
    organizationId?: string;
    includeCharts: boolean;
    includeInsights: boolean;
    format: ExportFormat;
}

export type TimeRange = '24h' | '7d' | '30d' | '90d';

// Analytics filters for API requests
export interface AnalyticsFilters {
    timeRange: TimeRange;
    organizationId?: string;
    documentTypes?: string[];
    userRoles?: string[];
    includeArchived?: boolean;
    includeTestData?: boolean;
}

// Animation trigger models
export interface KpiValueChange {
    metric: string;
    oldValue: number;
    newValue: number;
    timestamp: Date;
}

// Enhanced insights models
export interface AnalyticsInsight extends BusinessInsight {
    id: string;
    generatedAt: Date;
    validUntil?: Date;
    aiGenerated: boolean;
}

export interface TrendInsight extends AnalyticsInsight {
    trendDirection: 'up' | 'down' | 'stable';
    trendStrength: number; // 0-100
    forecastAccuracy: number; // 0-100
}

export interface AnomalyInsight extends AnalyticsInsight {
    anomalyScore: number; // 0-100, higher = more anomalous
    expectedValue: number;
    actualValue: number;
    possibleCauses: string[];
}
