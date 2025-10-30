

export enum Language{
    ENGLISH = 1,
    HEBREW = 2
}

export interface ILanguage {
    Description: string;
    Code: string;
    IsRtl: boolean;
    Enum: number;
    imgClass: string;
}

const LangList: ILanguage[] = [
    { Description: "English", Code: "en", IsRtl: false, Enum: 1, imgClass: "ngSelect-icon-usa"},
    { Description: "עברית", Code: "he", IsRtl: true, Enum: 2, imgClass:"ngSelect-icon-israel"},
    
];

export { LangList };
