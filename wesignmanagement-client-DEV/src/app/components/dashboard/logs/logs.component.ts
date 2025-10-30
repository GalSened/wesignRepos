import { Observable, fromEvent } from 'rxjs';
import { LogFilter } from './../../../models/log-filter.model';
import { Component, OnInit, ElementRef, ViewChild } from '@angular/core';
import { LogsApiService } from 'src/app/services/logs-api.service';
import { LogResult } from 'src/app/models/results/log-result.model';
import { tap, map, debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { PagerService } from 'src/app/services/pager.service';
import { LogLevel } from 'src/app/enums/log-level.enum';
import { PaginatorComponent } from '../../shared/paginator.component';

@Component({
  selector: 'app-logs',
  templateUrl: './logs.component.html',
  styleUrls: ['./logs.component.css']
})
export class LogsComponent implements OnInit {
  public isFromCalendarShown = false;
  public isToCalendarShown = false;
  public logsFilter: LogFilter = new LogFilter();
  private PAGE_SIZE = 10;
  public currentPage = 1;
  public pageCalc: any;
  public logs$: Observable<LogResult[]>;
  public logLevelOptions = { 0: "All", 1: "Debug", 2: "Information", 3: "Error" };
  public appSourceOptions = {0: "User Application", 1: "Signer Application", 2: "Management Application"};
  public LogLevel  = LogLevel; 
  public hours = { 0: "00", 1: "01", 2: "02", 3: "03", 4: "04", 5: "05", 6: "06", 7: "07", 8: "08", 9: "09", 10: "10", 11: "11", 12: "12", 
  13: "13", 14: "14",15: "15", 16: "16", 17: "17", 18: "18", 19: "19", 20: "20", 21: "21", 22: "22", 23: "23"};
  public mintues = {0: "00", 1: "01", 2: "02", 3: "03", 4: "04", 5: "05", 6: "06", 7: "07", 8: "08", 9: "09",
  10: "10", 11: "11", 12: "12", 13: "13", 14: "14", 15: "15", 16: "16", 17: "17", 18: "18", 19: "19",
  20: "20", 21: "21", 22: "22", 23: "23", 24: "24", 25: "25", 26: "26", 27: "27", 28: "28", 29: "29",
  30: "30", 31: "31", 32: "32", 33: "33", 34: "34", 35: "35", 36: "36", 37: "37", 38: "38", 39: "39",
  40: "40", 41: "41", 42: "42", 43: "43", 44: "44", 45: "45", 46: "46", 47: "47", 48: "48", 49: "49",
  50: "50", 51: "51", 52: "52", 53: "53", 54: "54", 55: "55", 56: "56", 57: "57", 58: "58", 59: "59", 60 : "60"  } 

  public inputText : string = "";
  @ViewChild('searchInput', { static: true }) searchInput: ElementRef;
  @ViewChild(PaginatorComponent) paginator:PaginatorComponent; 
  
  constructor(private logsApi: LogsApiService,
    private pager: PagerService) { }

  ngOnInit(): void {
    this.updateData();
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
      , distinctUntilChanged()
    ).subscribe((text: string) => {
      this.inputText = text;
      this.updateData(true);
      this.paginator.reset();

    });
    for (let index = 10; index < 60; index++){
      this.mintues
    }
  }

  public pageChanged(page: number) {
    this.currentPage = page;
    this.updateData();
  }

  private updateData(shouldReset = false) {
    this.logsFilter.limit = this.PAGE_SIZE;
    this.logsFilter.offset =  shouldReset ? 0 : (this.currentPage - 1) * this.PAGE_SIZE;
    this.logs$ = this.logsApi.Read(this.logsFilter).pipe(
      tap((data) => {
        const total = +data.headers.get("x-total-count");
        this.pageCalc = this.pager.getPager(total, this.currentPage, this.PAGE_SIZE);
      }),
      map((res) => res.body.logs),
    );
  }

  public showFromCalendar() {
    this.isFromCalendarShown = !this.isFromCalendarShown;
    this.isToCalendarShown = false;
  } 

  public showToCalendar() {
    this.isToCalendarShown = !this.isToCalendarShown;
    this.isFromCalendarShown = false;
  }

  public dropDownUpdateTime(newValue, timeType, dateType){
    if(timeType == 'hours' && dateType == 'from' && this.logsFilter.from){
      this.logsFilter.from.setHours(newValue);
    }
    else if(timeType == "mintues" && dateType == 'from' && this.logsFilter.from){
      this.logsFilter.from.setMinutes(newValue);
    }
    if(timeType == 'hours' && dateType == 'to' && this.logsFilter.to){
      this.logsFilter.to.setHours(newValue);
    }
    else if(timeType == "mintues" && dateType == 'to' && this.logsFilter.to){
      this.logsFilter.to.setMinutes(newValue);
    }
    this.updateData();
  }

  public fromSelected(date: Date) {
    this.logsFilter.from = date;
    this.updateData();
  }

  public removeDate($event: any, dateElement: any) {
    $event.preventDefault();
    this.logsFilter[dateElement] = null;
    this.updateData();
  }

  public toSelected(date: Date) {
    this.logsFilter.to = date;
    this.updateData();
  }

  public dropDownUpdate(newValue) {
    if (newValue && this.logsFilter.logLevel !== newValue) {
      this.logsFilter.logLevel = newValue;
      this.updateData();
    }
  }

  public applicationSourceDropDownUpdate(newValue) {
    if (newValue && this.logsFilter.applicationSource !== newValue) {
      this.logsFilter.applicationSource = newValue;
      this.updateData();
    }
  }

  //TODO limit from 
}
