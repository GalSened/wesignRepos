# WeSign - Technology Stack and Dependencies

## Core Framework
- **.NET 9.0** with ASP.NET Core Web API
- **Entity Framework Core 9.0.2** for data access
- **C# Language** with modern features

## Key Dependencies & Packages

### Authentication & Security
- `Microsoft.AspNetCore.Authentication.JwtBearer` (9.0.0) - JWT token authentication
- `Microsoft.FeatureManagement.AspNetCore` (4.0.0) - Feature flags
- `HtmlSanitizer` (8.1.870) - Input sanitization
- `AspNetCoreRateLimit` (5.0.0) - API rate limiting

### Background Processing & Jobs
- `Hangfire` (1.8.15) - Background job processing
- `Hangfire.MemoryStorage` (1.8.1.1) - In-memory job storage

### API & Documentation
- `Swashbuckle.AspNetCore` (7.0.0) - Swagger/OpenAPI documentation
- `Swashbuckle.AspNetCore.Annotations` (7.0.0) - API annotations
- `Microsoft.AspNetCore.Mvc.NewtonsoftJson` (9.0.0) - JSON serialization
- `FluentValidation.AspNetCore` (11.3.0) - Input validation

### Logging & Monitoring
- `Serilog.Settings.Configuration` (8.0.4) - Structured logging
- `Serilog.Sinks.File` (6.0.0) - File logging
- `Serilog.Sinks.MSSqlServer` (8.0.0) - Database logging
- `Serilog.Exceptions` (8.4.0) - Exception logging

### External Services & Integrations
- `CTHashSigner` (1.1.11) - Digital signing capabilities
- `System.Drawing.Common` (9.0.0) - Image processing

## Database Technology
- **SQL Server** with Entity Framework Core
- Migration-based schema management
- Comprehensive entity relationships and constraints

## Development Tools
- **Visual Studio 2022** (v17.2+)
- **User Secrets** for development configuration
- **XML Documentation** generation enabled