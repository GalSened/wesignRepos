import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import {
  DashboardKPIs,
  UsageAnalytics,
  SegmentationData,
  ProcessFlowData,
  AnalyticsFilterRequest
} from '@models/analytics/analytics-models';

export interface AnalyticsError {
  code: string;
  message: string;
  timestamp: Date;
  severity: 'low' | 'medium' | 'high' | 'critical';
  retryable: boolean;
  fallbackAvailable: boolean;
}

@Injectable({ providedIn: 'root' })
export class AnalyticsErrorHandlerService {
  private errorLog: AnalyticsError[] = [];
  private readonly MAX_ERROR_LOG_SIZE = 100;

  constructor() {}

  /**
   * Handles analytics API errors and provides fallback strategies
   */
  public handleAnalyticsError(
    error: any,
    endpoint: string,
    filter?: AnalyticsFilterRequest
  ): Observable<any> {
    const analyticsError = this.processError(error, endpoint);
    this.logError(analyticsError);

    // Determine fallback strategy based on error type and endpoint
    switch (endpoint) {
      case 'kpis':
        return this.getFallbackKPIs(filter);
      case 'usage':
        return this.getFallbackUsageAnalytics(filter);
      case 'segmentation':
        return this.getFallbackSegmentationData(filter);
      case 'process-flow':
        return this.getFallbackProcessFlowData(filter);
      default:
        return this.getEmptyFallback(endpoint);
    }
  }

  /**
   * Processes raw errors into structured analytics errors
   */
  private processError(error: any, endpoint: string): AnalyticsError {
    let code = 'UNKNOWN_ERROR';
    let message = 'An unknown error occurred';
    let severity: 'low' | 'medium' | 'high' | 'critical' = 'medium';
    let retryable = true;
    let fallbackAvailable = true;

    // HTTP Error handling
    if (error.status) {
      switch (error.status) {
        case 400:
          code = 'BAD_REQUEST';
          message = 'Invalid request parameters';
          severity = 'medium';
          retryable = false;
          break;
        case 401:
          code = 'UNAUTHORIZED';
          message = 'Authentication required';
          severity = 'high';
          retryable = false;
          fallbackAvailable = false;
          break;
        case 403:
          code = 'FORBIDDEN';
          message = 'Insufficient permissions for analytics data';
          severity = 'high';
          retryable = false;
          fallbackAvailable = false;
          break;
        case 404:
          code = 'NOT_FOUND';
          message = `Analytics endpoint not found: ${endpoint}`;
          severity = 'high';
          retryable = false;
          break;
        case 429:
          code = 'RATE_LIMITED';
          message = 'Too many requests. Please try again later.';
          severity = 'medium';
          retryable = true;
          break;
        case 500:
          code = 'SERVER_ERROR';
          message = 'Internal server error while processing analytics';
          severity = 'critical';
          retryable = true;
          break;
        case 502:
        case 503:
        case 504:
          code = 'SERVICE_UNAVAILABLE';
          message = 'Analytics service temporarily unavailable';
          severity = 'high';
          retryable = true;
          break;
        default:
          code = `HTTP_${error.status}`;
          message = error.message || `HTTP ${error.status} error`;
          severity = 'medium';
      }
    }
    // Network errors
    else if (error.name === 'NetworkError' || error.message?.includes('Network')) {
      code = 'NETWORK_ERROR';
      message = 'Network connection error';
      severity = 'high';
      retryable = true;
    }
    // Timeout errors
    else if (error.name === 'TimeoutError' || error.message?.includes('timeout')) {
      code = 'TIMEOUT_ERROR';
      message = 'Request timeout - analytics service not responding';
      severity = 'medium';
      retryable = true;
    }
    // Parse errors
    else if (error.name === 'SyntaxError') {
      code = 'PARSE_ERROR';
      message = 'Invalid response format from analytics service';
      severity = 'medium';
      retryable = false;
    }

    return {
      code,
      message,
      timestamp: new Date(),
      severity,
      retryable,
      fallbackAvailable
    };
  }

  /**
   * Logs error to internal error tracking
   */
  private logError(error: AnalyticsError): void {
    this.errorLog.push(error);

    // Maintain error log size
    if (this.errorLog.length > this.MAX_ERROR_LOG_SIZE) {
      this.errorLog = this.errorLog.slice(-this.MAX_ERROR_LOG_SIZE);
    }

    // Log to console for debugging
    console.error(`[Analytics Error] ${error.code}: ${error.message}`, error);

    // For critical errors, you might want to send to external logging service
    if (error.severity === 'critical') {
      this.sendToCentralLogging(error);
    }
  }

  /**
   * Provides fallback KPI data when main API fails
   */
  private getFallbackKPIs(filter?: AnalyticsFilterRequest): Observable<DashboardKPIs> {
    const fallbackKPIs = new DashboardKPIs();

    // Provide basic fallback data
    fallbackKPIs.dailyActiveUsers = 0;
    fallbackKPIs.weeklyActiveUsers = 0;
    fallbackKPIs.monthlyActiveUsers = 0;
    fallbackKPIs.documentsCreated = 0;
    fallbackKPIs.documentsSent = 0;
    fallbackKPIs.documentsSigned = 0;
    fallbackKPIs.overallSuccessRate = 0;
    fallbackKPIs.averageTimeToSign = 0;

    return of(fallbackKPIs);
  }

  /**
   * Provides fallback usage analytics when main API fails
   */
  private getFallbackUsageAnalytics(filter?: AnalyticsFilterRequest): Observable<UsageAnalytics> {
    const fallbackUsage = new UsageAnalytics();
    // Empty time series arrays - charts will show "No data available"
    return of(fallbackUsage);
  }

  /**
   * Provides fallback segmentation data when main API fails
   */
  private getFallbackSegmentationData(filter?: AnalyticsFilterRequest): Observable<SegmentationData> {
    const fallbackSegmentation = new SegmentationData();
    // Empty arrays - components will show "No data available"
    return of(fallbackSegmentation);
  }

  /**
   * Provides fallback process flow data when main API fails
   */
  private getFallbackProcessFlowData(filter?: AnalyticsFilterRequest): Observable<ProcessFlowData> {
    const fallbackProcessFlow = new ProcessFlowData();
    // Empty arrays - components will show "No data available"
    return of(fallbackProcessFlow);
  }

  /**
   * Generic empty fallback for unknown endpoints
   */
  private getEmptyFallback(endpoint: string): Observable<any> {
    console.warn(`No specific fallback available for endpoint: ${endpoint}`);
    return of({});
  }

  /**
   * Sends critical errors to external logging service
   */
  private sendToCentralLogging(error: AnalyticsError): void {
    // In production, this would send to services like Sentry, LogRocket, etc.
    // For now, just enhanced console logging
    console.error('[CRITICAL ANALYTICS ERROR]', {
      code: error.code,
      message: error.message,
      timestamp: error.timestamp,
      userAgent: navigator.userAgent,
      url: window.location.href
    });
  }

  /**
   * Gets recent error history for debugging
   */
  public getErrorHistory(): AnalyticsError[] {
    return [...this.errorLog];
  }

  /**
   * Clears error history
   */
  public clearErrorHistory(): void {
    this.errorLog = [];
  }

  /**
   * Checks if a specific error type is currently affecting the system
   */
  public hasActiveErrors(errorCode?: string): boolean {
    const recentErrors = this.errorLog.filter(
      error => (Date.now() - error.timestamp.getTime()) < 300000 // Last 5 minutes
    );

    if (errorCode) {
      return recentErrors.some(error => error.code === errorCode);
    }

    return recentErrors.length > 0;
  }

  /**
   * Determines if retry should be attempted based on error history
   */
  public shouldRetry(endpoint: string): boolean {
    const recentErrors = this.errorLog.filter(
      error => (Date.now() - error.timestamp.getTime()) < 60000 // Last minute
    );

    // Don't retry if we've had more than 3 errors in the last minute
    const errorCount = recentErrors.length;
    if (errorCount >= 3) {
      return false;
    }

    // Don't retry non-retryable errors
    const lastError = recentErrors[recentErrors.length - 1];
    if (lastError && !lastError.retryable) {
      return false;
    }

    return true;
  }

  /**
   * Gets user-friendly error message for display
   */
  public getUserFriendlyMessage(error: AnalyticsError): string {
    switch (error.code) {
      case 'UNAUTHORIZED':
        return 'Please log in again to view analytics data.';
      case 'FORBIDDEN':
        return 'You don\'t have permission to view analytics data.';
      case 'NETWORK_ERROR':
        return 'Connection problem. Please check your internet connection.';
      case 'SERVICE_UNAVAILABLE':
        return 'Analytics service is temporarily unavailable. Please try again later.';
      case 'RATE_LIMITED':
        return 'Too many requests. Please wait a moment before refreshing.';
      case 'TIMEOUT_ERROR':
        return 'Request timed out. Please try again.';
      default:
        return 'Unable to load analytics data. Using cached data where available.';
    }
  }
}