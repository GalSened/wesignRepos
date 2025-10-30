import { Injectable } from "@angular/core";
import { BaseResult, formatBaseResult } from "@models/base/base-result.model";
import { FLOW_NAMES, FLOW_STEP } from "@models/enums/flow-step.enum";
import { Store } from "@ngrx/store";
import { TranslateService } from "@ngx-translate/core";
import * as alertActions from "@state/actions/alert.actions";
import * as appActions from "@state/actions/app.actions";
import { AlertLevel, IAppState } from "@state/app-state.interface";
import { Errors } from '@models/error/errors.model';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Actions, ofType } from '@ngrx/effects';
import { bufferCount, first } from 'rxjs/operators';
import { DatePipe } from '@angular/common';
import { PageField, TextField } from '@models/template-api/page-data-result.model';
import { TextFieldType } from '@models/enums/text-field-type.enum';
import { ILanguage, LangList } from "@components/shared/languages.list";
import * as moment from 'moment';

@Injectable()
export class SharedService {

    constructor(
        private store: Store<IAppState>,
        private translate: TranslateService,
        private httpClient: HttpClient,
        private actions: Actions,
        private datePipe: DatePipe
    ) { }

    public setBusy(isBusy: boolean, message: string = "") {
        if (!message) {
            this.store.dispatch(new alertActions.SetBusyStateAction({
                IsBusy: isBusy,
                Message: "",
            }));
            return;
        }

        this.translate.get(message).subscribe((messageTranslated: string) => {
            this.store.dispatch(new alertActions.SetBusyStateAction({
                IsBusy: isBusy,
                Message: messageTranslated,
            }));
        });
    }

    public notifyPageLoaded() {
        this.store.dispatch(new alertActions.PageLoadedAction());
    }

    public setErrorAlert(result: Errors | BaseResult | string, shouldAutoHide: boolean = true) {
        if (typeof result === "string") {
            this.store.dispatch(new alertActions.SetAlertAction({
                Level: AlertLevel.ERROR,
                Message: result as string,
                ShouldAutoHide: shouldAutoHide
            }));
        } else if (result instanceof BaseResult) { // TODO - REMOVE if not in use
            this.translate.get(`SERVER_ERROR.${result.ResultCode}`).subscribe(
                (msg) => {
                    this.store.dispatch(new alertActions.SetAlertAction({
                        Level: AlertLevel.ERROR,
                        Message: formatBaseResult(msg, result),
                        ShouldAutoHide: shouldAutoHide
                    }));
                },
            );
        }
        else {
            //let er = new Errors(result);
            this.translate.get(`SERVER_ERROR.${result.errorCode}`).subscribe(
                (msg) => {
                    this.store.dispatch(new alertActions.SetAlertAction({
                        Level: AlertLevel.ERROR,
                        Message: msg,
                        ShouldAutoHide: shouldAutoHide
                    }));
                },
            );
        }
    }

    public setTranslateAlert(text: string, level: AlertLevel, shouldAutoHide: boolean = true) {
        this.translate.get(text).subscribe(msg => {
            this.store.dispatch(new alertActions.SetAlertAction({
                Level: level,
                Message: msg,
                ShouldAutoHide: shouldAutoHide
            }));
        });
    }

    public setSuccessAlert(msg: string, shouldAutoHide: boolean = true) { // TODO - remove
        this.translate.get(msg).subscribe((tMsg) => {
            this.store.dispatch(new alertActions.SetAlertAction({
                Level: AlertLevel.SUCCESS,
                Message: tMsg,
                ShouldAutoHide: shouldAutoHide
            }));
        });
    }

    public setFlowState(flowName: FLOW_NAMES, flowStep: FLOW_STEP) {
        this.store.dispatch(new appActions.SetFlowAction({
            FlowName: flowName,
            FlowStep: flowStep,
        }));
    }

    public getBackUrl() {
        const prevUrl = sessionStorage.getItem("prev.url");
        return prevUrl ? prevUrl : "/dashboard";
    }

    public static EnumToArrayHelper<E>(_enum: any): any {
        let values = Object.values(_enum).filter(value => isNaN(Number(value)) === true);
        let keys = Object.values(_enum).filter(value => isNaN(Number(value)) === false);
        return values.reduce((prev, curr, i) => {
            prev[keys[i] as string] = curr;
            return prev;
        }, {});
    }

    public getFormatsData(): Observable<any> {
        return this.httpClient.get('./assets/formats.json');
    }

    public getPlans(): Observable<any> {
        return this.httpClient.get('./assets/Plans.json');
    }

    public getSupportedCountries(): Observable<any> {
        return this.httpClient.get('./assets/SMSSupportedCountries.json');
    }

    public setLoadingBanner(pageCount: number, loadingText: string) {
        this.setBusy(true, loadingText);
        this.actions.pipe(
            ofType(alertActions.PAGE_LOADED),
            bufferCount(pageCount),
            first(),
        ).subscribe((_) => {
            this.setBusy(false);
        });
    }

    public getLocalTime(utcTime: string) {
        if (!utcTime) return '';

const local = moment.utc(utcTime).local().format();

return local.toString();
        
    }



    public FixFieldWithDotsInFieldName(fieldsforIntersectCheck: PageField[]) : PageField[]  {
     

        
        fieldsforIntersectCheck.forEach(element => {
            if(element.name.includes("."))
            {
                if(element.description == "")
                {
                    element.description = element.name;
                }
                element.name = element.name.replace(/\./g, "D");
            }
            if(element.name.includes(" "))
            {
                if(element.description == "")
                {
                    element.description = element.name;
                }
                element.name = element.name.replace(/\s/g, "");
            }

            let items = fieldsforIntersectCheck.filter(x => x.name == element.name);
            if(items.length > 1)
            {
                if(element.description == "")
                {
                    element.description = element.name;
                }
                element.name = element.name + Math.floor(Math.random() * 100);
            }
        });
            
       return fieldsforIntersectCheck;
    }

    public isFieldsIntersect(fieldsforIntersectCheck: PageField[]): PageField {
        
        
        let fields = fieldsforIntersectCheck.sort((x, y) => { return x.x >= y.x ? 1 : -1 })
        
        for (let i = 0; i < fields.length; ++i) {

            let items = fields.filter(x => x.page == fields[i].page && x.templateId == fields[i].templateId &&
                fields[i].x <= x.x && (fields[i].x + fields[i].width) >= x.x &&
                x != fields[i]);

            for (let j = 0; j < items.length; ++j) {
                if (fields[i] != undefined &&
                    items[j] != undefined && (
                        (fields[i].y <= items[j].y &&
                            (fields[i].y + fields[i].height) >= items[j].y) ||
                        (fields[i].y >= items[j].y &&
                            (items[j].y + items[j].height) >= fields[i].y)
                    )) {

                    return fields[i];
                }
            }


        }



        return undefined;
    }

    public removeDuplicates(items: any[]): any[] {
        let itemsSet = new Set(items);
        return Array.from(itemsSet);
    }

    //Format from yyyy-mm-dd to MMM d, y (for ex: Dec 24, 2021)
    public FormatDateTextFromDOMDateFormat(textfields: TextField[]) {
        textfields.forEach(textfield => {
            if (textfield.textFieldType == TextFieldType.Date) {
                let d = new Date(textfield.value);
                if (!isNaN(d.getDate())) {
                    textfield.value = this.datePipe.transform(d, 'MMM d, y');
                }
            }
        });

        return textfields;
    }

    //Format from MMM d, y (for ex: Dec 24, 2021) to yyyy-mm-dd
    public FormatDateTextToDOMDateFormat(textfields: TextField[]) {
        textfields.forEach(textfield => {
            if (textfield.textFieldType == TextFieldType.Date && textfield.value) {
                let d = new Date(textfield.value);
                if(d && !isNaN(d.getDate()))
                {
                    let day = ("0" + d.getDate()).slice(-2);
                    let month = d.getMonth();
                    month = month + 1;
                    let monthString = ("0" + month).slice(-2);
                    let year = d.getFullYear();
                    textfield.value = `${year}-${monthString}-${day}`
                }
            }
        });

        return textfields;
    }

    public scrollIntoInvalidField(ex) {
        window.scroll(0, 0);
        if (ex.errorCode == 400) {
            let errorValue = ex.errors.errors[Object.keys(ex.errors.errors)[0]];
            let openIndex = errorValue[0].lastIndexOf('[');
            let closeIndex = errorValue[0].indexOf(']', openIndex);
            let fieldName = errorValue[0].substring(openIndex + 1, closeIndex);
            let htmlElements = document.querySelectorAll('[data-fieldname="' + fieldName + '"]');
            if (htmlElements.length > 0) {
                setTimeout(() => {
                    for (let index = 0; index < htmlElements[0].childNodes.length; index++) {
                        const divChild = htmlElements[0].childNodes[index];
                        if (divChild instanceof HTMLDivElement) {
                            (<HTMLDivElement>divChild).classList.add("is-error");
                            htmlElements[0].scrollIntoView();
                            break;
                        }
                    }
                }, 2000);
            }
        }
    }

    public removeErrorClassFromElements(textFields: TextField[]) {
        textFields.forEach(textField => {

            let htmlElements = document.querySelectorAll('[data-fieldname="' + textField.name + '"]');
            if (htmlElements.length > 0) {
                let divChild = htmlElements[0].childNodes[0];
                if (divChild instanceof HTMLDivElement) {
                    (<HTMLDivElement>divChild).classList.remove("is-error");
                }
            }

        });
    }

    public getCurrentLanguage() {
        let languages: ILanguage[] = LangList;
        let language = document.getElementsByTagName("html")[0].getAttribute("dir");
        let isEng = language == "ltr";
        let selectedLanguage = isEng ? languages.find((l) => l.Code === "en")
            : languages.find((l) => l.Code === "he");

        return selectedLanguage;
    }

    public generateNewGuid() {
        return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
            var r = Math.random() * 16 | 0,
                v = c == 'x' ? r : (r & 0x3 | 0x8);
            return v.toString(16);
        });
    }

    public convertArrayBufferToErrorsObject(arrayBuffer: ArrayBuffer) {
        var enc = new TextDecoder("utf-8");
        let errorStr = enc.decode(arrayBuffer);
        let obj: Errors = JSON.parse(errorStr);
        let ex = new Errors(obj);

        return ex;
    }

    public getMinimumPasswordLengthFromError(keyToSearch: string, errors: Errors) {
        const pattern = /Password should contain at least one digit, one special character and at least (\d+) characters long/;
        if (errors.errors.status == 400) {
                let validationErrorValues = errors.errors.errors[keyToSearch];
                if (validationErrorValues != null) {
                    for (const message of validationErrorValues) {
                        const match = message.match(pattern);
                        if (match) {
                            return match[1];
                        }
                    }
                }
        }
        return null;
    }
}
