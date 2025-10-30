-- WeSign Analytics Materialized Views for Real-Time Dashboard
-- Based on actual WeSign codebase data structures
-- These views are optimized for read-replica analytics with minimal performance impact

-- =============================================================================
-- 1. REAL-TIME DOCUMENT FUNNEL VIEW
-- =============================================================================
-- Provides complete document lifecycle based on actual WeSign DocumentCollections model
CREATE MATERIALIZED VIEW [analytics].[mv_document_funnel_realtime]
WITH (DISTRIBUTION = HASH([DocumentCollectionId]))
AS
SELECT
    dc.DocumentCollectionId,
    dc.Name AS DocumentName,
    dc.DocumentStatus,
    dc.Mode AS SignMode,
    dc.CreationTime,
    dc.SignedTime,
    dc.IsWillDeletedIn24Hours,
    dc.UserId AS CreatedByUserId,
    u.GroupId AS OrganizationId,
    u.CompanyId,
    u.Name AS CreatedByUserName,
    u.Email AS CreatedByUserEmail,
    u.CompanyName AS OrganizationName,
    u.GroupName,

    -- Calculate key metrics based on actual DocStatus enum
    CASE
        WHEN dc.DocumentStatus >= 1 THEN 1 ELSE 0  -- Created = 1
    END AS IsCreated,

    CASE
        WHEN dc.DocumentStatus >= 2 THEN 1 ELSE 0  -- Sent = 2
    END AS IsSent,

    CASE
        WHEN dc.DocumentStatus >= 3 THEN 1 ELSE 0  -- Viewed = 3
    END AS IsViewed,

    CASE
        WHEN dc.DocumentStatus = 4 THEN 1 ELSE 0   -- Signed = 4
    END AS IsSigned,

    CASE
        WHEN dc.DocumentStatus = 5 THEN 1 ELSE 0   -- Declined = 5
    END AS IsDeclined,

    CASE
        WHEN dc.DocumentStatus = 6 THEN 1 ELSE 0   -- SendingFailed = 6
    END AS IsSendingFailed,

    CASE
        WHEN dc.DocumentStatus = 8 THEN 1 ELSE 0   -- Canceled = 8
    END AS IsCanceled,

    -- Time-to-sign calculations
    CASE
        WHEN dc.SignedTime IS NOT NULL
        THEN DATEDIFF(MINUTE, dc.CreationTime, dc.SignedTime)
        ELSE NULL
    END AS MinutesToSign,

    -- Device/Platform detection (if available in user agent)
    CASE
        WHEN dc.UserAgent LIKE '%Mobile%' OR dc.UserAgent LIKE '%Android%' OR dc.UserAgent LIKE '%iPhone%'
        THEN 'Mobile'
        WHEN dc.UserAgent LIKE '%Tablet%' OR dc.UserAgent LIKE '%iPad%'
        THEN 'Tablet'
        ELSE 'Desktop'
    END AS DeviceType,

    -- Browser detection
    CASE
        WHEN dc.UserAgent LIKE '%Chrome%' THEN 'Chrome'
        WHEN dc.UserAgent LIKE '%Firefox%' THEN 'Firefox'
        WHEN dc.UserAgent LIKE '%Safari%' THEN 'Safari'
        WHEN dc.UserAgent LIKE '%Edge%' THEN 'Edge'
        ELSE 'Other'
    END AS BrowserType,

    -- Stuck document detection
    CASE
        WHEN dc.DocumentStatus IN (1, 2) -- Sent or Opened but not completed
             AND DATEDIFF(HOUR, dc.CreationTime, GETUTCDATE()) > 24
        THEN 1 ELSE 0
    END AS IsStuckOver24h,

    CASE
        WHEN dc.DocumentStatus IN (1, 2)
             AND DATEDIFF(HOUR, dc.CreationTime, GETUTCDATE()) > 4
        THEN 1 ELSE 0
    END AS IsStuckOver4h,

    -- Template information (if using templates)
    COALESCE(t.TemplateId, 'ad-hoc') AS TemplateId,
    COALESCE(t.TemplateName, 'Ad-hoc Document') AS TemplateName,

    -- Time bucketing for aggregations
    DATEADD(SECOND, (DATEDIFF(SECOND, '2000-01-01', dc.CreationTime) / 30) * 30, '2000-01-01') AS CreatedTime_30s_Bucket,
    DATEADD(MINUTE, (DATEDIFF(MINUTE, '2000-01-01', dc.CreationTime) / 5) * 5, '2000-01-01') AS CreatedTime_5m_Bucket,
    DATEADD(HOUR, DATEDIFF(HOUR, '2000-01-01', dc.CreationTime), '2000-01-01') AS CreatedTime_1h_Bucket,

    -- Signer count and completion tracking
    (SELECT COUNT(*) FROM Signers s WHERE s.DocumentCollectionId = dc.DocumentCollectionId) AS TotalSigners,
    (SELECT COUNT(*) FROM Signers s WHERE s.DocumentCollectionId = dc.DocumentCollectionId AND s.Status = 'Signed') AS CompletedSigners,

    -- Row metadata
    GETUTCDATE() AS LastRefreshed

FROM DocumentCollections dc
INNER JOIN Users u ON dc.UserId = u.Id
LEFT JOIN Templates t ON dc.TemplateId = t.Id
WHERE dc.CreationTime >= DATEADD(DAY, -30, GETUTCDATE()) -- Keep 30 days of hot data
WITH CHECK OPTION;

-- Create clustered index for time-based queries
CREATE CLUSTERED INDEX IX_mv_document_funnel_realtime_CreationTime
ON [analytics].[mv_document_funnel_realtime] (CreationTime, DocumentCollectionId);

-- Create additional indexes for common query patterns
CREATE NONCLUSTERED INDEX IX_mv_document_funnel_realtime_Status
ON [analytics].[mv_document_funnel_realtime] (DocumentStatus, CreationTime)
INCLUDE (OrganizationId, TemplateId, DeviceType);

CREATE NONCLUSTERED INDEX IX_mv_document_funnel_realtime_Organization
ON [analytics].[mv_document_funnel_realtime] (OrganizationId, CreationTime)
INCLUDE (DocumentStatus, SignMode, MinutesToSign);

-- =============================================================================
-- 2. REAL-TIME THROUGHPUT BUCKETS
-- =============================================================================
-- Pre-aggregated data for fast dashboard loading
CREATE MATERIALIZED VIEW [analytics].[mv_throughput_buckets]
WITH (DISTRIBUTION = ROUND_ROBIN)
AS
SELECT
    -- 30-second time buckets for real-time monitoring
    DATEADD(SECOND, (DATEDIFF(SECOND, '2000-01-01', dc.CreationTime) / 30) * 30, '2000-01-01') AS bucket_start_30s,
    DATEADD(SECOND, ((DATEDIFF(SECOND, '2000-01-01', dc.CreationTime) / 30) + 1) * 30, '2000-01-01') AS bucket_end_30s,

    -- 5-minute buckets for trend analysis
    DATEADD(MINUTE, (DATEDIFF(MINUTE, '2000-01-01', dc.CreationTime) / 5) * 5, '2000-01-01') AS bucket_start_5m,

    -- Aggregate metrics per bucket
    COUNT(*) AS documents_created,
    SUM(CASE WHEN dc.DocumentStatus >= 1 THEN 1 ELSE 0 END) AS documents_sent,
    SUM(CASE WHEN dc.DocumentStatus >= 2 THEN 1 ELSE 0 END) AS documents_opened,
    SUM(CASE WHEN dc.DocumentStatus = 3 THEN 1 ELSE 0 END) AS documents_signed,
    SUM(CASE WHEN dc.DocumentStatus = 4 THEN 1 ELSE 0 END) AS documents_declined,
    SUM(CASE WHEN dc.DocumentStatus = 5 THEN 1 ELSE 0 END) AS documents_expired,

    -- Performance metrics
    AVG(CASE WHEN dc.SignedTime IS NOT NULL
        THEN DATEDIFF(MINUTE, dc.CreationTime, dc.SignedTime)
        ELSE NULL END) AS avg_minutes_to_sign,

    PERCENTILE_CONT(0.5) WITHIN GROUP (ORDER BY
        CASE WHEN dc.SignedTime IS NOT NULL
        THEN DATEDIFF(MINUTE, dc.CreationTime, dc.SignedTime)
        ELSE NULL END) AS median_minutes_to_sign,

    PERCENTILE_CONT(0.95) WITHIN GROUP (ORDER BY
        CASE WHEN dc.SignedTime IS NOT NULL
        THEN DATEDIFF(MINUTE, dc.CreationTime, dc.SignedTime)
        ELSE NULL END) AS p95_minutes_to_sign,

    -- Active users in bucket
    COUNT(DISTINCT dc.UserId) AS active_users,
    COUNT(DISTINCT u.GroupId) AS active_organizations,

    -- Device breakdown
    SUM(CASE WHEN dc.UserAgent LIKE '%Mobile%' OR dc.UserAgent LIKE '%Android%' OR dc.UserAgent LIKE '%iPhone%' THEN 1 ELSE 0 END) AS mobile_count,
    SUM(CASE WHEN dc.UserAgent LIKE '%Tablet%' OR dc.UserAgent LIKE '%iPad%' THEN 1 ELSE 0 END) AS tablet_count,
    SUM(CASE WHEN dc.UserAgent NOT LIKE '%Mobile%' AND dc.UserAgent NOT LIKE '%Tablet%' THEN 1 ELSE 0 END) AS desktop_count,

    -- Sign mode breakdown
    SUM(CASE WHEN dc.Mode = 0 THEN 1 ELSE 0 END) AS self_sign_count,
    SUM(CASE WHEN dc.Mode = 1 THEN 1 ELSE 0 END) AS group_sign_count,
    SUM(CASE WHEN dc.Mode = 2 THEN 1 ELSE 0 END) AS distribution_count,
    SUM(CASE WHEN dc.Mode = 3 THEN 1 ELSE 0 END) AS online_sign_count,

    GETUTCDATE() AS last_refreshed

FROM DocumentCollections dc
INNER JOIN Users u ON dc.UserId = u.Id
WHERE dc.CreationTime >= DATEADD(HOUR, -24, GETUTCDATE()) -- Keep 24 hours of hot buckets
GROUP BY
    DATEADD(SECOND, (DATEDIFF(SECOND, '2000-01-01', dc.CreationTime) / 30) * 30, '2000-01-01'),
    DATEADD(MINUTE, (DATEDIFF(MINUTE, '2000-01-01', dc.CreationTime) / 5) * 5, '2000-01-01')
WITH CHECK OPTION;

-- Clustered index on time bucket for fast time-range queries
CREATE CLUSTERED INDEX IX_mv_throughput_buckets_bucket_start
ON [analytics].[mv_throughput_buckets] (bucket_start_30s);

-- =============================================================================
-- 3. ACTIVE USERS TRACKING
-- =============================================================================
-- Daily/Weekly/Monthly active user calculations
CREATE MATERIALIZED VIEW [analytics].[mv_active_users]
WITH (DISTRIBUTION = ROUND_ROBIN)
AS
SELECT
    CAST(GETUTCDATE() AS DATE) as calculation_date,
    'DAU' as metric_type,
    COUNT(DISTINCT dc.UserId) as active_users,
    COUNT(DISTINCT u.GroupId) as active_organizations,
    COUNT(*) as total_documents
FROM DocumentCollections dc
INNER JOIN Users u ON dc.UserId = u.Id
WHERE dc.CreationTime >= CAST(GETUTCDATE() AS DATE) -- Today
GROUP BY CAST(GETUTCDATE() AS DATE)

UNION ALL

SELECT
    CAST(GETUTCDATE() AS DATE) as calculation_date,
    'WAU' as metric_type,
    COUNT(DISTINCT dc.UserId) as active_users,
    COUNT(DISTINCT u.GroupId) as active_organizations,
    COUNT(*) as total_documents
FROM DocumentCollections dc
INNER JOIN Users u ON dc.UserId = u.Id
WHERE dc.CreationTime >= DATEADD(DAY, -7, GETUTCDATE()) -- Last 7 days
GROUP BY CAST(GETUTCDATE() AS DATE)

UNION ALL

SELECT
    CAST(GETUTCDATE() AS DATE) as calculation_date,
    'MAU' as metric_type,
    COUNT(DISTINCT dc.UserId) as active_users,
    COUNT(DISTINCT u.GroupId) as active_organizations,
    COUNT(*) as total_documents
FROM DocumentCollections dc
INNER JOIN Users u ON dc.UserId = u.Id
WHERE dc.CreationTime >= DATEADD(DAY, -30, GETUTCDATE()) -- Last 30 days
GROUP BY CAST(GETUTCDATE() AS DATE)
WITH CHECK OPTION;

-- =============================================================================
-- 4. TEMPLATE ANALYTICS VIEW
-- =============================================================================
-- Template usage and performance analytics
CREATE MATERIALIZED VIEW [analytics].[mv_template_analytics]
WITH (DISTRIBUTION = HASH([TemplateId]))
AS
SELECT
    COALESCE(t.Id, 'ad-hoc') AS TemplateId,
    COALESCE(t.Name, 'Ad-hoc Document') AS TemplateName,
    u.GroupId AS OrganizationId,

    -- Usage metrics
    COUNT(*) AS usage_count_30d,
    COUNT(CASE WHEN dc.CreationTime >= DATEADD(DAY, -7, GETUTCDATE()) THEN 1 END) AS usage_count_7d,
    COUNT(CASE WHEN dc.CreationTime >= DATEADD(DAY, -1, GETUTCDATE()) THEN 1 END) AS usage_count_1d,

    -- Performance metrics
    AVG(CASE WHEN dc.DocumentStatus = 3 THEN 1.0 ELSE 0.0 END) AS completion_rate,
    AVG(CASE WHEN dc.SignedTime IS NOT NULL
        THEN DATEDIFF(MINUTE, dc.CreationTime, dc.SignedTime)
        ELSE NULL END) AS avg_time_to_sign_minutes,

    PERCENTILE_CONT(0.5) WITHIN GROUP (ORDER BY
        CASE WHEN dc.SignedTime IS NOT NULL
        THEN DATEDIFF(MINUTE, dc.CreationTime, dc.SignedTime)
        ELSE NULL END) AS median_time_to_sign_minutes,

    -- Abandonment tracking
    AVG(CASE WHEN dc.DocumentStatus IN (1, 2) AND DATEDIFF(HOUR, dc.CreationTime, GETUTCDATE()) > 24
        THEN 1.0 ELSE 0.0 END) AS abandonment_rate,

    -- Last usage
    MAX(dc.CreationTime) AS last_used,

    -- Complexity estimation (based on average time to sign)
    CASE
        WHEN AVG(CASE WHEN dc.SignedTime IS NOT NULL
            THEN DATEDIFF(MINUTE, dc.CreationTime, dc.SignedTime)
            ELSE NULL END) <= 5 THEN 'Simple'
        WHEN AVG(CASE WHEN dc.SignedTime IS NOT NULL
            THEN DATEDIFF(MINUTE, dc.CreationTime, dc.SignedTime)
            ELSE NULL END) <= 30 THEN 'Medium'
        ELSE 'Complex'
    END AS complexity_category,

    GETUTCDATE() AS last_refreshed

FROM DocumentCollections dc
INNER JOIN Users u ON dc.UserId = u.Id
LEFT JOIN Templates t ON dc.TemplateId = t.Id
WHERE dc.CreationTime >= DATEADD(DAY, -30, GETUTCDATE())
GROUP BY
    COALESCE(t.Id, 'ad-hoc'),
    COALESCE(t.Name, 'Ad-hoc Document'),
    u.GroupId
WITH CHECK OPTION;

-- Create index for template queries
CREATE CLUSTERED INDEX IX_mv_template_analytics_TemplateId
ON [analytics].[mv_template_analytics] (TemplateId, OrganizationId);

-- =============================================================================
-- 5. ORGANIZATION ANALYTICS VIEW
-- =============================================================================
-- Multi-tenant organization performance tracking
CREATE MATERIALIZED VIEW [analytics].[mv_organization_analytics]
WITH (DISTRIBUTION = HASH([OrganizationId]))
AS
SELECT
    u.GroupId AS OrganizationId,
    g.Name AS OrganizationName,

    -- Volume metrics
    COUNT(*) AS documents_count_30d,
    COUNT(CASE WHEN dc.CreationTime >= DATEADD(DAY, -7, GETUTCDATE()) THEN 1 END) AS documents_count_7d,
    COUNT(CASE WHEN dc.CreationTime >= DATEADD(DAY, -1, GETUTCDATE()) THEN 1 END) AS documents_count_1d,

    -- User activity
    COUNT(DISTINCT dc.UserId) AS active_users_30d,
    COUNT(DISTINCT CASE WHEN dc.CreationTime >= DATEADD(DAY, -7, GETUTCDATE()) THEN dc.UserId END) AS active_users_7d,

    -- Performance metrics
    AVG(CASE WHEN dc.DocumentStatus = 3 THEN 1.0 ELSE 0.0 END) AS success_rate,
    AVG(CASE WHEN dc.SignedTime IS NOT NULL
        THEN DATEDIFF(MINUTE, dc.CreationTime, dc.SignedTime)
        ELSE NULL END) AS avg_time_to_sign_minutes,

    -- Growth calculation (30d vs previous 30d)
    (COUNT(*) * 1.0 / NULLIF(
        (SELECT COUNT(*)
         FROM DocumentCollections dc2
         INNER JOIN Users u2 ON dc2.UserId = u2.Id
         WHERE u2.GroupId = u.GroupId
         AND dc2.CreationTime >= DATEADD(DAY, -60, GETUTCDATE())
         AND dc2.CreationTime < DATEADD(DAY, -30, GETUTCDATE())), 0) - 1) * 100 AS growth_rate_30d_percent,

    -- Tier classification based on volume
    CASE
        WHEN COUNT(*) >= 1000 THEN 'Enterprise'
        WHEN COUNT(*) >= 100 THEN 'Business'
        ELSE 'Standard'
    END AS organization_tier,

    -- Primary use case detection
    CASE
        WHEN SUM(CASE WHEN dc.Mode = 0 THEN 1 ELSE 0 END) > COUNT(*) * 0.6 THEN 'Self-Sign Heavy'
        WHEN SUM(CASE WHEN dc.Mode = 1 THEN 1 ELSE 0 END) > COUNT(*) * 0.6 THEN 'Group-Sign Heavy'
        WHEN SUM(CASE WHEN dc.Mode = 2 THEN 1 ELSE 0 END) > COUNT(*) * 0.6 THEN 'Distribution Heavy'
        ELSE 'Mixed Usage'
    END AS primary_use_case,

    GETUTCDATE() AS last_refreshed

FROM DocumentCollections dc
INNER JOIN Users u ON dc.UserId = u.Id
INNER JOIN Groups g ON u.GroupId = g.Id
WHERE dc.CreationTime >= DATEADD(DAY, -30, GETUTCDATE())
GROUP BY u.GroupId, g.Name
WITH CHECK OPTION;

-- Create index for organization queries
CREATE CLUSTERED INDEX IX_mv_organization_analytics_OrganizationId
ON [analytics].[mv_organization_analytics] (OrganizationId);

-- =============================================================================
-- REFRESH SCHEDULE SETUP
-- =============================================================================
-- Set up automatic refresh for materialized views
-- These should be scheduled to refresh every 30 seconds for real-time analytics

-- Create refresh stored procedure
CREATE PROCEDURE [analytics].[sp_refresh_realtime_views]
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        -- Refresh in dependency order
        EXEC sp_refreshview '[analytics].[mv_active_users]';
        EXEC sp_refreshview '[analytics].[mv_template_analytics]';
        EXEC sp_refreshview '[analytics].[mv_organization_analytics]';
        EXEC sp_refreshview '[analytics].[mv_throughput_buckets]';
        EXEC sp_refreshview '[analytics].[mv_document_funnel_realtime]';

        -- Log successful refresh
        INSERT INTO [analytics].[refresh_log] (refresh_time, status, message)
        VALUES (GETUTCDATE(), 'SUCCESS', 'All materialized views refreshed successfully');

    END TRY
    BEGIN CATCH
        -- Log error
        INSERT INTO [analytics].[refresh_log] (refresh_time, status, message, error_details)
        VALUES (
            GETUTCDATE(),
            'ERROR',
            'Failed to refresh materialized views',
            ERROR_MESSAGE()
        );

        -- Re-throw error for monitoring
        THROW;
    END CATCH
END;

-- Create refresh log table for monitoring
CREATE TABLE [analytics].[refresh_log] (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
    refresh_time DATETIME2 NOT NULL,
    status VARCHAR(20) NOT NULL,
    message NVARCHAR(500),
    error_details NVARCHAR(MAX),
    duration_ms INT
);

-- Create index on refresh log for monitoring queries
CREATE INDEX IX_refresh_log_time_status
ON [analytics].[refresh_log] (refresh_time DESC, status);