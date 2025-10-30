# WeSign Document Collections API Endpoints

## Base Route: v3/signatureDocumentCollections
**Controller:** DocumentCollectionsController.cs (Areas/Api)
**Authorization:** Required (Bearer token)

## Complete Endpoint List:

### 1. READ OPERATIONS (GET)
- **GET /** - Read all document collections with filtering and pagination
  - Parameters: key, sent, viewed, signed, declined, sendingFailed, canceled, userId, from, to, offset, limit, searchParameter
  - Returns: AllDocumentCollectionsResposneDTO with x-total-count header
  - Supports filtering by status, date range, user, and search parameters

- **GET /{id}** - Download document collection 
  - Single document → PDF file
  - Multiple documents → ZIP file
  - Returns: FileContentResult with x-file-name header

- **GET /{id}/ExtraInfo/json** - Download document extra info as JSON
  - Returns: DownloadExtraInfoDTO with base64 file data and metadata

- **GET /{id}/json** - Download document as JSON
  - Returns: DownloadDTO with base64 file data

- **GET /{id}/signer/{signerId}** - Download signer attachments
  - Returns: ZIP file with signer attachments

- **GET /info/{id}** - Get document collection info
  - Returns: DocumentCollectionResposneDTO

- **GET /{id}/senderLink/{signerId}** - Get sender signing link for live mode
  - Returns: SenderLiveSigningLinkDTO

- **GET /{id}/audit/{offset}** - Download audit trace document
  - Parameters: offset (timezone offset from UTC)
  - Returns: PDF audit trail file

- **GET /{id}/documents/{documentId}/pages** - Get pages count by document ID
  - Returns: DocumentCountResponseDTO

- **GET /{id}/documents/{documentId}/pages/{page}** - Get specific page info
  - Parameters: page number (starts from 1)
  - Returns: DocumentPageResponseDTO

- **GET /{id}/documents/{documentId}** - Get document pages info with range
  - Parameters: offset, limit, inViewMode
  - Returns: DocumentPagesRangeResponseDTO

- **GET /{id}/signers/{signerId}/method/{sendingMethod}** - Resend document to signer
  - Parameters: sendingMethod, shouldSend
  - Returns: SignerLink

- **GET /{id}/reactivate** - Reactivate document collection
  - Parameters: shouldSend
  - Returns: List<SignerLink>

- **GET /{id}/DocumentCollectionLinks** - Get signing links (feature gated)
  - Returns: SignerLink array

- **GET /export** - Export document collections to CSV
  - Parameters: sent, viewed, signed, declined, sendingFailed, canceled, language
  - Returns: CSV file

- **GET /exportDistribution** - Export distribution documents to CSV
  - Parameters: language
  - Returns: CSV file

- **GET /{id}/fields** - Export PDF fields to XML
  - Returns: XML file

- **GET /{id}/fields/json** - Export PDF fields as JSON data
  - Parameters: includeSigantures
  - Returns: PDFFields object

- **GET /{id}/fields/CsvXml** - Export PDF fields as CSV and XML in ZIP
  - Returns: ZIP file with both formats

### 2. CREATE OPERATIONS (POST)
- **POST /** - Create document collection from templates
  - Body: CreateDocumentCollectionDTO
  - Returns: CreateDocumentCollectionResposneDTO
  - Complex logic with signers, templates, notifications, authentication

- **POST /simple** - Create simple document (Lite version)
  - Body: CreateSimpleDocumentDTO
  - Returns: CreateDocumentCollectionResposneDTO
  - Simplified API for basic document creation

- **POST /downloadbatch** - Download multiple documents as batch
  - Body: DownloadBatchRequestDTO (array of IDs)
  - Returns: ZIP file or PDF file based on content

- **POST /share** - Share document collection to contact
  - Body: ShareDTO
  - Returns: OK status

### 3. UPDATE OPERATIONS (PUT)
- **PUT /deletebatch** - Delete multiple documents
  - Body: DeleteBatchRequestDTO (array of IDs)
  - Returns: OK status

- **PUT /{id}/cancel** - Cancel document collection
  - Returns: OK status

- **PUT /{id}/signer/{signerId}/replace** - Replace signer
  - Body: ReplaceSignerDTO
  - Returns: OK status

- **PUT /{id}/serversign** - Extra server signing
  - Returns: OK status

### 4. DELETE OPERATIONS
- **DELETE /{id}** - Delete document collection by ID
  - Returns: OK status

## Key DTOs and Models:
- CreateDocumentCollectionDTO: Complex document creation with signers, templates, modes
- CreateSimpleDocumentDTO: Simplified document creation
- DocumentCollectionResposneDTO: Standard response format
- AllDocumentCollectionsResposneDTO: List response with pagination
- DownloadBatchRequestDTO, DeleteBatchRequestDTO: Batch operations
- ShareDTO, ReplaceSignerDTO: Specific operation DTOs

## Important Features:
- File operations (PDF/ZIP downloads)
- Multi-signer workflows with ordering
- Authentication modes (OTP, IDP)
- Audit trails and field exports
- Batch operations for efficiency
- Feature flags for certain endpoints
- Comprehensive pagination and filtering
- Multi-format exports (CSV, XML, JSON)

## Security & Authentication:
- All endpoints require Bearer token authorization
- IP address tracking for document creation
- OTP and IDP authentication support
- Audit trail generation for compliance