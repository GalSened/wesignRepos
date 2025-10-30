# WeSign - Suggested Development Commands

## Building and Running

### Build Commands
```bash
# Build entire solution
dotnet build WeSignV3.sln

# Build specific project
dotnet build WeSign/WeSign.csproj

# Build in Release mode
dotnet build WeSignV3.sln -c Release
```

### Run Commands
```bash
# Run main Web API application
dotnet run --project WeSign

# Run with specific environment
dotnet run --project WeSign --environment Development

# Run Management application
dotnet run --project WeSignManagement

# Run Signer application
dotnet run --project WeSignSigner
```

## Database Operations

### Entity Framework Migrations
```bash
# Add new migration
dotnet ef migrations add MigrationName --project DAL --startup-project WeSign

# Update database
dotnet ef database update --project DAL --startup-project WeSign

# Generate SQL script
dotnet ef migrations script --project DAL --startup-project WeSign

# Remove last migration
dotnet ef migrations remove --project DAL --startup-project WeSign
```

### Database Management
```bash
# Drop database
dotnet ef database drop --project DAL --startup-project WeSign

# View migration history
dotnet ef migrations list --project DAL --startup-project WeSign
```

## Testing

### Run Tests
```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test BL.Tests/BL.Tests.csproj
dotnet test Common.Tests/Common.Tests.csproj
dotnet test PdfHandler.Tests/PdfHandler.Tests.csproj
dotnet test SignerBL.Tests/SignerBL.Tests.csproj
dotnet test ManagementBL.Tests/ManagementBL.Tests.csproj

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"
```

## Package Management

### NuGet Operations
```bash
# Restore packages
dotnet restore

# Add package to specific project
dotnet add WeSign package PackageName

# Update packages
dotnet list package --outdated
dotnet add package PackageName --version x.x.x
```

## Development Utilities

### Code Analysis
```bash
# Format code
dotnet format WeSignV3.sln

# Analyze code quality
dotnet build --verbosity normal
```

### Documentation
```bash
# Generate XML documentation (already configured in project files)
# Documentation is automatically generated during build
```

## Windows-Specific Commands

### File Operations
```cmd
# List files
dir
# List files recursively
dir /s

# Find files
where filename.ext

# Copy files
copy source destination
xcopy source destination /E /I
```

### Process Management
```cmd
# Kill process by name
taskkill /F /IM process_name.exe

# List running processes
tasklist

# Find process by port
netstat -ano | findstr :port_number
```

### Git Operations (Windows)
```cmd
# Standard Git operations
git status
git add .
git commit -m "message"
git push
git pull

# Windows-specific Git config
git config core.autocrlf true
```

## Environment Setup

### User Secrets (Development)
```bash
# Initialize user secrets
dotnet user-secrets init --project WeSign

# Set secret value
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=..." --project WeSign

# List secrets
dotnet user-secrets list --project WeSign
```

### Configuration Management
- **appsettings.json** - Base configuration
- **appsettings.Development.json** - Development overrides
- **User Secrets** - Sensitive development data
- **Environment Variables** - Production configuration

## Production Deployment

### Publishing
```bash
# Publish for production
dotnet publish WeSign -c Release -o ./publish

# Publish self-contained
dotnet publish WeSign -c Release --self-contained -r win-x64
```

## Monitoring & Debugging

### Hangfire Dashboard
- Navigate to `/jobs` endpoint when running
- Monitor background job execution
- View job history and failures

### Swagger UI
- Navigate to `/swagger` endpoint (if enabled in configuration)
- Explore API endpoints and test functionality
- View API documentation