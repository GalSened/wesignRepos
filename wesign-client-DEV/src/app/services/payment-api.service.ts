import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { PaymentDetailsRequest, ProductIdRequest } from '@models/payment/payment-details-request.model';
import { paymentResponse } from '@models/payment/payment-response.model';
import { User } from '@models/account/user.model';
import { AppConfigService } from './app-config.service';

@Injectable({ providedIn: 'root' })
export class PaymentApiService {


  //private paymentApi = environment.paymentapi;
  private paymentApi: string = "";
  constructor(private httpClient: HttpClient,
    private appConfigService: AppConfigService) {
    this.paymentApi = this.appConfigService.paymentapi;
  }


  public CheckStatus() {
    return this.httpClient.get<string>(this.paymentApi + "/Authentication/CheckStatus");
  }

  public openSessionPaymentApi(userEmail: string) {
    var item = new ProductIdRequest();
    item.productID = environment.productId;
    item.requestID = userEmail;
    return this.httpClient.post<string>(this.paymentApi + "/Authentication", item);
  }

  public sendPaymentRequest(paymentDetailsRequest: PaymentDetailsRequest, provider: string) {
    return this.httpClient.post<paymentResponse>(this.paymentApi + "/" + provider + "/Payment"
      , paymentDetailsRequest);
  }

}
