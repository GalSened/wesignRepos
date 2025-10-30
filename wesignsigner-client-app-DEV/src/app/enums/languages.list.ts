export interface ISignerLanguage {
    Language: string;
    imgClass: string;
}

const LangList: ISignerLanguage[] = [
    { Language: "English", imgClass: "ngSelect-icon-usa" },
    { Language: "עברית", imgClass: "ngSelect-icon-israel" },

];

export { LangList };