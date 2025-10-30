import { Component, OnInit, } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { NgxSpinnerService } from 'ngx-spinner';
import { SplitSignProcessStep } from 'src/app/enums/split-sign-process-step.enum';
import { IdentityCheckFlow } from 'src/app/models/requests/identity-flow-create.model';
import { IdentificationService } from 'src/app/services/identification.service';

@Component({
  selector: 'app-iedas-signing-flow-after-first-auth',
  templateUrl: './iedas-signing-flow-after-first-auth.component.html',
  styleUrls: ['./iedas-signing-flow-after-first-auth.component.scss']
})
export class IedasSigningFlowAfterFirstAuthComponent implements OnInit {

  public code: string;
  private error: string;
  private state: string;
  public showSuccessPage = false;
  public showDownloadButton = false;
  public token = "";

  constructor(private router: Router, private route: ActivatedRoute, private spinnerService: NgxSpinnerService, private identificationService: IdentificationService) { }

  ngOnInit(): void {
    this.spinnerService.show();
    this.route.queryParams.subscribe(

      params => {
        this.error = params.error;
        this.code = params.code;
        this.state = params.state;
        setTimeout(() => {

          this.doAfterFirstAuth();
        }, 1000);
      });
  }

  doAfterFirstAuth() {
    if (this.code) {
      this.spinnerService.show();
      let authFlow = new IdentityCheckFlow();

      authFlow.code = this.code;
      authFlow.signerToken = this.state;

      this.identificationService.CheckidentityFlowEIDASSign(authFlow).subscribe(
        (res) => {
          if (res.processStep == SplitSignProcessStep.inProgress) {
            window.location.href = res.url;
          }
          if (res.processStep == SplitSignProcessStep.success) {
            this.showDownloadButton = res.url != "";
            this.showSuccessPage = true;
            let newToken = res.url.substring(res.url.lastIndexOf('/') + 1);
            this.token = newToken;
          }

          this.spinnerService.hide();

        },
        err => {
          this.spinnerService.hide();
          this.router.navigate(['/error']);
        });
    }
  }
}