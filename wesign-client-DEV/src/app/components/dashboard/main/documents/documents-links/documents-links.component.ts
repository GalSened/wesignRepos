import { Component, HostBinding, OnInit } from '@angular/core';
import { DocFilter } from '@models/document-api/doc-filter.model';
import { UserSigningLinksResponse } from '@models/document-api/user-signing-links-response.model';
import { SignMode } from '@models/enums/sign-mode.enum';
import { LinksApiService } from '@services/links-api.service';
import { PagerService } from '@services/pager.service';
import { SharedService } from '@services/shared.service';

@Component({
  selector: 'sgn-documents-links',
  templateUrl: './documents-links.component.html',
  styles: []
})
export class DocumentsLinksComponent implements OnInit {
  @HostBinding('style.width') width = '100%';

  public docFilter: DocFilter = new DocFilter();
  public pageCalc: any;
  private PAGE_SIZE = 10;
  currentPage: number = 1;
  documentsCount: number;
  docs: UserSigningLinksResponse;
  public SignMode = SignMode;
  showOpenLink : boolean = false;
  currDocId: string;
  public orderByField: string = 'creationTime';
  public orderByDesc: boolean = true;
  public showSearchSpinner: boolean = false;
  constructor(private linksApiService: LinksApiService,
    private pager: PagerService,
    private sharedService: SharedService) { }

  ngOnInit() {
    this.updateData(true);
  }

  public pageChanged(page: number) {
    this.showSearchSpinner = true;
    this.currentPage = page;
    this.updateData(false);
  }

  public updateData(showLoading) {
    this.docFilter.limit = this.PAGE_SIZE;
    this.docFilter.offset = (this.currentPage - 1) * this.PAGE_SIZE;
    if(showLoading)
    {
      this.sharedService.setBusy(true, "DOCUMENT.LOADING")
    }
    this.linksApiService.getDocuments(this.docFilter).subscribe(
      (data) => {
        this.documentsCount = +data.headers.get("x-total-count");
        this.pageCalc = this.pager.getPager(this.documentsCount, this.currentPage, this.PAGE_SIZE);
        this.docs = data.body;
        
      },
      (error) => {
       
      },
      () => {
        this.sharedService.setBusy(false);
        this.showSearchSpinner = false;
      }
    );
  }
  public onDropDownSelect(value: number) {
    this.PAGE_SIZE = value;
    this.updateData(true)
}

  public onSignClick(docId: string) {
    this.showOpenLink = ! this.showOpenLink;
    this.currDocId = docId;
  }

  public openLink(){
    window.open(this.docs.documentCollections.find(x => x.documentCollectionId == this.currDocId).signingLink);
    this.closePopUp();
  }

  public closePopUp(){
    this.showOpenLink  = ! this.showOpenLink;
  }

  public getLocalTime(utcTime: string) {
    return this.sharedService.getLocalTime(utcTime);
  }

  public orderByFunction(prop: string) {
    if (prop) {
        if (this.orderByField == prop) {
            this.orderByDesc = !this.orderByDesc;
        }
        this.orderByField = prop;
    }
  }
  public trackByFn(index, item) {
    return index;
}
}
