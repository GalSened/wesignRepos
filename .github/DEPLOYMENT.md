# WeSign Deployment Guide

## Overview

Automated zero-downtime rolling deployments via GitHub Actions. Deploys are triggered manually (workflow_dispatch) and roll out to two Windows Server instances (APP1, APP2) behind an ALB, one at a time.

**Tested:** 2,330 consecutive requests during a full rolling deploy with **zero errors**.

## Architecture

```
GitHub Actions Runner (ubuntu)
        │
        ├── Build (.NET / Angular)
        ├── Upload artifacts to S3
        │
        ▼
┌──────────────────────────────────────────────┐
│              AWS (us-east-1)                 │
│                                              │
│   S3 ──────────────────────────────────────┐ │
│   (wesign-file-transfer-7x9k2m/deploy/)   │ │
│                                            │ │
│   ALB (WesignStg-ALB)                     │ │
│    ├── TG :80   (/health.html)            │ │
│    ├── TG :443  (/login)                  │ │
│    └── TG :10443 (/login)                 │ │
│         │              │                   │ │
│     ┌───▼───┐    ┌────▼────┐              │ │
│     │ APP1  │    │  APP2   │◄─── SSM ─────┘ │
│     │ (IIS) │    │  (IIS)  │                 │
│     └───────┘    └─────────┘                 │
└──────────────────────────────────────────────┘
```

**Rolling deploy flow:**
1. Deregister APP1 from ALB (APP2 handles all traffic)
2. Deploy new code to APP1 via SSM
3. Health check APP1, re-register in ALB
4. Wait for APP1 healthy
5. Repeat steps 1-4 for APP2

## Workflows

| Workflow | File | Trigger | What it deploys |
|----------|------|---------|-----------------|
| **Backend** | `deploy-backend.yml` | Manual | All 6 .NET backend services |
| **User Client** | `deploy-user-client.yml` | Manual | Angular user frontend |
| **Mgmt Client** | `deploy-mgmt-client.yml` | Manual | Angular management frontend |
| **Signer Client** | `deploy-signer-client.yml` | Manual | Angular signer frontend |
| **Deploy All** | `deploy-all.yml` | Manual | Any combination of the above (sequential) |
| **Rollback** | `rollback.yml` | Manual | Restore from server-side backup |

All workflows use `concurrency: wesign-deploy` to prevent overlapping deployments.

## How to Deploy

### Deploy Backend

1. Go to **Actions** > **Deploy WeSign Backend** > **Run workflow**
2. Select environment: `wesigndev`
3. Click **Run workflow**

The pipeline runs 4 jobs sequentially:
- **Build Backend** (~3 min) — builds all .NET services, uploads zips to S3
- **Deploy to APP1** (~5 min deploy + ~10 min ALB healthy wait)
- **Deploy to APP2** (same)
- **Notify Teams** — sends result to Teams channel

### Deploy Frontend

Same process: **Actions** > select the frontend workflow > **Run workflow**.

Frontend deploys are simpler — no IIS pool restarts needed (static files only).

### Deploy Everything

**Actions** > **Deploy All WeSign Components** > **Run workflow**

Select which components to deploy (checkboxes). They run sequentially:
Backend > User Client > Mgmt Client > Signer Client.

If any component fails, subsequent components are skipped.

### Rollback

**Actions** > **WeSign Rollback** > **Run workflow**

| Input | Options |
|-------|---------|
| Component | `backend`, `user-client`, `mgmt-client`, `signer-client`, `all` |
| Confirm | Must type `ROLLBACK` (safety check) |
| Server | `both`, `APP1-only`, `APP2-only` |

Rollback restores from the most recent server-side backup (up to 5 backups kept per service).

## Server Layout

```
C:\Comda\Wesign\
├── Sites\                          ← Live deployments
│   ├── UserBackend\                  IIS Pool: UserApi
│   ├── SignerBackend\                IIS Pool: SignerApi
│   ├── ManagementBackend\            IIS Pool: ManagementApi
│   ├── WSEAuth\                      IIS Pool: WseAuth
│   ├── PdfExternalService\           IIS Pool: PdfExternalConvertor
│   ├── HistoryService\               IIS Pool: HistoryServiceApi
│   ├── UserFrontend\                 (static files)
│   ├── ManagementFrontend\           (static files)
│   └── SignerFrontend\               (static files)
└── Backups\                        ← Automatic backups
    ├── UserBackend\
    │   ├── backup_20260304_131025\
    │   ├── backup_20260304_145512\
    │   └── ...                       (5 most recent kept)
    ├── SignerBackend\
    └── ...
```

## Config File Handling

**Config files are NEVER overwritten by deployments.** This includes:

- `appsettings.json`
- `appsettings.*.json` (Production, Development, etc.)
- `web.config`

How it works:
1. **Build step** strips config files from artifacts before uploading to S3
2. **Deploy step** saves existing configs, deploys new code, then restores the saved configs

If you need to update a config file on the servers, do it manually via SSM or RDP. The deploy pipeline will not touch it.

## IIS Health Check Endpoints

| Service | URL | Expected Response |
|---------|-----|-------------------|
| UserBackend | `https://localhost/userApi/` | HTTP 404 (no default route) |
| SignerBackend | `https://localhost/signerApi/` | HTTP 404 |
| ManagementBackend | `https://localhost:10443/managementApi/` | HTTP 404 |
| WSEAuth | `https://localhost/auth/` | HTTP 401 (auth required) |

Any HTTP status < 500 is considered healthy. The deploy retries up to 6 times (5 sec apart) before failing.

## GitHub Secrets & Variables

### Repository Secrets
| Secret | Description |
|--------|-------------|
| `AWS_ROLE_ARN` | IAM role ARN for OIDC authentication |
| `TEAMS_WEBHOOK_URL` | Microsoft Teams incoming webhook |
| `NUGET_AUTH_TOKEN` | GitHub PAT with `read:packages` scope for `comda-co-il` org |

### Environment Variables (wesigndev)
| Variable | Description |
|----------|-------------|
| `AWS_REGION` | `us-east-1` |
| `APP1_INSTANCE_ID` | EC2 instance ID for APP1 |
| `APP2_INSTANCE_ID` | EC2 instance ID for APP2 |
| `TG_APP_ARN` | Target group ARN (port 80) |
| `TG_HTTPS_ARN` | Target group ARN (port 443) |
| `TG_MGMT_ARN` | Target group ARN (port 10443) |
| `ARTIFACT_BUCKET` | S3 bucket for build artifacts |

## Build Details

### Backend (.NET 9.0)

- SDK pinned to 9.0.x via `global.json` (runner may have 10.0.x)
- Published as **portable framework-dependent** (no `-r win-x64`)
- Three Serilog sinks are added at build time (configured in server `appsettings.json` but not in `.csproj` files):
  - `Serilog.Sinks.MariaDB` — legacy DB logging
  - `Serilog.Sinks.AwsCloudWatch` — CloudWatch logging
  - `Serilog.Sinks.MySQL` — MySQL logging
- Private NuGet packages restored from **GitHub Packages** (`nuget.pkg.github.com/comda-co-il`)
  - Configured in `user-backend/nuget.config`
  - Authenticated via `NUGET_AUTH_TOKEN` secret (GitHub PAT with `read:packages` scope)
  - When a private package is updated on GitHub Packages, the next build picks it up automatically

### Frontends (Angular 15, Node 18)

| Frontend | Build Command | Output Directory |
|----------|---------------|-----------------|
| User Client | `npm run prod-publish` | `dist/` |
| Mgmt Client | `npm run prod-publish` | `dist/WeSignManagement-Client/` |
| Signer Client | `npm run prod-publish` | `dist/` (base-href=/signer) |

## Troubleshooting

### Deploy step fails but health checks passed
Check the SSM command output in the workflow logs. Common causes:
- PowerShell parse errors (check for unescaped `$variable:` patterns)
- SSM timeout (default 600s)

### ALB health check takes 10+ minutes
The HTTPS target group (`wesigndev-HTTPS`) requires **5 consecutive healthy checks** at 30-second intervals = 2.5 min minimum. In practice, with initial registration delay, it can take 5-15 minutes.

### Service crashes after deploy (HTTP 500)
1. Check IIS stdout logs: `C:\Comda\Wesign\Sites\<service>\logs\stdout*.log`
2. Enable stdout logging in `web.config`: set `stdoutLogEnabled="true"`
3. Most common cause: missing DLL referenced in `appsettings.json` Serilog config but not in the build

### Rollback
If a deploy fails, the workflow automatically rolls back the affected server and re-registers it in the ALB. To manually rollback, use the **WeSign Rollback** workflow.

## S3 Artifact Structure

```
s3://wesign-file-transfer-7x9k2m/
└── deploy/
    ├── backend/{run_id}/
    │   ├── UserBackend.zip
    │   ├── SignerBackend.zip
    │   ├── ManagementBackend.zip
    │   ├── WSEAuth.zip
    │   ├── PdfExternalService.zip
    │   └── HistoryService.zip
    ├── user-client/{run_id}/
    │   └── UserFrontend.zip
    ├── mgmt-client/{run_id}/
    │   └── ManagementFrontend.zip
    ├── signer-client/{run_id}/
    │   └── SignerFrontend.zip
    ├── scripts/{run_id}/
    │   ├── app1-deploy.ps1
    │   └── app2-deploy.ps1
    └── nuget-packages/
        └── (private NuGet .nupkg files)
```

Artifacts are stored per `run_id` and not automatically cleaned up. Consider adding an S3 lifecycle rule to expire objects under `deploy/` after 30 days.
