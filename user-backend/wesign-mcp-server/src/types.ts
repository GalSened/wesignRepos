/**
 * WeSign MCP Server Type Definitions
 */

export interface WeSignConfig {
  apiUrl: string;
  email: string;
  password: string;
  persistentSession: boolean;
}

export interface AuthTokens {
  accessToken: string;
  refreshToken: string;
  expiresAt: number;
}

export interface LoginResponse {
  accessToken: string;
  refreshToken: string;
  expiresIn: number;
  userId: string;
  email: string;
}

export interface DocumentUploadRequest {
  file: Buffer;
  fileName: string;
  name?: string;
}

export interface DocumentCollectionRequest {
  name: string;
  files: DocumentUploadRequest[];
}

export interface DocumentInfo {
  id: string;
  name: string;
  status: number;
  createdDate: string;
  modifiedDate: string;
  documentCollectionId: string;
}

export interface DocumentCollectionInfo {
  id: string;
  name: string;
  status: number;
  documents: DocumentInfo[];
  signers?: SignerInfo[];
}

export interface SignerInfo {
  id: string;
  name: string;
  email?: string;
  phone?: string;
  status: number;
  sendingMethod: SendingMethod;
}

export enum SendingMethod {
  SMS = 1,
  Email = 2,
  WhatsApp = 3
}

export enum DocumentStatus {
  Draft = 0,
  Pending = 1,
  Completed = 2,
  Cancelled = 3
}

export interface SignatureField {
  x: number;
  y: number;
  width: number;
  height: number;
  pageNumber: number;
  fieldType: FieldType;
}

export enum FieldType {
  Signature = 1,
  Initial = 2,
  Text = 3,
  Date = 4,
  Checkbox = 5
}

export interface TemplateInfo {
  id: string;
  name: string;
  description?: string;
  createdDate: string;
  usageCount?: number;
}

export interface ContactInfo {
  id: string;
  firstName: string;
  lastName: string;
  email?: string;
  phone?: string;
  company?: string;
  notes?: string;
  groupId?: string;
}

export interface ContactGroupInfo {
  id: string;
  name: string;
  description?: string;
  contactIds?: string[];
}

export interface SigningRequest {
  documentName: string;
  filePath: string;
  signers: SignerRequest[];
  senderNote?: string;
  redirectUrl?: string;
}

export interface SignerRequest {
  contactName: string;
  contactMeans: string;
  sendingMethod: SendingMethod;
  senderNote?: string;
  linkExpirationInHours?: number;
}

export interface SearchFilters {
  query?: string;
  status?: DocumentStatus;
  fromDate?: string;
  toDate?: string;
  signerName?: string;
  signerEmail?: string;
  limit?: number;
  offset?: number;
}

export interface UserInfo {
  id: string;
  email: string;
  name: string;
  phone?: string;
  language: number;
  groupId?: string;
}
