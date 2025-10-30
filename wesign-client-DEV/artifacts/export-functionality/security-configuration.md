# Step I: Security Configuration - Export Functionality

## Overview
Comprehensive security implementation for Export functionality covering authentication, authorization, data protection, input validation, and compliance with security standards and regulations.

## 1. Authentication & Authorization Framework

### Role-Based Access Control (RBAC)
```typescript
// security/export-permissions.service.ts
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

import { UserRole, ExportPermission, ExportFormat } from '../models/security.models';
import { AuthService } from '../../shared/services/auth.service';

@Injectable({
  providedIn: 'root'
})
export class ExportPermissionsService {
  private readonly rolePermissions: Map<UserRole, ExportPermission[]> = new Map([
    [UserRole.ProductManager, [
      ExportPermission.EXPORT_ALL_DATA,
      ExportPermission.EXPORT_PERSONAL_DATA,
      ExportPermission.EXPORT_ANALYTICS,
      ExportPermission.EXPORT_TEMPLATES_MANAGE,
      ExportPermission.EXPORT_SCHEDULE,
      ExportPermission.EXPORT_EMAIL_DELIVERY,
      ExportPermission.EXPORT_BULK_OPERATIONS
    ]],
    [UserRole.Support, [
      ExportPermission.EXPORT_CUSTOMER_DATA,
      ExportPermission.EXPORT_SUPPORT_ANALYTICS,
      ExportPermission.EXPORT_EMAIL_DELIVERY
    ]],
    [UserRole.Operations, [
      ExportPermission.EXPORT_SYSTEM_METRICS,
      ExportPermission.EXPORT_OPERATIONAL_DATA,
      ExportPermission.EXPORT_TEMPLATES_USE
    ]],
    [UserRole.StandardUser, [
      ExportPermission.EXPORT_PERSONAL_DATA,
      ExportPermission.EXPORT_TEMPLATES_USE
    ]],
    [UserRole.ReadOnlyUser, [
      ExportPermission.EXPORT_VIEW_ONLY
    ]]
  ]);

  private readonly formatRestrictions: Map<UserRole, ExportFormat[]> = new Map([
    [UserRole.ProductManager, [ExportFormat.PDF, ExportFormat.Excel, ExportFormat.CSV, ExportFormat.JSON, ExportFormat.XML]],
    [UserRole.Support, [ExportFormat.PDF, ExportFormat.Excel, ExportFormat.CSV]],
    [UserRole.Operations, [ExportFormat.CSV, ExportFormat.JSON]],
    [UserRole.StandardUser, [ExportFormat.PDF, ExportFormat.CSV]],
    [UserRole.ReadOnlyUser, [ExportFormat.PDF]]
  ]);

  constructor(private authService: AuthService) {}

  hasExportPermission(permission: ExportPermission): Observable<boolean> {
    return this.authService.getCurrentUser().pipe(
      map(user => {
        if (!user || !user.roles) return false;

        return user.roles.some(role => {
          const permissions = this.rolePermissions.get(role);
          return permissions?.includes(permission) || false;
        });
      })
    );
  }

  canUseFormat(format: ExportFormat): Observable<boolean> {
    return this.authService.getCurrentUser().pipe(
      map(user => {
        if (!user || !user.roles) return false;

        return user.roles.some(role => {
          const allowedFormats = this.formatRestrictions.get(role);
          return allowedFormats?.includes(format) || false;
        });
      })
    );
  }

  getMaxExportSize(): Observable<number> {
    return this.authService.getCurrentUser().pipe(
      map(user => {
        if (!user || !user.roles) return 0;

        // Size limits in bytes based on role
        const sizeLimits: Record<UserRole, number> = {
          [UserRole.ProductManager]: 500 * 1024 * 1024, // 500MB
          [UserRole.Support]: 100 * 1024 * 1024, // 100MB
          [UserRole.Operations]: 50 * 1024 * 1024, // 50MB
          [UserRole.StandardUser]: 25 * 1024 * 1024, // 25MB
          [UserRole.ReadOnlyUser]: 10 * 1024 * 1024 // 10MB
        };

        return Math.max(...user.roles.map(role => sizeLimits[role] || 0));
      })
    );
  }

  getExportQuota(): Observable<{ daily: number; monthly: number }> {
    return this.authService.getCurrentUser().pipe(
      map(user => {
        if (!user || !user.roles) return { daily: 0, monthly: 0 };

        const quotas: Record<UserRole, { daily: number; monthly: number }> = {
          [UserRole.ProductManager]: { daily: 50, monthly: 1000 },
          [UserRole.Support]: { daily: 20, monthly: 400 },
          [UserRole.Operations]: { daily: 30, monthly: 600 },
          [UserRole.StandardUser]: { daily: 10, monthly: 200 },
          [UserRole.ReadOnlyUser]: { daily: 5, monthly: 100 }
        };

        const userQuotas = user.roles.map(role => quotas[role] || { daily: 0, monthly: 0 });
        return {
          daily: Math.max(...userQuotas.map(q => q.daily)),
          monthly: Math.max(...userQuotas.map(q => q.monthly))
        };
      })
    );
  }

  validateDataAccess(dataSource: string, filters: any): Observable<boolean> {
    return this.authService.getCurrentUser().pipe(
      map(user => {
        if (!user) return false;

        // Implement data access validation based on user permissions
        // This would integrate with your data access control system
        const accessControl = this.getDataAccessRules(user, dataSource);
        return this.validateFiltersAgainstAccessControl(filters, accessControl);
      })
    );
  }

  private getDataAccessRules(user: any, dataSource: string): any {
    // Implement your data access control logic
    // Return rules that define what data the user can access
    return {
      allowedTenants: user.tenantIds || [],
      allowedDepartments: user.departmentIds || [],
      restrictedFields: this.getRestrictedFields(user.roles),
      timeRestrictions: this.getTimeRestrictions(user.roles)
    };
  }

  private getRestrictedFields(roles: UserRole[]): string[] {
    const restrictedFieldsByRole: Record<UserRole, string[]> = {
      [UserRole.ProductManager]: [],
      [UserRole.Support]: ['salary', 'ssn', 'personal_notes'],
      [UserRole.Operations]: ['personal_data', 'customer_details'],
      [UserRole.StandardUser]: ['admin_fields', 'internal_notes', 'financial_data'],
      [UserRole.ReadOnlyUser]: ['editable_fields', 'sensitive_data']
    };

    return Array.from(new Set(
      roles.flatMap(role => restrictedFieldsByRole[role] || [])
    ));
  }

  private getTimeRestrictions(roles: UserRole[]): { maxDays: number } {
    const timeRestrictions: Record<UserRole, number> = {
      [UserRole.ProductManager]: 1095, // 3 years
      [UserRole.Support]: 365, // 1 year
      [UserRole.Operations]: 180, // 6 months
      [UserRole.StandardUser]: 90, // 3 months
      [UserRole.ReadOnlyUser]: 30 // 1 month
    };

    return {
      maxDays: Math.max(...roles.map(role => timeRestrictions[role] || 0))
    };
  }

  private validateFiltersAgainstAccessControl(filters: any, accessControl: any): boolean {
    // Implement filter validation logic
    // Ensure user cannot access data they don't have permissions for
    return true; // Simplified for demo
  }
}
```

### JWT Token Validation
```typescript
// security/export-auth.guard.ts
import { Injectable } from '@angular/core';
import { CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot, Router } from '@angular/router';
import { Observable } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { of } from 'rxjs';

import { AuthService } from '../../shared/services/auth.service';
import { ExportPermissionsService } from './export-permissions.service';
import { ExportPermission } from '../models/security.models';

@Injectable({
  providedIn: 'root'
})
export class ExportAuthGuard implements CanActivate {
  constructor(
    private authService: AuthService,
    private exportPermissions: ExportPermissionsService,
    private router: Router
  ) {}

  canActivate(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot
  ): Observable<boolean> {
    const requiredPermission = route.data['requiredPermission'] as ExportPermission;

    return this.authService.validateToken().pipe(
      map(isValid => {
        if (!isValid) {
          this.router.navigate(['/login'], {
            queryParams: { returnUrl: state.url }
          });
          return false;
        }
        return true;
      }),
      map(isAuthenticated => {
        if (!isAuthenticated) return false;

        if (requiredPermission) {
          return this.exportPermissions.hasExportPermission(requiredPermission);
        }
        return true;
      }),
      catchError(error => {
        console.error('Authentication error:', error);
        this.router.navigate(['/unauthorized']);
        return of(false);
      })
    );
  }
}
```

## 2. Data Protection & Encryption

### Data Sanitization Service
```typescript
// security/data-sanitization.service.ts
import { Injectable } from '@angular/core';
import DOMPurify from 'dompurify';

@Injectable({
  providedIn: 'root'
})
export class DataSanitizationService {
  private readonly sensitiveFieldPatterns = [
    /credit[_-]?card/i,
    /ssn|social[_-]?security/i,
    /password|pwd/i,
    /api[_-]?key|token/i,
    /phone|mobile/i,
    /email/i,
    /address/i,
    /birth[_-]?date|dob/i
  ];

  private readonly dataClassification = {
    public: ['id', 'name', 'status', 'created_date', 'category'],
    internal: ['department', 'project_code', 'internal_notes'],
    confidential: ['salary', 'performance_rating', 'personal_notes'],
    restricted: ['ssn', 'credit_card', 'password', 'api_key']
  };

  sanitizeExportData(data: any[], userRoles: string[]): any[] {
    return data.map(record => this.sanitizeRecord(record, userRoles));
  }

  private sanitizeRecord(record: any, userRoles: string[]): any {
    const sanitizedRecord: any = {};

    for (const [key, value] of Object.entries(record)) {
      if (this.canAccessField(key, userRoles)) {
        sanitizedRecord[key] = this.sanitizeFieldValue(key, value, userRoles);
      }
    }

    return sanitizedRecord;
  }

  private canAccessField(fieldName: string, userRoles: string[]): boolean {
    const classification = this.getFieldClassification(fieldName);

    switch (classification) {
      case 'public':
        return true;
      case 'internal':
        return userRoles.some(role =>
          ['ProductManager', 'Support', 'Operations'].includes(role)
        );
      case 'confidential':
        return userRoles.some(role =>
          ['ProductManager', 'Support'].includes(role)
        );
      case 'restricted':
        return userRoles.includes('ProductManager');
      default:
        return false;
    }
  }

  private getFieldClassification(fieldName: string): string {
    for (const [classification, fields] of Object.entries(this.dataClassification)) {
      if (fields.includes(fieldName.toLowerCase()) ||
          this.matchesSensitivePattern(fieldName)) {
        return classification;
      }
    }
    return 'public';
  }

  private matchesSensitivePattern(fieldName: string): boolean {
    return this.sensitiveFieldPatterns.some(pattern => pattern.test(fieldName));
  }

  private sanitizeFieldValue(fieldName: string, value: any, userRoles: string[]): any {
    if (value === null || value === undefined) {
      return value;
    }

    // Apply field-specific sanitization
    if (this.isSensitiveField(fieldName)) {
      return this.maskSensitiveData(fieldName, value, userRoles);
    }

    // Sanitize string values for XSS prevention
    if (typeof value === 'string') {
      return DOMPurify.sanitize(value, { ALLOWED_TAGS: [] });
    }

    return value;
  }

  private isSensitiveField(fieldName: string): boolean {
    return this.matchesSensitivePattern(fieldName) ||
           this.getFieldClassification(fieldName) === 'restricted';
  }

  private maskSensitiveData(fieldName: string, value: any, userRoles: string[]): any {
    if (userRoles.includes('ProductManager')) {
      return value; // Full access for ProductManager
    }

    const stringValue = String(value);

    // Apply different masking strategies based on field type
    if (/email/i.test(fieldName)) {
      return this.maskEmail(stringValue);
    }

    if (/phone|mobile/i.test(fieldName)) {
      return this.maskPhone(stringValue);
    }

    if (/credit[_-]?card/i.test(fieldName)) {
      return this.maskCreditCard(stringValue);
    }

    if (/ssn|social[_-]?security/i.test(fieldName)) {
      return this.maskSSN(stringValue);
    }

    // Default masking: show first and last 2 characters
    if (stringValue.length > 4) {
      return stringValue.substring(0, 2) + '*'.repeat(stringValue.length - 4) +
             stringValue.substring(stringValue.length - 2);
    }

    return '*'.repeat(stringValue.length);
  }

  private maskEmail(email: string): string {
    const [localPart, domain] = email.split('@');
    if (!domain) return '***@***.***';

    const maskedLocal = localPart.length > 2
      ? localPart[0] + '*'.repeat(localPart.length - 2) + localPart[localPart.length - 1]
      : '***';

    return `${maskedLocal}@${domain}`;
  }

  private maskPhone(phone: string): string {
    const cleanPhone = phone.replace(/\D/g, '');
    if (cleanPhone.length >= 10) {
      return cleanPhone.substring(0, 3) + '-***-' + cleanPhone.substring(cleanPhone.length - 4);
    }
    return '***-***-****';
  }

  private maskCreditCard(cardNumber: string): string {
    const cleanCard = cardNumber.replace(/\D/g, '');
    if (cleanCard.length >= 12) {
      return '**** **** **** ' + cleanCard.substring(cleanCard.length - 4);
    }
    return '**** **** **** ****';
  }

  private maskSSN(ssn: string): string {
    const cleanSSN = ssn.replace(/\D/g, '');
    if (cleanSSN.length === 9) {
      return '***-**-' + cleanSSN.substring(5);
    }
    return '***-**-****';
  }

  // Content Security Policy validation
  validateExportContent(content: string): boolean {
    // Check for potential script injection
    const scriptPattern = /<script\b[^<]*(?:(?!<\/script>)<[^<]*)*<\/script>/gi;
    const eventPattern = /on\w+\s*=/gi;
    const jsPattern = /javascript:/gi;

    return !scriptPattern.test(content) &&
           !eventPattern.test(content) &&
           !jsPattern.test(content);
  }

  // File content validation
  validateFileContent(file: Blob, expectedType: string): Promise<boolean> {
    return new Promise((resolve) => {
      const reader = new FileReader();

      reader.onload = (event) => {
        const content = event.target?.result as string;

        // Basic file type validation
        if (!this.validateFileSignature(content, expectedType)) {
          resolve(false);
          return;
        }

        // Content validation
        resolve(this.validateExportContent(content));
      };

      reader.onerror = () => resolve(false);
      reader.readAsText(file.slice(0, 1024)); // Read first 1KB for validation
    });
  }

  private validateFileSignature(content: string, expectedType: string): boolean {
    const signatures: Record<string, string[]> = {
      'pdf': ['%PDF'],
      'csv': [',', ';', '\t'],
      'json': ['{', '['],
      'xml': ['<?xml', '<'],
      'excel': ['PK'] // Excel files are ZIP-based
    };

    const expectedSignatures = signatures[expectedType.toLowerCase()];
    if (!expectedSignatures) return true;

    return expectedSignatures.some(sig => content.startsWith(sig));
  }
}
```

## 3. Input Validation & XSS Prevention

### Export Configuration Validator
```typescript
// security/export-validator.service.ts
import { Injectable } from '@angular/core';
import { FormControl, FormGroup, ValidationErrors, ValidatorFn } from '@angular/forms';
import { Observable, of } from 'rxjs';
import { map } from 'rxjs/operators';

import { ExportConfig } from '../models/export.models';

@Injectable({
  providedIn: 'root'
})
export class ExportValidatorService {

  // Custom validators for export configuration
  static exportConfigValidator(): ValidatorFn {
    return (control: FormGroup): ValidationErrors | null => {
      const config = control.value as ExportConfig;
      const errors: ValidationErrors = {};

      // Validate date range
      if (config.dateRange) {
        const startDate = new Date(config.dateRange.startDate);
        const endDate = new Date(config.dateRange.endDate);
        const maxDays = 1095; // 3 years max

        if (startDate > endDate) {
          errors['invalidDateRange'] = 'Start date must be before end date';
        }

        const daysDiff = (endDate.getTime() - startDate.getTime()) / (1000 * 3600 * 24);
        if (daysDiff > maxDays) {
          errors['dateRangeTooLarge'] = `Date range cannot exceed ${maxDays} days`;
        }
      }

      // Validate filters for SQL injection
      if (config.filters) {
        const filterValidation = this.validateFilters(config.filters);
        if (!filterValidation.isValid) {
          errors['invalidFilters'] = filterValidation.errors;
        }
      }

      // Validate format options
      if (config.formatOptions) {
        const formatValidation = this.validateFormatOptions(config.format, config.formatOptions);
        if (!formatValidation.isValid) {
          errors['invalidFormatOptions'] = formatValidation.errors;
        }
      }

      return Object.keys(errors).length > 0 ? errors : null;
    };
  }

  static emailValidator(): ValidatorFn {
    return (control: FormControl): ValidationErrors | null => {
      const emails = control.value as string[];
      if (!emails || emails.length === 0) return null;

      const emailPattern = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;
      const invalidEmails = emails.filter(email => !emailPattern.test(email.trim()));

      if (invalidEmails.length > 0) {
        return {
          invalidEmails: {
            message: `Invalid email addresses: ${invalidEmails.join(', ')}`,
            invalidAddresses: invalidEmails
          }
        };
      }

      // Check for suspicious patterns
      const suspiciousPatterns = [
        /javascript:/gi,
        /<script/gi,
        /onclick/gi,
        /onerror/gi
      ];

      const suspiciousEmails = emails.filter(email =>
        suspiciousPatterns.some(pattern => pattern.test(email))
      );

      if (suspiciousEmails.length > 0) {
        return {
          suspiciousContent: {
            message: 'Email addresses contain suspicious content',
            suspiciousAddresses: suspiciousEmails
          }
        };
      }

      return null;
    };
  }

  private static validateFilters(filters: any): { isValid: boolean; errors: string[] } {
    const errors: string[] = [];

    // SQL injection patterns
    const sqlInjectionPatterns = [
      /('|(\')|(--)|(;)|(\|)|(\*)|(%)|(\+)|(=)|(<)|(>)|(\{)|(\})|(\[)|(\])|(\\)|(\/)|(\?)|(:))/gi,
      /(union|select|insert|update|delete|drop|create|alter|exec|execute)\s/gi,
      /(\w+\s*(=|!=|<>|<|>)\s*\w+\s*(or|and)\s*\w+\s*(=|!=|<>|<|>))/gi
    ];

    const validateValue = (value: any, path: string = '') => {
      if (typeof value === 'string') {
        sqlInjectionPatterns.forEach(pattern => {
          if (pattern.test(value)) {
            errors.push(`Potential SQL injection detected in ${path}: ${value}`);
          }
        });
      } else if (Array.isArray(value)) {
        value.forEach((item, index) => validateValue(item, `${path}[${index}]`));
      } else if (typeof value === 'object' && value !== null) {
        Object.entries(value).forEach(([key, val]) =>
          validateValue(val, path ? `${path}.${key}` : key)
        );
      }
    };

    validateValue(filters, 'filters');

    return {
      isValid: errors.length === 0,
      errors
    };
  }

  private static validateFormatOptions(format: string, options: any): { isValid: boolean; errors: string[] } {
    const errors: string[] = [];

    // Validate format-specific options
    switch (format.toLowerCase()) {
      case 'pdf':
        if (options.pageSize && !['A4', 'A3', 'Letter', 'Legal'].includes(options.pageSize)) {
          errors.push('Invalid PDF page size');
        }
        if (options.orientation && !['portrait', 'landscape'].includes(options.orientation)) {
          errors.push('Invalid PDF orientation');
        }
        break;

      case 'excel':
        if (options.worksheetName && options.worksheetName.length > 31) {
          errors.push('Excel worksheet name cannot exceed 31 characters');
        }
        if (options.worksheetName && /[\/\\\?\*\[\]]/.test(options.worksheetName)) {
          errors.push('Excel worksheet name contains invalid characters');
        }
        break;

      case 'csv':
        if (options.delimiter && ![',', ';', '\t', '|'].includes(options.delimiter)) {
          errors.push('Invalid CSV delimiter');
        }
        if (options.encoding && !['UTF-8', 'UTF-16', 'ASCII'].includes(options.encoding)) {
          errors.push('Invalid CSV encoding');
        }
        break;
    }

    // Check for script injection in string options
    const scriptPatterns = [
      /<script\b[^<]*(?:(?!<\/script>)<[^<]*)*<\/script>/gi,
      /javascript:/gi,
      /on\w+\s*=/gi
    ];

    const checkForScripts = (value: any, path: string = '') => {
      if (typeof value === 'string') {
        scriptPatterns.forEach(pattern => {
          if (pattern.test(value)) {
            errors.push(`Script injection detected in ${path}: ${value}`);
          }
        });
      } else if (Array.isArray(value)) {
        value.forEach((item, index) => checkForScripts(item, `${path}[${index}]`));
      } else if (typeof value === 'object' && value !== null) {
        Object.entries(value).forEach(([key, val]) =>
          checkForScripts(val, path ? `${path}.${key}` : key)
        );
      }
    };

    checkForScripts(options, 'formatOptions');

    return {
      isValid: errors.length === 0,
      errors
    };
  }

  // File upload validation
  validateUploadedFile(file: File): Observable<{ isValid: boolean; errors: string[] }> {
    return new Promise<{ isValid: boolean; errors: string[] }>((resolve) => {
      const errors: string[] = [];

      // File size validation (100MB max)
      const maxSize = 100 * 1024 * 1024;
      if (file.size > maxSize) {
        errors.push('File size exceeds maximum allowed size (100MB)');
      }

      // File type validation
      const allowedTypes = [
        'application/pdf',
        'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
        'application/vnd.ms-excel',
        'text/csv',
        'application/json',
        'application/xml',
        'text/xml'
      ];

      if (!allowedTypes.includes(file.type)) {
        errors.push('File type not allowed');
      }

      // File name validation
      const namePattern = /^[a-zA-Z0-9._-]+$/;
      if (!namePattern.test(file.name)) {
        errors.push('File name contains invalid characters');
      }

      // Check for suspicious file extensions
      const suspiciousExtensions = ['.exe', '.bat', '.cmd', '.scr', '.pif', '.com'];
      const hasExecutableExtension = suspiciousExtensions.some(ext =>
        file.name.toLowerCase().endsWith(ext)
      );

      if (hasExecutableExtension) {
        errors.push('Executable files are not allowed');
      }

      resolve({
        isValid: errors.length === 0,
        errors
      });
    }).then(result => of(result));
  }

  // Template validation
  validateExportTemplate(template: any): { isValid: boolean; errors: string[] } {
    const errors: string[] = [];

    // Template name validation
    if (!template.name || template.name.trim().length === 0) {
      errors.push('Template name is required');
    }

    if (template.name && template.name.length > 100) {
      errors.push('Template name cannot exceed 100 characters');
    }

    // Template description validation
    if (template.description && template.description.length > 500) {
      errors.push('Template description cannot exceed 500 characters');
    }

    // Validate template configuration
    if (template.config) {
      const configValidation = this.validateFilters(template.config.filters || {});
      if (!configValidation.isValid) {
        errors.push(...configValidation.errors);
      }

      const formatValidation = this.validateFormatOptions(
        template.config.format,
        template.config.formatOptions || {}
      );
      if (!formatValidation.isValid) {
        errors.push(...formatValidation.errors);
      }
    }

    return {
      isValid: errors.length === 0,
      errors
    };
  }
}
```

## 4. GDPR & Data Privacy Compliance

### GDPR Compliance Service
```typescript
// security/gdpr-compliance.service.ts
import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { map } from 'rxjs/operators';

import { GDPRConsent, DataProcessingPurpose, DataSubject } from '../models/gdpr.models';
import { AuthService } from '../../shared/services/auth.service';

@Injectable({
  providedIn: 'root'
})
export class GDPRComplianceService {
  private readonly dataProcessingPurposes: DataProcessingPurpose[] = [
    {
      id: 'export-analytics',
      name: 'Analytics Data Export',
      description: 'Export analytics data for business intelligence and reporting purposes',
      legalBasis: 'legitimate-interest',
      dataCategories: ['analytics', 'usage-statistics'],
      retentionPeriod: '3-years'
    },
    {
      id: 'export-personal',
      name: 'Personal Data Export',
      description: 'Export personal data for data portability requests',
      legalBasis: 'consent',
      dataCategories: ['personal-data', 'contact-information'],
      retentionPeriod: '7-years'
    },
    {
      id: 'export-compliance',
      name: 'Compliance Reporting',
      description: 'Export data for regulatory compliance and audit purposes',
      legalBasis: 'legal-obligation',
      dataCategories: ['audit-logs', 'compliance-data'],
      retentionPeriod: '10-years'
    }
  ];

  constructor(private authService: AuthService) {}

  checkGDPRConsent(purposeId: string): Observable<{ hasConsent: boolean; consent?: GDPRConsent }> {
    return this.authService.getCurrentUser().pipe(
      map(user => {
        if (!user) return { hasConsent: false };

        // Check if user has valid consent for the purpose
        const consent = user.gdprConsents?.find(c =>
          c.purposeId === purposeId &&
          c.isActive &&
          new Date(c.expiresAt) > new Date()
        );

        return {
          hasConsent: !!consent,
          consent
        };
      })
    );
  }

  requestGDPRConsent(purposeId: string): Observable<GDPRConsent> {
    const purpose = this.dataProcessingPurposes.find(p => p.id === purposeId);
    if (!purpose) {
      throw new Error(`Unknown data processing purpose: ${purposeId}`);
    }

    // In real implementation, this would show a consent dialog
    // and save the consent to the backend
    const consent: GDPRConsent = {
      id: `consent-${Date.now()}`,
      userId: 'current-user-id',
      purposeId,
      purpose,
      grantedAt: new Date(),
      expiresAt: new Date(Date.now() + 365 * 24 * 60 * 60 * 1000), // 1 year
      isActive: true,
      ipAddress: '192.168.1.1', // Would be captured from request
      userAgent: navigator.userAgent
    };

    return of(consent);
  }

  validateDataExportCompliance(exportConfig: any): Observable<{ isCompliant: boolean; issues: string[] }> {
    const issues: string[] = [];

    // Check if export includes personal data
    if (this.containsPersonalData(exportConfig)) {
      // Verify consent for personal data processing
      const purposeId = this.determinePurpose(exportConfig);

      return this.checkGDPRConsent(purposeId).pipe(
        map(({ hasConsent }) => {
          if (!hasConsent) {
            issues.push('GDPR consent required for personal data export');
          }

          // Check data minimization principle
          if (!this.validateDataMinimization(exportConfig)) {
            issues.push('Export violates data minimization principle');
          }

          // Check retention compliance
          if (!this.validateRetentionCompliance(exportConfig)) {
            issues.push('Export includes data beyond retention period');
          }

          return {
            isCompliant: issues.length === 0,
            issues
          };
        })
      );
    }

    return of({ isCompliant: true, issues: [] });
  }

  generateDataProcessingRecord(exportConfig: any): any {
    return {
      timestamp: new Date().toISOString(),
      userId: 'current-user-id',
      purpose: this.determinePurpose(exportConfig),
      dataCategories: this.identifyDataCategories(exportConfig),
      legalBasis: this.determineLegalBasis(exportConfig),
      dataSubjects: this.identifyDataSubjects(exportConfig),
      retentionPeriod: this.getRetentionPeriod(exportConfig),
      recipients: this.getDataRecipients(exportConfig),
      transferMethod: exportConfig.deliveryOptions?.method || 'download'
    };
  }

  generatePrivacyImpactAssessment(exportConfig: any): any {
    const risks = this.assessPrivacyRisks(exportConfig);

    return {
      id: `pia-${Date.now()}`,
      timestamp: new Date().toISOString(),
      exportConfig,
      risks,
      mitigation: this.generateMitigationMeasures(risks),
      approval: this.requiresApproval(risks),
      reviewer: this.getRequiredReviewer(risks)
    };
  }

  private containsPersonalData(exportConfig: any): boolean {
    const personalDataFields = [
      'email', 'phone', 'address', 'name', 'birth_date',
      'personal_notes', 'contact_info', 'user_profile'
    ];

    return exportConfig.columns?.some((column: string) =>
      personalDataFields.some(field =>
        column.toLowerCase().includes(field.toLowerCase())
      )
    ) || false;
  }

  private determinePurpose(exportConfig: any): string {
    // Logic to determine the purpose based on export configuration
    if (exportConfig.dataSource?.includes('analytics')) {
      return 'export-analytics';
    }
    if (this.containsPersonalData(exportConfig)) {
      return 'export-personal';
    }
    return 'export-compliance';
  }

  private validateDataMinimization(exportConfig: any): boolean {
    // Check if only necessary data is being exported
    // This would involve business logic specific to your application
    return exportConfig.columns?.length <= 20; // Simplified example
  }

  private validateRetentionCompliance(exportConfig: any): boolean {
    // Check if data being exported is within retention period
    const maxRetentionDays = 2555; // 7 years
    const startDate = new Date(exportConfig.dateRange?.startDate);
    const daysSinceStart = (Date.now() - startDate.getTime()) / (1000 * 60 * 60 * 24);

    return daysSinceStart <= maxRetentionDays;
  }

  private identifyDataCategories(exportConfig: any): string[] {
    const categories: string[] = [];

    if (this.containsPersonalData(exportConfig)) {
      categories.push('personal-data');
    }
    if (exportConfig.dataSource?.includes('analytics')) {
      categories.push('analytics');
    }
    if (exportConfig.columns?.some((col: string) => col.includes('audit'))) {
      categories.push('audit-logs');
    }

    return categories;
  }

  private determineLegalBasis(exportConfig: any): string {
    if (this.containsPersonalData(exportConfig)) {
      return 'consent';
    }
    if (exportConfig.purpose === 'compliance') {
      return 'legal-obligation';
    }
    return 'legitimate-interest';
  }

  private identifyDataSubjects(exportConfig: any): string[] {
    // Identify categories of data subjects affected
    return ['customers', 'employees', 'users']; // Simplified
  }

  private getRetentionPeriod(exportConfig: any): string {
    const purpose = this.dataProcessingPurposes.find(p =>
      p.id === this.determinePurpose(exportConfig)
    );
    return purpose?.retentionPeriod || '1-year';
  }

  private getDataRecipients(exportConfig: any): string[] {
    const recipients: string[] = ['internal-user'];

    if (exportConfig.deliveryOptions?.method === 'email') {
      recipients.push(...(exportConfig.deliveryOptions.emailRecipients || []));
    }

    return recipients;
  }

  private assessPrivacyRisks(exportConfig: any): any[] {
    const risks: any[] = [];

    if (this.containsPersonalData(exportConfig)) {
      risks.push({
        type: 'personal-data-exposure',
        severity: 'high',
        description: 'Export contains personal data that could be exposed'
      });
    }

    if (exportConfig.deliveryOptions?.method === 'email') {
      risks.push({
        type: 'email-transmission-risk',
        severity: 'medium',
        description: 'Data transmitted via email may be intercepted'
      });
    }

    if (exportConfig.estimatedRecords > 10000) {
      risks.push({
        type: 'large-dataset-risk',
        severity: 'medium',
        description: 'Large dataset increases exposure risk'
      });
    }

    return risks;
  }

  private generateMitigationMeasures(risks: any[]): string[] {
    const measures: string[] = [];

    if (risks.some(r => r.type === 'personal-data-exposure')) {
      measures.push('Data masking applied to sensitive fields');
      measures.push('Access logging enabled');
    }

    if (risks.some(r => r.type === 'email-transmission-risk')) {
      measures.push('Email encryption required');
      measures.push('Time-limited download links provided');
    }

    if (risks.some(r => r.type === 'large-dataset-risk')) {
      measures.push('Additional approval required');
      measures.push('Export chunking implemented');
    }

    return measures;
  }

  private requiresApproval(risks: any[]): boolean {
    return risks.some(r => r.severity === 'high') ||
           risks.length > 2;
  }

  private getRequiredReviewer(risks: any[]): string {
    if (risks.some(r => r.severity === 'high')) {
      return 'data-protection-officer';
    }
    if (risks.length > 1) {
      return 'privacy-manager';
    }
    return 'team-lead';
  }
}
```

## 5. Audit Logging & Monitoring

### Security Audit Service
```typescript
// security/security-audit.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import { AuditEvent, SecurityAlert, ComplianceReport } from '../models/audit.models';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class SecurityAuditService {
  private readonly auditApiUrl = `${environment.apiUrl}/api/audit`;

  constructor(private http: HttpClient) {}

  logExportEvent(event: Partial<AuditEvent>): Observable<void> {
    const auditEvent: AuditEvent = {
      id: this.generateEventId(),
      timestamp: new Date(),
      userId: event.userId || 'unknown',
      userRole: event.userRole || 'unknown',
      action: event.action || 'export',
      resource: event.resource || 'unknown',
      resourceId: event.resourceId,
      ipAddress: this.getClientIP(),
      userAgent: navigator.userAgent,
      outcome: event.outcome || 'unknown',
      metadata: {
        ...event.metadata,
        sessionId: this.getSessionId(),
        exportConfig: event.metadata?.exportConfig,
        dataSize: event.metadata?.dataSize,
        recordCount: event.metadata?.recordCount
      }
    };

    return this.http.post<void>(`${this.auditApiUrl}/events`, auditEvent);
  }

  logSecurityAlert(alert: Partial<SecurityAlert>): Observable<void> {
    const securityAlert: SecurityAlert = {
      id: this.generateAlertId(),
      timestamp: new Date(),
      severity: alert.severity || 'medium',
      category: alert.category || 'export-security',
      description: alert.description || 'Security event detected',
      userId: alert.userId,
      ipAddress: this.getClientIP(),
      metadata: alert.metadata || {}
    };

    return this.http.post<void>(`${this.auditApiUrl}/alerts`, securityAlert);
  }

  generateComplianceReport(period: 'daily' | 'weekly' | 'monthly'): Observable<ComplianceReport> {
    return this.http.get<ComplianceReport>(`${this.auditApiUrl}/compliance-report`, {
      params: { period }
    });
  }

  getAuditTrail(userId?: string, startDate?: Date, endDate?: Date): Observable<AuditEvent[]> {
    const params: any = {};

    if (userId) params.userId = userId;
    if (startDate) params.startDate = startDate.toISOString();
    if (endDate) params.endDate = endDate.toISOString();

    return this.http.get<AuditEvent[]>(`${this.auditApiUrl}/trail`, { params });
  }

  private generateEventId(): string {
    return `evt_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
  }

  private generateAlertId(): string {
    return `alt_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
  }

  private getClientIP(): string {
    // In a real application, this would be captured server-side
    return 'client-ip-masked';
  }

  private getSessionId(): string {
    return sessionStorage.getItem('sessionId') || 'unknown-session';
  }
}
```

## 6. Security Configuration Models

### Security Models
```typescript
// models/security.models.ts
export enum UserRole {
  ProductManager = 'ProductManager',
  Support = 'Support',
  Operations = 'Operations',
  StandardUser = 'StandardUser',
  ReadOnlyUser = 'ReadOnlyUser'
}

export enum ExportPermission {
  EXPORT_ALL_DATA = 'export:all-data',
  EXPORT_PERSONAL_DATA = 'export:personal-data',
  EXPORT_ANALYTICS = 'export:analytics',
  EXPORT_CUSTOMER_DATA = 'export:customer-data',
  EXPORT_SYSTEM_METRICS = 'export:system-metrics',
  EXPORT_OPERATIONAL_DATA = 'export:operational-data',
  EXPORT_TEMPLATES_MANAGE = 'export:templates:manage',
  EXPORT_TEMPLATES_USE = 'export:templates:use',
  EXPORT_SCHEDULE = 'export:schedule',
  EXPORT_EMAIL_DELIVERY = 'export:email-delivery',
  EXPORT_BULK_OPERATIONS = 'export:bulk-operations',
  EXPORT_VIEW_ONLY = 'export:view-only'
}

export interface SecurityContext {
  userId: string;
  roles: UserRole[];
  permissions: ExportPermission[];
  tenantId?: string;
  departmentId?: string;
  ipAddress: string;
  sessionId: string;
}

export interface DataAccessRule {
  field: string;
  accessLevel: 'full' | 'masked' | 'restricted' | 'denied';
  condition?: string;
  roles: UserRole[];
}

export interface ExportSecurityPolicy {
  id: string;
  name: string;
  description: string;
  rules: DataAccessRule[];
  maxExportSize: number;
  allowedFormats: string[];
  retentionDays: number;
  requiresApproval: boolean;
  approvers: string[];
}
```

This comprehensive security configuration ensures that the Export functionality meets enterprise security standards, complies with GDPR and other data protection regulations, and provides robust protection against common security threats while maintaining usability for authorized users.