Feature: Analytics Dashboard Main Page – Real-time KPI monitoring and insights

Background:
  Given the user is authenticated with WeSign
  And the user has appropriate role permissions (ProductManager, Support, or Operations)
  And the analytics system is healthy and collecting data

Scenario: ProductManager views dashboard with real-time updates
  Given I am logged in as a ProductManager
  When I navigate to "/dashboard/analytics"
  Then I should see the analytics dashboard load within 2 seconds
  And I should see KPI cards for DAU, MAU, success rate, and time to sign
  And I should see "Real-time connected" status indicator
  And I should see data age indicator showing "fresh" status
  And document IDs should be hashed for privacy protection
  And the dashboard should auto-refresh every 30 seconds
  And I should see trend indicators for each KPI

Scenario: Support user accesses dashboard with limited PII
  Given I am logged in as a Support user
  When I navigate to "/dashboard/analytics"
  Then I should see the analytics dashboard
  And I should see limited PII access with audit logging
  And stuck documents should show organization names but hashed document IDs
  And I should have access to export functionality

Scenario: Real-time data updates via SignalR
  Given I am on the analytics dashboard
  And the SignalR connection is established
  When new analytics data is published
  Then I should see KPI values update automatically within 1 second
  And updated values should animate/highlight to show changes
  And the "last updated" timestamp should refresh
  And the connection status should remain "connected"

Scenario: Dashboard loads with cached data when real-time is unavailable
  Given the SignalR connection is unavailable
  When I load the analytics dashboard
  Then I should see cached KPI data within 2 seconds
  And I should see "Disconnected" status indicator
  And I should see automatic retry attempts
  And data should still be functional via polling fallback

Scenario: Data freshness monitoring and alerts
  Given I am on the analytics dashboard
  When data age exceeds 90 seconds
  Then I should see data freshness indicator change to "stale"
  And I should see a warning indicator
  When data age exceeds 5 minutes
  Then I should see data freshness indicator change to "error"
  And I should see system health status as "warning" or "critical"

Scenario: Export functionality with role-based data
  Given I am on the analytics dashboard
  When I click the "Export" dropdown
  Then I should see export options for CSV, Excel, and PDF
  When I select "CSV" export
  Then a CSV file should download within 10 seconds
  And the CSV should contain filtered data based on my role
  And sensitive data should be appropriately masked

Scenario: Mobile responsive design
  Given I am accessing the dashboard on a mobile device
  When I load the analytics dashboard
  Then the layout should adapt to mobile screen size
  And all KPI cards should be readable and functional
  And touch interactions should work properly
  And charts should be scrollable horizontally if needed

Scenario: Accessibility compliance
  Given I am using a screen reader
  When I navigate the analytics dashboard
  Then all KPI values should be announced with proper labels
  And trend indicators should be described meaningfully
  And keyboard navigation should work through all interactive elements
  And color-blind users should be able to distinguish status indicators

Scenario: Error handling and recovery
  Given I am on the analytics dashboard
  When the API returns an error
  Then I should see a user-friendly error message
  And the dashboard should attempt automatic recovery
  And I should have a manual "Retry" option
  And partial data should still be displayed if available

Scenario: Internationalization (Hebrew RTL support)
  Given I have Hebrew language selected
  When I load the analytics dashboard
  Then all text should display in Hebrew
  And the layout should be right-to-left
  And numbers and dates should format correctly for Hebrew locale
  And charts and graphs should adapt to RTL layout

Scenario: Performance requirements validation
  Given I am loading the analytics dashboard
  When the page starts loading
  Then the initial view should appear within 2 seconds
  And all KPI data should load within 5 seconds
  And real-time updates should have <1 second latency
  And memory usage should remain stable during extended use