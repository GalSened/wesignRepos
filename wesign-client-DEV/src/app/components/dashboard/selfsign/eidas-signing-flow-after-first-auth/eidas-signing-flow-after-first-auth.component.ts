import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { SplitSignProcessStep } from '@models/enums/split-sign-process-step.enum';
import { IdentityCheckFlow } from '@models/self-sign-api/identity-check-flow.model';
import { SelfSignApiService } from '@services/self-sign-api.service';
import { SharedService } from '@services/shared.service';

@Component({
  selector: 'sgn-eidas-signing-flow-after-first-auth',
  templateUrl: './eidas-signing-flow-after-first-auth.component.html',
  styles: [
  ]
})
export class EidasSigningFlowAfterFirstAuthComponent implements OnInit {
  
  public code : string;
  private error : string;
  private state :string;
  public showSuccessPage: boolean = false;
  public showDownloadButton: boolean = false;
  public token : string= "";
  constructor(
    private router: Router,
    private route: ActivatedRoute,
    private sharedService: SharedService,
    private selfSignApiService: SelfSignApiService) {
    }
  
  ngOnInit(): void {
    this.sharedService.setBusy(true);
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
    this.sharedService.setBusy(true);
    if(this.code)
    {
      let authFlow = new IdentityCheckFlow();
      
        authFlow.code = this.code;
        authFlow.signerToken = this.state;
      
      this.selfSignApiService.CheckidentityFlowEIDASSign(authFlow).subscribe(
        (res) => {
          
          if(res.processStep == SplitSignProcessStep.inProgress)
          {
            window.location.href = res.url;
          }
          if(res.processStep == SplitSignProcessStep.success)
            {
              this.sharedService.setSuccessAlert("DOCUMENT.SAVED_SUCCESSFULY");
              this.sharedService.setBusy(false);          
              this.router.navigate(["dashboard", "success", "selfsign"]);
            }
            this.sharedService.setBusy(false);
          
          
        },
        err => {
          this.sharedService.setBusy(false);
          this.router.navigate(['/error']);
      
    }
  );
  }
}

}
