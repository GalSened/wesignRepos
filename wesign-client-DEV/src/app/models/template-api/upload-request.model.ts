import { Signer1Credential } from '@models/self-sign-api/signer-authentication.model';

export enum FileType {
    NONE = 0,
    PDF = 1,
    IMAGE = 2,
    DOCX = 3,
    DOC = 4,
}

export class UploadRequest {

    public Name: string;
    public Base64File: string;
    public MetaData: string;
    public IsOneTimeUseTemplate : boolean;
    public SourceTemplateId : string;
}

export class UploadRequests {

   public Name: string;
   public Requests: UploadRequest[];

   constructor() {
    this.Name = "";
    this.Requests = [];
   }
}

export class Signer1FileSiging {

    public FileName: string;
    public Base64File: string;
    public SigingFileType: SigningFileType;
    public Signer1Credential : Signer1Credential;
    
}

export enum SigningFileType {
    NONE = 0,
    WORD = 1,
    XML = 2,
    EXEL = 3,
    PDF = 4
}