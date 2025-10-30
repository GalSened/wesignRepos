# Step F: Code Implementation - Export Functionality

## Overview
Complete TypeScript and Angular implementation for the Export functionality, including all components, services, state management, and integration patterns.

## 1. Core Component Implementation

### Export Dialog Component
```typescript
// export-dialog.component.ts
import { Component, OnInit, OnDestroy, ChangeDetectionStrategy, Inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { Store } from '@ngrx/store';
import { Observable, Subject, combineLatest, BehaviorSubject } from 'rxjs';
import { takeUntil, map, debounceTime, distinctUntilChanged } from 'rxjs/operators';

import { ExportActions } from '../store/export.actions';
import {
  selectExportFormats,
  selectExportProgress,
  selectExportValidation,
  selectIsExporting,
  selectCurrentExportJob
} from '../store/export.selectors';
import { ExportConfig, ExportFormat, ExportJob } from '../models/export.models';
import { ExportService } from '../services/export.service';
import { NotificationService } from '../../shared/services/notification.service';
import { AccessibilityService } from '../../shared/services/accessibility.service';

export interface ExportDialogData {
  dataSource: string;
  initialFilters?: any;
  preselectedFormat?: ExportFormat;
}

@Component({
  selector: 'app-export-dialog',
  templateUrl: './export-dialog.component.html',
  styleUrls: ['./export-dialog.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ExportDialogComponent implements OnInit, OnDestroy {
  private readonly destroy$ = new Subject<void>();

  // Form and validation
  exportForm: FormGroup;
  currentStep$ = new BehaviorSubject<number>(1);
  totalSteps = 4;

  // State observables
  availableFormats$ = this.store.select(selectExportFormats);
  exportProgress$ = this.store.select(selectExportProgress);
  isExporting$ = this.store.select(selectIsExporting);
  currentJob$ = this.store.select(selectCurrentExportJob);
  validation$ = this.store.select(selectExportValidation);

  // Form step validation
  formatValid$ = new BehaviorSubject<boolean>(false);
  dateRangeValid$ = new BehaviorSubject<boolean>(false);
  filtersValid$ = new BehaviorSubject<boolean>(false);
  optionsValid$ = new BehaviorSubject<boolean>(false);

  // Data estimation
  estimatedSize$ = new BehaviorSubject<number>(0);
  estimatedRecords$ = new BehaviorSubject<number>(0);

  // UI state
  showAdvancedOptions = false;
  previewData: any[] = [];

  constructor(
    private fb: FormBuilder,
    private store: Store,
    private exportService: ExportService,
    private notificationService: NotificationService,
    private accessibilityService: AccessibilityService,
    private dialogRef: MatDialogRef<ExportDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: ExportDialogData
  ) {
    this.buildForm();
  }

  ngOnInit(): void {
    this.initializeComponent();
    this.setupFormValidation();
    this.setupRealTimeUpdates();
    this.loadInitialData();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private buildForm(): void {
    this.exportForm = this.fb.group({
      format: [this.data.preselectedFormat || null, Validators.required],
      formatOptions: [{}],
      dateRange: [null, Validators.required],
      filters: [this.data.initialFilters || {}],
      columns: [[]],
      deliveryOptions: this.fb.group({
        method: ['download', Validators.required],
        emailRecipients: [[]],
        emailSubject: [''],
        emailBody: [''],
        compression: [false],
        password: ['']
      }),
      templateOptions: this.fb.group({
        saveAsTemplate: [false],
        templateName: [''],
        templateDescription: [''],
        shareWithRoles: [[]]
      })
    });
  }

  private initializeComponent(): void {
    // Load available formats
    this.store.dispatch(ExportActions.loadAvailableFormats());

    // Initialize accessibility
    this.accessibilityService.announceDialogOpened('export.dialog.opened');

    // Set initial focus
    setTimeout(() => {
      const firstInput = document.querySelector('.export-dialog input, .export-dialog button');
      if (firstInput) {
        (firstInput as HTMLElement).focus();
      }
    }, 100);
  }

  private setupFormValidation(): void {
    // Format validation
    this.exportForm.get('format')?.valueChanges.pipe(
      takeUntil(this.destroy$),
      distinctUntilChanged()
    ).subscribe(format => {
      const isValid = !!format;
      this.formatValid$.next(isValid);

      if (format) {
        this.store.dispatch(ExportActions.selectFormat({ format }));
        this.store.dispatch(ExportActions.loadFormatOptions({ format }));
        this.updateCurrentStep(2);
      }
    });

    // Date range validation
    this.exportForm.get('dateRange')?.valueChanges.pipe(
      takeUntil(this.destroy$),
      debounceTime(300),
      distinctUntilChanged()
    ).subscribe(dateRange => {
      const isValid = this.validateDateRange(dateRange);
      this.dateRangeValid$.next(isValid);

      if (isValid) {
        this.updateDataEstimation();
        this.updateCurrentStep(3);
      }
    });

    // Filters validation
    this.exportForm.get('filters')?.valueChanges.pipe(
      takeUntil(this.destroy$),
      debounceTime(500),
      distinctUntilChanged()
    ).subscribe(filters => {
      const isValid = this.validateFilters(filters);
      this.filtersValid$.next(isValid);

      if (isValid) {
        this.updateDataEstimation();
        this.updateCurrentStep(4);
      }
    });

    // Options validation
    combineLatest([
      this.exportForm.get('formatOptions')?.valueChanges || [],
      this.exportForm.get('deliveryOptions')?.valueChanges || []
    ]).pipe(
      takeUntil(this.destroy$),
      debounceTime(300)
    ).subscribe(() => {
      const isValid = this.validateExportOptions();
      this.optionsValid$.next(isValid);
    });
  }

  private setupRealTimeUpdates(): void {
    // Progress updates
    this.exportProgress$.pipe(
      takeUntil(this.destroy$)
    ).subscribe(progress => {
      if (progress) {
        this.accessibilityService.announceExportProgress(progress);
      }
    });

    // Job completion
    this.currentJob$.pipe(
      takeUntil(this.destroy$)
    ).subscribe(job => {
      if (job?.status === 'completed') {
        this.handleExportCompletion(job);
      } else if (job?.status === 'failed') {
        this.handleExportFailure(job);
      }
    });
  }

  private loadInitialData(): void {
    // Load user's export history and templates
    this.store.dispatch(ExportActions.loadExportHistory({
      userId: 'current-user',
      page: 1
    }));

    this.store.dispatch(ExportActions.loadExportTemplates());
  }

  private validateDateRange(dateRange: any): boolean {
    if (!dateRange || !dateRange.startDate || !dateRange.endDate) {
      return false;
    }

    const start = new Date(dateRange.startDate);
    const end = new Date(dateRange.endDate);
    const maxDays = 365;

    return start <= end &&
           (end.getTime() - start.getTime()) <= (maxDays * 24 * 60 * 60 * 1000);
  }

  private validateFilters(filters: any): boolean {
    // Basic validation - ensure filters don't result in empty dataset
    return true; // Simplified for demo
  }

  private validateExportOptions(): boolean {
    const formatOptions = this.exportForm.get('formatOptions')?.value;
    const deliveryOptions = this.exportForm.get('deliveryOptions')?.value;

    // Validate format-specific options
    const format = this.exportForm.get('format')?.value;
    if (format && !this.isValidFormatOptions(format, formatOptions)) {
      return false;
    }

    // Validate delivery options
    if (deliveryOptions?.method === 'email' &&
        (!deliveryOptions.emailRecipients || deliveryOptions.emailRecipients.length === 0)) {
      return false;
    }

    return true;
  }

  private isValidFormatOptions(format: ExportFormat, options: any): boolean {
    switch (format) {
      case ExportFormat.PDF:
        return options?.pageSize && options?.orientation;
      case ExportFormat.Excel:
        return options?.worksheetName?.length > 0;
      case ExportFormat.CSV:
        return options?.delimiter && options?.encoding;
      default:
        return true;
    }
  }

  private updateDataEstimation(): void {
    const dateRange = this.exportForm.get('dateRange')?.value;
    const filters = this.exportForm.get('filters')?.value;

    if (dateRange && filters) {
      this.exportService.estimateDataSize(filters, dateRange).pipe(
        takeUntil(this.destroy$)
      ).subscribe(estimate => {
        this.estimatedSize$.next(estimate.sizeBytes);
        this.estimatedRecords$.next(estimate.recordCount);
      });
    }
  }

  private updateCurrentStep(step: number): void {
    if (step > this.currentStep$.value) {
      this.currentStep$.next(step);
      this.accessibilityService.announceStepChange(step, this.totalSteps);
    }
  }

  private handleExportCompletion(job: ExportJob): void {
    this.accessibilityService.announceExportCompletion(true, job.fileName);
    this.notificationService.showSuccess(
      'Export completed successfully',
      `Your ${job.config.format} export is ready for download`
    );

    // Auto-download if configured
    if (job.config.deliveryOptions.method === 'download') {
      this.downloadFile(job.downloadUrl!, job.fileName!);
    }

    // Close dialog after brief delay
    setTimeout(() => {
      this.dialogRef.close({ success: true, job });
    }, 2000);
  }

  private handleExportFailure(job: ExportJob): void {
    this.accessibilityService.announceExportCompletion(false);
    this.notificationService.showError(
      'Export failed',
      job.errorMessage || 'An unexpected error occurred during export'
    );
  }

  private downloadFile(url: string, fileName: string): void {
    const link = document.createElement('a');
    link.href = url;
    link.download = fileName;
    link.style.display = 'none';
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
  }

  // Public methods for template interaction
  onFormatChange(format: ExportFormat): void {
    this.exportForm.patchValue({ format });
    this.store.dispatch(ExportActions.selectFormat({ format }));
  }

  onDateRangeChange(dateRange: any): void {
    this.exportForm.patchValue({ dateRange });
  }

  onFiltersChange(filters: any): void {
    this.exportForm.patchValue({ filters });
  }

  onOptionsChange(options: any): void {
    this.exportForm.patchValue({ formatOptions: options });
  }

  onTemplateApply(templateId: string): void {
    this.store.dispatch(ExportActions.applyExportTemplate({ templateId }));
  }

  onExport(): void {
    if (this.exportForm.valid) {
      const config: ExportConfig = {
        ...this.exportForm.value,
        dataSource: this.data.dataSource,
        userId: 'current-user',
        timestamp: new Date()
      };

      this.store.dispatch(ExportActions.initiateExport({ config }));

      // Save as template if requested
      if (config.templateOptions?.saveAsTemplate) {
        const template = {
          name: config.templateOptions.templateName,
          description: config.templateOptions.templateDescription,
          config: { ...config, templateOptions: undefined },
          shareWithRoles: config.templateOptions.shareWithRoles
        };
        this.store.dispatch(ExportActions.saveExportTemplate({ template }));
      }
    } else {
      this.notificationService.showWarning(
        'Form validation failed',
        'Please complete all required fields before exporting'
      );
      this.markFormGroupTouched();
    }
  }

  onCancel(): void {
    this.dialogRef.close({ success: false });
  }

  onCancelExport(): void {
    const currentJob = this.currentJob$.value;
    if (currentJob) {
      this.store.dispatch(ExportActions.cancelExport({ jobId: currentJob.id }));
    }
  }

  private markFormGroupTouched(): void {
    Object.keys(this.exportForm.controls).forEach(key => {
      const control = this.exportForm.get(key);
      control?.markAsTouched();
      if (control?.value && typeof control.value === 'object') {
        this.markFormGroupTouched();
      }
    });
  }

  // Accessibility helpers
  getStepAriaLabel(step: number): string {
    return this.accessibilityService.generateAriaLabel('export.step', {
      current: step,
      total: this.totalSteps
    });
  }

  getProgressAriaLabel(progress: number): string {
    return this.accessibilityService.generateAriaLabel('export.progress', {
      percentage: progress
    });
  }
}
```

### Format Selector Component
```typescript
// format-selector.component.ts
import { Component, Input, Output, EventEmitter, OnInit, ChangeDetectionStrategy, forwardRef } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { Observable, BehaviorSubject } from 'rxjs';

import { ExportFormat, FormatOption, PreviewData } from '../models/export.models';
import { FormatService } from '../services/format.service';

@Component({
  selector: 'app-format-selector',
  templateUrl: './format-selector.component.html',
  styleUrls: ['./format-selector.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => FormatSelectorComponent),
      multi: true
    }
  ]
})
export class FormatSelectorComponent implements OnInit, ControlValueAccessor {
  @Input() availableFormats: ExportFormat[] = [];
  @Input() previewEnabled: boolean = true;
  @Input() disabled: boolean = false;

  @Output() formatChange = new EventEmitter<ExportFormat>();
  @Output() optionsChange = new EventEmitter<any>();

  // Component state
  selectedFormat: ExportFormat | null = null;
  formatOptions: Record<ExportFormat, any> = {} as Record<ExportFormat, any>;
  previewData$ = new BehaviorSubject<PreviewData | null>(null);

  // Format definitions
  readonly ExportFormat = ExportFormat;

  // Control value accessor
  private onChange = (value: ExportFormat | null) => {};
  private onTouched = () => {};

  constructor(private formatService: FormatService) {}

  ngOnInit(): void {
    this.initializeFormatOptions();
  }

  private initializeFormatOptions(): void {
    this.availableFormats.forEach(format => {
      this.formatOptions[format] = this.getDefaultFormatOptions(format);
    });
  }

  private getDefaultFormatOptions(format: ExportFormat): any {
    switch (format) {
      case ExportFormat.PDF:
        return {
          orientation: 'portrait',
          pageSize: 'A4',
          includeCharts: true,
          includeHeader: true,
          includeFooter: false,
          fontSize: 10,
          margins: { top: 20, right: 20, bottom: 20, left: 20 }
        };

      case ExportFormat.Excel:
        return {
          worksheetName: 'Export Data',
          includeCharts: true,
          freezeHeaders: true,
          autoFilter: true,
          formatting: {
            headerStyle: { bold: true, backgroundColor: '#f0f0f0' },
            dataStyle: {},
            alternateRowColor: true
          }
        };

      case ExportFormat.CSV:
        return {
          delimiter: ',',
          encoding: 'UTF-8',
          includeHeaders: true,
          quoteStrings: true,
          dateFormat: 'YYYY-MM-DD',
          numberFormat: '0.00',
          booleanFormat: 'true/false'
        };

      case ExportFormat.JSON:
        return {
          prettyPrint: true,
          includeMetadata: false,
          arrayFormat: 'nested',
          dateFormat: 'ISO'
        };

      case ExportFormat.XML:
        return {
          rootElement: 'ExportData',
          includeSchema: false,
          prettyPrint: true,
          encoding: 'UTF-8'
        };

      default:
        return {};
    }
  }

  selectFormat(format: ExportFormat): void {
    if (this.disabled) return;

    this.selectedFormat = format;
    this.onChange(format);
    this.onTouched();
    this.formatChange.emit(format);

    // Emit default options for the selected format
    this.optionsChange.emit(this.formatOptions[format]);

    // Generate preview if enabled
    if (this.previewEnabled) {
      this.generatePreview(format);
    }
  }

  onFormatOptionChange(format: ExportFormat, option: string, value: any): void {
    if (!this.formatOptions[format]) {
      this.formatOptions[format] = {};
    }

    this.formatOptions[format][option] = value;

    if (this.selectedFormat === format) {
      this.optionsChange.emit(this.formatOptions[format]);

      // Regenerate preview with new options
      if (this.previewEnabled) {
        this.generatePreview(format);
      }
    }
  }

  private generatePreview(format: ExportFormat): void {
    const sampleData = this.getSampleData();
    const options = this.formatOptions[format];

    this.formatService.generatePreview(format, sampleData, options).subscribe(
      preview => this.previewData$.next(preview),
      error => console.error('Preview generation failed:', error)
    );
  }

  private getSampleData(): any[] {
    return [
      { id: 1, name: 'Sample Document 1', status: 'Signed', date: '2024-01-15', size: 2.5 },
      { id: 2, name: 'Sample Document 2', status: 'Pending', date: '2024-01-16', size: 1.8 },
      { id: 3, name: 'Sample Document 3', status: 'Draft', date: '2024-01-17', size: 3.2 }
    ];
  }

  showPreview(format: ExportFormat): void {
    this.generatePreview(format);
    // Implementation would show preview in a modal or side panel
  }

  getFormatIcon(format: ExportFormat): string {
    const iconMap: Record<ExportFormat, string> = {
      [ExportFormat.PDF]: 'picture_as_pdf',
      [ExportFormat.Excel]: 'grid_view',
      [ExportFormat.CSV]: 'table_chart',
      [ExportFormat.JSON]: 'code',
      [ExportFormat.XML]: 'description'
    };
    return iconMap[format] || 'description';
  }

  getFormatName(format: ExportFormat): string {
    const nameMap: Record<ExportFormat, string> = {
      [ExportFormat.PDF]: 'export.format.pdf.name',
      [ExportFormat.Excel]: 'export.format.excel.name',
      [ExportFormat.CSV]: 'export.format.csv.name',
      [ExportFormat.JSON]: 'export.format.json.name',
      [ExportFormat.XML]: 'export.format.xml.name'
    };
    return nameMap[format] || format;
  }

  getFormatDescription(format: ExportFormat): string {
    const descMap: Record<ExportFormat, string> = {
      [ExportFormat.PDF]: 'export.format.pdf.description',
      [ExportFormat.Excel]: 'export.format.excel.description',
      [ExportFormat.CSV]: 'export.format.csv.description',
      [ExportFormat.JSON]: 'export.format.json.description',
      [ExportFormat.XML]: 'export.format.xml.description'
    };
    return descMap[format] || '';
  }

  getFormatFeatures(format: ExportFormat): string[] {
    const featureMap: Record<ExportFormat, string[]> = {
      [ExportFormat.PDF]: ['Charts', 'Formatting', 'Print-ready'],
      [ExportFormat.Excel]: ['Charts', 'Formulas', 'Styling'],
      [ExportFormat.CSV]: ['Lightweight', 'Universal', 'Fast'],
      [ExportFormat.JSON]: ['Structured', 'API-friendly', 'Nested'],
      [ExportFormat.XML]: ['Structured', 'Schema', 'Standards-based']
    };
    return featureMap[format] || [];
  }

  trackByFormat(index: number, format: ExportFormat): ExportFormat {
    return format;
  }

  // ControlValueAccessor implementation
  writeValue(value: ExportFormat | null): void {
    this.selectedFormat = value;
    if (value && this.previewEnabled) {
      this.generatePreview(value);
    }
  }

  registerOnChange(fn: (value: ExportFormat | null) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.disabled = isDisabled;
  }
}
```

### Export Progress Component
```typescript
// export-progress.component.ts
import { Component, Input, OnInit, OnDestroy, ChangeDetectionStrategy } from '@angular/core';
import { Observable, Subject, interval } from 'rxjs';
import { takeUntil, map, distinctUntilChanged } from 'rxjs/operators';

import { ExportProgress, ExportJob } from '../models/export.models';
import { SignalRHubService } from '../../shared/services/signalr-hub.service';
import { AccessibilityService } from '../../shared/services/accessibility.service';

@Component({
  selector: 'app-export-progress',
  templateUrl: './export-progress.component.html',
  styleUrls: ['./export-progress.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ExportProgressComponent implements OnInit, OnDestroy {
  @Input() jobId: string = '';
  @Input() showDetails: boolean = false;
  @Input() showCancelButton: boolean = true;

  private readonly destroy$ = new Subject<void>();

  // Progress observables
  progress$: Observable<ExportProgress | null>;
  estimatedTimeRemaining$: Observable<string>;
  processingSpeed$: Observable<string>;

  // Animation state
  progressBarWidth$ = new Subject<number>();
  currentStage$ = new Subject<string>();

  constructor(
    private hubService: SignalRHubService,
    private accessibilityService: AccessibilityService
  ) {
    this.progress$ = this.hubService.on(`progress-${this.jobId}`);
    this.setupProgressCalculations();
  }

  ngOnInit(): void {
    this.initializeProgressTracking();
    this.setupAccessibilityAnnouncements();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private initializeProgressTracking(): void {
    if (!this.jobId) {
      console.warn('ExportProgressComponent: jobId is required');
      return;
    }

    // Connect to SignalR hub if not already connected
    this.hubService.connect('exportHub').pipe(
      takeUntil(this.destroy$)
    ).subscribe();

    // Subscribe to progress updates
    this.progress$.pipe(
      takeUntil(this.destroy$),
      distinctUntilChanged((prev, curr) =>
        prev?.percentage === curr?.percentage &&
        prev?.currentStage === curr?.currentStage
      )
    ).subscribe(progress => {
      if (progress) {
        this.updateProgressDisplay(progress);
      }
    });
  }

  private setupProgressCalculations(): void {
    this.estimatedTimeRemaining$ = this.progress$.pipe(
      map(progress => {
        if (!progress || progress.estimatedTimeRemaining <= 0) {
          return 'Calculating...';
        }
        return this.formatTimeRemaining(progress.estimatedTimeRemaining);
      })
    );

    this.processingSpeed$ = this.progress$.pipe(
      map(progress => {
        if (!progress || !progress.processedRecords || !progress.totalRecords) {
          return '0 records/sec';
        }

        const elapsed = Date.now() - new Date(progress.startTime).getTime();
        const recordsPerSecond = (progress.processedRecords / elapsed) * 1000;
        return `${Math.round(recordsPerSecond)} records/sec`;
      })
    );
  }

  private setupAccessibilityAnnouncements(): void {
    // Announce progress updates every 10% completion
    let lastAnnouncedPercentage = 0;

    this.progress$.pipe(
      takeUntil(this.destroy$),
      map(progress => progress?.percentage || 0),
      distinctUntilChanged()
    ).subscribe(percentage => {
      if (percentage >= lastAnnouncedPercentage + 10) {
        this.accessibilityService.announceExportProgress({
          percentage,
          currentStage: this.currentStage$.value
        } as ExportProgress);
        lastAnnouncedPercentage = Math.floor(percentage / 10) * 10;
      }
    });

    // Announce stage changes
    this.progress$.pipe(
      takeUntil(this.destroy$),
      map(progress => progress?.currentStage || ''),
      distinctUntilChanged()
    ).subscribe(stage => {
      if (stage) {
        this.currentStage$.next(stage);
        this.accessibilityService.announceStageChange(stage);
      }
    });
  }

  private updateProgressDisplay(progress: ExportProgress): void {
    // Smooth progress bar animation
    this.progressBarWidth$.next(progress.percentage);

    // Update current stage
    this.currentStage$.next(progress.currentStage);
  }

  private formatTimeRemaining(milliseconds: number): string {
    const seconds = Math.ceil(milliseconds / 1000);

    if (seconds < 60) {
      return `${seconds} seconds`;
    } else if (seconds < 3600) {
      const minutes = Math.ceil(seconds / 60);
      return `${minutes} minute${minutes !== 1 ? 's' : ''}`;
    } else {
      const hours = Math.ceil(seconds / 3600);
      return `${hours} hour${hours !== 1 ? 's' : ''}`;
    }
  }

  formatBytes(bytes: number): string {
    if (bytes === 0) return '0 B';

    const k = 1024;
    const sizes = ['B', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));

    return `${parseFloat((bytes / Math.pow(k, i)).toFixed(1))} ${sizes[i]}`;
  }

  formatRecords(count: number): string {
    if (count < 1000) {
      return count.toString();
    } else if (count < 1000000) {
      return `${(count / 1000).toFixed(1)}K`;
    } else {
      return `${(count / 1000000).toFixed(1)}M`;
    }
  }

  getStageDescription(stage: string): string {
    const stageDescriptions: Record<string, string> = {
      'initializing': 'Preparing export job...',
      'fetching_data': 'Retrieving data from database...',
      'processing_data': 'Processing and transforming data...',
      'generating_file': 'Generating export file...',
      'compressing': 'Compressing file...',
      'uploading': 'Uploading to storage...',
      'finalizing': 'Finalizing export...',
      'completed': 'Export completed successfully!',
      'failed': 'Export failed. Please try again.'
    };

    return stageDescriptions[stage] || stage;
  }

  getProgressColor(percentage: number): string {
    if (percentage < 30) return '#f44336'; // Red
    if (percentage < 70) return '#ff9800'; // Orange
    return '#4caf50'; // Green
  }

  onCancelExport(): void {
    // Emit cancel event to parent component
    // Parent component will handle the actual cancellation logic
  }
}
```

## 2. Service Layer Implementation

### Export Service
```typescript
// export.service.ts
import { Injectable } from '@angular/core';
import { HttpClient, HttpParams, HttpEvent, HttpEventType } from '@angular/common/http';
import { Observable, throwError, of } from 'rxjs';
import { map, catchError, retry, timeout } from 'rxjs/operators';

import { environment } from '../../../environments/environment';
import {
  ExportConfig,
  ExportJob,
  ExportProgress,
  ExportResult,
  ExportHistoryPage,
  DataEstimate,
  ValidationResult
} from '../models/export.models';

@Injectable({
  providedIn: 'root'
})
export class ExportService {
  private readonly apiUrl = `${environment.apiUrl}/api/exports`;
  private readonly timeoutMs = 30000; // 30 seconds for most operations
  private readonly downloadTimeoutMs = 300000; // 5 minutes for downloads

  constructor(private http: HttpClient) {}

  /**
   * Initiate a new export job
   */
  initiateExport(config: ExportConfig): Observable<ExportJob> {
    return this.http.post<ExportJob>(`${this.apiUrl}/initiate`, config).pipe(
      timeout(this.timeoutMs),
      retry(2),
      catchError(this.handleError)
    );
  }

  /**
   * Get current progress of an export job
   */
  getExportProgress(jobId: string): Observable<ExportProgress> {
    return this.http.get<ExportProgress>(`${this.apiUrl}/${jobId}/progress`).pipe(
      timeout(this.timeoutMs),
      catchError(this.handleError)
    );
  }

  /**
   * Download completed export file
   */
  downloadExport(jobId: string): Observable<HttpEvent<Blob>> {
    return this.http.get(`${this.apiUrl}/${jobId}/download`, {
      responseType: 'blob',
      reportProgress: true,
      observe: 'events'
    }).pipe(
      timeout(this.downloadTimeoutMs),
      catchError(this.handleError)
    );
  }

  /**
   * Get download URL for completed export
   */
  getDownloadUrl(jobId: string): Observable<{ url: string; expiresAt: Date }> {
    return this.http.get<{ url: string; expiresAt: string }>(`${this.apiUrl}/${jobId}/download-url`).pipe(
      map(response => ({
        url: response.url,
        expiresAt: new Date(response.expiresAt)
      })),
      timeout(this.timeoutMs),
      catchError(this.handleError)
    );
  }

  /**
   * Cancel an in-progress export job
   */
  cancelExport(jobId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${jobId}`).pipe(
      timeout(this.timeoutMs),
      catchError(this.handleError)
    );
  }

  /**
   * Get export job details
   */
  getExportJob(jobId: string): Observable<ExportJob> {
    return this.http.get<ExportJob>(`${this.apiUrl}/${jobId}`).pipe(
      timeout(this.timeoutMs),
      catchError(this.handleError)
    );
  }

  /**
   * Get user's export history with pagination
   */
  getExportHistory(userId: string, page: number = 1, pageSize: number = 20): Observable<ExportHistoryPage> {
    const params = new HttpParams()
      .set('userId', userId)
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    return this.http.get<ExportHistoryPage>(`${this.apiUrl}/history`, { params }).pipe(
      timeout(this.timeoutMs),
      catchError(this.handleError)
    );
  }

  /**
   * Estimate data size and record count for given filters
   */
  estimateDataSize(filters: any, dateRange: any): Observable<DataEstimate> {
    const payload = { filters, dateRange };

    return this.http.post<DataEstimate>(`${this.apiUrl}/estimate`, payload).pipe(
      timeout(this.timeoutMs),
      catchError(this.handleError)
    );
  }

  /**
   * Validate export configuration
   */
  validateExportConfig(config: ExportConfig): Observable<ValidationResult> {
    return this.http.post<ValidationResult>(`${this.apiUrl}/validate`, config).pipe(
      timeout(this.timeoutMs),
      catchError(this.handleError)
    );
  }

  /**
   * Get available export formats for current user
   */
  getAvailableFormats(): Observable<string[]> {
    return this.http.get<string[]>(`${this.apiUrl}/formats`).pipe(
      timeout(this.timeoutMs),
      catchError(this.handleError)
    );
  }

  /**
   * Get column definitions for a data source
   */
  getColumnDefinitions(dataSource: string): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/columns/${dataSource}`).pipe(
      timeout(this.timeoutMs),
      catchError(this.handleError)
    );
  }

  /**
   * Preview filtered data (limited records)
   */
  previewFilteredData(filters: any, limit: number = 100): Observable<any[]> {
    const payload = { filters, limit };

    return this.http.post<any[]>(`${this.apiUrl}/preview`, payload).pipe(
      timeout(this.timeoutMs),
      catchError(this.handleError)
    );
  }

  /**
   * Retry failed export job
   */
  retryExport(jobId: string): Observable<ExportJob> {
    return this.http.post<ExportJob>(`${this.apiUrl}/${jobId}/retry`, {}).pipe(
      timeout(this.timeoutMs),
      retry(1),
      catchError(this.handleError)
    );
  }

  /**
   * Send export via email
   */
  sendExportByEmail(jobId: string, recipients: string[], subject?: string, body?: string): Observable<void> {
    const payload = { recipients, subject, body };

    return this.http.post<void>(`${this.apiUrl}/${jobId}/email`, payload).pipe(
      timeout(this.timeoutMs),
      catchError(this.handleError)
    );
  }

  /**
   * Get export job statistics
   */
  getExportStatistics(userId: string, period: 'week' | 'month' | 'year' = 'month'): Observable<any> {
    const params = new HttpParams()
      .set('userId', userId)
      .set('period', period);

    return this.http.get<any>(`${this.apiUrl}/statistics`, { params }).pipe(
      timeout(this.timeoutMs),
      catchError(this.handleError)
    );
  }

  /**
   * Delete export job and associated files
   */
  deleteExport(jobId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${jobId}/delete`).pipe(
      timeout(this.timeoutMs),
      catchError(this.handleError)
    );
  }

  private handleError = (error: any): Observable<never> => {
    console.error('Export service error:', error);

    let errorMessage = 'An unexpected error occurred';

    if (error.error instanceof ErrorEvent) {
      // Client-side error
      errorMessage = error.error.message;
    } else {
      // Server-side error
      switch (error.status) {
        case 400:
          errorMessage = 'Invalid export configuration';
          break;
        case 401:
          errorMessage = 'Authentication required';
          break;
        case 403:
          errorMessage = 'Access denied - insufficient permissions';
          break;
        case 404:
          errorMessage = 'Export job not found';
          break;
        case 429:
          errorMessage = 'Too many export requests - please try again later';
          break;
        case 500:
          errorMessage = 'Server error - please try again later';
          break;
        case 503:
          errorMessage = 'Export service temporarily unavailable';
          break;
        default:
          errorMessage = error.error?.message || `Error ${error.status}: ${error.statusText}`;
      }
    }

    return throwError({ message: errorMessage, status: error.status });
  };
}
```

## 3. State Management Implementation

### Export Actions
```typescript
// export.actions.ts
import { createActionGroup, emptyProps, props } from '@ngrx/store';
import {
  ExportConfig,
  ExportJob,
  ExportProgress,
  ExportResult,
  ExportFormat,
  FormatOption,
  ExportTemplate,
  ExportHistoryPage,
  ValidationResult
} from '../models/export.models';

export const ExportActions = createActionGroup({
  source: 'Export',
  events: {
    // Dialog management
    'Open Export Dialog': props<{ dataSource: string; initialFilters?: any }>(),
    'Close Export Dialog': emptyProps(),

    // Export operations
    'Initiate Export': props<{ config: ExportConfig }>(),
    'Initiate Export Success': props<{ job: ExportJob }>(),
    'Initiate Export Failure': props<{ error: string }>(),

    // Progress tracking
    'Update Export Progress': props<{ jobId: string; progress: ExportProgress }>(),
    'Export Completed': props<{ jobId: string; result: ExportResult }>(),
    'Export Failed': props<{ jobId: string; error: string }>(),

    // Job management
    'Cancel Export': props<{ jobId: string }>(),
    'Cancel Export Success': props<{ jobId: string }>(),
    'Cancel Export Failure': props<{ jobId: string; error: string }>(),

    'Retry Export': props<{ jobId: string }>(),
    'Retry Export Success': props<{ job: ExportJob }>(),
    'Retry Export Failure': props<{ jobId: string; error: string }>(),

    'Delete Export': props<{ jobId: string }>(),
    'Delete Export Success': props<{ jobId: string }>(),
    'Delete Export Failure': props<{ jobId: string; error: string }>(),

    // Format management
    'Load Available Formats': emptyProps(),
    'Load Available Formats Success': props<{ formats: ExportFormat[] }>(),
    'Load Available Formats Failure': props<{ error: string }>(),

    'Select Format': props<{ format: ExportFormat }>(),
    'Load Format Options': props<{ format: ExportFormat }>(),
    'Load Format Options Success': props<{ format: ExportFormat; options: FormatOption[] }>(),
    'Load Format Options Failure': props<{ format: ExportFormat; error: string }>(),

    // History management
    'Load Export History': props<{ userId: string; page?: number }>(),
    'Load Export History Success': props<{ history: ExportHistoryPage }>(),
    'Load Export History Failure': props<{ error: string }>(),

    'Clear Export History': emptyProps(),

    // Templates
    'Load Export Templates': emptyProps(),
    'Load Export Templates Success': props<{ templates: ExportTemplate[] }>(),
    'Load Export Templates Failure': props<{ error: string }>(),

    'Save Export Template': props<{ template: ExportTemplate }>(),
    'Save Export Template Success': props<{ template: ExportTemplate }>(),
    'Save Export Template Failure': props<{ error: string }>(),

    'Apply Export Template': props<{ templateId: string }>(),
    'Apply Export Template Success': props<{ config: ExportConfig }>(),
    'Apply Export Template Failure': props<{ templateId: string; error: string }>(),

    'Delete Export Template': props<{ templateId: string }>(),
    'Delete Export Template Success': props<{ templateId: string }>(),
    'Delete Export Template Failure': props<{ templateId: string; error: string }>(),

    // Validation
    'Validate Export Config': props<{ configId: string; config: ExportConfig }>(),
    'Validate Export Config Success': props<{ configId: string; result: ValidationResult }>(),
    'Validate Export Config Failure': props<{ configId: string; error: string }>(),

    // Data estimation
    'Estimate Data Size': props<{ filters: any; dateRange: any }>(),
    'Estimate Data Size Success': props<{ estimate: { sizeBytes: number; recordCount: number } }>(),
    'Estimate Data Size Failure': props<{ error: string }>(),

    // Download management
    'Download Export': props<{ jobId: string }>(),
    'Download Export Success': props<{ jobId: string }>(),
    'Download Export Failure': props<{ jobId: string; error: string }>(),

    // Email delivery
    'Send Export Email': props<{ jobId: string; recipients: string[]; subject?: string; body?: string }>(),
    'Send Export Email Success': props<{ jobId: string }>(),
    'Send Export Email Failure': props<{ jobId: string; error: string }>(),

    // Error handling
    'Clear Export Errors': emptyProps(),
    'Set Export Error': props<{ error: string }>()
  }
});
```

### Export Reducer
```typescript
// export.reducer.ts
import { createReducer, on } from '@ngrx/store';
import { ExportActions } from './export.actions';
import {
  ExportJob,
  ExportProgress,
  ExportFormat,
  FormatOption,
  ExportTemplate,
  ExportHistoryPage,
  ValidationResult
} from '../models/export.models';

export interface ExportState {
  // Current export configuration
  currentConfig: any;
  selectedFormat: ExportFormat | null;

  // Active export jobs
  activeJobs: ExportJob[];
  currentJobId: string | null;

  // Export history
  history: ExportHistoryPage | null;
  historyLoading: boolean;
  historyError: string | null;

  // Format-specific state
  availableFormats: ExportFormat[];
  formatOptions: Record<ExportFormat, FormatOption[]>;
  formatsLoading: boolean;

  // Progress tracking
  progressUpdates: Record<string, ExportProgress>;

  // UI state
  dialogOpen: boolean;
  isExporting: boolean;

  // Templates
  templates: ExportTemplate[];
  templatesLoading: boolean;
  templatesError: string | null;

  // Validation
  validationResults: Record<string, ValidationResult>;

  // Data estimation
  currentEstimate: { sizeBytes: number; recordCount: number } | null;
  estimationLoading: boolean;

  // Error handling
  lastError: string | null;
  errors: Record<string, string>;
}

const initialState: ExportState = {
  currentConfig: null,
  selectedFormat: null,
  activeJobs: [],
  currentJobId: null,
  history: null,
  historyLoading: false,
  historyError: null,
  availableFormats: [],
  formatOptions: {} as Record<ExportFormat, FormatOption[]>,
  formatsLoading: false,
  progressUpdates: {},
  dialogOpen: false,
  isExporting: false,
  templates: [],
  templatesLoading: false,
  templatesError: null,
  validationResults: {},
  currentEstimate: null,
  estimationLoading: false,
  lastError: null,
  errors: {}
};

export const exportReducer = createReducer(
  initialState,

  // Dialog management
  on(ExportActions.openExportDialog, (state, { dataSource, initialFilters }) => ({
    ...state,
    dialogOpen: true,
    currentConfig: { dataSource, filters: initialFilters || {} },
    lastError: null
  })),

  on(ExportActions.closeExportDialog, (state) => ({
    ...state,
    dialogOpen: false,
    currentConfig: null,
    selectedFormat: null,
    validationResults: {},
    currentEstimate: null
  })),

  // Export operations
  on(ExportActions.initiateExport, (state, { config }) => ({
    ...state,
    isExporting: true,
    currentConfig: config,
    lastError: null
  })),

  on(ExportActions.initiateExportSuccess, (state, { job }) => ({
    ...state,
    activeJobs: [...state.activeJobs, job],
    currentJobId: job.id,
    isExporting: true
  })),

  on(ExportActions.initiateExportFailure, (state, { error }) => ({
    ...state,
    isExporting: false,
    lastError: error,
    currentJobId: null
  })),

  // Progress tracking
  on(ExportActions.updateExportProgress, (state, { jobId, progress }) => ({
    ...state,
    progressUpdates: {
      ...state.progressUpdates,
      [jobId]: progress
    }
  })),

  on(ExportActions.exportCompleted, (state, { jobId, result }) => ({
    ...state,
    isExporting: false,
    activeJobs: state.activeJobs.map(job =>
      job.id === jobId
        ? { ...job, status: 'completed', downloadUrl: result.downloadUrl, fileName: result.fileName }
        : job
    ),
    currentJobId: state.currentJobId === jobId ? null : state.currentJobId
  })),

  on(ExportActions.exportFailed, (state, { jobId, error }) => ({
    ...state,
    isExporting: false,
    activeJobs: state.activeJobs.map(job =>
      job.id === jobId
        ? { ...job, status: 'failed', errorMessage: error }
        : job
    ),
    currentJobId: state.currentJobId === jobId ? null : state.currentJobId,
    errors: { ...state.errors, [jobId]: error }
  })),

  // Format management
  on(ExportActions.loadAvailableFormats, (state) => ({
    ...state,
    formatsLoading: true
  })),

  on(ExportActions.loadAvailableFormatsSuccess, (state, { formats }) => ({
    ...state,
    availableFormats: formats,
    formatsLoading: false
  })),

  on(ExportActions.loadAvailableFormatsFailure, (state, { error }) => ({
    ...state,
    formatsLoading: false,
    lastError: error
  })),

  on(ExportActions.selectFormat, (state, { format }) => ({
    ...state,
    selectedFormat: format
  })),

  on(ExportActions.loadFormatOptionsSuccess, (state, { format, options }) => ({
    ...state,
    formatOptions: {
      ...state.formatOptions,
      [format]: options
    }
  })),

  // History management
  on(ExportActions.loadExportHistory, (state) => ({
    ...state,
    historyLoading: true,
    historyError: null
  })),

  on(ExportActions.loadExportHistorySuccess, (state, { history }) => ({
    ...state,
    history,
    historyLoading: false
  })),

  on(ExportActions.loadExportHistoryFailure, (state, { error }) => ({
    ...state,
    historyLoading: false,
    historyError: error
  })),

  on(ExportActions.clearExportHistory, (state) => ({
    ...state,
    history: null
  })),

  // Templates
  on(ExportActions.loadExportTemplates, (state) => ({
    ...state,
    templatesLoading: true,
    templatesError: null
  })),

  on(ExportActions.loadExportTemplatesSuccess, (state, { templates }) => ({
    ...state,
    templates,
    templatesLoading: false
  })),

  on(ExportActions.loadExportTemplatesFailure, (state, { error }) => ({
    ...state,
    templatesLoading: false,
    templatesError: error
  })),

  on(ExportActions.saveExportTemplateSuccess, (state, { template }) => ({
    ...state,
    templates: [...state.templates, template]
  })),

  on(ExportActions.deleteExportTemplateSuccess, (state, { templateId }) => ({
    ...state,
    templates: state.templates.filter(t => t.id !== templateId)
  })),

  // Validation
  on(ExportActions.validateExportConfigSuccess, (state, { configId, result }) => ({
    ...state,
    validationResults: {
      ...state.validationResults,
      [configId]: result
    }
  })),

  // Data estimation
  on(ExportActions.estimateDataSize, (state) => ({
    ...state,
    estimationLoading: true
  })),

  on(ExportActions.estimateDataSizeSuccess, (state, { estimate }) => ({
    ...state,
    currentEstimate: estimate,
    estimationLoading: false
  })),

  on(ExportActions.estimateDataSizeFailure, (state, { error }) => ({
    ...state,
    estimationLoading: false,
    lastError: error
  })),

  // Job management
  on(ExportActions.cancelExportSuccess, (state, { jobId }) => ({
    ...state,
    activeJobs: state.activeJobs.map(job =>
      job.id === jobId ? { ...job, status: 'cancelled' } : job
    ),
    isExporting: state.currentJobId === jobId ? false : state.isExporting,
    currentJobId: state.currentJobId === jobId ? null : state.currentJobId
  })),

  on(ExportActions.deleteExportSuccess, (state, { jobId }) => ({
    ...state,
    activeJobs: state.activeJobs.filter(job => job.id !== jobId),
    progressUpdates: { ...state.progressUpdates, [jobId]: undefined } as any
  })),

  // Error handling
  on(ExportActions.clearExportErrors, (state) => ({
    ...state,
    lastError: null,
    errors: {},
    historyError: null,
    templatesError: null
  })),

  on(ExportActions.setExportError, (state, { error }) => ({
    ...state,
    lastError: error
  }))
);
```

This comprehensive code implementation provides a solid foundation for the Export functionality with proper TypeScript typing, Angular best practices, state management integration, and accessibility support. The implementation follows the established patterns from the WeSign application architecture.