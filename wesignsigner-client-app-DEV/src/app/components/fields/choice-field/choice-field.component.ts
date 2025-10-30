import { Component, HostListener, Input, OnInit } from '@angular/core';
import { Observable } from 'rxjs';
import { StoreOperationType } from 'src/app/enums/store-operation-type.enum';
import { WeSignFieldType } from 'src/app/enums/we-sign-field-type.enum';
import { choiceField } from 'src/app/models/pdffields/field.model';
import { FieldRequest } from 'src/app/models/requests/fields-request.model';
import { AppState } from 'src/app/models/state/app-state.model';
import { StateService } from 'src/app/services/state.service';

@Component({
  selector: 'app-choice-field',
  templateUrl: './choice-field.component.html',
  styleUrls: ['./choice-field.component.scss']
})
export class ChoiceFieldComponent implements OnInit {

  @Input() choice :choiceField;
  @Input() public pageHeight:number;
  @Input() public pageWidth :number;
  @Input() documentId: string;
  @Input() public disabled: boolean ;
  public isValid : boolean = true;
  public appState: AppState;
 
  top: number;
  left: number;
  width: number;
  height: number;
  hasOption :  boolean;
  
  constructor(public stateService: StateService) { }

  ngOnInit(): void {
    this.hasOption  = this.choice.options.length > 0;
    this.height = this.choice.height * this.pageHeight;
    this.width = this.choice.width * this.pageWidth;
    this.top  = this.choice.y* this.pageHeight;
    this.left  = this.choice.x* this.pageWidth;
    if(this.choice.mandatory && this.choice.selectedOption == ""){
      this.isValid = false;
    }

    this.stateService.state$.subscribe((data) => {
      this.appState = data;
      if(data.storeOperationType == StoreOperationType.SetCurrentJumpedField && data.currentJumpedField.name == this.choice.name && data.currentJumpedField.page == this.choice.page && data.currentJumpedField.yLocation == this.choice.y)
      {
        let element = <HTMLInputElement>document.getElementById(this.choice.name+ "_parent");
        let selectelement = <HTMLInputElement>document.getElementById(this.choice.name);
        element.scrollIntoView(false);
        selectelement.focus();
      }
      
    });
  }

  @HostListener("change")
  public onChanged() {
    if(this.choice.mandatory && this.choice.selectedOption == ""){
      this.isValid = false;
    }
    else{
      this.isValid = true;
    }

    let fieldData = new FieldRequest();
    fieldData.fieldName = this.choice.name;
    fieldData.fieldType = WeSignFieldType.ChoiceField;
    fieldData.fieldValue = this.choice.selectedOption;
   
    let element = document.getElementById(this.choice.name + "_parent");
    element.classList.remove("is-error");

    this.stateService.setFieldData(this.documentId, fieldData);
  }
}
