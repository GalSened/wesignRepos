import { Component, Input, OnInit } from '@angular/core';
import { Signer } from '@models/document-api/signer.model';
import { FLOW_STEP } from '@models/enums/flow-step.enum';
import { SignatureType } from '@models/enums/signature-type.enum';
import { TextFieldType } from '@models/enums/text-field-type.enum';
import { CheckBoxField, ChoiceField, PageField, RadioField, SignatureField, TextField } from '@models/template-api/page-data-result.model';
import { TranslateService } from '@ngx-translate/core';
import { GroupAssignService } from '@services/group-assign.service';
import { SharedService } from '@services/shared.service';
import { StateProcessService } from '@services/state-process.service';
import { AlertLevel, AppState, IAppState } from '@state/app-state.interface';
import { Observable } from 'rxjs';
import { Store } from "@ngrx/store";
import * as fieldActions from "@state/actions/fields.actions";
import { SignatureFieldKind } from '@models/enums/signature-field-kind.enum';
import { Modal } from '@models/modal/modal.model';

@Component({
    selector: 'sgn-assign-modal',
    templateUrl: 'assign-modal.component.html'
})

export class AssignModalComponent implements OnInit {
    isShown = false;
    selectedField: PageField;
    @Input() public signers: Signer[];
    @Input() public pages$: Observable<number[]>;
    @Input() public pagesCount: number;
    allSignturePageField: PageField[] = [];
    allTextPageField: PageField[] = [];
    allRadiosPageField: RadioField[] = [];
    SignatureType: SignatureType;
    isText: boolean = false;
    isSignature: boolean = false;
    isList = false;
    isCheckbox = false;
    isRadio = false;
    appState: AppState;
    wasMandatory = false;
    shouldSetAllFieldsToSigner: boolean;
    ShowDuplicate = false;
    previousName = "";
    shouldSetAllFieldsToSignerChanged = false;
    assignAllFieldsModal = new Modal()
    flowStep: FLOW_STEP;
    title = "Text Field";
    origSignerId: string;
    origname: string;
    ShowDuplicateFields = false;

    constructor(private stateService: StateProcessService, private translate: TranslateService,
        private sharedService: SharedService, private assignService: GroupAssignService, private store: Store<IAppState>) { }

    ngOnInit() {
        this.stateService.getState().subscribe((state: AppState) => {
            this.appState = state

            this.flowStep = state.FlowStep;
            this.selectedField = this.stateService.getSelectedPageField<PageField>(state, PageField);

            if (this.selectedField) {
                this.origSignerId = this.selectedField.signerId;
                this.origname = this.selectedField.name;
                this.previousName = this.selectedField.description;
                this.wasMandatory = this.selectedField.mandatory;
                this.ShowDuplicate = (this.selectedField instanceof TextField || this.selectedField instanceof SignatureField) && state.SelectedTemplates.length <= 1;
            }

            this.allSignturePageField = this.stateService.getPageFields<SignatureField>(state, SignatureField);
            this.allTextPageField = this.stateService.getPageFields<TextField>(state, TextField);
            this.allRadiosPageField = this.stateService.getPageFields<RadioField>(state, RadioField);
            // TODO - make it conditionally with if else
            this.isText = this.selectedField instanceof TextField;
            this.isSignature = this.selectedField instanceof SignatureField;
            this.isList = this.selectedField instanceof ChoiceField;
            this.isCheckbox = this.selectedField instanceof CheckBoxField;
            this.isRadio = this.selectedField instanceof RadioField;
            this.LoadTitle();

            this.translate.get(['DOCUMENT.SHOULD_ASSIGN_ALL_FIELDS_TITLE', 'DOCUMENT.SHOULD_ASSIGN_ALL_FIELDS_MESSAGE', 'BUTTONS.NO', 'BUTTONS.YES'])
                .subscribe((res: object) => {
                    let keys = Object.keys(res);
                    this.assignAllFieldsModal.title = res[keys[0]];
                    this.assignAllFieldsModal.content = res[keys[1]];
                    this.assignAllFieldsModal.rejectBtnText = res[keys[2]];
                    this.assignAllFieldsModal.confirmBtnText = res[keys[3]];
                });
        });
        this.assignService.showEditModalObserv.subscribe(state => this.isShown = state);
    }

    optionChanged(newValue, index) {
        (<ChoiceField>this.selectedField).options[index] = newValue;
    }

    duplicateFieldToPagesClose() {
        this.ShowDuplicateFields = false;
    }

    duplicateFieldToPagesAccept($event) {
        if ($event.allSelected as boolean) {
            this.copyToPages([])
        }
        else {
            this.copyToPages($event.pages)

        }

        this.ShowDuplicateFields = false;
    }

    SelectedUserChange(value) {
        if (this.signers && this.signers.length > 1 && this.shouldSetAllFieldsToSigner) {
            this.shouldSetAllFieldsToSigner = false;
        }
    }

    GetTextTitleByType() {
        let field = this.selectedField as TextField;
        if (field.textFieldType == TextFieldType.Date) {
            return this.translate.instant(`FIELDS.DATE`);
        }
        if (field.textFieldType == TextFieldType.Email) {
            return this.translate.instant(`FIELDS.EMAIL`);
        }
        if (field.textFieldType == TextFieldType.Number) {
            return this.translate.instant(`FIELDS.NUMBER`);
        }
        if (field.textFieldType == TextFieldType.Phone) {
            return this.translate.instant(`FIELDS.PHONE`);
        }
        if (field.textFieldType == TextFieldType.Time) {
            return this.translate.instant(`FIELDS.TIME`);
        }
        if (field.textFieldType == TextFieldType.List) {
            return this.translate.instant(`FIELDS.LIST`);
        }
        return this.translate.instant(`FIELDS.TEXT`);
    }

    LoadTitle() {
        if (this.isText) {
            this.title = this.GetTextTitleByType();
            return;
        }
        if (this.isSignature) {
            if ((this.selectedField as SignatureField).signatureKind == SignatureFieldKind.Initials) {
                this.title = this.translate.instant(`FIELDS.INITIALS_SIGNATURE`);
            }
            else {
                this.title = this.translate.instant(`FIELDS.SIGNATURE`);
            }
            return;
        }
        if (this.isCheckbox) {
            this.title = this.translate.instant(`FIELDS.CHECKBOX`);
            return;
        }
        if (this.isList) {
            this.title = this.translate.instant(`FIELDS.LIST`);
            return;
        }
        if (this.isRadio) {
            this.title = this.translate.instant(`FIELDS.RADIO`);
        }
    }

    public get SignatureTypeOptions() {
        return SharedService.EnumToArrayHelper<SignatureType>(SignatureType);
    };

    cancel() {
        this.selectedField.signerId = this.origSignerId;
        this.selectedField.name = this.origname;
        this.selectedField.description = this.previousName;

        if (this.selectedField.mandatory != this.wasMandatory) {
            this.selectedField.mandatory = this.wasMandatory;
            if (this.isRadio) {
                let radiosInGroup = this.allRadiosPageField.filter(x => x.groupName == (this.selectedField as RadioField).groupName && x.templateId == this.selectedField.templateId);
                radiosInGroup.forEach(radio => radio.mandatory = this.wasMandatory);
            }
            if (this.isText) {
                let textInGroup = this.allTextPageField.filter(x => x.description == this.selectedField.description && x.templateId == this.selectedField.templateId);
                textInGroup.forEach(text => text.mandatory = this.wasMandatory);
            }
        }

        this.isShown = false;
        if (this.shouldSetAllFieldsToSignerChanged) {
            this.shouldSetAllFieldsToSigner = !this.shouldSetAllFieldsToSigner
        }
        this.shouldSetAllFieldsToSignerChanged = false;
    }

    submit() {
        if (!this.selectedField.name) {

            this.sharedService.setTranslateAlert("FIELDS.NAME_EMPTY", AlertLevel.ERROR);
            return;
        }
        if (this.isSignature) {
            if (this.selectedField.name != this.origname) {
                let exist = this.allSignturePageField.filter(element => { return element.name == this.selectedField.name });

                if (exist.length > 1) {
                    this.sharedService.setTranslateAlert("FIELDS.SIG_WITH_THE_SAME_NAME_EXIST", AlertLevel.ERROR);
                    return;
                }
            }
        }

        if (this.selectedField) {
            if (this.selectedField.signerId == "-----") {
                this.selectedField.signerId = undefined
                this.assignService.returnFieldToNotAssignedUsers(this.selectedField);
            }
            else {
                this.assignService.addField(this.selectedField);
                if (this.shouldSetAllFieldsToSigner) {
                    this.assignAllFieldsModal.showModal = true
                }
                else if (this.isRadio) {
                    this.setAllRadioInGroupToSigner(this.selectedField.signerId);
                }
            }
        }

        this.previousName = this.selectedField.description
        this.isShown = false;
        this.shouldSetAllFieldsToSignerChanged = false
    }

    doAssignAllFields() {
        this.setAllFieldsToSigners(this.selectedField.signerId, this.selectedField.templateId);
        this.assignAllFieldsModal.showModal = false;
    }

    cancelAssignAllFields() {
        this.shouldSetAllFieldsToSigner = false;
        this.assignAllFieldsModal.showModal = false;
    }

    removeElement(arr: any[], index) {
        arr.splice(index, 1);
    }

    addElement(arr: any[], elem: any) {
        arr.push(elem);
    }

    mandatoryFieldChanged(e) {

        if (this.isRadio) {
            let radiosInGroup = this.allRadiosPageField.filter(x => x.groupName == (this.selectedField as RadioField).groupName && x.templateId == this.selectedField.templateId);
            radiosInGroup.forEach(radio => radio.mandatory = this.selectedField.mandatory);
        }
        if (this.isText) {
            let textInGroup = this.allTextPageField.filter(x => x.description == this.selectedField.description && x.templateId == this.selectedField.templateId);
            textInGroup.forEach(text => text.mandatory = this.selectedField.mandatory);
        }
    }

    setAllFieldsToSigners(signerId, templateId) {
        if (this.isSignature) {
            this.assignService.setSignatureFieldsToSigner(signerId, templateId);
            this.updateStateWithSignerId(signerId, true);
        }
        else {
            this.assignService.setFieldsToSigner(signerId, templateId);
            this.updateStateWithSignerId(signerId, false);
        }
    }


    setAllRadioInGroupToSigner(signerId) {
        let groupName = (<RadioField>this.selectedField).groupName
        this.assignService.setAllRadiosInGroupToSigner(signerId, groupName);
        this.updateStateWithSignerIdToRadioGroup(signerId, groupName);
    }

    updateStateWithSignerIdToRadioGroup(signerId, groupName) {

        this.stateService.getState().subscribe(
            (state: AppState) => {
                state.PageFields.forEach(
                    x => {

                        if (x instanceof RadioField && x.groupName == groupName) {
                            x.signerId = signerId;

                        }
                    }
                )
            }
        ).unsubscribe();
    }

    updateStateWithSignerId(signerId, isSignature) {
        this.stateService.getState().subscribe(
            (state: AppState) => {
                state.PageFields.forEach(
                    x => {
                        if ((isSignature && x instanceof SignatureField) ||
                            (!isSignature && !(x instanceof SignatureField))) {
                            x.signerId = signerId;

                        }
                    }
                )
            }
        ).unsubscribe();
    }

    copyToPages(copyToPages: number[]) {

        this.pages$.subscribe(pages => {

            pages.forEach(page => {
                if (copyToPages.length != 0) {
                    if (copyToPages.indexOf(page) == -1) {
                        return;
                    }
                }

                if (this.selectedField.page != page) {

                    let currentPagefield = this.getPageField();
                    currentPagefield.description = this.selectedField.description;
                    currentPagefield.height = this.selectedField.height;
                    currentPagefield.width = this.selectedField.width;
                    currentPagefield.mandatory = this.selectedField.mandatory;
                    currentPagefield.page = page;
                    let name = this.selectedField.name + "_" + page;
                    while (this.allSignturePageField.filter(item => item.name == name || item.description == name).length > 0 || this.allTextPageField.filter(item => item.name == name).length > 0) {
                        page = page * 10;
                        name = this.selectedField.name + "_" + page;
                    }

                    currentPagefield.name = name;
                    currentPagefield.signerId = this.selectedField.signerId;
                    currentPagefield.templateId = this.selectedField.templateId;
                    currentPagefield.x = this.selectedField.x;
                    currentPagefield.y = this.selectedField.y;
                    if (this.isSignature) {
                        currentPagefield.description = name;
                        (<SignatureField>currentPagefield).signingType = (<SignatureField>this.selectedField).signingType;
                        (<SignatureField>currentPagefield).signatureKind = (<SignatureField>this.selectedField).signatureKind;
                    }
                    else {
                        (<TextField>currentPagefield).textFieldType = (<TextField>this.selectedField).textFieldType;
                        (<TextField>currentPagefield).customerRegex = (<TextField>this.selectedField).customerRegex;
                        (<TextField>currentPagefield).value = (<TextField>this.selectedField).value;
                        //(<TextField>currentPagefield).length = (<TextField>this.selectedField).length;
                    }

                    this.store.dispatch(new fieldActions.AddPageFieldAction({
                        PageField: currentPagefield
                    }));
                    this.assignService.addField(currentPagefield);
                }
            });

        }).unsubscribe();
        this.submit();
    }

    SumbitToAllPages() {
        this.ShowDuplicateFields = true;
    }

    getPageField() {

        if (this.isSignature) {
            return new SignatureField();
        }
        else {
            return new TextField();
        }

    }

    changeFieldName(element) {
        if (this.selectedField.description != "") {
            let oldField = new PageField();
            oldField.name = element.origname;
            oldField.description = element.origname;
            oldField.signerId = this.selectedField.signerId;
            oldField.page = this.selectedField.page;
            oldField.templateId = this.selectedField.templateId;
            if(element.title == 'Radio'){
                this.selectedField.name = element.selectedField.description;
            }
            this.assignService.removeField(oldField);
            this.assignService.addField(this.selectedField);
        }
    }
}