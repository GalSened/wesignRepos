import { WeSignFieldType } from '../../enums/we-sign-field-type.enum';


export class FieldRequest {
    public fieldName : string;
    public fieldDescription : string;
    public fieldValue :string;    
    public fieldType :WeSignFieldType;
}

