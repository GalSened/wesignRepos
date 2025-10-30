
import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { DomSanitizer, SafeUrl } from '@angular/platform-browser';
import { ActivatedRoute, Router } from '@angular/router';
import { TranslateService } from '@ngx-translate/core';
import { NgxSpinnerService } from 'ngx-spinner';
import { LanguageService } from 'src/app/language.service';
import { Errors } from 'src/app/models/error/errors.model';
import { CreateAuthFlowModel, IdentityCheckFlow } from 'src/app/models/requests/identity-flow-create.model';
import { DocumentsService } from 'src/app/services/documents.service';
import { IdentificationService } from 'src/app/services/identification.service';
import { IdentityService } from 'src/app/services/identity.service';
import { StateService } from 'src/app/services/state.service';

@Component({
  selector: 'app-oauth-comsign-idp',
  templateUrl: './oauth-comsign-idp.component.html',
  styleUrls: ['./oauth-comsign-idp.component.scss']
})
export class OauthComsignIdpComponent implements OnInit {

  @Output() public hideOtpDetailsEvent = new EventEmitter<string>();
  @Output() public closeSpinner = new EventEmitter<any>();
  @Output() public showSpinner = new EventEmitter<any>();

  @Input() public token: string;
  private readonly visual_identity_operation_failed = 93;
  private readonly visual_identity_operation_failed_wrong_user = 94;
  current_url: SafeUrl;
  inAuthFlow = false;
  code: string;
  error: string;
  errorCodeMessage: string;
  isSubmitBusy: boolean;
  showAlertMessage: boolean;
  showErrorMessage: boolean;
  showVisualIdentityOperationFailed: boolean = false;;
  showVisualIdentityWrongUser: boolean = false;;
  inAuthFlowRequest = true;

  constructor(private documentsService: DocumentsService, private route: ActivatedRoute, private stateService: StateService,
    private router: Router, private sanitizer: DomSanitizer, private identityService: IdentityService, private translate: TranslateService,
    private spinnerService: NgxSpinnerService, private identificationService: IdentificationService, private languageService: LanguageService,) { }

  ngOnInit(): void {

    this.route.queryParams.subscribe(
      params => {
        this.error = params.error;
        this.code = params.code;
      });

    setTimeout(() => {
      this.identityService.connect(this.token);
      this.identityService.listenToonIdentityDone();

      this.identityService.reconnectSubject.subscribe(() => {
        this.identityService.connect(this.token);
        this.identityService.listenToonIdentityDone();
      });

      this.identityService.processDoneAdSubject.subscribe((identityToken) => {
        this.jumpToProcessCheck(identityToken)
      }
      );
    }, 3000);
    this.closeSpinner.emit();
  }

  public declineDocument() {
    this.stateService.showDeclineForm = true;
  }

  public CreateAuthFlow() {
    let authFlow = new CreateAuthFlowModel();
    authFlow.signerToken = this.token;
    this.showSpinner.emit();
    this.identificationService.CreateidentityFlow(authFlow).subscribe(
      (data) => {
        this.inAuthFlow = true;
        this.inAuthFlowRequest = false;
        //window.location.href  = data.identityFlowURL;
        // for redirect the user back if use the desktop and not mobile- not working good now. 
        // all the infrastructure exist need to active   
        this.current_url = this.sanitizer.bypassSecurityTrustResourceUrl(data.identityFlowURL)

        this.closeSpinner.emit();
      },
      (err) => {
        // need to add errors is happend

        this.router.navigate(["/"]);
      },
      () => {
      });
  }

  jumpToProcessCheck(identityToken) {
    var input = new IdentityCheckFlow();
    input.signerToken = this.token;
    input.code = identityToken;

    this.documentsService.getCollectionDataFlowInfo(this.token).subscribe(
      res => {
        this.languageService.fixLanguage(res.language);

        this.identificationService.checkidentityFlow(input).subscribe(
          (result) => {
            this.stateService.SetOauthDone(input.code);
            this.stateService.SetDocuementToken(result.token);
            this.current_url = "";
            this.inAuthFlow = false;
            this.router.navigate(["/signature/" + result.token]);
          },
          (err) => {
            this.spinnerService.hide();

            let result = new Errors(err.error);

            if (result.errorCode == this.visual_identity_operation_failed) {
              this.inAuthFlow = false;
              this.inAuthFlowRequest = false;
              this.showVisualIdentityOperationFailed = true;
            }

            else if (result.errorCode = this.visual_identity_operation_failed_wrong_user) {
              this.inAuthFlow = false;
              this.inAuthFlowRequest = false;
              this.showVisualIdentityWrongUser = true;
            }

            else {
              this.router.navigate["/"];
            }
          });
      });
  }
}