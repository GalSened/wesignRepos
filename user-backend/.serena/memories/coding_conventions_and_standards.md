# WeSign - Coding Conventions and Standards

## General C# Conventions

### Naming Conventions
- **Classes**: PascalCase (e.g., `UsersController`, `WeSignEntities`)
- **Methods**: PascalCase (e.g., `SignUpAsync`, `GetUser`)
- **Properties**: PascalCase (e.g., `Configuration`, `Users`)
- **Fields (private)**: camelCase with underscore prefix (e.g., `_logger`, `_userBl`)
- **Parameters**: camelCase (e.g., `userId`, `configuration`)
- **Constants**: PascalCase or UPPER_CASE (e.g., `MaxUploadFileSize`)

### Project Naming Patterns
- **Main applications**: Descriptive names (e.g., `WeSign`, `WeSignManagement`)
- **Layer projects**: Abbreviated names (e.g., `BL`, `DAL`, `Common`)
- **Test projects**: Original name + `.Tests` suffix (e.g., `BL.Tests`)
- **Service projects**: Descriptive purpose (e.g., `SignatureServiceConnector`)

## File and Folder Organization

### Controller Organization
- **Area-based routing**: `/Areas/Api/Controllers/` and `/Areas/Ui/`
- **Resource-based naming**: `UsersController`, `DocumentsController`
- **Action methods**: HTTP verb prefix where applicable (`GetUser`, `PostDocument`)

### Handler Pattern
- **Handler classes**: `{Domain}Handler` (e.g., `UsersHandler`, `DocumentsHandler`)
- **Location**: `BL/Handlers/` directory
- **Interface contracts**: Corresponding interfaces in `Common/Interfaces/`

### Model Organization
- **Request/Response DTOs**: Organized by domain in `Models/` directories
- **Entity classes**: In DAL project with EF configuration
- **Validation models**: Separate validator classes using FluentValidation

## Architecture Conventions

### Dependency Injection
- **Constructor injection**: Primary pattern for dependencies
- **Interface-based dependencies**: Always use interfaces for service contracts
- **Service registration**: Organized by domain/purpose in `Startup.ConfigureServices`

### Async/Await Patterns
- **Async method naming**: `Async` suffix (e.g., `SignUpAsync`)
- **Consistent async usage**: Throughout the application stack
- **ConfigureAwait**: Used appropriately in library code

### Error Handling
- **Custom middleware**: `ErrorHandlingMiddleware` for global exception handling
- **Validation**: FluentValidation for input validation
- **Logging**: Comprehensive logging using Serilog

### Security Patterns
- **JWT Authentication**: Bearer token-based authentication
- **Input sanitization**: HTML sanitization middleware
- **Rate limiting**: API rate limiting implementation
- **CORS configuration**: Explicit CORS policy definitions

## Database Conventions

### Entity Framework Patterns
- **DbContext**: Single `WeSignEntities` context
- **Entity configuration**: Separate configuration methods per entity type
- **Migration naming**: Descriptive migration names
- **Relationship configuration**: Explicit foreign key and navigation property setup

### Entity Design
- **Primary keys**: Typically `Id` property (integer or GUID)
- **Audit fields**: Common audit properties (CreatedDate, ModifiedDate)
- **Foreign keys**: Clear naming convention (e.g., `UserId`, `GroupId`)
- **Navigation properties**: Virtual properties for lazy loading

## API Design Conventions

### REST API Patterns
- **Resource-based URLs**: `/api/users`, `/api/documents`
- **HTTP methods**: Proper verb usage (GET, POST, PUT, DELETE)
- **Status codes**: Appropriate HTTP status code responses
- **Versioning**: URL-based versioning (`/v3/`)

### Request/Response Patterns
- **DTO usage**: Separate DTOs for requests and responses
- **Model validation**: Comprehensive input validation
- **Error responses**: Consistent error response format
- **Pagination**: Standard pagination patterns where applicable

### Documentation Standards
- **Swagger/OpenAPI**: Comprehensive API documentation
- **XML comments**: Method and class documentation
- **Attribute annotations**: Swagger annotations for enhanced documentation

## Testing Conventions

### Test Organization
- **Test project structure**: Mirror source project structure
- **Test naming**: Descriptive test method names
- **Test categories**: Unit tests, integration tests separation

### Testing Patterns
- **Arrange-Act-Assert**: Standard test structure
- **Mock usage**: Dependency mocking for unit tests
- **Test data**: Proper test data setup and cleanup

## Configuration Management

### Settings Pattern
- **Strongly-typed configuration**: Configuration classes (e.g., `GeneralSettings`)
- **Environment-specific**: Separate appsettings files per environment
- **Secret management**: User secrets for development, secure storage for production

### Feature Management
- **Feature flags**: `Microsoft.FeatureManagement` for feature toggles
- **Environment-based features**: Different feature sets per environment

## Performance Considerations

### Caching Patterns
- **Memory caching**: For frequently accessed data
- **Response caching**: For appropriate API endpoints
- **Background processing**: Hangfire for heavy operations

### Database Optimization
- **Eager/lazy loading**: Appropriate loading strategies
- **Query optimization**: Efficient LINQ queries
- **Connection management**: Proper connection lifecycle management

## Security Standards

### Authentication & Authorization
- **JWT tokens**: Secure token-based authentication
- **Role-based access**: Appropriate authorization attributes
- **Token refresh**: Secure token refresh mechanisms

### Input Validation
- **Server-side validation**: Never rely solely on client validation
- **SQL injection prevention**: Parameterized queries through EF
- **XSS prevention**: HTML sanitization for user inputs

### Audit & Logging
- **Comprehensive logging**: Business operations and security events
- **PII protection**: Careful handling of personally identifiable information
- **Audit trails**: Complete audit logging for compliance