# WeSign - Architecture Patterns and Code Structure

## Overall Architecture Pattern
**N-Tier Layered Architecture** with clear separation of concerns:

### Layer Structure
1. **Presentation Layer** (`WeSign`) - ASP.NET Core Web API with area-based routing
2. **Business Logic Layer** (`BL`) - Business rules and operations
3. **Data Access Layer** (`DAL`) - Entity Framework Core and database operations
4. **Common Layer** (`Common`) - Shared contracts, models, and utilities
5. **Supporting Services** - Certificate handling, PDF processing, external integrations

## Project Organization

### Core Applications
- **`WeSign`** - Main Web API application
- **`WeSignManagement`** - Management portal
- **`WeSignSigner`** - Desktop signing client
- **`SmartCardDesktopClient`** - Smart card integration client

### Business Logic Projects
- **`BL`** - Core business logic handlers
- **`SignerBL`** - Signer-specific business logic
- **`ManagementBL`** - Management-specific business logic

### Infrastructure Projects
- **`DAL`** - Data Access Layer with Entity Framework
- **`Common`** - Shared interfaces, models, enums, extensions
- **`Certificate`** - Certificate management operations
- **`PdfHandler`** - PDF processing and manipulation

### External Service Integration
- **`SignatureServiceConnector`** - External signature service integration
- **`PdfExternalService`** - External PDF processing service
- **`PDFConvertorWrapper`** - PDF conversion utilities
- **`MongoIntegratorService`** - MongoDB history integration
- **`WSE-ADAuth`** - Active Directory authentication

### Supporting Components
- **`UserWcfService`** - SOAP service for legacy integration
- **`CustomUrlProtocolInstaller`** - Custom protocol handler
- **`WeSignSetup`** - Installation package

## Key Architectural Patterns

### MVC with Areas
- **API Area** (`/Areas/Api/Controllers/`) - REST API endpoints
- **UI Area** (`/Areas/Ui/`) - Web interface controllers
- Route pattern: `{area}/{controller}/{action}`

### Dependency Injection Pattern
- Constructor injection throughout the application
- Interface-based service registration
- Service lifetime management (Singleton, Scoped, Transient)

### Repository/Handler Pattern
- Business logic encapsulated in Handler classes
- Clear separation between controllers and business operations
- Each domain has dedicated handler (UsersHandler, DocumentsHandler, etc.)

### Interface Segregation
- Extensive use of interfaces in `Common/Interfaces`
- Domain-specific interface organization
- Clear contracts between layers

### Entity Framework Code-First
- `WeSignEntities` as DbContext
- Migration-based schema evolution
- Comprehensive entity relationships and constraints

## Communication Patterns

### Real-time Communication
- **SignalR Hub** (`SmartCardSigningHub`) for real-time updates
- WebSocket-based communication for signing operations

### Background Processing
- **Hangfire** for job scheduling and background tasks
- Memory storage for job persistence
- Recurring job management

### External Integration
- **Message Queue** (RabbitMQ) for service communication
- **HTTP clients** for external service calls
- **SOAP services** for legacy system integration