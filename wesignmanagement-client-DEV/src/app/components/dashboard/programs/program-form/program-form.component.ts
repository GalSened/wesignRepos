import { Component, OnInit, EventEmitter, Output, Input } from '@angular/core';
import { Program } from 'src/app/models/program.model';
import { Errors } from 'src/app/models/error/errors.model';
import { ProgramsApiService } from 'src/app/services/programs-api.service';
import { SharedService } from 'src/app/services/shared.service';
import { Operation } from 'src/app/enums/operaion.enum';
import { NgxSpinnerService } from 'ngx-spinner';

@Component({
  selector: 'app-program-form',
  templateUrl: './program-form.component.html',
  styleUrls: ['./program-form.component.css']
})
export class ProgramFormComponent implements OnInit {



  @Input() public operation: Operation;
  @Input() public program: Program = new Program();
  public showUpdateAlert: boolean = false;
  public isBusy: boolean = false;
  public isError: boolean = false;
  public submited: boolean = false;
  public isUnlimitedUsers: boolean = false;
  public isUnlimitedTemplates: boolean = false;
  public isUnlimitedDocuments: boolean = false;
  public isUnlimitedSms: boolean = false;
  public isUnlimitedVisualIdentifications: boolean = false;
  public isUnlimitedVideoConference: boolean = false;
  public errorMessage: string = "Error - Please correct the marked fields";
  @Output() bannerAlertEvent = new EventEmitter<string>();

  constructor(private programsApiService: ProgramsApiService,
    private sharedService: SharedService,
    private spinner: NgxSpinnerService) { }

  @Output() public removeProgramForm = new EventEmitter<number>();

  ngOnInit(): void {
    console.log(this.program);
  }

  public update() {

    this.submited = true;
    if (this.operation == Operation.Create) {
      this.createProgram();
    }

    if (this.operation == Operation.Update) {
      this.showUpdateAlert = true;

    }


  }
  createProgram() {
    if (!this.isBusy) {
      this.isBusy = true;
      this.spinner.show();

      this.program.users = this.isUnlimitedUsers ? -1 : this.program.users;
      this.program.documentsPerMonth = this.isUnlimitedDocuments ? -1 : this.program.documentsPerMonth;
      this.program.templates = this.isUnlimitedTemplates ? -1 : this.program.templates;
      this.program.smsPerMonth = this.isUnlimitedSms ? -1 : this.program.smsPerMonth;
      this.program.visualIdentificationsPerMonth = this.isUnlimitedVisualIdentifications ? -1 : this.program.visualIdentificationsPerMonth;
      this.program.videoConferencePerMonth = this.isUnlimitedVideoConference ? -1 : this.program.videoConferencePerMonth;
      
      this.programsApiService.createProgram(this.program).subscribe(
        () => {
          this.hideTheForm();
          this.bannerAlertEvent.emit('eventDesc');
          this.isError = false;
          this.isBusy = false;
          this.spinner.hide();
          this.program = new Program();
        }, (err) => {
          this.isError = true;
          this.errorMessage = this.sharedService.getErrorMessage(new Errors(err.error));
          this.isBusy = false;
          this.spinner.hide();
        }
      );
    }
  }

  public cancel() {
    this.isBusy = false;
    this.hideTheForm();
  }

  public hideTheForm() {
    this.removeProgramForm.emit();
    window.scroll(0, 0);
  }

  public hideAlert() {
    this.showUpdateAlert = false;
    this.isBusy = false;

  }

  public updateProgram() {
    if (!this.isBusy) {
      this.showUpdateAlert = false;
      this.isBusy = true;
      this.spinner.show();
      this.program.users = this.isUnlimitedUsers ? -1 : this.program.users;
      this.program.documentsPerMonth = this.isUnlimitedDocuments ? -1 : this.program.documentsPerMonth;
      this.program.templates = this.isUnlimitedTemplates ? -1 : this.program.templates;
      this.program.smsPerMonth = this.isUnlimitedSms ? -1 : this.program.smsPerMonth;
      this.program.visualIdentificationsPerMonth = this.isUnlimitedVisualIdentifications ? -1 : this.program.visualIdentificationsPerMonth;
      this.program.videoConferencePerMonth = this.isUnlimitedVideoConference ? -1 : this.program.videoConferencePerMonth;
      this.programsApiService.updateProgram(this.program).subscribe(() => {
        this.isBusy = false;
        this.spinner.hide();
        this.bannerAlertEvent.emit('eventDesc');
        this.isError = false;
        this.program = new Program();
      }, (err) => {
        this.spinner.hide();
        this.isBusy = false;
        this.errorMessage = this.sharedService.getErrorMessage(new Errors(err.error));
        this.hideAlert();
      },
        () => {
          this.hideAlert();
          this.hideTheForm();

        });
    }
  }

  public handleUnlimitedInputs(programInput: Program) {
    if (programInput.users == -1) {
      this.isUnlimitedUsers = true;
    }
    if (programInput.templates == -1) {
      this.isUnlimitedTemplates = true;
    }
    if (programInput.documentsPerMonth == -1) {
      this.isUnlimitedDocuments = true;
    }
    if (programInput.smsPerMonth == -1) {
      this.isUnlimitedSms = true;
    }
    if (programInput.visualIdentificationsPerMonth == -1)
    {
      this.isUnlimitedVisualIdentifications = true; 
    }
    if (programInput.videoConferencePerMonth == -1)
      {
        this.isUnlimitedVideoConference = true; 
      }
  }

}
