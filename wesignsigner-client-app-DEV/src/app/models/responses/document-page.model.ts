import { pdfFields } from '../pdffields/pdffields.model';
import { SafeHtml } from '@angular/platform-browser';

export class DocumentPage {
    public documentId: string;
    public name: string;
    public signerFields: pdfFields;
    public otherFields: pdfFields;
    public pageImage: string;
    public ocrString: string;
    public ocrHtml: SafeHtml;
    public pageWidth: number;
    public pageHeight: number;
}

export class DocumentPages {
    public documentPages: DocumentPage[];
}