# WeSign - Data Model and Entity Structure

## Core Entity Framework Context
**`WeSignEntities`** - Main DbContext class with comprehensive entity management

## Primary Domain Entities

### User Management
- **`Users`** - Core user accounts with authentication details
- **`UserConfiguration`** - User-specific settings and preferences
- **`UserPasswordHistory`** - Password change audit trail
- **`UserTokens`** - JWT token management and refresh tokens
- **`UserOtpDetails`** - OTP authentication information

### Group & Organization Management
- **`Groups`** - User groups and organizational units
- **`Companies`** - Company/tenant information
- **`CompanyConfiguration`** - Company-specific settings
- **`Programs`** - Program/subscription management
- **`ProgramUtilization`** - Usage tracking and metrics
- **`ProgramUtilizationHistories`** - Historical usage data

### Document Management
- **`Documents`** - Individual document records
- **`DocumentCollections`** - Grouped document operations
- **`DocumentsSignatureFields`** - Signature field definitions on documents
- **`Templates`** - Document templates for reuse
- **`TemplatesSignatureFields`** - Signature fields on templates
- **`TemplatesTextFields`** - Text fields on templates

### Contact & Communication
- **`Contacts`** - External contact information
- **`ContactSeals`** - Contact signature seals/stamps
- **`ContactsGroup`** - Contact grouping
- **`ContactGroupMembers`** - Group membership relationships
- **`SingleLinkAdditionalResources`** - Additional resources for single-use links

### Signing & Authentication
- **`Signers`** - Document signers
- **`SignerTokensMapping`** - Signer authentication tokens
- **`Tablets`** - Tablet device registrations for signing
- **`CompanySigner1Details`** - Primary company signer information

### Active Directory Integration
- **`ActiveDirectoryConfigs`** - AD connection configuration
- **`ActiveDirectoryGroups`** - AD group mappings
- **`AdditionalGroupsMapper`** - Custom group mapping rules

### Audit & Logging
- **`Logs`** - Application audit logs
- **`SignerLogs`** - Signer-specific activity logs
- **`ManagementLogs`** - Management application logs

### Reporting
- **`UserPeriodicReports`** - User-level periodic reports
- **`ManagementPeriodicReports`** - Management-level reports
- **`ManagementPeriodicReportEmails`** - Report email distribution
- **`PeriodicReportFiles`** - Report file attachments

### System Configuration
- **`Configuration`** - Global system configuration settings

## Entity Relationships

### User-Centric Relationships
- Users → Groups (Many-to-One)
- Users → Companies (Many-to-One)
- Users → Programs (Many-to-One)
- Users → UserTokens (One-to-Many)
- Users → UserPasswordHistory (One-to-Many)

### Document Workflow Relationships
- DocumentCollections → Documents (One-to-Many)
- Documents → DocumentsSignatureFields (One-to-Many)
- Templates → TemplatesSignatureFields (One-to-Many)
- Templates → TemplatesTextFields (One-to-Many)

### Company Hierarchical Structure
- Companies → Groups (One-to-Many)
- Groups → Users (One-to-Many)
- Companies → Programs (One-to-Many)

## Database Configuration Features

### Custom Model Building
- **Entity-specific configuration methods** for each major domain
- **Relationship constraints** and foreign key definitions
- **Index optimization** for query performance
- **Data seeding** through `InitDbData()` method

### Migration Support
- **Code-First migrations** in `DAL/Migrations` directory
- **Version tracking** with comprehensive change logs
- **Database initialization** with default data

## Key Design Patterns

### Multi-Tenancy Support
- Company-based data isolation
- Group-level access control
- User-specific data segregation

### Audit Trail Implementation
- Comprehensive logging entities
- Historical data preservation
- Change tracking capabilities

### Hierarchical Data Organization
- Company → Group → User hierarchy
- Template → Document relationship patterns
- Configuration inheritance models