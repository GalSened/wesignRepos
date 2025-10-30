import { Component, ElementRef, HostListener, Input, OnInit, Renderer2, ViewChild } from '@angular/core';
import { StoreOperationType } from 'src/app/enums/store-operation-type.enum';
import { TextFieldType } from 'src/app/enums/text-field-type.enum';
import { WeSignFieldType } from 'src/app/enums/we-sign-field-type.enum';
import { textField } from 'src/app/models/pdffields/field.model';
import { FieldRequest } from 'src/app/models/requests/fields-request.model';
import { AppState } from 'src/app/models/state/app-state.model';
import { StateService } from 'src/app/services/state.service';

@Component({
  selector: 'app-text-field',
  templateUrl: './text-field.component.html',
  styleUrls: ['./text-field.component.scss']
})
export class TextFieldComponent implements OnInit {

  @Input() textfield: textField;
  @Input() documentId: string;
  @Input() public pageHeight: number;
  @Input() public pageWidth: number;
  @Input() public disabled: boolean;
  public top: number;
  public left: number;
  public width: number;
  public height: number;
  public fieldType: string = "text";
  public isValid: boolean = false;
  public pattern: string = null;
  public placeHolder: string = "";
  public iFeatherName: string = "type";
  public appState: AppState;
  private isFocus: boolean = false
  private currentDisplayDoc: number;
  public description: string
  public isMultilineText;
  @ViewChild("currentInput", { static: false }) public inputUIElement: ElementRef;

  constructor(public el: ElementRef, public renderer: Renderer2, private stateService: StateService) { }

  ngOnInit(): void {
    this.height = this.textfield.height * this.pageHeight;
    this.width = this.textfield.width * this.pageWidth;
    this.top = this.textfield.y * this.pageHeight;
    this.left = this.textfield.x * this.pageWidth;
    this.description = this.textfield.description
    this.isMultilineText = this.textfield.textFieldType == TextFieldType.Multiline ? true : false;
    this.loadPattern();
    this.loadPlaceHolder();
    this.stateService.state$.subscribe((data) => {
      if (data.documentsData) {
        let docData = data.documentsData.find(x => x.documentId == this.documentId);
        let textFieldList = docData.pdfFields.textFields;

        this.currentDisplayDoc = data.documentsData.indexOf(docData);
        if (textFieldList && textFieldList.length > 0) {
          let field = textFieldList.find(x => x.name == this.textfield.name)
          if (field) {
            let curr = textFieldList.find(x => x.name == this.textfield.name);
            if (curr.textFieldType == TextFieldType.Date && !this.disabled) {
              let d = new Date(curr.value);
              let day = ("0" + d.getDate()).slice(-2);
              let month = d.getMonth();
              month = month + 1;
              let monthString = ("0" + month).slice(-2);
              let year = d.getFullYear() >= 1000 || isNaN(d.getFullYear()) ? d.getFullYear() : this.FillYearFromDate(d);
              this.textfield.value = `${year}-${monthString}-${day}`
            } else {
              this.textfield.value = curr.value;
            }
          }

          else {
            this.DoForLocalField();
          }
        }

        else {
          this.DoForLocalField();
        }

        this.changeDirection(this.textfield.value);
        this.appState = data;

        if (data.storeOperationType == StoreOperationType.SetCurrentJumpedField && !this.isFocus && data.currentJumpedField.name == this.textfield.name && data.currentJumpedField.page == this.textfield.page && data.currentJumpedField.yLocation == this.textfield.y) {
          this.isFocus = true;
          this.inputUIElement.nativeElement.focus()

        }
        else {
          this.isFocus = false;
        }
      }
    });
  }

  focusField(event) {
    this.stateService.SetCurrentJumpedField({ page: this.textfield.page, yLocation: this.textfield.y, name: this.textfield.name, displayDoc: this.currentDisplayDoc });
  }

  isValidInput() {
    setTimeout(() => {
      let element = <HTMLInputElement>document.getElementById(this.textfield.name);
      this.isValid = element?.validity.valid;
    });
  }

  @HostListener("change")
  public onChanged() {
    let fieldData = new FieldRequest();
    fieldData.fieldName = this.textfield.name;
    fieldData.fieldType = WeSignFieldType.TextField;
    fieldData.fieldValue = this.textfield.value;
    fieldData.fieldDescription = this.textfield.description;
    this.stateService.setFieldData(this.documentId, fieldData);
  }
  private changeDirection(e) {
    var rtlRegex = /[\u0591-\u07FF\uFB1D-\uFDFD\uFE70-\uFEFC]/;
    var isRtl = rtlRegex.test(String.fromCharCode(e.which));
    var direction = isRtl ? 'rtl' : 'ltr';
    this.renderer.setAttribute(this.el.nativeElement, "dir", direction);
  }

  @HostListener("keypress", ["$event"])
  public onTextEntered(e) {
    // if (e.keyCode != 32) {
    //   this.changeDirection(e);
    // }

  };

  DoForLocalField() {
    if (this.textfield) {

      if (this.textfield.textFieldType == TextFieldType.Date && !this.disabled) {
        let d = new Date(this.textfield.value);
        let day = ("0" + d.getDate()).slice(-2);
        let month = d.getMonth();
        month = month + 1;
        let monthString = ("0" + month).slice(-2);
        let year = d.getFullYear() >= 1000 || isNaN(d.getFullYear()) ? d.getFullYear() : this.FillYearFromDate(d);
        this.textfield.value = `${year}-${monthString}-${day}`
      } else {
        this.textfield.value = this.textfield.value;
      }
    }
  }

  loadPattern() {
    if (this.textfield.textFieldType == TextFieldType.Date) {
      if (!this.disabled) {
        this.fieldType = "date";
      }

      //      this.pattern = "^([0-2][0-9]|(3)[0-1])(\/)(((0)[0-9])|((1)[0-2]))(\/)\d{4}$";
      this.iFeatherName = "calendar";
      return;
    }

    if (this.textfield.textFieldType == TextFieldType.Number) {
      // this.fieldType = "number";
      //this.pattern = "^[0-9]*$";
      this.iFeatherName = "hash"
    }

    if (this.textfield.textFieldType == TextFieldType.Phone) {
      this.fieldType = "tel";
      this.pattern = "^[0-9]*$";
      this.iFeatherName = "phone";
      return;
    }

    if (this.textfield.textFieldType == TextFieldType.Email) {
      this.fieldType = "email";
      this.pattern = "^[a-z0-9._%+-]+@[a-z0-9.-]+\.[a-z]{2,4}$";
      this.iFeatherName = "mail";
      return;
    }

    if (this.textfield.textFieldType == TextFieldType.Custom) {
      this.pattern = this.textfield.customerRegex;
      return;
    }

    if (this.textfield.textFieldType == TextFieldType.Time) {
      this.fieldType = "time";
      //this.pattern = "(((2[0-3])|(1[0-9])|0[1-9]):([0-5][0-9]))";
      this.iFeatherName = "clock";
    }
  }

  loadPlaceHolder() {
    if (this.textfield.textFieldType == TextFieldType.Date) {
      this.placeHolder = 'DD/MM/YYYY';
      return;
    }

    if (this.textfield.textFieldType == TextFieldType.Number) {
      this.placeHolder = '123456';
      return;
    }

    if (this.textfield.textFieldType == TextFieldType.Phone) {
      this.placeHolder = '05X-XXXXXXX';
      return;
    }

    if (this.textfield.textFieldType == TextFieldType.Email) {
      this.placeHolder = 'example@domain.com';
      return;
    }

    if (this.textfield.textFieldType == TextFieldType.Time) {
      this.placeHolder = 'HH:MM';
    }
  }

  public onKeyPress(e) {
    if (this.textfield.textFieldType != TextFieldType.Number)
      return
    var characters = String.fromCharCode(e.which);
    if (e.target.value != "" || characters != "-")
      if (!(/[0-9]/.test(characters))) {
        e.preventDefault();
      }
  }

  public onPaste(e) {
    if (this.textfield.textFieldType != TextFieldType.Number)
      return

    e.stopPropagation();
    e.preventDefault();
    let clipboardData = e.clipboardData
    let pastedData = clipboardData.getData('Text');
    if ((/^-?[0-9]*$/.test(e.target.value + pastedData)))
      e.target.value += pastedData;

  }

  FillYearFromDate(date): String {
    let yearString = date.getFullYear().toString();
    while (yearString.length < 4) {
      yearString = "0" + yearString;
    }
    return yearString
  }
}