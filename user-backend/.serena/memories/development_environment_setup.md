# WeSign Development Environment Setup Guide

## Prerequisites and Installation

### Required Software
1. **Visual Studio 2022** (Community/Professional/Enterprise)
   - Workloads: ASP.NET and web development, .NET desktop development
   - Individual Components: .NET 9.0 SDK, Entity Framework 9 tools
   
2. **Alternative: Visual Studio Code**
   - C# Extension by Microsoft
   - .NET Core Test Explorer
   - REST Client or Postman extension

3. **.NET 9.0 SDK**
   - Download from https://dotnet.microsoft.com/download
   - Verify installation: `dotnet --version` (should show 9.0.x)

4. **SQL Server**
   - SQL Server LocalDB (included with Visual Studio)
   - OR SQL Server Express (free)
   - OR Full SQL Server instance
   - SQL Server Management Studio (SSMS) recommended

5. **Git for Windows**
   - Download from https://git-scm.com/
   - Configure user name and email

### Repository Setup

1. **Clone Repository**
```bash
git clone [repository-url]
cd user-backend
```

2. **Solution Structure Overview**
The WeSignV3.sln contains these key projects:
```
WeSign/                    # Main Web API (Startup project)
BL/                       # Business Logic Layer  
DAL/                      # Data Access Layer
Common/                   # Shared components
Certificate/              # Smart card integration
PdfHandler/               # PDF processing
WeSignManagement/         # Management portal
WeSignSigner/            # Desktop client
[Project].Tests/          # Unit test projects
```

## Database Configuration

### Connection String Setup
1. **Update appsettings.json in WeSign project**:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=WeSignV3;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

2. **For SQL Server Express**:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.\\SQLEXPRESS;Database=WeSignV3;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

3. **Create Database and Apply Migrations**:
```bash
# Navigate to solution root
cd C:\path\to\user-backend

# Create database and apply migrations
dotnet ef database update --project DAL --startup-project WeSign

# Verify database creation
dotnet ef database info --project DAL --startup-project WeSign
```

## Project Dependencies

### Package Management
All projects use PackageReference format. Key dependencies:

**WeSign (Main API)**:
- ASP.NET Core 9.0
- Entity Framework Core 9.0.2
- JWT Authentication
- Swagger/OpenAPI
- Serilog logging
- Hangfire background jobs

**Project Reference Chain**:
```
WeSign → BL, DAL, Common, Certificate
BL → Common, PdfHandler  
DAL → Common
PdfHandler → Common, SignatureServiceConnector
WeSignManagement → Common, DAL, ManagementBL, PdfHandler
WeSignSigner → Common, DAL, SignerBL
```

### Restore Packages
```bash
# Restore all solution packages
dotnet restore WeSignV3.sln

# Or restore individual project
dotnet restore WeSign/WeSign.csproj
```

## Configuration Files

### appsettings.json Structure
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "[your-connection-string]"
  },
  "JwtSettings": {
    "SecretKey": "[jwt-secret-key]",
    "Issuer": "WeSign",
    "Audience": "WeSign",
    "ExpirationMinutes": 1440
  },
  "EmailSettings": {
    "SmtpServer": "smtp.example.com",
    "SmtpPort": 587,
    "Username": "[email-username]",
    "Password": "[email-password]"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "FeatureFlags": {
    "EnableFreeTrailUsers": true,
    "ShouldReturnActivationLinkInAPIResponse": true
  }
}
```

### appsettings.Development.json
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Information"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "[development-connection-string]"
  }
}
```

## IDE Configuration

### Visual Studio Setup
1. **Set WeSign as Startup Project**
   - Right-click WeSign project → Set as Startup Project

2. **Configure Multiple Startup Projects** (optional)
   - Solution Properties → Multiple startup projects
   - Set WeSign and WeSignManagement to Start

3. **Build Configuration**
   - Use Debug configuration for development
   - Ensure target framework is .NET 9.0

### Launch Profiles (launchSettings.json)
```json
{
  "profiles": {
    "WeSign": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": true,
      "launchUrl": "swagger",
      "applicationUrl": "https://localhost:7001;http://localhost:5000",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

## Development Workflow

### Build and Run
```bash
# Build entire solution
dotnet build WeSignV3.sln

# Run main API
dotnet run --project WeSign

# Run with hot reload
dotnet watch run --project WeSign

# Run specific project
dotnet run --project WeSignManagement
```

### Database Migrations
```bash
# Add new migration
dotnet ef migrations add [MigrationName] --project DAL --startup-project WeSign

# Update database
dotnet ef database update --project DAL --startup-project WeSign

# Revert to specific migration
dotnet ef database update [MigrationName] --project DAL --startup-project WeSign

# Remove last migration (if not applied)
dotnet ef migrations remove --project DAL --startup-project WeSign
```

### Testing
```bash
# Run all tests
dotnet test WeSignV3.sln

# Run specific test project
dotnet test BL.Tests/BL.Tests.csproj

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"
```

## Environment Variables

### Required Environment Variables
Create `.env` file or set system environment variables:
```
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=https://localhost:7001;http://localhost:5000
JWT_SECRET_KEY=[your-jwt-secret]
SMTP_PASSWORD=[email-password]
```

### User Secrets (Recommended for Development)
```bash
# Initialize user secrets
dotnet user-secrets init --project WeSign

# Set secret values
dotnet user-secrets set "JwtSettings:SecretKey" "[your-jwt-secret]" --project WeSign
dotnet user-secrets set "EmailSettings:Password" "[email-password]" --project WeSign

# List all secrets
dotnet user-secrets list --project WeSign
```

## Port Configuration

### Default Ports
- **WeSign API**: https://localhost:7001, http://localhost:5000
- **WeSignManagement**: https://localhost:7002, http://localhost:5001
- **WeSignSigner**: Desktop application (no port)

### Swagger Documentation
- **API Documentation**: https://localhost:7001/swagger
- **Management API**: https://localhost:7002/swagger

## Common Setup Issues and Solutions

### 1. Database Connection Issues
**Problem**: Cannot connect to LocalDB
**Solution**: 
- Verify LocalDB is installed: `sqllocaldb info`
- Create LocalDB instance: `sqllocaldb create MSSQLLocalDB`
- Start instance: `sqllocaldb start MSSQLLocalDB`

### 2. Migration Issues
**Problem**: Migration fails with foreign key errors
**Solution**:
- Drop database: `dotnet ef database drop --project DAL --startup-project WeSign`
- Recreate: `dotnet ef database update --project DAL --startup-project WeSign`

### 3. Package Restore Issues
**Problem**: NuGet packages won't restore
**Solution**:
- Clear NuGet cache: `dotnet nuget locals all --clear`
- Delete bin/obj folders: `git clean -xfd`
- Restore: `dotnet restore`

### 4. Certificate/HTTPS Issues
**Problem**: HTTPS certificate errors in development
**Solution**:
- Trust development certificate: `dotnet dev-certs https --trust`
- Clear certificates: `dotnet dev-certs https --clean`

## Debug Configuration

### Launch.json for VS Code
```json
{
  "version": "0.2.0",
  "configurations": [
    {
      "name": "Launch WeSign API",
      "type": "coreclr",
      "request": "launch",
      "preLaunchTask": "build",
      "program": "${workspaceFolder}/WeSign/bin/Debug/net9.0/WeSign.dll",
      "args": [],
      "cwd": "${workspaceFolder}/WeSign",
      "stopAtEntry": false,
      "serverReadyAction": {
        "action": "openExternally",
        "pattern": "\\bNow listening on:\\s+(https?://\\S+)"
      },
      "env": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      },
      "sourceFileMap": {
        "/Views": "${workspaceFolder}/Views"
      }
    }
  ]
}
```

### Performance Profiling
- Use Visual Studio Diagnostic Tools
- Enable ETW logging for Entity Framework
- Monitor SQL queries with SQL Profiler

This setup guide ensures new developers can quickly establish a working development environment for the WeSign platform.