import { CheckBoxField, ChoiceField, RadioFieldGroup, SignatureField, TextField } from "./page-data-result.model";

export class PDFFields {
    public textFields: TextField[];
    public signatureFields: SignatureField[];
    public checkBoxFields: CheckBoxField[];
    public radioGroupFields: RadioFieldGroup[] = [];
    public choiceFields: ChoiceField[];
}

export class UpdateTemplateRequest {
    public id: string;
    public name: string;
    public fields: PDFFields;
    constructor() {
        this.fields = new PDFFields();
    }
}
