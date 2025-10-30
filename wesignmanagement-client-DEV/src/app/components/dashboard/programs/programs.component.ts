import { ProgramsApiService } from './../../../services/programs-api.service';
import { ProgramFilter } from './../../../models/program-filter.model';
import { Component, OnInit, ElementRef, ViewChild, AfterViewInit } from '@angular/core';
import { Program } from 'src/app/models/program.model';
import { Observable , fromEvent} from 'rxjs';
import { tap, debounceTime, map, distinctUntilChanged } from 'rxjs/operators';
import { PagerService } from 'src/app/services/pager.service';
import { BannerAlertComponent } from '../../shared/banner-alert/banner-alert.component';
import { Errors } from 'src/app/models/error/errors.model';
import { SharedService } from 'src/app/services/shared.service';
import { Store } from '@ngrx/store';
import { IAppState, AppState } from 'src/app/state/app-state.interface';
import { Operation } from 'src/app/enums/operaion.enum';
import { JsonPipe } from '@angular/common';
import { $ } from 'protractor';
import { Router } from '@angular/router';
import { ProgramFormComponent } from './program-form/program-form.component';
import { Console } from 'console';
import { PaginatorComponent } from '../../shared/paginator.component';

@Component({
  selector: 'app-programs',
  templateUrl: './programs.component.html',
  styleUrls: ['./programs.component.css']
})
export class ProgramsComponent implements OnInit, AfterViewInit {

  private PAGE_SIZE = 10;
  private SUCCESS = 1;
  private FAILED = 2;
  public currentPage = 1;
  public pageCalc: any;
  public isFormShown: boolean = false;
  public program: Program = new Program();
  public operation: Operation;
  public isSorted: boolean = false;

  public programs$: Observable<Program[]>;
  public programFilter: ProgramFilter = new ProgramFilter();
  public Options = { "true": "Yes", "false": "No" };
  public showDeletionAlert: boolean = false;
  public appState: AppState;
  @ViewChild(ProgramFormComponent) child:ProgramFormComponent;
  
  @ViewChild('searchInput', { static: true }) searchInput: ElementRef;
  @ViewChild('programForm', { static: false }) programForm: any;
  @ViewChild('bannerAlert', { static: true }) bannerAlert: BannerAlertComponent;

  @ViewChild(PaginatorComponent) paginator:PaginatorComponent; 


  constructor(private pager: PagerService,
              private programsApiService: ProgramsApiService,
              private sharedService : SharedService, 
              private router: Router,
              private store: Store<IAppState>            
              ) { }

  ngOnInit() {
    this.store.select<any>('appstate').subscribe((state: any)=>{
      this.appState = state;
      this.isFormShown = this.appState.ShouldShowProgramForm;
    });
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
        ,debounceTime(1000)        
        // If previous query is diffent from current   
        ,distinctUntilChanged()
        ).subscribe((text: string) => {          
          this.updateData(true);
          this.paginator.reset();

        });
    }

    ngAfterViewInit() {
      

    }

  public showProgramForm() {
    this.isFormShown = true;
    this.operation = Operation.Create;
    this.program = new Program();
  }

  public hideForm(newValue) {

    this.isFormShown = false;
    this.updateData();
  }

  public updateData(shouldReset = false) {
    this.programFilter.limit = this.PAGE_SIZE;
    this.programFilter.offset =  shouldReset ? 0 : (this.currentPage - 1) * this.PAGE_SIZE;
    this.programs$ = this.programsApiService.Read(this.programFilter).pipe(
      tap((data) => {
        const total = +data.headers.get("x-total-count");
        this.pageCalc = this.pager.getPager(total, this.currentPage, this.PAGE_SIZE);
    
      }),
      map((res) => res.body.programs)
      ,
    );
  }

  
  public pageChanged(page: number) {
    this.currentPage = page;
    this.updateData();
  }

  public hideAlert(){
    this.showDeletionAlert = false;
  }

  public openDeletionAlert() {
    this.showDeletionAlert = true;
  }

  public deleteProgram( program: Program){
    this.programsApiService.deleteProgram(program).subscribe((res) => {
      this.bannerAlert.showBannerAlert("Program Successfully Deleted", this.SUCCESS);
      this.updateData();      
    }, (err) => {
      let errorMessage = this.sharedService.getErrorMessage(new Errors(err.error));
      this.bannerAlert.showBannerAlert(errorMessage, this.FAILED);
    });
    this.hideAlert();
  }

  public showBannerAlert(event){
    this.bannerAlert.showBannerAlert("Program Successfully Created", 1);
    this.updateData();
  }

  public stringifyUnlimitedNumber(inputName : string){
    if(document.getElementById(inputName) != null && (<HTMLInputElement>document.getElementById(inputName)).value == '-1'){
      (<HTMLInputElement>document.getElementById(inputName)).value = "Unlimited";
    }
  }

  public editProgramUsingForm(currentProgram : Program){
    this.operation = Operation.Update;
    this.program = currentProgram;
    this.isFormShown = true;
    this.child.handleUnlimitedInputs(currentProgram);
  }

  public nameSort() {
    this.programs$ = this.programs$.pipe(map((data) => {
      data.sort((a, b) => {
        var programA = a.name.toLocaleLowerCase(), programB = b.name.toLocaleLowerCase()
        return programA < programB ? this.isSorted ? 1 : -1 : this.isSorted ? -1 : 1;
      });
      return data;
    }))
    this.isSorted = !this.isSorted;
  }

  public usersSort() {
    this.programs$ = this.programs$.pipe(map((data) => {
      data.sort((a, b) => {
        let num = this.isSorted ? 1 : -1;
        return num * (a.users - b.users);
      });
      return data;
    }))
    this.isSorted = !this.isSorted;
  }

  public templatesSort() {
    this.programs$ = this.programs$.pipe(map((data) => {
      data.sort((a, b) => {
        let num = this.isSorted ? 1 : -1;
        return num * (a.templates - b.templates);
      });
      return data;
    }))
    this.isSorted = !this.isSorted;
  }

  public documentsSort() {
    this.programs$ = this.programs$.pipe(map((data) => {
      data.sort((a, b) => {
        let num = this.isSorted ? 1 : -1;
        return num * (a.documentsPerMonth - b.documentsPerMonth);
      });
      return data;
    }))
    this.isSorted = !this.isSorted;
  }

  public smsSort() {
    this.programs$ = this.programs$.pipe(map((data) => {
      data.sort((a, b) => {
        let num = this.isSorted ? 1 : -1;
        return num * (a.smsPerMonth - b.smsPerMonth);
      });
      return data;
    }))
    this.isSorted = !this.isSorted;
  }

  videoConferenceSort() {
    this.programs$ = this.programs$.pipe(map((data) => {
      data.sort((a, b) => {
        let num = this.isSorted ? 1 : -1;
        return num * (a.videoConferencePerMonth - b.videoConferencePerMonth);
      });
      return data;
    }))
    this.isSorted = !this.isSorted;
  }

  visualIdentificationsSort() {
    this.programs$ = this.programs$.pipe(map((data) => {
      data.sort((a, b) => {
        let num = this.isSorted ? 1 : -1;
        return num * (a.visualIdentificationsPerMonth - b.visualIdentificationsPerMonth);
      });
      return data;
    }))
    this.isSorted = !this.isSorted;
  }

}
