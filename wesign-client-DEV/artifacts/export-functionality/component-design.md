# Step D: Component Design - Export Functionality

## Overview
Detailed technical component design for the Export functionality including architecture patterns, interfaces, and implementation specifications.

## Component Architecture

### 1. Export Dialog Component
```typescript
@Component({
  selector: 'app-export-dialog',
  templateUrl: './export-dialog.component.html',
  styleUrls: ['./export-dialog.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ExportDialogComponent implements OnInit, OnDestroy {
  // State management
  @Input() isOpen: boolean = false;
  @Output() onClose = new EventEmitter<void>();
  @Output() onExport = new EventEmitter<ExportConfig>();

  // Form and validation
  exportForm: FormGroup;
  validation$ = new BehaviorSubject<ValidationResult>({});

  // Real-time updates
  progress$ = this.store.select(selectExportProgress);
  status$ = this.store.select(selectExportStatus);
}
```

### 2. Format Selector Component
```typescript
@Component({
  selector: 'app-format-selector',
  templateUrl: './format-selector.component.html',
  styleUrls: ['./format-selector.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class FormatSelectorComponent implements ControlValueAccessor {
  @Input() availableFormats: ExportFormat[] = [];
  @Input() previewEnabled: boolean = true;
  @Output() formatChange = new EventEmitter<ExportFormat>();

  selectedFormat: ExportFormat | null = null;
  formatOptions: Record<string, any> = {};
  previewData$ = new BehaviorSubject<PreviewData | null>(null);
}
```

### 3. Data Range Picker Component
```typescript
@Component({
  selector: 'app-data-range-picker',
  templateUrl: './data-range-picker.component.html',
  styleUrls: ['./data-range-picker.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class DataRangePickerComponent implements ControlValueAccessor, OnInit {
  @Input() presets: DateRangePreset[] = DEFAULT_PRESETS;
  @Input() maxRange: number = 365; // days
  @Output() rangeChange = new EventEmitter<DateRange>();

  dateForm: FormGroup;
  estimatedSize$ = new BehaviorSubject<number>(0);
  recordCount$ = new BehaviorSubject<number>(0);
}
```

### 4. Filter Configuration Component
```typescript
@Component({
  selector: 'app-filter-config',
  templateUrl: './filter-config.component.html',
  styleUrls: ['./filter-config.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class FilterConfigComponent implements OnInit, OnDestroy {
  @Input() availableColumns: ColumnDefinition[] = [];
  @Input() savedFilters: SavedFilter[] = [];
  @Output() filtersChange = new EventEmitter<FilterConfig>();

  filterForm: FormGroup;
  selectedColumns: ColumnDefinition[] = [];
  advancedFilters: FilterCondition[] = [];
  previewResults$ = new BehaviorSubject<FilterPreview | null>(null);
}
```

### 5. Export Progress Component
```typescript
@Component({
  selector: 'app-export-progress',
  templateUrl: './export-progress.component.html',
  styleUrls: ['./export-progress.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ExportProgressComponent implements OnInit, OnDestroy {
  @Input() jobId: string = '';
  @Input() showDetails: boolean = false;

  progress$ = this.hubConnection.on('progress');
  status$ = this.hubConnection.on('status');
  estimatedTime$ = new BehaviorSubject<number>(0);
  currentStage$ = new BehaviorSubject<string>('');
}
```

## Service Layer Design

### 1. Export Service
```typescript
@Injectable({
  providedIn: 'root'
})
export class ExportService {
  private readonly apiUrl = `${environment.apiUrl}/exports`;

  // Core export operations
  initiateExport(config: ExportConfig): Observable<ExportJob> {
    return this.http.post<ExportJob>(`${this.apiUrl}/initiate`, config);
  }

  getExportProgress(jobId: string): Observable<ExportProgress> {
    return this.http.get<ExportProgress>(`${this.apiUrl}/${jobId}/progress`);
  }

  downloadExport(jobId: string): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/${jobId}/download`, {
      responseType: 'blob',
      reportProgress: true,
      observe: 'events'
    });
  }

  cancelExport(jobId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${jobId}`);
  }

  getExportHistory(userId: string, page: number = 1): Observable<ExportHistoryPage> {
    return this.http.get<ExportHistoryPage>(`${this.apiUrl}/history`, {
      params: { userId, page: page.toString() }
    });
  }
}
```

### 2. Format Service
```typescript
@Injectable({
  providedIn: 'root'
})
export class FormatService {
  private readonly formats: Map<ExportFormat, FormatHandler> = new Map([
    [ExportFormat.PDF, new PdfFormatHandler()],
    [ExportFormat.Excel, new ExcelFormatHandler()],
    [ExportFormat.CSV, new CsvFormatHandler()],
    [ExportFormat.JSON, new JsonFormatHandler()],
    [ExportFormat.XML, new XmlFormatHandler()]
  ]);

  getAvailableFormats(): ExportFormat[] {
    return Array.from(this.formats.keys());
  }

  getFormatOptions(format: ExportFormat): Observable<FormatOption[]> {
    const handler = this.formats.get(format);
    return handler ? handler.getOptions() : of([]);
  }

  validateFormatConfig(format: ExportFormat, config: any): ValidationResult {
    const handler = this.formats.get(format);
    return handler ? handler.validate(config) : { isValid: false, errors: ['Unsupported format'] };
  }

  generatePreview(format: ExportFormat, data: any[], config: any): Observable<PreviewData> {
    const handler = this.formats.get(format);
    return handler ? handler.generatePreview(data, config) : throwError('Unsupported format');
  }
}
```

### 3. Data Service
```typescript
@Injectable({
  providedIn: 'root'
})
export class ExportDataService {
  constructor(
    private http: HttpClient,
    private analytics: AnalyticsService
  ) {}

  estimateDataSize(filters: FilterConfig, dateRange: DateRange): Observable<DataEstimate> {
    return this.http.post<DataEstimate>(`${this.apiUrl}/estimate`, {
      filters,
      dateRange
    });
  }

  validateFilters(filters: FilterConfig): Observable<ValidationResult> {
    return this.http.post<ValidationResult>(`${this.apiUrl}/validate-filters`, filters);
  }

  getColumnDefinitions(dataSource: string): Observable<ColumnDefinition[]> {
    return this.http.get<ColumnDefinition[]>(`${this.apiUrl}/columns/${dataSource}`);
  }

  previewFilteredData(filters: FilterConfig, limit: number = 100): Observable<any[]> {
    return this.http.post<any[]>(`${this.apiUrl}/preview`, {
      filters,
      limit
    });
  }
}
```

## State Management Design

### 1. Export State Interface
```typescript
export interface ExportState {
  // Current export configuration
  currentConfig: ExportConfig | null;

  // Active export jobs
  activeJobs: ExportJob[];

  // Export history
  history: ExportHistoryItem[];
  historyLoading: boolean;
  historyError: string | null;

  // Format-specific state
  availableFormats: ExportFormat[];
  formatOptions: Record<ExportFormat, FormatOption[]>;

  // Progress tracking
  progressUpdates: Record<string, ExportProgress>;

  // UI state
  dialogOpen: boolean;
  selectedFormat: ExportFormat | null;

  // Templates
  templates: ExportTemplate[];
  templatesLoading: boolean;

  // Validation
  validationResults: Record<string, ValidationResult>;
}
```

### 2. Export Actions
```typescript
export const ExportActions = createActionGroup({
  source: 'Export',
  events: {
    // Dialog management
    'Open Export Dialog': props<{ dataSource: string }>(),
    'Close Export Dialog': emptyProps(),

    // Export operations
    'Initiate Export': props<{ config: ExportConfig }>(),
    'Initiate Export Success': props<{ job: ExportJob }>(),
    'Initiate Export Failure': props<{ error: string }>(),

    // Progress tracking
    'Update Export Progress': props<{ jobId: string; progress: ExportProgress }>(),
    'Export Completed': props<{ jobId: string; result: ExportResult }>(),
    'Export Failed': props<{ jobId: string; error: string }>(),

    // Format management
    'Load Available Formats': emptyProps(),
    'Load Available Formats Success': props<{ formats: ExportFormat[] }>(),
    'Select Format': props<{ format: ExportFormat }>(),
    'Load Format Options': props<{ format: ExportFormat }>(),
    'Load Format Options Success': props<{ format: ExportFormat; options: FormatOption[] }>(),

    // History management
    'Load Export History': props<{ userId: string; page?: number }>(),
    'Load Export History Success': props<{ history: ExportHistoryPage }>(),
    'Load Export History Failure': props<{ error: string }>(),

    // Templates
    'Load Export Templates': emptyProps(),
    'Load Export Templates Success': props<{ templates: ExportTemplate[] }>(),
    'Save Export Template': props<{ template: ExportTemplate }>(),
    'Apply Export Template': props<{ templateId: string }>(),

    // Validation
    'Validate Export Config': props<{ config: ExportConfig }>(),
    'Validate Export Config Success': props<{ configId: string; result: ValidationResult }>(),
    'Validate Export Config Failure': props<{ configId: string; error: string }>()
  }
});
```

### 3. Export Effects
```typescript
@Injectable()
export class ExportEffects {
  constructor(
    private actions$: Actions,
    private exportService: ExportService,
    private formatService: FormatService,
    private notificationService: NotificationService,
    private hubService: SignalRHubService
  ) {}

  initiateExport$ = createEffect(() =>
    this.actions$.pipe(
      ofType(ExportActions.initiateExport),
      switchMap(({ config }) =>
        this.exportService.initiateExport(config).pipe(
          map(job => ExportActions.initiateExportSuccess({ job })),
          catchError(error => of(ExportActions.initiateExportFailure({ error: error.message })))
        )
      )
    )
  );

  trackExportProgress$ = createEffect(() =>
    this.actions$.pipe(
      ofType(ExportActions.initiateExportSuccess),
      switchMap(({ job }) =>
        this.hubService.connect('exportHub').pipe(
          switchMap(() =>
            this.hubService.on(`progress-${job.id}`).pipe(
              map(progress => ExportActions.updateExportProgress({
                jobId: job.id,
                progress
              })),
              takeUntil(
                this.actions$.pipe(
                  ofType(ExportActions.exportCompleted, ExportActions.exportFailed),
                  filter(action => action.jobId === job.id)
                )
              )
            )
          )
        )
      )
    )
  );

  loadAvailableFormats$ = createEffect(() =>
    this.actions$.pipe(
      ofType(ExportActions.loadAvailableFormats),
      switchMap(() =>
        of(this.formatService.getAvailableFormats()).pipe(
          map(formats => ExportActions.loadAvailableFormatsSuccess({ formats }))
        )
      )
    )
  );
}
```

## TypeScript Interfaces

### 1. Core Export Interfaces
```typescript
export interface ExportConfig {
  format: ExportFormat;
  formatOptions: Record<string, any>;
  dateRange: DateRange;
  filters: FilterConfig;
  columns: string[];
  deliveryOptions: DeliveryOptions;
  metadata: ExportMetadata;
}

export interface ExportJob {
  id: string;
  userId: string;
  config: ExportConfig;
  status: ExportStatus;
  createdAt: Date;
  startedAt?: Date;
  completedAt?: Date;
  estimatedSize: number;
  actualSize?: number;
  downloadUrl?: string;
  errorMessage?: string;
}

export interface ExportProgress {
  jobId: string;
  percentage: number;
  currentStage: string;
  estimatedTimeRemaining: number;
  processedRecords: number;
  totalRecords: number;
  bytesProcessed: number;
  totalBytes: number;
}

export interface ExportResult {
  jobId: string;
  downloadUrl: string;
  fileName: string;
  fileSize: number;
  checksum: string;
  expiresAt: Date;
}
```

### 2. Format-specific Interfaces
```typescript
export interface PdfFormatOptions {
  orientation: 'portrait' | 'landscape';
  pageSize: 'A4' | 'Letter' | 'Legal' | 'A3';
  includeCharts: boolean;
  includeHeader: boolean;
  includeFooter: boolean;
  watermark?: string;
  fontSize: number;
  margins: {
    top: number;
    right: number;
    bottom: number;
    left: number;
  };
}

export interface ExcelFormatOptions {
  worksheetName: string;
  includeCharts: boolean;
  freezeHeaders: boolean;
  autoFilter: boolean;
  formatting: {
    headerStyle: CellStyle;
    dataStyle: CellStyle;
    alternateRowColor: boolean;
  };
  chartOptions?: ChartOptions;
}

export interface CsvFormatOptions {
  delimiter: ',' | ';' | '\t' | '|';
  encoding: 'UTF-8' | 'UTF-16' | 'ASCII';
  includeHeaders: boolean;
  quoteStrings: boolean;
  dateFormat: string;
  numberFormat: string;
  booleanFormat: 'true/false' | '1/0' | 'yes/no';
}
```

### 3. Filter and Column Interfaces
```typescript
export interface FilterConfig {
  conditions: FilterCondition[];
  logic: 'AND' | 'OR';
  dateRange: DateRange;
  customFilters: Record<string, any>;
}

export interface FilterCondition {
  column: string;
  operator: FilterOperator;
  value: any;
  dataType: 'string' | 'number' | 'date' | 'boolean';
}

export interface ColumnDefinition {
  key: string;
  displayName: string;
  dataType: 'string' | 'number' | 'date' | 'boolean';
  required: boolean;
  sortable: boolean;
  filterable: boolean;
  exportable: boolean;
  format?: string;
  width?: number;
}

export interface DateRange {
  startDate: Date;
  endDate: Date;
  preset?: DateRangePreset;
}
```

## Component Templates

### 1. Export Dialog Template
```html
<!-- export-dialog.component.html -->
<mat-dialog-container class="export-dialog-container" role="dialog" [attr.aria-label]="'export.dialog.title' | translate">

  <!-- Dialog Header -->
  <mat-dialog-header class="export-dialog-header">
    <h2 mat-dialog-title>{{ 'export.dialog.title' | translate }}</h2>
    <button mat-icon-button mat-dialog-close aria-label="Close dialog">
      <mat-icon>close</mat-icon>
    </button>
  </mat-dialog-header>

  <!-- Dialog Content -->
  <mat-dialog-content class="export-dialog-content">
    <form [formGroup]="exportForm" class="export-form">

      <!-- Step 1: Format Selection -->
      <mat-card class="export-step">
        <mat-card-header>
          <mat-card-title>{{ 'export.format.title' | translate }}</mat-card-title>
        </mat-card-header>
        <mat-card-content>
          <app-format-selector
            formControlName="format"
            [availableFormats]="availableFormats$ | async"
            [previewEnabled]="true"
            (formatChange)="onFormatChange($event)">
          </app-format-selector>
        </mat-card-content>
      </mat-card>

      <!-- Step 2: Data Range -->
      <mat-card class="export-step" *ngIf="selectedFormat$ | async">
        <mat-card-header>
          <mat-card-title>{{ 'export.dateRange.title' | translate }}</mat-card-title>
        </mat-card-header>
        <mat-card-content>
          <app-data-range-picker
            formControlName="dateRange"
            [maxRange]="365"
            (rangeChange)="onDateRangeChange($event)">
          </app-data-range-picker>
        </mat-card-content>
      </mat-card>

      <!-- Step 3: Filters and Columns -->
      <mat-card class="export-step" *ngIf="dateRangeValid$ | async">
        <mat-card-header>
          <mat-card-title>{{ 'export.filters.title' | translate }}</mat-card-title>
        </mat-card-header>
        <mat-card-content>
          <app-filter-config
            formControlName="filters"
            [availableColumns]="availableColumns$ | async"
            [savedFilters]="savedFilters$ | async"
            (filtersChange)="onFiltersChange($event)">
          </app-filter-config>
        </mat-card-content>
      </mat-card>

      <!-- Step 4: Export Options -->
      <mat-card class="export-step" *ngIf="filtersValid$ | async">
        <mat-card-header>
          <mat-card-title>{{ 'export.options.title' | translate }}</mat-card-title>
        </mat-card-header>
        <mat-card-content>
          <app-export-options
            formControlName="options"
            [format]="selectedFormat$ | async"
            [estimatedSize]="estimatedSize$ | async">
          </app-export-options>
        </mat-card-content>
      </mat-card>

    </form>
  </mat-dialog-content>

  <!-- Dialog Actions -->
  <mat-dialog-actions class="export-dialog-actions">
    <button mat-button type="button" (click)="onCancel()">
      {{ 'export.actions.cancel' | translate }}
    </button>
    <button mat-raised-button
            color="primary"
            type="button"
            [disabled]="exportForm.invalid || (isExporting$ | async)"
            (click)="onExport()">
      <mat-icon *ngIf="!(isExporting$ | async)">file_download</mat-icon>
      <mat-icon *ngIf="isExporting$ | async">
        <mat-spinner diameter="20"></mat-spinner>
      </mat-icon>
      {{ 'export.actions.export' | translate }}
    </button>
  </mat-dialog-actions>

  <!-- Progress Overlay -->
  <div class="export-progress-overlay" *ngIf="isExporting$ | async">
    <app-export-progress
      [jobId]="currentJobId$ | async"
      [showDetails]="true">
    </app-export-progress>
  </div>

</mat-dialog-container>
```

### 2. Format Selector Template
```html
<!-- format-selector.component.html -->
<div class="format-selector" role="radiogroup" [attr.aria-label]="'export.format.selection' | translate">

  <div class="format-grid">
    <div class="format-option"
         *ngFor="let format of availableFormats; trackBy: trackByFormat"
         [class.selected]="selectedFormat === format"
         (click)="selectFormat(format)"
         (keydown.enter)="selectFormat(format)"
         (keydown.space)="selectFormat(format)"
         tabindex="0"
         role="radio"
         [attr.aria-checked]="selectedFormat === format"
         [attr.aria-label]="getFormatLabel(format)">

      <div class="format-icon">
        <mat-icon>{{ getFormatIcon(format) }}</mat-icon>
      </div>

      <div class="format-details">
        <h4 class="format-name">{{ getFormatName(format) | translate }}</h4>
        <p class="format-description">{{ getFormatDescription(format) | translate }}</p>

        <div class="format-features" *ngIf="getFormatFeatures(format) as features">
          <mat-chip-set>
            <mat-chip *ngFor="let feature of features">{{ feature | translate }}</mat-chip>
          </mat-chip-set>
        </div>
      </div>

      <div class="format-preview" *ngIf="previewEnabled && selectedFormat === format">
        <button mat-icon-button
                (click)="showPreview(format)"
                [attr.aria-label]="'export.format.preview' | translate">
          <mat-icon>preview</mat-icon>
        </button>
      </div>

    </div>
  </div>

  <!-- Format Options -->
  <div class="format-options" *ngIf="selectedFormat && formatOptions[selectedFormat]">
    <h5>{{ 'export.format.options' | translate }}</h5>

    <!-- PDF Options -->
    <div *ngIf="selectedFormat === ExportFormat.PDF" class="pdf-options">
      <mat-form-field>
        <mat-label>{{ 'export.pdf.orientation' | translate }}</mat-label>
        <mat-select [(value)]="formatOptions[selectedFormat].orientation">
          <mat-option value="portrait">{{ 'export.pdf.portrait' | translate }}</mat-option>
          <mat-option value="landscape">{{ 'export.pdf.landscape' | translate }}</mat-option>
        </mat-select>
      </mat-form-field>

      <mat-form-field>
        <mat-label>{{ 'export.pdf.pageSize' | translate }}</mat-label>
        <mat-select [(value)]="formatOptions[selectedFormat].pageSize">
          <mat-option value="A4">A4</mat-option>
          <mat-option value="Letter">Letter</mat-option>
          <mat-option value="Legal">Legal</mat-option>
        </mat-select>
      </mat-form-field>

      <mat-checkbox [(ngModel)]="formatOptions[selectedFormat].includeCharts">
        {{ 'export.pdf.includeCharts' | translate }}
      </mat-checkbox>
    </div>

    <!-- Excel Options -->
    <div *ngIf="selectedFormat === ExportFormat.Excel" class="excel-options">
      <mat-form-field>
        <mat-label>{{ 'export.excel.worksheetName' | translate }}</mat-label>
        <input matInput [(ngModel)]="formatOptions[selectedFormat].worksheetName">
      </mat-form-field>

      <mat-checkbox [(ngModel)]="formatOptions[selectedFormat].includeCharts">
        {{ 'export.excel.includeCharts' | translate }}
      </mat-checkbox>

      <mat-checkbox [(ngModel)]="formatOptions[selectedFormat].autoFilter">
        {{ 'export.excel.autoFilter' | translate }}
      </mat-checkbox>
    </div>

    <!-- CSV Options -->
    <div *ngIf="selectedFormat === ExportFormat.CSV" class="csv-options">
      <mat-form-field>
        <mat-label>{{ 'export.csv.delimiter' | translate }}</mat-label>
        <mat-select [(value)]="formatOptions[selectedFormat].delimiter">
          <mat-option value=",">,</mat-option>
          <mat-option value=";">;</mat-option>
          <mat-option value="\t">Tab</mat-option>
          <mat-option value="|">|</mat-option>
        </mat-select>
      </mat-form-field>

      <mat-form-field>
        <mat-label>{{ 'export.csv.encoding' | translate }}</mat-label>
        <mat-select [(value)]="formatOptions[selectedFormat].encoding">
          <mat-option value="UTF-8">UTF-8</mat-option>
          <mat-option value="UTF-16">UTF-16</mat-option>
          <mat-option value="ASCII">ASCII</mat-option>
        </mat-select>
      </mat-form-field>
    </div>

  </div>

</div>
```

## Styling Architecture

### 1. Component Styles (SCSS)
```scss
// export-dialog.component.scss
.export-dialog-container {
  width: 90vw;
  max-width: 1200px;
  min-height: 600px;
  max-height: 90vh;

  @media (max-width: 768px) {
    width: 95vw;
    min-height: 500px;
  }
}

.export-dialog-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 24px;
  border-bottom: 1px solid var(--divider-color);

  h2 {
    margin: 0;
    font-size: 1.5rem;
    font-weight: 500;
  }
}

.export-dialog-content {
  padding: 24px;
  overflow-y: auto;
  flex: 1;
}

.export-form {
  display: flex;
  flex-direction: column;
  gap: 24px;
}

.export-step {
  transition: all 0.3s ease;

  &:not(.active) {
    opacity: 0.6;
    pointer-events: none;
  }

  .mat-card-header {
    padding-bottom: 16px;
  }

  .mat-card-title {
    font-size: 1.25rem;
    font-weight: 500;
    color: var(--primary-color);
  }
}

.export-dialog-actions {
  padding: 16px 24px;
  border-top: 1px solid var(--divider-color);
  display: flex;
  justify-content: flex-end;
  gap: 12px;
}

.export-progress-overlay {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(255, 255, 255, 0.9);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
}

// RTL Support
[dir="rtl"] {
  .export-dialog-actions {
    justify-content: flex-start;
  }

  .export-dialog-header {
    text-align: right;
  }
}

// Dark Theme Support
.dark-theme {
  .export-progress-overlay {
    background: rgba(0, 0, 0, 0.9);
  }

  .export-step {
    &:not(.active) {
      opacity: 0.4;
    }
  }
}
```

### 2. Format Selector Styles
```scss
// format-selector.component.scss
.format-selector {
  width: 100%;
}

.format-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
  gap: 16px;
  margin-bottom: 24px;

  @media (max-width: 768px) {
    grid-template-columns: 1fr;
  }
}

.format-option {
  display: flex;
  align-items: center;
  padding: 16px;
  border: 2px solid var(--border-color);
  border-radius: 8px;
  cursor: pointer;
  transition: all 0.3s ease;
  background: var(--surface-color);

  &:hover {
    border-color: var(--primary-color);
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  }

  &:focus {
    outline: none;
    border-color: var(--primary-color);
    box-shadow: 0 0 0 3px rgba(var(--primary-rgb), 0.2);
  }

  &.selected {
    border-color: var(--primary-color);
    background: rgba(var(--primary-rgb), 0.05);

    .format-icon mat-icon {
      color: var(--primary-color);
    }
  }
}

.format-icon {
  margin-right: 16px;
  flex-shrink: 0;

  mat-icon {
    font-size: 32px;
    width: 32px;
    height: 32px;
    color: var(--text-secondary);
    transition: color 0.3s ease;
  }
}

.format-details {
  flex: 1;

  .format-name {
    margin: 0 0 8px 0;
    font-size: 1.1rem;
    font-weight: 500;
    color: var(--text-primary);
  }

  .format-description {
    margin: 0 0 12px 0;
    font-size: 0.9rem;
    color: var(--text-secondary);
    line-height: 1.4;
  }
}

.format-features {
  .mat-chip-set {
    margin: 0;
  }

  .mat-chip {
    font-size: 0.75rem;
    min-height: 24px;
    padding: 0 8px;
  }
}

.format-preview {
  margin-left: 16px;
  flex-shrink: 0;
}

.format-options {
  padding: 24px;
  background: var(--background-color);
  border-radius: 8px;
  border: 1px solid var(--border-color);

  h5 {
    margin: 0 0 16px 0;
    font-size: 1rem;
    font-weight: 500;
    color: var(--text-primary);
  }
}

.pdf-options,
.excel-options,
.csv-options {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 16px;
  align-items: start;

  .mat-form-field {
    width: 100%;
  }

  .mat-checkbox {
    grid-column: 1 / -1;
    margin-top: 8px;
  }
}

// RTL Support
[dir="rtl"] {
  .format-icon {
    margin-left: 16px;
    margin-right: 0;
  }

  .format-preview {
    margin-right: 16px;
    margin-left: 0;
  }
}
```

## Accessibility Implementation

### 1. ARIA Labels and Roles
```typescript
// accessibility.service.ts
@Injectable({
  providedIn: 'root'
})
export class AccessibilityService {

  generateAriaLabel(context: string, value?: any): string {
    const baseLabel = this.translate.instant(`accessibility.${context}`);
    return value ? `${baseLabel}: ${value}` : baseLabel;
  }

  announceExportProgress(progress: ExportProgress): void {
    const message = this.translate.instant('accessibility.export.progress', {
      percentage: progress.percentage,
      stage: progress.currentStage
    });
    this.liveAnnouncer.announce(message);
  }

  announceExportCompletion(success: boolean, fileName?: string): void {
    const messageKey = success ? 'accessibility.export.completed' : 'accessibility.export.failed';
    const message = this.translate.instant(messageKey, { fileName });
    this.liveAnnouncer.announce(message, success ? 'polite' : 'assertive');
  }
}
```

### 2. Keyboard Navigation
```typescript
// keyboard-navigation.directive.ts
@Directive({
  selector: '[appKeyboardNav]'
})
export class KeyboardNavigationDirective implements OnInit, OnDestroy {
  @Input() trapFocus: boolean = false;
  @Input() initialFocus: string = '';

  private focusableElements: HTMLElement[] = [];
  private currentIndex: number = 0;

  @HostListener('keydown', ['$event'])
  onKeyDown(event: KeyboardEvent): void {
    switch (event.key) {
      case 'Tab':
        if (this.trapFocus) {
          this.handleTabNavigation(event);
        }
        break;
      case 'ArrowDown':
      case 'ArrowRight':
        this.navigateNext(event);
        break;
      case 'ArrowUp':
      case 'ArrowLeft':
        this.navigatePrevious(event);
        break;
      case 'Home':
        this.navigateFirst(event);
        break;
      case 'End':
        this.navigateLast(event);
        break;
      case 'Escape':
        this.handleEscape();
        break;
    }
  }

  private updateFocusableElements(): void {
    const selectors = [
      'button:not([disabled])',
      'input:not([disabled])',
      'select:not([disabled])',
      'textarea:not([disabled])',
      '[tabindex]:not([tabindex="-1"])',
      'mat-radio-button:not([disabled])',
      'mat-checkbox:not([disabled])'
    ].join(', ');

    this.focusableElements = Array.from(
      this.elementRef.nativeElement.querySelectorAll(selectors)
    );
  }
}
```

This comprehensive component design provides a solid foundation for implementing the Export functionality with proper TypeScript typing, accessibility support, and responsive design patterns that align with the existing WeSign application architecture.