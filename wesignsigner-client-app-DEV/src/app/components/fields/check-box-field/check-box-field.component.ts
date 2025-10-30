import { Component, ElementRef, HostListener, Input, OnInit, Renderer2 } from '@angular/core';
import { StoreOperationType } from 'src/app/enums/store-operation-type.enum';
import { WeSignFieldType } from 'src/app/enums/we-sign-field-type.enum';
import { checkBoxField } from 'src/app/models/pdffields/field.model';
import { FieldRequest } from 'src/app/models/requests/fields-request.model';
import { StateService } from 'src/app/services/state.service';

@Component({
  selector: 'app-check-box-field',
  templateUrl: './check-box-field.component.html',
  styleUrls: ['./check-box-field.component.scss']
})
export class CheckBoxFieldComponent implements OnInit {

  @Input() public checkbox: checkBoxField;
  @Input() public pageHeight: number;
  @Input() documentId: string;
  @Input() public pageWidth: number;
  @Input() public disabled: boolean ;
  top: number;
  left: number;
  width: number;
  height: number;
  
  data: string = "";
  constructor(public stateService: StateService) {
    
  }


  onNoClick(event: Event): void {
    event.preventDefault();    
  }
  
  ngOnInit(): void {
    this.height = this.checkbox.height * this.pageHeight;
    this.width = this.checkbox.width * this.pageWidth;
    this.top = this.checkbox.y * this.pageHeight;
    this.left = this.checkbox.x * this.pageWidth;
    this.stateService.state$.subscribe((data) => {
      let element = <HTMLInputElement>document.getElementById(this.checkbox.name+ "_parent");
      let selectelement = <HTMLInputElement>document.getElementById(this.checkbox.name);
      if(data.storeOperationType == StoreOperationType.SetCurrentJumpedField &&  data.currentJumpedField.name == this.checkbox.name && data.currentJumpedField.page == this.checkbox.page && data.currentJumpedField.yLocation == this.checkbox.y)
      {
        
        element.scrollIntoView(false);
        element.classList.add("is-mandatory");
        selectelement.focus();
      }
      else
      {
        
        if(element)
        {
          if(element.classList.contains("is-mandatory"))
          {
          element.classList.remove("is-mandatory");
         }
        } 
      }
    });
  }


  @HostListener("change")
  public onChanged() {
    let fieldData = new FieldRequest();
    fieldData.fieldName = this.checkbox.name;
    fieldData.fieldDescription = this.checkbox.description;
    fieldData.fieldType = WeSignFieldType.CheckBoxField;
    fieldData.fieldValue = String(this.checkbox.isChecked);
    this.stateService.setFieldData(this.documentId, fieldData);
    

  }
}
