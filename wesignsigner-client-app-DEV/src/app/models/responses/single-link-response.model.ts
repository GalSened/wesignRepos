import { Language } from "./document-collection-count.model";

export class SingleLinkResponse {
    public url: string;
}

export class SingleLinkDataResponseDTO {
    public isSmsProviderSupportGloballySend: boolean = false;
    public language: Language = Language.en;
}