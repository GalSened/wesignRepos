# WeSign Project Structure - Detailed Guide

## Solution Architecture Overview

The WeSignV3.sln follows a multi-project architecture with clear separation of concerns across 25+ projects. This structure supports maintainability, testability, and scalability.

## Core Project Structure

### 1. Main Application Projects

#### WeSign (Main Web API)
```
WeSign/
├── Areas/
│   ├── Api/              # REST API Controllers
│   │   └── Controllers/  # API endpoints (v3/[controller])
│   │       ├── UsersController.cs
│   │       ├── DocumentCollectionsController.cs  
│   │       ├── TemplatesController.cs
│   │       ├── SignersController.cs
│   │       ├── ContactsController.cs
│   │       ├── AdminsController.cs
│   │       ├── ReportsController.cs
│   │       ├── DashboardController.cs
│   │       ├── DistributionController.cs
│   │       ├── SelfSignController.cs
│   │       ├── LinksController.cs
│   │       └── ConfigurationController.cs
│   └── Ui/               # UI Controllers  
│       └── Controllers/  # Web UI endpoints (Ui/v3/[controller])
│           └── [Same controller names as Api]
├── Models/               # DTOs and ViewModels
│   ├── Users/           # User-related DTOs
│   ├── Documents/       # Document DTOs
│   ├── Templates/       # Template DTOs
│   └── Common/          # Shared DTOs
├── Hubs/                # SignalR Hubs
├── Middleware/          # Custom middleware
├── Configuration/       # Startup configuration
├── appsettings.json     # Application configuration
├── appsettings.Development.json
├── Program.cs           # Application entry point (.NET 6+ style)
├── Startup.cs           # Service configuration
└── WeSign.csproj        # Project file with dependencies
```

#### WeSignManagement (Management Portal)
```
WeSignManagement/
├── Areas/
│   └── Management/      # Management-specific controllers
├── Models/              # Management DTOs
├── Views/               # Razor views (if any)
└── WeSignManagement.csproj
```

#### WeSignSigner (Desktop Client)
```
WeSignSigner/
├── Forms/               # Windows Forms UI
├── Services/            # Client-side services
├── Models/              # Client models
└── WeSignSigner.csproj  # WinForms project
```

### 2. Business Logic Layer (BL)

```
BL/
├── Handlers/            # Business logic handlers
│   ├── UsersHandler.cs  # User operations
│   ├── DocumentHandler.cs
│   ├── TemplateHandler.cs
│   ├── SignatureHandler.cs
│   └── [Other domain handlers]
├── Connectors/          # Data access connectors
│   ├── IUserConnector.cs
│   ├── UserConnector.cs
│   └── [Interface + Implementation pairs]
├── Services/            # Domain services
│   ├── EmailService.cs
│   ├── PdfService.cs
│   ├── AuthService.cs
│   └── [Other services]
├── Validators/          # FluentValidation validators
├── Extensions/          # BL-specific extensions
└── BL.csproj
```

### 3. Data Access Layer (DAL)

```
DAL/
├── DAOs/                # Data Access Objects
│   ├── Users/          # User-related entities
│   │   ├── UserDAO.cs
│   │   ├── UserConfigurationDAO.cs
│   │   └── UserTokensDAO.cs
│   ├── Documents/      # Document entities
│   ├── Templates/      # Template entities
│   ├── Companies/      # Company entities
│   ├── Groups/         # Group entities
│   ├── Contacts/       # Contact entities
│   └── [Other domain folders]
├── Configurations/     # Entity configurations
│   ├── UserConfiguration.cs
│   └── [EF Core configurations]
├── Migrations/         # EF Core migrations
│   ├── [Timestamped migration files]
├── WeSignEntities.cs   # Main DbContext
└── DAL.csproj
```

### 4. Common Layer

```
Common/
├── Enums/              # Application enums
│   ├── UserType.cs
│   ├── UserStatus.cs
│   ├── DocumentStatus.cs
│   └── [Other enums]
├── Extensions/         # Extension methods
│   ├── StringExtensions.cs
│   ├── DateTimeExtensions.cs
│   └── [Other extensions]
├── Models/             # Domain models
│   ├── User.cs
│   ├── Document.cs
│   ├── Template.cs
│   └── [Other domain models]
├── DTOs/               # Data Transfer Objects
├── Constants/          # Application constants
├── Exceptions/         # Custom exceptions
├── Attributes/         # Custom attributes
├── Utilities/          # Utility classes
└── Common.csproj
```

### 5. Specialized Projects

#### Certificate Project
```
Certificate/
├── Handlers/           # Smart card handlers
├── Models/             # Certificate models
├── Services/           # Certificate services
└── Certificate.csproj
```

#### PdfHandler Project
```
PdfHandler/
├── Services/           # PDF processing services
├── Models/             # PDF-related models
├── Utilities/          # PDF utilities
└── PdfHandler.csproj
```

#### SignatureServiceConnector
```
SignatureServiceConnector/
├── Services/           # External signature services
├── Models/             # Signature models
└── SignatureServiceConnector.csproj
```

### 6. Test Projects

```
[Project].Tests/        # Test projects for each main project
├── BL.Tests/
│   ├── Handlers/       # Handler unit tests
│   ├── Services/       # Service unit tests
│   └── Fixtures/       # Test fixtures
├── Common.Tests/
├── PdfHandler.Tests/
├── SignerBL.Tests/
└── ManagementBL.Tests/
```

## Project Dependencies

### Dependency Flow
```
WeSign → BL, DAL, Common, Certificate
├── Areas (Api/Ui) → Models
└── Models → Common

BL → Common, PdfHandler, SignatureServiceConnector
├── Handlers → Connectors, Services
├── Connectors → DAL
└── Services → Common

DAL → Common
├── DAOs → Models (Common)
└── WeSignEntities → DAOs

PdfHandler → Common, SignatureServiceConnector
Certificate → Common
WeSignManagement → Common, DAL, ManagementBL, PdfHandler
WeSignSigner → Common, DAL, SignerBL
```

### Package Reference Hierarchy
```csproj
WeSign.csproj:
- ASP.NET Core 9.0 packages
- Entity Framework Core 9.0.2
- JWT Authentication
- Swagger/OpenAPI
- Serilog logging
- Hangfire
- FluentValidation

BL.csproj:
- Common utilities
- Business logic dependencies

DAL.csproj:
- Entity Framework Core
- SQL Server provider

Common.csproj:
- Newtonsoft.Json
- System extensions
```

## Routing Architecture

### API Area Routing Structure
All API controllers follow the pattern:
```
[Area("Api")]
[Route("v3/[controller]")]
```

**Examples:**
- `POST /v3/users` → UsersController.SignUpAsync()
- `GET /v3/documentcollections` → DocumentCollectionsController.GetAll()
- `POST /v3/templates` → TemplatesController.Create()

### UI Area Routing Structure
```
[Area("Ui")]  
[Route("Ui/v3/[controller]")]
```

**Examples:**
- `GET /Ui/v3/users` → UI version of users
- `GET /Ui/v3/dashboard` → Dashboard UI

## Configuration Architecture

### Configuration Hierarchy
1. **appsettings.json** (Base configuration)
2. **appsettings.{Environment}.json** (Environment-specific)
3. **User Secrets** (Development secrets)
4. **Environment Variables** (Production secrets)

### Configuration Sections
```json
{
  "ConnectionStrings": { /* Database connections */ },
  "JwtSettings": { /* JWT configuration */ },
  "EmailSettings": { /* SMTP configuration */ },
  "FeatureFlags": { /* Feature toggles */ },
  "Logging": { /* Serilog configuration */ },
  "Hangfire": { /* Background job configuration */ }
}
```

## File Organization Patterns

### Naming Conventions
- **Controllers**: `{Domain}Controller.cs` (e.g., UsersController.cs)
- **DTOs**: `{Action}{Domain}DTO.cs` (e.g., CreateUserDTO.cs)
- **DAOs**: `{Domain}DAO.cs` (e.g., UserDAO.cs)
- **Handlers**: `{Domain}Handler.cs` (e.g., UsersHandler.cs)
- **Connectors**: `I{Domain}Connector.cs` + `{Domain}Connector.cs`
- **Services**: `I{Domain}Service.cs` + `{Domain}Service.cs`

### Folder Organization by Domain
Each major domain (Users, Documents, Templates, etc.) has:
- Models in Common/Models/
- DTOs in WeSign/Models/
- DAOs in DAL/DAOs/
- Business logic in BL/Handlers/
- Controllers in WeSign/Areas/Api/Controllers/

## Build and Deployment Structure

### Solution Configuration
- **Debug**: Development builds with full debugging
- **Release**: Optimized production builds

### Target Framework
- **.NET 9.0** across all projects
- **ASP.NET Core 9.0** for web projects
- **Entity Framework Core 9.0.2** for data access

### Output Structure
```
bin/
├── Debug/
│   └── net9.0/
│       ├── WeSign.dll
│       ├── BL.dll
│       ├── DAL.dll
│       └── [Other assemblies]
└── Release/
    └── net9.0/
        └── [Optimized assemblies]
```

## Integration Points

### SignalR Hubs
```csharp
// WeSign/Startup.cs
endpoints.MapHub<SmartCardSigningHub>("v3/smartcardsocket");
```

### Background Jobs
```csharp
// Hangfire integration in WeSign
services.AddHangfire(configuration => 
    configuration.UseMemoryStorage());
```

### Database Context Registration
```csharp
// WeSign/Startup.cs  
services.AddDbContext<WeSignEntities>(options =>
    options.UseSqlServer(connectionString));
```

This structure supports:
- **Scalability**: Clear separation allows independent scaling
- **Maintainability**: Domain-focused organization
- **Testability**: Each layer can be tested in isolation  
- **Extensibility**: New features fit into established patterns
- **Team Development**: Different teams can work on different layers/domains

The architecture follows Domain-Driven Design principles with Clean Architecture patterns, ensuring long-term maintainability and flexibility.