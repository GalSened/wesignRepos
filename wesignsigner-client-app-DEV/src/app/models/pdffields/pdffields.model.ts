import { textField, signatureField, radioFieldGroup, checkBoxField, choiceField } from './field.model';

export class pdfFields {
    public textFields: textField[];
    public signatureFields: signatureField[];
    public radioGroupFields: radioFieldGroup[];
    public choiceFields: choiceField[];
    public checkBoxFields: checkBoxField[];
}