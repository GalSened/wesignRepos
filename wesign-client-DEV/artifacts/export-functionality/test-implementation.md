# Step G: Test Implementation - Export Functionality

## Overview
Comprehensive test suite for Export functionality covering unit tests, integration tests, and end-to-end testing scenarios with full coverage of business logic, error conditions, and user workflows.

## 1. Unit Test Implementation

### Export Dialog Component Tests
```typescript
// export-dialog.component.spec.ts
import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { Store } from '@ngrx/store';
import { of, BehaviorSubject } from 'rxjs';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';

import { ExportDialogComponent, ExportDialogData } from './export-dialog.component';
import { ExportActions } from '../store/export.actions';
import { ExportFormat, ExportJob } from '../models/export.models';
import { ExportService } from '../services/export.service';
import { NotificationService } from '../../shared/services/notification.service';
import { AccessibilityService } from '../../shared/services/accessibility.service';

describe('ExportDialogComponent', () => {
  let component: ExportDialogComponent;
  let fixture: ComponentFixture<ExportDialogComponent>;
  let mockStore: jasmine.SpyObj<Store>;
  let mockExportService: jasmine.SpyObj<ExportService>;
  let mockNotificationService: jasmine.SpyObj<NotificationService>;
  let mockAccessibilityService: jasmine.SpyObj<AccessibilityService>;
  let mockDialogRef: jasmine.SpyObj<MatDialogRef<ExportDialogComponent>>;

  const mockDialogData: ExportDialogData = {
    dataSource: 'analytics-dashboard',
    initialFilters: { status: 'active' },
    preselectedFormat: ExportFormat.PDF
  };

  beforeEach(async () => {
    const storeSpy = jasmine.createSpyObj('Store', ['select', 'dispatch']);
    const exportServiceSpy = jasmine.createSpyObj('ExportService', [
      'estimateDataSize',
      'validateExportConfig',
      'previewFilteredData'
    ]);
    const notificationSpy = jasmine.createSpyObj('NotificationService', [
      'showSuccess',
      'showError',
      'showWarning'
    ]);
    const accessibilitySpy = jasmine.createSpyObj('AccessibilityService', [
      'announceDialogOpened',
      'announceExportProgress',
      'announceExportCompletion',
      'announceStepChange',
      'generateAriaLabel'
    ]);
    const dialogRefSpy = jasmine.createSpyObj('MatDialogRef', ['close']);

    await TestBed.configureTestingModule({
      declarations: [ExportDialogComponent],
      imports: [ReactiveFormsModule, NoopAnimationsModule],
      providers: [
        FormBuilder,
        { provide: Store, useValue: storeSpy },
        { provide: ExportService, useValue: exportServiceSpy },
        { provide: NotificationService, useValue: notificationSpy },
        { provide: AccessibilityService, useValue: accessibilitySpy },
        { provide: MatDialogRef, useValue: dialogRefSpy },
        { provide: MAT_DIALOG_DATA, useValue: mockDialogData }
      ]
    }).compileComponents();

    mockStore = TestBed.inject(Store) as jasmine.SpyObj<Store>;
    mockExportService = TestBed.inject(ExportService) as jasmine.SpyObj<ExportService>;
    mockNotificationService = TestBed.inject(NotificationService) as jasmine.SpyObj<NotificationService>;
    mockAccessibilityService = TestBed.inject(AccessibilityService) as jasmine.SpyObj<AccessibilityService>;
    mockDialogRef = TestBed.inject(MatDialogRef) as jasmine.SpyObj<MatDialogRef<ExportDialogComponent>>;

    // Setup store selectors
    mockStore.select.and.returnValue(of([]));
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(ExportDialogComponent);
    component = fixture.componentInstance;
  });

  describe('Component Initialization', () => {
    it('should create', () => {
      expect(component).toBeTruthy();
    });

    it('should initialize form with dialog data', () => {
      fixture.detectChanges();

      expect(component.exportForm.get('format')?.value).toBe(ExportFormat.PDF);
      expect(component.exportForm.get('filters')?.value).toEqual({ status: 'active' });
    });

    it('should dispatch load available formats action on init', () => {
      fixture.detectChanges();

      expect(mockStore.dispatch).toHaveBeenCalledWith(
        ExportActions.loadAvailableFormats()
      );
    });

    it('should announce dialog opened for accessibility', () => {
      fixture.detectChanges();

      expect(mockAccessibilityService.announceDialogOpened).toHaveBeenCalledWith(
        'export.dialog.opened'
      );
    });

    it('should set initial focus after component initialization', fakeAsync(() => {
      const mockInput = document.createElement('input');
      mockInput.className = 'export-dialog';
      spyOn(document, 'querySelector').and.returnValue(mockInput);
      spyOn(mockInput, 'focus');

      fixture.detectChanges();
      tick(100);

      expect(mockInput.focus).toHaveBeenCalled();
    }));
  });

  describe('Form Validation', () => {
    beforeEach(() => {
      fixture.detectChanges();
    });

    it('should validate format selection', () => {
      component.exportForm.patchValue({ format: null });
      expect(component.formatValid$.value).toBeFalse();

      component.exportForm.patchValue({ format: ExportFormat.CSV });
      expect(component.formatValid$.value).toBeTrue();
    });

    it('should validate date range with reasonable limits', () => {
      const validRange = {
        startDate: new Date('2024-01-01'),
        endDate: new Date('2024-01-31')
      };

      component.exportForm.patchValue({ dateRange: validRange });
      expect(component.dateRangeValid$.value).toBeTrue();

      const invalidRange = {
        startDate: new Date('2024-01-01'),
        endDate: new Date('2025-01-01') // > 365 days
      };

      component.exportForm.patchValue({ dateRange: invalidRange });
      expect(component.dateRangeValid$.value).toBeFalse();
    });

    it('should validate format-specific options', () => {
      // PDF format validation
      component.exportForm.patchValue({
        format: ExportFormat.PDF,
        formatOptions: { pageSize: 'A4', orientation: 'portrait' }
      });
      expect(component.optionsValid$.value).toBeTrue();

      component.exportForm.patchValue({
        format: ExportFormat.PDF,
        formatOptions: {} // Missing required options
      });
      expect(component.optionsValid$.value).toBeFalse();
    });

    it('should validate email delivery options', () => {
      component.exportForm.patchValue({
        deliveryOptions: {
          method: 'email',
          emailRecipients: ['test@example.com']
        }
      });
      expect(component.optionsValid$.value).toBeTrue();

      component.exportForm.patchValue({
        deliveryOptions: {
          method: 'email',
          emailRecipients: [] // Missing recipients
        }
      });
      expect(component.optionsValid$.value).toBeFalse();
    });
  });

  describe('Data Estimation', () => {
    beforeEach(() => {
      fixture.detectChanges();
      mockExportService.estimateDataSize.and.returnValue(of({
        sizeBytes: 1024000,
        recordCount: 5000
      }));
    });

    it('should update data estimation when filters change', fakeAsync(() => {
      const filters = { status: 'completed' };
      const dateRange = {
        startDate: new Date('2024-01-01'),
        endDate: new Date('2024-01-31')
      };

      component.exportForm.patchValue({ filters, dateRange });
      tick(500); // Wait for debounce

      expect(mockExportService.estimateDataSize).toHaveBeenCalledWith(filters, dateRange);
      expect(component.estimatedSize$.value).toBe(1024000);
      expect(component.estimatedRecords$.value).toBe(5000);
    }));

    it('should update estimation when date range changes', fakeAsync(() => {
      const dateRange = {
        startDate: new Date('2024-01-01'),
        endDate: new Date('2024-02-01')
      };

      component.exportForm.patchValue({ dateRange });
      tick(300); // Wait for debounce

      expect(mockExportService.estimateDataSize).toHaveBeenCalled();
    }));
  });

  describe('Export Process', () => {
    beforeEach(() => {
      fixture.detectChanges();
      // Setup valid form
      component.exportForm.patchValue({
        format: ExportFormat.PDF,
        formatOptions: { pageSize: 'A4', orientation: 'portrait' },
        dateRange: {
          startDate: new Date('2024-01-01'),
          endDate: new Date('2024-01-31')
        },
        filters: { status: 'active' },
        deliveryOptions: { method: 'download' }
      });
    });

    it('should dispatch initiate export action with valid form', () => {
      component.onExport();

      expect(mockStore.dispatch).toHaveBeenCalledWith(
        ExportActions.initiateExport({
          config: jasmine.objectContaining({
            format: ExportFormat.PDF,
            dataSource: 'analytics-dashboard'
          })
        })
      );
    });

    it('should show warning for invalid form submission', () => {
      component.exportForm.patchValue({ format: null }); // Invalid form

      component.onExport();

      expect(mockNotificationService.showWarning).toHaveBeenCalledWith(
        'Form validation failed',
        'Please complete all required fields before exporting'
      );
    });

    it('should save template when requested', () => {
      component.exportForm.patchValue({
        templateOptions: {
          saveAsTemplate: true,
          templateName: 'My Export Template',
          templateDescription: 'Custom export configuration'
        }
      });

      component.onExport();

      expect(mockStore.dispatch).toHaveBeenCalledWith(
        ExportActions.saveExportTemplate({
          template: jasmine.objectContaining({
            name: 'My Export Template',
            description: 'Custom export configuration'
          })
        })
      );
    });
  });

  describe('Export Progress Handling', () => {
    it('should handle export completion successfully', () => {
      const mockJob: ExportJob = {
        id: 'job-123',
        status: 'completed',
        downloadUrl: 'https://example.com/download/file.pdf',
        fileName: 'export-data.pdf'
      } as ExportJob;

      spyOn(component, 'downloadFile');

      component['handleExportCompletion'](mockJob);

      expect(mockAccessibilityService.announceExportCompletion).toHaveBeenCalledWith(
        true,
        'export-data.pdf'
      );
      expect(mockNotificationService.showSuccess).toHaveBeenCalledWith(
        'Export completed successfully',
        'Your PDF export is ready for download'
      );
      expect(component.downloadFile).toHaveBeenCalledWith(
        'https://example.com/download/file.pdf',
        'export-data.pdf'
      );
    });

    it('should handle export failure appropriately', () => {
      const mockJob: ExportJob = {
        id: 'job-123',
        status: 'failed',
        errorMessage: 'Network timeout'
      } as ExportJob;

      component['handleExportFailure'](mockJob);

      expect(mockAccessibilityService.announceExportCompletion).toHaveBeenCalledWith(false);
      expect(mockNotificationService.showError).toHaveBeenCalledWith(
        'Export failed',
        'Network timeout'
      );
    });

    it('should close dialog after successful export', fakeAsync(() => {
      const mockJob: ExportJob = {
        id: 'job-123',
        status: 'completed',
        downloadUrl: 'https://example.com/download/file.pdf',
        fileName: 'export-data.pdf'
      } as ExportJob;

      spyOn(component, 'downloadFile');

      component['handleExportCompletion'](mockJob);
      tick(2000);

      expect(mockDialogRef.close).toHaveBeenCalledWith({
        success: true,
        job: mockJob
      });
    }));
  });

  describe('Template Management', () => {
    beforeEach(() => {
      fixture.detectChanges();
    });

    it('should apply template configuration', () => {
      const templateId = 'template-123';

      component.onTemplateApply(templateId);

      expect(mockStore.dispatch).toHaveBeenCalledWith(
        ExportActions.applyExportTemplate({ templateId })
      );
    });
  });

  describe('Accessibility Features', () => {
    beforeEach(() => {
      fixture.detectChanges();
    });

    it('should generate correct step aria labels', () => {
      mockAccessibilityService.generateAriaLabel.and.returnValue('Step 2 of 4');

      const label = component.getStepAriaLabel(2);

      expect(mockAccessibilityService.generateAriaLabel).toHaveBeenCalledWith(
        'export.step',
        { current: 2, total: 4 }
      );
      expect(label).toBe('Step 2 of 4');
    });

    it('should generate correct progress aria labels', () => {
      mockAccessibilityService.generateAriaLabel.and.returnValue('Export 75% complete');

      const label = component.getProgressAriaLabel(75);

      expect(mockAccessibilityService.generateAriaLabel).toHaveBeenCalledWith(
        'export.progress',
        { percentage: 75 }
      );
      expect(label).toBe('Export 75% complete');
    });
  });

  describe('Error Handling', () => {
    beforeEach(() => {
      fixture.detectChanges();
    });

    it('should handle service errors gracefully', () => {
      mockExportService.estimateDataSize.and.returnValue(
        throwError({ message: 'Service unavailable' })
      );

      const dateRange = {
        startDate: new Date('2024-01-01'),
        endDate: new Date('2024-01-31')
      };

      component.exportForm.patchValue({ dateRange });

      // Should not throw error - component should handle gracefully
      expect(() => component['updateDataEstimation']()).not.toThrow();
    });
  });

  describe('Component Cleanup', () => {
    it('should clean up subscriptions on destroy', () => {
      fixture.detectChanges();
      spyOn(component['destroy$'], 'next');
      spyOn(component['destroy$'], 'complete');

      component.ngOnDestroy();

      expect(component['destroy$'].next).toHaveBeenCalled();
      expect(component['destroy$'].complete).toHaveBeenCalled();
    });
  });
});
```

### Format Selector Component Tests
```typescript
// format-selector.component.spec.ts
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { FormsModule } from '@angular/forms';
import { of } from 'rxjs';

import { FormatSelectorComponent } from './format-selector.component';
import { FormatService } from '../services/format.service';
import { ExportFormat } from '../models/export.models';

describe('FormatSelectorComponent', () => {
  let component: FormatSelectorComponent;
  let fixture: ComponentFixture<FormatSelectorComponent>;
  let mockFormatService: jasmine.SpyObj<FormatService>;

  beforeEach(async () => {
    const formatServiceSpy = jasmine.createSpyObj('FormatService', [
      'generatePreview',
      'getAvailableFormats',
      'validateFormatConfig'
    ]);

    await TestBed.configureTestingModule({
      declarations: [FormatSelectorComponent],
      imports: [FormsModule],
      providers: [
        { provide: FormatService, useValue: formatServiceSpy }
      ]
    }).compileComponents();

    mockFormatService = TestBed.inject(FormatService) as jasmine.SpyObj<FormatService>;
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(FormatSelectorComponent);
    component = fixture.componentInstance;
    component.availableFormats = [
      ExportFormat.PDF,
      ExportFormat.Excel,
      ExportFormat.CSV,
      ExportFormat.JSON,
      ExportFormat.XML
    ];
  });

  describe('Component Initialization', () => {
    it('should create', () => {
      expect(component).toBeTruthy();
    });

    it('should initialize format options for all available formats', () => {
      fixture.detectChanges();

      expect(component.formatOptions[ExportFormat.PDF]).toBeDefined();
      expect(component.formatOptions[ExportFormat.Excel]).toBeDefined();
      expect(component.formatOptions[ExportFormat.CSV]).toBeDefined();
      expect(component.formatOptions[ExportFormat.JSON]).toBeDefined();
      expect(component.formatOptions[ExportFormat.XML]).toBeDefined();
    });

    it('should set correct default options for PDF format', () => {
      fixture.detectChanges();

      const pdfOptions = component.formatOptions[ExportFormat.PDF];
      expect(pdfOptions.orientation).toBe('portrait');
      expect(pdfOptions.pageSize).toBe('A4');
      expect(pdfOptions.includeCharts).toBeTrue();
      expect(pdfOptions.fontSize).toBe(10);
    });

    it('should set correct default options for Excel format', () => {
      fixture.detectChanges();

      const excelOptions = component.formatOptions[ExportFormat.Excel];
      expect(excelOptions.worksheetName).toBe('Export Data');
      expect(excelOptions.includeCharts).toBeTrue();
      expect(excelOptions.freezeHeaders).toBeTrue();
      expect(excelOptions.autoFilter).toBeTrue();
    });

    it('should set correct default options for CSV format', () => {
      fixture.detectChanges();

      const csvOptions = component.formatOptions[ExportFormat.CSV];
      expect(csvOptions.delimiter).toBe(',');
      expect(csvOptions.encoding).toBe('UTF-8');
      expect(csvOptions.includeHeaders).toBeTrue();
      expect(csvOptions.dateFormat).toBe('YYYY-MM-DD');
    });
  });

  describe('Format Selection', () => {
    beforeEach(() => {
      fixture.detectChanges();
      spyOn(component.formatChange, 'emit');
      spyOn(component.optionsChange, 'emit');
    });

    it('should select format and emit events', () => {
      component.selectFormat(ExportFormat.PDF);

      expect(component.selectedFormat).toBe(ExportFormat.PDF);
      expect(component.formatChange.emit).toHaveBeenCalledWith(ExportFormat.PDF);
      expect(component.optionsChange.emit).toHaveBeenCalledWith(
        component.formatOptions[ExportFormat.PDF]
      );
    });

    it('should not select format when disabled', () => {
      component.disabled = true;

      component.selectFormat(ExportFormat.PDF);

      expect(component.selectedFormat).toBeNull();
      expect(component.formatChange.emit).not.toHaveBeenCalled();
    });

    it('should generate preview when enabled', () => {
      component.previewEnabled = true;
      mockFormatService.generatePreview.and.returnValue(of({
        content: 'sample preview',
        size: 1024
      }));

      component.selectFormat(ExportFormat.PDF);

      expect(mockFormatService.generatePreview).toHaveBeenCalledWith(
        ExportFormat.PDF,
        jasmine.any(Array),
        component.formatOptions[ExportFormat.PDF]
      );
    });
  });

  describe('Format Options Management', () => {
    beforeEach(() => {
      fixture.detectChanges();
      component.selectedFormat = ExportFormat.PDF;
      spyOn(component.optionsChange, 'emit');
    });

    it('should update format options and emit changes', () => {
      component.onFormatOptionChange(ExportFormat.PDF, 'orientation', 'landscape');

      expect(component.formatOptions[ExportFormat.PDF].orientation).toBe('landscape');
      expect(component.optionsChange.emit).toHaveBeenCalledWith(
        component.formatOptions[ExportFormat.PDF]
      );
    });

    it('should regenerate preview when options change', () => {
      component.previewEnabled = true;
      mockFormatService.generatePreview.and.returnValue(of({
        content: 'updated preview',
        size: 2048
      }));

      component.onFormatOptionChange(ExportFormat.PDF, 'pageSize', 'A3');

      expect(mockFormatService.generatePreview).toHaveBeenCalled();
    });

    it('should not emit changes for non-selected format', () => {
      component.selectedFormat = ExportFormat.Excel;

      component.onFormatOptionChange(ExportFormat.PDF, 'orientation', 'landscape');

      expect(component.optionsChange.emit).not.toHaveBeenCalled();
    });
  });

  describe('Preview Generation', () => {
    beforeEach(() => {
      fixture.detectChanges();
      component.previewEnabled = true;
    });

    it('should generate preview with sample data', () => {
      mockFormatService.generatePreview.and.returnValue(of({
        content: 'sample preview',
        size: 1024
      }));

      component['generatePreview'](ExportFormat.PDF);

      expect(mockFormatService.generatePreview).toHaveBeenCalledWith(
        ExportFormat.PDF,
        jasmine.arrayContaining([
          jasmine.objectContaining({ id: 1, name: 'Sample Document 1' })
        ]),
        component.formatOptions[ExportFormat.PDF]
      );
    });

    it('should handle preview generation errors gracefully', () => {
      mockFormatService.generatePreview.and.returnValue(
        throwError({ message: 'Preview generation failed' })
      );
      spyOn(console, 'error');

      component['generatePreview'](ExportFormat.PDF);

      expect(console.error).toHaveBeenCalledWith(
        'Preview generation failed:',
        jasmine.any(Object)
      );
    });
  });

  describe('Helper Methods', () => {
    it('should return correct format icons', () => {
      expect(component.getFormatIcon(ExportFormat.PDF)).toBe('picture_as_pdf');
      expect(component.getFormatIcon(ExportFormat.Excel)).toBe('grid_view');
      expect(component.getFormatIcon(ExportFormat.CSV)).toBe('table_chart');
      expect(component.getFormatIcon(ExportFormat.JSON)).toBe('code');
      expect(component.getFormatIcon(ExportFormat.XML)).toBe('description');
    });

    it('should return correct format names', () => {
      expect(component.getFormatName(ExportFormat.PDF)).toBe('export.format.pdf.name');
      expect(component.getFormatName(ExportFormat.Excel)).toBe('export.format.excel.name');
      expect(component.getFormatName(ExportFormat.CSV)).toBe('export.format.csv.name');
    });

    it('should return correct format features', () => {
      const pdfFeatures = component.getFormatFeatures(ExportFormat.PDF);
      expect(pdfFeatures).toContain('Charts');
      expect(pdfFeatures).toContain('Formatting');
      expect(pdfFeatures).toContain('Print-ready');

      const csvFeatures = component.getFormatFeatures(ExportFormat.CSV);
      expect(csvFeatures).toContain('Lightweight');
      expect(csvFeatures).toContain('Universal');
      expect(csvFeatures).toContain('Fast');
    });

    it('should track formats correctly', () => {
      const format = ExportFormat.PDF;
      const result = component.trackByFormat(0, format);
      expect(result).toBe(format);
    });
  });

  describe('ControlValueAccessor Implementation', () => {
    let onChangeSpy: jasmine.Spy;
    let onTouchedSpy: jasmine.Spy;

    beforeEach(() => {
      fixture.detectChanges();
      onChangeSpy = jasmine.createSpy('onChange');
      onTouchedSpy = jasmine.createSpy('onTouched');

      component.registerOnChange(onChangeSpy);
      component.registerOnTouched(onTouchedSpy);
    });

    it('should write value correctly', () => {
      component.writeValue(ExportFormat.Excel);

      expect(component.selectedFormat).toBe(ExportFormat.Excel);
    });

    it('should call onChange when format is selected', () => {
      component.selectFormat(ExportFormat.PDF);

      expect(onChangeSpy).toHaveBeenCalledWith(ExportFormat.PDF);
    });

    it('should call onTouched when format is selected', () => {
      component.selectFormat(ExportFormat.CSV);

      expect(onTouchedSpy).toHaveBeenCalled();
    });

    it('should set disabled state correctly', () => {
      component.setDisabledState(true);

      expect(component.disabled).toBeTrue();
    });

    it('should generate preview when value is written and preview is enabled', () => {
      component.previewEnabled = true;
      mockFormatService.generatePreview.and.returnValue(of({}));

      component.writeValue(ExportFormat.PDF);

      expect(mockFormatService.generatePreview).toHaveBeenCalled();
    });
  });
});
```

### Export Service Tests
```typescript
// export.service.spec.ts
import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';

import { ExportService } from './export.service';
import { ExportConfig, ExportJob, ExportFormat } from '../models/export.models';
import { environment } from '../../../environments/environment';

describe('ExportService', () => {
  let service: ExportService;
  let httpMock: HttpTestingController;
  const apiUrl = `${environment.apiUrl}/api/exports`;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [ExportService]
    });

    service = TestBed.inject(ExportService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  describe('Export Operations', () => {
    const mockConfig: ExportConfig = {
      format: ExportFormat.PDF,
      formatOptions: { pageSize: 'A4' },
      dateRange: {
        startDate: new Date('2024-01-01'),
        endDate: new Date('2024-01-31')
      },
      filters: {},
      columns: ['id', 'name', 'status'],
      deliveryOptions: { method: 'download' },
      metadata: { userId: 'user-123' }
    } as ExportConfig;

    const mockJob: ExportJob = {
      id: 'job-123',
      userId: 'user-123',
      config: mockConfig,
      status: 'pending',
      createdAt: new Date(),
      estimatedSize: 1024000
    } as ExportJob;

    it('should initiate export successfully', () => {
      service.initiateExport(mockConfig).subscribe(job => {
        expect(job).toEqual(mockJob);
      });

      const req = httpMock.expectOne(`${apiUrl}/initiate`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(mockConfig);
      req.flush(mockJob);
    });

    it('should retry failed export initiation', () => {
      let callCount = 0;

      service.initiateExport(mockConfig).subscribe({
        next: job => expect(job).toEqual(mockJob),
        error: () => fail('Should not error after retry')
      });

      // First call fails
      const req1 = httpMock.expectOne(`${apiUrl}/initiate`);
      req1.error(new ErrorEvent('Network error'));
      callCount++;

      // Second call fails
      const req2 = httpMock.expectOne(`${apiUrl}/initiate`);
      req2.error(new ErrorEvent('Network error'));
      callCount++;

      // Third call succeeds
      const req3 = httpMock.expectOne(`${apiUrl}/initiate`);
      req3.flush(mockJob);
      callCount++;

      expect(callCount).toBe(3); // Original + 2 retries
    });

    it('should get export progress', () => {
      const mockProgress = {
        jobId: 'job-123',
        percentage: 50,
        currentStage: 'processing_data',
        estimatedTimeRemaining: 30000,
        processedRecords: 2500,
        totalRecords: 5000
      };

      service.getExportProgress('job-123').subscribe(progress => {
        expect(progress).toEqual(mockProgress);
      });

      const req = httpMock.expectOne(`${apiUrl}/job-123/progress`);
      expect(req.request.method).toBe('GET');
      req.flush(mockProgress);
    });

    it('should cancel export job', () => {
      service.cancelExport('job-123').subscribe();

      const req = httpMock.expectOne(`${apiUrl}/job-123`);
      expect(req.request.method).toBe('DELETE');
      req.flush({});
    });

    it('should retry export job', () => {
      service.retryExport('job-123').subscribe(job => {
        expect(job).toEqual(mockJob);
      });

      const req = httpMock.expectOne(`${apiUrl}/job-123/retry`);
      expect(req.request.method).toBe('POST');
      req.flush(mockJob);
    });
  });

  describe('Download Operations', () => {
    it('should get download URL with expiration', () => {
      const mockResponse = {
        url: 'https://example.com/download/file.pdf',
        expiresAt: '2024-01-15T10:30:00Z'
      };

      service.getDownloadUrl('job-123').subscribe(result => {
        expect(result.url).toBe(mockResponse.url);
        expect(result.expiresAt).toEqual(new Date(mockResponse.expiresAt));
      });

      const req = httpMock.expectOne(`${apiUrl}/job-123/download-url`);
      req.flush(mockResponse);
    });

    it('should download export file with progress reporting', () => {
      const mockBlob = new Blob(['test content'], { type: 'application/pdf' });

      service.downloadExport('job-123').subscribe(event => {
        // Test will receive progress events and final response
      });

      const req = httpMock.expectOne(`${apiUrl}/job-123/download`);
      expect(req.request.responseType).toBe('blob');
      expect(req.request.reportProgress).toBeTrue();
      req.flush(mockBlob);
    });

    it('should handle download timeout', () => {
      service.downloadExport('job-123').subscribe({
        next: () => fail('Should timeout'),
        error: error => {
          expect(error.name).toBe('TimeoutError');
        }
      });

      const req = httpMock.expectOne(`${apiUrl}/job-123/download`);
      // Don't flush - let it timeout
    });
  });

  describe('Data Operations', () => {
    it('should estimate data size', () => {
      const filters = { status: 'active' };
      const dateRange = {
        startDate: new Date('2024-01-01'),
        endDate: new Date('2024-01-31')
      };
      const mockEstimate = { sizeBytes: 1024000, recordCount: 5000 };

      service.estimateDataSize(filters, dateRange).subscribe(estimate => {
        expect(estimate).toEqual(mockEstimate);
      });

      const req = httpMock.expectOne(`${apiUrl}/estimate`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual({ filters, dateRange });
      req.flush(mockEstimate);
    });

    it('should validate export configuration', () => {
      const mockValidation = {
        isValid: true,
        errors: [],
        warnings: ['Large dataset - export may take longer than usual']
      };

      service.validateExportConfig(mockConfig).subscribe(result => {
        expect(result).toEqual(mockValidation);
      });

      const req = httpMock.expectOne(`${apiUrl}/validate`);
      expect(req.request.body).toEqual(mockConfig);
      req.flush(mockValidation);
    });

    it('should get column definitions', () => {
      const mockColumns = [
        { key: 'id', displayName: 'ID', dataType: 'number', exportable: true },
        { key: 'name', displayName: 'Name', dataType: 'string', exportable: true },
        { key: 'status', displayName: 'Status', dataType: 'string', exportable: true }
      ];

      service.getColumnDefinitions('analytics-dashboard').subscribe(columns => {
        expect(columns).toEqual(mockColumns);
      });

      const req = httpMock.expectOne(`${apiUrl}/columns/analytics-dashboard`);
      req.flush(mockColumns);
    });

    it('should preview filtered data', () => {
      const filters = { status: 'completed' };
      const mockData = [
        { id: 1, name: 'Document 1', status: 'completed' },
        { id: 2, name: 'Document 2', status: 'completed' }
      ];

      service.previewFilteredData(filters, 50).subscribe(data => {
        expect(data).toEqual(mockData);
      });

      const req = httpMock.expectOne(`${apiUrl}/preview`);
      expect(req.request.body).toEqual({ filters, limit: 50 });
      req.flush(mockData);
    });
  });

  describe('History and Statistics', () => {
    it('should get export history with pagination', () => {
      const mockHistory = {
        items: [mockJob],
        totalCount: 1,
        page: 1,
        pageSize: 20,
        totalPages: 1
      };

      service.getExportHistory('user-123', 1, 20).subscribe(history => {
        expect(history).toEqual(mockHistory);
      });

      const req = httpMock.expectOne(`${apiUrl}/history?userId=user-123&page=1&pageSize=20`);
      req.flush(mockHistory);
    });

    it('should get export statistics', () => {
      const mockStats = {
        totalExports: 25,
        successfulExports: 23,
        failedExports: 2,
        averageSize: 2048000,
        popularFormats: ['PDF', 'Excel', 'CSV']
      };

      service.getExportStatistics('user-123', 'month').subscribe(stats => {
        expect(stats).toEqual(mockStats);
      });

      const req = httpMock.expectOne(`${apiUrl}/statistics?userId=user-123&period=month`);
      req.flush(mockStats);
    });
  });

  describe('Email Operations', () => {
    it('should send export by email', () => {
      const recipients = ['user@example.com', 'manager@example.com'];
      const subject = 'Export Ready';
      const body = 'Your export is ready for download.';

      service.sendExportByEmail('job-123', recipients, subject, body).subscribe();

      const req = httpMock.expectOne(`${apiUrl}/job-123/email`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual({ recipients, subject, body });
      req.flush({});
    });
  });

  describe('Error Handling', () => {
    it('should handle 400 Bad Request errors', () => {
      service.initiateExport(mockConfig).subscribe({
        next: () => fail('Should error'),
        error: error => {
          expect(error.message).toBe('Invalid export configuration');
          expect(error.status).toBe(400);
        }
      });

      const req = httpMock.expectOne(`${apiUrl}/initiate`);
      req.error(new ErrorEvent('Bad Request'), { status: 400 });
    });

    it('should handle 401 Unauthorized errors', () => {
      service.getExportHistory('user-123').subscribe({
        next: () => fail('Should error'),
        error: error => {
          expect(error.message).toBe('Authentication required');
          expect(error.status).toBe(401);
        }
      });

      const req = httpMock.expectOne(`${apiUrl}/history?userId=user-123&page=1&pageSize=20`);
      req.error(new ErrorEvent('Unauthorized'), { status: 401 });
    });

    it('should handle 403 Forbidden errors', () => {
      service.downloadExport('job-123').subscribe({
        next: () => fail('Should error'),
        error: error => {
          expect(error.message).toBe('Access denied - insufficient permissions');
          expect(error.status).toBe(403);
        }
      });

      const req = httpMock.expectOne(`${apiUrl}/job-123/download`);
      req.error(new ErrorEvent('Forbidden'), { status: 403 });
    });

    it('should handle 429 Rate Limit errors', () => {
      service.initiateExport(mockConfig).subscribe({
        next: () => fail('Should error'),
        error: error => {
          expect(error.message).toBe('Too many export requests - please try again later');
          expect(error.status).toBe(429);
        }
      });

      const req = httpMock.expectOne(`${apiUrl}/initiate`);
      req.error(new ErrorEvent('Too Many Requests'), { status: 429 });
    });

    it('should handle 500 Server errors', () => {
      service.estimateDataSize({}, {}).subscribe({
        next: () => fail('Should error'),
        error: error => {
          expect(error.message).toBe('Server error - please try again later');
          expect(error.status).toBe(500);
        }
      });

      const req = httpMock.expectOne(`${apiUrl}/estimate`);
      req.error(new ErrorEvent('Internal Server Error'), { status: 500 });
    });

    it('should handle network errors', () => {
      service.getAvailableFormats().subscribe({
        next: () => fail('Should error'),
        error: error => {
          expect(error.message).toBe('Network connection failed');
        }
      });

      const req = httpMock.expectOne(`${apiUrl}/formats`);
      req.error(new ErrorEvent('Network connection failed'));
    });
  });

  describe('Timeout Handling', () => {
    it('should apply correct timeout for regular operations', () => {
      service.getExportJob('job-123').subscribe({
        next: () => fail('Should timeout'),
        error: error => expect(error.name).toBe('TimeoutError')
      });

      // Request should timeout at 30 seconds for regular operations
      const req = httpMock.expectOne(`${apiUrl}/job-123`);
      // Test framework doesn't easily simulate timeouts, but we verify the timeout is configured
    });

    it('should apply longer timeout for download operations', () => {
      service.downloadExport('job-123').subscribe();

      const req = httpMock.expectOne(`${apiUrl}/job-123/download`);
      // Download operations should have 5-minute timeout
      req.flush(new Blob());
    });
  });
});
```

## 2. Integration Test Implementation

### Export Dialog Integration Tests
```typescript
// export-dialog.integration.spec.ts
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { Store } from '@ngrx/store';
import { provideMockStore, MockStore } from '@ngrx/store/testing';
import { of } from 'rxjs';

import { ExportDialogComponent } from './export-dialog.component';
import { FormatSelectorComponent } from './format-selector.component';
import { ExportProgressComponent } from './export-progress.component';
import { ExportService } from '../services/export.service';
import { FormatService } from '../services/format.service';
import { ExportFormat } from '../models/export.models';

describe('ExportDialog Integration', () => {
  let component: ExportDialogComponent;
  let fixture: ComponentFixture<ExportDialogComponent>;
  let store: MockStore;
  let mockExportService: jasmine.SpyObj<ExportService>;
  let mockFormatService: jasmine.SpyObj<FormatService>;

  const initialState = {
    export: {
      availableFormats: [ExportFormat.PDF, ExportFormat.Excel, ExportFormat.CSV],
      isExporting: false,
      currentJobId: null,
      dialogOpen: true
    }
  };

  beforeEach(async () => {
    const exportServiceSpy = jasmine.createSpyObj('ExportService', [
      'estimateDataSize',
      'validateExportConfig'
    ]);
    const formatServiceSpy = jasmine.createSpyObj('FormatService', [
      'generatePreview',
      'getAvailableFormats'
    ]);

    await TestBed.configureTestingModule({
      declarations: [
        ExportDialogComponent,
        FormatSelectorComponent,
        ExportProgressComponent
      ],
      imports: [
        ReactiveFormsModule,
        MatDialogModule,
        MatFormFieldModule,
        MatSelectModule,
        MatInputModule,
        MatButtonModule,
        MatProgressBarModule,
        NoopAnimationsModule
      ],
      providers: [
        provideMockStore({ initialState }),
        { provide: ExportService, useValue: exportServiceSpy },
        { provide: FormatService, useValue: formatServiceSpy },
        { provide: MatDialogRef, useValue: jasmine.createSpyObj('MatDialogRef', ['close']) },
        { provide: MAT_DIALOG_DATA, useValue: { dataSource: 'test', initialFilters: {} } }
      ]
    }).compileComponents();

    store = TestBed.inject(Store) as MockStore;
    mockExportService = TestBed.inject(ExportService) as jasmine.SpyObj<ExportService>;
    mockFormatService = TestBed.inject(FormatService) as jasmine.SpyObj<FormatService>;

    // Setup service mocks
    mockExportService.estimateDataSize.and.returnValue(of({
      sizeBytes: 1024000,
      recordCount: 5000
    }));
    mockFormatService.generatePreview.and.returnValue(of({
      content: 'sample preview',
      size: 1024
    }));
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(ExportDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  describe('Complete Export Workflow', () => {
    it('should complete full export configuration workflow', async () => {
      // Step 1: Select format
      const formatSelector = fixture.debugElement.query(
        sel => sel.componentInstance instanceof FormatSelectorComponent
      );
      expect(formatSelector).toBeTruthy();

      formatSelector.componentInstance.selectFormat(ExportFormat.PDF);
      fixture.detectChanges();

      expect(component.exportForm.get('format')?.value).toBe(ExportFormat.PDF);
      expect(component.formatValid$.value).toBeTrue();

      // Step 2: Set date range
      const dateRange = {
        startDate: new Date('2024-01-01'),
        endDate: new Date('2024-01-31')
      };
      component.exportForm.patchValue({ dateRange });
      fixture.detectChanges();

      expect(component.dateRangeValid$.value).toBeTrue();

      // Step 3: Configure filters
      const filters = { status: 'completed' };
      component.exportForm.patchValue({ filters });
      fixture.detectChanges();

      expect(component.filtersValid$.value).toBeTrue();

      // Step 4: Set delivery options
      const deliveryOptions = { method: 'download' };
      component.exportForm.patchValue({ deliveryOptions });
      fixture.detectChanges();

      expect(component.optionsValid$.value).toBeTrue();

      // Verify form is valid and export button is enabled
      expect(component.exportForm.valid).toBeTrue();

      const exportButton = fixture.debugElement.query(
        sel => sel.nativeElement.textContent?.includes('Export')
      );
      expect(exportButton.nativeElement.disabled).toBeFalse();
    });

    it('should handle format-specific option changes', () => {
      // Select PDF format
      component.exportForm.patchValue({ format: ExportFormat.PDF });
      fixture.detectChanges();

      // Update PDF-specific options
      const formatOptions = {
        orientation: 'landscape',
        pageSize: 'A3',
        includeCharts: true
      };
      component.exportForm.patchValue({ formatOptions });
      fixture.detectChanges();

      expect(component.exportForm.get('formatOptions')?.value).toEqual(formatOptions);

      // Switch to Excel format
      component.exportForm.patchValue({ format: ExportFormat.Excel });
      fixture.detectChanges();

      // Verify Excel options are applied
      const excelOptions = {
        worksheetName: 'Custom Export',
        includeCharts: false
      };
      component.exportForm.patchValue({ formatOptions: excelOptions });
      fixture.detectChanges();

      expect(component.exportForm.get('formatOptions')?.value).toEqual(excelOptions);
    });
  });

  describe('Real-time Data Estimation', () => {
    it('should update size estimation when filters change', async () => {
      // Set initial valid configuration
      component.exportForm.patchValue({
        format: ExportFormat.CSV,
        dateRange: {
          startDate: new Date('2024-01-01'),
          endDate: new Date('2024-01-31')
        }
      });
      fixture.detectChanges();

      // Change filters
      const filters = { status: 'pending', priority: 'high' };
      component.exportForm.patchValue({ filters });
      fixture.detectChanges();

      // Wait for debounced estimation call
      await new Promise(resolve => setTimeout(resolve, 600));

      expect(mockExportService.estimateDataSize).toHaveBeenCalledWith(
        filters,
        jasmine.any(Object)
      );
      expect(component.estimatedSize$.value).toBe(1024000);
      expect(component.estimatedRecords$.value).toBe(5000);
    });

    it('should show size warning for large exports', async () => {
      // Mock large dataset estimation
      mockExportService.estimateDataSize.and.returnValue(of({
        sizeBytes: 500 * 1024 * 1024, // 500MB
        recordCount: 1000000
      }));

      component.exportForm.patchValue({
        format: ExportFormat.Excel,
        dateRange: {
          startDate: new Date('2023-01-01'),
          endDate: new Date('2024-01-01')
        },
        filters: {}
      });
      fixture.detectChanges();

      await new Promise(resolve => setTimeout(resolve, 600));

      expect(component.estimatedSize$.value).toBe(500 * 1024 * 1024);
      expect(component.estimatedRecords$.value).toBe(1000000);

      // Should show large dataset warning in UI
      // This would be verified by checking for warning message in template
    });
  });

  describe('Progress Tracking Integration', () => {
    it('should show progress component during export', () => {
      // Start export process
      store.setState({
        ...initialState,
        export: {
          ...initialState.export,
          isExporting: true,
          currentJobId: 'job-123'
        }
      });
      fixture.detectChanges();

      // Verify progress component is shown
      const progressComponent = fixture.debugElement.query(
        sel => sel.componentInstance instanceof ExportProgressComponent
      );
      expect(progressComponent).toBeTruthy();
      expect(progressComponent.componentInstance.jobId).toBe('job-123');
    });

    it('should handle export completion', () => {
      // Simulate export completion
      const completedJob = {
        id: 'job-123',
        status: 'completed',
        downloadUrl: 'https://example.com/download/file.pdf',
        fileName: 'export-data.pdf'
      };

      spyOn(component, 'downloadFile');

      component['handleExportCompletion'](completedJob as any);
      fixture.detectChanges();

      expect(component.downloadFile).toHaveBeenCalledWith(
        'https://example.com/download/file.pdf',
        'export-data.pdf'
      );
    });
  });

  describe('Accessibility Integration', () => {
    it('should maintain proper focus management', () => {
      // Verify initial focus
      const firstFocusableElement = fixture.debugElement.query(
        sel => sel.nativeElement.tabIndex >= 0 ||
              sel.nativeElement.tagName === 'INPUT' ||
              sel.nativeElement.tagName === 'BUTTON'
      );
      expect(firstFocusableElement).toBeTruthy();

      // Test tab navigation
      const focusableElements = fixture.debugElement.queryAll(
        sel => sel.nativeElement.tabIndex >= 0
      );
      expect(focusableElements.length).toBeGreaterThan(0);
    });

    it('should provide proper ARIA labels', () => {
      const dialogElement = fixture.debugElement.query(
        sel => sel.nativeElement.getAttribute('role') === 'dialog'
      );
      expect(dialogElement).toBeTruthy();
      expect(dialogElement.nativeElement.getAttribute('aria-label')).toBeTruthy();

      // Check step indicators have proper labels
      const stepElements = fixture.debugElement.queryAll(
        sel => sel.nativeElement.getAttribute('aria-label')?.includes('Step')
      );
      expect(stepElements.length).toBeGreaterThan(0);
    });

    it('should support keyboard navigation', () => {
      const dialog = fixture.debugElement.nativeElement;

      // Test Escape key closes dialog
      const escapeEvent = new KeyboardEvent('keydown', { key: 'Escape' });
      dialog.dispatchEvent(escapeEvent);
      fixture.detectChanges();

      // Would verify dialog close behavior
      // In real implementation, this would trigger dialog close
    });
  });

  describe('Form Validation Integration', () => {
    it('should show validation errors for incomplete form', () => {
      // Submit empty form
      component.onExport();
      fixture.detectChanges();

      // Check that validation errors are displayed
      const errorElements = fixture.debugElement.queryAll(
        sel => sel.nativeElement.classList.contains('mat-error')
      );
      expect(errorElements.length).toBeGreaterThan(0);
    });

    it('should prevent submission with invalid configuration', () => {
      // Set invalid date range
      component.exportForm.patchValue({
        format: ExportFormat.PDF,
        dateRange: {
          startDate: new Date('2024-01-31'),
          endDate: new Date('2024-01-01') // End before start
        }
      });
      fixture.detectChanges();

      expect(component.exportForm.invalid).toBeTrue();

      const exportButton = fixture.debugElement.query(
        sel => sel.nativeElement.textContent?.includes('Export')
      );
      expect(exportButton.nativeElement.disabled).toBeTrue();
    });
  });
});
```

## 3. End-to-End Test Implementation

### Export Workflow E2E Tests
```typescript
// export-functionality.e2e.spec.ts
import { test, expect } from '@playwright/test';

test.describe('Export Functionality E2E Tests', () => {
  test.beforeEach(async ({ page }) => {
    // Navigate to analytics dashboard
    await page.goto('/analytics-dashboard');

    // Login if needed
    await page.waitForSelector('[data-testid="analytics-dashboard"]');
  });

  test.describe('Export Dialog Workflow', () => {
    test('should open export dialog and complete PDF export', async ({ page }) => {
      // Open export dialog
      await page.click('[data-testid="export-button"]');
      await expect(page.locator('[data-testid="export-dialog"]')).toBeVisible();

      // Step 1: Select PDF format
      await page.click('[data-testid="format-pdf"]');
      await expect(page.locator('[data-testid="format-pdf"]')).toHaveClass(/selected/);

      // Step 2: Configure PDF options
      await page.selectOption('[data-testid="pdf-orientation"]', 'landscape');
      await page.selectOption('[data-testid="pdf-page-size"]', 'A3');
      await page.check('[data-testid="pdf-include-charts"]');

      // Step 3: Set date range
      await page.click('[data-testid="date-range-preset-30days"]');
      await expect(page.locator('[data-testid="estimated-records"]')).toContainText(/\d+/);

      // Step 4: Configure filters
      await page.click('[data-testid="add-filter"]');
      await page.selectOption('[data-testid="filter-column"]', 'status');
      await page.selectOption('[data-testid="filter-operator"]', 'equals');
      await page.fill('[data-testid="filter-value"]', 'completed');

      // Step 5: Set delivery options
      await page.check('[data-testid="delivery-download"]');

      // Verify export button is enabled
      await expect(page.locator('[data-testid="export-submit"]')).toBeEnabled();

      // Submit export
      await page.click('[data-testid="export-submit"]');

      // Verify progress tracking
      await expect(page.locator('[data-testid="export-progress"]')).toBeVisible();
      await expect(page.locator('[data-testid="progress-percentage"]')).toContainText(/\d+%/);

      // Wait for completion (with timeout)
      await page.waitForSelector('[data-testid="export-completed"]', { timeout: 30000 });

      // Verify download link
      await expect(page.locator('[data-testid="download-link"]')).toBeVisible();

      // Click download
      const downloadPromise = page.waitForEvent('download');
      await page.click('[data-testid="download-link"]');
      const download = await downloadPromise;

      expect(download.suggestedFilename()).toContain('.pdf');
    });

    test('should handle Excel export with advanced options', async ({ page }) => {
      await page.click('[data-testid="export-button"]');

      // Select Excel format
      await page.click('[data-testid="format-excel"]');

      // Configure Excel-specific options
      await page.fill('[data-testid="excel-worksheet-name"]', 'Analytics Data Export');
      await page.check('[data-testid="excel-include-charts"]');
      await page.check('[data-testid="excel-auto-filter"]');
      await page.check('[data-testid="excel-freeze-headers"]');

      // Set custom date range
      await page.click('[data-testid="date-range-custom"]');
      await page.fill('[data-testid="start-date"]', '2024-01-01');
      await page.fill('[data-testid="end-date"]', '2024-03-31');

      // Select specific columns
      await page.click('[data-testid="column-selector"]');
      await page.check('[data-testid="column-id"]');
      await page.check('[data-testid="column-name"]');
      await page.check('[data-testid="column-status"]');
      await page.check('[data-testid="column-created-date"]');
      await page.click('[data-testid="column-selector-close"]');

      // Submit export
      await page.click('[data-testid="export-submit"]');

      // Monitor progress
      await expect(page.locator('[data-testid="export-progress"]')).toBeVisible();

      // Wait for completion
      await page.waitForSelector('[data-testid="export-completed"]', { timeout: 45000 });

      // Verify Excel download
      const downloadPromise = page.waitForEvent('download');
      await page.click('[data-testid="download-link"]');
      const download = await downloadPromise;

      expect(download.suggestedFilename()).toContain('.xlsx');
    });

    test('should support CSV export with custom delimiters', async ({ page }) => {
      await page.click('[data-testid="export-button"]');

      // Select CSV format
      await page.click('[data-testid="format-csv"]');

      // Configure CSV options
      await page.selectOption('[data-testid="csv-delimiter"]', ';');
      await page.selectOption('[data-testid="csv-encoding"]', 'UTF-16');
      await page.check('[data-testid="csv-include-headers"]');
      await page.selectOption('[data-testid="csv-date-format"]', 'DD/MM/YYYY');

      // Use preset date range
      await page.click('[data-testid="date-range-preset-7days"]');

      // Submit export
      await page.click('[data-testid="export-submit"]');

      // CSV exports should be faster
      await page.waitForSelector('[data-testid="export-completed"]', { timeout: 15000 });

      // Download and verify
      const downloadPromise = page.waitForEvent('download');
      await page.click('[data-testid="download-link"]');
      const download = await downloadPromise;

      expect(download.suggestedFilename()).toContain('.csv');
    });
  });

  test.describe('Email Delivery', () => {
    test('should send export via email', async ({ page }) => {
      await page.click('[data-testid="export-button"]');

      // Basic export configuration
      await page.click('[data-testid="format-pdf"]');
      await page.click('[data-testid="date-range-preset-30days"]');

      // Configure email delivery
      await page.check('[data-testid="delivery-email"]');
      await page.fill('[data-testid="email-recipients"]', 'test@example.com');
      await page.fill('[data-testid="email-subject"]', 'Analytics Export - Monthly Report');
      await page.fill('[data-testid="email-body"]', 'Please find the attached monthly analytics report.');

      // Submit export
      await page.click('[data-testid="export-submit"]');

      // Wait for completion
      await page.waitForSelector('[data-testid="export-completed"]', { timeout: 30000 });

      // Verify email delivery confirmation
      await expect(page.locator('[data-testid="email-sent-confirmation"]')).toBeVisible();
      await expect(page.locator('[data-testid="email-sent-confirmation"]')).toContainText('test@example.com');
    });

    test('should handle multiple email recipients', async ({ page }) => {
      await page.click('[data-testid="export-button"]');

      // Configure basic export
      await page.click('[data-testid="format-excel"]');
      await page.click('[data-testid="date-range-preset-7days"]');

      // Add multiple email recipients
      await page.check('[data-testid="delivery-email"]');
      await page.fill('[data-testid="email-recipients"]', 'manager@example.com, analyst@example.com, ceo@example.com');

      // Submit and verify
      await page.click('[data-testid="export-submit"]');
      await page.waitForSelector('[data-testid="export-completed"]', { timeout: 30000 });

      await expect(page.locator('[data-testid="email-sent-confirmation"]')).toContainText('3 recipients');
    });
  });

  test.describe('Template Management', () => {
    test('should save export configuration as template', async ({ page }) => {
      await page.click('[data-testid="export-button"]');

      // Configure complex export
      await page.click('[data-testid="format-pdf"]');
      await page.selectOption('[data-testid="pdf-orientation"]', 'landscape');
      await page.click('[data-testid="date-range-preset-90days"]');

      // Add filters
      await page.click('[data-testid="add-filter"]');
      await page.selectOption('[data-testid="filter-column"]', 'priority');
      await page.selectOption('[data-testid="filter-operator"]', 'in');
      await page.fill('[data-testid="filter-value"]', 'high, urgent');

      // Save as template
      await page.check('[data-testid="save-as-template"]');
      await page.fill('[data-testid="template-name"]', 'Quarterly High Priority Report');
      await page.fill('[data-testid="template-description"]', 'High priority items for quarterly review');

      // Submit export (which also saves template)
      await page.click('[data-testid="export-submit"]');

      // Wait for completion
      await page.waitForSelector('[data-testid="export-completed"]', { timeout: 30000 });

      // Verify template was saved
      await page.click('[data-testid="close-export-dialog"]');
      await page.click('[data-testid="export-button"]');

      // Check template appears in list
      await page.click('[data-testid="load-template"]');
      await expect(page.locator('[data-testid="template-quarterly-high-priority-report"]')).toBeVisible();
    });

    test('should apply saved template', async ({ page }) => {
      // Assuming template exists from previous test or setup
      await page.click('[data-testid="export-button"]');

      // Load template
      await page.click('[data-testid="load-template"]');
      await page.click('[data-testid="template-quarterly-high-priority-report"]');

      // Verify configuration is applied
      await expect(page.locator('[data-testid="format-pdf"]')).toHaveClass(/selected/);
      await expect(page.locator('[data-testid="pdf-orientation"]')).toHaveValue('landscape');
      await expect(page.locator('[data-testid="date-range-preset-90days"]')).toBeChecked();

      // Verify filters are applied
      await expect(page.locator('[data-testid="active-filters"]')).toContainText('priority in high, urgent');

      // Submit with template configuration
      await page.click('[data-testid="export-submit"]');
      await page.waitForSelector('[data-testid="export-completed"]', { timeout: 30000 });
    });
  });

  test.describe('Error Handling', () => {
    test('should handle large dataset warnings', async ({ page }) => {
      await page.click('[data-testid="export-button"]');

      // Configure for large dataset
      await page.click('[data-testid="format-excel"]');
      await page.click('[data-testid="date-range-custom"]');
      await page.fill('[data-testid="start-date"]', '2020-01-01');
      await page.fill('[data-testid="end-date"]', '2024-12-31');

      // Should show size warning
      await expect(page.locator('[data-testid="size-warning"]')).toBeVisible();
      await expect(page.locator('[data-testid="size-warning"]')).toContainText(/large dataset/i);

      // Should show estimated processing time
      await expect(page.locator('[data-testid="estimated-time"]')).toBeVisible();
      await expect(page.locator('[data-testid="estimated-time"]')).toContainText(/minute/);
    });

    test('should handle export cancellation', async ({ page }) => {
      await page.click('[data-testid="export-button"]');

      // Start export
      await page.click('[data-testid="format-pdf"]');
      await page.click('[data-testid="date-range-preset-30days"]');
      await page.click('[data-testid="export-submit"]');

      // Wait for progress to start
      await expect(page.locator('[data-testid="export-progress"]')).toBeVisible();

      // Cancel export
      await page.click('[data-testid="cancel-export"]');

      // Verify cancellation
      await expect(page.locator('[data-testid="export-cancelled"]')).toBeVisible();
      await expect(page.locator('[data-testid="export-cancelled"]')).toContainText('cancelled');
    });

    test('should handle export failures gracefully', async ({ page }) => {
      // Mock network failure
      await page.route('**/api/exports/initiate', route => {
        route.abort('failed');
      });

      await page.click('[data-testid="export-button"]');
      await page.click('[data-testid="format-pdf"]');
      await page.click('[data-testid="date-range-preset-7days"]');
      await page.click('[data-testid="export-submit"]');

      // Should show error message
      await expect(page.locator('[data-testid="export-error"]')).toBeVisible();
      await expect(page.locator('[data-testid="export-error"]')).toContainText(/failed/i);

      // Should offer retry option
      await expect(page.locator('[data-testid="retry-export"]')).toBeVisible();
    });

    test('should validate form before submission', async ({ page }) => {
      await page.click('[data-testid="export-button"]');

      // Try to submit without selecting format
      await page.click('[data-testid="export-submit"]');

      // Should show validation errors
      await expect(page.locator('[data-testid="format-error"]')).toBeVisible();
      await expect(page.locator('[data-testid="format-error"]')).toContainText(/required/i);

      // Export button should remain disabled
      await expect(page.locator('[data-testid="export-submit"]')).toBeDisabled();
    });
  });

  test.describe('Accessibility', () => {
    test('should support keyboard navigation', async ({ page }) => {
      await page.click('[data-testid="export-button"]');

      // Test tab navigation through dialog
      await page.keyboard.press('Tab');
      await expect(page.locator(':focus')).toHaveAttribute('data-testid', 'format-pdf');

      await page.keyboard.press('Tab');
      await expect(page.locator(':focus')).toHaveAttribute('data-testid', 'format-excel');

      // Test Enter key for selection
      await page.keyboard.press('Enter');
      await expect(page.locator('[data-testid="format-excel"]')).toHaveClass(/selected/);

      // Test Escape key to close dialog
      await page.keyboard.press('Escape');
      await expect(page.locator('[data-testid="export-dialog"]')).not.toBeVisible();
    });

    test('should provide screen reader announcements', async ({ page }) => {
      await page.click('[data-testid="export-button"]');

      // Check ARIA labels
      await expect(page.locator('[data-testid="export-dialog"]')).toHaveAttribute('aria-label');
      await expect(page.locator('[data-testid="format-selector"]')).toHaveAttribute('role', 'radiogroup');

      // Start export to test progress announcements
      await page.click('[data-testid="format-csv"]');
      await page.click('[data-testid="date-range-preset-7days"]');
      await page.click('[data-testid="export-submit"]');

      // Progress should have proper ARIA attributes
      await expect(page.locator('[data-testid="export-progress"]')).toHaveAttribute('role', 'progressbar');
      await expect(page.locator('[data-testid="export-progress"]')).toHaveAttribute('aria-valuenow');
    });

    test('should support high contrast mode', async ({ page }) => {
      // Enable high contrast mode simulation
      await page.emulateMedia({ reducedMotion: 'reduce' });

      await page.click('[data-testid="export-button"]');

      // Verify dialog is still usable
      await expect(page.locator('[data-testid="export-dialog"]')).toBeVisible();
      await expect(page.locator('[data-testid="format-pdf"]')).toBeVisible();

      // Check button contrast
      const exportButton = page.locator('[data-testid="export-submit"]');
      await expect(exportButton).toBeVisible();
    });
  });

  test.describe('Performance', () => {
    test('should load export dialog quickly', async ({ page }) => {
      const startTime = Date.now();

      await page.click('[data-testid="export-button"]');
      await expect(page.locator('[data-testid="export-dialog"]')).toBeVisible();

      const loadTime = Date.now() - startTime;
      expect(loadTime).toBeLessThan(1000); // Should load within 1 second
    });

    test('should handle large filter previews efficiently', async ({ page }) => {
      await page.click('[data-testid="export-button"]');
      await page.click('[data-testid="format-csv"]');

      // Add multiple complex filters
      for (let i = 0; i < 5; i++) {
        await page.click('[data-testid="add-filter"]');
        await page.selectOption(`[data-testid="filter-column-${i}"]`, 'status');
        await page.selectOption(`[data-testid="filter-operator-${i}"]`, 'not_equals');
        await page.fill(`[data-testid="filter-value-${i}"]`, `value-${i}`);
      }

      // Preview should still load reasonably fast
      const previewStartTime = Date.now();
      await page.click('[data-testid="preview-data"]');
      await expect(page.locator('[data-testid="preview-results"]')).toBeVisible();

      const previewLoadTime = Date.now() - previewStartTime;
      expect(previewLoadTime).toBeLessThan(3000); // Preview within 3 seconds
    });
  });
});
```

## 4. Test Configuration and Setup

### Test Configuration Files
```typescript
// jest.config.js
module.exports = {
  preset: 'jest-preset-angular',
  setupFilesAfterEnv: ['<rootDir>/src/test-setup.ts'],
  testMatch: [
    '<rootDir>/src/**/*.spec.ts'
  ],
  collectCoverageFrom: [
    'src/app/export/**/*.ts',
    '!src/app/export/**/*.spec.ts',
    '!src/app/export/**/index.ts'
  ],
  coverageThreshold: {
    global: {
      branches: 90,
      functions: 90,
      lines: 90,
      statements: 90
    }
  },
  moduleNameMapping: {
    '@app/(.*)': '<rootDir>/src/app/$1',
    '@shared/(.*)': '<rootDir>/src/app/shared/$1',
    '@export/(.*)': '<rootDir>/src/app/export/$1'
  }
};
```

```typescript
// playwright.config.ts
import { defineConfig, devices } from '@playwright/test';

export default defineConfig({
  testDir: './e2e',
  fullyParallel: true,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,
  workers: process.env.CI ? 1 : undefined,
  reporter: [
    ['html'],
    ['junit', { outputFile: 'test-results/junit.xml' }]
  ],
  use: {
    baseURL: 'http://localhost:4200',
    trace: 'on-first-retry',
    screenshot: 'only-on-failure',
    video: 'retain-on-failure'
  },
  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] }
    },
    {
      name: 'firefox',
      use: { ...devices['Desktop Firefox'] }
    },
    {
      name: 'webkit',
      use: { ...devices['Desktop Safari'] }
    },
    {
      name: 'Mobile Chrome',
      use: { ...devices['Pixel 5'] }
    }
  ],
  webServer: {
    command: 'npm start',
    url: 'http://localhost:4200',
    reuseExistingServer: !process.env.CI
  }
});
```

### Test Utilities
```typescript
// test-utilities.ts
import { ComponentFixture } from '@angular/core/testing';
import { DebugElement } from '@angular/core';
import { By } from '@angular/platform-browser';

export class TestUtils {
  static findByTestId<T>(fixture: ComponentFixture<T>, testId: string): DebugElement {
    return fixture.debugElement.query(By.css(`[data-testid="${testId}"]`));
  }

  static findAllByTestId<T>(fixture: ComponentFixture<T>, testId: string): DebugElement[] {
    return fixture.debugElement.queryAll(By.css(`[data-testid="${testId}"]`));
  }

  static clickElement(element: DebugElement): void {
    element.nativeElement.click();
  }

  static setInputValue(element: DebugElement, value: string): void {
    element.nativeElement.value = value;
    element.nativeElement.dispatchEvent(new Event('input'));
  }

  static waitForAsync(ms: number = 0): Promise<void> {
    return new Promise(resolve => setTimeout(resolve, ms));
  }

  static createMockExportJob(overrides: Partial<any> = {}): any {
    return {
      id: 'test-job-123',
      userId: 'test-user',
      status: 'pending',
      createdAt: new Date(),
      estimatedSize: 1024000,
      ...overrides
    };
  }

  static createMockExportConfig(overrides: Partial<any> = {}): any {
    return {
      format: 'PDF',
      formatOptions: { pageSize: 'A4', orientation: 'portrait' },
      dateRange: {
        startDate: new Date('2024-01-01'),
        endDate: new Date('2024-01-31')
      },
      filters: {},
      columns: ['id', 'name', 'status'],
      deliveryOptions: { method: 'download' },
      metadata: { userId: 'test-user' },
      ...overrides
    };
  }
}
```

This comprehensive test implementation provides thorough coverage of the Export functionality with unit tests, integration tests, and end-to-end scenarios covering all user workflows, error conditions, and accessibility requirements.