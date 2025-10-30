import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

export interface LoadingState {
  isLoading: boolean;
  loadingMessage?: string;
  progress?: number; // 0-100 for progress bars
  component?: string; // Which component is loading
}

@Injectable({ providedIn: 'root' })
export class AnalyticsLoadingService {
  private loadingStates = new Map<string, BehaviorSubject<LoadingState>>();

  constructor() {}

  /**
   * Sets loading state for a specific component
   */
  public setLoading(
    component: string,
    isLoading: boolean,
    message?: string,
    progress?: number
  ): void {
    if (!this.loadingStates.has(component)) {
      this.loadingStates.set(component, new BehaviorSubject<LoadingState>({
        isLoading: false,
        component
      }));
    }

    const state: LoadingState = {
      isLoading,
      loadingMessage: message,
      progress,
      component
    };

    this.loadingStates.get(component)!.next(state);
  }

  /**
   * Gets loading state observable for a specific component
   */
  public getLoadingState(component: string): Observable<LoadingState> {
    if (!this.loadingStates.has(component)) {
      this.loadingStates.set(component, new BehaviorSubject<LoadingState>({
        isLoading: false,
        component
      }));
    }

    return this.loadingStates.get(component)!.asObservable();
  }

  /**
   * Gets current loading state for a component (synchronous)
   */
  public isLoading(component: string): boolean {
    if (!this.loadingStates.has(component)) {
      return false;
    }

    return this.loadingStates.get(component)!.value.isLoading;
  }

  /**
   * Sets progress for a loading component
   */
  public setProgress(component: string, progress: number, message?: string): void {
    if (this.loadingStates.has(component)) {
      const currentState = this.loadingStates.get(component)!.value;
      this.setLoading(component, currentState.isLoading, message || currentState.loadingMessage, progress);
    }
  }

  /**
   * Starts loading for multiple components with orchestrated messages
   */
  public startAnalyticsDashboardLoading(): void {
    this.setLoading('dashboard', true, 'Initializing analytics dashboard...', 0);
    this.setLoading('kpi-cards', true, 'Loading key performance indicators...', 0);
    this.setLoading('usage-charts', true, 'Loading usage analytics...', 0);
    this.setLoading('segmentation-charts', true, 'Loading segmentation data...', 0);
    this.setLoading('process-flow', true, 'Loading process flow data...', 0);
  }

  /**
   * Updates loading progress in phases
   */
  public updateAnalyticsLoadingProgress(phase: 'kpis' | 'usage' | 'segmentation' | 'process-flow'): void {
    const progressMap = {
      'kpis': 25,
      'usage': 50,
      'segmentation': 75,
      'process-flow': 100
    };

    const messageMap = {
      'kpis': 'Loaded KPI data...',
      'usage': 'Loaded usage analytics...',
      'segmentation': 'Loaded segmentation data...',
      'process-flow': 'Analytics dashboard ready!'
    };

    const progress = progressMap[phase];
    const message = messageMap[phase];

    this.setProgress('dashboard', progress, message);

    // Complete individual component loading
    switch (phase) {
      case 'kpis':
        this.setLoading('kpi-cards', false);
        break;
      case 'usage':
        this.setLoading('usage-charts', false);
        break;
      case 'segmentation':
        this.setLoading('segmentation-charts', false);
        break;
      case 'process-flow':
        this.setLoading('process-flow', false);
        this.setLoading('dashboard', false); // Complete overall loading
        break;
    }
  }

  /**
   * Completes all analytics loading
   */
  public completeAnalyticsLoading(): void {
    this.setLoading('dashboard', false);
    this.setLoading('kpi-cards', false);
    this.setLoading('usage-charts', false);
    this.setLoading('segmentation-charts', false);
    this.setLoading('process-flow', false);
  }

  /**
   * Handles loading error state
   */
  public setLoadingError(component: string, errorMessage: string): void {
    this.setLoading(component, false, `Error: ${errorMessage}`);
  }

  /**
   * Gets all current loading states (for debugging)
   */
  public getAllLoadingStates(): { [key: string]: LoadingState } {
    const states: { [key: string]: LoadingState } = {};

    this.loadingStates.forEach((subject, component) => {
      states[component] = subject.value;
    });

    return states;
  }

  /**
   * Clears all loading states
   */
  public clearAllLoadingStates(): void {
    this.loadingStates.forEach((subject) => {
      subject.next({ isLoading: false, component: subject.value.component });
    });
  }

  /**
   * Check if any component is currently loading
   */
  public isAnyComponentLoading(): boolean {
    let isLoading = false;

    this.loadingStates.forEach((subject) => {
      if (subject.value.isLoading) {
        isLoading = true;
      }
    });

    return isLoading;
  }

  /**
   * Gets loading states for dashboard overview
   */
  public getDashboardLoadingOverview(): Observable<LoadingState> {
    return this.getLoadingState('dashboard');
  }
}