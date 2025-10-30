import { Injectable } from "@angular/core";
import { RadioField, PageField, RadioFieldGroup, TextField } from "@models/template-api/page-data-result.model";
import { Store } from "@ngrx/store";
import * as fieldActions from "@state/actions/fields.actions";
import { IAppState, AppState } from "@state/app-state.interface";
import { first } from "rxjs/operators";

@Injectable()
export class StateProcessService {

    constructor(private store: Store<IAppState>) { }

    public getStateSnapshot() {
        return this.store.select<any>('appstate').pipe(first());
    }

    public getState() {
        return this.store.select<any>('appstate');
    }

    public getPageFields<T extends PageField>(state: AppState, typeT: new () => T) {
        return state.PageFields
            .filter((pf) => pf instanceof typeT)
            .map((pf) => pf as T);
    }

    public getSelectedPageField<T extends PageField>(state: AppState, typeT: new () => T) {
        return state.PageFields.find(
            (tf) => tf instanceof typeT &&
                tf.name === state.SelectedField.FieldName && tf.templateId === state.SelectedField.TemplateId) as T;
    }

    public getSelectedRadioField(state: AppState) {
        // var categoryGroup = state.RadioGroupFields.filter(e => e.radioFields.filter(c => c.name === state.SelectedField)[0])[0];
        // return categoryGroup ? categoryGroup.radioFields.filter(c => c.name === state.SelectedField)[0] : null;

        // return state.RadioGroupNames
        //     .map(r => r.radioFields)
        //     .reduce((a, b) => { return a.concat(b); }, [])
        //     .find(r => r.name == state.SelectedField)

        //.filter(rg => rg.radioFields.some(r => r.name == state.SelectedField)).reduce((a,b)=>{return a})

        //.find(r => r.radioFields.some(a => a.name == state.SelectedField));

        // return state.PageFields
        //     .filter((pf) => pf instanceof RadioField)
        //     .map((pf) => pf as RadioField)
        //     // // .find((pf) => `${pf.name}_${pf.value}` === state.SelectedField);
        //     //  .find((pf) => `${pf.name}` === state.SelectedRadioGroup);      
        //     .find((pf) => pf.name === state.SelectedField);

        ;
    }

    public getRadioGroupFields(state: AppState) {
        
        let xxx = state.PageFields.filter((pf) => pf instanceof RadioField);
        let xxxx = xxx.map((pf) => pf as RadioField);
        const radioFields = xxxx.sort((a, b) => a.name.localeCompare(b.name));

        let groupsNames = radioFields.map(x=>x.groupName);
        let uniqeGroupsNames = groupsNames.filter((v,i) => groupsNames.indexOf(v) === i);
        const radioGroupFields: RadioFieldGroup[] = [];
        uniqeGroupsNames.forEach( name=>{
            let radioGroupField = new RadioFieldGroup();
            radioGroupField.name = name;
            radioGroupFields.push(radioGroupField);
        });
        radioGroupFields.forEach(group =>{
            let fields = radioFields.filter(x=>x.groupName == group.name);
            fields.forEach(field =>{
                if(field.isDefault){
                    group.selectedRadioName = field.name;
                }
                group.radioFields.push(field);
            });
        });

        return radioGroupFields;
        
    }

    public removeField(name: string, templateId: string) {
        this.store.dispatch(new fieldActions.RemovePageFieldAction({ SelectedField: { FieldName: name, TemplateId: templateId } }));
    }
}
