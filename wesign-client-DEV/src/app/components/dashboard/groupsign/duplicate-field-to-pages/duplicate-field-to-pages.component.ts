import { Component, EventEmitter, Input, Output } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'sgn-duplicate-field-to-pages',
  templateUrl: './duplicate-field-to-pages.component.html',
  styles: [
  ]
})
export class DuplicateFieldToPagesComponent {
public title:string = "This is a title";
@Input() public isShown: boolean = false;
public allSelected: boolean = true;
public selectedPages: string = "";
public errorMsg: string = "";
@Output() public accept = new EventEmitter<any>();
@Output() public closePopup = new EventEmitter<void>();
public cancel(){
  this.errorMsg  = "";
  this.allSelected = true;
  this.selectedPages= "";
  this.closePopup.emit();
}
constructor(private translate: TranslateService
) { }



parseSelectedPages(input: string): number[] {
  const pages: number[] = [];
  const parts = input.split(',');

  for (const part of parts) {
    if (part.includes('-')) {
      const [start, end] = part.split('-').map(Number);
      if (isNaN(start) || isNaN(end) || start > end) {
        this.translate.get(`FIELDS.ERROR_IN_SELECTED_PAGES`).subscribe(
          (msg) => {
            this.errorMsg = msg;
            
          });   
           
      }
      for (let i = start; i <= end; i++) {
        if (!pages.includes(i)) {
            pages.push(i);
        }
      }
    } else {
      const page = Number(part);
      if (isNaN(page)) {
        this.translate.get(`FIELDS.ERROR_IN_SELECTED_PAGES`).subscribe(
          (msg) => {
            this.errorMsg = msg;
            
          });   
           
      }
      if (!pages.includes(page)) {
        pages.push(page);
      }
    }
  }

  return pages;
}


public submit(){
  this.errorMsg  = "";
  let pages = [];
  
  if(!this.allSelected){
    if(this.selectedPages)
      pages =  this.parseSelectedPages(this.selectedPages);
    if(this.errorMsg || pages.length == 0 ){
      return;
    }
  
}



let result = {allSelected: this.allSelected, pages: pages};
  this.accept.emit(result);
  this.selectedPages= "";
  this.allSelected = true;
  
}
}
