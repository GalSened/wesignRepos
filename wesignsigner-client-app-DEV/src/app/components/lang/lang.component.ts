import { Component, OnInit } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { ISignerLanguage, LangList } from '../../enums/languages.list'
import { Language } from 'src/app/models/responses/document-collection-count.model';

@Component({
  selector: 'app-lang',
  templateUrl: './lang.component.html',
  styleUrls: ['./lang.component.scss']
})

export class LangComponent implements OnInit {

  public SelectedLanguage;
  public languages: ISignerLanguage[] = LangList;

  constructor(private translate: TranslateService) { }

  ngOnInit(): void {
    let prevLang = localStorage.getItem("Language");
    let lang = prevLang ?? document.getElementsByTagName("html")[0].lang;
    this.SelectedLanguage = lang == "en" ? LangList[0] : LangList[1];
    let els = document.getElementsByName("languageSelect") as NodeListOf<HTMLSelectElement>;
    els.forEach(el => {
      if (lang == "en") {
        el.selectedIndex = 0;
      }
      else if(lang == "he"){
        el.selectedIndex = 1;
      }
    });
  }

  public changeLangByLanguage(lang: Language) {
    if(lang == Language.en){
      this.SelectedLanguage = LangList[0];
    }
    else{
      this.SelectedLanguage = LangList[1];
    }
    localStorage.setItem("Language", lang == Language.en ? "en" : "he");
  }

  public changeLang(lang) {
    let code = lang.Language == "English" ? "en" : "he";
    let direction = code == "en" ? "ltr" : "rtl";
    this.translate.use(code);

    document.getElementsByTagName("html")[0].setAttribute("dir", direction);
    document.getElementsByTagName("html")[0].setAttribute("lang", code);
    localStorage.setItem("Language", code);
  }
}