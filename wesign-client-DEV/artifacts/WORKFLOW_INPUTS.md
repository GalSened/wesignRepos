# WeSign Analytics Dashboard - Workflow Inputs

## System Configuration

**SYSTEM_NAME**: WeSign Analytics Dashboard
**REPO_ROOT**: `C:\Users\gals\Desktop\wesign-client-DEV`
**ENVIRONMENTS**: Dev, DevTest, Prod
**CI_SYSTEM**: Jenkins (to be configured)

## Technology Stack

**UNIT_TEST_STACK**: Angular/Jasmine + Karma
**API_TEST_STACK**: Postman/Newman
**UI_TEST_STACK**: Pytest + Playwright

**Frontend**: Angular 15.2.10 + TypeScript
**Backend**: .NET Core + SignalR
**Database**: SQL Server with materialized views
**Storage**: AWS S3 data lake

## Test Infrastructure Locations

**E2E Tests**: `C:\Users\gals\seleniumpythontests-1\playwright_tests\`
**API Tests**: To be created in `tests/api/`
**Unit Tests**: Angular default in `src/app/` with `.spec.ts` files

## Page List (Ordered for Implementation)

1. **analytics-dashboard-main** - Main analytics dashboard page with real-time updates
2. **kpi-cards** - Real-time KPI cards with animations and status indicators
3. **realtime-charts** - Time series charts with live data updates
4. **export-functionality** - Data export features (CSV, Excel, PDF)
5. **health-monitoring** - System health and connection status monitoring

## PRD Path

**PRD_PATH**: Based on requirements for real-time analytics dashboard with:
- 30-second refresh intervals
- Role-based access control (PM, Support, Operations)
- PII protection with data filtering
- Real-time SignalR connections
- Production-grade performance (<2s load time, <500ms queries)

## Data Sources

**Test Accounts**: WeSign test users with different roles
**Test Data**: Mock DocumentCollections, Users, Templates
**Fixtures**: SQL seed data for analytics scenarios

## Feature Flags

**Analytics Features**:
- `ENABLE_REALTIME_UPDATES`: true
- `ENABLE_EXPORT_FUNCTIONALITY`: true
- `ENABLE_HEALTH_MONITORING`: true
- `ENABLE_ROLE_BASED_FILTERING`: true

## Non-Functional Requirements

**Performance**:
- Dashboard load time < 2 seconds
- API response time < 500ms
- Real-time update latency < 1 second

**Accessibility**: WCAG 2.1 AA compliance
**Security**: JWT authentication, role-based access, PII protection
**Internationalization**: English + Hebrew support with RTL

## Dependencies

**External Services**:
- AWS S3 for data lake
- SignalR hub for real-time connections
- SQL Server for analytics queries

**Internal Dependencies**:
- WeSign authentication service
- User management service
- Document lifecycle service