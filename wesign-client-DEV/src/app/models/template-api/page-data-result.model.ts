import { BaseResult } from "@models/base/base-result.model";
import { TextFieldType } from "@models/enums/text-field-type.enum";
import { Contact } from '@models/contacts/contact.model';
import { SignatureType } from '@models/enums/signature-type.enum';
import { Signer } from '@models/document-api/signer.model';
import { SignatureFieldKind } from '@models/enums/signature-field-kind.enum';
import { SafeHtml } from '@angular/platform-browser';

//export interface IField { }

export class PageField {
    public name: string;
    public description: string;
    public x: number;
    public y: number;
    public width: number;
    public height: number;
    public mandatory: boolean;
    public fieldGroup: number;
    public page: number;
    public templateId: string;
    public isValid: boolean = true;
    public signerId: string;
}

export class TextField extends PageField {
    public textFieldType: TextFieldType = TextFieldType.Text;
    public customerRegex: string;
    public value: string;
    public isHidden: boolean;    
}

export class SignatureField extends PageField {
    public image: string;
    //public signingType: string = "1";
    public signingType: SignatureType = SignatureType.Graphic;
     public signatureKind : SignatureFieldKind = SignatureFieldKind.Simple;
}

export class RadioField extends PageField {
    public isDefault: boolean = false;
    public value: string; // ******** TODO - check if needed ************
    public groupName: string;
}

export class RadioFieldGroup {
    public name: string;
    public radioFields: RadioField[] = [];
    public selectedRadioName: string;
}

export class CheckBoxField extends PageField {
    public isChecked: boolean;
    public value: string;
}

export class ChoiceField extends PageField { // List / selectbox
    public options: string[];
    public selectedOption: string;
    public value: string;
}

export class DocumentFields {
    public textFields: TextField[];
    public signatureFields: SignatureField[];
    public radioGroupFields: RadioFieldGroup[];
    public checkBoxFields: CheckBoxField[];
    public choiceFields: ChoiceField[];
}

export class PageDataResult extends BaseResult {
    public pdfFields: DocumentFields;
    public pageImage: string;
    public ocrString: string;
    public ocrHtml: SafeHtml;
    public templateId: number;
    public pageCount: number;
    public pageNumber: number;
    public pageWidth: number;
    public pageHeight: number;
}

export class TemplatePagesRangeResponse {
    public templatePages : PageDataResult [];
}

export class DocumentPageDataResult extends BaseResult {
    public pdfFields: DocumentFields;
    public pageImage: string;
    public ocrString: string;
    public ocrHtml: SafeHtml;
    public documentId: number;    
    public pageCount: number;
    public pageNumber: number;
    public pageWidth: number;
    public pageHeight: number;
}

export class DocumentPagesRangeResponse {
    public documentPages : DocumentPageDataResult [];
}

export class AssignedField {
    // public contact: Contact;
    public signer: Signer;
    public field: PageField;

    constructor(signer?: Signer, field?: PageField) {
        // this.contact = contact;
        this.signer = signer;
        this.field = field;
    }
}