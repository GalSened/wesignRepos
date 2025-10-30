import { Component, OnInit, ViewChild } from '@angular/core';
import { NgForm } from '@angular/forms';
import { ActivatedRoute, ParamMap, Router } from '@angular/router';
import { TranslateService } from '@ngx-translate/core';
import { of } from 'rxjs';
import { switchMap } from 'rxjs/operators';
import { LanguageService } from 'src/app/language.service';
import { Errors } from 'src/app/models/error/errors.model';
import { SingleLinkRequest } from 'src/app/models/requests/single-link-request.model';
import { Language } from 'src/app/models/responses/document-collection-count.model';
import { SingleLinkService } from 'src/app/services/single-link.service';
import { LangComponent } from '../lang/lang.component';


@Component({
  selector: 'app-single-link',
  templateUrl: './single-link.component.html',
  styleUrls: ['./single-link.component.scss']
})
export class SingleLinkComponent implements OnInit {

  public templateId: string;
  public submitted: boolean = false;
  public isAccecptToTurnsOfUse: boolean = false;
  public isBusy: boolean = false;
  public showError: boolean = false;
  public errorMessage: string = "";
  public docuemntUrl: string = "";
  public useEmail: boolean = true;
  private phoneExt: string = "972";
  signerMeans: any;
  public year: number = 2025;
  public isSmsProviderSupportGloballySend: boolean = false;
  @ViewChild('lang', { static: false }) lang: LangComponent;


  constructor(private route: ActivatedRoute,
    private router: Router,
    private singleLinkService: SingleLinkService,
    private languageService:LanguageService,
    private translate: TranslateService) {
  }


  ngOnInit() {

    this.year = new Date().getFullYear();

    this.route.paramMap.pipe(
      switchMap((params: ParamMap) => of(params.get('id')))
    ).subscribe((id) => {
      if (!this.isValidGuid(id)) {
        this.router.navigateByUrl('/');
      }
      this.templateId = id;
      this.errorMessage = this.translate.instant('ERROR.INPUT.6');
      this.singleLinkService.getData(this.templateId).subscribe(
        (data) => {
          this.isSmsProviderSupportGloballySend = data.isSmsProviderSupportGloballySend;
          this.languageService.fixLanguage(data.language);
          if(data.language==Language.he)
          {
            this.lang.changeLangByLanguage(data.language);
          }


        },
        (err) => {
          this.isSmsProviderSupportGloballySend = false;
        });
    });


  }

  onSubmit(form: NgForm) {
    this.submitted = true;
    this.isBusy = true;
    this.errorMessage = "";
    if (!form.valid) {
      this.isBusy = false;
      return;
    }

    let singleLinkRequest = new SingleLinkRequest();
    var inputs = JSON.parse(JSON.stringify(form.value));
    singleLinkRequest.templateId = this.templateId;
    singleLinkRequest.signerMeans = inputs.signerMeans ;
    singleLinkRequest.phoneExtension = `+${this.phoneExt}`;
    singleLinkRequest.fullname = inputs.fullname;
    this.singleLinkService.createDocuemnt(singleLinkRequest).subscribe(
      (data) => {
        this.docuemntUrl = data.url;
        window.location.href = data.url;
        this.isBusy = false;
        this.submitted = false;
      },
      (err) => {
        let result = new Errors(err.error);
        this.errorMessage = this.translate.instant('SERVER_ERROR.' + result.errorCode);
        this.isBusy = false;
        this.submitted = false;
        this.showError = true;
      });
  }

  isValidGuid(value: string) {
    var regex = /[a-f0-9]{8}(?:-[a-f0-9]{4}){3}-[a-f0-9]{12}/i;
    return regex.exec(value) != null;

  }

  public onSignerMethodChanged(event) {
    this.useEmail = event.target.selectedIndex == 0;

  }
  onCountryChange(obj) {
    this.phoneExt = obj.dialCode;
  }
}
