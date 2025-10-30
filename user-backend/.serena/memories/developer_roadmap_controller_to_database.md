# WeSign Developer Learning Roadmap - Controller to Database

## Overview
This roadmap takes you from the API surface (Controllers) down to the database layer, following the actual request flow through the WeSign architecture. Each step builds on the previous one.

## 🎯 Learning Path: User Registration Flow

We'll follow a **user registration request** from controller to database to understand the complete architecture.

---

## Phase 1: The Entry Point - API Controller Layer
**Duration: Day 1**
**Goal: Understand how HTTP requests enter the system**

### 1.1 Start Here: UsersController.cs
**File:** `WeSign/Areas/Api/Controllers/UsersController.cs`

```csharp
[Area("Api")]
[Route("v3/[controller]")]
public class UsersController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> SignUpAsync(CreateUserDTO input)
    {
        // This is your starting point - trace from here
    }
}
```

**Key Learning Points:**
- **Area-based routing**: `[Area("Api")]` groups related controllers
- **Route template**: `v3/[controller]` creates `/v3/users` endpoint
- **HTTP verb mapping**: `[HttpPost]` handles POST requests
- **DTO pattern**: `CreateUserDTO` defines the API contract
- **Async pattern**: All operations are async for scalability

**Exercise 1:** 
1. Open `WeSign/Areas/Api/Controllers/UsersController.cs`
2. Find the `SignUpAsync` method (line ~40)
3. Trace what happens with the `CreateUserDTO input`
4. Notice the dependency injection of `IUserBl _userBl`

### 1.2 Understanding the Controller Pattern
**Pattern Analysis:**
- Controllers are thin - they don't contain business logic
- They handle HTTP concerns: routing, model binding, response formatting
- They delegate to business logic layer via injected services
- They transform between DTOs (API contracts) and domain models

**Common Controller Attributes You'll See:**
```csharp
[ApiController]           // Automatic model validation
[Area("Api")]            // Groups controllers by area
[Route("v3/[controller]")] // URL routing template
[Authorize]              // Requires authentication
[SwaggerResponse(...)]   // API documentation
```

### 1.3 DTO (Data Transfer Object) Layer
**File:** `WeSign/Models/Users/CreateUserDTO.cs`

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

**Key Learning Points:**
- DTOs define the API surface - what clients send/receive
- They're different from domain models (internal representation)
- They include API-specific fields (like ReCAPCHA) not in domain
- Default values can be set (`SendActivationLink = true`)

**Exercise 2:**
1. Compare `CreateUserDTO` with the `User` domain model in Common/Models/
2. Notice which fields are API-only vs domain-only
3. Find other DTOs in WeSign/Models/ to see the pattern

---

## Phase 2: Business Logic Layer - The Brain
**Duration: Day 2-3**
**Goal: Understand business rules and orchestration**

### 2.1 Business Logic Interface
**File:** `BL/Interfaces/IUserBl.cs` (or similar)

The controller calls `_userBl.SignUp()` - this is the business logic interface that defines what operations are available.

### 2.2 Business Logic Handler
**File:** `BL/Handlers/UsersHandler.cs`

This is where the real work happens:

```csharp
public async Task<string> SignUp(User user, bool sendActivationLink = true)
{
    // 1. INPUT VALIDATION
    if (user == null)
        throw new Exception($"Null input - user is null");

    // 2. CONFIGURATION CHECKS
    Configuration configuration = await _configuration.ReadAppConfiguration();
    if (!configuration.EnableFreeTrailUsers)
        throw new InvalidOperationException(ResultCode.ForbiddenToCreateFreeTrailUser.GetNumericString());

    // 3. BUSINESS RULES
    user.CompanyId = Consts.FREE_ACCOUNTS_COMPANY_ID;
    user.Type = UserType.Editor;
    user.CreationTime = _dater.UtcNow();

    // 4. SECURITY
    if (!string.IsNullOrWhiteSpace(user.Password))
    {
        user.Password = _pkbdf2Handler.Generate(user.Password);
        user.PasswordSetupTime = _dater.UtcNow();
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
        // Cleanup on failure
        await _programUtilizationConnector.Delete(new ProgramUtilization { Id = user.ProgramUtilization?.Id ?? Guid.Empty });
        await _groupConnector.Delete(new Group { Id = user.GroupId });
        throw;
    }

    // 7. SIDE EFFECTS (Email, Logging)
    _logger.Information("Successfully create user [{UserId}] with email {UserEmail}", user.Id, user.Email);
    string link = await HandleEmailNotification(user, passwordSent, sendActivationLink);

    return link;
}
```

**Key Learning Points:**
- **Dependency Injection**: Handler has 25+ injected services
- **Configuration-Driven**: Business rules controlled by configuration
- **Security**: Password hashing with PBKDF2
- **Transactions**: Manual rollback pattern for consistency
- **Logging**: Structured logging with context
- **Error Handling**: Comprehensive exception management

### 2.3 Handler Dependencies
**Understanding the Constructor:**

```csharp
public UsersHandler(
    ILogger<UsersHandler> logger,
    IUserConnector userConnector,          // Data access
    IGroupConnector groupConnector,        // Related data
    IProgramUtilizationConnector programUtilizationConnector,
    IEmailService email,                   // External services
    IPKBDF2Handler pkbdf2Handler,         // Security
    IOneTimeTokens oneTimeTokens,         // Token management
    IDater dater,                         // Time abstraction
    IConfiguration configuration,         // Settings
    // ... 15+ more dependencies
)
```

**Dependency Categories:**
- **Data Access**: `*Connector` interfaces
- **External Services**: Email, SMS, etc.
- **Security**: Hashing, tokens, encryption
- **Infrastructure**: Logging, configuration, time
- **Domain Services**: Business-specific operations

**Exercise 3:**
1. Open `BL/Handlers/UsersHandler.cs`
2. Count all dependencies in the constructor
3. Categorize each dependency by its purpose
4. Find where each dependency is used in the `SignUp` method

---

## Phase 3: Data Access Abstraction - Connectors
**Duration: Day 4**
**Goal: Understand the data access abstraction layer**

### 3.1 Connector Interface Pattern
**File:** `BL/Connectors/IUserConnector.cs` (likely location)

```csharp
public interface IUserConnector
{
    Task<User> Create(User user);
    Task<User> GetById(Guid id);
    Task<User> GetByEmail(string email);
    Task<bool> Exists(User user);
    Task Update(User user);
    Task Delete(Guid id);
    // ... more methods
}
```

### 3.2 Connector Implementation
**File:** `BL/Connectors/UserConnector.cs`

```csharp
public class UserConnector : IUserConnector
{
    private readonly WeSignEntities _context;
    private readonly ILogger<UserConnector> _logger;

    public async Task<User> Create(User user)
    {
        // Convert domain model to DAO
        var userDao = new UserDAO(user);
        
        // Entity Framework operations
        _context.Users.Add(userDao);
        await _context.SaveChangesAsync();
        
        // Convert back to domain model
        return userDao.ToDomainModel();
    }

    public async Task<bool> Exists(User user)
    {
        return await _context.Users
            .AnyAsync(u => u.Email == user.Email);
    }
}
```

**Key Learning Points:**
- **Repository Pattern**: Connectors abstract data access
- **Domain/DAO Separation**: Domain models != Database entities
- **Entity Framework**: ORM for database operations
- **Conversion Layer**: Transform between domain models and DAOs

**Exercise 4:**
1. Find the UserConnector implementation
2. Trace how `Create()` method works
3. Notice the UserDAO conversion pattern
4. Find other Connector classes and see the pattern

---

## Phase 4: Domain Models - Business Objects
**Duration: Day 5**
**Goal: Understand the core business entities**

### 4.1 Domain Model
**File:** `Common/Models/User.cs`

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
    
    // Navigation properties
    public UserConfiguration UserConfiguration { get; set; }
    public ProgramUtilization ProgramUtilization { get; set; }
    
    // Collections
    public List<AdditionalGroupMapper> AdditionalGroupsMapper { get; set; }
    
    // Business methods (if any)
    public bool IsActive => Status == UserStatus.Active;
}
```

**Key Learning Points:**
- **Pure Business Objects**: No database or API concerns
- **Rich Models**: Can contain business logic methods
- **Navigation Properties**: Related entities
- **Value Objects**: Enums like UserType, UserStatus

### 4.2 Enums and Value Objects
**Files:** `Common/Enums/`

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

**Exercise 5:**
1. Open `Common/Models/User.cs`
2. Compare it with `CreateUserDTO` - what's different?
3. Look at the enums in `Common/Enums/`
4. Find other domain models and see the patterns

---

## Phase 5: Data Layer - Entity Framework
**Duration: Day 6-7**
**Goal: Understand database mapping and persistence**

### 5.1 Data Access Object (DAO)
**File:** `DAL/DAOs/Users/UserDAO.cs`

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
    
    // EF Navigation Properties
    public virtual ProgramUtilizationDAO ProgramUtilization { get; set; }
    public virtual UserConfigurationDAO UserConfiguration { get; set; }
    public virtual CompanyDAO Company { get; set; }
    public virtual ICollection<DocumentCollectionDAO> DocumentCollections { get; set; }

    // Conversion constructors
    public UserDAO() { }
    
    public UserDAO(User user)
    {
        Id = user.Id == Guid.Empty ? default : user.Id;
        CompanyId = user.CompanyId == Guid.Empty ? default : user.CompanyId;
        Name = user.Name;
        Email = user.Email;
        // ... map all properties
    }

    // Conversion method
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

**Key Learning Points:**
- **Entity Framework Entities**: Database-focused objects
- **Attributes**: `[Key]`, navigation properties marked `virtual`
- **Conversion Pattern**: Constructors and methods to/from domain models
- **Collections**: `ICollection<>` for one-to-many relationships

### 5.2 DbContext Configuration
**File:** `DAL/WeSignEntities.cs`

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
        ConfigureDocumentEntity(modelBuilder);
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
        });
    }
}
```

**Key Learning Points:**
- **DbContext**: Central point for database operations
- **DbSet Properties**: Each represents a table
- **Fluent API**: Configure entities, relationships, constraints
- **Code-First**: Database schema generated from code

### 5.3 Migrations
**Folder:** `DAL/Migrations/`

When you change DAOs, you create migrations:
```bash
dotnet ef migrations add AddUserLastLoginDate --project DAL --startup-project WeSign
```

**Exercise 6:**
1. Open `DAL/DAOs/Users/UserDAO.cs`
2. Find the constructor that takes a `User` parameter
3. Look at `DAL/WeSignEntities.cs` and find the `Users` DbSet
4. Examine a migration file in `DAL/Migrations/`

---

## Phase 6: Configuration and Dependency Injection
**Duration: Day 8**
**Goal: Understand how everything connects**

### 6.1 Service Registration
**File:** `WeSign/Startup.cs`

```csharp
public void ConfigureServices(IServiceCollection services)
{
    // Database
    services.AddDbContext<WeSignEntities>(options =>
        options.UseSqlServer(connectionString));

    // Business Logic
    services.AddScoped<IUserBl, UsersHandler>();
    services.AddScoped<IUserConnector, UserConnector>();
    
    // Infrastructure
    services.AddScoped<IEmailService, EmailService>();
    services.AddScoped<IPKBDF2Handler, PKBDF2Handler>();
    
    // Configuration
    services.Configure<JwtSettings>(Configuration.GetSection("JwtSettings"));
}
```

### 6.2 Request Pipeline
**File:** `WeSign/Startup.cs`

```csharp
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
    });
}
```

**Exercise 7:**
1. Open `WeSign/Startup.cs`
2. Find where `UsersHandler` is registered
3. Find where the database context is configured
4. Trace the middleware pipeline

---

## 🎯 Complete Flow Summary

**HTTP Request → Controller → Business Logic → Data Access → Database**

```
1. POST /v3/users with CreateUserDTO
   ↓
2. UsersController.SignUpAsync(CreateUserDTO input)
   ↓
3. Convert DTO to Domain Model: new User() { ... }
   ↓
4. _userBl.SignUp(user, input.SendActivationLink)
   ↓
5. UsersHandler.SignUp(User user, bool sendActivationLink)
   - Validates business rules
   - Applies security (password hashing)
   - Coordinates multiple operations
   ↓
6. _userConnector.Create(user)
   ↓
7. UserConnector.Create(User user)
   - Convert to DAO: new UserDAO(user)
   - _context.Users.Add(userDao)
   - _context.SaveChangesAsync()
   ↓
8. Entity Framework generates SQL:
   INSERT INTO Users (Id, Email, Name, ...) VALUES (...)
   ↓
9. SQL Server Database
```

## 🚀 Next Steps Roadmap

### Week 1: Master the Pattern
- Follow this exact flow for other operations (GetById, Update, Delete)
- Try other controllers (DocumentCollections, Templates)
- Practice reading the flow in both directions

### Week 2: Understand Variations
- Authentication flows
- File upload operations
- Background jobs (Hangfire)
- SignalR real-time features

### Week 3: Advanced Patterns
- Transaction handling
- Error handling strategies
- Performance optimization
- Testing each layer

### Week 4: Build Features
- Add a new API endpoint following this pattern
- Add new fields to existing entities
- Implement business rules

## 🛠 Debugging Checklist

When tracing issues, check each layer:

1. **Controller**: Are DTOs binding correctly?
2. **Handler**: Are business rules being applied?
3. **Connector**: Is data access working?
4. **DAO**: Are entity mappings correct?
5. **Database**: Is the SQL being generated correctly?

**Common Issues:**
- DTO → Domain model mapping missing fields
- Business logic not handling edge cases
- DAO conversion losing data
- Database constraints violated
- Dependency injection not configured

This roadmap gives you a systematic way to understand and work with the WeSign codebase, starting from what users see (APIs) and drilling down to where data lives (database).