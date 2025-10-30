import { Injectable } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { Language } from './models/responses/document-collection-count.model';

@Injectable({
  providedIn: 'root'
})
export class LanguageService {

  constructor(private translate: TranslateService) { }

  public changeLang(lang) {
    let code = lang.Language == "English" ? "en" : "he";
    let direction = code == "en" ? "ltr" : "rtl";
    this.translate.use(code);

    document.getElementsByTagName("html")[0].setAttribute("dir", direction);
    document.getElementsByTagName("html")[0].setAttribute("lang", code);
  }

  public changeLangFromNumber(lang: number) {
    let code = lang == 1 ? "en" : "he";
    let direction = code == "en" ? "ltr" : "rtl";
    this.translate.use(code);

    document.getElementsByTagName("html")[0].setAttribute("dir", direction);
    document.getElementsByTagName("html")[0].setAttribute("lang", code);
  }

  public fixLanguage(language: Language) {
    this.changeLangFromNumber(language);
    let els = document.getElementsByName("languageSelect") as NodeListOf<HTMLSelectElement>;
    els.forEach(el => {
      el.selectedIndex = language == Language.en ? 0 : 1;

    });
  }
}