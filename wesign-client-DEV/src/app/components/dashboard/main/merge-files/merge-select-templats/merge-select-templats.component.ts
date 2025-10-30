import { AfterViewInit, Component, ElementRef, EventEmitter, OnDestroy, OnInit, Output, ViewChild } from '@angular/core';
import { Errors } from '@models/error/errors.model';
import { TemplateFilter } from '@models/template-api/template-filter.model';
import { TemplateInfo } from '@models/template-api/template-info.model';
import { TemplateInfos } from '@models/template-api/template-infos.model';
import { PagerService } from '@services/pager.service';
import { SharedService } from '@services/shared.service';
import { TemplateApiService } from '@services/template-api.service';

import { Subscription, fromEvent } from 'rxjs';
import { debounceTime, distinctUntilChanged, map } from 'rxjs/operators';

@Component({
  selector: 'sgn-merge-select-templats',
  templateUrl: './merge-select-templats.component.html',
  styles: [
  ]
})
export class MergeSelectTemplatsComponent implements OnInit ,OnDestroy, AfterViewInit{
  @Output() public hide = new EventEmitter<any>();
  @Output() public selectTemplate = new EventEmitter<TemplateInfo>();
  @ViewChild('searchInput', { static: true }) searchInput: ElementRef;
  public templateFilter: TemplateFilter = new TemplateFilter();
  public templateInfos: TemplateInfos = new TemplateInfos();
  public pageCalc: any;
  public currentPage = 1;
  private PAGE_SIZE = 10;
  public templateCount: number;
  private templatesSubscription: Subscription;
  public showSearchSpinner: boolean = false; 
  constructor(  private templateApiService: TemplateApiService,
    private pager: PagerService, private sharedService: SharedService,) { }
  ngAfterViewInit(): void {
    fromEvent(this.searchInput.nativeElement, 'keyup').pipe(
      // get value
      map((event: any) => {
        return event.target.value;
      })
      // if character length greater then 2
      //,filter(res => res.length > 2)
      //Search function will not get called when the field is empty
      //,filter(Boolean)
      // Time in milliseconds between key events
      , debounceTime(1000)
      // If previous query is diffent from current   
      , distinctUntilChanged(),
    ).subscribe((text: string) => {
      this.currentPage = 1;
      this.showSearchSpinner = true;
      this.updateData(false);
    });
  }


  public ngOnDestroy() {    
    this.templatesSubscription.unsubscribe();    
}

  public updateData(showLoading) {
    
    this.templateFilter.Limit = this.PAGE_SIZE;
    this.templateFilter.Offset = (this.currentPage - 1) * this.PAGE_SIZE;
    if (showLoading){
    this.sharedService.setBusy(true, "TEMPLATE.LOADING")
    }

    this.templatesSubscription = this.templateApiService.getTemplates(this.templateFilter).subscribe((data) => {

        this.templateCount = +data.headers.get("x-total-count");
        this.pageCalc = this.pager.getPager(this.templateCount, this.currentPage, this.PAGE_SIZE);

        if (this.templateCount === 0) {
            this.templateInfos.templates.length = 0;
        } else {
            this.templateInfos = data.body;
            console.log(this.templateInfos );
            //this.templateInfos.templates.forEach((t) => t.userName = this.getUserById(t.userId));
        }
        
    }, error => {
        this.sharedService.setErrorAlert(new Errors(error.errors))
        
    },
    () => 
    {
        this.sharedService.setBusy(false);
       
        this.showSearchSpinner = false;
    }
    );
}

  public pageChanged(page)
  {
    this.currentPage = page;
    this.showSearchSpinner = true;
    this.updateData(false);
  }
  public sendTemplateToParent(template)
  {
        this.selectTemplate.emit(template);
  }
  public cancel()
  {
    this.hide.emit();
  }

  ngOnInit(): void {
    this.updateData(true);
  }

}
