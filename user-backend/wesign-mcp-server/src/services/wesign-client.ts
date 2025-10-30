/**
 * WeSign API Client
 * Handles all HTTP communication with the WeSign backend
 */

import axios, { AxiosInstance, AxiosError } from 'axios';
import FormData from 'form-data';
import {
  WeSignConfig,
  AuthTokens,
  LoginResponse,
  DocumentCollectionInfo,
  TemplateInfo,
  ContactInfo,
  ContactGroupInfo,
  UserInfo,
  SearchFilters
} from '../types.js';
import * as fs from 'fs';

export class WeSignClient {
  private axiosInstance: AxiosInstance;
  private tokens: AuthTokens | null = null;
  private config: WeSignConfig;

  constructor(config: WeSignConfig) {
    this.config = config;
    this.axiosInstance = axios.create({
      baseURL: config.apiUrl,
      timeout: 30000,
      headers: {
        'Content-Type': 'application/json',
      },
    });

    // Add request interceptor to inject auth token
    this.axiosInstance.interceptors.request.use(
      async (config) => {
        if (this.tokens) {
          // Check if token is expired
          if (Date.now() >= this.tokens.expiresAt) {
            await this.refreshToken();
          }
          config.headers.Authorization = `Bearer ${this.tokens.accessToken}`;
        }
        return config;
      },
      (error) => Promise.reject(error)
    );

    // Add response interceptor for error handling
    this.axiosInstance.interceptors.response.use(
      (response) => response,
      async (error: AxiosError) => {
        if (error.response?.status === 401) {
          // Token expired, try to refresh
          try {
            await this.refreshToken();
            // Retry the original request
            if (error.config) {
              return this.axiosInstance.request(error.config);
            }
          } catch (refreshError) {
            // Refresh failed, need to login again
            this.tokens = null;
            throw new Error('Authentication failed. Please login again.');
          }
        }
        throw error;
      }
    );
  }

  /**
   * Authentication Methods
   */

  async login(email?: string, password?: string): Promise<LoginResponse> {
    const loginEmail = email || this.config.email;
    const loginPassword = password || this.config.password;

    const response = await this.axiosInstance.post<LoginResponse>('/userapi/ui/Users/Login', {
      email: loginEmail,
      password: loginPassword,
      persistent: this.config.persistentSession,
    });

    this.tokens = {
      accessToken: response.data.accessToken,
      refreshToken: response.data.refreshToken,
      expiresAt: Date.now() + (response.data.expiresIn * 1000) - 60000, // Subtract 1 minute for safety
    };

    return response.data;
  }

  async logout(): Promise<void> {
    if (this.tokens) {
      await this.axiosInstance.post('/userapi/ui/Users/Logout');
      this.tokens = null;
    }
  }

  async refreshToken(): Promise<void> {
    if (!this.tokens?.refreshToken) {
      throw new Error('No refresh token available');
    }

    const response = await this.axiosInstance.post<LoginResponse>('/userapi/ui/Users/RefreshToken', {
      refreshToken: this.tokens.refreshToken,
    });

    this.tokens = {
      accessToken: response.data.accessToken,
      refreshToken: response.data.refreshToken,
      expiresAt: Date.now() + (response.data.expiresIn * 1000) - 60000,
    };
  }

  isAuthenticated(): boolean {
    return this.tokens !== null && Date.now() < this.tokens.expiresAt;
  }

  /**
   * Document Management Methods
   */

  async uploadDocument(filePath: string, customName?: string): Promise<{ documentId: string; documentCollectionId: string }> {
    const formData = new FormData();
    formData.append('file', fs.createReadStream(filePath));
    if (customName) {
      formData.append('name', customName);
    }

    const response = await this.axiosInstance.post('/userapi/ui/DocumentCollections/Upload', formData, {
      headers: {
        ...formData.getHeaders(),
      },
    });

    return response.data;
  }

  async createDocumentCollection(name: string, filePaths: string[]): Promise<{ documentCollectionId: string }> {
    const formData = new FormData();
    formData.append('name', name);

    for (const filePath of filePaths) {
      formData.append('files', fs.createReadStream(filePath));
    }

    const response = await this.axiosInstance.post('/userapi/ui/DocumentCollections/CreateCollection', formData, {
      headers: {
        ...formData.getHeaders(),
      },
    });

    return response.data;
  }

  async getDocumentInfo(documentCollectionId: string): Promise<DocumentCollectionInfo> {
    const response = await this.axiosInstance.get(`/userapi/ui/DocumentCollections/${documentCollectionId}`);
    return response.data;
  }

  async listDocuments(limit: number = 50, offset: number = 0): Promise<DocumentCollectionInfo[]> {
    const response = await this.axiosInstance.get('/userapi/ui/DocumentCollections', {
      params: { limit, offset },
    });
    return response.data;
  }

  async downloadDocument(documentCollectionId: string, documentId: string): Promise<Buffer> {
    const response = await this.axiosInstance.get(
      `/userapi/ui/DocumentCollections/${documentCollectionId}/Documents/${documentId}/Download`,
      { responseType: 'arraybuffer' }
    );
    return Buffer.from(response.data);
  }

  async searchDocuments(filters: SearchFilters): Promise<DocumentCollectionInfo[]> {
    const response = await this.axiosInstance.post('/userapi/ui/DocumentCollections/Search', filters);
    return response.data;
  }

  async cancelDocument(documentCollectionId: string): Promise<void> {
    await this.axiosInstance.post(`/userapi/ui/DocumentCollections/${documentCollectionId}/Cancel`);
  }

  async reactivateDocument(documentCollectionId: string): Promise<void> {
    await this.axiosInstance.post(`/userapi/ui/DocumentCollections/${documentCollectionId}/Reactivate`);
  }

  /**
   * Template Management Methods
   */

  async createTemplate(filePath: string, name: string, description?: string): Promise<{ templateId: string }> {
    const formData = new FormData();
    formData.append('file', fs.createReadStream(filePath));
    formData.append('name', name);
    if (description) {
      formData.append('description', description);
    }

    const response = await this.axiosInstance.post('/userapi/ui/Templates/Create', formData, {
      headers: {
        ...formData.getHeaders(),
      },
    });

    return response.data;
  }

  async listTemplates(limit: number = 50, offset: number = 0): Promise<TemplateInfo[]> {
    const response = await this.axiosInstance.get('/userapi/ui/Templates', {
      params: { limit, offset },
    });
    return response.data;
  }

  async getTemplate(templateId: string): Promise<TemplateInfo> {
    const response = await this.axiosInstance.get(`/userapi/ui/Templates/${templateId}`);
    return response.data;
  }

  async useTemplate(templateId: string, documentName: string): Promise<{ documentCollectionId: string }> {
    const response = await this.axiosInstance.post('/userapi/ui/Templates/Use', {
      templateId,
      documentName,
    });
    return response.data;
  }

  async updateTemplateFields(templateId: string, signatureFields: any[]): Promise<void> {
    await this.axiosInstance.put(`/userapi/ui/Templates/${templateId}/Fields`, {
      signatureFields,
    });
  }

  /**
   * Self-Signing Methods
   */

  async createSelfSign(filePath: string, documentName?: string, sourceTemplateId?: string): Promise<{ documentCollectionId: string; documentId: string }> {
    const formData = new FormData();
    formData.append('file', fs.createReadStream(filePath));
    if (documentName) {
      formData.append('name', documentName);
    }
    if (sourceTemplateId) {
      formData.append('sourceTemplateId', sourceTemplateId);
    }

    const response = await this.axiosInstance.post('/userapi/ui/SelfSign/Create', formData, {
      headers: {
        ...formData.getHeaders(),
      },
    });

    return response.data;
  }

  async addSignatureFields(documentCollectionId: string, documentId: string, fields: any[]): Promise<void> {
    await this.axiosInstance.post(`/userapi/ui/SelfSign/${documentCollectionId}/Documents/${documentId}/Fields`, {
      fields,
    });
  }

  async completeSigning(documentCollectionId: string, documentId: string): Promise<Buffer> {
    const response = await this.axiosInstance.post(
      `/userapi/ui/SelfSign/${documentCollectionId}/Documents/${documentId}/Complete`,
      {},
      { responseType: 'arraybuffer' }
    );
    return Buffer.from(response.data);
  }

  async saveDraft(documentCollectionId: string, documentId: string, fields?: any[]): Promise<void> {
    await this.axiosInstance.post(`/userapi/ui/SelfSign/${documentCollectionId}/Documents/${documentId}/SaveDraft`, {
      fields,
    });
  }

  async declineDocument(documentCollectionId: string, documentId: string, reason?: string): Promise<void> {
    await this.axiosInstance.post(`/userapi/ui/SelfSign/${documentCollectionId}/Documents/${documentId}/Decline`, {
      reason,
    });
  }

  /**
   * Distribution/Signing Workflow Methods
   */

  async sendForSignature(documentName: string, filePath: string, signers: any[], senderNote?: string, redirectUrl?: string): Promise<{ documentCollectionId: string }> {
    const formData = new FormData();
    formData.append('file', fs.createReadStream(filePath));
    formData.append('documentName', documentName);
    formData.append('signers', JSON.stringify(signers));
    if (senderNote) {
      formData.append('senderNote', senderNote);
    }
    if (redirectUrl) {
      formData.append('redirectUrl', redirectUrl);
    }

    const response = await this.axiosInstance.post('/userapi/ui/Distribution/Send', formData, {
      headers: {
        ...formData.getHeaders(),
      },
    });

    return response.data;
  }

  async sendSimpleDocument(templateId: string, documentName: string, signerName: string, signerMeans: string, redirectUrl?: string): Promise<{ documentCollectionId: string }> {
    const response = await this.axiosInstance.post('/userapi/ui/Distribution/SendSimple', {
      templateId,
      documentName,
      signerName,
      signerMeans,
      redirectUrl,
    });
    return response.data;
  }

  async resendToSigner(documentCollectionId: string, signerId: string, sendingMethod: number): Promise<void> {
    await this.axiosInstance.post(`/userapi/ui/Distribution/${documentCollectionId}/Signers/${signerId}/Resend`, {
      sendingMethod,
    });
  }

  async replaceSigner(documentCollectionId: string, signerId: string, newSigner: any): Promise<void> {
    await this.axiosInstance.post(`/userapi/ui/Distribution/${documentCollectionId}/Signers/${signerId}/Replace`, newSigner);
  }

  async shareDocument(documentCollectionId: string, emails: string[], message?: string): Promise<void> {
    await this.axiosInstance.post(`/userapi/ui/DocumentCollections/${documentCollectionId}/Share`, {
      emails,
      message,
    });
  }

  async getSignerLink(documentCollectionId: string, signerId: string): Promise<{ link: string }> {
    const response = await this.axiosInstance.get(`/userapi/ui/Distribution/${documentCollectionId}/Signers/${signerId}/Link`);
    return response.data;
  }

  /**
   * Contact Management Methods
   */

  async createContact(contact: Partial<ContactInfo>): Promise<{ contactId: string }> {
    const response = await this.axiosInstance.post('/userapi/ui/Contacts/Create', contact);
    return response.data;
  }

  async createContactsBulk(contacts: Partial<ContactInfo>[]): Promise<{ contactIds: string[] }> {
    const response = await this.axiosInstance.post('/userapi/ui/Contacts/CreateBulk', { contacts });
    return response.data;
  }

  async listContacts(query?: string, groupId?: string, limit: number = 100, offset: number = 0): Promise<ContactInfo[]> {
    const response = await this.axiosInstance.get('/userapi/ui/Contacts', {
      params: { query, groupId, limit, offset },
    });
    return response.data;
  }

  async getContact(contactId: string): Promise<ContactInfo> {
    const response = await this.axiosInstance.get(`/userapi/ui/Contacts/${contactId}`);
    return response.data;
  }

  async updateContact(contactId: string, contact: Partial<ContactInfo>): Promise<void> {
    await this.axiosInstance.put(`/userapi/ui/Contacts/${contactId}`, contact);
  }

  async deleteContact(contactId: string): Promise<void> {
    await this.axiosInstance.delete(`/userapi/ui/Contacts/${contactId}`);
  }

  async deleteContactsBatch(contactIds: string[]): Promise<void> {
    await this.axiosInstance.post('/userapi/ui/Contacts/DeleteBatch', { contactIds });
  }

  /**
   * Contact Group Methods
   */

  async listContactGroups(limit: number = 100, offset: number = 0): Promise<ContactGroupInfo[]> {
    const response = await this.axiosInstance.get('/userapi/ui/Contacts/Groups', {
      params: { limit, offset },
    });
    return response.data;
  }

  async getContactGroup(groupId: string): Promise<ContactGroupInfo> {
    const response = await this.axiosInstance.get(`/userapi/ui/Contacts/Groups/${groupId}`);
    return response.data;
  }

  async createContactGroup(name: string, description?: string, contactIds?: string[]): Promise<{ groupId: string }> {
    const response = await this.axiosInstance.post('/userapi/ui/Contacts/Groups/Create', {
      name,
      description,
      contactIds,
    });
    return response.data;
  }

  async updateContactGroup(groupId: string, name: string, description?: string, contactIds?: string[]): Promise<void> {
    await this.axiosInstance.put(`/userapi/ui/Contacts/Groups/${groupId}`, {
      name,
      description,
      contactIds,
    });
  }

  async deleteContactGroup(groupId: string): Promise<void> {
    await this.axiosInstance.delete(`/userapi/ui/Contacts/Groups/${groupId}`);
  }

  /**
   * User Management Methods
   */

  async getUserInfo(): Promise<UserInfo> {
    const response = await this.axiosInstance.get('/userapi/ui/Users/Me');
    return response.data;
  }

  async updateUserInfo(name: string, email: string, phone?: string, language?: number): Promise<void> {
    await this.axiosInstance.put('/userapi/ui/Users/Me', {
      name,
      email,
      phone,
      language,
    });
  }

  /**
   * Utility Methods
   */

  async getSigningStatus(documentCollectionId: string): Promise<any> {
    const response = await this.axiosInstance.get(`/userapi/ui/DocumentCollections/${documentCollectionId}/Status`);
    return response.data;
  }
}
