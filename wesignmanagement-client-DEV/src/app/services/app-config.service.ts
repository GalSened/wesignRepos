
import { Injectable } from "@angular/core";

@Injectable()
export class AppConfigService {

  private appConfig: string = "";

  constructor() { }

  loadAppConfig() {
    let metaTages = document.getElementsByTagName('meta');
    for (let i = 0; i < metaTages.length; i++) {
      let baseUrl =metaTages[i].getAttribute('data-api-endpoint');
      if (baseUrl != null) {
        this.appConfig = baseUrl;
        return;
      }
    }    
  }

  get apiUrl() {

    if (!this.appConfig) {
      throw Error('Config file not loaded!');
    }

    return this.appConfig;
  }

}
