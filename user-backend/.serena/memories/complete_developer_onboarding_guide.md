# WeSign Complete Developer Onboarding Guide

## Table of Contents
1. [Development Environment Setup](#development-environment-setup)
2. [Project Structure Overview](#project-structure-overview)
3. [Learning Roadmap: Controller to Database](#learning-roadmap-controller-to-database)
4. [Implementation Patterns](#implementation-patterns)
5. [API Development Guide](#api-development-guide)
6. [Testing Guidelines](#testing-guidelines)
7. [Debugging and Troubleshooting](#debugging-and-troubleshooting)
8. [Common Development Scenarios](#common-development-scenarios)

---

# Development Environment Setup

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
   - SQL Server Management Studio (SSMS) recommended

5. **Git for Windows**
   - Configure user name and email

### Repository Setup

1. **Clone Repository**
```bash
git clone [repository-url]
cd user-backend
```

2. **Base Directory:** `C:\Users\gals\source\repos\user-backend\`

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

2. **Create Database and Apply Migrations**:
```bash
cd C:\Users\gals\source\repos\user-backend
dotnet ef database update --project DAL --startup-project WeSign
```

### Configuration Files
- **Main Config:** `WeSign\appsettings.json`
- **Development:** `WeSign\appsettings.Development.json`

## Build and Run
```bash
# Build entire solution
dotnet build WeSignV3.sln

# Run main API
dotnet run --project WeSign

# Access Swagger: https://localhost:7001/swagger
```

---

# Project Structure Overview

## Solution Architecture Overview
The WeSignV3.sln follows a multi-project architecture with 25+ projects:

```
WeSign/                    # Main Web API (Startup project)
├── Areas/
│   ├── Api/              # REST API Controllers (v3/[controller])
│   │   └── Controllers/  
│   │       ├── UsersController.cs
│   │       ├── DocumentCollectionsController.cs  
│   │       ├── TemplatesController.cs
│   │       └── [10+ more controllers]
│   └── Ui/               # UI Controllers (Ui/v3/[controller])
├── Models/               # DTOs and ViewModels
├── Hubs/                # SignalR Hubs
├── appsettings.json     # Configuration
└── Startup.cs           # Service configuration

BL/                       # Business Logic Layer
├── Handlers/             # Business logic handlers
├── Connectors/           # Data access connectors
└── Services/             # Domain services

DAL/                      # Data Access Layer
├── DAOs/                 # Data Access Objects
├── WeSignEntities.cs     # Entity Framework DbContext
└── Migrations/           # Database migrations

Common/                   # Shared components
├── Enums/                # Application enums
├── Models/               # Domain models
└── Extensions/           # Extension methods

[Specialized Projects]
Certificate/              # Smart card integration
PdfHandler/               # PDF processing
WeSignManagement/         # Management portal
WeSignSigner/            # Desktop client
UserWcfService/          # Legacy WCF services
```

## Project Dependencies Flow
```
WeSign → BL, DAL, Common, Certificate
BL → Common, PdfHandler, SignatureServiceConnector
DAL → Common
WeSignManagement → Common, DAL, ManagementBL, PdfHandler
WeSignSigner → Common, DAL, SignerBL
```

---

# Learning Roadmap: Controller to Database

## 🎯 Follow the User Registration Flow

We'll trace a **user registration request** from HTTP request to database to understand the complete architecture.

## Phase 1: Entry Point - API Controller Layer
**Duration: Day 1** | **Goal: Understand HTTP request handling**

### 1.1 Start Here: UsersController.cs
**📂 File:** `WeSign\Areas\Api\Controllers\UsersController.cs`

```csharp
[Area("Api")]
[Route("v3/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserBl _userBl;

    [HttpPost]
    public async Task<IActionResult> SignUpAsync(CreateUserDTO input)
    {
        // 1. DTO to Domain model mapping
        var user = new User()
        {
            Name = input.Name,
            Password = input.Password,
            Email = input.Email.ToLower(),
            UserConfiguration = new UserConfiguration()
            {
                Language = input.Language
            },
            Username = input.Username,
        };

        // 2. Business logic delegation
        await _userBl.ValidateReCAPCHAAsync(input.ReCAPCHA);
        string link = await _userBl.SignUp(user, input.SendActivationLink);
        
        // 3. HTTP response
        return Ok(new LinkResponse { Link = link });
    }
}
```

**Key Learning Points:**
- **Area-based routing**: `[Area("Api")]` creates `/v3/users` endpoint
- **DTO pattern**: `CreateUserDTO` defines API contract
- **Thin controllers**: No business logic, just HTTP concerns
- **Dependency injection**: `IUserBl _userBl` injected service

### 1.2 DTO (Data Transfer Object)
**📂 File:** `WeSign\Models\Users\CreateUserDTO.cs`

```csharp
public class CreateUserDTO
{
    public string Name { get; set; }
    public Language Language { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string ReCAPCHA { get; set; }                    // API-specific field
    public bool SendActivationLink { get; set; } = true;   // Default value
    public string Username { get; set; }
}
```

**Exercise 1:** 
1. Open `WeSign\Areas\Api\Controllers\UsersController.cs`
2. Find the `SignUpAsync` method
3. Compare `CreateUserDTO` with domain `User` model
4. Notice API-specific fields like `ReCAPCHA`

---

## Phase 2: Business Logic Layer - The Brain
**Duration: Day 2-3** | **Goal: Understand business orchestration**

### 2.1 Business Logic Handler
**📂 File:** `BL\Handlers\UsersHandler.cs`

```csharp
public class UsersHandler
{
    // 25+ dependency injections
    private readonly ILogger<UsersHandler> _logger;
    private readonly IUserConnector _userConnector;          // Data access
    private readonly IGroupConnector _groupConnector;        // Related data
    private readonly IEmailService _email;                   // External services
    private readonly IPKBDF2Handler _pkbdf2Handler;          // Security
    private readonly IConfiguration _configuration;          // Settings
    // ... 20+ more dependencies

    public async Task<string> SignUp(User user, bool sendActivationLink = true)
    {
        // 1. INPUT VALIDATION
        if (user == null)
            throw new Exception($"Null input - user is null");

        // 2. CONFIGURATION CHECKS
        Configuration configuration = await _configuration.ReadAppConfiguration();
        if (!configuration.EnableFreeTrailUsers)
            throw new InvalidOperationException(ResultCode.ForbiddenToCreateFreeTrailUser.GetNumericString());

        // 3. BUSINESS RULES APPLICATION
        user.CompanyId = Consts.FREE_ACCOUNTS_COMPANY_ID;
        user.Type = UserType.Editor;
        user.CreationTime = _dater.UtcNow();

        // 4. SECURITY - Password hashing
        bool passwordSent = false;
        if (!string.IsNullOrWhiteSpace(user.Password))
        {
            user.Password = _pkbdf2Handler.Generate(user.Password);
            user.PasswordSetupTime = _dater.UtcNow();
            passwordSent = true;
        }

        // 5. BUSINESS VALIDATION
        if (await _userConnector.Exists(user))
        {
            _logger.Warning($"User {user.Email} try to sign up Again");
            return "";
        }

        // 6. TRANSACTION-LIKE OPERATIONS
        try
        {
            await InitProgramUtilization(user);
            await CreateGroup(user);
            await _userConnector.Create(user);
        }
        catch
        {
            // Manual rollback on failure
            await _programUtilizationConnector.Delete(new ProgramUtilization { Id = user.ProgramUtilization?.Id ?? Guid.Empty });
            await _groupConnector.Delete(new Group { Id = user.GroupId });
            throw;
        }

        // 7. SUCCESS LOGGING
        _logger.Information("Successfully create user [{UserId}] with email {UserEmail}", user.Id, user.Email);

        // 8. SIDE EFFECTS - Email notifications
        string link = "";
        if (passwordSent)
        {
            link = await _email.Activation(user, sendActivationLink);
        }
        else
        {
            string resetPasswordToken = await _oneTimeTokens.GenerateResetPasswordToken(user);
            link = await _email.ResetPassword(user, resetPasswordToken);
        }

        return link;
    }
}
```

**Key Patterns:**
- **Extensive DI**: 25+ injected services
- **Configuration-driven**: Feature flags control behavior
- **Security**: PBKDF2 password hashing
- **Manual transactions**: Cleanup on failure
- **Structured logging**: With context
- **Side effects**: Email, logging after main operation

**Exercise 2:**
1. Open `BL\Handlers\UsersHandler.cs`
2. Count dependencies in constructor
3. Trace the `SignUp` method flow
4. Find helper methods like `InitProgramUtilization`

---

## Phase 3: Data Access Abstraction - Connectors
**Duration: Day 4** | **Goal: Understand data access patterns**

### 3.1 Connector Interface
**📂 File:** `BL\Connectors\IUserConnector.cs` (likely location)

```csharp
public interface IUserConnector
{
    Task<User> Create(User user);
    Task<User> GetById(Guid id);
    Task<User> GetByEmail(string email);
    Task<bool> Exists(User user);
    Task Update(User user);
    Task Delete(Guid id);
}
```

### 3.2 Connector Implementation
**📂 File:** `BL\Connectors\UserConnector.cs`

```csharp
public class UserConnector : IUserConnector
{
    private readonly WeSignEntities _context;
    private readonly ILogger<UserConnector> _logger;

    public async Task<User> Create(User user)
    {
        // 1. Convert domain model to DAO
        var userDao = new UserDAO(user);
        
        // 2. Entity Framework operations
        _context.Users.Add(userDao);
        await _context.SaveChangesAsync();
        
        // 3. Convert back to domain model
        return userDao.ToDomainModel();
    }

    public async Task<bool> Exists(User user)
    {
        return await _context.Users
            .AnyAsync(u => u.Email == user.Email);
    }
}
```

**Key Patterns:**
- **Repository pattern**: Abstract data access
- **Domain/DAO separation**: Different models for business vs database
- **EF Core**: ORM for database operations
- **Conversion layer**: Transform between layers

**Exercise 3:**
1. Find UserConnector implementation
2. Trace the `Create()` method
3. Notice DAO conversion pattern
4. Compare with other Connector classes

---

## Phase 4: Domain Models - Business Objects
**Duration: Day 5** | **Goal: Understand core business entities**

### 4.1 Domain Model
**📂 File:** `Common\Models\User.cs`

```csharp
public class User
{
    public Guid Id { get; set; }
    public Guid GroupId { get; set; }
    public Guid CompanyId { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public UserType Type { get; set; }
    public UserStatus Status { get; set; }
    public DateTime CreationTime { get; set; }
    public DateTime PasswordSetupTime { get; set; }
    
    // Navigation properties
    public UserConfiguration UserConfiguration { get; set; }
    public ProgramUtilization ProgramUtilization { get; set; }
    
    // Collections
    public List<AdditionalGroupMapper> AdditionalGroupsMapper { get; set; }
    
    // Business methods
    public bool IsActive => Status == UserStatus.Active;
}
```

### 4.2 Enums and Value Objects
**📂 Files:** `Common\Enums\`

```csharp
public enum UserType
{
    Viewer = 1,
    Editor = 2,
    Admin = 3,
    CompanyAdmin = 4
}

public enum UserStatus
{
    Inactive = 0,
    Active = 1,
    Suspended = 2,
    Deleted = 3
}
```

**Key Points:**
- **Pure business objects**: No database or API concerns
- **Rich models**: Can contain business logic
- **Navigation properties**: Related entities
- **Value objects**: Enums for type safety

**Exercise 4:**
1. Compare `User` vs `CreateUserDTO` vs `UserDAO`
2. Find other domain models in Common/Models/
3. Explore enums in Common/Enums/

---

## Phase 5: Data Layer - Entity Framework
**Duration: Day 6-7** | **Goal: Understand database persistence**

### 5.1 Data Access Object (DAO)
**📂 File:** `DAL\DAOs\Users\UserDAO.cs`

```csharp
public class UserDAO
{
    [Key]
    public Guid Id { get; set; }
    public Guid GroupId { get; set; }
    public Guid CompanyId { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public UserType Type { get; set; }
    public UserStatus Status { get; set; }
    public DateTime CreationTime { get; set; }
    public DateTime PasswordSetupTime { get; set; }
    
    // EF Navigation Properties (virtual for lazy loading)
    public virtual ProgramUtilizationDAO ProgramUtilization { get; set; }
    public virtual UserConfigurationDAO UserConfiguration { get; set; }
    public virtual CompanyDAO Company { get; set; }
    public virtual ICollection<DocumentCollectionDAO> DocumentCollections { get; set; }
    public virtual ICollection<AdditionalGroupMapperDAO> AdditionalGroupsMapper { get; set; }

    // Parameterless constructor for EF
    public UserDAO() { }
    
    // Conversion constructor
    public UserDAO(User user)
    {
        Id = user.Id == Guid.Empty ? default : user.Id;
        CompanyId = user.CompanyId == Guid.Empty ? default : user.CompanyId;
        GroupId = user.GroupId == Guid.Empty ? default : user.GroupId;
        Name = user.Name;
        Email = user.Email;
        Password = user.Password;
        CreationTime = user.CreationTime;
        Status = user.Status;
        Type = user.Type;
        PasswordSetupTime = user.PasswordSetupTime;
        UserConfiguration = new UserConfigurationDAO(user.UserConfiguration);
        // ... map all properties
    }

    // Conversion method back to domain
    public User ToDomainModel()
    {
        return new User
        {
            Id = this.Id,
            Name = this.Name,
            Email = this.Email,
            // ... map all properties back
        };
    }
}
```

### 5.2 DbContext Configuration
**📂 File:** `DAL\WeSignEntities.cs`

```csharp
public class WeSignEntities : DbContext
{
    public DbSet<UserDAO> Users { get; set; }
    public DbSet<CompanyDAO> Companies { get; set; }
    public DbSet<DocumentCollectionDAO> DocumentCollections { get; set; }
    // ... 30+ more DbSets

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureUserEntity(modelBuilder);
        ConfigureCompanyEntity(modelBuilder);
        // ... configure all entities
    }

    private void ConfigureUserEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserDAO>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.HasIndex(e => e.Email).IsUnique();
            
            // Relationships
            entity.HasOne(d => d.Company)
                  .WithMany(p => p.Users)
                  .HasForeignKey(d => d.CompanyId);

            entity.HasOne(d => d.UserConfiguration)
                  .WithOne(p => p.User)
                  .HasForeignKey<UserConfigurationDAO>(d => d.UserId);
        });
    }
}
```

**Key Points:**
- **EF entities**: Database-focused with attributes
- **Virtual properties**: Enable lazy loading
- **Conversion pattern**: To/from domain models
- **Fluent API**: Configure relationships, constraints

### 5.3 Database Migrations
**📂 Folder:** `DAL\Migrations\`

```bash
# Add migration
dotnet ef migrations add AddUserLastLoginDate --project DAL --startup-project WeSign

# Update database
dotnet ef database update --project DAL --startup-project WeSign
```

**Exercise 5:**
1. Open `DAL\DAOs\Users\UserDAO.cs`
2. Find conversion constructor and methods
3. Examine `DAL\WeSignEntities.cs`
4. Look at migration files in `DAL\Migrations\`

---

## Phase 6: Configuration and Dependency Injection
**Duration: Day 8** | **Goal: Understand system wiring**

### 6.1 Service Registration
**📂 File:** `WeSign\Startup.cs`

```csharp
public void ConfigureServices(IServiceCollection services)
{
    // Database
    services.AddDbContext<WeSignEntities>(options =>
        options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

    // Business Logic
    services.AddScoped<IUserBl, UsersHandler>();
    services.AddScoped<IUserConnector, UserConnector>();
    
    // Infrastructure Services
    services.AddScoped<IEmailService, EmailService>();
    services.AddScoped<IPKBDF2Handler, PKBDF2Handler>();
    services.AddScoped<IOneTimeTokens, OneTimeTokensService>();
    services.AddScoped<IDater, Dater>();
    
    // Configuration
    services.Configure<JwtSettings>(Configuration.GetSection("JwtSettings"));
    
    // External Services
    services.AddHangfire(config => config.UseMemoryStorage());
    services.AddSignalR();
    
    // Authentication
    services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options => { /* JWT config */ });
}

public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseEndpoints(endpoints =>
    {
        endpoints.MapControllerRoute(
            name: "areas",
            pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
        endpoints.MapHub<SmartCardSigningHub>("v3/smartcardsocket");
    });
}
```

**Exercise 6:**
1. Find all service registrations in Startup.cs
2. Trace how UsersHandler gets its dependencies
3. Understand the middleware pipeline order

---

# 🎯 Complete Request Flow Summary

**HTTP POST /v3/users → Controller → Business Logic → Data Access → Database**

```
1. HTTP Request: POST /v3/users
   Body: CreateUserDTO { Name, Email, Password, ... }
   ↓
2. Controller: UsersController.SignUpAsync(CreateUserDTO input)
   - Model binding and validation
   - DTO to domain model conversion: new User() { ... }
   ↓
3. Business Logic: UsersHandler.SignUp(User user, bool sendActivationLink)
   - Input validation
   - Configuration checks
   - Business rules application
   - Security (password hashing)
   - Existence validation
   - Complex orchestration with rollback
   ↓
4. Data Access: UserConnector.Create(User user)
   - Domain to DAO conversion: new UserDAO(user)
   - Entity Framework operations
   ↓
5. Database Context: WeSignEntities
   - _context.Users.Add(userDao)
   - await _context.SaveChangesAsync()
   ↓
6. Database: SQL Server
   - INSERT INTO Users (Id, Email, Name, ...) VALUES (...)
   ↓
7. Response Flow (reverse):
   DAO → Domain → Business Logic → Controller → HTTP Response
```

---

# Implementation Patterns

## Controller Patterns
```csharp
// Standard API controller structure
[Area("Api")]
[Route("v3/[controller]")]
[ApiController]
[Authorize]  // When authentication required
public class ExampleController : ControllerBase
{
    private readonly IExampleBl _exampleBl;

    [HttpPost]
    public async Task<IActionResult> Create(CreateExampleDTO input)
    {
        var domainModel = new Example
        {
            // Map DTO to domain model
        };
        
        var result = await _exampleBl.Create(domainModel);
        return Ok(result);
    }
}
```

## Handler Pattern
```csharp
public class ExampleHandler : IExampleBl
{
    // Constructor with dependencies
    public ExampleHandler(
        ILogger<ExampleHandler> logger,
        IExampleConnector connector,
        IConfiguration configuration)
    {
        _logger = logger;
        _connector = connector;
        _configuration = configuration;
    }

    public async Task<Example> Create(Example example)
    {
        // 1. Validation
        // 2. Business rules
        // 3. Data persistence
        // 4. Side effects
        // 5. Return result
    }
}
```

## Connector Pattern
```csharp
public class ExampleConnector : IExampleConnector
{
    private readonly WeSignEntities _context;

    public async Task<Example> Create(Example example)
    {
        var dao = new ExampleDAO(example);
        _context.Examples.Add(dao);
        await _context.SaveChangesAsync();
        return dao.ToDomainModel();
    }
}
```

---

# API Development Guide

## Adding a New API Endpoint

### Step 1: Create DTO
**📂 File:** `WeSign\Models\[Domain]\[Action][Domain]DTO.cs`

```csharp
public class UpdateUserDTO
{
    public string Name { get; set; }
    public string Email { get; set; }
    // Add validation attributes as needed
    [Required]
    [EmailAddress]
    public string Email { get; set; }
}
```

### Step 2: Add Controller Action
**📂 File:** `WeSign\Areas\Api\Controllers\UsersController.cs`

```csharp
[HttpPut("{id}")]
public async Task<IActionResult> UpdateAsync(Guid id, UpdateUserDTO input)
{
    var user = await _userBl.GetByIdAsync(id);
    if (user == null) return NotFound();
    
    user.Name = input.Name;
    user.Email = input.Email;
    
    await _userBl.UpdateAsync(user);
    return Ok();
}
```

### Step 3: Implement Business Logic
**📂 File:** `BL\Handlers\UsersHandler.cs`

```csharp
public async Task UpdateAsync(User user)
{
    // Validation
    if (await _userConnector.EmailExistsForOtherUser(user.Email, user.Id))
        throw new InvalidOperationException("Email already exists");
    
    // Business logic
    user.LastModifiedDate = _dater.UtcNow();
    
    // Persistence
    await _userConnector.Update(user);
    
    // Logging
    _logger.Information("Updated user {UserId}", user.Id);
}
```

### Step 4: Add Data Access Method
**📂 File:** `BL\Connectors\UserConnector.cs`

```csharp
public async Task Update(User user)
{
    var existingDao = await _context.Users.FindAsync(user.Id);
    if (existingDao == null)
        throw new NotFoundException($"User {user.Id} not found");
        
    // Update properties
    existingDao.Name = user.Name;
    existingDao.Email = user.Email;
    existingDao.LastModifiedDate = user.LastModifiedDate;
    
    await _context.SaveChangesAsync();
}
```

---

# Testing Guidelines

## Unit Test Structure
```csharp
[TestClass]
public class UsersHandlerTests
{
    private Mock<IUserConnector> _userConnectorMock;
    private Mock<ILogger<UsersHandler>> _loggerMock;
    private Mock<IConfiguration> _configurationMock;
    private UsersHandler _handler;

    [TestInitialize]
    public void Setup()
    {
        _userConnectorMock = new Mock<IUserConnector>();
        _loggerMock = new Mock<ILogger<UsersHandler>>();
        _configurationMock = new Mock<IConfiguration>();
        
        _handler = new UsersHandler(
            _loggerMock.Object, 
            _userConnectorMock.Object, 
            _configurationMock.Object
            /* other mocks */);
    }

    [TestMethod]
    public async Task SignUp_ValidUser_ReturnsActivationLink()
    {
        // Arrange
        var user = new User { Email = "test@example.com", Name = "Test User" };
        _userConnectorMock.Setup(x => x.Exists(It.IsAny<User>())).ReturnsAsync(false);
        _configurationMock.Setup(x => x.ReadAppConfiguration()).ReturnsAsync(new Configuration { EnableFreeTrailUsers = true });

        // Act
        var result = await _handler.SignUp(user, true);

        // Assert
        Assert.IsNotNull(result);
        _userConnectorMock.Verify(x => x.Create(It.IsAny<User>()), Times.Once);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Successfully create user")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [TestMethod]
    public async Task SignUp_DuplicateEmail_ReturnsEmptyString()
    {
        // Arrange
        var user = new User { Email = "existing@example.com" };
        _userConnectorMock.Setup(x => x.Exists(user)).ReturnsAsync(true);

        // Act
        var result = await _handler.SignUp(user, true);

        // Assert
        Assert.AreEqual("", result);
        _userConnectorMock.Verify(x => x.Create(It.IsAny<User>()), Times.Never);
    }
}
```

## Integration Test Patterns
```csharp
[TestClass]
public class UsersControllerIntegrationTests : IntegrationTestBase
{
    [TestMethod]
    public async Task SignUp_ValidRequest_Returns200()
    {
        // Arrange
        var request = new CreateUserDTO
        {
            Name = "Test User",
            Email = "test@example.com",
            Password = "SecurePassword123"
        };

        // Act
        var response = await TestClient.PostAsJsonAsync("/v3/users", request);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<LinkResponse>(content);
        Assert.IsNotNull(result.Link);
    }
}
```

---

# Debugging and Troubleshooting

## Common Issues and Solutions

### 1. Entity Framework Issues
**Symptoms:** Database connection errors, migration failures
**Solutions:**
- Check connection string in appsettings.json
- Verify database exists: `dotnet ef database info --project DAL --startup-project WeSign`
- Update migrations: `dotnet ef database update --project DAL --startup-project WeSign`
- Check SQL with SQL Profiler or EF logging

### 2. Dependency Injection Errors
**Symptoms:** Service resolution exceptions at startup
**Solutions:**
- Ensure all services registered in Startup.cs
- Check interface/implementation matching
- Verify constructor parameter types
- Use logging to trace service resolution

### 3. Authentication/Authorization Problems
**Symptoms:** 401 Unauthorized, 403 Forbidden responses
**Solutions:**
- Verify JWT configuration in appsettings.json
- Check token expiration and claims
- Use browser dev tools to inspect Authorization headers
- Test with Postman/Thunder Client

### 4. API Routing Issues
**Symptoms:** 404 Not Found for existing endpoints
**Solutions:**
- Check Area and Route attributes on controllers
- Verify endpoint URL structure: `/v3/[controller]/[action]`
- Test with Swagger UI
- Check middleware order in Startup.cs

## Debugging Techniques

### 1. Structured Logging
```csharp
// Good logging with context
_logger.Information("User {UserId} attempted signup with email {Email}", 
    user.Id, user.Email);

// Error logging with full context  
_logger.Error(ex, "Failed to create user {UserId} with email {Email}", 
    user.Id, user.Email);

// Warning for business rule violations
_logger.Warning("User {Email} attempted duplicate signup", user.Email);
```

### 2. Breakpoint Strategy
- **Controllers**: Set breakpoints at method entry to verify request binding
- **Handlers**: Set breakpoints before/after major business logic
- **Connectors**: Set breakpoints before database operations
- **Exception handling**: Set breakpoints in catch blocks

### 3. Database Debugging
```csharp
// Enable EF logging in appsettings.json
{
  "Logging": {
    "LogLevel": {
      "Microsoft.EntityFrameworkCore.Database.Command": "Information"
    }
  }
}
```

### 4. API Testing Tools
- **Swagger UI**: https://localhost:7001/swagger
- **Postman**: Import OpenAPI spec from Swagger
- **Visual Studio**: HTTP file testing
- **Browser DevTools**: Network tab for request/response inspection

## Performance Troubleshooting

### Database Performance
- Use EF query logging to identify N+1 queries
- Implement proper Include() for eager loading
- Add database indexes for frequently queried columns
- Use async methods consistently

### Memory Issues
- Check for circular references in navigation properties
- Dispose DbContext properly (handled by DI container)
- Monitor object lifetimes with memory profiler

### Request Performance
- Use Application Insights for production monitoring
- Implement health checks: `/health`
- Monitor response times in logs
- Use async/await consistently

---

# Common Development Scenarios

## Database Schema Changes

### Adding New Property to Entity
1. **Update Domain Model** (`Common\Models\User.cs`):
```csharp
public DateTime LastLoginDate { get; set; }
```

2. **Update DAO** (`DAL\DAOs\Users\UserDAO.cs`):
```csharp
public DateTime LastLoginDate { get; set; }

// Update conversion constructor
public UserDAO(User user)
{
    // ... existing mappings
    LastLoginDate = user.LastLoginDate;
}
```

3. **Update Entity Configuration** (if needed):
```csharp
entity.Property(e => e.LastLoginDate).HasDefaultValue(DateTime.MinValue);
```

4. **Generate and Apply Migration**:
```bash
dotnet ef migrations add AddLastLoginDate --project DAL --startup-project WeSign
dotnet ef database update --project DAL --startup-project WeSign
```

### Adding New Entity
1. **Create Domain Model** (`Common\Models\Notification.cs`)
2. **Create DAO** (`DAL\DAOs\Notifications\NotificationDAO.cs`)
3. **Add to DbContext** (`DAL\WeSignEntities.cs`):
```csharp
public DbSet<NotificationDAO> Notifications { get; set; }
```
4. **Configure Entity** and generate migration

## Adding New Business Feature

### Example: User Profile Update
1. **Create DTO** (`WeSign\Models\Users\UpdateProfileDTO.cs`)
2. **Add Controller Action** (`WeSign\Areas\Api\Controllers\UsersController.cs`)
3. **Implement Business Logic** (`BL\Handlers\UsersHandler.cs`)
4. **Add Data Access Method** (`BL\Connectors\UserConnector.cs`)
5. **Write Tests** for each layer

## Configuration Management

### Adding New Configuration Section
1. **Update appsettings.json**:
```json
{
  "NotificationSettings": {
    "EmailEnabled": true,
    "SmsEnabled": false,
    "PushEnabled": true
  }
}
```

2. **Create Configuration Class** (`Common\Configuration\NotificationSettings.cs`):
```csharp
public class NotificationSettings
{
    public bool EmailEnabled { get; set; }
    public bool SmsEnabled { get; set; }
    public bool PushEnabled { get; set; }
}
```

3. **Register in Startup.cs**:
```csharp
services.Configure<NotificationSettings>(Configuration.GetSection("NotificationSettings"));
```

4. **Inject in Handler**:
```csharp
public class NotificationHandler
{
    private readonly IOptions<NotificationSettings> _settings;
    
    public NotificationHandler(IOptions<NotificationSettings> settings)
    {
        _settings = settings;
    }
}
```

---

# File Locations Quick Reference

## Key Files for User Registration Flow:

1. **Controller Entry Point:**
   - `WeSign\Areas\Api\Controllers\UsersController.cs` (SignUpAsync method ~line 40-60)

2. **Business Logic:**
   - `BL\Handlers\UsersHandler.cs` (SignUp method ~line 108-168)

3. **Data Access:**
   - `BL\Connectors\UserConnector.cs` (Create method)
   - Interface: `BL\Connectors\IUserConnector.cs`

4. **Domain Model:**
   - `Common\Models\User.cs`

5. **Data Layer:**
   - `DAL\DAOs\Users\UserDAO.cs`
   - `DAL\WeSignEntities.cs` (DbContext)

6. **Configuration:**
   - `WeSign\Startup.cs`
   - `WeSign\appsettings.json`

7. **DTOs:**
   - `WeSign\Models\Users\CreateUserDTO.cs`

8. **Enums:**
   - `Common\Enums\UserType.cs`
   - `Common\Enums\UserStatus.cs`

## Navigation Tips:
- **Base Path:** `C:\Users\gals\source\repos\user-backend\`
- **VS Shortcuts:** Ctrl+T (Go to Type), F12 (Go to Definition), Ctrl+Shift+F (Find in Files)
- **Solution File:** `WeSignV3.sln`

This complete guide provides everything a new developer needs to understand and work effectively with the WeSign codebase, from environment setup through advanced development scenarios.