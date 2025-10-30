# WeSign - Task Completion Workflow

## When a Development Task is Completed

### Code Quality Checks
1. **Build Verification**
   ```bash
   dotnet build WeSignV3.sln
   ```
   - Ensure no compilation errors
   - Verify all projects build successfully
   - Check for build warnings and address critical ones

2. **Code Formatting**
   ```bash
   dotnet format WeSignV3.sln
   ```
   - Apply consistent code formatting
   - Ensure adherence to coding standards

### Testing Requirements
1. **Unit Tests**
   ```bash
   # Run all test projects
   dotnet test BL.Tests/BL.Tests.csproj
   dotnet test Common.Tests/Common.Tests.csproj
   dotnet test PdfHandler.Tests/PdfHandler.Tests.csproj
   dotnet test SignerBL.Tests/SignerBL.Tests.csproj
   dotnet test ManagementBL.Tests/ManagementBL.Tests.csproj
   
   # Run all tests together
   dotnet test
   ```

2. **Integration Testing** (if applicable)
   - Test API endpoints with Swagger UI (`/swagger`)
   - Verify database operations work correctly
   - Test external service integrations

### Database Operations (if schema changed)
1. **Migration Management**
   ```bash
   # Add migration if database changes were made
   dotnet ef migrations add DescriptiveMigrationName --project DAL --startup-project WeSign
   
   # Update development database
   dotnet ef database update --project DAL --startup-project WeSign
   ```

2. **Migration Verification**
   - Verify migration scripts are correct
   - Test migration on a copy of production data (if available)
   - Ensure rollback scenarios are considered

### Documentation Updates
1. **Code Documentation**
   - Ensure XML documentation is complete for public APIs
   - Update README.md if functionality changes
   - Document any new configuration settings

2. **API Documentation**
   - Verify Swagger documentation reflects changes
   - Update API version if breaking changes were made
   - Test API documentation through Swagger UI

### Configuration Review
1. **Settings Verification**
   - Review `appsettings.json` for new configuration needs
   - Update User Secrets for development if needed
   - Document any new environment variables for production

2. **Feature Flags** (if applicable)
   - Update feature management configuration
   - Test feature toggle functionality

### Security Considerations
1. **Input Validation**
   - Ensure all new inputs are properly validated
   - Verify HTML sanitization is applied where needed
   - Check for potential SQL injection or XSS vulnerabilities

2. **Authentication & Authorization**
   - Verify JWT token handling is correct
   - Ensure proper authorization attributes are applied
   - Test role-based access control

### Performance Verification
1. **Background Jobs** (if applicable)
   - Test Hangfire job execution through `/jobs` dashboard
   - Verify job scheduling and recurring jobs work correctly

2. **Database Performance**
   - Review new queries for efficiency
   - Check if new indexes are needed
   - Verify Entity Framework query performance

### Pre-Commit Checklist
- [ ] All builds pass without errors
- [ ] All tests pass
- [ ] Code is properly formatted
- [ ] Database migrations are tested
- [ ] Documentation is updated
- [ ] Security considerations are addressed
- [ ] Performance impact is acceptable
- [ ] Configuration changes are documented

### Git Workflow
```bash
# Stage changes
git add .

# Commit with descriptive message
git commit -m "feat: Add user authentication improvements

- Enhanced JWT token validation
- Added password complexity requirements
- Updated user registration flow
- Added comprehensive unit tests"

# Push to feature branch
git push origin feature/user-auth-improvements
```

### Deployment Preparation (Production)
1. **Production Build**
   ```bash
   dotnet publish WeSign -c Release -o ./publish
   ```

2. **Environment-Specific Configuration**
   - Ensure production connection strings are configured
   - Verify external service endpoints
   - Check SSL/TLS certificate configuration

3. **Migration Planning**
   - Plan database migration timing
   - Prepare rollback procedures
   - Coordinate with DevOps/Infrastructure team

### Post-Deployment Verification
1. **Health Checks**
   - Verify application starts successfully
   - Check all database connections
   - Test critical user workflows

2. **Monitoring**
   - Review application logs for errors
   - Monitor performance metrics
   - Verify background jobs are running

## Emergency Procedures

### Rollback Process
1. **Application Rollback**
   - Deploy previous stable version
   - Verify application functionality

2. **Database Rollback** (if needed)
   ```bash
   # Rollback to specific migration
   dotnet ef database update PreviousMigrationName --project DAL --startup-project WeSign
   ```

### Critical Issue Response
1. **Immediate Actions**
   - Identify the scope of the issue
   - Stop problematic processes if necessary
   - Notify stakeholders

2. **Resolution Steps**
   - Apply hotfix if possible
   - Test hotfix thoroughly
   - Deploy with expedited process
   - Monitor closely post-deployment