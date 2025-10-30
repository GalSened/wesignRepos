
export class PaymentDetailsRequest {
  public token: string;
  public product: Product;
  public cancelUrl: string;
  public sucessUrl: string;
  public firstName: string;
  public lastName: string;
  //public tax: number;
  //public shipping: number;
  //public expiryMonths: number;

  constructor() {
    this.cancelUrl = "";
    this.sucessUrl = "";
    this.product = new Product();
  }
}

export class Product {
  public description: string;
  public curreny: number;
  public price: number;
  public productType: number;
  public language: number;
  public planId: string;
}

export class ProductIdRequest {
  public productID: string;
  public requestID: string;
}
