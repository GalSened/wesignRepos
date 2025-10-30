import { TextFieldType } from "src/app/enums/text-field-type.enum";
import { WeSignFieldType } from "src/app/enums/we-sign-field-type.enum";

export class DocumentCollectionHtmlDataResponse{
    public fieldsData: FieldData[];
    public htmlContent: string;
    public jsContent: string;
}

export class FieldData{
    public name: string;    
    public type: WeSignFieldType ;
    public textFieldType : TextFieldType;
    public value: string;    
}