import { Component, ElementRef, EventEmitter, HostListener, Input, OnInit, Output, ViewChild } from '@angular/core';
import { UploadRequest } from '@models/template-api/upload-request.model';

@Component({
  selector: 'sgn-merge-file-selector',
  templateUrl: './merge-file-selector.component.html',
  styles: [
  ]
})
export class MergeFileSelectorComponent implements OnInit {
  @ViewChild("fileInput", { static: true }) public el1: ElementRef;
  @Input() public name:string;
  @Input() public title:string;
  @Output() public clear = new EventEmitter<any>();
  @Output() public dataSelect = new EventEmitter<any>();
  public acceptTypes : string = ".jpg,.jpeg,.pdf,.docx,.png";
  public showTemplateSelector : boolean = false;
  public isFileLoded : boolean;
  public fileName:string ="";
  public fileUpload()
  {


   if(this.el1.nativeElement.files.length > 0) {

      let file = this.el1.nativeElement.files[0];

      if (file) {
      this.readFile(file);
            
          
        };
    } 

  }

  readFile(file)
  {

    const reader = new FileReader();
    reader.readAsDataURL(file);     
    reader.onload = () => {
      
        this.fileName = file.name.split(".")[0].slice(0, 50);
        
      this.dataSelect.emit(reader.result.toString());
  }
}
 
  constructor() { }

  public clearFile()
  {
    this.fileName = "";
    this.clear.emit();
  }
  ngOnInit(): void {
  }

  public selectTemplate(template)
  {    
      this.showTemplateSelectorForm();
      this.fileName = template.name;
      this.dataSelect.emit(template.templateId);
  }

  public showTemplateSelectorForm()
  {
    this.showTemplateSelector = !this.showTemplateSelector;
  }
  @HostListener('dragover',['$event']) OnDragOver(evt){
    evt.preventDefault();
    evt.stopPropagation();
    //console.log("Drag over")
  }

  @HostListener('dragleave',['$event']) OnDragLeave(evt){
    evt.preventDefault();
    evt.stopPropagation();
    //console.log("Drag Leave")
  }

  @HostListener('drop',['$event']) OnDrop(evt){
    evt.preventDefault();
    evt.stopPropagation();
    console.log("fileDropped");
    let files = evt.dataTransfer.files;
    if(files.length > 0 ){
      let file = files[0];
      if (file) {
      this.readFile(file);
      }
      // need to know the item drop
    }
  }

}
