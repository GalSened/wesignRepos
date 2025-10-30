import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { Plan } from '@models/account/plan.model';
import { PaymentApiService } from '@services/payment-api.service';
import { SharedService } from '@services/shared.service';
import { PaymentDetailsRequest } from '@models/payment/payment-details-request.model';
import { Router, ActivatedRoute } from '@angular/router';
import { User } from '@models/account/user.model';
import { UserApiService } from '@services/user-api.service';
import { switchMap } from 'rxjs/operators';
import { Currency } from '@models/enums/currency.enum';


@Component({
  selector: 'sgn-plan',
  templateUrl: './plan.component.html',
  styles: []
})
export class PlanComponent implements OnInit {

  
  @Input() public plan: Plan;
  @Input() public userEmail: string;
  @Input() public name: string;
  @Input() public showButton: boolean = true;
  @Input() public fromIsrael: boolean = false;
 
  @Output() public clickEvent = new EventEmitter();

  public isEng: boolean;
  public isBusy: boolean;
  public currency : Currency = Currency.USD;
  public yearly: boolean = false;

  ngOnChanges(changes: any) {
    this.yearly = this.plan.numberOfMonths == 12;
}

  constructor(
    private sharedService: SharedService,
    private paymentApiService: PaymentApiService,
    private router: Router
  ) { }

  ngOnInit() {    
    this.isEng = this.sharedService.getCurrentLanguage().Code == "en"; 
  }
   get getSelectdSign():string{
      switch(this.currency)
      {
    
        case Currency.ILS:
          {
            return "₪"
          }
        case Currency.USD:
            {
             return "$"
            }
        case Currency.EUR:
          {
            return "€"
          }

      }

  }
  get getSelectedPricePerMonth():number{
    switch(this.currency)
    {
  
      case Currency.ILS:
        {
          return this.plan.pricePerMonthILS;
        }
      case Currency.USD:
          {
           return this.plan.pricePerMonthUSD;
          }
      case Currency.EUR:
        {
          return this.plan.pricePerMonthEUR;
        }

    }
  }

  public onOptionsSelected(value:string)
  {
    if(value =="1")
    {
      this.currency = Currency.ILS
    }
    if(value == "2")
    {
      this.currency = Currency.USD;
    }
    if(value == "3")
    {
      this.currency = Currency.EUR;
    }

  }

  public payClick($event: any, provider: string) {
    this.isBusy = true;
    this.sharedService.setBusy(true, "TINY.PAYMENT.IN_PROCESS_PLEASE_WAIT");
    $event.preventDefault();    
    this.isEng = this.sharedService.getCurrentLanguage().Code == "en";
    this.paymentApiService.openSessionPaymentApi(this.userEmail).pipe(
      switchMap((_token) => {
        let _nameArr = this.name.split(" ") || [this.name];

        var paymentDetailsRequest = new PaymentDetailsRequest();
        paymentDetailsRequest.token = _token;
        paymentDetailsRequest.product.curreny =this.currency; 
        paymentDetailsRequest.product.language = this.isEng ? 2 : 1; 
        //paymentDetailsRequest.product.description = this.plan.programID; // Todo - remove;
        paymentDetailsRequest.product.productType = 0; // WESIGN
        paymentDetailsRequest.product.planId = this.plan.programID;
        paymentDetailsRequest.firstName = _nameArr[0];
        paymentDetailsRequest.lastName = _nameArr.length > 1 ? _nameArr[1] : "";
        paymentDetailsRequest.sucessUrl =this.isEng ?   this.plan.successURLEng:  this.plan.successURLHeb ;


        return this.paymentApiService.sendPaymentRequest(paymentDetailsRequest, provider);
      })
    )
      .subscribe((paymentResult) => {
        //window.open(paymentResult.approvalURL);
        if (paymentResult.approvalURL)
          window.location.href = paymentResult.approvalURL;
        else {
          const prevUrl = "/dashboard";
          this.router.navigateByUrl(prevUrl);
          this.sharedService.setBusy(false);
        }
      },
        (error) => {
          this.sharedService.setBusy(false);
        },
        () => { this.sharedService.setBusy(false); });
  }

  public onClick() {
    this.clickEvent.emit(this.plan.programID);
  }

}
