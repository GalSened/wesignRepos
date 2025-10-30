export class TemplateSingleLink{
    public templateId :string ;
    public singleLinkAdditionalResources: SingleLinkAdditionalResource[];
}

export class SingleLinkAdditionalResource{
    
    public templateId: string;
    public type: number;
    public data: string;
    public isMandatory: boolean;

}