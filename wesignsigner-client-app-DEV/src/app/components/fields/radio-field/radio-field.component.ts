import { Component, HostListener, Input, OnInit } from '@angular/core';
import { element } from 'protractor';
import { StoreOperationType } from 'src/app/enums/store-operation-type.enum';
import { WeSignFieldType } from 'src/app/enums/we-sign-field-type.enum';
import { radioField } from 'src/app/models/pdffields/field.model';
import { FieldRequest } from 'src/app/models/requests/fields-request.model';
import { AppState } from 'src/app/models/state/app-state.model';
import { StateService } from 'src/app/services/state.service';

@Component({
  selector: 'app-radio-field',
  templateUrl: './radio-field.component.html',
  styleUrls: ['./radio-field.component.scss']
})
export class RadioFieldComponent implements OnInit {

  top: number;
  left: number;
  width: number;
  height: number;
  appState: AppState;
  hasValueForGroup: boolean = false;
  @Input() radio: radioField;
  @Input() groupname: string;
  @Input() pageHeight: number;
  @Input() pageWidth: number;
  @Input() documentId: string;
  @Input() selected: string;
  @Input() disabled: boolean;

  constructor(public stateService: StateService) { }

  ngOnInit(): void {
    this.height = this.radio.height * this.pageHeight;
    this.width = this.radio.width * this.pageWidth;
    this.top = this.radio.y * this.pageHeight;
    this.left = this.radio.x * this.pageWidth;
    this.stateService.state$.subscribe((data) => {
      if (data.documentsData) {
        this.appState = data;
      }

      let radiosElements = (<NodeListOf<HTMLInputElement>>document.getElementsByName(this.groupname));
      radiosElements.forEach(element => {
        if (element.checked) {
          this.hasValueForGroup = true;
        }
      });

      if (data.storeOperationType == StoreOperationType.SetCurrentJumpedField &&
        data.currentJumpedField.name == this.radio.name &&
        data.currentJumpedField.page == this.radio.page &&
        data.currentJumpedField.yLocation == this.radio.y) {
        let element = <HTMLInputElement>document.getElementById(this.groupname + "_" + data.currentJumpedField.name);
        element.scrollIntoView(false);
        element.classList.add("is-mandatory");
        element.focus();
      }
    });
  }

  @HostListener("change")
  public onChanged() {
    let fieldData = new FieldRequest();
    fieldData.fieldName = this.groupname;
    fieldData.fieldType = WeSignFieldType.RadioGroupField;
    fieldData.fieldValue = this.radio.name;

    let elements = document.getElementsByTagName("input")
    var arr = [].slice.call(elements)
    arr = arr.filter(x => x.id.includes(this.groupname));

    arr.forEach(element => {
      element.parentElement.classList.remove("is-error");
    })

    this.stateService.setFieldData(this.documentId, fieldData);
  }
}