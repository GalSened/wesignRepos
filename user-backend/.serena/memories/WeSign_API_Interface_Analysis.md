# WeSign API Interface Analysis

## Main Service Interface: IUserSoapService
**File**: UserWcfService/IUserSoapService.cs

### API Endpoints by Module:

#### 1. Users Module (4 endpoints)
- SignUpAsync - User registration
- LoginAsync - User authentication  
- GetCurrentUserDetailsAsync - Get current user info
- UpdateUserAsync - Update user profile

#### 2. Contacts Module (4 endpoints)
- GetContactAsync - Get single contact by ID
- GetContactsAsync - Search/list contacts with filters (offset, limit, popular, recent, tablet mode)
- CreateContactAsync - Create new contact
- UpdateContactAsync - Update existing contact

#### 3. Templates Module (8 endpoints)
- GetTemplatesAsync - Get templates with filters (key, from, to, offset, limit, popular, recent)
- DownloadTemplateAsync - Download template file
- GetTemplatePagesCountAsync - Get page count for template
- GetTemplatesPagesInfoAsync - Get pages info with pagination
- CreateTemplateAsync - Create new template
- DuplicateTemplateAsync - Duplicate existing template
- UpdateTemplateAsync - Update template
- DeleteTemplateAsync - Delete template

#### 4. Document Collections Module (15 endpoints)
- GetDocumentCollectionsAsync - List collections with status filters (sent, viewed, signed, declined, sendingFailed, canceled)
- GetDocumentCollectionInfoAsync - Get collection details
- DownloadDocumentCollectionAsync - Download collection
- DownloadDocumentCollectionAttchmentAsync - Download attachments
- DownloadDocumentCollectionTraceAsync - Download trace/audit log
- GetDocumentCollectionPagesCountAsync - Get page count
- GetDocumentCollectionsPagesInfoAsync - Get pages info
- CreateDocumentCollectionAsync - Create new collection
- DeleteDocumentCollectionAsync - Delete collection
- ResendDocumentCollectionAsync - Resend to signer
- ShareDocumentCollectionAsync - Share collection
- ExportDocumentCollectionAsync - Export collections data
- ExportDocumentCollectionPdfFieldsAsync - Export PDF fields
- CancelDocumentCollectionAsync - Cancel collection
- ReplaceSignerAsync - Replace signer in collection

## Total: 31 API Endpoints identified in SOAP interface

## Next Steps:
1. Find REST/HTTP controllers
2. Map DTOs and request/response models
3. Identify business logic layer integration
4. Create comprehensive test coverage matrix