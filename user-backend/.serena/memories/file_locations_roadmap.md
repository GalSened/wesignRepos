# WeSign File Locations - Developer Roadmap with Exact Paths

## 🎯 User Registration Flow - Exact File Locations

### Phase 1: Entry Point - Controller Layer

#### 📂 Main Controller File
**Path:** `C:\Users\gals\source\repos\user-backend\WeSign\Areas\Api\Controllers\UsersController.cs`
- **Method to find:** `SignUpAsync` (around line 40-60)
- **What to look for:** `[HttpPost]` attribute and `CreateUserDTO input` parameter

#### 📂 DTO Definition
**Path:** `C:\Users\gals\source\repos\user-backend\WeSign\Models\Users\CreateUserDTO.cs`
- **What to examine:** Property definitions and default values
- **Key insight:** Compare with domain model later

#### 📂 Alternative Controller Areas
**Path:** `C:\Users\gals\source\repos\user-backend\WeSign\Areas\Ui\Controllers\UsersController.cs`
- **Purpose:** UI-focused endpoints (same structure, different area)

### Phase 2: Business Logic Layer

#### 📂 Business Logic Handler (Main File)
**Path:** `C:\Users\gals\source\repos\user-backend\BL\Handlers\UsersHandler.cs`
- **Method to trace:** `SignUp` method (around line 108-168)
- **Key sections:**
  - Constructor with 25+ dependencies (top of file)
  - SignUp method business logic
  - Helper methods like `InitProgramUtilization`, `CreateGroup`

#### 📂 Business Logic Interface (Likely location)
**Path:** `C:\Users\gals\source\repos\user-backend\BL\Interfaces\IUserBl.cs` (if exists)
- **Alternative paths to check:**
  - `C:\Users\gals\source\repos\user-backend\BL\IUsersHandler.cs`
  - `C:\Users\gals\source\repos\user-backend\BL\Abstractions\IUserBl.cs`

### Phase 3: Data Access Connectors

#### 📂 User Connector Implementation
**Likely paths to check:**
1. `C:\Users\gals\source\repos\user-backend\BL\Connectors\UserConnector.cs`
2. `C:\Users\gals\source\repos\user-backend\BL\Connectors\Users\UserConnector.cs`
3. `C:\Users\gals\source\repos\user-backend\DAL\Connectors\UserConnector.cs`

#### 📂 User Connector Interface
**Likely paths:**
1. `C:\Users\gals\source\repos\user-backend\BL\Connectors\IUserConnector.cs`
2. `C:\Users\gals\source\repos\user-backend\BL\Interfaces\IUserConnector.cs`

### Phase 4: Domain Models

#### 📂 User Domain Model
**Path:** `C:\Users\gals\source\repos\user-backend\Common\Models\User.cs`
- **What to compare:** Properties vs CreateUserDTO vs UserDAO

#### 📂 Enums and Value Objects
**Base path:** `C:\Users\gals\source\repos\user-backend\Common\Enums\`
- **Files to check:**
  - `UserType.cs`
  - `UserStatus.cs`
  - `CreationSource.cs`
  - `Language.cs`

### Phase 5: Data Layer

#### 📂 User Data Access Object
**Path:** `C:\Users\gals\source\repos\user-backend\DAL\DAOs\Users\UserDAO.cs`
- **Key methods:**
  - Constructor taking `User` parameter (line ~48-70)
  - Properties matching database columns
  - Navigation properties (virtual)

#### 📂 Database Context
**Path:** `C:\Users\gals\source\repos\user-backend\DAL\WeSignEntities.cs`
- **What to find:**
  - `DbSet<UserDAO> Users` property
  - `ConfigureUserEntity` method
  - Other entity configurations

#### 📂 Entity Configurations (Alternative location)
**Possible path:** `C:\Users\gals\source\repos\user-backend\DAL\Configurations\UserConfiguration.cs`

#### 📂 Migrations
**Base path:** `C:\Users\gals\source\repos\user-backend\DAL\Migrations\`
- **Files:** Timestamped migration files
- **To examine:** Recent migrations involving User entity

### Phase 6: Configuration and Startup

#### 📂 Main Startup Configuration
**Path:** `C:\Users\gals\source\repos\user-backend\WeSign\Startup.cs`
- **Key methods:**
  - `ConfigureServices` (dependency injection)
  - `Configure` (middleware pipeline)

#### 📂 Program Entry Point (.NET 6+ style)
**Path:** `C:\Users\gals\source\repos\user-backend\WeSign\Program.cs`

#### 📂 Configuration Files
**Paths:**
- `C:\Users\gals\source\repos\user-backend\WeSign\appsettings.json`
- `C:\Users\gals\source\repos\user-backend\WeSign\appsettings.Development.json`

## 🔍 How to Navigate and Verify Files Exist

### Using Visual Studio
1. Open `WeSignV3.sln` from `C:\Users\gals\source\repos\user-backend\`
2. Use Solution Explorer to navigate to specific projects
3. Use Ctrl+T (Go to Type) to find classes by name
4. Use Ctrl+Shift+F to search across all files

### Using File Explorer
Base directory: `C:\Users\gals\source\repos\user-backend\`

### Key Project Folders
- `WeSign\` - Main API project
- `BL\` - Business Logic Layer
- `DAL\` - Data Access Layer  
- `Common\` - Shared models and utilities
- `WeSignManagement\` - Management portal
- `WeSignSigner\` - Desktop client
- `Certificate\` - Smart card integration
- `PdfHandler\` - PDF processing

## 🎯 Step-by-Step File Discovery Process

### Step 1: Start with the Controller
1. Open: `WeSign\Areas\Api\Controllers\UsersController.cs`
2. Find: `SignUpAsync` method
3. Note: The `IUserBl _userBl` dependency injection

### Step 2: Find the Business Logic
1. Search for: "class UsersHandler" or "IUserBl"
2. Likely in: `BL\Handlers\UsersHandler.cs`
3. Look for: `SignUp` method implementation

### Step 3: Trace the Data Access
1. In UsersHandler, find calls to `_userConnector`
2. Search for: "class UserConnector" or "IUserConnector"
3. Look for: `Create`, `Exists`, `Update` methods

### Step 4: Find the Domain Model
1. In any class, look for `new User()` or `User user`
2. Go to definition (F12 in Visual Studio)
3. Should lead to: `Common\Models\User.cs`

### Step 5: Find the DAO
1. In UserConnector, look for `new UserDAO()`
2. Go to definition
3. Should lead to: `DAL\DAOs\Users\UserDAO.cs`

### Step 6: Find the DbContext
1. In UserConnector, look for `_context` or `WeSignEntities`
2. Go to definition
3. Should lead to: `DAL\WeSignEntities.cs`

## 🚨 If Files Are Not Where Expected

### Alternative Search Strategy
1. **Use Solution Explorer filter**: Type partial class name
2. **Global search**: Ctrl+Shift+F for class names
3. **File search**: Ctrl+Shift+T for file names

### Common Variations in Organization
- Some projects use `Services` folder instead of `Handlers`
- Interfaces might be in `Abstractions` or `Contracts` folders
- DAOs might be directly in `DAL\Models` instead of `DAL\DAOs`

### If a File Doesn't Exist
- It might be implemented inline in another file
- Check for generic implementations
- Look in base classes or abstract classes

## 📋 File Discovery Checklist

**Phase 1 - Controller Layer:**
- [ ] Found `UsersController.cs` in WeSign/Areas/Api/Controllers/
- [ ] Located `SignUpAsync` method
- [ ] Found `CreateUserDTO.cs` in WeSign/Models/Users/

**Phase 2 - Business Layer:**
- [ ] Found `UsersHandler.cs` in BL/Handlers/
- [ ] Located `SignUp` method with business logic
- [ ] Identified all constructor dependencies

**Phase 3 - Data Access:**
- [ ] Found connector class (UserConnector or similar)
- [ ] Located interface definition
- [ ] Identified data access patterns

**Phase 4 - Domain Layer:**
- [ ] Found `User.cs` domain model in Common/Models/
- [ ] Located relevant enums in Common/Enums/

**Phase 5 - Data Layer:**
- [ ] Found `UserDAO.cs` in DAL/DAOs/Users/
- [ ] Located `WeSignEntities.cs` DbContext
- [ ] Found migration files in DAL/Migrations/

**Phase 6 - Configuration:**
- [ ] Found `Startup.cs` in WeSign/
- [ ] Located configuration files (appsettings.json)

This roadmap gives you exact starting points and search strategies to locate every component in the WeSign architecture!