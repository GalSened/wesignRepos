import { BaseField, IField } from './basefield.model';
import { TextFieldType } from '../../enums/text-field-type.enum';
import { SignatureType } from '../../enums/signature-type.enum';
import { SignatureFieldKind } from 'src/app/enums/signature-field-kind.enum';

export class textField extends BaseField {
    public textFieldType: TextFieldType = TextFieldType.Text;
    public customerRegex: string;
    public value: string;
    public isHidden: boolean;    
}

export class signatureField extends BaseField {
    public image: string;
    public signingType: SignatureType;
    public signatureKind : SignatureFieldKind = SignatureFieldKind.Simple;
}

export class radioField extends BaseField {
    public isDefault: boolean = false;
    public value: string; // ******** TODO - check if needed ************
    public groupName: string;
}

export class radioFieldGroup implements IField {
    public name: string;
    public radioFields: radioField[] = [];
    public selectedRadioName: string;
}
export class checkBoxField extends BaseField {
    public isChecked: boolean;
    
}
export class choiceField extends BaseField {
    public options: string[];
    public selectedOption: string;
}