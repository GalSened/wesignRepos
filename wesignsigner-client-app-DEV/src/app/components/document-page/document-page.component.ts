import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { DocumentPage } from 'src/app/models/responses/document-page.model';

@Component({
  selector: 'app-document-page',
  templateUrl: './document-page.component.html',
  styleUrls: ['./document-page.component.scss']
})
export class DocumentPageComponent implements OnInit {

  @Input() page: DocumentPage;
  @Input() pageNumber: number;
  @Input() isSender: boolean = false;
  @Output() public changePageScroll = new EventEmitter<number>();
  
  constructor() { }

  public onMouseEnter()
  {
      this.changePageScroll.emit(this.pageNumber);
  }

  ngOnInit(): void {
  }
}