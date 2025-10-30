import { Component, ElementRef, EventEmitter, HostListener, Input, OnInit, Output, ViewChild } from '@angular/core';

@Component({
  selector: 'sgn-upload-file-area',
  templateUrl: './upload-file-area.component.html',
  styles: []
})
export class UploadFileAreaComponent implements OnInit {
  public isFileLoded: boolean = false;
  @Input() acceptTypes: string = "";  
  @Output() public fileUploadEvent = new EventEmitter<any>();
  @ViewChild("fileInput", { static: true }) public el: ElementRef;

  constructor() { }

  ngOnInit() {
  }

  public fileUpload(){
    if (this.el.nativeElement.files.length > 0) {
      let file = this.el.nativeElement.files[0];
      this.fileUploadEvent.emit(file);
    }
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
    let files = evt.dataTransfer.files;
    if(files.length > 0 ){
      let file = files[0];
      this.fileUploadEvent.emit(file);
    }
  }

}
