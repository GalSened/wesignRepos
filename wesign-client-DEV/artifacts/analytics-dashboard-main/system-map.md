# System Map - Analytics Dashboard Main Page

## Page Identification
- **Page Name**: Analytics Dashboard Main Page
- **Page Key**: analytics-dashboard-main
- **Route**: `/dashboard/analytics`

## Component Architecture

```
UI Layer (Angular 15.2.10)
├── AnalyticsDashboardComponent
│   ├── KpiCardsComponent
│   ├── UsageChartsComponent
│   ├── SegmentationChartsComponent
│   └── ProcessFlowComponent
├── AnalyticsApiService
├── AnalyticsLoadingService
└── AnalyticsErrorHandlerService
```

## Data Flow Diagram

```
[Browser] ←→ [Angular App] ←→ [SignalR Hub] ←→ [Analytics API] ←→ [SQL Views] ←→ [WeSign DB]
    ↑              ↑                                   ↑                    ↑
    │              │                                   │                    │
    │         [Service Worker]                    [S3 Cache]         [Materialized Views]
    │              │                                   │                    │
[Local Storage] [IndexedDB]                      [Data Lake]         [Document Collections]
```

## Files and Dependencies

### Frontend Files
- `src/app/components/dashboard/analytics-dashboard/analytics-dashboard.component.ts`
- `src/app/components/dashboard/analytics-dashboard/analytics-dashboard.component.html`
- `src/app/components/dashboard/analytics-dashboard/analytics-dashboard.component.scss`
- `src/app/services/analytics-api.service.ts`
- `src/app/models/analytics/analytics-models.ts`

### Backend Files
- `src/controllers/AnalyticsController.cs`
- `src/hubs/AnalyticsHub.cs`
- `src/services/analytics/AnalyticsApiService.cs`
- `src/services/analytics/AnalyticsCollectorService.cs`
- `src/database/analytics/01_materialized_views.sql`

### API Endpoints
- `GET /api/analytics/kpi/latest`
- `GET /api/analytics/health`
- `POST /api/analytics/export`
- `WS /analyticsHub` (SignalR)

### Dependencies
- Authentication: JWT token validation
- Authorization: Role-based access (PM, Support, Operations)
- Real-time: SignalR hub connection
- Data: SQL materialized views
- Storage: S3 data lake for time series

### Feature Flags
- `ENABLE_REALTIME_UPDATES`: true
- `ENABLE_HEALTH_MONITORING`: true
- `ENABLE_ROLE_BASED_FILTERING`: true

## Security Considerations
- JWT authentication required
- Role-based data filtering
- PII protection (hashed document IDs for PM role)
- Audit logging for sensitive operations