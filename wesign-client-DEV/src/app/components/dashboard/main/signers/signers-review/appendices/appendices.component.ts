import { Component, ElementRef, EventEmitter, Input, OnInit, Output, ViewChild } from '@angular/core';
import { Appendix, DocumentCollectionCreateRequest, DocumentSigner } from '@models/document-api/document-create-request.model';

@Component({
  selector: 'sgn-appendices',
  templateUrl: './appendices.component.html',
  styles: []
})
export class AppendicesComponent implements OnInit {

  @Output() public hide = new EventEmitter<any>();
  @Output() public getDocumentAppendices = new EventEmitter<Appendix[]>();
  @Input() signer: DocumentSigner;

  @ViewChild("fileInput", { static: true }) public el: ElementRef;
  public file: any = null;
  @Input() public senderAppendices: Appendix[];
  fileName: string;
  currIndex: number = 0;
  @Input()
  documentAppendices: Appendix[];

  constructor() { }

  ngOnInit() {
    if (!this.signer.senderAppendices) {
      this.senderAppendices = [];
    } else {
      this.senderAppendices = this.signer.senderAppendices;
      this.currIndex = this.signer.senderAppendices.length;
    }
    this.senderAppendices = this.senderAppendices.concat(this.documentAppendices);
    this.senderAppendices = this.removeDuplication(this.senderAppendices);
    this.senderAppendices.push(new Appendix());
  }

  //Remove equals objects(all properties equals). and not remove duplication by a key.
  removeDuplication(fieldsArr: Appendix[]) {
    let newArr = [];

    fieldsArr.forEach(x => {

      let index = newArr.findIndex(a => a.name == x.name);
      if (index < 0) {
        newArr.push(x);
      }
      else {
        let item = newArr[index];
        if (x.name != item.name || x.base64file != item.base64file) {
          newArr.push(x);
        }
      }
    }
    );

    return newArr;
  }



  public send() {
    this.senderAppendices.pop();
    if (this.senderAppendices.length > 0) {
      this.signer.senderAppendices = this.senderAppendices.filter(x => !this.documentAppendices.includes(x));
      for (let index = 0; index < this.signer.senderAppendices.length; index++) {
        this.signer.senderAppendices[index].name = this.sanitizeFilename(this.signer.senderAppendices[index].name); 
      }
    }
    else {
      this.signer.senderAppendices = [];
    }
    this.getDocumentAppendices.emit(this.documentAppendices);
    this.hide.emit();
  }

  public close() {
    this.hide.emit();
  }

  public fileDropped() {

    if (this.el.nativeElement.files.length > 0) {
      this.file = this.el.nativeElement.files[0];
      let appendix = new Appendix();
      appendix.name = this.file.name.split('.').slice(0, -1).join('.');

      const reader = new FileReader();
      reader.readAsDataURL(this.file);
      reader.onload = () => {
        appendix.base64file = reader.result.toString();
        if (appendix.base64file.includes('application/octet-stream') && this.file.name.endsWith(".msg")) {
          appendix.base64file = appendix.base64file.replace('application/octet-stream', 'application/vnd.ms-outlook');
        }
        this.senderAppendices[this.currIndex] = appendix;
        this.addAppendix();
      };

    }
  }

  public addAppendix() {
    if (!this.isAppendixEmpty(this.senderAppendices.length - 1)) {
      this.senderAppendices.push(new Appendix());
      this.currIndex++;
    }
  }

  public removeAppendix(index) {
    if (index == this.currIndex && this.isAppendixEmpty(index)) {
      return;
    }
    let lastIndex = this.senderAppendices.length - 2;
    lastIndex = lastIndex == 0 ? 1 : lastIndex;
    if (lastIndex > 0) {
      this.senderAppendices[index] = this.senderAppendices[lastIndex];
      if (this.isAppendixEmpty(this.senderAppendices.length - 1)) {
        this.senderAppendices.pop();
        this.currIndex--;
      }
      this.senderAppendices.pop();
      this.senderAppendices.push(new Appendix());
    }
  }


  private isAppendixEmpty(index) {
    return !this.senderAppendices[index].name && !this.senderAppendices[index].base64file;
  }

  public AddAppendixToAllSigners(appendix, index) {
    if (!appendix || !appendix.name) {
      return;
    }
    if ((<HTMLInputElement>document.getElementById(index)).checked) {
      this.documentAppendices.push(appendix);
    }
    else {
      this.documentAppendices = this.documentAppendices.filter(x => x.name != appendix.name)
    }
  }

  private sanitizeFilename(filename: string): string {
    // Define a regular expression that matches all illegal characters except Hebrew characters
    const illegalChars = /[#%&{}\\<>*?/$!'"`:@+`|=]|[^\x00-\x7F\u0590-\u05FF]/g;
    
    // Remove illegal characters from the filename
    filename = filename.replace(illegalChars, '');
    
    // Replace spaces with underscores
    filename = filename.replace(/\s+/g, '_');
    
    return filename;
  }
}

