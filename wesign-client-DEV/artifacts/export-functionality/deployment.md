# Export Functionality - Deployment (Step L)

## Deployment Strategy

### Overview
Comprehensive deployment configuration for the WeSign Analytics Export functionality using containerized microservices architecture with Kubernetes orchestration and progressive rollout strategies.

### Deployment Architecture

```yaml
# Microservices Architecture
services:
  export-api:
    description: "REST API for export request management"
    technology: "Node.js/Express + TypeScript"
    scaling: "Horizontal (3-10 replicas)"

  export-processor:
    description: "Background workers for data processing"
    technology: "Node.js Worker Threads"
    scaling: "Vertical (CPU-intensive)"

  export-storage:
    description: "File storage and download service"
    technology: "MinIO/S3 compatible storage"
    scaling: "Storage-based autoscaling"

  export-queue:
    description: "Message queue for async processing"
    technology: "Redis/Bull Queue"
    scaling: "Redis Cluster"
```

## Environment Configurations

### Development Environment

```yaml
# docker-compose.dev.yml
version: '3.8'
services:
  export-api-dev:
    build:
      context: .
      dockerfile: Dockerfile.dev
    ports:
      - "3000:3000"
    environment:
      - NODE_ENV=development
      - EXPORT_DB_HOST=localhost
      - EXPORT_DB_NAME=wesign_exports_dev
      - EXPORT_REDIS_HOST=localhost
      - EXPORT_STORAGE_TYPE=local
      - EXPORT_LOG_LEVEL=debug
    volumes:
      - ./src:/app/src
      - ./exports:/app/exports
    depends_on:
      - postgres-dev
      - redis-dev

  postgres-dev:
    image: postgres:15-alpine
    environment:
      POSTGRES_DB: wesign_exports_dev
      POSTGRES_USER: export_user
      POSTGRES_PASSWORD: dev_password
    ports:
      - "5432:5432"
    volumes:
      - postgres_dev_data:/var/lib/postgresql/data

  redis-dev:
    image: redis:7-alpine
    ports:
      - "6379:6379"
    command: redis-server --appendonly yes
    volumes:
      - redis_dev_data:/data

  minio-dev:
    image: minio/minio:latest
    ports:
      - "9000:9000"
      - "9090:9090"
    environment:
      MINIO_ROOT_USER: minioadmin
      MINIO_ROOT_PASSWORD: minioadmin
    command: server /data --console-address ":9090"
    volumes:
      - minio_dev_data:/data

volumes:
  postgres_dev_data:
  redis_dev_data:
  minio_dev_data:
```

### Staging Environment

```yaml
# kubernetes/staging/namespace.yml
apiVersion: v1
kind: Namespace
metadata:
  name: wesign-export-staging
  labels:
    environment: staging
    app: wesign-export

---
# kubernetes/staging/configmap.yml
apiVersion: v1
kind: ConfigMap
metadata:
  name: export-config
  namespace: wesign-export-staging
data:
  NODE_ENV: "staging"
  EXPORT_MAX_CONCURRENT: "25"
  EXPORT_WORKER_INSTANCES: "2"
  EXPORT_FILE_RETENTION_DAYS: "3"
  EXPORT_LOG_LEVEL: "info"
  EXPORT_STORAGE_TYPE: "s3"
  EXPORT_S3_BUCKET: "wesign-exports-staging"
  EXPORT_S3_REGION: "us-east-1"

---
# kubernetes/staging/secret.yml
apiVersion: v1
kind: Secret
metadata:
  name: export-secrets
  namespace: wesign-export-staging
type: Opaque
data:
  EXPORT_DB_PASSWORD: ${EXPORT_DB_PASSWORD_BASE64}
  EXPORT_REDIS_PASSWORD: ${EXPORT_REDIS_PASSWORD_BASE64}
  EXPORT_JWT_SECRET: ${EXPORT_JWT_SECRET_BASE64}
  EXPORT_ENCRYPTION_KEY: ${EXPORT_ENCRYPTION_KEY_BASE64}
  AWS_ACCESS_KEY_ID: ${AWS_ACCESS_KEY_ID_BASE64}
  AWS_SECRET_ACCESS_KEY: ${AWS_SECRET_ACCESS_KEY_BASE64}

---
# kubernetes/staging/deployment.yml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: export-api
  namespace: wesign-export-staging
  labels:
    app: export-api
    version: v1
spec:
  replicas: 2
  strategy:
    type: RollingUpdate
    rollingUpdate:
      maxSurge: 1
      maxUnavailable: 0
  selector:
    matchLabels:
      app: export-api
  template:
    metadata:
      labels:
        app: export-api
        version: v1
    spec:
      serviceAccountName: export-service-account
      containers:
      - name: export-api
        image: wesign/export-api:staging-latest
        ports:
        - containerPort: 3000
          name: http
        envFrom:
        - configMapRef:
            name: export-config
        - secretRef:
            name: export-secrets
        resources:
          requests:
            memory: "256Mi"
            cpu: "100m"
          limits:
            memory: "512Mi"
            cpu: "250m"
        livenessProbe:
          httpGet:
            path: /health
            port: 3000
          initialDelaySeconds: 30
          periodSeconds: 10
          timeoutSeconds: 5
          failureThreshold: 3
        readinessProbe:
          httpGet:
            path: /ready
            port: 3000
          initialDelaySeconds: 5
          periodSeconds: 5
          timeoutSeconds: 3
          failureThreshold: 3
        lifecycle:
          preStop:
            exec:
              command: ["/bin/sh", "-c", "sleep 10"]

---
# kubernetes/staging/service.yml
apiVersion: v1
kind: Service
metadata:
  name: export-api-service
  namespace: wesign-export-staging
spec:
  selector:
    app: export-api
  ports:
  - name: http
    port: 80
    targetPort: 3000
  type: ClusterIP

---
# kubernetes/staging/ingress.yml
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: export-api-ingress
  namespace: wesign-export-staging
  annotations:
    kubernetes.io/ingress.class: "nginx"
    nginx.ingress.kubernetes.io/ssl-redirect: "true"
    nginx.ingress.kubernetes.io/force-ssl-redirect: "true"
    nginx.ingress.kubernetes.io/proxy-body-size: "100m"
    nginx.ingress.kubernetes.io/proxy-read-timeout: "300"
    nginx.ingress.kubernetes.io/proxy-send-timeout: "300"
spec:
  tls:
  - hosts:
    - export-staging.wesign.com
    secretName: export-staging-tls
  rules:
  - host: export-staging.wesign.com
    http:
      paths:
      - path: /api/exports
        pathType: Prefix
        backend:
          service:
            name: export-api-service
            port:
              number: 80
```

### Production Environment

```yaml
# kubernetes/production/deployment.yml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: export-api
  namespace: wesign-export-production
  labels:
    app: export-api
    version: v1
spec:
  replicas: 5
  strategy:
    type: RollingUpdate
    rollingUpdate:
      maxSurge: 2
      maxUnavailable: 1
  selector:
    matchLabels:
      app: export-api
  template:
    metadata:
      labels:
        app: export-api
        version: v1
      annotations:
        prometheus.io/scrape: "true"
        prometheus.io/port: "3000"
        prometheus.io/path: "/metrics"
    spec:
      serviceAccountName: export-service-account
      nodeSelector:
        node-type: application
      tolerations:
      - key: "application"
        operator: "Equal"
        value: "true"
        effect: "NoSchedule"
      affinity:
        podAntiAffinity:
          preferredDuringSchedulingIgnoredDuringExecution:
          - weight: 100
            podAffinityTerm:
              labelSelector:
                matchExpressions:
                - key: app
                  operator: In
                  values:
                  - export-api
              topologyKey: kubernetes.io/hostname
      containers:
      - name: export-api
        image: wesign/export-api:v1.0.0
        ports:
        - containerPort: 3000
          name: http
        - containerPort: 9090
          name: metrics
        envFrom:
        - configMapRef:
            name: export-config
        - secretRef:
            name: export-secrets
        env:
        - name: POD_NAME
          valueFrom:
            fieldRef:
              fieldPath: metadata.name
        - name: POD_NAMESPACE
          valueFrom:
            fieldRef:
              fieldPath: metadata.namespace
        resources:
          requests:
            memory: "512Mi"
            cpu: "250m"
          limits:
            memory: "1Gi"
            cpu: "500m"
        livenessProbe:
          httpGet:
            path: /health
            port: 3000
          initialDelaySeconds: 30
          periodSeconds: 10
          timeoutSeconds: 5
          failureThreshold: 3
        readinessProbe:
          httpGet:
            path: /ready
            port: 3000
          initialDelaySeconds: 5
          periodSeconds: 5
          timeoutSeconds: 3
          failureThreshold: 3
        lifecycle:
          preStop:
            exec:
              command: ["/bin/sh", "-c", "sleep 15"]
        volumeMounts:
        - name: tmp-volume
          mountPath: /tmp
        - name: export-logs
          mountPath: /app/logs
      volumes:
      - name: tmp-volume
        emptyDir:
          sizeLimit: 1Gi
      - name: export-logs
        emptyDir:
          sizeLimit: 500Mi

---
# kubernetes/production/hpa.yml
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata:
  name: export-api-hpa
  namespace: wesign-export-production
spec:
  scaleTargetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: export-api
  minReplicas: 3
  maxReplicas: 10
  metrics:
  - type: Resource
    resource:
      name: cpu
      target:
        type: Utilization
        averageUtilization: 70
  - type: Resource
    resource:
      name: memory
      target:
        type: Utilization
        averageUtilization: 80
  behavior:
    scaleUp:
      stabilizationWindowSeconds: 60
      policies:
      - type: Percent
        value: 50
        periodSeconds: 60
    scaleDown:
      stabilizationWindowSeconds: 300
      policies:
      - type: Percent
        value: 10
        periodSeconds: 60

---
# kubernetes/production/pdb.yml
apiVersion: policy/v1
kind: PodDisruptionBudget
metadata:
  name: export-api-pdb
  namespace: wesign-export-production
spec:
  minAvailable: 2
  selector:
    matchLabels:
      app: export-api
```

## Worker Deployment Configuration

```yaml
# kubernetes/production/export-worker-deployment.yml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: export-worker
  namespace: wesign-export-production
  labels:
    app: export-worker
    component: processor
spec:
  replicas: 3
  strategy:
    type: RollingUpdate
    rollingUpdate:
      maxSurge: 1
      maxUnavailable: 0
  selector:
    matchLabels:
      app: export-worker
  template:
    metadata:
      labels:
        app: export-worker
        component: processor
    spec:
      serviceAccountName: export-worker-service-account
      nodeSelector:
        node-type: worker
      containers:
      - name: export-worker
        image: wesign/export-worker:v1.0.0
        envFrom:
        - configMapRef:
            name: export-config
        - secretRef:
            name: export-secrets
        env:
        - name: WORKER_TYPE
          value: "export-processor"
        - name: WORKER_CONCURRENCY
          value: "5"
        resources:
          requests:
            memory: "1Gi"
            cpu: "500m"
          limits:
            memory: "2Gi"
            cpu: "1000m"
        livenessProbe:
          httpGet:
            path: /worker/health
            port: 3001
          initialDelaySeconds: 60
          periodSeconds: 30
          timeoutSeconds: 10
          failureThreshold: 3
        readinessProbe:
          httpGet:
            path: /worker/ready
            port: 3001
          initialDelaySeconds: 10
          periodSeconds: 10
          timeoutSeconds: 5
          failureThreshold: 3
        volumeMounts:
        - name: worker-tmp
          mountPath: /tmp/exports
        - name: worker-logs
          mountPath: /app/logs
      volumes:
      - name: worker-tmp
        emptyDir:
          sizeLimit: 5Gi
      - name: worker-logs
        emptyDir:
          sizeLimit: 1Gi

---
# kubernetes/production/export-worker-hpa.yml
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata:
  name: export-worker-hpa
  namespace: wesign-export-production
spec:
  scaleTargetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: export-worker
  minReplicas: 2
  maxReplicas: 8
  metrics:
  - type: Resource
    resource:
      name: cpu
      target:
        type: Utilization
        averageUtilization: 80
  - type: Pods
    pods:
      metric:
        name: export_queue_length
      target:
        type: AverageValue
        averageValue: "10"
  behavior:
    scaleUp:
      stabilizationWindowSeconds: 120
      policies:
      - type: Pods
        value: 2
        periodSeconds: 60
    scaleDown:
      stabilizationWindowSeconds: 600
      policies:
      - type: Percent
        value: 25
        periodSeconds: 120
```

## Database Migration and Setup

```typescript
// migrations/001_create_export_tables.ts
import { Knex } from 'knex';

export async function up(knex: Knex): Promise<void> {
  // Export requests table
  await knex.schema.createTable('export_requests', (table) => {
    table.uuid('id').primary().defaultTo(knex.raw('gen_random_uuid()'));
    table.uuid('user_id').notNullable();
    table.string('data_source', 50).notNullable();
    table.string('format', 20).notNullable();
    table.jsonb('filters').notNullable().defaultTo('{}');
    table.jsonb('options').notNullable().defaultTo('{}');
    table.string('status', 20).notNullable().defaultTo('queued');
    table.integer('progress').defaultTo(0);
    table.string('file_path').nullable();
    table.bigint('file_size').nullable();
    table.text('error_message').nullable();
    table.timestamp('created_at').defaultTo(knex.fn.now());
    table.timestamp('updated_at').defaultTo(knex.fn.now());
    table.timestamp('expires_at').nullable();
    table.timestamp('completed_at').nullable();

    // Indexes
    table.index(['user_id', 'created_at']);
    table.index(['status', 'created_at']);
    table.index(['expires_at']);
  });

  // Export audit log table
  await knex.schema.createTable('export_audit_logs', (table) => {
    table.uuid('id').primary().defaultTo(knex.raw('gen_random_uuid()'));
    table.uuid('export_request_id').notNullable();
    table.uuid('user_id').notNullable();
    table.string('action', 50).notNullable();
    table.jsonb('details').notNullable().defaultTo('{}');
    table.string('ip_address', 45).nullable();
    table.string('user_agent').nullable();
    table.timestamp('created_at').defaultTo(knex.fn.now());

    // Foreign key and indexes
    table.foreign('export_request_id').references('id').inTable('export_requests');
    table.index(['export_request_id']);
    table.index(['user_id', 'created_at']);
    table.index(['action', 'created_at']);
  });

  // Export permissions table
  await knex.schema.createTable('export_permissions', (table) => {
    table.uuid('id').primary().defaultTo(knex.raw('gen_random_uuid()'));
    table.string('role', 50).notNullable();
    table.string('data_source', 50).notNullable();
    table.jsonb('allowed_fields').notNullable().defaultTo('[]');
    table.jsonb('restrictions').notNullable().defaultTo('{}');
    table.bigint('max_file_size').defaultTo(52428800); // 50MB default
    table.integer('max_concurrent_exports').defaultTo(3);
    table.timestamp('created_at').defaultTo(knex.fn.now());
    table.timestamp('updated_at').defaultTo(knex.fn.now());

    // Unique constraint and indexes
    table.unique(['role', 'data_source']);
    table.index(['role']);
  });
}

export async function down(knex: Knex): Promise<void> {
  await knex.schema.dropTableIfExists('export_audit_logs');
  await knex.schema.dropTableIfExists('export_permissions');
  await knex.schema.dropTableIfExists('export_requests');
}
```

```sql
-- Initial data seeding
INSERT INTO export_permissions (role, data_source, allowed_fields, restrictions, max_file_size, max_concurrent_exports)
VALUES
  ('ProductManager', 'analytics', '["*"]', '{}', 104857600, 5),
  ('ProductManager', 'reports', '["*"]', '{}', 104857600, 5),
  ('ProductManager', 'dashboards', '["*"]', '{}', 52428800, 3),

  ('Support', 'analytics', '["userId", "timestamp", "documentId", "status"]', '{"excludePersonalData": true}', 52428800, 2),
  ('Support', 'reports', '["systemHealth", "errorLogs", "performanceMetrics"]', '{}', 52428800, 2),

  ('Operations', 'reports', '["systemHealth", "performanceMetrics", "resourceUsage"]', '{}', 104857600, 3),
  ('Operations', 'analytics', '["systemMetrics", "performanceData"]', '{"excludeUserData": true}', 52428800, 3);
```

## CI/CD Pipeline Configuration

```yaml
# .github/workflows/export-service-deploy.yml
name: Export Service Deployment

on:
  push:
    branches: [main, develop]
    paths: ['src/export/**', 'kubernetes/export/**']
  pull_request:
    branches: [main]
    paths: ['src/export/**']

env:
  REGISTRY: ghcr.io
  IMAGE_NAME: wesign/export-service

jobs:
  test:
    runs-on: ubuntu-latest
    services:
      postgres:
        image: postgres:15
        env:
          POSTGRES_PASSWORD: test_password
          POSTGRES_DB: export_test
        options: >-
          --health-cmd pg_isready
          --health-interval 10s
          --health-timeout 5s
          --health-retries 5
        ports:
          - 5432:5432

      redis:
        image: redis:7-alpine
        options: >-
          --health-cmd "redis-cli ping"
          --health-interval 10s
          --health-timeout 5s
          --health-retries 5
        ports:
          - 6379:6379

    steps:
    - uses: actions/checkout@v4

    - name: Setup Node.js
      uses: actions/setup-node@v4
      with:
        node-version: '18'
        cache: 'npm'

    - name: Install dependencies
      run: npm ci

    - name: Run database migrations
      run: npm run migrate:test
      env:
        DATABASE_URL: postgresql://postgres:test_password@localhost:5432/export_test

    - name: Run unit tests
      run: npm run test:unit
      env:
        NODE_ENV: test
        DATABASE_URL: postgresql://postgres:test_password@localhost:5432/export_test
        REDIS_URL: redis://localhost:6379

    - name: Run integration tests
      run: npm run test:integration
      env:
        NODE_ENV: test
        DATABASE_URL: postgresql://postgres:test_password@localhost:5432/export_test
        REDIS_URL: redis://localhost:6379

    - name: Run E2E tests
      run: npm run test:e2e
      env:
        NODE_ENV: test
        DATABASE_URL: postgresql://postgres:test_password@localhost:5432/export_test
        REDIS_URL: redis://localhost:6379

  build:
    needs: test
    runs-on: ubuntu-latest
    outputs:
      image-tag: ${{ steps.meta.outputs.tags }}
      image-digest: ${{ steps.build.outputs.digest }}
    steps:
    - uses: actions/checkout@v4

    - name: Set up Docker Buildx
      uses: docker/setup-buildx-action@v3

    - name: Log in to Container Registry
      uses: docker/login-action@v3
      with:
        registry: ${{ env.REGISTRY }}
        username: ${{ github.actor }}
        password: ${{ secrets.GITHUB_TOKEN }}

    - name: Extract metadata
      id: meta
      uses: docker/metadata-action@v5
      with:
        images: ${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}
        tags: |
          type=ref,event=branch
          type=ref,event=pr
          type=sha,prefix={{branch}}-
          type=raw,value=latest,enable={{is_default_branch}}

    - name: Build and push Docker image
      id: build
      uses: docker/build-push-action@v5
      with:
        context: .
        file: ./Dockerfile.production
        push: true
        tags: ${{ steps.meta.outputs.tags }}
        labels: ${{ steps.meta.outputs.labels }}
        cache-from: type=gha
        cache-to: type=gha,mode=max
        platforms: linux/amd64,linux/arm64

  security-scan:
    needs: build
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v4

    - name: Run Trivy vulnerability scanner
      uses: aquasecurity/trivy-action@master
      with:
        image-ref: ${{ needs.build.outputs.image-tag }}
        format: 'sarif'
        output: 'trivy-results.sarif'

    - name: Upload Trivy scan results
      uses: github/codeql-action/upload-sarif@v3
      with:
        sarif_file: 'trivy-results.sarif'

  deploy-staging:
    needs: [build, security-scan]
    runs-on: ubuntu-latest
    if: github.ref == 'refs/heads/develop'
    environment: staging
    steps:
    - uses: actions/checkout@v4

    - name: Configure kubectl
      uses: azure/k8s-set-context@v3
      with:
        method: kubeconfig
        kubeconfig: ${{ secrets.KUBE_CONFIG_STAGING }}

    - name: Deploy to staging
      run: |
        kubectl set image deployment/export-api export-api=${{ needs.build.outputs.image-tag }} -n wesign-export-staging
        kubectl rollout status deployment/export-api -n wesign-export-staging --timeout=300s

    - name: Run smoke tests
      run: |
        kubectl wait --for=condition=available deployment/export-api -n wesign-export-staging --timeout=300s
        npm run test:smoke -- --base-url=https://export-staging.wesign.com

  deploy-production:
    needs: [build, security-scan, deploy-staging]
    runs-on: ubuntu-latest
    if: github.ref == 'refs/heads/main'
    environment: production
    steps:
    - uses: actions/checkout@v4

    - name: Configure kubectl
      uses: azure/k8s-set-context@v3
      with:
        method: kubeconfig
        kubeconfig: ${{ secrets.KUBE_CONFIG_PRODUCTION }}

    - name: Deploy to production
      run: |
        # Update image tag
        kubectl set image deployment/export-api export-api=${{ needs.build.outputs.image-tag }} -n wesign-export-production
        kubectl set image deployment/export-worker export-worker=${{ needs.build.outputs.image-tag }} -n wesign-export-production

        # Wait for rollout
        kubectl rollout status deployment/export-api -n wesign-export-production --timeout=600s
        kubectl rollout status deployment/export-worker -n wesign-export-production --timeout=600s

    - name: Post-deployment verification
      run: |
        # Wait for readiness
        kubectl wait --for=condition=available deployment/export-api -n wesign-export-production --timeout=300s
        kubectl wait --for=condition=available deployment/export-worker -n wesign-export-production --timeout=300s

        # Run health checks
        npm run test:health -- --base-url=https://export.wesign.com

    - name: Notify Slack
      uses: 8398a7/action-slack@v3
      with:
        status: ${{ job.status }}
        channel: '#deployments'
        webhook_url: ${{ secrets.SLACK_WEBHOOK }}
      if: always()
```

## Infrastructure as Code

```terraform
# terraform/export-infrastructure.tf
provider "aws" {
  region = var.aws_region
}

# S3 Bucket for export files
resource "aws_s3_bucket" "export_files" {
  bucket = "wesign-exports-${var.environment}"

  tags = {
    Environment = var.environment
    Service     = "export"
    Purpose     = "file-storage"
  }
}

resource "aws_s3_bucket_versioning" "export_files_versioning" {
  bucket = aws_s3_bucket.export_files.id
  versioning_configuration {
    status = "Enabled"
  }
}

resource "aws_s3_bucket_encryption" "export_files_encryption" {
  bucket = aws_s3_bucket.export_files.id

  server_side_encryption_configuration {
    rule {
      apply_server_side_encryption_by_default {
        sse_algorithm = "AES256"
      }
    }
  }
}

resource "aws_s3_bucket_lifecycle_configuration" "export_files_lifecycle" {
  bucket = aws_s3_bucket.export_files.id

  rule {
    id     = "export_cleanup"
    status = "Enabled"

    expiration {
      days = var.export_retention_days
    }

    noncurrent_version_expiration {
      noncurrent_days = 7
    }

    abort_incomplete_multipart_upload {
      days_after_initiation = 1
    }
  }
}

# RDS Instance for export metadata
resource "aws_db_instance" "export_db" {
  identifier = "wesign-export-${var.environment}"

  engine          = "postgres"
  engine_version  = "15.4"
  instance_class  = var.db_instance_class
  allocated_storage = var.db_allocated_storage
  max_allocated_storage = var.db_max_allocated_storage

  db_name  = "wesign_exports"
  username = var.db_username
  password = var.db_password

  vpc_security_group_ids = [aws_security_group.export_db.id]
  db_subnet_group_name   = aws_db_subnet_group.export_db.name

  backup_retention_period = var.backup_retention_period
  backup_window          = "03:00-04:00"
  maintenance_window     = "Sun:04:00-Sun:05:00"

  skip_final_snapshot = var.environment != "production"
  final_snapshot_identifier = var.environment == "production" ? "wesign-export-final-snapshot" : null

  performance_insights_enabled = var.environment == "production"
  monitoring_interval = var.environment == "production" ? 60 : 0
  monitoring_role_arn = var.environment == "production" ? aws_iam_role.rds_enhanced_monitoring[0].arn : null

  tags = {
    Environment = var.environment
    Service     = "export"
    Purpose     = "metadata-storage"
  }
}

# ElastiCache Redis for queue and caching
resource "aws_elasticache_subnet_group" "export_redis" {
  name       = "wesign-export-redis-${var.environment}"
  subnet_ids = var.private_subnet_ids
}

resource "aws_elasticache_replication_group" "export_redis" {
  replication_group_id         = "wesign-export-${var.environment}"
  description                  = "Redis cluster for export service"

  engine               = "redis"
  engine_version       = "7.0"
  node_type           = var.redis_node_type
  port                = 6379
  parameter_group_name = "default.redis7"

  num_cache_clusters = var.redis_num_replicas + 1

  subnet_group_name  = aws_elasticache_subnet_group.export_redis.name
  security_group_ids = [aws_security_group.export_redis.id]

  at_rest_encryption_enabled = true
  transit_encryption_enabled = true
  auth_token                 = var.redis_auth_token

  log_delivery_configuration {
    destination      = aws_cloudwatch_log_group.redis_slow_log.name
    destination_type = "cloudwatch-logs"
    log_format       = "text"
    log_type         = "slow-log"
  }

  tags = {
    Environment = var.environment
    Service     = "export"
    Purpose     = "queue-cache"
  }
}

# CloudWatch Log Groups
resource "aws_cloudwatch_log_group" "export_application" {
  name              = "/aws/eks/wesign-export-${var.environment}/application"
  retention_in_days = var.log_retention_days

  tags = {
    Environment = var.environment
    Service     = "export"
    Purpose     = "application-logs"
  }
}

resource "aws_cloudwatch_log_group" "redis_slow_log" {
  name              = "/aws/elasticache/wesign-export-${var.environment}/redis-slow"
  retention_in_days = var.log_retention_days
}

# Security Groups
resource "aws_security_group" "export_db" {
  name_prefix = "wesign-export-db-${var.environment}"
  vpc_id      = var.vpc_id

  ingress {
    from_port   = 5432
    to_port     = 5432
    protocol    = "tcp"
    cidr_blocks = var.private_subnet_cidrs
  }

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }

  tags = {
    Name        = "wesign-export-db-${var.environment}"
    Environment = var.environment
  }
}

resource "aws_security_group" "export_redis" {
  name_prefix = "wesign-export-redis-${var.environment}"
  vpc_id      = var.vpc_id

  ingress {
    from_port   = 6379
    to_port     = 6379
    protocol    = "tcp"
    cidr_blocks = var.private_subnet_cidrs
  }

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }

  tags = {
    Name        = "wesign-export-redis-${var.environment}"
    Environment = var.environment
  }
}

# IAM Role for enhanced monitoring
resource "aws_iam_role" "rds_enhanced_monitoring" {
  count = var.environment == "production" ? 1 : 0
  name  = "wesign-export-rds-monitoring-${var.environment}"

  assume_role_policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Action = "sts:AssumeRole"
        Effect = "Allow"
        Principal = {
          Service = "monitoring.rds.amazonaws.com"
        }
      }
    ]
  })
}

resource "aws_iam_role_policy_attachment" "rds_enhanced_monitoring" {
  count      = var.environment == "production" ? 1 : 0
  role       = aws_iam_role.rds_enhanced_monitoring[0].name
  policy_arn = "arn:aws:iam::aws:policy/service-role/AmazonRDSEnhancedMonitoringRole"
}

# Variables
variable "aws_region" {
  description = "AWS region"
  type        = string
  default     = "us-east-1"
}

variable "environment" {
  description = "Environment name"
  type        = string
}

variable "vpc_id" {
  description = "VPC ID"
  type        = string
}

variable "private_subnet_ids" {
  description = "Private subnet IDs"
  type        = list(string)
}

variable "private_subnet_cidrs" {
  description = "Private subnet CIDR blocks"
  type        = list(string)
}

variable "db_instance_class" {
  description = "RDS instance class"
  type        = string
  default     = "db.t3.medium"
}

variable "db_allocated_storage" {
  description = "RDS allocated storage in GB"
  type        = number
  default     = 100
}

variable "db_max_allocated_storage" {
  description = "RDS max allocated storage in GB"
  type        = number
  default     = 1000
}

variable "redis_node_type" {
  description = "ElastiCache node type"
  type        = string
  default     = "cache.t3.medium"
}

variable "redis_num_replicas" {
  description = "Number of Redis replicas"
  type        = number
  default     = 1
}

variable "export_retention_days" {
  description = "Export file retention in days"
  type        = number
  default     = 7
}

variable "log_retention_days" {
  description = "CloudWatch log retention in days"
  type        = number
  default     = 14
}

variable "backup_retention_period" {
  description = "RDS backup retention period in days"
  type        = number
  default     = 7
}

# Sensitive variables
variable "db_username" {
  description = "Database username"
  type        = string
  sensitive   = true
}

variable "db_password" {
  description = "Database password"
  type        = string
  sensitive   = true
}

variable "redis_auth_token" {
  description = "Redis authentication token"
  type        = string
  sensitive   = true
}
```

## Monitoring and Alerting

```yaml
# kubernetes/monitoring/servicemonitor.yml
apiVersion: monitoring.coreos.com/v1
kind: ServiceMonitor
metadata:
  name: export-service-monitor
  namespace: wesign-export-production
  labels:
    app: export-service
    prometheus: wesign
spec:
  selector:
    matchLabels:
      app: export-api
  endpoints:
  - port: metrics
    interval: 30s
    path: /metrics
    honorLabels: true

---
# kubernetes/monitoring/prometheusrule.yml
apiVersion: monitoring.coreos.com/v1
kind: PrometheusRule
metadata:
  name: export-service-alerts
  namespace: wesign-export-production
  labels:
    app: export-service
    prometheus: wesign
spec:
  groups:
  - name: export-service
    rules:
    - alert: ExportServiceDown
      expr: up{job="export-api"} == 0
      for: 1m
      labels:
        severity: critical
        service: export-api
      annotations:
        summary: "Export service is down"
        description: "Export API service has been down for more than 1 minute"

    - alert: ExportHighErrorRate
      expr: rate(http_requests_total{job="export-api",status=~"5.."}[5m]) > 0.1
      for: 5m
      labels:
        severity: warning
        service: export-api
      annotations:
        summary: "High error rate in export service"
        description: "Export service error rate is {{ $value }} requests/second"

    - alert: ExportQueueBacklog
      expr: export_queue_length > 100
      for: 2m
      labels:
        severity: warning
        service: export-worker
      annotations:
        summary: "Export queue backlog"
        description: "Export queue has {{ $value }} pending jobs"

    - alert: ExportProcessingTimeout
      expr: rate(export_processing_timeouts_total[5m]) > 0.05
      for: 5m
      labels:
        severity: warning
        service: export-worker
      annotations:
        summary: "Export processing timeouts"
        description: "Export processing timeout rate is {{ $value }} timeouts/second"

    - alert: ExportDiskSpaceLow
      expr: (100 - (node_filesystem_avail_bytes{mountpoint="/tmp/exports"} / node_filesystem_size_bytes{mountpoint="/tmp/exports"} * 100)) > 80
      for: 5m
      labels:
        severity: warning
        service: export-worker
      annotations:
        summary: "Low disk space for exports"
        description: "Export disk usage is at {{ $value }}%"
```

## Rollback Procedures

```bash
#!/bin/bash
# scripts/rollback-export-service.sh

set -euo pipefail

NAMESPACE="${1:-wesign-export-production}"
ENVIRONMENT="${2:-production}"

echo "🔄 Starting rollback procedure for export service in $NAMESPACE"

# Check current deployment status
echo "📊 Current deployment status:"
kubectl get deployments -n "$NAMESPACE" -l app=export-api
kubectl get deployments -n "$NAMESPACE" -l app=export-worker

# Get rollback target
PREVIOUS_REVISION=$(kubectl rollout history deployment/export-api -n "$NAMESPACE" --revision=0 | tail -n 2 | head -n 1 | awk '{print $1}')

echo "🎯 Rolling back to revision: $PREVIOUS_REVISION"

# Confirm rollback
read -p "⚠️  Are you sure you want to rollback? (y/N): " -n 1 -r
echo
if [[ ! $REPLY =~ ^[Yy]$ ]]; then
    echo "❌ Rollback cancelled"
    exit 1
fi

# Perform rollback
echo "🔄 Rolling back export-api..."
kubectl rollout undo deployment/export-api -n "$NAMESPACE" --to-revision="$PREVIOUS_REVISION"

echo "🔄 Rolling back export-worker..."
kubectl rollout undo deployment/export-worker -n "$NAMESPACE" --to-revision="$PREVIOUS_REVISION"

# Wait for rollback completion
echo "⏳ Waiting for rollback to complete..."
kubectl rollout status deployment/export-api -n "$NAMESPACE" --timeout=300s
kubectl rollout status deployment/export-worker -n "$NAMESPACE" --timeout=300s

# Health check
echo "🔍 Performing post-rollback health check..."
sleep 30

# Check service health
HEALTH_STATUS=$(kubectl exec -n "$NAMESPACE" deployment/export-api -- curl -s -o /dev/null -w "%{http_code}" http://localhost:3000/health)

if [ "$HEALTH_STATUS" = "200" ]; then
    echo "✅ Rollback completed successfully"
    echo "📊 Current status:"
    kubectl get pods -n "$NAMESPACE" -l app=export-api
    kubectl get pods -n "$NAMESPACE" -l app=export-worker
else
    echo "❌ Health check failed after rollback"
    echo "🚨 Manual intervention required"
    exit 1
fi

# Notify team
if [ "$ENVIRONMENT" = "production" ]; then
    echo "📢 Notifying team about production rollback..."
    # Add Slack notification or other alerting here
fi

echo "🎉 Rollback procedure completed successfully"
```

This comprehensive deployment configuration provides a robust, scalable, and production-ready setup for the Export functionality with proper CI/CD pipelines, infrastructure as code, monitoring, and operational procedures.