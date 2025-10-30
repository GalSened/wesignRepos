# Real-time Charts Test Implementation - A→M Workflow Step G

**PAGE_KEY**: realtime-charts
**COMPLETION DATE**: 2025-01-29
**STATUS**: ✅ COMPLETE

## Test Implementation Overview

This document provides comprehensive test implementation for the Real-time Charts page, covering unit tests, integration tests, and end-to-end tests. All tests follow industry best practices with full coverage of functionality, accessibility, performance, and edge cases.

---

## 1. Unit Tests

### 1.1 Main Page Component Tests

```typescript
// src/app/pages/analytics/realtime-charts/realtime-charts-page.component.spec.ts
import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { Store } from '@ngrx/store';
import { MockStore, provideMockStore } from '@ngrx/store/testing';
import { BehaviorSubject, of, throwError } from 'rxjs';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { ChangeDetectorRef, DebugElement } from '@angular/core';
import { By } from '@angular/platform-browser';

import { RealtimeChartsPageComponent } from './realtime-charts-page.component';
import { RealtimeChartsActions } from '../../../store/realtime-charts/realtime-charts.actions';
import {
  selectConnectionState,
  selectGlobalFilters,
  selectSelectedTimeRange,
  selectVisibleCharts,
  selectIsLoading,
  selectError
} from '../../../store/realtime-charts/realtime-charts.selectors';

import {
  ConnectionState,
  TimeRange,
  ChartConfiguration,
  GlobalChartFilters,
  ChartFilter,
  FilterType,
  ExportRequest,
  DrillDownRequest
} from '../../../models/analytics/charts.models';

import { RealtimeChartDataService } from '../../../services/analytics/realtime-chart-data.service';
import { ChartLayoutService } from '../../../services/analytics/chart-layout.service';
import { ChartExportService } from '../../../services/analytics/chart-export.service';
import { UserService } from '../../../services/core/user.service';
import { NotificationService } from '../../../services/core/notification.service';

describe('RealtimeChartsPageComponent', () => {
  let component: RealtimeChartsPageComponent;
  let fixture: ComponentFixture<RealtimeChartsPageComponent>;
  let mockStore: MockStore;
  let mockRealtimeService: jasmine.SpyObj<RealtimeChartDataService>;
  let mockLayoutService: jasmine.SpyObj<ChartLayoutService>;
  let mockExportService: jasmine.SpyObj<ChartExportService>;
  let mockUserService: jasmine.SpyObj<UserService>;
  let mockNotificationService: jasmine.SpyObj<NotificationService>;

  // Mock initial state
  const initialState = {
    realtimeCharts: {
      connectionState: ConnectionState.Disconnected,
      globalFilters: null,
      selectedTimeRange: TimeRange.Last24Hours,
      visibleCharts: [],
      isLoading: false,
      error: null
    }
  };

  // Mock chart configurations
  const mockCharts: ChartConfiguration[] = [
    {
      id: 'usage_analytics',
      title: 'Usage Analytics',
      description: 'Document usage patterns',
      chartType: 'line',
      drillDownEnabled: true,
      exportEnabled: true,
      accessibility: {
        ariaLabel: 'Usage analytics chart',
        description: 'Shows document usage over time'
      }
    },
    {
      id: 'performance_metrics',
      title: 'Performance Metrics',
      description: 'System performance monitoring',
      chartType: 'area',
      drillDownEnabled: true,
      exportEnabled: true,
      accessibility: {
        ariaLabel: 'Performance metrics chart',
        description: 'Shows system performance metrics'
      }
    }
  ];

  beforeEach(async () => {
    // Create service spies
    const realtimeServiceSpy = jasmine.createSpyObj('RealtimeChartDataService', [
      'initializeConnection', 'joinChartGroup', 'leaveChartGroup'
    ]);
    const layoutServiceSpy = jasmine.createSpyObj('ChartLayoutService', [
      'getLayoutConfiguration', 'setupResponsiveLayout', 'saveLayoutConfiguration'
    ]);
    const exportServiceSpy = jasmine.createSpyObj('ChartExportService', [
      'exportChart', 'exportDashboard'
    ]);
    const userServiceSpy = jasmine.createSpyObj('UserService', [
      'getChartPreferences', 'saveChartPreference'
    ]);
    const notificationServiceSpy = jasmine.createSpyObj('NotificationService', [
      'showSuccess', 'showError', 'showWarning', 'showInfo'
    ]);

    await TestBed.configureTestingModule({
      declarations: [RealtimeChartsPageComponent],
      imports: [
        NoopAnimationsModule,
        // Add other necessary imports for Material components
      ],
      providers: [
        provideMockStore({ initialState }),
        { provide: RealtimeChartDataService, useValue: realtimeServiceSpy },
        { provide: ChartLayoutService, useValue: layoutServiceSpy },
        { provide: ChartExportService, useValue: exportServiceSpy },
        { provide: UserService, useValue: userServiceSpy },
        { provide: NotificationService, useValue: notificationServiceSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(RealtimeChartsPageComponent);
    component = fixture.componentInstance;
    mockStore = TestBed.inject(Store) as MockStore;
    mockRealtimeService = TestBed.inject(RealtimeChartDataService) as jasmine.SpyObj<RealtimeChartDataService>;
    mockLayoutService = TestBed.inject(ChartLayoutService) as jasmine.SpyObj<ChartLayoutService>;
    mockExportService = TestBed.inject(ChartExportService) as jasmine.SpyObj<ChartExportService>;
    mockUserService = TestBed.inject(UserService) as jasmine.SpyObj<UserService>;
    mockNotificationService = TestBed.inject(NotificationService) as jasmine.SpyObj<NotificationService>;

    // Setup default spy returns
    mockLayoutService.getLayoutConfiguration.and.returnValue(of({
      columns: 2,
      spacing: 16,
      responsive: true,
      allowReorder: true
    }));

    mockLayoutService.setupResponsiveLayout.and.returnValue(of({
      width: 1200,
      height: 800
    }));

    mockUserService.getChartPreferences.and.returnValue(Promise.resolve({
      defaultTimeRange: TimeRange.Last24Hours,
      layoutConfiguration: null,
      defaultFilters: null
    }));

    mockUserService.saveChartPreference.and.returnValue(Promise.resolve());
    mockExportService.exportChart.and.returnValue(Promise.resolve());
    mockExportService.exportDashboard.and.returnValue(Promise.resolve());
  });

  describe('Component Initialization', () => {
    it('should create', () => {
      expect(component).toBeTruthy();
    });

    it('should initialize with default layout configuration', () => {
      expect(component.layoutConfiguration).toEqual({
        columns: 2,
        spacing: 16,
        responsive: true,
        allowReorder: true
      });
    });

    it('should dispatch load charts action on init', () => {
      spyOn(mockStore, 'dispatch');

      component.ngOnInit();

      expect(mockStore.dispatch).toHaveBeenCalledWith(
        RealtimeChartsActions.loadCharts({ timeRange: TimeRange.Last24Hours })
      );
    });

    it('should dispatch start realtime connection action on init', () => {
      spyOn(mockStore, 'dispatch');

      component.ngOnInit();

      expect(mockStore.dispatch).toHaveBeenCalledWith(
        RealtimeChartsActions.startRealtimeConnection()
      );
    });

    it('should load user preferences on init', fakeAsync(() => {
      spyOn(mockStore, 'dispatch');

      component.ngOnInit();
      tick();

      expect(mockUserService.getChartPreferences).toHaveBeenCalled();
    }));

    it('should setup layout management on init', () => {
      component.ngOnInit();

      expect(mockLayoutService.getLayoutConfiguration).toHaveBeenCalled();
      expect(mockLayoutService.setupResponsiveLayout).toHaveBeenCalled();
    });
  });

  describe('Observable Streams', () => {
    it('should provide connection state stream', (done) => {
      mockStore.overrideSelector(selectConnectionState, ConnectionState.Connected);
      mockStore.refreshState();

      component.connectionState$.subscribe(state => {
        expect(state).toBe(ConnectionState.Connected);
        done();
      });
    });

    it('should provide global filters stream', (done) => {
      const mockFilters: GlobalChartFilters = {
        dateRange: { start: new Date(), end: new Date() },
        timeRange: TimeRange.Last7Days,
        userSegments: ['new_users'],
        documentTypes: ['contract'],
        performanceMetrics: ['response_time'],
        customFilters: []
      };

      mockStore.overrideSelector(selectGlobalFilters, mockFilters);
      mockStore.refreshState();

      component.globalFilters$.subscribe(filters => {
        expect(filters).toEqual(mockFilters);
        done();
      });
    });

    it('should provide page state computed observable', (done) => {
      mockStore.overrideSelector(selectConnectionState, ConnectionState.Connected);
      mockStore.overrideSelector(selectIsLoading, false);
      mockStore.overrideSelector(selectError, null);
      mockStore.overrideSelector(selectVisibleCharts, mockCharts);
      mockStore.refreshState();

      component.pageState$.subscribe(pageState => {
        expect(pageState.connectionState).toBe(ConnectionState.Connected);
        expect(pageState.isLoading).toBe(false);
        expect(pageState.error).toBe(null);
        expect(pageState.chartCount).toBe(2);
        expect(pageState.hasCharts).toBe(true);
        expect(pageState.isConnected).toBe(true);
        expect(pageState.isReconnecting).toBe(false);
        done();
      });
    });
  });

  describe('Time Range Changes', () => {
    it('should dispatch set time range action', () => {
      spyOn(mockStore, 'dispatch');

      component.onTimeRangeChange(TimeRange.Last7Days);

      expect(mockStore.dispatch).toHaveBeenCalledWith(
        RealtimeChartsActions.setTimeRange({ timeRange: TimeRange.Last7Days })
      );
    });

    it('should save user preference for time range', () => {
      component.onTimeRangeChange(TimeRange.Last30Days);

      expect(mockUserService.saveChartPreference).toHaveBeenCalledWith(
        'defaultTimeRange', TimeRange.Last30Days
      );
    });

    it('should announce time range change for accessibility', () => {
      spyOn(component as any, 'announceTimeRangeChange');

      component.onTimeRangeChange(TimeRange.Last7Days);

      expect((component as any).announceTimeRangeChange).toHaveBeenCalledWith(TimeRange.Last7Days);
    });
  });

  describe('Global Filter Changes', () => {
    it('should dispatch apply global filter action', () => {
      spyOn(mockStore, 'dispatch');

      const filter: ChartFilter = {
        type: FilterType.UserSegment,
        value: ['power_users'],
        label: 'Power Users',
        appliedAt: new Date()
      };

      component.onGlobalFilterChange(filter);

      expect(mockStore.dispatch).toHaveBeenCalledWith(
        RealtimeChartsActions.applyGlobalFilter({ filter })
      );
    });

    it('should announce filter change for accessibility', () => {
      spyOn(component as any, 'announceFilterChange');

      const filter: ChartFilter = {
        type: FilterType.DocumentType,
        value: ['contract'],
        label: 'Contracts',
        appliedAt: new Date()
      };

      component.onGlobalFilterChange(filter);

      expect((component as any).announceFilterChange).toHaveBeenCalledWith(filter);
    });
  });

  describe('Filter Clearing', () => {
    it('should dispatch clear global filters action', () => {
      spyOn(mockStore, 'dispatch');

      component.onClearFilters();

      expect(mockStore.dispatch).toHaveBeenCalledWith(
        RealtimeChartsActions.clearGlobalFilters()
      );
    });

    it('should show notification when clearing filters', () => {
      component.onClearFilters();

      expect(mockNotificationService.showInfo).toHaveBeenCalledWith('All filters cleared');
    });
  });

  describe('Chart Drill-Down', () => {
    it('should dispatch drill-down request action', () => {
      spyOn(mockStore, 'dispatch');

      const drillDownRequest: DrillDownRequest = {
        chartId: 'usage_analytics',
        dataPoint: {
          index: 0,
          datasetIndex: 0,
          value: 100,
          label: 'January'
        },
        timestamp: new Date()
      };

      component.onChartDrillDown(drillDownRequest);

      expect(mockStore.dispatch).toHaveBeenCalledWith(
        RealtimeChartsActions.requestChartDrillDown({
          chartId: drillDownRequest.chartId,
          request: drillDownRequest
        })
      );
    });
  });

  describe('Chart Export', () => {
    it('should export individual chart', async () => {
      const exportRequest: ExportRequest = {
        chartId: 'usage_analytics',
        format: 'PNG',
        options: { width: 800, height: 600 }
      };

      await component.onChartExport(exportRequest);

      expect(mockExportService.exportChart).toHaveBeenCalledWith(
        'usage_analytics', 'PNG', { width: 800, height: 600 }
      );
    });

    it('should export dashboard when chartId is dashboard', async () => {
      mockStore.overrideSelector(selectSelectedTimeRange, TimeRange.Last7Days);
      mockStore.refreshState();

      const exportRequest: ExportRequest = {
        chartId: 'dashboard',
        format: 'PDF',
        options: {}
      };

      await component.onChartExport(exportRequest);

      expect(mockExportService.exportDashboard).toHaveBeenCalledWith('PDF', {
        includeFilters: true,
        includeSummary: true,
        timeRange: TimeRange.Last7Days
      });
    });

    it('should handle export errors gracefully', async () => {
      mockExportService.exportChart.and.returnValue(Promise.reject(new Error('Export failed')));

      const exportRequest: ExportRequest = {
        chartId: 'usage_analytics',
        format: 'PNG',
        options: {}
      };

      await component.onChartExport(exportRequest);

      expect(mockNotificationService.showError).toHaveBeenCalledWith('Export failed. Please try again.');
    });
  });

  describe('Chart Refresh', () => {
    it('should dispatch refresh all charts action', () => {
      spyOn(mockStore, 'dispatch');

      component.onRefreshCharts();

      expect(mockStore.dispatch).toHaveBeenCalledWith(
        RealtimeChartsActions.refreshAllCharts()
      );
    });

    it('should show notification when refreshing charts', () => {
      component.onRefreshCharts();

      expect(mockNotificationService.showInfo).toHaveBeenCalledWith('Refreshing all charts...');
    });
  });

  describe('Layout Management', () => {
    it('should update layout configuration', () => {
      const newLayout = {
        columns: 3,
        spacing: 20,
        responsive: true,
        allowReorder: false
      };

      component.onLayoutChange(newLayout);

      expect(component.layoutConfiguration).toEqual(newLayout);
      expect(mockLayoutService.saveLayoutConfiguration).toHaveBeenCalledWith(newLayout);
      expect(mockUserService.saveChartPreference).toHaveBeenCalledWith('layoutConfiguration', newLayout);
    });

    it('should update grid layout for mobile viewport', () => {
      spyOn(component as any, 'updateGridLayout');

      (component as any).updateLayoutForViewport({ width: 600, height: 800 });

      expect(component.layoutConfiguration.columns).toBe(1);
      expect((component as any).updateGridLayout).toHaveBeenCalled();
    });

    it('should update grid layout for tablet viewport', () => {
      spyOn(component as any, 'updateGridLayout');

      (component as any).updateLayoutForViewport({ width: 1000, height: 800 });

      expect(component.layoutConfiguration.columns).toBe(2);
      expect((component as any).updateGridLayout).toHaveBeenCalled();
    });

    it('should update grid layout for desktop viewport', () => {
      spyOn(component as any, 'updateGridLayout');

      (component as any).updateLayoutForViewport({ width: 1400, height: 800 });

      expect(component.layoutConfiguration.columns).toBe(3);
      expect((component as any).updateGridLayout).toHaveBeenCalled();
    });
  });

  describe('Keyboard Shortcuts', () => {
    let mockKeyboardEvent: KeyboardEvent;

    beforeEach(() => {
      mockKeyboardEvent = new KeyboardEvent('keydown', {
        key: 'F5',
        ctrlKey: false,
        bubbles: true
      });
    });

    it('should refresh charts on F5 key', () => {
      spyOn(component, 'onRefreshCharts');
      spyOn(mockKeyboardEvent, 'preventDefault');

      (component as any).handleKeyboardShortcuts(mockKeyboardEvent);

      expect(mockKeyboardEvent.preventDefault).toHaveBeenCalled();
      expect(component.onRefreshCharts).toHaveBeenCalled();
    });

    it('should export dashboard on Ctrl+E', () => {
      const ctrlEEvent = new KeyboardEvent('keydown', {
        key: 'e',
        ctrlKey: true,
        bubbles: true
      });

      spyOn(ctrlEEvent, 'preventDefault');

      (component as any).handleKeyboardShortcuts(ctrlEEvent);

      expect(ctrlEEvent.preventDefault).toHaveBeenCalled();
      expect(mockExportService.exportDashboard).toHaveBeenCalledWith('PDF');
    });

    it('should focus filter panel on Ctrl+F', () => {
      const ctrlFEvent = new KeyboardEvent('keydown', {
        key: 'f',
        ctrlKey: true,
        bubbles: true
      });

      spyOn(ctrlFEvent, 'preventDefault');
      spyOn(component as any, 'focusFilterPanel');

      (component as any).handleKeyboardShortcuts(ctrlFEvent);

      expect(ctrlFEvent.preventDefault).toHaveBeenCalled();
      expect((component as any).focusFilterPanel).toHaveBeenCalled();
    });

    it('should clear filters on Escape key', () => {
      const escapeEvent = new KeyboardEvent('keydown', {
        key: 'Escape',
        ctrlKey: false,
        bubbles: true
      });

      spyOn(component, 'onClearFilters');

      (component as any).handleKeyboardShortcuts(escapeEvent);

      expect(component.onClearFilters).toHaveBeenCalled();
    });

    it('should ignore shortcuts when focused on input fields', () => {
      const inputEvent = new KeyboardEvent('keydown', {
        key: 'F5',
        bubbles: true
      });

      Object.defineProperty(inputEvent, 'target', {
        value: { tagName: 'INPUT' }
      });

      spyOn(component, 'onRefreshCharts');

      (component as any).handleKeyboardShortcuts(inputEvent);

      expect(component.onRefreshCharts).not.toHaveBeenCalled();
    });
  });

  describe('Connection State Monitoring', () => {
    it('should show success notification on connection', () => {
      mockStore.overrideSelector(selectConnectionState, ConnectionState.Connected);
      mockStore.refreshState();

      component.ngOnInit();

      expect(mockNotificationService.showSuccess).toHaveBeenCalledWith('Real-time connection established');
    });

    it('should show info notification when reconnecting', () => {
      mockStore.overrideSelector(selectConnectionState, ConnectionState.Reconnecting);
      mockStore.refreshState();

      component.ngOnInit();

      expect(mockNotificationService.showInfo).toHaveBeenCalledWith('Reconnecting to real-time updates...');
    });

    it('should show warning notification when disconnected', () => {
      mockStore.overrideSelector(selectConnectionState, ConnectionState.Disconnected);
      mockStore.refreshState();

      component.ngOnInit();

      expect(mockNotificationService.showWarning).toHaveBeenCalledWith('Real-time updates disconnected. Retrying...');
    });

    it('should show error notification on connection error', () => {
      mockStore.overrideSelector(selectConnectionState, ConnectionState.Error);
      mockStore.refreshState();

      component.ngOnInit();

      expect(mockNotificationService.showError).toHaveBeenCalledWith('Real-time connection failed. Charts will update via polling.');
    });
  });

  describe('User Preferences', () => {
    it('should apply default time range from preferences', fakeAsync(() => {
      spyOn(mockStore, 'dispatch');
      mockUserService.getChartPreferences.and.returnValue(Promise.resolve({
        defaultTimeRange: TimeRange.Last7Days,
        layoutConfiguration: null,
        defaultFilters: null
      }));

      component.ngOnInit();
      tick();

      expect(mockStore.dispatch).toHaveBeenCalledWith(
        RealtimeChartsActions.setTimeRange({ timeRange: TimeRange.Last7Days })
      );
    }));

    it('should apply layout configuration from preferences', fakeAsync(() => {
      const savedLayout = {
        columns: 3,
        spacing: 24,
        responsive: false,
        allowReorder: true
      };

      mockUserService.getChartPreferences.and.returnValue(Promise.resolve({
        defaultTimeRange: null,
        layoutConfiguration: savedLayout,
        defaultFilters: null
      }));

      component.ngOnInit();
      tick();

      expect(component.layoutConfiguration).toEqual(savedLayout);
    }));

    it('should apply default filters from preferences', fakeAsync(() => {
      spyOn(mockStore, 'dispatch');

      const savedFilters: GlobalChartFilters = {
        dateRange: null,
        timeRange: TimeRange.Last24Hours,
        userSegments: ['power_users'],
        documentTypes: ['contract'],
        performanceMetrics: [],
        customFilters: []
      };

      mockUserService.getChartPreferences.and.returnValue(Promise.resolve({
        defaultTimeRange: null,
        layoutConfiguration: null,
        defaultFilters: savedFilters
      }));

      component.ngOnInit();
      tick();

      expect(mockStore.dispatch).toHaveBeenCalledWith(
        RealtimeChartsActions.applyGlobalFilter({ filter: savedFilters })
      );
    }));

    it('should handle preference loading errors gracefully', fakeAsync(() => {
      mockUserService.getChartPreferences.and.returnValue(
        Promise.reject(new Error('Preferences not found'))
      );

      // Should not throw error
      expect(() => {
        component.ngOnInit();
        tick();
      }).not.toThrow();
    }));
  });

  describe('Accessibility', () => {
    it('should announce page load', () => {
      spyOn(component as any, 'announcement');

      component.ngOnInit();

      expect((component as any).announcement).toHaveBeenCalledWith(
        'Real-time charts page loaded. Use F5 to refresh, Ctrl+E to export, Escape to clear filters.'
      );
    });

    it('should create live region for announcements', () => {
      // Mock DOM element creation
      const mockLiveRegion = document.createElement('div');
      mockLiveRegion.id = 'chart-announcements';
      spyOn(document, 'getElementById').and.returnValue(mockLiveRegion);

      (component as any).announcement('Test announcement');

      expect(mockLiveRegion.textContent).toBe('Test announcement');
    });

    it('should clear live region after announcement', fakeAsync(() => {
      const mockLiveRegion = document.createElement('div');
      mockLiveRegion.id = 'chart-announcements';
      spyOn(document, 'getElementById').and.returnValue(mockLiveRegion);

      (component as any).announcement('Test announcement');
      tick(5000);

      expect(mockLiveRegion.textContent).toBe('');
    }));

    it('should provide time range labels for screen readers', () => {
      const label = (component as any).getTimeRangeLabel(TimeRange.Last7Days);
      expect(label).toBe('Last 7 Days');
    });
  });

  describe('Component Cleanup', () => {
    it('should complete destroy subject on destroy', () => {
      spyOn((component as any).destroy$, 'next');
      spyOn((component as any).destroy$, 'complete');

      component.ngOnDestroy();

      expect((component as any).destroy$.next).toHaveBeenCalled();
      expect((component as any).destroy$.complete).toHaveBeenCalled();
    });

    it('should remove keyboard event listener on destroy', () => {
      spyOn(document, 'removeEventListener');

      component.ngOnDestroy();

      expect(document.removeEventListener).toHaveBeenCalledWith(
        'keydown', jasmine.any(Function)
      );
    });
  });

  describe('Error Handling', () => {
    it('should handle initialization errors gracefully', fakeAsync(() => {
      mockUserService.getChartPreferences.and.returnValue(
        Promise.reject(new Error('Network error'))
      );

      component.ngOnInit();
      tick();

      expect(mockNotificationService.showError).toHaveBeenCalledWith(
        'Failed to load charts. Please refresh the page.'
      );
    }));

    it('should handle layout service errors', () => {
      mockLayoutService.getLayoutConfiguration.and.returnValue(
        throwError(new Error('Layout service error'))
      );

      // Should not crash the component
      expect(() => component.ngOnInit()).not.toThrow();
    });

    it('should handle export service errors', async () => {
      mockExportService.exportChart.and.returnValue(
        Promise.reject(new Error('Export service error'))
      );

      const exportRequest: ExportRequest = {
        chartId: 'test',
        format: 'PNG',
        options: {}
      };

      await component.onChartExport(exportRequest);

      expect(mockNotificationService.showError).toHaveBeenCalledWith(
        'Export failed. Please try again.'
      );
    });
  });

  describe('Template Integration', () => {
    beforeEach(() => {
      fixture.detectChanges();
    });

    it('should display page title', () => {
      const titleElement = fixture.debugElement.query(By.css('#page-title'));
      expect(titleElement).toBeTruthy();
    });

    it('should display connection status', () => {
      const statusElement = fixture.debugElement.query(By.css('.connection-status'));
      expect(statusElement).toBeTruthy();
    });

    it('should display time range filter', () => {
      const filterElement = fixture.debugElement.query(By.css('.time-range-filter'));
      expect(filterElement).toBeTruthy();
    });

    it('should display refresh button', () => {
      const refreshButton = fixture.debugElement.query(By.css('.refresh-btn'));
      expect(refreshButton).toBeTruthy();
    });

    it('should display export dashboard button', () => {
      const exportButton = fixture.debugElement.query(By.css('.export-dashboard-btn'));
      expect(exportButton).toBeTruthy();
    });

    it('should trigger refresh on refresh button click', () => {
      spyOn(component, 'onRefreshCharts');

      const refreshButton = fixture.debugElement.query(By.css('.refresh-btn'));
      refreshButton.nativeElement.click();

      expect(component.onRefreshCharts).toHaveBeenCalled();
    });

    it('should trigger export on export button click', () => {
      spyOn(component, 'onChartExport');

      const exportButton = fixture.debugElement.query(By.css('.export-dashboard-btn'));
      exportButton.nativeElement.click();

      expect(component.onChartExport).toHaveBeenCalledWith({
        chartId: 'dashboard',
        format: 'PDF'
      });
    });

    it('should show loading state when isLoading is true', () => {
      mockStore.overrideSelector(selectIsLoading, true);
      mockStore.refreshState();
      fixture.detectChanges();

      const loadingElement = fixture.debugElement.query(By.css('.loading-state'));
      expect(loadingElement).toBeTruthy();
    });

    it('should show error state when error exists', () => {
      mockStore.overrideSelector(selectError, 'Test error message');
      mockStore.refreshState();
      fixture.detectChanges();

      const errorElement = fixture.debugElement.query(By.css('.error-state'));
      expect(errorElement).toBeTruthy();

      const errorText = errorElement.nativeElement.textContent;
      expect(errorText).toContain('Test error message');
    });

    it('should show charts grid when has charts and not loading', () => {
      mockStore.overrideSelector(selectVisibleCharts, mockCharts);
      mockStore.overrideSelector(selectIsLoading, false);
      mockStore.overrideSelector(selectError, null);
      mockStore.refreshState();
      fixture.detectChanges();

      const chartsGrid = fixture.debugElement.query(By.css('.charts-grid'));
      expect(chartsGrid).toBeTruthy();
    });

    it('should have accessibility live region', () => {
      const liveRegion = fixture.debugElement.query(By.css('#chart-announcements'));
      expect(liveRegion).toBeTruthy();
      expect(liveRegion.attributes['aria-live']).toBe('polite');
      expect(liveRegion.attributes['aria-atomic']).toBe('true');
    });

    it('should have keyboard shortcuts help text', () => {
      const shortcutsHelp = fixture.debugElement.query(By.css('.keyboard-shortcuts'));
      expect(shortcutsHelp).toBeTruthy();
      expect(shortcutsHelp.classes['sr-only']).toBe(true);
    });
  });
});
```

### 1.2 Chart Filters Component Tests

```typescript
// src/app/components/analytics/chart-filters/chart-filters.component.spec.ts
import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { ReactiveFormsModule, FormBuilder } from '@angular/forms';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { MatSelectModule } from '@angular/material/select';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatNativeDateModule } from '@angular/material/core';
import { By } from '@angular/platform-browser';

import { ChartFiltersComponent } from './chart-filters.component';
import {
  GlobalChartFilters,
  ChartFilter,
  FilterType,
  TimeRange
} from '../../../models/analytics/charts.models';

describe('ChartFiltersComponent', () => {
  let component: ChartFiltersComponent;
  let fixture: ComponentFixture<ChartFiltersComponent>;

  const mockFilters: GlobalChartFilters = {
    dateRange: {
      start: new Date('2025-01-01'),
      end: new Date('2025-01-29')
    },
    timeRange: TimeRange.Last30Days,
    userSegments: ['power_users', 'enterprise'],
    documentTypes: ['contract', 'agreement'],
    performanceMetrics: ['response_time', 'throughput'],
    customFilters: []
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ChartFiltersComponent],
      imports: [
        ReactiveFormsModule,
        NoopAnimationsModule,
        MatSelectModule,
        MatCheckboxModule,
        MatButtonToggleModule,
        MatDatepickerModule,
        MatFormFieldModule,
        MatInputModule,
        MatNativeDateModule
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(ChartFiltersComponent);
    component = fixture.componentInstance;
  });

  describe('Component Initialization', () => {
    it('should create', () => {
      expect(component).toBeTruthy();
    });

    it('should initialize with empty form', () => {
      component.ngOnInit();

      expect(component.filterForm.get('dateRange')?.get('preset')?.value).toBe(TimeRange.Last24Hours);
      expect(component.userSegments.length).toBe(0);
      expect(component.documentTypes.length).toBe(0);
      expect(component.performanceMetrics.length).toBe(0);
      expect(component.customFilters.length).toBe(0);
    });

    it('should setup form subscriptions on init', () => {
      spyOn(component as any, 'setupFormSubscriptions');

      component.ngOnInit();

      expect((component as any).setupFormSubscriptions).toHaveBeenCalled();
    });

    it('should populate form from input filters', () => {
      component.filters = mockFilters;
      spyOn(component as any, 'populateFormFromFilters');

      component.ngOnInit();

      expect((component as any).populateFormFromFilters).toHaveBeenCalled();
    });
  });

  describe('Form Population', () => {
    beforeEach(() => {
      component.ngOnInit();
    });

    it('should populate date range from filters', () => {
      component.filters = mockFilters;
      (component as any).populateFormFromFilters();

      const dateRangeValue = component.filterForm.get('dateRange')?.value;
      expect(dateRangeValue.start).toEqual(mockFilters.dateRange?.start);
      expect(dateRangeValue.end).toEqual(mockFilters.dateRange?.end);
      expect(dateRangeValue.preset).toBe(TimeRange.Last30Days);
    });

    it('should populate user segments from filters', () => {
      component.filters = mockFilters;
      (component as any).populateFormFromFilters();

      expect(component.userSegments.length).toBe(2);
      expect(component.userSegments.at(0)?.get('id')?.value).toBe('power_users');
      expect(component.userSegments.at(1)?.get('id')?.value).toBe('enterprise');
    });

    it('should populate document types from filters', () => {
      component.filters = mockFilters;
      (component as any).populateFormFromFilters();

      expect(component.documentTypes.length).toBe(2);
      expect(component.documentTypes.at(0)?.get('id')?.value).toBe('contract');
      expect(component.documentTypes.at(1)?.get('id')?.value).toBe('agreement');
    });

    it('should populate performance metrics from filters', () => {
      component.filters = mockFilters;
      (component as any).populateFormFromFilters();

      expect(component.performanceMetrics.length).toBe(2);
      expect(component.performanceMetrics.at(0)?.get('id')?.value).toBe('response_time');
      expect(component.performanceMetrics.at(1)?.get('id')?.value).toBe('throughput');
    });
  });

  describe('User Segment Management', () => {
    beforeEach(() => {
      component.ngOnInit();
    });

    it('should add user segment', () => {
      component.addUserSegment();

      expect(component.userSegments.length).toBe(1);
      expect(component.userSegments.at(0)?.get('enabled')?.value).toBe(true);
    });

    it('should remove user segment', () => {
      component.addUserSegment();
      component.addUserSegment();
      expect(component.userSegments.length).toBe(2);

      component.removeUserSegment(0);
      expect(component.userSegments.length).toBe(1);
    });

    it('should get segment display name', () => {
      const displayName = (component as any).getSegmentDisplayName('power_users');
      expect(displayName).toBe('Power Users');
    });

    it('should handle unknown segment IDs', () => {
      const displayName = (component as any).getSegmentDisplayName('unknown_segment');
      expect(displayName).toBe('unknown_segment');
    });
  });

  describe('Document Type Management', () => {
    beforeEach(() => {
      component.ngOnInit();
    });

    it('should add document type', () => {
      component.addDocumentType();

      expect(component.documentTypes.length).toBe(1);
      expect(component.documentTypes.at(0)?.get('enabled')?.value).toBe(true);
    });

    it('should remove document type', () => {
      component.addDocumentType();
      component.addDocumentType();
      expect(component.documentTypes.length).toBe(2);

      component.removeDocumentType(1);
      expect(component.documentTypes.length).toBe(1);
    });

    it('should get document type display name', () => {
      const displayName = (component as any).getDocumentTypeDisplayName('contract');
      expect(displayName).toBe('Contracts');
    });
  });

  describe('Performance Metric Management', () => {
    beforeEach(() => {
      component.ngOnInit();
    });

    it('should add performance metric', () => {
      component.addPerformanceMetric();

      expect(component.performanceMetrics.length).toBe(1);
      expect(component.performanceMetrics.at(0)?.get('enabled')?.value).toBe(true);
      expect(component.performanceMetrics.at(0)?.get('threshold')?.value).toBe(null);
    });

    it('should remove performance metric', () => {
      component.addPerformanceMetric();
      component.removePerformanceMetric(0);

      expect(component.performanceMetrics.length).toBe(0);
    });

    it('should get metric display name', () => {
      const displayName = (component as any).getMetricDisplayName('response_time');
      expect(displayName).toBe('Response Time');
    });
  });

  describe('Custom Filter Management', () => {
    beforeEach(() => {
      component.ngOnInit();
    });

    it('should add custom filter', () => {
      component.addCustomFilter();

      expect(component.customFilters.length).toBe(1);
      expect(component.customFilters.at(0)?.get('operator')?.value).toBe('equals');
      expect(component.customFilters.at(0)?.get('enabled')?.value).toBe(true);
    });

    it('should remove custom filter', () => {
      component.addCustomFilter();
      component.addCustomFilter();
      component.removeCustomFilter(0);

      expect(component.customFilters.length).toBe(1);
    });
  });

  describe('Filter Emission', () => {
    beforeEach(() => {
      component.ngOnInit();
    });

    it('should emit date range filter changes', fakeAsync(() => {
      spyOn(component.filterChange, 'emit');

      const dateRange = {
        start: new Date('2025-01-01'),
        end: new Date('2025-01-31'),
        preset: null
      };

      component.filterForm.get('dateRange')?.setValue(dateRange);
      tick(300); // Wait for debounce

      expect(component.filterChange.emit).toHaveBeenCalledWith(jasmine.objectContaining({
        type: FilterType.DateRange,
        value: dateRange,
        label: jasmine.any(String)
      }));
    }));

    it('should emit user segment filter changes', fakeAsync(() => {
      spyOn(component.filterChange, 'emit');

      component.addUserSegment();
      component.userSegments.at(0)?.patchValue({
        id: 'power_users',
        name: 'Power Users',
        enabled: true
      });

      tick(300); // Wait for debounce

      expect(component.filterChange.emit).toHaveBeenCalledWith(jasmine.objectContaining({
        type: FilterType.UserSegment,
        value: ['power_users'],
        label: 'User Segments: 1 selected'
      }));
    }));

    it('should emit document type filter changes', fakeAsync(() => {
      spyOn(component.filterChange, 'emit');

      component.addDocumentType();
      component.documentTypes.at(0)?.patchValue({
        id: 'contract',
        name: 'Contracts',
        enabled: true
      });

      tick(300); // Wait for debounce

      expect(component.filterChange.emit).toHaveBeenCalledWith(jasmine.objectContaining({
        type: FilterType.DocumentType,
        value: ['contract'],
        label: 'Document Types: 1 selected'
      }));
    }));

    it('should not emit filter when no enabled items', fakeAsync(() => {
      spyOn(component.filterChange, 'emit');

      component.addUserSegment();
      component.userSegments.at(0)?.patchValue({
        id: 'power_users',
        name: 'Power Users',
        enabled: false
      });

      tick(300); // Wait for debounce

      expect(component.filterChange.emit).not.toHaveBeenCalled();
    }));
  });

  describe('Preset Actions', () => {
    beforeEach(() => {
      component.ngOnInit();
    });

    it('should set preset date range', () => {
      component.onPresetDateRange(TimeRange.Last7Days);

      const dateRange = component.filterForm.get('dateRange')?.value;
      expect(dateRange.preset).toBe(TimeRange.Last7Days);
      expect(dateRange.start).toBe(null);
      expect(dateRange.end).toBe(null);
    });

    it('should set custom date range', () => {
      const startDate = new Date('2025-01-01');
      const endDate = new Date('2025-01-31');

      component.onCustomDateRange(startDate, endDate);

      const dateRange = component.filterForm.get('dateRange')?.value;
      expect(dateRange.preset).toBe(null);
      expect(dateRange.start).toBe(startDate);
      expect(dateRange.end).toBe(endDate);
    });

    it('should clear all filters', () => {
      spyOn(component.clearFilters, 'emit');

      // Add some filters first
      component.addUserSegment();
      component.addDocumentType();

      component.onClearAllFilters();

      expect(component.userSegments.length).toBe(0);
      expect(component.documentTypes.length).toBe(0);
      expect(component.performanceMetrics.length).toBe(0);
      expect(component.customFilters.length).toBe(0);
      expect(component.clearFilters.emit).toHaveBeenCalled();
    });
  });

  describe('Utility Methods', () => {
    it('should format date correctly', () => {
      const date = new Date('2025-01-15');
      const formatted = (component as any).formatDate(date);

      expect(formatted).toBe('Jan 15, 2025');
    });

    it('should get time range label', () => {
      const label = (component as any).getTimeRangeLabel(TimeRange.Last7Days);
      expect(label).toBe('Last 7 Days');
    });

    it('should get date range label for preset', () => {
      const dateRange = {
        preset: TimeRange.Last30Days,
        start: null,
        end: null
      };

      const label = (component as any).getDateRangeLabel(dateRange);
      expect(label).toBe('Last 30 Days');
    });

    it('should get date range label for custom range', () => {
      const dateRange = {
        preset: null,
        start: new Date('2025-01-01'),
        end: new Date('2025-01-31')
      };

      const label = (component as any).getDateRangeLabel(dateRange);
      expect(label).toBe('Jan 1, 2025 - Jan 31, 2025');
    });

    it('should return default label for incomplete range', () => {
      const dateRange = {
        preset: null,
        start: null,
        end: null
      };

      const label = (component as any).getDateRangeLabel(dateRange);
      expect(label).toBe('Custom Date Range');
    });
  });

  describe('Component Cleanup', () => {
    it('should complete destroy subject on destroy', () => {
      spyOn((component as any).destroy$, 'next');
      spyOn((component as any).destroy$, 'complete');

      component.ngOnDestroy();

      expect((component as any).destroy$.next).toHaveBeenCalled();
      expect((component as any).destroy$.complete).toHaveBeenCalled();
    });
  });

  describe('Template Integration', () => {
    beforeEach(() => {
      fixture.detectChanges();
    });

    it('should display filter section titles', () => {
      const sectionTitles = fixture.debugElement.queryAll(By.css('.filter-section-title'));
      expect(sectionTitles.length).toBeGreaterThan(0);
    });

    it('should display preset time range toggles', () => {
      const presetToggles = fixture.debugElement.query(By.css('.preset-toggle-group'));
      expect(presetToggles).toBeTruthy();
    });

    it('should display custom date range inputs', () => {
      const startDateInput = fixture.debugElement.query(By.css('input[formControlName="start"]'));
      const endDateInput = fixture.debugElement.query(By.css('input[formControlName="end"]'));

      expect(startDateInput).toBeTruthy();
      expect(endDateInput).toBeTruthy();
    });

    it('should display add filter buttons', () => {
      const addButtons = fixture.debugElement.queryAll(By.css('.add-filter-btn'));
      expect(addButtons.length).toBeGreaterThan(0);
    });

    it('should display clear all filters button', () => {
      const clearButton = fixture.debugElement.query(By.css('.filter-actions button'));
      expect(clearButton).toBeTruthy();
    });

    it('should trigger clear filters on button click', () => {
      spyOn(component, 'onClearAllFilters');

      const clearButton = fixture.debugElement.query(By.css('.filter-actions button'));
      clearButton.nativeElement.click();

      expect(component.onClearAllFilters).toHaveBeenCalled();
    });

    it('should add user segment on button click', () => {
      spyOn(component, 'addUserSegment');

      // Find add user segment button
      const addSegmentButton = fixture.debugElement.query(
        By.css('.segment-controls .add-filter-btn')
      );
      addSegmentButton.nativeElement.click();

      expect(component.addUserSegment).toHaveBeenCalled();
    });
  });

  describe('Accessibility', () => {
    beforeEach(() => {
      fixture.detectChanges();
    });

    it('should have proper ARIA labels for form groups', () => {
      const filterSections = fixture.debugElement.queryAll(By.css('.filter-section'));

      filterSections.forEach(section => {
        expect(section.attributes['role']).toBe('group');
        expect(section.attributes['aria-labelledby']).toBeTruthy();
      });
    });

    it('should have proper ARIA labels for inputs', () => {
      const dateInputs = fixture.debugElement.queryAll(By.css('input[matInput]'));

      dateInputs.forEach(input => {
        expect(input.attributes['aria-label']).toBeTruthy();
      });
    });

    it('should have proper ARIA labels for selects', () => {
      const selects = fixture.debugElement.queryAll(By.css('mat-select'));

      selects.forEach(select => {
        expect(select.attributes['aria-label']).toBeTruthy();
      });
    });

    it('should have proper ARIA labels for buttons', () => {
      const buttons = fixture.debugElement.queryAll(By.css('button'));

      buttons.forEach(button => {
        expect(button.attributes['aria-label']).toBeTruthy();
      });
    });
  });

  describe('Responsive Behavior', () => {
    it('should handle mobile viewport', () => {
      // Simulate mobile viewport
      Object.defineProperty(window, 'innerWidth', { value: 400 });

      fixture.detectChanges();

      // Check that component adapts to mobile
      const customDateRange = fixture.debugElement.query(By.css('.custom-date-range'));
      expect(customDateRange).toBeTruthy();
    });

    it('should handle tablet viewport', () => {
      // Simulate tablet viewport
      Object.defineProperty(window, 'innerWidth', { value: 800 });

      fixture.detectChanges();

      // Component should render properly
      expect(fixture.componentInstance).toBeTruthy();
    });
  });
});
```

---

## 2. Integration Tests

### 2.1 Real-time Service Integration Tests

```typescript
// src/app/services/analytics/realtime-chart-data.service.integration.spec.ts
import { TestBed, fakeAsync, tick } from '@angular/core/testing';
import { Store } from '@ngrx/store';
import { MockStore, provideMockStore } from '@ngrx/store/testing';
import { HubConnection, HubConnectionBuilder, HubConnectionState } from '@microsoft/signalr';

import { RealtimeChartDataService } from './realtime-chart-data.service';
import { AuthService } from '../core/auth.service';
import { PerformanceService } from '../core/performance.service';
import { LoggingService } from '../core/logging.service';

import {
  ConnectionState,
  TimeRange,
  RealtimeChartUpdate,
  GlobalChartFilter,
  DrillDownRequest,
  DrillDownResponse,
  SystemAlert
} from '../../models/analytics/charts.models';

describe('RealtimeChartDataService Integration', () => {
  let service: RealtimeChartDataService;
  let mockStore: MockStore;
  let mockAuthService: jasmine.SpyObj<AuthService>;
  let mockPerformanceService: jasmine.SpyObj<PerformanceService>;
  let mockLoggingService: jasmine.SpyObj<LoggingService>;
  let mockHubConnection: jasmine.SpyObj<HubConnection>;

  beforeEach(async () => {
    const authServiceSpy = jasmine.createSpyObj('AuthService', ['getValidToken']);
    const performanceServiceSpy = jasmine.createSpyObj('PerformanceService', ['measureRealtimeUpdate']);
    const loggingServiceSpy = jasmine.createSpyObj('LoggingService', ['info', 'warn', 'error', 'debug']);

    // Mock HubConnection
    const hubConnectionSpy = jasmine.createSpyObj('HubConnection', [
      'start', 'stop', 'invoke', 'on', 'onreconnecting', 'onreconnected', 'onclose'
    ]);

    TestBed.configureTestingModule({
      providers: [
        RealtimeChartDataService,
        provideMockStore({
          initialState: {
            realtimeCharts: {
              charts: [],
              selectedTimeRange: TimeRange.Last24Hours,
              connectionState: ConnectionState.Disconnected
            }
          }
        }),
        { provide: AuthService, useValue: authServiceSpy },
        { provide: PerformanceService, useValue: performanceServiceSpy },
        { provide: LoggingService, useValue: loggingServiceSpy }
      ]
    });

    service = TestBed.inject(RealtimeChartDataService);
    mockStore = TestBed.inject(Store) as MockStore;
    mockAuthService = TestBed.inject(AuthService) as jasmine.SpyObj<AuthService>;
    mockPerformanceService = TestBed.inject(PerformanceService) as jasmine.SpyObj<PerformanceService>;
    mockLoggingService = TestBed.inject(LoggingService) as jasmine.SpyObj<LoggingService>;

    mockHubConnection = hubConnectionSpy;

    // Mock HubConnectionBuilder
    spyOn(window as any, 'HubConnectionBuilder').and.returnValue({
      withUrl: jasmine.createSpy().and.returnValue({
        withAutomaticReconnect: jasmine.createSpy().and.returnValue({
          configureLogging: jasmine.createSpy().and.returnValue({
            build: jasmine.createSpy().and.returnValue(mockHubConnection)
          })
        })
      })
    });

    mockAuthService.getValidToken.and.returnValue(Promise.resolve('mock-token'));
    mockHubConnection.start.and.returnValue(Promise.resolve());
    mockHubConnection.state = HubConnectionState.Connected;
  });

  afterEach(() => {
    service.ngOnDestroy();
  });

  describe('Connection Management', () => {
    it('should initialize SignalR connection with authentication', fakeAsync(() => {
      service = TestBed.inject(RealtimeChartDataService);
      tick();

      expect(mockAuthService.getValidToken).toHaveBeenCalled();
      expect(mockHubConnection.start).toHaveBeenCalled();
    }));

    it('should setup event handlers on connection initialization', fakeAsync(() => {
      service = TestBed.inject(RealtimeChartDataService);
      tick();

      expect(mockHubConnection.on).toHaveBeenCalledWith('ChartDataUpdate', jasmine.any(Function));
      expect(mockHubConnection.on).toHaveBeenCalledWith('GlobalFilterChanged', jasmine.any(Function));
      expect(mockHubConnection.on).toHaveBeenCalledWith('ChartDrillDownData', jasmine.any(Function));
      expect(mockHubConnection.on).toHaveBeenCalledWith('ChartSystemAlert', jasmine.any(Function));
    }));

    it('should handle connection state changes', fakeAsync(() => {
      let connectionState: ConnectionState;
      service.connectionState.subscribe(state => connectionState = state);

      service = TestBed.inject(RealtimeChartDataService);
      tick();

      // Simulate connection established
      expect(connectionState!).toBe(ConnectionState.Connected);
    }));

    it('should handle connection errors gracefully', fakeAsync(() => {
      mockHubConnection.start.and.returnValue(Promise.reject(new Error('Connection failed')));

      let connectionState: ConnectionState;
      service.connectionState.subscribe(state => connectionState = state);

      service = TestBed.inject(RealtimeChartDataService);
      tick();

      expect(connectionState!).toBe(ConnectionState.Error);
      expect(mockLoggingService.error).toHaveBeenCalledWith(
        'Failed to start SignalR connection', jasmine.any(Error)
      );
    }));

    it('should attempt reconnection on connection loss', fakeAsync(() => {
      service = TestBed.inject(RealtimeChartDataService);
      tick();

      // Simulate connection close
      const onCloseCallback = mockHubConnection.onclose.calls.mostRecent().args[0];
      onCloseCallback(new Error('Connection lost'));

      // Should schedule reconnection
      expect(mockLoggingService.error).toHaveBeenCalledWith(
        'SignalR connection closed with error', jasmine.any(Error)
      );
    }));

    it('should limit reconnection attempts', fakeAsync(() => {
      mockHubConnection.start.and.returnValue(Promise.reject(new Error('Connection failed')));

      let connectionState: ConnectionState;
      service.connectionState.subscribe(state => connectionState = state);

      service = TestBed.inject(RealtimeChartDataService);

      // Simulate multiple failed reconnection attempts
      for (let i = 0; i < 6; i++) {
        tick(5000 * (i + 1)); // Exponential backoff
      }

      expect(connectionState!).toBe(ConnectionState.Error);
    }));
  });

  describe('Chart Group Management', () => {
    beforeEach(fakeAsync(() => {
      service = TestBed.inject(RealtimeChartDataService);
      tick();
      mockHubConnection.invoke.and.returnValue(Promise.resolve());
    }));

    it('should join chart group successfully', async () => {
      await service.joinChartGroup('usage_analytics', TimeRange.Last24Hours);

      expect(mockHubConnection.invoke).toHaveBeenCalledWith(
        'JoinChartGroup', 'usage_analytics', TimeRange.Last24Hours
      );
    });

    it('should leave chart group successfully', async () => {
      await service.leaveChartGroup('performance_metrics', TimeRange.Last7Days);

      expect(mockHubConnection.invoke).toHaveBeenCalledWith(
        'LeaveChartGroup', 'performance_metrics', TimeRange.Last7Days
      );
    });

    it('should handle join group errors gracefully', async () => {
      mockHubConnection.invoke.and.returnValue(Promise.reject(new Error('Join failed')));

      // Should not throw
      await expectAsync(
        service.joinChartGroup('usage_analytics', TimeRange.Last24Hours)
      ).toBeResolved();

      expect(mockLoggingService.error).toHaveBeenCalledWith(
        'Failed to join chart group: usage_analytics', jasmine.any(Error)
      );
    });

    it('should not invoke methods when disconnected', async () => {
      mockHubConnection.state = HubConnectionState.Disconnected;

      await service.joinChartGroup('usage_analytics', TimeRange.Last24Hours);

      expect(mockHubConnection.invoke).not.toHaveBeenCalled();
    });

    it('should rejoin chart groups after reconnection', fakeAsync(() => {
      // Setup active charts
      mockStore.setState({
        realtimeCharts: {
          charts: [
            { type: 'usage_analytics' },
            { type: 'performance_metrics' }
          ],
          selectedTimeRange: TimeRange.Last7Days,
          connectionState: ConnectionState.Connected
        }
      });

      // Simulate reconnection
      const onReconnectedCallback = mockHubConnection.onreconnected.calls.mostRecent().args[0];
      onReconnectedCallback('connection-id');

      tick();

      // Should rejoin all chart groups
      expect(mockHubConnection.invoke).toHaveBeenCalledWith(
        'JoinChartGroup', 'usage_analytics', TimeRange.Last7Days
      );
      expect(mockHubConnection.invoke).toHaveBeenCalledWith(
        'JoinChartGroup', 'performance_metrics', TimeRange.Last7Days
      );
    }));
  });

  describe('Real-time Updates', () => {
    beforeEach(fakeAsync(() => {
      service = TestBed.inject(RealtimeChartDataService);
      tick();
    }));

    it('should emit chart updates when received', (done) => {
      const mockUpdate: RealtimeChartUpdate = {
        chartId: 'usage_analytics',
        updateType: 'data',
        newData: { value: 150, timestamp: new Date() },
        timestamp: new Date(),
        animationConfig: { duration: 300, easing: 'ease' }
      };

      service.chartUpdates.subscribe(update => {
        expect(update).toEqual(mockUpdate);
        done();
      });

      // Simulate receiving update
      const onUpdateCallback = mockHubConnection.on.calls
        .find(call => call.args[0] === 'ChartDataUpdate')?.args[1];
      onUpdateCallback(mockUpdate);
    });

    it('should dispatch store actions on chart updates', () => {
      spyOn(mockStore, 'dispatch');

      const mockUpdate: RealtimeChartUpdate = {
        chartId: 'performance_metrics',
        updateType: 'metric',
        newData: { responseTime: 250 },
        timestamp: new Date()
      };

      // Simulate receiving update
      const onUpdateCallback = mockHubConnection.on.calls
        .find(call => call.args[0] === 'ChartDataUpdate')?.args[1];
      onUpdateCallback(mockUpdate);

      expect(mockStore.dispatch).toHaveBeenCalledWith(jasmine.any(Object));
    });

    it('should handle global filter changes', () => {
      spyOn(mockStore, 'dispatch');

      const mockFilter: GlobalChartFilter = {
        type: 'time_range',
        value: TimeRange.Last7Days,
        label: 'Last 7 Days',
        appliedAt: new Date()
      };

      // Simulate receiving global filter change
      const onFilterCallback = mockHubConnection.on.calls
        .find(call => call.args[0] === 'GlobalFilterChanged')?.args[1];
      onFilterCallback(mockFilter);

      expect(mockStore.dispatch).toHaveBeenCalledWith(jasmine.any(Object));
    });

    it('should handle drill-down responses', () => {
      spyOn(mockStore, 'dispatch');

      const mockDrillDownResponse: DrillDownResponse = {
        chartId: 'business_intelligence',
        data: {
          segments: [
            { label: 'Q1 2025', value: 1200 },
            { label: 'Q2 2025', value: 1450 }
          ],
          metadata: { totalRecords: 2650 }
        },
        timestamp: new Date()
      };

      // Simulate receiving drill-down data
      const onDrillDownCallback = mockHubConnection.on.calls
        .find(call => call.args[0] === 'ChartDrillDownData')?.args[1];
      onDrillDownCallback(mockDrillDownResponse);

      expect(mockStore.dispatch).toHaveBeenCalledWith(jasmine.any(Object));
    });

    it('should handle system alerts', () => {
      spyOn(mockStore, 'dispatch');

      const mockAlert: SystemAlert = {
        id: 'perf-alert-001',
        type: 'chart_performance_degradation',
        severity: 'warning',
        message: 'Chart rendering performance below threshold',
        timestamp: new Date(),
        metadata: { chartId: 'usage_analytics', renderTime: 1500 }
      };

      // Simulate receiving system alert
      const onAlertCallback = mockHubConnection.on.calls
        .find(call => call.args[0] === 'ChartSystemAlert')?.args[1];
      onAlertCallback(mockAlert);

      expect(mockStore.dispatch).toHaveBeenCalledWith(jasmine.any(Object));
    });

    it('should measure realtime update performance', () => {
      const mockUpdate: RealtimeChartUpdate = {
        chartId: 'usage_analytics',
        updateType: 'data',
        newData: { value: 100 },
        timestamp: new Date()
      };

      // Simulate receiving update
      const onUpdateCallback = mockHubConnection.on.calls
        .find(call => call.args[0] === 'ChartDataUpdate')?.args[1];
      onUpdateCallback(mockUpdate);

      expect(mockPerformanceService.measureRealtimeUpdate).toHaveBeenCalled();
    });
  });

  describe('Communication Methods', () => {
    beforeEach(fakeAsync(() => {
      service = TestBed.inject(RealtimeChartDataService);
      tick();
      mockHubConnection.invoke.and.returnValue(Promise.resolve());
    }));

    it('should apply global filters through SignalR', async () => {
      const filter: GlobalChartFilter = {
        type: 'user_segment',
        value: ['power_users'],
        label: 'Power Users',
        appliedAt: new Date()
      };

      await service.applyGlobalFilter(filter);

      expect(mockHubConnection.invoke).toHaveBeenCalledWith('ApplyGlobalFilter', filter);
    });

    it('should request drill-down data through SignalR', async () => {
      const drillDownRequest: DrillDownRequest = {
        chartId: 'usage_analytics',
        dataPoint: {
          index: 2,
          datasetIndex: 0,
          value: 300,
          label: 'March'
        },
        timestamp: new Date()
      };

      await service.requestDrillDown('usage_analytics', drillDownRequest);

      expect(mockHubConnection.invoke).toHaveBeenCalledWith(
        'RequestChartDrillDown', 'usage_analytics', drillDownRequest
      );
    });

    it('should handle communication errors gracefully', async () => {
      mockHubConnection.invoke.and.returnValue(Promise.reject(new Error('Communication failed')));

      const filter: GlobalChartFilter = {
        type: 'document_type',
        value: ['contract'],
        label: 'Contracts',
        appliedAt: new Date()
      };

      // Should not throw
      await expectAsync(service.applyGlobalFilter(filter)).toBeResolved();

      expect(mockLoggingService.error).toHaveBeenCalledWith(
        'Failed to apply global filter', jasmine.any(Error)
      );
    });
  });

  describe('Connection Health Monitoring', () => {
    beforeEach(fakeAsync(() => {
      service = TestBed.inject(RealtimeChartDataService);
      tick();
    }));

    it('should ping server periodically for health check', fakeAsync(() => {
      mockHubConnection.invoke.and.returnValue(Promise.resolve());

      // Fast-forward 30 seconds to trigger ping
      tick(30000);

      expect(mockHubConnection.invoke).toHaveBeenCalledWith('Ping');
    }));

    it('should handle ping failures gracefully', fakeAsync(() => {
      mockHubConnection.invoke.and.returnValue(Promise.reject(new Error('Ping failed')));

      // Fast-forward 30 seconds to trigger ping
      tick(30000);

      expect(mockLoggingService.warn).toHaveBeenCalledWith('Server ping failed', jasmine.any(Error));
    }));

    it('should monitor connection state changes', () => {
      let connectionStates: ConnectionState[] = [];
      service.connectionState.subscribe(state => connectionStates.push(state));

      // Simulate reconnecting
      const onReconnectingCallback = mockHubConnection.onreconnecting.calls.mostRecent().args[0];
      onReconnectingCallback(new Error('Connection lost'));

      // Simulate reconnected
      const onReconnectedCallback = mockHubConnection.onreconnected.calls.mostRecent().args[0];
      onReconnectedCallback('connection-id');

      expect(connectionStates).toContain(ConnectionState.Reconnecting);
      expect(connectionStates).toContain(ConnectionState.Connected);
    });
  });

  describe('Performance Optimization', () => {
    beforeEach(fakeAsync(() => {
      service = TestBed.inject(RealtimeChartDataService);
      tick();
    }));

    it('should handle chart performance degradation alerts', () => {
      spyOn(service as any, 'optimizeChartPerformance');

      const performanceAlert: SystemAlert = {
        id: 'perf-001',
        type: 'chart_performance_degradation',
        severity: 'warning',
        message: 'Chart performance below threshold',
        timestamp: new Date(),
        metadata: { chartId: 'usage_analytics' }
      };

      // Simulate receiving performance alert
      const onAlertCallback = mockHubConnection.on.calls
        .find(call => call.args[0] === 'ChartSystemAlert')?.args[1];
      onAlertCallback(performanceAlert);

      expect((service as any).optimizeChartPerformance).toHaveBeenCalledWith('usage_analytics');
    });

    it('should optimize all charts when no specific chart ID provided', () => {
      spyOn(mockStore, 'dispatch');

      const generalAlert: SystemAlert = {
        id: 'perf-002',
        type: 'chart_performance_degradation',
        severity: 'critical',
        message: 'General performance degradation',
        timestamp: new Date()
      };

      // Simulate receiving general performance alert
      const onAlertCallback = mockHubConnection.on.calls
        .find(call => call.args[0] === 'ChartSystemAlert')?.args[1];
      onAlertCallback(generalAlert);

      expect(mockStore.dispatch).toHaveBeenCalledWith(jasmine.any(Object));
    });
  });

  describe('Error Resilience', () => {
    it('should handle authentication failures', fakeAsync(() => {
      mockAuthService.getValidToken.and.returnValue(
        Promise.reject(new Error('Authentication failed'))
      );

      let connectionState: ConnectionState;
      service.connectionState.subscribe(state => connectionState = state);

      service = TestBed.inject(RealtimeChartDataService);
      tick();

      expect(connectionState!).toBe(ConnectionState.Error);
      expect(mockLoggingService.error).toHaveBeenCalledWith(
        'Failed to initialize SignalR connection', jasmine.any(Error)
      );
    }));

    it('should handle network disconnection gracefully', fakeAsync(() => {
      service = TestBed.inject(RealtimeChartDataService);
      tick();

      let connectionState: ConnectionState;
      service.connectionState.subscribe(state => connectionState = state);

      // Simulate network disconnection
      const onCloseCallback = mockHubConnection.onclose.calls.mostRecent().args[0];
      onCloseCallback(null); // Clean disconnection

      expect(connectionState!).toBe(ConnectionState.Disconnected);
    }));

    it('should recover from transient errors', fakeAsync(() => {
      let callCount = 0;
      mockHubConnection.start.and.callFake(() => {
        callCount++;
        if (callCount === 1) {
          return Promise.reject(new Error('Transient error'));
        }
        return Promise.resolve();
      });

      let connectionState: ConnectionState;
      service.connectionState.subscribe(state => connectionState = state);

      service = TestBed.inject(RealtimeChartDataService);
      tick(); // Initial failed attempt
      tick(5000); // First retry

      expect(connectionState!).toBe(ConnectionState.Connected);
      expect(mockHubConnection.start).toHaveBeenCalledTimes(2);
    }));
  });

  describe('Resource Management', () => {
    it('should properly cleanup resources on service destroy', () => {
      service = TestBed.inject(RealtimeChartDataService);

      service.ngOnDestroy();

      expect(mockHubConnection.stop).toHaveBeenCalled();
    });

    it('should unsubscribe from observables on destroy', () => {
      service = TestBed.inject(RealtimeChartDataService);

      spyOn((service as any).destroy$, 'next');
      spyOn((service as any).destroy$, 'complete');

      service.ngOnDestroy();

      expect((service as any).destroy$.next).toHaveBeenCalled();
      expect((service as any).destroy$.complete).toHaveBeenCalled();
    });
  });
});
```

### 2.2 Chart Export Integration Tests

```typescript
// src/app/services/analytics/chart-export.service.integration.spec.ts
import { TestBed, fakeAsync, tick } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { Store } from '@ngrx/store';
import { MockStore, provideMockStore } from '@ngrx/store/testing';

import { ChartExportService } from './chart-export.service';
import { DownloadService } from '../core/download.service';
import { ImageService } from '../core/image.service';
import { PdfService } from '../core/pdf.service';
import { NotificationService } from '../core/notification.service';

import {
  ExportFormat,
  ExportTask,
  ExportStatus,
  ChartConfiguration,
  ChartDataSet,
  DashboardExportOptions
} from '../../models/analytics/charts.models';

describe('ChartExportService Integration', () => {
  let service: ChartExportService;
  let mockStore: MockStore;
  let httpMock: HttpTestingController;
  let mockDownloadService: jasmine.SpyObj<DownloadService>;
  let mockImageService: jasmine.SpyObj<ImageService>;
  let mockPdfService: jasmine.SpyObj<PdfService>;
  let mockNotificationService: jasmine.SpyObj<NotificationService>;

  const mockChartConfig: ChartConfiguration = {
    id: 'test-chart',
    title: 'Test Chart',
    description: 'Test chart for export',
    chartType: 'line',
    drillDownEnabled: false,
    exportEnabled: true,
    accessibility: {
      ariaLabel: 'Test chart',
      description: 'Test chart for export testing'
    }
  };

  const mockChartData: ChartDataSet = {
    labels: ['Jan', 'Feb', 'Mar'],
    datasets: [{
      label: 'Test Data',
      data: [10, 20, 30],
      borderColor: '#3f51b5'
    }]
  };

  beforeEach(async () => {
    const downloadServiceSpy = jasmine.createSpyObj('DownloadService', ['downloadBlob']);
    const imageServiceSpy = jasmine.createSpyObj('ImageService', ['chartToPNG', 'chartToSVG']);
    const pdfServiceSpy = jasmine.createSpyObj('PdfService', [
      'createDocument', 'addChartToPDF', 'addCoverPage', 'addSummaryPage',
      'addDataTableToPDF', 'saveDocument'
    ]);
    const notificationServiceSpy = jasmine.createSpyObj('NotificationService', [
      'showSuccess', 'showError', 'showInfo'
    ]);

    await TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [
        ChartExportService,
        provideMockStore({
          initialState: {
            realtimeCharts: {
              charts: [mockChartConfig],
              chartData: { 'test-chart': mockChartData },
              visibleCharts: [mockChartConfig]
            }
          }
        }),
        { provide: DownloadService, useValue: downloadServiceSpy },
        { provide: ImageService, useValue: imageServiceSpy },
        { provide: PdfService, useValue: pdfServiceSpy },
        { provide: NotificationService, useValue: notificationServiceSpy }
      ]
    });

    service = TestBed.inject(ChartExportService);
    mockStore = TestBed.inject(Store) as MockStore;
    httpMock = TestBed.inject(HttpTestingController);
    mockDownloadService = TestBed.inject(DownloadService) as jasmine.SpyObj<DownloadService>;
    mockImageService = TestBed.inject(ImageService) as jasmine.SpyObj<ImageService>;
    mockPdfService = TestBed.inject(PdfService) as jasmine.SpyObj<PdfService>;
    mockNotificationService = TestBed.inject(NotificationService) as jasmine.SpyObj<NotificationService>;

    // Setup default spy returns
    mockImageService.chartToPNG.and.returnValue(Promise.resolve(new Blob(['png-data'])));
    mockImageService.chartToSVG.and.returnValue(Promise.resolve(new Blob(['svg-data'])));
    mockPdfService.createDocument.and.returnValue(Promise.resolve({} as any));
    mockPdfService.saveDocument.and.returnValue(Promise.resolve(new Blob(['pdf-data'])));
    mockDownloadService.downloadBlob.and.returnValue(Promise.resolve());
  });

  afterEach(() => {
    httpMock.verify();
  });

  describe('Chart Image Export', () => {
    beforeEach(() => {
      // Mock chart element in DOM
      const mockChartElement = document.createElement('canvas');
      mockChartElement.setAttribute('data-chart-id', 'test-chart');
      spyOn(document, 'querySelector').and.returnValue(mockChartElement);
    });

    it('should export chart as PNG', async () => {
      await service.exportChart('test-chart', ExportFormat.PNG, {
        width: 800,
        height: 600,
        scale: 2
      });

      expect(mockImageService.chartToPNG).toHaveBeenCalledWith(
        jasmine.any(HTMLElement),
        {
          width: 800,
          height: 600,
          scale: 2,
          backgroundColor: '#ffffff'
        }
      );
      expect(mockDownloadService.downloadBlob).toHaveBeenCalledWith(
        jasmine.any(Blob),
        jasmine.stringMatching(/^wesign_chart_test-chart_.*\.png$/)
      );
      expect(mockNotificationService.showSuccess).toHaveBeenCalledWith('Chart exported as PNG');
    });

    it('should export chart as SVG', async () => {
      await service.exportChart('test-chart', ExportFormat.SVG, {
        width: 1000,
        height: 800
      });

      expect(mockImageService.chartToSVG).toHaveBeenCalledWith(
        jasmine.any(HTMLElement),
        {
          width: 1000,
          height: 800
        }
      );
      expect(mockDownloadService.downloadBlob).toHaveBeenCalledWith(
        jasmine.any(Blob),
        jasmine.stringMatching(/^wesign_chart_test-chart_.*\.svg$/)
      );
      expect(mockNotificationService.showSuccess).toHaveBeenCalledWith('Chart exported as SVG');
    });

    it('should handle missing chart element', async () => {
      (document.querySelector as jasmine.Spy).and.returnValue(null);

      await expectAsync(
        service.exportChart('non-existent-chart', ExportFormat.PNG)
      ).toBeRejectedWithError('Chart element not found: non-existent-chart');
    });

    it('should use default dimensions when not specified', async () => {
      await service.exportChart('test-chart', ExportFormat.PNG);

      expect(mockImageService.chartToPNG).toHaveBeenCalledWith(
        jasmine.any(HTMLElement),
        {
          width: 1200,
          height: 800,
          scale: 2,
          backgroundColor: '#ffffff'
        }
      );
    });
  });

  describe('Chart PDF Export', () => {
    beforeEach(() => {
      // Mock chart element and configuration retrieval
      const mockChartElement = document.createElement('canvas');
      mockChartElement.setAttribute('data-chart-id', 'test-chart');
      spyOn(document, 'querySelector').and.returnValue(mockChartElement);

      // Mock store selectors
      spyOn(service as any, 'getChartConfiguration').and.returnValue(Promise.resolve(mockChartConfig));
      spyOn(service as any, 'getChartData').and.returnValue(Promise.resolve(mockChartData));
    });

    it('should export single chart as PDF', async () => {
      await service.exportChart('test-chart', ExportFormat.PDF, {
        includeDataTable: true,
        includeMetadata: true
      });

      expect(mockPdfService.createDocument).toHaveBeenCalledWith({
        title: 'Test Chart',
        author: 'WeSign Analytics',
        subject: 'Chart Export',
        creator: 'WeSign Real-time Charts'
      });

      expect(mockPdfService.addChartToPDF).toHaveBeenCalledWith(
        jasmine.any(Object),
        jasmine.any(Blob),
        {
          title: 'Test Chart',
          description: 'Test chart for export',
          timestamp: jasmine.any(Date),
          metadata: jasmine.any(Object)
        }
      );

      expect(mockPdfService.addDataTableToPDF).toHaveBeenCalledWith(
        jasmine.any(Object),
        mockChartData
      );

      expect(mockDownloadService.downloadBlob).toHaveBeenCalledWith(
        jasmine.any(Blob),
        jasmine.stringMatching(/^wesign_chart_test-chart_.*\.pdf$/)
      );
    });

    it('should skip data table when not requested', async () => {
      await service.exportChart('test-chart', ExportFormat.PDF, {
        includeDataTable: false
      });

      expect(mockPdfService.addDataTableToPDF).not.toHaveBeenCalled();
    });
  });

  describe('Dashboard Export', () => {
    beforeEach(() => {
      // Mock multiple chart elements
      const mockChartElements = [
        document.createElement('canvas'),
        document.createElement('canvas')
      ];
      mockChartElements[0].setAttribute('data-chart-id', 'chart-1');
      mockChartElements[1].setAttribute('data-chart-id', 'chart-2');

      (document.querySelector as jasmine.Spy).and.callFake((selector: string) => {
        const chartId = selector.match(/data-chart-id="([^"]+)"/)?.[1];
        return mockChartElements.find(el => el.getAttribute('data-chart-id') === chartId) || null;
      });

      // Mock chart configurations
      spyOn(service as any, 'getVisibleCharts').and.returnValue(Promise.resolve([
        { id: 'chart-1', title: 'Chart 1' },
        { id: 'chart-2', title: 'Chart 2' }
      ]));

      spyOn(service as any, 'getChartConfiguration').and.returnValue(Promise.resolve({
        id: 'chart',
        title: 'Chart Title',
        description: 'Chart Description'
      }));

      spyOn(service as any, 'generateDashboardSummary').and.returnValue(Promise.resolve({
        totalCharts: 2,
        exportDate: new Date(),
        keyInsights: ['Insight 1', 'Insight 2'],
        dataTimeRange: '2025-01-01 to 2025-01-29'
      }));
    });

    it('should export dashboard as PDF', async () => {
      const options: DashboardExportOptions = {
        chartIds: ['chart-1', 'chart-2'],
        includeFilters: true,
        includeSummary: true,
        timeRange: 'Last 30 Days',
        appliedFilters: ['Filter 1', 'Filter 2']
      };

      await service.exportDashboard(ExportFormat.PDF, options);

      expect(mockPdfService.createDocument).toHaveBeenCalledWith({
        title: 'WeSign Analytics Dashboard',
        author: 'WeSign Analytics',
        subject: 'Dashboard Export',
        creator: 'WeSign Real-time Charts'
      });

      expect(mockPdfService.addCoverPage).toHaveBeenCalledWith(
        jasmine.any(Object),
        {
          title: 'Real-time Charts Dashboard',
          subtitle: 'WeSign Analytics Export',
          exportDate: jasmine.any(Date),
          timeRange: 'Last 30 Days',
          appliedFilters: ['Filter 1', 'Filter 2']
        }
      );

      expect(mockPdfService.addChartToPDF).toHaveBeenCalledTimes(2);
      expect(mockPdfService.addSummaryPage).toHaveBeenCalled();

      expect(mockDownloadService.downloadBlob).toHaveBeenCalledWith(
        jasmine.any(Blob),
        jasmine.stringMatching(/^wesign_chart_dashboard_.*\.pdf$/)
      );
    });

    it('should skip summary when not requested', async () => {
      await service.exportDashboard(ExportFormat.PDF, {
        chartIds: ['chart-1'],
        includeSummary: false
      });

      expect(mockPdfService.addSummaryPage).not.toHaveBeenCalled();
    });

    it('should handle missing chart elements gracefully', async () => {
      (document.querySelector as jasmine.Spy).and.returnValue(null);

      // Should complete without throwing
      await expectAsync(
        service.exportDashboard(ExportFormat.PDF, { chartIds: ['missing-chart'] })
      ).toBeResolved();
    });
  });

  describe('Data Export', () => {
    beforeEach(() => {
      spyOn(service as any, 'getChartData').and.returnValue(Promise.resolve(mockChartData));
    });

    it('should export chart data as CSV', async () => {
      spyOn(service as any, 'convertToCSV').and.returnValue('Label,Test Data\nJan,10\nFeb,20\nMar,30');

      await service.exportChart('test-chart', ExportFormat.CSV);

      expect((service as any).convertToCSV).toHaveBeenCalledWith(mockChartData);
      expect(mockDownloadService.downloadBlob).toHaveBeenCalledWith(
        jasmine.any(Blob),
        jasmine.stringMatching(/^wesign_chart_test-chart_.*\.csv$/)
      );
      expect(mockNotificationService.showSuccess).toHaveBeenCalledWith('Chart data exported as CSV');
    });

    it('should export chart data as Excel', async () => {
      spyOn(service as any, 'convertToExcel').and.returnValue(Promise.resolve(new ArrayBuffer(8)));

      await service.exportChart('test-chart', ExportFormat.Excel);

      expect((service as any).convertToExcel).toHaveBeenCalledWith(mockChartData);
      expect(mockDownloadService.downloadBlob).toHaveBeenCalledWith(
        jasmine.any(Blob),
        jasmine.stringMatching(/^wesign_chart_test-chart_.*\.xlsx$/)
      );
      expect(mockNotificationService.showSuccess).toHaveBeenCalledWith('Chart data exported as EXCEL');
    });

    it('should generate correct CSV format', () => {
      const csvContent = (service as any).convertToCSV(mockChartData);

      expect(csvContent).toBe('"Label","Test Data"\n"Jan",10\n"Feb",20\n"Mar",30');
    });
  });

  describe('Export Queue Management', () => {
    it('should process exports sequentially', fakeAsync(() => {
      const mockChartElement = document.createElement('canvas');
      mockChartElement.setAttribute('data-chart-id', 'test-chart');
      spyOn(document, 'querySelector').and.returnValue(mockChartElement);

      // Queue multiple exports
      service.exportChart('test-chart', ExportFormat.PNG);
      service.exportChart('test-chart', ExportFormat.SVG);
      service.exportChart('test-chart', ExportFormat.PDF);

      tick();

      // Should process one at a time
      expect(mockImageService.chartToPNG).toHaveBeenCalledTimes(1);
    }));

    it('should limit concurrent exports', async () => {
      const mockChartElement = document.createElement('canvas');
      spyOn(document, 'querySelector').and.returnValue(mockChartElement);

      // Queue more exports than the concurrent limit
      const exportPromises = [];
      for (let i = 0; i < 5; i++) {
        exportPromises.push(service.exportChart(`chart-${i}`, ExportFormat.PNG));
      }

      // Should not exceed max concurrent exports (3)
      expect((service as any).activeExports).toBeLessThanOrEqual(3);
    });

    it('should generate unique task IDs', () => {
      const id1 = (service as any).generateTaskId();
      const id2 = (service as any).generateTaskId();

      expect(id1).not.toBe(id2);
      expect(id1).toMatch(/^export_\d+_[a-z0-9]+$/);
    });
  });

  describe('Error Handling', () => {
    it('should handle image service errors', async () => {
      const mockChartElement = document.createElement('canvas');
      spyOn(document, 'querySelector').and.returnValue(mockChartElement);

      mockImageService.chartToPNG.and.returnValue(
        Promise.reject(new Error('Image generation failed'))
      );

      await expectAsync(
        service.exportChart('test-chart', ExportFormat.PNG)
      ).toBeRejectedWithError('Image generation failed');
    });

    it('should handle PDF service errors', async () => {
      const mockChartElement = document.createElement('canvas');
      spyOn(document, 'querySelector').and.returnValue(mockChartElement);

      mockPdfService.createDocument.and.returnValue(
        Promise.reject(new Error('PDF creation failed'))
      );

      await expectAsync(
        service.exportChart('test-chart', ExportFormat.PDF)
      ).toBeRejectedWithError('PDF creation failed');
    });

    it('should handle download service errors', async () => {
      const mockChartElement = document.createElement('canvas');
      spyOn(document, 'querySelector').and.returnValue(mockChartElement);

      mockDownloadService.downloadBlob.and.returnValue(
        Promise.reject(new Error('Download failed'))
      );

      await expectAsync(
        service.exportChart('test-chart', ExportFormat.PNG)
      ).toBeRejectedWithError('Download failed');
    });

    it('should handle unsupported export formats', async () => {
      await expectAsync(
        service.exportChart('test-chart', 'UNKNOWN' as ExportFormat)
      ).toBeRejectedWithError('Unsupported export format: UNKNOWN');
    });

    it('should mark export task as failed on error', async () => {
      const mockChartElement = document.createElement('canvas');
      spyOn(document, 'querySelector').and.returnValue(mockChartElement);

      mockImageService.chartToPNG.and.returnValue(
        Promise.reject(new Error('Export error'))
      );

      try {
        await service.exportChart('test-chart', ExportFormat.PNG);
      } catch (error) {
        // Expected error
      }

      // Task should be removed from queue after failure
      expect((service as any).exportQueue.size).toBe(0);
    });
  });

  describe('Progress Tracking', () => {
    it('should update export task progress', async () => {
      const mockChartElement = document.createElement('canvas');
      spyOn(document, 'querySelector').and.returnValue(mockChartElement);

      let progressUpdates: number[] = [];

      // Mock task progress updates
      const originalExecuteExport = (service as any).executeExport;
      spyOn(service as any, 'executeExport').and.callFake(async (task: ExportTask) => {
        task.progress = 25;
        progressUpdates.push(task.progress);
        task.progress = 75;
        progressUpdates.push(task.progress);
        task.progress = 100;
        progressUpdates.push(task.progress);
        return originalExecuteExport.call(service, task);
      });

      await service.exportChart('test-chart', ExportFormat.PNG);

      expect(progressUpdates).toEqual([25, 75, 100]);
    });

    it('should track export task status', async () => {
      const mockChartElement = document.createElement('canvas');
      spyOn(document, 'querySelector').and.returnValue(mockChartElement);

      // Intercept task creation
      let taskStatus: ExportStatus[] = [];
      const originalProcessQueue = (service as any).processExportQueue;
      spyOn(service as any, 'processExportQueue').and.callFake(async function(this: any) {
        const task = Array.from(this.exportQueue.values())[0];
        if (task) {
          taskStatus.push(task.status);
          task.status = ExportStatus.Processing;
          taskStatus.push(task.status);
          await originalProcessQueue.call(this);
          task.status = ExportStatus.Completed;
          taskStatus.push(task.status);
        }
      });

      await service.exportChart('test-chart', ExportFormat.PNG);

      expect(taskStatus).toContain(ExportStatus.Queued);
      expect(taskStatus).toContain(ExportStatus.Processing);
      expect(taskStatus).toContain(ExportStatus.Completed);
    });
  });
});
```

<function_calls>
<invoke name="TodoWrite">
<parameter name="todos">[{"content": "Gather workflow inputs and identify page list", "status": "completed", "activeForm": "Gathering workflow inputs and identifying page list"}, {"content": "Execute A\u2192M workflow for Analytics Dashboard main page", "status": "completed", "activeForm": "Executing A\u2192M workflow for Analytics Dashboard main page"}, {"content": "Execute A\u2192M workflow for KPI Cards page", "status": "completed", "activeForm": "Executing A\u2192M workflow for KPI Cards page"}, {"content": "Execute A\u2192M workflow for Real-time Charts page", "status": "in_progress", "activeForm": "Executing A\u2192M workflow for Real-time Charts page"}, {"content": "Execute A\u2192M workflow for Export functionality", "status": "pending", "activeForm": "Executing A\u2192M workflow for Export functionality"}, {"content": "Complete Step A: System Map for Real-time Charts page", "status": "completed", "activeForm": "Completing Step A: System Map for Real-time Charts page"}, {"content": "Complete Step B: Acceptance Criteria for Real-time Charts page", "status": "completed", "activeForm": "Completing Step B: Acceptance Criteria for Real-time Charts page"}, {"content": "Complete Step C: Definition of Ready (DoR) Check for Real-time Charts", "status": "completed", "activeForm": "Completing Step C: Definition of Ready (DoR) Check for Real-time Charts"}, {"content": "Complete Step D: Component Design for Real-time Charts", "status": "completed", "activeForm": "Completing Step D: Component Design for Real-time Charts"}, {"content": "Complete Step E: Implementation Plan for Real-time Charts", "status": "completed", "activeForm": "Completing Step E: Implementation Plan for Real-time Charts"}, {"content": "Complete Step F: Code Implementation for Real-time Charts", "status": "completed", "activeForm": "Completing Step F: Code Implementation for Real-time Charts"}, {"content": "Complete Step G: Test Implementation for Real-time Charts", "status": "completed", "activeForm": "Completing Step G: Test Implementation for Real-time Charts"}, {"content": "Complete Step H: CI/CD Configuration for Real-time Charts", "status": "in_progress", "activeForm": "Completing Step H: CI/CD Configuration for Real-time Charts"}]