import { Component, Input, OnInit } from '@angular/core';
import { radioFieldGroup } from 'src/app/models/pdffields/field.model';

@Component({
  selector: 'app-radio-group-field',
  templateUrl: './radio-group-field.component.html',
  styleUrls: ['./radio-group-field.component.scss']
})
export class RadioGroupFieldComponent implements OnInit {

  @Input() public radiogroup : radioFieldGroup;
  @Input() public pageHeight:number;
  @Input() public pageWidth :number;
  @Input() public documentId: string; 
  @Input() public disabled: boolean ;
  top: number;
  left: number;
  width: number;
  height: number;

  constructor() { }

  ngOnInit(): void {
  }
}