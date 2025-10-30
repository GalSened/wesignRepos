export class CreateAuthFlowModel{
    public signerToken : string;
    
}

export class CreateAuthFlowResultModel{
    public identityFlowURL : string;
    
}
export class IdentityCheckFlow{
    public  signerToken : string;
    public code: string ;
}
export class IdentityCheckFlowResultModel{
    public token : string;
    
}
