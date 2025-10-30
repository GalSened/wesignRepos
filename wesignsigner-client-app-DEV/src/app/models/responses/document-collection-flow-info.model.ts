import { Language, OtpMode } from "./document-collection-count.model";

export class DocumentCollectionDataFlowInfoResponse {
    mapperID : string;
    otpMode: OtpMode = OtpMode.None;
    language: Language = Language.en;
    companyLogo : string;
    means : string;
    name : string;
    visualIdentificationRequired : boolean = false;
    shouldDisplaySignerNameInSignature: boolean = false;
    shouldDisplayMeaningOfSignature: boolean = false;
    shouldSignEidasSignatureFlow : boolean = false;
}