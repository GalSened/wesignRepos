# WeSign Developer Onboarding Guide - Detailed Implementation

## Table of Contents
1. [Development Environment Setup](#development-environment-setup)
2. [Code Architecture Deep Dive](#code-architecture-deep-dive) 
3. [Implementation Patterns](#implementation-patterns)
4. [API Development Walkthrough](#api-development-walkthrough)
5. [Data Layer Implementation](#data-layer-implementation)
6. [Business Logic Patterns](#business-logic-patterns)
7. [Common Development Scenarios](#common-development-scenarios)
8. [Testing Guidelines](#testing-guidelines)
9. [Debugging and Troubleshooting](#debugging-and-troubleshooting)

## Development Environment Setup

### Prerequisites
- Visual Studio 2022 (or VS Code with C# extension)
- .NET 9.0 SDK
- SQL Server (LocalDB or full instance)
- Git for version control

### Solution Structure Overview
The WeSignV3.sln contains 25+ projects organized in a clean architecture pattern:

```
WeSign/                     # Main Web API (ASP.NET Core)
├── Areas/
│   ├── Api/               # REST API Controllers
│   └── UI/                # Web UI Controllers
├── Models/                # DTOs and ViewModels
└── Startup.cs            # Configuration

BL/                        # Business Logic Layer
├── Handlers/             # Business logic handlers
├── Connectors/           # Data access connectors
└── Services/             # Domain services

DAL/                       # Data Access Layer
├── DAOs/                 # Data Access Objects
├── WeSignEntities.cs     # Entity Framework DbContext
└── Migrations/           # Database migrations

Common/                    # Shared components
├── Enums/                # Application enums
├── Extensions/           # Extension methods
└── Models/               # Domain models

Certificate/               # Smart card integration
PDF/                      # PDF processing
UserWcfService/           # Legacy WCF services
WeSignManagement/         # Management portal
WeSignSigner/            # Desktop signing client
```

### Key Configuration Files
- `WeSign/appsettings.json`: Main API configuration
- `WeSign/WeSign.csproj`: Project dependencies and settings
- `WeSignV3.sln`: Solution structure definition

## Code Architecture Deep Dive

### N-Tier Layered Architecture
The application follows a strict separation of concerns:

1. **Presentation Layer (WeSign)**
   - Controllers handle HTTP requests/responses
   - Areas separate API and UI concerns
   - DTOs for data transfer

2. **Business Logic Layer (BL)**
   - Handlers contain business rules
   - Services provide domain functionality
   - Connectors abstract data access

3. **Data Access Layer (DAL)**
   - Entity Framework Code-First approach
   - DAOs represent database entities
   - Repository pattern implementation

## Implementation Patterns

### Handler Pattern Example
The `UsersHandler` class demonstrates the core business logic pattern:

```csharp
public class UsersHandler
{
    // 25+ dependency injections for various services
    private readonly ILogger<UsersHandler> _logger;
    private readonly IUserConnector _userConnector;
    private readonly IEmailService _email;
    private readonly IPKBDF2Handler _pkbdf2Handler;
    // ... more dependencies

    public async Task<string> SignUp(User user, bool sendActivationLink = true)
    {
        // Input validation
        if (user == null)
            throw new Exception($"Null input - user is null");

        // Configuration check
        Configuration configuration = await _configuration.ReadAppConfiguration();
        if (!configuration.EnableFreeTrailUsers)
            throw new InvalidOperationException(ResultCode.ForbiddenToCreateFreeTrailUser.GetNumericString());

        // Business logic implementation
        user.CompanyId = Consts.FREE_ACCOUNTS_COMPANY_ID;
        user.Type = UserType.Editor;
        user.CreationTime = _dater.UtcNow();

        // Password handling
        if (!string.IsNullOrWhiteSpace(user.Password))
        {
            user.Password = _pkbdf2Handler.Generate(user.Password);
            user.PasswordSetupTime = _dater.UtcNow();
        }

        // Existence check
        if (await _userConnector.Exists(user))
        {
            _logger.Warning($"User {user.Email} try to sign up Again");
            return "";
        }

        // Database operations with transaction-like behavior
        try
        {
            await InitProgramUtilization(user);
            await CreateGroup(user);
            await _userConnector.Create(user);
        }
        catch
        {
            // Cleanup on failure
            await _programUtilizationConnector.Delete(new ProgramUtilization { Id = user.ProgramUtilization?.Id ?? Guid.Empty });
            await _groupConnector.Delete(new Group { Id = user.GroupId });
            throw;
        }

        // Success logging
        _logger.Information("Successfully create user [{UserId}] with email {UserEmail}", user.Id, user.Email);

        // Email notification logic
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

### Key Patterns Demonstrated:
1. **Dependency Injection**: Constructor injection with 25+ services
2. **Configuration-Driven Logic**: Feature flags and settings
3. **Transaction-like Behavior**: Manual rollback on exceptions
4. **Structured Logging**: Using Serilog with context
5. **Password Security**: PBKDF2 hashing
6. **Email Integration**: Conditional email sending
7. **Error Handling**: Comprehensive exception management

## API Development Walkthrough

### Controller Structure
Controllers follow ASP.NET Core patterns with area-based organization:

```csharp
[Area("Api")]
[Route("v3/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserBl _userBl;

    [HttpPost]
    public async Task<IActionResult> SignUpAsync(CreateUserDTO input)
    {
        // DTO to Domain model mapping
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

        // Business logic delegation
        await _userBl.ValidateReCAPCHAAsync(input.ReCAPCHA);
        string link = await _userBl.SignUp(user, input.SendActivationLink);
        
        return Ok(new LinkResponse { Link = link });
    }
}
```

### DTO Pattern
Data Transfer Objects provide clean API contracts:

```csharp
public class CreateUserDTO
{
    public string Name { get; set; }
    public Language Language { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string ReCAPCHA { get; set; }
    public bool SendActivationLink { get; set; } = true;
    public string Username { get; set; }
}
```

## Data Layer Implementation

### Entity Framework Configuration
The `WeSignEntities` DbContext manages 30+ entities with comprehensive configuration:

```csharp
public class WeSignEntities : DbContext
{
    public DbSet<UserDAO> Users { get; set; }
    public DbSet<CompanyDAO> Companies { get; set; }
    // ... 30+ more DbSets

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureUserEntity(modelBuilder);
        ConfigureCompanyEntity(modelBuilder);
        // ... configuration methods for each entity
    }
}
```

### DAO Pattern
Data Access Objects represent database entities with conversion logic:

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
    
    // Navigation properties
    public virtual ProgramUtilizationDAO ProgramUtilization { get; set; }
    public virtual UserConfigurationDAO UserConfiguration { get; set; }
    public virtual CompanyDAO Company { get; set; }
    
    // Collection navigation properties
    public virtual ICollection<DocumentCollectionDAO> DocumentCollections { get; set; }
    public virtual ICollection<AdditionalGroupMapperDAO> AdditionalGroupsMapper { get; set; }

    // Conversion constructor
    public UserDAO(User user)
    {
        Id = user.Id == Guid.Empty ? default : user.Id;
        CompanyId = user.CompanyId == Guid.Empty ? default : user.CompanyId;
        GroupId = user.GroupId == Guid.Empty ? default : user.GroupId;
        Name = user.Name;
        Email = user.Email;
        Password = user.Password;
        // ... mapping all properties
    }
}
```

## Business Logic Patterns

### Service Dependencies
The business layer uses extensive dependency injection:

```csharp
public class UsersHandler
{
    public UsersHandler(
        ILogger<UsersHandler> logger,
        IUserConnector userConnector,
        IGroupConnector groupConnector,
        IProgramUtilizationConnector programUtilizationConnector,
        IEmailService email,
        IPKBDF2Handler pkbdf2Handler,
        IOneTimeTokens oneTimeTokens,
        IDater dater,
        IConfiguration configuration,
        // ... 15+ more dependencies
    )
    {
        _logger = logger;
        _userConnector = userConnector;
        // ... assignment of all dependencies
    }
}
```

### Configuration Management
Feature flags and settings drive business logic:

```csharp
Configuration configuration = await _configuration.ReadAppConfiguration();
if (!configuration.EnableFreeTrailUsers)
{
    throw new InvalidOperationException(ResultCode.ForbiddenToCreateFreeTrailUser.GetNumericString());
}

// Later in the same method:
if (!(await _configuration.ReadAppConfiguration()).ShouldReturnActivationLinkInAPIResponse)
{
    link = "";
}
```

## Common Development Scenarios

### Adding a New API Endpoint

1. **Create DTO in Models folder**:
```csharp
// WeSign/Models/Users/UpdateUserDTO.cs
public class UpdateUserDTO
{
    public string Name { get; set; }
    public string Email { get; set; }
    // Add validation attributes as needed
}
```

2. **Add Controller Action**:
```csharp
[HttpPut("{id}")]
public async Task<IActionResult> UpdateUserAsync(Guid id, UpdateUserDTO input)
{
    var user = await _userBl.GetByIdAsync(id);
    if (user == null) return NotFound();
    
    user.Name = input.Name;
    user.Email = input.Email;
    
    await _userBl.UpdateAsync(user);
    return Ok();
}
```

3. **Implement Business Logic**:
```csharp
// BL/Handlers/UsersHandler.cs
public async Task UpdateAsync(User user)
{
    if (await _userConnector.EmailExistsForOtherUser(user.Email, user.Id))
    {
        throw new InvalidOperationException("Email already exists");
    }
    
    await _userConnector.Update(user);
    _logger.Information("Updated user {UserId}", user.Id);
}
```

### Database Migration Workflow

1. **Modify DAO**:
```csharp
public class UserDAO
{
    // Add new property
    public DateTime LastLoginDate { get; set; }
}
```

2. **Update Entity Configuration**:
```csharp
private void ConfigureUserEntity(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<UserDAO>(entity =>
    {
        entity.Property(e => e.LastLoginDate).HasDefaultValue(DateTime.MinValue);
    });
}
```

3. **Generate Migration**:
```bash
dotnet ef migrations add AddLastLoginDate --project DAL --startup-project WeSign
```

4. **Apply Migration**:
```bash
dotnet ef database update --project DAL --startup-project WeSign
```

## Testing Guidelines

### Unit Test Structure
```csharp
[TestClass]
public class UsersHandlerTests
{
    private Mock<IUserConnector> _userConnectorMock;
    private Mock<ILogger<UsersHandler>> _loggerMock;
    private UsersHandler _handler;

    [TestInitialize]
    public void Setup()
    {
        _userConnectorMock = new Mock<IUserConnector>();
        _loggerMock = new Mock<ILogger<UsersHandler>>();
        _handler = new UsersHandler(_loggerMock.Object, _userConnectorMock.Object, /* other mocks */);
    }

    [TestMethod]
    public async Task SignUp_ValidUser_ReturnsActivationLink()
    {
        // Arrange
        var user = new User { Email = "test@example.com", Name = "Test User" };
        _userConnectorMock.Setup(x => x.Exists(It.IsAny<User>())).ReturnsAsync(false);

        // Act
        var result = await _handler.SignUp(user, true);

        // Assert
        Assert.IsNotNull(result);
        _userConnectorMock.Verify(x => x.Create(It.IsAny<User>()), Times.Once);
    }
}
```

### Integration Test Patterns
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
    }
}
```

## Debugging and Troubleshooting

### Common Issues and Solutions

1. **Entity Framework Issues**:
   - Check connection string in appsettings.json
   - Verify migrations are up to date: `dotnet ef database update`
   - Use SQL Profiler to monitor database calls

2. **Dependency Injection Errors**:
   - Ensure services are registered in Startup.cs
   - Check constructor parameter types match registrations
   - Use logging to trace service resolution

3. **Authentication Problems**:
   - Verify JWT configuration in appsettings.json
   - Check token expiration and claims
   - Use browser dev tools to inspect Authorization headers

### Logging Best Practices
```csharp
// Structured logging with context
_logger.Information("User {UserId} attempted signup with email {Email}", user.Id, user.Email);

// Error logging with full context
_logger.Error(ex, "Failed to create user {UserId} with email {Email}", user.Id, user.Email);

// Warning for business rule violations
_logger.Warning("User {Email} attempted duplicate signup", user.Email);
```

### Performance Monitoring
- Use Application Insights for production monitoring
- Monitor database query performance with Entity Framework logging
- Implement health checks for external dependencies

This detailed guide provides new developers with concrete examples, implementation patterns, and troubleshooting guidance specific to the WeSign codebase architecture.