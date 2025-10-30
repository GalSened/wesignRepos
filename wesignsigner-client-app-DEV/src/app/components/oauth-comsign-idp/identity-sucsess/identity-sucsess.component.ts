import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, ParamMap, Router } from '@angular/router';
import { TranslateService } from '@ngx-translate/core';
import { NgxSpinnerService } from 'ngx-spinner';
import { of } from 'rxjs';
import { switchMap } from 'rxjs/operators';
import { LanguageService } from 'src/app/language.service';
import { Errors } from 'src/app/models/error/errors.model';
import { IdentityCheckFlow } from 'src/app/models/requests/identity-flow-create.model';
import { DocumentsService } from 'src/app/services/documents.service';
import { IdentificationService } from 'src/app/services/identification.service';
import { IdentityService } from 'src/app/services/identity.service';
import { StateService } from 'src/app/services/state.service';

@Component({
  selector: 'app-identity-sucsess',
  templateUrl: './identity-sucsess.component.html',
  styleUrls: ['./identity-sucsess.component.scss']
})
export class IdentitySucsessComponent implements OnInit {

  private readonly visual_identity_operation_failed = 93;
  private readonly visual_identity_operation_failed_wrong_user = 94;

  public showVisualtIdentityOperationFailed: boolean = false;
  public showVisualtIdentityWrongUser: boolean = false;

  constructor(private documentsService: DocumentsService, private router: Router, private route: ActivatedRoute,
    private spinnerService: NgxSpinnerService, private stateService: StateService, private translate: TranslateService,
    private identityService: IdentityService, private identificationService: IdentificationService, private languageService: LanguageService) { }

  ngOnInit(): void {
    /*  this.showVisualtIdentityOperationFailed = true;   */

    this.route.paramMap.pipe(
      switchMap((params: ParamMap) => of(params.get('signerToken')))
    ).subscribe((id) => {
      setTimeout(() => {
        this.identityService.connect(id);

        this.identityService.reconnectSubject.subscribe(() => {
          this.identityService.connect(id);
        });
      }, 3000);

      this.route.queryParams.subscribe(params => {
        this.spinnerService.show();
        let token = id;
        if (!token || !params.token) {
          this.router.navigate(["/"]);
          this.spinnerService.hide();
        }

        var input = new IdentityCheckFlow();
        input.signerToken = token;
        input.code = params.token;

        this.documentsService.getCollectionDataFlowInfo(input.signerToken).subscribe(
          res => {
            setTimeout(() => {
              this.identityService.processDone(token, params.token);
            }, 3000);


            this.languageService.fixLanguage(res.language);

            this.identificationService.checkidentityFlow(input).subscribe(
              (result) => {
                this.stateService.SetOauthDone(input.code);
                this.stateService.SetDocuementToken(result.token);
                this.router.navigate(["signature/" + result.token]);
              },
              (err) => {
                this.spinnerService.hide();
                let result = new Errors(err.error);

                if (result.errorCode == this.visual_identity_operation_failed) {
                  this.showVisualtIdentityOperationFailed = true;
                }

                else if (result.errorCode = this.visual_identity_operation_failed_wrong_user) {
                  this.showVisualtIdentityWrongUser = true;
                }

                else {
                  this.router.navigate["/"];
                }
              });
          });
      })
    });
  }
}