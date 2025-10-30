import { Component, OnInit } from "@angular/core";
import { Store } from "@ngrx/store";
import { TranslateService } from "@ngx-translate/core";
import { SharedService } from '@services/shared.service';
import * as actions from "@state/actions/app.actions";
import { IAppState } from "@state/app-state.interface";
import { ILanguage, LangList } from "./languages.list";
import { NgSelectModule } from '@ng-select/ng-select';
import { FormsModule } from '@angular/forms';

@Component({
    selector: "sgn-lang-selector",
    templateUrl: "lang-selector.component.html",
})

export class LangSelectorComponent implements OnInit {

    public languages: ILanguage[] = LangList;
    public selectedLanguage: ILanguage;

    public languageOptions = LangList.reduce((prev, curr) => {
        prev[curr.Enum] = curr.Description;
        return prev;
    }, {});

    constructor(private store: Store<IAppState>,
         private translate: TranslateService,
         private sharedService: SharedService) {

        if (!this.translate.currentLang) {
            this.translate.use("en");
        }
    }

    public ngOnInit() {
        this.selectedLanguage = this.sharedService.getCurrentLanguage();
        let isEng = this.selectedLanguage.Code == "en";
        if(sessionStorage.getItem("language") != undefined)
        {
            let lenCode = sessionStorage.getItem("language");
            
            if((isEng && lenCode != "en"))
            {               
                this.languageSelected(this.languages[1]);                
            }
        }        
    }

 
    
    public languageSelected(language: ILanguage) {      
        if(language){
            sessionStorage.setItem("language", language.Code);
            this.selectedLanguage = language;
            this.translate.use(this.selectedLanguage.Code);
            //this.showLangCombo=false;
            document.getElementsByTagName("html")[0].setAttribute("dir", language.IsRtl ? "rtl" : "ltr");
            document.getElementsByTagName("html")[0].setAttribute("lang", language.Code);
            this.store.dispatch(new actions.SetLangAction({ Language: language.Code, IsRtl: language.IsRtl }));
        }
    }
}
