export class TextNameToValue {
    public Name: string;
    public Value: string;
}

export class SignatureNameToValue {
    public Name: string;
    public Image: string;
}

export class CheckboxNameToValue {
    public Name: string;
    public Checked: string;
}

export class RadioGroupNameToValue {
    public Name: string;
    public SelectedValue: string;
}
export class ChoiceNameToValue {
    public Name: string;
    public SelectedOption: string;
}

export enum Operation {
    Save = 1,
    Decline = 2,
    Close = 3
}

export enum ApiMode {
    SelfSign = 1,
    Workflow = 2,
}

export class DocUpdateRequest {
    public Operation: Operation;
    public ApiMode: ApiMode;
    public Fields: {
        TextNameToValue: TextNameToValue[],
        SignatureNameToValue: SignatureNameToValue[]
        CheckboxNameToValue: CheckboxNameToValue[];
        RadioGroupNameToValue: RadioGroupNameToValue[];
        ChoiceNameToValue: ChoiceNameToValue[];
    };

    constructor() {
        this.Fields = {
            CheckboxNameToValue: [],
            ChoiceNameToValue: [],
            RadioGroupNameToValue: [],
            SignatureNameToValue: [],
            TextNameToValue: [],
        };
    }
}