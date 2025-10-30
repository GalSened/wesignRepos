import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, of, BehaviorSubject, fromEvent } from 'rxjs';
import { map, catchError, retry, timeout, shareReplay, switchMap, startWith } from 'rxjs/operators';
import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { AppConfigService } from './app-config.service';
import { AnalyticsErrorHandlerService } from './analytics-error-handler.service';
import {
  DashboardKPIs,
  UsageAnalytics,
  SegmentationData,
  ProcessFlowData,
  AnalyticsFilterRequest,
  AnalyticsApiResponse,
  StuckDocumentInfo,
  TimeSeriesPoint
} from '@models/analytics/analytics-models';

@Injectable({ providedIn: 'root' })
export class AnalyticsApiService {
  private storage: Storage = sessionStorage || localStorage;
  private readonly JWT_TOKEN: string = 'JWT_TOKEN';
  private analyticsApi: string = '';
  
  // Enhanced real-time state management
  private hubConnection: HubConnection | null = null;
  private connectionState$ = new BehaviorSubject<ConnectionState>({
    status: 'disconnected',
    reconnectAttempts: 0
  });
  private realtimeUpdates$ = new Subject<RealtimeUpdate>();
  private dataFreshness$ = new BehaviorSubject<DataFreshness>({
    age: 0,
    status: 'fresh',
    lastUpdated: new Date(),
    source: 'cached'
  });
  private healthStatus$ = new BehaviorSubject<HealthStatus | null>(null);

  constructor(
    private httpClient: HttpClient,
    private appConfigService: AppConfigService,
    private errorHandler: AnalyticsErrorHandlerService
  ) {
    this.analyticsApi = this.appConfigService.apiUrl + '/api/analytics';
    this.setupHeartbeatMonitoring();
  }

  private get accessToken(): string {
    return this.storage.getItem(this.JWT_TOKEN) || localStorage.getItem(this.JWT_TOKEN);
  }

  private get authHeaders() {
    return {
      'Authorization': `Bearer ${this.accessToken}`,
      'Content-Type': 'application/json'
    };
  }

  // Enhanced KPI endpoint with new models
  public getLatestKPIs(filters?: AnalyticsFilters): Observable<KpiSnapshot> {
    const params = this.buildFiltersParams(filters);

    return this.httpClient.get<AnalyticsApiResponse<KpiSnapshot>>(
      `${this.analyticsApi}/kpi/latest`,
      { headers: this.authHeaders, params }
    ).pipe(
      timeout(15000),
      retry({
        count: 2,
        delay: (error, retryCount) => {
          if (!this.errorHandler.shouldRetry('kpis')) {
            throw error;
          }
          return of(null).pipe(delay(1000 * retryCount));
        }
      }),
      map(response => {
        // Update data freshness tracking
        this.updateDataFreshness(response.data.timestamp, 'polling');
        return response.data;
      }),
      shareReplay(1),
      catchError(error => this.errorHandler.handleAnalyticsError(error, 'kpis', filters))
    );
  }

  public getUsageAnalytics(filters?: AnalyticsFilters): Observable<UsageAnalytics> {
    const params = this.buildFiltersParams(filters);

    return this.httpClient.get<AnalyticsApiResponse<UsageAnalytics>>(
      `${this.analyticsApi}/usage`,
      { headers: this.authHeaders, params }
    ).pipe(
      timeout(20000),
      retry(1),
      map(response => response.data),
      catchError(error => this.errorHandler.handleAnalyticsError(error, 'usage', filters))
    );
  }

  public getSegmentationData(filters?: AnalyticsFilters): Observable<SegmentationData> {
    const params = this.buildFiltersParams(filters);

    return this.httpClient.get<AnalyticsApiResponse<SegmentationData>>(
      `${this.analyticsApi}/segmentation`,
      { headers: this.authHeaders, params }
    ).pipe(
      timeout(20000),
      retry(1),
      map(response => response.data),
      catchError(error => this.errorHandler.handleAnalyticsError(error, 'segmentation', filters))
    );
  }

  public getProcessFlowData(filters?: AnalyticsFilters): Observable<ProcessFlowData> {
    const params = this.buildFiltersParams(filters);

    return this.httpClient.get<AnalyticsApiResponse<ProcessFlowData>>(
      `${this.analyticsApi}/process-flow`,
      { headers: this.authHeaders, params }
    ).pipe(
      timeout(20000),
      retry(1),
      map(response => response.data),
      catchError(error => this.errorHandler.handleAnalyticsError(error, 'process-flow', filters))
    );
  }

  public getTimeSeriesData(request: TimeSeriesRequest): Observable<TimeSeriesResponse> {
    const params = this.buildTimeSeriesParams(request);

    return this.httpClient.get<TimeSeriesResponse>(
      `${this.analyticsApi}/kpi/series`,
      { headers: this.authHeaders, params }
    ).pipe(
      timeout(30000),
      retry(1),
      catchError(error => this.errorHandler.handleAnalyticsError(error, 'timeseries', request))
    );
  }

  public getStuckDocuments(request: StuckDocumentsRequest): Observable<StuckDocumentsResponse> {
    const params = this.buildStuckDocumentsParams(request);

    return this.httpClient.get<StuckDocumentsResponse>(
      `${this.analyticsApi}/kpi/stuck`,
      { headers: this.authHeaders, params }
    ).pipe(
      timeout(20000),
      retry(1),
      catchError(error => this.errorHandler.handleAnalyticsError(error, 'stuck-documents', request))
    );
  }

  public getHealthStatus(): Observable<HealthStatus> {
    return this.httpClient.get<HealthStatus>(
      `${this.analyticsApi}/health`,
      { headers: this.authHeaders }
    ).pipe(
      timeout(5000),
      tap(health => this.healthStatus$.next(health)),
      catchError(error => {
        const errorHealth: HealthStatus = {
          status: 'critical',
          services: {
            database: { status: 'critical', responseTime: -1, lastChecked: new Date(), errorRate: 100 },
            signalr: { status: 'critical', responseTime: -1, lastChecked: new Date(), errorRate: 100 },
            s3: { status: 'critical', responseTime: -1, lastChecked: new Date(), errorRate: 100 },
            analytics: { status: 'critical', responseTime: -1, lastChecked: new Date(), errorRate: 100 }
          },
          overallScore: 0,
          lastChecked: new Date()
        };
        this.healthStatus$.next(errorHealth);
        return of(errorHealth);
      })
    );
  }

  // Enhanced export functionality
  public exportDashboard(format: ExportFormat, filters: ExportFilters): Observable<Blob> {
    const request = {
      format,
      filters,
      timestamp: new Date().toISOString()
    };

    return this.httpClient.post(`${this.analyticsApi}/export`, request, {
      headers: this.authHeaders,
      responseType: 'blob'
    }).pipe(
      timeout(60000),
      catchError(error => {
        console.error('Export failed:', error);
        throw error;
      })
    );
  }

  // Real-time SignalR implementation
  public async initializeSignalRConnection(): Promise<void> {
    if (!this.accessToken) {
      console.warn('No auth token available for SignalR connection');
      this.connectionState$.next({ status: 'error', reconnectAttempts: 0 });
      return;
    }

    try {
      this.connectionState$.next({ status: 'reconnecting', reconnectAttempts: 0 });

      this.hubConnection = new HubConnectionBuilder()
        .withUrl(`${this.appConfigService.baseUrl}/analyticsHub`, {
          accessTokenFactory: () => this.accessToken,
          withCredentials: true
        })
        .withAutomaticReconnect([0, 2000, 10000, 30000])
        .configureLogging(LogLevel.Information)
        .build();

      this.setupSignalRHandlers();
      await this.hubConnection.start();
      await this.joinAnalyticsStream();

      this.connectionState$.next({ 
        status: 'connected', 
        reconnectAttempts: 0,
        lastConnected: new Date(),
        connectionId: this.hubConnection.connectionId || undefined
      });

    } catch (error) {
      console.error('Failed to initialize SignalR connection:', error);
      this.connectionState$.next({ 
        status: 'error', 
        reconnectAttempts: this.connectionState$.value.reconnectAttempts + 1
      });
    }
  }

  private setupSignalRHandlers(): void {
    if (!this.hubConnection) return;

    // Analytics updates
    this.hubConnection.on('AnalyticsUpdate', (update: RealtimeUpdate) => {
      console.log('Received analytics update:', update.type);
      this.realtimeUpdates$.next(update);
      this.updateDataFreshness(update.timestamp, 'realtime');
    });

    // Health status updates
    this.hubConnection.on('HealthStatusChanged', (health: HealthStatus) => {
      console.log('Health status changed:', health.status);
      this.healthStatus$.next(health);
      
      const healthUpdate: RealtimeUpdate = {
        type: 'health_change',
        data: health,
        timestamp: new Date().toISOString()
      };
      this.realtimeUpdates$.next(healthUpdate);
    });

    // Connection events
    this.hubConnection.onclose((error) => {
      console.log('SignalR connection closed:', error);
      this.connectionState$.next({ 
        status: 'disconnected', 
        reconnectAttempts: this.connectionState$.value.reconnectAttempts
      });
    });

    this.hubConnection.onreconnecting((error) => {
      console.log('SignalR reconnecting:', error);
      this.connectionState$.next({ 
        status: 'reconnecting', 
        reconnectAttempts: this.connectionState$.value.reconnectAttempts + 1
      });
    });

    this.hubConnection.onreconnected((connectionId) => {
      console.log('SignalR reconnected:', connectionId);
      this.connectionState$.next({ 
        status: 'connected', 
        reconnectAttempts: 0,
        lastConnected: new Date(),
        connectionId
      });
      this.joinAnalyticsStream();
    });
  }

  private async joinAnalyticsStream(): Promise<void> {
    if (this.hubConnection?.state === 'Connected') {
      try {
        await this.hubConnection.invoke('JoinAnalyticsStream');
        console.log('Joined analytics stream');
      } catch (error) {
        console.error('Failed to join analytics stream:', error);
      }
    }
  }

  public async reconnectSignalR(): Promise<void> {
    if (this.hubConnection) {
      try {
        await this.hubConnection.stop();
      } catch (error) {
        console.warn('Error stopping existing connection:', error);
      }
    }

    await this.initializeSignalRConnection();
  }

  public async subscribeToMetric(metricName: string): Promise<void> {
    if (this.hubConnection?.state === 'Connected') {
      try {
        await this.hubConnection.invoke('SubscribeToMetric', metricName);
        console.log(`Subscribed to metric: ${metricName}`);
      } catch (error) {
        console.error(`Failed to subscribe to metric ${metricName}:`, error);
      }
    }
  }

  public async sendHeartbeat(): Promise<void> {
    if (this.hubConnection?.state === 'Connected') {
      try {
        const startTime = performance.now();
        await this.hubConnection.invoke('Heartbeat');
        const latency = performance.now() - startTime;
        
        // Update connection state with latency
        const currentState = this.connectionState$.value;
        this.connectionState$.next({ ...currentState, latency });
      } catch (error) {
        console.error('Failed to send heartbeat:', error);
      }
    }
  }

  public async disconnect(): Promise<void> {
    if (this.hubConnection) {
      try {
        await this.hubConnection.invoke('LeaveAnalyticsStream');
        await this.hubConnection.stop();
      } catch (error) {
        console.error('Error during SignalR disconnect:', error);
      }
    }
  }

  // Observable getters for real-time data
  public getRealtimeUpdates(): Observable<RealtimeUpdate> {
    return this.realtimeUpdates$.asObservable();
  }

  public getConnectionState(): Observable<ConnectionState> {
    return this.connectionState$.asObservable();
  }

  public getDataFreshness(): Observable<DataFreshness> {
    return this.dataFreshness$.asObservable();
  }

  public getHealthStatusStream(): Observable<HealthStatus | null> {
    return this.healthStatus$.asObservable();
  }

  // Server-Sent Events streaming (fallback)
  public getAnalyticsStream(): Observable<RealtimeUpdate> {
    const eventSource = new EventSource(`${this.analyticsApi}/stream`, {
      withCredentials: true
    });

    return new Observable(observer => {
      eventSource.onmessage = event => {
        try {
          const data = JSON.parse(event.data) as RealtimeUpdate;
          this.updateDataFreshness(data.timestamp, 'realtime');
          observer.next(data);
        } catch (error) {
          console.error('Failed to parse SSE data:', error);
        }
      };

      eventSource.onerror = error => {
        console.error('SSE error:', error);
        observer.error(error);
      };

      return () => {
        eventSource.close();
      };
    });
  }

  // Helper methods
  private buildFiltersParams(filters?: AnalyticsFilters): HttpParams {
    let params = new HttpParams();

    if (filters) {
      if (filters.timeRange) {
        params = params.set('timeRange', filters.timeRange);
      }
      if (filters.organizationId) {
        params = params.set('organizationId', filters.organizationId);
      }
      if (filters.documentTypes?.length) {
        filters.documentTypes.forEach(type => {
          params = params.append('documentTypes', type);
        });
      }
      if (filters.userRoles?.length) {
        filters.userRoles.forEach(role => {
          params = params.append('userRoles', role);
        });
      }
      if (filters.includeArchived !== undefined) {
        params = params.set('includeArchived', filters.includeArchived.toString());
      }
      if (filters.includeTestData !== undefined) {
        params = params.set('includeTestData', filters.includeTestData.toString());
      }
    }

    return params;
  }

  private buildTimeSeriesParams(request: TimeSeriesRequest): HttpParams {
    let params = new HttpParams()
      .set('from', request.from.toISOString())
      .set('to', request.to.toISOString())
      .set('granularity', request.granularity || '5m');

    if (request.metrics?.length) {
      request.metrics.forEach(metric => {
        params = params.append('metrics', metric);
      });
    }

    if (request.organizationId) {
      params = params.set('organizationId', request.organizationId);
    }
    if (request.templateId) {
      params = params.set('templateId', request.templateId);
    }
    if (request.deviceType) {
      params = params.set('deviceType', request.deviceType);
    }
    if (request.channel) {
      params = params.set('channel', request.channel);
    }

    return params;
  }

  private buildStuckDocumentsParams(request: StuckDocumentsRequest): HttpParams {
    let params = new HttpParams()
      .set('thresholdHours', request.thresholdHours.toString());

    if (request.since) {
      params = params.set('since', request.since.toISOString());
    }
    if (request.organizationId) {
      params = params.set('organizationId', request.organizationId);
    }
    if (request.templateId) {
      params = params.set('templateId', request.templateId);
    }

    return params;
  }

  private updateDataFreshness(timestamp: string, source: 'realtime' | 'polling' | 'cached'): void {
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
      source
    });
  }

  private setupHeartbeatMonitoring(): void {
    // Send heartbeat every 30 seconds when connected
    interval(30000).pipe(
      filter(() => this.connectionState$.value.status === 'connected')
    ).subscribe(() => {
      this.sendHeartbeat();
    });

    // Monitor data freshness every 5 seconds
    interval(5000).subscribe(() => {
      const freshness = this.dataFreshness$.value;
      if (freshness.lastUpdated) {
        this.updateDataFreshness(freshness.lastUpdated.toISOString(), freshness.source);
      }
    });
  }
}

// Request/Response models for the new endpoints
export interface TimeSeriesRequest {
  metrics: string[];
  from: Date;
  to: Date;
  granularity?: string;
  organizationId?: string;
  templateId?: string;
  deviceType?: string;
  channel?: string;
}

export interface TimeSeriesResponse {
  series: TimeSeries[];
  meta: SeriesMetadata;
}

export interface TimeSeries {
  metric: string;
  points: TimeSeriesPoint[];
}

export interface SeriesMetadata {
  granularity: string;
  filters: any;
  totalPoints: number;
  userRole: string;
}

export interface StuckDocumentsRequest {
  since?: Date;
  thresholdHours: number;
  organizationId?: string;
  templateId?: string;
}

export interface StuckDocumentsResponse {
  documents: StuckDocumentInfo[];
  totalCount: number;
  thresholdHours: number;
  generatedAt: Date;
  userRole: string;
}

export interface ExportRequest {
  format: string;
  filters: AnalyticsFilterRequest;
  metrics: string[];
  includeRawData: boolean;
}

export interface HealthResponse {
  status: string;
  dataAgeSeconds: number;
  cacheState?: string;
  s3ReadLatencyMs?: number;
  lastWatermark?: Date;
  systemComponents?: { [key: string]: string };
  timestamp: Date;
}